#region Imports

using System;
using Yagasoft.CustomJobs.Job.Abstract;
using Yagasoft.CustomJobs.Job.MultiTarget;
using Yagasoft.Libraries.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

#endregion

namespace Yagasoft.CustomJobs.Job
{
	internal static class JobRunFactory
	{
		internal static JobRun GetJobRun(CustomJob job, bool isRetry, IOrganizationService service,
			IOrganizationServiceFactory factory, CrmLog log)
		{
			try
			{
				log.LogFunctionStart();

				if (IsRecurrent(job, log))
				{
					return new RecurrentRunJob(job, service, log);
				}

				log.Log("Single run job.");

				JobTarget target;

				if (IsSingleOrNoTarget(job, log))
				{
					target = new SingleTargetJob(job, service, factory, log);
				}
				else if (IsPaging(job, log))
				{
					if (isRetry)
					{
						target = new PagingRetryJob(job, service, factory, log);
					}
					else
					{
						target = new PagingNormalJob(job, service, factory, log);
					}
				}
				else
				{
					if (isRetry)
					{
						target = new NoPagingRetryJob(job, service, factory, log);
					}
					else
					{
						target = new NoPagingNormalJob(job, service, factory, log);
					}
				}

				return new SingleRunJob(job, target, service, log);
			}
			catch (Exception ex)
			{
				log.Log(ex);
				throw;
			}
			finally
			{
				log.LogFunctionEnd();
			}
		}

		private static bool IsRecurrent(CustomJob job, CrmLog log)
		{
			try
			{
				log.LogFunctionStart();

				var isRecurrent = job.RecurrentJob == true;
				log.Log($"{isRecurrent}");
				return isRecurrent;
			}
			catch (Exception ex)
			{
				log.Log(ex);
				throw;
			}
			finally
			{
				log.LogFunctionEnd();
			}
		}

		private static bool IsSingleOrNoTarget(CustomJob job, CrmLog log)
		{
			try
			{
				log.LogFunctionStart();

				var isSingleOrNoTarget = string.IsNullOrWhiteSpace(job.TargetLogicalName)
					|| job.TargetID?.Split(',').Length <= 1;
				log.Log($"{isSingleOrNoTarget}");
				return isSingleOrNoTarget;
			}
			catch (Exception ex)
			{
				log.Log(ex);
				throw;
			}
			finally
			{
				log.LogFunctionEnd();
			}
		}

		private static bool IsPaging(CustomJob job, CrmLog log)
		{
			try
			{
				log.LogFunctionStart();

				var isPaging = job.RecordsPerPage > 0;
				log.Log($"{isPaging}");
				return isPaging;
			}
			catch (Exception ex)
			{
				log.Log(ex);
				throw;
			}
			finally
			{
				log.LogFunctionEnd();
			}
		}

		private static bool IsSinglePage(CustomJob job, CrmLog log, IOrganizationService service)
		{
			if (string.IsNullOrWhiteSpace(job.TargetXML) || IsSingleOrNoTarget(job, log))
			{
				return true;
			}
			
			log.Log("Converting FetchXML to QueryExpression ...");
			var query = ((FetchXmlToQueryExpressionResponse)
				service.Execute(
					new FetchXmlToQueryExpressionRequest
					{
						FetchXml = job.TargetXML
					})).Query;
			log.Log("Converted.");

			// we only need to check if there are more pages or not
			query.ColumnSet = new ColumnSet(false);

			query.PageInfo =
				new PagingInfo
				{
					Count = job.RecordsPerPage ?? 5000,
					PageNumber = job.PageNumber ?? 1,
					PagingCookie = job.PagingCookie
				};

			log.Log($"Retrieving a max of {query.PageInfo.Count} per page ...");
			log.Log($"Retrieving page {query.PageInfo.PageNumber} ...");
			var result = service.RetrieveMultiple(query);
			log.Log($"More records: {result.MoreRecords}.");

			return result.MoreRecords;
		}
	}
}
