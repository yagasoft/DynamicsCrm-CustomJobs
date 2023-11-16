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
	[Log]
	public class CustomJobEngine
	{
		public readonly BlockingQueue<Guid> Jobs = new();

		private readonly int jobsPercentage;
		private readonly string serviceId;

		private readonly object jobQueryLock = new();

		private readonly ILogger debugLog;

		public CustomJobEngine(int jobsPercentage, string serviceId, ILogger debugLog)
		{
			this.jobsPercentage = jobsPercentage;
			this.serviceId = serviceId;
			this.debugLog = debugLog;
		}

		public IReadOnlyCollection<Guid> GetNextJobBatch(IOrganizationService service, ILogger log)
		{
			service.Require(nameof(service));
			log.Require(nameof(log));

			log.LogInfo("Fetching job records ...");

			var jobIds = new GlobalActions
				.ys_GenericCustomJobActionGetNextJobBatch(jobsPercentage, serviceId)
				.Execute(service)
				.Jobs;

			log.LogInfo($"Found {jobIds.Split(',').Length} jobs.");
			log.LogInfo($"Job IDs.", jobIds);

			return jobIds.Split(',')
				.Select(j => Guid.TryParse(j, out var jobId) ? (Guid?)jobId : null)
				.Where(j => j != null)
				.Select(j => j.Value).ToArray();
		}

		public void QueueJobs(IReadOnlyCollection<Guid> jobRecords, IOrganizationService service, ILogger log)
		{
			service.Require(nameof(service));
			log.Require(nameof(log));
			jobRecords.Require(nameof(jobRecords));

			if (jobRecords?.Any() != true)
			{
				return;
			}

			var enqueuedJobs = new List<Guid>();

			foreach (var jobRecord in jobRecords)
			{
				try
				{
					service.Update(
						new CustomJob
						{
							Id = jobRecord,
							StatusReason = CustomJob.StatusReasonEnum.Queued
						});

					Jobs.Enqueue(jobRecord);
					enqueuedJobs.Add(jobRecord);
				}
				catch (Exception ex)
				{
					// TODO log to Event Log
					log.LogError($"Job '{jobRecord}' failed to enqueue.");
					log.Log(ex, information: ex.BuildExceptionMessage());
				}
			}

			log.Log(new LogEntry("Queue Information", enqueuedJobs.StringAggregate("\r\n")));
		}

		public void FixDataCorruptJobs(IOrganizationService service, ILogger log)
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

		public void ProcessQueuedJob(Guid? id, EngineParams engineParams, IOrganizationService service, ILogger log)
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
