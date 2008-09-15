using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.ComponentModel;
using TVA.Parsing;
using TVA.IO.FilePath;
using TVA.Configuration;

//*******************************************************************************************************
//  TVA.IO.IsamDataFileBase.vb - Base class for Indexed Sequential Access Method files
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
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
//
//*******************************************************************************************************



namespace TVA
{
    namespace IO
    {

        public abstract partial class IsamDataFileBase<T> : IPersistSettings, ISupportInitialize where T : IBinaryDataProvider
        {


            #region " Member Declaration "

            private string m_name;
            private bool m_loadOnOpen;
            private bool m_reloadOnModify;
            private bool m_saveOnClose;
            private int m_autoSaveInterval;
            private int m_minimumRecordCount;
            private bool m_persistSettings;
            private string m_settingsCategoryName;

            private FileStream m_fileStream;
            private List<T> m_fileRecords;
            private ManualResetEvent m_loadWaitHandle;
            private ManualResetEvent m_saveWaitHandle;

            private System.Timers.Timer m_autoSaveTimer;

            #endregion

            #region " Event Declaration "

            private EventHandler FileOpeningEvent;
            public event EventHandler FileOpening
            {
                add
                {
                    FileOpeningEvent = (EventHandler)System.Delegate.Combine(FileOpeningEvent, value);
                }
                remove
                {
                    FileOpeningEvent = (EventHandler)System.Delegate.Remove(FileOpeningEvent, value);
                }
            }

            private EventHandler FileOpenedEvent;
            public event EventHandler FileOpened
            {
                add
                {
                    FileOpenedEvent = (EventHandler)System.Delegate.Combine(FileOpenedEvent, value);
                }
                remove
                {
                    FileOpenedEvent = (EventHandler)System.Delegate.Remove(FileOpenedEvent, value);
                }
            }

            private EventHandler FileClosingEvent;
            public event EventHandler FileClosing
            {
                add
                {
                    FileClosingEvent = (EventHandler)System.Delegate.Combine(FileClosingEvent, value);
                }
                remove
                {
                    FileClosingEvent = (EventHandler)System.Delegate.Remove(FileClosingEvent, value);
                }
            }

            private EventHandler FileClosedEvent;
            public event EventHandler FileClosed
            {
                add
                {
                    FileClosedEvent = (EventHandler)System.Delegate.Combine(FileClosedEvent, value);
                }
                remove
                {
                    FileClosedEvent = (EventHandler)System.Delegate.Remove(FileClosedEvent, value);
                }
            }

            private EventHandler FileModifiedEvent;
            public event EventHandler FileModified
            {
                add
                {
                    FileModifiedEvent = (EventHandler)System.Delegate.Combine(FileModifiedEvent, value);
                }
                remove
                {
                    FileModifiedEvent = (EventHandler)System.Delegate.Remove(FileModifiedEvent, value);
                }
            }

            private EventHandler DataLoadingEvent;
            public event EventHandler DataLoading
            {
                add
                {
                    DataLoadingEvent = (EventHandler)System.Delegate.Combine(DataLoadingEvent, value);
                }
                remove
                {
                    DataLoadingEvent = (EventHandler)System.Delegate.Remove(DataLoadingEvent, value);
                }
            }

            private EventHandler DataLoadedEvent;
            public event EventHandler DataLoaded
            {
                add
                {
                    DataLoadedEvent = (EventHandler)System.Delegate.Combine(DataLoadedEvent, value);
                }
                remove
                {
                    DataLoadedEvent = (EventHandler)System.Delegate.Remove(DataLoadedEvent, value);
                }
            }

            private EventHandler DataSavingEvent;
            public event EventHandler DataSaving
            {
                add
                {
                    DataSavingEvent = (EventHandler)System.Delegate.Combine(DataSavingEvent, value);
                }
                remove
                {
                    DataSavingEvent = (EventHandler)System.Delegate.Remove(DataSavingEvent, value);
                }
            }

            private EventHandler DataSavedEvent;
            public event EventHandler DataSaved
            {
                add
                {
                    DataSavedEvent = (EventHandler)System.Delegate.Combine(DataSavedEvent, value);
                }
                remove
                {
                    DataSavedEvent = (EventHandler)System.Delegate.Remove(DataSavedEvent, value);
                }
            }

            private EventHandler DataReadStartEvent;
            public event EventHandler DataReadStart
            {
                add
                {
                    DataReadStartEvent = (EventHandler)System.Delegate.Combine(DataReadStartEvent, value);
                }
                remove
                {
                    DataReadStartEvent = (EventHandler)System.Delegate.Remove(DataReadStartEvent, value);
                }
            }

            private EventHandler DataReadCompleteEvent;
            public event EventHandler DataReadComplete
            {
                add
                {
                    DataReadCompleteEvent = (EventHandler)System.Delegate.Combine(DataReadCompleteEvent, value);
                }
                remove
                {
                    DataReadCompleteEvent = (EventHandler)System.Delegate.Remove(DataReadCompleteEvent, value);
                }
            }

