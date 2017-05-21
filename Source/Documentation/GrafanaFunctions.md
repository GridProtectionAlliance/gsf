# GSF Grafana Functions

The Grafana interfaces defined in the Grid Solutions Framework allow for aggregation and operational functions on a per-series and per-set basis. The following defines the available functions and group operations that are available for a data source implementing the GSF Grafana interface, e.g., [openHistorian](https://github.com/GridProtectionAlliance/openHistorian). Note that any data source that implements the [GrafanaDataSourceBase](https://github.com/GridProtectionAlliance/gsf/blob/master/Source/Libraries/Adapters/GrafanaAdapters/GrafanaDataSourceBase.cs) class will automatically inherit this functionality.

## Group Operations

Each Grafana series function can be operated on in aggregate using a group operator prefix:

### Set

Series functions can operate over the set of defined series, producing a single result series, where the target function is executed over each series, horizontally, end-to-end by prefixing the function name with `Set`.

* Example: `SetAverage(FILTER ActiveMeasurements WHERE SignalType='FREQ')`

### Slice

Series functions can operate over the set of defined series, producing a single result series, where the target function is executed over each series as a group, vertically, per time-slice by prefixing the function name with `Slice`. When operating on a set of series data with a slice function, a new required parameter for time tolerance will be introduced as the first parameter to the function. The parameter is a floating-point value that must be greater than or equal to zero that represents the desired time tolerance, in seconds, for the time slice.

* Example: `SliceSum(0.0333, FILTER ActiveMeasurements WHERE SignalType='IPHM')`

## Series Functions

Many series functions have parameters that can be required or optional. Optional values will always define a default state. Currently, parameter values must be a constant value.

### Execution Modes
Each of the series functions include documentation for the mode of execution required by the function. These modes determine the level of processing expense and memory burden incurred by the function. The impacts of the execution modes increase as the time-range or resolution of the series data increases.

| Execution Mode | Description | Impact |
|----------------|-------------|--------|
| _Deferred enumeration_ | Series data will be processed serially outside of function | Minimal processing and memory impact |
| _Immediate enumeration_ | Series data will be processed serially inside the function | Increased processing impact, minimal memory impact |
| _Immediate in-memory array load_ | Series data will be loaded into an array and processed inside the function | Higher processing and memory impact |

### Available Functions

* [Average](#average)
* [Minimum](#minimum)
* [Maximum](#maximum)
* [Total](#total)
* [Range](#range)
* [Count](#count)
* [Distinct](#distinct)
* [AbsoluteValue](#absolutevalue)
* [Add](#add)
* [Multiply](#multiply)
* [Round](#round)
* [Floor](#floor)
* [Ceiling](#ceiling)
* [Truncate](#truncate)
* [StandardDeviation](#standarddeviation)
* [Median](#median)
* [Mode](#mode)
* [Top](#top)
* [Bottom](#bottom)
* [Random](#random)
* [First](#first)
* [Last](#last)
* [Percentile](#percentile)
* [Difference](#difference)
* [TimeDifference](#timedifference)
* [Derivative](#derivative)
* [TimeIntegration](#timeintegration)
* [Interval](#interval)
* [IncludeRange](#includerange)
* [ExcludeRange](#excluderange)
* [FilterNaN](#filternan)
* [UnwrapAngle](#unwrapangle)
* [WrapAngle](#wrapangle)
* [Label](#label)

## Average

Returns a single value that represents the mean of the values in the source series.

* Signature: `Average(expression)`
* Returns: Single value
* Example: `Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Average`, `Avg`, `Mean`
* Execution: [Immediate enumeration](#execution-modes)

## Minimum

Returns a single value that is the minimum of the values in the source series.

* Signature: `Minimum(expression)`
* Returns: Single value
* Example: `Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Minimum`, `Min`
* Execution: [Immediate enumeration](#execution-modes)

## Maximum

Returns a single value that is the maximum of the values in the source series.

* Signature: `Maximum(expression)`
* Returns: Single value
* Example: `Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Maximum`, `Max`
* Execution: [Immediate enumeration](#execution-modes)

## Total

Returns a single value that represents the sum of the values in the source series.

* Signature: `Total(expression)`
* Returns: Single value
* Example: `Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Total`, `Sum`
* Execution: [Immediate enumeration](#execution-modes)

## Range

Returns a single value that represents the range, i.e., `maximum - minimum`, of the values in the source series.

* Signature: `Range(expression)`
* Returns: Single value
* Example: `Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Range`
* Execution: [Immediate enumeration](#execution-modes)

## Count

Returns a single value that is the count of the values in the source series.

* Signature: `Count(expression)`
* Returns: Single value
* Example: `Count(PPA:1; PPA:2; PPA:3)`
* Variants: `Count`
* Execution:[Immediate enumeration](#execution-modes)

## Distinct

Returns a series of values that represent the unique set of values in the source series.

* Signature: `Distinct(expression)`
* Returns: Series of values
* Example: `Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Distinct`, `Unique`
* Execution: [Deferred enumeration](#execution-modes)

## AbsoluteValue

Returns a series of values that represent the absolute value each of the values in the source series.

* Signature: `AbsoluteValue(expression)`
* Returns: Series of values
* Example: `AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `AbsoluteValue`, `Abs`
* Execution: [Deferred enumeration](#execution-modes)

## Add

Returns a series of values that represent each of the values in the source series added with N.
N is a floating point value representing an additive offset to be applied to each value the source series.

* Signature: `Add(N, expression)`
* Returns: Series of values
* Example: `Add(-1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `Add`
* Execution: [Deferred enumeration](#execution-modes)

## Multiply

Returns a series of values that represent each of the values in the source series multiplied by N.
N is a floating point value representing a multiplicative factor to be applied to each value the source series.

* Signature: `Multiply(N, expression)`
* Returns: Series of values
* Example: `Multiply(0.5, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `Multiply`
* Execution: [Deferred enumeration](#execution-modes)

## Round

Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.

* Signature: `Round([N = 0], expression)`
* Returns: Series of values
* Example: `Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Round`
* Execution: [Deferred enumeration](#execution-modes)

## Floor

Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.

* Signature: `Floor(expression)`
* Returns: Series of values
* Example: `Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Floor`
* Execution: [Deferred enumeration](#execution-modes)

## Ceiling

Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.

* Signature: `Ceiling(expression)`
* Returns: Series of values
* Example: `Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Ceiling`, `Ceil`
* Execution: [Deferred enumeration](#execution-modes)

## Truncate

Returns a series of values that represent the integral part of each of the values in the source series.

* Signature: `Truncate(expression)`
* Returns: Series of values
* Example: `Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Truncate`, `Trunc`
* Execution: [Deferred enumeration](#execution-modes)

## StandardDeviation

Returns a single value that represents the standard deviation of the values in the source series. First parameter, optional, is a boolean flag representing if the sample based calculation should be used - defaults to false, which means the population based calculation should be used.

* Signature: `StandardDeviation([useSampleCalc = false], expression)`
* Returns: Single value
* Example: `StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `StandardDeviation`, `StdDev`
* Execution: [Immediate in-memory array load](#execution-modes)

## Median

Returns a single value that represents the median of the values in the source series.

* Signature: `Median(expression)`
* Returns: Single value
* Example: `Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')`
* Variants: `Median`, `Med`, `Mid`
* Execution: [Immediate in-memory array load](#execution-modes)

## Mode

Returns a single value that represents the mode of the values in the source series.

* Signature: `Mode(expression)`
* Returns: Single value
* Example: `Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')`
* Variants: `Mode`
* Execution: [Immediate in-memory array load](#execution-modes)

## Top

Returns a series of N, or N% of total, values that are the largest in the source series.
N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.

* Signature: `Top(N|N%, [normalizeTime = true], expression)`
* Returns: Series of values
* Example: `Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Top`, `Largest`
* Execution: [Immediate in-memory array load](#execution-modes)

## Bottom

Returns a series of N, or N% of total, values that are the smallest in the source series.
N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.

* Signature: `Bottom(N|N%, [normalizeTime = true], expression)`
* Returns: Series of values
* Example: `Bottom(100, false, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Bottom`, `Bot`, `Smallest`
* Execution: [Immediate in-memory array load](#execution-modes)

## Random

Returns a series of N, or N% of total, values that are a random sample of the values in the source series.
N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.

* Signature: `Random(N|N%, [normalizeTime = true], expression)`
* Returns: Series of values
* Example: `Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `Random`, `Rand`, `Sample`
* Execution: [Immediate in-memory array load](#execution-modes)

## First

Returns a series of N, or N% of total, values from the start of the source series.
N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.

* Signature: `First([N|N% = 1], expression)`
* Returns: Series of values
* Example: `First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `First`
* Execution: [Immediate in-memory array load](#execution-modes)

## Last

Returns a series of N, or N% of total, values from the end of the source series.
N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.

* Signature: `Last([N|N% = 1], expression)`
* Returns: Series of values
* Example: `Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Last`
* Execution: [Immediate in-memory array load](#execution-modes)

## Percentile

Returns a single value that represents the Nth order percentile for the sorted values in the source series.
N is a floating point value, representing a percentage, that must range from 0 to 100.

* Signature: `Percentile(N[%], expression)`
* Returns: Single value
* Example: `Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `Percentile`, `Pctl`
* Execution: [Immediate in-memory array load](#execution-modes)

## Difference

Returns a series of values that represent the difference between consecutive values in the source series.

* Signature: `Difference(expression)`
* Returns: Series of values
* Example: `Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Difference`, `Diff`
* Execution: [Deferred enumeration](#execution-modes)

## TimeDifference

Returns a series of values that represent the time difference, in time units, between consecutive values in the source series. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Seconds.

* Signature: `TimeDifference([units = Seconds], expression)`
* Returns: Series of values
* Example: `TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `TimeDifference`, `TimeDiff`, `Elapsed`
* Execution: [Deferred enumeration](#execution-modes)

## Derivative

Returns a series of values that represent the rate of change, per time units, for the difference between consecutive values in the source series. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Seconds.

* Signature: `Derivative([units = Seconds], expression)`
* Returns: Series of values
* Example: `Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Derivative`, `Der`
* Execution: [Deferred enumeration](#execution-modes)

## TimeIntegration

Returns a single value that represents the time-based integration, i.e., the sum of `V(n) * (T(n) - T(n-1))` where time difference is calculated in the specified time units, of the values in the source series. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Hours.

* Signature: `TimeIntegration([units = Hours], expression)`
* Returns: Single value
* Example: `TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')`
* Variants: `TimeIntegration`, `TimeInt`
* Execution: [Immediate enumeration](#execution-modes)

## Interval

Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in seconds. N is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in seconds, for the returned data. Setting N value to zero will request non-decimated, full resolution data from the data source. A zero value will always produce the most accurate aggregation calculation results but will increase query burden on data source for large time ranges.

* Signature: `Interval(N, expression)`
* Returns: Series of values
* Example: `Sum(Interval(0, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))`
* Variants: `Interval`
* Execution: [Deferred enumeration](#execution-modes)

## IncludeRange

Returns a series of values that represent a filtered set of the values in the source series where each value falls between the specified low and high. The low and high parameter values are floating-point numbers that represent the range of values allowed in the return series. Third parameter, optional, is a boolean flag that determines if range values are inclusive, i.e., allowed values are >= low and <= high - defaults to false, which means values are exclusive, i.e., allowed values are > low and < high. Function allows a fourth optional parameter that is a boolean flag - when four parameters are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive.

* Signature: `IncludeRange(low, high, [inclusive = false], expression)` -_or_- `IncludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)`
* Returns: Series of values
* Example: `IncludeRange(59.90, 60.10, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `IncludeRange`, `Include`
* Execution: [Deferred enumeration](#execution-modes)

## ExcludeRange

Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high. The low and high parameter values are floating-point numbers that represent the range of values excluded in the return series. Third parameter, optional, is a boolean flag that determines if range values are inclusive, i.e., excluded values are <= low or >= high - defaults to false, which means values are exclusive, i.e., excluded values are < low or > high. Function allows a fourth optional parameter that is a boolean flag - when four parameters are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive.

* Signature: `ExcludeRange(low, high, [inclusive = false], expression)` -_or_- `ExcludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)`
* Returns: Series of values
* Example: `ExcludeRange(-180.0, 180.0, true, false, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHA')`
* Variants: `ExcludeRange`, `Exclude`
* Execution: [Deferred enumeration](#execution-modes)

## FilterNaN

Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN. First parameter, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.

* Signature: `FilterNaN([alsoFilterInfinity = true], expression)`
* Returns: Series of values
* Example: `FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `FilterNaN`
* Execution: [Deferred enumeration](#execution-modes)

## UnwrapAngle

Returns a series of values that represent an adjusted set of angles that are unwrapped, per specified angle units, so that a comparable mathematical operation can be executed. For example, for angles that wrap between -180 and +180 degrees, this algorithm unwraps the values to make the values mathematically comparable. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.

* Signature: `UnwrapAngle([units = Degrees], expression)`
* Returns: Series of values
* Example: `UnwrapAngle(FSX_PMU2-PA1:VH; REA_PMU3-PA2:VH)`
* Variants: `UnwrapAngle`, `Unwrap`
* Execution: [Immediate in-memory array load](#execution-modes)

## WrapAngle

Returns a series of values that represent an adjusted set of angles that are wrapped, per specified angle units, so that angle values are consistently between -180 and +180 degrees. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.

* Signature: `WrapAngle([units = Degrees], expression)`
* Returns: Series of values
* Example: `WrapAngle(Radians, FILTER TOP 5 ActiveMeasurements WHERE SignalType LIKE '%PHA')`
* Variants: `WrapAngle`, `Wrap`
* Execution: [Deferred enumeration](#execution-modes)

## Label

Renames a series with the specified label value. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three series in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3". Group operations on this function will be ignored.

* Signature: `Label(value, expression)`
* Returns: Series of values
* Example: `Label('AvgFreq', SetAvg(FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))`
* Variants: `Label`, `Name`
* Execution: [Deferred enumeration](#execution-modes)
