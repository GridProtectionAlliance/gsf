//******************************************************************************************************
//  ConfigurationFrame.cs - Gbtc
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
//  01/02/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using GSF;
using GSF.PhasorProtocols;
using GSF.Units.EE;

namespace SELPDCImporter
{
    public sealed class FrequencyDefinition : FrequencyDefinitionBase
    {
        public FrequencyDefinition(ConfigurationCell parent)
            : base(parent, $"{parent.StationName} Freq", 1000, 100, 0.0D)
        {
        }

        public new ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        public override int MaximumLabelLength => int.MaxValue;
    }

    public sealed class PhasorDefinition : PhasorDefinitionBase
    {
        public PhasorDefinition(ConfigurationCell parent, string label, string description, PhasorType type, char phase, PhasorDefinition voltageReference = null)
            : base(parent, label, type == PhasorType.Voltage ? 2725785U : 2423U, 0.0D, type, voltageReference)
        {
            Description = description;
            Phase = phase;
        }

        public new ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        public string Description { get; }

        public char Phase { get; }

        public override int MaximumLabelLength => int.MaxValue;
    }

    public sealed class AnalogDefinition : AnalogDefinitionBase
    {
        public AnalogDefinition(ConfigurationCell parent, string label, string description, AnalogType type = AnalogType.SinglePointOnWave)
            : base(parent, label, 1373291U, 0.0, type)
        {
            Description = description;
        }

        public new ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        public string Description { get; }
    }

    public sealed class DigitalDefinition : DigitalDefinitionBase
    {
        public DigitalDefinition(ConfigurationCell parent, string label, string description)
            : base(parent, label)
        {
            Description = description;
        }

        public new ConfigurationCell Parent
        {
            get => base.Parent as ConfigurationCell;
            set => base.Parent = value;
        }

        public string Description { get; }

        public override int MaximumLabelLength => int.MaxValue;
    }

    public sealed class ConfigurationCell : ConfigurationCellBase
    {
        public ConfigurationCell(ConfigurationFrame parent, string name, ushort idCode)
            : base(parent, idCode, int.MaxValue, int.MaxValue, int.MaxValue)
        {
            StationName = name;
            IDLabel = name.GetCleanAcronym();
            FrequencyDefinition = new FrequencyDefinition(this);
        }

        public override DataFormat AnalogDataFormat { get; set; } = DataFormat.FloatingPoint;

        public override DataFormat FrequencyDataFormat { get; set; } = DataFormat.FloatingPoint;

        public override DataFormat PhasorDataFormat { get; set; } = DataFormat.FloatingPoint;

        public override CoordinateFormat PhasorCoordinateFormat { get; set; } = CoordinateFormat.Polar;
    }

    public sealed class ConfigurationCellCollection : GSF.PhasorProtocols.ConfigurationCellCollection
    {
        public ConfigurationCellCollection()
            : base(int.MaxValue, false)
        {
        }

        public new ConfigurationCell this[int index]
        {
            get => base[index] as ConfigurationCell;
            set => base[index] = value;
        }
    }

    public sealed class ConfigurationFrame : ConfigurationFrameBase
    {
        public ConfigurationFrame(ushort idCode, ushort frameRate, string name)
            : base(idCode, new ConfigurationCellCollection(), DateTime.UtcNow.Ticks, frameRate)
        {
            Name = name;
            Acronym = name.GetCleanAcronym();
        }

        public Dictionary<string, string> Settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string ConnectionString => Settings.JoinKeyValuePairs();

        public Dictionary<string, string> DeviceIPs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string TargetDeviceIP;

        public string TransportProtocol;

        public string Name { get; }

        public string Acronym { get; }

        public new ConfigurationCellCollection Cells => base.Cells as ConfigurationCellCollection;

        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length) => 0;

        public string GeneratePDCDetails()
        {
            StringBuilder pdcDetails = new StringBuilder();

            pdcDetails.AppendLine($"Acronym: {Acronym}");
            pdcDetails.AppendLine($"Name: {Name}");
            pdcDetails.AppendLine($"ID Code: {IDCode:N0}");
            pdcDetails.AppendLine($"Data Rate: {FrameRate:N0} frames / second");
            pdcDetails.AppendLine($"IP Addresses: {string.Join(", ", DeviceIPs.Values)}");
            pdcDetails.AppendLine($"Transport Protocol: {TransportProtocol}");
            pdcDetails.AppendLine($"PMU Count: {Cells.Count:N0}");

            for (int i = 0; i < Cells.Count; i++)
            {
                ConfigurationCell configCell = Cells[i];

                pdcDetails.AppendLine();
                pdcDetails.AppendLine($"PMU {i + 1:N0} Details:");
                pdcDetails.AppendLine($"   Acronym: {configCell.IDLabel}");
                pdcDetails.AppendLine($"   Name: {configCell.StationName}");
                pdcDetails.AppendLine($"   ID Code: {configCell.IDCode:N0}");
                pdcDetails.AppendLine($"   Nominal Frequency: {(int)configCell.NominalFrequency}Hz");
                pdcDetails.AppendLine($"   Phasor Count: {configCell.PhasorDefinitions.Count:N0}");
                pdcDetails.AppendLine($"   Analog Count: {configCell.AnalogDefinitions.Count:N0}");
                pdcDetails.AppendLine($"   Digital Count: {configCell.DigitalDefinitions.Count:N0}");

                for (int j = 0; j < configCell.PhasorDefinitions.Count; j++)
                {
                    if (configCell.PhasorDefinitions[j] is not PhasorDefinition phasor)
                        continue;

                    pdcDetails.AppendLine();
                    pdcDetails.AppendLine($"   Phasor {j + 1:N0} Details:");
                    pdcDetails.AppendLine($"      Name: {phasor.Label}");
                    pdcDetails.AppendLine($"      Type: {phasor.PhasorType}");
                    pdcDetails.AppendLine($"      Phase: {phasor.Phase}");
                    pdcDetails.AppendLine($"      Description: {phasor.Description}");
                }

                for (int j = 0; j < configCell.AnalogDefinitions.Count; j++)
                {
                    if (configCell.AnalogDefinitions[j] is not AnalogDefinition analog)
                        continue;

                    pdcDetails.AppendLine();
                    pdcDetails.AppendLine($"   Analog {j + 1:N0} Details:");
                    pdcDetails.AppendLine($"      Name: {analog.Label}");
                    pdcDetails.AppendLine($"      Type: {analog.AnalogType}");
                    pdcDetails.AppendLine($"      Description: {analog.Description}");
                }

                for (int j = 0; j < configCell.DigitalDefinitions.Count; j++)
                {
                    if (configCell.DigitalDefinitions[j] is not DigitalDefinition digital)
                        continue;

                    pdcDetails.AppendLine();
                    pdcDetails.AppendLine($"   Digital {j + 1:N0} Details:");
                    pdcDetails.AppendLine($"      Name: {digital.Label}");
                    pdcDetails.AppendLine($"      Description: {digital.Description}");
                }
            }

            return pdcDetails.ToString();
        }
    }
}