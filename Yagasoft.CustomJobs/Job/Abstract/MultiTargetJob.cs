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
	[Log]
	internal abstract class MultiTargetJob : JobTarget
	{
		protected MultiTargetJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log) : base(job, service, serviceFactory, log)
		{ }

		protected List<CustomJobFailedTarget> GetRetryTargets(int? page = null, int? count = null)
		{
			var job =
				new CustomJob
				{
					Id = Job.Id
				};

			if (page != null && count != null)
			{
				log.Log($"Loading retry targets with page {page.Value} and count {count.Value} ...", LogLevel.Debug);
				job.LoadRelation(CustomJob.RelationNames.CustomJobFailedTargetsOfCustomJob, Service, false,
					count.Value, page.Value, CustomJobFailedTarget.Fields.ID);
				log.Log($"Loaded retry targets with page {page.Value} and count {count.Value}.");
			}
			else
			{
				log.Log($"Loading retry targets ...", LogLevel.Debug);
				job.LoadRelation(CustomJob.RelationNames.CustomJobFailedTargetsOfCustomJob, Service,
					CustomJobFailedTarget.Fields.ID);
				log.Log($"Loaded retry targets.", LogLevel.Debug);
			}

			var targets = job.CustomJobFailedTargetsOfCustomJob ?? new CustomJobFailedTarget[0];
			log.Log($"Retry targets count: {targets.Length}.");

			return targets.ToList();
		}

		protected List<CustomJobFailedTarget> AssociateFailedTargets(ExecutionFailures failures, Guid jobId)
		{
			var failedTargets = failures.Exceptions
				.Select(failure =>
					new CustomJobFailedTarget
					{
						ID = jobId == failure.Key ? "No Target" : failure.Key.ToString(),
						CustomJob = jobId,
						FailureMessage = BuildNoTraceExceptionMessage(failure.Value),
					});
			var retryTargets = GetRetryTargets();
			var newFailedTargets = failedTargets.Except(retryTargets, new CustomFailedJobComparer()).ToList();

			foreach (var failedTarget in newFailedTargets)
			{
				log.Log($"Creating failed target record for '{failedTarget.ID}' ...", LogLevel.Debug);
				failedTarget.CustomJob = jobId;
				Service.Create(failedTarget);
				log.Log($"Created failed target record for '{failedTarget.ID}'.");
			}

			return newFailedTargets;
		}

		protected List<CustomJobFailedTarget> DeleteSuccessfulFailed(ExecutionFailures failures = null)
		{
			var failedTargets = failures?.Exceptions
				.Select(failure =>
					new CustomJobFailedTarget
					{
						ID = failure.Key.ToString()
					}).ToList();
			var retryTargets = GetRetryTargets();
			var successfulRetryTargets =
				failedTargets == null
					? retryTargets
					: retryTargets.Except(failedTargets, new CustomFailedJobComparer()).ToList();

			foreach (var successfulTarget in successfulRetryTargets)
			{
				log.Log($"Deleting successful target record '{successfulTarget.ID}' ...", LogLevel.Debug);
				Service.Delete(CustomJobFailedTarget.EntityLogicalName, successfulTarget.Id);
				log.Log($"Deleted successful target record '{successfulTarget.ID}'.");
			}

			var remainingFailures = failedTargets?.Except(successfulRetryTargets).ToList();

			if (remainingFailures == null)
			{
				return null;
			}

			// set the ID for the remaining failed targets' records
			// it was built on the fly without retrieve from CRM, so the IDs are empty
			foreach (var failure in remainingFailures)
			{
				var id = retryTargets.FirstOrDefault(retry => retry.ID == failure.ID)?.Id;

				if (id == null)
				{
					continue;
				}

				failure.Id = id.Value;
			}

			return remainingFailures;
		}
	}
}
