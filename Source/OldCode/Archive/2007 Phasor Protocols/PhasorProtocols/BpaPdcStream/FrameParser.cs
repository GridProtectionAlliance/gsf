//*******************************************************************************************************
//  FrameParser.vb - BPA PDCstream Frame Parser
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;

namespace PhasorProtocols
{
    namespace BpaPdcStream
    {
        /// <summary>This class parses a BPA PDC binary data stream and returns parsed data via events</summary>
        /// <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
        [CLSCompliant(false)]
        public class FrameParser : FrameParserBase
        {
            #region " Public Member Declarations "

            // We shadow base class events with their BPA PDCstream specific derived versions for convinience in case users consume this class directly
            public delegate void ReceivedCommonFrameHeaderEventHandler(ICommonFrameHeader frame);
            public event ReceivedCommonFrameHeaderEventHandler ReceivedCommonFrameHeader;

            public delegate void ReceivedConfigurationFrameEventHandler(ConfigurationFrame frame);
            public new event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;

            public delegate void ReceivedDataFrameEventHandler(DataFrame frame);
            public new event ReceivedDataFrameEventHandler ReceivedDataFrame;

            #endregion

            #region " Private Member Declarations "

            private ConfigurationFrame m_configurationFrame;
            private string m_configurationFileName;
            private bool m_refreshConfigurationFileOnChange;
            private bool m_parseWordCountFromByte;
            private long m_lastUpdateNotification;
            private FileSystemWatcher m_configurationFileWatcher;
            private bool m_disposed;

            #endregion

            #region " Construction Functions "

            public FrameParser()
            {
            }

            public FrameParser(string configurationFileName)
            {

                m_configurationFileName = configurationFileName;
                m_refreshConfigurationFileOnChange = true;
                ResetFileWatcher();

            }

            public FrameParser(IConfigurationFrame configurationFrame)
            {

                m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame);
                m_configurationFileName = m_configurationFrame.ConfigurationFileName;
                m_refreshConfigurationFileOnChange = true;
                ResetFileWatcher();

            }

            public FrameParser(ConfigurationFrame configurationFrame)
            {

                m_configurationFrame = configurationFrame;
                m_configurationFileName = m_configurationFrame.ConfigurationFileName;
                m_refreshConfigurationFileOnChange = true;
                ResetFileWatcher();

            }

            #endregion

            #region " Public Methods Implementation "

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

            public string ConfigurationFileName
            {
                get
                {
                    if (m_configurationFrame == null)
                    {
                        return m_configurationFrame.ConfigurationFileName;
                    }
                    else
                    {
                        return m_configurationFileName;
                    }
                }
                set
                {
                    m_configurationFileName = value;
                    ResetFileWatcher();
                }
            }

            /// <summary>Set to True to automatically reload configuration file when it has changed on disk</summary>
            public bool RefreshConfigurationFileOnChange
            {
                get
                {
                    return m_refreshConfigurationFileOnChange;
                }
                set
                {
                    m_refreshConfigurationFileOnChange = value;
                    ResetFileWatcher();
                }
            }

            /// <summary>
            /// Set to True to interpret word count in packet header from a byte instead of a word - if the sync byte
            /// (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC
            /// stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the
            /// frame length, setting this property to True will correctly interpret the word count.
            /// </summary>
            public bool ParseWordCountFromByte
            {
                get
                {
                    return m_parseWordCountFromByte;
                }
                set
                {
                    m_parseWordCountFromByte = value;
                }
            }

            public override string Status
            {
                get
                {
                    System.Text.StringBuilder status = new StringBuilder();

                    status.Append("    INI configuration file: ");
                    status.Append(m_configurationFileName);
                    status.AppendLine();
                    if (m_configurationFrame != null)
                    {
                        status.Append("       BPA PDC stream type: ");
                        status.Append(Enum.GetName(typeof(StreamType), m_configurationFrame.StreamType));
                        status.AppendLine();
                        status.Append("   BPA PDC revision number: ");
                        status.Append(Enum.GetName(typeof(RevisionNumber), m_configurationFrame.RevisionNumber));
                        status.AppendLine();
                    }
                    status.Append(base.Status);

                    return status.ToString();
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
                    BpaPdcStream.ConnectionParameters parameters = value as BpaPdcStream.ConnectionParameters;

                    if (parameters != null)
                    {
                        base.ConnectionParameters = parameters;

                        // Assign new incoming connection parameter values
                        m_configurationFileName = parameters.ConfigurationFileName;
                        m_parseWordCountFromByte = parameters.ParseWordCountFromByte;
                        m_refreshConfigurationFileOnChange = parameters.RefreshConfigurationFileOnChange;
                        ResetFileWatcher();
                    }
                }
            }

