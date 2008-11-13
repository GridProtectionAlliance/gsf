using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
//using PhasorProtocols.Common;

//*******************************************************************************************************
//  FrequencyDefinitionBase.vb - Frequency and df/dt value definition base class
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
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent definition of a frequency and df/dt value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class FrequencyDefinitionBase : ChannelDefinitionBase, IFrequencyDefinition
    {



        private int m_dfdtScale;
        private float m_dfdtOffset;

        protected FrequencyDefinitionBase()
        {
        }

        protected FrequencyDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize frequency definition
            m_dfdtScale = info.GetInt32("dfdtScale");
            m_dfdtOffset = info.GetSingle("dfdtOffset");

        }

        protected FrequencyDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {


        }

        protected FrequencyDefinitionBase(IConfigurationCell parent, string label, int scale, float offset, int dfdtScale, float dfdtOffset)
            : base(parent, 0, label, scale, offset)
        {


            m_dfdtScale = dfdtScale;
            m_dfdtOffset = dfdtOffset;

        }

        protected FrequencyDefinitionBase(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            : base(parent, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)
        protected FrequencyDefinitionBase(IConfigurationCell parent, IFrequencyDefinition frequencyDefinition)
            : this(parent, frequencyDefinition.Label, frequencyDefinition.ScalingFactor, frequencyDefinition.Offset, frequencyDefinition.DfDtScalingFactor, frequencyDefinition.DfDtOffset)
        {


        }

        public override DataFormat DataFormat
        {
            get
            {
                return Parent.FrequencyDataFormat;
            }
        }

        public virtual LineFrequency NominalFrequency
        {
            get
            {
                return Parent.NominalFrequency;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Index
        {
            get
            {
                return base.Index;
            }
            set
            {
                base.Index = value;
            }
        }

        public override float Offset
        {
            get
            {
                return (float)Parent.NominalFrequency;
            }
            set
            {
                throw (new NotSupportedException("Frequency offset is read-only - it is determined by nominal frequency specified in containing condiguration cell"));
            }
        }

        public virtual float DfDtOffset
        {
            get
            {
                return m_dfdtOffset;
            }
            set
            {
                m_dfdtOffset = value;
            }
        }

        public virtual int DfDtScalingFactor
        {
            get
            {
                return m_dfdtScale;
            }
            set
            {
                m_dfdtScale = value;
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize frequency definition
            info.AddValue("dfdtScale", m_dfdtScale);
            info.AddValue("dfdtOffset", m_dfdtOffset);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("df/dt Offset", DfDtOffset.ToString());
                baseAttributes.Add("df/dt Scaling Factor", DfDtScalingFactor.ToString());

                return baseAttributes;
            }
        }

    }
}
