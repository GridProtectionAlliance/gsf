//*******************************************************************************************************
//  LogFile.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/09/2007 - Pinal C. Patel
//       Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//  09/19/2008 - James R Carroll
//       Converted to C#.
//  10/21/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using PCS.Collections;
using PCS.Configuration;

namespace PCS.IO
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the operation to be performed on the <see cref="LogFile"/> when it is full.
    /// </summary>
    public enum LogFileFullOperation
    {
        /// <summary>
        /// Truncates the existing entries in the <see cref="LogFile"/> to make space for new entries.
        /// </summary>
        Truncate,
        /// <summary>
        /// Rolls over to a new <see cref="LogFile"/>, and keeps the full <see cref="LogFile"/> for reference.
        /// </summary>
        Rollover
    }

    #endregion

    /// <summary>
    /// Represents a file that can be used for logging messages in real-time.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="LogFile"/> for logging messages:
    /// <code>
    /// using System;
    /// using PCS.IO;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         LogFile log = new LogFile();
    ///         log.Initialize();                       // Initialize the log file.
    ///         log.Open();                             // Open the log file.
    ///         log.WriteTimestampedLine("Test entry"); // Write message to the log file.
    ///         log.Flush();                            // Flush message to the log file.
    ///         log.Close();                            // Close the log file.
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    [ToolboxBitmap(typeof(LogFile))]
    public class LogFile : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the minimum size for a <see cref="LogFile"/>.
        /// </summary>
        public const int MinFileSize = 1;

        /// <summary>
        /// Specifies the maximum size for a <see cref="LogFile"/>.
        /// </summary>
        public const int MaxFileSize = 10;

        /// <summary>
        /// Specifies the default value for the <see cref="FileName"/> property.
        /// </summary>
        public const string DefaultFileName = "LogFile.txt";

        /// <summary>
        /// Specifies the default value for the <see cref="FileSize"/> property.
        /// </summary>
        public const int DefaultFileSize = 3;

        /// <summary>
        /// Specifies the default value for the <see cref="FileFullOperation"/> property.
        /// </summary>
        public const LogFileFullOperation DefaultFileFullOperation = LogFileFullOperation.Truncate;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "LogFile";

        // Events

        /// <summary>
        /// Occurs when the <see cref="LogFile"/> is full.
        /// </summary>
        [Description("Occurs when the LogFile is full.")]
        public event EventHandler FileFull;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while writing entries to the <see cref="LogFile"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered during writing of log entries.
        /// </remarks>
        [Description("Occurs when an Exception is encountered while writing entries to the LogFile.")]
        public event EventHandler<EventArgs<Exception>> LogException;

        // Fields
        private string m_fileName;
        private int m_fileSize;
        private LogFileFullOperation m_fileFullOperation;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private FileStream m_fileStream;
        private ManualResetEvent m_operationWaitHandle;
        private ProcessQueue<string> m_logEntryQueue;
        private Encoding m_textEncoding;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        public LogFile()
            : base()
        {
            m_fileName = DefaultFileName;
            m_fileSize = DefaultFileSize;
            m_fileFullOperation = DefaultFileFullOperation;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_textEncoding = Encoding.Default;
            m_operationWaitHandle = new ManualResetEvent(true);
            m_logEntryQueue = ProcessQueue<string>.CreateSynchronousQueue(WriteLogEntries);

            this.FileFull += LogFile_FileFull;
            m_logEntryQueue.ProcessException += ProcessExceptionHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="LogFile"/>.</param>
        public LogFile(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="LogFile"/>, including the file extension.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        [Category("Settings"),
        DefaultValue(DefaultFileName),
        Description("Name of the LogFile, including the file extension.")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_fileName = value;
                if (IsOpen)
                {
                    Close();
                    Open();
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the <see cref="LogFile"/> in MB.
        /// </summary>
        ///<exception cref="ArgumentOutOfRangeException">The value being set outside the <see cref="MinFileSize"/> and <see cref="MaxFileSize"/> range.</exception>
        [Category("Settings"),
        DefaultValue(DefaultFileSize),
        Description("Size of the LogFile in MB.")]
        public int FileSize
        {
            get
            {
                return m_fileSize;
            }
            set
            {
                if (value < MinFileSize || value > MaxFileSize)
                    throw new ArgumentOutOfRangeException("FileSize", string.Format("Value must be between {0} and {1}.", MinFileSize, MaxFileSize));

                m_fileSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of operation to be performed when the <see cref="LogFile"/> is full.
        /// </summary>
        [Category("Behavior"),
        DefaultValue(DefaultFileFullOperation),
        Description("Type of operation to be performed when the LogFile is full.")]
        public LogFileFullOperation FileFullOperation
        {
            get
            {
                return m_fileFullOperation;
            }
            set
            {
                m_fileFullOperation = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="LogFile"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of LogFile object are to be saved to the config file.")]
        public bool PersistSettings
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
        /// Gets or sets the category under which the settings of <see cref="LogFile"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of LogFile object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used to encode the messages being logged.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get
            {
                return m_textEncoding;
            }
            set
            {
                if (value == null)
                    m_textEncoding = Encoding.Default;
                else
                    m_textEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="LogFile"/> object is currently enabled.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_logEntryQueue.Enabled;
            }
            set
            {
                m_logEntryQueue.Enabled = value;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="LogFile"/> is open.
        /// </summary>
        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return (m_fileStream != null);
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="LogFile"/> object.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return m_settingsCategory;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="LogFile"/> object.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append("     Configuration section: ");
                status.Append(m_settingsCategory);
                status.AppendLine();
                status.Append("       Maximum export size: ");
                status.Append(m_fileSize.ToString());
                status.Append(" MB");
                status.AppendLine();
                status.Append("       File full operation: ");
                status.Append(m_fileFullOperation);
                status.AppendLine();

                if (m_logEntryQueue != null)
                    status.Append(m_logEntryQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the <see cref="LogFile"/> for use if it is closed.
        /// </summary>
        public void Open()
        {
            if (!IsOpen)
            {
                // Initialize if uninitialized.
                Initialize();

                // Get the absolute file path if a relative path is specified.
                m_fileName = FilePath.GetAbsolutePath(m_fileName);

                // Create the folder in which the log file will reside it, if it does not exist.
                if (!Directory.Exists(FilePath.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(FilePath.GetDirectoryName(m_fileName));

                // Open the log file (if it exists) or creates it (if it does not exist).
                m_fileStream = new FileStream(m_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                // Scrolls to the end of the file so that existing data is not overwritten.
                m_fileStream.Seek(0, SeekOrigin.End);

                // If this is a new log file, set its creation date to current date. This is done to prevent historic
                // log files (when FileFullOperation = Rollover) from having the same start time in their filename.
                if (m_fileStream.Length == 0)
                    (new FileInfo(m_fileName)).CreationTime = DateTime.Now;

                // Start the queue to which log entries are going to be added.
                m_logEntryQueue.Start();
            }
        }

        /// <summary>
        /// Closes the <see cref="LogFile"/> if it is open.
        /// </summary>
        /// <remarks>
        /// Forces queued log entries to be flushed to the <see cref="LogFile"/>.
        /// </remarks>
        public void Close()
        {
            Close(true);
        }

        /// <summary>
        /// Closes the <see cref="LogFile"/> if it is open.
        /// </summary>
        /// <param name="flushQueuedEntries">true, if queued log entries are to be written to the <see cref="LogFile"/>; otherwise, false.</param>
        public void Close(bool flushQueuedEntries)
        {
            if (IsOpen)
            {
                if (flushQueuedEntries)
                    // Writes all queued log entries to the file.
                    Flush();
                else
                    // Stops processing the queued log entries.
                    m_logEntryQueue.Stop();

                if (m_fileStream != null)
                {
                    // Closes the log file.
                    m_fileStream.Close();
                    m_fileStream = null;
                }
            }
        }

        /// <summary>
        /// Forces queued log entries to be written to the <see cref="LogFile"/>.
        /// </summary>
        public void Flush()
        {
            if (IsOpen)
                m_logEntryQueue.Flush();
        }

        /// <summary>
        /// Queues the text for writing to the <see cref="LogFile"/>.
        /// </summary>
        /// <param name="text">The text to be written to the <see cref="LogFile"/>.</param>
        public void Write(string text)
        {
            // Yield to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne();

            if (IsOpen)
                // Queue the text for writing to the log file.
                m_logEntryQueue.Add(text);
            else
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
        }

        /// <summary>
        /// Queues the text for writing to the <see cref="LogFile"/>.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        /// <remarks>
        /// In addition to the specified text, a "newline" character will be appended to the text.
        /// </remarks>
        public void WriteLine(string text)
        {
            Write(text + "\r\n");
        }

        /// <summary>
        /// Queues the text for writing to the log file.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        /// <remarks>
        /// In addition to the specified text, a timestamp will be prepended, and a "newline" character will appended to the text.
        /// </remarks>
        public void WriteTimestampedLine(string text)
        {
            Write("[" + DateTime.Now.ToString() + "] " + text + "\r\n");
        }

        /// <summary>
        /// Initializes the <see cref="LogFile"/> object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="LogFile"/> 
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
        /// Saves settings for the <see cref="LogFile"/> object to the config file if the <see cref="PersistSettings"/> 
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
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                // Add settings if they don't exist in config file.
                settings.Add("FileName", m_fileName, "Name of the log file including its path.");
                settings.Add("FileSize", m_fileSize, "Maximum size of the log file in MB.");
                settings.Add("FileFullOperation", m_fileFullOperation, "Operation (Truncate; Rollover) that is to be performed on the file when it is full.");
                // Update settings with the latest property values.
                element = settings["FileName"];
                element.Update(m_fileName, element.Description, element.Encrypted);
                element = settings["FileSize"];
                element.Update(m_fileSize, element.Description, element.Encrypted);
                element = settings["FileFullOperation"];
                element.Update(m_fileFullOperation, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="LogFile"/> object from the config file if the <see cref="PersistSettings"/> 
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
                FileName = settings["FileName", true].ValueAs(m_fileName);
                FileSize = settings["FileSize", true].ValueAs(m_fileSize);
                FileFullOperation = settings["FileFullOperation", true].ValueAs(m_fileFullOperation);
            }
        }

        /// <summary>
        /// Performs necessary operations before the <see cref="LogFile"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="LogFile"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
        }

        /// <summary>
        /// Performs necessary operations after the <see cref="LogFile"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="LogFile"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Reads and returns the text from the <see cref="LogFile"/>.
        /// </summary>
        /// <returns>The text read from the <see cref="LogFile"/>.</returns>
        public string ReadText()
        {
            // Yields to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne();

            if (IsOpen)
            {
                byte[] buffer = null;

                lock (m_fileStream)
                {
                    buffer = new byte[m_fileStream.Length];
                    m_fileStream.Seek(0, SeekOrigin.Begin);
                    m_fileStream.Read(buffer, 0, buffer.Length);
                }

                return m_textEncoding.GetString(buffer);
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Reads text from the <see cref="LogFile"/> and returns a list of lines created by seperating the text by the "newline"
        /// characters if and where present.
        /// </summary>
        /// <returns>A list of lines from the text read from the <see cref="LogFile"/>.</returns>
        public List<string> ReadLines()
        {
            return new List<string>(ReadText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
        }

        /// <summary>
        /// Raises the <see cref="FileFull"/> event.
        /// </summary>
        protected virtual void OnFileFull()
        {
            if (FileFull != null)
                FileFull(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="LogException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="LogException"/> event.</param>
        protected virtual void OnLogException(Exception ex)
        {
            if (LogException != null)
                LogException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LogFile"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

                        if (m_fileStream != null)
                            m_fileStream.Dispose();

                        if (m_logEntryQueue != null)
                        {
                            m_logEntryQueue.ProcessException -= ProcessExceptionHandler;
                            m_logEntryQueue.Dispose();
                        }

                        if (m_operationWaitHandle != null)
                            m_operationWaitHandle.Close();
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        private void WriteLogEntries(string[] items)
        {
            long currentFileSize = 0;
            long maximumFileSize = (long)(m_fileSize * 1048576);

            lock (m_fileStream)
            {
                currentFileSize = m_fileStream.Length;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (!string.IsNullOrEmpty(items[i]))
                {
                    // Write entries with text.
                    byte[] buffer = m_textEncoding.GetBytes(items[i]);

                    if (currentFileSize + buffer.Length <= maximumFileSize)
                    {
                        // Writes the entry.
                        lock (m_fileStream)
                        {
                            m_fileStream.Write(buffer, 0, buffer.Length);
                        }

                        currentFileSize += buffer.Length;
                    }
                    else
                    {
                        // Either truncates the file or rolls over to a new file because the current file is full.
                        // Prior to acting, it requeues the entries that have not been written to the file.
                        for (int j = items.Length - 1; j >= i; j--)
                        {
                            m_logEntryQueue.Insert(0, items[j]);
                        }

                        // Truncates file or roll over to new file.
                        OnFileFull();

                        return;
                    }
                }
            }
        }

        private void LogFile_FileFull(object sender, System.EventArgs e)
        {
            // Signals that the "file full operation" is in progress.
            m_operationWaitHandle.Reset();

            switch (m_fileFullOperation)
            {
                case LogFileFullOperation.Truncate:
                    // Deletes the existing log entries, and makes way from new ones.
                    try
                    {
                        Close(false);
                        File.Delete(m_fileName);
                    }
                    finally
                    {
                        Open();
                    }
                    break;
                case LogFileFullOperation.Rollover:
                    // Rolls over to a new log file, and keeps the current file for history.
                    try
                    {
                        Close(false);
                        File.Move(m_fileName, FilePath.GetDirectoryName(m_fileName) + 
                                              FilePath.GetFileNameWithoutExtension(m_fileName) + "_" + 
                                              File.GetCreationTime(m_fileName).ToString("yyyy-MM-dd hh!mm!ss") + "_to_" +
                                              File.GetLastWriteTime(m_fileName).ToString("yyyy-MM-dd hh!mm!ss") + FilePath.GetExtension(m_fileName));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Open();
                    }
                    break;
            }

            // Signals that the "file full operation" is complete.
            m_operationWaitHandle.Set();
        }

        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnLogException(e.Argument);
        }

        #endregion
    }
}