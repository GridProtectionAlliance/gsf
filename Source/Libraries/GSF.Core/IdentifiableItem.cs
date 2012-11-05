//******************************************************************************************************
//  IdentifiableItem.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/12/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/08/2008 - J. Ritchie Carroll
//       Converted to C#.
//  11/05/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************

namespace GSF
{
    /// <summary>
    /// Represents an identifiable item.
    /// </summary>
    /// <typeparam name="TId">Type of the identifier to be used for identification.</typeparam>
    /// <typeparam name="TItem">Type of the item that is to be made identifiable.</typeparam>
    public class IdentifiableItem<TId, TItem>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Defines the identifier of the <see cref="Item"/>.
        /// </summary>
        public TId ID;

        /// <summary>
        /// Defines the buffer being made identifiable by its associated <see cref="ID"/>.
        /// </summary>
        public TItem Item;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifiableItem{TId, TItem}"/> class.
        /// </summary>
        public IdentifiableItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifiableItem{TId, TItem}"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <paramref name="item"/>.</param>
        /// <param name="item">The item being associated with the <paramref name="id"/> to make it identifiable.</param>
        public IdentifiableItem(TId id, TItem item)
        {
            ID = id;
            Item = item;
        }

        #endregion
    }
}