//*******************************************************************************************************
//  StreamParserBase.cs
//  Copyright ©2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/12/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using PCS.Configuration;
using PCS.Collections;

namespace PCS.Parsing
{
    /// <summary>
    /// This class defines the fundamental functionality for parsing any stream of binary data.
    /// </summary>
    /// <remarks>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </remarks>
    [Description("Defines the fundamental functionality for parsing any binary data stream."),
    DesignerCategory("Component"), 
    DefaultEvent("ParsingException"), 
    DefaultProperty("ExecuteParseOnSeparateThread")]
    public abstract class StreamParserBase : Stream, IComponent, IStreamParser
    {
        #region [ Members ]

        // Nested Types

        // Constants

        /// <summary>
        /// Specified the default value for the <see cref="ProtocolSyncBytes"/> property.
        /// </summary>
        public static readonly byte[] DefaultProtocolSyncBytes = new byte[] { 0xAA };

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ParsingEngine";
        
        /// <summary>
        /// Specifies the default value for the <see cref="ExecuteParseOnSeparateThread"/> property.
        /// </summary>
        public const bool DefaultExecuteParseOnSeparateThread = true;

        // Events

        /// <summary>
        /// Occurs when data image fails deserialized due to an exception.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the remaining portion of the binary image that failed to parse.
        /// </remarks>
        [Description("Occurs when data image cannot be deserialized to the output type that the data image represented.")]
        public event EventHandler<EventArgs<byte[]>> DataDiscarded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while attempting to parse data.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing data.
        /// </remarks>
        [Description("Occurs when an Exception is encountered while writing entries to the LogFile.")]
        public event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Occurs when component is disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private ProcessQueue<byte[]> m_bufferQueue;
        private byte[] m_unparsedBuffer;
        private byte[] m_protocolSyncBytes;
        private bool m_executeParseOnSeparateThread;
        private bool m_dataStreamInitialized;
        private long m_buffersProcessed;
        private long m_startTime;
        private long m_stopTime;
        private string m_settingsCategory;
        private bool m_persistSettings;
        private ISite m_componentSite;
        private bool m_initialized;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="StreamParserBase"/> class.
        /// </summary>
        protected StreamParserBase()
	    {
            m_protocolSyncBytes = DefaultProtocolSyncBytes;
            m_executeParseOnSeparateThread = DefaultExecuteParseOnSeparateThread;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data parser object is currently enabled.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        /// <summary>
        /// Gets or sets the <see cref="ISite"/> associated with the <see cref="IComponent"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="ISite"/> object associated with the component; or null, if the component does not have a site.
        /// </returns>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ISite Site
        {
            get
            {
                return m_componentSite;
            }
            set
            {
                m_componentSite = value;
            }
        }

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        [Browsable(false)]
        public abstract bool ProtocolUsesSyncBytes
        {
            get;
        }

        /// <summary>
        /// Gets or sets synchronization bytes for this parsing implementation, if used.
        /// </summary>
        [Category("Settings"),
        Description("Specifies the sequence of synchronization bytes for this parsing implementation, if used.")]
        public virtual byte[] ProtocolSyncBytes
        {
            get
            {
                return m_protocolSyncBytes;
            }
            set
            {
                m_protocolSyncBytes = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that detemines if the internal buffer queue is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this property to true enables the internal buffer queue causing any data to added to <see cref="Write"/>
        /// method to be queued for parsing on a separate thread. If the property is set to false, the parse will occur
        /// immediately on the thread that invoked the <see cref="Write"/> method.
        /// </para>
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultExecuteParseOnSeparateThread),
        Description("Indicates if an internal buffer queue is used while parsing data.")]
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
                        m_bufferQueue = CreateBufferQueue();
                        m_bufferQueue.ProcessException += m_bufferQueue_ProcessException;
                    }

