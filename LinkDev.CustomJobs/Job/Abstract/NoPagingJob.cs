#region Imports

using System.Linq;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace LinkDev.CustomJobs.Job.Abstract
{
	[Log]
	internal abstract class NoPagingJob : MultiTargetJob
	{
		protected NoPagingJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log) : base(job, service, serviceFactory, log)
		{ }

		internal override JobRunStatus ProcessTarget()
		{
			var pagingInfo = GetTargets();
			var result = Execute(pagingInfo);
			var isFailed = result.Exceptions.Any();
			var isSuccess = !isFailed;

			if (isSuccess)
			{
				return ProcessSuccess();
			}
			else
			{
				return ProcessFailure(result);
			}
		}

		protected override JobRunStatus ProcessSuccess()
		{
			DeleteSuccessfulFailed();
			return base.ProcessSuccess();
		}

		protected override JobRunStatus ProcessFailure(ExecutionFailures executionResult)
		{
			AssociateFailedTargets(executionResult, Job.Id);
			DeleteSuccessfulFailed(executionResult);
			return base.ProcessFailure(executionResult);
		}
	}
}
