#region Imports

using System;
using System.Activities;
using System.Linq;
using LinkDev.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Workflow;

#endregion

namespace LinkDev.CustomJobs
{
	/// <summary>
	///     This custom workflow step ... .<br />
	///     Version: 0.0.1
	/// </summary>
	public class JobsEngineClearLogs : CodeActivity
	{
		#region Arguments

		[RequiredArgument]
		[Input("Custom Job")]
		[ReferenceTarget(CustomJob.EntityLogicalName)]
		public InArgument<EntityReference> CustomJobReference { get; set; }

		#endregion

		protected override void Execute(CodeActivityContext executionContext)
		{
			new JobEngineClearLogsLogic().Execute(this, executionContext);
		}
	}

	internal class JobEngineClearLogsLogic : StepLogic<JobsEngineClearLogs>
	{
		protected override void ExecuteLogic()
		{
			var job = codeActivity.CustomJobReference.Get(executionContext);

			log.Log($"Fetching all logs related to the job '{job.Id}' ...", LogLevel.Debug);
			var logs = (from logQ in new XrmServiceContext(service).CustomJobLogSet
						where logQ.CustomJob == job.Id
						select logQ.CustomJobLogId).ToList();
			log.Log($"Fetched all logs related to the job '{job.Id}': {logs.Count}.");

			log.Log($"Deleting log entries ...", LogLevel.Debug);

			var request = new ExecuteMultipleRequest
						  {
							  Requests = new OrganizationRequestCollection(),
							  Settings = new ExecuteMultipleSettings
										 {
											 ContinueOnError = true,
											 ReturnResponses = false
										 }
						  };

			for (var i = 0; i < Math.Ceiling(logs.Count / 1000d); i++)
			{
				request.Requests.Clear();
				request.Requests.AddRange(logs.Skip(i).Take(1000)
					.Select(logId => new DeleteRequest
					{
						Target = new EntityReference(CustomJobLog.EntityLogicalName, logId.Value)
					}));

				service.Execute(request);
			}

			log.Log($"Deleted log entries.");
		}
	}
}
