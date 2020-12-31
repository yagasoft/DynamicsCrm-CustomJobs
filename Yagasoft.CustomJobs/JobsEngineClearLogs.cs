#region Imports

using System;
using System.Activities;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Workflow;

#endregion

namespace Yagasoft.CustomJobs
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
			var job = codeActivity.CustomJobReference.Get(ExecutionContext);

			Log.Log($"Fetching all logs related to the job '{job.Id}' ...");
			var logs = (from logQ in new XrmServiceContext(Service).CustomJobLogSet
						where logQ.CustomJob == job.Id
						select logQ.CustomJobLogId).ToList();
			Log.Log($"Fetched all logs related to the job '{job.Id}': {logs.Count}.");

			Log.Log($"Deleting Log entries ...");

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

				Service.Execute(request);
			}

			Log.Log($"Deleted Log entries.");
		}
	}
}
