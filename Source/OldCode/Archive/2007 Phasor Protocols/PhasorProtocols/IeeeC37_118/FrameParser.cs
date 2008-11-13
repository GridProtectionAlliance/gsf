//*******************************************************************************************************
//  FrameParser.vb - IEEE C37.118 Frame Parser
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

namespace PCS.PhasorProtocols
{
    namespace IeeeC37_118
    {
        /// <summary>This class parses an IEEE C37.118 binary data stream and returns parsed data via events</summary>
        /// <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
        [CLSCompliant(false)]
        public class FrameParser : FrameParserBase
        {
            #region " Public Member Declarations "

            // We shadow base class events with their IEEE C37.118 specific derived versions for convinience in case users consume this class directly
            public delegate void ReceivedCommonFrameHeaderEventHandler(ICommonFrameHeader frame);
            public event ReceivedCommonFrameHeaderEventHandler ReceivedCommonFrameHeader;

            public delegate void ReceivedConfigurationFrame1EventHandler(ConfigurationFrame frame);
            public event ReceivedConfigurationFrame1EventHandler ReceivedConfigurationFrame1;

            public delegate void ReceivedConfigurationFrame2EventHandler(ConfigurationFrame frame);
            public event ReceivedConfigurationFrame2EventHandler ReceivedConfigurationFrame2;

            public delegate void ReceivedDataFrameEventHandler(DataFrame frame);
            public new event ReceivedDataFrameEventHandler ReceivedDataFrame;

            public delegate void ReceivedHeaderFrameEventHandler(HeaderFrame frame);
            public new event ReceivedHeaderFrameEventHandler ReceivedHeaderFrame;

            public delegate void ReceivedCommandFrameEventHandler(CommandFrame frame);
            public new event ReceivedCommandFrameEventHandler ReceivedCommandFrame;

            #endregion

            #region " Private Member Declarations "

            private DraftRevision m_draftRevision;
            private ConfigurationFrame m_configurationFrame2;
            private bool m_configurationChangeHandled;

            #endregion

            #region " Construction Functions "

            public FrameParser()
            {

                m_draftRevision = IeeeC37_118.DraftRevision.Draft7;

            }

            public FrameParser(IConfigurationFrame configurationFrame)
                : this()
            {

                m_configurationFrame2 = CastToDerivedConfigurationFrame(configurationFrame);

            }

            public FrameParser(DraftRevision draftRevision)
            {

                m_draftRevision = draftRevision;

            }

            public FrameParser(DraftRevision draftRevision, ConfigurationFrame configurationFrame2)
            {

                m_draftRevision = draftRevision;
                m_configurationFrame2 = configurationFrame2;

            }

            #endregion

            #region " Public Methods Implementation "

            public DraftRevision DraftRevision
            {
                get
                {
                    return m_draftRevision;
                }
                set
                {
                    m_draftRevision = value;
                }
            }

            public int TimeBase
            {
                get
                {
                    if (m_configurationFrame2 == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return m_configurationFrame2.TimeBase;
                    }
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
                    return m_configurationFrame2;
                }
                set
                {
                    m_configurationFrame2 = CastToDerivedConfigurationFrame(value);
                }
            }

            public override void Start()
            {

                m_configurationChangeHandled = false;
                base.Start();

            }

            public override string Status
            {
                get
                {
                    StringBuilder status = new StringBuilder();
                    status.Append("IEEEC37.118 draft revision: ");
                    status.Append(m_draftRevision);
                    status.AppendLine();
                    status.Append("         Current time base: ");
                    status.Append(TimeBase);
                    status.AppendLine();
                    status.Append(base.Status);

                    return status.ToString();
                }
            }

            #endregion

            #region " Protected Methods Implementation "

