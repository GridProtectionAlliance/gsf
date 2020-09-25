//******************************************************************************************************
//  GrafanaDataSourceBase_FunctionParsing.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  09/25/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GSF;
using GSF.Collections;

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

    partial class GrafanaDataSourceBase
    {
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
        private static readonly Regex s_selectExpression;
        private static readonly Dictionary<SeriesFunction, int> s_requiredParameters;
        private static readonly Dictionary<SeriesFunction, int> s_optionalParameters;
        private static readonly string[] s_groupOperationNames;

        // Static Constructor
        static GrafanaDataSourceBase()
        {
            const string GetExpression = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";

            // RegEx instance to find all series functions
            s_groupOperationNames = Enum.GetNames(typeof(GroupOperation));
            s_seriesFunctions = new Regex($@"({string.Join("|", s_groupOperationNames)})?\w+\s*(?<!(\s+IN\s*)|((\)|\'|\s+)AND\s*)|((\)|\'|\s+)OR\s*))\((([^\(\)]|(?<counter>\()|(?<-counter>\)))*(?(counter)(?!)))\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            // RegEx instance used to parse meta-data for target search queries using a reduced SQL SELECT statement syntax
            s_selectExpression = new Regex(@"(SELECT\s+(TOP\s+(?<MaxRows>\d+)\s+)?(\s*(?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)?\s*FROM\s+(?<TableName>\w+)\s+WHERE\s+(?<Expression>.+)\s+ORDER\s+BY\s+(?<SortField>\w+))|(SELECT\s+(TOP\s+(?<MaxRows>\d+)\s+)?(\s*(?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)?\s*FROM\s+(?<TableName>\w+)\s+WHERE\s+(?<Expression>.+))|(SELECT\s+(TOP\s+(?<MaxRows>\d+)\s+)?(\s*(?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)?\s*FROM\s+(?<TableName>\w+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        // Find matching series function for expression that has function syntax
        private static Tuple<SeriesFunction, string, GroupOperation> ParseSeriesFunction(Match matchedFunction)
        {
            Tuple<SeriesFunction, string, GroupOperation> result = TargetCache<Tuple<SeriesFunction, string, GroupOperation>>.GetOrAdd(matchedFunction.Value, () =>
            {
                GroupOperation groupOperation;
                Match match;
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
                match = s_averageExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Average, match.Result("${Expression}").Trim(), groupOperation);

                // Look for minimum function
                match = s_minimumExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Minimum, match.Result("${Expression}").Trim(), groupOperation);

                // Look for maximum function
                match = s_maximumExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Maximum, match.Result("${Expression}").Trim(), groupOperation);

                // Look for total function
                match = s_totalExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Total, match.Result("${Expression}").Trim(), groupOperation);

                // Look for range function
                match = s_rangeExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Range, match.Result("${Expression}").Trim(), groupOperation);

                // Look for count function
                match = s_countExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Count, match.Result("${Expression}").Trim(), groupOperation);

                // Look for distinct function
                match = s_distinctExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Distinct, match.Result("${Expression}").Trim(), groupOperation);

                // Look for absolute value function
                match = s_absoluteValueExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.AbsoluteValue, match.Result("${Expression}").Trim(), groupOperation);

                // Look for add function
                match = s_addExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Add, match.Result("${Expression}").Trim(), groupOperation);

                // Look for subtract function
                match = s_subtractExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Subtract, match.Result("${Expression}").Trim(), groupOperation);

                // Look for multiply function
                match = s_multiplyExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Multiply, match.Result("${Expression}").Trim(), groupOperation);

                // Look for divide function
                match = s_divideExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Divide, match.Result("${Expression}").Trim(), groupOperation);

                // Look for round function
                match = s_roundExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Round, match.Result("${Expression}").Trim(), groupOperation);

                // Look for floor function
                match = s_floorExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Floor, match.Result("${Expression}").Trim(), groupOperation);

                // Look for ceiling function
                match = s_ceilingExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Ceiling, match.Result("${Expression}").Trim(), groupOperation);

                // Look for truncate function
                match = s_truncateExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Truncate, match.Result("${Expression}").Trim(), groupOperation);

                // Look for standard deviation function
                match = s_standardDeviationExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.StandardDeviation, match.Result("${Expression}").Trim(), groupOperation);

                // Look for median function
                match = s_medianExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Median, match.Result("${Expression}").Trim(), groupOperation);

                // Look for mode function
                match = s_modeExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Mode, match.Result("${Expression}").Trim(), groupOperation);

                // Look for top function
                match = s_topExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Top, match.Result("${Expression}").Trim(), groupOperation);

                // Look for bottom function
                match = s_bottomExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Bottom, match.Result("${Expression}").Trim(), groupOperation);

                // Look for random function
                match = s_randomExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Random, match.Result("${Expression}").Trim(), groupOperation);

                // Look for first function
                match = s_firstExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.First, match.Result("${Expression}").Trim(), groupOperation);

                // Look for last function
                match = s_lastExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Last, match.Result("${Expression}").Trim(), groupOperation);

                // Look for percentile function
                match = s_percentileExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Percentile, match.Result("${Expression}").Trim(), groupOperation);

                // Look for difference function
                match = s_differenceExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Difference, match.Result("${Expression}").Trim(), groupOperation);

                // Look for time difference function
                match = s_timeDifferenceExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.TimeDifference, match.Result("${Expression}").Trim(), groupOperation);

                // Look for derivative function
                match = s_derivativeExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Derivative, match.Result("${Expression}").Trim(), groupOperation);

                // Look for time integration function
                match = s_timeIntegrationExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.TimeIntegration, match.Result("${Expression}").Trim(), groupOperation);

                // Look for interval function
                match = s_intervalExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Interval, match.Result("${Expression}").Trim(), groupOperation);

                // Look for include range function
                match = s_includeRangeExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.IncludeRange, match.Result("${Expression}").Trim(), groupOperation);

                // Look for exclude range function
                match = s_excludeRangeExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.ExcludeRange, match.Result("${Expression}").Trim(), groupOperation);

                // Look for filter NaN function
                match = s_filterNaNExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.FilterNaN, match.Result("${Expression}").Trim(), groupOperation);

                // Look for unwrap angle function
                match = s_unwrapAngleExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.UnwrapAngle, match.Result("${Expression}").Trim(), groupOperation);

                // Look for wrap angle function
                match = s_wrapAngleExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.WrapAngle, match.Result("${Expression}").Trim(), groupOperation);

                // Look for label function
                match = s_labelExpression.Match(expression);

                if (match.Success)
                    return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.Label, match.Result("${Expression}").Trim(), groupOperation);

                // Target is not a recognized function
                return new Tuple<SeriesFunction, string, GroupOperation>(SeriesFunction.None, expression, GroupOperation.None);
            });

            if (result.Item1 == SeriesFunction.None)
                throw new InvalidOperationException($"Unrecognized series function '{matchedFunction.Value}'");

            return result;
        }

        // Attempt to parse an expression that has SQL SELECT syntax
        private static bool ParseSelectExpression(string selectExpression, out string tableName, out string[] fieldNames, out string expression, out string sortField, out int topCount)
        {
            tableName = null;
            fieldNames = null;
            expression = null;
            sortField = null;
            topCount = 0;

            if (string.IsNullOrWhiteSpace(selectExpression))
                return false;

            Match match = s_selectExpression.Match(selectExpression.ReplaceControlCharacters());

            if (!match.Success)
                return false;

            tableName = match.Result("${TableName}").Trim();
            fieldNames = match.Groups["FieldName"].Captures.Cast<Capture>().Select(capture => capture.Value).ToArray();
            expression = match.Result("${Expression}").Trim();
            sortField = match.Result("${SortField}").Trim();

            string maxRows = match.Result("${MaxRows}").Trim();

            if (string.IsNullOrEmpty(maxRows) || !int.TryParse(maxRows, out topCount))
                topCount = int.MaxValue;

            return true;
        }
    }
}
