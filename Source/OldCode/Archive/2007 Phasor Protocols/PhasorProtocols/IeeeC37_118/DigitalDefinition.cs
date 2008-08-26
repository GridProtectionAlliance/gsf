using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Text;
//using PhasorProtocols.Common;

//*******************************************************************************************************
//  DigitalDefinition.vb - IEEE C37.118 Digital definition
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class DigitalDefinition : DigitalDefinitionBase
        {



            private short m_normalStatus;
            private short m_validInputs;
            private string m_label;
            private bool m_parentAquired;
            private DraftRevision m_draftRevision;

            protected DigitalDefinition()
            {
            }

            protected DigitalDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize digital definition
                m_normalStatus = info.GetInt16("normalStatus");
                m_validInputs = info.GetInt16("validInputs");
                m_label = info.GetString("digitalLabels");

            }

            public DigitalDefinition(ConfigurationCell parent)
                : base(parent)
            {


            }

            public DigitalDefinition(ConfigurationCell parent, int index, string label)
                : base(parent, index, label)
            {


            }

            public DigitalDefinition(ConfigurationCell parent, byte[] binaryImage, int startIndex)
                : base(parent, binaryImage, startIndex)
            {


            }

            public DigitalDefinition(ConfigurationCell parent, IDigitalDefinition digitalDefinition)
                : base(parent, digitalDefinition)
            {


            }

            internal static IDigitalDefinition CreateNewDigitalDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            {

                return new DigitalDefinition((ConfigurationCell)parent, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCell Parent
            {
                get
                {
                    return (ConfigurationCell)base.Parent;
                }
            }

            public int LabelCount
            {
                get
                {
                    if (DraftRevision == DraftRevision.Draft6)
                    {
                        return 1;
                    }
                    else
                    {
                        return 16;
                    }
                }
            }

            // We hide this from the editor just because this is a large combined string of all digital labels,
            // and it will make more sense for consumers to use the "Labels" property
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string Label
            {
                get
                {
                    return m_label;
                }
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        value = "undefined";
                    }

                    if (value.Trim().Length > MaximumLabelLength)
                    {
                        throw (new OverflowException("Label length cannot exceed " + MaximumLabelLength));
                    }
                    else
                    {
                        // We override this function since base class automatically "fixes-up" labels
                        // by removing duplicate white space characters - this can throw off the
                        // label offsets which would break the "Labels" property (below)
                        m_label = value.Trim();
                    }
                }
            }

            /// <summary>Accesses individual labels for each bit in the digital Definition</summary>
            /// <param name="index">Desired bit label to access</param>
            /// <remarks>
            /// <para>In the final version of the protocol each digital bit can be labeled, but we read them out as one big string in the "Label" property so this property allows individual access to each label</para>
            /// <para>Note that the draft 6 implementation of the protocol supports one label for all 16-bits, however draft 7 (i.e., version 1) supports a label for each of the 16 bits</para>
            /// </remarks>
            public string Labels(int index)
            {
                if (index < 0 || index >= LabelCount)
                {
                    throw (new IndexOutOfRangeException("Invalid label index specified.  Note that there are " + LabelCount + " labels per digital available in " + Enum.GetName(typeof(DraftRevision), DraftRevision) + " of the IEEE C37.118 protocol"));
                }

                return PhasorProtocols.Common.GetValidLabel(Label.PadRight(MaximumLabelLength).Substring(index * 16, base.MaximumLabelLength));
            }
            public void SetLabels(int index, string value)
            {
                if (index < 0 || index >= LabelCount)
                {
                    throw (new IndexOutOfRangeException("Invalid label index specified.  Note that there are " + LabelCount + " labels per digital available in " + Enum.GetName(typeof(DraftRevision), DraftRevision) + " of the IEEE C37.118 protocol"));
                }

                if (value.Trim().Length > base.MaximumLabelLength)
                {
                    throw (new OverflowException("Label length cannot exceed " + base.MaximumLabelLength));
                }
                else
                {
                    string current = Label.PadRight(MaximumLabelLength);
                    string left = "";
                    string right = "";

                    if (index > 0)
                    {
                        left = current.Substring(0, index * base.MaximumLabelLength);
                    }
                    if (index < 15)
                    {
                        right = current.Substring((index + 1) * base.MaximumLabelLength);
                    }

                    Label = left + PhasorProtocols.Common.GetValidLabel(value).PadRight(base.MaximumLabelLength) + right;
                }
            }

            public override int MaximumLabelLength
            {
                get
                {
                    return LabelCount * base.MaximumLabelLength;
                }
            }

            public short NormalStatus
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

            public short ValidInputs
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

            public DraftRevision DraftRevision
            {
                get
                {
                    if (m_parentAquired)
                    {
                        return m_draftRevision;
                    }
                    else
                    {
                        // We must assume version 1 until a parent reference is available
                        // Note: parent class, being higher up in the chain, is not available during early
                        // points of deserialization of this class - however, this method gets called
                        // to determine proper number of maximum digital labels - hence the need for
                        // this function - since we had to do this anyway, we took the opportunity to
                        // cache this value locally for speed
                        if ((Parent != null) && (Parent.Parent != null))
                        {
                            m_parentAquired = true;
                            m_draftRevision = Parent.Parent.DraftRevision;
                            return m_draftRevision;
                        }
                        else
                        {
                            return DraftRevision.Draft7;
                        }
                    }
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                if (DraftRevision == DraftRevision.Draft6)
                {
                    // Handle single label the standard way (parsing out null value)
                    base.ParseBodyImage(state, binaryImage, startIndex);
                }
                else
                {
                    // For "multiple" labels - we just replace null's with spaces
                    for (int x = startIndex; x <= startIndex + MaximumLabelLength - 1; x++)
                    {
                        if (binaryImage[x] == 0)
                        {
                            binaryImage[x] = 32;
                        }
                    }

                    Label = Encoding.ASCII.GetString(binaryImage, startIndex, MaximumLabelLength);
                }

            }

            internal static int ConversionFactorLength
            {
                get
                {
                    return 4;
                }
            }

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

            internal void ParseConversionFactor(byte[] binaryImage, int startIndex)
            {

                m_normalStatus = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                m_validInputs = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2);

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize digital definition
                info.AddValue("normalStatus", m_normalStatus);
                info.AddValue("validInputs", m_validInputs);
                info.AddValue("digitalLabels", m_label);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;
                    byte[] normalStatusBytes = BitConverter.GetBytes(NormalStatus);
                    byte[] validInputsBytes = BitConverter.GetBytes(ValidInputs);

                    baseAttributes.Add("Normal Status", NormalStatus.ToString());
                    baseAttributes.Add("Normal Status (Big Endian Bits)", ((ByteEncoding)ByteEncoding.BigEndianBinary).GetString(normalStatusBytes));
                    baseAttributes.Add("Normal Status (Hexadecimal)", "0x" + ((ByteEncoding)ByteEncoding.Hexadecimal).GetString(normalStatusBytes));

                    baseAttributes.Add("Valid Inputs", ValidInputs.ToString());
                    baseAttributes.Add("Valid Inputs (Big Endian Bits)", ((ByteEncoding)ByteEncoding.BigEndianBinary).GetString(validInputsBytes));
                    baseAttributes.Add("Valid Inputs (Hexadecimal)", "0x" + ((ByteEncoding)ByteEncoding.Hexadecimal).GetString(validInputsBytes));

                    if (DraftRevision > DraftRevision.Draft6)
                    {
                        baseAttributes.Add("Bit Label Count", LabelCount.ToString());
                        for (int x = 0; x <= LabelCount - 1; x++)
                        {
                            baseAttributes.Add("     Bit " + x + " Label", Labels(x));
                        }
                    }

                    return baseAttributes;
                }
            }

        }

    }

}
