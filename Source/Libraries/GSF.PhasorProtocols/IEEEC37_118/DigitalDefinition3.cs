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
using System.Runtime.Serialization;
using System.Text;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 configuration frame 3 implementation of a <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable]
    public class DigitalDefinition3 : ChannelDefinitionBase3, IDigitalDefinition
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        // Fields
        private ushort m_normalStatus;
        private ushort m_validInputs;
        private string m_label;

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
            m_normalStatus = normalStatus;
            m_validInputs = validInputs;
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalDefinition3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize digital definition
            m_normalStatus = info.GetUInt16("normalStatus");
            m_validInputs = info.GetUInt16("validInputs");
            m_label = info.GetString("digitalLabels");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell3"/> parent of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public new virtual ConfigurationCell3 Parent
        {
            get => base.Parent as ConfigurationCell3;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets normal status for this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public ushort NormalStatus
        {
            get => m_normalStatus;
            set => m_normalStatus = value;
        }

        /// <summary>
        /// Gets or sets valid input for this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public ushort ValidInputs
        {
            get => m_validInputs;
            set => m_validInputs = value;
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="Label"/> of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public override int MaximumLabelLength => LabelCount * 255;

        /// <summary>
        /// Gets the number of labels defined in this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public int LabelCount => 16;

        /// <summary>
        /// Gets or sets the combined set of label images of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Label
        {
            // We hide this from the editor just because this is a large combined string of all digital labels,
            // and it will make more sense for consumers to use the "Labels" property
            get => m_label;
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "undefined";

                if (value.Trim().Length > MaximumLabelLength)
                    throw new OverflowException($"Label length cannot exceed {MaximumLabelLength}");

                // We override this function since base class automatically "fixes-up" labels
                // by removing duplicate white space characters
                m_label = value.Trim();

                // We pass value along to base class for posterity...
                base.Label = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="IEEEC37_118.DraftRevision"/> of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        public DraftRevision DraftRevision => DraftRevision.Std2011;

        /// <summary>
        /// Gets the <see cref="GSF.PhasorProtocols.DataFormat"/> of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Data format for digital values will always be <see cref="GSF.PhasorProtocols.DataFormat.FixedInteger"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override DataFormat DataFormat => DataFormat.FixedInteger;

        /// <summary>
        /// Gets or sets the offset of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Offset for digital values will always be 0; assigning a value other than 0 will thrown an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">Digital values represent bit flags and thus do not support an offset.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override double Offset
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
        public sealed override uint ScalingValue
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
        public sealed override double ScalePerBit => 1.0D;

        /// <summary>
        /// Gets conversion factor image of this <see cref="DigitalDefinition3"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];

                BigEndian.CopyBytes(m_normalStatus, buffer, 0);
                BigEndian.CopyBytes(m_validInputs, buffer, 2);

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

                if (DraftRevision > DraftRevision.Draft6)
                {
                    baseAttributes.Add("Bit Label Count", LabelCount.ToString());

                    for (int x = 0; x < LabelCount; x++)
                    {
                        baseAttributes.Add($"     Bit {x} Label", GetLabel(x));
                    }
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the individual labels for specified bit in this <see cref="DigitalDefinition3"/>.
        /// </summary>
        /// <param name="index">Index of desired bit label to access.</param>
        /// <remarks>
        /// <para>In the final version of the protocol each digital bit can be labeled, but we read them out as one big string in the "Label" property so this property allows individual access to each label.</para>
        /// <para>Note that the draft 6 implementation of the protocol supports one label for all 16-bits, however draft 7 (i.e., version 1) supports a label for each of the 16 bits.</para>
        /// </remarks>
        /// <returns>A <see cref="string"/> value of the label corresponding to the parameter.</returns>
        public string GetLabel(int index)
        {
            if (index < 0 || index >= LabelCount)
                throw new IndexOutOfRangeException($"Invalid label index specified.  Note that there are {LabelCount} labels per digital available in {DraftRevision} of the IEEE C37.118 protocol");

            return Label.PadRight(MaximumLabelLength).Substring(index * 16, 16).GetValidLabel();
        }

        /// <summary>
        /// Sets the individual labels for specified bit in this <see cref="DigitalDefinition3"/>.
        /// </summary>
        /// <param name="index">Index of desired bit label to access.</param>
        /// <param name="value">Value of the bit label to assign.</param>
        /// <remarks>
        /// <para>In the final version of the protocol each digital bit can be labeled, but we read them out as one big string in the "Label" property so this property allows individual access to each label.</para>
        /// <para>Note that the draft 6 implementation of the protocol supports one label for all 16-bits, however draft 7 (i.e., version 1) supports a label for each of the 16 bits.</para>
        /// </remarks>
        public void SetLabel(int index, string value)
        {
            if (index < 0 || index >= LabelCount)
                throw new IndexOutOfRangeException($"Invalid label index specified.  Note that there are {LabelCount} labels per digital available in {DraftRevision} of the IEEE C37.118 protocol");

            if (value.Trim().Length > 255)
                throw new OverflowException($"Individual label length cannot exceed 255");

            string current = Label.PadRight(MaximumLabelLength);
            string left = "";
            string right = "";

            if (index > 0)
                left = current.Substring(0, index * 16);

            if (index < 15)
                right = current.Substring((index + 1) * 16);

            Label = left + value.GetValidLabel().PadRight(16) + right;
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            int parseLength = MaximumLabelLength;
            byte[] labelBuffer = new byte[16];
            string[] labels = new string[16];

            for (int i = 0; i < 16; i++)
            {
                // Get next label buffer
                Buffer.BlockCopy(buffer, startIndex + i * 16, labelBuffer, 0, 16);

                bool foundNull = false;

                // Replace null characters with spaces; since characters after null
                // are usually invalid garbage, blank these out with spaces as well
                for (int j = 0; j < 16; j++)
                {
                    if (foundNull || labelBuffer[j] == 0)
                    {
                        foundNull = true;
                        labelBuffer[j] = 32;
                    }
                }

                // Interpret bytes as an ASCII string
                labels[i] = Encoding.ASCII.GetString(labelBuffer, 0, 16);
            }

            // Concatenate all labels together into one large string
            Label = string.Concat(labels);

            return parseLength;
        }

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] buffer, int startIndex)
        {
            m_normalStatus = BigEndian.ToUInt16(buffer, startIndex);
            m_validInputs = BigEndian.ToUInt16(buffer, startIndex + 2);

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
            info.AddValue("normalStatus", m_normalStatus);
            info.AddValue("validInputs", m_validInputs);
            info.AddValue("digitalLabels", m_label);
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