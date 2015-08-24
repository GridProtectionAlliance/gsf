//*******************************************************************************************************
//  ConfigurationFrame.cs
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
//  02/08/2007 - James R Carroll & Jian (Ryan) Zuo
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace TVA.PhasorProtocols.FNet
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timeOffset">The time offset of F-NET device in <see cref="Ticks"/>.</param>
        /// <param name="stationName">The station name of the F-NET device.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an F-NET configuration frame.
        /// </remarks>
        public ConfigurationFrame(ushort idCode, Ticks timestamp, ushort frameRate, LineFrequency nominalFrequency, Ticks timeOffset, string stationName)
            : base(idCode, new ConfigurationCellCollection(), timestamp, frameRate)
        {
            ConfigurationCell configCell = new ConfigurationCell(this, nominalFrequency, timeOffset);

            // FNet protocol sends data for one device
            Cells.Add(configCell);

            // Assign station name
            if (string.IsNullOrEmpty(stationName))
                configCell.StationName = "F-NET Unit-" + idCode;
            else
                configCell.StationName = stationName;

            // Add a single frequency definition
            configCell.FrequencyDefinition = new FrequencyDefinition(configCell, "Line frequency");

            // Add a single phasor definition
            configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "120V Phasor", PhasorType.Voltage, null));
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public LineFrequency NominalFrequency
        {
            // Since F-NET only supports a single device there will only be one cell, so we just share this value
            // with our only child and expose the value at the parent level for convenience
            get
            {
                return Cells[0].NominalFrequency;
            }
            set
            {
                Cells[0].NominalFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the time offset of F-NET device in <see cref="Ticks"/>.
        /// </summary>
        /// <remarks>
        /// F-NET devices normally report time in 11 seconds past real-time, this property defines the offset for this this artificial delay.
        /// Note that the parameter value is in ticks to allow a very high-resolution offset;  1 second = 10,000,000 ticks.
        /// </remarks>
        public Ticks TimeOffset
        {
            // Since F-NET only supports a single device there will only be one cell, so we just share this value
            // with our only child and expose the value at the parent level for convenience
            get
            {
                return Cells[0].TimeOffset;
            }
            set
            {
                Cells[0].TimeOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the longitude (i.e., East/West geospatial position) of the device.
        /// </summary>
        public double Longitude
        {
            // Since F-NET only supports a single device there will only be one cell, so we just share this value
            // with our only child and expose the value at the parent level for convenience
            get
            {
                return Cells[0].Longitude;
            }
            set
            {
                Cells[0].Longitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the latitude (i.e., North/South geospatial position) of the device.
        /// </summary>
        public double Latitude
        {
            // Since F-NET only supports a single device there will only be one cell, so we just share this value
            // with our only child and expose the value at the parent level for convenience
            get
            {
                return Cells[0].Latitude;
            }
            set
            {
                Cells[0].Latitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of satellites visible to the GPS timing source of the device.
        /// </summary>
        public int NumberOfSatellites
        {
            // Since F-NET only supports a single device there will only be one cell, so we just share this value
            // with our only child and expose the value at the parent level for convenience
            get
            {
                return Cells[0].NumberOfSatellites;
            }
            set
            {
                Cells[0].NumberOfSatellites = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// F-NET doesn't use checksums - this always returns true.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            return true;
        }

        /// <summary>
        /// Method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">F-NET doesn't use checksums.</exception>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}