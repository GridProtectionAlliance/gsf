//******************************************************************************************************
//  GaussianDistributionTest.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/07/2014 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Text;
using System.Windows.Forms;
using GSF.NumericalAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{
    [TestClass]
    public class GaussianDistributionTest
    {
        [TestMethod]
        public void GenerateValues()
        {
            GaussianDistribution r = new GaussianDistribution(0, 1, -1, 1);
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < 10000; x++)
            {
                sb.AppendLine(r.Next().ToString());
            }
            Clipboard.SetText(sb.ToString());
        }

    }
}
