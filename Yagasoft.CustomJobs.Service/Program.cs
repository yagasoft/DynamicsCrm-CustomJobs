#region Imports

using System;
using System.ServiceProcess;

#endregion

namespace Yagasoft.CustomJobs.Service
{
	static class Program
	{
		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			if (Environment.UserInteractive)  
			{  
				var service = new CustomJobService();  
				service.Debug(args);
			}  
			else  
			{  
				var servicesToRun =
					new ServiceBase[]
					{
						new CustomJobService()
					};
				ServiceBase.Run(servicesToRun);
			}  
		}
	}
}
