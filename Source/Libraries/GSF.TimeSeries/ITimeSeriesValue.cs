//******************************************************************************************************
//  ITimeSeriesValue.cs - Gbtc
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
//  06/29/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents the fundamental binary value interface for a time-series value.
    /// </summary>
    public interface ITimeSeriesValue
    {
        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="ITimeSeriesValue{T}"/>.
        /// </summary>
        /// <remarks>
        /// This is the fundamental identifier of the <see cref="ITimeSeriesValue{T}"/>.
        /// </remarks>
        Guid ID { get; }

        /// <summary>
        /// Gets or sets the raw value of this <see cref="ITimeSeriesValue{T}"/>.
        /// </summary>
        BigBinaryValue Value { get; set; }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ITimeSeriesValue{T}"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        Ticks Timestamp { get; set; }

        /// <summary>
        /// Get the hash code for the <see cref="ITimeSeriesValue.ID"/>.
        /// </summary>
        /// <returns>Hash code for the <see cref="ITimeSeriesValue.ID"/>.</returns>
        /// <remarks>Implementers should always return the hash code based on <see cref="ITimeSeriesValue.ID"/> of measurement.</remarks>
        int GetHashCode();
    }

    /// <summary>
    /// Represents the fundamental typed interface for a time-series value.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the time-series value.</typeparam>
    public interface ITimeSeriesValue<T> : ITimeSeriesValue
    {
        /// <summary>
        /// Gets or sets the raw typed value of this <see cref="ITimeSeriesValue{T}"/>.
        /// </summary>
        new T Value { get; set; }
    }
}
