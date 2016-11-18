//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/16/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSF;
using GSF.IO;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.Anonymous;
using GSF.TimeSeries;
using GSF.Units;
using GSF.Units.EE;

namespace ProtocolTester
{
    /// <summary>
    /// This is a test tool designed to help with protocol development, it is not intended for deployment...
    /// </summary>
    // ReSharper disable ConvertToConstant.Local
    public class Program
    {
        private static readonly bool WriteLogs = false;
        private static readonly bool TestConcentrator = false;

        private static Concentrator concentrator;
        private static MultiProtocolFrameParser parser;
        private static ConcurrentDictionary<string, MeasurementMetadata> m_definedMeasurements;
        private static ConcurrentDictionary<ushort, ConfigurationCell> m_definedDevices;
        private static StreamWriter m_exportFile;
        private static uint measurementID;
        private static long frameCount;
        private static long byteCount;

        public static void Main(string[] args)
        {
            m_definedMeasurements = new ConcurrentDictionary<string, MeasurementMetadata>();
            m_definedDevices = new ConcurrentDictionary<ushort, ConfigurationCell>();

            if (WriteLogs)
                m_exportFile = new StreamWriter(FilePath.GetAbsolutePath("InputTimestamps.csv"));

            if (TestConcentrator)
            {
                // Create a new concentrator
                concentrator = new Concentrator(WriteLogs, FilePath.GetAbsolutePath("OutputTimestamps.csv"));
                concentrator.TimeResolution = 333000;
                concentrator.FramesPerSecond = 30;
                concentrator.LagTime = 3.0D;
                concentrator.LeadTime = 9.0D;
                concentrator.PerformTimestampReasonabilityCheck = false;
                concentrator.ProcessByReceivedTimestamp = true;
                concentrator.Start();
            }

            // Create a new protocol parser
            parser = new MultiProtocolFrameParser();
            parser.AllowedParsingExceptions = 500;
            parser.ParsingExceptionWindow = 5;

            // Attach to desired events
            parser.ConnectionAttempt += parser_ConnectionAttempt;
            parser.ConnectionEstablished += parser_ConnectionEstablished;
            parser.ConnectionException += parser_ConnectionException;
            parser.ParsingException += parser_ParsingException;
            parser.ReceivedConfigurationFrame += parser_ReceivedConfigurationFrame;
            parser.ReceivedDataFrame += parser_ReceivedDataFrame;
            parser.ReceivedFrameBufferImage += parser_ReceivedFrameBufferImage;
            parser.ConnectionTerminated += parser_ConnectionTerminated;

            // Define the connection string
            //parser.ConnectionString = @"phasorProtocol=IeeeC37_118V1; transportProtocol=UDP; localport=5000; server=233.123.123.123:5000; interface=0.0.0.0";
            //parser.ConnectionString = @"phasorProtocol=Ieee1344; transportProtocol=File; file=D:\Projects\Applications\openPDC\Synchrophasor\Current Version\Build\Output\Debug\Applications\openPDC\Sample1344.PmuCapture";
            //parser.ConnectionString = @"phasorProtocol=Macrodyne; accessID=1; transportProtocol=File; skipDisableRealTimeData = true; file=C:\Users\Ritchie\Desktop\Development\Macrodyne\ING.out; iniFileName=C:\Users\Ritchie\Desktop\Development\Macrodyne\BCH18Aug2011.ini; deviceLabel=ING1; protocolVersion=G";
            //parser.ConnectionString = @"phasorProtocol=Iec61850_90_5; accessID=1; transportProtocol=UDP; skipDisableRealTimeData = true; localPort=102; interface=0.0.0.0; commandChannel={transportProtocol=TCP; server=172.21.1.201:4712; interface=0.0.0.0}";
            //parser.ConnectionString = @"phasorProtocol=FNET; transportProtocol=TCP; server=172.21.4.100:4001; interface=0.0.0.0; isListener=false";
            //parser.ConnectionString = @"phasorProtocol=Macrodyne; transportProtocol=Serial; port=COM6; baudrate=38400; parity=None; stopbits=One; databits=8; dtrenable=False; rtsenable=False;";
            //parser.ConnectionString = @"phasorProtocol=SelFastMessage; transportProtocol=Serial; port=COM5; baudrate=57600; parity=None; stopbits=One; databits=8; dtrenable=False; rtsenable=False;";            
            //parser.ConnectionString = @"phasorProtocol=IEEEC37_118v1; transportProtocol=File; file=C:\Users\Ritchie\Desktop\MTI_Test_3phase.PmuCapture; checkSumValidationFrameTypes=DataFrame,HeaderFrame,CommandFrame";
            parser.ConnectionString = @"phasorProtocol=IEEEC37_118V1; transportProtocol=tcp; accessID=105; server=172.31.105.135:4712; interface=0.0.0.0; isListener=false";

            Dictionary<string, string> settings = parser.ConnectionString.ParseKeyValuePairs();
            string setting;

            // TODO: These should be optional picked up from connection string inside of MPFP
            // Apply other settings as needed
            if (settings.TryGetValue("accessID", out setting))
                parser.DeviceID = ushort.Parse(setting);

            if (settings.TryGetValue("simulateTimestamp", out setting))
                parser.InjectSimulatedTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("allowedParsingExceptions", out setting))
                parser.AllowedParsingExceptions = int.Parse(setting);

