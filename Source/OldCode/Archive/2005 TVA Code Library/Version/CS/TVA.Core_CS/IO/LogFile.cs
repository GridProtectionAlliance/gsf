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
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using TVA.Collections;
using TVA.Configuration;

namespace TVA.IO
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the operation to be performed on the log file when it is full.
    /// </summary>
    public enum LogFileFullOperation
    {
        /// <summary>
        /// Truncates the existing entries in the log file to make space for new entries.
        /// </summary>
        Truncate,
        /// <summary>
        /// Rolls over to a new log file, and keeps the full log file for reference.
        /// </summary>
        Rollover
    }

    #endregion

    [ToolboxBitmap(typeof(LogFile))]
    public partial class LogFile : IPersistSettings, ISupportInitialize
    {       
        #region [ Members ]

        // Constants

        /// <summary>
        /// The minimum size for a log file.
        /// </summary>
        public const int MinimumFileSize = 1;

        /// <summary>
        /// The maximum size for a log file.
        /// </summary>
        public const int MaximumFileSize = 10;

        /// <summary>
        /// Default value for Name property.
        /// </summary>
        public const string DefaultName = "LogFile.txt";

        /// <summary>
        /// Default value for Size property.
        /// </summary>
        public const int DefaultSize = 3;

        /// <summary>
        /// Default value for AutoOpen property.
        /// </summary>
        public const bool DefaultAutoOpen = false;

        /// <summary>
        /// Default value for FileFullOperation property.
        /// </summary>
        public const LogFileFullOperation DefaultFileFullOperation = LogFileFullOperation.Truncate;

        /// <summary>
        /// Default value for PersistSettings property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Default value for SettingsCategoryName property.
        /// </summary>
        public const string DefaultSettingsCategoryName = "LogFile";

        // Events

        /// <summary>
        /// Occurs when the log file is being opened.
        /// </summary>
        [Description("Occurs when the log file is being opened.")]
        public event EventHandler FileOpening;

        /// <summary>
        /// Occurs when the log file has been opened.
        /// </summary>
        [Description("Occurs when the log file has been opened.")]
        public event EventHandler FileOpened;

        /// <summary>
        /// Occurs when the log file is being closed.
        /// </summary>
        [Description("Occurs when the log file is being closed.")]
        public event EventHandler FileClosing;

        /// <summary>
        /// Occurs when the log file has been closed.
        /// </summary>
        [Description("Occurs when the log file has been closed.")]
        public event EventHandler FileClosed;

        /// <summary>
        /// Occurs when the log file is full.
        /// </summary>
        [Description("Occurs when the log file is full.")]
        public event EventHandler FileFull;

        /// <summary>
        /// Occurs when an exception is encountered while writing entries to the log file.
        /// </summary>
        [Description("Occurs when an exception is encountered while writing entries to the log file.")]
        public event EventHandler<EventArgs<Exception>> LogException;

        // Fields
        private string m_name;
        private int m_size;
        private bool m_autoOpen;
        private LogFileFullOperation m_fileFullOperation;
        private bool m_persistSettings;
        private string m_settingsCategoryName;
        private FileStream m_fileStream;
        private ManualResetEvent m_operationWaitHandle;
        private ProcessQueue<string> m_logEntryQueue;
        private Encoding m_textEncoding;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        // TODO: Make a single file and move constructor initialization here...

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the log file, including the file extension.
        /// </summary>
        /// <returns>The name of the log file, including the file extension.</returns>
        [Category("Settings"), DefaultValue(DefaultName), Description("The name of the log file, including the file extension.")]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_name = value;
                    if (IsOpen)
                    {
                        Close();
                        Open();
                    }
                }
                else
                {
                    throw new ArgumentNullException("Name");
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the log file in MB.
        /// </summary>
        /// <returns>The size of the log file in MB.</returns>
        [Category("Settings"), DefaultValue(DefaultSize), Description("The size of the log file in MB.")]
        public int Size
        {
            get
            {
                return m_size;
            }
            set
            {
                if (value >= MinimumFileSize && value <= MaximumFileSize)
                    m_size = value;
                else
                    throw new ArgumentOutOfRangeException("Size", string.Format("Value must be between {0} and {1}", MinimumFileSize, MaximumFileSize));
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the log file is to be opened automatically after the
        /// component has finished initializing.
        /// </summary>
        /// <returns>True, if the log file is to be opened after the component has finished initializing; otherwise,
        /// false.</returns>
        [Category("Behavior"), DefaultValue(DefaultAutoOpen), Description("Indicates whether the log file is to be opened automatically after the component has finished initializing.")]
        public bool AutoOpen
        {
            get
            {
                return m_autoOpen;
            }
            set
            {
                m_autoOpen = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of operation to be performed when the log file is full.
        /// </summary>
        /// <returns>One of the TVA.IO.LogFileFullOperation values.</returns>
        [Category("Behavior"), DefaultValue(DefaultFileFullOperation), Description("The type of operation to be performed when the log file is full.")]
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
        /// Gets or sets a boolean value indicating whether the component settings are to be persisted to the config
        /// file.
        /// </summary>
        /// <returns>True, if the component settings are to be persisted to the config file; otherwise, false.</returns>
        [Category("Persistance"), DefaultValue(DefaultPersistSettings), Description("Indicates whether the component settings are to be persisted to the config file.")]
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
        /// Gets or sets the category name under which the component settings are to be saved in the config file.
        /// </summary>
        /// <returns>The category name under which the component settings are to be saved in the config file.</returns>
        [Category("Persistance"), DefaultValue(DefaultSettingsCategoryName), Description("The category name under which the component settings are to be saved in the config file.")]
        public string SettingsCategoryName
        {
            get
            {
                return m_settingsCategoryName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_settingsCategoryName = value;
                else
                    throw new ArgumentNullException("SettingsCategoryName");
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the log file is open.
        /// </summary>
        /// <returns>True, if the log file is open; otherwise, false.</returns>
        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return (m_fileStream != null);
            }
        }

        /// <summary>
        /// Gets or sets the encoding to be used to encode text data being exported.
        /// </summary>
        /// <value>The encoding to be used to encode text data being exported.</value>
        /// <returns>The encoding to be used to encode text data being exported.</returns>
        [Browsable(false)]
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the log file if it is closed.
        /// </summary>
        public void Open()
        {
            if (!IsOpen)
            {
                if (FileOpening != null)
                    FileOpening(this, EventArgs.Empty);

                // Gets the absolute file path if a relative path is specified.
                m_name = FilePath.AbsolutePath(m_name);
                
                // Creates the folder in which the log file will reside it, if it does not exist.
                if (!Directory.Exists(FilePath.JustPath(m_name)))
                {
                    Directory.CreateDirectory(FilePath.JustPath(m_name));
                }
                // Opens the log file (if it exists) or creates it (if it does not exist).
                m_fileStream = new FileStream(m_name, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                // Scrolls to the end of the file so that existing data is not overwritten.
                m_fileStream.Seek(0, SeekOrigin.End);

                // If this is a new log file, set its creation date to current date. This is done to prevent historic
                // log files (when FileFullOperation = Rollover) from having the same start time in their filename.
                if (m_fileStream.Length == 0)
                    (new FileInfo(m_name)).CreationTime = DateTime.Now;

                // Starts the queue to which log entries are going to be added.
                m_logEntryQueue.Start();

                if (FileOpened != null)
                    FileOpened(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Closes the log file if it is open.
        /// </summary>
        public void Close()
        {
            Close(true);
        }

        /// <summary>
        /// Closes the log file if it is open.
        /// </summary>
        /// <param name="flushQueuedEntries">True, if queued log entries are to be written to the log file; otherwise,
        /// false.</param>
        public void Close(bool flushQueuedEntries)
        {
            if (IsOpen)
            {
                if (FileClosing != null)
                    FileClosing(this, EventArgs.Empty);

                if (flushQueuedEntries)
                {
                    // Writes all queued log entries to the file.
                    m_logEntryQueue.Flush();
                }
                else
                {
                    // Stops processing the queued log entries.
                    m_logEntryQueue.Stop();
                }

                if (m_fileStream != null)
                {
                    // Closes the log file.
                    m_fileStream.Dispose();
                    m_fileStream = null;
                }

                if (FileClosed != null)
                    FileClosed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Queues the text for writing to the log file.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        public void Write(string text)
        {
            // Yields to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne();

            if (IsOpen)
            {
                // Queues the text for writing to the log file.
                m_logEntryQueue.Add(text);
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
            }
        }

        /// <summary>
        /// Queues the text for writing to the log file.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        /// <remarks>A "newline" character will automatically be appended to the text.</remarks>
        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine);
        }

        /// <summary>
        /// Queues the text for writing to the log file.
        /// </summary>
        /// <param name="text">The text to be written to the log file.</param>
        /// <remarks>
        /// <para>A timestamp will automatically be preprended to the text.</para>
        /// <para>A "newline" character will automatically be appended to the text.</para>
        /// </remarks>
        public void WriteTimestampedLine(string text)
        {
            Write("[" + DateTime.Now.ToString() + "] " + text + Environment.NewLine);
        }

        /// <summary>
        /// Loads the component settings from the config file, if present.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

                if (settings.Count > 0)
                {
                    Name = settings["Name"].ValueAs(m_name);
                    Size = settings["Size"].ValueAs(m_size);
                    AutoOpen = settings["AutoOpen"].ValueAs(m_autoOpen);
                    FileFullOperation = settings["FileFullOperation"].ValueAs(m_fileFullOperation);
                }
            }
            catch
            {
                // Exceptions will occur if the settings are not present in the config file.
            }
        }

        /// <summary>
        /// Saves the component settings to the config file.
        /// </summary>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];
                    CategorizedSettingsElement setting;

                    settings.Clear();
                    
                    setting = settings["Name", true];
                    setting.Value = m_name;
                    setting.Description = "Name of the log file including its path.";

                    setting = settings["Size", true];
                    setting.Value = m_size.ToString();
                    setting.Description = "Maximum size of the log file in MB.";

                    setting = settings["AutoOpen", true];
                    setting.Value = m_autoOpen.ToString();
                    setting.Description = "True if the log file is to be open automatically after initialization is complete; otherwise False.";

                    setting = settings["FileFullOperation", true];
                    setting.Value = m_fileFullOperation.ToString();
                    setting.Description = "Operation (Truncate; Rollover) that is to be performed on the file when it is full.";

                    ConfigurationFile.Current.Save();
                }
                catch
                {
                    // Exceptions may occur if the settings cannot be saved to the config file.
                }
            }
        }

        public void BeginInit()
        {
            // No prerequisites before the component is initialized.
        }

        public void EndInit()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                LoadSettings();         // Loads settings from the config file.
                if (m_autoOpen) Open(); // Opens the file automatically, if specified.
            }
        }

        /// <summary>
        /// Reads and returns the text from the log file.
        /// </summary>
        /// <returns>The text read from the log file.</returns>
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
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
            }
        }

        /// <summary>
        /// Reads text from the log file and returns a list of lines created by seperating the text by the "newline"
        /// characters if and where present.
        /// </summary>
        /// <returns>A list of lines from the text read from the log file.</returns>
        public List<string> ReadLines()
        {
            return new List<string>(ReadText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
        }

        private void WriteLogEntries(string[] items)
        {
            long currentFileSize = 0;
            long maximumFileSize = (long)(m_size * 1048576);

            lock (m_fileStream)
            {
                currentFileSize = m_fileStream.Length;
            }

            for (int i = 0; i <= items.Length - 1; i++)
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
                            m_fileStream.Flush();
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
                        if (FileFull != null)
                            FileFull(this, EventArgs.Empty);

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
                        File.Delete(m_name);
                    }
                    finally
                    {
                        Open();
                    }
                    break;
                case LogFileFullOperation.Rollover:
                    string historyFileName = FilePath.JustPath(m_name) + FilePath.NoFileExtension(m_name) + "_" + 
                        File.GetCreationTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") + "_to_" +
                        File.GetLastWriteTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") + FilePath.JustFileExtension(m_name);

                    // Rolls over to a new log file, and keeps the current file for history.
                    try
                    {
                        Close(false);
                        File.Move(m_name, historyFileName);
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

        private void m_logEntryQueue_ProcessException(Exception ex)
        {
            if (LogException != null)
                LogException(this, new EventArgs<Exception>(ex));
        }

        #endregion
    }
}