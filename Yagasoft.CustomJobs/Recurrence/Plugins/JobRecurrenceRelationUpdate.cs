#region Imports

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Recurrence.Plugins
{
	/// <summary>
	///     This plugin handles custom job recurrence relation updates.<br />
	///     Version: 0.1.1
	/// </summary>
	public class JobRecurrenceRelationUpdate : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			new JobRecurrenceRelationUpdateLogic().Execute(this, serviceProvider, false);
		}
	}

	internal class JobRecurrenceRelationUpdateLogic : PluginLogic<JobRecurrenceRelationUpdate>
	{
		public JobRecurrenceRelationUpdateLogic() : base(null, PluginStage.All, CustomJobRecurrence.EntityLogicalName)
		{ }

		protected override void ExecuteLogic()
		{
			switch (Message)
			{
				case "Create":
				{
					var target = GetTarget<CustomJobRecurrence>();
					var job = target.CustomJob.GetValueOrDefault();
					Helpers.SetTargetDate(job, Helpers.LoadRules(job, Service, Log), Service, Log);
					break;
				}
				
				case "Update":
				{
					var preImage = GetPreImage<CustomJobRecurrence>();
					var job = preImage.CustomJob.GetValueOrDefault();
					Helpers.SetTargetDate(job, Helpers.LoadRules(job, Service, Log), Service, Log);
					var postImage = GetPostImage<CustomJobRecurrence>();
					job = postImage.CustomJob.GetValueOrDefault();
					Helpers.SetTargetDate(job, Helpers.LoadRules(job, Service, Log), Service, Log);
					break;
				}
				
				case "Delete":
				{
					var preImage = GetPreImage<CustomJobRecurrence>();
					var job = preImage.CustomJob.GetValueOrDefault();
					Helpers.SetTargetDate(job, Helpers.LoadRules(job, Service, Log), Service, Log);
					break;
				}
			}
		}
	}
}
