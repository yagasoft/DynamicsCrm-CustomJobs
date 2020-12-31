#region Imports

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job.MultiTarget
{
	[Log]
	internal class NoPagingRetryJob : NoPagingJob
	{
		public NoPagingRetryJob(CustomJob job, EngineParams engineParams, IOrganizationService service, CrmLog log)
			: base(job, engineParams, service, log)
		{ }

		protected override JobPagingInfo GetTargets()
		{
			return new()
				   {
					   Targets = GetRetryTargets().Select(target => Guid.Parse(target.ID)).ToList()
				   };
		}
	}
}
