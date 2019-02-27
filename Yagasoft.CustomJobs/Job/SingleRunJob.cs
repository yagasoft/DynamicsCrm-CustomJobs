#region Imports

using Yagasoft.CustomJobs.Job.Abstract;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace Yagasoft.CustomJobs.Job
{
	[Log]
	internal class SingleRunJob : JobRun
	{
		protected JobTarget JobTarget;

		public SingleRunJob(CustomJob job, JobTarget target, IOrganizationService service, CrmLog log)
			: base(job, service, log)
		{
			JobTarget = target;
		}

		protected override JobRunStatus ProcessRecurrence()
		{
			return JobTarget.ProcessTarget();
		}
	}
}
