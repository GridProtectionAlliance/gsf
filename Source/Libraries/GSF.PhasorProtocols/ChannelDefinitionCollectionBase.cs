//******************************************************************************************************
//  ChannelDefinitionCollectionBase.cs - Gbtc
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
//  3/7/2005 - J. Ritchie Carroll
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

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IChannelDefinition"/> objects.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannelDefinition"/> type that the <see cref="ChannelDefinitionCollectionBase{T}"/> contains.</typeparam>
    [Serializable]
    public abstract class ChannelDefinitionCollectionBase<T> : ChannelCollectionBase<T> where T : IChannelDefinition
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionCollectionBase{T}"/> using specified <paramref name="lastValidIndex"/>.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <param name="fixedElementSize">Flag that indicates if collections elements have a fixed size.</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="short.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        protected ChannelDefinitionCollectionBase(int lastValidIndex, bool fixedElementSize = true)
            : base(lastValidIndex, fixedElementSize)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionCollectionBase{T}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelDefinitionCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Inserts an element into the <see cref="ChannelDefinitionCollectionBase{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            // We make sure IChannelDefinition items added to their parent collection know what their index is
            item.Index = index;
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        protected override void SetItem(int index, T item)
        {
            // We make sure new IChannelDefinition item knows what its index is
            item.Index = index;
            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="ChannelDefinitionCollectionBase{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            // Removing an item from the collection would reorder items in the collection from the
            // specified index forward. Given how the phasor protocols are being used, removing
            // an item would be very rare as most collection counts are known in advance...
            for (int i = index; i < Count; i++)
                 this[i].Index = i;
        }

        #endregion
    }
}