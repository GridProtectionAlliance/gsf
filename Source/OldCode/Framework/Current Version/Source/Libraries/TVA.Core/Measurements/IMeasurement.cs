//*******************************************************************************************************
//  IMeasurement.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/8/2005 - James R. Carroll
//       Generated original version of source code.
//  09/17/2008 - James R. Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Measurements
{
    /// <summary>
    /// Represents an interface for an abstract measurement value measured by a device at an extact time.
    /// </summary>
    /// <remarks>
    /// This interface abstractly represents a measured value at an exact time interval.
    /// </remarks>
    [CLSCompliant(false)]
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
        /// Gets or sets a boolean value determining if the quality of the numeric value of this <see cref="IMeasurement"/> is good.
        /// </summary>
        bool ValueQualityIsGood { get; set; }

        /// <summary>
        /// Gets or sets a boolean value determining if the quality of the timestamp of this <see cref="IMeasurement"/> is good.
        /// </summary>
        bool TimestampQualityIsGood { get; set; }
    }
}