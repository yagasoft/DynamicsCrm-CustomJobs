#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using LinkDev.CustomJobs.Job.Abstract;
using LinkDev.Libraries.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using static LinkDev.CustomJobs.Helpers;

#endregion

namespace LinkDev.CustomJobs.Job.MultiTarget
{
	[Log]
	internal class PagingNormalJob : PagingJob
	{
		public PagingNormalJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log) : base(job, service, serviceFactory, log)
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
				log.Log("Converting FetchXML to QueryExpression ...", LogLevel.Debug);
				var query = ((FetchXmlToQueryExpressionResponse)
					Service.Execute(
						new FetchXmlToQueryExpressionRequest
						{
							FetchXml = Job.TargetXML
						})).Query;
				log.Log("Converted.", LogLevel.Debug);

				query.PageInfo =
					new PagingInfo
					{
						Count = Job.RecordsPerPage ?? 5000,
						PageNumber = Job.PageNumber ?? 1,
						PagingCookie = Job.PagingCookie
					};

				log.Log($"Retrieving a max of {query.PageInfo.Count} per page ...", LogLevel.Debug);
				log.Log($"Retrieving page {query.PageInfo.PageNumber} ...", LogLevel.Debug);
				var result = Service.RetrieveMultiple(query);
				targets.AddRange(result.Entities.Select(entity => entity.Id));
				log.Log($"Found {result.Entities.Count}.");

				jobPagingInfo.NextPage = query.PageInfo.PageNumber = query.PageInfo.PageNumber + 1;
				jobPagingInfo.Cookie = query.PageInfo.PagingCookie = result.PagingCookie;
			}

			jobPagingInfo.Targets = targets;
			log.Log($"Target count: {targets.Count}.");

			return jobPagingInfo;
		}

		protected override JobRunStatus ProcessSuccessPaging(JobPagingInfo pagingInfo)
		{
			log.Log($"Updating paging info of job, page {pagingInfo.NextPage} ...", LogLevel.Debug);
			UpdatePaging(Service, Job.Id, pagingInfo);
			log.Log($"Updated paging info of job, page {pagingInfo.NextPage}.");

			log.Log($"Updating status of job to 'waiting' ...", LogLevel.Debug);
			SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, Job.Id, false);
			log.Log($"Updated status of job to 'waiting.");

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
					log.Log("Running failure action ...");

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
						log.Log(exception);
					}

					log.Log("Finished running failure action.");
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
						log.Log($"Updating status of job parent '{parentId}' to 'failed' ...", LogLevel.Debug);
						Close(Service, CustomJob.StatusReasonEnum.Failure, parentId, true);
						log.Log($"Updated status of job parent '{parentId}' to 'failed'.");
					}

					status.ParentId = parentId;
				}

				status.IsClose = true;
			}
			else
			{
				log.Log($"Creating a retry sub-job ...", LogLevel.Debug);
				var extendingJob = BuildSubJob(Job, CustomJob.StatusReasonEnum.Retry, $"Retry Page {Job.PageNumber ?? 1}");
				extendingJob.CurrentRetryRun = 1;
				extendingJob.Id = Service.Create(extendingJob);
				log.Log($"Created a retry sub-job '{extendingJob.Name}'.");

				newFailures = AssociateFailedTargets(failures, extendingJob.Id);

				log.Log($"Updating status of job 'waiting on subs' ...", LogLevel.Debug);
				SetStatus(Service, CustomJob.StatusReasonEnum.WaitingOnSubJobs, Job.Id, false);
				log.Log($"Updating status of job 'waiting on subs'.");
			}

			status.IsSuccess = false;
			status.LatestRunMessage = $"Page {Job.PageNumber} failed.";
			status.RunTargetFailures = newFailures;

			return status;
		}
	}
}
