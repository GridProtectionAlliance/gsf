//*******************************************************************************************************
//  IsamDataFileBase.cs
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
using TVA.Configuration;
using TVA.Parsing;

namespace TVA.IO
{
    /// <summary>Base class for Indexed Sequential Access Method files.</summary>
    public abstract partial class IsamDataFileBase<T> : IPersistSettings, ISupportInitialize where T : IBinaryDataProvider
    {
        #region [ Members ]

        // Constants
        public const string Extension = ".dat";

        // Events
        public event EventHandler FileOpening;
        public event EventHandler FileOpened;
        public event EventHandler FileClosing;
        public event EventHandler FileClosed;
        public event EventHandler FileModified;
        public event EventHandler DataLoading;
        public event EventHandler DataLoaded;
        public event EventHandler DataSaving;
        public event EventHandler DataSaved;

        // TODO: These events were established but never called, either need to delete or implemement
        //public event EventHandler DataReadStart;
        //public event EventHandler DataReadComplete;
        //public event EventHandler<GenericEventArgs<ProcessProgress<int>>> DataReadProgress;
        //public event EventHandler DataWriteStart;
        //public event EventHandler DataWriteComplete;
        //public event EventHandler<GenericEventArgs<ProcessProgress<int>>> DataWriteProgress;

        // Fields
        private string m_name;
        private bool m_loadOnOpen;
        private bool m_reloadOnModify;
        private bool m_saveOnClose;
        private int m_autoSaveInterval;
        private int m_minimumRecordCount;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private FileStream m_fileStream;
        private List<T> m_fileRecords;
        private ManualResetEvent m_loadWaitHandle;
        private ManualResetEvent m_saveWaitHandle;
        private System.Timers.Timer m_autoSaveTimer;

        #endregion

        #region [ Constructors ]

        // TODO: Make a single file and move constructor initialization here...

        #endregion

        #region [ Properties ]

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
                    if (string.Compare(Path.GetExtension(value), Extension, true) == 0)
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
                        throw new ArgumentException(string.Format("Name must have an extension of {0}.", Extension));
                    }
                }
                else
                {
                    throw new ArgumentNullException("Name");
                }
            }
        }

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
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
                }
            }
        }

        [Browsable(false)]
        public bool IsSynchronized
        {
            get
            {
                return (InMemoryRecordCount == PersistedRecordCount);
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
                return InMemoryRecordCount * RecordSize / 1024;
            }
        }

        [Browsable(false)]
        public int InMemoryRecordCount
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
        public int PersistedRecordCount
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
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
                }
            }
        }

        [Browsable(false)]
        public abstract int RecordSize
        {
            get;
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

        #endregion

        #region [ Methods ]

        public void Open()
        {
            if (!IsOpen)
            {
                if (FileOpening != null)
                    FileOpening(this, EventArgs.Empty);

                m_name = FilePath.GetAbsolutePath(m_name);

                if (!Directory.Exists(Path.GetDirectoryName(m_name)))
                    Directory.CreateDirectory(Path.GetDirectoryName(m_name));

                if (File.Exists(m_name))
                {
                    // Opens existing file.
                    m_fileStream = new FileStream(m_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                {
                    // Creates file.
                    m_fileStream = new FileStream(m_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }

                if (m_loadOnOpen) Load();

                // Makes sure that we have the minimum number of records specified.
                for (int i = PersistedRecordCount + 1; i <= m_minimumRecordCount; i++)
                {
                    Write(i, NewRecord(i));
                }

                if (m_reloadOnModify)
                {
                    // Watches for any modifications made to the file.
                    FileSystemWatcher.Path = Path.GetDirectoryName(m_name);
                    FileSystemWatcher.Filter = Path.GetFileName(m_name);
                    FileSystemWatcher.EnableRaisingEvents = true;
                }

                if (m_autoSaveInterval > 0)
                {
                    // Starts the timer for saving data automatically.
                    m_autoSaveTimer.Interval = m_autoSaveInterval;
                    m_autoSaveTimer.Start();
                }

                if (FileOpened != null)
                    FileOpened(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            if (IsOpen)
            {
                if (FileClosing != null)
                    FileClosing(this, EventArgs.Empty);

                // Stops the timers if they are ticking.
                m_autoSaveTimer.Stop();

                // Stops monitoring for changes to the file.
                FileSystemWatcher.EnableRaisingEvents = false;

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

                if (FileClosed != null)
                    FileClosed(this, EventArgs.Empty);
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
                    if (DataLoading != null)
                        DataLoading(this, EventArgs.Empty);

                    if (m_fileRecords == null)
                        m_fileRecords = new List<T>();

                    List<T> records = new List<T>(ReadFromDisk());

                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }

                    if (DataLoaded != null)
                        DataLoaded(this, EventArgs.Empty);
                }
                finally
                {
                    m_loadWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
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
                    if (DataSaving != null)
                        DataSaving(this, EventArgs.Empty);

                    // Saves (persists) records to the file, if present in memory.
                    if (m_fileRecords != null)
                    {
                        lock (m_fileRecords)
                        {
                            WriteToDisk(m_fileRecords);
                        }

                        if (InMemoryRecordCount < PersistedRecordCount)
                        {
                            lock (m_fileStream)
                            {
                                m_fileStream.SetLength(InMemoryRecordCount * RecordSize);
                            }
                        }
                    }

                    if (DataSaved != null)
                        DataSaved(this, EventArgs.Empty);
                }
                finally
                {
                    m_saveWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
            }
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
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
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
                        int lastRecordID = InMemoryRecordCount;

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
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
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
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
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
                    if (m_fileRecords == null && recordID <= PersistedRecordCount)
                    {
                        // Reads the requested record exists in the file.
                        record = ReadFromDisk(recordID);
                    }
                    else if ((m_fileRecords != null) && recordID <= InMemoryRecordCount)
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
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name));
            }
        }

        public abstract T NewRecord(int id);

        public abstract T NewRecord(int id, byte[] binaryImage);

        public void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];

                if (settings.Count > 0)
                {
                    Name = settings["Name"].ValueAs(m_name);
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
                    setting.Value = m_name;
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
            int recordCount = PersistedRecordCount;

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
            if (FileModified != null)
                FileModified(this, EventArgs.Empty);

            // Reload the file when it is modified externally, but only if it has been loaded once.
            if ((m_fileRecords != null) && m_reloadOnModify) Load();
        }

        #endregion
    }
}