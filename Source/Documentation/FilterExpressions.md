# Filter Expressions

Filter expressions can be used to filter inputs and outputs for streams, subscribers and adapters.
This syntax is similar to a SQL `WHERE` clause, but does not implement the full SQL language. See [DataTable.Select()](https://msdn.microsoft.com/en-us/library/system.data.datatable.select(v=vs.110).aspx) for more information.

## Filtering Syntax
```sql
FILTER <TableName> [TOP n] WHERE <Expression> [ORDER BY <SortField>]
```

## Available Options and Clauses
| Keyword | Example | Description | Required?|
|---------|---------|-------------|:----------:|
| FILTER | See [Examples](#examples) | Starts the filter expression | Yes|
| TOP n| TOP 100 | Selects only the first number of items | No|
| WHERE &lt;Expression&gt; | WHERE SignalType='FREQ' | Uses [DataTable.Select(string)](https://msdn.microsoft.com/en-us/library/det4aw50(v=vs.110).aspx) | Yes |
| ORDER BY &lt;ColumnName&gt; | ORDER BY SignalType | Uses [DataTable.Select(string, string)](https://msdn.microsoft.com/en-us/library/way3dy9w(v=vs.110).aspx) | No |

## Examples

An example input filter to only pass measurements with the company of `GSF` and type of `Frequency(FREQ)`.
```sql
inputMeasurementKeys = {
    FILTER ActiveMeasurements WHERE Company='GSF' AND SignalType='FREQ' ORDER BY ID
};
```

An example input filter to only pass `Statistics(STAT)` measurments.
```sql
inputMeasurementKeys = {
    FILTER ActiveMeasurements WHERE SignalType = 'STAT'
};
```

An example output filter to only send `Current Angle` and `Voltage Angle` for the `Positive Sequence(+)` measurments.
```sql
outputMeasurements = {
    FILTER ActiveMeasurements WHERE SignalType IN ('IPHA','VPHA') AND Phase='+' ORDER BY PhasorID
};
```

See [more examples](http://www.csharp-examples.net/dataview-rowfilter/) of allowed `DataTable.Select()` syntax.

## Available Table Definitions

Available table defintions are defined in the `ConfigurationEntity` table of the host [Time-Series Library](https://www.gridprotectionalliance.org/technology.asp#TSL) application configuration database.

For selection of input and output measurements for adapters, `ActiveMeasurements` is the most common source. Other commonly defined tables are listed here for reference as well.

 * [ActiveMeasurements](#activemeasurements) 
 * [ActionAdapters](#actionadapters) 
 * [Alarms](#alarms) 
 * [ConfigurationDataSet](#configurationdataset) 
 * [ConfigurationEntity](#configurationentity) 
 * [InputAdapters](#inputadapters) 
 * [InputStreamDevices](#inputstreamdevices) 
 * [MeasurementGroupMeasurements](#measurementgroupmeasurements) 
 * [MeasurementGroups](#measurementgroups) 
 * [NodeInfo](#nodeinfo) 
 * [OutputAdapters](#outputadapters) 
 * [OutputStreamDevicePhasors](#outputstreamdevicephasors) 
 * [OutputStreamDevices](#outputstreamdevices) 
 * [OutputStreamMeasurements](#outputstreammeasurements) 
 * [Statistics](#statistics) 
 * [SubscriberMeasurementGroups](#subscribermeasurementgroups) 
 * [SubscriberMeasurements](#subscribermeasurements) 
 * [Subscribers](#subscribers) 

### ActiveMeasurements
| ColumnName       | DataType  |
|------------------|-----------|
|SourceNodeID| Guid|
|ID| string|
|SignalID| Guid|
|PointTag| string|
|AlternateTag| string|
|SignalReference| string|
|Internal| int|
|Subscribed| int|
|Device| string|
|DeviceID| int|
|FramesPerSecond| int|
|Protocol| string|
|ProtocolType| string|
|SignalType| string|
|EngineeringUnits| string|
|PhasorID| int|
|PhasorType| string|
|Phase| string|
|Adder| double|
|Multiplier| double|
|Company| string|
|Longitude| Decimal|
|Latitude| Decimal|
|Description| string|
|UpdatedOn| DateTime|

### ActionAdapters
| ColumnName       | DataType  |
|------------------|-----------|
|ID| int|
|AdapterName| string|
|AssemblyName| string|
|TypeName| string|
|Connectionstring| string|

### Alarms
| ColumnName       | DataType  |
|------------------|-----------|
|ID| int|
|TagName| string|
|SignalID| Guid|
|AssociatedMeasurementID| Guid|
|Description| string|
|Severity| int|
|Operation| int|
|SetPoint| double|
|Tolerance| double|
|Delay| double|
|Hysteresis| double|
|LoadOrder| int|
|Enabled| bool|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|

### ConfigurationDataSet
| ColumnName       | DataType  |
|------------------|-----------|
|Version| string|

### ConfigurationEntity
| ColumnName       | DataType  |
|------------------|-----------|
|SourceName| string|
|RuntimeName| string|
|Description| string|
|LoadOrder| int|
|Enabled| bool|

### InputAdapters
| ColumnName       | DataType  |
|------------------|-----------|
|ID| int|
|AdapterName| string|
|AssemblyName| string|
|TypeName| string|
|Connectionstring| string|

### InputStreamDevices
| ColumnName       | DataType  |
|------------------|-----------|
|ParentID| int|
|ID| int|
|Acronym| string|
|Name| string|
|AccessID| int|

### MeasurementGroupMeasurements
| ColumnName       | DataType  |
|------------------|-----------|
|MeasurementGroupID| int|
|SignalID| Guid|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|

### MeasurementGroups
| ColumnName       | DataType  |
|------------------|-----------|
|ID| int|
|Name| string|
|Description| string|
|FilterExpression| string|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|

### NodeInfo
| ColumnName       | DataType  |
|------------------|-----------|
|Name| string|
|CompanyName| string|
|Longitude| Decimal|
|Latitude| Decimal|
|Description| string|
|ImagePath| string|
|Settings| string|
|MenuType| string|
|MenuData| string|
|Master| bool|
|Enabled| bool|

### OutputAdapters
| ColumnName       | DataType  |
|------------------|-----------|
|ID| int|
|AdapterName| string|
|AssemblyName| string|
|TypeName| string|
|Connectionstring| string|

### OutputStreamDevicePhasors
| ColumnName       | DataType  |
|------------------|-----------|
|OutputStreamDeviceID| int|
|ID| int|
|Label| string|
|Type| string|
|Phase| string|
|ScalingValue| int|
|LoadOrder| int|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|
|OutputStreamDeviceAnalogs||
|OutputStreamDeviceID| int|
|ID| int|
|Label| string|
|Type| int|
|ScalingValue| int|
|LoadOrder| int|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|
|OutputStreamDeviceDigitals||
|OutputStreamDeviceID| int|
|ID| int|
|Label| string|
|MaskValue| int|
|LoadOrder| int|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|

### OutputStreamDevices
| ColumnName       | DataType  |
|------------------|-----------|
|ParentID| int|
|ID| int|
|IDCode| int|
|Acronym| string|
|BpaAcronym| string|
|Name| string|
|PhasorDataFormat| string|
|FrequencyDataFormat| string|
|AnalogDataFormat| string|
|CoordinateFormat| string|
|LoadOrder| int|

### OutputStreamMeasurements
| ColumnName       | DataType  |
|------------------|-----------|
|AdapterID| int|
|Historian| string|
|PointID| int|
|SignalReference| string|

### Statistics
| ColumnName       | DataType  |
|------------------|-----------|
|ID|int|
|Source|string|
|SignalIndex|int|
|Name|string|
|Description|string|
|AssemblyName|string|
|TypeName|string|
|MethodName|string|
|Arguments|string|
|IsConnectedState|bool|
|DataType|string|
|DisplayFormat|string|
|Enabled|bool|

### SubscriberMeasurementGroups
| ColumnName       | DataType  |
|------------------|-----------|
|SubscriberID| Guid|
|MeasurementGroupID| int|
|Allowed| bool|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|

### SubscriberMeasurements
| ColumnName       | DataType  |
|------------------|-----------|
|SubscriberID| Guid|
|SignalID| Guid|
|Allowed| bool|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|

### Subscribers
| ColumnName       | DataType  |
|------------------|-----------|
|ID| Guid|
|Acronym| string|
|Name| string|
|SharedSecret| string|
|AuthKey| string|
|ValidIPAddresses| string|
|RemoteCertificateFile| string|
|ValidPolicyErrors| string|
|ValidChainFlags| string|
|AccessControlFilter| string|
|Enabled| bool|
|CreatedOn| DateTime|
|CreatedBy| string|
|UpdatedOn| DateTime|
|UpdatedBy| string|
