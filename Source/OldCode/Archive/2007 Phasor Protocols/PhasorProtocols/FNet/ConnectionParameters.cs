//*******************************************************************************************************
//  ConnectionParameters.cs
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
//  02/26/2007 - James R Carroll & Jian (Ryan) Zuo
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols.FNet
{
    /// <summary>
    /// Represents the extra connection parameters required for a connection to a F-NET device.
    /// </summary>
    /// <remarks>
    /// This class is designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.
    /// As a result the <see cref="CategoryAttribute"/> and <see cref="DescriptionAttribute"/> elements should be defined for
    /// each of the exposed properties.
    /// </remarks>
    [Serializable()]
    public class ConnectionParameters : ConnectionParametersBase
    {        
        #region [ Members ]

        // Fields
        private Ticks m_timeOffset;
        private short m_frameRate;
        private LineFrequency m_nominalFrequency;
        private string m_stationName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConnectionParameters"/>.
        /// </summary>
        public ConnectionParameters()
        {
            m_timeOffset = Common.DefaultTimeOffset;
            m_frameRate = Common.DefaultFrameRate;
            m_nominalFrequency = Common.DefaultNominalFrequency;
            m_stationName = Common.DefaultStationName;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConnectionParameters(SerializationInfo info, StreamingContext context)
        {
            // Deserialize connection parameters
            m_timeOffset = info.GetInt64("timeOffset");
            m_frameRate = info.GetInt16("frameRate");
            m_nominalFrequency = (LineFrequency)info.GetValue("nominalFrequency", typeof(LineFrequency));
            m_stationName = info.GetString("stationName");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets time offset of the F-NET device in <see cref="Ticks"/>.
        /// </summary>
        /// <remarks>
        /// F-NET devices normally report time in 11 seconds past real-time, this property defines the offset for this this artificial delay.
        /// Note that the parameter value is in ticks to allow a very high-resolution offset;  1 second = 10,000,000 ticks.
        /// </remarks>
        [Category("Optional Connection Parameters"),
        Description("F-NET devices normally report time in 11 seconds past real-time, this parameter adjusts for this artificial delay.  Note parameter is in ticks (1 second = 10,000,000 ticks)."),
        DefaultValue(Common.DefaultTimeOffset)]
        public Ticks TimeOffset
        {
            get
            {
                return m_timeOffset;
            }
            set
            {
                m_timeOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the configured frame rate for the F-NET device.
        /// </summary>
        /// <remarks>
        /// This is typically set to 10 frames per second.
        /// </remarks>
        [Category("Optional Connection Parameters"),
        Description("Configured frame rate for F-NET device."),
        DefaultValue(Common.DefaultFrameRate)]
        public short FrameRate
        {
            get
            {
                return m_frameRate;
            }
            set
            {
                if (value < 1)
                    m_frameRate = Common.DefaultFrameRate;
                else
                    m_frameRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of this F-NET device.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Configured nominal frequency for F-NET device."),
        DefaultValue(typeof(LineFrequency), "Hz60")]
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

        /// <summary>
        /// Gets or sets the station name for the F-NET device.
        /// </summary>
        [Category("Optional Connection Parameters"),
        Description("Station name to use for F-NET device."),
        DefaultValue(Common.DefaultStationName)]
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize connection parameters
            info.AddValue("timeOffset", (long)m_timeOffset);
            info.AddValue("frameRate", m_frameRate);
            info.AddValue("nominalFrequency", m_nominalFrequency, typeof(LineFrequency));
            info.AddValue("stationName", m_stationName);
        }

        #endregion
    }
}