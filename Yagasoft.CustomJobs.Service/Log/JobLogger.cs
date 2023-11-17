using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Xrm.Sdk;
using NLog;
using NLog.Fluent;

using Yagasoft.Libraries.Common;
using ILogger = Yagasoft.Libraries.Common.ILogger;
using LogLevel = Yagasoft.Libraries.Common.LogLevel;

namespace Yagasoft.CustomJobs.Service.Log
{
	internal class JobLogger : LoggerBase
	{
		private static readonly bool isLogAllOnError =
			LogManager.Configuration.Variables["isLogAllOnError"].Render(LogEventInfo.CreateNullEvent()) == "true";

		private static readonly LogLevel maxLogLevel =
			LogManager.Configuration.Variables["maxLevel"].Render(LogEventInfo.CreateNullEvent()) switch {
				"Off" => LogLevel.None,
				"Error" => LogLevel.Error,
				"Warn" => LogLevel.Warning,
				"Trace" => LogLevel.Debug,
				_ => LogLevel.Info
				};
		
		private readonly Logger nlogger;
		private readonly FixedSizeQueue<LogEntry> logQueue = new(9999);

		public static ILogger GetLogger(string name, [CallerMemberName] string callingFunction = "")
		{
			return new JobLogger(LogManager.GetLogger(name), callingFunction);
		}
		
		private JobLogger(Logger nlogger, [CallerMemberName] string callingFunction = "")
			: base(maxLogLevel, callingFunction)
		{
			this.nlogger = nlogger;
			InitialiseLog();
		}

		private void InitialiseLog()
		{
			LogEntryGiven +=
				(sender, args) =>
				{
					if (args.LogEntry.Level == LogLevel.None)
					{
						args.LogEntry.Level = LogLevel.Info;
					}
				};
			
					if (!ExecutionStarted)
					{
						LogExecutionStart((LogEntry)null, ParentLog.EntryFunction, 0);
					}
		}
		
		protected override void ProcessLogEntry(LogEntry logEntry)
		{
			if (logEntry.ExceptionThrown)
			{
				ExceptionLogEntry = logEntry;
			}

			if (isLogAllOnError)
			{
				if (logEntry.Level == LogLevel.Error)
			{
				while (logQueue.Any())
				{
					SendToNLog(logQueue.Dequeue());
				}
				
				SendToNLog(logEntry);
				return;
			}
			}

			if (logEntry.Level > MaxLogLevel)
			{
				if (isLogAllOnError)
				{
					logQueue.Enqueue(logEntry);
				}
				
				return;
			}
			
			SendToNLog(logEntry);
		}

		private void SendToNLog(LogEntry logEntry)
		{
			var logLevel =
				logEntry.Level switch {
					LogLevel.None => NLog.LogLevel.Off,
					LogLevel.Error => NLog.LogLevel.Error,
					LogLevel.Warning => NLog.LogLevel.Warn,
					LogLevel.Info => NLog.LogLevel.Info,
					LogLevel.Debug => NLog.LogLevel.Debug,
					_ => throw new ArgumentOutOfRangeException(nameof(logEntry.Level), logEntry.Level, "Log level not supported.")
					};

			nlogger.Log(logLevel,
				"{msg},{index},{start},{ms},{info},{trace},{exMsg},{exName},{exSrc},{exTrace},{exInMsg},{exInName},{exInSrc},{exInTrace},{usr},{asm},{class},{fn},{line},{regType},{regId},{regName}",
				logEntry.Message, logEntry.CurrentEntryIndex, logEntry.StartDate, logEntry.ElapsedTime > -1 ? logEntry.ElapsedTime : null,
				logEntry.Information, logEntry.StackTrace,
				logEntry.Exception?.Message, logEntry.Exception?.GetType().Name, logEntry.Exception?.Source, logEntry.Exception?.StackTrace,
				logEntry.Exception?.InnerException?.Message, logEntry.Exception?.InnerException?.GetType().Name,
				logEntry.Exception?.InnerException?.Source, logEntry.Exception?.InnerException?.StackTrace,
				logEntry.UserId,
				logEntry.Assembly ?? Helpers.GetAssemblyName(-1, "Yagasoft.CustomJobs.Service"),
				logEntry.CallingClass, logEntry.CallingFunction, logEntry.CallingLineNumber,
				logEntry.RegardingType, logEntry.RegardingId, logEntry.RegardingName);
		}
	}
}
