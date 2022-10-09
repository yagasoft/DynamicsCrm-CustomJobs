#region Imports

using System;
using System.Activities;
using System.Linq;
using Yagasoft.Libraries.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Workflow;

#endregion

namespace Yagasoft.CustomJobs.Process.Steps
{
	/// <summary>
	///     This custom workflow step ... .<br />
	///     Version: 1.1.1
	/// </summary>
	public class YsGetNextJobBatch : CodeActivity
	{
		#region Arguments

		[Input("Percentage")]
		[Default("100")]
		public InArgument<int> PercentageArg { get; set; }

		[RequiredArgument]
		[Input("Lock ID")]
		public InArgument<string> LockIdArg { get; set; }

		[RequiredArgument]
		[Output("Jobs")]
		public OutArgument<string> JobsArg { get; set; }

		#endregion

		protected override void Execute(CodeActivityContext executionContext)
		{
			new YsGetNextJobBatchLogic().Execute(this, executionContext);
		}
	}

	internal class YsGetNextJobBatchLogic : StepLogic<YsGetNextJobBatch>
	{
		protected override void ExecuteLogic()
		{
			var percent = codeActivity.PercentageArg.Get(ExecutionContext);

			var lockId = codeActivity.LockIdArg.Get(ExecutionContext);
			lockId.RequireFilled(nameof(lockId));

			var config = CrmHelpers.GetGenericConfig(Service, Context.OrganizationId).ToEntity<CommonConfiguration>();
			var jobTimeout = TimeSpan.FromMinutes(config.JobTimeout.GetValueOrDefault(20));
			Log.Log($"jobTimeout: {jobTimeout}");

			codeActivity.JobsArg.Set(ExecutionContext,
				Batch.GetNextJobBatch(percent == 0 ? 100 : percent, lockId, jobTimeout, Service, Log).StringAggregate());
		}
	}
}
