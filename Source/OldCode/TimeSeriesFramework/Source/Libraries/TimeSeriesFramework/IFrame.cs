//******************************************************************************************************
//  IFrame.cs - Gbtc
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
    /// Abstract frame interface representing a collection of measurements at an exact moment in time.
    /// </summary>
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
        /// Gets or sets total number of measurements that have been sorted into this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// If this property has not been assigned a value, implementors should return measurement count.
        /// </remarks>
        int SortedMeasurements { get; set; }

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