#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine
{
	public class CustomJobEngine
	{
		public BlockingQueue<Guid> Jobs = new();

		private readonly TimeSpan timeout;

		private readonly object jobQueryLock = new();

		public CustomJobEngine(TimeSpan timeout)
		{
			this.timeout = timeout;
		}

		public IEnumerable<Guid> GetNextJobBatch(IOrganizationService service, CrmLog log)
		{
			service.Require(nameof(service));
			log.Require(nameof(log));

			var xrmContext = new XrmServiceContext(service) { MergeOption = MergeOption.NoTracking };

			log.LogInfo("Fetching job records ...");

			CustomJob[] jobRecords;

			lock (jobQueryLock)
			{
				jobRecords =
					(from job in xrmContext.CustomJobSet
					 where ((job.TargetDate == null || job.TargetDate < DateTime.UtcNow)
						 && (job.StatusReason == CustomJob.StatusReasonEnum.Waiting
							 || job.StatusReason == CustomJob.StatusReasonEnum.Retry))
						 || (job.ModifiedOn < (DateTime.UtcNow - timeout)
							 && (job.StatusReason == CustomJob.StatusReasonEnum.Running
								 || job.StatusReason == CustomJob.StatusReasonEnum.Queued))
					 select new CustomJob
							{
								CustomJobId = job.CustomJobId,
								StatusReason = job.StatusReason
							}).ToArray();
			}

			log.LogInfo($"Fetched job records '{jobRecords.Length}'.");

			log.LogInfo("Fetching parent job records with no children ...");

			var parentQuery = new FetchExpression(
				string.Intern($@"<fetch no-lock='true'>
  <entity name='ldv_customjob'>
    <attribute name='ldv_customjobid' />
    <attribute name='statuscode' />
    <filter>
      <condition attribute='statuscode' operator='eq' value='{(int)CustomJob.StatusReasonEnum.WaitingOnSubJobs}' />
      <condition entityname='subjob' attribute='ldv_customjobid' operator='null' />
    </filter>
    <link-entity name='ldv_customjob' from='ldv_parentjobid' to='ldv_customjobid' link-type='outer' alias='subjob' >
      <filter>
        <condition attribute='statecode' operator='eq' value='{CustomJob.StatusEnum.Active}' />
      </filter>
    </link-entity>
  </entity>
</fetch>"));

			DataCollection<Entity> parentJobs;

			lock (jobQueryLock)
			{
				parentJobs = service.RetrieveMultiple(parentQuery).Entities;
			}

			log.LogInfo($"Fetched parent job records '{jobRecords.Length}'.");

			jobRecords = jobRecords.Union(parentJobs.Select(j => j.ToEntity<CustomJob>())
				.Select(j => new CustomJob { CustomJobId = j.CustomJobId, StatusReason = j.StatusReason })).ToArray();

			log.LogInfo($"Fetched" +
				$" {jobRecords.Count(job => new[] { CustomJob.StatusReasonEnum.Queued, CustomJob.StatusReasonEnum.Running, CustomJob.StatusReasonEnum.WaitingOnSubJobs }.Contains(job.StatusReason.GetValueOrDefault()))}"
				+ $" stalled job records.");

			return jobRecords.Select(j => j.Id);
		}

		public void QueueJobs(IEnumerable<Guid> jobRecordsParam, IOrganizationService service, CrmLog log)
		{
			service.Require(nameof(service));
			log.Require(nameof(log));

			var jobRecords = jobRecordsParam?.ToArray();
			jobRecords.Require(nameof(jobRecords));

			if (jobRecords?.Any() != true)
			{
				return;
			}

			foreach (var jobRecord in jobRecords)
			{
				service.Update(
					new CustomJob
					{
						Id = jobRecord,
						StatusReason = CustomJob.StatusReasonEnum.Queued
					});

				Jobs.Enqueue(jobRecord);
			}

			log.Log(new LogEntry("Queue Information",
				information: jobRecords.Select(record => record.ToString()).Aggregate((r1, r2) => r1 + "\r\n" + r2)));
		}

		public void FixDataCorruptJobs(IOrganizationService service, CrmLog log)
		{
			service.Require(nameof(service));
			log.Require(nameof(log));

			var xrmContext = new XrmServiceContext(service) { MergeOption = MergeOption.NoTracking };

			log.LogInfo("Fetching date-corrupted records ...");

			Guid[] corruptJobs;

			lock (jobQueryLock)
			{
				corruptJobs =
					(from job in xrmContext.CustomJobSet
					 where job.TargetDate > new DateTime(2099, 1, 1) && job.StatusReason == CustomJob.StatusReasonEnum.Waiting
						 && job.RecurrentJob == true
					 select job.CustomJobId).ToArray().Where(id => id.HasValue).Select(id => id.GetValueOrDefault()).ToArray();
			}

			log.LogInfo($"Fetched {corruptJobs.Length} date-corrupted records.");

			if (corruptJobs.Any())
			{
				log.LogInfo("Triggering recurrence recalculation of records ...");
				foreach (var jobId in corruptJobs)
				{
					service.Update(
						new CustomJob
						{
							Id = jobId,
							RecurrenceUpdatedTrigger = new Random().Next(111111, 99999999).ToString()
						});
					log.LogInfo($"Triggered recurrence recalculation of '{jobId}'.");
				}
				log.LogInfo("Finished triggering recurrence recalculation of records.");
			}
		}

		public void ProcessQueuedJob(Guid? id, EngineParams engineParams, IOrganizationService service, CrmLog log)
		{
			id.Require(nameof(id));
			log.Require(nameof(log));

			var job = service.Retrieve(CustomJob.EntityLogicalName, id.GetValueOrDefault(), new ColumnSet(true)).ToEntity<CustomJob>();
			var isRetry = job.StatusReason == CustomJob.StatusReasonEnum.Retry;

			if (isRetry)
			{
				log.Log("Retrying ...");
			}

			log.Log("Setting job to 'running' ...");
			service.Update(
				new CustomJob
				{
					Id = job.Id,
					StatusReason = CustomJob.StatusReasonEnum.Running
				});
			log.Log("Set job to 'running'.");

			var run = JobRunFactory.GetJobRun(job, engineParams, isRetry, service, log);
			run.Process();
		}
	}
}
