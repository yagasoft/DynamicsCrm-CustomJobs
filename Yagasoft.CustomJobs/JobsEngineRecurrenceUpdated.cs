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
			var target = ((Entity)context.InputParameters["Target"]).ToEntity<CustomJob>();

			if (target.RecurrenceUpdatedTrigger == null)
			{
				log.Log("Recurrence update not triggered. Exiting ...");
				return;
			}

			var postImage = context.PostEntityImages.FirstOrDefault().Value?.ToEntity<CustomJob>();

			if (postImage == null)
			{
				throw new InvalidPluginExecutionException("A full post image must be registered on this plugin step.");
			}

			var customJob =
				new CustomJob
				{
					Id = target.Id
				};

			log.Log($"Loading related recurrences.", LogLevel.Debug);
			customJob.LoadRelation(CustomJob.RelationNames.Recurrences, service);

			if (customJob.Recurrences == null)
			{
				if (postImage.RecurrentJob == true)
				{
					log.Log("Setting to non-recurrent ...", LogLevel.Debug);
					service.Update(
						new CustomJob
						{
							Id = customJob.Id,
							RecurrentJob = false
						});
					log.Log("Set to non-recurrent.");
				}

				log.Log($"No recurrences set. Exiting ...");
				return;
			}

			DateTime? targetDate = null;

			foreach (var recurrenceRule in customJob.Recurrences)
			{
				log.Log($"Getting next recurrence for {recurrenceRule.Id}.", LogLevel.Debug);
				var action = new GlobalActions.ys_CustomJobGetNextRecurrenceDate(recurrenceRule.ToEntityReference(), service);
				var nextRecurrence = action.Execute().NextTargetDate;
				log.Log($"Next recurrence: '{nextRecurrence}'.");

				if (nextRecurrence > new DateTime(1900) && (targetDate == null || nextRecurrence < targetDate))
				{
					targetDate = nextRecurrence;
				}
			}

			if (targetDate == null)
			{
				log.Log("Updating latest run message and date.", LogLevel.Debug);
				service.Update(
					new CustomJob
					{
						Id = target.Id,
						LatestRunMessage = "Recurrence reached its cycle end.",
						TargetDate = null,
						PreviousTargetDate = DateTime.UtcNow,
						Status = CustomJob.StatusEnum.Inactive,
						StatusReason = CustomJob.StatusReasonEnum.Success
					});
				log.Log("Updated.");

				return;
			}

			log.Log("Updating target date.", LogLevel.Debug);
			service.Update(
				new CustomJob
				{
					Id = customJob.Id,
					RecurrentJob = customJob.Recurrences.Any(),
					TargetDate = targetDate
				});
		}
	}
}
