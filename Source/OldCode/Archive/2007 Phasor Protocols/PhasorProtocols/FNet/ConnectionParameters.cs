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
using System.ComponentModel;
using System.Runtime.Serialization;
//using PhasorProtocols.FNet.Common;

//*******************************************************************************************************
//  ConnectionParameters.vb - FNet specific connection parameters
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
//  02/26/2007 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    namespace FNet
    {

        [Serializable()]
        public class ConnectionParameters : ConnectionParametersBase
        {



            private long m_ticksOffset;
            private short m_frameRate;
            private LineFrequency m_nominalFrequency;
            private string m_stationName;

            protected ConnectionParameters(SerializationInfo info, StreamingContext context)
            {

                // Deserialize connection parameters
                m_ticksOffset = info.GetInt64("ticksOffset");
                m_frameRate = info.GetInt16("frameRate");
                m_nominalFrequency = (LineFrequency)info.GetValue("nominalFrequency", typeof(LineFrequency));
                m_stationName = info.GetString("stationName");

            }

            public ConnectionParameters()
            {

                m_ticksOffset = Common.DefaultTicksOffset;
                m_frameRate = Common.DefaultFrameRate;
                m_nominalFrequency = Common.DefaultNominalFrequency;

            }

            [Category("Optional Connection Parameters"), Description("FNET devices normally report time in 11 seconds past real-time, this parameter adjusts for this artificial delay.  Note parameter is in ticks (1 second = 10000000 ticks)."), DefaultValue(Common.DefaultTicksOffset)]
            public long TicksOffset
            {
                get
                {
                    return m_ticksOffset;
                }
                set
                {
                    m_ticksOffset = value;
                }
            }

            [Category("Optional Connection Parameters"), Description("Configured frame rate for FNET device."), DefaultValue(Common.DefaultFrameRate)]
            public short FrameRate
            {
                get
                {
                    return m_frameRate;
                }
                set
                {
                    if (value < 1)
                    {
                        m_frameRate = Common.DefaultFrameRate;
                    }
                    else
                    {
                        m_frameRate = value;
                    }
                }
            }

            [Category("Optional Connection Parameters"), Description("Configured nominal frequency for FNET device."), DefaultValue(typeof(LineFrequency), "Hz60")]
            public LineFrequency NominalFrequency
            {
                get
                {
                    return m_nominalFrequency;
                }
                set
                {
                    m_nominalFrequency = value;
                }
            }

            [Category("Optional Connection Parameters"), Description("Station name to use for FNET device.")]
            public string StationName
            {
                get
                {
                    return m_stationName;
                }
                set
                {
                    m_stationName = value;
                }
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {

                // Serialize connection parameters
                info.AddValue("ticksOffset", m_ticksOffset);
                info.AddValue("frameRate", m_frameRate);
                info.AddValue("nominalFrequency", m_nominalFrequency, typeof(LineFrequency));
                info.AddValue("stationName", m_stationName);

            }

        }

    }

}
