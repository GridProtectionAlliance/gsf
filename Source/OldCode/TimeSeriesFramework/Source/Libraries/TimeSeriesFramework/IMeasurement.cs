//******************************************************************************************************
//  IMeasurement.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Method signature for function used to apply a value filter over a sequence of <see cref="IMeasurement"/> values.
    /// </summary>
    /// <param name="source">Sequence of <see cref="IMeasurement"/> values over which to apply filter.</param>
    /// <returns>Result of filter applied to sequence of <see cref="IMeasurement"/> values.</returns>
    public delegate double MeasurementValueFilterFunction(IEnumerable<IMeasurement> source);

    /// <summary>
    /// Represents an interface for an abstract measurement value measured by a device at an exact time.
    /// </summary>
    /// <remarks>
    /// This interface abstractly represents a measured value at an exact time interval.
    /// </remarks>
    public interface IMeasurement : IEquatable<IMeasurement>, IComparable<IMeasurement>, IComparable
    {
        /// <summary>
        /// Gets or sets the numeric ID of this <see cref="IMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to Source, typically creates the primary key for a <see cref="IMeasurement"/>.</para>
        /// </remarks>
        uint ID { get; set; }

        /// <summary>
        /// Gets or sets the source of this <see cref="IMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to ID, typically creates the primary key for a <see cref="IMeasurement"/>.</para>
        /// <para>This value is typically used to track the archive name in which <see cref="IMeasurement"/> is stored.</para>
        /// </remarks>
        string Source { get; set; }

        /// <summary>
        /// Returns the primary key of this <see cref="IMeasurement"/>.
        /// </summary>
        MeasurementKey Key { get; }

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="IMeasurement"/>, if available.
        /// </summary>
        Guid SignalID { get; set; }

        /// <summary>
        /// Gets or sets the text based tag name of this <see cref="IMeasurement"/>.
        /// </summary>
        string TagName { get; set; }

        /// <summary>
        /// Gets or sets the raw value of this <see cref="IMeasurement"/> (i.e., the numeric value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Gets the adjusted numeric value of this <see cref="IMeasurement"/>, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// <para>Implementors need to account for <see cref="Adder"/> and <see cref="Multiplier"/> in return value, e.g.:<br/>
        /// <c>return <see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>
        /// </para>
        /// </remarks>
        double AdjustedValue { get; }

        /// <summary>
        /// Defines an offset to add to the <see cref="IMeasurement"/> value.
        /// </summary>
        /// <remarks>
        /// Implementors should make sure this value defaults to zero.
        /// </remarks>
        double Adder { get; set; }

        /// <summary>
        /// Defines a mulplicative offset to apply to the <see cref="IMeasurement"/> value.
        /// </summary>
        /// <remarks>
        /// Implementors should make sure this value defaults to one.
        /// </remarks>
        double Multiplier { get; set; }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="IMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        Ticks Timestamp { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that determines if the quality of the numeric value of this <see cref="IMeasurement"/> is good.
        /// </summary>
        bool ValueQualityIsGood { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that determines if the quality of the timestamp of this <see cref="IMeasurement"/> is good.
        /// </summary>
        bool TimestampQualityIsGood { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that determines if this <see cref="IMeasurement"/> has been discarded during sorting.
        /// </summary>
        bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets function used to apply a downsampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        MeasurementValueFilterFunction MeasurementValueFilter { get; set; }

        /// <summary>
        /// Get the hash code for the <see cref="IMeasurement"/>.<see cref="MeasurementKey"/>.
        /// </summary>
        /// <returns>Hash code for the <see cref="IMeasurement"/>.<see cref="MeasurementKey"/>.</returns>
        /// <remarks>Implementors should always return the hash code based on <see cref="MeasurementKey"/> of measurement.</remarks>
        int GetHashCode();
    }
}