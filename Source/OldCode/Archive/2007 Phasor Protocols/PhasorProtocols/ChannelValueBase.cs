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
using PCS.Measurements;

//*******************************************************************************************************
//  ChannelValueBase.vb - Channel data value base class
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

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of any kind of data value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelValueBase<T> : ChannelBase, IChannelValue<T> where T : IChannelDefinition
    {



        private IDataCell m_parent;
        private T m_definition;
        private IMeasurement[] m_measurements;

        protected ChannelValueBase()
        {
        }

        protected ChannelValueBase(SerializationInfo info, StreamingContext context)
        {

            // Deserialize channel value
            m_parent = (IDataCell)info.GetValue("parent", typeof(IDataCell));
            m_definition = (T)info.GetValue("definition", typeof(T));

        }

        protected ChannelValueBase(IDataCell parent)
        {

            m_parent = parent;

        }

        protected ChannelValueBase(IDataCell parent, T channelDefinition)
        {

            m_parent = parent;
            m_definition = channelDefinition;

        }

        // Derived classes are expected to expose a Protected Sub New(ByVal channelValue As IChannelValue(Of T))
        protected ChannelValueBase(IChannelValue<T> channelValue)
            : this(channelValue.Parent, channelValue.Definition)
        {


        }

        public virtual IDataCell Parent
        {
            get
            {
                return m_parent;
            }
        }

        public virtual T Definition
        {
            get
            {
                return m_definition;
            }
            set
            {
                m_definition = value;
            }
        }

        public virtual DataFormat DataFormat
        {
            get
            {
                return m_definition.DataFormat;
            }
        }

        public virtual string Label
        {
            get
            {
                return m_definition.Label;
            }
        }

        public abstract bool IsEmpty
        {
            get;
        }

        public abstract float this[int index]
        {
            get;
            set;
        }

        public abstract int CompositeValueCount
        {
            get;
        }

        public virtual IMeasurement[] Measurements
        {
            get
            {
                // Create a measurement instance for each composite value the derived channel value exposes
                if (m_measurements == null)
                {
                    m_measurements = new IMeasurement[CompositeValueCount];

                    for (int x = 0; x <= m_measurements.Length - 1; x++)
                    {
                        m_measurements[x] = new ChannelValueMeasurement<T>(this, x);
                    }
                }

                return m_measurements;
            }
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            // Serialize channel value
            info.AddValue("parent", m_parent, typeof(IDataCell));
            info.AddValue("definition", m_definition, typeof(T));

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Label", Label);
                baseAttributes.Add("Data Format", (int)DataFormat + ": " + DataFormat);
                baseAttributes.Add("Is Empty", IsEmpty.ToString());
                baseAttributes.Add("Total Composite Values", CompositeValueCount.ToString());

                for (int x = 0; x <= CompositeValueCount - 1; x++)
                {
                    baseAttributes.Add("     Composite Value " + x, this[x].ToString());
                }

                return baseAttributes;
            }
        }

    }

}
