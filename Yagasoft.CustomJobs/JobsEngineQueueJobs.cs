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
			var xrmContext = new XrmServiceContext(service) { MergeOption = MergeOption.NoTracking };

			// get the triggering record
			var target = (Entity)context.InputParameters["Target"];
			log.SetRegarding(context.PrimaryEntityName, target.Id);

			log.Log("Fetching job records ...", LogLevel.Debug);
			var jobRecordsQuery =
				from job in xrmContext.CustomJobSet
				where ((job.TargetDate == null || job.TargetDate < DateTime.UtcNow)
					&& (job.StatusReason == CustomJob.StatusReasonEnum.Waiting
						|| job.StatusReason == CustomJob.StatusReasonEnum.Retry))
					|| (job.ModifiedOn < DateTime.UtcNow.AddMinutes(-8)
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

			log.Log($"Fetched job records '{jobRecords.Count}'.");

			log.Log("Fetching parent job records ...");

			var parentQuery = new FetchExpression(
				"<fetch distinct='true' >" +
					"  <entity name='ldv_customjob' >" +
					"    <attribute name='ldv_customjobid' />" +
					"    <attribute name='statuscode' />" +
					"    <filter>" +
					"      <condition attribute='statuscode' operator='eq' value='753240009' />" +
					"      <condition entityname='subjob' attribute='ldv_customjobid' operator='null' />" +
					"    </filter>" +
					"    <link-entity name='ldv_customjob' from='ldv_parentjobid' to='ldv_customjobid' link-type='outer' alias='subjob' >" +
					"      <filter>" +
					"        <condition attribute='statecode' operator='eq' value='0' />" +
					"      </filter>" +
					"    </link-entity>" +
					"  </entity>" +
					"</fetch>");
			var parentJobs = service.RetrieveMultiple(parentQuery).Entities;

			log.Log($"Fetched parent job records '{jobRecords.Count}'.");

			jobRecords = jobRecords.Union(parentJobs.Select(j => j.ToEntity<CustomJob>())
				.Select(j => new { j.CustomJobId, j.StatusReason })).ToList();
			
			log.Log($"Fetched" +
				$" {jobRecords.Count(job => new [] { CustomJob.StatusReasonEnum.Queued, CustomJob.StatusReasonEnum.Running, CustomJob.StatusReasonEnum.WaitingOnSubJobs }.Contains(job.StatusReason.GetValueOrDefault()))}"
				+ $" stalled job records.");

			if (jobRecords.Any())
			{

				foreach (var jobRecord in jobRecords)
				{
					service.Update(
						new CustomJob
						{
							Id = jobRecord.CustomJobId.Value,
							StatusReason = CustomJob.StatusReasonEnum.Queued,
							RunTrigger = new Random().Next(11111, 999999).ToString()
						});
				}

				log.Log(new LogEntry("Queue Information",
					information: jobRecords.Select(record => record.ToString()).Aggregate((r1, r2) => r1 + "\r\n" + r2)));
			}

			log.Log("Fetching date-corrupted records ...");
			var corruptJobs =
				(from job in xrmContext.CustomJobSet
				 where job.TargetDate > new DateTime(2099, 1, 1) && job.StatusReason == CustomJob.StatusReasonEnum.Waiting
					 && job.RecurrentJob == true
				 select job.CustomJobId).ToArray().Where(id => id.HasValue).Select(id => id.GetValueOrDefault()).ToArray();
			log.Log($"Fetched {corruptJobs.Length} date-corrupted records.");

			if (corruptJobs.Any())
			{
				log.Log("Triggering recurrence recalculation of records ...");
				foreach (var jobId in corruptJobs)
				{
					service.Update(
						new CustomJob
						{
							Id = jobId,
							RecurrenceUpdatedTrigger = new Random().Next(111111, 99999999).ToString()
						});
					log.Log($"Triggered recurrence recalculation of '{jobId}'.");
				}
				log.Log("Finished triggering recurrence recalculation of records.");
			}
		}
	}
}
