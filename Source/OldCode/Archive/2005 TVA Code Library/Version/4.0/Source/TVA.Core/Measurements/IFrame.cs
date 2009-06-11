//*******************************************************************************************************
//  IFrame.cs
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
//  12/08/2005 - J. Ritchie Carroll
//      Initial version of source generated
//  09/16/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Measurements
{
    /// <summary>
    /// Abstract frame interface representing a collection of measurements at an exact moment in time.
    /// </summary>
    [CLSCompliant(false)]
    public interface IFrame : IEquatable<IFrame>, IComparable<IFrame>, IComparable
    {
        /// <summary>
        /// Keyed measurements in this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// Represents a dictionary of measurements, keyed by <see cref="MeasurementKey"/>.
        /// </remarks>
        IDictionary<MeasurementKey, IMeasurement> Measurements { get; }

        /// <summary>
        /// Gets or sets published state of this <see cref="IFrame"/>.
        /// </summary>
        bool Published { get; set; }

        /// <summary>
        /// Gets or sets total number of measurements that have been published for this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// If this property has not been assigned a value, implementors should return measurement count.
        /// </remarks>
        int PublishedMeasurements { get; set; }

        /// <summary>
        /// Gets or sets exact timestamp, in <see cref="Ticks"/>, of the data represented in this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        Ticks Timestamp { get; set; }

        /// <summary>
        /// Gets ot sets reference to last <see cref="IMeasurement"/> that was sorted into this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// <para>This value is used to help monitor slow moving measurements that are being sorted into the <see cref="IFrame"/>.</para>
        /// <para>Implementors need only track the value.</para>
        /// </remarks>
        IMeasurement LastSortedMeasurement { get; set; }
    }
}