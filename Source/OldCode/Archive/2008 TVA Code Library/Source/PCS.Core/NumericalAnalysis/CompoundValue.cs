//*******************************************************************************************************
//  CompoundValue.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (PCS.Shared.Math).
//  08/23/2007 - Darrell Zuercher
//       Edited code comments.
//  09/18/2008 - J. Ritchie Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.ObjectModel;

namespace PCS.NumericalAnalysis
{
    /// <summary>
    /// Represents a collection of individual values that together represent a compound value once all the values have been assigned.
    /// </summary>
    /// <remarks>
    /// Composite values can be cumulated until all values have been assigned so that a compound value can be created.
    /// </remarks>
    /// <typeparam name="T"><see cref="Type"/> of composite values.</typeparam>
    public class CompoundValue<T> : Collection<Assignable<T>>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CompoundValue{T}"/>.
        /// </summary>
        public CompoundValue()
        {
        }

        /// <summary>
        /// Creates a new <see cref="CompoundValue{T}"/> specifing the total number of composite values to track.
        /// </summary>
        /// <remarks>
        /// The specified <paramref name="count"/> of items are added to the <see cref="CompoundValue{T}"/>, each
        /// item will have a default value (e.g., null or zero) and will be marked as unassigned.
        /// </remarks>
        /// <param name="count">Total number of composite values to track.</param>
        public CompoundValue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Add(new Assignable<T>());
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a boolean value indicating if all composite values have been assigned a value.
        /// </summary>
        /// <returns>True, if all composite values have been assigned a value; otherwise, false.</returns>
        public bool AllAssigned
        {
            get
            {
                bool allAssigned = true;

                for (int x = 0; x < Count; x++)
                {
                    if (!this[x].Assigned)
                    {
                        allAssigned = false;
                        break;
                    }
                }

                return allAssigned;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a new <see cref="Assignable{T}"/> composite value to the <see cref="CompoundValue{T}"/> with the specified
        /// <paramref name="value"/> and <c><see cref="Assignable{T}.Assigned"/> = true</c>.
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            Add(new Assignable<T>(value));
        }

        /// <summary>
        /// Gets an array of all the <see cref="Assignable{T}.Value"/> elements of the <see cref="CompoundValue{T}"/>.
        /// </summary>
        /// <returns>A new array containing copies of the <see cref="Assignable{T}.Value"/> elements of the <see cref="CompoundValue{T}"/>.</returns>
        public T[] GetCompositeValues()
        {
            T[] values = new T[Count];

            for (int i = 0; i < Count; i++)
            {
                values[i] = this[i].Value;
            }

            return values;
        }

        #endregion

        #region [ Possible Future Optimization ]

        // The following code would aggressively try to maintain the "all assigned" state and would be useful in cases
        // where the user was managing a large set of compound values.  However it comes with the price of subscribing
        // to each item's "Changed" event which hence forces the implementation of IDisposable to account for proper
        // unsubscription of item "Changed" events. Perhaps a dervied class with this functionality would be better so
        // that this class could be kept lightweight and disposeless. The following code compiled at writing.

        //private bool m_allAssigned;
        //private bool m_disposed;

        ///// <summary>
        ///// Gets a boolean value indicating if all composite values have been assigned a value.
        ///// </summary>
        ///// <returns>True, if all composite values have been assigned a value; otherwise, false.</returns>
        //public bool AllAssigned
        //{
        //    get
        //    {
        //        if (m_allAssigned)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            bool allAssigned = true;

        //            for (int x = 0; x < Count; x++)
        //            {
        //                if (!this[x].Assigned)
        //                {
        //                    allAssigned = false;
        //                    break;
        //                }
        //            }

        //            if (allAssigned)
        //                m_allAssigned = true;

        //            return allAssigned;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Inserts an element into the <see cref="CompoundValue{T}"/> at the specified index.
        ///// </summary>
        ///// <param name="index">The zero-based index at which item should be inserted.</param>
        ///// <param name="item">The object to insert.</param>
        //protected override void InsertItem(int index, Assignable<T> item)
        //{
        //    // Subscribe to item's changed event
        //    item.Changed += OnValueChanged;

        //    // Maintain state of all assigned flag
        //    OnValueChanged(item, EventArgs.Empty);

        //    // Add item to base class
        //    base.InsertItem(index, item);
        //}

        ///// <summary>
        ///// Replaces the element at the specified <paramref name="index"/>.
        ///// </summary>
        ///// <param name="index">The zero-based index of the element to replace.</param>
        ///// <param name="item">The new value for the element at the specified index.</param>
        //protected override void SetItem(int index, Assignable<T> item)
        //{
        //    // See if user is assigning a new item
        //    if (!object.ReferenceEquals(item, this[index]))
        //    {
        //        // Unsubscribe from old item's changed event
        //        this[index].Changed -= OnValueChanged;

        //        // Subscribe to new item's changed event
        //        item.Changed += OnValueChanged;

        //        // Maintain state of all assigned flag
        //        OnValueChanged(item, EventArgs.Empty);

        //        // Assign new value to base class
        //        base.SetItem(index, item);
        //    }
        //}

        ///// <summary>
        ///// Removes the element at the specified <paramref name="index"/>.
        ///// </summary>
        ///// <param name="index">The zero-based index of the element to remove.</param>
        //protected override void RemoveItem(int index)
        //{
        //    // Unsubscribe from item's changed event
        //    this[index].Changed -= OnValueChanged;

        //    // Remove item from base class
        //    base.RemoveItem(index);
        //}

        ///// <summary>
        ///// Removes all elements from the <see cref="CompoundValue{T}"/>.
        ///// </summary>
        //protected override void ClearItems()
        //{
        //    foreach (Assignable<T> item in this)
        //    {
        //        // Unsubscribe from item's changed event
        //        item.Changed -= OnValueChanged;
        //    }

        //    m_allAssigned = false;

        //    // Clear items from base class
        //    base.ClearItems();
        //}

        //// AssignedValue<T>.Changed event handler
        //private void OnValueChanged(object sender, EventArgs e)
        //{
        //    // Maintain state of all assigned flag
        //    if (m_allAssigned || Count == 0)
        //    {
        //        Assignable<T> value = sender as Assignable<T>;

        //        if (value != null)
        //            m_allAssigned = value.Assigned;
        //    }
        //}

        ///// <summary>
        ///// Releases the unmanaged resources before the <see cref="CompoundValue"/> object is reclaimed by <see cref="GC"/>.
        ///// </summary>
        //~CompoundValue()
        //{
        //    Dispose(false);
        //}

        ///// <summary>
        ///// Releases all the resources used by the <see cref="CompoundValue"/> object.
        ///// </summary>
        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        ///// <summary>
        ///// Releases the unmanaged resources used by the <see cref="CompoundValue"/> object and optionally releases the managed resources.
        ///// </summary>
        ///// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!m_disposed)
        //    {
        //        try
        //        {
        //            // We dispose of this class to make sure we unsubscribe from item changed events
        //            if (disposing)
        //                ClearItems();
        //        }
        //        finally
        //        {
        //            m_disposed = true;  // Prevent duplicate dispose.
        //        }
        //    }
        //}

        #endregion
    }
}