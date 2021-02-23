//******************************************************************************************************
//  ConfigurationFrame3.cs - Gbtc
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
//  12/06/2011 - Andrew Krohne
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Parsing;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationFrame"/>, type 3, that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationFrame3 : ConfigurationFrame1
    {
        #region [ Members ]

        // Constants
        private new const int FixedHeaderLength = ConfigurationFrame1.FixedHeaderLength + 2;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame3"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an IEEE C37.118 configuration frame, type 3.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigurationFrame3()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame3"/> from specified parameters.
        /// </summary>
        /// <param name="timebase">Timebase to use for fraction second resolution.</param>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame3"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame3"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame3"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE C37.118 configuration frame, type 3.
        /// </remarks>
        public ConfigurationFrame3(uint timebase, ushort idCode, Ticks timestamp, ushort frameRate)
            : base(timebase, idCode, timestamp, frameRate)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="IEEEC37_118.DraftRevision"/> of this <see cref="ConfigurationFrame3"/>.
        /// </summary>
        public override DraftRevision DraftRevision => DraftRevision.Std2011;

        /// <summary>
        /// Gets the <see cref="FrameType"/> of this <see cref="ConfigurationFrame3"/>.
        /// </summary>
        public override FrameType TypeID => IEEEC37_118.FrameType.ConfigurationFrame3;

        /// <summary>
        /// Gets continuation index for fragmented frames of this <see cref="ConfigurationFrame3"/>.
        /// </summary>
        public ushort ContinuationIndex => CommonHeader.ContinuationIndex;

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => FixedHeaderLength;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame3"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length for use in the common header image
                unchecked
                {
                    CommonHeader.FrameLength = (ushort)BinaryLength;
                }

                byte[] buffer = new byte[FixedHeaderLength];
                int index = 0;

                CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);
                BigEndian.CopyBytes((ushort)0, buffer, index); // CONT_IDX
                BigEndian.CopyBytes(m_timebase, buffer, index + 2);
                BigEndian.CopyBytes((ushort)Cells.Count, buffer, index + 6);

                return buffer;
            }
        }

        /// <summary>
        /// Returns a collection of binary images where each image represents a frame,
        /// i.e., a portion of a complete configuration 3 frame image to be published.
        /// Each returned frame in the collection will be no larger than 65,535 bytes.
        /// </summary>
        /// <remarks>
        /// This property manages creating multiple frame images for a config
        /// frame 3 instance using the CONT_IDX field for fragmented frames.
        /// </remarks>
        public IEnumerable<byte[]> BinaryImageFrames
        {
            get
            {
                // Define absolute maximum frame length
                const ushort MaxFrameLength = ushort.MaxValue;

                // Get the full binary image, this will begin with common frame header
                byte[] image = this.BinaryImage();
                int imageLength = image.Length;

                if (imageLength < MaxFrameLength)
                {
                    // Full image fits within one frame, return current image as-is
                    yield return image;
                }
                else
                {
                    // Each frame header length is 14 bytes of common header plus two bytes for CONT_IDX
                    const ushort FrameHeaderLength = CommonFrameHeader.FixedLength + 2;

                    // Maximum frame data payload length is max frame length minus frame header length
                    const ushort MaxFrameDataLength = MaxFrameLength - FrameHeaderLength;

                    // Handle image fragmentation for multiple frame publication, note that each fragment
                    // to published is a chunk of the original image referred to here as a "frame"
                    ushort continuationIndex = 0;
                    byte[] header = CommonHeader.BinaryImage;

                    // Calculate remaining image length after first frame which (1) already has a header,
                    // and (2) will always be MaxFrameLength, i.e., 65,535 bytes in length.
                    int lengthAfterFirst = imageLength - MaxFrameLength;

                    // All frames beyond initial frame will need an "injected" header, so we calculate total
                    // number of frames to be published based on maximum data payload length, i.e., maximum
                    // frame size without a header length, headers will get added to frame before publication.
                    // This will yield frames that are never larger than MaxFrameLength, i.e., 65,535 bytes.
                    int lastFrameSize = lengthAfterFirst % MaxFrameDataLength; // Last frame length, if uneven
                    int frames = 1 + lengthAfterFirst / MaxFrameDataLength + (lastFrameSize > 0 ? 1 : 0);
                    int imageIndex = 0;

                    if (frames > MaxFrameLength)
                        throw new OverflowException($"Configuration frame 3 size would yield {frames:N0} fragments exceeding maximum of {MaxFrameLength:N0}. Absolute maximum configuration 3 frame size is {(long)MaxFrameLength * MaxFrameLength:N0} bytes (~4GB).");

                    for (int i = 0; i < frames; i++)
                    {
                        bool firstFrame = i == 0;
                        bool lastFrame = i == frames - 1;
                        int frameSize;

                        if (firstFrame)
                            frameSize = MaxFrameLength; // First frame already includes header
                        else if (lastFrame)
                            frameSize = lastFrameSize > 0 ? lastFrameSize : MaxFrameDataLength;
                        else
                            frameSize = MaxFrameDataLength;

                        // Increment CONT_IDX, note index is always 0xFFFF for last frame
                        continuationIndex = lastFrame ? MaxFrameLength : ++continuationIndex;

                        if (firstFrame)
                        {
                            byte[] frame = image.BlockCopy(0, frameSize);

                            // Initial frame already has full header, replace CONT_IDX with a value of 1
                            // that indicates this is the first frame in a series of frames that follow:
                            Buffer.BlockCopy(BigEndian.GetBytes(continuationIndex), 0, frame, CommonFrameHeader.FixedLength, 2);
                            
                            // Return first frame
                            yield return frame;
                        }
                        else
                        {
                            byte[] frame = new byte[FrameHeaderLength + frameSize];

                            // Copy fixed header bytes into frame
                            Buffer.BlockCopy(header, 0, frame, 0, CommonFrameHeader.FixedLength);
                            
                            // Copy CONT_IDX into frame
                            Buffer.BlockCopy(BigEndian.GetBytes(continuationIndex), 0, frame, CommonFrameHeader.FixedLength, 2);
                            
                            // Copy next chunk of bytes from full image into frame
                            Buffer.BlockCopy(image, imageIndex, frame, FrameHeaderLength, frameSize);

                            // Return next frame
                            yield return frame;
                        }
                        
                        imageIndex += frameSize;
                    }
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden to parse from cumulated frame images.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            CommonFrameHeader frameHeader = CommonHeader;

            // Handle normal parsing if configuration frame is not fragmented
            if (frameHeader is null || frameHeader.ContinuationIndex == 0)
                return base.ParseBinaryImage(buffer, startIndex, length);

            // If all configuration frame images have been received, we can safely start parsing
            if (frameHeader.IsLastFrame)
            {
                using (FrameImageCollector frameImages = frameHeader.FrameImages)
                {
                    if (frameImages is null)
                        return State.ParsedBinaryLength;

                    // Each individual frame will already have had a CRC check, so we implement standard parse to
                    // bypass ChannelBase CRC frame validation on cumulative frame image
                    buffer = frameImages.BinaryImage;
                    length = frameImages.BinaryLength;
                    startIndex = 0;

                    // Parse out header, body and footer images
                    startIndex += ParseHeaderImage(buffer, startIndex, length);
                    startIndex += ParseBodyImage(buffer, startIndex, length - startIndex);
                    startIndex += ParseFooterImage(buffer, startIndex, length - startIndex);

                    if (TrustHeaderLength)
                        return State.ParsedBinaryLength;
                    
                    // Include 2 bytes for CRC that was already validated
                    return startIndex + 2;
                }
            }

            // There are more configuration frame 3 images coming, keep parser moving
            // by returning total frame length that was already parsed (or cumulated).
            return State.ParsedBinaryLength;
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            // Skip past header that was already parsed...
            startIndex += CommonFrameHeader.FixedLength + 2;

            m_timebase = BigEndian.ToUInt32(buffer, startIndex) & ~Common.TimeQualityFlagsMask;
            CommonHeader.Timebase = m_timebase;
            State.CellCount = BigEndian.ToUInt16(buffer, startIndex + 4);

            return FixedHeaderLength;
        }

        #endregion
    }
}