//*******************************************************************************************************
//  DigitalDefinition.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/24/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace PCS.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IDigitalDefinition"/>.
    /// </summary>
    [Serializable()]
    public class DigitalDefinition : DigitalDefinitionBase
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        // Fields
        private ushort m_normalStatus;
        private ushort m_validInputs;
        private string m_label;
        private bool m_parentAquired;
        private DraftRevision m_draftRevision;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinition"/>.</param>
        public DigitalDefinition(ConfigurationCell parent, string label)
            : base(parent, label)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalDefinition(SerializationInfo info, StreamingContext context)
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
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="DigitalDefinition"/>.
        /// </summary>
        public virtual new ConfigurationCell Parent
        {
            get
            {
                return base.Parent as ConfigurationCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets normal status for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public ushort NormalStatus
        {
            get
            {
                return m_normalStatus;
            }
            set
            {
                m_normalStatus = value;
            }
        }

        /// <summary>
        /// Gets or sets valid input for this <see cref="DigitalDefinition"/>.
        /// </summary>
        public ushort ValidInputs
        {
            get
            {
                return m_validInputs;
            }
            set
            {
                m_validInputs = value;
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="Label"/> of this <see cref="DigitalDefinition"/>.
        /// </summary>
        public override int MaximumLabelLength
        {
            get
            {
                return LabelCount * 16;
            }
        }

        /// <summary>
        /// Gets the number of labels defined in this <see cref="DigitalDefinition"/>.
        /// </summary>
        public int LabelCount
        {
            get
            {
                if (DraftRevision == DraftRevision.Draft6)
                    return 1;
                else
                    return 16;
            }
        }

        /// <summary>
        /// Gets or sets the combined set of label images of this <see cref="DigitalDefinition"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Label
        {
            // We hide this from the editor just because this is a large combined string of all digital labels,
            // and it will make more sense for consumers to use the "Labels" property
            get
            {
                return m_label;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "undefined";

                if (value.Trim().Length > MaximumLabelLength)
                    throw new OverflowException("Label length cannot exceed " + MaximumLabelLength);
                else
                    // We override this function since base class automatically "fixes-up" labels
                    // by removing duplicate white space characters - this can throw off the
                    // label offsets which would break the Get/Set Label methods (below)
                    m_label = value.Trim();
            }
        }

        /// <summary>
        /// Gets the <see cref="IeeeC37_118.DraftRevision"/> of this <see cref="DigitalDefinition"/>.
        /// </summary>
        public DraftRevision DraftRevision
        {
            get
            {
                if (m_parentAquired)
                    return m_draftRevision;
                else
                {
                    // We must assume version 1 until a parent reference is available. The parent class,
                    // being higher up in the chain, is not available during early points of
                    // deserialization of this class - however, this method gets called to determine
                    // proper number of maximum digital labels - hence the need for this function -
                    // since we had to do this anyway, we took the opportunity to cache this value
                    // locally for speed in future calls
                    if (Parent != null && Parent.Parent != null)
                    {
                        m_parentAquired = true;
                        m_draftRevision = Parent.Parent.DraftRevision;
                        return m_draftRevision;
                    }
                    else
                        return DraftRevision.Draft7;
                }
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="DigitalDefinition"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];

                EndianOrder.BigEndian.CopyBytes(m_normalStatus, buffer, 0);
                EndianOrder.BigEndian.CopyBytes(m_validInputs, buffer, 2);

                return buffer;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="DigitalDefinition"/> object.
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
                baseAttributes.Add("Normal Status (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(normalStatusBytes));

                baseAttributes.Add("Valid Inputs", ValidInputs.ToString());
                baseAttributes.Add("Valid Inputs (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(validInputsBytes));
                baseAttributes.Add("Valid Inputs (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(validInputsBytes));

                if (DraftRevision > DraftRevision.Draft6)
                {
                    baseAttributes.Add("Bit Label Count", LabelCount.ToString());

                    for (int x = 0; x < LabelCount; x++)
                    {
                        baseAttributes.Add("     Bit " + x + " Label", GetLabel(x));
                    }
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the individual labels for specified bit in this <see cref="DigitalDefinition"/>.
        /// </summary>
        /// <param name="index">Index of desired bit label to access.</param>
        /// <remarks>
        /// <para>In the final version of the protocol each digital bit can be labeled, but we read them out as one big string in the "Label" property so this property allows individual access to each label.</para>
        /// <para>Note that the draft 6 implementation of the protocol supports one label for all 16-bits, however draft 7 (i.e., version 1) supports a label for each of the 16 bits.</para>
        /// </remarks>
        public string GetLabel(int index)
        {
            if (index < 0 || index >= LabelCount)
                throw new IndexOutOfRangeException("Invalid label index specified.  Note that there are " + LabelCount + " labels per digital available in " + DraftRevision + " of the IEEE C37.118 protocol");

            return Label.PadRight(MaximumLabelLength).Substring(index * 16, 16).GetValidLabel();
        }

        /// <summary>
        /// Sets the individual labels for specified bit in this <see cref="DigitalDefinition"/>.
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
                throw new IndexOutOfRangeException("Invalid label index specified.  Note that there are " + LabelCount + " labels per digital available in " + DraftRevision + " of the IEEE C37.118 protocol");

            if (value.Trim().Length > 16)
                throw new OverflowException("Individual label length cannot exceed " + 16);
            else
            {
                string current = Label.PadRight(MaximumLabelLength);
                string left = "";
                string right = "";

                if (index > 0)
                    left = current.Substring(0, index * 16);

                if (index < 15)
                    right = current.Substring((index + 1) * 16);

                Label = left + value.PadRight(16).GetValidLabel() + right;
            }
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            if (DraftRevision == DraftRevision.Draft6)
            {
                // Handle single label the standard way (parsing out null value)
                return base.ParseBodyImage(binaryImage, startIndex, length);
            }
            else
            {
                int parseLength = MaximumLabelLength;

                // For "multiple" labels - we just replace null's with spaces
                for (int x = startIndex; x < startIndex + parseLength; x++)
                {
                    if (binaryImage[x] == 0)
                        binaryImage[x] = 32;
                }

                Label = Encoding.ASCII.GetString(binaryImage, startIndex, parseLength);

                return parseLength;
            }
        }

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        internal int ParseConversionFactor(byte[] binaryImage, int startIndex)
        {
            m_normalStatus = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);
            m_validInputs = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 2);

            return ConversionFactorLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
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
        internal static IDigitalDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IDigitalDefinition digitalDefinition = new DigitalDefinition(parent);

            parsedLength = digitalDefinition.Initialize(binaryImage, startIndex, 0);

            return digitalDefinition;
        }

        #endregion        
    }
}