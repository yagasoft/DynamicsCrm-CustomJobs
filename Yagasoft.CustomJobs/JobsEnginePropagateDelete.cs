//         Project / File: Yagasoft.Plugins.Common / CustomJobHandler.cs

#region Imports

using System;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

#endregion

namespace Yagasoft.CustomJobs
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
			var targetRef = (EntityReference) Context.InputParameters["Target"];

			if (targetRef.LogicalName == CustomJobLog.EntityLogicalName)
			{
				Log.Log("Processing Log deletion ...");
				var jobLog = new CustomJobLog
							 {
								 Id = targetRef.Id
							 };
				jobLog.LoadRelation(CustomJobLog.RelationNames.CustomJobFailedTargetsOfLogEntry, Service);
				var failures = jobLog.CustomJobFailedTargetsOfLogEntry;

				if (failures != null)
				{
					Log.Log($"Found {failures.Length} failed job entries.");

					foreach (var failure in failures)
					{
						Log.Log($"Deleting '{failure.Id}' ...");
						Service.Delete(CustomJobFailedTarget.EntityLogicalName, failure.Id);
						Log.Log($"Deleted '{failure.Id}'.");
					}
				}
			}
		}
	}
}
