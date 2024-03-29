﻿#region Imports

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk.Query;

using NLog;

using Yagasoft.CustomJobs.Engine;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.Libraries.Common;
using Yagasoft.Libraries.EnhancedOrgService.Helpers;
using Yagasoft.Libraries.EnhancedOrgService.Params;
using Yagasoft.Libraries.EnhancedOrgService.Router;
using Yagasoft.Libraries.EnhancedOrgService.Services.Enhanced;
using Yagasoft.Log;
using ILogger = Yagasoft.Libraries.Common.ILogger;
using Timer = System.Timers.Timer;

#endregion

namespace Yagasoft.CustomJobs.Service
{
	[Log]
	public partial class CustomJobService : ServiceBase
	{
		private string serviceId;
		private CustomJobEngine customJobEngine;

		private TimeSpan jobCheckInterval;
		private TimeSpan jobTimeout;

		private int maxConnectionsPerNode;

		private EngineParams engineParams;

		private Thread jobQueueThread;
		private Thread fixDataThread;
		private Thread jobExecutionThread;

		private bool isRunning;
		private bool isExit;
		private readonly object initLockObj = new();

		private IEnhancedOrgService service;

		private SemaphoreSlim semaphore;

		private readonly CancellationTokenSource exitCancellationToken = new();

		private static ILogger debugLog;

		public CustomJobService()
		{
			debugLog = JobLogger.GetLogger("debug");
			InitializeComponent();
		}

		internal void Debug(string[] args)
		{
			OnStart(args);
			Console.ReadKey();
			OnStop();
		}

		protected override void OnStart(string[] args)
		{
			new Thread(InitialiseService).Start();
		}

		private void InitialiseService()
		{
			ILogger log = null;

			try
			{
				log = JobLogger.GetLogger("init");

				while (!isRunning && !isExit)
				{
					try
					{
						serviceId = ConfigHelpers.Get("ServiceId");
						serviceId = serviceId == "MyService" ? Guid.NewGuid().ToString() : serviceId;
						log.Log($"Service ID: {serviceId}");

						InitialiseConnections(log);

						log.Log("Retrieving CRM configuration ...");
						var config = CrmHelpers.GetGenericConfig(service, Guid.NewGuid()).ToEntity<CommonConfiguration>();

						if (config.JobsPlatform != CommonConfiguration.JobsPlatformEnum.Service)
						{
							throw new ConfigurationErrorsException($"Platform is not set to 'service' in the configuration. ({config.JobsPlatform})");
						}

						jobCheckInterval = TimeSpan.FromMinutes(config.JobCheckInterval.GetValueOrDefault(1));
						log.Log($"jobCheckInterval: {jobCheckInterval}");
						jobTimeout = TimeSpan.FromMinutes(config.JobTimeout.GetValueOrDefault(20));
						log.Log($"jobTimeout: {jobTimeout}");

						engineParams =
							new EngineParams
							{
								TargetExecutionMode = config.TargetExecutionMode ?? CommonConfiguration.TargetExecutionModeEnum.Sequential,
								MaximumDegreeOfParallelism = Math.Min(config.MaxDegreeofParallelism ?? 1, maxConnectionsPerNode),
								JobTimeout = config.JobTimeout ?? 20
							};

						log.Log($"TargetExecutionMode: {engineParams.TargetExecutionMode}");
						log.Log($"MaximumDegreeOfParallelism: {engineParams.MaximumDegreeOfParallelism}");

						semaphore = new SemaphoreSlim(config.MaxJobsinParallel.GetValueOrDefault(1));

						if (!int.TryParse(ConfigHelpers.Get("JobsPercentage"), out var jobsPercentage))
						{
							jobsPercentage = 100;
						}

						log.Log($"Starting engine ...");
						customJobEngine = new CustomJobEngine(jobsPercentage, serviceId, debugLog);

						jobQueueThread = new Thread(RunJobQueue);
						fixDataThread = new Thread(RunFixData);
						jobExecutionThread = new Thread(RunJobExecution);

						lock (initLockObj)
						{
							if (!isExit)
							{
								isRunning = true;
							}
						}

						log.Log($"Starting threads ...");
						jobQueueThread.Start();
						fixDataThread.Start();
						jobExecutionThread.Start();
					}
					catch (Exception ex)
					{
						// TODO log to Event Log
						log.Log(ex, information: ex.BuildExceptionMessage());
					}

					if (!isRunning && !isExit)
					{
						Task.Delay(60000).Wait(exitCancellationToken.Token);
					}
				}
			}
			catch (OperationCanceledException)
			{ }
			catch (Exception ex)
			{
				// TODO log to Event Log
				log?.Log(ex, information: ex.BuildExceptionMessage());
				log?.ExecutionFailed();
			}
			finally
			{
				if (!isRunning)
				{
					log?.ExecutionFailed();
				}

				log?.LogExecutionEnd();
			}
		}

