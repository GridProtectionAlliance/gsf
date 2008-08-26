//*******************************************************************************************************
//  FrameParser.vb - FNet Frame Parser
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using TVA;
using TVA.Interop;

namespace PhasorProtocols
{
    namespace FNet
    {
        /// <summary>This class parses an FNet binary data stream and returns parsed data via events</summary>
        /// <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
        [CLSCompliant(false)]
        public class FrameParser : FrameParserBase
        {
            #region " Public Member Declarations "

            // We shadow base class events with their FNET specific derived versions for convinience in case users consume this class directly

            /// <summary>This event is raised when a virtual Configuration Frame has been created</summary>
            /// <remarks>
            /// <para>See Std IEEE 1344 for the definition of a configuration frame.  This FNET implementation defines a similar concept</para>
            /// <para>Note that the FNET data steam does not contain a parsable configuration frame, but a virtual one is created on reception of the first data frame</para>
            /// </remarks>
            public delegate void ReceivedConfigurationFrameEventHandler(ConfigurationFrame frame);
            public new event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;

            /// <summary>This event is raised when a Data Frame has been parsed</summary>
            /// <remarks>See Std IEEE 1344 for the definition of a data frame.  FNET uses a similar concept</remarks>
            public delegate void ReceivedDataFrameEventHandler(DataFrame frame);
            public new event ReceivedDataFrameEventHandler ReceivedDataFrame;

            #endregion

            #region " Private Member Declarations "

            private ConfigurationFrame m_configurationFrame;
            private long m_ticksOffset;
            private short m_frameRate;
            private LineFrequency m_nominalFrequency;
            private string m_stationName;

            #endregion

            #region " Construction Functions "

            public FrameParser()
                : this(Common.DefaultFrameRate, Common.DefaultNominalFrequency, Common.DefaultTicksOffset)
            {

                // FNet devices default to 10 frames per second, 60Hz and 11 second time offset

            }

            public FrameParser(IConfigurationFrame configurationFrame)
                : this(configurationFrame.FrameRate, Common.DefaultNominalFrequency, Common.DefaultTicksOffset)
            {


                m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame);

                // If abstract configuration frame has any cells, we use first cell's nominal frequency...
                if (m_configurationFrame.Cells.Count > 0)
                {
                    m_nominalFrequency = ConfigurationFrame.Cells[0].NominalFrequency;
                }

            }

            public FrameParser(ConfigurationFrame configurationFrame)
                : this(configurationFrame.FrameRate, configurationFrame.NominalFrequency, configurationFrame.TicksOffset)
            {

                m_configurationFrame = configurationFrame;

            }

            public FrameParser(short frameRate, LineFrequency nominalFrequency, long ticksOffset)
            {

                m_frameRate = frameRate;
                m_nominalFrequency = nominalFrequency;
                m_ticksOffset = ticksOffset;

            }

            #endregion

            #region " Public Methods Implementation "

            public override byte ProtocolSyncByte
            {
                get
                {
                    return Common.StartByte;
                }
            }

            public override bool ProtocolUsesSyncByte
            {
                get
                {
                    return true;
                }
            }

            public override IConfigurationFrame ConfigurationFrame
            {
                get
                {
                    return m_configurationFrame;
                }
                set
                {
                    m_configurationFrame = CastToDerivedConfigurationFrame(value);
                }
            }

            public long TicksOffset
            {
                get
                {
                    return m_ticksOffset;
                }
                set
                {
                    m_ticksOffset = value;
                }
            }

            public short FrameRate
            {
                get
                {
                    return m_frameRate;
                }
                set
                {
                    m_frameRate = value;
                }
            }

            public LineFrequency NominalFrequency
            {
                get
                {
                    return m_nominalFrequency;
                }
                set
                {
                    m_nominalFrequency = value;
                }
            }

            public string StationName
            {
                get
                {
                    return m_stationName;
                }
                set
                {
                    m_stationName = value;
                }
            }

