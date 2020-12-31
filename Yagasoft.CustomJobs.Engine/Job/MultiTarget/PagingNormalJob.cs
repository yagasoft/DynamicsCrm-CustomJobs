#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;
using static Yagasoft.CustomJobs.Engine.Helpers;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job.MultiTarget
{
	[Log]
	internal class PagingNormalJob : PagingJob
	{
		public PagingNormalJob(CustomJob job, EngineParams engineParams, IOrganizationService service, CrmLog log)
			: base(job, engineParams, service, log)
		{ }

		internal override JobRunStatus ProcessTarget()
		{
			var pagingInfo = GetTargets();

			if (pagingInfo.Targets.Any())
			{
				var result = Execute(pagingInfo);
				var isFailed = result.Exceptions.Any();
				var isSuccess = !isFailed;

				if (isSuccess)
				{
					return ProcessSuccessPaging(pagingInfo);
				}
				else
				{
					return ProcessFailurePaging(result, pagingInfo);
				}
			}
			else
			{
				return ProcessSuccess();
			}
		}

		protected override JobPagingInfo GetTargets()
		{
			var jobPagingInfo = new JobPagingInfo();

			var targets = new List<Guid>();

			if (!string.IsNullOrEmpty(Job.TargetLogicalName))
			{
				Log.Log("Converting FetchXML to QueryExpression ...");
				var query = ((FetchXmlToQueryExpressionResponse)
					Service.Execute(
						new FetchXmlToQueryExpressionRequest
						{
							FetchXml = Job.TargetXML
						})).Query;
				Log.Log("Converted.");

				query.PageInfo =
					new PagingInfo
					{
						Count = Job.RecordsPerPage ?? 5000,
						PageNumber = Job.PageNumber ?? 1,
						PagingCookie = Job.PagingCookie
					};

				Log.Log($"Retrieving a max of {query.PageInfo.Count} per page ...");
				Log.Log($"Retrieving page {query.PageInfo.PageNumber} ...");
				var result = Service.RetrieveMultiple(query);
				targets.AddRange(result.Entities.Select(entity => entity.Id));
				Log.Log($"Found {result.Entities.Count}.");

				jobPagingInfo.NextPage = query.PageInfo.PageNumber += 1;
				jobPagingInfo.Cookie = query.PageInfo.PagingCookie = result.PagingCookie;
			}

			jobPagingInfo.Targets = targets;
			Log.Log($"Target count: {targets.Count}.");

			return jobPagingInfo;
		}

		protected override JobRunStatus ProcessSuccessPaging(JobPagingInfo pagingInfo)
		{
			Log.Log($"Updating paging info of job, page {pagingInfo.NextPage} ...");
			UpdatePaging(Service, Job.Id, pagingInfo);
			Log.Log($"Updated paging info of job, page {pagingInfo.NextPage}.");

			Log.Log($"Updating status of job to 'waiting' ...");
			SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, Job.Id, false);
			Log.Log($"Updated status of job to 'waiting.");

			return
				new JobRunStatus
				{
					IsSuccess = true,
					LatestRunMessage = $"Page {Job.PageNumber} was successful."
				};
		}

		protected override JobRunStatus ProcessFailurePaging(ExecutionFailures failures, JobPagingInfo pagingInfo)
		{
			List<CustomJobFailedTarget> newFailures;
			var status = new JobRunStatus();

			if (IsNoRetry())
			{
				newFailures = AssociateFailedTargets(failures, Job.Id);

				if (Job.FailureAction != null)
				{
					Log.Log("Running failure action ...");

					try
					{
						Service.Execute(
							new ExecuteWorkflowRequest
							{
								EntityId = Job.Id,
								WorkflowId = Job.FailureAction.Value
							});
					}
					catch (Exception exception)
					{
						Log.LogError(exception);
					}

					Log.Log("Finished running failure action.");
				}

				if (Job.IgnoreFailure == true)
				{
					status = ProcessSuccessPaging(pagingInfo);
					status.RunTargetFailures = newFailures;

					if (IsSubJob())
					{
						status.ParentId = Job.ParentJob.Value;
					}

					return status;
				}

				if (IsSubJob())
				{
					var parentId = Job.ParentJob.Value;

					if (IsWaitingOnSubJobs(Service, parentId))
					{
						Log.Log($"Updating status of job parent '{parentId}' to 'failed' ...");
						Close(Service, CustomJob.StatusReasonEnum.Failure, parentId, true);
						Log.Log($"Updated status of job parent '{parentId}' to 'failed'.");
					}

					status.ParentId = parentId;
				}

				status.IsClose = true;
			}
			else
			{
				Log.Log($"Creating a retry sub-job ...");
				var extendingJob = BuildSubJob(Job, CustomJob.StatusReasonEnum.Retry, $"Retry Page {Job.PageNumber ?? 1}");
				extendingJob.CurrentRetryRun = 1;
				extendingJob.Id = Service.Create(extendingJob);
				Log.Log($"Created a retry sub-job '{extendingJob.Name}'.");

				newFailures = AssociateFailedTargets(failures, extendingJob.Id);

				Log.Log($"Updating status of job 'waiting on subs' ...");
				SetStatus(Service, CustomJob.StatusReasonEnum.WaitingOnSubJobs, Job.Id, false);
				Log.Log($"Updating status of job 'waiting on subs'.");
			}

			status.IsSuccess = false;
			status.LatestRunMessage = $"Page {Job.PageNumber} failed.";
			status.RunTargetFailures = newFailures;

			return status;
		}
	}
}