            public delegate void DataReadProgressEventHandler(object Of);
            private DataReadProgressEventHandler DataReadProgressEvent;

            public event DataReadProgressEventHandler DataReadProgress
            {
                add
                {
                    DataReadProgressEvent = (DataReadProgressEventHandler)System.Delegate.Combine(DataReadProgressEvent, value);
                }
                remove
                {
                    DataReadProgressEvent = (DataReadProgressEventHandler)System.Delegate.Remove(DataReadProgressEvent, value);
                }
            }

            private EventHandler DataWriteStartEvent;
            public event EventHandler DataWriteStart
            {
                add
                {
                    DataWriteStartEvent = (EventHandler)System.Delegate.Combine(DataWriteStartEvent, value);
                }
                remove
                {
                    DataWriteStartEvent = (EventHandler)System.Delegate.Remove(DataWriteStartEvent, value);
                }
            }

            private EventHandler DataWriteCompleteEvent;
            public event EventHandler DataWriteComplete
            {
                add
                {
                    DataWriteCompleteEvent = (EventHandler)System.Delegate.Combine(DataWriteCompleteEvent, value);
                }
                remove
                {
                    DataWriteCompleteEvent = (EventHandler)System.Delegate.Remove(DataWriteCompleteEvent, value);
                }
            }

            public delegate void DataWriteProgressEventHandler(object Of);
            private DataWriteProgressEventHandler DataWriteProgressEvent;

            public event DataWriteProgressEventHandler DataWriteProgress
            {
                add
                {
                    DataWriteProgressEvent = (DataWriteProgressEventHandler)System.Delegate.Combine(DataWriteProgressEvent, value);
                }
                remove
                {
                    DataWriteProgressEvent = (DataWriteProgressEventHandler)System.Delegate.Remove(DataWriteProgressEvent, value);
                }
            }


            #endregion

            #region " Code Scope: Public "

