//******************************************************************************************************
//  ScheduledTaskTest.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/09/2014 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Threading;
using GSF.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.Threading
{
    [TestClass]
    public class ScheduledTaskTest
    {
        int m_count;
        ScheduledTask m_task;

        [TestMethod]
        public void TestMethod1()
        {
            m_task = new ScheduledTask(ThreadingMode.DedicatedBackground,ThreadPriority.Highest);
            m_task.Running += task_Running;
            m_task.Start(10);
            Thread.Sleep(1000);
            m_task.Dispose();
            System.Console.WriteLine("Disposed");
        }

        void task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            if (e.Argument == ScheduledTaskRunningReason.Disposing)
                return;
            m_task.Start(10);
            m_count++;
            System.Console.WriteLine(m_count);
        }
    }
}
