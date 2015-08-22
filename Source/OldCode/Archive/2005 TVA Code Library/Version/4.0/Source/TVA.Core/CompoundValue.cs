/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TVA
{
    /// <summary>
    /// Represents a collection of individual values that together represent a compound value once all the values have been assigned.
    /// </summary>
    /// <remarks>
    /// Composite values are stored as <see cref="Nullable{T}"/> and can be cumulated until all values have been assigned so that a
    /// compound value can be created.
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
                return this.All(item => item.HasValue);
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
                return this.All(item => !item.HasValue);
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