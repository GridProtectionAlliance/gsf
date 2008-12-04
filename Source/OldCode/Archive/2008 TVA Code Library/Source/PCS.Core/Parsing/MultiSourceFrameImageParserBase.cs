//*******************************************************************************************************
//  MultiSourceFrameParserBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using PCS.Collections;

namespace PCS.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for automating the parsing of multiple
    /// binary data streams, each represented as frames with common headers and returning the parsed data via an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </para>
    /// <para>
    /// This class is more specific than the <see cref="BinaryImageParserBase"/> in that it can automate the parsing of a
    /// particular protocol that is formatted as a series of frames that have a common method of identification.
    /// Automation of type creation occurs by loading implementations of common types that implement the
    /// <see cref="ISupportFrameImage{TTypeIdentifier}"/> interface. The common method of identification is handled by
    /// creating a class derived from the <see cref="ICommonHeader{TTypeIdentifier}"/> which primarily includes a
    /// TypeID property, but also should include any state information needed to parse a particular frame if necessary.
    /// Derived classes override the <see cref="FrameImageParserBase{TTypeIdentifier, TOutputType}.ParseCommonHeader"/>
    /// function in order to parse the <see cref="ICommonHeader{TTypeIdentifier}"/> from a provided binary image.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    public abstract class MultiSourceFrameParserBase<TSourceIdentifier, TTypeIdentifier, TOutputType> : FrameImageParserBase<TTypeIdentifier, TOutputType> where TOutputType : ISupportFrameImage<TTypeIdentifier>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one or more object of the <see cref="Type"/> 
        /// that the data image was for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T1, T2}.Argument1"/> is the ID of the source for the data image.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T1, T2}.Argument2"/> is a list of objects deserialized from the data image.
        /// </para>
        /// </remarks>
        [Description("Occurs when a data image is deserialized successfully to one or more object of the Type that the data image was for.")]
        public new event EventHandler<EventArgs<TSourceIdentifier, ICollection<TOutputType>>> DataParsed;

        // Fields
        private ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>> m_bufferQueue;
        private Dictionary<TSourceIdentifier, bool> m_sourceInitialized;
        private List<TOutputType> m_parsedOutputs;
        private TSourceIdentifier m_sourceID;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        public MultiSourceFrameParserBase()
        {
            m_bufferQueue = CreateBufferQueue();
            m_bufferQueue.ProcessException += m_bufferQueue_ProcessException;
            m_sourceInitialized = new Dictionary<TSourceIdentifier, bool>();
            m_parsedOutputs = new List<TOutputType>();

            // We attach to base class events so we can cumulate outputs per data source and handle data errors
            base.DataParsed += CumulateParsedOutput;
            base.DataDiscarded += ResetDataSourceInitialization;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public virtual int QueuedBuffers
        {
            get
            {
                if (m_bufferQueue != null)
                    return m_bufferQueue.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the current run-time statistics of the <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        public virtual ProcessQueueStatistics CurrentStatistics
        {
            get
            {
                return m_bufferQueue.CurrentStatistics;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.Append(m_bufferQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> object and optionally releases the managed resources.
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

                        if (m_sourceInitialized != null)
                        {
                            m_sourceInitialized.Clear();
                        }
                        m_sourceInitialized = null;

                        if (m_parsedOutputs != null)
                        {
                            m_parsedOutputs.Clear();
                        }
                        m_parsedOutputs = null;

                        // Detach from base class events
                        base.DataParsed -= CumulateParsedOutput;
                        base.DataDiscarded -= ResetDataSourceInitialization;
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
        /// Start the streaming data parser.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_sourceInitialized.Clear();
            m_bufferQueue.Start();
        }

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_bufferQueue.Stop();
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="buffer">An array of bytes to queue for parsing</param>
        public void Parse(TSourceIdentifier source, byte[] buffer)
        {
            m_bufferQueue.Add(new IdentifiableItem<TSourceIdentifier, byte[]>(source, buffer));
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the queue.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public void Parse(TSourceIdentifier source, byte[] buffer, int offset, int count)
        {
            m_bufferQueue.Add(new IdentifiableItem<TSourceIdentifier, byte[]>(source, buffer.BlockCopy(offset, count)));
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,byte[])"/> method instead to make sure data source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This function should not be called directly.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("This function should not be called directly, call the Parse method to queue data for parsing instead.");
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be parsed immediately.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the user has called <see cref="Start"/> method, this method will process all remaining buffers on the calling thread until all
        /// queued buffers have been parsed - the <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>
        /// will then be automatically stopped. This method is typically called on shutdown to make sure any remaining queued buffers get
        /// parsed before the class instance is destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be queued while the flush is executing. The flush will continue to parse buffers as quickly
        /// as possible until the internal buffer queue is empty. Unless the user stops queueing data to be parsed (i.e. calling the
        /// <see cref="Write"/> method), the flush call may never return (not a happy situtation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> does not clear queue prior to destruction.
        /// If the user fails to call this method before the class is destructed, there may be data that remains unparsed in the internal buffer.
        /// </para>
        /// </remarks>
        public override void Flush()
        {
            m_bufferQueue.Flush();
        }

        /// <summary>
        /// Creates the internal buffer queue.
        /// </summary>
        /// <remarks>
        /// This method is virtual to allow derived classes to customize the style of processing queue used when consumers
        /// choose to implement an internal buffer queue.  Default type is a real-time queue with the default settings.
        /// </remarks>
        /// <returns>New internal buffer processing queue (i.e., a new <see cref="ProcessQueue{T}"/>).</returns>
        protected virtual ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>> CreateBufferQueue()
        {
            return ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>>.CreateRealTimeQueue(ParseQueuedBuffers);
        }

        // We process all queued data buffers that are available at once...
        private void ParseQueuedBuffers(IdentifiableItem<TSourceIdentifier, byte[]>[] buffers)
        {
            byte[] buffer;

            foreach (IdentifiableItem<TSourceIdentifier, byte[]> item in buffers)
            {
                m_sourceID = item.ID;
                buffer = item.Item;

                // Check to see if this data source has been initialized
                if (ProtocolUsesSyncBytes && !m_sourceInitialized.TryGetValue(m_sourceID, out StreamInitialized))
                    m_sourceInitialized.Add(m_sourceID, true);

                // Clear any existing parsed outputs
                m_parsedOutputs.Clear();

                // Start parsing sequence for this buffer - this will cumulate new parsed outputs
                base.Write(buffer, 0, buffer.Length);

                // Expose parsed data
                if (m_parsedOutputs.Count > 0)
                    OnDataParsed(m_sourceID, m_parsedOutputs);
            }
        }

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="sourceID">Data source ID.</param>
        /// <param name="parsedData">List of parsed events.</param>
        protected void OnDataParsed(TSourceIdentifier sourceID, List<TOutputType> parsedData)
        {            
            if (DataParsed != null)
                DataParsed(this, new EventArgs<TSourceIdentifier, ICollection<TOutputType>>(sourceID, parsedData));
        }

        // Cumulate output data for current data source
        private void CumulateParsedOutput(object sender, EventArgs<TOutputType> data)
        {
            m_parsedOutputs.Add(data.Argument);
        }

        // If an error occurs during parsing from a data source, we reset its initialization state
        private void ResetDataSourceInitialization(object sender, EventArgs<byte[]> data)
        {
            if (ProtocolUsesSyncBytes)
                m_sourceInitialized[m_sourceID] = false;
        }

        // We just bubble any exceptions captured in process queue out to parsing exception event...
        private void m_bufferQueue_ProcessException(Exception ex)
        {
            OnParsingException(ex);
        }

        #endregion
    }
}
