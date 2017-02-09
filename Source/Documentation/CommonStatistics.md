# Common Time-series Statistics

| Source | Name | Description |
|:------:|------|-------------|
| Device | Data Quality Errors | Number of data quality errors reported by device during last reporting interval. |
| Device | Time Quality Errors | Number of time quality errors reported by device during last reporting interval. |
| Device | Measurements Received | Number of measurements received from device during last reporting interval. |
| Device | Device Errors | Number of device errors reported by device during last reporting interval. |
| Device | Measurements Expected | Expected number of measurements received from device during last reporting interval. |
| Device | Measurements With Error | Number of measurements received while device was reporting errors during last reporting interval. |
| Device | Measurements Defined | Number of defined measurements (per frame) from device during last reporting interval. |
| InputStream | Last Report Time | Timestamp of last received data frame from input stream. |
| InputStream | Total Frames | Total number of frames received from input stream during last reporting interval. |
| InputStream | Missing Frames | Number of frames that were not received from input stream during last reporting interval. |
| InputStream | Missing Data | Number of data units that were not received at least once from input stream during last reporting interval. |
| InputStream | Total Data Frames | Number of data frames received from input stream during last reporting interval. |
| InputStream | Total Configuration Frames | Number of configuration frames received from input stream during last reporting interval. |
| InputStream | Total Header Frames | Number of header frames received from input stream during last reporting interval. |
| InputStream | Received Configuration | Boolean value representing if input stream has received (or has cached) a configuration frame during last reporting interval. |
| InputStream | Configuration Changes | Number of configuration changes reported by input stream during last reporting interval. |
| InputStream | Minimum Latency | Minimum latency from input stream, in milliseconds, during last reporting interval. |
| InputStream | Maximum Latency | Maximum latency from input stream, in milliseconds, during last reporting interval. |
| InputStream | Average Latency | Average latency, in milliseconds, for data received from input stream during last reporting interval. |
| InputStream | Defined Frame Rate | Frame rate as defined by input stream during last reporting interval. |
| InputStream | Actual Frame Rate | Latest actual mean frame rate for data received from input stream during last reporting interval. |
| InputStream | Actual Data Rate | Latest actual mean Mbps data rate for data received from input stream during last reporting interval. |
| InputStream | CRC Errors | Number of CRC errors reported from input stream during last reporting interval. |
| InputStream | Out of Order Frames | Number of out-of-order frames received from input stream during last reporting interval. |
| InputStream | Input Stream Connected | Boolean value representing if input stream was continually connected during last reporting interval. |
| InputStream | Total Bytes Received | Number of bytes received from the input source during last reporting interval. |
| InputStream | Lifetime Measurements | Number of processed measurements reported by the input stream during the lifetime of the input stream. |
| InputStream | Lifetime Bytes Received | Number of bytes received from the input source during the lifetime of the input stream. |
| InputStream | Minimum Measurements Per Second | The minimum number of measurements received per second during the last reporting interval. |
| InputStream | Maximum Measurements Per Second | The maximum number of measurements received per second during the last reporting interval. |
| InputStream | Average Measurements Per Second | The maximum number of measurements received per second during the last reporting interval. |
| InputStream | Lifetime Minimum Latency | Minimum latency from input stream, in milliseconds, during the lifetime of the input stream. |
| InputStream | Lifetime Maximum Latency | Maximum latency from input stream, in milliseconds, during the lifetime of the input stream. |
| InputStream | Lifetime Average Latency | Average latency, in milliseconds, for data received from input stream during the lifetime of the input stream. |
| InputStream | Up Time | Total number of seconds input stream has been running. |
| OutputStream | Expected Measurements | Number of expected measurements reported by the output stream during last reporting interval. |
| OutputStream | Received Measurements | Number of received measurements reported by the output stream during last reporting interval. |
| OutputStream | Processed Measurements | Number of processed measurements reported by the output stream during last reporting interval. |
| OutputStream | Discarded Measurements | Number of discarded measurements reported by output stream during last reporting interval. |
| OutputStream | Published Measurements | Number of published measurements reported by output stream during last reporting interval. |
| OutputStream | Downsampled Measurements | Number of downsampled measurements reported by the output stream during last reporting interval. |
| OutputStream | Measurements Sorted by Arrival | Number of measurements sorted by arrival reported by the output stream during last reporting interval. |
| OutputStream | Missed Sorts by Timeout | Number of missed sorts by timeout reported by the output stream during last reporting interval. |
| OutputStream | Frames Ahead of Schedule | Number of frames ahead of schedule reported by the output stream during last reporting interval. |
| OutputStream | Published Frames | Number of published frames reported by the output stream during last reporting interval. |
| OutputStream | Output Stream Connected | Boolean value representing if the output stream was continually connected during last reporting interval. |
| OutputStream | Minimum Latency | Minimum latency from output stream, in milliseconds, during last reporting interval. |
| OutputStream | Maximum Latency | Maximum latency from output stream, in milliseconds, during last reporting interval. |
| OutputStream | Average Latency | Average latency, in milliseconds, for data published from output stream during last reporting interval. |
| OutputStream | Connected Clients | Number of clients connected to the command channel of the output stream during last reporting interval. |
| OutputStream | Total Bytes Sent | Number of bytes sent from output stream during last reporting interval. |
| OutputStream | Lifetime Measurements | Number of processed measurements reported by the output stream during the lifetime of the output stream. |
| OutputStream | Lifetime Bytes Sent | Number of bytes sent from the output source during the lifetime of the output stream. |
| OutputStream | Minimum Measurements Per Second | The minimum number of measurements sent per second during the last reporting interval. |
| OutputStream | Maximum Measurements Per Second | The maximum number of measurements sent per second during the last reporting interval. |
| OutputStream | Average Measurements Per Second | The maximum number of measurements sent per second during the last reporting interval. |
| OutputStream | Lifetime Minimum Latency | Minimum latency from output stream, in milliseconds, during the lifetime of the output stream. |
| OutputStream | Lifetime Maximum Latency | Maximum latency from output stream, in milliseconds, during the lifetime of the output stream. |
| OutputStream | Lifetime Average Latency | Average latency from output stream, in milliseconds, during the lifetime of the output stream. |
| OutputStream | Lifetime Discarded Measurements | Number of discarded measurements reported by output stream during the lifetime of the output stream. |
| OutputStream | Lifetime Downsampled Measurements | Number of downsampled measurements reported by the output stream during the lifetime of the output stream. |
| OutputStream | Up Time | Total number of seconds output stream has been running. |
| Publisher | Publisher Connected | Boolean value representing if the publisher was continually connected during last reporting interval. |
| Publisher | Connected Clients | Number of clients connected to the command channel of the publisher during last reporting interval. |
| Publisher | Processed Measurements | Number of processed measurements reported by the publisher during last reporting interval. |
| Publisher | Total Bytes Sent | Number of bytes sent by the publisher during the last reporting interval. |
| Publisher | Lifetime Measurements | Number of processed measurements reported by the publisher during the lifetime of the publisher. |
| Publisher | Lifetime Bytes Sent | Number of bytes sent by the publisher during the lifetime of the publisher. |
| Publisher | Minimum Measurements Per Second | The minimum number of measurements sent per second during the last reporting interval. |
| Publisher | Maximum Measurements Per Second | The maximum number of measurements sent per second during the last reporting interval. |
| Publisher | Average Measurements Per Second | The average number of measurements sent per second during the last reporting interval. |
| Publisher | Lifetime Minimum Latency | Minimum latency from output stream, in milliseconds, during the lifetime of the publisher. |
| Publisher | Lifetime Maximum Latency | Maximum latency from output stream, in milliseconds, during the lifetime of the publisher. |
| Publisher | Lifetime Average Latency | Average latency from output stream, in milliseconds, during the lifetime of the publisher. |
| Publisher | Up Time | Total number of seconds publisher has been running. |
| Subscriber | Subscriber Connected | Boolean value representing if the subscriber was continually connected during last reporting interval. |
| Subscriber | Processed Measurements | Number of processed measurements reported by the subscriber during last reporting interval. |
| Subscriber | Total Bytes Received | Number of bytes received from subscriber during last reporting interval. |
| Subscriber | Authorized Signal Count | Number of signals authorized to the subscriber by the publisher. |
| Subscriber | Subscriber Authenticated | Boolean value representing if the subscriber was authenticated to the publisher during last reporting interval. |
| Subscriber | Unauthorized Signal Count | Number of signals denied to the subscriber by the publisher. |
| Subscriber | Lifetime Measurements | Number of processed measurements reported by the subscriber during the lifetime of the subscriber. |
| Subscriber | Lifetime Bytes Received | Number of bytes received from subscriber during the lifetime of the subscriber. |
| Subscriber | Minimum Measurements Per Second | The minimum number of measurements received per second during the last reporting interval. |
| Subscriber | Maximum Measurements Per Second | The maximum number of measurements received per second during the last reporting interval. |
| Subscriber | Average Measurements Per Second | The average number of measurements received per second during the last reporting interval. |
| Subscriber | Lifetime Minimum Latency | Minimum latency from output stream, in milliseconds, during the lifetime of the subscriber. |
| Subscriber | Lifetime Maximum Latency | Maximum latency from output stream, in milliseconds, during the lifetime of the subscriber. |
| Subscriber | Lifetime Average Latency | Average latency from output stream, in milliseconds, during the lifetime of the subscriber. |
| Subscriber | Up Time | Total number of seconds subscriber has been running. |
| System | CPU Usage | Percentage of CPU currently used by this process. |
| System | Average CPU Usage | Average percentage of CPU used by this process. |
| System | Memory Usage | Amount of memory currently used by this process in megabytes. |
| System | Average Memory Usage | Average amount of memory used by this process in megabytes. |
| System | Thread Count | Number of threads currently used by this process. |
| System | Average Thread Count | Average number of threads used by this process. |
| System | Threading Contention Rate | Current thread lock contention rate in attempts per second. |
| System | Average Threading Contention Rate | Average thread lock contention rate in attempts per second. |
| System | IO Usage | Amount of IO currently used by this process in kilobytes per second. |
| System | Average IO Usage | Average amount of IO used by this process in kilobytes per second. |
| System | IP Data Send Rate | Number of IP datagrams (or bytes on Mono) currently sent by this process per second. |
| System | Average IP Data Send Rate | Average number of IP datagrams (or bytes on Mono) sent by this process per second. |
| System | IP Data Receive Rate | Number of IP datagrams (or bytes on Mono) currently received by this process per second. |
| System | Average IP Data Receive Rate | Average number of IP datagrams (or bytes on Mono) received by this process per second. |
| System | Up Time | Total number of seconds system has been running. |
