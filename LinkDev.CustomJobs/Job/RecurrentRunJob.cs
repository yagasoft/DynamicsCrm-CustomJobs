#region Imports

using System;
using LinkDev.CustomJobs.Job.Abstract;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace LinkDev.CustomJobs.Job
{
	[Log]
	internal class RecurrentRunJob : JobRun
	{
		public RecurrentRunJob(CustomJob job, IOrganizationService service, CrmLog log) : base(job, service, log)
		{ }

		protected override JobRunStatus ProcessRecurrence()
		{
			log.Log("Creating sub-job ...", LogLevel.Debug);
			var extendingJob = Helpers.BuildSubJob(Job, CustomJob.StatusReasonEnum.Waiting, $"Recurrence '{Job.TargetDate}' UTC");
			Service.Create(extendingJob);
			log.Log($"Created sub-job '{extendingJob.Name}'.");

			log.Log("Updating status to 'waiting on subs' ...", LogLevel.Debug);
			Service.Update(
				new CustomJob
				{
					Id = Job.Id,
					StatusReason = CustomJob.StatusReasonEnum.WaitingOnSubJobs
				});
			log.Log("Updated status to 'waiting on subs'.");

			return
				new JobRunStatus
				{
					IsSuccess = true,
					LatestRunMessage = "Waiting on sub jobs."
				};
		}
	}
}
