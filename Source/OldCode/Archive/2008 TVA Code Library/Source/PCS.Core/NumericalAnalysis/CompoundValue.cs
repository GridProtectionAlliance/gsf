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
    public class CompoundValue<T> : Collection<AssignedValue<T>>
    {
        #region [ Members ]

        // Fields
        private bool m_allAssigned;

        #endregion

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
        /// <param name="count">Total number of composite values to track.</param>
        public CompoundValue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Add(new AssignedValue<T>());
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
                if (m_allAssigned)
                {
                    return true;
                }
                else
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

                    if (allAssigned)
                        m_allAssigned = true;

                    return allAssigned;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Inserts an element into the <see cref="CompoundValue{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, AssignedValue<T> item)
        {
            // Subscribe to item's changed event
            item.Changed += OnValueChanged;

            // Maintain state of all assigned flag
            OnValueChanged(item, EventArgs.Empty);

            // Add item to base class
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the element at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, AssignedValue<T> item)
        {
            // See if user is assigning a new item
            if (!object.ReferenceEquals(item, this[index]))
            {
                // Unsubscribe from old item's changed event
                this[index].Changed -= OnValueChanged;

                // Subscribe to new item's changed event
                item.Changed += OnValueChanged;

                // Maintain state of all assigned flag
                OnValueChanged(item, EventArgs.Empty);

                // Assign new value to base class
                base.SetItem(index, item);
            }
        }

        /// <summary>
        /// Removes the element at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            // Unsubscribe from item's changed event
            this[index].Changed -= OnValueChanged;

            // Remove item from base class
            base.RemoveItem(index);
        }

        /// <summary>
        /// Removes all elements from the <see cref="CompoundValue{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (AssignedValue<T> item in this)
            {
                // Unsubscribe from item's changed event
                item.Changed -= OnValueChanged;
            }

            m_allAssigned = false;

            // Clear items from base class
            base.ClearItems();
        }

        // AssignedValue<T>.Changed event handler
        private void OnValueChanged(object sender, EventArgs e)
        {
            // Maintain state of all assigned flag
            if (m_allAssigned || Count == 0)
            {
                AssignedValue<T> value = sender as AssignedValue<T>;

                if (value != null)
                    m_allAssigned = value.Assigned;
            }
        }

        #endregion
    }
}