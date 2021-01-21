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
using GSF.Units.EE;

namespace SELPDCImporter
{
    // By replacing this one class, a converter from any other PDC configuration can be created. The result
    // should always be a ConfigurationFrame that has all needed information to create an input configuration
    // in the host GPA synchrophasor application, e.g., openPDC.
    public static class SELPDCConfig
    {
        public const string DefaultLineFrequency = "60";
        public const ushort DefaultIDCode = 1;
        public const ushort DefaultFrameRate = 30;
        public const string DefaultPhasorType = "Voltage";
        public const char DefaultPhase = '+';

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
                  .Descendants("Value")
                  .FirstOrDefault()?.Value ?? DefaultLineFrequency;

            LineFrequency nominalFrequency = nominalFrequencySetting.Equals("50") ?
                LineFrequency.Hz50 :
                LineFrequency.Hz60;

            // Load all "Setting" elements from "ServerGateway" settings group
            XElement[] serverGatewaySettings = settingsGroups
                  .WhereAttribute("Type").Is("ServerGateway")
                  .Descendants("Setting").ToArray();

            // Load ID code setting
            string idCodeSetting = serverGatewaySettings
                .WhereAttribute("Name").Is("ID")
                .FirstOrDefault()?.Value ?? DefaultIDCode.ToString();

            if (!ushort.TryParse(idCodeSetting, out ushort idCode))
                idCode = DefaultIDCode;

            // Load frame rate setting
            string frameRateSetting = serverGatewaySettings
               .WhereAttribute("Name").Is("DataRate")
               .FirstOrDefault()?.Value ?? DefaultFrameRate.ToString();

            if (!ushort.TryParse(frameRateSetting, out ushort frameRate))
                frameRate = DefaultFrameRate;

            // With an ID code and frame rate, there's enough info to create a config frame
            ConfigurationFrame configFrame = new ConfigurationFrame(idCode, frameRate);

            // Load all output PMUs
            IEnumerable<XElement> pmuElements = serverGatewaySettings
                .WhereAttribute("Name").Is("OutputTags")
                .Descendants("PMU");

            foreach (XElement pmuElement in pmuElements)
            {
                string name = pmuElement.Attribute("Name")?.Value;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!ushort.TryParse(pmuElement.Attribute("ID")?.Value, out idCode))
                    continue;

                // With a name and an ID code, there's enough info to create a config cell representing the PMU
                ConfigurationCell pmu = new ConfigurationCell(configFrame, name, idCode)
                {
                    NominalFrequency = nominalFrequency,
                };

                // Load all PMU tags
                XElement[] tagElements = pmuElement.Descendants("Tag").ToArray();

                // Load all phasor tags
                IEnumerable<XElement> phasorElements = tagElements
                    .WhereAttribute("Type").Is("Phasor");

                foreach (XElement phasorElement in phasorElements)
                {
                    name = phasorElement.Attribute("Name")?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = phasorElement.Attribute("Description")?.Value;

                    string quantityType = phasorElement.Attribute("QuantityType")?.Value ?? DefaultPhasorType;

                    PhasorType phasorType = quantityType.Equals("Current") ?
                        PhasorType.Current :
                        PhasorType.Voltage;

                    string phaseSetting = phasorElement.Attribute("Phase")?.Value.Trim();

                    if (string.IsNullOrWhiteSpace(phaseSetting))
                        phaseSetting = GuessPhase(phasorElement.Attribute("OriginalName")?.Value ?? name);

                    char phase = string.IsNullOrWhiteSpace(phaseSetting) ? DefaultPhase : TranslatePhase(phaseSetting[0]);

                    if (string.IsNullOrWhiteSpace(description))
                        description = $"{pmu.StationName} phase {phase} {phasorType.ToString().ToLowerInvariant()} phasor";

                    pmu.PhasorDefinitions.Add(new PhasorDefinition(pmu, name, description, phasorType, phase));
                }

                // Load all analog tags
                IEnumerable<XElement> analogElements = tagElements
                    .WhereAttribute("Type").Is("Analog");

                foreach (XElement analogElement in analogElements)
                {
                    name = analogElement.Attribute("Name")?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = analogElement.Attribute("Description")?.Value;

                    if (string.IsNullOrWhiteSpace(description))
                        description = $"{pmu.StationName} {name} analog value";

                    pmu.AnalogDefinitions.Add(new AnalogDefinition(pmu, name, description));
                }

                // Load all digital tags
                IEnumerable<XElement> digitalElements = tagElements
                    .WhereAttribute("Type").Is("Digital");

                foreach (XElement digitalElement in digitalElements)
                {
                    name = digitalElement.Attribute("Name")?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string description = digitalElement.Attribute("Description")?.Value;

                    if (string.IsNullOrWhiteSpace(description))
                        description = $"{pmu.StationName} {name} digital value";

                    pmu.DigitalDefinitions.Add(new DigitalDefinition(pmu, name, description));
                }

                configFrame.Cells.Add(pmu);
            }

            return configFrame;
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
