#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job
{
	[Log]
	internal class SingleTargetJob : JobTarget
	{
		public SingleTargetJob(CustomJob job, EngineParams engineParams, IOrganizationService service, CrmLog log)
			: base(job, engineParams, service, log)
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
				Log.Log($"Target entity: {Job.TargetLogicalName}.");

				if (string.IsNullOrEmpty(Job.TargetXML))
				{
					Log.Log($"Target XML is empty.");
					Guid targetId;

					if (!Guid.TryParse(Job.TargetID, out targetId))
					{
						throw new FormatException($"Couldn't find proper target ID in job.");
					}

					target = targetId;
				}
			}

			Log.Log($"Target: {target}.");

			return new JobPagingInfo
				   {
					   Targets = target.HasValue ? new List<Guid> { target.Value } : null
				   };
		}
	}
}
