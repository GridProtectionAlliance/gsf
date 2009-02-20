//*******************************************************************************************************
//  ChannelDefinitionCollectionBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IChannelDefinition"/> objects.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannelDefintion"/> type that the <see cref="ChannelDefinitionCollectionBase{T}"/> contains.</typeparam>
    [Serializable()]
    public abstract class ChannelDefinitionCollectionBase<T> : ChannelCollectionBase<T> where T : IChannelDefinition
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionCollectionBase{T}"/>.
        /// </summary>
        protected ChannelDefinitionCollectionBase()
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

        /// <summary>
        /// Creates a new <see cref="ChannelDefinitionCollectionBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="Int16.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        protected ChannelDefinitionCollectionBase(int lastValidIndex)
            : base(lastValidIndex)
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
            {
                this[i].Index = i;
            }
        }

        #endregion
    }
}