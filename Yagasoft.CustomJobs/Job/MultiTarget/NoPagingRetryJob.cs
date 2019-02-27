#region Imports

using System;
using System.Linq;
using Yagasoft.CustomJobs.Job.Abstract;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace Yagasoft.CustomJobs.Job.MultiTarget
{
	[Log]
	internal class NoPagingRetryJob : NoPagingJob
	{
		public NoPagingRetryJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log) : base(job, service, serviceFactory, log)
		{ }

		protected override JobPagingInfo GetTargets()
		{
			return new JobPagingInfo
				   {
					   Targets = GetRetryTargets().Select(target => Guid.Parse(target.ID)).ToList()
				   };
		}
	}
}