            public const string Extension = ".dat";

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
                        if (string.Compare(JustFileExtension(value), Extension, true) == 0)
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
                            throw (new ArgumentException(string.Format("Name must have an extension of {0}.", Extension)));
                        }
                    }
                    else
                    {
                        throw (new ArgumentNullException("Name"));
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
                        throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
                    }
                }
            }

            [Browsable(false)]
            public bool IsSynchronized
            {
                get
                {
                    return InMemoryRecordCount == PersistedRecordCount;
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
                        throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
                    }
                }
            }

            public void Open()
            {

                if (!IsOpen)
                {
                    if (FileOpeningEvent != null)
                        FileOpeningEvent(this, EventArgs.Empty);

                    m_name = AbsolutePath(m_name);
                    if (!Directory.Exists(JustPath(m_name)))
                    {
                        Directory.CreateDirectory(JustPath(m_name));
                    }
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

                    if (m_loadOnOpen)
                    {
                        Load();
                    }

                    // Makes sure that we have the minimum number of records specified.
                    for (int i = PersistedRecordCount + 1; i <= m_minimumRecordCount; i++)
                    {
                        Write(i, NewRecord(i));
                    }

                    if (m_reloadOnModify)
                    {
                        // Watches for any modifications made to the file.
                        FileSystemWatcher.Path = JustPath(m_name);
                        FileSystemWatcher.Filter = JustFileName(m_name);
                        FileSystemWatcher.EnableRaisingEvents = true;
                    }

                    if (m_autoSaveInterval > 0)
                    {
                        // Starts the timer for saving data automatically.
                        m_autoSaveTimer.Interval = m_autoSaveInterval;
                        m_autoSaveTimer.Start();
                    }

                    if (FileOpenedEvent != null)
                        FileOpenedEvent(this, EventArgs.Empty);
                }

            }

            public void Close()
            {

                if (IsOpen)
                {
                    if (FileClosingEvent != null)
                        FileClosingEvent(this, EventArgs.Empty);

                    // Stops the timers if they are ticking.
                    m_autoSaveTimer.Stop();

                    // Stops monitoring for changes to the file.
                    FileSystemWatcher.EnableRaisingEvents = false;

                    // Saves records back to the file if specified.
                    if (m_saveOnClose)
                    {
                        Save();
                    }

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

                    if (FileClosedEvent != null)
                        FileClosedEvent(this, EventArgs.Empty);
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
                        if (DataLoadingEvent != null)
                            DataLoadingEvent(this, EventArgs.Empty);

                        if (m_fileRecords == null)
                        {
                            m_fileRecords = new List<T>();
                        }

                        List<T> records = new List<T>(ReadFromDisk());
                        lock (m_fileRecords)
                        {
                            m_fileRecords.Clear();
                            m_fileRecords.InsertRange(0, records);
                        }

                        if (DataLoadedEvent != null)
                            DataLoadedEvent(this, EventArgs.Empty);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_loadWaitHandle.Set();
                    }
                }
                else
                {
                    throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
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
                        if (DataSavingEvent != null)
                            DataSavingEvent(this, EventArgs.Empty);

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

                        if (DataSavedEvent != null)
                            DataSavedEvent(this, EventArgs.Empty);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_saveWaitHandle.Set();
                    }
                }
                else
                {
                    throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
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
                    throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
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
                                    m_fileRecords(recordID - 1) = record;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw (new ArgumentNullException("record"));
                    }
                }
                else
                {
                    throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
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
                    throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
                }

            }

            public virtual T Read(int recordID)
            {

                if (IsOpen)
                {
                    T record = null;
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
                                record = m_fileRecords(recordID - 1);
                            }
                        }
                    }

                    return record;
                }
                else
                {
                    throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_name)));
                }

            }

            [Browsable(false)]
            public abstract int RecordSize
            {
                get;
            }

            public abstract T NewRecord(int id);

            public abstract T NewRecord(int id, byte[] binaryImage);

            #region " Interface Implementation "

            #region " IPersistSettings "

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

            public string SettingsCategoryName
            {
                get
                {
                    return m_settingsCategoryName;
                }
                set
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        m_settingsCategoryName = value;
                    }
                    else
                    {
                        throw (new ArgumentNullException("SettingsCategoryName"));
                    }
                }
            }

            public void LoadSettings()
            {

                try
                {
                    CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                    if (with_1.Count > 0)
                    {
                        Name = with_1.Item("Name").GetTypedValue(m_name);
                        LoadOnOpen = with_1.Item("LoadOnOpen").GetTypedValue(m_loadOnOpen);
                        ReloadOnModify = with_1.Item("ReloadOnModify").GetTypedValue(m_reloadOnModify);
                        SaveOnClose = with_1.Item("SaveOnClose").GetTypedValue(m_saveOnClose);
                        AutoSaveInterval = with_1.Item("AutoSaveInterval").GetTypedValue(m_autoSaveInterval);
                        MinimumRecordCount = with_1.Item("MinimumRecordCount").GetTypedValue(m_minimumRecordCount);
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
                        CategorizedSettingsElementCollection with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                        with_1.Clear();
                        object with_2 = with_1.Item("Name", true);
                        with_2.Value = m_name;
                        with_2.Description = "Name of the file including its path.";
                        object with_3 = with_1.Item("LoadOnOpen", true);
                        with_3.Value = m_loadOnOpen.ToString();
                        with_3.Description = "True if file is to be loaded when opened; otherwise False.";
                        object with_4 = with_1.Item("ReloadOnModify", true);
                        with_4.Value = m_reloadOnModify.ToString();
                        with_4.Description = "True if file is to be re-loaded when modified; otherwise False.";
                        object with_5 = with_1.Item("SaveOnClose", true);
                        with_5.Value = m_saveOnClose.ToString();
                        with_5.Description = "True if file is to be saved when closed; otherwise False.";
                        object with_6 = with_1.Item("AutoSaveInterval", true);
                        with_6.Value = m_autoSaveInterval.ToString();
                        with_6.Description = "Interval in milliseconds at which the file is to be saved automatically. A value of -1 indicates that automatic saving is disabled.";
                        object with_7 = with_1.Item("MinimumRecordCount", true);
                        with_7.Value = m_minimumRecordCount.ToString();
                        with_7.Description = "Minimum number of records that the file must have.";
                        TVA.Configuration.Common.SaveSettings();
                    }
                    catch
                    {
                        // Exceptions may occur if the settings cannot be saved to the config file.
                    }
                }

            }

            #endregion

            #region " ISupportInitialize "

            public void BeginInit()
            {

                // No prerequisites before the component is initialized.

            }

            public void EndInit()
            {

                if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    LoadSettings(); // Loads settings from the config file.
                }

            }

            #endregion

            #endregion

            #endregion

            #region " Code Scope: Private "

            private void WriteToDisk(List<T> records)
            {

                for (int i = 1; i <= records.Count; i++)
                {
                    WriteToDisk(i, records(i - 1));
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

            #region " Event Handlers "

            #region " m_autoSaveTimer "

            private void m_autoSaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {

                if (IsOpen)
                {
                    Save(); // Automatically save records to the file if the file is open.
                }

            }

            #endregion

            #region " FileSystemWatcher "

            private void FileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
            {

                if (FileModifiedEvent != null)
                    FileModifiedEvent(this, EventArgs.Empty);

                // Reload the file when it is modified externally, but only if it has been loaded once.
                if ((m_fileRecords != null) && m_reloadOnModify)
                {
                    Load();
                }

            }

            #endregion

            #endregion

            #endregion

        }

    }
}
