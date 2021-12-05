//******************************************************************************************************
//  DigitalDefinition3.cs - Gbtc
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 configuration frame 3 implementation of a <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable]
    public sealed class DigitalDefinition3 : ChannelDefinitionBase3, IDigitalDefinition
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        private const int BitLabelCount = 16;

        // Fields
        private byte[] m_labelImage;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition3"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinition3"/>.</param>
        public DigitalDefinition3(IConfigurationCell parent)
            : base(parent)
        {
            ScalingValue = 1;
            Offset = 0.0D;
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell3"/> parent of this <see cref="DigitalDefinition3"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinition3"/>.</param>
        /// <param name="normalStatus">The normal status for this <see cref="DigitalDefinition3"/>.</param>
        /// <param name="validInputs">The valid input for this <see cref="DigitalDefinition3"/>.</param>
        public DigitalDefinition3(ConfigurationCell3 parent, string label, ushort normalStatus, ushort validInputs)
            : base(parent, label, 1, 0.0D)
        {
            NormalStatus = normalStatus;
            ValidInputs = validInputs;
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        private DigitalDefinition3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize digital definition
            NormalStatus = info.GetUInt16("normalStatus");
            ValidInputs = info.GetUInt16("validInputs");
            Label = info.GetString("digitalLabels");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell3"/> parent of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public new ConfigurationCell3 Parent
        {
            get => base.Parent as ConfigurationCell3;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets normal status for this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public ushort NormalStatus { get; set; }

        /// <summary>
        /// Gets or sets valid input for this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public ushort ValidInputs { get; set; }

        /// <summary>
        /// Gets the maximum length of the <see cref="Label"/> of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public override int MaximumLabelLength => LabelCount * byte.MaxValue;

        /// <summary>
        /// Gets the number of labels defined in this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public int LabelCount => BitLabelCount;

        /// <summary>
        /// Gets array of 16-digital labels.
        /// </summary>
        public string[] Labels { get; } = new string[BitLabelCount];

        /// <summary>
        /// Gets or sets the combined set of label images of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Label
        {
            // We hide this from the editor just because this is a large combined string of all digital labels,
            // and it will make more sense for consumers to use the "Labels" property
            get => string.Join("|", Labels.Select(label => label.RemoveCharacter('|').GetValidLabel()));
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "undefined";

                if (value.Trim().Length > MaximumLabelLength)
                    throw new OverflowException($"Label length cannot exceed {MaximumLabelLength}");

                string[] labels = value.Split('|');

                for (int i = 0; i < BitLabelCount; i++)
                    Labels[i] = i < labels.Length ? labels[i].Trim() : "";

                using (MemoryStream stream = new())
                {
                    for (int i = 0; i < BitLabelCount; i++)
                    {
                        byte[] buffer = ConfigurationCell3.EncodeLengthPrefixedString(Labels[i]);
                        stream.Write(buffer, 0, buffer.Length);
                    }

                    m_labelImage = stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the binary image of the <see cref="Label"/> of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public override byte[] LabelImage => m_labelImage;

        /// <summary>
        /// Gets the length of the <see cref="DigitalDefinition3"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                // Force a refresh of label image before getting BodyImage
                Label = Label;
                return LabelImage.Length; 
            }
        }

        /// <summary>
        /// Gets the <see cref="IEEEC37_118.DraftRevision"/> of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public DraftRevision DraftRevision => DraftRevision.Std2011;

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Data format for digital values will always be <see cref="DataFormat.FixedInteger"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DataFormat DataFormat => DataFormat.FixedInteger;

        /// <summary>
        /// Gets or sets the offset of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Offset for digital values will always be 0; assigning a value other than 0 will thrown an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">Digital values represent bit flags and thus do not support an offset.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override double Offset
        {
            get => base.Offset;
            set
            {
                if (value == 0)
                    base.Offset = value;
                else
                    throw new NotImplementedException("Digital values represent bit flags and thus do not support an offset");
            }
        }

        /// <summary>
        /// Gets or sets the integer scaling value of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Scaling value for digital values will always be 1; assigning a value other than 1 will thrown an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">Digital values represent bit flags and thus are not scaled.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override uint ScalingValue
        {
            get => base.ScalingValue;
            set
            {
                if (value == 1)
                    base.ScalingValue = value;
                else
                    throw new NotImplementedException("Digital values represent bit flags and thus are not scaled");
            }
        }

        /// <summary>
        /// Gets the scale/bit for the <see cref="ScalingValue"/> of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Scale/bit for digital values will always be 1.0.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override double ScalePerBit => 1.0D;

        /// <summary>
        /// Gets conversion factor image of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];

                BigEndian.CopyBytes(NormalStatus, buffer, 0);
                BigEndian.CopyBytes(ValidInputs, buffer, 2);

                return buffer;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="DigitalDefinition3"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                byte[] normalStatusBytes = BitConverter.GetBytes(NormalStatus);
                byte[] validInputsBytes = BitConverter.GetBytes(ValidInputs);

                baseAttributes.Add("Normal Status", NormalStatus.ToString());
                baseAttributes.Add("Normal Status (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(normalStatusBytes));
                baseAttributes.Add("Normal Status (Hexadecimal)", $"0x{ByteEncoding.Hexadecimal.GetString(normalStatusBytes)}");

                baseAttributes.Add("Valid Inputs", ValidInputs.ToString());
                baseAttributes.Add("Valid Inputs (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(validInputsBytes));
                baseAttributes.Add("Valid Inputs (Hexadecimal)", $"0x{ByteEncoding.Hexadecimal.GetString(validInputsBytes)}");

                baseAttributes.Add("Bit Label Count", LabelCount.ToString());

                for (int x = 0; x < LabelCount; x++)
                    baseAttributes.Add($"     Bit {x} Label", Labels[x]);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            int index = startIndex;

            for (int i = 0; i < BitLabelCount; i++)
                Labels[i] = ConfigurationCell3.DecodeLengthPrefixedString(buffer, ref index);

            return index - startIndex;
        }

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] buffer, int startIndex)
        {
            NormalStatus = BigEndian.ToUInt16(buffer, startIndex);
            ValidInputs = BigEndian.ToUInt16(buffer, startIndex + 2);

            return ConversionFactorLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize digital definition
            info.AddValue("normalStatus", NormalStatus);
            info.AddValue("validInputs", ValidInputs);
            info.AddValue("digitalLabels", Label);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 digital definition
        internal static IDigitalDefinition CreateNewDefinition(IConfigurationCell parent, byte[] buffer, int startIndex, out int parsedLength)
        {
            IDigitalDefinition digitalDefinition = new DigitalDefinition3(parent);

            parsedLength = digitalDefinition.ParseBinaryImage(buffer, startIndex, 0);

            return digitalDefinition;
        }

        #endregion
    }
}