		private void InitialiseConnections(ILogger log)
		{
			if (!int.TryParse(ConfigHelpers.Get("MaxConnectionsPerNode"), out maxConnectionsPerNode))
			{
				throw new ConfigurationErrorsException("Max connections is misformatted in configuration.");
			}

			log.Log($"maxConnectionsPerNode: {maxConnectionsPerNode}");

			if (maxConnectionsPerNode < 3)
			{
				throw new ConfigurationErrorsException("Max connections must be 3 or above.");
			}

			var connections = ConfigHelpers.Get("Connections")
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(e => ConfigHelpers.Get(e.Trim())).ToArray();

			if (!connections.Any())
			{
				throw new ConfigurationErrorsException("Missing CRM connection string in configuration.");
			}

			if (connections.Length > 1)
			{
				log.Log($"Starting load-balanced CRM service ...");

				var serviceParams =
					new ServiceParams
					{
						OperationHistoryLimit = 1,
						ConnectionParams = new ConnectionParams { Timeout = TimeSpan.FromMinutes(engineParams.JobTimeout) },
						PoolParams = new PoolParams { PoolSize = maxConnectionsPerNode }
					}.AutoSetMaxPerformanceParams();

				service =
					EnhancedServiceHelper.GetSelfBalancingService(serviceParams,
						connections.Select(
							e =>
							{
								var poolParams = serviceParams.Copy();
								poolParams.ConnectionParams.ConnectionString = e;
								return EnhancedServiceHelper.GetPool(poolParams);
							}).ToArray(),
						new RouterRules
						{
							RouterMode = RouterMode.LeastLatency
						}).Result;
			}
			else
			{
				log.Log($"Starting CRM service ...");
				service = EnhancedServiceHelper.GetPoolingService(connections.First(), maxConnectionsPerNode);
			}
		}

		protected override void OnStop()
		{
			lock (initLockObj)
			{
				var log = JobLogger.GetLogger("decommission");

				isExit = true;
				isRunning = false;

				try
				{
					if (!exitCancellationToken.IsCancellationRequested)
					{
						exitCancellationToken.Cancel();
					}

					log.Log("Waiting for jobQueueThread ...");
					jobQueueThread?.Join();

					log.Log("Waiting for jobExecutionThread ...");
					jobExecutionThread?.Join();

					log.Log("Waiting for fixDataThread ...");
					fixDataThread?.Join();

					if (service != null)
					{
						try
						{
							ResetServiceLockedJobs(log);
						}
						catch (OperationCanceledException)
						{ }
						catch (Exception ex)
						{
							log.ExecutionFailed();
							log.Log(ex);
						}
					}
				}
				catch (Exception ex)
				{
					log?.ExecutionFailed();
					log?.Log(ex);
					// TODO log to Event Log
				}
				finally
				{
					log?.LogExecutionEnd();
					debugLog?.LogExecutionEnd();
				}
			}
		}

		private void RunJobQueue()
		{
			ILogger log = null;

			try
			{
				log = JobLogger.GetLogger("queue");

				while (isRunning)
				{
					try
					{
						try
						{
							var jobBatch = customJobEngine.GetNextJobBatch(service, log);
							customJobEngine.QueueJobs(jobBatch, service, log);
						}
						catch (Exception ex)
						{
							log.Log(ex, information: ex.BuildExceptionMessage());
						}
						finally
						{
							Task.Delay(jobCheckInterval).Wait(exitCancellationToken.Token);
						}
					}
					catch
					{
						// TODO log to Event Log
						Task.Delay(1000).Wait(exitCancellationToken.Token);
					}
				}
			}
			catch (OperationCanceledException)
			{ }
			catch (Exception ex)
			{
				// TODO log to Event Log
				log?.Log(ex, information: ex.BuildExceptionMessage());
				log?.ExecutionFailed();
			}
			finally
			{
				log?.LogExecutionEnd();
			}
		}

