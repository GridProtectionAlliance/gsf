//******************************************************************************************************
//  PhasorDefinition3.cs - Gbtc
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
//  12/27/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.Units.EE;

// ReSharper disable SuggestBaseTypeForParameter
namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 configuration frame 3 implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable]
    public sealed class PhasorDefinition3 : ChannelDefinitionBase3, IPhasorDefinition
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 12;
        private const byte PhasorTypeIndicator = (byte)PhasorTypeIndication.Type;

        // Fields
        private IPhasorDefinition m_voltageReference;
        private byte m_phasorTypeIndication;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinition3"/>.</param>
        public PhasorDefinition3(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell3"/> parent of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="offset">The offset of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinition3"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        /// <param name="phase">The phase of this <see cref="PhasorDefinition3"/>.</param>
        public PhasorDefinition3(ConfigurationCell3 parent, string label, uint scale, double offset, PhasorType type, PhasorDefinition3 voltageReference, char phase)
            : base(parent, label, scale, offset)
        {
            PhasorType = type;
            m_voltageReference = type == PhasorType.Voltage ? this : voltageReference;

            switch (phase)
            {
                case 'A':
                    PhasorComponent = PhasorComponent.PhaseA;
                    break;
                case 'B':
                    PhasorComponent = PhasorComponent.PhaseB;
                    break;
                case 'C':
                    PhasorComponent = PhasorComponent.PhaseC;
                    break;
                case '+':
                    PhasorComponent = PhasorComponent.PositiveSequence;
                    break;
                case '-':
                    PhasorComponent = PhasorComponent.NegativeSequence;
                    break;
                case '0':
                    PhasorComponent = PhasorComponent.ZeroSequence;
                    break;
                default:
                    // If phase is not a value IEEE C37.118-2011 supports, default to positive sequence 
                    PhasorComponent = PhasorComponent.PositiveSequence;
                    break;
            }
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        private PhasorDefinition3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize phasor definition
            m_phasorTypeIndication = info.GetByte("phasorTypeIndication");
            m_voltageReference = (IPhasorDefinition)info.GetValue("voltageReference", typeof(IPhasorDefinition));
            PhasorDataModifications = (PhasorDataModifications)info.GetValue("phasorDataModifications", typeof(PhasorDataModifications));
            UserFlags = info.GetByte("userFlags");
            MagnitudeMultiplier = info.GetSingle("magnitudeMultiplier");
            AngleAdder = info.GetSingle("angleAdder");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell3"/> parent of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public new ConfigurationCell3 Parent
        {
            get => base.Parent as ConfigurationCell3;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets the <see cref="DataFormat"/> of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public override DataFormat DataFormat => Parent.PhasorDataFormat;

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public CoordinateFormat CoordinateFormat => Parent.PhasorCoordinateFormat;

        /// <summary>
        /// Gets or sets the <see cref="AngleFormat"/> of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public AngleFormat AngleFormat => Parent.PhasorAngleFormat;

        /// <summary>
        /// Gets or sets the <see cref="PhasorType"/> of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public PhasorType PhasorType
        { 
            get => (m_phasorTypeIndication & PhasorTypeIndicator) == 0 ? PhasorType.Voltage : PhasorType.Current; 
            set
            {
                if (value == PhasorType.Voltage)
                    m_phasorTypeIndication = (byte)(m_phasorTypeIndication & ~PhasorTypeIndicator);
                else
                    m_phasorTypeIndication |= PhasorTypeIndicator;
            }
        }

        /// <summary>
        /// Gets or sets the associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).
        /// </summary>
        /// <remarks>
        /// This only applies to current phasors.
        /// </remarks>
        public IPhasorDefinition VoltageReference
        {
            get => m_voltageReference;
            set => m_voltageReference = PhasorType == PhasorType.Voltage ? this : value;
        }

        /// <summary>
        /// Gets or sets <see cref="IEEEC37_118.PhasorDataModifications"/> of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public PhasorDataModifications PhasorDataModifications { get; set; } = PhasorDataModifications.NoModifications;

        /// <summary>
        /// Gets or sets <see cref="IEEEC37_118.PhasorComponent"/> of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public PhasorComponent PhasorComponent
        {
            get => (PhasorComponent)(m_phasorTypeIndication & (byte)PhasorTypeIndication.ComponentMask);
            set => m_phasorTypeIndication = (byte)((m_phasorTypeIndication & ~(byte)PhasorTypeIndication.ComponentMask) | (byte)value);
        }

        /// <summary>
        /// Gets or sets user defined flags of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        public byte UserFlags { get; set; }

        /// <summary>
        /// Gets or sets any multiplier to be applied to the phasor magnitudes.
        /// </summary>
        public float MagnitudeMultiplier { get; set; } = 1.0F;

        /// <summary>
        /// Gets or sets and adder to be applied to the phasor angle, in radians.
        /// </summary>
        public float AngleAdder { get; set; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="PhasorDefinition3"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Phasor Type", $"{(int)PhasorType}: {PhasorType}");
                baseAttributes.Add("Phasor Data Modifications", $"{(int)PhasorDataModifications}: {PhasorDataModifications}");
                baseAttributes.Add("Phasor Component", $"{(int)PhasorComponent}: {PhasorComponent}");

                string voltageLevel = Enum.TryParse($"{UserFlags}", out VoltageLevel level) ? $"{level.Value()}kV" : "kV level undefined";
                baseAttributes.Add("User Flags", $"0x{UserFlags:X}: {voltageLevel}");
                
                baseAttributes.Add("Magnitude Multiplier", $"{MagnitudeMultiplier}");
                baseAttributes.Add("Angle Adder", $"{AngleAdder}");

                return baseAttributes;
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];

                // First word
                BigEndian.CopyBytes((ushort)PhasorDataModifications, buffer, 0);
                buffer[2] = m_phasorTypeIndication;
                buffer[3] = UserFlags;

                // Second word
                BigEndian.CopyBytes(MagnitudeMultiplier, buffer, 4);

                // Third word
                BigEndian.CopyBytes(AngleAdder, buffer, 8);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the string representation of this <see cref="PhasorDefinition3"/>.
        /// </summary>
        /// <returns>String representation of this <see cref="PhasorDefinition3"/>.</returns>
        public override string ToString() => 
            (PhasorType == PhasorType.Current ? "I: " : "V: ") + Label;

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize phasor definition
            info.AddValue("phasorTypeIndication", m_phasorTypeIndication);
            info.AddValue("voltageReference", m_voltageReference, typeof(IPhasorDefinition));
            info.AddValue("phasorDataModifications", PhasorDataModifications, typeof(PhasorDataModifications));
            info.AddValue("userFlags", UserFlags);
            info.AddValue("magnitudeMultiplier", MagnitudeMultiplier);
            info.AddValue("angleAdder", AngleAdder);
        }

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] buffer, int startIndex)
        {
            // First word
            PhasorDataModifications = (PhasorDataModifications)BigEndian.ToUInt16(buffer, startIndex);
            m_phasorTypeIndication = buffer[startIndex + 2];
            UserFlags = buffer[startIndex + 3];

            // Second word
            MagnitudeMultiplier = BigEndian.ToSingle(buffer, startIndex + 4);

            // Third word:
            AngleAdder = BigEndian.ToSingle(buffer, startIndex + 8);

            return ConversionFactorLength;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 phasor definition
        internal static IPhasorDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IPhasorDefinition phasorDefinition = new PhasorDefinition3(parent);

            parsedLength = phasorDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return phasorDefinition;
        }

        #endregion
    }
}