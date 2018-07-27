#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using LinkDev.CustomJobs.Job.Abstract;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;
using static LinkDev.CustomJobs.Helpers;

#endregion

namespace LinkDev.CustomJobs.Job.MultiTarget
{
	[Log]
	internal class PagingRetryJob : PagingJob
	{
		public PagingRetryJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log) : base(job, service, serviceFactory, log)
		{ }

		internal override JobRunStatus ProcessTarget()
		{
			var pagingInfo = GetTargets();

			var failures = Execute(pagingInfo);
			var isFailed = failures.Exceptions.Any();
			var isSuccess = !isFailed;

			return isSuccess
				? ProcessSuccessPaging(pagingInfo)
				: ProcessFailurePaging(failures, pagingInfo);
		}

		protected override JobPagingInfo GetTargets()
		{
			return new JobPagingInfo
				   {
					   Targets = GetRetryTargets().Select(target => Guid.Parse(target.ID)).ToList(),
					   NextPage = (Job.PageNumber ?? 1) + 1,
					   Cookie = Job.PagingCookie
				   };
		}

		protected override JobRunStatus ProcessSuccessPaging(JobPagingInfo pagingInfo)
		{
			DeleteSuccessfulFailed();

			if (IsSubJob())
			{
				var parentId = Job.ParentJob.Value;

				if (IsWaitingOnSubJobs(Service, parentId))
				{
					log.Log($"Updating paging info of parent job, page {pagingInfo.NextPage} ...", LogLevel.Debug);
					UpdatePaging(Service, parentId, pagingInfo);
					log.Log($"Updated paging info of parent job, page {pagingInfo.NextPage}.");
				}
			}

			return ProcessSuccess();
		}

		protected override JobRunStatus ProcessFailurePaging(ExecutionFailures failures, JobPagingInfo pagingInfo)
		{
			var remainingFailed = DeleteSuccessfulFailed(failures);
			var status = new JobRunStatus();

			if (IsNoRetry())
			{
				if (IsSubJob())
				{
					var parentId = Job.ParentJob.Value;

					AssociateFailedToParent(remainingFailed);

					if (IsWaitingOnSubJobs(Service, parentId))
					{
						if (Job.IgnoreFailure == true)
						{
							log.Log($"Updating paging info of job, page {pagingInfo.NextPage} ...", LogLevel.Debug);
							UpdatePaging(Service, parentId, pagingInfo);
							log.Log($"Updated paging info of job, page {pagingInfo.NextPage}.");

							log.Log($"Updating status of job parent '{parentId}' to 'waiting' ...", LogLevel.Debug);
							SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, parentId, IsParentRecurrentJob());
							log.Log($"Updated status of job parent '{parentId}' to 'waiting'.");
						}
						else
						{
							log.Log($"Updating status of job parent '{parentId}' to 'failed' ...", LogLevel.Debug);
							Close(Service, CustomJob.StatusReasonEnum.Failure, Job.ParentJob.Value, true);
							log.Log($"Updated status of job parent '{parentId}' to 'failed'.");
						}
					}
				}

				status.IsClose = true;
			}
			else
			{
				if (Job.RetrySchedule != null)
				{
					UpdateRetryTargetDate(Service, Job, log);
				}

				IncrementRetry(Job.CurrentRetryRun ?? 0);

				log.Log($"Updating status of job to 'retry' ...", LogLevel.Debug);
				SetStatus(Service, CustomJob.StatusReasonEnum.Retry, Job.Id, false);
				log.Log($"Updated status of job 'retry'.");
			}

			status.IsSuccess = false;
			status.LatestRunMessage = $"Retry run failed.";
			return status;
		}

		protected void AssociateFailedToParent(List<CustomJobFailedTarget> remainingFailed)
		{
			if (!IsSubJob())
			{
				log.Log("Job doesn't have a parent.", LogLevel.Warning);
			}

			foreach (var failedTarget in remainingFailed)
			{
				log.Log($"Associate failed target '{failedTarget.ID}' to parent '{Job.ParentJob}'  ...", LogLevel.Debug);
				Service.Update(new CustomJobFailedTarget
							   {
								   Id = failedTarget.Id,
								   CustomJob = Job.ParentJob
							   });
				log.Log($"Associated failed target '{failedTarget.ID}' to parent '{Job.ParentJob}'.");
			}
		}
	}
}
