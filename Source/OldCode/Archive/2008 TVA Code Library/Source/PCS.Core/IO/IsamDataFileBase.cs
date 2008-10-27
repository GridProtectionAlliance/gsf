//*******************************************************************************************************
//  IsamDataFileBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/08/2007 - Pinal C. Patel
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
using System.IO;
using System.Threading;
using PCS.Configuration;
using PCS.Parsing;

namespace PCS.IO
{
    public abstract class IsamDataFileBase<T> : Component where T : IBinaryDataProvider
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the required extension of the <see cref="FileName"/>.
        /// </summary>
        public const string Extension = ".dat";

        /// <summary>
        /// Specifies the default value for the <see cref="FileName"/> property.
        /// </summary>
        public const string DefaultFileName = "IsamDataFile" + Extension;

        /// <summary>
        /// Specifies the default value for the <see cref="LoadOnOpen"/> property.
        /// </summary>
        public const bool DefaultLoadOnOpen = true;

        /// <summary>
        /// Specifies the default value for the <see cref="SaveOnClose"/> property.
        /// </summary>
        public const bool DefaultSaveOnClose = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReloadOnModify"/> property.
        /// </summary>
        public const bool DefaultReloadOnModify = true;

        /// <summary>
        /// Specifies the default value for the <see cref="AutoSaveInterval"/> property.
        /// </summary>
        public const int DefaultAutoSaveInterval = -1;

        /// <summary>
        /// Specifes the default value for the <see cref="MinimumRecordCount"/> property.
        /// </summary>
        public const int DefaultMinimumRecordCount = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "IsamDataFile";

        // Events

        /// <summary>
        /// Occurs when data is being read from disk into memory.
        /// </summary>
        [Description("Occurs when data is being read from disk into memory.")]
        public event EventHandler DataLoading;

        /// <summary>
        /// Occurs when data has been read from disk into memory.
        /// </summary>
        [Description("Occurs when data has been read from disk into memory.")]
        public event EventHandler DataLoaded;

        /// <summary>
        /// Occurs when data is being saved from memory onto disk.
        /// </summary>
        [Description("Occurs when data is being saved from memory onto disk.")]
        public event EventHandler DataSaving;

        /// <summary>
        /// Occurs when data has been saved from memory onto disk.
        /// </summary>
        [Description("Occurs when data has been saved from memory onto disk.")]
        public event EventHandler DataSaved;

        /// <summary>
        /// Occurs when file data on the disk is modified.
        /// </summary>
        [Description("Occurs when file data on the disk is modified.")]
        public event EventHandler FileModified;

        // Fields
        private string m_fileName;
        private bool m_loadOnOpen;
        private bool m_saveOnClose;
        private bool m_reloadOnModify;
        private int m_autoSaveInterval;
        private int m_minimumRecordCount;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private FileStream m_fileStream;
        private List<T> m_fileRecords;
        private ManualResetEvent m_loadWaitHandle;
        private ManualResetEvent m_saveWaitHandle;
        private System.Timers.Timer m_autoSaveTimer;
        private FileSystemWatcher m_fileSystemWatcher;

        #endregion

        #region [ Constructors ]

