#region Imports

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using static LinkDev.Libraries.Common.CrmHelpers;
using static LinkDev.CustomJobs.Helpers;

#endregion

namespace LinkDev.CustomJobs
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
			log.Log("Getting input arguments ...", LogLevel.Debug);

			var name = executionContext.GetValue(codeActivity.NameArg);
			var actionName = executionContext.GetValue(codeActivity.ActionNameArg);
			var inputParams = executionContext.GetValue(codeActivity.InputParamsArg);
			var workflowRef = executionContext.GetValue(codeActivity.WorkflowRefArg);
			var userRef = executionContext.GetValue(codeActivity.UserRefArg);
			var targetDate = executionContext.GetValue(codeActivity.TargetDateArg);
			var timer = executionContext.GetValue(codeActivity.TimerArg);
			var timerBase = executionContext.GetValue(codeActivity.TimerBaseArg);
			var workingHours = executionContext.GetValue(codeActivity.WorkingHoursArg);
			var targetLogicalName = executionContext.GetValue(codeActivity.TargetLogicalNameArg);
			var targetId = executionContext.GetValue(codeActivity.TargetIdArg);
			var targetXml = executionContext.GetValue(codeActivity.TargetXmlArg);
			var count = executionContext.GetValue(codeActivity.CountArg);
			var recurrenceIdsCsv = executionContext.GetValue(codeActivity.RecurrenceIdsCsvArg);

			log.Log("Done.", LogLevel.Debug);

			log.Log("Validating input arguments ...", LogLevel.Debug);

			if (actionName != null && workflowRef != null)
			{
				throw new InvalidPluginExecutionException("Either an action or workflow can be specified.");
			}

			if (actionName == null && workflowRef == null)
			{
				throw new InvalidPluginExecutionException("An action or workflow must be specified.");
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

			if (workflowRef != null && !IsRecordExists(service, "workflow", workflowRef.Id))
			{
				throw new InvalidPluginExecutionException($"Couldn't find workflow with ID '{workflowRef.Id}'.");
			}

			log.Log("Input arguments are valid.");

			log.Log("Validating recurrences ...", LogLevel.Debug);

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

					if (!IsRecordExists(service, RecurrenceRule.EntityLogicalName, guid))
					{
						throw new InvalidPluginExecutionException($"Couldn't find recurrence with ID '{guid}'.");
					}

					recurrences.Add(new EntityReference(RecurrenceRule.EntityLogicalName, guid));
				}
			}

			log.Log("Recurrences are valid.");

			log.Log("Creating job ...", LogLevel.Debug);

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

			var id = service.Create(job);

			log.Log($"Job created with ID '{id}'.");

			if (recurrences.Any())
			{
				log.Log("Associating recurrences ...", LogLevel.Debug);

				service.Associate(CustomJob.EntityLogicalName, id, new Relationship(CustomJob.Relations.NToN.Recurrences),
					new EntityReferenceCollection(recurrences));

				log.Log("Done.", LogLevel.Debug);
			}

			if (job.MarkForWaiting != true)
			{
				SetStatus(service, CustomJob.StatusReasonEnum.Waiting, id, false);
			}

			executionContext.SetValue(codeActivity.NewJobArg, new EntityReference(CustomJob.EntityLogicalName, id));
		}
	}
}
