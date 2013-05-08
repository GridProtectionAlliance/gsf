//******************************************************************************************************
//  Concentrator.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/16/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.IO;
using System.Linq;
using GSF.TimeSeries;

namespace ProtocolTester
{
    public class Concentrator : ConcentratorBase
    {
        private readonly StreamWriter m_exportFile;
        private readonly bool m_writeLogs;

        public Concentrator(bool writeLogs, string exportFileName)
        {
            m_writeLogs = writeLogs;

            if (m_writeLogs)
                m_exportFile = new StreamWriter(exportFileName);
        }

        public override void Stop()
        {
            base.Stop();

            if (m_exportFile != null)
                m_exportFile.Close();
        }

        protected override void PublishFrame(IFrame frame, int index)
        {
            if (m_writeLogs)
                m_exportFile.WriteLine(frame.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + string.Concat(frame.Measurements.Values.Select(measurement => "," + measurement.AdjustedValue.ToString())));
        }
    }
}
