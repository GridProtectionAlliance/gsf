//******************************************************************************************************
//  Record.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/30/2012 - Mehulbhai Thakkar, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSF.PQDIF.Physical
{
    #region [ Enumerations ]

    /// <summary>
    /// Enumeration that defines type of record as per header definition.
    /// </summary>
    public enum RecordType
    {
        /// <summary>
        /// Represents a record level tag which identifies the container record.
        /// Always the first one in the file, and only one per file.
        /// </summary>
        Container,

        /// <summary>
        /// Represents a record level tag which identifies the data source record.
        /// Instrument level information.
        /// </summary>
        DataSource,

        /// <summary>
        /// Represents an optional record level tag which identifies configuration parameters.
        /// Used to capture configuration changes on the data source.
        /// </summary>
        MonitorSettings,

        /// <summary>
        /// Represents a record-level tag which identifies an observation.
        /// Used to capture an event, measurements etc.
        /// </summary>
        Observation,

        /// <summary>
        /// Represents a record-level tag which identifies a blank record.
        /// </summary>
        Blank,

        /// <summary>
        /// Represents a record-level tag which is unknown to this library.
        /// </summary>
        Unknown
    }

    #endregion

    /// <summary>
    /// Represents a record in a PQDIF file. This class
    /// exposes the physical structure of the record.
    /// </summary>
    public class Record
    {
        #region [ Members ]

        // Fields
        private RecordHeader m_header;
        private RecordBody m_body;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new instance of the <see cref="Record"/> class.
        /// </summary>
        public Record()
            : this(new RecordHeader(), new RecordBody())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Record"/> class.
        /// </summary>
        /// <param name="recordTypeTag">The tag which identifies the type of the record.</param>
        public Record(Guid recordTypeTag)
            : this(new RecordHeader(recordTypeTag), new RecordBody(recordTypeTag))
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="Record"/> class.
        /// </summary>
        /// <param name="header">The record header.</param>
        /// <param name="body">The record body.</param>
        public Record(RecordHeader header, RecordBody body)
        {
            Header = header;
            Body = body;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the record header.
        /// </summary>
        public RecordHeader Header
        {
            get
            {
                return m_header;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException("value");

                m_header = value;
            }
        }

        /// <summary>
        /// Gets or sets the record body.
        /// </summary>
        public RecordBody Body
        {
            get
            {
                return m_body;
            }
            set
            {
                m_body = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a string representation of the record.
        /// </summary>
        /// <returns>A string representation of the record.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(m_header);
            builder.AppendLine();
            builder.AppendLine();
            builder.Append(m_body);

            return builder.ToString();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Dictionary<Guid, RecordType> RecordTypeTagMap = CreateRecordTypeTagMap();

        // Static Properties

        /// <summary>
        /// The standard signature used for all PQDIF records.
        /// </summary>
        public static Guid Signature { get; } = new Guid("4a111440-e49f-11cf-9900-505144494600");

        // Static Methods

        /// <summary>
        /// Gets the <see cref="RecordType"/> identified by the given tag.
        /// </summary>
        /// <param name="recordTypeTag">The tag of the <see cref="RecordType"/>.</param>
        /// <returns>The <see cref="RecordType"/> identified by the given tag.</returns>
        public static RecordType GetRecordType(Guid recordTypeTag)
        {
            RecordType type;

            if (RecordTypeTagMap.TryGetValue(recordTypeTag, out type))
                return type;

            return RecordType.Unknown;
        }

        /// <summary>
        /// Gets the globally unique identifier used to identify the given record type.
        /// </summary>
        /// <param name="recordType">The record type to search for.</param>
        /// <returns>The globally unique identifier used to identify the given record type.</returns>
        public static Guid GetTypeAsTag(RecordType recordType)
        {   
            return RecordTypeTagMap.First(pair => pair.Value == recordType).Key;
        }

        // Creates the dictionary mapping tags to their record type.
        private static Dictionary<Guid, RecordType> CreateRecordTypeTagMap()
        {
            return new Dictionary<Guid, RecordType>
            {
                { new Guid("89738606-f1c3-11cf-9d89-0080c72e70a3"), RecordType.Container },
                { new Guid("89738619-f1c3-11cf-9d89-0080c72e70a3"), RecordType.DataSource },
                { new Guid("b48d858c-f5f5-11cf-9d89-0080c72e70a3"), RecordType.MonitorSettings },
                { new Guid("8973861a-f1c3-11cf-9d89-0080c72e70a3"), RecordType.Observation },
                { new Guid("89738618-f1c3-11cf-9d89-0080c72e70a3"), RecordType.Blank }
            };
        }

        #endregion
    }
}
