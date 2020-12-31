#region Imports

using System;
using System.Activities;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

#endregion

namespace Yagasoft.CustomJobs
{
	public class JobsEngineQueueJobs : CodeActivity
	{
		protected override void Execute(CodeActivityContext context)
		{
			new JobsEngineQueueJobsLogic().Execute(this, context, false);
		}
	}

	internal class JobsEngineQueueJobsLogic : StepLogic<JobsEngineQueueJobs>
	{
		protected override void ExecuteLogic()
		{
			var xrmContext = new XrmServiceContext(Service) { MergeOption = MergeOption.NoTracking };

			// get the triggering record
			var target = (Entity)Context.InputParameters["Target"];
			Log.SetRegarding(Context.PrimaryEntityName, target.Id);

			var config = CrmHelpers.GetGenericConfig(Service, Context.OrganizationId.ToString()).ToEntity<CommonConfiguration>();

			if (config.JobsPlatform != CommonConfiguration.JobsPlatformEnum.CRM)
			{
				throw new InvalidPluginExecutionException($"Cannot queue the job because target platform is not set to CRM"
					+ $" (current: {config.JobsPlatform}).");
			}
			
			Log.Log("Fetching job records ...");
			var jobRecordsQuery =
				from job in xrmContext.CustomJobSet
				where ((job.TargetDate == null || job.TargetDate < DateTime.UtcNow)
					&& (job.StatusReason == CustomJob.StatusReasonEnum.Waiting
						|| job.StatusReason == CustomJob.StatusReasonEnum.Retry))
					|| (job.ModifiedOn < DateTime.UtcNow.AddMinutes(-config.JobTimeout.GetValueOrDefault(20))
						&& (job.StatusReason == CustomJob.StatusReasonEnum.Running
							|| job.StatusReason == CustomJob.StatusReasonEnum.Queued))
				select new
					   {
						   job.CustomJobId,
						   job.StatusReason
					   };

			var maxJobsPerRun = xrmContext.CustomJobEngineSet.Select(engine => engine.MaxJobsPerRun).FirstOrDefault();

			if (maxJobsPerRun > 0)
			{
				jobRecordsQuery = jobRecordsQuery.Take(maxJobsPerRun.Value);
			}

			var jobRecords = jobRecordsQuery.ToList();

			Log.Log($"Fetched job records '{jobRecords.Count}'.");

			Log.Log("Fetching parent job records ...");

			var parentQuery = new FetchExpression(
				$@"
<fetch no-lock='true'>
  <entity name='ldv_customjob'>
    <attribute name='ldv_customjobid' />
    <attribute name='statuscode' />
    <filter>
      <condition attribute='statuscode' operator='eq' value='{CustomJob.StatusReasonEnum.WaitingOnSubJobs}' />
      <condition entityname='subjob' attribute='ldv_customjobid' operator='null' />
    </filter>
    <link-entity name='ldv_customjob' from='ldv_parentjobid' to='ldv_customjobid' link-type='outer' alias='subjob' >
      <filter>
        <condition attribute='statecode' operator='eq' value='0' />
      </filter>
    </link-entity>
  </entity>
</fetch>");
			var parentJobs = Service.RetrieveMultiple(parentQuery).Entities;

			Log.Log($"Fetched parent job records '{jobRecords.Count}'.");

			jobRecords = jobRecords
				.Union(parentJobs
					.Select(j => j.ToEntity<CustomJob>())
					.Select(j => new { j.CustomJobId, j.StatusReason }))
				.Where(j => j.CustomJobId.HasValue).ToList();
			
			Log.Log($"Fetched" +
				$" {jobRecords.Count(job => new [] { CustomJob.StatusReasonEnum.Queued, CustomJob.StatusReasonEnum.Running, CustomJob.StatusReasonEnum.WaitingOnSubJobs }.Contains(job.StatusReason.GetValueOrDefault()))}"
				+ $" stalled job records.");

			if (jobRecords.Any())
			{
				foreach (var jobRecord in jobRecords)
				{
					Service.Update(
						new CustomJob
						{
							Id = jobRecord.CustomJobId.GetValueOrDefault(),
							StatusReason = CustomJob.StatusReasonEnum.Queued,
							RunTrigger = new Random().Next(11111, 999999).ToString()
						});
				}

				Log.Log(new LogEntry("Queue Information",
					information: jobRecords.Select(record => record.ToString()).Aggregate((r1, r2) => r1 + "\r\n" + r2)));
			}

			Log.Log("Fetching date-corrupted records ...");
			var corruptJobs =
				(from job in xrmContext.CustomJobSet
				 where job.TargetDate > new DateTime(2099, 1, 1) && job.StatusReason == CustomJob.StatusReasonEnum.Waiting
					 && job.RecurrentJob == true
				 select job.CustomJobId).ToArray().Where(id => id.HasValue).Select(id => id.GetValueOrDefault()).ToArray();
			Log.Log($"Fetched {corruptJobs.Length} date-corrupted records.");

			if (corruptJobs.Any())
			{
				Log.Log("Triggering recurrence recalculation of records ...");
				foreach (var jobId in corruptJobs)
				{
					Service.Update(
						new CustomJob
						{
							Id = jobId,
							RecurrenceUpdatedTrigger = new Random().Next(111111, 99999999).ToString()
						});
					Log.Log($"Triggered recurrence recalculation of '{jobId}'.");
				}
				Log.Log("Finished triggering recurrence recalculation of records.");
			}
		}
	}
}
