//******************************************************************************************************
//  DataFrame.cs - Gbtc
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
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Parsing;

namespace GSF.PhasorProtocols.FNET
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IDataFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class DataFrame : DataFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, int>
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;
        private uint m_sampleIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an F-NET data frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DataFrame()
            : base(new DataCellCollection(), 0, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from specified parameters.
        /// </summary>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="DataFrame"/>.</param>
        /// <param name="configurationFrame">The <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.</param>
        /// <param name="sampleIndex">The sample index of this <see cref="DataFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate a F-NET data frame.
        /// </remarks>
        public DataFrame(Ticks timestamp, ConfigurationFrame configurationFrame, uint sampleIndex)
            : base(new DataCellCollection(), timestamp, configurationFrame)
        {
            m_sampleIndex = sampleIndex;
        }

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data frame
            m_sampleIndex = info.GetUInt32("sampleIndex");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="DataFrame"/>.
        /// </summary>
        public new DataCellCollection Cells => base.Cells as DataCellCollection;

        /// <summary>
        /// Gets or sets <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.
        /// </summary>
        public new ConfigurationFrame ConfigurationFrame
        {
            get => base.ConfigurationFrame as ConfigurationFrame;
            set => base.ConfigurationFrame = value;
        }

        /// <summary>
        /// Gets the identifier that is used to identify the F-NET frame.
        /// </summary>
        public int TypeID =>
            // F-NET only defines a single frame type...
            0;

        /// <summary>
        /// Gets or sets the sample index of this <see cref="DataFrame"/>.
        /// </summary>
        public uint SampleIndex
        {
            get => m_sampleIndex;
            set => m_sampleIndex = value;
        }

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get => m_frameHeader;
            set
            {
                m_frameHeader = value;

                if (!(m_frameHeader is null))
                    State = m_frameHeader.State as IDataFrameParsingState;
            }
        }

        // This interface implementation satisfies ISupportFrameImage<int>.CommonHeader
        ICommonHeader<int> ISupportFrameImage<int>.CommonHeader
        {
            get => CommonHeader;
            set => CommonHeader = value as CommonFrameHeader;
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Sample Index", SampleIndex.ToString());

                return baseAttributes;
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
        /// <param name="buffer">Array of <see cref="Byte"/>s.</param>
        /// <param name="length">An <see cref="Int32"/> value for the bytes to read.</param>
        /// <param name="offset">An <see cref="Int32"/> value for offset to read from.</param>
        /// <returns>An <see cref="UInt16"/> as the checksum.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data frame
            info.AddValue("sampleIndex", m_sampleIndex);
        }

        #endregion
    }
}