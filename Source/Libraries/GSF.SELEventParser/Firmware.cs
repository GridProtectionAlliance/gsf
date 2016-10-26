//******************************************************************************************************
//  FirmwareID.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  11/19/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Text.RegularExpressions;

namespace GSF.SELEventParser
{
    public class Firmware
    {
        #region [ Members ]

        // Fields
        private string m_id;
        private int m_checksum;

        #endregion

        #region [ Properties ]

        public string ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        public int Checksum
        {
            get
            {
                return m_checksum;
            }
            set
            {
                m_checksum = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        public static Firmware Parse(string[] lines, ref int index)
        {
            const string FirmwareIDRegex = @"FID=(\S+)";
            const string ChecksumRegex = @"CID=(?:0x)?(\S+)";

            Firmware firmware = new Firmware();
            Match firmwareIDMatch;
            Match checksumMatch;

            // Firmware ID and checksum are on the same line --
            // match both regular expressions with the first line
            firmwareIDMatch = Regex.Match(lines[index], FirmwareIDRegex);
            checksumMatch = Regex.Match(lines[index], ChecksumRegex);

            // Get the firmware ID
            if (firmwareIDMatch.Success)
                firmware.ID = firmwareIDMatch.Groups[1].Value;

            // Get the firmware checksum
            if (checksumMatch.Success)
                firmware.Checksum = Convert.ToInt32(checksumMatch.Groups[1].Value, 16);

            // If either match was a success, advance to the next line
            if (firmwareIDMatch.Success || checksumMatch.Success)
                index++;

            return firmware;
        }

        #endregion
    }
}
