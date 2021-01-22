//******************************************************************************************************
//  SELPDCConfig.cs - Gbtc
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
using System.Linq;
using System.Xml.Linq;
using GSF;
using GSF.Communication;
using GSF.FuzzyStrings;
using GSF.PhasorProtocols;
using GSF.Units.EE;

namespace SELPDCImporter
{
    // By replacing this one class, a converter from any other PDC configuration can be created. The result
    // should always be a ConfigurationFrame that has all needed information to create an input configuration
    // in the host GPA synchrophasor application, e.g., openPDC.
    public static class SELPDCConfig
    {
        public const string DefaultLineFrequency = "60";
        public const string DefaultServerName = "SELPDC";
        public const ushort DefaultIDCode = 1;
        public const ushort DefaultFrameRate = 30;
        public const string DefaultPhasorType = "Voltage";
        public const char DefaultPhase = '+';
        public const string DefaultTransportProtocol = "TCP";
        public const ushort DefaultCommandPort = 4713;
        public const ushort DefaultDataPort = 4950;
        public const ushort DefaultPort = 4712;
        public const bool DefaultMulticastEnabled = false;
        public const string DefaultMulticastGroup = "224.0.1.0";
        public const string DefaultGatewayIP = "192.168.1.1";

        private static IEnumerable<Tuple<XElement, string>> WhereAttribute(this IEnumerable<XElement> source, string attributeName) =>
            source.Select(element => new Tuple<XElement, string>(element, element.Attribute(attributeName)?.Value))
            .Where(tuple => !string.IsNullOrWhiteSpace(tuple.Item2));

        private static IEnumerable<XElement> Is(this IEnumerable<Tuple<XElement, string>> source, string attributeValue) =>
            source.Where(tuple => string.Equals(tuple.Item2, attributeValue, StringComparison.OrdinalIgnoreCase))
            .Select(tuple => tuple.Item1);

        public static ConfigurationFrame Parse(string configFile)
        {
            XDocument config = XDocument.Load(configFile);

            // Load all "SettingsGroup" elements
            XElement[] settingsGroups = config.Descendants("SettingsGroup").ToArray();

            // Load nominal frequency from "Globals" setting group
            string nominalFrequencySetting = settingsGroups
                .WhereAttribute("Type").Is("Globals")
                .Descendants("Setting")
                .WhereAttribute("Name").Is("Frequency")
                .GetValue(DefaultLineFrequency);

            LineFrequency nominalFrequency = nominalFrequencySetting.Equals("50") ?
                LineFrequency.Hz50 :
                LineFrequency.Hz60;

            // Load all "Setting" elements from "ServerGateway" settings group
            XElement[] serverGatewaySettings = settingsGroups
                .WhereAttribute("Type").Is("ServerGateway")
                .Descendants("Setting")
                .ToArray();

            // Load server name setting
            string serverName = serverGatewaySettings
                .WhereAttribute("Name").Is("ServerName")
                .GetValue(DefaultServerName);

            // Load ID code setting
            ushort idCode = serverGatewaySettings
                .WhereAttribute("Name").Is("ID")
                .GetValue(DefaultIDCode);

            // Load frame rate setting
            ushort frameRate = serverGatewaySettings
               .WhereAttribute("Name").Is("DataRate")
               .GetValue(DefaultFrameRate);

            // With an ID code, frame rate and server name, there's enough info to create a config frame
            ConfigurationFrame configFrame = new ConfigurationFrame(idCode, frameRate, serverName);

            // Load all output PMUs
            IEnumerable<XElement> pmus = serverGatewaySettings
                .WhereAttribute("Name").Is("OutputTags")
                .Descendants("PMU");

            foreach (XElement pmu in pmus)
            {
                string name = pmu.Attribute("Name")?.Value;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!ushort.TryParse(pmu.Attribute("ID")?.Value, out idCode))
                    continue;

                // With a name and an ID code, there's enough info to create a config cell representing the PMU
                ConfigurationCell configCell = new ConfigurationCell(configFrame, name, idCode)
                {
                    NominalFrequency = nominalFrequency,
                };

                // Load all PMU tags
                XElement[] tags = pmu.Descendants("Tag").ToArray();

                // Load all phasor tags
                IEnumerable<XElement> phasors = tags
                    .WhereAttribute("Type").Is("Phasor");

                foreach (XElement phasor in phasors)
                {
                    name = phasor.Attribute("Name")?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = phasor.Attribute("Description")?.Value;

                    string quantityType = phasor.Attribute("QuantityType")?.Value ?? DefaultPhasorType;

                    PhasorType phasorType = quantityType.Equals("Current") ?
                        PhasorType.Current :
                        PhasorType.Voltage;

                    string phaseSetting = phasor.Attribute("Phase")?.Value.Trim();

                    if (string.IsNullOrWhiteSpace(phaseSetting))
                        phaseSetting = GuessPhase(phasor.Attribute("OriginalName")?.Value ?? name);

                    char phase = string.IsNullOrWhiteSpace(phaseSetting) ? DefaultPhase : TranslatePhase(phaseSetting[0]);

                    if (string.IsNullOrWhiteSpace(description))
                        description = $"{configCell.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";

                    configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, name, description, phasorType, phase));
                }