		private void RunFixData()
		{
			ILogger log = null;

			try
			{
				log = JobLogger.GetLogger("fixer");

				while (isRunning)
				{
					try
					{
						try
						{
							customJobEngine.FixDataCorruptJobs(service, log);
						}
						catch (Exception ex)
						{
							log.Log(ex, information: ex.BuildExceptionMessage());
							log.ExecutionFailed();
						}
						finally
						{
							Task.Delay(jobCheckInterval).Wait(exitCancellationToken.Token);
						}
					}
					catch
					{
						// TODO log to Event Log
						Task.Delay(1000).Wait(exitCancellationToken.Token);
					}
				}
			}
			catch (OperationCanceledException)
			{ }
			catch (Exception ex)
			{
				// TODO log to Event Log
				log?.Log(ex, information: ex.BuildExceptionMessage());
				log?.ExecutionFailed();
			}
			finally
			{
				log?.LogExecutionEnd();
			}
		}

		private void RunJobExecution()
		{
			try
			{
				while (isRunning)
				{
					try
					{
						var job = customJobEngine.Jobs.Dequeue(exitCancellationToken);

						Task.Factory.StartNew(
							() =>
							{
								ILogger log = null;

								try
								{
									try
									{
										semaphore.Wait(exitCancellationToken.Token);

										var jobRow = service.Retrieve<CustomJob>(CustomJob.EntityLogicalName,
											job, new ColumnSet(CustomJob.Fields.ParentJob));
										log = JobLogger.GetLogger((jobRow.ParentJob ?? job).ToString().ToUpper());

										customJobEngine.ProcessQueuedJob(job, engineParams, service, log);
									}
									catch (OperationCanceledException)
									{ }
									catch (Exception ex)
									{
										log?.ExecutionFailed();

										log ??= JobLogger.GetLogger("exec");
										log.Log(ex, information: ex.BuildExceptionMessage());
									}
									finally
									{
										log ??= JobLogger.GetLogger("exec");

										if (IsJobExist(job, log))
										{
											ResetJob(job, log);
										}
									}
								}
								catch
								{
									// TODO log to Event Log
								}
								finally
								{
										log?.LogExecutionEnd();
									semaphore.Release();
								}
							},
							CancellationTokenSource.CreateLinkedTokenSource(
								new CancellationTokenSource(TimeSpan
									.FromMilliseconds(jobTimeout.TotalMilliseconds)).Token,
								exitCancellationToken.Token).Token,
							TaskCreationOptions.LongRunning,
							TaskScheduler.Default);
					}
					catch (OperationCanceledException)
					{ }
					catch
					{
						// TODO log to Event Log
						Task.Delay(1000).Wait(exitCancellationToken.Token);
					}
				}
			}
			catch (OperationCanceledException)
			{ }
			catch (Exception ex)
			{
				// TODO log to Event Log
			}
		}

		private void ResetServiceLockedJobs(ILogger log)
		{
			log.Log($"Retrieving service-locked jobs ...");

			var query = new FetchExpression(
				$@"<fetch no-lock='true'>
  <entity name='{CustomJob.EntityLogicalName}'>
    <attribute name='{CustomJob.Fields.CustomJobId}' />
    <filter>
      <condition attribute='{CustomJob.Fields.LockID}' operator='eq' value='{serviceId}' />
    </filter>
  </entity>
</fetch>");

			var jobs = service.RetrieveMultiple(query).Entities.ToEntity<CustomJob>().Select(j => j.Id).ToArray();
			log.Log($"Found: {jobs.Length}");

			foreach (var job in jobs)
			{
				ResetJob(job, log);
			}
		}

		private void ResetJob(Guid job, ILogger log)
		{
			try
			{
				log.Log($"Resetting lock on {job} ...");
				service.Update(
					new CustomJob
					{
						Id = job,
						LockID = null
					});
			}
			catch (Exception ex)
			{
				log.Log($"Failed to reset lock.");
				log.Log(ex);
			}
		}

		private bool IsJobExist(Guid job, ILogger log)
		{
			log.Log($"Checking job '{job}' still exists ...");

			var query = new FetchExpression(
				$@"<fetch no-lock='true'>
  <entity name='{CustomJob.EntityLogicalName}'>
    <attribute name='{CustomJob.Fields.CustomJobId}' />
    <filter>
      <condition attribute='{CustomJob.Fields.CustomJobId}' operator='eq' value='{job}' />
    </filter>
  </entity>
</fetch>");

			var isExist = service.RetrieveMultiple(query).Entities.FirstOrDefault()?.ToEntity<CustomJob>().Id != null;
			log.Log($"isExist: {isExist}");

			return isExist;
		}
	}
}
