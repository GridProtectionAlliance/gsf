//*******************************************************************************************************
//  MultiSourceFrameImageParserBase.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TVA.Collections;

namespace TVA.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for automating the parsing of multiple
    /// binary data streams, each represented as frames with common headers and returning the parsed data via an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is more specific than the <see cref="BinaryImageParserBase"/> in that it can automate the parsing of a
    /// particular protocol that is formatted as a series of frames that have a common method of identification.
    /// Automation of type creation occurs by loading implementations of common types that implement the
    /// <see cref="ISupportFrameImage{TTypeIdentifier}"/> interface. The common method of identification is handled by
    /// creating a class derived from the <see cref="ICommonHeader{TTypeIdentifier}"/> which primarily includes a
    /// TypeID property, but also should include any state information needed to parse a particular frame if necessary.
    /// Derived classes override the <see cref="FrameImageParserBase{TTypeIdentifier, TOutputType}.ParseCommonHeader"/>
    /// method in order to parse the <see cref="ICommonHeader{TTypeIdentifier}"/> from a provided binary image.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    public abstract class MultiSourceFrameImageParserBase<TSourceIdentifier, TTypeIdentifier, TOutputType> : FrameImageParserBase<TTypeIdentifier, TOutputType> where TOutputType : ISupportFrameImage<TTypeIdentifier>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one or more objects of the <see cref="Type"/> 
        /// that the data image was for.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the source for the data image.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is a list of objects deserialized from the data image.
        /// </remarks>
        [Description("Occurs when a data image is deserialized successfully to one or more object of the Type that the data image was for.")]
        public new event EventHandler<EventArgs<TSourceIdentifier, IList<TOutputType>>> DataParsed;

        // Fields
        private ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>> m_bufferQueue;
        private Dictionary<TSourceIdentifier, bool> m_sourceInitialized;
        private Dictionary<TSourceIdentifier, byte[]> m_unparsedBuffers;
        private List<TOutputType> m_parsedOutputs;
        private TSourceIdentifier m_sourceID;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        protected MultiSourceFrameImageParserBase()
        {
            m_bufferQueue = CreateBufferQueue();
            m_bufferQueue.ProcessException += ProcessExceptionHandler;
            
            if (ProtocolUsesSyncBytes)
                m_sourceInitialized = new Dictionary<TSourceIdentifier, bool>();

            m_unparsedBuffers = new Dictionary<TSourceIdentifier, byte[]>();
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
        /// Gets the current run-time statistics of the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        public virtual ProcessQueueStatistics CurrentStatistics
        {
            get
            {
                return m_bufferQueue.CurrentStatistics;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>.
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
        /// Releases the unmanaged resources used by the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> object and optionally releases the managed resources.
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
                            m_bufferQueue.ProcessException -= ProcessExceptionHandler;
                            m_bufferQueue.Dispose();
                        }
                        m_bufferQueue = null;

                        if (m_sourceInitialized != null)
                        {
                            m_sourceInitialized.Clear();
                        }
                        m_sourceInitialized = null;

                        if (m_unparsedBuffers != null)
                        {
                            m_unparsedBuffers.Clear();
                        }
                        m_unparsedBuffers = null;

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

            if (m_sourceInitialized != null)
                m_sourceInitialized.Clear();

            m_unparsedBuffers.Clear();
            m_bufferQueue.Start();
        }

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        public override void Stop()
        {
            m_bufferQueue.Stop();
            base.Stop();
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
        /// Queues the object implementing the <see cref="ISupportBinaryImage"/> interface, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="image">Object to be parsed that implements the <see cref="ISupportBinaryImage"/> interface.</param>
        /// <remarks>
        /// This method takes the binary image from <see cref="ISupportBinaryImage"/> and writes the buffer to the <see cref="BinaryImageParserBase"/> stream for parsing.
        /// </remarks>
        public void Parse(TSourceIdentifier source, ISupportBinaryImage image)
        {
            Parse(source, image.BinaryImage);
        }

        /// <summary>
        /// Clears the internal buffer of unparsed data received from the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <remarks>
        /// This method can be used to ensure that partial data received from the <paramref name="source"/> is not kept in memory indefinitely.
        /// </remarks>
        public void PurgeBuffer(TSourceIdentifier source)
        {
            lock (m_unparsedBuffers)
            {
                if (m_unparsedBuffers.ContainsKey(source))
                    m_unparsedBuffers.Remove(source);
            }
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,ISupportBinaryImage)"/> method instead to make sure data source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This method should not be called directly.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Parse(ISupportBinaryImage image)
        {
            throw new NotImplementedException("This method should not be called directly, call the Parse(TSourceIdentifier,ISupportBinaryImage) method to queue data for parsing instead.");
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,byte[],int,int)"/> method instead to make sure data source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This method should not be called directly.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("This method should not be called directly, call the Parse(TSourceIdentifier,byte[],int,int) method to queue data for parsing instead.");
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be parsed immediately.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the user has called <see cref="Start"/> method, this method will process all remaining buffers on the calling thread until all
        /// queued buffers have been parsed - the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/>
        /// will then be automatically stopped. This method is typically called on shutdown to make sure any remaining queued buffers get
        /// parsed before the class instance is destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be queued while the flush is executing. The flush will continue to parse buffers as quickly
        /// as possible until the internal buffer queue is empty. Unless the user stops queueing data to be parsed (i.e. calling the
        /// <see cref="Parse(TSourceIdentifier,byte[])"/> method), the flush call may never return (not a happy situtation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> does not clear queue prior to destruction.
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
        /// <para>
        /// This method is virtual to allow derived classes to customize the style of processing queue used when consumers
        /// choose to implement an internal buffer queue.  Default type is a real-time queue with the default settings.
        /// When overriding, use the <see cref="ParseQueuedBuffers"/> method for the <see cref="ProcessQueue{T}"/>) item
        /// processing delegate.
        /// </para>
        /// <para>
        /// Note that current design only supports synchronous parsing - consumer overriding this method to return
        /// an asynchronous (i.e., multi-threaded) process queue will need to redesign the processing delegate.
        /// </para>
        /// </remarks>
        /// <returns>New internal buffer processing queue (i.e., a new <see cref="ProcessQueue{T}"/>).</returns>
        protected virtual ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>> CreateBufferQueue()
        {
            return ProcessQueue<IdentifiableItem<TSourceIdentifier, byte[]>>.CreateRealTimeQueue(ParseQueuedBuffers);
        }

        /// <summary>
        /// This method is used by the internal <see cref="ProcessQueue{T}"/> to process all queued data buffers.
        /// </summary>
        /// <param name="buffers">Source identifiable buffers to process.</param>
        /// <remarks>
        /// This is the item processing delegate to use when overriding the <see cref="CreateBufferQueue"/> method.
        /// </remarks>
        protected void ParseQueuedBuffers(IdentifiableItem<TSourceIdentifier, byte[]>[] buffers)
        {
            byte[] buffer;

            // Process all queued data buffers...
            foreach (IdentifiableItem<TSourceIdentifier, byte[]> item in buffers)
            {
                m_sourceID = item.ID;
                buffer = item.Item;

                // Check to see if this data source has been initialized
                if (m_sourceInitialized != null && !m_sourceInitialized.TryGetValue(m_sourceID, out StreamInitialized))
                    m_sourceInitialized.Add(m_sourceID, true);

                // Restore any unparsed buffers for this data source, if any
                lock (m_unparsedBuffers)
                {
                    if (!m_unparsedBuffers.TryGetValue(m_sourceID, out UnparsedBuffer))
                        m_unparsedBuffers.Add(m_sourceID, null);
                }

                // Clear any existing parsed outputs
                m_parsedOutputs.Clear();

                // Start parsing sequence for this buffer - this will cumulate new parsed outputs
                base.Write(buffer, 0, buffer.Length);

                // Track last unparsed buffer for this data source
                lock (m_unparsedBuffers)
                {
                    m_unparsedBuffers[m_sourceID] = UnparsedBuffer;
                }

                // Expose any parsed data
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
                DataParsed(this, new EventArgs<TSourceIdentifier, IList<TOutputType>>(sourceID, parsedData));
        }

        // Cumulate output data for current data source
        private void CumulateParsedOutput(object sender, EventArgs<TOutputType> data)
        {
            m_parsedOutputs.Add(data.Argument);
        }

        // If an error occurs during parsing from a data source, we reset its initialization state
        private void ResetDataSourceInitialization(object sender, EventArgs<byte[]> data)
        {
            if (m_sourceInitialized != null)
                m_sourceInitialized[m_sourceID] = false;
        }

        // We just bubble any exceptions captured in process queue out to parsing exception event...
        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnParsingException(e.Argument);
        }

        #endregion
    }
}
