//*******************************************************************************************************
//  FrameParser.vb - IEEE1344 Frame Parser
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
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace PhasorProtocols
{
    namespace Ieee1344
    {
        /// <summary>This class parses an IEEE 1344 binary data stream and returns parsed data via events</summary>
        /// <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
        [CLSCompliant(false)]
        public class FrameParser : FrameParserBase
        {
            #region " Public Member Declarations "

            // We shadow base class events with their IEEE 1344 specific derived versions for convinience in case users consume this class directly
            public delegate void ReceivedCommonFrameHeaderEventHandler(ICommonFrameHeader frame);
            public event ReceivedCommonFrameHeaderEventHandler ReceivedCommonFrameHeader;

            public delegate void ReceivedConfigurationFrameEventHandler(ConfigurationFrame frame);
            public new event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;

            public delegate void ReceivedDataFrameEventHandler(DataFrame frame);
            public new event ReceivedDataFrameEventHandler ReceivedDataFrame;

            public delegate void ReceivedHeaderFrameEventHandler(HeaderFrame frame);
            public new event ReceivedHeaderFrameEventHandler ReceivedHeaderFrame;

            #endregion

            #region " Private Member Declarations "

            private ConfigurationFrame m_configurationFrame;
            private CommonFrameHeader.CommonFrameHeaderInstance m_configurationFrameCollection;
            private CommonFrameHeader.CommonFrameHeaderInstance m_headerFrameCollection;

            #endregion

            #region " Construction Functions "

            public FrameParser()
            {

            }

            public FrameParser(IConfigurationFrame configurationFrame)
            {

                m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame);

            }

            public FrameParser(ConfigurationFrame configurationFrame)
            {

                m_configurationFrame = configurationFrame;

            }

            #endregion

            #region " Public Methods Implementation "

            public override bool ProtocolUsesSyncByte
            {
                get
                {
                    return false;
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

            #endregion

            #region " Protected Methods Implementation "

            [SuppressMessage("Microsoft.Performance", "CA1800")]
            protected override void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength)
            {
                // See if there is enough data in the buffer to parse the common frame header.
                // Note that in order to get status flags (which contain frame length), we need at least two more bytes
                if (length >= CommonFrameHeader.BinaryLength + 2)
                {
                    // Parse frame header
                    ICommonFrameHeader parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, buffer, offset);

                    // See if there is enough data in the buffer to parse the entire frame
                    if (length >= parsedFrameHeader.FrameLength)
                    {
                        // Expose the frame buffer image in case client needs this data for any reason
                        RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength);

                        // Entire frame is availble, so we go ahead and parse it
                        switch (parsedFrameHeader.FrameType)
                        {
                            case FrameType.DataFrame:
                                if (m_configurationFrame == null)
                                {
                                    // Until we receive configuration frame, we at least expose the part of the data frame we have parsed
                                    RaiseReceivedCommonFrameHeader(parsedFrameHeader);
                                }
                                else
                                {
                                    // We can only start parsing data frames once we have successfully received a configuration frame...
                                    RaiseReceivedDataFrame(new DataFrame(parsedFrameHeader, m_configurationFrame, buffer, offset));
                                }
                                break;
                            case FrameType.ConfigurationFrame:
                                // Cumulate all partial frames together as one complete frame
                                CommonFrameHeader.CommonFrameHeaderInstance configFrame = (CommonFrameHeader.CommonFrameHeaderInstance)parsedFrameHeader;

                                if (configFrame.IsFirstFrame)
                                {
                                    m_configurationFrameCollection = configFrame;
                                }

                                if (m_configurationFrameCollection != null)
                                {
                                    try
                                    {
                                        m_configurationFrameCollection.AppendFrameImage(buffer, offset, configFrame.FrameLength);

                                        if (configFrame.IsLastFrame)
                                        {
                                            m_configurationFrame = new ConfigurationFrame(m_configurationFrameCollection, m_configurationFrameCollection.BinaryImage, 0);
                                            RaiseReceivedConfigurationFrame(m_configurationFrame);
                                            m_configurationFrameCollection = null;
                                        }
                                    }
                                    catch
                                    {
                                        // If CRC check or other exception occurs, we cancel frame cumulation process
                                        m_configurationFrameCollection = null;
                                        throw;
                                    }
                                }
                                break;
                            case FrameType.HeaderFrame:
                                // Cumulate all partial frames together as one complete frame
                                CommonFrameHeader.CommonFrameHeaderInstance headerFrame = (CommonFrameHeader.CommonFrameHeaderInstance)parsedFrameHeader;

                                if (headerFrame.IsFirstFrame)
                                {
                                    m_headerFrameCollection = headerFrame;
                                }

                                if (m_headerFrameCollection != null)
                                {
                                    try
                                    {
                                        m_headerFrameCollection.AppendFrameImage(buffer, offset, headerFrame.FrameLength);

                                        if (headerFrame.IsLastFrame)
                                        {
                                            RaiseReceivedHeaderFrame(new HeaderFrame(m_headerFrameCollection, m_headerFrameCollection.BinaryImage, 0));
                                            m_headerFrameCollection = null;
                                        }
                                    }
                                    catch
                                    {
                                        // If CRC check or other exception occurs, we cancel frame cumulation process
                                        m_headerFrameCollection = null;
                                        throw;
                                    }
                                }
                                break;
                        }

                        parsedFrameLength = parsedFrameHeader.FrameLength;
                    }
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

            protected override void RaiseReceivedHeaderFrame(IHeaderFrame frame)
            {
                base.RaiseReceivedHeaderFrame(frame);
                if (ReceivedHeaderFrame != null)
                    ReceivedHeaderFrame((HeaderFrame)frame);
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

            #endregion
        }
    }
}
