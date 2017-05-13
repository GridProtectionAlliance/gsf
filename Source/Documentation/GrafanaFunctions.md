# GSF Grafana Functions

The Grafana interfaces defined in the Grid Solutions Framework allow for aggregation and operational functions on a per-series and per-set basis. The following defines the available functions that are available for a data source implementing the GSF Grafana interface, e.g., openHistorian.

## Group Operations

Each Grafana series function can be operated on in aggregate using a group operator prefix:

### Set

Series functions can operate over the set of defined series, producing a single result series, where the target function is executed over each series, horizontally, end-to-end by prefixing the function name with `Set`.

* Example: `SetAverage(FILTER ActiveMeasurements WHERE SignalType='FREQ')`

### Slice

Series functions can operate over the set of defined series, producing a single result series, where the target function is executed over each series as a group, vertically, per time-slice by prefixing the function name with `Slice`. When operating on a set of series data with a slice function, a new required parameter for time tolerance will be introduced as the first parameter to the function. The parameter is a floating-point value that must be greater than or equal to zero that represents the desired time tolerance, in seconds, for the time slice.

* Example: `SliceSum(0.0333, FILTER ActiveMeasurements WHERE SignalType='IPHM')`

## Series Functions

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
* [StandardDeviationSample](#standarddeviationsample)
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
* [Label](#label)

## Average

Returns a single value that represents the mean of the values in the source series.

* Signature: `Average(expression)`
* Example: `Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Average`, `Avg`, `Mean`
* Execution: Immediate enumeration

## Minimum

Returns a single value that is the minimum of the values in the source series.

* Signature: `Minimum(expression)`
* Example: `Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Minimum`, `Min`
* Execution: Immediate enumeration

## Maximum

Returns a single value that is the maximum of the values in the source series.

* Signature: `Maximum(expression)`
* Example: `Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Maximum`, `Max`
* Execution: Immediate enumeration

## Total

Returns a single value that represents the sum of the values in the source series.

* Signature: `Total(expression)`
* Example: `Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Total`, `Sum`
* Execution: Immediate enumeration

## Range

Returns a single value that represents the range, i.e., `maximum - minimum`, of the values in the source series.

* Signature: `Range(expression)`
* Example: `Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Range`
* Execution: Immediate enumeration

## Count

Returns a single value that is the count of the values in the source series.

* Signature: `Count(expression)`
* Example: `Count(PPA:1; PPA:2; PPA:3)`
* Variants: `Count`
* Execution: Immediate enumeration

## Distinct

Returns a series of values that represent the unique set of values in the source series.

* Signature: `Distinct(expression)`
* Example: `Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Distinct`, `Unique`
* Execution: Deferred enumeration

## AbsoluteValue

Returns a series of values that represent the absolute value each of the values in the source series.

* Signature: `AbsoluteValue(expression)`
* Example: `AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `AbsoluteValue`, `Abs`
* Execution: Deferred enumeration

## Add

Returns a series of values that represent each of the values in the source series added with N.
N is a floating point value representing an additive offset to be applied to each value the source series.

* Signature: `Add(N, expression)`
* Example: `Add(-1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `Add`
* Execution: Deferred enumeration

## Multiply

Returns a series of values that represent each of the values in the source series multiplied by N.
N is a floating point value representing a multiplicative factor to be applied to each value the source series.

* Signature: `Multiply(N, expression)`
* Example: `Multiply(0.5, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `Multiply`
* Execution: Deferred enumeration

## Round

Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.

* Signature: `Round([N], expression)`
* Example: `Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Round`
* Execution: Deferred enumeration

## Floor

Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.

* Signature: `Floor(expression)`
* Example: `Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Floor`
* Execution: Deferred enumeration

## Ceiling

Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.

* Signature: `Ceiling(expression)`
* Example: `Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Ceiling`, `Ceil`
* Execution: Deferred enumeration

## Truncate

Returns a series of values that represent the integral part of each of the values in the source series.

* Signature: `Truncate(expression)`
* Example: `Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Truncate`, `Trunc`
* Execution: Deferred enumeration

## StandardDeviation

Returns a single value that represents the standard deviation of the values in the source series.

* Signature: `StandardDeviation(expression)`
* Example: `StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `StandardDeviation`, `StdDev`
* Execution: Immediate in-memory array load

## StandardDeviationSample

Returns a single value that represents the standard deviation, using sample calculation, of the values in the source series.

* Signature: `StandardDeviationSample(expression)`
* Example: `StandardDeviationSample(FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `StandardDeviationSample`, `StdDevSamp`
* Execution: Immediate in-memory array load

## Median

Returns a single value that represents the median of the values in the source series.

* Signature: `Median(expression)`
* Example: `Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')`
* Variants: `Median`, `Med`, `Mid`
* Execution: Immediate in-memory array load

## Mode

Returns a single value that represents the mode of the values in the source series.

* Signature: `Mode(expression)`
* Example: `Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')`
* Variants: `Mode`
* Execution: Immediate in-memory array load

## Top

Returns a series of N, or N% of total, values that are the largest in the source series.
N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.

* Signature: `Top(N|N%, [normalizeTime], expression)`
* Example: `Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Top`, `Largest`
* Execution: Immediate in-memory array load

## Bottom

Returns a series of N, or N% of total, values that are the smallest in the source series.
N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.

* Signature: `Bottom(N|N%, [normalizeTime], expression)`
* Example: `Bottom(100, false, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Bottom`, `Bot`, `Smallest`
* Execution: Immediate in-memory array load

## Random

Returns a series of N, or N% of total, values that are a random sample of the values in the source series.
N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.

* Signature: `Random(N|N%, [normalizeTime], expression)`
* Example: `Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `Random`, `Rand`, `Sample`
* Execution: Immediate in-memory array load

## First

Returns a series of N, or N% of total, values from the start of the source series.
N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.

* Signature: `First([N|N%], expression)`
* Example: `First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `First`
* Execution: Immediate in-memory array load

## Last

Returns a series of N, or N% of total, values from the end of the source series.
N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.

* Signature: `Last([N|N%], expression)`
* Example: `Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Last`
* Execution: Immediate in-memory array load

## Percentile

Returns a single value that represents the Nth order percentile for the sorted values in the source series.
N is a floating point value, representing a percentage, that must range from 0 to 100.

* Signature: `Percentile(N[%], expression)`
* Example: `Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `Percentile`, `Pctl`
* Execution: Immediate in-memory array load

## Difference

Returns a series of values that represent the difference between consecutive values in the source series.

* Signature: `Difference(expression)`
* Example: `Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Difference`, `Diff`
* Execution: Deferred enumeration

## TimeDifference

Returns a series of values that represent the time difference, in seconds, between consecutive values in the source series.

* Signature: `TimeDifference(expression)`
* Example: `TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `TimeDifference`, `TimeDiff`, `Elapsed`
* Execution: Deferred enumeration

## Derivative

Returns a series of values that represent the rate of change, per second, for the difference between consecutive values in the source series.

* Signature: `Derivative(expression)`
* Example: `Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Derivative`, `Der`
* Execution: Deferred enumeration

## TimeIntegration

Returns a single value that represents the time-based integration, i.e., the sum of `V(n) * (T(n) - T(n-1))` where time difference is
calculated in hours, of the values in the source series.

* Signature: `TimeIntegration(expression)`
* Example: `TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')`
* Variants: `TimeIntegration`, `TimeInt`
* Execution: Immediate enumeration

## Interval

Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in seconds. N is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in seconds, for the returned data. Setting N value to zero will request non-decimated, full resolution data from the data source. A zero value will always produce the most accurate aggregation calculation results but will increase query burden on data source for large time ranges.

* Signature: `Interval(N, expression)`
* Example: `Sum(Interval(0, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))`
* Variants: `Interval`
* Execution: Deferred enumeration

## UnwrapAngle

Returns a series of values that represent an adjusted set of angles that are unwrapped, per specified angle units, so that a comparable mathematical operation can be executed. For example, for angles that wrap between -180 and +180 degrees, this algorithm unwraps the values to make the values mathematically comparable. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.

* Signature: `UnwrapAngle([units], expression)`
* Example: `UnwrapAngle(Degrees, FSX_PMU2-PA1:VH; REA_PMU3-PA2:VH)`
* Variants: `UnwrapAngle`
* Execution: Immediate in-memory array load

## WrapAngle

Returns a series of values that represent an adjusted set of angles that are wrapped, per specified angle units, so that angle values are consistently between -180 and +180 degrees. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.

* Signature: `WrapAngle([units], expression)`
* Example: `WrapAngle(Radians, FILTER TOP 5 ActiveMeasurements WHERE SignalType LIKE '%PHA')`
* Variants: `WrapAngle`
* Execution: Deferred enumeration

## Label

Renames a series with the specified label value. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three series in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3".

* Signature: `Label(value, expression)`
* Example: `Label('AvgFreq', SetAvg(FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))`
* Variants: `Label`, `Name`
* Execution: Deferred enumeration
