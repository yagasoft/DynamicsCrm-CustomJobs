#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Process
{
	public static class Batch
	{
		public static IReadOnlyCollection<Guid> GetNextJobBatch(int percentage, string lockId, TimeSpan timeout,
			IOrganizationService service, ILogger log)
		{
			service.Require(nameof(service));
			log.Require(nameof(log));

			var xrmContext = new XrmServiceContext(service) { MergeOption = MergeOption.NoTracking };

			log.Log($"percentage: {percentage}");
			log.Log($"lockId: {lockId}");
			log.Log($"timeout: {timeout}");

			log.LogDebug("Locking transaction ...");

			Lock(service, log);

			log.LogInfo("Fetching job records ...");

			var jobRecords =
				(from job in xrmContext.CustomJobSet
				 where
					(	
						(
						 (job.TargetDate == null || job.TargetDate < DateTime.UtcNow)
							 &&
							 (job.StatusReason == CustomJob.StatusReasonEnum.Waiting
								 || job.StatusReason == CustomJob.StatusReasonEnum.Retry)
							 && (job.LockID == null || job.LockID == lockId)
						 )
						 ||
						 (
							 job.ModifiedOn < (DateTime.UtcNow - timeout)
								 &&
								 (job.StatusReason == CustomJob.StatusReasonEnum.Running
									 || job.StatusReason == CustomJob.StatusReasonEnum.Queued)
						 )
					)
					&& job.Status == CustomJob.StatusEnum.Active
				 select new CustomJob
						{
							CustomJobId = job.CustomJobId,
							StatusReason = job.StatusReason
						}).ToArray();

			log.LogInfo($"Fetched job records '{jobRecords.Length}'.");

			log.LogInfo("Fetching parent job records with no children ...");

			var parentQuery = new FetchExpression(
				string.Intern($@"<fetch>
  <entity name='{CustomJob.EntityLogicalName}'>
    <attribute name='{CustomJob.Fields.CustomJobId}' />
    <attribute name='{CustomJob.Fields.StatusReason}' />
    <filter>
      <condition attribute='{CustomJob.Fields.StatusReason}' operator='eq' value='{(int)
	      CustomJob.StatusReasonEnum.WaitingOnSubJobs}' />
      <condition entityname='subjob' attribute='{CustomJob.Fields.CustomJobId}' operator='null' />
      <filter type='or' >
          <condition attribute='{CustomJob.Fields.LockID}' operator='null' />
          <condition attribute='{CustomJob.Fields.LockID}' operator='eq' value='{lockId}' />
          <condition attribute='{CustomJob.Fields.ModifiedOn}' operator='lt' value='{(DateTime.UtcNow - timeout)}' />
      </filter>
      <condition attribute='{CustomJob.Fields.Status}' operator='eq' value='{(int)
	      CustomJob.StatusEnum.Active}' />
    </filter>
    <link-entity name='{CustomJob.EntityLogicalName}' from='{CustomJob.Fields.ParentJob}' to='{CustomJob.Fields.CustomJobId}' link-type='outer' alias='subjob' >
      <filter>
        <condition attribute='{CustomJob.Fields.Status}' operator='eq' value='{(int)CustomJob.StatusEnum.Active}' />
      </filter>
    </link-entity>
  </entity>
</fetch>"));

			var parentJobs = service.RetrieveMultiple(parentQuery).Entities;
			log.LogInfo($"Fetched parent job records '{jobRecords.Length}'.");

			jobRecords = jobRecords.Union(parentJobs.Select(j => j.ToEntity<CustomJob>())
				.Select(j => new CustomJob { CustomJobId = j.CustomJobId, StatusReason = j.StatusReason })).ToArray();

			log.LogInfo($"Fetched" +
				$" {jobRecords.Count(job => new[] { CustomJob.StatusReasonEnum.Queued, CustomJob.StatusReasonEnum.Running, CustomJob.StatusReasonEnum.WaitingOnSubJobs }.Contains(job.StatusReason.GetValueOrDefault()))}"
				+ $" stalled job records.");

			log.LogInfo($"Slicing jobs to percentage: {percentage} ...");
			jobRecords = jobRecords.Take((int)Math.Ceiling(jobRecords.Length * percentage / 100.0)).ToArray();
			log.LogInfo($"Engine with ID: '{lockId}', took: {jobRecords.Length} jobs.");

			var jobIds = jobRecords.Select(j => j.Id).ToArray();
			log.LogInfo($"Job IDs.", jobIds.StringAggregate());
			
			// TODO lock the jobs themselves with the lock ID given to avoid race condition in the service

			return jobIds;
		}

		public static void Lock(IOrganizationService service, ILogger log)
		{
			service.Require(nameof(service));	
			log.Require(nameof(log));

			var engineId =
				CacheHelpers.GetFromMemCacheAdd("Yagasoft.CustomJobs.Process.Batch.Lock.EngineId",
					() =>
					{
						var query = new FetchExpression(
							string.Intern($@"<fetch no-lock='true'>
  <entity name='{CustomJobEngine.EntityLogicalName}'>
    <attribute name='ys_customjobengineid' />
  </entity>
</fetch>"));

						log.LogDebug("Retrieving CRM Engine ID ...");
						return service.RetrieveMultiple(query).Entities.FirstOrDefault()?.ToEntity<CustomJobEngine>().Id;
					});

			log.LogDebug($"Engine ID: {engineId}.");
			engineId.Require(nameof(engineId), "Cannot find a row under the Custom Job Engine table.");

			log.LogDebug($"Locking on row ...");
			service.Update(
				new CustomJobEngine
				{
					Id = engineId.GetValueOrDefault(),
					Lock = DateTime.UtcNow.ToString()
				});
		}
	}
}
