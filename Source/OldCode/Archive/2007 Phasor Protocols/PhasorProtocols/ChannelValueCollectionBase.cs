//*******************************************************************************************************
//  ChannelValueCollectionBase.cs
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
//  03/07/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Security.Permissions;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IChannelValue{TDefinition}"/> objects.
    /// </summary>
    /// <typeparam name="TDefinition">Specific <see cref="IChannelDefinition"/> type that the <see cref="IChannelValue{TDefinition}"/> references.</typeparam>
    /// <typeparam name="TValue">Specific <see cref="IChannelValue{TDefinition}"/> type that the <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/> contains.</typeparam>
    [Serializable()]
    public abstract class ChannelValueCollectionBase<TDefinition, TValue> : ChannelCollectionBase<TValue>
        where TDefinition : IChannelDefinition
        where TValue : IChannelValue<TDefinition>
    {
        #region [ Members ]

        // Fields
        private int m_fixedCount;
        private int m_floatCount;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/> using specified <paramref name="lastValidIndex"/>.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="Int16.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        protected ChannelValueCollectionBase(int lastValidIndex)
            : base(lastValidIndex)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelValueCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize extra elements
            m_fixedCount = info.GetInt32("fixedCount");
            m_floatCount = info.GetInt32("floatCount");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the length of the <see cref="ChannelCollectionBase{T}.BinaryImage"/>.
        /// </summary>
        public override int BinaryLength
        {
            get
            {
                if (m_fixedCount == 0 || m_floatCount == 0)
                {
                    // Cells will be constant length, so we can quickly calculate lengths
                    return base.BinaryLength;
                }
                else
                {
                    // Cells will be different lengths, so we must manually sum lengths
                    return this.Sum(frame => frame.BinaryLength);
                }
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if all of the composite values have been assigned a value.
        /// </summary>
        /// <returns><c>true</c>, if all composite values have been assigned a value; otherwise, <c>false</c>.</returns>
        public virtual bool AllValuesAssigned
        {
            get
            {
                return this.All(item => !item.IsEmpty);
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Fixed Count", m_fixedCount.ToString());
                baseAttributes.Add("Float Count", m_floatCount.ToString());
                baseAttributes.Add("All Values Assigned", AllValuesAssigned.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize extra elements
            info.AddValue("fixedCount", m_fixedCount);
            info.AddValue("floatCount", m_floatCount);
        }

        // In typical usage, all channel values will be of the same data type - but we can't anticipate all
        // possible uses of collection, so we track totals of each data type so we can quickly ascertain if
        // all the items in the collection are of the same data type

        /// <summary>
        /// Inserts an element into the <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, TValue item)
        {
            if (item.DataFormat == DataFormat.FixedInteger)
                m_fixedCount++;
            else
                m_floatCount++;

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero.  -or- <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void SetItem(int index, TValue item)
        {
            if (this[index].DataFormat == DataFormat.FixedInteger)
                m_fixedCount--;
            else
                m_floatCount--;

            if (item.DataFormat == DataFormat.FixedInteger)
                m_fixedCount++;
            else
                m_floatCount++;

            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero.  -or- <paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void RemoveItem(int index)
        {
            if (this[index].DataFormat == DataFormat.FixedInteger)
                m_fixedCount--;
            else
                m_floatCount--;

            base.RemoveItem(index);
        }

        /// <summary>
        /// Removes all elements from the <see cref="ChannelValueCollectionBase{TDefinition,TValue}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            m_fixedCount = 0;
            m_floatCount = 0;

            base.ClearItems();
        }

        #endregion
    }
}