//******************************************************************************************************
//  MetadataRecord.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/28/2015 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.IO.Checksums;

namespace KafkaAdapters
{
    /// <summary>
    /// Represents a record of Kafka time-series metadata.
    /// </summary>
    public class MetadataRecord
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="MetadataRecord"/>.
        /// </summary>
        public MetadataRecord()
        {
            UniqueID = Guid.Empty.ToString();
        }

        #endregion

        #region [ Members ]

        // Constants

        /// <summary>
        /// Date-time format used by <see cref="MetadataRecord"/>.
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        // Fields

        /// <summary>
        /// Measurement ID, i.e., locally unique metadata index.
        /// </summary>
        public uint ID;

        /// <summary>
        /// Cross-system unique measurement identification.
        /// </summary>
        public string UniqueID;

        /// <summary>
        /// Assigned point tag of measurement, if any.
        /// </summary>
        public string PointTag;

        /// <summary>
        /// Acronym of primary measurement source, e.g., a historian.
        /// </summary>
        public string Source;

        /// <summary>
        /// Acronym of device that creates measurements.
        /// </summary>
        public string Device;

        /// <summary>
        /// Longitude of device that creates the measurements.
        /// </summary>
        public float Longitude;

        /// <summary>
        /// Latitude of device that creates the measurements.
        /// </summary>
        public float Latitude;

        /// <summary>
        /// Acronym of device protocol used for transporting measured values.
        /// </summary>
        public string Protocol;

        /// <summary>
        /// Acronym of type of measured value, e.g., FREQ for frequency.
        /// </summary>
        public string SignalType;

        /// <summary>
        /// Engineering units associated with signal type, if any.
        /// </summary>
        public string EngineeringUnits;

        /// <summary>
        /// When measurement is part of a phasor, value will be V for voltage or I for current; otherwise null.
        /// </summary>
        public string PhasorType;

        /// <summary>
        /// When measurement is part of a phasor, represents the phase of the measured value; otherwise null.
        /// </summary>
        public string Phase;

        /// <summary>
        /// Description of the measurement.
        /// </summary>
        public string Description;

        /// <summary>
        /// Last update date and time of the metadata record in UTC. Format = yyyy-MM-dd HH:mm:ss.fff
        /// </summary>
        public string LastUpdate;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="UniqueID"/> string as a <see cref="Guid"/>.
        /// </summary>
        /// <returns><see cref="UniqueID"/> as a <see cref="Guid"/>.</returns>
        public Guid ParseSignalID()
        {
            Guid id;
            Guid.TryParse(UniqueID, out id);
            return id;
        }

        /// <summary>
        /// Calculates a CRC-32 based check-sum on this <see cref="MetadataRecord"/> instance.
        /// </summary>
        /// <returns>CRC-32 based check-sum on this <see cref="MetadataRecord"/> instance.</returns>
        public long CalculateChecksum()
        {
            Crc32 checksum = new Crc32();

            checksum.Update(BitConverter.GetBytes(ID));
            checksum.Update(Encoding.Default.GetBytes(UniqueID ?? ""));
            checksum.Update(Encoding.Default.GetBytes(PointTag ?? ""));
            checksum.Update(Encoding.Default.GetBytes(Source ?? ""));
            checksum.Update(Encoding.Default.GetBytes(Device ?? ""));
            checksum.Update(BitConverter.GetBytes(Longitude));
            checksum.Update(BitConverter.GetBytes(Latitude));
            checksum.Update(Encoding.Default.GetBytes(Protocol ?? ""));
            checksum.Update(Encoding.Default.GetBytes(SignalType ?? ""));
            checksum.Update(Encoding.Default.GetBytes(EngineeringUnits ?? ""));
            checksum.Update(Encoding.Default.GetBytes(PhasorType ?? ""));
            checksum.Update(Encoding.Default.GetBytes(Phase ?? ""));
            checksum.Update(Encoding.Default.GetBytes(Description ?? ""));
            checksum.Update(Encoding.Default.GetBytes(LastUpdate ?? ""));

            return checksum.Value;
        }

        #endregion
    }
}
