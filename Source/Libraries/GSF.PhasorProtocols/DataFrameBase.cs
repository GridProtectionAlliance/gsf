//******************************************************************************************************
//  DataFrameBase.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.TimeSeries;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="IDataFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public abstract class DataFrameBase : ChannelFrameBase<IDataCell>, IDataFrame
    {
        #region [ Members ]

        // Fields
        private IConfigurationFrame m_configurationFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrameBase"/> from specified parameters.
        /// </summary>
        /// <param name="cells">The reference to the collection of cells for this <see cref="DataFrameBase"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="DataFrameBase"/>.</param>
        /// <param name="configurationFrame">The <see cref="IConfigurationFrame"/> associated with this <see cref="DataFrameBase"/>.</param>
        protected DataFrameBase(DataCellCollection cells, Ticks timestamp, IConfigurationFrame configurationFrame)
            : base(0, cells, timestamp)
        {
            m_configurationFrame = configurationFrame;
        }

        /// <summary>
        /// Creates a new <see cref="DataFrameBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data frame
            m_configurationFrame = (IConfigurationFrame)info.GetValue("configurationFrame", typeof(IConfigurationFrame));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="DataFrameBase"/>.
        /// </summary>
        public override FundamentalFrameType FrameType => FundamentalFrameType.DataFrame;

        /// <summary>
        /// Gets or sets <see cref="IConfigurationFrame"/> associated with this <see cref="DataFrameBase"/>.
        /// </summary>
        public virtual IConfigurationFrame ConfigurationFrame
        {
            get => m_configurationFrame;
            set => m_configurationFrame = value;
        }

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="DataFrameBase"/>.
        /// </summary>
        public new virtual DataCellCollection Cells => base.Cells as DataCellCollection;

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="DataFrameBase"/>.
        /// </summary>
        public new virtual IDataFrameParsingState State
        {
            get => base.State as IDataFrameParsingState;
            set => base.State = value;
        }

        /// <summary>
        /// Gets or sets protocol specific quality flags for this <see cref="DataFrameBase"/>.
        /// </summary>
        public virtual uint QualityFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the numeric ID code for this <see cref="DataFrameBase"/>.
        /// </summary>
        /// <remarks>
        /// This value is read-only for <see cref="DataFrameBase"/>; assigning a value will throw an exception. Value returned
        /// is the <see cref="IChannelFrame.IDCode"/> of the associated <see cref="ConfigurationFrame"/>.
        /// </remarks>
        /// <exception cref="NotSupportedException">IDCode of a data frame is read-only, change IDCode is associated configuration frame instead.</exception>
        public override ushort IDCode
        {
            get => m_configurationFrame?.IDCode ?? 0;
            set => throw new NotSupportedException("IDCode of a data frame is read-only, change IDCode is associated configuration frame instead");
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden to ensure assignment of configuration frame.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            // Make sure configuration frame gets assigned before parsing begins...
            IDataFrameParsingState state = State;
            IConfigurationFrame configurationFrame = state.ConfigurationFrame;

            if (configurationFrame is null)
                return state.ParsedBinaryLength;

            ConfigurationFrame = configurationFrame;

            // Handle normal parsing
            return base.ParseBinaryImage(buffer, startIndex, length);
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
            info.AddValue("configurationFrame", m_configurationFrame, typeof(IConfigurationFrame));
        }

        // Gets the quality flags of the data frame as a measurement value.
        IMeasurement IDataFrame.GetQualityFlagsMeasurement()
        {
            return new Measurement()
            {
                Timestamp = Timestamp,
                Value = QualityFlags
            };
        }

        #endregion
    }
}