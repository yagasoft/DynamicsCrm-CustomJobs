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

			var postImage = Context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

			if (postImage == null)
			{
				throw new InvalidPluginExecutionException("A full post image must be registered on this plugin step.");
			}

			var customJob =
				new CustomJob
				{
					Id = target.Id
				};

			Log.Log($"Loading related recurrences.");
			customJob.LoadRelation(CustomJob.RelationNames.Recurrences, Service);

			if (customJob.Recurrences == null)
			{
				if (postImage.RecurrentJob == true)
				{
					Log.Log("Setting to non-recurrent ...");
					Service.Update(
						new CustomJob
						{
							Id = customJob.Id,
							RecurrentJob = false
						});
					Log.Log("Set to non-recurrent.");
				}

				Log.Log($"No recurrences set. Exiting ...");
				return;
			}

			DateTime? targetDate = null;

			foreach (var recurrenceRule in customJob.Recurrences)
			{
				Log.Log($"Getting next recurrence for {recurrenceRule.Id}.");
				var action = new GlobalActions.ys_CustomJobGetNextRecurrenceDate(recurrenceRule.ToEntityReference(), Service);
				var nextRecurrence = action.Execute().NextTargetDate;
				Log.Log($"Next recurrence: '{nextRecurrence}'.");

				if (nextRecurrence > new DateTime(1900) && (targetDate == null || nextRecurrence < targetDate))
				{
					targetDate = nextRecurrence;
				}
			}

			if (targetDate == null)
			{
				Log.Log("Updating latest run message and date.");
				Service.Update(
					new CustomJob
					{
						Id = target.Id,
						LatestRunMessage = "Recurrence reached its cycle end.",
						TargetDate = null,
						PreviousTargetDate = DateTime.UtcNow,
						Status = CustomJob.StatusEnum.Inactive,
						StatusReason = CustomJob.StatusReasonEnum.Success
					});
				Log.Log("Updated.");

				return;
			}

			Log.Log("Updating target date.");
			Service.Update(
				new CustomJob
				{
					Id = customJob.Id,
					RecurrentJob = customJob.Recurrences.Any(),
					TargetDate = targetDate
				});
		}
	}
}
