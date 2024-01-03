//         Project / File: Yagasoft.Plugins.Common / CustomJobHandler.cs

#region Imports

using System;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;

#endregion

namespace Yagasoft.CustomJobs
{
	/// <summary>
	///     This plugin ... .<br />
	///     Version: 0.1.1
	/// </summary>
	public class JobsEngineRecurrenceUpdated : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			new JobsEngineRecurrenceUpdatedLogic().Execute(this, serviceProvider, false);
		}
	}

	internal class JobsEngineRecurrenceUpdatedLogic : PluginLogic<JobsEngineRecurrenceUpdated>
	{
		public JobsEngineRecurrenceUpdatedLogic() : base(null, PluginStage.All)
		{ }

		protected override void ExecuteLogic()
		{
			var target = ((Entity)Context.InputParameters["Target"]).ToEntity<CustomJob>();

			if (target.RecurrenceUpdatedTrigger == null)
			{
				Log.Log("Recurrence update not triggered. Exiting ...");
				return;
			}

			Recurrence.Helpers.SetTargetDate(target.Id, Recurrence.Helpers.LoadRules(target.Id, Service, Log), Service, Log);
		}

	}
}
