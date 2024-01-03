#region Imports

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using static Yagasoft.Libraries.Common.CrmHelpers;
using static Yagasoft.CustomJobs.Helpers;

#endregion

namespace Yagasoft.CustomJobs
{
	public class JobsEngineCreateJob : CodeActivity
	{
		#region Arguments

		[Input("Name")]
		public InArgument<string> NameArg { get; set; }

		[Input("Action Name")]
		public InArgument<string> ActionNameArg { get; set; }

		[Input("Input Parameters")]
		public InArgument<string> InputParamsArg { get; set; }

		[Input("Workflow")]
		[ReferenceTarget("workflow")]
		public InArgument<EntityReference> WorkflowRefArg { get; set; }

		[Input("Url")]
		public InArgument<string> UrlArg { get; set; }

		[Input("Context User")]
		[ReferenceTarget("systemuser")]
		public InArgument<EntityReference> UserRefArg { get; set; }

		[Input("Target Date")]
		public InArgument<DateTime> TargetDateArg { get; set; }

		[Input("Timer")]
		public InArgument<int> TimerArg { get; set; }

		[Input("Timer Base")]
		public InArgument<DateTime> TimerBaseArg { get; set; }

		[Input("Only Working Hours"), Default("False")]
		public InArgument<bool> WorkingHoursArg { get; set; }

		[Input("Target Logical Name")]
		public InArgument<string> TargetLogicalNameArg { get; set; }

		[Input("Target ID")]
		public InArgument<string> TargetIdArg { get; set; }

		[Input("Target XML")]
		public InArgument<string> TargetXmlArg { get; set; }

		[Input("Records Per Page")]
		public InArgument<int> CountArg { get; set; }

		[Input("Recurrence IDs CSV")]
		public InArgument<string> RecurrenceIdsCsvArg { get; set; }

		[Output("New Job")]
		[ReferenceTarget(CustomJob.EntityLogicalName)]
		public OutArgument<EntityReference> NewJobArg { get; set; }

		#endregion

		protected override void Execute(CodeActivityContext context)
		{
			new JobsEngineCreateJobLogic().Execute(this, context);
		}
	}

	internal class JobsEngineCreateJobLogic : StepLogic<JobsEngineCreateJob>
	{
		protected override void ExecuteLogic()
		{
			Log.Log("Getting input arguments ...");

			var name = ExecutionContext.GetValue(codeActivity.NameArg);
			var actionName = ExecutionContext.GetValue(codeActivity.ActionNameArg);
			var inputParams = ExecutionContext.GetValue(codeActivity.InputParamsArg);
			var workflowRef = ExecutionContext.GetValue(codeActivity.WorkflowRefArg);
			var url = ExecutionContext.GetValue(codeActivity.UrlArg);
			var userRef = ExecutionContext.GetValue(codeActivity.UserRefArg);
			var targetDate = ExecutionContext.GetValue(codeActivity.TargetDateArg);
			var timer = ExecutionContext.GetValue(codeActivity.TimerArg);
			var timerBase = ExecutionContext.GetValue(codeActivity.TimerBaseArg);
			var workingHours = ExecutionContext.GetValue(codeActivity.WorkingHoursArg);
			var targetLogicalName = ExecutionContext.GetValue(codeActivity.TargetLogicalNameArg);
			var targetId = ExecutionContext.GetValue(codeActivity.TargetIdArg);
			var targetXml = ExecutionContext.GetValue(codeActivity.TargetXmlArg);
			var count = ExecutionContext.GetValue(codeActivity.CountArg);
			var recurrenceIdsCsv = ExecutionContext.GetValue(codeActivity.RecurrenceIdsCsvArg);

			Log.Log("Done.");

			Log.Log("Validating input arguments ...");

			if (actionName != null && workflowRef != null && url.IsFilled())
			{
				throw new InvalidPluginExecutionException("Either an action or workflow or URL can be specified.");
			}

			if (actionName == null && workflowRef == null && url.IsEmpty())
			{
				throw new InvalidPluginExecutionException("An action or workflow or URL must be specified.");
			}

			if (targetDate != new DateTime(1, 1, 1) && timer != 0)
			{
				throw new InvalidPluginExecutionException($"Either a target date or timer can be specified.");
			}

			if (timer != 0 && timerBase == new DateTime(1, 1, 1))
			{
				throw new InvalidPluginExecutionException("A timer base must be specified.");
			}

			if (targetId != null && targetXml != null)
			{
				throw new InvalidPluginExecutionException("Either a target ID or XML can be specified.");
			}

			if ((targetId != null || targetXml != null) && targetLogicalName == null)
			{
				throw new InvalidPluginExecutionException("Target logical name must be specified.");
			}

			if (workflowRef != null && !IsRecordExists(Service, "workflow", workflowRef.Id))
			{
				throw new InvalidPluginExecutionException($"Couldn't find workflow with ID '{workflowRef.Id}'.");
			}

			Log.Log("Input arguments are valid.");

			Log.Log("Validating recurrences ...");

			var recurrencesString = recurrenceIdsCsv?.Split(',');
			var recurrences = new List<EntityReference>();

			if (recurrencesString != null && recurrencesString.Length > 0)
			{
				if (timer != 0)
				{
					throw new InvalidPluginExecutionException("Either a timer or recurrence can be specified.");
				}

				foreach (var recurrenceString in recurrencesString)
				{
					Guid guid;

					if (!Guid.TryParse(recurrenceString, out guid))
					{
						throw new InvalidPluginExecutionException("Poorly formatted recurrence ID.");
					}

					if (!IsRecordExists(Service, RecurrenceRule.EntityLogicalName, guid))
					{
						throw new InvalidPluginExecutionException($"Couldn't find recurrence with ID '{guid}'.");
					}

					recurrences.Add(new EntityReference(RecurrenceRule.EntityLogicalName, guid));
				}
			}

			Log.Log("Recurrences are valid.");

			Log.Log("Creating job ...");

			var job = new CustomJob
					  {
						  Name = name,
						  ActionName = actionName,
						  SerialisedInputParams = inputParams,
						  Workflow = workflowRef?.Id,
						  ContextUser = userRef?.Id,
						  TargetDate = targetDate == new DateTime(1, 1, 1) ? null : new DateTime?(targetDate),
						  Timer = timer == 0 ? null : new int?(timer),
						  TimerBase = timerBase == new DateTime(1, 1, 1) || timer == 0 ? null : new DateTime?(timerBase),
						  OnlyWorkingHours = workingHours,
						  TargetLogicalName = targetLogicalName,
						  TargetID = targetId,
						  TargetXML = targetXml,
						  RecordsPerPage = count == 0 ? null : new int?(count),
						  RecurrentJob = recurrences.Any(),
					  };
			job.MarkForWaiting = job.Timer != null || job.RecurrentJob == true;

			var id = Service.Create(job);

			Log.Log($"Job created with ID '{id}'.");

			if (recurrences.Any())
			{
				Log.Log("Associating recurrences ...");

				foreach (var recurrence in recurrences)
				{
					Service.Create(
						new CustomJobRecurrence
						{
							CustomJob = id,
							RecurrenceRule = recurrence.Id
						});
				}

				Log.Log("Done.");
			}

			if (job.MarkForWaiting != true)
			{
				SetStatus(Service, CustomJob.StatusReasonEnum.Waiting, id, false);
			}

			ExecutionContext.SetValue(codeActivity.NewJobArg, new EntityReference(CustomJob.EntityLogicalName, id));
		}
	}
}
