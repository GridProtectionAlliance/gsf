//******************************************************************************************************
//  GrafanaDataSourceBase.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/22/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.NumericalAnalysis;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Web;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable AccessToModifiedClosure
namespace GrafanaAdapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines available functions that can operate on <see cref="TimeSeriesValues"/>.
    /// </summary>
    public enum SeriesFunction
    {
        /// <summary>
        /// Returns a single value that represents the mean of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Average(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Average, Avg, Mean<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        Average,
        /// <summary>
        /// Returns a single value that is the minimum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Minimum(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Minimum, Min<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        Minimum,
        /// <summary>
        /// Returns a single value that is the maximum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Maximum(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Maximum, Max<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        Maximum,
        /// <summary>
        /// Returns a single value that represents the sum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Total(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Total, Sum<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        Total,
        /// <summary>
        /// Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Range(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Range<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        Range,
        /// <summary>
        /// Returns a single value that is the count of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Count(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
        /// Variants: Count<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        Count,
        /// <summary>
        /// Returns a series of values that represent the unique set of values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Distinct(expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Distinct, Unique<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Distinct,
        /// <summary>
        /// Returns a series of values that represent the absolute value each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>AbsoluteValue(expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: AbsoluteValue, Abs<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        AbsoluteValue,
        /// <summary>
        /// Returns a series of values that represent each of the values in the source series added with N.
        /// N is a floating point value representing an additive offset to be applied to each value the source series.
        /// N can either be constant value or a named target available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Add(N, expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Add(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: Add<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Add,
        /// <summary>
        /// Returns a series of values that represent each of the values in the source series subtracted by N.
        /// N is a floating point value representing an subtractive offset to be applied to each value the source series.
        /// N can either be constant value or a named target available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Subtract(N, expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Subtract(2.2, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: Subtract<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Subtract,
        /// <summary>
        /// Returns a series of values that represent each of the values in the source series multiplied by N.
        /// N is a floating point value representing a multiplicative factor to be applied to each value the source series.
        /// N can either be constant value or a named target available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Multiply(N, expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Multiply(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: Multiply<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Multiply,
        /// <summary>
        /// Returns a series of values that represent each of the values in the source series divided by N.
        /// N is a floating point value representing a divisive factor to be applied to each value the source series.
        /// N can either be constant value or a named target available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Divide(N, expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Divide(1.732, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: Divide<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Divide,
        /// <summary>
        /// Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
        /// N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Round([N = 0], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Round<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Round,
        /// <summary>
        /// Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Floor(expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Floor<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Floor,
        /// <summary>
        /// Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Ceiling(expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Ceiling, Ceil<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Ceiling,
        /// <summary>
        /// Returns a series of values that represent the integral part of each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Truncate(expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Truncate, Trunc<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Truncate,
        /// <summary>
        /// Returns a single value that represents the standard deviation of the values in the source series. First parameter,
        /// optional, is a boolean flag representing if the sample based calculation should be used - defaults to false, which
        /// means the population based calculation should be used.
        /// </summary>
        /// <remarks>
        /// Signature: <c>StandardDeviation([useSampleCalc = false], expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: StandardDeviation, StdDev<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        StandardDeviation,
        /// <summary>
        /// Returns a single value that represents the median of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Median(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')</c><br/>
        /// Variants: Median, Med, Mid<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Median,
        /// <summary>
        /// Returns a single value that represents the mode of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Mode(expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
        /// Variants: Mode<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Mode,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are the largest in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
        /// N can either be constant value or a named target available from the expression. Any target values that fall between 0
        /// and 1 will be treated as a percentage.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Top(N|N%, [normalizeTime = true], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Top, Largest<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Top,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are the smallest in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
        /// N can either be constant value or a named target available from the expression. Any target values that fall between 0
        /// and 1 will be treated as a percentage.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Bottom(N|N%, [normalizeTime = true], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Bottom(100, false, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Bottom, Bot, Smallest<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Bottom,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are a random sample of the values in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
        /// N can either be constant value or a named target available from the expression. Any target values that fall between 0
        /// and 1 will be treated as a percentage.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Random(N|N%, [normalizeTime = true], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: Random, Rand, Sample<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Random,
        /// <summary>
        /// Returns a series of N, or N% of total, values from the start of the source series.
        /// N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
        /// N can either be constant value or a named target available from the expression. Any target values that fall between 0
        /// and 1 will be treated as a percentage.
        /// </summary>
        /// <remarks>
        /// Signature: <c>First([N|N% = 1], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: First<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        First,
        /// <summary>
        /// Returns a series of N, or N% of total, values from the end of the source series.
        /// N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
        /// N can either be constant value or a named target available from the expression. Any target values that fall between 0
        /// and 1 will be treated as a percentage.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Last([N|N% = 1], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Last<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Last,
        /// <summary>
        /// Returns a single value that represents the Nth order percentile for the sorted values in the source series.
        /// N is a floating point value, representing a percentage, that must range from 0 to 100.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Percentile(N[%], expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: Percentile, Pctl<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Percentile,
        /// <summary>
        /// Returns a series of values that represent the difference between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Difference(expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Difference, Diff<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Difference,
        /// <summary>
        /// Returns a series of values that represent the time difference, in time units, between consecutive values in the source series. The units
        /// parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds,
        /// Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or
        /// AtomicUnitsOfTime - defaults to Seconds.
        /// </summary>
        /// <remarks>
        /// Signature: <c>TimeDifference([units = Seconds], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: TimeDifference, TimeDiff, Elapsed<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        TimeDifference,
        /// <summary>
        /// Returns a series of values that represent the rate of change, per time units, for the difference between consecutive values in the source
        /// series. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds,
        /// Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals),
        /// PlanckTime or AtomicUnitsOfTime - defaults to Seconds.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Derivative([units = Seconds], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Derivative, Der<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Derivative,
        /// <summary>
        /// Returns a single value that represents the time-based integration, i.e., the sum of <c>V(n) * (T(n) - T(n-1))</c> where time difference is
        /// calculated in the specified time units, of the values in the source series. The units parameter, optional, specifies the type of time units
        /// and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional
        /// Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Hours.
        /// </summary>
        /// <remarks>
        /// Signature: <c>TimeIntegration([units = Hours], expression)</c><br/>
        /// Returns: Single value.<br/>
        /// Example: <c>TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')</c><br/>
        /// Variants: TimeIntegration, TimeInt<br/>
        /// Execution: Immediate enumeration.
        /// </remarks>
        TimeIntegration,
        /// <summary>
        /// Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in time units.
        /// N is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in time units, for the returned
        /// data. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds,
        /// Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals),
        /// PlanckTime or AtomicUnitsOfTime - defaults to Seconds. Setting N value to zero will request non-decimated, full resolution data from the data
        /// source. A zero N value will always produce the most accurate aggregation calculation results but will increase query burden for large time ranges.
        /// N can either be constant value or a named target available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Interval(N, [units = Seconds], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Sum(Interval(0, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))</c><br/>
        /// Variants: Interval<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Interval,
        /// <summary>
        /// Returns a series of values that represent a filtered set of the values in the source series where each value falls between the specified low and high.
        /// The low and high parameter values are floating-point numbers that represent the range of values allowed in the return series. Third parameter, optional,
        /// is a boolean flag that determines if range values are inclusive, i.e., allowed values are &gt;= low and &lt;= high - defaults to false, which means
        /// values are exclusive, i.e., allowed values are &gt; low and &lt; high. Function allows a fourth optional parameter that is a boolean flag - when four
        /// parameters are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive.
        /// The low and high parameter values can either be constant values or named targets available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>IncludeRange(low, high, [inclusive = false], expression)</c> -or- <c>IncludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>IncludeRange(59.90, 60.10, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: IncludeRange, Include<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        IncludeRange,
        /// <summary>
        /// Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high.
        /// The low and high parameter values are floating-point numbers that represent the range of values excluded in the return series. Third parameter, optional,
        /// is a boolean flag that determines if range values are inclusive, i.e., excluded values are &lt;= low or &gt;= high - defaults to false, which means
        /// values are exclusive, i.e., excluded values are &lt; low or &gt; high. Function allows a fourth optional parameter that is a boolean flag - when four
        /// parameters are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive.
        /// The low and high parameter values can either be constant values or named targets available from the expression.
        /// </summary>
        /// <remarks>
        /// Signature: <c>ExcludeRange(low, high, [inclusive = false], expression)</c> -or- <c>ExcludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>ExcludeRange(-180.0, 180.0, true, false, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHA')</c><br/>
        /// Variants: ExcludeRange, Exclude<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        ExcludeRange,
        /// <summary>
        /// Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.
        /// First parameter, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.
        /// </summary>
        /// <remarks>
        /// Signature: <c>FilterNaN([alsoFilterInfinity = true], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: FilterNaN<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>        
        FilterNaN,
        /// <summary>
        /// Returns a series of values that represent an adjusted set of angles that are unwrapped, per specified angle units, so that a comparable mathematical
        /// operation can be executed. For example, for angles that wrap between -180 and +180 degrees, this algorithm unwraps the values to make the values
        /// mathematically comparable. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads,
        /// ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.
        /// </summary>
        /// <remarks>
        /// Signature: <c>UnwrapAngle([units = Degrees], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>UnwrapAngle(FSX_PMU2-PA1:VH; REA_PMU3-PA2:VH)</c><br/>
        /// Variants: UnwrapAngle, Unwrap<br/>
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        UnwrapAngle,
        /// <summary>
        /// Returns a series of values that represent an adjusted set of angles that are wrapped, per specified angle units, so that angle values are consistently
        /// between -180 and +180 degrees. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians,
        /// Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.
        /// </summary>
        /// <remarks>
        /// Signature: <c>WrapAngle([units = Degrees], expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>WrapAngle(Radians, FILTER TOP 5 ActiveMeasurements WHERE SignalType LIKE '%PHA')</c><br/>
        /// Variants: WrapAngle, Wrap<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        WrapAngle,
        /// <summary>
        /// Renames a series with the specified label value. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three
        /// series in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3". Group operations on this
        /// function will be ignored. The label parameter also supports substitutions when root target metadata can be resolved. For series values that directly
        /// map to a point tag, metadata value substitutions for the tag can be used in the label value - for example: {ID}, {SignalID}, {PointTag}, {AlternateTag},
        /// {SignalReference}, {Device}, {FramesPerSecond}, {Protocol}, {ProtocolType}, {SignalType}, {EngineeringUnits}, {PhasorType}, {Company}, {Description} -
        /// where applicable, these substitutions can be used in any combination.
        /// </summary>
        /// <remarks>
        /// Signature: <c>Label(value, expression)</c><br/>
        /// Returns: Series of values.<br/>
        /// Example: <c>Label('AvgFreq', SetAvg(FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))</c><br/>
        /// Variants: Label, Name<br/>
        /// Execution: Deferred enumeration.
        /// </remarks>
        Label,
        /// <summary>
        /// Not a recognized function.
        /// </summary>
        None
    }

    /// <summary>
    /// Group series operations.
    /// </summary>
    public enum GroupOperation
    {
        /// <summary>
        /// Operates on each series end-to-end.
        /// </summary>
        Set,
        /// <summary>
        /// Operates on each series as a group per time-slice.
        /// </summary>
        Slice,
        /// <summary>
        /// Performs no group operation on the series set.
        /// </summary>
        None
    }

    #endregion

    /// <summary>
    /// Represents a base implementation for Grafana data sources.
    /// </summary>
    [Serializable]
    public abstract class GrafanaDataSourceBase
    {
        #region [ Members ]

        // Nested Types
        private class TargetTimeUnit
        {
            public TimeUnit Unit;

            public double Factor = double.NaN;

            public static bool TryParse(string value, out TargetTimeUnit targetTimeUnit)
            {
                TimeUnit timeUnit;

                if (Enum.TryParse(value, out timeUnit))
                {
                    targetTimeUnit = new TargetTimeUnit
                    {
                        Unit = timeUnit
                    };

                    return true;
                }

                switch (value?.ToLowerInvariant())
                {
                    case "milliseconds":
                        targetTimeUnit = new TargetTimeUnit
                        {
                            Unit = TimeUnit.Seconds,
                            Factor = SI.Milli
                        };

                        return true;
                    case "microseconds":
                        targetTimeUnit = new TargetTimeUnit
                        {
                            Unit = TimeUnit.Seconds,
                            Factor = SI.Micro
                        };

                        return true;
                    case "nanoseconds":
                        targetTimeUnit = new TargetTimeUnit
                        {
                            Unit = TimeUnit.Seconds,
                            Factor = SI.Nano
                        };

                        return true;
                }

                targetTimeUnit = null;
                return false;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets instance name for this <see cref="GrafanaDataSourceBase"/> implementation.
        /// </summary>
        public virtual string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based meta-data source available to this <see cref="GrafanaDataSourceBase"/> implementation.
        /// </summary>
        public virtual DataSet Metadata { get; set; }

        /// <summary>
        /// Gets or sets maximum number of search targets to return during a search query.
        /// </summary>
        public int MaximumSearchTargetsPerRequest { get; set; } = 200;

        /// <summary>
        /// Gets or sets maximum number of annotations to return during an annotations query.
        /// </summary>
        public int MaximumAnnotationsPerRequest { get; set; } = 100;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Search data source for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public virtual Task<string[]> Search(Target request)
        {
            string target = request.target == "select metric" ? "" : request.target;

            return Task.Factory.StartNew(() =>
            {
                return TargetCache<string[]>.GetOrAdd($"search!{target}", () => Metadata.Tables["ActiveMeasurements"].Select($"ID LIKE '{InstanceName}:%' AND PointTag LIKE '%{target}%'").Take(MaximumSearchTargetsPerRequest).Select(row => $"{row["PointTag"]}").ToArray());
            });
        }

        /// <summary>
        /// Search data source for a list of columns from a specific table.
        /// </summary>
        /// <param name="request">Table Name.</param>
        public virtual Task<string[]> SearchFields(Target request)
        {
            return Task.Factory.StartNew(() =>
            {
                return TargetCache<string[]>.GetOrAdd($"search!fields!{request.target}", () => Metadata.Tables[request.target].Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray());
            });
        }

        /// <summary>
        /// Search data source for a list of tables.
        /// </summary>
        /// <param name="request">Request - ignored.</param>
        public virtual Task<string[]> SearchFilters(Target request)
        {
            return Task.Factory.StartNew(() =>
            {
                // Any table that includes columns for ID, SignalID, PointTag, Adder and Multipler can be used as measurement sources for filter expressions
                return TargetCache<string[]>.GetOrAdd("search!filters!{63F7E9F6B334}", () => Metadata.Tables.Cast<DataTable>().Where(table => new[] { "ID", "SignalID", "PointTag", "Adder", "Multiplier" }.All(fieldName => table.Columns.Contains(fieldName))).Select(table => table.TableName).ToArray());
            });
        }

        /// <summary>
        /// Search data source for a list of columns from a specific table to use for ORDER BY expression.
        /// </summary>
        /// <param name="request">Table Name.</param>
        public virtual Task<string[]> SearchOrderBys(Target request)
        {
            return Task.Factory.StartNew(() =>
            {
                // Result will typically be the same list as SearchFields but allows ability to deviate in case certain fields are not suitable for ORDER BY expression
                return TargetCache<string[]>.GetOrAdd($"search!orderbys!{request.target}", () => Metadata.Tables[request.target].Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray());
            });
        }

        /// <summary>
        /// Queries data source for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Queried annotations from data source.</returns>
        public virtual async Task<List<AnnotationResponse>> Annotations(AnnotationRequest request, CancellationToken cancellationToken)
        {
            bool useFilterExpression;
            AnnotationType type = request.ParseQueryType(out useFilterExpression);
            Dictionary<string, DataRow> definitions = request.ParseSourceDefinitions(type, Metadata, useFilterExpression);
            List<TimeSeriesValues> annotationData = await Query(request.ExtractQueryRequest(definitions.Keys, MaximumAnnotationsPerRequest), cancellationToken);
            List<AnnotationResponse> responses = new List<AnnotationResponse>();

            foreach (TimeSeriesValues values in annotationData)
            {
                DataRow definition;
                string[] parts = values.target.Split(',');
                string target;

                // Remove "Interval(0, {target})" from target if defined
                if (parts.Length > 1)
                {
                    target = parts[1].Trim();
                    target = target.Length > 1 ? target.Substring(0, target.Length - 1).Trim() : parts[0].Trim();
                }
                else
                {
                    target = parts[0].Trim();
                }

                if (definitions.TryGetValue(target, out definition))
                {
                    foreach (double[] datapoint in values.datapoints)
                    {
                        if (type.IsApplicable(datapoint))
                        {
                            AnnotationResponse response = new AnnotationResponse
                            {
                                annotation = request.annotation,
                                time = datapoint[TimeSeriesValues.Time]
                            };

                            type.PopulateResponse(response, target, definition, datapoint, Metadata);

                            responses.Add(response);
                        }
                    }
                }

            }

            return responses;
        }

        /// <summary>
        /// Queries data source returning data as Grafana time-series data set.
        /// </summary>
        /// <param name="request">Query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<List<TimeSeriesValues>> Query(QueryRequest request, CancellationToken cancellationToken)
        {
            // Task allows processing of multiple simultaneous queries
            return Task.Factory.StartNew(() =>
            {
                if (!request.format?.Equals("json", StringComparison.OrdinalIgnoreCase) ?? false)
                    throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

                DateTime startTime = request.range.from.ParseJsonTimestamp();
                DateTime stopTime = request.range.to.ParseJsonTimestamp();

                foreach (Target target in request.targets)
                    target.target = target.target?.Trim() ?? "";

                DataSourceValueGroup[] valueGroups = request.targets.Select(target => QueryTarget(target, target.target, startTime, stopTime, request.interval, true, cancellationToken)).SelectMany(groups => groups).ToArray();

                // Establish result series sequentially so that order remains consistent between calls
                List<TimeSeriesValues> result = valueGroups.Select(valueGroup => new TimeSeriesValues
                {
                    target = valueGroup.Target,
                    rootTarget = valueGroup.RootTarget,
                    latitude = LookupTargetCoordinate(valueGroup.RootTarget, "Latitude"),
                    longitude = LookupTargetCoordinate(valueGroup.RootTarget, "Longitude")
                }).ToList();

                // Process series data in parallel
                Parallel.ForEach(result, new ParallelOptions { CancellationToken = cancellationToken }, series =>
                {
                    // For deferred enumerations, any work to be done is left till last moment - in this case "ToList()" invokes actual operation                    
                    DataSourceValueGroup valueGroup = valueGroups.First(group => group.Target.Equals(series.target));
                    IEnumerable<DataSourceValue> values = valueGroup.Source;

                    if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                        values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

                    if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                        values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

                    series.datapoints = values.Select(dataValue => new[] { dataValue.Value, dataValue.Time }).ToList();
                });

                #region [ Original "request.maxDataPoints" Implementation ]

                //int maxDataPoints = (int)(request.maxDataPoints * 1.1D);

                //// Make a final pass through data to decimate returned point volume (for graphing purposes), if needed

                //foreach (TimeSeriesValues series in result)
                //{
                //    if (series.datapoints.Count > maxDataPoints)
                //    {
                //        double indexFactor = series.datapoints.Count / (double)request.maxDataPoints;
                //        series.datapoints = Enumerable.Range(0, request.maxDataPoints).Select(index => series.datapoints[(int)(index * indexFactor)]).ToList();
                //    }
                //}

                #endregion

                return result;
            },
            cancellationToken);
        }

        /// <summary>
        /// Starts a query that will read data source values, given a set of point IDs and targets, over a time range.
        /// </summary>
        /// <param name="startTime">Start-time for query.</param>
        /// <param name="stopTime">Stop-time for query.</param>
        /// <param name="interval">Interval from Grafana request.</param>
        /// <param name="decimate">Flag that determines if data should be decimated over provided time range.</param>
        /// <param name="targetMap">Set of IDs with associated targets to query.</param>
        /// <returns>Queried data source data in terms of value and time.</returns>
        protected abstract IEnumerable<DataSourceValue> QueryDataSourceValues(DateTime startTime, DateTime stopTime, string interval, bool decimate, Dictionary<ulong, string> targetMap);

        private DataRow LookupTargetMetadata(string target)
        {
            return TargetCache<DataRow>.GetOrAdd(target, () =>
            {
                try
                {
                    return Metadata.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'").FirstOrDefault();
                }
                catch
                {
                    return null;
                }                
            });
        }

        private float LookupTargetCoordinate(string target, string field)
        {
            return TargetCache<float>.GetOrAdd($"{target}_{field}", () => LookupTargetMetadata(target)?.ConvertNullableField<float>(field) ?? 0.0F);
        }

        private IEnumerable<DataSourceValueGroup> QueryTarget(Target sourceTarget, string queryExpression, DateTime startTime, DateTime stopTime, string interval, bool decimate, CancellationToken cancellationToken)
        {
            // A single target might look like the following:
            // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))

            HashSet<string> targetSet = new HashSet<string>(new[] { queryExpression }, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing should be ignored
            HashSet<string> reducedTargetSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Match> seriesFunctions = new List<Match>();

            foreach (string target in targetSet)
            {
                // Find any series functions in target
                Match[] matchedFunctions = TargetCache<Match[]>.GetOrAdd(target, () =>
                {
                    lock (s_seriesFunctions)
                        return s_seriesFunctions.Matches(target).Cast<Match>().ToArray();
                });

                if (matchedFunctions.Length > 0)
                {
                    seriesFunctions.AddRange(matchedFunctions);

                    // Reduce target to non-function expressions - important so later split on ';' succeeds properly
                    string reducedTarget = target;

                    foreach (string expression in matchedFunctions.Select(match => match.Value))
                        reducedTarget = reducedTarget.Replace(expression, "");

                    if (!string.IsNullOrWhiteSpace(reducedTarget))
                        reducedTargetSet.Add(reducedTarget);
                }
                else
                {
                    reducedTargetSet.Add(target);
                }
            }

            if (seriesFunctions.Count > 0)
            {
                // Execute series functions
                foreach (Tuple<SeriesFunction, string, GroupOperation> parsedFunction in seriesFunctions.Select(ParseSeriesFunction))
                    foreach (DataSourceValueGroup valueGroup in ExecuteSeriesFunction(sourceTarget, parsedFunction, startTime, stopTime, interval, decimate, cancellationToken))
                        yield return valueGroup;

                // Use reduced target set that excludes any series functions
                targetSet = reducedTargetSet;
            }

            // Query any remaining targets
            if (targetSet.Count > 0)
            {
                // Split remaining targets on semi-colon, this way even multiple filter expressions can be used as inputs to functions
                string[] allTargets = targetSet.Select(target => target.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(currentTargets => currentTargets).ToArray();

                // Expand target set to include point tags for all parsed inputs
                foreach (string target in allTargets)
                    targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () => AdapterBase.ParseInputMeasurementKeys(Metadata, false, target).Select(key => key.TagFromKey(Metadata)).ToArray()));

                Dictionary<ulong, string> targetMap = new Dictionary<ulong, string>();

                // Target set now contains both original expressions and newly parsed individual point tags - to create final point list we
                // are only interested in the point tags, provided either by direct user entry or derived by parsing filter expressions
                foreach (string target in targetSet)
                {
                    // Reduce all targets down to a dictionary of point ID's mapped to point tags
                    MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(Metadata));

                    if (key != MeasurementKey.Undefined)
                        targetMap[key.ID] = target;
                }

                long readCount = 0;

                // Query underlying data source for each target - to prevent parallel read from data source we enumerate immediately
                List<DataSourceValue> dataValues = QueryDataSourceValues(startTime, stopTime, interval, decimate, targetMap)
                    .TakeWhile(dataValue => readCount++ % 10000 != 0 || !cancellationToken.IsCancellationRequested).ToList();

                foreach (KeyValuePair<ulong, string> target in targetMap)
                    yield return new DataSourceValueGroup
                    {
                        Target = target.Value,
                        RootTarget = target.Value,
                        SourceTarget = sourceTarget,
                        Source = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value))
                    };
            }
        }

        private IEnumerable<DataSourceValueGroup> ExecuteSeriesFunction(Target sourceTarget, Tuple<SeriesFunction, string, GroupOperation> parsedFunction, DateTime startTime, DateTime stopTime, string interval, bool decimate, CancellationToken cancellationToken)
        {
            SeriesFunction seriesFunction = parsedFunction.Item1;
            string expression = parsedFunction.Item2;
            GroupOperation groupOperation = parsedFunction.Item3;

            // Parse out function parameters and target expression
            Tuple<string[], string> expressionParameters = TargetCache<Tuple<string[], string>>.GetOrAdd(expression, () =>
            {
                List<string> parsedParameters = new List<string>();

                // Extract any required function parameters
                int requiredParameters = s_requiredParameters[seriesFunction]; // Safe: no lock needed since content doesn't change

                // Any slice operation adds one required parameter for time tolerance
                if (groupOperation == GroupOperation.Slice)
                    requiredParameters++;

                if (requiredParameters > 0)
                {
                    int index = 0;

                    for (int i = 0; i < requiredParameters && index > -1; i++)
                        index = expression.IndexOf(',', index + 1);

                    if (index > -1)
                        parsedParameters.AddRange(expression.Substring(0, index).Split(','));

                    if (parsedParameters.Count == requiredParameters)
                        expression = expression.Substring(index + 1).Trim();
                    else
                        throw new FormatException($"Expected {requiredParameters + 1} parameters, received {parsedParameters.Count + 1} in: {(groupOperation == GroupOperation.None ? "" : groupOperation.ToString())}{seriesFunction}({expression})");
                }

                // Extract any provided optional function parameters
                int optionalParameters = s_optionalParameters[seriesFunction]; // Safe: no lock needed since content doesn't change

                Func<string, bool> hasSubExpression = target => target.StartsWith("FILTER", StringComparison.OrdinalIgnoreCase) || target.Contains("(");

                if (optionalParameters > 0)
                {
                    int index = expression.IndexOf(',');
                    int lastIndex;

                    if (index > -1 && !hasSubExpression(expression.Substring(0, index)))
                    {
                        lastIndex = index;

                        for (int i = 1; i < optionalParameters && index > -1; i++)
                        {
                            index = expression.IndexOf(',', index + 1);

                            if (index > -1 && hasSubExpression(expression.Substring(lastIndex + 1, index - lastIndex - 1).Trim()))
                            {
                                index = lastIndex;
                                break;
                            }

                            lastIndex = index;
                        }

                        if (index > -1)
                        {
                            parsedParameters.AddRange(expression.Substring(0, index).Split(','));
                            expression = expression.Substring(index + 1).Trim();
                        }
                    }
                }

                return new Tuple<string[], string>(parsedParameters.ToArray(), expression);
            });

            string[] parameters = expressionParameters.Item1;
            string queryExpression = expressionParameters.Item2;   // Final function parameter is always target expression

            // When accurate calculation results are requested, query data source at full resolution
            if (seriesFunction == SeriesFunction.Interval && ParseFloat(parameters[0]) == 0.0D)
                decimate = false;

            // Query function expression to get series data
            IEnumerable<DataSourceValueGroup> dataset = QueryTarget(sourceTarget, queryExpression, startTime, stopTime, interval, decimate, cancellationToken);

            // Handle label function as a special edge case - groups operations on label are ignored
            if (seriesFunction == SeriesFunction.Label)
            {
                // Derive label
                string label = parameters[0];

                if (label.StartsWith("\"") || label.StartsWith("'"))
                    label = label.Substring(1, label.Length - 2);

                DataSourceValueGroup[] groups = dataset.ToArray();

                for (int i = 0; i < groups.Length; i++)
                {
                    string target = groups[i].RootTarget;

                    string seriesLabel = TargetCache<string>.GetOrAdd($"{label}@{target}", () =>
                    {
                        string derivedLabel = label;
                        DataRow record = target.MetadataRecordFromTag(Metadata);

                        if ((object)record != null && derivedLabel.IndexOf('{') >= 0)
                        {
                            foreach (string fieldName in record.Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName))
                                derivedLabel = derivedLabel.ReplaceCaseInsensitive($"{{{fieldName}}}", record[fieldName].ToString());
                        }

                        // ReSharper disable once AccessToModifiedClosure
                        if (derivedLabel.Equals(label, StringComparison.Ordinal))
                            derivedLabel = $"{label}{(groups.Length > 1 ? $" {i + 1}" : "")}";

                        return derivedLabel;
                    });
                                                              
                    yield return new DataSourceValueGroup
                    {
                        Target = seriesLabel,
                        RootTarget = target,
                        SourceTarget = sourceTarget,
                        Source = groups[i].Source
                    };
                }
            }
            else
            {
                switch (groupOperation)
                {
                    case GroupOperation.Set:
                        // Flatten all series into a single enumerable
                        DataSourceValueGroup result = new DataSourceValueGroup
                        {
                            Target = $"Set{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{queryExpression})",
                            RootTarget = queryExpression,
                            SourceTarget = sourceTarget,
                            Source = ExecuteSeriesFunctionOverSource(dataset.AsParallel().WithCancellation(cancellationToken).SelectMany(source => source.Source), seriesFunction, parameters)
                        };

                        // Handle edge-case set operations - for these functions there is data in the target series as well
                        if (seriesFunction == SeriesFunction.Minimum || seriesFunction == SeriesFunction.Maximum || seriesFunction == SeriesFunction.Median || seriesFunction == SeriesFunction.Mode)
                        {
                            DataSourceValue dataValue = result.Source.First();
                            result.Target = $"Set{seriesFunction} = {dataValue.Target}";
                            result.RootTarget = dataValue.Target;
                        }

                        yield return result;

                        break;
                    case GroupOperation.Slice:
                        TimeSliceScanner scanner = new TimeSliceScanner(dataset, ParseFloat(parameters[0]) / SI.Milli);
                        parameters = parameters.Skip(1).ToArray();

                        // Flatten all series into a single enumerable
                        yield return new DataSourceValueGroup
                        {
                            Target = $"Slice{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{queryExpression})",
                            RootTarget = queryExpression,
                            SourceTarget = sourceTarget,
                            Source = ExecuteSeriesFunctionOverTimeSlices(scanner, seriesFunction, parameters)
                        };

                        break;
                    default:
                        foreach (DataSourceValueGroup dataValues in dataset)
                            yield return new DataSourceValueGroup
                            {
                                Target = $"{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{dataValues.Target})",
                                RootTarget = dataValues.Target,
                                SourceTarget = sourceTarget,
                                Source = ExecuteSeriesFunctionOverSource(dataValues.Source, seriesFunction, parameters)
                            };

                        break;
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_seriesFunctions;
        private static readonly Regex s_averageExpression;
        private static readonly Regex s_minimumExpression;
        private static readonly Regex s_maximumExpression;
        private static readonly Regex s_totalExpression;
        private static readonly Regex s_rangeExpression;
        private static readonly Regex s_countExpression;
        private static readonly Regex s_distinctExpression;
        private static readonly Regex s_absoluteValueExpression;
        private static readonly Regex s_addExpression;
        private static readonly Regex s_subtractExpression;
        private static readonly Regex s_multiplyExpression;
        private static readonly Regex s_divideExpression;
        private static readonly Regex s_roundExpression;
        private static readonly Regex s_floorExpression;
        private static readonly Regex s_ceilingExpression;
        private static readonly Regex s_truncateExpression;
        private static readonly Regex s_standardDeviationExpression;
        private static readonly Regex s_medianExpression;
        private static readonly Regex s_modeExpression;
        private static readonly Regex s_topExpression;
        private static readonly Regex s_bottomExpression;
        private static readonly Regex s_randomExpression;
        private static readonly Regex s_firstExpression;
        private static readonly Regex s_lastExpression;
        private static readonly Regex s_percentileExpression;
        private static readonly Regex s_differenceExpression;
        private static readonly Regex s_timeDifferenceExpression;
        private static readonly Regex s_derivativeExpression;
        private static readonly Regex s_timeIntegrationExpression;
        private static readonly Regex s_intervalExpression;
        private static readonly Regex s_includeRangeExpression;
        private static readonly Regex s_excludeRangeExpression;
        private static readonly Regex s_filterNaNExpression;
        private static readonly Regex s_unwrapAngleExpression;
        private static readonly Regex s_wrapAngleExpression;
        private static readonly Regex s_labelExpression;
        private static readonly Dictionary<SeriesFunction, int> s_requiredParameters;
        private static readonly Dictionary<SeriesFunction, int> s_optionalParameters;
        private static readonly string[] s_groupOperationNames;

        // Static Constructor
        static GrafanaDataSourceBase()
        {
            const string GetExpression = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";

            // RegEx instance to find all series functions
            s_groupOperationNames = Enum.GetNames(typeof(GroupOperation));
            s_seriesFunctions = new Regex($@"({string.Join("|", s_groupOperationNames)})?\w+\s*(?<!\s+IN\s+)\((([^\(\)]|(?<counter>\()|(?<-counter>\)))*(?(counter)(?!)))\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // RegEx instances to identify specific functions and extract internal expressions
            s_averageExpression = new Regex(string.Format(GetExpression, "(Average|Avg|Mean)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_minimumExpression = new Regex(string.Format(GetExpression, "(Minimum|Min)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_maximumExpression = new Regex(string.Format(GetExpression, "(Maximum|Max)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_totalExpression = new Regex(string.Format(GetExpression, "(Total|Sum)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_rangeExpression = new Regex(string.Format(GetExpression, "Range"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_countExpression = new Regex(string.Format(GetExpression, "Count"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_distinctExpression = new Regex(string.Format(GetExpression, "(Distinct|Unique)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_absoluteValueExpression = new Regex(string.Format(GetExpression, "(AbsoluteValue|Abs)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_addExpression = new Regex(string.Format(GetExpression, "Add"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_subtractExpression = new Regex(string.Format(GetExpression, "Subtract"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_multiplyExpression = new Regex(string.Format(GetExpression, "Multiply"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_divideExpression = new Regex(string.Format(GetExpression, "Divide"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_roundExpression = new Regex(string.Format(GetExpression, "Round"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_floorExpression = new Regex(string.Format(GetExpression, "Floor"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_ceilingExpression = new Regex(string.Format(GetExpression, "(Ceiling|Ceil)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_truncateExpression = new Regex(string.Format(GetExpression, "(Truncate|Trunc)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_standardDeviationExpression = new Regex(string.Format(GetExpression, "(StandardDeviation|StdDev)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_medianExpression = new Regex(string.Format(GetExpression, "(Median|Med|Mid)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_modeExpression = new Regex(string.Format(GetExpression, "Mode"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_topExpression = new Regex(string.Format(GetExpression, "(Top|Largest)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_bottomExpression = new Regex(string.Format(GetExpression, "(Bottom|Bot|Smallest)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_randomExpression = new Regex(string.Format(GetExpression, "(Random|Rand|Sample)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_firstExpression = new Regex(string.Format(GetExpression, "First"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_lastExpression = new Regex(string.Format(GetExpression, "Last"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_percentileExpression = new Regex(string.Format(GetExpression, "(Percentile|Pctl)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_differenceExpression = new Regex(string.Format(GetExpression, "(Difference|Diff)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeDifferenceExpression = new Regex(string.Format(GetExpression, "(TimeDifference|TimeDiff|Elapsed)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_derivativeExpression = new Regex(string.Format(GetExpression, "(Derivative|Der)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeIntegrationExpression = new Regex(string.Format(GetExpression, "(TimeIntegration|TimeInt)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_intervalExpression = new Regex(string.Format(GetExpression, "Interval"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_includeRangeExpression = new Regex(string.Format(GetExpression, "(IncludeRange|Include)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_excludeRangeExpression = new Regex(string.Format(GetExpression, "(ExcludeRange|Exclude)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_filterNaNExpression = new Regex(string.Format(GetExpression, "FilterNaN"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_unwrapAngleExpression = new Regex(string.Format(GetExpression, "(UnwrapAngle|Unwrap)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_wrapAngleExpression = new Regex(string.Format(GetExpression, "(WrapAngle|Wrap)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_labelExpression = new Regex(string.Format(GetExpression, "(Label|Name)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Define required parameter counts for each function
            s_requiredParameters = new Dictionary<SeriesFunction, int>
            {
                [SeriesFunction.Average] = 0,
                [SeriesFunction.Minimum] = 0,
                [SeriesFunction.Maximum] = 0,
                [SeriesFunction.Total] = 0,
                [SeriesFunction.Range] = 0,
                [SeriesFunction.Count] = 0,
                [SeriesFunction.Distinct] = 0,
                [SeriesFunction.AbsoluteValue] = 0,
                [SeriesFunction.Add] = 1,
                [SeriesFunction.Subtract] = 1,
                [SeriesFunction.Multiply] = 1,
                [SeriesFunction.Divide] = 1,
                [SeriesFunction.Round] = 0,
                [SeriesFunction.Floor] = 0,
                [SeriesFunction.Ceiling] = 0,
                [SeriesFunction.Truncate] = 0,
                [SeriesFunction.StandardDeviation] = 0,
                [SeriesFunction.Median] = 0,
                [SeriesFunction.Mode] = 0,
                [SeriesFunction.Top] = 1,
                [SeriesFunction.Bottom] = 1,
                [SeriesFunction.Random] = 1,
                [SeriesFunction.First] = 0,
                [SeriesFunction.Last] = 0,
                [SeriesFunction.Percentile] = 1,
                [SeriesFunction.Difference] = 0,
                [SeriesFunction.TimeDifference] = 0,
                [SeriesFunction.Derivative] = 0,
                [SeriesFunction.TimeIntegration] = 0,
                [SeriesFunction.Interval] = 1,
                [SeriesFunction.IncludeRange] = 2,
                [SeriesFunction.ExcludeRange] = 2,
                [SeriesFunction.FilterNaN] = 0,
                [SeriesFunction.UnwrapAngle] = 0,
                [SeriesFunction.WrapAngle] = 0,
                [SeriesFunction.Label] = 1
            };

            // Define optional parameter counts for each function
            s_optionalParameters = new Dictionary<SeriesFunction, int>
            {
                [SeriesFunction.Average] = 0,
                [SeriesFunction.Minimum] = 0,
                [SeriesFunction.Maximum] = 0,
                [SeriesFunction.Total] = 0,
                [SeriesFunction.Range] = 0,
                [SeriesFunction.Count] = 0,
                [SeriesFunction.Distinct] = 0,
                [SeriesFunction.AbsoluteValue] = 0,
                [SeriesFunction.Add] = 0,
                [SeriesFunction.Subtract] = 0,
                [SeriesFunction.Multiply] = 0,
                [SeriesFunction.Divide] = 0,
                [SeriesFunction.Round] = 1,
                [SeriesFunction.Floor] = 0,
                [SeriesFunction.Ceiling] = 0,
                [SeriesFunction.Truncate] = 0,
                [SeriesFunction.StandardDeviation] = 1,
                [SeriesFunction.Median] = 0,
                [SeriesFunction.Mode] = 0,
                [SeriesFunction.Top] = 1,
                [SeriesFunction.Bottom] = 1,
                [SeriesFunction.Random] = 1,
                [SeriesFunction.First] = 1,
                [SeriesFunction.Last] = 1,
                [SeriesFunction.Percentile] = 0,
                [SeriesFunction.Difference] = 0,
                [SeriesFunction.TimeDifference] = 1,
                [SeriesFunction.Derivative] = 1,
                [SeriesFunction.TimeIntegration] = 1,
                [SeriesFunction.Interval] = 0,
                [SeriesFunction.IncludeRange] = 2,
                [SeriesFunction.ExcludeRange] = 2,
                [SeriesFunction.FilterNaN] = 1,
                [SeriesFunction.UnwrapAngle] = 1,
                [SeriesFunction.WrapAngle] = 1,
                [SeriesFunction.Label] = 0
            };
        }

        // Static Methods
        private static Tuple<SeriesFunction, string, GroupOperation> ParseSeriesFunction(Match matchedFunction)
        {
            Tuple<SeriesFunction, string, GroupOperation> result = TargetCache<Tuple<SeriesFunction, string, GroupOperation>>.GetOrAdd(matchedFunction.Value, () =>
            {
                GroupOperation groupOperation;
                Match filterMatch;
                string expression;

                // Determine if expression is defined as a group operation
                if (matchedFunction.Groups[1].Success)
                {
                    groupOperation = (GroupOperation)s_groupOperationNames.IndexOf(groupOperationName => matchedFunction.Groups[1].Value.Equals(groupOperationName, StringComparison.OrdinalIgnoreCase));
                    expression = matchedFunction.Value.Substring(groupOperation.ToString().Length);
                }
                else
                {
                    groupOperation = GroupOperation.None;
                    expression = matchedFunction.Value;
                }

                // Look for average function
                lock (s_averageExpression)
                    filterMatch = s_averageExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Average, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for minimum function
                lock (s_minimumExpression)
                    filterMatch = s_minimumExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Minimum, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for maximum function
                lock (s_maximumExpression)
                    filterMatch = s_maximumExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Maximum, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for total function
                lock (s_totalExpression)
                    filterMatch = s_totalExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Total, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for range function
                lock (s_rangeExpression)
                    filterMatch = s_rangeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Range, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for count function
                lock (s_countExpression)
                    filterMatch = s_countExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Count, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for distinct function
                lock (s_distinctExpression)
                    filterMatch = s_distinctExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Distinct, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for absolute value function
                lock (s_absoluteValueExpression)
                    filterMatch = s_absoluteValueExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.AbsoluteValue, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for add function
                lock (s_addExpression)
                    filterMatch = s_addExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Add, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for subtract function
                lock (s_subtractExpression)
                    filterMatch = s_subtractExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Subtract, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for multiply function
                lock (s_multiplyExpression)
                    filterMatch = s_multiplyExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Multiply, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for divide function
                lock (s_divideExpression)
                    filterMatch = s_divideExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Divide, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for round function
                lock (s_roundExpression)
                    filterMatch = s_roundExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Round, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for floor function
                lock (s_floorExpression)
                    filterMatch = s_floorExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Floor, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for ceiling function
                lock (s_ceilingExpression)
                    filterMatch = s_ceilingExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Ceiling, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for truncate function
                lock (s_truncateExpression)
                    filterMatch = s_truncateExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Truncate, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for standard deviation function
                lock (s_standardDeviationExpression)
                    filterMatch = s_standardDeviationExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.StandardDeviation, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for median function
                lock (s_medianExpression)
                    filterMatch = s_medianExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Median, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for mode function
                lock (s_modeExpression)
                    filterMatch = s_modeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Mode, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for top function
                lock (s_topExpression)
                    filterMatch = s_topExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Top, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for bottom function
                lock (s_bottomExpression)
                    filterMatch = s_bottomExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Bottom, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for random function
                lock (s_randomExpression)
                    filterMatch = s_randomExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Random, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for first function
                lock (s_firstExpression)
                    filterMatch = s_firstExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.First, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for last function
                lock (s_lastExpression)
                    filterMatch = s_lastExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Last, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for percentile function
                lock (s_percentileExpression)
                    filterMatch = s_percentileExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Percentile, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for difference function
                lock (s_differenceExpression)
                    filterMatch = s_differenceExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Difference, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for time difference function
                lock (s_timeDifferenceExpression)
                    filterMatch = s_timeDifferenceExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.TimeDifference, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for derivative function
                lock (s_derivativeExpression)
                    filterMatch = s_derivativeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Derivative, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for time integration function
                lock (s_timeIntegrationExpression)
                    filterMatch = s_timeIntegrationExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.TimeIntegration, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for interval function
                lock (s_intervalExpression)
                    filterMatch = s_intervalExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Interval, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for include range function
                lock (s_includeRangeExpression)
                    filterMatch = s_includeRangeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.IncludeRange, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for exclude range function
                lock (s_excludeRangeExpression)
                    filterMatch = s_excludeRangeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.ExcludeRange, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for filter NaN function
                lock (s_filterNaNExpression)
                    filterMatch = s_filterNaNExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.FilterNaN, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for unwrap angle function
                lock (s_unwrapAngleExpression)
                    filterMatch = s_unwrapAngleExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.UnwrapAngle, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for wrap angle function
                lock (s_wrapAngleExpression)
                    filterMatch = s_wrapAngleExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.WrapAngle, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Look for label function
                lock (s_labelExpression)
                    filterMatch = s_labelExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Label, filterMatch.Result("${Expression}").Trim(), groupOperation);

                // Target is not a recognized function
                return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.None, expression, GroupOperation.None);
            });

            if (result.Item1 == SeriesFunction.None)
                throw new InvalidOperationException($"Unrecognized series function '{matchedFunction.Value}'");

            return result;
        }

        // Execute series function over a set of points from each series at the same time-slice
        private static IEnumerable<DataSourceValue> ExecuteSeriesFunctionOverTimeSlices(TimeSliceScanner scanner, SeriesFunction seriesFunction, string[] parameters)
        {
            while (!scanner.DataReadComplete)
                foreach (DataSourceValue dataValue in ExecuteSeriesFunctionOverSource(scanner.ReadNextTimeSlice(), seriesFunction, parameters, true))
                    yield return dataValue;
        }

        // Design philosophy: whenever possible this function should delay source enumeration since source data sets could be very large.
        private static IEnumerable<DataSourceValue> ExecuteSeriesFunctionOverSource(IEnumerable<DataSourceValue> source, SeriesFunction seriesFunction, string[] parameters, bool isSliceOperation = false)
        {
            DataSourceValue[] values;
            DataSourceValue result = new DataSourceValue();
            double lastValue = double.NaN;
            double lastTime = 0.0D;
            string lastTarget = null;

            IEnumerable<double> trackedValues = source.Select(dataValue =>
            {
                lastTime = dataValue.Time;
                lastTarget = dataValue.Target;
                return dataValue.Value;
            });

            double baseTime, timeStep, value, low, high;
            bool normalizeTime, lowInclusive, highInclusive;
            int count;
            TargetTimeUnit timeUnit;
            AngleUnit angleUnit;

            switch (seriesFunction)
            {
                case SeriesFunction.Minimum:
                    DataSourceValue minValue = new DataSourceValue { Value = double.MaxValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value <= minValue.Value)
                            minValue = dataValue;
                    }

                    if (minValue.Time > 0.0D)
                        yield return minValue;

                    break;
                case SeriesFunction.Maximum:
                    DataSourceValue maxValue = new DataSourceValue { Value = double.MinValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value >= maxValue.Value)
                            maxValue = dataValue;
                    }

                    if (maxValue.Time > 0.0D)
                        yield return maxValue;

                    break;
                case SeriesFunction.Average:
                    result.Value = trackedValues.Average();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Total:
                    result.Value = trackedValues.Sum();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Range:
                    DataSourceValue rangeMin = new DataSourceValue { Value = double.MaxValue };
                    DataSourceValue rangeMax = new DataSourceValue { Value = double.MinValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value <= rangeMin.Value)
                            rangeMin = dataValue;

                        if (dataValue.Value >= rangeMax.Value)
                            rangeMax = dataValue;
                    }

                    if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
                    {
                        result = rangeMax;
                        result.Value = rangeMax.Value - rangeMin.Value;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Count:
                    result.Value = trackedValues.Count();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Distinct:
                    foreach (DataSourceValue dataValue in source.DistinctBy(dataValue => dataValue.Value))
                        yield return dataValue;

                    break;
                case SeriesFunction.AbsoluteValue:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Abs(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Add:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value + value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Subtract:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value - value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Multiply:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value * value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Divide:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value / value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Round:
                    count = parameters.Length == 0 ? 0 : ParseInt(parameters[0]);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Round(dataValue.Value, count), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Floor:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Floor(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Ceiling:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Ceiling(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Truncate:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Truncate(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.StandardDeviation:
                    result.Value = trackedValues.StandardDeviation(parameters.Length > 0 && parameters[0].Trim().ParseBoolean());
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Median:
                    values = source.Median();

                    if (values.Length == 0)
                        yield break;

                    result = values.Last();

                    if (values.Length > 1)
                        result.Value = values.Select(dataValue => dataValue.Value).Average();

                    yield return result;
                    break;
                case SeriesFunction.Mode:
                    values = source.ToArray();
                    yield return values.MajorityBy(values.Last(), dataValue => dataValue.Value, false);
                    break;
                case SeriesFunction.Top:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : (a.Value > b.Value ? 1 : 0));

                    foreach (DataSourceValue dataValue in values.Take(count).Select((dataValue, i) => new DataSourceValue { Value = dataValue.Value, Time = normalizeTime ? baseTime + i * timeStep : dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Bottom:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    Array.Sort(values, (a, b) => a.Value > b.Value ? -1 : (a.Value < b.Value ? 1 : 0));

                    foreach (DataSourceValue dataValue in values.Take(count).Select((dataValue, i) => new DataSourceValue { Value = dataValue.Value, Time = normalizeTime ? baseTime + i * timeStep : dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Random:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    List<int> indexes = new List<int>(Enumerable.Range(0, values.Length));
                    indexes.Scramble();

                    foreach (DataSourceValue dataValue in indexes.Take(count).Select((index, i) => new DataSourceValue { Value = values[index].Value, Time = normalizeTime ? baseTime + i * timeStep : values[index].Time, Target = values[index].Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.First:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = parameters.Length == 0 ? 1 : ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    for (int i = 0; i < count; i++)
                        yield return values[i];

                    break;
                case SeriesFunction.Last:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = parameters.Length == 0 ? 1 : ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    for (int i = 0; i < count; i++)
                        yield return values[values.Length - 1 - i];

                    break;
                case SeriesFunction.Percentile:
                    double percent = ParsePercentage(parameters[0]);
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : (a.Value > b.Value ? 1 : 0));
                    count = values.Length;

                    if (percent == 0.0D)
                    {
                        yield return values.First();
                    }
                    else if (percent == 100.0D)
                    {
                        yield return values.Last();
                    }
                    else
                    {
                        double n = (count - 1) * (percent / 100.0D) + 1.0D;
                        int k = (int)n;
                        DataSourceValue kData = values[k];
                        double d = n - k;
                        double k0 = values[k - 1].Value;
                        double k1 = kData.Value;

                        result.Value = k0 + d * (k1 - k0);
                        result.Time = kData.Time;
                        result.Target = kData.Target;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Difference:
                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = dataValue.Value - lastValue, Time = dataValue.Time, Target = lastTarget };

                        lastValue = dataValue.Value;
                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }
                    break;
                case SeriesFunction.TimeDifference:
                    if (parameters.Length == 0 || !TargetTimeUnit.TryParse(parameters[0], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, timeUnit), Time = dataValue.Time, Target = lastTarget };

                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }
                    break;
                case SeriesFunction.Derivative:
                    if (parameters.Length == 0 || !TargetTimeUnit.TryParse(parameters[0], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = (dataValue.Value - lastValue) / ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, timeUnit), Time = dataValue.Time, Target = lastTarget };

                        lastValue = dataValue.Value;
                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }
                    break;
                case SeriesFunction.TimeIntegration:
                    if (parameters.Length == 0 || !TargetTimeUnit.TryParse(parameters[0], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Hours };

                    result.Value = 0.0D;

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            result.Value += dataValue.Value * ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, timeUnit);

                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }

                    if (lastTime > 0.0D)
                    {
                        result.Time = lastTime;
                        result.Target = lastTarget;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Interval:
                    if (parameters.Length == 1 || !TargetTimeUnit.TryParse(parameters[1], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds };

                    value = FromTimeUnits(ParseFloat(parameters[0], source, true, isSliceOperation), timeUnit) / SI.Milli;

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                        {
                            if (dataValue.Time - lastTime > value)
                            {
                                lastTime = dataValue.Time;
                                yield return dataValue;
                            }
                        }
                        else
                        {
                            lastTime = dataValue.Time;
                            yield return dataValue;
                        }
                    }
                    break;
                case SeriesFunction.IncludeRange:
                    low = ParseFloat(parameters[0], source, false, isSliceOperation);
                    high = ParseFloat(parameters[1], source, false, isSliceOperation);
                    lowInclusive = parameters.Length > 2 && parameters[2].Trim().ParseBoolean();
                    highInclusive = parameters.Length > 3 ? parameters[3].Trim().ParseBoolean() : lowInclusive;

                    foreach (DataSourceValue dataValue in source.Where(dataValue => (lowInclusive ? dataValue.Value >= low : dataValue.Value > low) && (highInclusive ? dataValue.Value <= high : dataValue.Value < high)))
                        yield return dataValue;

                    break;
                case SeriesFunction.ExcludeRange:
                    low = ParseFloat(parameters[0], source, false, isSliceOperation);
                    high = ParseFloat(parameters[1], source, false, isSliceOperation);
                    lowInclusive = parameters.Length > 2 && parameters[2].Trim().ParseBoolean();
                    highInclusive = parameters.Length > 3 ? parameters[3].Trim().ParseBoolean() : lowInclusive;

                    foreach (DataSourceValue dataValue in source.Where(dataValue => (lowInclusive ? dataValue.Value <= low : dataValue.Value < low) || (highInclusive ? dataValue.Value >= high : dataValue.Value > high)))
                        yield return dataValue;

                    break;
                case SeriesFunction.FilterNaN:
                    bool alsoFilterInifinity = parameters.Length == 0 || parameters[0].Trim().ParseBoolean();

                    foreach (DataSourceValue dataValue in source.Where(dataValue => !(double.IsNaN(dataValue.Value) || alsoFilterInifinity && double.IsInfinity(dataValue.Value))))
                        yield return dataValue;

                    break;
                case SeriesFunction.UnwrapAngle:
                    if (parameters.Length == 0 || !Enum.TryParse(parameters[0], true, out angleUnit))
                        angleUnit = AngleUnit.Degrees;

                    values = source.ToArray();

                    foreach (DataSourceValue dataValue in Angle.Unwrap(values.Select(dataValue => Angle.ConvertFrom(dataValue.Value, angleUnit))).Select((angle, index) => new DataSourceValue { Value = angle.ConvertTo(angleUnit), Time = values[index].Time, Target = values[index].Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.WrapAngle:
                    if (parameters.Length == 0 || !Enum.TryParse(parameters[0], true, out angleUnit))
                        angleUnit = AngleUnit.Degrees;

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Angle.ConvertFrom(dataValue.Value, angleUnit).ToRange(-Math.PI, false).ConvertTo(angleUnit), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
            }
        }

        private static Time FromTimeUnits(double value, TargetTimeUnit target)
        {
            double time = Time.ConvertFrom(value, target.Unit);

            if (!double.IsNaN(target.Factor))
                time *= target.Factor;

            return time;
        }

        private static double ToTimeUnits(Time value, TargetTimeUnit target)
        {
            double time = value.ConvertTo(target.Unit);

            if (!double.IsNaN(target.Factor))
                time /= target.Factor;

            return time;
        }

        private static int ParseInt(string parameter, bool includeZero = true)
        {
            int value;

            parameter = parameter.Trim();

            if (!int.TryParse(parameter, out value))
                throw new FormatException($"Could not parse '{parameter}' as an integer value.");

            if (includeZero)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
            }
            else
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than or equal to zero.");
            }

            return value;
        }

        private static double ParseFloat(string parameter, IEnumerable<DataSourceValue> source = null, bool validateGTEZero = true, bool isSliceOperation = false)
        {
            double value;

            parameter = parameter.Trim();

            Tuple<bool, double> cache = TargetCache<Tuple<bool, double>>.GetOrAdd(parameter, () =>
            {
                double result;
                bool success = double.TryParse(parameter, out result);
                return new Tuple<bool, double>(success, result);
            });

            if (cache.Item1)
            {
                value = cache.Item2;
            }
            else
            {
                if ((object)source == null)
                    throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

                double defaultValue = 0.0D;
                bool hasDefaultValue = false;

                if (parameter.IndexOf(';') > -1)
                {
                    string[] parts = parameter.Split(';');

                    if (parts.Length >= 2)
                    {
                        parameter = parts[0].Trim();
                        defaultValue = ParseFloat(parts[1], source, validateGTEZero, isSliceOperation);
                        hasDefaultValue = true;
                    }
                }

                DataSourceValue result = source.FirstOrDefault(dataValue => dataValue.Target.Equals(parameter, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(result.Target))
                {
                    // Slice operations may not have a target for a given slice - in this case function should use a default value and not fail
                    if (isSliceOperation || hasDefaultValue)
                        result.Value = defaultValue;
                    else
                        throw new FormatException($"Value target '{parameter}' could not be found in dataset nor parsed as a floating-point value.");
                }

                value = result.Value;
            }

            if (validateGTEZero)
            {
                if (value < 0.0D)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
            }

            return value;
        }

        private static int ParseCount(string parameter, DataSourceValue[] values, bool isSliceOperation)
        {
            int length = values.Length;
            int count;

            if (length == 0)
                return 0;

            parameter = parameter.Trim();

            Tuple<bool, int> cache = TargetCache<Tuple<bool, int>>.GetOrAdd(parameter, () =>
            {
                bool success = true;
                int result;

                if (parameter.EndsWith("%"))
                {
                    try
                    {
                        double percent = ParsePercentage(parameter, false);
                        result = (int)(length * (percent / 100.0D));

                        if (result == 0)
                            result = 1;
                    }
                    catch
                    {
                        success = false;
                        result = 0;
                    }
                }
                else
                {
                    success = int.TryParse(parameter, out result);
                }

                return new Tuple<bool, int>(success, result);
            });

            if (cache.Item1)
            {
                count = cache.Item2;
            }
            else
            {
                if (parameter.EndsWith("%"))
                    throw new ArgumentOutOfRangeException($"Could not parse '{parameter}' as a floating-point value or percentage is outside range of greater than 0 and less than or equal to 100.");

                double defaultValue = 1.0D;
                bool hasDefaultValue = false;

                if (parameter.IndexOf(';') > -1)
                {
                    string[] parts = parameter.Split(';');

                    if (parts.Length >= 2)
                    {
                        parameter = parts[0].Trim();
                        defaultValue = ParseCount(parts[1], values, isSliceOperation);
                        hasDefaultValue = true;
                    }
                }

                DataSourceValue result = values.FirstOrDefault(dataValue => dataValue.Target.Equals(parameter, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(result.Target))
                {
                    // Slice operations may not have a target for a given slice - in this case function should use a default value and not fail
                    if (isSliceOperation || hasDefaultValue)
                        result.Value = defaultValue;
                    else
                        throw new FormatException($"Value target '{parameter}' could not be found in dataset nor parsed as an integer value.");
                }

                // Treat fractional numbers as a percentage of length
                if (result.Value > 0.0D && result.Value < 1.0D)
                    count = (int)(length * result.Value);
                else
                    count = (int)result.Value;
            }

            if (count < 1)
                throw new ArgumentOutOfRangeException($"Count '{count}' is less than one.");

            return count;
        }

        private static double ParsePercentage(string parameter, bool includeZero = true)
        {
            double percent;

            parameter = parameter.Trim();

            if (parameter.EndsWith("%"))
                parameter = parameter.Substring(0, parameter.Length - 1);

            if (!double.TryParse(parameter, out percent))
                throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

            if (includeZero)
            {
                if (percent < 0.0D || percent > 100.0D)
                    throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of 0 to 100, inclusive.");
            }
            else
            {
                if (percent <= 0.0D || percent > 100.0D)
                    throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of greater than 0 and less than or equal to 100.");
            }

            return percent;
        }

        #endregion
    }
}