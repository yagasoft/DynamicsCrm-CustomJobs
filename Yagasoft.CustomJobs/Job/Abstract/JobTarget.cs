#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using static Yagasoft.CustomJobs.Helpers;

#endregion

namespace Yagasoft.CustomJobs.Job.Abstract
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
		protected IOrganizationService Service;
		protected IOrganizationServiceFactory ServiceFactory;
		protected CrmLog log;

		protected JobTarget(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log)
		{
			Job = job;
			Service = service;
			ServiceFactory = serviceFactory;
			this.log = log;
		}

		internal abstract JobRunStatus ProcessTarget();

		protected abstract JobPagingInfo GetTargets();

		protected ExecutionFailures Execute(JobPagingInfo pagingInfo)
		{
			var result = new ExecutionFailures();

			var contextService = GetServiceInContext();
			var targets = pagingInfo.Targets;
			Guid workflowId;

			if (Guid.TryParse(Job.ActionName, out workflowId)
				|| ((workflowId = Job.Workflow.GetValueOrDefault()) != Guid.Empty))
			{
				log.Log($"Running workflow '{workflowId}' ...");

				if (targets?.Any() == true)
				{
					foreach (var target in targets)
					{
						try
						{
							log.Log($"Running workflow on '{target}'.");
							contextService.Execute(
								new ExecuteWorkflowRequest
								{
									EntityId = target,
									WorkflowId = workflowId
								});
						}
						catch (Exception ex)
						{
							log.Log($"Failed at '{target}':'{ex.Message}'.");
							result.Exceptions[target] = ex;

							if (Job.ContinueOnError == false)
							{
								break;
							}
						}
					}
				}
				else
				{
					log.Log("No targets to run workflow on.");
				}

				log.Log($"Finished running workflow '{workflowId}'.");
			}
			else
			{
				var action = new OrganizationRequest(Job.ActionName);

				if (!string.IsNullOrEmpty(Job.SerialisedInputParams))
				{
					log.Log($"Inputs: '{Job.SerialisedInputParams}'.");
					action.Parameters.AddRange(GetInputs());
				}

				log.Log($"Executing action '{Job.ActionName}' ...");

				if (targets?.Any() == true)
				{
					foreach (var target in targets)
					{
						try
						{
							log.Log($"Executing action on '{target}' ...");
							action["Target"] = new EntityReference(Job.TargetLogicalName, target);
							contextService.Execute(action);
						}
						catch (Exception ex)
						{
							log.Log($"Failed at '{target}':'{ex.Message}'.");
							result.Exceptions[target] = ex;

							if (Job.ContinueOnError == false)
							{
								break;
							}
						}
					}
				}
				else if (string.IsNullOrEmpty(Job.TargetLogicalName))
				{
					try
					{
						log.Log($"Executing global action ...");
						contextService.Execute(action);
					}
					catch (Exception ex)
					{
						log.Log($"Failed: '{ex.Message}'.");
						result.Exceptions[Job.Id] = ex;
					}
				}

				log.Log($"Executed action '{Job.ActionName}'.");
			}

			return result;
		}

		[NoLog]
		protected IOrganizationService GetServiceInContext()
		{
			var contextService = Service;

			if (Job.ContextUser != null)
			{
				contextService = ServiceFactory.CreateOrganizationService(Job.ContextUser);
				log.Log($"Got context service for user '{Job.ContextUser}'.");
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
					log.Log($"Setting status of parent '{parentId}' to 'waiting' ...");
					SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, parentId, IsParentRecurrentJob());
					log.Log($"Set status of parent '{parentId}' to 'waiting'.");
				}
			}
			else if (IsRecurrentJob())
			{
				log.Log($"Setting status to 'waiting' ...");
				SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, Job.Id, IsParentRecurrentJob());
				log.Log($"Set status to 'waiting'.");
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
							log.Log($"Setting status of parent '{parentId}' to 'waiting' ...");
							SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, parentId, IsParentRecurrentJob());
							log.Log($"Set status of parent '{parentId}' to 'waiting'.");
						}
						else
						{
							log.Log($"Setting status of parent '{parentId}' to 'failed' ...");
							Close(Service, CustomJob.StatusReasonEnum.Failure, parentId, true);
							log.Log($"Set status of parent '{parentId}' to 'failed'.");
						}
					}

					status.ParentId = parentId;
				}

				if (Job.FailureAction != null)
				{
					log.Log("Running failure action ...");

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
						log.Log(exception);
					}

					log.Log("Finished running failure action.");
				}

				status.IsClose = true;
			}
			else
			{
				if (Job.RetrySchedule != null)
				{
					UpdateRetryTargetDate(Service, Job, log);
				}

				IncrementRetry(Job.CurrentRetryRun ?? 0);
				log.Log($"Setting status of job to 'retry' ...");
				SetStatus(Service, CustomJob.StatusReasonEnum.Retry, Job.Id, false);
				log.Log($"Set status of job to 'retry'.");
			}

			var failures = executionResult.Exceptions
				.Select(failure =>
					new CustomJobFailedTarget
					{
						ID = failure.Key.ToString(),
						FailureMessage = BuildNoTraceExceptionMessage(failure.Value)
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
				parameters = SerialiserHelpers.DeserialiseSimpleJson(Job.SerialisedInputParams)
					.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
			}
			catch
			{
				log.Log("Parameters are not in JSON format, or are poorly formatted JSON.");

				try
				{
					parameters = SerialiserHelpers.DeserialiseStrictXml<IDictionary<string, object>>(Job.SerialisedInputParams);
				}
				catch
				{
					log.Log("Parameters are not in XML format, or are poorly formatted XML.");
				}

				if (parameters == null)
				{
					throw new InvalidPluginExecutionException("Failed to parse input params.");
				}
			}

			return parameters;
		}

		protected bool IsRecurrentJob()
		{
			var result = Job.RecurrentJob == true;
			log.Log($"{result}");
			return result;
		}

		protected bool IsParentRecurrentJob()
		{
			var result = Job.ParentRecurrent == true;
			log.Log($"{result}");
			return result;
		}

		protected bool IsSuccessDelete()
		{
			var result = Job.DeleteOnSuccess == true;
			log.Log($"{result}");
			return result;
		}

		protected bool IsSubJob()
		{
			var result = Job.ParentJob != null;
			log.Log($"{result}");
			return result;
		}

		protected void IncrementRetry(int currentRun)
		{
			log.Log($"Updating current retry run to {currentRun + 1} ...");
			Service.Update(
				new CustomJob
				{
					Id = Job.Id,
					CurrentRetryRun = currentRun + 1
				});
			log.Log($"Updated current retry run to {currentRun + 1}.");
		}

		protected bool IsNoRetry()
		{
			var isNoRetry = (Job.MaxRetryCount == null || (Job.CurrentRetryRun ?? 0) >= Job.MaxRetryCount)
				&& Job.RetrySchedule == null;
			log.Log($"Is no retry: {isNoRetry}");
			return isNoRetry;
		}
	}
}
