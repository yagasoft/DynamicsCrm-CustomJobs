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
		private GenericConfiguration config;
		private string orgId;

		public JobsEngineJobHandlerLogic() : base(null, PluginStage.All)
		{ }

		[NoLog]
		protected override void ExecuteLogic()
		{
			orgId = context.OrganizationId.ToString();
			config = GetGenericConfig(service, orgId).ToEntity<GenericConfiguration>();
			config.DefaultCalendar.Require(nameof(config.DefaultCalendar));

			CustomJob target = null;
			CustomJob image = null;

			try
			{
				// get the triggering record
				var targetGeneric = (Entity)context.InputParameters["Target"];
				target = targetGeneric.ToEntity<CustomJob>();
				image = context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

				// sync
				if (context.Mode == 0)
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
							log.Log("Logging failure ...", LogLevel.Debug);
							service.Update(
								new CustomJob
								{
									Id = target.Id,
									LatestRunMessage = "Job handler failed to execute.\r\n" + BuildExceptionMessage(ex),
									PreviousTargetDate = DateTime.UtcNow,
									TargetDate = null
								});
							log.Log("Logged.");

							// if a recurrent job throws an exception (parent), then check for retry
							// normally those jobs create sub-jobs that handle their own retry, but this is in case
							// the main job itself fails, which should be extremely rare
							if (image?.RecurrentJob == true && image.RetrySchedule != null)
							{
								UpdateRetryTargetDate(service, target, log);

								log.Log($"Updating status of job to 'Waiting' ...");
								SetStatus(service, CustomJob.StatusReasonEnum.Waiting, target.Id, false);
								log.Log($"Updated status of job 'Waiting'.");
							}
						}
						catch
						{
							// ignored
						}

						if (image?.RetrySchedule == null)
						{
							log.Log($"Setting job to 'Failure' ...", LogLevel.Debug);
							service.Update(
								new CustomJob
								{
									Id = target.Id,
									Status = CustomJob.StatusEnum.Inactive,
									StatusReason = CustomJob.StatusReasonEnum.Failure
								});
							log.Log($"Set job to 'Failure'.");
						}
					}
				}
				catch
				{
					log.Log("Failed to set job 'failed' status.", LogLevel.Debug);
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
			if (context.Stage == 40)
			{
				log.Log("Processing post-operation actions ...", LogLevel.Debug);

				var postImage = context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

				if (postImage == null && context.MessageName == "Update")
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
					log.Log("Name is empty, updating ...", LogLevel.Debug);
					service.Update(
						new CustomJob
						{
							Id = customJob.Id,
							Name = BuildJobName(customJob)
						});
					log.Log("Name was empty, updated.");
				}

				return;
			}

			if (context.MessageName == "Update")
			{
				log.Log("Update message, fetching target job from CRM ...", LogLevel.Debug);
				customJob = service.Retrieve(targetGeneric.LogicalName, targetGeneric.Id, new ColumnSet(true))
					.ToEntity<CustomJob>();
				log.Log("Fetched target job from CRM.", LogLevel.Debug);
			}

			foreach (var attribute in targetGeneric.Attributes)
			{
				customJob[attribute.Key] = attribute.Value;
			}

			// fill page if empty
			if (customJob.RecordsPerPage != null && customJob.PageNumber == null)
			{
				log.Log("Records per page set, but not the page number; setting to '1'.");
				targetGeneric[CustomJob.Fields.PageNumber] = 1;
			}

			// clear failed targets as we are starting over and reset retry runs
			if (customJob.StatusReason == CustomJob.StatusReasonEnum.Draft)
			{
				log.Log("Draft status.");
				log.Log("Clearing retry run, page number, cookie, and last message.");
				targetGeneric[CustomJob.Fields.CurrentRetryRun] = null;
				targetGeneric[CustomJob.Fields.PageNumber] = null;
				targetGeneric[CustomJob.Fields.PagingCookie] = null;
				targetGeneric[CustomJob.Fields.LatestRunMessage] = null;

				log.Log("Loading failed targets for this job ...", LogLevel.Debug);
				var tempJob =
					new CustomJob
					{
						Id = customJob.Id
					};
				tempJob.LoadRelation(CustomJob.RelationNames.CustomJobFailedTargetsOfCustomJob, service);
				log.Log("Loaded failed targets for this job.", LogLevel.Debug);

				if (tempJob.CustomJobFailedTargetsOfCustomJob != null)
				{
					log.Log($"Failed targets: {tempJob.CustomJobFailedTargetsOfCustomJob.Length}.");
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

					log.Log($"Deleting failed targets ...", LogLevel.Debug);
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

						service.Execute(request);
					}
					log.Log($"Deleted failed targets.");
				}
			}

			// cancel active sub-jobs
			if (customJob.StatusReason == CustomJob.StatusReasonEnum.Draft
				|| customJob.StatusReason == CustomJob.StatusReasonEnum.Cancelled)
			{
				log.Log($"{customJob.StatusReason} status.");
				var tempJob =
					new CustomJob
					{
						Id = customJob.Id
					};

				log.Log("Loading active children of this job ...", LogLevel.Debug);
				var filter = new FilterExpression();
				filter.AddCondition(CustomJob.Fields.Status, ConditionOperator.Equal, (int)CustomJob.StatusEnum.Active);
				tempJob.LoadRelation(CustomJob.RelationNames.CustomJobsOfParentJob, service, false, filter);
				log.Log("Loaded active children of this job.", LogLevel.Debug);

				if (tempJob.CustomJobsOfParentJob != null)
				{
					log.Log($"Active children: {tempJob.CustomJobsOfParentJob.Length}.");
					foreach (var job in tempJob.CustomJobsOfParentJob)
					{
						log.Log($"Setting sub job '{job.Id}' to cancelled ...", LogLevel.Debug);
						service.Update(
							new CustomJob
							{
								Id = job.Id,
								Status = CustomJob.StatusEnum.Inactive,
								StatusReason = CustomJob.StatusReasonEnum.Cancelled
							});
						log.Log($"Set sub job '{job.Id}' to cancelled.");
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
				log.Log($"Set job name to '{targetGeneric[CustomJob.Fields.Name]}'.");
			}

			if (customJob.StatusReason == CustomJob.StatusReasonEnum.Draft
				&& customJob.MarkForWaiting == true && customJob.TargetDate != null
				&& context.MessageName != "Create")
			{
				log.Log("Setting job to waiting because of flag ...", LogLevel.Debug);
				targetGeneric[CustomJob.Fields.MarkForWaiting] = false;
				targetGeneric[CustomJob.Fields.StatusReason] = CustomJob.StatusReasonEnum.Waiting.ToOptionSetValue();
				log.Log("Set job to waiting because of flag.");
			}
		}

		private string BuildJobName(CustomJob customJob)
		{
			customJob.Require(nameof(customJob));

			CustomJob preImage = null;

			if (context.MessageName == "Update")
			{
				preImage = context.PreEntityImages?.FirstOrDefault().Value?.ToEntity<CustomJob>();

				if (preImage == null)
				{
					throw new InvalidPluginExecutionException("A full pre image must be registered on this plugin step.");
				}
			}

			if (context.MessageName == "Create" && !string.IsNullOrEmpty(customJob.Name))
			{
				log.Log("'Create' message and name is filled; using the custom name.");
				return customJob.Name;
			}

			var customJobTemp =
				new CustomJob
				{
					Id = customJob.Id,
					ActionName = customJob.ActionName,
					Workflow = customJob.Workflow
				};

			log.Log("Loading lookup labels ...", LogLevel.Debug);
			customJobTemp.LoadLookupLabels(service);
			log.Log("Loaded lookup labels.", LogLevel.Debug);

			var label = customJobTemp.WorkflowLabels?.FirstOrDefault(p => p.Key == 1033).Value;
			var newName = $"{customJob.TargetLogicalName}" +
				" (" + (string.IsNullOrEmpty(customJob.TargetID)
					? (string.IsNullOrEmpty(customJob.TargetXML) ? "no target" : "multi-target")
					: customJob.TargetID) + ")" +
				$"{(string.IsNullOrEmpty(customJobTemp.ActionName) ? "" : " : " + customJobTemp.ActionName)}" +
				$"{(string.IsNullOrEmpty(label) ? "" : " : " + label)}";
			log.Log($"Assumed new name: {newName}.");

			if (context.MessageName == "Update" && preImage != null)
			{
				log.Log($"Updating message; comparing updated name.", LogLevel.Debug);
				customJobTemp =
					new CustomJob
					{
						Id = customJob.Id,
						ActionName = preImage.ActionName,
						Workflow = preImage.Workflow
					};

				log.Log("Loading lookup labels of pre-image ...", LogLevel.Debug);
				customJobTemp.LoadLookupLabels(service);
				log.Log("Loaded lookup labels of pre-image.", LogLevel.Debug);

				var preLabel = customJobTemp.WorkflowLabels?.FirstOrDefault(p => p.Key == 1033).Value;
				var preName = $"{preImage.TargetLogicalName}" +
					" (" + (string.IsNullOrEmpty(preImage.TargetID)
						? (string.IsNullOrEmpty(preImage.TargetXML) ? "no target" : "multi-target")
						: preImage.TargetID) + ")" +
					$"{(string.IsNullOrEmpty(customJobTemp.ActionName) ? "" : " : " + customJobTemp.ActionName)}" +
					$"{(string.IsNullOrEmpty(preLabel) ? "" : " : " + preLabel)}";
				log.Log($"Pre-image name: {preName}.");

				var existingName = customJob.Name;
				newName = (string.IsNullOrEmpty(existingName) || preName == existingName) ? newName : existingName;
				log.Log($"Final new name: {newName}.");
			}

			return newName.Trim(' ').Trim(':').Trim(' ');
		}

		private void ValidateJobParams(CustomJob postImage)
		{
			postImage.Require(nameof(postImage));

			if (postImage.ActionName != null && postImage.Workflow != null)
			{
				throw new InvalidPluginExecutionException("Either an action or workflow can be specified.");
			}

			if (string.IsNullOrEmpty(postImage.ActionName) && postImage.Workflow == null)
			{
				throw new InvalidPluginExecutionException("An action or workflow must be specified.");
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

			var preImage = context.PreEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

			if (preImage == null)
			{
				if (context.MessageName == "Create")
				{
					preImage = targetGeneric.ToEntity<CustomJob>();
				}
				else
				{
					throw new InvalidPluginExecutionException("A full pre image must be registered on this plugin step.");
				}
			}

			var postImage = context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

			if (postImage == null)
			{
				throw new InvalidPluginExecutionException("A full post image must be registered on this plugin step.");
			}

			var target = targetGeneric.ToEntity<CustomJob>();
			log.SetRegarding(target.LogicalName, target.Id, postImage.Name);
			log.SetTitle(postImage, "ldv_name");

			if (postImage.StatusReason != CustomJob.StatusReasonEnum.Queued)
			{
				log.Log($"'{postImage.StatusReason}' status.");
				var timerModified = target.Timer != preImage.Timer || target.TimerBase != preImage.TimerBase;
				var targetDateEmpty = postImage.TargetDate == null;
				var timerParamSet = postImage.Timer != null;

				if ((timerModified || targetDateEmpty) && timerParamSet)
				{
					log.Log("Timer modified. Calculating target date ...", LogLevel.Debug);

					var baseDate = postImage.TimerBase ?? DateTime.UtcNow;
					var timer = postImage.Timer ?? 1;
					var targetDate =
						postImage.OnlyWorkingHours == true
							? SlaHelpers.GetDueDate(Guid.Parse(config.DefaultCalendar), baseDate, timer, service, orgId)
							: baseDate.AddMinutes(timer);

					log.Log($"New target date: '{targetDate}'.");

					log.Log("Setting job target date ...", LogLevel.Debug);
					service.Update(
						new CustomJob
						{
							Id = target.Id,
							TargetDate = targetDate,
							TimerBase = baseDate
						});
					log.Log("Set job target date.");
				}

				log.Log("Job is not queued, exiting ...");
				return;
			}

			ProcessQueuedJob(postImage);
		}

		private void ProcessQueuedJob(CustomJob postImage)
		{
			postImage.Require(nameof(postImage));

			var isRetry = context.PreEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>().StatusReason
				== CustomJob.StatusReasonEnum.Retry;

			if (isRetry)
			{
				log.Log("Retrying ...");
			}

			log.Log("Setting job to 'running' ...", LogLevel.Debug);
			service.Update(
				new CustomJob
				{
					Id = postImage.Id,
					StatusReason = CustomJob.StatusReasonEnum.Running
				});
			log.Log("Set job to 'running'.");

			var run = JobRunFactory.GetJobRun(postImage, isRetry, service, serviceFactory, log);
			run.Process();
		}
	}
}
