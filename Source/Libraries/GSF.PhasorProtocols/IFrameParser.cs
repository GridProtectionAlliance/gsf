//******************************************************************************************************
//  IFrameParser.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Source channel, e.g., data or command.
    /// </summary>
    [Serializable]
    public enum SourceChannel
    {
        /// <summary>
        /// Source channel is command (e.g., TCP).
        /// </summary>
        Command,
        /// <summary>
        /// Source channel is data (e.g., UDP).
        /// </summary>
        Data,
        /// <summary>
        /// Source channel is other.
        /// </summary>
        Other
    }

    /// <summary>
    /// Check-sum validation frame-type flags.
    /// </summary>
    [Serializable, Flags]
    public enum CheckSumValidationFrameTypes : byte
    {
        /// <summary>
        /// Include no frame types for check-sum validation.
        /// </summary>
        NoFrames = (byte)Bits.Nil,
        /// <summary>
        /// Include configuration frame for check-sum validation.
        /// </summary>
        ConfigurationFrame = (byte)Bits.Bit00,
        /// <summary>
        /// Include data frame for check-sum validation.
        /// </summary>
        DataFrame = (byte)Bits.Bit01,
        /// <summary>
        /// Include header frame for check-sum validation.
        /// </summary>
        HeaderFrame = (byte)Bits.Bit02,
        /// <summary>
        /// Include command frame for check-sum validation.
        /// </summary>
        CommandFrame = (byte)Bits.Bit03,
        /// <summary>
        /// Include all frame types for check-sum validation.
        /// </summary>
        AllFrames = 0xFF
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent representation of a frame parser.
    /// </summary>
    /// <remarks>
    /// This interface allows protocol specific frame parsers to be handled abstractly by the <see cref="MultiProtocolFrameParser"/>.
    /// </remarks>
    public interface IFrameParser : IProvideStatus, IDisposable
    {
        /// <summary>
        /// Occurs when any <see cref="IChannelFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="IChannelFrame"/> that was received.
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the flag to determine if frame publication should continue.
        /// </remarks>
        event EventHandler<EventArgs<IChannelFrame, bool>> ReceivedChannelFrame;

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ICommandFrame"/> that was received.
        /// </remarks>
        event EventHandler<EventArgs<ICommandFrame>> ReceivedCommandFrame;

        /// <summary>
        /// Occurs when a <see cref="IConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IConfigurationFrame"/> that was received.
        /// </remarks>
        event EventHandler<EventArgs<IConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a <see cref="IDataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IDataFrame"/> that was received.
        /// </remarks>
        event EventHandler<EventArgs<IDataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when a <see cref="IHeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IHeaderFrame"/> that was received.
        /// </remarks>
        event EventHandler<EventArgs<IHeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an undetermined <see cref="IChannelFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the undetermined <see cref="IChannelFrame"/> that was received.
        /// </remarks>
        event EventHandler<EventArgs<IChannelFrame>> ReceivedUndeterminedFrame;

        /// <summary>
        /// Occurs when a frame image has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the length of the frame image that was received.
        /// </remarks>
        event EventHandler<EventArgs<FundamentalFrameType, int>> ReceivedFrameImage;

        /// <summary>
        /// Occurs when a frame buffer image has been received.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument2"/> is the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument3"/> is the offset into the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument4"/> is the length of data in the buffer that contains the frame image that was received.
        /// </para>
        /// <para>
        /// Consumers should use the more efficient <see cref="ReceivedFrameImage"/> event if the buffer is not needed.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<FundamentalFrameType, byte[], int, int>> ReceivedFrameBufferImage;

        /// <summary>
        /// Occurs when a device sends a notification that its configuration has changed.
        /// </summary>
        event EventHandler ConfigurationChanged;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while attempting to parse data.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing data.
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Occurs when buffer parsing has completed.
        /// </summary>
        event EventHandler BufferParsed;

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        int QueuedBuffers { get; }

        /// <summary>
        /// Gets the total number of frames that are currently queued for publication, if any.
        /// </summary>
        int QueuedOutputs { get; }

        /// <summary>
        /// Gets the number of redundant frames in each packet.
        /// </summary>
        /// <remarks>
        /// This value is used when calculating statistics. It is assumed that for each
        /// frame that is received, that frame will be included in the next <c>n</c>
        /// packets, where <c>n</c> is the number of redundant frames per packet.
        /// </remarks>
        int RedundantFramesPerPacket { get; }

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        IConfigurationFrame ConfigurationFrame { get; set; }

        /// <summary>
        /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be needed for parsing.
        /// </summary>
        IConnectionParameters ConnectionParameters { get; set; }

        /// <summary>
        /// Gets or sets flags that determine if check-sums for specified frames should be validated.
        /// </summary>
        /// <remarks>
        /// It is expected that this will normally be set to <see cref="GSF.PhasorProtocols.CheckSumValidationFrameTypes.AllFrames"/>.
        /// </remarks>
        CheckSumValidationFrameTypes CheckSumValidationFrameTypes { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if header lengths should be trusted over parsed byte count.
        /// </summary>
        /// <remarks>
        /// It is expected that this will normally be left as <c>true</c>.
        /// </remarks>
        bool TrustHeaderLength { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data parser is currently enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Start the streaming data parser.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        void Stop();

        /// <summary>
        /// Writes a sequence of bytes onto the <see cref="IBinaryImageParser"/> stream for parsing.
        /// </summary>
        /// <param name="source">Defines the source channel for the data.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        void Parse(SourceChannel source, byte[] buffer, int offset, int count);
    }
}