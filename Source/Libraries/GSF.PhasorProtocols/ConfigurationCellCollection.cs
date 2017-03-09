//******************************************************************************************************
//  ConfigurationCellCollection.cs - Gbtc
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
//  10/09/2010 - Ritchie
//       Added "TryGetByStationName" lookup function.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Runtime.Serialization;
using GSF.Collections;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IConfigurationCell"/> objects.
    /// </summary>
    [Serializable]
    public class ConfigurationCellCollection : ChannelCellCollectionBase<IConfigurationCell>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/> from specified parameters.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <param name="constantCellLength">Sets flag that determines if the lengths of <see cref="IConfigurationCell"/> elements in this <see cref="ConfigurationCellCollection"/> are constant.</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="short.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        public ConfigurationCellCollection(int lastValidIndex, bool constantCellLength)
            : base(lastValidIndex, constantCellLength)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to get <see cref="IConfigurationCell"/> with the specified station name.
        /// </summary>
        /// <param name="stationName">The <see cref="IConfigurationCell.StationName"/> to find.</param>
        /// <param name="configurationCell">
        /// When this method returns, contains the <see cref="IConfigurationCell"/> with the specified <paramref name="stationName"/>, if found;
        /// otherwise, null is returned.
        /// </param>
        /// <returns><c>true</c> if the <see cref="ConfigurationCellCollection"/> contains an element with the specified <paramref name="stationName"/>; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetByStationName(string stationName, out IConfigurationCell configurationCell)
        {
            configurationCell = this.FirstOrDefault(cell => stationName.TruncateRight(cell.MaximumStationNameLength).Equals(cell.StationName, StringComparison.OrdinalIgnoreCase));
            return (object)configurationCell != null;
        }

        /// <summary>
        /// Attempts to get <see cref="IConfigurationCell"/> with the specified label.
        /// </summary>
        /// <param name="label">The <see cref="IConfigurationCell.IDLabel"/> to find.</param>
        /// <param name="configurationCell">
        /// When this method returns, contains the <see cref="IConfigurationCell"/> with the specified <paramref name="label"/>, if found;
        /// otherwise, null is returned.
        /// </param>
        /// <returns><c>true</c> if the <see cref="ConfigurationCellCollection"/> contains an element with the specified <paramref name="label"/>; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetByIDLabel(string label, out IConfigurationCell configurationCell)
        {
            configurationCell = this.FirstOrDefault(cell => label.Equals(cell.IDLabel.TruncateRight(cell.IDLabelLength), StringComparison.OrdinalIgnoreCase));
            return (object)configurationCell != null;
        }

        /// <summary>
        /// Attempts to get <see cref="IConfigurationCell"/> with the specified ID code.
        /// </summary>
        /// <param name="idCode">The <see cref="IChannelCell.IDCode"/> to find.</param>
        /// <param name="configurationCell">
        /// When this method returns, contains the <see cref="IConfigurationCell"/> with the specified <paramref name="idCode"/>, if found;
        /// otherwise, null is returned.
        /// </param>
        /// <returns><c>true</c> if the <see cref="ConfigurationCellCollection"/> contains an element with the specified <paramref name="idCode"/>; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetByIDCode(ushort idCode, out IConfigurationCell configurationCell)
        {
            configurationCell = this.FirstOrDefault(cell => cell.IDCode == idCode);
            return (object)configurationCell != null;
        }

        /// <summary>
        /// Attempts to find the index of the <see cref="IConfigurationCell"/> with the specified ID label.
        /// </summary>
        /// <param name="label">The <see cref="IConfigurationCell.IDLabel"/> to find.</param>
        /// <returns>Index of the <see cref="ConfigurationCellCollection"/> that contains the specified <paramref name="label"/>; otherwise, <c>-1</c>.</returns>
        public virtual int IndexOfIDLabel(string label)
        {
            return this.IndexOf(cell => label.Equals(cell.IDLabel.TruncateRight(cell.IDLabelLength), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Attempts to find the index of the <see cref="IConfigurationCell"/> with the specified station name.
        /// </summary>
        /// <param name="stationName">The <see cref="IConfigurationCell.StationName"/> to find.</param>
        /// <returns>Index of the <see cref="ConfigurationCellCollection"/> that contains the specified <paramref name="stationName"/>; otherwise, <c>-1</c>.</returns>
        public virtual int IndexOfStationName(string stationName)
        {
            return this.IndexOf(cell => stationName.TruncateRight(cell.MaximumStationNameLength).Equals(cell.StationName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Attempts to find the index of the <see cref="IConfigurationCell"/> with the specified ID code.
        /// </summary>
        /// <param name="idCode">The <see cref="IChannelCell.IDCode"/> to find.</param>
        /// <returns>Index of the <see cref="ConfigurationCellCollection"/> that contains the specified <paramref name="idCode"/>; otherwise, <c>-1</c>.</returns>
        public virtual int IndexOfIDCode(ushort idCode)
        {
            return this.IndexOf(cell => cell.IDCode == idCode);
        }

        #endregion
    }
}