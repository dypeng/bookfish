using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MGTasks;
using Quartz;

namespace UnitTest
{
    [TestClass]
    public class UTMGTasks
    {
        [TestMethod]
        public void UT_ConFirmOrder()
        {
            IJob job = new ConFirmOrder();
            job.Execute(null);
        }
    }
}
