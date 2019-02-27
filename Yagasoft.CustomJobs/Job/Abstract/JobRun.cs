#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;
using static Yagasoft.CustomJobs.Helpers;

#endregion

namespace Yagasoft.CustomJobs.Job.Abstract
{
	internal class JobRunStatus
	{
		internal bool IsSuccess { get; set; }
		internal bool IsClose { get; set; }
		internal string LatestRunMessage { get; set; }
		internal Guid? ParentId { get; set; }
		internal List<CustomJobFailedTarget> RunTargetFailures { get; set; }
	}

	[Log]
	internal abstract class JobRun
	{
		protected IOrganizationService Service;
		protected CustomJob Job;
		protected CrmLog log;

		protected JobRun(CustomJob job, IOrganizationService service, CrmLog log)
		{
			Job = job;
			Service = service;
			this.log = log;
		}

		internal void Process()
		{
			var runStatus = ProcessRecurrence();
			LogRunStatus(runStatus);
			PostRunActions(runStatus);
		}

		private void LogRunStatus(JobRunStatus runStatus)
		{
			if (!runStatus.IsSuccess && runStatus.RunTargetFailures?.Any() == true)
			{
				runStatus.LatestRunMessage = "Failed run.\r\n\r\nFirst Exception: " +
					runStatus.RunTargetFailures.FirstOrDefault()?.FailureMessage;
			}
			else
			{
				runStatus.LatestRunMessage = runStatus.LatestRunMessage
					?? (runStatus.IsSuccess ? "Successful run." : "Failed run.");
			}

			log.Log($"Updating latest run message: '{runStatus.LatestRunMessage}' ...", LogLevel.Debug);
			log.Log($"Updating latest date: '{Job.TargetDate ?? DateTime.UtcNow}' UTC ...", LogLevel.Debug);
			Service.Update(
				new CustomJob
				{
					Id = Job.Id,
					LatestRunMessage = runStatus.LatestRunMessage,
					PreviousTargetDate = Job.TargetDate ?? DateTime.UtcNow
				});
			log.Log($"Updated latest run message: '{runStatus.LatestRunMessage}'.");
			log.Log($"Updated latest date: '{Job.TargetDate ?? DateTime.UtcNow}' UTC.");

			if (Job.GenerateLogs == true)
			{
				log.Log("Creating log entry ...", LogLevel.Debug);
				var logEntry =
					new CustomJobLog
					{
						Message = DateTime.Now + ": " + (runStatus.IsSuccess ? "Successful run." : "Failed run."),
						ExecutionDate = Job.TargetDate ?? DateTime.UtcNow,
						ExecutionFullMessage = runStatus.LatestRunMessage,
						CustomJob = Job.Id,
						StatusReason = runStatus.IsSuccess
							? CustomJobLog.StatusReasonEnum.Success
							: CustomJobLog.StatusReasonEnum.Failure
					};

				if (runStatus.RunTargetFailures?.Any() == true)
				{
					runStatus.RunTargetFailures.ForEach(failure => failure.CustomJob = null);
					logEntry.CustomJobFailedTargetsOfLogEntry = runStatus.RunTargetFailures.ToArray();
				}

				Service.Create(logEntry);
				log.Log("Created log entry.");

				if (runStatus.ParentId.HasValue)
				{
					logEntry.Message += " [Sub-Job]";
					logEntry.CustomJob = runStatus.ParentId.Value;
					Service.Create(logEntry);
					log.Log("Created log entry in parent.");
				}
			}
		}

		private void PostRunActions(JobRunStatus runStatus)
		{
			if (runStatus.IsClose)
			{
				var finalStatus = runStatus.IsSuccess
					? CustomJob.StatusReasonEnum.Success
					: CustomJob.StatusReasonEnum.Failure;

				Close(Service, finalStatus, Job.Id);
			}

			if (Job.DeleteOnSuccess == true
				&& runStatus.IsSuccess && runStatus.IsClose && runStatus.RunTargetFailures?.Any() != true)
			{
				log.Log("Checking target failures ...");
				var job = new CustomJob { Id = Job.Id };
				job.LoadRelation(CustomJob.RelationNames.CustomJobFailedTargetsOfCustomJob, Service, false, 1);
				var isFailedExist = job.CustomJobFailedTargetsOfCustomJob?.Any() == true;
				log.Log($"Failed exist: {isFailedExist}.");

				if (!isFailedExist)
				{
					log.Log("Deleting job ...", LogLevel.Debug);
					Service.Delete(CustomJob.EntityLogicalName, Job.Id);
					log.Log("Deleted job.");
				}
			}
		}

		protected abstract JobRunStatus ProcessRecurrence();
	}
}
