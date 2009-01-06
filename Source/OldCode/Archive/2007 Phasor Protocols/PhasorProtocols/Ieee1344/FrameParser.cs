//*******************************************************************************************************
//  FrameParser.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using PCS.Parsing;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// This class parses an IEEE 1344 binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    [CLSCompliant(false)]
    public class FrameParser : FrameParserBase<FrameType>
    {
        private ConfigurationFrame m_configurationFrame;
        private FrameImageCollector m_configurationFrameImages;
        private FrameImageCollector m_headerFrameImages;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="HeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="HeaderFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<HeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="CommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="CommandFrame"/> that was received.
        /// <para>
        /// Command frames are normally sent, not received, but there is nothing that prevents this.
        /// </para>
        /// </remarks>
        public new event EventHandler<EventArgs<CommandFrame>> ReceivedCommandFrame;

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

        protected override int ParseCommonHeader(byte[] buffer, int offset, int length, out ICommonHeader<FrameType> commonHeader)
        {
            // See if there is enough data in the buffer to parse the common frame header.
            // Note that in order to get status flags (which contain frame length), we need at least two more bytes
            if (length >= CommonFrameHeader.FixedLength + 2)
            {
                // Parse common frame header
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(m_configurationFrame, buffer, offset);

                // As an optimization, we also make sure entire frame buffer image is available to be parsed
                if (length >= parsedFrameHeader.FrameLength)
                {
                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(parsedFrameHeader.FrameType, buffer, offset, parsedFrameHeader.FrameLength);
                    commonHeader = (ICommonHeader<FrameType>)parsedFrameHeader;

                    // Handle special parsing states
                    switch (parsedFrameHeader.TypeID)
                    {
                        case FrameType.DataFrame:
                            // Assign data frame parsing state
                            parsedFrameHeader.State = new DataFrameParsingState(parsedFrameHeader.FrameLength, m_configurationFrame, Ieee1344.DataCell.CreateNewDataCell);
                            break;
                        case FrameType.ConfigurationFrame:
                            // Assign configuration frame parsing state
                            parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, Ieee1344.ConfigurationCell.CreateNewConfigurationCell);
                            
                            // Cumulate configuration frame images...
                            CumulateFrameImage(parsedFrameHeader, buffer, offset, ref m_configurationFrameImages);
                            break;
                        case FrameType.HeaderFrame:
                            // Assign header frame parsing state
                            parsedFrameHeader.State = new HeaderFrameParsingState(parsedFrameHeader.FrameLength, parsedFrameHeader.FrameLength - CommonFrameHeader.FixedLength - 2);

                            // Cumulate header frame images...
                            CumulateFrameImage(parsedFrameHeader, buffer, offset, ref m_headerFrameImages);
                            break;
                    }

                    return CommonFrameHeader.FixedLength;
                }
            }

            commonHeader = null;
            return 0;
        }

        // Cumulates frame images
        private void CumulateFrameImage(CommonFrameHeader parsedFrameHeader, byte[] buffer, int offset, ref FrameImageCollector frameImages)
        {
            // If this is the first frame, cumulate all partial frames together as one complete frame
            if (parsedFrameHeader.IsFirstFrame)
                frameImages = new FrameImageCollector();

            try
            {
                // Append next frame image
                frameImages.AppendFrameImage(buffer, offset, parsedFrameHeader.FrameLength);
            }
            catch
            {
                // Stop accumulation if CRC check fails
                frameImages = null;
                throw;
            }

            // Store a reference to frame image collection in common header state so configuration frame
            // can be parsed from entire combined binary image collection when last frame is received
            parsedFrameHeader.FrameImages = frameImages;

            // Clear local reference to frame image collection if this is the last frame
            if (parsedFrameHeader.IsLastFrame)
                frameImages = null;
        }

        public override bool ProtocolUsesSyncBytes
        {
            get
            {
                // IEEE 1344 doesn't use synchronization bytes
                return false;
            }
        }

        protected override void OnReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            // IEEE 1344 configuration frames can span multiple frame images, so we don't allow base class to raise this event until all frames have been assembled...
            ISupportFrameImage<FrameType> frameImage = frame as ISupportFrameImage<FrameType>;

            if (frameImage != null)
            {
                CommonFrameHeader commonHeader = frameImage.CommonHeader as CommonFrameHeader;

                if (commonHeader != null && commonHeader.IsLastFrame)
                    base.OnReceivedConfigurationFrame(frame);
            }
        }

        protected override void OnReceivedHeaderFrame(IHeaderFrame frame)
        {
            // IEEE 1344 header frames can span multiple frame images, so we don't allow base class to raise this event until all frames have been assembled...
            ISupportFrameImage<FrameType> frameImage = frame as ISupportFrameImage<FrameType>;

            if (frameImage != null)
            {
                CommonFrameHeader commonHeader = frameImage.CommonHeader as CommonFrameHeader;

                if (commonHeader != null && commonHeader.IsLastFrame)
                    base.OnReceivedHeaderFrame(frame);
            }
        }

        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise IEEE 1344 specific channel frame events, if any have been subscribed
            if (frame != null && (ReceivedDataFrame != null || ReceivedConfigurationFrame != null || ReceivedHeaderFrame != null || ReceivedCommandFrame != null))
            {
                DataFrame dataFrame = frame as DataFrame;

                if (dataFrame != null)
                {
                    if (ReceivedDataFrame != null)
                        ReceivedDataFrame(this, new EventArgs<DataFrame>(dataFrame));
                }
                else
                {
                    ConfigurationFrame configFrame = frame as ConfigurationFrame;

                    if (configFrame != null && configFrame.CommonHeader.IsLastFrame)
                    {
                        if (ReceivedConfigurationFrame != null)
                            ReceivedConfigurationFrame(this, new EventArgs<ConfigurationFrame>(configFrame));

                        m_configurationFrame = configFrame;
                    }
                    else
                    {
                        HeaderFrame headerFrame = frame as HeaderFrame;

                        if (headerFrame != null && headerFrame.CommonHeader.IsLastFrame)
                        {
                            if (ReceivedHeaderFrame != null)
                                ReceivedHeaderFrame(this, new EventArgs<HeaderFrame>(headerFrame));
                        }
                        else
                        {
                            CommandFrame commandFrame = frame as CommandFrame;

                            if (commandFrame != null)
                            {
                                if (ReceivedCommandFrame != null)
                                    ReceivedCommandFrame(this, new EventArgs<CommandFrame>(commandFrame));
                            }
                        }
                    }
                }
            }
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

        //#region " Public Member Declarations "

        //// We shadow base class events with their IEEE 1344 specific derived versions for convinience in case users consume this class directly
        //public delegate void ReceivedCommonFrameHeaderEventHandler(ICommonFrameHeader frame);
        //public event ReceivedCommonFrameHeaderEventHandler ReceivedCommonFrameHeader;

        //public delegate void ReceivedConfigurationFrameEventHandler(ConfigurationFrame frame);
        //public new event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;

        //public delegate void ReceivedDataFrameEventHandler(DataFrame frame);
        //public new event ReceivedDataFrameEventHandler ReceivedDataFrame;

        //public delegate void ReceivedHeaderFrameEventHandler(HeaderFrame frame);
        //public new event ReceivedHeaderFrameEventHandler ReceivedHeaderFrame;

        //#endregion

        //#region " Private Member Declarations "

        //private ConfigurationFrame m_configurationFrame;
        //private CommonFrameHeader.CommonFrameHeaderInstance m_configurationFrameCollection;
        //private CommonFrameHeader.CommonFrameHeaderInstance m_headerFrameCollection;

        //#endregion

        //#region " Construction Functions "

        //public FrameParser()
        //{

        //}

        //public FrameParser(IConfigurationFrame configurationFrame)
        //{

        //    m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame);

        //}

        //public FrameParser(ConfigurationFrame configurationFrame)
        //{

        //    m_configurationFrame = configurationFrame;

        //}

        //#endregion

        //#region " Public Methods Implementation "

        //public override bool ProtocolUsesSyncByte
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        //public override IConfigurationFrame ConfigurationFrame
        //{
        //    get
        //    {
        //        return m_configurationFrame;
        //    }
        //    set
        //    {
        //        m_configurationFrame = CastToDerivedConfigurationFrame(value);
        //    }
        //}

        //#endregion

        //#region " Protected Methods Implementation "

        //[SuppressMessage("Microsoft.Performance", "CA1800")]
        //protected override void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength)
        //{
        //    // See if there is enough data in the buffer to parse the common frame header.
        //    // Note that in order to get status flags (which contain frame length), we need at least two more bytes
        //    if (length >= CommonFrameHeader.BinaryLength + 2)
        //    {
        //        // Parse frame header
        //        ICommonFrameHeader parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, buffer, offset);

        //        // See if there is enough data in the buffer to parse the entire frame
        //        if (length >= parsedFrameHeader.FrameLength)
        //        {
        //            // Expose the frame buffer image in case client needs this data for any reason
        //            RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength);

        //            // Entire frame is availble, so we go ahead and parse it
        //            switch (parsedFrameHeader.FrameType)
        //            {
        //                case FrameType.DataFrame:
        //                    if (m_configurationFrame == null)
        //                    {
        //                        // Until we receive configuration frame, we at least expose the part of the data frame we have parsed
        //                        RaiseReceivedCommonFrameHeader(parsedFrameHeader);
        //                    }
        //                    else
        //                    {
        //                        // We can only start parsing data frames once we have successfully received a configuration frame...
        //                        RaiseReceivedDataFrame(new DataFrame(parsedFrameHeader, m_configurationFrame, buffer, offset));
        //                    }
        //                    break;
        //                case FrameType.ConfigurationFrame:
        //                    // Cumulate all partial frames together as one complete frame
        //                    CommonFrameHeader.CommonFrameHeaderInstance configFrame = (CommonFrameHeader.CommonFrameHeaderInstance)parsedFrameHeader;

        //                    if (configFrame.IsFirstFrame)
        //                    {
        //                        m_configurationFrameCollection = configFrame;
        //                    }

        //                    if (m_configurationFrameCollection != null)
        //                    {
        //                        try
        //                        {
        //                            m_configurationFrameCollection.AppendFrameImage(buffer, offset, configFrame.FrameLength);

        //                            if (configFrame.IsLastFrame)
        //                            {
        //                                m_configurationFrame = new ConfigurationFrame(m_configurationFrameCollection, m_configurationFrameCollection.BinaryImage, 0);
        //                                RaiseReceivedConfigurationFrame(m_configurationFrame);
        //                                m_configurationFrameCollection = null;
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            // If CRC check fails or other exception occurs, we cancel frame cumulation process
        //                            m_configurationFrameCollection = null;
        //                            throw;
        //                        }
        //                    }
        //                    break;
        //                case FrameType.HeaderFrame:
        //                    // Cumulate all partial frames together as one complete frame
        //                    CommonFrameHeader.CommonFrameHeaderInstance headerFrame = (CommonFrameHeader.CommonFrameHeaderInstance)parsedFrameHeader;

        //                    if (headerFrame.IsFirstFrame)
        //                    {
        //                        m_headerFrameCollection = headerFrame;
        //                    }

        //                    if (m_headerFrameCollection != null)
        //                    {
        //                        try
        //                        {
        //                            m_headerFrameCollection.AppendFrameImage(buffer, offset, headerFrame.FrameLength);

        //                            if (headerFrame.IsLastFrame)
        //                            {
        //                                RaiseReceivedHeaderFrame(new HeaderFrame(m_headerFrameCollection, m_headerFrameCollection.BinaryImage, 0));
        //                                m_headerFrameCollection = null;
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            // If CRC check fails or other exception occurs, we cancel frame cumulation process
        //                            m_headerFrameCollection = null;
        //                            throw;
        //                        }
        //                    }
        //                    break;
        //            }

        //            parsedFrameLength = parsedFrameHeader.FrameLength;
        //        }
        //    }
        //}

        //// We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
        //protected override void RaiseReceivedConfigurationFrame(IConfigurationFrame frame)
        //{
        //    base.RaiseReceivedConfigurationFrame(frame);
        //    if (ReceivedConfigurationFrame != null)
        //        ReceivedConfigurationFrame((ConfigurationFrame)frame);
        //}

        //protected override void RaiseReceivedDataFrame(IDataFrame frame)
        //{
        //    base.RaiseReceivedDataFrame(frame);
        //    if (ReceivedDataFrame != null)
        //        ReceivedDataFrame((DataFrame)frame);
        //}

        //protected override void RaiseReceivedHeaderFrame(IHeaderFrame frame)
        //{
        //    base.RaiseReceivedHeaderFrame(frame);
        //    if (ReceivedHeaderFrame != null)
        //        ReceivedHeaderFrame((HeaderFrame)frame);
        //}

        //#endregion

        //#region " Private Methods Implementation "

        //private void RaiseReceivedCommonFrameHeader(ICommonFrameHeader frame)
        //{
        //    base.RaiseReceivedUndeterminedFrame(frame);
        //    if (ReceivedCommonFrameHeader != null)
        //        ReceivedCommonFrameHeader(frame);
        //}

        //private ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame configurationFrame)
        //{
        //    ConfigurationFrame derivedFrame = configurationFrame as ConfigurationFrame;

        //    if (derivedFrame == null)
        //    {
        //        return new ConfigurationFrame(configurationFrame);
        //    }
        //    else
        //    {
        //        return derivedFrame;
        //    }
        //}

        //#endregion
    }
}
