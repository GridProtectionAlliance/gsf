using System.Diagnostics;
using System;
////using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
////using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  PhasorDefinitionBase.vb - Phasor value definition base class
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
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
    /// <summary>This class represents the common implementation of the protocol independent definition of a phasor value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class PhasorDefinitionBase : ChannelDefinitionBase, IPhasorDefinition
    {



        private PhasorType m_type;
        private IPhasorDefinition m_voltageReference;

        protected PhasorDefinitionBase()
        {
        }

        protected PhasorDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize phasor definition
            m_type = (PhasorType)info.GetValue("type", typeof(PhasorType));
            m_voltageReference = (IPhasorDefinition)info.GetValue("voltageReference", typeof(IPhasorDefinition));

        }

        protected PhasorDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {


            m_type = PhasorType.Voltage;
            m_voltageReference = this;

        }

        protected PhasorDefinitionBase(IConfigurationCell parent, int index, string label, int scale, float offset, PhasorType type, IPhasorDefinition voltageReference)
            : base(parent, index, label, scale, offset)
        {


            m_type = type;

            if (type == PhasorType.Voltage)
            {
                m_voltageReference = this;
            }
            else
            {
                m_voltageReference = voltageReference;
            }

        }

        protected PhasorDefinitionBase(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            : base(parent, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal phasorDefinition As IPhasorDefinition)
        protected PhasorDefinitionBase(IConfigurationCell parent, IPhasorDefinition phasorDefinition)
            : this(parent, phasorDefinition.Index, phasorDefinition.Label, phasorDefinition.ScalingFactor, phasorDefinition.Offset, phasorDefinition.Type, phasorDefinition.VoltageReference)
        {


        }

        public override DataFormat DataFormat
        {
            get
            {
                return Parent.PhasorDataFormat;
            }
        }

        CoordinateFormat IPhasorDefinition.CoordinateFormat
        {
            get
            {
                return this.CoordinateFormat;
            }
        }

        public virtual CoordinateFormat CoordinateFormat
        {
            get
            {
                return Parent.PhasorCoordinateFormat;
            }
        }

        //PhasorType IPhasorDefinition.Type
        //{
        //    get
        //    {
        //        return this.Type;
        //    }
        //    set
        //    {
        //        this.Type = value;
        //    }
        //}

        public virtual PhasorType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }

        //IPhasorDefinition IPhasorDefinition.VoltageReference
        //{
        //    get
        //    {
        //        return this.VoltageReference;
        //    }
        //    set
        //    {
        //        this.VoltageReference = value;
        //    }
        //}

        public virtual IPhasorDefinition VoltageReference
        {
            get
            {
                return m_voltageReference;
            }
            set
            {
                if (m_type == PhasorType.Voltage)
                {
                    if (value != this)
                    {
                        throw (new NotImplementedException("Voltage phasors do not have a voltage reference"));
                    }
                }
                else
                {
                    m_voltageReference = value;
                }
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize phasor definition
            info.AddValue("type", m_type, typeof(PhasorType));
            info.AddValue("voltageReference", m_voltageReference, typeof(IPhasorDefinition));

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Phasor Type", (int)Type + ": " + Type);

                return baseAttributes;
            }
        }

    }
}
