![Logo](https://www.gridprotectionalliance.org/images/products/ProductTitles75/tsl.png)

The Time-Series Library (TSL) included within the Grid Solutions Framework is a powerful collection of methods to process real-time streaming data. Implemented with an adapter-based extensibility model for pluggable inputs, actions and outputs, the TSL allows fully customizable real-time data processing. Systems using the TSL inherit the ability to manage, process and respond to fast-moving time-series data at low latency.

The TSL can also be used to manage time-series data that includes an accurate time-stamp at the moment of measurement, such as values acquired from source devices with GPS time-sources. Time-series values with accurate time-stamps can be routed to user-developed adapters that inherit the ability to time-align, i.e., concentrate, values with other time-series values that were measured at the same time. Once time-series values are aligned by time, an action - or calculation - can be performed over a slice of all the data received for the same time-stamp.

![TSL Overview](https://gridprotectionalliance.org/images/technology/TSLoverview540R1.png)

Because of its modular design, systems implemented using the TSL become general purpose distributed stream processing engines. New fully self-contained, derived time-series based applications can be easily developed using the TSL that include all the needed user interface components for configuration management, database schemas for various databases systems (e.g., SQL Server, MySQL, Oracle and SQLite), and historical time-series data archiving. See the GSF [Time-Series Library components overview](https://gridprotectionalliance.org/docs/products/gsf/tsl-components-2015.pdf) for more information.
