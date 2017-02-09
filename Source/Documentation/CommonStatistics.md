# Common Time-Series Statistics

| Source | Tag Suffix | Name | Description |
|--------|:----------:|------|-------------|
| Device | !PMU-ST1 | Data Quality Errors | Number of data quality errors reported by device during last reporting interval. |
| Device | !PMU-ST2 | Time Quality Errors | Number of time quality errors reported by device during last reporting interval. |
| Device | !PMU-ST3 | Device Errors | Number of device errors reported by device during last reporting interval. |
| Device | !PMU-ST4 | Measurements Received | Number of measurements received from device during last reporting interval. |
| Device | !PMU-ST5 | Measurements Expected | Expected number of measurements received from device during last reporting interval. |
| Device | !PMU-ST6 | Measurements With Error | Number of measurements received while device was reporting errors during last reporting interval. |
| Device | !PMU-ST7 | Measurements Defined | Number of defined measurements (per frame) from device during last reporting interval. |
| InputStream | !IS-ST2 | Last Report Time | Timestamp of last received data frame from input stream. |
| InputStream | !IS-ST1 | Total Frames | Total number of frames received from input stream during last reporting interval. |
| InputStream | !IS-ST3 | Missing Frames | Number of frames that were not received from input stream during last reporting interval. |
| InputStream | !IS-ST18 | Missing Data | Number of data units that were not received at least once from input stream during last reporting interval. |
| InputStream | !IS-ST11 | Total Data Frames | Number of data frames received from input stream during last reporting interval. |
| InputStream | !IS-ST12 | Total Configuration Frames | Number of configuration frames received from input stream during last reporting interval. |
| InputStream | !IS-ST13 | Total Header Frames | Number of header frames received from input stream during last reporting interval. |
| InputStream | !IS-ST9 | Received Configuration | Boolean value representing if input stream has received (or has cached) a configuration frame during last reporting interval. |
| InputStream | !IS-ST10 | Configuration Changes | Number of configuration changes reported by input stream during last reporting interval. |
| InputStream | !IS-ST6 | Minimum Latency | Minimum latency from input stream, in milliseconds, during last reporting interval. |
| InputStream | !IS-ST7 | Maximum Latency | Maximum latency from input stream, in milliseconds, during last reporting interval. |
| InputStream | !IS-ST14 | Average Latency | Average latency, in milliseconds, for data received from input stream during last reporting interval. |
| InputStream | !IS-ST15 | Defined Frame Rate | Frame rate as defined by input stream during last reporting interval. |
| InputStream | !IS-ST16 | Actual Frame Rate | Latest actual mean frame rate for data received from input stream during last reporting interval. |
| InputStream | !IS-ST17 | Actual Data Rate | Latest actual mean Mbps data rate for data received from input stream during last reporting interval. |
| InputStream | !IS-ST4 | CRC Errors | Number of CRC errors reported from input stream during last reporting interval. |
| InputStream | !IS-ST5 | Out of Order Frames | Number of out-of-order frames received from input stream during last reporting interval. |
| InputStream | !IS-ST8 | Input Stream Connected | Boolean value representing if input stream was continually connected during last reporting interval. |
| InputStream | !IS-ST19 | Total Bytes Received | Number of bytes received from the input source during last reporting interval. |
| InputStream | !IS-ST20 | Lifetime Measurements | Number of processed measurements reported by the input stream during the lifetime of the input stream. |
| InputStream | !IS-ST21 | Lifetime Bytes Received | Number of bytes received from the input source during the lifetime of the input stream. |
| InputStream | !IS-ST22 | Minimum Measurements Per Second | The minimum number of measurements received per second during the last reporting interval. |
| InputStream | !IS-ST23 | Maximum Measurements Per Second | The maximum number of measurements received per second during the last reporting interval. |
| InputStream | !IS-ST24 | Average Measurements Per Second | The maximum number of measurements received per second during the last reporting interval. |
| InputStream | !IS-ST25 | Lifetime Minimum Latency | Minimum latency from input stream, in milliseconds, during the lifetime of the input stream. |
| InputStream | !IS-ST26 | Lifetime Maximum Latency | Maximum latency from input stream, in milliseconds, during the lifetime of the input stream. |
| InputStream | !IS-ST27 | Lifetime Average Latency | Average latency, in milliseconds, for data received from input stream during the lifetime of the input stream. |
| InputStream | !IS-ST28 | Up Time | Total number of seconds input stream has been running. |
| OutputStream | !OS-ST3 | Expected Measurements | Number of expected measurements reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST2 | Received Measurements | Number of received measurements reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST4 | Processed Measurements | Number of processed measurements reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST1 | Discarded Measurements | Number of discarded measurements reported by output stream during last reporting interval. |
| OutputStream | !OS-ST6 | Published Measurements | Number of published measurements reported by output stream during last reporting interval. |
| OutputStream | !OS-ST7 | Downsampled Measurements | Number of downsampled measurements reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST5 | Measurements Sorted by Arrival | Number of measurements sorted by arrival reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST8 | Missed Sorts by Timeout | Number of missed sorts by timeout reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST9 | Frames Ahead of Schedule | Number of frames ahead of schedule reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST10 | Published Frames | Number of published frames reported by the output stream during last reporting interval. |
| OutputStream | !OS-ST11 | Output Stream Connected | Boolean value representing if the output stream was continually connected during last reporting interval. |
| OutputStream | !OS-ST12 | Minimum Latency | Minimum latency from output stream, in milliseconds, during last reporting interval. |
| OutputStream | !OS-ST13 | Maximum Latency | Maximum latency from output stream, in milliseconds, during last reporting interval. |
| OutputStream | !OS-ST14 | Average Latency | Average latency, in milliseconds, for data published from output stream during last reporting interval. |
| OutputStream | !OS-ST15 | Connected Clients | Number of clients connected to the command channel of the output stream during last reporting interval. |
| OutputStream | !OS-ST16 | Total Bytes Sent | Number of bytes sent from output stream during last reporting interval. |
| OutputStream | !OS-ST17 | Lifetime Measurements | Number of processed measurements reported by the output stream during the lifetime of the output stream. |
| OutputStream | !OS-ST18 | Lifetime Bytes Sent | Number of bytes sent from the output source during the lifetime of the output stream. |
| OutputStream | !OS-ST19 | Minimum Measurements Per Second | The minimum number of measurements sent per second during the last reporting interval. |
| OutputStream | !OS-ST20 | Maximum Measurements Per Second | The maximum number of measurements sent per second during the last reporting interval. |
| OutputStream | !OS-ST21 | Average Measurements Per Second | The maximum number of measurements sent per second during the last reporting interval. |
| OutputStream | !OS-ST22 | Lifetime Minimum Latency | Minimum latency from output stream, in milliseconds, during the lifetime of the output stream. |
| OutputStream | !OS-ST23 | Lifetime Maximum Latency | Maximum latency from output stream, in milliseconds, during the lifetime of the output stream. |
| OutputStream | !OS-ST24 | Lifetime Average Latency | Average latency from output stream, in milliseconds, during the lifetime of the output stream. |
| OutputStream | !OS-ST25 | Lifetime Discarded Measurements | Number of discarded measurements reported by output stream during the lifetime of the output stream. |
| OutputStream | !OS-ST26 | Lifetime Downsampled Measurements | Number of downsampled measurements reported by the output stream during the lifetime of the output stream. |
| OutputStream | !OS-ST27 | Up Time | Total number of seconds output stream has been running. |
| Publisher | !PUB-ST1 | Publisher Connected | Boolean value representing if the publisher was continually connected during last reporting interval. |
| Publisher | !PUB-ST2 | Connected Clients | Number of clients connected to the command channel of the publisher during last reporting interval. |
| Publisher | !PUB-ST3 | Processed Measurements | Number of processed measurements reported by the publisher during last reporting interval. |
| Publisher | !PUB-ST4 | Total Bytes Sent | Number of bytes sent by the publisher during the last reporting interval. |
| Publisher | !PUB-ST5 | Lifetime Measurements | Number of processed measurements reported by the publisher during the lifetime of the publisher. |
| Publisher | !PUB-ST6 | Lifetime Bytes Sent | Number of bytes sent by the publisher during the lifetime of the publisher. |
| Publisher | !PUB-ST7 | Minimum Measurements Per Second | The minimum number of measurements sent per second during the last reporting interval. |
| Publisher | !PUB-ST8 | Maximum Measurements Per Second | The maximum number of measurements sent per second during the last reporting interval. |
| Publisher | !PUB-ST9 | Average Measurements Per Second | The average number of measurements sent per second during the last reporting interval. |
| Publisher | !PUB-ST10 | Lifetime Minimum Latency | Minimum latency from output stream, in milliseconds, during the lifetime of the publisher. |
| Publisher | !PUB-ST11 | Lifetime Maximum Latency | Maximum latency from output stream, in milliseconds, during the lifetime of the publisher. |
| Publisher | !PUB-ST12 | Lifetime Average Latency | Average latency from output stream, in milliseconds, during the lifetime of the publisher. |
| Publisher | !PUB-ST13 | Up Time | Total number of seconds publisher has been running. |
| Subscriber | !SUB-ST1 | Subscriber Connected | Boolean value representing if the subscriber was continually connected during last reporting interval. |
| Subscriber | !SUB-ST3 | Processed Measurements | Number of processed measurements reported by the subscriber during last reporting interval. |
| Subscriber | !SUB-ST4 | Total Bytes Received | Number of bytes received from subscriber during last reporting interval. |
| Subscriber | !SUB-ST5 | Authorized Signal Count | Number of signals authorized to the subscriber by the publisher. |
| Subscriber | !SUB-ST2 | Subscriber Authenticated | Boolean value representing if the subscriber was authenticated to the publisher during last reporting interval. |
| Subscriber | !SUB-ST6 | Unauthorized Signal Count | Number of signals denied to the subscriber by the publisher. |
| Subscriber | !SUB-ST7 | Lifetime Measurements | Number of processed measurements reported by the subscriber during the lifetime of the subscriber. |
| Subscriber | !SUB-ST8 | Lifetime Bytes Received | Number of bytes received from subscriber during the lifetime of the subscriber. |
| Subscriber | !SUB-ST9 | Minimum Measurements Per Second | The minimum number of measurements received per second during the last reporting interval. |
| Subscriber | !SUB-ST10 | Maximum Measurements Per Second | The maximum number of measurements received per second during the last reporting interval. |
| Subscriber | !SUB-ST11 | Average Measurements Per Second | The average number of measurements received per second during the last reporting interval. |
| Subscriber | !SUB-ST12 | Lifetime Minimum Latency | Minimum latency from output stream, in milliseconds, during the lifetime of the subscriber. |
| Subscriber | !SUB-ST13 | Lifetime Maximum Latency | Maximum latency from output stream, in milliseconds, during the lifetime of the subscriber. |
| Subscriber | !SUB-ST14 | Lifetime Average Latency | Average latency from output stream, in milliseconds, during the lifetime of the subscriber. |
| Subscriber | !SUB-ST15 | Up Time | Total number of seconds subscriber has been running. |
| System | !SYSTEM-ST1 | CPU Usage | Percentage of CPU currently used by this process. |
| System | !SYSTEM-ST2 | Average CPU Usage | Average percentage of CPU used by this process. |
| System | !SYSTEM-ST3 | Memory Usage | Amount of memory currently used by this process in megabytes. |
| System | !SYSTEM-ST4 | Average Memory Usage | Average amount of memory used by this process in megabytes. |
| System | !SYSTEM-ST5 | Thread Count | Number of threads currently used by this process. |
| System | !SYSTEM-ST6 | Average Thread Count | Average number of threads used by this process. |
| System | !SYSTEM-ST7 | Threading Contention Rate | Current thread lock contention rate in attempts per second. |
| System | !SYSTEM-ST8 | Average Threading Contention Rate | Average thread lock contention rate in attempts per second. |
| System | !SYSTEM-ST9 | IO Usage | Amount of IO currently used by this process in kilobytes per second. |
| System | !SYSTEM-ST10 | Average IO Usage | Average amount of IO used by this process in kilobytes per second. |
| System | !SYSTEM-ST11 | IP Data Send Rate | Number of IP datagrams (or bytes on Mono) currently sent by this process per second. |
| System | !SYSTEM-ST12 | Average IP Data Send Rate | Average number of IP datagrams (or bytes on Mono) sent by this process per second. |
| System | !SYSTEM-ST13 | IP Data Receive Rate | Number of IP datagrams (or bytes on Mono) currently received by this process per second. |
| System | !SYSTEM-ST14 | Average IP Data Receive Rate | Average number of IP datagrams (or bytes on Mono) received by this process per second. |
| System | !SYSTEM-ST15 | Up Time | Total number of seconds system has been running. |
