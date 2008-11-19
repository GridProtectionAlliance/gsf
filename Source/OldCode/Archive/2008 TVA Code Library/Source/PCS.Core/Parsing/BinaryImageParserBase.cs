//*******************************************************************************************************
//  FrameParserBase.cs
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
//  02/12/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using PCS.Configuration;
using PCS.Collections;

namespace PCS.Parsing
{
    /// <summary>
    /// This class defines the basic functionality for parsing a binary data stream and return the parsed data via events.
    /// </summary>
    /// <remarks>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </remarks>
    [Description("Defines the basic functionality for parsing a binary data stream and return the parsed data via events."), DefaultEvent("ParsingException"), DefaultProperty("ExecuteParseOnSeparateThread")]
    public abstract class FrameParserBase : Stream, IComponent, IFrameParser
    {
        #region [ Members ]

        // Nested Types

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "BinaryDataParser";
        
        /// <summary>
        /// Default data stream protocol synchrnonization byte.
        /// </summary>
        public const byte DefaultProtocolSyncByte = 0xAA;

        // Delegates

        // Events

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
        private bool m_executeParseOnSeparateThread;
        private MemoryStream m_dataStream;
        private byte m_protocolSyncByte;
        private ISite m_componentSite;
        private string m_settingsCategory;
        private bool m_persistSettings;
        private bool m_initialized;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FrameParserBase"/> class.
        /// </summary>
        protected FrameParserBase()
	    {
            m_protocolSyncByte = DefaultProtocolSyncByte;
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
        public ISite Site
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
        /// Gets or sets flag that detemines if the internal buffer queue is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this property to true enables the internal buffer queue causing any data to added to <see cref="Write"/>
        /// method to be queued for parsing on a separate thread. If the property is set to false, the parse will occur
        /// immediately on the thread that invoked the <see cref="Write"/> method.
        /// </para>
        /// </remarks>
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
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        [Browsable(false)]
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

        /// <summary>
        /// Gets flag that determines if this parsing implementation uses a synchronization byte.
        /// </summary>
        [Browsable(false)]
        public abstract bool ProtocolUsesSyncByte
        {
            get;
        }

        /// <summary>
        /// Gets or sets synchronization byte for this parsing implementation, is used.
        /// </summary>
        [Browsable(false)]
        public virtual byte ProtocolSyncByte
        {
            get
            {
                return m_protocolSyncByte;
            }
            set
            {
                m_protocolSyncByte = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <remarks>
        /// The <see cref="FrameParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
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
        /// The <see cref="FrameParserBase"/> is implemented as a WriteOnly stream, so this defaults to false.
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
        /// The <see cref="FrameParserBase"/> is implemented as a WriteOnly stream, so this defaults to true.
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
        /// Gets current status of <see cref="FrameParserBase"/>.
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
                if (ProtocolUsesSyncByte)
                {
                    status.Append(" Data synchronization byte: 0x");
                    status.Append(ProtocolSyncByte.ToString("X"));
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
                    status.Append("Data source acquisition thread");
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]
				
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameParserBase"/> object and optionally releases the managed resources.
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

                        if (m_dataStream != null) m_dataStream.Dispose();
                        m_dataStream = null;

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
        /// Start the data parser.
        /// </summary>
        public virtual void Start()
        {
            m_initialized = !ProtocolUsesSyncByte;
            
            if (m_executeParseOnSeparateThread)
                m_bufferQueue.Start();

            m_enabled = true;
        }

        /// <summary>
        /// Stops the data parser.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;

            if (m_executeParseOnSeparateThread)
                m_bufferQueue.Stop();
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
            // thus the attempt below to "align" data stream to specified ProtocolSyncByte.
            if (m_initialized)
            {
                if (m_executeParseOnSeparateThread)
                {
                    // Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                    // Since source buffer may be reused, we queue a "copy" the buffer.
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
                        // Since source buffer may be reused, we queue a "copy" the buffer.
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

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be parsed immediately.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note this method has no effect if <see cref="ExecuteParseOnSeparateThread"/> is false, i.e., there will be nothing to
        /// flush if the internal buffer queue is not enabled.
        /// </para>
        /// <para>
        /// If the user has called <see cref="Start"/> method, this method will block the current thread until all queued buffers
        /// have been parsed - the <see cref="FrameParserBase"/> will then be automatically stopped. This method is typically called
        /// on shutdown to make sure any remaining queued buffers get parsed before the class instance is destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be queued while the flush is executing. The flush will continue to parse buffers as quickly
        /// as possible until the internal buffer queue is empty. Unless the user stops queueing data to be parsed (i.e. calling the
        /// <see cref="Write"/> method), the flush call may never return (not a happy situtation on shutdown).
        /// <para>
        /// The <see cref="FramePraserBase"/> does not clear queue prior to destruction. If the user fails to call this method before the
        /// class is destructed, there may be data that remains unparsed in the internal buffer.
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
        public void Initialize()
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
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["OptimizeParsing", true].Update(m_optimizeParsing, "True if data parsing is to be done in an optimal mode; otherwise False.");
                settings["DataAssemblyAttempts", true].Update(m_dataAssemblyAttempts, "Number of attempts to be made for assembling partial data images before parsing it.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the data parser object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                OptimizeParsing = settings["OptimizeParsing", true].ValueAs(m_optimizeParsing);
                DataAssemblyAttempts = settings["DataAssemblyAttempts", true].ValueAs(m_dataAssemblyAttempts);
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
        public void BeginInit()
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
        public void EndInit()
        {
            if (!DesignMode)
                Initialize();
        }

        /// <summary>
        /// Protocol specific frame parsing algorithm.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing</param>
        /// <param name="length">Maximum length of valid data from offset</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// <para>
        /// Implementors can choose to focus on parsing a single frame in the buffer even if there are other frames available in the buffer.
        /// Base class will continue to move through buffer on behalf of derived class until all the buffer has been processed.  Just return
        /// the total amount of data was parsed and the remaining unparsed will be prepended to next received buffer.
        /// </para>
        /// <para>
        /// Derived implementations should return an integer value that represents the length of the data that was parsed, and zero if no
        /// data was able to be parsed. Note that exceptions are expensive when parsing fast moving streaming data and a good code practice
        /// for implementations of this method will be to not throw an exception when there is not enough data to parse the data, instead
        /// check the <paramref name="length"/> property to insure there is enough data in buffer to represent the desired image. If there
        /// is not enough data to represent the image return zero and the base class will prepend buffer onto next incoming set of data.
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
                    // Call derived class frame parsing algorithm - this is protocol specific
                    parsedFrameLength = ParseFrame(buffer, offset, endOfBuffer - offset + 1);

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
                m_initialized = !ProtocolUsesSyncByte;;
                m_dataStream = null;
                OnParsingException(ex);
            }
        }

        private void m_bufferQueue_ProcessException(Exception ex)
        {
            OnParsingException(ex);
        }

        #endregion
    }
}
