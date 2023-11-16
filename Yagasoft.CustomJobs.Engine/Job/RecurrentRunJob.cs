#region Imports

using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job
{
	[Log]
	internal class RecurrentRunJob : JobRun
	{
		public RecurrentRunJob(CustomJob job, EngineParams engineParams, IOrganizationService service, ILogger log)
			: base(job, engineParams, service, log)
		{ }

		protected override JobRunStatus ProcessRecurrence()
		{
			log.Log("Creating sub-job ...");
			var extendingJob = Helpers.BuildSubJob(Job, CustomJob.StatusReasonEnum.Waiting, $"Recurrence '{Job.TargetDate}' UTC");
			Service.Create(extendingJob);
			log.Log($"Created sub-job '{extendingJob.Name}'.");

			log.Log("Updating status to 'waiting on subs' ...");
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
