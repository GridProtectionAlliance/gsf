//******************************************************************************************************
//  IMeasurement.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/14/2011 - J. Ritchie Carroll
//       Added received and published timestamps for measurements.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GSF.TimeSeries
{
    /// <summary>
    /// A set of fields that are common to <see cref="IMeasurement"/> but rarely change. 
    /// This will allow them to be quickly transferred from one <see cref="IMeasurement"/> to 
    /// another. This class is Immutable, so any change to these values requires that the class be recreated.
    /// </summary>
    [Serializable]
    public class CommonMeasurementFields
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
        /// Creates a <see cref="CommonMeasurementFields"/>
        /// </summary>
        /// <param name="key">Gets or sets the primary key of this <see cref="IMeasurement"/>.</param>
        /// <param name="tagName">Gets or sets the text based tag name of this <see cref="IMeasurement"/>.</param>
        /// <param name="adder">Defines an offset to add to the <see cref="IMeasurement"/> value.</param>
        /// <param name="multiplier">Defines a multiplicative offset to apply to the <see cref="IMeasurement"/> value.</param>
        /// <param name="measurementValueFilter">Gets or sets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.</param>
        public CommonMeasurementFields(MeasurementKey key, string tagName, double adder, double multiplier, MeasurementValueFilterFunction measurementValueFilter)
        {
            Key = key;
            TagName = tagName;
            Adder = adder;
            Multiplier = multiplier;
            MeasurementValueFilter = measurementValueFilter;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommonMeasurementFields"/> using the provided <see pref="key"/>. All other fields remain the same.
        /// </summary>
        /// <param name="key">the key to set.</param>
        /// <returns></returns>
        public CommonMeasurementFields ChangeKey(MeasurementKey key)
        {
            if (Key == key)
                return this;
            return new CommonMeasurementFields(key, TagName, Adder, Multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommonMeasurementFields"/> using the provided <see pref="adder"/>. All other fields remain the same.
        /// </summary>
        /// <param name="adder">the adder to set.</param>
        /// <returns></returns>
        public CommonMeasurementFields ChangeAdder(double adder)
        {
            if (Adder == adder)
                return this;
            return new CommonMeasurementFields(Key, TagName, adder, Multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommonMeasurementFields"/> using the provided <see pref="adder"/> and <see pref="multiplier"/>. All other fields remain the same.
        /// </summary>
        /// <param name="adder">the adder to set.</param>
        /// <param name="multiplier">the multiplier to set.</param>
        /// <returns></returns>
        public CommonMeasurementFields ChangeAdderMultiplier(double adder, double multiplier)
        {
            if (Adder == adder && Multiplier == multiplier)
                return this;
            return new CommonMeasurementFields(Key, TagName, adder, multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommonMeasurementFields"/> using the provided <see pref="multiplier"/>. All other fields remain the same.
        /// </summary>
        /// <param name="multiplier">the multiplier to set.</param>
        /// <returns></returns>
        public CommonMeasurementFields ChangeMultiplier(double multiplier)
        {
            if (Multiplier == multiplier)
                return this;
            return new CommonMeasurementFields(Key, TagName, Adder, multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommonMeasurementFields"/> using the provided <see pref="tagName"/>. All other fields remain the same.
        /// </summary>
        /// <param name="tagName">the tagName to set.</param>
        /// <returns></returns>
        public CommonMeasurementFields ChangeTagName(string tagName)
        {
            if (TagName == tagName)
                return this;
            return new CommonMeasurementFields(Key, tagName, Adder, Multiplier, MeasurementValueFilter);
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommonMeasurementFields"/> using the provided <see pref="measurementValueFilter"/>. All other fields remain the same.
        /// </summary>
        /// <param name="measurementValueFilter">the measurementValueFilter to set.</param>
        /// <returns></returns>
        public CommonMeasurementFields ChangeMeasurementValueFilter(MeasurementValueFilterFunction measurementValueFilter)
        {
            if (ReferenceEquals(MeasurementValueFilter, measurementValueFilter))
                return this;
            return new CommonMeasurementFields(Key, TagName, Adder, Multiplier, measurementValueFilter);
        }

        /// <summary>
        /// Represents an undefined <see cref="CommonMeasurementFields"/>.
        /// </summary>
        public static readonly CommonMeasurementFields Undefined = new CommonMeasurementFields(MeasurementKey.Undefined, null, 0, 1, null);
    }

    /// <summary>
    /// Method signature for function used to apply a value filter over a sequence of <see cref="IMeasurement"/> values.
    /// </summary>
    /// <param name="source">Sequence of <see cref="IMeasurement"/> values over which to apply filter.</param>
    /// <returns>Result of filter applied to sequence of <see cref="IMeasurement"/> values.</returns>
    public delegate double MeasurementValueFilterFunction(IEnumerable<IMeasurement> source);

    #region [ Enumerations ]

    /// <summary>
    /// Measurement state flags.
    /// </summary>
    [Flags]
    public enum MeasurementStateFlags : uint
    {
        /// <summary>
        /// Defines normal state.
        /// </summary>
        Normal = (uint)Bits.Nil,
        /// <summary>
        /// Defines bad data state.
        /// </summary>
        BadData = (uint)Bits.Bit00,
        /// <summary>
        /// Defines suspect data state.
        /// </summary>
        SuspectData = (uint)Bits.Bit01,
        /// <summary>
        /// Defines over range error, i.e., unreasonable high value.
        /// </summary>
        OverRangeError = (uint)Bits.Bit02,
        /// <summary>
        /// Defines under range error, i.e., unreasonable low value.
        /// </summary>
        UnderRangeError = (uint)Bits.Bit03,
        /// <summary>
        /// Defines alarm for high value.
        /// </summary>
        AlarmHigh = (uint)Bits.Bit04,
        /// <summary>
        /// Defines alarm for low value.
        /// </summary>
        AlarmLow = (uint)Bits.Bit05,
        /// <summary>
        /// Defines warning for high value.
        /// </summary>
        WarningHigh = (uint)Bits.Bit06,
        /// <summary>
        /// Defines warning for low value.
        /// </summary>
        WarningLow = (uint)Bits.Bit07,
        /// <summary>
        /// Defines alarm for flat-lined value, i.e., latched value test alarm.
        /// </summary>
        FlatlineAlarm = (uint)Bits.Bit08,
        /// <summary>
        /// Defines comparison alarm, i.e., outside threshold of comparison with a real-time value.
        /// </summary>
        ComparisonAlarm = (uint)Bits.Bit09,
        /// <summary>
        /// Defines rate-of-change alarm.
        /// </summary>
        ROCAlarm = (uint)Bits.Bit10,
        /// <summary>
        /// Defines bad value received.
        /// </summary>
        ReceivedAsBad = (uint)Bits.Bit11,
        /// <summary>
        /// Defines calculated value state.
        /// </summary>
        CalcuatedValue = (uint)Bits.Bit12,
        /// <summary>
        /// Defines calculation error with the value.
        /// </summary>
        CalculationError = (uint)Bits.Bit13,
        /// <summary>
        /// Defines calculation warning with the value.
        /// </summary>
        CalculationWarning = (uint)Bits.Bit14,
        /// <summary>
        /// Defines reserved quality flag.
        /// </summary>
        ReservedQualityFlag = (uint)Bits.Bit15,
        /// <summary>
        /// Defines bad time state.
        /// </summary>
        BadTime = (uint)Bits.Bit16,
        /// <summary>
        /// Defines suspect time state.
        /// </summary>
        SuspectTime = (uint)Bits.Bit17,
        /// <summary>
        /// Defines late time alarm.
        /// </summary>
        LateTimeAlarm = (uint)Bits.Bit18,
        /// <summary>
        /// Defines future time alarm.
        /// </summary>
        FutureTimeAlarm = (uint)Bits.Bit19,
        /// <summary>
        /// Defines up-sampled state.
        /// </summary>
        UpSampled = (uint)Bits.Bit20,
        /// <summary>
        /// Defines down-sampled state.
        /// </summary>
        DownSampled = (uint)Bits.Bit21,
        /// <summary>
        /// Defines discarded value state.
        /// </summary>
        DiscardedValue = (uint)Bits.Bit22,
        /// <summary>
        /// Defines reserved time flag.
        /// </summary>
        ReservedTimeFlag = (uint)Bits.Bit23,
        /// <summary>
        /// Defines user defined flag 1.
        /// </summary>
        UserDefinedFlag1 = (uint)Bits.Bit24,
        /// <summary>
        /// Defines user defined flag 2.
        /// </summary>
        UserDefinedFlag2 = (uint)Bits.Bit25,
        /// <summary>
        /// Defines user defined flag 3.
        /// </summary>
        UserDefinedFlag3 = (uint)Bits.Bit26,
        /// <summary>
        /// Defines user defined flag 4.
        /// </summary>
        UserDefinedFlag4 = (uint)Bits.Bit27,
        /// <summary>
        /// Defines user defined flag 5.
        /// </summary>
        UserDefinedFlag5 = (uint)Bits.Bit28,
        /// <summary>
        /// Defines system error state.
        /// </summary>
        SystemError = (uint)Bits.Bit29,
        /// <summary>
        /// Defines system warning state.
        /// </summary>
        SystemWarning = (uint)Bits.Bit30,
        /// <summary>
        /// Defines measurement error flag.
        /// </summary>
        MeasurementError = (uint)Bits.Bit31
    }

    #endregion

    /// <summary>
    /// Represents an interface for an abstract measurement value measured by a device at an exact time.
    /// </summary>
    /// <remarks>
    /// This interface abstractly represents a measured value at an exact time interval.
    /// </remarks>
    public interface IMeasurement : ITimeSeriesValue<double>, IEquatable<ITimeSeriesValue>, IComparable<ITimeSeriesValue>, IComparable
    {
        /// <summary>
        /// Gets or sets the primary key of this <see cref="IMeasurement"/>.
        /// </summary>
        MeasurementKey Key
        {
            get;
            //set;
        }

        /// <summary>
        /// Gets or sets the text based tag name of this <see cref="IMeasurement"/>.
        /// </summary>
        string TagName
        {
            get;
            //set;
        }

        /// <summary>
        /// Gets the adjusted numeric value of this <see cref="IMeasurement"/>, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// <para>Implementers need to account for <see cref="Adder"/> and <see cref="Multiplier"/> in return value, e.g.:<br/>
        /// <c>return <see cref="ITimeSeriesValue{T}.Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>
        /// </para>
        /// </remarks>
        double AdjustedValue
        {
            get;
        }

        /// <summary>
        /// Defines an offset to add to the <see cref="IMeasurement"/> value.
        /// </summary>
        /// <remarks>
        /// Implementers should make sure this value defaults to zero.
        /// </remarks>
        double Adder
        {
            get;
            //set;
        }

        /// <summary>
        /// Defines a multiplicative offset to apply to the <see cref="IMeasurement"/> value.
        /// </summary>
        /// <remarks>
        /// Implementers should make sure this value defaults to one.
        /// </remarks>
        double Multiplier
        {
            get;
            //set;
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="IMeasurement"/>.
        /// </summary>
        MeasurementStateFlags StateFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of when this <see cref="IMeasurement"/> was received (i.e., created).
        /// </summary>
        /// <remarks>
        /// <para>Implementers should set this timestamp to be the ticks of <see cref="DateTime.UtcNow"/> of when this class was created.</para>
        /// <para>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.</para>
        /// </remarks>
        Ticks ReceivedTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of when this <see cref="IMeasurement"/> was published (post-processing).
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        Ticks PublishedTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        MeasurementValueFilterFunction MeasurementValueFilter
        {
            get;
            //set;
        }

        /// <summary>
        /// Contains common fields that rarely change in <see cref="IMeasurement"/> 
        /// so they can be quickly initialized when creating a new <see cref="IMeasurement"/>.
        /// </summary>
        CommonMeasurementFields CommonMeasurementFields
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Defines static extension functions for <see cref="IMeasurement"/> implementations.
    /// </summary>
    /// <remarks>
    /// These helper functions map to the previously defined corresponding properties to help with the transition of <see cref="MeasurementStateFlags"/>.
    /// </remarks>
    public static class IMeasurementExtensions
    {
        private static readonly ConcurrentDictionary<string, int> s_signalTypeIDs = new ConcurrentDictionary<string, int>();
        private static readonly ConcurrentDictionary<Guid, int> s_signalTypeIDCache = new ConcurrentDictionary<Guid, int>();

        /// <summary>
        /// Returns <c>true</c> if <see cref="MeasurementStateFlags.BadData"/> is not set.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to test.</param>
        /// <returns><c>true</c> if <see cref="MeasurementStateFlags.BadData"/> is not set.</returns>
        public static bool ValueQualityIsGood(this IMeasurement measurement)
        {
            return (measurement.StateFlags & MeasurementStateFlags.BadData) == 0;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="MeasurementStateFlags.BadTime"/> is not set.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to test.</param>
        /// <returns><c>true</c> if <see cref="MeasurementStateFlags.BadTime"/> is not set.</returns>
        public static bool TimestampQualityIsGood(this IMeasurement measurement)
        {
            return (measurement.StateFlags & MeasurementStateFlags.BadTime) == 0;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="MeasurementStateFlags.DiscardedValue"/> is set.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to test.</param>
        /// <returns><c>true</c> if <see cref="MeasurementStateFlags.DiscardedValue"/> is not set.</returns>
        public static bool IsDiscarded(this IMeasurement measurement)
        {
            return (measurement.StateFlags & MeasurementStateFlags.DiscardedValue) > 0;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="MeasurementStateFlags.CalcuatedValue"/> is set.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to test.</param>
        /// <returns><c>true</c> if <see cref="MeasurementStateFlags.CalcuatedValue"/> is not set.</returns>
        public static bool IsCalculated(this IMeasurement measurement)
        {
            return (measurement.StateFlags & MeasurementStateFlags.CalcuatedValue) > 0;
        }

        /// <summary>
        /// Returns the measurement ID if defined, otherwise the run-time signal ID associated with the measurement key.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to test.</param>
        /// <returns>Measurement ID if defined, otherwise the run-time signal ID associated with the measurement key.</returns>
        public static Guid RuntimeSignalID(this IMeasurement measurement)
        {
            return measurement.ID;
        }

        /// <summary>
        /// Returns the <see cref="MeasurementKey"/> values of a <see cref="IMeasurement"/> enumeration.
        /// </summary>
        /// <param name="measurements"><see cref="IMeasurement"/> enumeration to convert.</param>
        /// <returns><see cref="MeasurementKey"/> values of the <see cref="IMeasurement"/> enumeration.</returns>
        public static MeasurementKey[] MeasurementKeys(this IEnumerable<IMeasurement> measurements)
        {
            if ((object)measurements == null)
                return new MeasurementKey[0];

            return measurements.Select(m => m.Key).ToArray();
        }

        /// <summary>
        /// Gets a unique (run-time only) signal type ID for the given <paramref name="measurement"/> useful for sorting.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> to obtain signal type for.</param>
        /// <param name="dataSource"><see cref="DataSet"/> that contains measurement metadata.</param>
        /// <returns>Unique (run-time only) signal type ID for the given <paramref name="measurement"/>.</returns>
        public static int GetSignalType(this IMeasurement measurement, DataSet dataSource)
        {
            // This uses string hash code over something like item count + 1 to generate a unique run-time ID of the signal type since chances of a hash code collision
            // over a small set of signal types is much smaller than the possibility of duplicates due to possible race conditions when using item count + 1
            return s_signalTypeIDCache.GetOrAdd(measurement.ID, signalID => s_signalTypeIDs.GetOrAdd(LookupSignalType(signalID, dataSource), signalType => signalType.GetHashCode()));
        }

        // Lookup signal type for given measurement ID
        private static string LookupSignalType(Guid signalID, DataSet dataSource)
        {
            try
            {
                DataRow[] filteredRows = dataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID = '{0}'", signalID.ToString()));
                return filteredRows.Length > 0 ? filteredRows[0]["SignalType"].ToString().ToUpper().Trim() : "NONE";
            }
            catch
            {
                return "NONE";
            }
        }

        /// <summary>
        /// Sets the tag name
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="tagName"></param>
        public static void SetTagName(this IMeasurement measurement, string tagName)
        {
            measurement.CommonMeasurementFields = measurement.CommonMeasurementFields.ChangeTagName(tagName);
        }

        /// <summary>
        /// Sets the Key
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="key"></param>
        public static void SetKey(this IMeasurement measurement, MeasurementKey key)
        {
            measurement.CommonMeasurementFields = measurement.CommonMeasurementFields.ChangeKey(key);
        }

        /// <summary>
        /// Sets the Adder
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="adder"></param>
        public static void SetAdder(this IMeasurement measurement, double adder)
        {
            measurement.CommonMeasurementFields = measurement.CommonMeasurementFields.ChangeAdder(adder);
        }

        /// <summary>
        /// Sets the multiplier
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="multiplier"></param>
        public static void SetMultiplier(this IMeasurement measurement, double multiplier)
        {
            measurement.CommonMeasurementFields = measurement.CommonMeasurementFields.ChangeMultiplier(multiplier);
        }

    }
}