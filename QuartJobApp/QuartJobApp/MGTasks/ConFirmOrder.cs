using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Quartz;
using System.Data.SqlClient;
using System.Data;

namespace MGTasks
{
    public class ConFirmOrder : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ConFirmOrder));
        void IJob.Execute(IJobExecutionContext context)
        {
            logger.InfoFormat("第一个自定义任务");
            logger.InfoFormat(DBUntity.SqlHelper.connectionString);
           
        }
    }
}
