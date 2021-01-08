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
//  04/30/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents a Macrodyne implementation of a collection of <see cref="IConfigurationCell"/> objects.
    /// </summary>
    [Serializable]
    public class ConfigurationCellCollection : PhasorProtocols.ConfigurationCellCollection
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/>.
        /// </summary>
        public ConfigurationCellCollection()
            : base(0, true)
        {
            // Macrodyne only supports a single device - so there should only be one cell - since there's only one cell, cell lengths will be constant :)
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/> with alternate maximum cell count.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="short.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        public ConfigurationCellCollection(int lastValidIndex)
            : base(lastValidIndex, false)
        {
            // For devices defined in INI collection we allow as many devices as needed
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

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="ConfigurationCell"/> at specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of value to get or set.</param>
        public new ConfigurationCell this[int index]
        {
            get => base[index] as ConfigurationCell;
            set => base[index] = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to retrieve a <see cref="ConfigurationCell"/> from this <see cref="ConfigurationCellCollection"/> with the specified <paramref name="sectionEntry"/>.
        /// </summary>
        /// <param name="sectionEntry"><see cref="ConfigurationCell.SectionEntry"/> value to try to find.</param>
        /// <param name="configurationCell"><see cref="ConfigurationCell"/> with the specified <paramref name="sectionEntry"/> if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if <see cref="ConfigurationCell"/> with the specified <paramref name="sectionEntry"/> is found; otherwise <c>false</c>.</returns>
        public bool TryGetBySectionEntry(string sectionEntry, ref ConfigurationCell configurationCell)
        {
            for (int i = 0; i < Count; i++)
            {
                configurationCell = this[i];
                if (string.Compare(configurationCell.SectionEntry, sectionEntry, true) == 0)
                    return true;
            }

            configurationCell = null;
            return false;
        }

        #endregion
    }
}