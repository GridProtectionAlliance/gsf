//******************************************************************************************************
//  MetadataFile.cs - Gbtc
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
//  03/08/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  09/03/2009 - Pinal C. Patel
//       Added Read() overload that takes string as its parameter for performing flexible reads.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/16/2009 - Pinal C. Patel
//       Changed the default value for SettingsCategory property to the type name.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using GSF.Configuration;
using GSF.IO;

namespace GSF.Historian.Files
{
    /// <summary>
    /// Defines legacy operational modes for the <see cref="MetadataFile"/>.
    /// </summary>
    public enum MetadataFileLegacyMode
    {
        /// <summary>
        /// Only use new "..._dbase.dat2" format.
        /// </summary>
        Disabled,
        /// <summary>
        /// Use new "..._dbase.dat2" format and also maintain a legacy "..._dbase.dat" file.
        /// </summary>
        Compatible,
        /// <summary>
        /// Only use legacy "..._dbase.dat" file.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Represents a file containing <see cref="MetadataRecord"/>s.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    [ToolboxBitmap(typeof(MetadataFile))]
    public class MetadataFile : IsamDataFileBase<MetadataRecord>
    {
        #region [ Members ]

        // Nested Types
        private class LegacyMetadataFile : IsamDataFileBase<MetadataRecord>
        {
            #region [ Constructors ]

            /// <summary>
            /// Creates a new <see cref="LegacyMetadataFile"/>.
            /// </summary>
            /// <param name="fileName">Legacy file name.</param>
            public LegacyMetadataFile(string fileName)
            {
                FileName = fileName;
                FileAccessMode = FileAccess.Write;
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Gets the binary size of a <see cref="MetadataRecord"/>.
            /// </summary>
            /// <returns>A 32-bit signed integer.</returns>
            protected override int GetRecordSize()
            {
                return MetadataRecord.FixedLength;
            }

            /// <summary>
            /// Creates a new <see cref="MetadataRecord"/> with the specified <paramref name="recordIndex"/>.
            /// </summary>
            /// <param name="recordIndex">1-based index of the <see cref="MetadataRecord"/>.</param>
            /// <returns>A <see cref="MetadataRecord"/> object.</returns>
            protected override MetadataRecord CreateNewRecord(int recordIndex)
            {
                return new MetadataRecord(recordIndex, MetadataFileLegacyMode.Enabled);
            }

            #endregion
        }

