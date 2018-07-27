#region Imports

using LinkDev.CustomJobs.Job.Abstract;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace LinkDev.CustomJobs.Job
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
