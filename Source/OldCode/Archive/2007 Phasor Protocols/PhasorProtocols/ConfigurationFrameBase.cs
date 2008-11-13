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
//using PCS.DateTime.Common;

//*******************************************************************************************************
//  ConfigurationFrameBase.vb - Configuration frame base class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of a configuration frame that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ConfigurationFrameBase : ChannelFrameBase<IConfigurationCell>, IConfigurationFrame
    {



        private short m_frameRate;
        private decimal m_ticksPerFrame;

        protected ConfigurationFrameBase()
        {
        }

        protected ConfigurationFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize configuration frame
            FrameRate = info.GetInt16("frameRate");

        }

        protected ConfigurationFrameBase(ConfigurationCellCollection cells)
            : base(cells)
        {


        }

        protected ConfigurationFrameBase(ushort idCode, ConfigurationCellCollection cells, long ticks, short frameRate)
            : base(idCode, cells, ticks)
        {

            this.FrameRate = frameRate;

        }

        // Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As int)
        // and automatically pass in state parameter
        protected ConfigurationFrameBase(IConfigurationFrameParsingState state, byte[] binaryImage, int startIndex)
            : base(state, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame)
        protected ConfigurationFrameBase(IConfigurationFrame configurationFrame)
            : this(configurationFrame.IDCode, configurationFrame.Cells, configurationFrame.Ticks, configurationFrame.FrameRate)
        {


        }

        protected override FundamentalFrameType FundamentalFrameType
        {
            get
            {
                return PhasorProtocols.FundamentalFrameType.ConfigurationFrame;
            }
        }

        public virtual new ConfigurationCellCollection Cells
        {
            get
            {
                return (ConfigurationCellCollection)base.Cells;
            }
        }

        public virtual short FrameRate
        {
            get
            {
                return m_frameRate;
            }
            set
            {
                m_frameRate = value;
                m_ticksPerFrame = decimal.Divide(TVA.DateTime.Common.get_SecondsToTicks(1), m_frameRate);
            }
        }

        public virtual decimal TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
        }

        public virtual void SetNominalFrequency(LineFrequency value)
        {

            foreach (IConfigurationCell cell in Cells)
            {
                cell.NominalFrequency = value;
            }

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameRate", m_frameRate);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frame Rate", FrameRate.ToString());
                baseAttributes.Add("Ticks Per Frame", TicksPerFrame.ToString());

                return baseAttributes;
            }
        }

    }
}
