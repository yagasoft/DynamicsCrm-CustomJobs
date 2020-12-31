#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine
{
	internal class CustomFailedJobComparer : IEqualityComparer<CustomJobFailedTarget>
	{
		public bool Equals(CustomJobFailedTarget x, CustomJobFailedTarget y)
		{
			return x.ID == y.ID;
		}

		public int GetHashCode(CustomJobFailedTarget obj)
		{
			return obj.ID.GetHashCode();
		}
	}

	internal static class Helpers
	{
		internal static CustomJob BuildSubJob(CustomJob parentJob, CustomJob.StatusReasonEnum status, string suffix)
		{
			var fieldExclusions =
				new[]
				{
					CustomJob.Fields.CustomJobId,
					CustomJob.Fields.Name,
					CustomJob.Fields.RecurrentJob,
					CustomJob.Fields.RunTrigger,
					CustomJob.Fields.RecurrenceUpdatedTrigger,
					CustomJob.Fields.ParentJob,
					CustomJob.Fields.DeleteOnSuccess,
					CustomJob.Fields.RetrySchedule,
					CustomJob.Fields.Status,
					CustomJob.Fields.StatusReason,
					CustomJob.Fields.LatestRunMessage,
					CustomJob.Fields.MarkForWaiting,
					CustomJob.Fields.PreviousTargetDate,
					CustomJob.Fields.ParentRecurrent
				};
			var extendingjob = new CustomJob();

			foreach (var key in parentJob.Attributes.Keys.Except(fieldExclusions))
			{
				extendingjob[key] = parentJob[key];
			}

			extendingjob.Name = $"{parentJob.Name} | {suffix}";
			extendingjob.DeleteOnSuccess = parentJob.DeleteSubJobsOnSuccess;
			extendingjob.StatusReason = status;
			extendingjob.ParentJob = parentJob.Id;
			extendingjob.ParentRecurrent = parentJob.RecurrentJob;
			extendingjob.RetrySchedule = parentJob.SubJobsRetrySchedule;
			extendingjob.MarkForWaiting = false;

			return extendingjob;
		}

		//internal static bool IsRecurrentJob(IOrganizationService service, Guid jobId)
		//{
		//	return service.Retrieve(CustomJob.EntityLogicalName, jobId, new ColumnSet(CustomJob.Fields.RecurrentJob))
		//		.GetAttributeValue<bool?>(CustomJob.Fields.RecurrentJob) == true;
		//}

		internal static bool IsWaitingOnSubJobs(IOrganizationService service, Guid jobId)
		{
			return service.Retrieve(CustomJob.EntityLogicalName, jobId, new ColumnSet(CustomJob.Fields.StatusReason))
				.GetAttributeValue<OptionSetValue>(CustomJob.Fields.StatusReason)?.Value
				== (int)CustomJob.StatusReasonEnum.WaitingOnSubJobs;
		}

		internal static void UpdatePaging(IOrganizationService service, Guid jobId, JobPagingInfo pagingInfo)
		{
			service.Update(
				new CustomJob
				{
					Id = jobId,
					PageNumber = pagingInfo.NextPage,
					PagingCookie = pagingInfo.Cookie
				});
		}

		internal static void SetStatus(IOrganizationService service, CustomJob.StatusReasonEnum status,
			Guid jobId, bool isTriggerRecurrence)
		{
			var job =
				new CustomJob
				{
					Id = jobId,
					StatusReason = status
				};

			if (isTriggerRecurrence)
			{
				job.RecurrenceUpdatedTrigger = new Random().Next(111111, 99999999).ToString();
				job.TargetDate = new DateTime(2100, 1, 1);
			}

			service.Update(job);
		}

		internal static void Close(IOrganizationService service, CustomJob.StatusReasonEnum status, Guid jobId,
			bool isPropagate = false)
		{
			service.Update(
				new CustomJob
				{
					Id = jobId,
					Status = CustomJob.StatusEnum.Inactive,
					StatusReason = status
				});

			if (isPropagate)
			{
				var parent = service.Retrieve(CustomJob.EntityLogicalName, jobId, new ColumnSet(CustomJob.Fields.ParentJob))
					.ToEntity<CustomJob>().ParentJob;

				if (parent != null)
				{
					Close(service, status, parent.Value, true);
				}
			}
		}

		internal static string BuildNoTraceExceptionMessage(Exception ex)
		{
			return "\r\nException: " + ex.GetType() + " => \"" + ex.Message + "\"." +
				"\r\nSource: " + ex.Source +
				(ex.InnerException == null
					? ""
					: "\r\n\r\nInner exception: " + ex.InnerException.GetType() + " => \"" +
						ex.InnerException.Message + "\"." +
						"\r\nSource: " + ex.InnerException.Source +
						"\r\n\r\n" + ex.InnerException.StackTrace);
		}

		public static void UpdateRetryTargetDate(IOrganizationService service, CustomJob job, CrmLog log)
		{
			DateTime? nextRecurrence;

			if (job.RetrySchedule == null)
			{
				log.Log("No retry schedule.");
				nextRecurrence = DateTime.UtcNow.AddMinutes(1);
			}
			else
			{
				log.Log($"Getting next retry occurrence for {job.RetrySchedule}.");
				var action = new GlobalActions.ys_CustomJobGetNextRecurrenceDate(
					new EntityReference(RecurrenceRule.EntityLogicalName, job.RetrySchedule.Value), service);
				nextRecurrence = action.Execute().NextTargetDate;
				log.Log($"Next retry occurrence: '{nextRecurrence}'.");
			}

			var targetDate = nextRecurrence > new DateTime(1900) ? nextRecurrence : null;

			log.Log($"Updating target date to '{targetDate}' UTC ...");
			service.Update(
				new CustomJob
				{
					Id = job.Id,
					TargetDate = targetDate
				});
			log.Log($"Updated target date to '{targetDate}' UTC");
		}
	}
}
