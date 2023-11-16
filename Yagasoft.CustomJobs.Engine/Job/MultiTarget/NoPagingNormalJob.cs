#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job.MultiTarget
{
	[Log]
	internal class NoPagingNormalJob : NoPagingJob
	{
		public NoPagingNormalJob(CustomJob job, EngineParams engineParams, IOrganizationService service, ILogger log)
			: base(job, engineParams, service, log)
		{ }

		protected override JobPagingInfo GetTargets()
		{
			var jobPagingInfo = new JobPagingInfo();

			var targets = new List<Guid>();

			if (!string.IsNullOrEmpty(Job.TargetLogicalName))
			{
				Log.Log("Converting FetchXML to QueryExpression ...");
				var query = ((FetchXmlToQueryExpressionResponse)
					Service.Execute(
						new FetchXmlToQueryExpressionRequest
						{
							FetchXml = Job.TargetXML
						})).Query;
				Log.Log("Converted.");

				query.PageInfo =
					new PagingInfo
					{
						Count = Job.RecordsPerPage ?? 5000,
						PageNumber = Job.PageNumber ?? 1,
						PagingCookie = Job.PagingCookie
					};

				Log.Log($"Retrieving a max of {query.PageInfo.Count} per page ...");
				EntityCollection result;

				do
				{
					Log.Log($"Retrieving page {query.PageInfo.PageNumber} ...");
					result = Service.RetrieveMultiple(query);
					targets.AddRange(result.Entities.Select(entity => entity.Id));
					Log.Log($"Found {result.Entities.Count}.");

					jobPagingInfo.NextPage = query.PageInfo.PageNumber += 1;
					jobPagingInfo.Cookie = query.PageInfo.PagingCookie = result.PagingCookie;
				}
				while (result.MoreRecords && Job.PageNumber == null);
			}

			jobPagingInfo.Targets = targets;
			Log.Log($"Target count: {targets.Count}.");

			return jobPagingInfo;
		}
	}
}
