//******************************************************************************************************
//  APPPDCConfig.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/01/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GSF.Communication;
using GSF.PhasorProtocols;
using GSF.Units.EE;
using ShowMessageFunc = System.Func<string, string, System.Windows.Forms.MessageBoxButtons, System.Windows.Forms.MessageBoxIcon, System.Windows.Forms.DialogResult>;

namespace APPPDCImporter
{
    // By replacing this one class, a converter from any other PDC configuration can be created. The result
    // should always be a ConfigurationFrame that has all needed information to create an input configuration
    // in the host GPA synchrophasor application, e.g., openPDC.
    public static class APPPDCConfig
    {
        public const string IPAddressToken = "[[ipAddress]]";
        public const string DefaultLineFrequency = "60";
        public const string DefaultServerName = "APPPDC";
        public const ushort DefaultIDCode = 1;
        public const ushort DefaultFrameRate = 30;
        public const string DefaultPhasorType = "Voltage";
        public const char DefaultPhase = '+';
        public const string DefaultTransportProtocol = "TCP";
        public const ushort DefaultPort = 4712;
        public const ushort DefaultDataPort = 4713;

        public static ConfigurationFrame Parse(string configFile, ShowMessageFunc showMessage)
        {
            string[] rows = File.ReadAllLines(configFile);

            Dictionary<string, string> configSettings = rows
                .Select(row => row.Split('='))
                .ToDictionary(key => key[0], value => value[1], StringComparer.OrdinalIgnoreCase);

            // Fixed for now
            LineFrequency nominalFrequency = LineFrequency.Hz60;

            // Load server name setting
            string serverName = configSettings["IPAddr"];

            // Load ID code setting
            if (!ushort.TryParse(configSettings["IdCode"], out ushort idCode))
                idCode = DefaultIDCode;

            // Load frame rate setting
            if (!ushort.TryParse(configSettings["DataRate"], out ushort frameRate))
                frameRate = DefaultFrameRate;

            // With an ID code, frame rate and server name, there's enough info to create a config frame
            ConfigurationFrame configFrame = new(idCode, frameRate, serverName);

            // Load all output PMUs
            int index = -1;

            while (configSettings.TryGetValue($"PmuId{++index}", out string setting) && ushort.TryParse(setting, out idCode))
            {
                string name = configSettings[$"PmuStn{index}"];

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                // With a name and an ID code, there's enough info to create a config cell representing the PMU
                ConfigurationCell configCell = new(configFrame, name, idCode)
                {
                    NominalFrequency = nominalFrequency,
                };

                name = "VaChan";
                PhasorType phasorType = PhasorType.Voltage;
                char phase = 'A';
                string description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                name = "VbChan";
                phasorType = PhasorType.Voltage;
                phase = 'B';
                description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                name = "VcChan";
                phasorType = PhasorType.Voltage;
                phase = 'C';
                description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                name = "IaChan";
                phasorType = PhasorType.Current;
                phase = 'A';
                description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                name = "IbChan";
                phasorType = PhasorType.Current;
                phase = 'B';
                description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                name = "IcChan";
                phasorType = PhasorType.Current;
                phase = 'C';
                description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                name = "InChan";
                phasorType = PhasorType.Current;
                phase = 'N';
                description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));

                configFrame.Cells.Add(configCell);
            }


            // Get connection details configured for APP PDC
            configFrame.TransportProtocol = DefaultTransportProtocol;

            if (!ushort.TryParse(configSettings["PortTCP"], out ushort port))
                port = DefaultPort;

            //if (!ushort.TryParse(configSettings["PortUDP"], out ushort dataPort))
            //    dataPort = DefaultDataPort;

            // Setup MultiProtocolFrameParser style connection string
            Dictionary<string, string> getTcpSettings(bool commandChannel = false)
            {
                // Use of simple "protocol" key in commandChannel and separation of
                // server/port keys are required to create a proper "PmuConnection"
                // file that can be successfully parsed by the PMU Connection Tester
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [commandChannel ? "protocol" : nameof(TransportProtocol)] = nameof(TransportProtocol.Tcp),
                    ["server"] = IPAddressToken,
                    ["port"] = $"{port}",
                    ["isListener"] = "false",
                    ["interface"] = "0.0.0.0"
                };
            }

            Dictionary<string, string> settings = configFrame.Settings;

            settings[nameof(PhasorProtocol)] = nameof(PhasorProtocol.IEEEC37_118V1);

            foreach (KeyValuePair<string, string> pair in getTcpSettings())
                settings[pair.Key] = pair.Value;

            // Save device IP configuration
            Dictionary<string, string> deviceIPs = configFrame.DeviceIPs;

            deviceIPs[serverName] = serverName;
            configFrame.TargetDeviceIP = serverName;

            return configFrame;
        }

        private static string GetValue(this IEnumerable<XElement> target, string defaultValue)
        {
            string value = target
                .Descendants("Value")
                .FirstOrDefault()?.Value ?? $"{defaultValue}";

            if (string.IsNullOrWhiteSpace(value))
                value = defaultValue ?? "";

            return value.Trim();
        }

        private static T GetValue<T>(this IEnumerable<XElement> target, T defaultValue) where T : IConvertible
        {
            Type type = typeof(T);
            string result = target.GetValue($"{defaultValue}");

            if (type == typeof(ushort))
            {
                if (!ushort.TryParse(result, out ushort value))
                    value = defaultValue.ToUInt16(null);

                return (T)Convert.ChangeType(value, type);
            }
            
            if (type == typeof(bool))
            {
                bool.TryParse(result, out bool value);
                return (T)Convert.ChangeType(value, type);
            }

            return (T)Convert.ChangeType(result, type);
        }

        private static string GuessPhase(string name) => 
            string.IsNullOrWhiteSpace(name) || name.Length < 2 ? null : name[1].ToString();

        private static char TranslatePhase(char phase) =>
            phase switch
            {
                '1' => '+',
                '2' => '-',
                 _  => phase
            };
    }
}