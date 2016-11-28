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

using System;
using GSF.Diagnostics;
using GSF.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.GSF.Threading
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            ShutdownHandler.Initialize();

            Logger.Console.Verbose = VerboseLevel.Ultra;
            LogPublisher pub2;

            var pub1 = Logger.CreatePublisher(typeof(LoggerTest), MessageClass.Application);
            using (Logger.AppendStackMessages("Test2", "Adapter2"))
                pub2 = Logger.CreatePublisher(typeof(int), MessageClass.Application);

            Error1(pub2);
            using (Logger.SuppressFirstChanceExceptionLogMessages())
            {
                Error1(pub2);
            }
            using (Logger.SuppressLogMessages())
            {
                Error1(pub1);
            }

            using (Logger.SuppressLogMessages())
            using (Logger.AppendStackMessages("Test1", "Adapter1"))
            using (Logger.OverrideSuppressLogMessages())
            {
                Error1(pub2);
            }

            ShutdownHandler.InitiateSafeShutdown();
        }

        private static void Error1(LogPublisher pub2)
        {
            try
            {
                int value = int.Parse("dk20");
            }
            catch (Exception ex)
            {
                pub2.Publish(MessageLevel.Critical, "Failed Cast", null, null, ex);
            }
        }
    }
}
