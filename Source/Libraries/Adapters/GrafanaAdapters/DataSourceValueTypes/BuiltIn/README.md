### These are the Built-In data source value types for any Grafana Data Source

New data source values types can be added by creating a new assembly that contains the data type and dropping it in the host application folder, commonly wherever the `GrafanaAdapters.dll` assembly is located. The host application will automatically load any assemblies that implement the [`IDataSourceValueType`](../IDataSourceValueType.cs) interface and make them available to all Grafana data source implementations.

Any new data source type is required to be a `struct` and must implement the [`IDataSourceValueType`](../IDataSourceValueType.cs) interface. See examples in this folder for more details.