        // Fields
        private MetadataFileLegacyMode m_legacyMode;
        private string m_baseFileName;
        private readonly Dictionary<int, MetadataRecord> m_records;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataFile"/> class.
        /// </summary>
        public MetadataFile()
        {
            SettingsCategory = this.GetType().Name;
            m_legacyMode = MetadataFileLegacyMode.Disabled;  // Default to only using new file format
            m_records = new Dictionary<int, MetadataRecord>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Determines legacy operation mode for the <see cref="MetadataFile"/>.
        /// </summary>
        public MetadataFileLegacyMode LegacyMode
        {
            get
            {
                return m_legacyMode;
            }
            set
            {
                if (m_legacyMode != value)
                {
                    m_legacyMode = value;

                    if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                        m_records.Clear();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="IsamDataFileBase{T}.FileName"/> when the file is open will cause the file to be re-opened.
        /// </remarks>
        public override string FileName
        {
            get
            {
                return base.FileName;
            }
            set
            {
                base.FileName = value;

                if ((object)value == null)
                {
                    m_baseFileName = null;
                }
                else
                {
                    if (value.EndsWith("2"))
                        m_baseFileName = value.Substring(0, value.Length - 1);
                    else
                        m_baseFileName = value;
                }
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the file data on disk is corrupt.
        /// </summary>
        public override bool IsCorrupt
        {
            get
            {
                if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                    return base.IsCorrupt;

                return false;
            }
        }

        /// <summary>
        /// Gets the number of file records on the disk.
        /// </summary>
        public override int RecordsOnDisk
        {
            get
            {
                if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                    return base.RecordsOnDisk;

                // Difficult to calculate with variable length strings, so we just return count in memory
                return RecordsInMemory;
            }
        }

        /// <summary>
        /// Gets the number of file records loaded in memory.
        /// </summary>
        public override int RecordsInMemory
        {
            get
            {
                if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                    return base.RecordsInMemory;

                return m_records.Keys.DefaultIfEmpty(0).Max();
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="MetadataFile"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendFormat("     Legacy operation mode: {0}", m_legacyMode);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves settings of the file to the config file if the <see cref="IsamDataFileBase{T}.PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="IsamDataFileBase{T}.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void SaveSettings()
        {
            if (PersistSettings)
            {
                base.SaveSettings();

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["LegacyMode", true].Update(m_legacyMode);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings of the file from the config file if the <see cref="IsamDataFileBase{T}.PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="IsamDataFileBase{T}.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void LoadSettings()
        {
            if (PersistSettings)
            {
                base.LoadSettings();

                // Load settings from the specified category.
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];
                settings.Add("LegacyMode", m_legacyMode, "Metadata file legacy format mode. Value is one of \"Disabled\", \"Compatible\" or \"Enabled\" where \"Disabled\" means only use new format, \"Compatible\" means use new format and also write a legacy format file for compatibility and \"Enabled\" means only use the legacy format.");
                LegacyMode = settings["LegacyMode"].ValueAs(m_legacyMode);

                // Define new file name for non-legacy implementations
                if (m_legacyMode != MetadataFileLegacyMode.Enabled && !FileName.EndsWith("2"))
                    FileName += "2";

                // By design of the new metadata file format, data is always loaded into memory - regardless of configuration setting
                if (m_legacyMode != MetadataFileLegacyMode.Enabled)
                    LoadOnOpen = true;
            }
        }

        /// <summary>
        /// Reads <see cref="MetadataRecord"/>s that matches the <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">Comma or semi-colon delimited list of IDs or text for which the matching <see cref="MetadataRecord"/>s are to be retrieved.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object of <see cref="MetadataRecord"/>.</returns>
        public IEnumerable<MetadataRecord> Read(string searchPattern)
        {
            int id;
            foreach (string searchPart in searchPattern.Split(',', ';'))
            {
                // Iterate through all parts.
                if (int.TryParse(searchPattern, out id))
                {
                    // Exact id is specified.
                    yield return Read(id);
                }
                else
                {
                    // Text is specified, so search for matches.
                    foreach (MetadataRecord record in Read())
                    {
                        if (record.Name.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            record.Synonym1.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            record.Synonym2.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            record.Synonym3.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            record.Description.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            record.Remarks.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            record.HardwareInfo.IndexOf(searchPart, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            yield return record;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        public override void Open()
        {
            // Handle special cases for readers
            if (!PersistSettings || FileAccessMode == FileAccess.Read)
            {
                // Need to fall back on using old file format if a new one doesn't exist for historical reads
                if (!File.Exists(FileName) && !string.IsNullOrEmpty(m_baseFileName) && File.Exists(m_baseFileName))
                    base.FileName = m_baseFileName;

                // Use new file format if a newer one exists
                if (m_legacyMode != MetadataFileLegacyMode.Enabled && !FileName.EndsWith("2") && File.Exists(FileName + "2"))
                    FileName += "2";

                // Fall back on legacy mode if needed
                if (!FileName.EndsWith("2") && File.Exists(FileName))
                    m_legacyMode = MetadataFileLegacyMode.Enabled;
            }

            base.Open();
        }

        /// <summary>
        /// Loads records from disk into memory.
        /// </summary>
        public override void Load()
        {
            if (m_legacyMode == MetadataFileLegacyMode.Enabled)
            {
                base.Load();
                return;
            }

            if (!IsOpen)
                throw new InvalidOperationException(string.Format("MetadataFile \"{0}\" is not open", FileName));

            // Waits for any pending request to save records before completing.
            SaveWaitHandle.Wait();

            // Waits for any prior request to load records before completing.
            LoadWaitHandle.Wait();
            LoadWaitHandle.Reset();

            try
            {
                OnDataLoading();

                lock (FileDataLock)
                {
                    if (FileData.Length > 0)
                    {
                        FileData.Seek(0, SeekOrigin.Begin);
                        BinaryReader reader = new BinaryReader(FileData);
                        int count = reader.ReadInt32();

                        for (int i = 0; i < count; i++)
                        {
                            MetadataRecord record = new MetadataRecord(reader);
                            m_records[record.HistorianID] = record;
                        }
                    }
                }

                OnDataLoaded();
            }
            finally
            {
                LoadWaitHandle.Set();
            }
        }

        /// <summary>
        /// Reads file records from disk if records were not loaded in memory otherwise returns the records in memory.
        /// </summary>
        /// <returns>Records of the file.</returns>
        public override IEnumerable<MetadataRecord> Read()
        {
            if (m_legacyMode == MetadataFileLegacyMode.Enabled)
            {
                foreach (MetadataRecord record in base.Read())
                {
                    yield return record;
                }
            }

            if (!IsOpen)
                throw new InvalidOperationException(string.Format("MetadataFile \"{0}\" is not open", FileName));

            foreach (MetadataRecord record in m_records.Values)
            {
                yield return record;
            }
        }

        /// <summary>
        /// Reads specified file record from disk if records were not loaded in memory otherwise returns the record in memory.
        /// </summary>
        /// <param name="recordIndex">1-based index of the record to be read.</param>
        /// <returns>Record with the specified ID if it exists; otherwise null.</returns>
        public override MetadataRecord Read(int recordIndex)
        {
            if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                return base.Read(recordIndex);

            if (!IsOpen)
                throw new InvalidOperationException(string.Format("MetadataFile \"{0}\" is not open", FileName));

            MetadataRecord record;

            if (m_records.TryGetValue(recordIndex, out record))
                return record;

            // For compatibility with legacy mode, return a blank record if index is less than max
            if (recordIndex < m_records.Keys.Max())
                return new MetadataRecord(recordIndex, m_legacyMode);

            return null;
        }

        /// <summary>
        /// Saves records loaded in memory to disk.
        /// </summary>
        /// <remarks>
        /// <see cref="IsamDataFileBase{T}.Save"/> is equivalent to <see cref="FileStream.Flush()"/> when records are not loaded in memory.
        /// </remarks>
        public override void Save()
        {
            if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                base.Save();

            if (m_legacyMode == MetadataFileLegacyMode.Compatible)
                SaveLegacyMetadataFile();

            if (m_legacyMode == MetadataFileLegacyMode.Disabled || m_legacyMode == MetadataFileLegacyMode.Compatible)
            {
                if (!IsOpen)
                    throw new InvalidOperationException(string.Format("MetadataFile \"{0}\" is not open", FileName));

                // Waits for any pending request to save records before completing.
                SaveWaitHandle.Wait();

                // Waits for any prior request to load records before completing.
                LoadWaitHandle.Wait();
                SaveWaitHandle.Reset();

                try
                {
                    OnDataSaving();

                    lock (FileDataLock)
                    {
                        FileData.SetLength(0);
                        BinaryWriter writer = new BinaryWriter(FileData);
                        writer.Write(m_records.Count);

                        foreach (MetadataRecord record in m_records.Values)
                        {
                            record.WriteImage(writer);
                        }

                        FileData.Flush();
                        File.SetLastWriteTime(FileName, DateTime.Now);
                    }

                    OnDataSaved();
                }
                finally
                {
                    SaveWaitHandle.Set();
                }
            }
        }

        /// <summary>
        /// Writes specified records to disk if records were not loaded in memory otherwise updates the records in memory.
        /// </summary>
        /// <param name="records">Records to be written.</param>
        /// <remarks>
        /// This operation will causes existing records to be deleted and replaced with the ones specified.
        /// </remarks>
        public override void Write(IEnumerable<MetadataRecord> records)
        {
            if (m_legacyMode == MetadataFileLegacyMode.Enabled)
                base.Write(records);

            if (m_legacyMode == MetadataFileLegacyMode.Compatible)
                SaveLegacyMetadataFile(records);

            if (m_legacyMode == MetadataFileLegacyMode.Disabled || m_legacyMode == MetadataFileLegacyMode.Compatible)
            {
                if (!IsOpen)
                    throw new InvalidOperationException(string.Format("MetadataFile \"{0}\" is not open", FileName));

                m_records.Clear();

                foreach (MetadataRecord record in records)
                {
                    m_records[record.HistorianID] = record;
                }
            }
        }

        /// <summary>
        /// Writes specified record to disk if records were not loaded in memory otherwise updates the record in memory.
        /// </summary>
        /// <param name="recordIndex">1-based index of the record to be written.</param>
        /// <param name="record">Record to be written.</param>
        public override void Write(int recordIndex, MetadataRecord record)
        {
            if ((object)record == null)
                throw new ArgumentNullException(nameof(record));

            if (m_legacyMode == MetadataFileLegacyMode.Enabled || m_legacyMode == MetadataFileLegacyMode.Compatible)
                base.Write(recordIndex, record);

            if (m_legacyMode == MetadataFileLegacyMode.Disabled || m_legacyMode == MetadataFileLegacyMode.Compatible)
            {
                m_records[recordIndex] = record;
            }
        }

        private void SaveLegacyMetadataFile()
        {
            using (LegacyMetadataFile metadataFile = new LegacyMetadataFile(m_baseFileName))
            {
                foreach (KeyValuePair<int, MetadataRecord> record in m_records)
                {
                    metadataFile.Write(record.Key, record.Value);
                }

                metadataFile.Save();
            }
        }

        private void SaveLegacyMetadataFile(IEnumerable<MetadataRecord> records)
        {
            using (LegacyMetadataFile metadataFile = new LegacyMetadataFile(m_baseFileName))
            {
                metadataFile.Write(records);
                metadataFile.Save();
            }
        }

        /// <summary>
        /// Gets the binary size of a <see cref="MetadataRecord"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer.</returns>
        protected override int GetRecordSize()
        {
            return MetadataRecord.FixedLength;
        }

        /// <summary>
        /// Creates a new <see cref="MetadataRecord"/> with the specified <paramref name="recordIndex"/>.
        /// </summary>
        /// <param name="recordIndex">1-based index of the <see cref="MetadataRecord"/>.</param>
        /// <returns>A <see cref="MetadataRecord"/> object.</returns>
        protected override MetadataRecord CreateNewRecord(int recordIndex)
        {
            return new MetadataRecord(recordIndex, m_legacyMode);
        }

        #endregion
    }
}
