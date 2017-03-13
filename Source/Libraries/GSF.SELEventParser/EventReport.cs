//******************************************************************************************************
//  EventReport.cs - Gbtc
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GSF.SELEventParser
{
    public class EventReport
    {
        #region [ Members ]

        // Fields
        private string m_command;
        private Header m_header;
        private Firmware m_firmware;
        private int m_eventNumber;
        private AnalogSection m_analogSection;
        private Dictionary<string, string> m_groupSettings = new Dictionary<string, string>();
        private Dictionary<string, string> m_controlEquations = new Dictionary<string, string>();
        private Dictionary<string, string> m_globalSettings = new Dictionary<string, string>();

        #endregion

        #region [ Properties ]

        public string Command
        {
            get
            {
                return m_command;
            }
            set
            {
                m_command = value;
            }
        }

        public Header Header
        {
            get
            {
                return m_header;
            }
            set
            {
                m_header = value;
            }
        }

        public Firmware Firmware
        {
            get
            {
                return m_firmware;
            }
            set
            {
                m_firmware = value;
            }
        }

        public int EventNumber
        {
            get
            {
                return m_eventNumber;
            }
            set
            {
                m_eventNumber = value;
            }
        }

        public AnalogSection AnalogSection
        {
            get
            {
                return m_analogSection;
            }
            set
            {
                m_analogSection = value;
            }
        }

        public void SetGroupSetting(string key, string value)
        {
            if (m_groupSettings.ContainsKey(key))
            {
                m_groupSettings[key] = value;
            }
            else
            {
                m_groupSettings.Add(key, value);
            }
        }

        public string GetGroupSettings(string key)
        {
            string result = null;

            if (m_groupSettings.ContainsKey(key))
            {
                result = m_groupSettings[key];
            }

            return result;
        }

        public Dictionary<string, string> GroupSetting
        {
            set
            {
                m_groupSettings = value;
            }
        }

        public void SetControlEquations(string key, string value)
        {
            if (m_controlEquations.ContainsKey(key))
            {
                m_controlEquations[key] = value;
            }
            else
            {
                m_controlEquations.Add(key, value);
            }
        }

        public string GetControlEquations(string key)
        {
            string result = null;

            if (m_controlEquations.ContainsKey(key))
            {
                result = m_controlEquations[key];
            }

            return result;
        }

        public Dictionary<string, string> ControlEquations
        {
            set
            {
                m_controlEquations = value;
            }
        }

        public void SetGlobalSetting(string key, string value)
        {
            if (m_globalSettings.ContainsKey(key))
            {
                m_globalSettings[key] = value;
            }
            else
            {
                m_globalSettings.Add(key, value);
            }
        }

        public string GetGlobalSettings(string key)
        {
            string result = null;

            if (m_globalSettings.ContainsKey(key))
            {
                result = m_globalSettings[key];
            }

            return result;
        }

        public Dictionary<string, string> GlobalSetting
        {
            set
            {
                m_globalSettings = value;
            }
        }



        #endregion

        #region [ Static ]

        // Static Methods

        public static EventReport Parse(string[] lines, ref int index)
        {
            EventReport eventReport = new EventReport();
            int firmwareIndex;

            // Parse the report header
            eventReport.Header = Header.Parse(lines, ref index);

            // Skip to the next nonblank line
            EventFile.SkipBlanks(lines, ref index);

            // Get the index of the line where
            // the firmware information is located
            firmwareIndex = index;

            // Parse the firmware and event number
            eventReport.Firmware = Firmware.Parse(lines, ref firmwareIndex);
            eventReport.EventNumber = ParseEventNumber(lines, ref index);
            index = Math.Max(firmwareIndex, index);

            // Skip to the next nonblank line
            EventFile.SkipBlanks(lines, ref index);

            // Parse the analog section of the report
            eventReport.AnalogSection = AnalogSection.Parse(eventReport.Header.EventTime, lines, ref index);

            if (lines.Length < index)
            {
                // skip digitals for now
                while (!lines[index].ToLower().Contains("group settings") && index < lines.Length) { ++index; }
                ++index;
                // Parse Group settings
                eventReport.GroupSetting = Settings.Parse(lines, ref index);
                EventFile.SkipBlanks(lines, ref index);

                // Parse Logic control equations
                ++index;
                EventFile.SkipBlanks(lines, ref index);
                eventReport.ControlEquations = ControlEquation.Parse(lines, ref index);
                EventFile.SkipBlanks(lines, ref index);

                // Parse Global Settings
                eventReport.GlobalSetting = Settings.Parse(lines, ref index);
            }
            return eventReport;
        }

        private static int ParseEventNumber(string[] lines, ref int index)
        {
            const string EventNumberRegex = @"Event Number = (\d+)";

            // Match the regular expression to get the event number
            Match eventNumberMatch = Regex.Match(lines[index], EventNumberRegex);

            if (eventNumberMatch.Success)
            {
                // Match was successful so
                // advance to the next line
                index++;

                // Get the event number
                return Convert.ToInt32(eventNumberMatch.Groups[1].Value);
            }

            // Unable to get event number
            // so default to zero
            return 0;
        }

        #endregion
    }
}
