//*******************************************************************************************************
//  FrameParserBase.cs
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
//  02/12/2007 - James R Carroll
//       Generated original version of source code.
//  12/16/2008 - James R Carroll
//      Converted class to inherit from FrameImageParserBase.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using TVA.Collections;
using TVA.Parsing;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents a frame parser that defines the basic functionality for a protocol to parse a binary data stream and return the parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parsers are implemented as a write-only streams so that data can come from any source.<br/>
    /// See <see cref="FrameImageParserBase{TFrameIdentifier, TCommonFrameHeader}"/> for more detail.
    /// </remarks>
    /// <typeparam name="TFrameIdentifier">Frame type identifier used to distinguish frames.</typeparam>
    public abstract class FrameParserBase<TFrameIdentifier> : FrameImageParserBase<TFrameIdentifier, ISupportFrameImage<TFrameIdentifier>>, IFrameParser
    {
        #region [ Members ]

        // Events

        // Derived classes will typically also expose events to provide instances to the protocol specific final derived channel frames

        /// <summary>
        /// Occurs when a <see cref="IConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a <see cref="IDataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IDataFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IDataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when a <see cref="IHeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IHeaderFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IHeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ICommandFrame"/> that was received.
        /// <para>
        /// Command frames are normally sent, not received, but there is nothing that prevents this.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> ReceivedCommandFrame;

        /// <summary>
        /// Occurs when an undetermined <see cref="IChannelFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the undetermined <see cref="IChannelFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IChannelFrame>> ReceivedUndeterminedFrame;

        /// <summary>
        /// Occurs when a frame buffer image has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument2"/> is the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument3"/> is the offset into the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument4"/> is the length of data in the buffer that contains the frame image that was received..
        /// </remarks>
        public event EventHandler<EventArgs<FundamentalFrameType, byte[], int, int>> ReceivedFrameBufferImage;

        /// <summary>
        /// Occurs when a device sends a notification that its configuration has changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        // Fields
        private ProcessQueue<byte[]> m_bufferQueue;
        private IConnectionParameters m_connectionParameters;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParserBase{TypeIndentifier}"/>.
        /// </summary>
        protected FrameParserBase()
        {
            // We attach to base class DataParsed event to automatically redirect and cast channel frames to their specific output events
            base.DataParsed += base_DataParsed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a flag that allows frame parsing to be executed on a separate thread (i.e., other than communications thread).
        /// </summary>
        /// <remarks>
        /// This is typically only needed when data frames are very large. This change will happen dynamically, even if a connection is active.
        /// </remarks>
        public virtual bool ExecuteParseOnSeparateThread
        {
            get
            {
                return (m_bufferQueue != null);
            }
            set
            {
                // This property allows a dynamic change in state of how to process streaming data
                if (value)
                {
                    if (m_bufferQueue == null)
                    {
                        m_bufferQueue = CreateBufferQueue();
                        m_bufferQueue.ProcessException += m_bufferQueue_ProcessException;
                    }

                    if (Enabled && !m_bufferQueue.Enabled)
                        m_bufferQueue.Start();
                }
                else
                {
                    if (m_bufferQueue != null)
                    {
                        m_bufferQueue.Stop();
                        m_bufferQueue.ProcessException -= m_bufferQueue_ProcessException;
                    }

                    m_bufferQueue = null;
                }
            }
        }

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public virtual int QueuedBuffers
        {
            get
            {
                if (m_bufferQueue == null)
                    return 0;
                else
                    return m_bufferQueue.Count;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        public abstract IConfigurationFrame ConfigurationFrame { get; set; }

        /// <summary>
        /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be needed for parsing.
        /// </summary>
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

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParserBase{TypeIndentifier}"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append(base.Status);
                status.Append("     Received config frame: ");
                status.Append(ConfigurationFrame == null ? "No" : "Yes");
                status.AppendLine();
                if (ConfigurationFrame != null)
                {
                    status.Append("   Devices in config frame: ");
                    status.Append(ConfigurationFrame.Cells.Count);
                    status.Append(" total - ");
                    status.AppendLine();
                    for (int x = 0; x < ConfigurationFrame.Cells.Count; x++)
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
                if (m_bufferQueue == null)
                {
                    status.Append("Communications thread");
                    status.AppendLine();
                }
                else
                {
                    status.Append("Independent thread using queued data");
                    status.AppendLine();
                    status.Append(m_bufferQueue.Status);
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameParserBase{TypeIndentifier}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_bufferQueue != null)
                        {
                            m_bufferQueue.ProcessException -= m_bufferQueue_ProcessException;
                            m_bufferQueue.Dispose();
                        }
                        m_bufferQueue = null;

                        // Detach from base class events
                        base.DataParsed -= base_DataParsed;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the frame parser given the specified type implementations.
        /// </summary>
        public override void Start(IEnumerable<Type> implementations)
        {
            base.Start(implementations);

            if (m_bufferQueue != null)
                m_bufferQueue.Start();
        }

        /// <summary>
        /// Stops the frame parser.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (m_bufferQueue != null)
                m_bufferQueue.Stop();
        }

        /// <summary>
        /// Writes a sequence of bytes onto the stream for parsing.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_bufferQueue == null)
                // Directly parse frame using calling thread (typically communications thread)
                base.Write(buffer, offset, count);
            else
                // Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                m_bufferQueue.Add(buffer.BlockCopy(offset, count));
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be parsed immediately.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only relevant when <see cref="ExecuteParseOnSeparateThread"/> is true; otherwise this method has no effect.
        /// </para>
        /// <para>
        /// If the user has called <see cref="Start"/> method, this method will process all remaining buffers on the calling thread
        /// until all queued buffers have been parsed - the <see cref="FrameParserBase{TFrameIdentifier}"/> will then be automatically
        /// stopped. This method is typically called on shutdown to make sure any remaining queued buffers get parsed before the class
        /// instance is destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be queued while the flush is executing. The flush will continue to parse buffers as quickly
        /// as possible until the internal buffer queue is empty. Unless the user stops queueing data to be parsed (i.e. calling the
        /// <see cref="Write"/> method), the flush call may never return (not a happy situtation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="FrameParserBase{TFrameIdentifier}"/> does not clear queue prior to destruction. If the user fails to call
        /// this method before the class is destructed, there may be data that remains unparsed in the internal buffer.
        /// </para>
        /// </remarks>
        public override void Flush()
        {
            if (m_bufferQueue != null)
                m_bufferQueue.Flush();
        }

        /// <summary>
        /// Raises the <see cref="ReceivedConfigurationFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IConfigurationFrame"/> to send to <see cref="ReceivedConfigurationFrame"/> event.</param>
        protected virtual void OnReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            if (ReceivedConfigurationFrame != null)
                ReceivedConfigurationFrame(this, new EventArgs<IConfigurationFrame>(frame));
        }

        /// <summary>
        /// Raises the <see cref="ReceivedDataFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IDataFrame"/> to send to <see cref="ReceivedDataFrame"/> event.</param>
        protected virtual void OnReceivedDataFrame(IDataFrame frame)
        {
            if (ReceivedDataFrame != null)
                ReceivedDataFrame(this, new EventArgs<IDataFrame>(frame));
        }

        /// <summary>
        /// Raises the <see cref="ReceivedHeaderFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IHeaderFrame"/> to send to <see cref="ReceivedHeaderFrame"/> event.</param>
        protected virtual void OnReceivedHeaderFrame(IHeaderFrame frame)
        {
            if (ReceivedHeaderFrame != null)
                ReceivedHeaderFrame(this, new EventArgs<IHeaderFrame>(frame));
        }

        /// <summary>
        /// Raises the <see cref="ReceivedCommandFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="ICommandFrame"/> to send to <see cref="ReceivedCommandFrame"/> event.</param>
        protected virtual void OnReceivedCommandFrame(ICommandFrame frame)
        {
            if (ReceivedCommandFrame != null)
                ReceivedCommandFrame(this, new EventArgs<ICommandFrame>(frame));
        }

        /// <summary>
        /// Raises the <see cref="ReceivedUndeterminedFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> to send to <see cref="ReceivedUndeterminedFrame"/> event.</param>
        protected virtual void OnReceivedUndeterminedFrame(IChannelFrame frame)
        {
            if (ReceivedUndeterminedFrame != null)
                ReceivedUndeterminedFrame(this, new EventArgs<IChannelFrame>(frame));
        }

        /// <summary>
        /// Raises the <see cref="ReceivedFrameBufferImage"/> event.
        /// </summary>
        /// <param name="frameType"><see cref="FundamentalFrameType"/> to send to <see cref="ReceivedFrameBufferImage"/> event.</param>
        /// <param name="binaryImage">Frame buffer image to send to <see cref="ReceivedFrameBufferImage"/> event.</param>
        /// <param name="offset">Offset into frame buffer image to send to <see cref="ReceivedFrameBufferImage"/> event.</param>
        /// <param name="length">Length of data in frame buffer image to send to <see cref="ReceivedFrameBufferImage"/> event.</param>
        protected virtual void OnReceivedFrameBufferImage(FundamentalFrameType frameType, byte[] binaryImage, int offset, int length)
        {
            if (ReceivedFrameBufferImage != null)
                ReceivedFrameBufferImage(this, new EventArgs<FundamentalFrameType, byte[], int, int>(frameType, binaryImage, offset, length));
        }

        /// <summary>
        /// Raises the <see cref="ConfigurationChanged"/> event.
        /// </summary>
        protected virtual void OnConfigurationChanged()
        {
            if (ConfigurationChanged != null)
                ConfigurationChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="IDataFrame"/>, <see cref="IConfigurationFrame"/>, <see cref="ICommandFrame"/> or <see cref="IHeaderFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected virtual void OnReceivedChannelFrame(IChannelFrame frame)
        {
            if (frame != null)
            {
                IDataFrame dataFrame = frame as IDataFrame;

                if (dataFrame != null)
                {
                    // Frame was a data frame
                    OnReceivedDataFrame(dataFrame);
                }
                else
                {
                    IConfigurationFrame configFrame = frame as IConfigurationFrame;

                    if (configFrame != null)
                    {
                        // Frame was a configuration frame
                        OnReceivedConfigurationFrame(configFrame);
                    }
                    else
                    {
                        IHeaderFrame headerFrame = frame as IHeaderFrame;

                        if (headerFrame != null)
                        {
                            // Frame was a header frame
                            OnReceivedHeaderFrame(headerFrame);
                        }
                        else
                        {
                            ICommandFrame commandFrame = frame as ICommandFrame;

                            if (commandFrame != null)
                            {
                                // Frame was a command frame
                                OnReceivedCommandFrame(commandFrame);
                            }
                            else
                            {
                                // Frame type was undetermined
                                OnReceivedUndeterminedFrame(frame);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the internal buffer queue.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is virtual to allow derived classes to customize the style of processing queue used when consumers
        /// choose to implement an internal buffer queue (i.e., set <see cref="ExecuteParseOnSeparateThread"/> to true).
        /// Default type is a real-time queue with the default settings. When overriding this method, be sure to use the
        /// <see cref="ParseQueuedBuffers"/> method for the <see cref="ProcessQueue{T}"/>) item processing delegate.
        /// </para>
        /// <para>
        /// Note that current design only supports synchronous parsing - consumer overriding this method to return
        /// an asynchronous (i.e., multi-threaded) process queue will need to redesign the processing delegate.
        /// </para>
        /// </remarks>
        /// <returns>New internal buffer processing queue (i.e., a new <see cref="ProcessQueue{T}"/>).</returns>
        protected virtual ProcessQueue<byte[]> CreateBufferQueue()
        {
            return ProcessQueue<byte[]>.CreateRealTimeQueue(ParseQueuedBuffers);
        }

        /// <summary>
        /// This method is used by the internal <see cref="ProcessQueue{T}"/> to process all queued data buffers.
        /// </summary>
        /// <param name="buffers">Queued buffers to process.</param>
        /// <remarks>
        /// This is the item processing delegate to use when overriding the <see cref="CreateBufferQueue"/> method.
        /// </remarks>
        protected void ParseQueuedBuffers(byte[][] buffers)
        {
            // Parse combined data buffers
            byte[] combinedBuffers = buffers.Combine();
            base.Write(combinedBuffers, 0, combinedBuffers.Length);
        }

        // Handles reception of data from base class event "DataParsed"
        private void base_DataParsed(object sender, EventArgs<ISupportFrameImage<TFrameIdentifier>> e)
        {
            // Call overridable channel frame function handler...
            OnReceivedChannelFrame(e.Argument as IChannelFrame);
        }

        // Handles any exceptions encountered in the buffer queue
        private void m_bufferQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnParsingException(e.Argument);
        }

        #endregion
    }
}