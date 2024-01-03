using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yagasoft.CustomJobs.Engine.Config
{
    public class EngineParams
    {
	    public CommonConfiguration.TargetExecutionModeEnum TargetExecutionMode { get; set; } =
			CommonConfiguration.TargetExecutionModeEnum.Sequential;
	    public int MaximumDegreeOfParallelism { get; set; } = 10;

	    public int JobTimeout { get; set; } = 20;
    }
}
