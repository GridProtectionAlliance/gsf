# GSF Grafana Functions

The Grafana interfaces defined in the Grid Solutions Framework allow for aggregation and operational functions on a per-series and per-group basis. The following defines the [available functions](#available-functions) and [group operations](#group-operations) that are available for a data source implementing the GSF Grafana interface, e.g., [openHistorian](https://github.com/GridProtectionAlliance/openHistorian). Note that any time-series style data source that implements the [GrafanaDataSourceBase](https://github.com/GridProtectionAlliance/gsf/blob/master/Source/Libraries/Adapters/GrafanaAdapters/GrafanaDataSourceBase.cs) class will automatically inherit this functionality.

## Series Functions

Various functions are available that can be applied to each series that come from a specified expression, see full list of [available functions](#available-functions) below. Series expressions can be an individual listing of point tag names, Guid-based signal IDs or measurement keys separated by semi-colons - _or_ - a [filter expression](https://github.com/GridProtectionAlliance/gsf/blob/master/Source/Documentation/FilterExpressions.md) that will select several series at once. Filter expressions and individual points, with or without functions, may be selected simultaneously when separated with semi-colons.

* Example: `PPA:15; STAT:20; SetSum(Count(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); Range(PPA:99; Sum(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))`

Many series functions have parameters that can be required or optional &ndash; optional values will always define a default state. Parameter values must be a constant value or, where applicable, a named target available from the expression. Named targets are intended to work with group operations, i.e., [Set](#set) or [Slice](#slice), since group operations provide access to multiple series values from within a single series. The actual value used for a named target parameter will be the first encountered value for the target series &ndash; in the case of slice group operations, this will be the first value encountered in each slice. Named target parameters can optionally specify multiple fall-back series and one final default constant value each separated by a semi-colon to use when the named target series is not available, e.g.: `SliceSubtract(1, T1;T2;5.5, T1;T2;T3)`

To better understand named targets, consider the following steps:

> NOTE: This example is for illustrative purposes only, use the [`Reference`](#reference) function to get a difference between two or more angles taking wrapping and unwrapping into consideration.

 1. The following expression produces two unwrapped voltage phase angle series:

    [`UnwrapAngle(DOM_GPLAINS-BUS1:VH; TVA_SHELBY-BUS1:VH)`](#unwrapangle)

 2. Values from one of the series can now be subtracted from values in both of the series at every 1/30 of a second slice:

    [`SliceSubtract(0.0333, TVA_SHELBY-BUS1:VH, UnwrapAngle(DOM_GPLAINS-BUS1:VH; TVA_SHELBY-BUS1:VH))`](#subtract)

 3. Using a [Slice](#slice) operation on functions that return multiple series can produce multiple values at the same timestamp, however, since values produced by one of the series will now always be zero, the zero values can be excluded:

    [`ExcludeRange(0, 0, SliceSubtract(0.0333, TVA_SHELBY-BUS1:VH, UnwrapAngle(DOM_GPLAINS-BUS1:VH; TVA_SHELBY-BUS1:VH)))`](#excluderange)

### Execution Modes
Each of the series functions include documentation for the mode of execution required by the function. These modes determine the level of processing expense and memory burden incurred by the function. The impacts of the execution modes increase as the time-range or resolution of the series data increases.

| Execution Mode | Description | Impact |
|----------------|-------------|--------|
| _Deferred enumeration_ | Series data will be processed serially outside of function | Minimal processing and memory impact |
| _Immediate enumeration_ | Series data will be processed serially inside the function | Increased processing impact, minimal memory impact |
| _Immediate in-memory array load_ | Series data will be loaded into an array and processed inside the function | Higher processing and memory impact |

## Group Operations

Many Grafana series functions can be operated on in aggregate using a group operator prefix. Each of the series functions includes documentation for the group operation modes allowed by the function.

### Set

Series functions can operate over the set of defined series, producing a single result series, where the target function is executed over each series, horizontally, end-to-end by prefixing the function name with `Set`.

* Example: `SetAverage(FILTER ActiveMeasurements WHERE SignalType='FREQ')`

### Slice

Series functions can operate over the set of defined series, producing one or more result series, where the target function is executed over each series as a group, vertically, per time-slice by prefixing the function name with `Slice`. When operating on a set of series data with a slice function, a new required parameter for time tolerance will be introduced as the first parameter to the function. The parameter is a floating-point value that must be greater than or equal to zero that represents the desired time tolerance, in seconds, for the time slice.

* Example: `SliceSum(0.0333, FILTER ActiveMeasurements WHERE SignalType='IPHM')`

## Special Commands

The following optional special command operations can be specified as part of any filter expression:

| Command | Description |
| ------- | ----------- |
| `DropEmptySeries` | Ensures any empty series are hidden from display. Example: `; dropEmptySeries` |
| `FullResolutionQuery` | Ensures query returns non-decimated, full resolution data. Example: `; fullResolutionData` |
| `IncludePeaks` | Ensures decimated data includes both min/max interval peaks, note this can reduce query performance. Example: `; includePeaks` |
| `RadialDistribution` | Updates query coordinate metadata, i.e., longitude/latitude, where values overlap in a radial distribution. Example: `; radialDistribution` |
| `SquareDistribution` | Updates query coordinate metadata, i.e., longitude/latitude, where values overlap in a square distribution. Example: `; squareDistribution` |
| `Imports={expr}` | Adds custom .NET type imports that can be used with the [`Evaluate`](#evaluate) function. `expr` defines a key-value pair definition of assembly name, i.e., `AssemblyName` = DLL filename without suffix, and type name, i.e., `TypeName` = fully qualified case-sensitive type name, to be imported. Key-value pairs are separated with commas and multiple imports are by separated semi-colons. `expr` must be surrounded by braces. Example: `; imports={AssemblyName=mscorlib, TypeName=System.TimeSpan; AssemblyName=MyCode, TypeName=MyCode.MyClass}` |

## Available Functions

* [`AbsoluteValue`](#absolutevalue)
* [`AddMetadata`](#addmetadata)
* [`Average`](#average)
* [`Bottom`](#bottom)
* [`Ceiling`](#ceiling)
* [`Clamp`](#clamp)
* [`Count`](#count)
* [`Derivative`](#derivative)
* [`Difference`](#difference)
* [`Distinct`](#distinct)
* [`Evaluate`](#evaluate)
* [`ExceedsAt`](#exceedsat)
* [`ExcludeRange`](#excluderange)
* [`FilterNaN`](#filternan)
* [`FilterOutliers`](#filteroutliers)
* [`First`](#first)
* [`Floor`](#floor)
* [`IncludeRange`](#includerange)
* [`Interval`](#interval)
* [`KalmanFilter`](#kalmanfilter)
* [`Label`](#label)
* [`Last`](#last)
* [`Maximum`](#maximum)
* [`Median`](#median)
* [`Minimum`](#minimum)
* [`Mode`](#mode)
* [`Modulo`](#modulo)
* [`MovingAverage`](#movingaverage)
* [`Percentile`](#percentile)
* [`Pow`](#pow)
* [`Random`](#random)
* [`Range`](#range)
* [`Reference`](#reference)
* [`RollingAverage`](#rollingaverage)
* [`Round`](#round)
* [`Scale`](#scale)
* [`Shift`](#shift)
* [`Sqrt`](#sqrt)
* [`StandardDeviation`](#standarddeviation)
* [`Switch`](#switch)
* [`TimeDifference`](#timedifference)
* [`TimeIntegration`](#timeintegration)
* [`Top`](#top)
* [`Total`](#total)
* [`Truncate`](#truncate)
* [`UnwrapAngle`](#unwrapangle)
* [`WrapAngle`](#wrapangle)

## AbsoluteValue

Returns a series of values that represent the absolute value each of the values in the source series. 

* Signature: `AbsoluteValue(expression)`
* Returns: Series of values.
* Example: `AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `AbsoluteValue`, `Abs`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## AddMetadata

Returns a series with an extra MetaData Field. 

* Signature: `AddMetaData(field, value, expression)`
* Returns: Series of values.
* Example: `AddMetaData('Company', 'GPA', FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Average

Returns a single value that represents the mean of the values in the source series. 

* Signature: `Average(expression)`
* Returns: Single value.
* Example: `Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Average`, `Avg`, `Mean`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice, Set

## Bottom

Returns a series of `N`, or `N%` of total, values that are the smallest in the source series. `N` is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100. Third parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true. `N` can either be constant value or a named target available from the expression. Any target values that fall between 0 and 1 will be treated as a percentage. 

* Signature: `Bottom(N|N%, [normalizeTime = true], expression)`
* Returns: Series of values.
* Example: `Bottom(100, false, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Bottom`, `Bot`, `Smallest`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Ceiling

Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series. 

* Signature: `Ceiling(expression)`
* Returns: Series of values.
* Example: `Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Ceiling`, `Ceil`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Clamp

Returns a series of values that represent each of the values in the source series clamped to the inclusive range of `min` and `max`. `min` is lower bound of the result and `max` is the upper bound of the result. 

* Signature: `Clap(min, max, expression)`
* Returns: Series of values.
* Example: `Clamp(49.95, 50.05, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Clamp`, `Limit`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Count

Returns a single value that is the count of the values in the source series. 

* Signature: `Count(expression)`
* Returns: Single value.
* Example: `Count(PPA:1; PPA:2; PPA:3)`
* Variants: `Count`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice, Set

## Derivative

Returns a series of values that represent the rate of change, per time units, for the difference between consecutive values in the source series. The `units` parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Seconds. 

* Signature: `Derivative([units = Seconds], expression)`
* Returns: Series of values.
* Example: `Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Derivative`, `Der`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Set

## Difference

Returns a series of values that represent the difference between consecutive values in the source series. 

* Signature: `Difference(expression)`
* Returns: Series of values.
* Example: `Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Difference`, `Diff`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Distinct

Returns a series of values that represent the unique set of values in the source series. 

* Signature: `Distinct(expression)`
* Returns: Series of values.
* Example: `Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Distinct`, `Unique`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Evaluate

Returns a single value that represents the evaluation of an expression over a slice of the values in the source series. The `sliceTolerance` parameter is a floating-point value that must be greater than or equal to 0.001 that represents the desired time tolerance, in seconds, for the time slice. The `evalExpression` parameter must always be expressed in braces, e.g., `{ expression }`; expression is strongly typed, but not case-sensitive; expression is expected to return a value that can be evaluated as a floating-point number. Aliases of target tag names are used as variable names in the `evalExpression`  when defined. If no alias is defined, all non-valid characters will be removed from target tag name, for example, variable name for tag `PMU.032-PZR_CI:ANG` would be `PMU032PZR_CIANG`. All targets are also available as index suffixed variables named `_v`, for example, first and second target values are available as `_v0` and `_v1`. The `Evaluate` function is always evaluated as a slice, any specified group operation prefix will be ignored. Default system types available to expressions are `System.Math` and `System.DateTime`. See [details on valid expressions](https://www.codeproject.com/Articles/19768/Flee-Fast-Lightweight-Expression-Evaluator). Use the `Imports` command to define more types for `evalExpression`. 

* Signature: `Evaluate(sliceTolerance, evalExpression, filterExpression)`
* Returns: Single value per slice
* Example 1: `Evaluate(0.0333, { R* Sin(T* PI / 180)}, T=GPA_SHELBY-PA1:VH; R=GPA_SHELBY-PM1:V)`
* Example 2: `Eval(0.0333, { (GPA_SHELBYPA2VH - GPA_SHELBYPA1VH) % 360 - 180}, GPA_SHELBY-PA1:VH; GPA_SHELBY-PA2:VH)`
* Example 3: `eval(0.5, { (if (_v0 &gt; 62, _v2, if (_v0 &lt; 57, _v2, _v0)) + if (_v1 &gt; 62, _v2, if (_v1 &lt; 57, _v2, _v1))) / 2 }, FILTER TOP 3 ActiveMeasurements WHERE SignalType = 'FREQ')`
* Example 4: `evaluate(0.0333, { if (abs(b - a) &gt; 180, if (sign(b - a) &lt; 0, b - a + 360, b - a - 360), b - a)}, a=PMU.009-PZR.AV:ANG; b=PMU.008-PZR.AV:ANG)`
* Variants: `Evaluate`, `Eval`
* Execution: [Deferred enumeration](#execution-modes)
* Group Operations: Slice

The following special command-level parameter is available to the `Evaluate` function: `Imports={expr}` This command adds custom .NET type imports that can be used with the `Evaluate` function. `expr`defines a key-value pair definition of assembly name, i.e., `AssemblyName` = DLL filename without suffix, and type name, i.e., `TypeName` = fully qualified case-sensitive type name, to be imported. Key-value pairs are separated with commas and multiple imports are by separated semicolons. `expr` must be surrounded by braces. Example: `; imports={AssemblyName=mscorlib, TypeName=System.TimeSpan; AssemblyName=MyCode, TypeName=MyCode.MyClass}`<br/> 
## ExceedsAt

Returns a series of values at which a value exceeds the given threshold. The `threhsold` parameter value is a floating-point number that represents the threshold to be exceeded. Second parameter, `fallsBelow`, optional, is a boolean flag that determines if the value should be considered inversely as falling below the threshold instead of exceeding. `returnDurations`, optional, is a boolean that determines if the duration (in seconds) from where value exceeded threshold should be returned instead of the original value. Forth parameter, `reportEndMarker`, is a boolean flag that determines if a value should be reported at the point when threshold stops being exceeding the threshold. 

* Signature: `ExceedsAt(threshold, [fallsBelow = false], [returnDurations = false], [reportEndMarker = false], expression)`
* Returns: Series of values.
* Example 1: `ExceedsAt(60.05, false, FILTER ActiveMeasurements WHERE SignalType LIKE '%FREQ')`
* Example 2: `Exceeds(59.95, true, FILTER ActiveMeasurements WHERE SignalType LIKE '%FREQ')`
* Variants: `ExceedsAt`, `Exceeds`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: None

## ExcludeRange

Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high. The `low` and `high` parameter values are floating-point numbers that represent the range of values excluded in the return series. Third parameter, optional, is a boolean flag that determines if range values are inclusive, i.e., excluded values are &lt;= low or &gt;= high - defaults to false, which means values are exclusive, i.e., excluded values are &lt; low or &gt; high. Function allows a fourth optional parameter that is a boolean flag - when four parameters are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive. The `low` and `high` parameter values can either be constant values or named targets available from the expression. 

* Signature: `ExcludeRange(low, high, [inclusive = false], expression)` -or- `ExcludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)`
* Returns: Series of values.
* Example: `ExcludeRange(-180.0, 180.0, true, false, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHA')`
* Variants: `ExcludeRange`, `Exclude`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## FilterNaN

Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN. Parameter `alsoFilterInfinity`, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true. 

* Signature: `FilterNaN([alsoFilterInfinity = true], expression)`
* Returns: Series of values.
* Example: `FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `FilterNaN`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## FilterOutliers

Returns a series of values that represent the core data around the mean, excluding outliers, using Z-score. The `confidence` parameter, optional, is a floating-point value, suffixed with '%' representing a percentage, that must be greater than zero and less than one-hundred, that specifies the amount of data to retain around the mean, representing the central portion of the dataset - defaults to 95%. Second parameter, `invertFilter`, optional, is a boolean flag that determines if outliers should be rejected or retained - default is false, i.e., keep core data rejecting outliers; otherwise, true excludes core data and retains only outliers. Third parameter, `minSamples`, optional, is an integer value that specifies the minimum number of samples required for outlier detection - defaults to 20 for single series temporal analysis and 3 for multi-series slice analysis; if fewer samples are provided, the function returns the entire dataset unfiltered to ensure improved statistical validity of the Z-score calculations, or no data when `invertFilter` is true. If all values are considered identical, i.e., the standard deviation is zero, function will return the entire dataset unfiltered, or no data when `invertFilter` is true, since this represents a uniform distribution. The `confidence` parameter value can either be a constant value or a named target available from the expression. Any target values that fall between 0 and 1 will be treated as a percentage. 

* Signature: `FilterOutliers([confidence = 95%], [invertFilter = false], [minSamples = 20 or 3 for slice], expression)`
* Returns: Series of values.
* Example 1: `FilterOutliers(85%, BROWNS_FERRY:FREQ)`
* Example 2: `SliceFilterOutliers(0.033, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `FilterOutliers`, `ZScoreFilter`, `GaussianFilter`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## First

Returns a series of `N`, or `N%` of total, values from the start of the source series. `N` is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1. `N` can either be constant value or a named target available from the expression. Any target values that fall between 0 and 1 will be treated as a percentage. 

* Signature: `First([N|N% = 1], expression)`
* Returns: Series of values.
* Example: `First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `First`
* Execution: [Immediate in-memory array load](#execution-modes), when `N` is defined; otherwise, immediate enumeration of one, i.e., first value.
* Group Operations: Slice, Set

## Floor

Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series. 

* Signature: `Floor(expression)`
* Returns: Series of values.
* Example: `Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Floor`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## IncludeRange

Returns a series of values that represent a filtered set of the values in the source series where each value falls between the specified low and high. The `low` and `high` parameter values are floating-point numbers that represent the range of values allowed in the return series. Third parameter, optional, is a boolean flag that determines if range values are inclusive, i.e., allowed values are &gt;= low and &lt;= high - defaults to false, which means values are exclusive, i.e., allowed values are &gt; low and &lt; high. Function allows a fourth optional parameter that is a boolean flag - when four parameters are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive. The `low` and `high` parameter values can either be constant values or named targets available from the expression. 

* Signature: `IncludeRange(low, high, [inclusive = false], expression)` -or- `IncludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)`
* Returns: Series of values.
* Example: `IncludeRange(59.90, 60.10, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `IncludeRange`, `Include`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Interval

Returns a series of values that represent a decimated set of the values in the source series based on the specified interval `N`, in time units. `N` is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in time units, for the returned data. The `units`parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Seconds. `N` can either be constant value or a named target available from the expression. 

* Signature: `Interval(N, [units = Seconds], expression)`
* Returns: Series of values.
* Example: `Sum(Interval(5, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))`
* Variants: `Interval`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Set

## KalmanFilter

Returns a series of values that are passed though a Kalman filter which predicts the next state based on the current estimate useful for filtering out noise or reducing variance from a series of values. Optional parameters include `processNoise` which represents how much the system state is expected to change between measurements, `measurementNoise` which represents the confidence in the measurements, and `estimatedError` which represents the initial guess about the error in the state estimate. 

* Signature: `KalmanFilter([processNoise = 1e-5], [measurementNoise = 1e-3], [estimatedError = 1], expression)`
* Returns: Series of values.
* Example: `LQE(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `KalmanFilter`, `LQE`, `LinearQuadraticEstimate`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Label

Renames a series with the specified label `value`. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three series in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3". Group operations on this function will be ignored. Label `value`parameter can be optionally quoted with single or double quotes.

 The label parameter also supports substitutions when root target metadata can be resolved. For series values that directly map to a point tag, metadata value substitutions for the tag can be used in the label value - for example: `{Alias}`, `{ID}`, `{SignalID}`, `{PointTag}`, `{AlternateTag}`, `{SignalReference}`, `{Device}`, `{FramesPerSecond}`, `{Protocol}`, `{ProtocolType}`, `{SignalType}`, `{EngineeringUnits}`, `{PhasorType}`, `{PhasorLabel}`, `{BaseKV}`, `{Company}`, `{Longitude}`, `{Latitude}`, `{Description}`, etc. Each of these fields come from the "ActiveMeasurements" metadata source, as defined in the "ConfigurationEntity" table. Where applicable, substitutions can be used along with fixed label text in any combination, e.g.: `'Series {ID} [{PointTag}]'`.

 Other metadata sources that target time-series measurements can also be used for substitutions so long the source is defined in the "ConfigurationEntity" table and the metadata columns include a "PointTag" field that can be matched to the target Grafana series name. To use any field from another defined metadata source, use the following substitution parameter format: `{TableName.FieldName}`. 

* Signature: `Label(value, expression)`
* Returns: Series of values.
* Example 1: `Label('AvgFreq', SetAvg(FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))`
* Example 2: `Label("{Alias} {EngineeringUnits}", Shelby=GPA_SHELBY:FREQ)`
* Example 3: `Label({AlternateTag}, FILTER TOP 10 ActiveMeasurements WHERE SignalType LIKE '%PH%')`
* Example 4: `Label('Shelby {ScadaTags.CircuitName} MW', FILTER ScadaTags WHERE SignalType='MW' AND Substation='SHELBY')`
* Variants: `Label`, `Name`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: None

## Last

Returns a series of `N`, or `N%` of total, values from the end of the source series. `N`, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1. `N` can either be constant value or a named target available from the expression. Any target values that fall between 0 and 1 will be treated as a percentage. 

* Signature: `Last([N|N% = 1], expression)`
* Returns: Series of values.
* Example: `Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Last`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Maximum

Returns a single value that is the maximum of the values in the source series. 

* Signature: `Maximum(expression)`
* Returns: Single value.
* Example: `Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Maximum`, `Max`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice, Set

## Median

Returns a single value that represents the median of the values in the source series. 

* Signature: `Median(expression)`
* Returns: Single value.
* Example: `Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')`
* Variants: `Median`, `Med`, `Mid`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Minimum

Returns a single value that is the minimum of the values in the source series. 

* Signature: `Minimum(expression)`
* Returns: Single value.
* Example: `Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Minimum`, `Min`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice, Set

## Mode

Returns a single value that represents the mode of the values in the source series. The `numberOfBins` parameter is used to define how many bins to use when computing the mode for float-point values. A value of zero means use a majority-value algorithm which treats all inputs as integer-based values. When using a value of zero for the number of bins, user should consider using an integer function like [Round](#round), with zero digits, [Ceiling](#ceiling), [Floor](#floor) or [Truncate](#truncate) as an input to this function to ensure the conversion of values to integer-based values is handled as expected. 

* Signature: `Mode([numberOfBins = 0], expression)`
* Returns: Single value.
* Example 1: `Mode(FILTER TOP 50 ActiveMeasurements WHERE SignalType='DIGI')`
* Example 2: `Mode(20, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Example 3: `Mode(Round(FILTER ActiveMeasurements WHERE SignalType='FREQ'))`
* Example 4: `Scale(100, true, Mode(0, Floor(Scale(100, FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))))`
* Variants: `Mode`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Modulo

Returns a series of values that represent each of the values in the source series modulo by `N`. `N` is a floating point value representing a divisive factor to be applied to each value the source series. `N` can either be constant value or a named target available from the expression. 

* Signature: `Modulo(N, expression)`
* Returns: Series of values.
* Example: `Mod(2, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `Modulo`, `Modulus`, `Mod`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## MovingAverage

Returns a series of values that represent the moving average of the values in the source series. The `windowSize` parameter, optional, is a positive integer value representing a total number of windows to use for the moving average. If no `windowSize` is provided, the default value is the square root of the total input values in the series. The `windowSize` can either be a constant value or a named target available from the expression. Function operates using a simple moving average (SMA) algorithm. 

* Signature: `MovingAverage([windowSize = sqrt(len)], expression)`
* Returns: Series of values.
* Example: `MovingAvg(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `MovingAverage`, `MovingAvg`, `MovingMean`, `SimpleMovingAverage`, `SMA`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Percentile

Returns a single value that represents the `N`th order percentile for the sorted values in the source series. `N` is a floating point value, representing a percentage, that must range from 0 to 100. 

* Signature: `Percentile(N[%], expression)`
* Returns: Single value.
* Example: `Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `Percentile`, `Pctl`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Pow

Returns a series of values that represent each of the values in the source series raised to the power of `N`. `N` is a floating point value representing an exponent used to raise each value of the source series to the specified power. `N` can either be constant value or a named target available from the expression. 

* Signature: `Pow(N, expression)`
* Returns: Series of values.
* Example: `Pow(2, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Variants: `Pow`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Random

Returns a series of `N`, or `N%` of total, values that are a random sample of the values in the source series. `N` is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100. Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true. `N` can either be constant value or a named target available from the expression. Any target values that fall between 0 and 1 will be treated as a percentage. 

* Signature: `Random(N|N%, [normalizeTime = true], expression)`
* Returns: Series of values.
* Example: `Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `Random`, `Rand`, `Sample`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Range

Returns a single value that represents the range, i.e., `maximum - minimum`, of the values in the source series. 

* Signature: `Range(expression)`
* Returns: Single value.
* Example: `Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Range`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice, Set

## Reference

Returns a slice of angle differences to the first angle (i.e., the reference) for a series of angles. The `sliceTolerance` parameter is a floating-point value that must be greater than or equal to 0.001 that represents the desired time tolerance, in seconds, for the time slice. Parameter `adjustCoordinateMidPoint`, optional, is a boolean flag that determines if the metadata of the coordinate system, i.e., longitude/latitude values, should be adjusted to the midpoint between reference and the angle values in the slice - defaults to false. Parameter `applyWrapOps`, optional, is a boolean flag that determines if angles should be unwrapped before computing differences then rewrapped - defaults to true. The `units`parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees. 

* Signature: `Reference(sliceTolerance, [adjustCoordinateMidPoint = false], [applyWrapOps = true], [units = Degrees], expression)`
* Returns: Single value.
* Example 1: `Ref(0.033, true, false, BROWNS_FERRY:BUS1.ANG; FILTER ActiveMeasurements WHERE SignalType='IPHA')`
* Example 2: `Reference(0.25, BROWNS_FERRY:BUS1; FILTER PhasorValues WHERE SignalType='IPHM')`
* Variants: `Reference`, `Ref`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice

## RollingAverage

Returns a series of values that represent the rolling average of the values in the source series. The `windowSize` parameter, optional, is a positive integer value representing a total number of data points to use for each of the values in the rolling average results. If no `windowSize` is provided, the default value is the square root of the total input values in the series. The `windowSize` can either be constant value or a named target available from the expression. Function operates by producing a mean over each data window. 

* Signature: `RollingAverage([windowSize = sqrt(len)], expression)`
* Returns: Series of values.
* Example: `RollingAvg(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `RollingAverage`, `RollingAvg`, `RollingMean`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Round

Returns a series of values that represent the rounded value, with specified fractional digits, of each of the values in the source series. Parameter `digits`, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0. 

* Signature: `Round([digits = 0], expression)`
* Returns: Series of values.
* Example: `Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Round`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Scale

Returns a series of values that represent each of the values in the source series scaled by `N`. `N` is a floating point value representing a scaling factor (multiplier or reciprocal) to be applied to each value the source series. `N` can either be constant value or a named target available from the expression. The `asReciprocal` is a boolean parameter that, when `true`, requests that `N` be treated as a reciprocal, i.e., 1 / `N`, thus resulting in a division operation instead of multiplication - defaults to `false`. 

* Signature: `Scale(N, [asReciprocal = false], expression)`
* Returns: Series of values.
* Example 1: `Scale(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Example 2: `Scale(0.5, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Example 3: `Scale(60, true, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Scale`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Shift

Returns a series of values that represent each of the values in the source series shifted by `N`. `N` is a floating point value representing an additive (positive or negative) offset to be applied to each value the source series. `N` can either be constant value or a named target available from the expression. 

* Signature: `Shift(N, expression)`
* Returns: Series of values.
* Example 1: `Shift(2.2, FILTER ActiveMeasurements WHERE SignalType='CALC')`
* Example 2: `Shift(-60, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Shift`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## Sqrt

Returns a series of values that represent the square root each of the values in the source series. 

* Signature: `Sqrt(expression)`
* Returns: Series of values.
* Example: `Sqrt(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Sqrt`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## StandardDeviation

Returns a single value that represents the standard deviation of the values in the source series. Parameter `useSampleCalc`, optional, is a boolean flag representing if the sample based calculation should be used - defaults to false, which means the population based calculation should be used. 

* Signature: `StandardDeviation([useSampleCalc = false], expression)`
* Returns: Single value.
* Example: `StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')`
* Variants: `StandardDeviation`, `StdDev`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Switch

Returns a single value selected using the first series of a slice of values as the zero-based index from the remaining series. The `sliceTolerance` parameter is a floating-point value that must be greater than or equal to 0.001 that represents the desired time tolerance, in seconds, for the time slice. 

* Signature: `Switch(sliceTolerance, expression)`
* Returns: Single value.
* Example: `Switch(IndexSeriesTag; FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Switch`, `Select`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice

## TimeDifference

Returns a series of values that represent the time difference, in time units, between consecutive values in the source series. The `units` parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Seconds. 

* Signature: `TimeDifference([units = Seconds], expression)`
* Returns: Series of values.
* Example: `TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `TimeDifference`, `TimeDiff`, `Elapsed`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Set

## TimeIntegration

Returns a single value that represents the time-based integration, i.e., the sum of `V(n) * (T(n) - T(n-1))` where time difference is calculated in the specified time units of the values in the source series. The `units`parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Hours. 

* Signature: `TimeIntegration([units = Hours], expression)`
* Returns: Single value.
* Example: `TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')`
* Variants: `TimeIntegration`, `TimeInt`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Set

## Top

Returns a series of `N`, or `N%` of total, values that are the largest in the source series. `N` is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100. Third parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true. `N` can either be constant value or a named target available from the expression. Any target values that fall between 0 and 1 will be treated as a percentage. 

* Signature: `Top(N|N%, [normalizeTime = true], expression)`
* Returns: Series of values.
* Example: `Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')`
* Variants: `Top`, `Largest`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## Total

Returns a single value that represents the sum of the values in the source series. 

* Signature: `Total(expression)`
* Returns: Single value.
* Example: `Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Total`, `Add`, `Sum`
* Execution: [Immediate enumeration](#execution-modes).
* Group Operations: Slice, Set

## Truncate

Returns a series of values that represent the integral part of each of the values in the source series. 

* Signature: `Truncate(expression)`
* Returns: Series of values.
* Example: `Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')`
* Variants: `Truncate`, `Trunc`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set

## UnwrapAngle

Returns a series of values that represent an adjusted set of angles that are unwrapped, per specified angle units, so that a comparable mathematical operation can be executed. For example, for angles that wrap between -180 and +180 degrees, this algorithm unwraps the values to make the values mathematically comparable. The `units`parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees. 

* Signature: `UnwrapAngle([units = Degrees], expression)`
* Returns: Series of values.
* Example: `UnwrapAngle(FSX_PMU2-PA1:VH; REA_PMU3-PA2:VH)`
* Variants: `UnwrapAngle`, `Unwrap`
* Execution: [Immediate in-memory array load](#execution-modes).
* Group Operations: Slice, Set

## WrapAngle

Returns a series of values that represent an adjusted set of angles that are wrapped, per specified angle units, so that angle values are consistently between -180 and +180 degrees. The `units`parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees. 

* Signature: `WrapAngle([units = Degrees], expression)`
* Returns: Series of values.
* Example: `WrapAngle(Radians, FILTER TOP 5 ActiveMeasurements WHERE SignalType LIKE '%PHA')`
* Variants: `WrapAngle`, `Wrap`
* Execution: [Deferred enumeration](#execution-modes).
* Group Operations: Slice, Set
