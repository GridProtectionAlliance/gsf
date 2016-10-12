//******************************************************************************************************
//  Header.cs - Gbtc
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
//  11/05/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Text.RegularExpressions;

namespace GSF.SELEventParser
{
    public class Header
    {
        #region [ Members ]

        // Fields
        private string m_relayID;
        private DateTime m_eventTime;
        private string m_stationID;
        private int m_serialNumber;

        #endregion

        #region [ Properties ]

        public string RelayID
        {
            get
            {
                return m_relayID;
            }
            set
            {
                m_relayID = value;
            }
        }

        public string StationID
        {
            get
            {
                return m_stationID;
            }
            set
            {
                m_stationID = value;
            }
        }

        public DateTime EventTime
        {
            get
            {
                return m_eventTime;
            }
            set
            {
                m_eventTime = value;
            }
        }

        public int SerialNumber
        {
            get
            {
                return m_serialNumber;
            }
            set
            {
                m_serialNumber = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        public static Header Parse(string[] lines, ref int index)
        {
            const string HeaderLine1 = @"(\S.*)\s+Date:\s+(\S+)\s+Time:\s+(\S+)";
            const string HeaderLine2 = @"(\S.*)(?:\s+Serial Number: (\d+))?";

            Header header = new Header();

            Match regexMatch;
            DateTime eventTime;
            string eventTimeString;
            int serialNumber;

            if (index < lines.Length)
            {
                // Apply regex match to get information contained on first line of header
                regexMatch = Regex.Match(lines[index], HeaderLine1);

                if (regexMatch.Success)
                {
                    // Get relay ID from line 1
                    header.RelayID = regexMatch.Groups[1].Value.Trim();

                    // Build date/time string for parsing
                    eventTimeString = string.Format("{0} {1}", regexMatch.Groups[2].Value, regexMatch.Groups[3].Value);

                    // Get event time from line 1
                    if (EventFile.TryParseDateTime(eventTimeString, out eventTime))
                        header.EventTime = eventTime;

                    // Advance to the next line
                    index++;

                    if (index < lines.Length)
                    {
                        // Apply regex match to get information contained on second line of header
                        regexMatch = Regex.Match(lines[index], HeaderLine2);

                        if (regexMatch.Success)
                        {
                            // Get station ID and serial number from line 2
                            header.StationID = regexMatch.Groups[1].Value.Trim();

                            if (int.TryParse(regexMatch.Groups[2].Value, out serialNumber))
                                header.SerialNumber = serialNumber;

                            // Advance to the next line
                            index++;
                        }
                    }
                }
            }

            return header;
        }

        #endregion
    }
}
