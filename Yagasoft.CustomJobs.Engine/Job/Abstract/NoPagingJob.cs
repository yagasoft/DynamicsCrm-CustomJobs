#region Imports

using System.Linq;
using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job.Abstract
{
	[Log]
	internal abstract class NoPagingJob : MultiTargetJob
	{
		protected NoPagingJob(CustomJob job, EngineParams engineParams, IOrganizationService service, CrmLog log)
			: base(job, engineParams, service, log)
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
