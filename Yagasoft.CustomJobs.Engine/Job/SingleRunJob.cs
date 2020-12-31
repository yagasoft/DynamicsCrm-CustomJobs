#region Imports

using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job
{
	[Log]
	internal class SingleRunJob : JobRun
	{
		protected JobTarget JobTarget;

		public SingleRunJob(CustomJob job, EngineParams engineParams, JobTarget target, IOrganizationService service, CrmLog log)
			: base(job, engineParams, service, log)
		{
			JobTarget = target;
		}

		protected override JobRunStatus ProcessRecurrence()
		{
			return JobTarget.ProcessTarget();
		}
	}
}
