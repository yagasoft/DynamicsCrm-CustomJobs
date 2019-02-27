#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using Yagasoft.CustomJobs.Job.Abstract;
using Yagasoft.Libraries.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

#endregion

namespace Yagasoft.CustomJobs.Job.MultiTarget
{
	[Log]
	internal class NoPagingNormalJob : NoPagingJob
	{
		public NoPagingNormalJob(CustomJob job, IOrganizationService service, IOrganizationServiceFactory serviceFactory,
			CrmLog log) : base(job, service, serviceFactory, log)
		{ }

		protected override JobPagingInfo GetTargets()
		{
			var jobPagingInfo = new JobPagingInfo();

			var targets = new List<Guid>();

			if (!string.IsNullOrEmpty(Job.TargetLogicalName))
			{
				log.Log("Converting FetchXML to QueryExpression ...", LogLevel.Debug);
				var query = ((FetchXmlToQueryExpressionResponse)
					Service.Execute(new FetchXmlToQueryExpressionRequest
									{
										FetchXml = Job.TargetXML
									})).Query;
				log.Log("Converted.", LogLevel.Debug);

				query.PageInfo = new PagingInfo
								 {
									 Count = Job.RecordsPerPage ?? 5000,
									 PageNumber = Job.PageNumber ?? 1,
									 PagingCookie = Job.PagingCookie
								 };

				log.Log($"Retrieving a max of {query.PageInfo.Count} per page ...", LogLevel.Debug);
				EntityCollection result;

				do
				{
					log.Log($"Retrieving page {query.PageInfo.PageNumber} ...", LogLevel.Debug);
					result = Service.RetrieveMultiple(query);
					targets.AddRange(result.Entities.Select(entity => entity.Id));
					log.Log($"Found {result.Entities.Count}.");

					jobPagingInfo.NextPage = query.PageInfo.PageNumber = query.PageInfo.PageNumber + 1;
					jobPagingInfo.Cookie = query.PageInfo.PagingCookie = result.PagingCookie;
				}
				while (result.MoreRecords && Job.PageNumber == null);
			}

			jobPagingInfo.Targets = targets;
			log.Log($"Target count: {targets.Count}.");

			return jobPagingInfo;
		}
	}
}
