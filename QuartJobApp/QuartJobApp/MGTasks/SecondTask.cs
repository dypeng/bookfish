using log4net;
using Quartz;
using System;

namespace MGTasks
{
    public class SecondTask : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SecondTask));
        void IJob.Execute(IJobExecutionContext context)
        {
            logger.InfoFormat("第二个自定义任务");
        }
    }
}
