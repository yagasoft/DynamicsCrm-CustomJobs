#region Imports

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.Libraries.Common;
using static Yagasoft.CustomJobs.Engine.Helpers;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job.Abstract
{
	internal class JobPagingInfo
	{
		internal List<Guid> Targets;
		internal string Cookie;
		internal int NextPage;
	}

	internal class ExecutionFailures
	{
		internal IDictionary<Guid, Exception> Exceptions = new Dictionary<Guid, Exception>();
	}

	[Log]
	internal abstract class JobTarget
	{
		protected CustomJob Job;
		protected EngineParams Params;
		protected IOrganizationService Service;
		protected CrmLog Log;

		private static readonly HttpClient httpClient = new();

		protected JobTarget(CustomJob job, EngineParams engineParams, IOrganizationService service, CrmLog log)
		{
			Job = job;
			Params = engineParams;
			Service = service;
			Log = log;
		}

		internal abstract JobRunStatus ProcessTarget();

		protected abstract JobPagingInfo GetTargets();

		protected ExecutionFailures Execute(JobPagingInfo pagingInfo)
		{
			var result = new ExecutionFailures();

			var contextService = GetServiceInContext();
			var targets = pagingInfo.Targets;

			if (Job.URL.IsFilled())
			{
				CallUrl(targets, result);
			}
			else if (string.IsNullOrEmpty(Job.TargetLogicalName))
			{
				CallGlobalAction(contextService, result);
			}
			else
			{
				if (Guid.TryParse(Job.ActionName, out var workflowId)
					|| ((workflowId = Job.Workflow.GetValueOrDefault()) != Guid.Empty))
				{
					Log.Log($"Running workflow '{workflowId}' ...");
				}
				else if (Job.ActionName.IsFilled())
				{
					Log.Log($"Executing action '{Job.ActionName}' ...");
				}
				else
				{
					Log.LogWarning("Nothing found to execute. Review job configuration or parameters.");
				}
				
				if (targets?.Any() == true)
				{
					Log.Log("Preparing targets for execution ...");

					var requests = PrepareTargetRequests(targets, workflowId).ToArray();

					var degreeOfParallelism = Params.MaximumDegreeOfParallelism;

					switch (Params.TargetExecutionMode)
					{
						case CommonConfiguration.TargetExecutionModeEnum.Sequential:
						case CommonConfiguration.TargetExecutionModeEnum.ExecuteMultiple:
							degreeOfParallelism = 1;
							break;
					}
					
					switch (Params.TargetExecutionMode)
					{
						case CommonConfiguration.TargetExecutionModeEnum.ExecuteMultiple:
						case CommonConfiguration.TargetExecutionModeEnum.Combined:
						{
							ConcurrentBag<KeyValuePair<OrganizationRequest, ExecuteBulkResponse>> faults = new();

							var pRequests = Enumerable
								.Range(0, (requests.Length / 999) + 1)
								.Select(i => requests.Skip(i * 999).Take(999))
								.Where(e => e.Any());

							Parallel.ForEach(pRequests,
								new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism },
								(iRequests, state) =>
								{
									var iResponses = CrmHelpers.ExecuteBulk(Service,
										iRequests.Select(e => e.Request).ToArray(), true, 999,
										Job.ContinueOnError == true);

									foreach (var response in iResponses.Where(p => p.Value.Fault != null))
									{
										faults.Add(response);
									}

									if (Job.ContinueOnError == false && faults.Any())
									{
										state.Stop();
									}
								});

							if (faults.Any())
							{
								Log.Log($"Failed to execute on targets.");

								foreach (var fault in faults)
								{
									Guid? failedTarget = null;

									if (fault.Key is ExecuteWorkflowRequest wf)
									{
										failedTarget = wf.EntityId;
									}
									else if (fault.Key.Parameters.TryGetValue("Target", out var failedTargetRaw))
									{
										failedTarget = failedTargetRaw as Guid?;
									}

									var faultMessage = fault.Value.FaultMessage;

									Log.Log($"Failed at '{failedTarget}':'{faultMessage}'.");
									result.Exceptions[failedTarget.GetValueOrDefault()] =
										new FaultException<OrganizationServiceFault>(fault.Value.Fault);
								}
							}
							
							break;
						}
						
						default:
							Parallel.ForEach(requests,
								new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism },
								(request, state) =>
								{
									try
									{
										Log.Log($"Running workflow on '{request.Target}'.");
										contextService.Execute(request.Request);
									}
									catch (Exception ex)
									{
										Log.Log($"Failed at '{request.Target}':'{ex.Message}'.");
										result.Exceptions[request.Target] = ex;

										if (Job.ContinueOnError == false)
										{
											state.Stop();
										}
									}
								});
							
							break;
					}
				}
				else
				{
					Log.Log("No targets to run workflow on.");
				}

				Log.Log($"Finished execution.");
			}

			return result;
		}

		private IEnumerable<TargetRequest> PrepareTargetRequests(List<Guid> targets, Guid workflowId)
		{
			var requests = targets.Select(
				t =>
				{
					if (workflowId != Guid.Empty)
					{
						return
							new TargetRequest
							{
								Target = t,
								Request =
									new ExecuteWorkflowRequest
									{
										EntityId = t,
										WorkflowId = workflowId
									}
							};
					}

					if (Job.ActionName.IsFilled())
					{
						var action = PrepAction();

						action["Target"] = new EntityReference(Job.TargetLogicalName, t);

						return
							new TargetRequest
							{
								Target = t,
								Request =
									new ExecuteWorkflowRequest
									{
										EntityId = t,
										WorkflowId = workflowId
									}
							};
					}

					return null;
				})
				.Where(e => e != null);

			return requests;
		}

		private OrganizationRequest PrepAction()
		{
			var action = new OrganizationRequest(Job.ActionName);

			if (Job.SerialisedInputParams.IsFilled())
			{
				Log.Log($"Inputs: '{Job.SerialisedInputParams}'.");
				action.Parameters.AddRange(GetInputs());
			}
			return action;
		}

		private void CallGlobalAction(IOrganizationService contextService, ExecutionFailures result)
		{
			try
			{
				Log.Log($"Executing global action: {Job.ActionName} ...");
				contextService.Execute(PrepAction());
			}
			catch (Exception ex)
			{
				Log.Log($"Failed: '{ex.Message}'.");
				result.Exceptions[Job.Id] = ex;
			}

			Log.Log($"Executed action '{Job.ActionName}'.");
		}

		private void CallUrl(List<Guid> targets, ExecutionFailures result)
		{
			var jobId = Job.Id.ToString().ToUpper();

			var uriBuilder = new UriBuilder(Job.URL);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["jobId"] = jobId;
			var paramsString = Job.SerialisedInputParams;
			object parameters = null;

			if (paramsString.IsFilled())
			{
				Log.Log($"Params: '{paramsString}'.");

				try
				{
					parameters = JsonConvert.DeserializeObject(paramsString);
				}
				catch
				{
					Log.LogError($"Failed to parse job parameters as JSON.");
				}
			}

			Log.Log($"Calling URL '{Job.URL}' ...");

			if (targets?.Any() == true)
			{
				foreach (var target in targets)
				{
					try
					{
						query["targetId"] = target.ToString().ToUpper();
						CallEndpoint(uriBuilder, query, target, parameters);
					}
					catch (Exception ex)
					{
						Log.Log($"Failed at '{target}':'{ex.Message}'.");
						result.Exceptions[target] = ex;

						if (Job.ContinueOnError == false)
						{
							break;
						}
					}
				}
			}
			else if (Job.TargetLogicalName.IsEmpty())
			{
				try
				{
					query["targetId"] = "";
					CallEndpoint(uriBuilder, query, null, parameters);
				}
				catch (Exception ex)
				{
					Log.Log($"Failed: '{ex.Message}'.");
					result.Exceptions[Job.Id] = ex;
				}
			}

			Log.Log($"Called URL '{Job.URL}'.");
		}

		private void CallEndpoint(UriBuilder uriBuilder, NameValueCollection query, Guid? target, object parameters)
		{
			uriBuilder.Query = query.ToString();
			var uri = uriBuilder.Uri;

			Log.Log($"Calling URL '{uri}' on '{target}' ...");

			var response = httpClient.PostAsJsonAsync(uri, parameters).Result;

			try
			{
				Log.Log($"Response: {JsonConvert.SerializeObject(response)} ...");
			}
			catch
			{
				// ignored
			}

			var responseBody = response.Content.ReadAsStringAsync().Result;
			Log.Log($"Response body: {responseBody}.");

			if (!response.IsSuccessStatusCode)
			{
				throw new ServiceActivationException($"Failed to make request to '{uri}' ({response.StatusCode}).");
			}
		}

		[NoLog]
		protected IOrganizationService GetServiceInContext()
		{
			var contextService = Service;

			// TODO impersonate
			////if (Job.ContextUser != null)
			////{
			////	contextService = ServiceFactory.CreateOrganizationService(Job.ContextUser);
			////	Log.Log($"Got Context service for user '{Job.ContextUser}'.");
			////}

			if (Job.ContextUser != null)
			{
				Log.LogWarning($"Impersonation is not supported on this platform.");
			}

			return contextService;
		}

		protected virtual JobRunStatus ProcessSuccess()
		{
			if (IsSubJob())
			{
				var parentId = Job.ParentJob.Value;

				if (IsWaitingOnSubJobs(Service, parentId))
				{
					Log.Log($"Setting status of parent '{parentId}' to 'waiting' ...");
					SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, parentId, IsParentRecurrentJob());
					Log.Log($"Set status of parent '{parentId}' to 'waiting'.");
				}
			}
			else if (IsRecurrentJob())
			{
				Log.Log($"Setting status to 'waiting' ...");
				SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, Job.Id, IsParentRecurrentJob());
				Log.Log($"Set status to 'waiting'.");
			}

			return
				new JobRunStatus
				{
					IsSuccess = true,
					IsClose = !IsRecurrentJob()
				};
		}

		protected virtual JobRunStatus ProcessFailure(ExecutionFailures executionResult)
		{
			var status = new JobRunStatus();

			if (IsNoRetry())
			{
				if (IsSubJob())
				{
					var parentId = Job.ParentJob.Value;

					if (IsWaitingOnSubJobs(Service, parentId))
					{
						if (Job.IgnoreFailure == true)
						{
							Log.Log($"Setting status of parent '{parentId}' to 'waiting' ...");
							SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, parentId, IsParentRecurrentJob());
							Log.Log($"Set status of parent '{parentId}' to 'waiting'.");
						}
						else
						{
							Log.Log($"Setting status of parent '{parentId}' to 'failed' ...");
							Close(Service, CustomJob.StatusReasonEnum.Failure, parentId, true);
							Log.Log($"Set status of parent '{parentId}' to 'failed'.");
						}
					}

					status.ParentId = parentId;
				}

				if (Job.FailureAction != null)
				{
					Log.Log("Running failure action ...");

					try
					{
						Service.Execute(
							new ExecuteWorkflowRequest
							{
								EntityId = Job.Id,
								WorkflowId = Job.FailureAction.Value
							});
					}
					catch (Exception exception)
					{
						Log.LogError(exception);
					}

					Log.Log("Finished running failure action.");
				}

				status.IsClose = true;
			}
			else
			{
				if (Job.RetrySchedule != null)
				{
					UpdateRetryTargetDate(Service, Job, Log);
				}

				IncrementRetry(Job.CurrentRetryRun ?? 0);
				Log.Log($"Setting status of job to 'retry' ...");
				SetStatus(Service, CustomJob.StatusReasonEnum.Retry, Job.Id, false);
				Log.Log($"Set status of job to 'retry'.");
			}

			var failures = executionResult.Exceptions
				.Select(failure =>
					new CustomJobFailedTarget
					{
						ID = failure.Key.ToString(),
						FailureMessage = failure.Value.BuildExceptionMessage()
					});

			status.IsSuccess = false;
			status.RunTargetFailures = failures.ToList();

			return status;
		}

		protected IDictionary<string, object> GetInputs()
		{
			IDictionary<string, object> parameters = null;

			try
			{
				parameters = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Job.SerialisedInputParams);
			}
			catch
			{
				Log.Log("Parameters are not in a JSON format, or are poorly formatted strict JSON.");

				try
				{
					parameters = SerialiserHelpers.DeserialiseSimpleJson(Job.SerialisedInputParams)
						.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
				}
				catch
				{
					Log.Log("Parameters are not in a JSON format, or are poorly formatted simple JSON.");

					try
					{
						parameters = SerialiserHelpers.DeserialiseStrictXml<IDictionary<string, object>>(Job.SerialisedInputParams);
					}
					catch
					{
						Log.Log("Parameters are not in an XML format, or are poorly formatted XML.");
					}

					if (parameters == null)
					{
						throw new FormatException("Failed to parse input params.");
					}
				}
			}

			return parameters;
		}

		protected bool IsRecurrentJob()
		{
			var result = Job.RecurrentJob == true;
			Log.Log($"IsRecurrentJob: {result}");
			return result;
		}

		protected bool IsParentRecurrentJob()
		{
			var result = Job.ParentRecurrent == true;
			Log.Log($"IsParentRecurrentJob: {result}");
			return result;
		}

		protected bool IsSuccessDelete()
		{
			var result = Job.DeleteOnSuccess == true;
			Log.Log($"IsSuccessDelete: {result}");
			return result;
		}

		protected bool IsSubJob()
		{
			var result = Job.ParentJob != null;
			Log.Log($"IsSubJob: {result}");
			return result;
		}

		protected void IncrementRetry(int currentRun)
		{
			Log.Log($"Updating current retry run to {currentRun + 1} ...");
			Service.Update(
				new CustomJob
				{
					Id = Job.Id,
					CurrentRetryRun = currentRun + 1
				});
			Log.Log($"Updated current retry run to {currentRun + 1}.");
		}

		protected bool IsNoRetry()
		{
			var isNoRetry = (Job.MaxRetryCount == null || (Job.CurrentRetryRun ?? 0) >= Job.MaxRetryCount)
				&& Job.RetrySchedule == null;
			Log.Log($"Is no retry: {isNoRetry}");
			return isNoRetry;
		}

		private class TargetRequest
		{
			internal Guid Target { get; set; }
			internal OrganizationRequest Request { get; set; }
		}
	}
}
