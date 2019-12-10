//******************************************************************************************************
//  BinaryImageParserBase.cs - Gbtc
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
//  02/12/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using GSF.Units;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.Parsing
{
    /// <summary>
    /// This class defines the fundamental functionality for parsing any stream of binary data.
    /// </summary>
    /// <remarks>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </remarks>
    public abstract class BinaryImageParserBase : Stream, IBinaryImageParser
    {
        #region [ Members ]

        /// <summary>
        /// Specifies the default value for the <see cref="ProtocolSyncBytes"/> property.
        /// </summary>
        public static readonly byte[] DefaultProtocolSyncBytes = { 0xAA };

        // Events

        /// <summary>
        /// Occurs when data image fails deserialized due to an exception.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the remaining portion of the binary image that failed to parse.
        /// </remarks>
        public event EventHandler<EventArgs<byte[]>> DataDiscarded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while attempting to parse data.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing data.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Occurs when buffer parsing has completed.
        /// </summary>
        public event EventHandler BufferParsed;

        // Fields

        /// <summary>
        /// Tracks if data stream has been initialized.
        /// </summary>
        /// <remarks>
        /// Only relevant if <see cref="ProtocolUsesSyncBytes"/> is true.
        /// </remarks>
        protected bool StreamInitialized;

        /// <summary>
        /// Remaining unparsed buffer from last parsing execution, if any.
        /// </summary>
        protected byte[] UnparsedBuffer;

        private Ticks m_startTime;
        private Ticks m_stopTime;
        private bool m_enabled;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="BinaryImageParserBase"/> class.
        /// </summary>
        protected BinaryImageParserBase()
        {
            ProtocolSyncBytes = DefaultProtocolSyncBytes;
            Name = GetType().Name;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data parser is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start the <see cref="BinaryImageParserBase"/> if it is not started,
        /// setting to false will stop the <see cref="BinaryImageParserBase"/> if it is started.
        /// </remarks>
        public virtual bool Enabled
        {
            get => m_enabled;
            set
            {
                if (value && !m_enabled)
                    Start();
                else if (!value && m_enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public abstract bool ProtocolUsesSyncBytes { get; }

        /// <summary>
        /// Gets or sets synchronization bytes for this parsing implementation, if used.
        /// </summary>
        public virtual byte[] ProtocolSyncBytes { get; set; }

        /// <summary>
        /// Gets the total amount of time, in seconds, that the <see cref="BinaryImageParserBase"/> has been active.
        /// </summary>
        public Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                        processingTime = m_stopTime - m_startTime;
                    else
                        processingTime = DateTime.Now.Ticks - m_startTime;
                }

                if (processingTime < 0)
                    processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets the total number of buffer images processed so far.
        /// </summary>
        /// <returns>Total number of buffer images processed so far.</returns>
        public long TotalProcessedBuffers { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <remarks>
        /// The <see cref="BinaryImageParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
        /// </remarks>
        public override bool CanRead => false;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <remarks>
        /// The <see cref="BinaryImageParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
        /// </remarks>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <remarks>
        /// The <see cref="BinaryImageParserBase"/> is implemented as a WriteOnly stream, so this defaults to true.
        /// </remarks>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets current status of <see cref="BinaryImageParserBase"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("      Current parser state: ");
                status.Append(m_enabled ? "Active" : "Idle");
                status.AppendLine();

                if (ProtocolUsesSyncBytes)
                {
                    status.Append("Data synchronization bytes: 0x");
                    status.Append(ByteEncoding.Hexadecimal.GetString(ProtocolSyncBytes));
                    status.AppendLine();
                }

                status.Append("     Total parser run-time: ");
                status.Append(RunTime);
                status.AppendLine();
                status.Append("   Total buffers processed: ");
                status.Append(TotalProcessedBuffers);
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the name of <see cref="BinaryImageParserBase"/>.
        /// </summary>
        public virtual string Name { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the streaming data parser.
        /// </summary>
        public virtual void Start()
        {
            // Reset statistics
            TotalProcessedBuffers = 0;
            m_stopTime = 0;
            m_startTime = DateTime.Now.Ticks;

            // Initialized state depends whether or not derived class uses a protocol synchronization byte
            StreamInitialized = !ProtocolUsesSyncBytes;

            m_enabled = true;
        }

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;
            m_stopTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Parses the object implementing the <see cref="ISupportBinaryImage"/> interface.
        /// </summary>
        /// <param name="image">Object to be parsed that implements the <see cref="ISupportBinaryImage"/> interface.</param>
        /// <remarks>
        /// This function takes the binary image from <see cref="ISupportBinaryImage"/> and writes the buffer to the <see cref="BinaryImageParserBase"/> stream for parsing.
        /// </remarks>
        public virtual void Parse(ISupportBinaryImage image)
        {
            int length = image.BinaryLength;
            byte[] buffer = new byte[length];

            // Generate the binary image
            image.GenerateBinaryImage(buffer, 0);

            // Write the buffer to the parsing queue
            Write(buffer, 0, length);
        }

        // Stream implementation overrides

        /// <summary>
        /// Writes a sequence of bytes onto the stream for parsing.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!m_enabled)
                return;

            // If ProtocolUsesSyncByte is true, first call to write after start will be uninitialized,
            // thus the attempt below to "align" data stream to specified ProtocolSyncBytes.
            if (StreamInitialized)
            {
                // Directly parse frame using calling thread (typically communications thread)
                ParseBuffer(buffer, offset, count);
            }
            else
            {
                // Initial stream may be anywhere in the middle of a frame, so we attempt to locate sync byte(s) to "line-up" data stream
                int syncBytesPosition = buffer.IndexOfSequence(ProtocolSyncBytes, offset, count);

                if (syncBytesPosition > -1)
                {
                    StreamInitialized = true;
                    ParseBuffer(buffer, syncBytesPosition, count - (syncBytesPosition - offset));
                }
            }

            // Track total processed buffer images
            TotalProcessedBuffers++;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data
        /// to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
        }

        #region [ Unimplemented Stream Overrides ]

        // The parser is designed as a write only stream - so the following methods do not apply

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Cannot read from WriteOnly stream.</exception>
        /// <param name="buffer">Array of <see cref="Byte"/>s.</param>
        /// <param name="count">An <see cref="Int32"/> value for the offset.</param>
        /// <param name="offset">An <see cref="Int32"/> value for the count.</param>
        /// <returns>An <see cref="Int32"/> as the number of bytes read. Well. It would, if implemented.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException("Cannot read from WriteOnly stream");

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no position.</exception>
        /// <param name="offset">A <see cref="Int64"/> value for the offset.</param>
        /// <param name="origin">A <see cref="SeekOrigin"/>.</param>
        /// <returns>Returns a <see cref="Int64"/> value indicating the point that was sought.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException("WriteOnly stream has no position");

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no length.</exception>
        /// <param name="value">A <see cref="Int64"/> value.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void SetLength(long value) => throw new NotImplementedException("WriteOnly stream has no length");

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <remarks>
        /// WriteOnly stream has no length. Returned value will always be -1.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Length => -1;

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <remarks>
        /// WriteOnly stream has no position. Returned value will always be -1 and any assigned value will be ignored.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Position
        {
            get => -1;
            set { }
        }

        #endregion

        // Parse buffer image - user implements protocol specific "ParseFrame" function to extract data from image
        private void ParseBuffer(byte[] buffer, int offset, int count)
        {
            int endOfBuffer = 0;

            try
            {
                // Prepend any left over buffer data from last parse call
                if (UnparsedBuffer != null)
                {
                    // Combine remaining buffer from last call and current buffer together as a single image
                    buffer = UnparsedBuffer.Combine(0, UnparsedBuffer.Length, buffer, offset, count);
                    offset = 0;
                    count = buffer.Length;
                    UnparsedBuffer = null;
                }

                endOfBuffer = offset + count - 1;

                // Move through buffer parsing all available frames
                while (!(offset > endOfBuffer) && m_enabled)
                {
                    int parsedFrameLength;

                    try
                    {
                        // Call derived class frame parsing algorithm - this is protocol specific
                        parsedFrameLength = ParseFrame(buffer, offset, endOfBuffer - offset + 1);
                    }
                    catch (Exception ex)
                    {
                        // If protocol defines synchronization bytes there's a chance at recovering any unused portion of this buffer,
                        // otherwise we'll just rethrow the exception and scrap the buffer...
                        if (ProtocolUsesSyncBytes)
                        {
                            // Attempt to locate sync byte(s) after exception to line data stream back up
                            int syncBytesPosition = buffer.IndexOfSequence(ProtocolSyncBytes, offset + 1, endOfBuffer - offset);

                            if (syncBytesPosition > -1)
                            {
                                // Found the next sync byte(s), pass through malformed frame
                                parsedFrameLength = syncBytesPosition - offset;

                                // We'll still let consumer know there was an issue
                                OnDataDiscarded(buffer.BlockCopy(offset, parsedFrameLength));
                                OnParsingException(new InvalidOperationException($"Attempting to realign parsing to found protocol sync-byte{(ProtocolSyncBytes.Length > 1 ? "s" : "")} after parsing exception: {ex.Message}", ex));
                            }
                            else
                            {
                                throw;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }

                    // Returned value represents total bytes of data in the buffer image that were
                    // parsed. There could still be data remaining in the buffer, but user parsing
                    // algorithm could have decided that there was not enough buffer available for
                    // parsing and returned a "zero" - meaning no data was parsed.
                    if (parsedFrameLength > 0)
                    {
                        // If frame was parsed, increment buffer offset by frame length
                        offset += parsedFrameLength;
                    }
                    else
                    {
                        // If not, save off remaining buffer to prepend onto next read
                        UnparsedBuffer = buffer.BlockCopy(offset, endOfBuffer - offset + 1);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Probable malformed data image, discard data on move on...
                StreamInitialized = !ProtocolUsesSyncBytes;
                UnparsedBuffer = null;

                if (endOfBuffer != 0)
                    count = endOfBuffer - offset + 1;

                OnDataDiscarded(buffer.BlockCopy(offset, count));
                OnParsingException(ex);
            }

            // Notify consumer that buffer parsing is complete
            OnBufferParsed();
        }

        /// <summary>
        /// Protocol specific frame parsing algorithm.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// <para>
        /// Implementers can choose to focus on parsing a single frame in the buffer even if there are other frames available in the buffer.
        /// Base class will continue to move through buffer on behalf of derived class until all the buffer has been processed.  Just return
        /// the total amount of data was parsed and the remaining unparsed will be prepended to next received buffer.
        /// </para>
        /// <para>
        /// Derived implementations should return an integer value that represents the length of the data that was parsed, and zero if not
        /// enough data was able to be parsed. Note that exceptions are expensive when parsing fast moving streaming data and a good coding
        /// practice for implementations of this method will be to not throw an exception when there is not enough data to parse the data,
        /// instead check the <paramref name="length"/> property to verify there is enough buffer data to represent the desired image. If
        /// there is not enough data to represent the image return zero and base class will prepend buffer onto next incoming set of data.
        /// </para>
        /// <para>
        /// Because of the expense incurred when an exception is thrown, any exceptions encountered in the derived implementations of this method
        /// will cause the current data buffer to be discarded and a <see cref="ParsingException"/> event to be raised.  Doing this prevents
        /// exceptions from being thrown repeatedly for the same data. If your code implementation recognizes a malformed image, you can raise
        /// a custom event to indicate this instead of throwing as exception and keep moving through the buffer as an optimization.
        /// </para>
        /// </remarks>
        protected abstract int ParseFrame(byte[] buffer, int offset, int length);

        /// <summary>
        /// Raises the <see cref="DataDiscarded"/> event.
        /// </summary>
        /// <param name="buffer">Source buffer that contains output that failed to parse.</param>
        protected virtual void OnDataDiscarded(byte[] buffer) => DataDiscarded?.Invoke(this, new EventArgs<byte[]>(buffer));

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that was encountered during parsing.</param>
        protected virtual void OnParsingException(Exception ex) => ParsingException?.Invoke(this, new EventArgs<Exception>(ex));

        /// <summary>
        /// Raises the <see cref="BufferParsed"/> event.
        /// </summary>
        protected virtual void OnBufferParsed() => BufferParsed?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}