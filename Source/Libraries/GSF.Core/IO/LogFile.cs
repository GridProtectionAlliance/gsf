//******************************************************************************************************
//  LogFile.cs - Gbtc
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
//  04/09/2007 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/21/2008 - Pinal C. Patel
//       Edited code comments.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  07/02/2009 - Pinal C. Patel
//       Modified state alternating properties to reopen the file when changed.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  07/30/2012 - Vijay Sukhavasi
//       Added user option to control time duration of rollover. 
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************
// ReSharper disable MustUseReturnValue

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF.Collections;
using GSF.Configuration;
using GSF.Threading;
using Timer = System.Timers.Timer;

namespace GSF.IO
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
    /// using GSF.IO;
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
        /// Specifies the default value for the <see cref="LogFilesDuration"/> property.
        /// </summary>
        public const double DefaultLogFilesDuration = 0.0;

        /// <summary>
        /// Specifies the default value for the <see cref="FlushTimerInterval"/> property.
        /// </summary>
        public const double DefaultFlushTimerInterval = 10.0D;

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
        private FileStream m_fileStream;
        private readonly object m_fileStreamLock;
        private ManualResetEvent m_operationWaitHandle;
        private ProcessQueue<string> m_logEntryQueue;
        private Timer m_flushTimer;
        private Encoding m_textEncoding;
        private bool m_initialized;
        private double m_flushTimerInterval;
        private readonly Dictionary<DateTime, string> m_savedFilesWithTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        public LogFile()
        {
            m_fileName = DefaultFileName;
            m_fileSize = DefaultFileSize;
            FileFullOperation = DefaultFileFullOperation;
            LogFilesDuration = DefaultLogFilesDuration;
            PersistSettings = DefaultPersistSettings;
            Name = DefaultSettingsCategory;
            m_textEncoding = Encoding.Default;
            m_operationWaitHandle = new ManualResetEvent(true);
            m_savedFilesWithTime = new Dictionary<DateTime, string>();
            m_logEntryQueue = ProcessQueue<string>.CreateRealTimeQueue(WriteLogEntries);
            m_logEntryQueue.SynchronizedOperationType = OptimizationOptions.PreferDedicatedThreads ? SynchronizedOperationType.DedicatedForeground : SynchronizedOperationType.Long;
            m_flushTimer = new Timer();
            m_flushTimerInterval = 10.0D;
            m_fileStreamLock = new object();

            FileFull += LogFile_FileFull;
            m_logEntryQueue.ProcessException += ProcessExceptionHandler;
            m_flushTimer.Elapsed += FlushTimer_Elapsed;
            m_flushTimer.AutoReset = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="LogFile"/>.</param>
        public LogFile(IContainer container)
            : this()
        {
            container?.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="LogFile"/>, including the file extension.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Settings")]
        [DefaultValue(DefaultFileName)]
        [Description("Name of the LogFile, including the file extension.")]
        public string FileName
        {
            get => m_fileName;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_fileName = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the size of the <see cref="LogFile"/> in MB.
        /// </summary>
        ///<exception cref="ArgumentOutOfRangeException">The value being assigned outside the <see cref="MinFileSize"/> and <see cref="MaxFileSize"/> range.</exception>
        [Category("Settings")]
        [DefaultValue(DefaultFileSize)]
        [Description("Size of the LogFile in MB.")]
        public int FileSize
        {
            get => m_fileSize;
            set
            {
                if (value is < MinFileSize or > MaxFileSize)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Value must be between {MinFileSize} and {MaxFileSize}");

                m_fileSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of operation to be performed when the <see cref="LogFile"/> is full.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(DefaultFileFullOperation)]
        [Description("Type of operation to be performed when the LogFile is full.")]
        public LogFileFullOperation FileFullOperation { get; set; }

        /// <summary>
        /// Gets or sets the time duration, in hours, to save the <see cref="LogFile"/> .
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(DefaultLogFilesDuration)]
        [Description("Time duration in hours to save the log files.")]
        public double LogFilesDuration { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds of inactivity before the <see cref="LogFile"/> automatically flushes the file stream.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(DefaultFlushTimerInterval)]
        [Description("Number of seconds of inactivity before the log file automatically flushes the file stream.")]
        public double FlushTimerInterval
        {
            get => m_flushTimerInterval;
            set
            {
                m_flushTimerInterval = value;

                if (m_flushTimerInterval > 0.0D)
                    m_flushTimer.Interval = m_flushTimerInterval * 1000.0D;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="LogFile"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistence")]
        [DefaultValue(DefaultPersistSettings)]
        [Description("Indicates whether the settings of LogFile object are to be saved to the config file.")]
        public bool PersistSettings { get; set; }

        /// <summary>
        /// Gets or sets the category under which the settings of <see cref="LogFile"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Persistence")]
        [DefaultValue(DefaultSettingsCategory)]
        [Description("Category under which the settings of LogFile object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get => Name;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used to encode the messages being logged.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get => m_textEncoding;
            set => m_textEncoding = value ?? Encoding.Default;
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="LogFile"/> object is currently enabled.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get => IsOpen;
            set
            {
                if (value && !Enabled)
                    Open();
                else if (!value && Enabled)
                    Close();
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="LogFile"/> is open.
        /// </summary>
        [Browsable(false)]
        public bool IsOpen => m_fileStream is not null;

        /// <summary>
        /// Gets the unique identifier of the <see cref="LogFile"/> object.
        /// </summary>
        [Browsable(false)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the descriptive status of the <see cref="LogFile"/> object.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new();

                status.Append("     Configuration section: ");
                status.Append(Name);
                status.AppendLine();
                status.Append("       Maximum export size: ");
                status.Append(m_fileSize.ToString());
                status.Append(" MB");
                status.AppendLine();
                status.Append("       File full operation: ");
                status.Append(FileFullOperation);
                status.AppendLine();

                if (m_logEntryQueue is not null)
                    status.Append(m_logEntryQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="LogFile"/> object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="LogFile"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (m_initialized)
                return;

            LoadSettings();         // Load settings from the config file.
            m_initialized = true;   // Initialize only once.
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
            //if (DesignMode)
            //    return;

            //try
            //{
            //    // Nothing needs to be done before component is initialized.
            //}
            //catch (Exception)
            //{
            //    // Prevent the IDE from crashing when component is in design mode.
            //}
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
            if (DesignMode)
                return;

            try
            {
                Initialize();
            }
            catch (Exception)
            {
                // Prevent the IDE from crashing when component is in design mode.
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="LogFile"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (!PersistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(Name))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[Name];
            
            settings["FileName", true].Update(m_fileName);
            settings["FileSize", true].Update(m_fileSize);
            settings["FileFullOperation", true].Update(FileFullOperation);
            settings["LogFilesDuration", true].Update(LogFilesDuration);
            settings["FlushTimerInterval", true].Update(m_flushTimerInterval);
            
            config.Save();
        }

        /// <summary>
        /// Loads saved settings for the <see cref="LogFile"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (!PersistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(Name))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[Name];
            
            settings.Add("FileName", m_fileName, "Name of the log file including its path.");
            settings.Add("FileSize", m_fileSize, "Maximum size of the log file in MB.");
            settings.Add("FileFullOperation", FileFullOperation, "Operation (Truncate; Rollover) that is to be performed on the file when it is full.");
            settings.Add("LogFilesDuration", LogFilesDuration, "Time duration in hours to save the log files,files older than this duration are purged automatically");
            settings.Add("FlushTimerInterval", m_flushTimerInterval, "Number of seconds of inactivity before the log file automatically flushes the file stream.");
            
            FileName = settings["FileName"].ValueAs(m_fileName);
            FileSize = settings["FileSize"].ValueAs(m_fileSize);
            FileFullOperation = settings["FileFullOperation"].ValueAs(FileFullOperation);
            LogFilesDuration = settings["LogFilesDuration"].ValueAs(LogFilesDuration);
            FlushTimerInterval = settings["FlushTimerInterval"].ValueAs(m_flushTimerInterval);
        }

        /// <summary>
        /// Opens the <see cref="LogFile"/> for use if it is closed.
        /// </summary>
        public void Open()
        {
            if (IsOpen)
                return;

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
                new FileInfo(m_fileName).CreationTime = DateTime.Now;

            // Start the queue to which log entries are going to be added.
            m_logEntryQueue.Start();
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
            if (!IsOpen)
                return;
            
            if (flushQueuedEntries)
                Flush();                // Write all queued log entries to the file
            else
                m_logEntryQueue.Stop(); // Stop processing the queued log entries

            if (m_fileStream is null)
                return;
            
            // Closes the log file.
            m_fileStream.Close();
            m_fileStream = null;
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
                m_logEntryQueue.Add(text);  // Queue the text for writing to the log file.
            else
                throw new InvalidOperationException($"{GetType().Name} \"{m_fileName}\" is not open");
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
            Write(text + Environment.NewLine);
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
            WriteLine($"[{DateTime.Now}] {text}");
        }

        /// <summary>
        /// Reads and returns the text from the <see cref="LogFile"/>.
        /// </summary>
        /// <returns>The text read from the <see cref="LogFile"/>.</returns>
        public string ReadText()
        {
            // Yields to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne();

            if (!IsOpen)
                throw new InvalidOperationException($"{GetType().Name} \"{m_fileName}\" is not open");

            byte[] buffer;

            lock (m_fileStreamLock)
            {
                buffer = new byte[m_fileStream.Length];
                m_fileStream.Seek(0, SeekOrigin.Begin);
                m_fileStream.Read(buffer, 0, buffer.Length);
            }

            return m_textEncoding.GetString(buffer);

        }

        /// <summary>
        /// Reads text from the <see cref="LogFile"/> and returns a list of lines created by separating the text by the "newline"
        /// characters if and where present.
        /// </summary>
        /// <param name="removeEmptyLines"><c>true</c>, if empty lines are to be removed from the list; otherwise, <c>false</c>>.</param>
        /// <returns>A list of lines from the text read from the <see cref="LogFile"/>.</returns>
        public List<string> ReadLines(bool removeEmptyLines = false)
        {
            return new List<string>(ReadText().Split(new[] { Environment.NewLine }, removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None));
        }

        /// <summary>
        /// Raises the <see cref="FileFull"/> event.
        /// </summary>
        protected virtual void OnFileFull()
        {
            FileFull?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="LogException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="LogException"/> event.</param>
        protected virtual void OnLogException(Exception ex)
        {
            LogException?.Invoke(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LogFile"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            
            try
            {
                if (!disposing)
                    return;

                SaveSettings();

                FileFull -= LogFile_FileFull;

                if (m_operationWaitHandle is not null)
                {
                    m_operationWaitHandle.Close();
                    m_operationWaitHandle = null;
                }

                if (m_flushTimer is not null)
                {
                    m_flushTimer.Elapsed -= FlushTimer_Elapsed;
                    m_flushTimer.Dispose();
                    m_flushTimer = null;
                }

                if (m_fileStream is not null)
                {
                    m_fileStream.Dispose();
                    m_fileStream = null;
                }

                if (m_logEntryQueue is not null)
                {
                    m_logEntryQueue.ProcessException -= ProcessExceptionHandler;
                    m_logEntryQueue.Dispose();
                    m_logEntryQueue = null;
                }
            }
            finally
            {
                IsDisposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Re-opens the file if currently open.
        /// </summary>
        private void ReOpen()
        {
            if (!IsOpen)
                return;

            Close();
            Open();
        }

        private void WriteLogEntries(string[] items)
        {
            long currentFileSize;
            long maximumFileSize = m_fileSize * 1048576L;

            m_flushTimer?.Stop();

            lock (m_fileStreamLock)
            {
                currentFileSize = m_fileStream.Length;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (string.IsNullOrEmpty(items[i]))
                    continue;

                // Write entries with text.
                byte[] buffer = m_textEncoding.GetBytes(items[i]);

                if (currentFileSize + buffer.Length <= maximumFileSize)
                {
                    // Writes the entry.
                    lock (m_fileStreamLock)
                    {
                        m_fileStream.Write(buffer, 0, buffer.Length);
                    }

                    currentFileSize += buffer.Length;
                }
                else
                {
                    // Either truncates the file or rolls over to a new file because the current file is full.
                    // Prior to acting, it re-queues the entries that have not been written to the file.
                    for (int j = items.Length - 1; j >= i; j--)
                    {
                        m_logEntryQueue.Insert(0, items[j]);
                    }

                    // Truncates file or roll over to new file.
                    OnFileFull();

                    return;
                }
            }

            if (m_flushTimerInterval > 0.0D && m_flushTimer is not null)
                m_flushTimer.Start();
        }

        private void FlushTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                m_fileStream?.Flush();
            }
            catch (ObjectDisposedException)
            {
                // If the file stream has been disposed,
                // then we know it's already been flushed
                // so we just go on with our lives
            }
        }

        private void LogFile_FileFull(object sender, EventArgs e)
        {
            try
            {
                // Signals that the "file full operation" is in progress.
                m_operationWaitHandle.Reset();

                switch (FileFullOperation)
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
                            string directoryName = FilePath.GetDirectoryName(m_fileName);
                            string rootFileName = FilePath.GetFileNameWithoutExtension(m_fileName);

                            // We want this to run only once, when system starts.
                            if (m_savedFilesWithTime.Count < 1)
                            {
                                string expression = $"{rootFileName}*";
                                string[] files = Directory.GetFiles(directoryName, expression);

                                foreach (string preStartLogFile in files)
                                {
                                    if (!m_savedFilesWithTime.ContainsKey(File.GetLastWriteTime(preStartLogFile)))
                                        m_savedFilesWithTime.Add(File.GetLastWriteTime(preStartLogFile), preStartLogFile);
                                }
                            }

                            Close(false);

                            DateTime creationTime = File.GetCreationTime(m_fileName);
                            DateTime lastWriteTime = File.GetLastWriteTime(m_fileName);

                            string destinationFile;
                            bool fileExists = false;

                            // In cases where logs are filling very fast you may encounter situations
                            // where the log file names could overlap. In order to help with this
                            // case dated file names include milliseconds and verify file uniqueness
                            do
                            {
                                // Keep adding milliseconds to last write time until file name is unique
                                if (fileExists)
                                    lastWriteTime = lastWriteTime.AddMilliseconds(1.0D);

                                destinationFile = Path.Combine(directoryName, rootFileName) + "_" +
                                    creationTime.ToString("yyyy-MM-dd HH!mm!ss!fff") + "_to_" +
                                    lastWriteTime.ToString("yyyy-MM-dd HH!mm!ss!fff") + FilePath.GetExtension(m_fileName);

                                fileExists = File.Exists(destinationFile);
                            }
                            while (fileExists);

                            File.Move(m_fileName, destinationFile);

                            m_savedFilesWithTime.Add(lastWriteTime, destinationFile);

                            List<DateTime> savedTimes = m_savedFilesWithTime.Keys.ToList();
                            savedTimes.Sort();

                            // Save at least one file even if duration is 0 or negative.
                            for (int i = 0; i < savedTimes.Count - 1; i++)
                            {
                                // True only when time difference is reached.
                                if (DateTime.Compare(DateTime.Now.AddHours(-LogFilesDuration), savedTimes[i]) <= 0)
                                    continue;

                                File.Delete(m_savedFilesWithTime[savedTimes[i]]);
                                m_savedFilesWithTime.Remove(savedTimes[i]);
                            }
                        }
                        finally
                        {
                            Open();
                        }
                        break;
                }
            }
            finally
            {
                // Signals that the "file full operation" is complete.
                m_operationWaitHandle.Set();
            }
        }

        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnLogException(e.Argument);
        }

        #endregion
    }
}