            if (settings.TryGetValue("parsingExceptionWindow", out setting))
                parser.ParsingExceptionWindow = Ticks.FromSeconds(double.Parse(setting));

            if (settings.TryGetValue("autoStartDataParsingSequence", out setting))
                parser.AutoStartDataParsingSequence = setting.ParseBoolean();

            if (settings.TryGetValue("skipDisableRealTimeData", out setting))
                parser.SkipDisableRealTimeData = setting.ParseBoolean();

            // When connecting to a file based resource you may want to loop the data
            parser.AutoRepeatCapturedPlayback = true;

            // Start frame parser
            parser.AutoStartDataParsingSequence = true;

            Console.WriteLine("ConnectionString: {0}", parser.ConnectionString);

            parser.Start();

            // To keep the console open while receiving live data with AutoRepeatCapturedPlayback = false, uncomment the following line of code:

            Console.WriteLine("Press <enter> to terminate application...");

            Console.ReadLine();

            parser.Stop();

            // Stop concentrator
            if (TestConcentrator)
                concentrator.Stop();

            if (WriteLogs)
                m_exportFile.Close();
        }

        static void parser_ReceivedFrameBufferImage(object sender, EventArgs<FundamentalFrameType, byte[], int, int> e)
        {
            const int interval = (int)SI.Kilo * 2;

            bool showMessage = byteCount + e.Argument4 >= (byteCount / interval + 1) * interval;

            byteCount += e.Argument4;

            if (showMessage)
                Console.WriteLine("{0:N0} bytes of data have been processed so far...", byteCount);
        }

