//         Project / File: Yagasoft.Plugins.Common / CustomJobHandler.cs

#region Imports

using System;
using System.Linq;
using Yagasoft.CustomJobs.Job;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using static Yagasoft.CustomJobs.Helpers;
using static Yagasoft.Libraries.Common.CrmHelpers;

#endregion

namespace Yagasoft.CustomJobs
{
	/// <summary>
	///     This plugin ... .<br />
	///     Version: 0.1.1
	/// </summary>
	public class JobsEngineJobHandler : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			new JobsEngineJobHandlerLogic().Execute(this, serviceProvider, false);
		}
	}

	[Log]
	internal class JobsEngineJobHandlerLogic : PluginLogic<JobsEngineJobHandler>
	{
		private CommonConfiguration config;
		private Guid orgId;

		public JobsEngineJobHandlerLogic() : base(null, PluginStage.All)
		{ }

		[NoLog]
		protected override void ExecuteLogic()
		{
			orgId = Context.OrganizationId;
			config = GetGenericConfig(Service, orgId).ToEntity<CommonConfiguration>();

			CustomJob target = null;
			CustomJob image = null;

			try
			{
				// get the triggering record
				var targetGeneric = (Entity)Context.InputParameters["Target"];
				target = targetGeneric.ToEntity<CustomJob>();
				image = Context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

				// sync
				if (Context.Mode == 0)
				{
					PerformSyncActions(targetGeneric);
					return;
				}

				PerformAsyncActions(targetGeneric);
			}
			catch (Exception ex)
			{
				try
				{
					if (target?.Id != null)
					{
						try
						{
							Log.Log("Logging failure ...");
							Service.Update(
								new CustomJob
								{
									Id = target.Id,
									LatestRunMessage = "Job handler failed to execute.\r\n" + BuildExceptionMessage(ex),
									PreviousTargetDate = DateTime.UtcNow,
									TargetDate = null
								});
							Log.Log("Logged.");

							// if a recurrent job throws an exception (parent), then check for retry
							// normally those jobs create sub-jobs that handle their own retry, but this is in case
							// the main job itself fails, which should be extremely rare
							if (image?.RecurrentJob == true && image.RetrySchedule != null)
							{
								UpdateRetryTargetDate(Service, target, Log);

								Log.Log($"Updating status of job to 'Waiting' ...");
								SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, target.Id, false);
								Log.Log($"Updated status of job 'Waiting'.");
							}
						}
						catch
						{
							// ignored
						}

						if (image?.RetrySchedule == null)
						{
							Log.Log($"Setting job to 'Failure' ...");
							Service.Update(
								new CustomJob
								{
									Id = target.Id,
									Status = CustomJob.StatusEnum.Inactive,
									StatusReason = CustomJob.StatusReasonEnum.Failure
								});
							Log.Log($"Set job to 'Failure'.");
						}
					}
				}
				catch
				{
					Log.Log("Failed to set job 'failed' status.");
				}

				throw;
			}
		}

		#region Sync

		private void PerformSyncActions(Entity targetGeneric)
		{
			targetGeneric.Require(nameof(targetGeneric));

			var customJob = new CustomJob();

			// post
			if ((Context as IPluginExecutionContext)?.Stage == 40)
			{
				Log.Log("Processing post-operation actions ...");

				var postImage = Context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

				if (postImage == null && Context.MessageName == "Update")
				{
					throw new InvalidPluginExecutionException("A full post image must be registered on this plugin step.");
				}

				postImage = postImage ?? targetGeneric.ToEntity<CustomJob>();

				foreach (var attribute in postImage.Attributes)
				{
					customJob[attribute.Key] = attribute.Value;
				}

				ValidateJobParams(customJob);

				if (string.IsNullOrEmpty(customJob.Name))
				{
					Log.Log("Name is empty, updating ...");
					Service.Update(
						new CustomJob
						{
							Id = customJob.Id,
							Name = BuildJobName(customJob)
						});
					Log.Log("Name was empty, updated.");
				}

				return;
			}

			if (Context.MessageName == "Update")
			{
				Log.Log("Update message, fetching target job from CRM ...");
				customJob = Service.Retrieve(targetGeneric.LogicalName, targetGeneric.Id, new ColumnSet(true))
					.ToEntity<CustomJob>();
				Log.Log("Fetched target job from CRM.");
			}

			foreach (var attribute in targetGeneric.Attributes)
			{
				customJob[attribute.Key] = attribute.Value;
			}

			// fill page if empty
			if (customJob.RecordsPerPage != null && customJob.PageNumber == null)
			{
				Log.Log("Records per page set, but not the page number; setting to '1'.");
				targetGeneric[CustomJob.Fields.PageNumber] = 1;
			}

			// clear failed targets as we are starting over and reset retry runs
			if (customJob.StatusReason == CustomJob.StatusReasonEnum.Draft)
			{
				Log.Log("Draft status.");
				Log.Log("Clearing retry run, page number, cookie, and last message.");
				targetGeneric[CustomJob.Fields.CurrentRetryRun] = null;
				targetGeneric[CustomJob.Fields.PageNumber] = null;
				targetGeneric[CustomJob.Fields.PagingCookie] = null;
				targetGeneric[CustomJob.Fields.LatestRunMessage] = null;

				Log.Log("Loading failed targets for this job ...");
				var tempJob =
					new CustomJob
					{
						Id = customJob.Id
					};
				tempJob.LoadRelation(CustomJob.RelationNames.CustomJobFailedTargetsOfCustomJob, Service);
				Log.Log("Loaded failed targets for this job.");

				if (tempJob.CustomJobFailedTargetsOfCustomJob != null)
				{
					Log.Log($"Failed targets: {tempJob.CustomJobFailedTargetsOfCustomJob.Length}.");
					var failures = tempJob.CustomJobFailedTargetsOfCustomJob;

					var request =
						new ExecuteMultipleRequest
						{
							Requests = new OrganizationRequestCollection(),
							Settings =
								new ExecuteMultipleSettings
								{
									ContinueOnError = true,
									ReturnResponses = false
								}
						};

					Log.Log($"Deleting failed targets ...");
					for (var i = 0; i < Math.Ceiling(failures.Length / 1000d); i++)
					{
						request.Requests.Clear();
						request.Requests.AddRange(failures.Skip(i * 1000).Take(1000)
							.Select(failure =>
								new DeleteRequest
								{
									Target = new EntityReference(CustomJobFailedTarget.EntityLogicalName,
										failure.Id)
								}));

						Service.Execute(request);
					}
					Log.Log($"Deleted failed targets.");
				}
			}

			// cancel active sub-jobs
			if (customJob.StatusReason == CustomJob.StatusReasonEnum.Draft
				|| customJob.StatusReason == CustomJob.StatusReasonEnum.Cancelled)
			{
				Log.Log($"{customJob.StatusReason} status.");
				var tempJob =
					new CustomJob
					{
						Id = customJob.Id
					};

				Log.Log("Loading active children of this job ...");
				var filter = new FilterExpression();
				filter.AddCondition(CustomJob.Fields.Status, ConditionOperator.Equal, (int)CustomJob.StatusEnum.Active);
				tempJob.LoadRelation(CustomJob.RelationNames.CustomJobsOfParentJob, Service, false, filter);
				Log.Log("Loaded active children of this job.");

				if (tempJob.CustomJobsOfParentJob != null)
				{
					Log.Log($"Active children: {tempJob.CustomJobsOfParentJob.Length}.");
					foreach (var job in tempJob.CustomJobsOfParentJob)
					{
						Log.Log($"Setting sub job '{job.Id}' to cancelled ...");
						Service.Update(
							new CustomJob
							{
								Id = job.Id,
								Status = CustomJob.StatusEnum.Inactive,
								StatusReason = CustomJob.StatusReasonEnum.Cancelled
							});
						Log.Log($"Set sub job '{job.Id}' to cancelled.");
					}
				}
			}

			var isNamingFieldsUpdated = targetGeneric.Attributes.Keys
				.Any(key => new[]
							{
								CustomJob.Fields.TargetLogicalName,
								CustomJob.Fields.ActionName,
								CustomJob.Fields.Workflow,
								CustomJob.Fields.TargetID,
								CustomJob.Fields.TargetXML
							}.Contains(key));

			if (string.IsNullOrEmpty(customJob.Name) || isNamingFieldsUpdated)
			{
				targetGeneric[CustomJob.Fields.Name] = BuildJobName(customJob);
				Log.Log($"Set job name to '{targetGeneric[CustomJob.Fields.Name]}'.");
			}

			if (customJob.StatusReason == CustomJob.StatusReasonEnum.Draft
				&& customJob.MarkForWaiting == true && customJob.TargetDate != null
				&& Context.MessageName != "Create")
			{
				Log.Log("Setting job to waiting because of flag ...");
				targetGeneric[CustomJob.Fields.MarkForWaiting] = false;
				targetGeneric[CustomJob.Fields.StatusReason] = CustomJob.StatusReasonEnum.Waiting.ToOptionSetValue();
				Log.Log("Set job to waiting because of flag.");
			}
		}

		private string BuildJobName(CustomJob customJob)
		{
			customJob.Require(nameof(customJob));

			CustomJob preImage = null;

			if (Context.MessageName == "Update")
			{
				preImage = Context.PreEntityImages?.FirstOrDefault().Value?.ToEntity<CustomJob>();

				if (preImage == null)
				{
					throw new InvalidPluginExecutionException("A full pre image must be registered on this plugin step.");
				}
			}

			if (Context.MessageName == "Create" && !string.IsNullOrEmpty(customJob.Name))
			{
				Log.Log("'Create' message and name is filled; using the custom name.");
				return customJob.Name;
			}

			var customJobTemp =
				new CustomJob
				{
					Id = customJob.Id,
					ActionName = customJob.ActionName,
					Workflow = customJob.Workflow
				};

			Log.Log("Loading lookup labels ...");
			customJobTemp.LoadLookupLabels(Service);
			Log.Log("Loaded lookup labels.");

			var label = customJobTemp.WorkflowLabels?.FirstOrDefault(p => p.Key == 1033).Value;
			var newName = $"{customJob.TargetLogicalName}" +
				" (" + (string.IsNullOrEmpty(customJob.TargetID)
					? (string.IsNullOrEmpty(customJob.TargetXML) ? "no target" : "multi-target")
					: customJob.TargetID) + ")" +
				$"{(string.IsNullOrEmpty(customJobTemp.ActionName) ? "" : " : " + customJobTemp.ActionName)}" +
				$"{(string.IsNullOrEmpty(label) ? "" : " : " + label)}";
			Log.Log($"Assumed new name: {newName}.");

			if (Context.MessageName == "Update" && preImage != null)
			{
				Log.Log($"Updating message; comparing updated name.");
				customJobTemp =
					new CustomJob
					{
						Id = customJob.Id,
						ActionName = preImage.ActionName,
						Workflow = preImage.Workflow
					};

				Log.Log("Loading lookup labels of pre-image ...");
				customJobTemp.LoadLookupLabels(Service);
				Log.Log("Loaded lookup labels of pre-image.");

				var preLabel = customJobTemp.WorkflowLabels?.FirstOrDefault(p => p.Key == 1033).Value;
				var preName = $"{preImage.TargetLogicalName}" +
					" (" + (string.IsNullOrEmpty(preImage.TargetID)
						? (string.IsNullOrEmpty(preImage.TargetXML) ? "no target" : "multi-target")
						: preImage.TargetID) + ")" +
					$"{(string.IsNullOrEmpty(customJobTemp.ActionName) ? "" : " : " + customJobTemp.ActionName)}" +
					$"{(string.IsNullOrEmpty(preLabel) ? "" : " : " + preLabel)}";
				Log.Log($"Pre-image name: {preName}.");

				var existingName = customJob.Name;
				newName = (string.IsNullOrEmpty(existingName) || preName == existingName) ? newName : existingName;
				Log.Log($"Final new name: {newName}.");
			}

			return newName.Trim(' ').Trim(':').Trim(' ');
		}

		private void ValidateJobParams(CustomJob postImage)
		{
			postImage.Require(nameof(postImage));

			if (postImage.ActionName != null && postImage.Workflow != null && postImage.URL.IsFilled())
			{
				throw new InvalidPluginExecutionException("Either an action or workflow or URL can be specified.");
			}

			if (string.IsNullOrEmpty(postImage.ActionName) && postImage.Workflow == null && postImage.URL.IsEmpty())
			{
				throw new InvalidPluginExecutionException("An action or workflow or URL must be specified.");
			}

			if (postImage.TargetID != null && postImage.TargetXML != null)
			{
				throw new InvalidPluginExecutionException("Either a target ID or XML can be specified.");
			}

			if ((postImage.TargetID != null || postImage.TargetXML != null)
				&& string.IsNullOrEmpty(postImage.TargetLogicalName))
			{
				throw new InvalidPluginExecutionException("Target logical name must be specified.");
			}
		}

		#endregion

		private void PerformAsyncActions(Entity targetGeneric)
		{
			targetGeneric.Require(nameof(targetGeneric));

			var preImage = Context.PreEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

			if (preImage == null)
			{
				if (Context.MessageName == "Create")
				{
					preImage = targetGeneric.ToEntity<CustomJob>();
				}
				else
				{
					throw new InvalidPluginExecutionException("A full pre image must be registered on this plugin step.");
				}
			}

			var postImage = Context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

			if (postImage == null)
			{
				throw new InvalidPluginExecutionException("A full post image must be registered on this plugin step.");
			}

			var target = targetGeneric.ToEntity<CustomJob>();
			Log.SetRegarding(target.LogicalName, target.Id, postImage.Name);
			Log.SetTitle(postImage, "ldv_name");

			if (postImage.StatusReason != CustomJob.StatusReasonEnum.Queued)
			{
				Log.Log($"'{postImage.StatusReason}' status.");
				var timerModified = target.Timer != preImage.Timer || target.TimerBase != preImage.TimerBase;
				var targetDateEmpty = postImage.TargetDate == null;
				var timerParamSet = postImage.Timer != null;

				if ((timerModified || targetDateEmpty) && timerParamSet)
				{
					Log.Log("Timer modified. Calculating target date ...");

					var baseDate = postImage.TimerBase ?? DateTime.UtcNow;
					var timer = postImage.Timer ?? 1;
					var targetDate =
						postImage.OnlyWorkingHours == true && config.DefaultCalendar.IsNotEmpty()
							? SlaHelpers.GetDueDate(Guid.Parse(config.DefaultCalendar), baseDate, timer, Service, orgId)
							: baseDate.AddMinutes(timer);

					Log.Log($"New target date: '{targetDate}'.");

					Log.Log("Setting job target date ...");
					Service.Update(
						new CustomJob
						{
							Id = target.Id,
							TargetDate = targetDate,
							TimerBase = baseDate
						});
					Log.Log("Set job target date.");
				}

				Log.Log("Job is not queued, exiting ...");
				return;
			}

			if (config.JobsPlatform != CommonConfiguration.JobsPlatformEnum.CRM)
			{
				throw new InvalidPluginExecutionException($"Cannot process the job because target platform is not set to CRM"
					+ $" (current: {config.JobsPlatform}).");
			}
			
			ProcessQueuedJob(postImage);
		}

		private void ProcessQueuedJob(CustomJob postImage)
		{
			postImage.Require(nameof(postImage));

			var isRetry = Context.PreEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>().StatusReason
				== CustomJob.StatusReasonEnum.Retry;

			if (isRetry)
			{
				Log.Log("Retrying ...");
			}

			Log.Log("Setting job to 'running' ...");
			Service.Update(
				new CustomJob
				{
					Id = postImage.Id,
					StatusReason = CustomJob.StatusReasonEnum.Running
				});
			Log.Log("Set job to 'running'.");

			var run = JobRunFactory.GetJobRun(postImage, isRetry, Service, ServiceFactory, Log);
			run.Process();
		}
	}
}
