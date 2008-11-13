//*******************************************************************************************************
//  FrameParserBase.vb - Frame Parser Base Class
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
//  02/12/2007 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using PCS;
using PCS.Collections;

namespace PCS.PhasorProtocols
{
    /// <summary>This class defines the basic functionality for a protocol to parse a binary data stream and return the parsed data via events</summary>
    /// <remarks>Frame parsers are implemented as a write-only stream - this way data can come from any source</remarks>
    [CLSCompliant(false)]
    public abstract class FrameParserBase : Stream, IFrameParser
    {
        #region " Public Member Declarations "

        // Derived classes will typically also expose events to provide instances to the protocol specific final derived channel frames
        public event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;
        public event ReceivedDataFrameEventHandler ReceivedDataFrame;
        public event ReceivedHeaderFrameEventHandler ReceivedHeaderFrame;
        public event ReceivedCommandFrameEventHandler ReceivedCommandFrame;
        public event ReceivedUndeterminedFrameEventHandler ReceivedUndeterminedFrame;
        public event ReceivedFrameBufferImageEventHandler ReceivedFrameBufferImage;
        public event ConfigurationChangedEventHandler ConfigurationChanged;
        public event DataStreamExceptionEventHandler DataStreamException;

        #endregion

        #region " Private Member Declarations "

        private ProcessQueue<byte[]> m_bufferQueue;
        private bool m_executeParseOnSeparateThread;
        private MemoryStream m_dataStream;
        private bool m_initialized;
        private bool m_enabled;
        private IConnectionParameters m_connectionParameters;
        private bool m_disposed;

        #endregion

        #region " Public Methods Implementation "

        public virtual void Start()
        {

            m_initialized = !ProtocolUsesSyncByte;
            if (m_executeParseOnSeparateThread)
            {
                m_bufferQueue.Start();
            }
            m_enabled = true;

        }