                    if (m_enabled && !m_bufferQueue.Enabled)
                        m_bufferQueue.Start();

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

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of data parser object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of data parser object are to be saved to the config file.")]
        public virtual bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the settings of data parser object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of data parser object are to be saved to the config file if the PersistSettings property is set to true.")]
        public virtual string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets the total amount of time, in seconds, that the <see cref="StreamParserBase"/> has been active.
        /// </summary>
        [Browsable(false)]
        public virtual double RunTime
        {
            get
            {
                long processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                        processingTime = m_stopTime - m_startTime;
                    else
                        processingTime = DateTime.Now.Ticks - m_startTime;
                }

                if (processingTime < 0)
                    processingTime = 0;

                return Ticks.ToSeconds(processingTime);
            }
        }

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        [Browsable(false)]
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
        /// Gets the total number of buffer images processed so far.
        /// </summary>
        /// <returns>Total number of buffer images processed so far.</returns>
        [Browsable(false)]
        public long TotalProcessedBuffers
        {
            get
            {
                return m_buffersProcessed;
            }
        }

        /// <summary>
        /// Gets the current run-time statistics of the <see cref="StreamParserBase"/>.
        /// </summary>
        [Browsable(false)]
        public virtual ProcessQueueStatistics CurrentStatistics
        {
            get
            {
                if (m_bufferQueue != null)
                {
                    return m_bufferQueue.CurrentStatistics;
                }
                else
                {
                    // Infer some statistics when using calling source thread
                    ProcessQueueStatistics stats = default(ProcessQueueStatistics);
                    
                    stats.ActiveThreads = 1;
                    stats.IsEnabled = m_enabled;
                    stats.IsProcessing = true;
                    stats.ProcessingStyle = QueueProcessingStyle.OneAtATime;
                    stats.ProcessTimeout = Timeout.Infinite;
                    stats.ThreadingMode = QueueThreadingMode.Synchronous;
                    stats.RunTime = RunTime;
                    stats.TotalProcessedItems = m_buffersProcessed;

                    return stats;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <remarks>
        /// The <see cref="StreamParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
        /// </remarks>
        [Browsable(false)]
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <remarks>
        /// The <see cref="StreamParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
        /// </remarks>
        [Browsable(false)]
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <remarks>
        /// The <see cref="StreamParserBase"/> is implemented as a WriteOnly stream, so this defaults to true.
        /// </remarks>
        [Browsable(false)]
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        
        /// <summary>
        /// Gets the unique display name of the <see cref="StreamParserBase"/> object.
        /// </summary>
        [Browsable(false)]
        public virtual string Name
        {
            get
            {
                // We just return the settings category name for unique identification of this component
                return m_settingsCategory;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="StreamParserBase"/>.
        /// </summary>
        [Browsable(false)]
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
                    status.Append("Data synchronization bytes: ");
                    status.Append(ByteEncoding.Hexadecimal.GetString(ProtocolSyncBytes));
                    status.AppendLine();
                }

                status.Append("  Parsing execution source: ");
                status.Append(m_executeParseOnSeparateThread ? "Independent thread using queued data" : "Data acquisition thread");
                status.AppendLine();
                status.Append("     Total parser run-time: ");
                status.Append(Seconds.ToText(RunTime));
                status.AppendLine();
                status.Append("   Total buffers processed: ");
                status.Append(m_buffersProcessed);
                status.AppendLine();

                if (m_bufferQueue != null)
                    status.Append(m_bufferQueue.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets design mode of component site, if component has a site; or false, if the component does not have a site.
        /// </summary>
        [Browsable(false)]
        protected virtual bool DesignMode
        {
            get
            {
                return m_componentSite != null && m_componentSite.DesignMode;
            }
        }

        #endregion

        #region [ Methods ]
				
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="StreamParserBase"/> object and optionally releases the managed resources.
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
                            m_bufferQueue.Dispose();
                            m_bufferQueue.ProcessException -= m_bufferQueue_ProcessException;
                        }
                        m_bufferQueue = null;

                        m_unparsedBuffer = null;
                        m_enabled = false;
                    }
                    
                    // Since this is a component we raise the Disposed event
                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
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
        public virtual void Start()
        {
            // Make sure parser is initialized
            Initialize();

            m_buffersProcessed = 0;
            m_stopTime = 0;
            m_startTime = DateTime.Now.Ticks;

            // Initialized state depends whether or not derived class uses a protocol synchrnonization byte
            m_dataStreamInitialized = !ProtocolUsesSyncBytes;
            
            if (m_executeParseOnSeparateThread)
                m_bufferQueue.Start();

            m_enabled = true;
        }

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;

            if (m_executeParseOnSeparateThread)
                m_bufferQueue.Stop();

            m_stopTime = DateTime.Now.Ticks;
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
            // If ProtocolUsesSyncByte is true, first call to write after start will be uninitialized,
            // thus the attempt below to "align" data stream to specified ProtocolSyncBytes.
            if (m_dataStreamInitialized)
            {
                if (m_executeParseOnSeparateThread)
                {
                    // Queue up received data buffer for parsing and return to data collection as quickly as possible...
                    // Since source buffer may be reused, we queue a "copy" the buffer for later processing.
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
                // Initial stream may be anywhere in the middle of a frame, so we attempt to locate sync byte(s) to "line-up" data stream
                int syncBytesPosition = buffer.IndexOfSequence(ProtocolSyncBytes, offset, count);

                if (syncBytesPosition > -1)
                {
                    // Initialize data stream starting at located sync byte
                    if (m_executeParseOnSeparateThread)
                    {
                        // Since source buffer may be reused, we queue a "copy" the buffer for later processing.
                        m_bufferQueue.Add(buffer.BlockCopy(syncBytesPosition, count - syncBytesPosition));
                    }
                    else
                    {
                        ParseBuffer(buffer, syncBytesPosition, count - syncBytesPosition);
                    }

                    m_dataStreamInitialized = true;
                }
            }

            // Track total processed buffer images
            m_buffersProcessed++;
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be parsed immediately.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note this method has no effect if <see cref="ExecuteParseOnSeparateThread"/> is false, i.e., there will be nothing to
        /// flush if the internal buffer queue is not enabled.
        /// </para>
        /// <para>
        /// If the user has called <see cref="Start"/> method, this method will process all remaining buffers on the calling thread
        /// until all queued buffers have been parsed - the <see cref="StreamParserBase"/> will then be automatically stopped. This
        /// method is typically called on shutdown to make sure any remaining queued buffers get parsed before the class instance is
        /// destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be queued while the flush is executing. The flush will continue to parse buffers as quickly
        /// as possible until the internal buffer queue is empty. Unless the user stops queueing data to be parsed (i.e. calling the
        /// <see cref="Write"/> method), the flush call may never return (not a happy situtation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="StreamParserBase"/> does not clear queue prior to destruction. If the user fails to call this method
        /// before the class is destructed, there may be data that remains unparsed in the internal buffer.
        /// </para>
        /// </remarks>
        public override void Flush()
        {
            if (m_bufferQueue != null)
                m_bufferQueue.Flush();
        }

        #region [ Unimplemented Stream Overrides ]

        // The parser is designed as a write only stream - so the following methods do not apply

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Cannnot read from WriteOnly stream.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("Cannnot read from WriteOnly stream.");
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no position.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException("WriteOnly stream has no position.");
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no length.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void SetLength(long value)
        {
            throw new NotImplementedException("WriteOnly stream has no length.");
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no length.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Length
        {
            get
            {
                throw new NotImplementedException("WriteOnly stream has no length.");
            }
        }

        /// <summary>
        /// The parser is designed as a write only stream, so this method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">WriteOnly stream has no position.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override long Position
        {
            get
            {
                throw new NotImplementedException("WriteOnly stream has no position.");
            }
            set
            {
                throw new NotImplementedException("WriteOnly stream has no position.");
            }
        }

        #endregion

        /// <summary>
        /// Initializes the data parser object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the data parser 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves settings for the data parser object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["ExecuteParseOnSeparateThread", true].Update(m_executeParseOnSeparateThread, "True if the internal buffer queue is enabled; otherwise False.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the data parser object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                ExecuteParseOnSeparateThread = settings["ExecuteParseOnSeparateThread", true].ValueAs(m_executeParseOnSeparateThread);
            }
        }

        /// <summary>
        /// Performs necessary operations before the data parser object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the data parser object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
        }

        /// <summary>
        /// Performs necessary operations after the data parser object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the data parser object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void EndInit()
        {
            if (!DesignMode)
                Initialize();
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
        /// Implementors can choose to focus on parsing a single frame in the buffer even if there are other frames available in the buffer.
        /// Base class will continue to move through buffer on behalf of derived class until all the buffer has been processed.  Just return
        /// the total amount of data was parsed and the remaining unparsed will be prepended to next received buffer.
        /// </para>
        /// <para>
        /// Derived implementations should return an integer value that represents the length of the data that was parsed, and zero if not
        /// enough data was able to be parsed. Note that exceptions are expensive when parsing fast moving streaming data and a good coding
        /// practice for implementations of this method will be to not throw an exception when there is not enough data to parse the data,
        /// instead check the <paramref name="length"/> property to insure there is enough data in buffer to represent the desired image. If
        /// there is not enough data to represent the image return zero and the base class will prepend buffer onto next incoming set of data.
        /// </para>
        /// <para>
        /// Because of the expense incurred when an exception is thrown, any exceptions encounted in the derived implementations of this method
        /// will cause the current data buffer to be discarded and a <see cref="ParsingException"/> event to be raised.  Doing this prevents
        /// exceptions from being thrown repeatedly for the same data. If your code implementation recognizes a malformed image, raise a custom
        /// event to indicate this instead of throwing as exception and keep moving through the buffer.
        /// </para>
        /// </remarks>
        protected abstract int ParseFrame(byte[] buffer, int offset, int length);

        /// <summary>
        /// Raises the <see cref="DataDiscarded"/> event.
        /// </summary>
        /// <param name="buffer">Source buffer that contains output that failed to parse.</param>
        protected virtual void OnDataDiscarded(byte[] buffer)
        {
            if (DataDiscarded != null)
                DataDiscarded(this, new EventArgs<byte[]>(buffer));
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that was encountered during parsing.</param>
        protected virtual void OnParsingException(Exception ex)
        {
            if (ParsingException != null)
                ParsingException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Creates the internal buffer queue that will be used when <see cref="ExecuteParseOnSeparateThread"/> is set to true.
        /// </summary>
        /// <remarks>
        /// This method is virtual to allow derived classes to customize the style of processing queue used when consumers
        /// choose to implement an internal buffer queue.  Default type is a real-time queue with the default settings.
        /// </remarks>
        /// <returns>New internal buffer processing queue (i.e., a new <see cref="ProcessQueue{T}"/>).</returns>
        protected virtual ProcessQueue<byte[]> CreateBufferQueue()
        {
            return ProcessQueue<byte[]>.CreateRealTimeQueue(ParseQueuedBuffers);
        }

        // We process all queued data buffers that are available at once...
        private void ParseQueuedBuffers(byte[][] buffers)
        {
            byte[] combinedBuffer;

            // Process queue ensures that there will always be at least one buffer...
            if (buffers.Length > 1)
                combinedBuffer = buffers.Combine();
            else
                combinedBuffer = buffers[0];    // Parse single buffer directly as an optimization

            // Parse combined buffers
            ParseBuffer(combinedBuffer, 0, combinedBuffer.Length);
        }

        // Parse buffer image - user implements protocol specific "ParseFrame" function to extract data from image
        private void ParseBuffer(byte[] buffer, int offset, int count)
        {
            try
            {
                // Prepend any left over buffer data from last parse call
                if (m_unparsedBuffer != null)
                {
                    // Combine remaining buffer from last call and current buffer together as a single image
                    buffer = m_unparsedBuffer.Combine(0, m_unparsedBuffer.Length, buffer, offset, count);
                    offset = 0;
                    count = buffer.Length;
                    m_unparsedBuffer = null;
                }

                int endOfBuffer = offset + count - 1;
                int parsedFrameLength;

                // Move through buffer parsing all available frames
                while (!(offset > endOfBuffer))
                {
                    // Call derived class frame parsing algorithm - this is protocol specific
                    parsedFrameLength = ParseFrame(buffer, offset, endOfBuffer - offset + 1);

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
                        m_unparsedBuffer = buffer.BlockCopy(offset, count - offset);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Probable malformed data image, discard data on move on...
                m_dataStreamInitialized = !ProtocolUsesSyncBytes;
                m_unparsedBuffer = null;

                OnDataDiscarded(buffer.BlockCopy(offset, count - offset));
                OnParsingException(ex);
            }
        }

        // We just bubble any exceptions captured in process queue out to parsing exception event...
        private void m_bufferQueue_ProcessException(Exception ex)
        {
            OnParsingException(ex);
        }

        #endregion
    }
}
