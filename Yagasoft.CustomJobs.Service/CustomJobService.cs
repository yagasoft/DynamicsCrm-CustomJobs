#region Imports

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Yagasoft.CustomJobs.Engine;
using Yagasoft.CustomJobs.Engine.Config;
using Yagasoft.Libraries.Common;
using Yagasoft.Libraries.EnhancedOrgService.Helpers;
using Yagasoft.Libraries.EnhancedOrgService.Params;
using Yagasoft.Libraries.EnhancedOrgService.Router;
using Yagasoft.Libraries.EnhancedOrgService.Services.Enhanced;
using Timer = System.Timers.Timer;

#endregion

namespace Yagasoft.CustomJobs.Service
{
	public partial class CustomJobService : ServiceBase
	{
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

		public CustomJobService()
		{
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
			CrmLog log = null;

			try
			{
				while (!isRunning && !isExit)
				{
					try
					{
						log = GetLog("Initialisation");

						InitialiseConnections(log);

						log.Log("Retrieving CRM configuration ...");
						var config = CrmHelpers.GetGenericConfig(service, "123").ToEntity<CommonConfiguration>();

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
								MaximumDegreeOfParallelism = Math.Min(config.MaxDegreeofParallelism ?? 1, maxConnectionsPerNode)
							};

						log.Log($"TargetExecutionMode: {engineParams.TargetExecutionMode}");
						log.Log($"MaximumDegreeOfParallelism: {engineParams.MaximumDegreeOfParallelism}");

						semaphore = new SemaphoreSlim(config.MaxJobsPerRun.GetValueOrDefault(1));

						log.Log($"Starting engine ...");
						customJobEngine = new CustomJobEngine(jobTimeout);

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
						log ??= GetLog("Initialisation");
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
			finally
			{
				if (!isRunning)
				{
					log?.ExecutionFailed();
				}

				log?.LogExecutionEnd();
			}
		}

		private void InitialiseConnections(CrmLog log)
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
				service =
					EnhancedServiceHelper.GetSelfBalancingService(
						connections.Select(e =>
							new EnhancedServiceParams(e)
							{
								PoolParams =
									new PoolParams
									{
										PoolSize = maxConnectionsPerNode
									},
								OperationHistoryLimit = 1
							}.AutoSetMaxPerformanceParams()),
						new RouterRules
						{
							RouterMode = RouterMode.LeastLatency
						}).Result;
			}
			else
			{
				log.Log($"Starting CRM service ...");
				service = EnhancedServiceHelper.GetPool(connections.First()).GetService(maxConnectionsPerNode);
			}
		}

		protected override void OnStop()
		{
			lock (initLockObj)
			{
				isExit = false;
				isRunning = false;

				if (!exitCancellationToken.IsCancellationRequested)
				{
					exitCancellationToken.Cancel();
				}
			}
		}

		private void RunJobQueue()
		{
			try
			{
				while (isRunning)
				{
					try
					{
						CrmLog log = null;

						try
						{
							log = GetLog("QueueJobs");
							var jobBatch = customJobEngine.GetNextJobBatch(service, log);
							customJobEngine.QueueJobs(jobBatch, service, log);
						}
						catch (Exception ex)
						{
							log ??= GetLog("QueueJobs");
							log.Log(ex, information: ex.BuildExceptionMessage());
							log.ExecutionFailed();
						}
						finally
						{
							log?.LogExecutionEnd();
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
		}

		private void RunFixData()
		{
			try
			{
				while (isRunning)
				{
					try
					{
						CrmLog log = null;

						try
						{
							log = GetLog("FixData");
							customJobEngine.FixDataCorruptJobs(service, log);
						}
						catch (Exception ex)
						{
							log ??= GetLog("FixData");
							log.Log(ex, information: ex.BuildExceptionMessage());
							log.ExecutionFailed();
						}
						finally
						{
							log?.LogExecutionEnd();
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

						Task.Run(
							() =>
							{
								try
								{
									CrmLog log = null;

									try
									{
										semaphore.Wait(exitCancellationToken.Token);
										
										var jobRow = service.Retrieve<CustomJob>(CustomJob.EntityLogicalName,
											job, new ColumnSet(CustomJob.Fields.ParentJob));
										log = GetLog((jobRow.ParentJob ?? job).ToString().ToUpper());
										
										customJobEngine.ProcessQueuedJob(job, engineParams, service, log);
									}
									catch (OperationCanceledException)
									{ }
									catch (Exception ex)
									{
										log ??= GetLog("JobStartFail");
										log.Log(ex, information: ex.BuildExceptionMessage());
										log.ExecutionFailed();
									}
									finally
									{
										log?.LogExecutionEnd();
										semaphore.Release();
									}
								}
								catch
								{
									// TODO log to Event Log
								}

								return null;
							},
							CancellationTokenSource.CreateLinkedTokenSource(
								new CancellationTokenSource(TimeSpan
									.FromMilliseconds(jobTimeout.TotalMilliseconds)).Token,
								exitCancellationToken.Token).Token);
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
		}

		private static CrmLog GetLog(string id)
		{
			var folder = ConfigHelpers.Get("LogFolderPath");
			folder = folder == "." ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs") : folder;

			var level = ConfigHelpers.Get("LogLevel");
			var dateFormat = ConfigHelpers.Get("LogFileDateFormat");

			int? frequency = null;

			if (int.TryParse(ConfigHelpers.Get("LogFileSplitFrequency"), out var frequencyParse))
			{
				frequency = frequencyParse;
			}

			if (!int.TryParse(ConfigHelpers.Get("LogFileSplitMode"), out var splitMode))
			{
				splitMode = (int)SplitMode.Size;
			}

			if (!bool.TryParse(ConfigHelpers.Get("LogFileGroupById"), out var groupById))
			{
				groupById = true;
			}

			if (!int.TryParse(ConfigHelpers.Get("LogFileMaxSizeKb"), out var maxSizeKb))
			{
				maxSizeKb = int.MaxValue;
			}

			if (groupById)
			{
				folder = Path.Combine(folder, id);
			}

			var log = new CrmLog(true, (LogLevel)int.Parse(level));

			log.InitOfflineLog(Path.Combine(folder, $"Log-{id}.csv"), false,
				new FileConfiguration
				{
					FileSplitMode = (SplitMode)splitMode,
					MaxFileSizeKb = maxSizeKb < 1 ? int.MaxValue : maxSizeKb,
					FileSplitFrequency = (SplitFrequency?)frequency,
					FileDateFormat = DateTime.Now.ToString(dateFormat)
				});

			return log;
		}
	}
}
