//*******************************************************************************************************
//  IMeasurement.cs
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
//  12/8/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/17/2008 - James R Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;

namespace PCS.Measurements
{
    /// <summary>Abstract measurement interface representing a value measured by a device at an extact time.</summary>
    /// <remarks>This interface abstractly represents a measured value at an exact time interval.</remarks>
    public interface IMeasurement : IEquatable<IMeasurement>, IComparable<IMeasurement>, IComparable
    {
        /// <summary>Gets or sets the numeric ID of this measurement.</summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to Source, typically creates the primary key for a measurement.</para>
        /// </remarks>
        int ID { get; set; }

        /// <summary>Gets or sets the source of this measurement.</summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to ID, typically creates the primary key for a measurement.</para>
        /// <para>This value is typically used to track the archive name in which measurement is stored.</para>
        /// </remarks>
        string Source { get; set; }

        /// <summary>Returns the primary key of this measurement.</summary>
        MeasurementKey Key { get; }

        /// <summary>Gets or sets the text based tag name of this measurement.</summary>
        string TagName { get; set; }

        /// <summary>Gets or sets the raw value of this measurement (i.e., the numeric value that is not offset by adder and multiplier).</summary>
        double Value { get; set; }

        /// <summary>Gets the adjusted numeric value of this measurement, taking into account the specified adder and multiplier offsets.</summary>
        /// <remarks>
        /// <para>Implementors need to account for adder and multiplier in return value, e.g.:</para>
        /// <code>return Value * Multiplier + Adder</code>
        /// </remarks>
        double AdjustedValue { get; }

        /// <summary>Defines an offset to add to the measurement value.</summary>
        /// <remarks>Implementors should make sure this value defaults to zero.</remarks>
        [DefaultValue(0.0)]
        double Adder { get; set; }

        /// <summary>Defines a mulplicative offset to apply to the measurement value.</summary>
        /// <remarks>Implementors should make sure this value defaults to one.</remarks>
        [DefaultValue(1.0)]
        double Multiplier { get; set; }

        /// <summary>Gets or sets exact timestamp, in ticks, of the data represented by this measurement.</summary>
        /// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.</remarks>
        Ticks Timestamp { get; set; }

        /// <summary>Gets or sets a boolean value determining if the quality of the numeric value of this measurement is good.</summary>
        bool ValueQualityIsGood { get; set; }

        /// <summary>Gets or sets a boolean value determining if the quality of the timestamp of this measurement is good.</summary>
        bool TimestampQualityIsGood { get; set; }
    }
}