        public virtual void Stop()
        {

            m_enabled = false;
            if (m_executeParseOnSeparateThread)
            {
                m_bufferQueue.Stop();
            }

        }

        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value && !m_enabled)
                    Start();
                else if (!value && m_enabled)
                    Stop();
            }
        }

        public virtual bool ExecuteParseOnSeparateThread
        {

            get
            {
                return m_executeParseOnSeparateThread;
            }
            set
            {
                // This property allows a dynamic change in state of how to process streaming data
                if (value)
                {
                    if (m_bufferQueue == null)
                    {
                        m_bufferQueue = ProcessQueue<byte[]>.CreateRealTimeQueue(ProcessQueuedBuffers);
                        m_bufferQueue.ProcessException += m_bufferQueue_ProcessException;
                    }
                    if (m_enabled && !m_bufferQueue.Enabled)
                    {
                        m_bufferQueue.Start();
                    }
                    m_executeParseOnSeparateThread = true;
                }
                else
                {
                    m_executeParseOnSeparateThread = false;
                    if (m_bufferQueue != null)
                    {
                        m_bufferQueue.Stop();
                        m_bufferQueue.ProcessException -= m_bufferQueue_ProcessException;
                    }
                    m_bufferQueue = null;
                }
            }
        }

        public virtual int QueuedBuffers
        {
            get
            {
                if (m_executeParseOnSeparateThread)
                {
                    return m_bufferQueue.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public abstract IConfigurationFrame ConfigurationFrame
        {
            get;
            set;
        }

        public abstract bool ProtocolUsesSyncByte
        {
            get;
        }

        public virtual byte ProtocolSyncByte
        {
            get
            {
                return PhasorProtocols.Common.SyncByte;
            }
        }

        // Stream implementation overrides
        public override void Write(byte[] buffer, int offset, int count)
        {

            if (m_initialized)
            {
                if (m_executeParseOnSeparateThread)
                {
                    // Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                    m_bufferQueue.Add(buffer.BlockCopy(offset, count));
                }
                else
                {
                    // Directly parse frame using calling thread (typically communications thread)
                    ParseBuffer(buffer, offset, count);
                }
            }
            else
            {
                // Initial stream may be anywhere in the middle of a frame, so we attempt to locate sync byte to "line-up" data stream
                int syncBytePosition = Array.IndexOf(buffer, ProtocolSyncByte, offset, count);

                if (syncBytePosition > -1)
                {
                    // Initialize data stream starting at located sync byte
                    if (m_executeParseOnSeparateThread)
                    {
                        m_bufferQueue.Add(buffer.BlockCopy(syncBytePosition, count - syncBytePosition));
                    }
                    else
                    {
                        ParseBuffer(buffer, syncBytePosition, count - syncBytePosition);
                    }

                    m_initialized = true;
                }
            }

        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("      Current parser state: ");
                if (m_enabled)
                {
                    status.Append("Active");
                }
                else
                {
                    status.Append("Idle");
                }
                status.AppendLine();
                if (ProtocolUsesSyncByte)
                {
                    status.Append(" Data synchronization byte: 0x");
                    status.Append(ProtocolSyncByte.ToString("X"));
                    status.AppendLine();
                }
                status.Append("     Received config frame: ");
                status.Append(ConfigurationFrame == null ? "No" : "Yes");
                status.AppendLine();
                if (ConfigurationFrame != null)
                {
                    status.Append("   Devices in config frame: ");
                    status.Append(ConfigurationFrame.Cells.Count);
                    status.Append(" total - ");
                    status.AppendLine();
                    for (int x = 0; x <= ConfigurationFrame.Cells.Count - 1; x++)
                    {
                        status.Append("               (");
                        status.Append(ConfigurationFrame.Cells[x].IDCode);
                        status.Append(") ");
                        status.Append(ConfigurationFrame.Cells[x].StationName);
                        status.AppendLine();
                    }
                    status.Append("     Configured frame rate: ");
                    status.Append(ConfigurationFrame.FrameRate);
                    status.AppendLine();
                }
                status.Append("  Parsing execution source: ");
                if (m_executeParseOnSeparateThread)
                {
                    status.Append("Independent thread using queued data");
                    status.AppendLine();
                    status.Append(m_bufferQueue.Status);
                }
                else
                {
                    status.Append("Communications thread");
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        public virtual IConnectionParameters ConnectionParameters
        {
            get
            {
                return m_connectionParameters;
            }
            set
            {
                m_connectionParameters = value;
            }
        }

        #region " Unimplemented Stream Overrides "

        // This is a write only stream - so the following methods do not apply to this stream
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Read(byte[] buffer, int offset, int count)
        {

            throw (new NotImplementedException("Cannnot read from WriteOnly stream"));

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {

            throw (new NotImplementedException("WriteOnly stream has no position"));

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void SetLength(long value)
        {

            throw (new NotImplementedException("WriteOnly stream has no length"));

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Length
        {
            get
            {
                throw (new NotImplementedException("WriteOnly stream has no length"));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Position
        {
            get
            {
                throw (new NotImplementedException("WriteOnly stream has no position"));
            }
            set
            {
                throw (new NotImplementedException("WriteOnly stream has no position"));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Flush()
        {

            // Nothing to do, no need to throw an error...

        }

        #endregion

        #endregion

        #region " Protected Methods Implementation "

        /// <summary>
        /// Protocol specific frame parsing algorithm
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing</param>
        /// <param name="length">Maximum length of valid data from offset</param>
        /// <param name="parsedFrameLength">Derived implementations update this value with the length of the data that was parsed</param>
        /// <remarks>
        /// Implementors can choose to focus on parsing a single frame in the buffer even if there are other frames available in the buffer.
        /// Base class will continue to move through buffer on behalf of derived class until all the buffer has been processed.  Any data
        /// that remains unparsed will be prepended to next received buffer.
        /// </remarks>
        protected abstract void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength);

        protected virtual void RaiseReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            if (ReceivedConfigurationFrame != null)
                ReceivedConfigurationFrame(frame);
        }

        protected virtual void RaiseReceivedDataFrame(IDataFrame frame)
        {
            if (ReceivedDataFrame != null)
                ReceivedDataFrame(frame);
        }

        protected virtual void RaiseReceivedHeaderFrame(IHeaderFrame frame)
        {
            if (ReceivedHeaderFrame != null)
                ReceivedHeaderFrame(frame);
        }

        protected virtual void RaiseReceivedCommandFrame(ICommandFrame frame)
        {
            if (ReceivedCommandFrame != null)
                ReceivedCommandFrame(frame);
        }

        protected virtual void RaiseReceivedUndeterminedFrame(IChannelFrame frame)
        {
            if (ReceivedUndeterminedFrame != null)
                ReceivedUndeterminedFrame(frame);
        }

        protected virtual void RaiseReceivedFrameBufferImage(FundamentalFrameType frameType, byte[] binaryImage, int offset, int length)
        {
            if (ReceivedFrameBufferImage != null)
                ReceivedFrameBufferImage(frameType, binaryImage, offset, length);
        }

        protected virtual void RaiseConfigurationChangeDetected()
        {
            if (ConfigurationChanged != null)
                ConfigurationChanged();
        }

        protected virtual void RaiseDataStreamException(Exception ex)
        {
            if (DataStreamException != null)
                DataStreamException(ex);
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (m_bufferQueue != null)
                    {
                        m_bufferQueue.Dispose();
                        m_bufferQueue.ProcessException -= m_bufferQueue_ProcessException;
                    }
                    m_bufferQueue = null;

                    if (m_dataStream != null) m_dataStream.Dispose();
                    m_dataStream = null;

                    m_enabled = false;
                }
            }

            m_disposed = true;
        }

        #endregion

        #region " Private Methods Implementation "

        // We process all queued data buffers that are available at once...
        private void ProcessQueuedBuffers(byte[][] buffers)
        {
            MemoryStream combinedBuffer = new MemoryStream();

            // Combine all currently queued buffers
            for (int x = 0; x <= buffers.Length - 1; x++)
            {
                combinedBuffer.Write(buffers[x], 0, buffers[x].Length);
            }

            // Parse combined data buffers
            ParseBuffer(combinedBuffer.ToArray(), 0, (int)combinedBuffer.Length);
        }

        private void ParseBuffer(byte[] buffer, int offset, int count)
        {
            try
            {
                // Prepend any left over buffer data from last parse call
                if (m_dataStream != null)
                {
                    MemoryStream combinedBuffer = new MemoryStream();

                    combinedBuffer.Write(m_dataStream.ToArray(), 0, (int)m_dataStream.Length);
                    m_dataStream = null;

                    // Append new incoming data
                    combinedBuffer.Write(buffer, offset, count);

                    // Pull all combined data together as one big buffer
                    buffer = combinedBuffer.ToArray();
                    offset = 0;
                    count = (int)combinedBuffer.Length;
                }

                int endOfBuffer = offset + count - 1;
                int parsedFrameLength;

                // Move through buffer parsing all available frames
                while (!(offset > endOfBuffer))
                {
                    parsedFrameLength = 0;

                    // Call derived class frame parsing algorithm - this is protocol specific
                    ParseFrame(buffer, offset, endOfBuffer - offset + 1, ref parsedFrameLength);

                    if (parsedFrameLength > 0)
                    {
                        // If frame was parsed, increment buffer offset by frame length
                        offset += parsedFrameLength;
                    }
                    else
                    {
                        // If not, save off remaining buffer to prepend onto next read
                        m_dataStream = new MemoryStream();
                        m_dataStream.Write(buffer, offset, count - offset);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                m_initialized = false;
                m_dataStream = null;
                RaiseDataStreamException(ex);
            }
        }

        private void m_bufferQueue_ProcessException(System.Exception ex)
        {
            RaiseDataStreamException(ex);
        }

        #endregion
    }
}
