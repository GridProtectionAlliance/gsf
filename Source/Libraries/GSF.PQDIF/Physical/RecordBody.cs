//******************************************************************************************************
//  RecordBody.cs - Gbtc
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
//  04/30/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.PQDIF.Physical
{
    /// <summary>
    /// The body of a PQDIF <see cref="Record"/>. The body is part of the
    /// physical structure of a PQDIF file. It laid out as a hierarchy of
    /// <see cref="Element"/>s, starting with a
    /// <see cref="CollectionElement"/> that contains other elements.
    /// </summary>
    public class RecordBody
    {
        #region [ Members ]

        // Fields
        private CollectionElement m_collection;
        private uint m_checksum;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RecordBody"/> class.
        /// </summary>
        public RecordBody()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RecordBody"/> class.
        /// </summary>
        /// <param name="rootTag">Tag of the collection root element of the record.</param>
        public RecordBody(Guid rootTag)
        {
            m_collection = new CollectionElement();
            m_collection.TagOfElement = rootTag;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="CollectionElement"/> at the top
        /// of the hierarchy. This can be used to traverse the hierarchy.
        /// </summary>
        public CollectionElement Collection
        {
            get
            {
                return m_collection ?? (m_collection = new CollectionElement());
            }
            set
            {
                m_collection = value;
            }
        }

        /// <summary>
        /// Gets or sets the CRC32 checksum of the record body.
        /// </summary>
        public uint Checksum
        {
            get
            {
                return m_checksum;
            }
            set
            {
                m_checksum = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a string representation of the record body.
        /// </summary>
        /// <returns>A string representation of the record body.</returns>
        public override string ToString()
        {
            return m_collection.ToString();
        }

        #endregion

    }
}
