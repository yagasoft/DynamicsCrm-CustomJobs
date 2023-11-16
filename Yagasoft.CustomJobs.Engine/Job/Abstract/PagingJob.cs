using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.Libraries.Common;

namespace Yagasoft.CustomJobs.Engine.Job.Abstract
{
	internal abstract class PagingJob : MultiTargetJob
	{
		protected PagingJob(CustomJob job, EngineParams engineParams, IOrganizationService service, ILogger log)
			: base(job, engineParams, service, log)
		{
		}

		protected abstract JobRunStatus ProcessSuccessPaging(JobPagingInfo pagingInfo);

		protected abstract JobRunStatus ProcessFailurePaging(ExecutionFailures failures, JobPagingInfo pagingInfo);
	}
}
