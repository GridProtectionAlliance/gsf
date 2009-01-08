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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PCS.NumericalAnalysis
{
    /// <summary>
    /// Represents a collection of individual values that together represent a compound value once all the values have been assigned.
    /// </summary>
    /// <remarks>
    /// Composite values can be cumulated until all values have been assigned so that a compound value can be created.
    /// </remarks>
    /// <typeparam name="T"><see cref="Type"/> of composite values.</typeparam>
    public class CompoundValue<T> : Collection<T?> where T: struct
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
        /// The specified <paramref name="count"/> of items are added to the <see cref="CompoundValue{T}"/>,
        /// each item will be marked as unassigned (i.e., null).
        /// </remarks>
        /// <param name="count">Total number of composite values to track.</param>
        public CompoundValue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Add(null);
            }
        }

        /// <summary>
        /// Creates a new <see cref="CompoundValue{T}"/> from the specified list.
        /// </summary>
        /// <param name="values">List of values used to initialize <see cref="CompoundValue{T}"/>.</param>
        public CompoundValue(IEnumerable<T> values)
            : base(values.ToList().ConvertAll<T?>(item => item))
        {
        }

        /// <summary>
        /// Creates a new <see cref="CompoundValue{T}"/> from the specified list.
        /// </summary>
        /// <param name="values">List of values used to initialize <see cref="CompoundValue{T}"/>.</param>
        public CompoundValue(IEnumerable<T?> values)
            : base(values.ToList())
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a boolean value indicating if all of the composite values have been assigned a value.
        /// </summary>
        /// <returns>True, if all composite values have been assigned a value; otherwise, false.</returns>
        public bool AllAssigned
        {
            get
            {
                return this.All<T?>(item => item.HasValue);
            }
        }

        /// <summary>
        /// Gets a boolean value indicating if none of the composite values have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        public bool NoneAssigned
        {
            get
            {
                return this.All<T?>(item => !item.HasValue);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets an array of all the <see cref="Nullable{T}.GetValueOrDefault()"/> elements of the <see cref="CompoundValue{T}"/>.
        /// </summary>
        /// <returns>A new array containing copies of the <see cref="Nullable{T}.GetValueOrDefault()"/> elements of the <see cref="CompoundValue{T}"/>.</returns>
        public T[] ToArray()
        {
            T[] values = new T[Count];

            for (int i = 0; i < Count; i++)
            {
                values[i] = this[i].GetValueOrDefault();
            }

            return values;
        }

        #endregion
    }
}