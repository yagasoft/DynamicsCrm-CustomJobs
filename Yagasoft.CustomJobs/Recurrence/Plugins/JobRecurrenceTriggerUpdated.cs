//         Project / File: Yagasoft.Plugins.Common / CustomJobHandler.cs

#region Imports

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Recurrence.Plugins
{
	/// <summary>
	///     This plugin ... .<br />
	///     Version: 0.1.1
	/// </summary>
	public class JobRecurrenceTriggerUpdated : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			new JobRecurrenceTriggerUpdatedLogic().Execute(this, serviceProvider, false);
		}
	}

	internal class JobRecurrenceTriggerUpdatedLogic : PluginLogic<JobRecurrenceTriggerUpdated>
	{
		public JobRecurrenceTriggerUpdatedLogic() : base(null, PluginStage.All, CustomJobRecurrence.EntityLogicalName)
		{ }

		protected override void ExecuteLogic()
		{
			var target = GetTarget<CustomJobRecurrence>();

			if (target.RecurrenceUpdatedTrigger == null)
			{
				Log.Log("Recurrence update not triggered. Exiting ...");
				return;
			}

			var postImage = GetPostImage<CustomJobRecurrence>();
			var job = postImage.CustomJob.GetValueOrDefault();

			Helpers.SetTargetDate(job, Helpers.LoadRules(job, Service, Log), Service, Log);
		}

	}
}
