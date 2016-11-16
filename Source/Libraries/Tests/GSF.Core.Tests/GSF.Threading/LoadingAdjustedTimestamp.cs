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
using GSF.Diagnostics;
using GSF.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.GSF.Threading
{
    [TestClass]
    public class LoadingAdjustedTimestampTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Logger.Console.Verbose = VerboseLevel.Ultra;
            System.Console.WriteLine(LoadingAdjustedTimestamp.CurrentTime);
            Thread.Sleep(1000);
            System.Console.WriteLine(LoadingAdjustedTimestamp.CurrentTime);

        }

    }
}