            public override string Status
            {
                get
                {
                    if (m_configurationFrame == null)
                    {
                        return base.Status;
                    }
                    else
                    {
                        System.Text.StringBuilder status = new StringBuilder();
                        status.Append("        Reported longitude: ");
                        status.Append(m_configurationFrame.Longitude);
                        status.Append('°');
                        status.AppendLine();
                        status.Append("         Reported latitude: ");
                        status.Append(m_configurationFrame.Latitude);
                        status.Append('°');
                        status.AppendLine();
                        status.Append("      Number of satellites:");
                        status.Append(m_configurationFrame.NumberOfSatellites);
                        status.AppendLine();
                        status.Append(base.Status);

                        return status.ToString();
                    }
                }
            }

            public override IConnectionParameters ConnectionParameters
            {
                get
                {
                    return base.ConnectionParameters;
                }
                set
                {
                    FNet.ConnectionParameters parameters = value as FNet.ConnectionParameters;

                    if (parameters != null)
                    {
                        base.ConnectionParameters = parameters;

                        // Assign new incoming connection parameter values
                        m_ticksOffset = parameters.TicksOffset;
                        m_frameRate = parameters.FrameRate;
                        m_nominalFrequency = parameters.NominalFrequency;
                        m_stationName = parameters.StationName;
                    }
                }
            }

            #endregion

            #region " Protected Methods Implementation "

            protected override void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength)
            {

                int startByteIndex = -1;
                int endByteIndex = -1;

                // See if there is enough data in the buffer to parse the entire frame
                for (int x = offset; x <= offset + length - 1; x++)
                {
                    // Found start index
                    if (buffer[x] == Common.StartByte)
                    {
                        startByteIndex = x;
                    }

                    if (buffer[x] == Common.EndByte)
                    {
                        if (startByteIndex == -1)
                        {
                            // Found end before beginning, bad buffer - keep looking
                            continue;
                        }
                        else
                        {
                            // Found a complete buffer
                            endByteIndex = x;
                            break;
                        }
                    }
                }

                // If there was an entire frame to parse, begin actual parse sequence
                if (endByteIndex > -1)
                {
                    // Entire frame is available, so we go ahead and parse it
                    RaiseReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, startByteIndex, endByteIndex - startByteIndex + 1);

                    // If no configuration frame has been created, we create one now
                    if (m_configurationFrame == null)
                    {
                        // Pre-parse first FNet data frame to get unit ID field and establish a virutal configuration frame
                        string[] data = TVA.Text.Common.RemoveDuplicateWhiteSpace(Encoding.ASCII.GetString(buffer, startByteIndex + 1, endByteIndex - startByteIndex - 1)).Trim().Split(' ');

                        // Make sure all the needed data elements exist (could be a bad frame)
                        if (data.Length >= 8)
                        {
                            // Create virtual configuration frame
                            m_configurationFrame = new ConfigurationFrame(Convert.ToUInt16(data[Element.UnitID]), DateTime.Now.Ticks, m_frameRate, m_nominalFrequency, m_stationName, m_ticksOffset);

                            // Notify clients of new configuration frame
                            RaiseReceivedConfigurationFrame(m_configurationFrame);

                            // Provide new FNet data frame to clients
                            RaiseReceivedDataFrame(new DataFrame(m_configurationFrame, buffer, startByteIndex));
                        }
                    }
                    else
                    {
                        // Provide new FNet data frame to clients
                        RaiseReceivedDataFrame(new DataFrame(m_configurationFrame, buffer, startByteIndex));
                    }

                    // Define actual parsed frame length so base class can increment offset past end of data frame
                    parsedFrameLength = endByteIndex - startByteIndex + 1;
                }
            }

            // We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
            protected override void RaiseReceivedConfigurationFrame(IConfigurationFrame frame)
            {
                base.RaiseReceivedConfigurationFrame(frame);
                if (ReceivedConfigurationFrame != null)
                    ReceivedConfigurationFrame((ConfigurationFrame)frame);
            }

            protected override void RaiseReceivedDataFrame(IDataFrame frame)
            {
                base.RaiseReceivedDataFrame(frame);
                if (ReceivedDataFrame != null)
                    ReceivedDataFrame((DataFrame)frame);
            }

            #endregion

            #region " Private Methods Implementation "

            private ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame configurationFrame)
            {
                ConfigurationFrame derivedFrame = configurationFrame as ConfigurationFrame;

                if (derivedFrame == null)
                {
                    return new ConfigurationFrame(configurationFrame);
                }
                else
                {
                    return derivedFrame;
                }
            }

            #endregion
        }
    }
}
