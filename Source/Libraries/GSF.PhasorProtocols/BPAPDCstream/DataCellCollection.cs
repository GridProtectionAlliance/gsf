//******************************************************************************************************
//  DataCellCollection.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents a BPA PDCstream implementation of a collection of <see cref="IDataCell"/> objects.
    /// </summary>
    [Serializable]
    public class DataCellCollection : PhasorProtocols.DataCellCollection
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellCollection"/>.
        /// </summary>
        public DataCellCollection()
            : base(ushort.MaxValue, false)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataCellCollection"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataCell"/> at specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of value to get or set.</param>
        public new DataCell this[int index]
        {
            get => base[index] as DataCell;
            set => base[index] = value;
        }

        #endregion
    }
}