                // Load all analog tags
                IEnumerable<XElement> analogs = tags
                    .WhereAttribute("Type").Is("Analog");

                foreach (XElement analog in analogs)
                {
                    name = analog.Attribute("Name")?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = analog.Attribute("Description")?.Value;

                    if (string.IsNullOrWhiteSpace(description))
                        description = $"{configCell.StationName} {name} analog value";

                    configCell.AnalogDefinitions.Add(new AnalogDefinition(configCell, name, description));
                }

                // Load all digital tags
                IEnumerable<XElement> digitals = tags
                    .WhereAttribute("Type").Is("Digital");

                foreach (XElement digital in digitals)
                {
                    name = digital.Attribute("Name")?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = digital.Attribute("Description")?.Value;

                    if (string.IsNullOrWhiteSpace(description))
                        description = $"{configCell.StationName} {name} digital value";

                    configCell.DigitalDefinitions.Add(new DigitalDefinition(configCell, name, description));
                }

                configFrame.Cells.Add(configCell);
            }

            // Get connection details configured for SEL PDC
            string transportProtocol = serverGatewaySettings
                .WhereAttribute("Name").Is("TransportProtocol")
                .GetValue(DefaultTransportProtocol)
                .ToUpperInvariant();

            ushort commandPort = serverGatewaySettings
               .WhereAttribute("Name").Is("CommandPort")
               .GetValue(DefaultCommandPort);

            ushort dataPort = serverGatewaySettings
               .WhereAttribute("Name").Is("DataPort")
               .GetValue(DefaultDataPort);

            ushort port = serverGatewaySettings
               .WhereAttribute("Name").Is("Port")
               .GetValue(DefaultPort);

            bool multicastEnabled = serverGatewaySettings
               .WhereAttribute("Name").Is("MulticastEnabled")
               .GetValue(DefaultMulticastEnabled);

            string multicastGroup = serverGatewaySettings
                .WhereAttribute("Name").Is("MulticastGroup")
                .GetValue(DefaultMulticastGroup);

            // Setup MultiProtocolFrameParser style connection string
            Dictionary<string, string> getTcpSettings()
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(TransportProtocol)] = nameof(TransportProtocol.Tcp),
                    ["server"] = $"{{0}}:{port}", // Parameter {0} for replacement with actual IP later
                    ["isListener"] = "false",
                    ["interface"] = "0.0.0.0"
                };
            }

            Dictionary<string, string> settings = configFrame.Settings;

            settings[nameof(PhasorProtocol)] = nameof(PhasorProtocol.IEEEC37_118V1);

            if (transportProtocol == "TCP")
            {
                foreach (KeyValuePair<string, string> pair in getTcpSettings())
                    settings[pair.Key] = pair.Value;
            }
            else if (transportProtocol.StartsWith("UDP"))
            {
                settings[nameof(TransportProtocol)] = nameof(TransportProtocol.Udp);
                settings["localPort"] = $"{dataPort}";
                settings["interface"] = "0.0.0.0";

                switch (transportProtocol)
                {
                    case "UDP_T":
                    case "UDP_U":
                        settings["commandChannel"] = getTcpSettings().JoinKeyValuePairs();
                        break;
                }

                if (multicastEnabled)
                {
                    settings["server"] = multicastGroup;
                    settings["remotePort"] = $"{commandPort}";
                }
            }
            else
            {
                settings["error"] = "No valid connection protocol detected";
            }

            // Save device IP configuration
            Dictionary<string, string> deviceIPs = configFrame.DeviceIPs;

            // Load "NetworkSettingsGroup" element from settings group
            XElement networkSettingsGroup = settingsGroups
                .WhereAttribute("Type").Is("NetworkSettingsGroup")
                .FirstOrDefault();

            if (networkSettingsGroup is null)
            {
                deviceIPs["error"] = "No valid devices IPs detected";
            }
            else
            {
                string gatewayIP = networkSettingsGroup
                    .Descendants("Setting")
                    .WhereAttribute("Name").Is("Gateway")
                    .GetValue(DefaultGatewayIP);

                // Get array of settings for each nic interface
                IEnumerable<XElement[]> interfaceSettingsMap = networkSettingsGroup
                    .Descendants("SettingsGroup")
                    .WhereAttribute("Type").Is("NetworkGroup")
                    .Descendants("Instance")
                    .WhereAttribute("Type").Is("NetworkInterfaceCard")
                    .Select(element => element.Descendants("Setting").ToArray());

                foreach (XElement[] interfaceSettings in interfaceSettingsMap)
                {
                    string name = interfaceSettings
                        .WhereAttribute("Name").Is("Name")
                        .GetValue(null);

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string ipAddress = interfaceSettings
                        .WhereAttribute("Name").Is("IPAddress")
                        .GetValue(null);

                    if (string.IsNullOrWhiteSpace(ipAddress))
                        continue;

                    deviceIPs[name] = ipAddress;
                }

                // Set the target device IP to the one that matches gateway IP the closest,
                // this is just a guess, user must select proper device IP
                configFrame.TargetDeviceIP = deviceIPs
                    .ToDictionary(pair => pair.Key, pair => gatewayIP.OverlapCoefficient(pair.Value))
                    .Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            }

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