        public IsamDataFileBase()
        {
            m_fileName = this.GetType().Name + Extension;
            m_minimumRecordCount = 0;
            m_loadOnOpen = true;
            m_reloadOnModify = true;
            m_autoSaveInterval = -1;
            m_settingsCategory = this.GetType().Name;

            m_autoSaveTimer = new System.Timers.Timer();
            m_autoSaveTimer.Elapsed += m_autoSaveTimer_Elapsed;

            m_loadWaitHandle = new ManualResetEvent(true);
            m_saveWaitHandle = new ManualResetEvent(true);
            m_fileSystemWatcher = new FileSystemWatcher();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the ISAM file.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="FileName"/> when the file is open will cause the file to be re-opend.
        /// </remarks>
        [Category("Settings"), 
        DefaultValue(DefaultFileName),
        Description("Name of the ISAM file.")]
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

                if (string.Compare(Path.GetExtension(value), Extension, true) != 0)
                    throw new ArgumentException(string.Format("Name must have an extension of {0}.", Extension));

                m_fileName = value;
                if (IsOpen)
                {
                    Close();
                    Open();
                }
            }
        }

        [Category("Settings")]
        public int AutoSaveInterval
        {
            get
            {
                return m_autoSaveInterval;
            }
            set
            {
                m_autoSaveInterval = value;
            }
        }

        [Category("Settings")]
        public int MinimumRecordCount
        {
            get
            {
                return m_minimumRecordCount;
            }
            set
            {
                m_minimumRecordCount = value;
            }
        }

        [Category("Behavior")]
        public bool LoadOnOpen
        {
            get
            {
                return m_loadOnOpen;
            }
            set
            {
                m_loadOnOpen = value;
            }
        }

        [Category("Behavior")]
        public bool ReloadOnModify
        {
            get
            {
                return m_reloadOnModify;
            }
            set
            {
                m_reloadOnModify = value;
            }
        }

        [Category("Behavior")]
        public bool SaveOnClose
        {
            get
            {
                return m_saveOnClose;
            }
            set
            {
                m_saveOnClose = value;
            }
        }

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

        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_settingsCategory = value;
                else
                    throw new ArgumentNullException("SettingsCategoryName");
            }
        }

        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return (m_fileStream != null);
            }
        }

        [Browsable(false)]
        public bool IsCorrupt
        {
            get
            {
                if (IsOpen)
                {
                    long fileLength;

                    lock (m_fileStream)
                    {
                        fileLength = m_fileStream.Length;
                    }

                    return (fileLength % RecordSize != 0);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
                }
            }
        }

        [Browsable(false)]
        public bool IsSynchronized
        {
            get
            {
                return (RecordsInMemory == RecordsOnDisk);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>In KB.</remarks>
        [Browsable(false)]
        public long MemoryUsage
        {
            get
            {
                return RecordsInMemory * RecordSize / 1024;
            }
        }

        [Browsable(false)]
        public int RecordsOnDisk
        {
            get
            {
                if (IsOpen)
                {
                    long fileLength;

                    lock (m_fileStream)
                    {
                        fileLength = m_fileStream.Length;
                    }

                    return Convert.ToInt32(fileLength / RecordSize);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
                }
            }
        }

        [Browsable(false)]
        public int RecordsInMemory
        {
            get
            {
                int recordCount = 0;

                if (m_fileRecords != null)
                {
                    lock (m_fileRecords)
                    {
                        recordCount = m_fileRecords.Count;
                    }
                }

                return recordCount;
            }
        }

        [Browsable(false)]
        public abstract int RecordSize
        {
            get;
        }

        #endregion

        #region [ Methods ]

        public void Open()
        {
            if (!IsOpen)
            {
                m_fileName = FilePath.GetAbsolutePath(m_fileName);

                if (!Directory.Exists(Path.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(m_fileName));

                if (File.Exists(m_fileName))
                {
                    // Opens existing file.
                    m_fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                {
                    // Creates file.
                    m_fileStream = new FileStream(m_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }

                if (m_loadOnOpen) Load();

                // Makes sure that we have the minimum number of records specified.
                for (int i = RecordsOnDisk + 1; i <= m_minimumRecordCount; i++)
                {
                    Write(i, NewRecord(i));
                }

                if (m_reloadOnModify)
                {
                    // Watches for any modifications made to the file.
                    m_fileSystemWatcher.Path = Path.GetDirectoryName(m_fileName);
                    m_fileSystemWatcher.Filter = Path.GetFileName(m_fileName);
                    m_fileSystemWatcher.EnableRaisingEvents = true;
                }

                if (m_autoSaveInterval > 0)
                {
                    // Starts the timer for saving data automatically.
                    m_autoSaveTimer.Interval = m_autoSaveInterval;
                    m_autoSaveTimer.Start();
                }
            }
        }

        public void Close()
        {
            if (IsOpen)
            {
                // Stops the timers if they are ticking.
                m_autoSaveTimer.Stop();

                // Stops monitoring for changes to the file.
                m_fileSystemWatcher.EnableRaisingEvents = false;

                // Saves records back to the file if specified.
                if (m_saveOnClose) Save();

                // Releases all of the used resources.
                if (m_fileStream != null)
                {
                    lock (m_fileStream)
                    {
                        m_fileStream.Dispose();
                    }
                }

                m_fileStream = null;

                if (m_fileRecords != null)
                {
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                    }
                }

                m_fileRecords = null;
            }
        }

        public void Load()
        {
            if (IsOpen)
            {
                // Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne();

                // Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne();
                m_loadWaitHandle.Reset();

                try
                {
                    OnDataLoading(EventArgs.Empty);

                    if (m_fileRecords == null)
                        m_fileRecords = new List<T>();

                    List<T> records = new List<T>(ReadFromDisk());
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }

                    OnDataLoaded(EventArgs.Empty);
                }
                finally
                {
                    m_loadWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        public void Save()
        {
            if (IsOpen)
            {
                // Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne();

                // Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne();
                m_saveWaitHandle.Reset();

                try
                {
                    OnDataSaving(EventArgs.Empty);

                    // Saves (persists) records to the file, if present in memory.
                    if (m_fileRecords != null)
                    {
                        lock (m_fileRecords)
                        {
                            WriteToDisk(m_fileRecords);
                        }

                        if (RecordsInMemory < RecordsOnDisk)
                        {
                            lock (m_fileStream)
                            {
                                m_fileStream.SetLength(RecordsInMemory * RecordSize);
                            }
                        }
                    }

                    OnDataSaved(EventArgs.Empty);
                }
                finally
                {
                    m_saveWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        public void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];

                if (settings.Count > 0)
                {
                    FileName = settings["Name"].ValueAs(m_fileName);
                    LoadOnOpen = settings["LoadOnOpen"].ValueAs(m_loadOnOpen);
                    ReloadOnModify = settings["ReloadOnModify"].ValueAs(m_reloadOnModify);
                    SaveOnClose = settings["SaveOnClose"].ValueAs(m_saveOnClose);
                    AutoSaveInterval = settings["AutoSaveInterval"].ValueAs(m_autoSaveInterval);
                    MinimumRecordCount = settings["MinimumRecordCount"].ValueAs(m_minimumRecordCount);
                }
            }
            catch
            {
                // Exceptions will occur if the settings are not present in the config file.
            }
        }

        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];
                    CategorizedSettingsElement setting;

                    settings.Clear();

                    setting = settings["Name", true];
                    setting.Value = m_fileName;
                    setting.Description = "Name of the file including its path.";

                    setting = settings["LoadOnOpen", true];
                    setting.Value = m_loadOnOpen.ToString();
                    setting.Description = "True if file is to be loaded when opened; otherwise False.";

                    setting = settings["ReloadOnModify", true];
                    setting.Value = m_reloadOnModify.ToString();
                    setting.Description = "True if file is to be re-loaded when modified; otherwise False.";

                    setting = settings["SaveOnClose", true];
                    setting.Value = m_saveOnClose.ToString();
                    setting.Description = "True if file is to be saved when closed; otherwise False.";

                    setting = settings["AutoSaveInterval", true];
                    setting.Value = m_autoSaveInterval.ToString();
                    setting.Description = "Interval in milliseconds at which the file is to be saved automatically. A value of -1 indicates that automatic saving is disabled.";

                    setting = settings["MinimumRecordCount", true];
                    setting.Value = m_minimumRecordCount.ToString();
                    setting.Description = "Minimum number of records that the file must have.";

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
                LoadSettings(); // Loads settings from the config file.
        }

        public virtual void Write(List<T> records)
        {
            if (IsOpen)
            {
                if (m_fileRecords == null)
                {
                    WriteToDisk(records);
                }
                else
                {
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        public virtual void Write(int recordID, T record)
        {
            if (IsOpen)
            {
                if (record != null)
                {
                    if (m_fileRecords == null)
                    {
                        // We're writing directly to the file.
                        WriteToDisk(recordID, record);
                    }
                    else
                    {
                        // We're updating the in-memory record list.
                        int lastRecordID = RecordsInMemory;

                        if (recordID > lastRecordID)
                        {
                            if (recordID > lastRecordID + 1)
                            {
                                for (int i = lastRecordID + 1; i <= recordID - 1; i++)
                                {
                                    Write(i, NewRecord(i));
                                }
                            }

                            lock (m_fileRecords)
                            {
                                m_fileRecords.Add(record);
                            }
                        }
                        else
                        {
                            // Updates the existing record with the new one.
                            lock (m_fileRecords)
                            {
                                m_fileRecords[recordID - 1] = record;
                            }
                        }
                    }
                }
                else
                {
                    throw new ArgumentNullException("record");
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        public virtual List<T> Read()
        {
            if (IsOpen)
            {
                List<T> records = new List<T>();

                if (m_fileRecords == null)
                {
                    // Reads persisted records if no records are in memory.
                    records.InsertRange(0, ReadFromDisk());
                }
                else
                {
                    // Reads records in memory.
                    lock (m_fileRecords)
                    {
                        records.InsertRange(0, m_fileRecords);
                    }
                }

                return records;
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        public virtual T Read(int recordID)
        {
            if (IsOpen)
            {
                T record = default(T);

                if (recordID > 0)
                {
                    // ID of the requested record is valid.
                    if (m_fileRecords == null && recordID <= RecordsOnDisk)
                    {
                        // Reads the requested record exists in the file.
                        record = ReadFromDisk(recordID);
                    }
                    else if ((m_fileRecords != null) && recordID <= RecordsInMemory)
                    {
                        // Uses the requested record from memory.
                        lock (m_fileRecords)
                        {
                            record = m_fileRecords[recordID - 1];
                        }
                    }
                }

                return record;
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        public abstract T NewRecord(int id);

        public abstract T NewRecord(int id, byte[] binaryImage);

        protected virtual void OnFileModified(EventArgs e)
        {
            if (FileModified != null)
                FileModified(this, e);
        }

        protected virtual void OnDataLoading(EventArgs e)
        {
            if (DataLoading != null)
                DataLoading(this, e);
        }

        protected virtual void OnDataLoaded(EventArgs e)
        {
            if (DataLoaded != null)
                DataLoaded(this, e);
        }

        protected virtual void OnDataSaving(EventArgs e)
        {
            if (DataSaving != null)
                DataSaving(this, e);
        }

        protected virtual void OnDataSaved(EventArgs e)
        {
            if (DataSaved != null)
                DataSaved(this, e);
        }

        //protected override void Dispose(bool disposing)
        //{
        //    try
        //    {
        //        if (disposing)
        //        {
        //            Close();        // Closes the file.
        //            SaveSettings(); // Saves settings to the config file.

        //            if (FileSystemWatcher != null)
        //            {
        //                FileSystemWatcher.Changed -= FileSystemWatcher_Changed;
        //                FileSystemWatcher.Dispose();
        //            }
        //            FileSystemWatcher = null;

        //            if (m_loadWaitHandle != null) m_loadWaitHandle.Close();
        //            m_loadWaitHandle = null;

        //            if (m_saveWaitHandle != null) m_saveWaitHandle.Close();
        //            m_saveWaitHandle = null;

        //            if (m_autoSaveTimer != null) m_autoSaveTimer.Dispose();
        //            m_autoSaveTimer = null;

        //            if (components != null)
        //                components.Dispose();
        //        }
        //    }
        //    finally
        //    {
        //        base.Dispose(disposing);
        //    }
        //}

        private void WriteToDisk(List<T> records)
        {
            for (int i = 1; i <= records.Count; i++)
            {
                WriteToDisk(i, records[i - 1]);
            }
        }

        private void WriteToDisk(int recordID, T record)
        {
            lock (m_fileStream)
            {
                m_fileStream.Seek((recordID - 1) * record.BinaryLength, SeekOrigin.Begin);
                m_fileStream.Write(record.BinaryImage, 0, record.BinaryLength);
                m_fileStream.Flush();
            }
        }

        private List<T> ReadFromDisk()
        {
            List<T> records = new List<T>();
            int recordCount = RecordsOnDisk;

            for (int i = 1; i <= recordCount; i++)
            {
                records.Add(ReadFromDisk(i));
            }

            return records;
        }

        private T ReadFromDisk(int recordID)
        {
            byte[] binaryImage = new byte[RecordSize];

            lock (m_fileStream)
            {
                m_fileStream.Seek((recordID - 1) * RecordSize, SeekOrigin.Begin);
                m_fileStream.Read(binaryImage, 0, binaryImage.Length);
            }

            return NewRecord(recordID, binaryImage);
        }

        private void m_autoSaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Automatically save records to the file if the file is open.
            if (IsOpen) Save();
        }

        private void FileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            OnFileModified(EventArgs.Empty);

            // Reload the file when it is modified externally, but only if it has been loaded once.
            if ((m_fileRecords != null) && m_reloadOnModify) Load();
        }

        #endregion
    }
}
