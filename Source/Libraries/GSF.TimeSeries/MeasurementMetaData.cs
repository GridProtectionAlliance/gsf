//******************************************************************************************************
//  CommonMeasurementFields.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/18/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a set of meta-data fields for <see cref="IMeasurement"/> that should rarely change.
    /// </summary>
    /// <remarks>
    /// This class allows measurement meta-data to be quickly transferred from one <see cref="IMeasurement"/> to 
    /// another. This class is immutable, so any change to these values requires that the class be recreated.
    /// </remarks>
    [Serializable]
    public class MeasurementMetadata
    {
        /// <summary>
        /// Gets or sets the primary key of this <see cref="IMeasurement"/>.
        /// </summary>
        public readonly MeasurementKey Key;

        /// <summary>
        /// Gets or sets the text based tag name of this <see cref="IMeasurement"/>.
        /// </summary>
        public readonly string TagName;

        /// <summary>
        /// Defines an offset to add to the <see cref="IMeasurement"/> value.
        /// </summary>
        /// <remarks>
        /// Implementers should make sure this value defaults to zero.
        /// </remarks>
        public readonly double Adder;

        /// <summary>
        /// Defines a multiplicative offset to apply to the <see cref="IMeasurement"/> value.
        /// </summary>
        /// <remarks>
        /// Implementers should make sure this value defaults to one.
        /// </remarks>
        public readonly double Multiplier;

        /// <summary>
        /// Gets or sets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        public readonly MeasurementValueFilterFunction MeasurementValueFilter;

        /// <summary>
        /// Creates a <see cref="MeasurementMetadata"/>
        /// </summary>
        /// <param name="key">Gets or sets the primary key of this <see cref="IMeasurement"/>.</param>
        /// <param name="tagName">Gets or sets the text based tag name of this <see cref="IMeasurement"/>.</param>
        /// <param name="adder">Defines an offset to add to the <see cref="IMeasurement"/> value.</param>
        /// <param name="multiplier">Defines a multiplicative offset to apply to the <see cref="IMeasurement"/> value.</param>
        /// <param name="measurementValueFilter">Gets or sets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.</param>
        public MeasurementMetadata(MeasurementKey key, string tagName, double adder, double multiplier, MeasurementValueFilterFunction measurementValueFilter)
        {
            Key = key;
            TagName = tagName;
            Adder = adder;
            Multiplier = multiplier;
            MeasurementValueFilter = measurementValueFilter;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeasurementMetadata"/> using the provided measurement <paramref name="key"/>. All other fields remain the same.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <returns>New instance of <see cref="MeasurementMetadata"/> using the provided measurement <paramref name="key"/>.</returns>
        public MeasurementMetadata ChangeKey(MeasurementKey key)
        {
            if (Key == key)
                return this;

            return new MeasurementMetadata(key, TagName, Adder, Multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="adder"/>. All other fields remain the same.
        /// </summary>
        /// <param name="adder">The adder to set.</param>
        /// <returns>New instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="adder"/>.</returns>
        public MeasurementMetadata ChangeAdder(double adder)
        {
            if (Adder == adder)
                return this;

            return new MeasurementMetadata(Key, TagName, adder, Multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="adder"/> and <paramref name="multiplier"/>. All other fields remain the same.
        /// </summary>
        /// <param name="adder">The adder to set.</param>
        /// <param name="multiplier">The multiplier to set.</param>
        /// <returns>New instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="adder"/> and <paramref name="multiplier"/>.</returns>
        public MeasurementMetadata ChangeAdderMultiplier(double adder, double multiplier)
        {
            if (Adder == adder && Multiplier == multiplier)
                return this;

            return new MeasurementMetadata(Key, TagName, adder, multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="multiplier"/>. All other fields remain the same.
        /// </summary>
        /// <param name="multiplier">The multiplier to set.</param>
        /// <returns>New instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="multiplier"/>.</returns>
        public MeasurementMetadata ChangeMultiplier(double multiplier)
        {
            if (Multiplier == multiplier)
                return this;

            return new MeasurementMetadata(Key, TagName, Adder, multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="tagName"/>. All other fields remain the same.
        /// </summary>
        /// <param name="tagName">The tag name to set.</param>
        /// <returns>New instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="tagName"/>.</returns>
        public MeasurementMetadata ChangeTagName(string tagName)
        {
            if (TagName == tagName)
                return this;

            return new MeasurementMetadata(Key, tagName, Adder, Multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="measurementValueFilter"/>. All other fields remain the same.
        /// </summary>
        /// <param name="measurementValueFilter">the measurementValueFilter to set.</param>
        /// <returns>New instance of <see cref="MeasurementMetadata"/> using the provided <paramref name="measurementValueFilter"/>.</returns>
        public MeasurementMetadata ChangeMeasurementValueFilter(MeasurementValueFilterFunction measurementValueFilter)
        {
            if (ReferenceEquals(MeasurementValueFilter, measurementValueFilter))
                return this;

            return new MeasurementMetadata(Key, TagName, Adder, Multiplier, measurementValueFilter);
        }

        /// <summary>
        /// Represents an undefined <see cref="MeasurementMetadata"/>.
        /// </summary>
        public static readonly MeasurementMetadata Undefined = new MeasurementMetadata(MeasurementKey.Undefined, null, 0, 1, null);
    }
}