#region Imports

using System;
using System.Linq;
using LinkDev.CustomJobs.Job.Abstract;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace LinkDev.CustomJobs.Job.MultiTarget
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
