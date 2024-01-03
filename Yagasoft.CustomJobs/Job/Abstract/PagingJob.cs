using System;
using System.Collections.Generic;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;

namespace Yagasoft.CustomJobs.Job.Abstract
{
	internal abstract class PagingJob : MultiTargetJob
	{
		public PagingJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			ILogger log) : base(job, service, serviceFactory, log)
		{
		}

		protected abstract JobRunStatus ProcessSuccessPaging(JobPagingInfo pagingInfo);

		protected abstract JobRunStatus ProcessFailurePaging(ExecutionFailures failures, JobPagingInfo pagingInfo);
	}
}