        private static void parser_ReceivedDataFrame(object sender, EventArgs<IDataFrame> e)
        {
            IDataFrame dataFrame = e.Argument;

            if (WriteLogs)
                m_exportFile.WriteLine(dataFrame.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + string.Concat(dataFrame.Cells.Select(cell => "," + cell.FrequencyValue.Frequency.ToString())));

            // Pass frame measurements to concentrator...
            if (TestConcentrator)
                ExtractFrameMeasurements(dataFrame);

            // Increase the frame count each time a frame is received
            frameCount++;

            //IDataCell device = dataFrame.Cells[0];
            //Console.Write(device.FrequencyValue.Frequency.ToString("0.000Hz "));

            // Print information each time we receive 60 frames (every 2 seconds for 30 frames per second)
            // Also check to assure the DataFrame has at least one Cell
            if (frameCount % 60 == 0)
            {
                Console.WriteLine("Received {0} data frames so far...", frameCount);

                if (dataFrame.Cells.Count > 0)
                {
                    IDataCell device = dataFrame.Cells[0];
                    Console.WriteLine("    Last frequency: {0}Hz", device.FrequencyValue.Frequency);

                    for (int x = 0; x < device.PhasorValues.Count; x++)
                    {
                        Console.WriteLine("PMU {0} Phasor {1} Angle = {2}", device.IDCode, x, device.PhasorValues[x].Angle);
                        Console.WriteLine("PMU {0} Phasor {1} Magnitude = {2}", device.IDCode, x, device.PhasorValues[x].Magnitude);
                    }

                    Console.WriteLine("    Last Timestamp: {0}", device.GetStatusFlagsMeasurement().Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }

                if ((object)concentrator != null)
                    Console.WriteLine(concentrator.Status);
            }
        }

        private static void parser_ReceivedConfigurationFrame(object sender, EventArgs<IConfigurationFrame> e)
        {
            // Notify the user when a configuration frame is received
            Console.WriteLine("Received configuration frame with {0} device(s)", e.Argument.Cells.Count);
        }

        private static void parser_ParsingException(object sender, EventArgs<Exception> e)
        {
            // Output the exception to the user
            Console.WriteLine("Parsing exception: {0}", e.Argument);
        }

        private static void parser_ConnectionException(object sender, EventArgs<Exception, int> e)
        {
            // Display which connection attempt failed and the exception that occurred
            Console.WriteLine("Connection attempt {0} failed due to exception: {1}",
                e.Argument2, e.Argument1);
        }

        private static void parser_ConnectionEstablished(object sender, EventArgs e)
        {
            // Notify the user when the connection is established
            Console.WriteLine("Established {0} {1} based connection...",
                parser.PhasorProtocol.GetFormattedProtocolName(),
                parser.TransportProtocol.ToString().ToUpper());
        }

        static void parser_ConnectionTerminated(object sender, EventArgs e)
        {
            Console.WriteLine("Connection terminated.");
        }

        private static void parser_ConnectionAttempt(object sender, EventArgs e)
        {
            // Let the user know we are attempting to connect
            Console.WriteLine("Attempting connection...");
        }

        private static void ExtractFrameMeasurements(IDataFrame frame)
        {
            const int AngleIndex = (int)CompositePhasorValue.Angle;
            const int MagnitudeIndex = (int)CompositePhasorValue.Magnitude;
            const int FrequencyIndex = (int)CompositeFrequencyValue.Frequency;
            const int DfDtIndex = (int)CompositeFrequencyValue.DfDt;

            List<IMeasurement> mappedMeasurements = new List<IMeasurement>();
            ConfigurationCell definedDevice;
            PhasorValueCollection phasors;
            AnalogValueCollection analogs;
            DigitalValueCollection digitals;
            IMeasurement[] measurements;
            int x, count;

            // Loop through each parsed device in the data frame
            foreach (IDataCell parsedDevice in frame.Cells)
            {
                try
                {
                    // Lookup device by its label (if needed), then by its ID code
                    definedDevice = m_definedDevices.GetOrAdd(parsedDevice.IDCode, id => new ConfigurationCell(id)
                    {
                        StationName = parsedDevice.StationName,
                        IDLabel = parsedDevice.IDLabel.ToNonNullNorWhiteSpace(parsedDevice.StationName),
                        IDCode = parsedDevice.IDCode
                    });

                    // Map status flags (SF) from device data cell itself (IDataCell implements IMeasurement
                    // and exposes the status flags as its value)
                    MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.Status), parsedDevice.GetStatusFlagsMeasurement());

                    // Map phase angles (PAn) and magnitudes (PMn)
                    phasors = parsedDevice.PhasorValues;
                    count = phasors.Count;

                    for (x = 0; x < count; x++)
                    {
                        // Get composite phasor measurements
                        measurements = phasors[x].Measurements;

                        // Map angle
                        MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.Angle, x, count), measurements[AngleIndex]);

                        // Map magnitude
                        MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.Magnitude, x, count), measurements[MagnitudeIndex]);
                    }

                    // Map frequency (FQ) and dF/dt (DF)
                    measurements = parsedDevice.FrequencyValue.Measurements;

                    // Map frequency
                    MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.Frequency), measurements[FrequencyIndex]);

                    // Map dF/dt
                    MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.DfDt), measurements[DfDtIndex]);

                    // Map analog values (AVn)
                    analogs = parsedDevice.AnalogValues;
                    count = analogs.Count;

                    for (x = 0; x < count; x++)
                    {
                        // Map analog value
                        MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.Analog, x, count), analogs[x].Measurements[0]);
                    }

                    // Map digital values (DVn)
                    digitals = parsedDevice.DigitalValues;
                    count = digitals.Count;

                    for (x = 0; x < count; x++)
                    {
                        // Map digital value
                        MapMeasurementAttributes(mappedMeasurements, definedDevice.GetSignalReference(SignalKind.Digital, x, count), digitals[x].Measurements[0]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception encountered while mapping \"{0}\" data frame cell \"{1}\" elements to measurements: {2}", frame.IDCode, parsedDevice.StationName.ToNonNullString("[undefined]"), ex.Message);
                }
            }

            concentrator.SortMeasurements(mappedMeasurements);
        }

        private static void MapMeasurementAttributes(ICollection<IMeasurement> mappedMeasurements, string signalReference, IMeasurement parsedMeasurement)
        {
            // Coming into this function the parsed measurement value will only have a "value" and a "timestamp";
            // the measurement will not yet be associated with an actual historian measurement ID as the measurement
            // will have come directly out of the parsed phasor protocol data frame.  We take the generated signal
            // reference and use that to lookup the actual historian measurement ID, source, adder and multipler.
            MeasurementMetadata definedMeasurement = m_definedMeasurements.GetOrAdd(signalReference,
                signal => MeasurementKey.LookUpOrCreate(Guid.NewGuid(), signal, ++measurementID).Metadata);

            // Assign ID and other relevant attributes to the parsed measurement value
            parsedMeasurement.Metadata = definedMeasurement;

            // Add the updated measurement value to the destination measurement collection
            mappedMeasurements.Add(parsedMeasurement);
        }
    }
}