            protected override void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength)
            {

                // See if there is enough data in the buffer to parse the common frame header
                if (length >= CommonFrameHeader.BinaryLength)
                {
                    // Parse frame header
                    ICommonFrameHeader parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame2, buffer, offset);

                    // See if there is enough data in the buffer to parse the entire frame
                    if (length >= parsedFrameHeader.FrameLength)
                    {
                        // Expose the frame buffer image in case client needs this data for any reason
                        RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength);

                        // Entire frame is available, so we go ahead and parse it
                        switch (parsedFrameHeader.FrameType)
                        {
                            case FrameType.DataFrame:
                                if (m_configurationFrame2 == null)
                                {
                                    // Until we receive configuration frame 2, we at least expose the part of the frame we have parsed
                                    RaiseReceivedCommonFrameHeader(parsedFrameHeader);
                                }
                                else
                                {
                                    // We can only start parsing data frames once we have successfully received a configuration frame 2...
                                    RaiseReceivedDataFrame(new DataFrame(parsedFrameHeader, m_configurationFrame2, buffer, offset));
                                }
                                break;
                            case FrameType.ConfigurationFrame2:
                                switch (m_draftRevision)
                                {
                                    case DraftRevision.Draft6:
                                        ConfigurationFrameDraft6 configFrameD6 = new ConfigurationFrameDraft6(parsedFrameHeader, buffer, offset);
                                        m_configurationFrame2 = configFrameD6;
                                        RaiseReceivedConfigurationFrame2(configFrameD6);
                                        break;
                                    case DraftRevision.Draft7:
                                        ConfigurationFrame configFrame = new ConfigurationFrame(parsedFrameHeader, buffer, offset);
                                        m_configurationFrame2 = configFrame;
                                        RaiseReceivedConfigurationFrame2(configFrame);
                                        break;
                                }
                                break;
                            case FrameType.ConfigurationFrame1:
                                switch (m_draftRevision)
                                {
                                    case DraftRevision.Draft6:
                                        RaiseReceivedConfigurationFrame1(new ConfigurationFrameDraft6(parsedFrameHeader, buffer, offset));
                                        break;
                                    case DraftRevision.Draft7:
                                        RaiseReceivedConfigurationFrame1(new ConfigurationFrame(parsedFrameHeader, buffer, offset));
                                        break;
                                }
                                break;
                            case FrameType.HeaderFrame:
                                RaiseReceivedHeaderFrame(new HeaderFrame(parsedFrameHeader, buffer, offset));
                                break;
                            case FrameType.CommandFrame:
                                RaiseReceivedCommandFrame(new CommandFrame(parsedFrameHeader, buffer, offset));
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

                bool configurationChangeDetected = false;

                DataCellCollection dataCells = (DataCellCollection)frame.Cells;

                for (int x = 0; x <= dataCells.Count - 1; x++)
                {
                    if (dataCells[x].ConfigurationChangeDetected)
                    {
                        configurationChangeDetected = true;

                        // Configuration change detection flag should terminate after one minute, but
                        // we only want to send a single notification
                        if (!m_configurationChangeHandled)
                        {
                            m_configurationChangeHandled = true;
                            base.RaiseConfigurationChangeDetected();
                        }

                        break;
                    }
                }

                if (!configurationChangeDetected)
                    m_configurationChangeHandled = false;
            }

            protected override void RaiseReceivedHeaderFrame(IHeaderFrame frame)
            {
                base.RaiseReceivedHeaderFrame(frame);
                if (ReceivedHeaderFrame != null)
                    ReceivedHeaderFrame((HeaderFrame)frame);
            }

            protected override void RaiseReceivedCommandFrame(ICommandFrame frame)
            {
                base.RaiseReceivedCommandFrame(frame);
                if (ReceivedCommandFrame != null)
                    ReceivedCommandFrame((CommandFrame)frame);
            }

            #endregion

            #region " Private Methods Implementation "

            private void RaiseReceivedCommonFrameHeader(ICommonFrameHeader frame)
            {
                base.RaiseReceivedUndeterminedFrame(frame);
                if (ReceivedCommonFrameHeader != null)
                    ReceivedCommonFrameHeader(frame);
            }

            private void RaiseReceivedConfigurationFrame1(ConfigurationFrame frame)
            {
                base.RaiseReceivedConfigurationFrame(frame);
                if (ReceivedConfigurationFrame1 != null)
                    ReceivedConfigurationFrame1(frame);
            }

            private void RaiseReceivedConfigurationFrame2(ConfigurationFrame frame)
            {
                base.RaiseReceivedConfigurationFrame(frame);
                if (ReceivedConfigurationFrame2 != null)
                    ReceivedConfigurationFrame2(frame);
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