            #endregion

            #region " Protected Methods Implementation "

            protected override void Dispose(bool disposing)
            {

                if (!m_disposed)
                {
                    base.Dispose(disposing);

                    if (disposing)
                    {
                        if (m_configurationFileWatcher != null)
                        {
                            m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
                            m_configurationFileWatcher.Dispose();
                        }
                        m_configurationFileWatcher = null;
                    }
                }

                m_disposed = true;

            }

            protected override void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength)
            {

                // See if there is enough data in the buffer to parse the common frame header.
                // Note that in order to get time tag for data frames, we'll need at least six more bytes
                if (length >= CommonFrameHeader.BinaryLength + 6)
                {
                    // Parse frame header
                    ICommonFrameHeader parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, m_parseWordCountFromByte, buffer, offset);

                    // See if there is enough data in the buffer to parse the entire frame
                    if (length >= parsedFrameHeader.FrameLength)
                    {
                        // Expose the frame buffer image in case client needs this data for any reason
                        RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength);

                        // Entire frame is available, so we go ahead and parse it
                        switch (parsedFrameHeader.FrameType)
                        {
                            case FrameType.DataFrame:
                                if (m_configurationFrame == null)
                                {
                                    // Until we receive configuration frame, we at least expose the part of the frame we have parsed
                                    RaiseReceivedCommonFrameHeader(parsedFrameHeader);
                                }
                                else
                                {
                                    // We can only start parsing data frames once we have successfully received a configuration frame...
                                    RaiseReceivedDataFrame(new DataFrame(parsedFrameHeader, m_configurationFrame, buffer, offset));
                                }
                                break;
                            case FrameType.ConfigurationFrame:
                                // Parse new configuration frame
                                ConfigurationFrame configFrame = new ConfigurationFrame(parsedFrameHeader, m_configurationFileName, buffer, offset);
                                m_configurationFrame = configFrame;
                                RaiseReceivedConfigurationFrame(configFrame);
                                break;
                        }

                        parsedFrameLength = parsedFrameHeader.FrameLength;
                    }
                }

            }

            // We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
            protected override void RaiseReceivedDataFrame(IDataFrame frame)
            {

                base.RaiseReceivedDataFrame(frame);
                if (ReceivedDataFrame != null)
                    ReceivedDataFrame((DataFrame)frame);

            }

            protected override void RaiseReceivedConfigurationFrame(IConfigurationFrame frame)
            {

                base.RaiseReceivedConfigurationFrame(frame);
                if (ReceivedConfigurationFrame != null)
                    ReceivedConfigurationFrame((ConfigurationFrame)frame);

            }

            #endregion

            #region " Private Methods Implementation "

            private void RaiseReceivedCommonFrameHeader(ICommonFrameHeader frame)
            {
                base.RaiseReceivedUndeterminedFrame(frame);
                if (ReceivedCommonFrameHeader != null)
                    ReceivedCommonFrameHeader(frame);
            }
            
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

            private void m_configurationFileWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
            {

                // File watcher sends several notifications for file change - we only want to report one,
                // so we ignore repeated file change notifications that occur within 1/2 a second
                if (m_lastUpdateNotification == 0 || TVA.DateTime.Common.get_TicksToSeconds(DateTime.Now.Ticks - m_lastUpdateNotification) > 0.5)
                {
                    if (m_configurationFrame != null)
                    {
                        m_configurationFrame.Refresh();
                    }
                    RaiseConfigurationChangeDetected();
                }

                m_lastUpdateNotification = DateTime.Now.Ticks;

            }

            private void ResetFileWatcher()
            {
                if (m_configurationFileWatcher != null)
                {
                    m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
                    m_configurationFileWatcher.Dispose();
                }
                m_configurationFileWatcher = null;

                if (m_refreshConfigurationFileOnChange && !string.IsNullOrEmpty(m_configurationFileName) && File.Exists(m_configurationFileName))
                {
                    try
                    {
                        // Create a new file watcher for configuration file - we'll automatically refresh configuration file
                        // when this file gets updated...
                        m_configurationFileWatcher = new FileSystemWatcher(TVA.IO.FilePath.JustPath(m_configurationFileName), TVA.IO.FilePath.JustFileName(m_configurationFileName));
                        m_configurationFileWatcher.Changed += m_configurationFileWatcher_Changed;
                        m_configurationFileWatcher.EnableRaisingEvents = true;
                        m_configurationFileWatcher.IncludeSubdirectories = false;
                        m_configurationFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                    }
                    catch (Exception ex)
                    {
                        RaiseDataStreamException(ex);
                    }
                }
            }

            #endregion
        }
    }
}
