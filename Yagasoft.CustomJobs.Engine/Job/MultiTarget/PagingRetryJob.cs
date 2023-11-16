#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;
using static Yagasoft.CustomJobs.Engine.Helpers;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job.MultiTarget
{
	[Log]
	internal class PagingRetryJob : PagingJob
	{
		public PagingRetryJob(CustomJob job, EngineParams engineParams, IOrganizationService service, ILogger log)
			: base(job, engineParams, service, log)
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
			return new()
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
					Log.Log($"Updating paging info of parent job, page {pagingInfo.NextPage} ...");
					UpdatePaging(Service, parentId, pagingInfo);
					Log.Log($"Updated paging info of parent job, page {pagingInfo.NextPage}.");
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
							Log.Log($"Updating paging info of job, page {pagingInfo.NextPage} ...");
							UpdatePaging(Service, parentId, pagingInfo);
							Log.Log($"Updated paging info of job, page {pagingInfo.NextPage}.");

							Log.Log($"Updating status of job parent '{parentId}' to 'waiting' ...");
							SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, parentId, IsParentRecurrentJob());
							Log.Log($"Updated status of job parent '{parentId}' to 'waiting'.");
						}
						else
						{
							Log.Log($"Updating status of job parent '{parentId}' to 'failed' ...");
							Close(Service, CustomJob.StatusReasonEnum.Failure, Job.ParentJob.Value, true);
							Log.Log($"Updated status of job parent '{parentId}' to 'failed'.");
						}
					}
				}

				status.IsClose = true;
			}
			else
			{
				if (Job.RetrySchedule != null)
				{
					UpdateRetryTargetDate(Service, Job, Log);
				}

				IncrementRetry(Job.CurrentRetryRun ?? 0);

				Log.Log($"Updating status of job to 'retry' ...");
				SetStatus(Service, CustomJob.StatusReasonEnum.Retry, Job.Id, false);
				Log.Log($"Updated status of job 'retry'.");
			}

			status.IsSuccess = false;
			status.LatestRunMessage = $"Retry run failed.";
			return status;
		}

		protected void AssociateFailedToParent(List<CustomJobFailedTarget> remainingFailed)
		{
			if (!IsSubJob())
			{
				Log.Log("Job doesn't have a parent.", LogLevel.Warning);
			}

			foreach (var failedTarget in remainingFailed)
			{
				Log.Log($"Associate failed target '{failedTarget.ID}' to parent '{Job.ParentJob}'  ...");
				Service.Update(new CustomJobFailedTarget
							   {
								   Id = failedTarget.Id,
								   CustomJob = Job.ParentJob
							   });
				Log.Log($"Associated failed target '{failedTarget.ID}' to parent '{Job.ParentJob}'.");
			}
		}
	}
}
