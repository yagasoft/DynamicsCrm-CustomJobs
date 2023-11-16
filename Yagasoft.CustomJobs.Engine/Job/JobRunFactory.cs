#region Imports

using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.CustomJobs.Engine.Job.Abstract;
using Yagasoft.CustomJobs.Engine.Job.MultiTarget;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Engine.Job
{
	internal static class JobRunFactory
	{
		internal static JobRun GetJobRun(CustomJob job, EngineParams engineParams, bool isRetry, IOrganizationService service, ILogger log)
		{
			try
			{
				log.LogFunctionStart();

				if (IsRecurrent(job, log))
				{
					return new RecurrentRunJob(job, engineParams, service, log);
				}

				log.Log("Single run job.");

				JobTarget target;

				if (IsSingleOrNoTarget(job, log))
				{
					target = new SingleTargetJob(job, engineParams, service, log);
				}
				else if (IsPaging(job, log))
				{
					if (isRetry)
					{
						target = new PagingRetryJob(job, engineParams, service, log);
					}
					else
					{
						target = new PagingNormalJob(job, engineParams, service, log);
					}
				}
				else
				{
					if (isRetry)
					{
						target = new NoPagingRetryJob(job, engineParams, service, log);
					}
					else
					{
						target = new NoPagingNormalJob(job, engineParams, service, log);
					}
				}

				return new SingleRunJob(job, engineParams, target, service, log);
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

		private static bool IsRecurrent(CustomJob job, ILogger log)
		{
			try
			{
				log.LogFunctionStart();

				var isRecurrent = job.RecurrentJob == true;
				log.Log($"IsRecurrent: {isRecurrent}");
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

		private static bool IsSingleOrNoTarget(CustomJob job, ILogger log)
		{
			try
			{
				log.LogFunctionStart();

				var isSingleOrNoTarget = string.IsNullOrWhiteSpace(job.TargetLogicalName)
					|| job.TargetID?.Split(',').Length <= 1;
				log.Log($"IsSingleOrNoTarget: {isSingleOrNoTarget}");
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

		private static bool IsPaging(CustomJob job, ILogger log)
		{
			try
			{
				log.LogFunctionStart();

				var isPaging = job.RecordsPerPage > 0;
				log.Log($"IsPaging: {isPaging}");
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

		private static bool IsSinglePage(CustomJob job, ILogger log, IOrganizationService service)
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
