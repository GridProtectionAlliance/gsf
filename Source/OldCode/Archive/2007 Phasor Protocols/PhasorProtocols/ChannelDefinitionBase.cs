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
using System.Text;
//using PhasorProtocols.Common;
//using TVA.Text.Common;

//*******************************************************************************************************
//  ChannelDefinitionBase.vb - Channel data definition base class
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
//  3/7/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent definition of any kind of data.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelDefinitionBase : ChannelBase, IChannelDefinition
    {



        private IConfigurationCell m_parent;
        private int m_index;
        private string m_label;
        private int m_scale;
        private float m_offset;

        protected ChannelDefinitionBase()
        {
        }

        protected ChannelDefinitionBase(SerializationInfo info, StreamingContext context)
        {

            // Deserialize channel definition
            m_parent = (IConfigurationCell)info.GetValue("parent", typeof(IConfigurationCell));
            m_index = info.GetInt32("index");
            this.Label = info.GetString("label");
            m_scale = info.GetInt32("scale");
            m_offset = info.GetSingle("offset");

        }

        protected ChannelDefinitionBase(IConfigurationCell parent)
        {

            m_parent = parent;
            m_label = "undefined";
            m_scale = 1;

        }

        protected ChannelDefinitionBase(IConfigurationCell parent, int index, string label, int scale, float offset)
        {

            m_parent = parent;
            m_index = index;
            this.Label = label;
            m_scale = scale;
            m_offset = offset;

        }

        protected ChannelDefinitionBase(IConfigurationCell parent, byte[] binaryImage, int startIndex)
        {

            m_parent = parent;
            ParseBinaryImage(null, binaryImage, startIndex);

        }

        // Derived classes are expected to expose a Protected Sub New(ByVal channelDefinition As IChannelDefinition)
        protected ChannelDefinitionBase(IChannelDefinition channelDefinition)
            : this(channelDefinition.Parent, channelDefinition.Index, channelDefinition.Label, channelDefinition.ScalingFactor, channelDefinition.Offset)
        {


        }

        public virtual IConfigurationCell Parent
        {
            get
            {
                return m_parent;
            }
        }

        public abstract DataFormat DataFormat
        {
            get;
        }

        public virtual int Index
        {
            get
            {
                return m_index;
            }
            set
            {
                m_index = value;
            }
        }

        public virtual float Offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
            }
        }

        public virtual int ScalingFactor
        {
            get
            {
                return m_scale;
            }
            set
            {
                if (value > MaximumScalingFactor)
                {
                    throw (new OverflowException("Scaling factor value cannot exceed " + MaximumScalingFactor));
                }
                m_scale = value;
            }
        }

        public virtual float ConversionFactor
        {
            get
            {
                return m_scale * ScalePerBit;
            }
            set
            {
                ScalingFactor = (int)(value / ScalePerBit);
            }
        }

        public virtual float ScalePerBit
        {
            get
            {
                // Typical scale/bit is 10^-5
                return 0.00001F;
            }
        }

        public virtual int MaximumScalingFactor
        {
            get
            {
                // Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space
                return 0x1FFFFFF;
            }
        }

        public virtual string Label
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
                    m_label = PhasorProtocols.Common.GetValidLabel(value).Trim();
                }
            }
        }

        public virtual byte[] LabelImage
        {
            get
            {
                return Encoding.ASCII.GetBytes(m_label.PadRight(MaximumLabelLength));
            }
        }

        public virtual int MaximumLabelLength
        {
            get
            {
                // Typical label length is 16 characters
                return 16;
            }
        }

        public bool Equals(IChannelDefinition other)
        {

            return (CompareTo(other) == 0);

        }

        public virtual int CompareTo(object obj)
        {

            IChannelDefinition other = obj as IChannelDefinition;
            if (other != null)
            {
                return CompareTo(other);
            }
            throw (new ArgumentException("ChannelDefinition can only be compared to other IChannelDefinitions"));

        }

        public int CompareTo(IChannelDefinition other)
        {

            // We sort channel Definitions by index
            return m_index.CompareTo(other.Index);

        }

        protected override ushort BodyLength
        {
            get
            {
                return (ushort)MaximumLabelLength;
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                return LabelImage;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            int length = Array.IndexOf(binaryImage, (byte)0, startIndex, MaximumLabelLength) - startIndex;

            if (length < 0)
            {
                length = MaximumLabelLength;
            }

            Label = Encoding.ASCII.GetString(binaryImage, startIndex, length);

        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            // Serialize channel definition
            info.AddValue("parent", m_parent, typeof(IConfigurationCell));
            info.AddValue("index", m_index);
            info.AddValue("label", m_label);
            info.AddValue("scale", m_scale);
            info.AddValue("offset", m_offset);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Label", Label);
                baseAttributes.Add("Index", Index.ToString());
                baseAttributes.Add("Offset", Offset.ToString());
                baseAttributes.Add("Data Format", (int)DataFormat + ": " + Enum.GetName(typeof(DataFormat), DataFormat));
                baseAttributes.Add("Scaling Factor", ScalingFactor.ToString());
                baseAttributes.Add("Scale per Bit", ScalePerBit.ToString());
                baseAttributes.Add("Maximum Scaling Factor", MaximumScalingFactor.ToString());
                baseAttributes.Add("Conversion Factor", ConversionFactor.ToString());
                baseAttributes.Add("Maximum Label Length", MaximumLabelLength.ToString());

                return baseAttributes;
            }
        }

    }
}
