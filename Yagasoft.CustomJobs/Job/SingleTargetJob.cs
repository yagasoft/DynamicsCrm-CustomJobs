#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Yagasoft.CustomJobs.Job.Abstract;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace Yagasoft.CustomJobs.Job
{
	[Log]
	internal class SingleTargetJob : JobTarget
	{
		public SingleTargetJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory factory,
			ILogger log) : base(job, service, factory, log)
		{ }

		internal override JobRunStatus ProcessTarget()
		{
			var targets = GetTargets();
			var result = Execute(targets);
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

		protected override JobPagingInfo GetTargets()
		{
			Guid? target = null;

			if (!string.IsNullOrEmpty(Job.TargetLogicalName))
			{
				log.Log($"Target entity: {Job.TargetLogicalName}.");

				if (string.IsNullOrEmpty(Job.TargetXML))
				{
					log.Log($"Target XML is empty.");
					Guid targetId;

					if (!Guid.TryParse(Job.TargetID, out targetId))
					{
						throw new InvalidPluginExecutionException($"Couldn't find proper target ID in job.");
					}

					target = targetId;
				}
			}

			log.Log($"Target: {target}.");

			return new JobPagingInfo
				   {
					   Targets = target.HasValue ? new List<Guid> { target.Value } : null
				   };
		}
	}
}
