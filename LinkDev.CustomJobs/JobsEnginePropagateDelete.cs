//         Project / File: LinkDev.Plugins.Common / CustomJobHandler.cs

#region Imports

using System;
using System.Linq;
using LinkDev.Libraries.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

#endregion

namespace LinkDev.CustomJobs
{
	/// <summary>
	///     This plugin ... .<br />
	///     Version: 0.1.1
	/// </summary>
	public class JobsEnginePropagateDelete : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			new JobsEnginePropagateDeleteLogic().Execute(this, serviceProvider, false);
		}
	}

	internal class JobsEnginePropagateDeleteLogic : PluginLogic<JobsEnginePropagateDelete>
	{
		public JobsEnginePropagateDeleteLogic() : base(null, PluginStage.All)
		{
		}

		protected override void ExecuteLogic()
		{
			var targetRef = (EntityReference) context.InputParameters["Target"];

			if (targetRef.LogicalName == CustomJobLog.EntityLogicalName)
			{
				log.Log("Processing log deletion ...", LogLevel.Debug);
				var jobLog = new CustomJobLog
							 {
								 Id = targetRef.Id
							 };
				jobLog.LoadRelation(CustomJobLog.RelationNames.CustomJobFailedTargetsOfLogEntry, service);
				var failures = jobLog.CustomJobFailedTargetsOfLogEntry;

				if (failures != null)
				{
					log.Log($"Found {failures.Length} failed job entries.");

					foreach (var failure in failures)
					{
						log.Log($"Deleting '{failure.Id}' ...", LogLevel.Debug);
						service.Delete(CustomJobFailedTarget.EntityLogicalName, failure.Id);
						log.Log($"Deleted '{failure.Id}'.");
					}
				}
			}
		}
	}
}
