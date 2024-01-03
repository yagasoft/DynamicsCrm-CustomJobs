using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Yagasoft.Libraries.Common;

namespace Yagasoft.CustomJobs.Recurrence
{
	internal static class Helpers
	{
		internal static void SetTargetDate(Guid target, IReadOnlyList<Guid> rules, IOrganizationService service, ILogger logger)
		{
			DateTime? targetDate = null;

			foreach (var recurrenceRule in rules)
			{
				logger.Log($"Getting next recurrence for {recurrenceRule}.");
				var action = new GlobalActions.ys_CustomJobGetNextRecurrenceDate(
					new EntityReference(RecurrenceRule.EntityLogicalName, recurrenceRule), service);
				var nextRecurrence = action.Execute().NextTargetDate;
				logger.Log($"Next recurrence: '{nextRecurrence}'.");

				if (nextRecurrence > new DateTime(1900) && (targetDate == null || nextRecurrence < targetDate))
				{
					targetDate = nextRecurrence;
				}
			}

			if (targetDate == null)
			{
				logger.Log("Updating latest run message and date.");
				service.Update(
					new CustomJob
					{
						Id = target,
						LatestRunMessage = "Recurrence reached its cycle end.",
						TargetDate = null,
						PreviousTargetDate = DateTime.UtcNow,
						Status = CustomJob.StatusEnum.Inactive,
						StatusReason = CustomJob.StatusReasonEnum.Success
					});
				logger.Log("Updated.");

				return;
			}

			logger.Log("Updating target date.");
			service.Update(
				new CustomJob
				{
					Id = target,
					RecurrentJob = rules.Any(),
					TargetDate = targetDate
				});
		}

		internal static IReadOnlyList<Guid> LoadRules(Guid target, IOrganizationService service, ILogger logger)
		{
			var customJob =
				new CustomJob
				{
					Id = target
				};

			logger.Log($"Loading related recurrences.");
			customJob.LoadRelation(CustomJob.RelationNames.RecurrenceRules, service, CustomJobRecurrence.Fields.RecurrenceRule);

			if (customJob.RecurrenceRules == null)
			{
				logger.Log($"No recurrences set. Exiting ...");
				return Array.Empty<Guid>();
			}
			
			return customJob.RecurrenceRules.Select(e => e.RecurrenceRule.GetValueOrDefault()).ToArray();
		}
	}
}
