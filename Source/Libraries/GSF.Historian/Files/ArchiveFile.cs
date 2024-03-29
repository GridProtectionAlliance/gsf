﻿//******************************************************************************************************
//  ArchiveFile.cs - Gbtc
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
//  02/18/2007 - Pinal C. Patel
//       Generated original version of source code.
//  01/23/2008 - Pinal C. Patel
//       Added code to better utilize memory by disposing inactive data blocks.
//       Added ProcessAlarmNotification event to notify the service that alarm notifications are to be 
//       issued for the specified point.
//  03/31/2008 - Pinal C. Patel
//       Added CacheWrites and ConserveMemory properties for performance improvement.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  07/02/2009 - Pinal C. Patel
//       Modified state alternating properties to reopen the file when changed.
//  09/02/2009 - Pinal C. Patel
//       Modified code to prevent writes to dependency files when their access mode doesn't allow writes.
//  09/10/2009 - Pinal C. Patel
//       Modified ReadMetaData(), ReadStateData(), ReadMetaDataSummary() and ReadStateDataSummary() to
//       check for null references, indicating no matching record, before returning the binary image.
//  09/11/2009 - Pinal C. Patel
//       Modified code to ensure the validity of dependency files by synchronizing them.
//       Removed event handler on StateFile.FileModified event to avoid unnecessary processing.
//  09/14/2009 - Pinal C. Patel
//       Fixed NullReferenceException encountered in Statistics if accessed when file is being opened.
//       Fixed issue in MetadataFile property related to event handlers.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Implemented the IProvideStatus interface.
//  09/23/2009 - Pinal C. Patel
//       Edited code comments.
//       Removed the dependency on ArchiveDataPoint.
//  10/14/2009 - Pinal C. Patel
//       Re-coded the way current data was being written for maximum write throughput.
//       Fixed DivideByZero exception in Statistics property.
//       Fixed a issue in quality-based alarm processing.
//       Removed unused/unnecessary event raised during the write process.
//  11/06/2009 - Pinal C. Patel
//       Modified Read() and Write() methods to wait on the rollover process.
//  12/01/2009 - Pinal C. Patel
//       Removed unused RolloverOnFull property.
//       Fixed a issue in the rollover process that is encountered only when dependency files are 
//       configured to not load records in memory.
//  12/02/2009 - Pinal C. Patel
//       Modified Status property to show the total number of historic archive files.
//       Fixed a issue in the update of historic archive file list.
//  12/03/2009 - Pinal C. Patel
//       Updated Read() to incorporate changes made to ArchiveFileAllocationTable.FindDataBlocks().
//  12/08/2009 - Pinal C. Patel
//       Modified to save the FAT at the end of rollover process.
//  03/03/2010 - Pinal C. Patel
//       Added MaxHistoricArchiveFiles property to limit the number of history files to be kept.
//  03/18/2010 - Pinal C. Patel
//       Modified ReadData() to use the current ArchiveFile instance for reading data from the current
//       file instead of creating a new instance to avoid complications when rolling over to a new file.
//  04/28/2010 - Pinal C. Patel
//       Modified WriteData() overload that takes a collection of IDataPoint to not check file state.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/18/2010 - J. Ritchie Carroll
//       Added a exception handler for reading (exposed via DataReadException event) to make sure
//       bad data or corruption in an archive file does not stop the read process.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  10/03/2012 - J. Ritchie Carroll
//       Modified to support resumable read after roll-over completes.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/22/2014 - J. Ritchie Carroll
//       Fixed issue with *DELETE* setting for ArchiveOffloadLocation.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
using GSF.Parsing;
using Timer = System.Timers.Timer;

namespace GSF.Historian.Files
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the type of <see cref="ArchiveFile"/>.
    /// </summary>
    public enum ArchiveFileType
    {
        /// <summary>
        /// <see cref="ArchiveFile"/> is being used for archiving current data.
        /// </summary>
        Active,
        /// <summary>
        /// <see cref="ArchiveFile"/> to be used in the rollover process.
        /// </summary>
        Standby,
        /// <summary>
        /// <see cref="ArchiveFile"/> is full and is not to be used for current data.
        /// </summary>
        Historic
    }

    #endregion

    /// <summary>
    /// Represents a file that contains <see cref="ArchiveDataPoint"/>s.
    /// </summary>
    /// <seealso cref="ArchiveDataPoint"/>
    /// <seealso cref="ArchiveFileAllocationTable"/>
    [ToolboxBitmap(typeof(ArchiveFile))]
    public class ArchiveFile : Component, IArchive, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents information about an <see cref="ArchiveFile"/>.
        /// </summary>
        private class Info : IComparable
        {
            public Info(string fileName)
            {
                FileName = fileName;
            }

            /// <summary>
            /// Name of the <see cref="ArchiveFile"/>.
            /// </summary>
            public readonly string FileName;

            /// <summary>
            /// Start <see cref="TimeTag"/> of the <see cref="ArchiveFile"/>.
            /// </summary>
            public TimeTag StartTimeTag;

            /// <summary>
            /// End <see cref="TimeTag"/> of the <see cref="ArchiveFile"/>.
            /// </summary>
            public TimeTag EndTimeTag;

            public int CompareTo(object obj)
            {
                Info other = obj as Info;

                if ((object)other == null)
                    return 1;

                int result = StartTimeTag.CompareTo(other.StartTimeTag);

                if (result != 0)
                    return result;

                return EndTimeTag.CompareTo(other.EndTimeTag);
            }

            public override bool Equals(object obj)
            {
                Info other = obj as Info;

                if ((object)other == null)
                    return false;

                // We will only compare file name for equality because the result will be incorrect if one of
                // the ArchiveFileInfo instance is created from the filename by GetHistoricFileInfo() function.
                return string.Compare(FilePath.GetFileName(FileName), FilePath.GetFileName(other.FileName), StringComparison.OrdinalIgnoreCase) == 0;
            }

            public override int GetHashCode()
            {
                return FileName.GetHashCode();
            }
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="FileName"/> property.
        /// </summary>
        public const string DefaultFileName = "ArchiveFile" + FileExtension;

        /// <summary>
        /// Specifies the default value for the <see cref="FileType"/> property.
        /// </summary>
        public const ArchiveFileType DefaultFileType = ArchiveFileType.Active;

        /// <summary>
        /// Specifies the default value for the <see cref="FileSize"/> property.
        /// </summary>
        public const int DefaultFileSize = 1500;

        /// <summary>
        /// Specifies the default value for the <see cref="FileAccessMode"/> property.
        /// </summary>
        public const FileAccess DefaultFileAccessMode = FileAccess.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="DataBlockSize"/> property.
        /// </summary>
        public const int DefaultDataBlockSize = 8;

        /// <summary>
        /// Specifies the default value for the <see cref="RolloverPreparationThreshold"/> property.
        /// </summary>
        public const double DefaultRolloverPreparationThreshold = 75.0D;

        /// <summary>
        /// Specifies the default value for the <see cref="ArchiveOffloadCount"/> property.
        /// </summary>
        public const int DefaultArchiveOffloadCount = 5;

        /// <summary>
        /// Specifies the default value for the <see cref="ArchiveOffloadLocation"/> property.
        /// </summary>
        public const string DefaultArchiveOffloadLocation = "";

        /// <summary>
        /// Specifies the default value for the <see cref="ArchiveOffloadThreshold"/> property.
        /// </summary>
        public const double DefaultArchiveOffloadThreshold = 90.0D;

        /// <summary>
        /// Specifies the default value for the <see cref="ArchiveOffloadMaxAge"/> property.
        /// </summary>
        public const int DefaultArchiveOffloadMaxAge = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxHistoricArchiveFiles"/> property.
        /// </summary>
        public const int DefaultMaxHistoricArchiveFiles = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="LeadTimeTolerance"/> property.
        /// </summary>
        public const int DefaultLeadTimeTolerance = 15;

        /// <summary>
        /// Specifies the default value for the <see cref="CompressData"/> property.
        /// </summary>
        public const bool DefaultCompressData = true;

        /// <summary>
        /// Specifies the default value for the <see cref="DiscardOutOfSequenceData"/> property.
        /// </summary>
        public const bool DefaultDiscardOutOfSequenceData = true;

        /// <summary>
        /// Specifies the default value for the <see cref="CacheWrites"/> property.
        /// </summary>
        public const bool DefaultCacheWrites = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ConserveMemory"/> property.
        /// </summary>
        public const bool DefaultConserveMemory = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MonitorNewArchiveFiles"/> property.
        /// </summary>
        public const bool DefaultMonitorNewArchiveFiles = false;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ArchiveFile";

        /// <summary>
        /// Specifies the extension for current and historic <see cref="ArchiveFile"/>.
        /// </summary>
        private const string FileExtension = ".d";

        /// <summary>
        /// Specifies the extension for a standby <see cref="ArchiveFile"/>.
        /// </summary>
        private const string StandbyFileExtension = ".standby";

        /// <summary>
        /// Specifies the interval (in milliseconds) for the memory conservation process to run.
        /// </summary>
        private const int DataBlockCheckInterval = 60000;

        // Events

        /// <summary>
        /// Occurs when the active <see cref="ArchiveFile"/> if full.
        /// </summary>
        [Category("File"),
        Description("Occurs when the active ArchiveFile if full.")]
        public event EventHandler FileFull;

        /// <summary>
        /// Occurs when the process of offloading historic <see cref="ArchiveFile"/>s is started.
        /// </summary>
        [Category("Archive"),
        Description("Occurs when the process of offloading historic ArchiveFiles is started.")]
        public event EventHandler OffloadStart;

        /// <summary>
        /// Occurs when the process of offloading historic <see cref="ArchiveFile"/>s is complete.
        /// </summary>
        [Category("Archive"),
        Description("Occurs when the process of offloading historic ArchiveFiles is complete.")]
        public event EventHandler OffloadComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during the historic <see cref="ArchiveFile"/> offload process.
        /// </summary>
        [Category("Archive"),
        Description("Occurs when an Exception is encountered during the historic ArchiveFile offload process.")]
        public event EventHandler<EventArgs<Exception>> OffloadException;

        /// <summary>
        /// Occurs when an historic <see cref="ArchiveFile"/> is being offloaded.
        /// </summary>
        [Category("Archive"),
        Description("Occurs when an historic ArchiveFile is being offloaded.")]
        public event EventHandler<EventArgs<ProcessProgress<int>>> OffloadProgress;

        /// <summary>
        /// Occurs when <see cref="Rollover()"/> to a new <see cref="ArchiveFile"/> is started.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when Rollover to a new ArchiveFile is started.")]
        public event EventHandler RolloverStart;

        /// <summary>
        /// Occurs when <see cref="Rollover()"/> to a new <see cref="ArchiveFile"/> is complete.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when Rollover to a new ArchiveFile is complete.")]
        public event EventHandler RolloverComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during the <see cref="Rollover()"/> process.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when an Exception is encountered during the Rollover() process.")]
        public event EventHandler<EventArgs<Exception>> RolloverException;

        /// <summary>
        /// Occurs when the process of creating a standby <see cref="ArchiveFile"/> is started.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when the process of creating a standby ArchiveFile is started.")]
        public event EventHandler RolloverPreparationStart;

        /// <summary>
        /// Occurs when the process of creating a standby <see cref="ArchiveFile"/> is complete.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when the process of creating a standby ArchiveFile is complete.")]
        public event EventHandler RolloverPreparationComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during the standby <see cref="ArchiveFile"/> creation process.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when an Exception is encountered during the standby ArchiveFile creation process.")]
        public event EventHandler<EventArgs<Exception>> RolloverPreparationException;

        /// <summary>
        /// Occurs when the process of building historic <see cref="ArchiveFile"/> list is started.
        /// </summary>
        [Category("File"),
        Description("Occurs when the process of building historic ArchiveFile list is started.")]
        public event EventHandler HistoricFileListBuildStart;

        /// <summary>
        /// Occurs when the process of building historic <see cref="ArchiveFile"/> list is complete.
        /// </summary>
        [Category("File"),
        Description("Occurs when the process of building historic ArchiveFile list is complete.")]
        public event EventHandler HistoricFileListBuildComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered in historic <see cref="ArchiveFile"/> list building process.
        /// </summary>
        [Category("File"),
        Description("Occurs when an Exception is encountered in historic ArchiveFile list building process.")]
        public event EventHandler<EventArgs<Exception>> HistoricFileListBuildException;

        /// <summary>
        /// Occurs when the historic <see cref="ArchiveFile"/> list is updated to reflect addition or deletion of historic <see cref="ArchiveFile"/>s.
        /// </summary>
        [Category("File"),
        Description("Occurs when the historic ArchiveFile list is updated to reflect addition or deletion of historic ArchiveFiles.")]
        public event EventHandler HistoricFileListUpdated;

        /// <summary>
        /// Occurs when <see cref="IDataPoint"/> is received for which a <see cref="StateRecord"/> or <see cref="MetadataRecord"/> does not exist or is marked as disabled.
        /// </summary>
        [Category("Data"),
        Description("Occurs when IDataPoint is received for which a StateRecord or MetadataRecord does not exist or is marked as disabled.")]
        public event EventHandler<EventArgs<IDataPoint>> OrphanDataReceived;

        /// <summary>
        /// Occurs when <see cref="IDataPoint"/> is received with <see cref="TimeTag"/> ahead of the local clock by more than the <see cref="LeadTimeTolerance"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when IDataPoint is received with TimeTag ahead of the local clock by more than the LeadTimeTolerance.")]
        public event EventHandler<EventArgs<IDataPoint>> FutureDataReceived;

        /// <summary>
        /// Occurs when <see cref="IDataPoint"/> that belongs to a historic <see cref="ArchiveFile"/> is received for archival.
        /// </summary>
        [Category("Data"),
        Description("Occurs when IDataPoint that belongs to a historic ArchiveFile is received for archival.")]
        public event EventHandler<EventArgs<IDataPoint>> HistoricDataReceived;

        /// <summary>
        /// Occurs when misaligned (by time) <see cref="IDataPoint"/> is received for archival.
        /// </summary>
        [Category("Data"),
        Description("Occurs when misaligned (by time) IDataPoint is received for archival.")]
        public event EventHandler<EventArgs<IDataPoint>> OutOfSequenceDataReceived;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while reading <see cref="IDataPoint"/> from the current or historic <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while reading IDataPoint from the current or historic ArchiveFile.")]
        public event EventHandler<EventArgs<Exception>> DataReadException;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while writing <see cref="IDataPoint"/> to the current or historic <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while writing IDataPoint to the current or historic ArchiveFile.")]
        public event EventHandler<EventArgs<Exception>> DataWriteException;

        /// <summary>
        /// Occurs when <see cref="IDataPoint"/> triggers an alarm notification.
        /// </summary>
        [Category("File"),
        Description("Occurs when IDataPoint triggers an alarm notification.")]
        public event EventHandler<EventArgs<StateRecord>> ProcessAlarmNotification;

        /// <summary>
        /// Occurs when associated Metadata file is updated.
        /// </summary>
        [Category("Metadata"),
        Description("Occurs when associated Metadata file is updated.")]
        public event EventHandler MetadataUpdated;

        // Fields

        // Component
        private string m_fileName;
        private ArchiveFileType m_fileType;
        private double m_fileSize;
        private FileAccess m_fileAccessMode;
        private int m_dataBlockSize;
        private double m_rolloverPreparationThreshold;
        private string m_archiveOffloadLocation;
        private int m_archiveOffloadCount;
        private double m_archiveOffloadThreshold;
        private int m_archiveOffloadMaxAge;
        private int m_maxHistoricArchiveFiles;
        private double m_leadTimeTolerance;
        private bool m_compressData;
        private bool m_discardOutOfSequenceData;
        private bool m_cacheWrites;
        private bool m_conserveMemory;
        private bool m_monitorNewArchiveFiles;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private ArchiveFileAllocationTable m_fat;

        // Operational
        private bool m_disposed;
        private bool m_initialized;
        private FileStream m_fileStream;
        private List<ArchiveDataBlock> m_dataBlocks;
        private List<Info> m_historicArchiveFiles;
        private readonly Dictionary<int, decimal> m_delayedAlarmProcessing;
        private volatile bool m_rolloverInProgress;
        private long m_activeFileReaders;

        // Threading
        private Thread m_rolloverPreparationThread;
        private Thread m_buildHistoricFileListThread;
        private readonly ManualResetEvent m_rolloverWaitHandle;

        // Components
        private StateFile m_stateFile;
        private IntercomFile m_intercomFile;
        private MetadataFile m_metadataFile;
        private readonly Timer m_conserveMemoryTimer;
        private readonly ProcessQueue<IDataPoint> m_currentDataQueue;
        private readonly ProcessQueue<IDataPoint> m_historicDataQueue;
        private readonly ProcessQueue<IDataPoint> m_outOfSequenceDataQueue;
        private SafeFileWatcher m_currentLocationFileWatcher;
        private SafeFileWatcher m_offloadLocationFileWatcher;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFile"/> class.
        /// </summary>
        public ArchiveFile()
        {
            m_fileName = DefaultFileName;
            m_fileType = DefaultFileType;
            m_fileSize = DefaultFileSize;
            m_fileAccessMode = DefaultFileAccessMode;
            m_dataBlockSize = DefaultDataBlockSize;
            m_rolloverPreparationThreshold = DefaultRolloverPreparationThreshold;
            m_archiveOffloadLocation = DefaultArchiveOffloadLocation;
            m_archiveOffloadCount = DefaultArchiveOffloadCount;
            m_archiveOffloadThreshold = DefaultArchiveOffloadThreshold;
            m_archiveOffloadMaxAge = DefaultArchiveOffloadMaxAge;
            m_maxHistoricArchiveFiles = DefaultMaxHistoricArchiveFiles;
            m_leadTimeTolerance = DefaultLeadTimeTolerance;
            m_compressData = DefaultCompressData;
            m_discardOutOfSequenceData = DefaultDiscardOutOfSequenceData;
            m_cacheWrites = DefaultCacheWrites;
            m_conserveMemory = DefaultConserveMemory;
            m_monitorNewArchiveFiles = DefaultMonitorNewArchiveFiles;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;

            m_delayedAlarmProcessing = new Dictionary<int, decimal>();
            m_rolloverWaitHandle = new ManualResetEvent(true);
            m_rolloverPreparationThread = new Thread(PrepareForRollover);
            m_buildHistoricFileListThread = new Thread(BuildHistoricFileList);

            m_conserveMemoryTimer = new Timer();
            m_conserveMemoryTimer.Elapsed += ConserveMemoryTimer_Elapsed;

            m_currentDataQueue = ProcessQueue<IDataPoint>.CreateRealTimeQueue(WriteToCurrentArchiveFile);
            m_currentDataQueue.ProcessException += CurrentDataQueue_ProcessException;

            m_historicDataQueue = ProcessQueue<IDataPoint>.CreateRealTimeQueue(WriteToHistoricArchiveFile);
            m_historicDataQueue.ProcessException += HistoricDataQueue_ProcessException;

            m_outOfSequenceDataQueue = ProcessQueue<IDataPoint>.CreateRealTimeQueue(InsertInCurrentArchiveFile);
            m_outOfSequenceDataQueue.ProcessException += OutOfSequenceDataQueue_ProcessException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ArchiveFile"/>.</param>
        public ArchiveFile(IContainer container)
            : this()
        {
            if ((object)container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        /// <exception cref="ArgumentException">The value being assigned contains an invalid file extension.</exception>
        [Category("Configuration"),
        DefaultValue(DefaultFileName),
        Description("Name of the ArchiveFile.")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                if (string.Compare(FilePath.GetExtension(value), FileExtension, StringComparison.OrdinalIgnoreCase) != 0 &&
                    string.Compare(FilePath.GetExtension(value), StandbyFileExtension, StringComparison.OrdinalIgnoreCase) != 0)
                    throw (new ArgumentException(string.Format("{0} must have an extension of {1} or {2}.", GetType().Name, FileExtension, StandbyFileExtension)));

                m_fileName = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ArchiveFileType"/> of the <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Configuration"),
        DefaultValue(DefaultFileType),
        Description("Type of the ArchiveFile.")]
        public ArchiveFileType FileType
        {
            get
            {
                return m_fileType;
            }
            set
            {
                m_fileType = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the size (in MB) of the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        [Category("Configuration"),
        DefaultValue(DefaultFileSize),
        Description("Size (in MB) of the ArchiveFile.")]
        public double FileSize
        {
            get
            {
                return m_fileSize;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive");

                m_fileSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileAccess"/> value to use when opening the <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Configuration"),
        DefaultValue(DefaultFileAccessMode),
        Description("System.IO.FileAccess value to use when opening the ArchiveFile.")]
        public FileAccess FileAccessMode
        {
            get
            {
                return m_fileAccessMode;
            }
            set
            {
                m_fileAccessMode = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets the size (in KB) of the <see cref="ArchiveDataBlock"/>s.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        [Category("Configuration"),
        DefaultValue(DefaultDataBlockSize),
        Description("Size (in KB) of the ArchiveDataBlocks.")]
        public int DataBlockSize
        {
            get
            {
                // This is the only redundant property between the ArchiveFile component and the FAT, so
                // we ensure that this information is synched at least at run time if not at design time.
                if ((object)m_fat == null)
                    return m_dataBlockSize; // Design time.

                return m_fat.DataBlockSize; // Run time.
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive");

                m_dataBlockSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ArchiveFile"/> usage (in %) that will trigger the creation of an empty <see cref="ArchiveFile"/> for rollover.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 1 and 95.</exception>
        [Category("Rollover"),
        DefaultValue(DefaultRolloverPreparationThreshold),
        Description("ArchiveFile usage (in %) that will trigger the creation of an empty ArchiveFile for rollover.")]
        public double RolloverPreparationThreshold
        {
            get
            {
                return m_rolloverPreparationThreshold;
            }
            set
            {
                if (value < 1.0D || value > 95.0D)
                    throw new ArgumentOutOfRangeException(nameof(value), "RolloverPreparationThreshold value must be between 1 and 95");

                m_rolloverPreparationThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of historic <see cref="ArchiveFile"/>s to be offloaded to the <see cref="ArchiveOffloadLocation"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        [Category("Archive"),
        DefaultValue(DefaultArchiveOffloadCount),
        Description("Number of historic ArchiveFiles to be offloaded to the ArchiveOffloadLocation.")]
        public int ArchiveOffloadCount
        {
            get
            {
                return m_archiveOffloadCount;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive");

                m_archiveOffloadCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the directory where historic <see cref="ArchiveFile"/>s are to be offloaded to make space in the primary archive location.
        /// Set to *DELETE* to remove historic files instead of moving them to an offload location.
        /// </summary>
        [Category("Archive"),
        DefaultValue(DefaultArchiveOffloadLocation),
        Description("Path to the directory where historic ArchiveFiles are to be offloaded to make space in the primary archive location. Set to *DELETE* to remove files instead of offloading.")]
        public string ArchiveOffloadLocation
        {
            get
            {
                return m_archiveOffloadLocation;
            }
            set
            {
                m_archiveOffloadLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the free disk space (in %) of the primary archive location that triggers the offload of historic <see cref="ArchiveFile"/>s.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is not between 0 and 99.</exception>
        [Category("Archive"),
        DefaultValue(DefaultArchiveOffloadThreshold),
        Description("Free disk space (in %) of the primary archive location that triggers the offload of historic ArchiveFiles.")]
        public double ArchiveOffloadThreshold
        {
            get
            {
                return m_archiveOffloadThreshold;
            }
            set
            {
                if (value <= 0.0D || value > 99.0D)
                    throw new ArgumentOutOfRangeException(nameof(value), "ArchiveOffloadThreshold value must be between 0 and 99");

                m_archiveOffloadThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of days before an archive file triggers the offload of historic <see cref="ArchiveFile"/>s.
        /// </summary>
        [Category("Archive"),
        DefaultValue(DefaultArchiveOffloadMaxAge),
        Description("Maximum number of days before an archive file will be offloaded.")]
        public int ArchiveOffloadMaxAge
        {
            get
            {
                return m_archiveOffloadMaxAge;
            }
            set
            {
                m_archiveOffloadMaxAge = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of historic <see cref="ArchiveFile"/>s to be kept at both the primary and offload locations combined.
        /// </summary>
        /// <remarks>
        /// Set <see cref="MaxHistoricArchiveFiles"/> to -1 to keep historic <see cref="ArchiveFile"/>s indefinitely.
        /// </remarks>
        [Category("Archive"),
        DefaultValue(DefaultMaxHistoricArchiveFiles),
        Description("Gets or sets the maximum number of historic ArchiveFiles to be kept at both the primary and offload locations combined.")]
        public int MaxHistoricArchiveFiles
        {
            get
            {
                return m_maxHistoricArchiveFiles;
            }
            set
            {
                if (value < 1)
                    m_maxHistoricArchiveFiles = -1;
                else
                    m_maxHistoricArchiveFiles = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of minutes by which incoming <see cref="ArchiveDataPoint"/> can be ahead of local system clock and still be considered valid.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not zero or positive.</exception>
        [Category("Data"),
        DefaultValue(DefaultLeadTimeTolerance),
        Description("Number of minutes by which incoming ArchiveDataPoint can be ahead of local system clock and still be considered valid.")]
        public double LeadTimeTolerance
        {
            get
            {
                return m_leadTimeTolerance;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be zero or positive");

                m_leadTimeTolerance = value;
            }
        }

        /// <summary>
        /// Gets or set a boolean value that indicates whether incoming <see cref="ArchiveDataPoint"/>s are to be compressed to save space.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultCompressData),
        Description("Indicates whether incoming ArchiveDataPoints are to be compressed to save space.")]
        public bool CompressData
        {
            get
            {
                return m_compressData;
            }
            set
            {
                m_compressData = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether incoming <see cref="ArchiveDataPoint"/>s with out-of-sequence <see cref="TimeTag"/> are to be discarded.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultDiscardOutOfSequenceData),
        Description("Indicates whether incoming ArchiveDataPoints with out-of-sequence TimeTag are to be discarded.")]
        public bool DiscardOutOfSequenceData
        {
            get
            {
                return m_discardOutOfSequenceData;
            }
            set
            {
                m_discardOutOfSequenceData = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether writes to the disk are to be cached for performance efficiency.
        /// </summary>
        [Category("Performance"),
        DefaultValue(DefaultCacheWrites),
        Description("indicates whether writes to the disk are to be cached for performance efficiency.")]
        public bool CacheWrites
        {
            get
            {
                return m_cacheWrites;
            }
            set
            {
                m_cacheWrites = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether memory usage is to be kept low for performance efficiency.
        /// </summary>
        [Category("Performance"),
        DefaultValue(DefaultConserveMemory),
        Description("Indicates whether memory usage is to be kept low for performance efficiency.")]
        public bool ConserveMemory
        {
            get
            {
                return m_conserveMemory;
            }
            set
            {
                m_conserveMemory = value;
                ReOpen();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether to monitor and load newly encountered archive files.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultMonitorNewArchiveFiles),
        Description("Indicates whether to monitor and load newly encountered archive files.")]
        public bool MonitorNewArchiveFiles
        {
            get
            {
                return m_monitorNewArchiveFiles;
            }
            set
            {
                m_monitorNewArchiveFiles = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="StateFile"/> used by the <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Dependencies"),
        Description("StateFile used by the ArchiveFile.")]
        public StateFile StateFile
        {
            get
            {
                return m_stateFile;
            }
            set
            {
                m_stateFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataFile"/> used by the <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Dependencies"),
        Description("MetadataFile used by the ArchiveFile.")]
        public MetadataFile MetadataFile
        {
            get
            {
                return m_metadataFile;
            }
            set
            {
                if ((object)m_metadataFile != null)
                {
                    // Detach events from any existing instance
                    m_metadataFile.FileModified -= MetadataFile_FileModified;
                }

                m_metadataFile = value;

                if ((object)m_metadataFile != null)
                {
                    // Attach events to new instance
                    m_metadataFile.FileModified += MetadataFile_FileModified;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IntercomFile"/> used by the <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Dependencies"),
        Description("IntercomFile used by the ArchiveFile.")]
        public IntercomFile IntercomFile
        {
            get
            {
                return m_intercomFile;
            }
            set
            {
                m_intercomFile = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ArchiveFile"/> are to be saved to the config file.
        /// </summary>
        [Category("Persistence"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of ArchiveFile are to be saved to the config file.")]
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
        /// Gets or sets the category under which the settings of <see cref="ArchiveFile"/> are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistence"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of ArchiveFile are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ArchiveFile"/> is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return IsOpen;
            }
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
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="ArchiveFile"/>.
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
        /// Gets the descriptive status of the <see cref="ArchiveFile"/>.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("                 File name: ");
                status.Append(FilePath.TrimFileName(m_fileName, 30));
                status.AppendLine();
                status.Append("                File state: ");
                status.Append(IsOpen ? "Open" : "Closed");
                status.AppendLine();
                status.Append("          File access mode: ");
                status.Append(m_fileAccessMode);
                status.AppendLine();

                if (IsOpen)
                {
                    ArchiveFileStatistics statistics = Statistics;

                    status.Append("                File usage: ");
                    status.Append(statistics.FileUsage.ToString("0.00") + "%");
                    status.AppendLine();
                    status.Append("          Compression rate: ");
                    status.Append(statistics.CompressionRate.ToString("0.00") + "%");
                    status.AppendLine();
                    status.Append("      Data points received: ");
                    status.Append(m_fat.DataPointsReceived);
                    status.AppendLine();
                    status.Append("      Data points archived: ");
                    status.Append(m_fat.DataPointsArchived);
                    status.AppendLine();
                    status.Append("       Average write speed: ");
                    status.Append(statistics.AverageWriteSpeed + " per Second");
                    status.AppendLine();
                    status.Append("          Averaging window: ");
                    status.Append(statistics.AveragingWindow.ToString(3));
                    status.AppendLine();

                    if ((object)m_historicArchiveFiles != null)
                    {
                        status.Append("    Historic archive files: ");

                        lock (m_historicArchiveFiles)
                        {
                            status.Append(m_historicArchiveFiles.Count);
                        }

                        status.AppendLine();
                    }
                }

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="ArchiveFile"/> is currently open.
        /// </summary>
        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return ((object)m_fileStream != null && (object)m_fat != null);
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="FileStream"/> of the <see cref="ArchiveFile"/>.
        /// </summary>
        [Browsable(false)]
        public FileStream FileData
        {
            get
            {
                return m_fileStream;
            }
        }

        /// <summary>
        /// Gets the <see cref="ArchiveFileAllocationTable"/> of the <see cref="ArchiveFile"/>.
        /// </summary>
        [Browsable(false)]
        public ArchiveFileAllocationTable Fat
        {
            get
            {
                return m_fat;
            }
        }

        /// <summary>
        /// Gets the <see cref="ArchiveFileStatistics"/> object of the <see cref="ArchiveFile"/>.
        /// </summary>
        [Browsable(false)]
        public ArchiveFileStatistics Statistics
        {
            get
            {
                ArchiveFileStatistics statistics = new ArchiveFileStatistics();

                if (IsOpen)
                {
                    // Calculate file usage.
                    IntercomRecord system = m_intercomFile.Read(1);

                    if (m_fileType == ArchiveFileType.Active && (object)system != null)
                        statistics.FileUsage = ((float)system.DataBlocksUsed / (float)m_fat.DataBlockCount) * 100;
                    else
                        statistics.FileUsage = ((float)m_fat.DataBlocksUsed / (float)m_fat.DataBlockCount) * 100;

                    // Calculate compression rate.
                    if (m_fat.DataPointsReceived >= 1)
                        statistics.CompressionRate = ((float)(m_fat.DataPointsReceived - m_fat.DataPointsArchived) / (float)m_fat.DataPointsReceived) * 100;

                    if (m_currentDataQueue.RunTime >= 1.0D)
                    {
                        statistics.AveragingWindow = m_currentDataQueue.RunTime;

                        statistics.AverageWriteSpeed =
                            (int)((m_currentDataQueue.CurrentStatistics.TotalProcessedItems -
                            (m_historicDataQueue.CurrentStatistics.TotalProcessedItems +
                            m_historicDataQueue.CurrentStatistics.QueueCount +
                            m_historicDataQueue.CurrentStatistics.ItemsBeingProcessed +
                            m_outOfSequenceDataQueue.CurrentStatistics.TotalProcessedItems +
                            m_outOfSequenceDataQueue.CurrentStatistics.QueueCount +
                            m_outOfSequenceDataQueue.CurrentStatistics.ItemsBeingProcessed)) / (long)statistics.AveragingWindow);
                    }
                }

                return statistics;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessQueueStatistics"/> for the internal current data write <see cref="ProcessQueue{T}"/>.
        /// </summary>
        [Browsable(false)]
        public ProcessQueueStatistics CurrentWriteStatistics
        {
            get
            {
                return m_currentDataQueue.CurrentStatistics;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessQueueStatistics"/> for the internal historic data write <see cref="ProcessQueue{T}"/>.
        /// </summary>
        [Browsable(false)]
        public ProcessQueueStatistics HistoricWriteStatistics
        {
            get
            {
                return m_historicDataQueue.CurrentStatistics;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessQueueStatistics"/> for the internal out-of-sequence data write <see cref="ProcessQueue{T}"/>.
        /// </summary>
        [Browsable(false)]
        public ProcessQueueStatistics OutOfSequenceWriteStatistics
        {
            get
            {
                return m_outOfSequenceDataQueue.CurrentStatistics;
            }
        }

        /// <summary>
        /// Gets wait handle used to synchronize roll-over access.
        /// </summary>
        protected internal ManualResetEvent RolloverWaitHandle
        {
            get
            {
                return m_rolloverWaitHandle;
            }
        }

        /// <summary>
        /// Gets the name of the standby <see cref="ArchiveFile"/>.
        /// </summary>
        private string StandbyArchiveFileName
        {
            get
            {
                return Path.ChangeExtension(m_fileName, StandbyFileExtension);
            }
        }

        /// <summary>
        /// Gets the name to be used when promoting an active <see cref="ArchiveFile"/> to historic <see cref="ArchiveFile"/>.
        /// </summary>
        private string HistoryArchiveFileName
        {
            get
            {
                return FilePath.GetDirectoryName(m_fileName) + (FilePath.GetFileNameWithoutExtension(m_fileName) + "_" + m_fat.FileStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "_to_" + m_fat.FileEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + FileExtension).Replace(':', '!');
            }
        }

        /// <summary>
        /// Gets the pattern to be used when searching for historic <see cref="ArchiveFile"/>s.
        /// </summary>
        private string HistoricFilesSearchPattern
        {
            get
            {
                return FilePath.GetFileNameWithoutExtension(m_fileName) + "_*_to_*" + FileExtension;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ArchiveFile"/> is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.

                m_initialized = true;   // Initialize only once.

                if (m_monitorNewArchiveFiles)
                {
                    m_currentLocationFileWatcher = new SafeFileWatcher();
                    m_currentLocationFileWatcher.IncludeSubdirectories = true;
                    m_currentLocationFileWatcher.Renamed += FileWatcher_Renamed;
                    m_currentLocationFileWatcher.Deleted += FileWatcher_Deleted;
                    m_currentLocationFileWatcher.Created += FileWatcher_Created;

                    m_offloadLocationFileWatcher = new SafeFileWatcher();
                    m_offloadLocationFileWatcher.IncludeSubdirectories = true;
                    m_offloadLocationFileWatcher.Renamed += FileWatcher_Renamed;
                    m_offloadLocationFileWatcher.Deleted += FileWatcher_Deleted;
                    m_offloadLocationFileWatcher.Created += FileWatcher_Created;
                }
            }
        }
        /// <summary>
        /// Performs necessary operations before the <see cref="ArchiveFile"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ArchiveFile"/> is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            if (!DesignMode)
            {
                try
                {
                    //if ((object)m_currentLocationFileWatcher != null)
                    //    m_currentLocationFileWatcher.BeginInit();

                    //if ((object)m_offloadLocationFileWatcher != null)
                    //    m_offloadLocationFileWatcher.BeginInit();
                }
                catch
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Performs necessary operations after the <see cref="ArchiveFile"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ArchiveFile"/> is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();

                    //m_currentLocationFileWatcher.EndInit();
                    //m_offloadLocationFileWatcher.EndInit();
                }
                catch
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="ArchiveFile"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                settings["FileName", true].Update(m_fileName);
                settings["FileType", true].Update(m_fileType);
                settings["FileSize", true].Update(m_fileSize);
                settings["DataBlockSize", true].Update(m_dataBlockSize);
                settings["RolloverPreparationThreshold", true].Update(m_rolloverPreparationThreshold);
                settings["ArchiveOffloadLocation", true].Update(m_archiveOffloadLocation);
                settings["ArchiveOffloadCount", true].Update(m_archiveOffloadCount);
                settings["ArchiveOffloadThreshold", true].Update(m_archiveOffloadThreshold);
                settings["ArchiveOffloadMaxAge", true].Update(m_archiveOffloadMaxAge);
                settings["MaxHistoricArchiveFiles", true].Update(m_maxHistoricArchiveFiles);
                settings["LeadTimeTolerance", true].Update(m_leadTimeTolerance);
                settings["CompressData", true].Update(m_compressData);
                settings["DiscardOutOfSequenceData", true].Update(m_discardOutOfSequenceData);
                settings["CacheWrites", true].Update(m_cacheWrites);
                settings["ConserveMemory", true].Update(m_conserveMemory);
                settings["MonitorNewArchiveFiles", true].Update(m_monitorNewArchiveFiles);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ArchiveFile"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                settings.Add("FileName", m_fileName, "Name of the file including its path.");
                settings.Add("FileType", m_fileType, "Type (Active; Standby; Historic) of the file.");
                settings.Add("FileSize", m_fileSize, "Size (in MB) of the file. Typical size = 100.");
                settings.Add("DataBlockSize", m_dataBlockSize, "Size (in KB) of the data blocks in the file.");
                settings.Add("RolloverPreparationThreshold", m_rolloverPreparationThreshold, "Percentage file full when the rollover preparation should begin.");
                settings.Add("ArchiveOffloadLocation", m_archiveOffloadLocation, "Path to the location where historic files are to be moved when disk start getting full. Set to *DELETE* to remove files instead of offloading.");
                settings.Add("ArchiveOffloadCount", m_archiveOffloadCount, "Number of files that are to be moved to the offload location when the disk starts getting full.");
                settings.Add("ArchiveOffloadThreshold", m_archiveOffloadThreshold, "Percentage disk full when the historic files should be moved to the offload location.");
                settings.Add("ArchiveOffloadMaxAge", m_archiveOffloadMaxAge, "Maximum number of days before an archive file will be offloaded.");
                settings.Add("MaxHistoricArchiveFiles", m_maxHistoricArchiveFiles, "Maximum number of historic files to be kept at both the primary and offload locations combined.");
                settings.Add("LeadTimeTolerance", m_leadTimeTolerance, "Number of minutes by which incoming data points can be ahead of local system clock and still be considered valid.");
                settings.Add("CompressData", m_compressData, "True if compression (swinging door - lossy) is to be performed on the incoming data points; otherwise False.");
                settings.Add("DiscardOutOfSequenceData", m_discardOutOfSequenceData, "True if out-of-sequence data points are to be discarded; otherwise False.");
                settings.Add("CacheWrites", m_cacheWrites, "True if writes are to be cached for performance; otherwise False.");
                settings.Add("ConserveMemory", m_conserveMemory, "True if attempts are to be made to conserve memory; otherwise False.");
                settings.Add("MonitorNewArchiveFiles", m_monitorNewArchiveFiles, "True to monitor and load newly encountered archive files; otherwise False.");

                FileName = settings["FileName"].ValueAs(m_fileName);
                FileType = settings["FileType"].ValueAs(m_fileType);
                FileSize = settings["FileSize"].ValueAs(m_fileSize);
                DataBlockSize = settings["DataBlockSize"].ValueAs(m_dataBlockSize);
                RolloverPreparationThreshold = settings["RolloverPreparationThreshold"].ValueAs(m_rolloverPreparationThreshold);
                ArchiveOffloadLocation = settings["ArchiveOffloadLocation"].ValueAs(m_archiveOffloadLocation);
                ArchiveOffloadCount = settings["ArchiveOffloadCount"].ValueAs(m_archiveOffloadCount);
                ArchiveOffloadThreshold = settings["ArchiveOffloadThreshold"].ValueAs(m_archiveOffloadThreshold);
                ArchiveOffloadMaxAge = settings["ArchiveOffloadMaxAge"].ValueAs(m_archiveOffloadMaxAge);
                MaxHistoricArchiveFiles = settings["MaxHistoricArchiveFiles"].ValueAs(m_maxHistoricArchiveFiles);
                LeadTimeTolerance = settings["LeadTimeTolerance"].ValueAs(m_leadTimeTolerance);
                CompressData = settings["CompressData"].ValueAs(m_compressData);
                DiscardOutOfSequenceData = settings["DiscardOutOfSequenceData"].ValueAs(m_discardOutOfSequenceData);
                CacheWrites = settings["CacheWrites"].ValueAs(m_cacheWrites);
                ConserveMemory = settings["ConserveMemory"].ValueAs(m_conserveMemory);
            }
        }

        /// <summary>
        /// Opens the <see cref="ArchiveFile"/> for use.
        /// </summary>
        /// <exception cref="InvalidOperationException">One or all of the <see cref="StateFile"/>, <see cref="IntercomFile"/> or <see cref="MetadataFile"/> properties are not set.</exception>
        public void Open()
        {
            if (!IsOpen)
            {
                // Check for the existence of dependencies.
                if ((object)m_stateFile == null || (object)m_intercomFile == null || (object)m_metadataFile == null)
                    throw (new InvalidOperationException("One or more of the dependency files are not specified."));

                // Validate file type against its name.
                if (Path.GetExtension(m_fileName).ToNonNullString().ToLower() == StandbyFileExtension)
                    m_fileType = ArchiveFileType.Standby;
                else if (Regex.IsMatch(m_fileName.ToLower(), string.Format(".+_.+_to_.+\\{0}$", FileExtension)))
                    m_fileType = ArchiveFileType.Historic;
                else
                    m_fileType = ArchiveFileType.Active;

                // Get the absolute path for the file name.
                m_fileName = FilePath.GetAbsolutePath(m_fileName);

                // Create the directory if it does not exist.
                if (!Directory.Exists(FilePath.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(FilePath.GetDirectoryName(m_fileName));

                // Validate a roll-over is not in progress when opening archive as read-only
                if (m_fileType == ArchiveFileType.Active && m_fileAccessMode == FileAccess.Read)
                {
                    // Open intercom file if closed.
                    if (!m_intercomFile.IsOpen)
                        m_intercomFile.Open();

                    m_intercomFile.Load();
                    IntercomRecord record = m_intercomFile.Read(1);
                    int waitCount = 0;

                    while ((object)record != null && record.RolloverInProgress && waitCount < 30)
                    {
                        Thread.Sleep(1000);
                        m_intercomFile.Load();
                        record = m_intercomFile.Read(1);
                        waitCount++;
                    }
                }

                OpenStream();

                // Open state file if closed.
                if (!m_stateFile.IsOpen)
                    m_stateFile.Open();

                // Open intercom file if closed.
                if (!m_intercomFile.IsOpen)
                    m_intercomFile.Open();

                // Open metadata file if closed.
                if (!m_metadataFile.IsOpen)
                    m_metadataFile.Open();

                // Don't proceed further for standby and historic files.
                if (m_fileType != ArchiveFileType.Active)
                    return;

                // Start internal process queues.
                m_currentDataQueue.Start();
                m_historicDataQueue.Start();
                m_outOfSequenceDataQueue.Start();

                // Create data block lookup list.
                if (m_stateFile.RecordsInMemory > 0)
                    m_dataBlocks = new List<ArchiveDataBlock>(new ArchiveDataBlock[m_stateFile.RecordsInMemory]);
                else
                    m_dataBlocks = new List<ArchiveDataBlock>(new ArchiveDataBlock[m_stateFile.RecordsOnDisk]);

                // Validate the dependency files.
                SyncStateFile(null);

                if (m_intercomFile.FileAccessMode != FileAccess.Read)
                {
                    // Ensure that "rollover in progress" is not set.
                    IntercomRecord system = m_intercomFile.Read(1);

                    if ((object)system == null)
                        system = new IntercomRecord(1);

                    system.RolloverInProgress = false;
                    m_intercomFile.Write(1, system);
                }

                // Start the memory conservation process.
                if (m_conserveMemory)
                {
                    m_conserveMemoryTimer.Interval = DataBlockCheckInterval;
                    m_conserveMemoryTimer.Start();
                }

                if (m_fileType == ArchiveFileType.Active)
                {
                    // Start preparing the list of historic files.
                    m_buildHistoricFileListThread = new Thread(BuildHistoricFileList);
                    m_buildHistoricFileListThread.Priority = ThreadPriority.Lowest;
                    m_buildHistoricFileListThread.Start();

                    // Start file watchers to monitor file system changes.
                    if (m_monitorNewArchiveFiles)
                    {
                        if ((object)m_currentLocationFileWatcher != null)
                        {
                            m_currentLocationFileWatcher.Filter = HistoricFilesSearchPattern;
                            m_currentLocationFileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                            m_currentLocationFileWatcher.EnableRaisingEvents = true;
                        }

                        if (Directory.Exists(m_archiveOffloadLocation) && (object)m_offloadLocationFileWatcher != null)
                        {
                            m_offloadLocationFileWatcher.Filter = HistoricFilesSearchPattern;
                            m_offloadLocationFileWatcher.Path = m_archiveOffloadLocation;
                            m_offloadLocationFileWatcher.EnableRaisingEvents = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Closes the <see cref="ArchiveFile"/> if it <see cref="IsOpen"/>.
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                // Abort all asynchronous processing.
                m_rolloverPreparationThread.Abort();
                m_buildHistoricFileListThread.Abort();

                // Stop all timer based processing.
                m_conserveMemoryTimer.Stop();

                // Stop the historic and out-of-sequence data queues.
                m_currentDataQueue.Flush();
                m_historicDataQueue.Flush();
                m_outOfSequenceDataQueue.Flush();

                CloseStream();

                if ((object)m_dataBlocks != null)
                {
                    lock (m_dataBlocks)
                    {
                        m_dataBlocks.Clear();
                    }
                    m_dataBlocks = null;
                }

                // Stop watching for historic archive files.
                if ((object)m_currentLocationFileWatcher != null)
                    m_currentLocationFileWatcher.EnableRaisingEvents = false;

                if ((object)m_offloadLocationFileWatcher != null)
                    m_offloadLocationFileWatcher.EnableRaisingEvents = false;

                // Clear the list of historic archive files.
                if ((object)m_historicArchiveFiles != null)
                {
                    lock (m_historicArchiveFiles)
                    {
                        m_historicArchiveFiles.Clear();
                    }
                    m_historicArchiveFiles = null;
                }
            }
        }

        /// <summary>
        /// Saves the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="ArchiveFile"/> is not open.</exception>
        public void Save()
        {
            if (IsOpen)
            {
                m_fat.Save();
            }
            else
            {
                throw new InvalidOperationException(string.Format("\"{0}\" is not open", m_fileName));
            }
        }

        /// <summary>
        /// Performs rollover of active <see cref="ArchiveFile"/> to a new <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="ArchiveFile"/> is not <see cref="ArchiveFileType.Active"/>.</exception>
        public void Rollover()
        {
            if (m_fileType != ArchiveFileType.Active)
                throw new InvalidOperationException("Cannot rollover a file that is not active");

            try
            {
                // Notify internal components about the rollover.
                m_rolloverWaitHandle.Reset();

                // Raise roll-over start event - this also sets m_rolloverInProgress flag
                OnRolloverStart();

                // Notify external components about the rollover.
                IntercomRecord system = m_intercomFile.Read(1);
                system.DataBlocksUsed = 0;
                system.RolloverInProgress = true;
                system.LatestDataID = -1;
                system.LatestDataTime = TimeTag.MinValue;
                m_intercomFile.Write(1, system);
                m_intercomFile.Save();

                // Figure out the end date for this file.
                StateRecord state;
                TimeTag endTime = m_fat.FileEndTime;

                for (int i = 1; i <= m_stateFile.RecordsOnDisk; i++)
                {
                    state = m_stateFile.Read(i);
                    state.ActiveDataBlockIndex = -1;
                    state.ActiveDataBlockSlot = 1;

                    if (state.ArchivedData.Time.CompareTo(endTime) > 0)
                        endTime = state.ArchivedData.Time;

                    m_stateFile.Write(state.HistorianID, state);
                }

                m_fat.FileEndTime = endTime;

                m_stateFile.Save();
                Save();

                // Wait for any pending readers to release
                WaitForReadersRelease();

                // Clear all of the cached data blocks.
                lock (m_dataBlocks)
                {
                    for (int i = 0; i < m_dataBlocks.Count; i++)
                    {
                        m_dataBlocks[i] = null;
                    }
                }

                string historyFileName = HistoryArchiveFileName;
                string standbyFileName = StandbyArchiveFileName;

                CloseStream();

                // CRITICAL: Exception can be encountered if exclusive lock to the current file cannot be obtained.
                if (File.Exists(m_fileName))
                {
                    try
                    {
                        FilePath.WaitForWriteLock(m_fileName, 60);  // Wait for an exclusive lock on the file.
                        MoveFile(m_fileName, historyFileName);      // Make the active archive file historic.

                        if (File.Exists(standbyFileName))
                        {
                            // We have a "standby" archive file for us to use, so we'll use it. It is possible that
                            // the "standby" file may not be available for use if it could not be created due to
                            // insufficient disk space during the "rollover preparation stage". If that's the case,
                            // Open() below will try to create a new archive file, but will only succeed if there
                            // is enough disk space.
                            MoveFile(standbyFileName, m_fileName); // Make the standby archive file active.
                        }
                    }
                    catch
                    {
                        OpenStream();
                        throw;
                    }
                }

                // CRITICAL: Exception can be encountered if a "standby" archive is not present for us to use and
                //           we cannot create a new archive file probably because there isn't enough disk space.
                try
                {
                    OpenStream();

                    m_fat.FileStartTime = endTime;
                    m_fat.Save();

                    // Notify server that rollover is complete.
                    system.RolloverInProgress = false;
                    m_intercomFile.Write(1, system);
                    m_intercomFile.Save();

                    // Raise roll-over complete event - this also resets m_rolloverInProgress flag
                    OnRolloverComplete();

                    // Notify other threads that rollover is complete.
                    m_rolloverWaitHandle.Set();
                }
                catch
                {
                    CloseStream(); // Close the file if we fail to open it.
                    DeleteFile(m_fileName);
                    throw; // Rethrow the exception so that the exception event can be raised.
                }
            }
            catch (Exception ex)
            {
                // Raise roll-over exception event if there is an error - this will also reset m_rolloverInProgress flag
                OnRolloverException(ex);
            }
        }

        /// <summary>
        /// Requests a resynchronization of the state file.
        /// </summary>
        public void SynchronizeStateFile()
        {
            ThreadPool.QueueUserWorkItem(SyncStateFile);
        }

        /// <summary>
        /// Writes the specified <paramref name="dataPoint"/> to the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to be written.</param>
        public void WriteData(IDataPoint dataPoint)
        {
            // Yeild to archive rollover process.
            m_rolloverWaitHandle.WaitOne();

            // Ensure that the current file is open.
            if (!IsOpen)
                throw new InvalidOperationException(string.Format("\"{0}\" file is not open", m_fileName));

            // Ensure that the current file is active.
            if (m_fileType != ArchiveFileType.Active)
                throw new InvalidOperationException("Data can only be directly written to files that are Active");

            m_currentDataQueue.Add(dataPoint);
        }

        /// <summary>
        /// Writes the specified <paramref name="dataPoints"/> to the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="dataPoints"><see cref="ArchiveDataPoint"/> points to be written.</param>
        public void WriteData(IEnumerable<IDataPoint> dataPoints)
        {
            foreach (IDataPoint dataPoint in dataPoints)
            {
                WriteData(dataPoint);
            }
        }

        /// <summary>
        /// Writes <paramref name="metadata"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="metadata"><see cref="MetadataRecord"/> data.</param>
        public void WriteMetaData(int historianID, byte[] metadata)
        {
            MetadataFile.Write(historianID, new MetadataRecord(historianID, MetadataFile.LegacyMode, metadata, 0, metadata.Length));
            MetadataFile.Save();
        }

        /// <summary>
        /// Writes <paramref name="statedata"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="statedata"><see cref="StateRecord"/> data.</param>
        public void WriteStateData(int historianID, byte[] statedata)
        {
            StateFile.Write(historianID, new StateRecord(historianID, statedata, 0, statedata.Length));
            StateFile.Save();
        }

        /// <summary>
        /// Reads all <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, bool timeSorted = true)
        {
            return ReadData(historianID, TimeTag.MinValue, timeSorted);
        }

        /// <summary>
        /// Reads all <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/> for the specified <paramref name="historianIDs"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, bool timeSorted = true)
        {
            return ReadData(historianIDs, TimeTag.MinValue, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, string startTime, bool timeSorted = true)
        {
            return ReadData(historianID, startTime, TimeTag.MinValue.ToString(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, string startTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, TimeTag.MinValue.ToString(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime"><see cref="String"/> representation of the end time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, string startTime, string endTime, bool timeSorted = true)
        {
            return ReadData(historianID, TimeTag.Parse(startTime), TimeTag.Parse(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime"><see cref="String"/> representation of the end time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, string startTime, string endTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, TimeTag.Parse(startTime), TimeTag.Parse(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, DateTime startTime, bool timeSorted = true)
        {
            return ReadData(historianID, startTime, TimeTag.MinValue.ToDateTime(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, DateTime startTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, TimeTag.MinValue.ToDateTime(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, DateTime startTime, DateTime endTime, bool timeSorted = true)
        {
            return ReadData(historianID, new TimeTag(startTime), new TimeTag(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, DateTime startTime, DateTime endTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, new TimeTag(startTime), new TimeTag(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime, bool timeSorted = true)
        {
            return ReadData(historianID, startTime, TimeTag.MaxValue, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, TimeTag.MaxValue, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime, TimeTag endTime, bool timeSorted = true)
        {
            return ReadData(new[] { historianID }, startTime, endTime, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, TimeTag endTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, endTime, null, timeSorted);
        }

        // Read data implementation
        private IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, TimeTag endTime, IDataPoint resumeFrom, bool timeSorted)
        {
            // Yield to archive rollover process.
            m_rolloverWaitHandle.WaitOne();

            // Ensure that the current file is open.
            if (!IsOpen)
                throw new InvalidOperationException(string.Format("\"{0}\" file is not open", m_fileName));

            // Ensure that the current file is active.
            if (m_fileType != ArchiveFileType.Active)
                throw new InvalidOperationException("Data can only be directly read from files that are Active");

            // Ensure that the start and end time are valid.
            if (startTime.CompareTo(endTime) > 0)
                throw new ArgumentException("End Time precedes Start Time in the specified time span");

            List<Info> dataFiles = new List<Info>();
            bool pendingRollover = false;
            bool usingActiveFile = false;

            if (startTime.CompareTo(m_fat.FileStartTime) < 0)
            {
                // Data is to be read from historic file(s) - make sure that the list has been built
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                lock (m_historicArchiveFiles)
                {
                    dataFiles.AddRange(m_historicArchiveFiles.FindAll(info => FindHistoricArchiveFileForRead(info, startTime, endTime)));
                }
            }

            if (endTime.CompareTo(m_fat.FileStartTime) >= 0)
            {
                // Data is to be read from the active file.
                Info activeFileInfo = new Info(m_fileName)
                {
                    StartTimeTag = m_fat.FileStartTime,
                    EndTimeTag = m_fat.FileEndTime
                };

                dataFiles.Add(activeFileInfo);
            }

            // Read data from all qualifying files.
            foreach (Info dataFile in dataFiles)
            {
                ArchiveFile file = null;

                try
                {
                    if (string.Compare(dataFile.FileName, m_fileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // Read data from current file.
                        usingActiveFile = true;
                        file = this;

                        // Atomically increment total number of readers for active file
                        Interlocked.Increment(ref m_activeFileReaders);

                        // Handle race conditions between rollover
                        // and incrementing the active readers
                        while (m_rolloverInProgress)
                        {
                            Interlocked.Decrement(ref m_activeFileReaders);
                            m_rolloverWaitHandle.WaitOne();
                            Interlocked.Increment(ref m_activeFileReaders);
                        }
                    }
                    else
                    {
                        // Read data from historic file.
                        usingActiveFile = false;
                        file = new ArchiveFile();
                        file.FileName = dataFile.FileName;
                        file.StateFile = m_stateFile;
                        file.IntercomFile = m_intercomFile;
                        file.MetadataFile = m_metadataFile;
                        file.FileAccessMode = FileAccess.Read;
                        file.Open();
                    }

                    // Create new data point scanner for the desired points in this file and given time range
                    IArchiveFileScanner scanner;

                    if (timeSorted)
                        scanner = new TimeSortedArchiveFileScanner();
                    else
                        scanner = new ArchiveFileScanner();

                    scanner.FileAllocationTable = file.Fat;
                    scanner.HistorianIDs = historianIDs;
                    scanner.StartTime = startTime;
                    scanner.EndTime = endTime;
                    scanner.ResumeFrom = resumeFrom;
                    scanner.DataReadExceptionHandler = (sender, e) => OnDataReadException(e.Argument);

                    // Reset resumeFrom to scan from beginning after picking up where left off from roll over
                    resumeFrom = null;

                    // Return data points
                    foreach (IDataPoint dataPoint in scanner.Read())
                    {
                        yield return dataPoint;

                        // If a rollover needs to happen, we need to relinquish read lock and close file
                        if (m_rolloverInProgress)
                        {
                            resumeFrom = dataPoint;
                            pendingRollover = true;
                            break;
                        }
                    }
                }
                finally
                {
                    if (usingActiveFile)
                    {
                        // Atomically decrement active file reader count to signal in-process code that read is complete or yielded
                        Interlocked.Decrement(ref m_activeFileReaders);
                    }
                    else if ((object)file != null)
                    {
                        file.Dispose();
                    }
                }

                if (pendingRollover)
                    break;
            }

            if (pendingRollover)
            {
                // Recurse into this function with an updated start time and last read point ID so that read can
                // resume right where it left off - recursed function call will wait until rollover is complete
                foreach (IDataPoint dataPoint in ReadData(historianIDs, startTime, endTime, resumeFrom, timeSorted))
                {
                    yield return dataPoint;
                }
            }
        }

        /// <summary>
        /// Reads <see cref="MetadataRecord"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="MetadataRecord"/> of <see cref="MetadataRecord"/> if found; otherwise null.</returns>
        public byte[] ReadMetaData(int historianID)
        {
            MetadataRecord record = MetadataFile.Read(historianID);

            if ((object)record == null)
                return null;

            return record.BinaryImage();
        }

        /// <summary>
        /// Reads <see cref="StateRecord"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="StateRecord"/> of <see cref="StateRecord"/> if found; otherwise null.</returns>
        public byte[] ReadStateData(int historianID)
        {
            StateRecord record = StateFile.Read(historianID);

            if ((object)record == null)
                return null;

            return record.BinaryImage();
        }

        /// <summary>
        /// Reads <see cref="MetadataRecordSummary"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="MetadataRecordSummary"/> of <see cref="MetadataRecordSummary"/> if found; otherwise null.</returns>
        public byte[] ReadMetaDataSummary(int historianID)
        {
            MetadataRecord record = MetadataFile.Read(historianID);

            if ((object)record == null)
                return null;

            return record.Summary.BinaryImage();
        }

        /// <summary>
        /// Reads <see cref="StateRecordSummary"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="StateRecordSummary"/> of <see cref="StateRecordSummary"/> if found; otherwise null.</returns>
        public byte[] ReadStateDataSummary(int historianID)
        {
            StateRecord record = StateFile.Read(historianID);

            if ((object)record == null)
                return null;

            return record.Summary.BinaryImage();
        }

        /// <summary>
        /// Waits for all readers to relinquish read locks on active file.
        /// </summary>
        /// <returns>True if readers released read locks in timely fashion.</returns>
        protected internal bool WaitForReadersRelease()
        {
            int waitCount = 0;

            // Wait up to five seconds for readers to release
            while (Interlocked.Read(ref m_activeFileReaders) > 0 && waitCount < 5)
            {
                Thread.Sleep(1000);
                waitCount++;
            }

            bool allReleased = (Interlocked.Read(ref m_activeFileReaders) <= 0);

            return allReleased;
        }

        /// <summary>
        /// Raises the <see cref="FileFull"/> event.
        /// </summary>
        protected virtual void OnFileFull()
        {
            if ((object)FileFull != null)
                FileFull(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverStart"/> event.
        /// </summary>
        protected internal virtual void OnRolloverStart()
        {
            m_rolloverInProgress = true;

            if ((object)RolloverStart != null)
                RolloverStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverComplete"/> event.
        /// </summary>
        protected internal virtual void OnRolloverComplete()
        {
            m_rolloverInProgress = false;

            if ((object)RolloverComplete != null)
                RolloverComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="RolloverException"/> event.</param>
        protected internal virtual void OnRolloverException(Exception ex)
        {
            m_rolloverInProgress = false;

            if ((object)RolloverException != null)
                RolloverException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="OffloadStart"/> event.
        /// </summary>
        protected virtual void OnOffloadStart()
        {
            if ((object)OffloadStart != null)
                OffloadStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OffloadComplete"/> event.
        /// </summary>
        protected virtual void OnOffloadComplete()
        {
            if ((object)OffloadComplete != null)
                OffloadComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OffloadException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="OffloadException"/> event.</param>
        protected virtual void OnOffloadException(Exception ex)
        {
            if ((object)OffloadException != null)
                OffloadException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="OffloadProgress"/> event.
        /// </summary>
        /// <param name="offloadProgress"><see cref="ProcessProgress{T}"/> to send to <see cref="OffloadProgress"/> event.</param>
        protected virtual void OnOffloadProgress(ProcessProgress<int> offloadProgress)
        {
            if ((object)OffloadProgress != null)
                OffloadProgress(this, new EventArgs<ProcessProgress<int>>(offloadProgress));
        }

        /// <summary>
        /// Raises the <see cref="RolloverPreparationStart"/> event.
        /// </summary>
        protected virtual void OnRolloverPreparationStart()
        {
            if ((object)RolloverPreparationStart != null)
                RolloverPreparationStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverPreparationComplete"/> event.
        /// </summary>
        protected virtual void OnRolloverPreparationComplete()
        {
            if ((object)RolloverPreparationComplete != null)
                RolloverPreparationComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverPreparationException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="RolloverPreparationException"/> event.</param>
        protected virtual void OnRolloverPreparationException(Exception ex)
        {
            if ((object)RolloverPreparationException != null)
                RolloverPreparationException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="HistoricFileListBuildStart"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListBuildStart()
        {
            if ((object)HistoricFileListBuildStart != null)
                HistoricFileListBuildStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HistoricFileListBuildComplete"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListBuildComplete()
        {
            if ((object)HistoricFileListBuildComplete != null)
                HistoricFileListBuildComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raise the <see cref="HistoricFileListBuildException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="HistoricFileListBuildException"/> event.</param>
        protected virtual void OnHistoricFileListBuildException(Exception ex)
        {
            if ((object)HistoricFileListBuildException != null)
                HistoricFileListBuildException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="HistoricFileListUpdated"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListUpdated()
        {
            if ((object)HistoricFileListUpdated != null)
                HistoricFileListUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OrphanDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="OrphanDataReceived"/> event.</param>
        protected virtual void OnOrphanDataReceived(IDataPoint dataPoint)
        {
            if ((object)OrphanDataReceived != null)
                OrphanDataReceived(this, new EventArgs<IDataPoint>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="FutureDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="FutureDataReceived"/> event.</param>
        protected virtual void OnFutureDataReceived(IDataPoint dataPoint)
        {
            if ((object)FutureDataReceived != null)
                FutureDataReceived(this, new EventArgs<IDataPoint>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="HistoricDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="HistoricDataReceived"/> event.</param>
        protected virtual void OnHistoricDataReceived(IDataPoint dataPoint)
        {
            if ((object)HistoricDataReceived != null)
                HistoricDataReceived(this, new EventArgs<IDataPoint>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="OutOfSequenceDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="OutOfSequenceDataReceived"/> event.</param>
        protected virtual void OnOutOfSequenceDataReceived(IDataPoint dataPoint)
        {
            if ((object)OutOfSequenceDataReceived != null)
                OutOfSequenceDataReceived(this, new EventArgs<IDataPoint>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="DataReadException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataReadException"/> event.</param>
        protected virtual void OnDataReadException(Exception ex)
        {
            if ((object)DataReadException != null)
                DataReadException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="DataWriteException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataWriteException"/> event.</param>
        protected virtual void OnDataWriteException(Exception ex)
        {
            if ((object)DataWriteException != null)
                DataWriteException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="ProcessAlarmNotification"/> event.
        /// </summary>
        /// <param name="pointState"><see cref="StateRecord"/> to send to <see cref="ProcessAlarmNotification"/> event.</param>
        protected virtual void OnProcessAlarmNotification(StateRecord pointState)
        {
            if ((object)ProcessAlarmNotification != null)
                ProcessAlarmNotification(this, new EventArgs<StateRecord>(pointState));
        }

        /// <summary>
        /// Raises the <see cref="MetadataUpdated"/> event.
        /// </summary>
        protected virtual void OnMetadataUpdated()
        {
            if ((object)MetadataUpdated != null)
                MetadataUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ArchiveFile"/> and optionally releases the managed resources.
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
                        Close();
                        SaveSettings();

                        if ((object)m_rolloverWaitHandle != null)
                            m_rolloverWaitHandle.Close();

                        if ((object)m_conserveMemoryTimer != null)
                        {
                            m_conserveMemoryTimer.Elapsed -= ConserveMemoryTimer_Elapsed;
                            m_conserveMemoryTimer.Dispose();
                        }

                        if ((object)m_currentDataQueue != null)
                        {
                            m_currentDataQueue.ProcessException -= CurrentDataQueue_ProcessException;
                            m_currentDataQueue.Dispose();
                        }

                        if ((object)m_historicDataQueue != null)
                        {
                            m_historicDataQueue.ProcessException -= HistoricDataQueue_ProcessException;
                            m_historicDataQueue.Dispose();
                        }

                        if ((object)m_outOfSequenceDataQueue != null)
                        {
                            m_outOfSequenceDataQueue.ProcessException -= OutOfSequenceDataQueue_ProcessException;
                            m_outOfSequenceDataQueue.Dispose();
                        }

                        if ((object)m_currentLocationFileWatcher != null)
                        {
                            m_currentLocationFileWatcher.Renamed -= FileWatcher_Renamed;
                            m_currentLocationFileWatcher.Deleted -= FileWatcher_Deleted;
                            m_currentLocationFileWatcher.Created -= FileWatcher_Created;
                            m_currentLocationFileWatcher.Dispose();
                        }

                        if ((object)m_offloadLocationFileWatcher != null)
                        {
                            m_offloadLocationFileWatcher.Renamed -= FileWatcher_Renamed;
                            m_offloadLocationFileWatcher.Deleted -= FileWatcher_Deleted;
                            m_offloadLocationFileWatcher.Created -= FileWatcher_Created;
                            m_offloadLocationFileWatcher.Dispose();
                        }

                        // Detach from all of the dependency files.
                        StateFile = null;
                        MetadataFile = null;
                        IntercomFile = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        #region [ Helper Methods ]

        private void ReOpen()
        {
            if (IsOpen)
            {
                Close();
                Open();
            }
        }

        internal void OpenStream()
        {
            try
            {
                if (File.Exists(m_fileName))
                {
                    int attempts = 0;

                    while (true)
                    {
                        try
                        {
                            attempts++;
                            m_fileStream = new FileStream(m_fileName, FileMode.Open, m_fileAccessMode, FileShare.ReadWrite);
                            m_fat = new ArchiveFileAllocationTable(this);
                            break;
                        }
                        catch
                        {
                            m_fileStream?.Dispose();

                            if (attempts >= 4)
                                throw;

                            Thread.Sleep(500);
                        }
                    }
                }
                else if (m_fileAccessMode == FileAccess.Read)
                {
                    // File does not exist, so we create a placeholder file in the temp directory
                    m_fileStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    m_fat = new ArchiveFileAllocationTable(this);
                    m_fat.FileStartTime = TimeTag.MaxValue;
                    m_fat.FileEndTime = TimeTag.MaxValue;

                    // Manually call file monitoring event if file watchers are not enabled
                    if (!m_monitorNewArchiveFiles)
                        FileWatcher_Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FilePath.GetAbsolutePath(m_fileName), FilePath.GetFileName(m_fileName)));
                }
                else
                {
                    // File does not exist, so we have to create it and initialize it.
                    m_fileStream = new FileStream(m_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    m_fat = new ArchiveFileAllocationTable(this);
                    m_fat.Save(true);

                    // Manually call file monitoring event if file watchers are not enabled
                    if (!m_monitorNewArchiveFiles)
                        FileWatcher_Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FilePath.GetAbsolutePath(m_fileName), FilePath.GetFileName(m_fileName)));
                }
            }
            catch
            {
                m_fileStream?.Dispose();
                throw;
            }
        }

        internal void CloseStream()
        {
            m_fat = null;

            if ((object)m_fileStream != null)
            {
                lock (m_fileStream)
                {
                    m_fileStream.Flush();
                    m_fileStream.Close();
                    m_fileStream.Dispose();
                }
                m_fileStream = null;
            }
        }

        private void SyncStateFile(object state)
        {
            lock (this)
            {
                if ((object)m_dataBlocks != null && m_stateFile.IsOpen && m_metadataFile.IsOpen &&
                    m_stateFile.FileAccessMode != FileAccess.Read &&
                    m_metadataFile.RecordsOnDisk > m_stateFile.RecordsOnDisk)
                {
                    // Since we have more number of records in the Metadata File than in the State File we'll synchronize
                    // the number of records in both the files (very important) by writing a new record to the State
                    // File with an ID same as the number of records on disk for Metadata File. Doing so will cause the
                    // State File to grow in-memory or on-disk depending on how it's configured.
                    m_stateFile.Write(m_metadataFile.RecordsOnDisk, new StateRecord(m_metadataFile.RecordsOnDisk));
                    m_stateFile.Save();

                    // We synchronize the block list with the number of state records physically present on the disk.
                    lock (m_dataBlocks)
                    {
                        m_dataBlocks.AddRange(new ArchiveDataBlock[m_stateFile.RecordsOnDisk - m_dataBlocks.Count]);
                    }
                }
            }
        }

        private void BuildHistoricFileList()
        {
            if ((object)m_historicArchiveFiles == null)
            {
                // The list of historic files has not been created, so we'll create it.
                try
                {
                    m_historicArchiveFiles = new List<Info>();

                    OnHistoricFileListBuildStart();

                    // We can safely assume that we'll always get information about the historic file because, the
                    // the search pattern ensures that we only can a list of historic archive files and not all files.
                    Info historicFileInfo;

                    // Prevent the historic file list from being updated by the file watchers.
                    lock (m_historicArchiveFiles)
                    {
                        foreach (string historicFileName in Directory.GetFiles(FilePath.GetDirectoryName(m_fileName), HistoricFilesSearchPattern))
                        {
                            historicFileInfo = GetHistoricFileInfo(historicFileName);
                            if ((object)historicFileInfo != null)
                            {
                                m_historicArchiveFiles.Add(historicFileInfo);
                            }
                        }

                        if (Directory.Exists(m_archiveOffloadLocation))
                        {
                            foreach (string historicFileName in Directory.GetFiles(m_archiveOffloadLocation, HistoricFilesSearchPattern))
                            {
                                historicFileInfo = GetHistoricFileInfo(historicFileName);
                                if ((object)historicFileInfo != null)
                                {
                                    m_historicArchiveFiles.Add(historicFileInfo);
                                }
                            }
                        }
                    }

                    OnHistoricFileListBuildComplete();
                }
                catch (ThreadAbortException)
                {
                    // This thread must die now...
                }
                catch (Exception ex)
                {
                    OnHistoricFileListBuildException(ex);
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Implementation is correct")]
        private void PrepareForRollover()
        {
            try
            {
                DriveInfo archiveDrive = new DriveInfo(Path.GetPathRoot(m_fileName).ToNonNullString());

                // We'll start offloading historic files if we've reached the offload threshold.
                if (m_archiveOffloadMaxAge > 0)
                    OffloadMaxAgedFiles();

                if (archiveDrive.AvailableFreeSpace < archiveDrive.TotalSize * (1 - (m_archiveOffloadThreshold / 100)))
                    OffloadHistoricFiles();

                // Maintain maximum number of historic files, if configured to do so
                MaintainMaximumNumberOfHistoricFiles();

                OnRolloverPreparationStart();

                // Opening and closing a new archive file in "standby" mode will create a "standby" archive file.
                ArchiveFile standbyArchiveFile = new ArchiveFile();
                standbyArchiveFile.FileName = StandbyArchiveFileName;
                standbyArchiveFile.FileSize = m_fileSize;
                standbyArchiveFile.DataBlockSize = m_dataBlockSize;
                standbyArchiveFile.StateFile = m_stateFile;
                standbyArchiveFile.IntercomFile = m_intercomFile;
                standbyArchiveFile.MetadataFile = m_metadataFile;

                try
                {
                    standbyArchiveFile.Open();
                }
                catch
                {
                    string standbyFileName = standbyArchiveFile.FileName;
                    standbyArchiveFile.Close();

                    // We didn't succeed in creating a "standby" archive file, so we'll delete it if it was created
                    // partially (might happen if there isn't enough disk space or thread is aborted). This is to
                    // ensure that this preparation processes is kicked off again until a valid "standby" archive
                    // file is successfully created.
                    DeleteFile(standbyFileName);

                    throw; // Rethrow the exception so the appropriate action is taken.
                }
                finally
                {
                    standbyArchiveFile.Dispose();
                }

                OnRolloverPreparationComplete();
            }
            catch (ThreadAbortException)
            {
                // This thread must die now...
            }
            catch (Exception ex)
            {
                OnRolloverPreparationException(ex);
            }
        }

        private void OffloadMaxAgedFiles()
        {
            if (Directory.Exists(m_archiveOffloadLocation))
            {
                // Wait until the historic file list has been built.
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                try
                {
                    OnOffloadStart();

                    // The offload path that is specified is a valid one so we'll gather a list of all historic
                    // files in the directory where the current (active) archive file is located.
                    List<Info> newHistoricFiles;

                    lock (m_historicArchiveFiles)
                    {
                        newHistoricFiles = m_historicArchiveFiles.FindAll(info => IsNewHistoricArchiveFile(info, m_fileName));
                    }

                    // Sorting the list will sort the historic files from oldest to newest.
                    newHistoricFiles.Sort();

                    List<Info> filesToOffload = new List<Info>();

                    foreach (Info file in newHistoricFiles)
                    {
                        if ((DateTime.UtcNow - file.StartTimeTag.ToDateTime()).TotalDays > m_archiveOffloadMaxAge)
                            filesToOffload.Add(file);
                    }

                    // We'll offload the specified number of oldest historic files to the offload location if the
                    // number of historic files is more than the offload count or all of the historic files if the
                    // offload count is smaller the available number of historic files.
                    ProcessProgress<int> offloadProgress = new ProcessProgress<int>("FileOffload");

                    offloadProgress.Total = filesToOffload.Count;

                    for (int i = 0; i < offloadProgress.Total; i++)
                    {
                        // Don't attempt to offload active file
                        if (string.Compare(filesToOffload[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            string destinationFileName = FilePath.AddPathSuffix(m_archiveOffloadLocation) + FilePath.GetFileName(filesToOffload[i].FileName);

                            try
                            {
                                DeleteFile(destinationFileName);
                                MoveFile(filesToOffload[i].FileName, destinationFileName);
                            }
                            catch (Exception ex)
                            {
                                OnOffloadException(new InvalidOperationException(string.Format("Failed to offload historic file \"{0}\": {1}", FilePath.GetFileName(filesToOffload[i].FileName), ex.Message), ex));
                            }

                            offloadProgress.Complete++;
                            offloadProgress.ProgressMessage = FilePath.GetFileName(filesToOffload[i].FileName);

                            OnOffloadProgress(offloadProgress);
                        }
                    }

                    OnOffloadComplete();
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    OnOffloadException(ex);
                }
            }
            else if (string.Compare(m_archiveOffloadLocation.ToNonNullString().Trim(), "*DELETE*", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Handle case where user has requested historic files be deleted instead of offloaded

                // Wait until the historic file list has been built.
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                try
                {
                    OnOffloadStart();

                    // Get a local copy of all the historic archive files.
                    List<Info> allHistoricFiles;

                    lock (m_historicArchiveFiles)
                    {
                        allHistoricFiles = new List<Info>(m_historicArchiveFiles);
                    }

                    // Start deleting historic files from oldest to newest.
                    allHistoricFiles.Sort();

                    List<Info> filesToDelete = new List<Info>();

                    foreach (Info file in allHistoricFiles)
                    {
                        if ((DateTime.UtcNow - file.StartTimeTag.ToDateTime()).TotalDays > m_archiveOffloadMaxAge)
                            filesToDelete.Add(file);
                    }

                    ProcessProgress<int> offloadProgress = new ProcessProgress<int>("FileOffload");
                    offloadProgress.Total = filesToDelete.Count;

                    for (int i = 0; i < filesToDelete.Count; i++)
                    {
                        // Don't attempt to offload active file
                        if (string.Compare(filesToDelete[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            try
                            {
                                DeleteFile(filesToDelete[i].FileName);
                            }
                            catch (Exception ex)
                            {
                                OnOffloadException(new InvalidOperationException(string.Format("Failed to remove historic file \"{0}\": {1}", FilePath.GetFileName(filesToDelete[i].FileName), ex.Message), ex));
                            }
                        }

                        offloadProgress.Complete++;
                        offloadProgress.ProgressMessage = FilePath.GetFileName(filesToDelete[i].FileName);

                        OnOffloadProgress(offloadProgress);
                    }

                    OnOffloadComplete();
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    OnOffloadException(ex);
                }
            }
        }

        private void OffloadHistoricFiles()
        {
            if (Directory.Exists(m_archiveOffloadLocation))
            {
                // Wait until the historic file list has been built.
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                try
                {
                    OnOffloadStart();

                    // The offload path that is specified is a valid one so we'll gather a list of all historic
                    // files in the directory where the current (active) archive file is located.
                    List<Info> newHistoricFiles;

                    lock (m_historicArchiveFiles)
                    {
                        newHistoricFiles = m_historicArchiveFiles.FindAll(info => IsNewHistoricArchiveFile(info, m_fileName));
                    }

                    // Sorting the list will sort the historic files from oldest to newest.
                    newHistoricFiles.Sort();

                    // We'll offload the specified number of oldest historic files to the offload location if the
                    // number of historic files is more than the offload count or all of the historic files if the
                    // offload count is smaller the available number of historic files.
                    ProcessProgress<int> offloadProgress = new ProcessProgress<int>("FileOffload");

                    offloadProgress.Total = (newHistoricFiles.Count < m_archiveOffloadCount ? newHistoricFiles.Count : m_archiveOffloadCount);

                    for (int i = 0; i < offloadProgress.Total; i++)
                    {
                        // Don't attempt to offload active file
                        if (string.Compare(newHistoricFiles[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            string destinationFileName = FilePath.AddPathSuffix(m_archiveOffloadLocation) + FilePath.GetFileName(newHistoricFiles[i].FileName);

                            try
                            {
                                DeleteFile(destinationFileName);
                                MoveFile(newHistoricFiles[i].FileName, destinationFileName);
                            }
                            catch (Exception ex)
                            {
                                OnOffloadException(new InvalidOperationException(string.Format("Failed to offload historic file \"{0}\": {1}", FilePath.GetFileName(newHistoricFiles[i].FileName), ex.Message), ex));
                            }

                            offloadProgress.Complete++;
                            offloadProgress.ProgressMessage = FilePath.GetFileName(newHistoricFiles[i].FileName);

                            OnOffloadProgress(offloadProgress);
                        }
                    }

                    OnOffloadComplete();
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    OnOffloadException(ex);
                }
            }
            else if (string.Compare(m_archiveOffloadLocation.ToNonNullString().Trim(), "*DELETE*", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Handle case where user has requested historic files be deleted instead of offloaded

                // Wait until the historic file list has been built.
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                try
                {
                    OnOffloadStart();

                    // Get a local copy of all the historic archive files.
                    List<Info> allHistoricFiles;

                    lock (m_historicArchiveFiles)
                    {
                        allHistoricFiles = new List<Info>(m_historicArchiveFiles);
                    }

                    // Determine total number of files to remove
                    int filesToDelete = Common.Min(m_archiveOffloadCount, allHistoricFiles.Count);

                    ProcessProgress<int> offloadProgress = new ProcessProgress<int>("FileOffload");
                    offloadProgress.Total = filesToDelete;

                    // Start deleting historic files from oldest to newest.
                    allHistoricFiles.Sort();

                    for (int i = 0; i < filesToDelete; i++)
                    {
                        // Don't attempt to offload active file
                        if (string.Compare(allHistoricFiles[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            try
                            {
                                DeleteFile(allHistoricFiles[i].FileName);
                            }
                            catch (Exception ex)
                            {
                                OnOffloadException(new InvalidOperationException(string.Format("Failed to remove historic file \"{0}\": {1}", FilePath.GetFileName(allHistoricFiles[i].FileName), ex.Message), ex));
                            }
                        }

                        offloadProgress.Complete++;
                        offloadProgress.ProgressMessage = FilePath.GetFileName(allHistoricFiles[i].FileName);

                        OnOffloadProgress(offloadProgress);
                    }

                    OnOffloadComplete();
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    OnOffloadException(ex);
                }
            }
        }

        private void MaintainMaximumNumberOfHistoricFiles()
        {
            if (m_maxHistoricArchiveFiles >= 1)
            {
                // Wait until the historic file list has been built.
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                // Get a local copy of all the historic archive files.
                List<Info> allHistoricFiles;

                lock (m_historicArchiveFiles)
                {
                    allHistoricFiles = new List<Info>(m_historicArchiveFiles);
                }

                // Start deleting historic files from oldest to newest.
                if (allHistoricFiles.Count > m_maxHistoricArchiveFiles)
                {
                    allHistoricFiles.Sort();

                    while (allHistoricFiles.Count > m_maxHistoricArchiveFiles)
                    {
                        try
                        {
                            DeleteFile(allHistoricFiles[0].FileName);
                        }
                        catch (Exception ex)
                        {
                            OnOffloadException(new InvalidOperationException(string.Format("Failed during attempt to maintain maximum number of historic files - file \"{0}\" could not be deleted: {1}", FilePath.GetFileName(allHistoricFiles[0].FileName), ex.Message), ex));
                        }
                        finally
                        {
                            allHistoricFiles.RemoveAt(0);
                        }
                    }
                }
            }
        }

        private Info GetHistoricFileInfo(string fileName)
        {
            Info fileInfo = null;

            try
            {
                // Validate the file name to determine whether the given file is actually a historic file
                if (!Regex.IsMatch(fileName, string.Format(".+_.+_to_.+\\{0}$", FileExtension)))
                    return null;

                if (File.Exists(fileName))
                {
                    // We'll open the file and get relevant information about it.
                    ArchiveFile historicArchiveFile = new ArchiveFile();

                    historicArchiveFile.FileName = fileName;
                    historicArchiveFile.StateFile = m_stateFile;
                    historicArchiveFile.IntercomFile = m_intercomFile;
                    historicArchiveFile.MetadataFile = m_metadataFile;
                    historicArchiveFile.FileAccessMode = FileAccess.Read;

                    try
                    {
                        historicArchiveFile.Open();

                        fileInfo = new Info(fileName)
                        {
                            StartTimeTag = historicArchiveFile.Fat.FileStartTime,
                            EndTimeTag = historicArchiveFile.Fat.FileEndTime
                        };
                    }
                    catch (Exception ex)
                    {
                        OnHistoricFileListBuildException(new InvalidOperationException(string.Format("Failed to open historic data file \"{0}\" due to exception: {1}", FilePath.GetFileName(fileName), ex.Message), ex));
                    }
                    finally
                    {
                        historicArchiveFile.Dispose();
                    }
                }
                else
                {
                    // We'll resolve to getting the file information from its name only if the file no longer exists
                    // at the location. This will be the case when file is moved to a different location. In this
                    // case the file information we provide is only as good as the file name.
                    string datesString = FilePath.GetFileNameWithoutExtension(fileName).Substring((FilePath.GetFileNameWithoutExtension(m_fileName) + "_").Length);
                    string[] fileStartEndDates = datesString.Split(new string[] { "_to_" }, StringSplitOptions.None);

                    fileInfo = new Info(fileName);

                    if (fileStartEndDates.Length == 2)
                    {
                        fileInfo.StartTimeTag = new TimeTag(Convert.ToDateTime(fileStartEndDates[0].Replace('!', ':')));
                        fileInfo.EndTimeTag = new TimeTag(Convert.ToDateTime(fileStartEndDates[1].Replace('!', ':')));
                    }
                }
            }
            catch (Exception ex)
            {
                OnHistoricFileListBuildException(new InvalidOperationException(string.Format("Failed during information access attempt for historic data file \"{0}\" due to exception: {1}", FilePath.GetFileName(fileName), ex.Message), ex));
            }

            return fileInfo;
        }

        #endregion

        #region [ Find Predicates ]

        private bool FindHistoricArchiveFileForRead(Info fileInfo, TimeTag startTime, TimeTag endTime)
        {
            return ((object)fileInfo != null &&
                ((startTime.CompareTo(fileInfo.StartTimeTag) >= 0 && startTime.CompareTo(fileInfo.EndTimeTag) <= 0) ||
                (endTime.CompareTo(fileInfo.StartTimeTag) >= 0 && endTime.CompareTo(fileInfo.EndTimeTag) <= 0) ||
                (startTime.CompareTo(fileInfo.StartTimeTag) < 0 && endTime.CompareTo(fileInfo.EndTimeTag) > 0)));
        }

        private bool FindHistoricArchiveFileForWrite(Info fileInfo, TimeTag searchTime)
        {
            return ((object)fileInfo != null &&
                    searchTime.CompareTo(fileInfo.StartTimeTag) >= 0 &&
                    searchTime.CompareTo(fileInfo.EndTimeTag) <= 0);
        }

        // Determines if the historic file is in the primary archive location (not offloaded).
        private bool IsNewHistoricArchiveFile(Info fileInfo, string fileName)
        {
            return ((object)fileInfo != null &&
                    string.Compare(FilePath.GetDirectoryName(fileName), FilePath.GetDirectoryName(fileInfo.FileName), StringComparison.OrdinalIgnoreCase) == 0);
        }

        #endregion

        #region [ Queue Delegates ]

        private void WriteToCurrentArchiveFile(IDataPoint[] items)
        {
            Dictionary<int, List<IDataPoint>> sortedDataPoints = new Dictionary<int, List<IDataPoint>>();
            Exception ex;

            // First we'll separate all point data by ID.
            for (int i = 0; i < items.Length; i++)
            {
                if (!sortedDataPoints.ContainsKey(items[i].HistorianID))
                    sortedDataPoints.Add(items[i].HistorianID, new List<IDataPoint>());

                sortedDataPoints[items[i].HistorianID].Add(items[i]);
            }

            IntercomRecord system = m_intercomFile.Read(1);

            foreach (int pointID in sortedDataPoints.Keys)
            {
                // Initialize local variables.
                StateRecord state = m_stateFile.Read(pointID);
                MetadataRecord metadata = m_metadataFile.Read(pointID);
                IDataPoint dataPoint;

                for (int i = 0; i < sortedDataPoints[pointID].Count; i++)
                {
                    dataPoint = sortedDataPoints[pointID][i];

                    // Ensure that the received data is to be archived.
                    if ((object)state == null || (object)metadata == null || !metadata.GeneralFlags.Enabled)
                    {
                        OnOrphanDataReceived(dataPoint);
                        continue;
                    }

                    // Ensure that data is not far out in to the future.
                    if (dataPoint.Time > DateTime.UtcNow.AddMinutes(m_leadTimeTolerance))
                    {
                        OnFutureDataReceived(dataPoint);
                        continue;
                    }

                    // Perform quality check if data quality is not set.
                    if ((int)dataPoint.Quality == 31)
                    {
                        // Note: Here we're checking if the Quality is 31 instead of -1 because the quality value is stored
                        // in the first 5 bits (QualityMask = 31) of Flags in the point data. Initially when the Quality is
                        // set to -1, all the bits Flags (a 32-bit integer) are set to 1. And therefore, when we get the
                        // Quality, which is a masked value of Flags, we get 31 and not -1.
                        switch (metadata.GeneralFlags.DataType)
                        {
                            case DataType.Analog:
                                if (dataPoint.Value >= metadata.AnalogFields.HighRange)
                                    dataPoint.Quality = Quality.UnreasonableHigh;
                                else if (dataPoint.Value >= metadata.AnalogFields.HighAlarm)
                                    dataPoint.Quality = Quality.ValueAboveHiHiAlarm;
                                else if (dataPoint.Value >= metadata.AnalogFields.HighWarning)
                                    dataPoint.Quality = Quality.ValueAboveHiAlarm;
                                else if (dataPoint.Value <= metadata.AnalogFields.LowRange)
                                    dataPoint.Quality = Quality.UnreasonableLow;
                                else if (dataPoint.Value <= metadata.AnalogFields.LowAlarm)
                                    dataPoint.Quality = Quality.ValueBelowLoLoAlarm;
                                else if (dataPoint.Value <= metadata.AnalogFields.LowWarning)
                                    dataPoint.Quality = Quality.ValueBelowLoAlarm;
                                else
                                    dataPoint.Quality = Quality.Good;
                                break;
                            case DataType.Digital:
                                if ((int)dataPoint.Value == metadata.DigitalFields.AlarmState)
                                    dataPoint.Quality = Quality.LogicalAlarm;
                                else
                                    dataPoint.Quality = Quality.Good;
                                break;
                        }
                    }

                    // Update information about the latest data point received.
                    if (dataPoint.Time.CompareTo(system.LatestDataTime) > 0)
                    {
                        system.LatestDataID = dataPoint.HistorianID;
                        system.LatestDataTime = dataPoint.Time;
                        m_intercomFile.Write(1, system);
                    }

                    // Check for data that out-of-sequence based on it's time.
                    if (dataPoint.Time.CompareTo(state.PreviousData.Time) <= 0)
                    {
                        if (dataPoint.Time == state.PreviousData.Time)
                        {
                            // Discard data that is an exact duplicate of data in line for archival.
                            if (dataPoint.Value == state.PreviousData.Value && dataPoint.Quality == state.PreviousData.Quality)
                                continue;
                        }
                        else
                        {
                            // Queue out-of-sequence data for processing if it is not be discarded.
                            if (!m_discardOutOfSequenceData)
                                m_outOfSequenceDataQueue.Add(dataPoint);

                            OnOutOfSequenceDataReceived(dataPoint);
                            continue;
                        }
                    }

                    // [BEGIN]   Data compression
                    bool archiveData = false;
                    bool calculateSlopes = false;
                    float compressionLimit = metadata.AnalogFields.CompressionLimit;

                    // Set the compression limit to a very low number for digital points.
                    if (metadata.GeneralFlags.DataType == DataType.Digital)
                        compressionLimit = 0.000000001f;

                    state.CurrentData = new StateRecordDataPoint(dataPoint);

                    if (state.ArchivedData.IsEmpty)
                    {
                        // This is the first time data is received.
                        state.CurrentData = new StateRecordDataPoint(-1);
                        archiveData = true;
                    }
                    else if (state.PreviousData.IsEmpty)
                    {
                        // This is the second time data is received.
                        calculateSlopes = true;
                    }
                    else
                    {
                        // Process quality-based alarming if enabled.
                        if (metadata.GeneralFlags.AlarmEnabled)
                        {
                            if (metadata.AlarmFlags.Value.CheckBits(BitExtensions.BitVal((int)state.CurrentData.Quality)))
                            {
                                // Current data quality warrants alarming based on the alarming settings.
                                decimal delay = 0;
                                switch (metadata.GeneralFlags.DataType)
                                {
                                    case DataType.Analog:
                                        delay = (decimal)metadata.AnalogFields.AlarmDelay;
                                        break;
                                    case DataType.Digital:
                                        delay = (decimal)metadata.DigitalFields.AlarmDelay;
                                        break;
                                }

                                // Dispatch the alarm immediately or after a given time based on settings.
                                if (delay > 0)
                                {
                                    // Wait before dispatching alarm.
                                    decimal first;
                                    if (m_delayedAlarmProcessing.TryGetValue(dataPoint.HistorianID, out first))
                                    {
                                        if (state.CurrentData.Time.Value - first > delay)
                                        {
                                            // Wait is now over, dispatch the alarm.
                                            m_delayedAlarmProcessing.Remove(dataPoint.HistorianID);
                                            OnProcessAlarmNotification(state);
                                        }
                                    }
                                    else
                                    {
                                        m_delayedAlarmProcessing.Add(state.HistorianID, state.CurrentData.Time.Value);
                                    }
                                }
                                else
                                {
                                    // Dispatch the alarm immediately.
                                    OnProcessAlarmNotification(state);
                                }
                            }
                            else
                            {
                                m_delayedAlarmProcessing.Remove(dataPoint.HistorianID);
                            }
                        }

                        if (m_compressData)
                        {
                            // Data is to be compressed.
                            if (metadata.CompressionMinTime > 0 && state.CurrentData.Time.Value - state.ArchivedData.Time.Value < metadata.CompressionMinTime)
                            {
                                // CompressionMinTime is in effect.
                                archiveData = false;
                                calculateSlopes = false;
                            }
                            else if (state.CurrentData.Quality != state.ArchivedData.Quality || state.CurrentData.Quality != state.PreviousData.Quality || (metadata.CompressionMaxTime > 0 && state.PreviousData.Time.Value - state.ArchivedData.Time.Value > metadata.CompressionMaxTime))
                            {
                                // Quality changed or CompressionMaxTime is exceeded.
                                dataPoint = new ArchiveDataPoint(state.PreviousData);
                                archiveData = true;
                                calculateSlopes = true;
                            }
                            else
                            {
                                // Perform a compression test.
                                double slope1;
                                double slope2;
                                double currentSlope;

                                slope1 = (state.CurrentData.Value - (state.ArchivedData.Value + compressionLimit)) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                                slope2 = (state.CurrentData.Value - (state.ArchivedData.Value - compressionLimit)) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                                currentSlope = (state.CurrentData.Value - state.ArchivedData.Value) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);

                                if (slope1 >= state.Slope1)
                                    state.Slope1 = slope1;

                                if (slope2 <= state.Slope2)
                                    state.Slope2 = slope2;

                                if (currentSlope <= state.Slope1 || currentSlope >= state.Slope2)
                                {
                                    dataPoint = new ArchiveDataPoint(state.PreviousData);
                                    archiveData = true;
                                    calculateSlopes = true;
                                }
                            }
                        }
                        else
                        {
                            // Data is not to be compressed.
                            dataPoint = new ArchiveDataPoint(state.PreviousData);
                            archiveData = true;
                        }
                    }
                    // [END]     Data compression

                    // [BEGIN]   Data archival
                    m_fat.DataPointsReceived++;

                    if (archiveData)
                    {
                        if (dataPoint.Time.CompareTo(m_fat.FileStartTime) >= 0)
                        {
                            // Data belongs to this file.
                            ArchiveDataBlock dataBlock;
                            lock (m_dataBlocks)
                            {
                                dataBlock = m_dataBlocks[dataPoint.HistorianID - 1];
                            }

                            if ((object)dataBlock == null || dataBlock.SlotsAvailable == 0)
                            {
                                // Need to find a data block for writting the data.
                                if ((object)dataBlock != null)
                                    state.ActiveDataBlockIndex = -1;

                                if (state.ActiveDataBlockIndex >= 0)
                                {
                                    // Retrieve previously used data block.
                                    dataBlock = m_fat.RequestDataBlock(dataPoint.HistorianID, dataPoint.Time, state.ActiveDataBlockIndex);
                                }
                                else
                                {
                                    // Time to request a brand new data block.
                                    dataBlock = m_fat.RequestDataBlock(dataPoint.HistorianID, dataPoint.Time, system.DataBlocksUsed);
                                }

                                if ((object)dataBlock != null)
                                {
                                    // Update the total number of data blocks used.
                                    if (dataBlock.SlotsUsed == 0 && system.DataBlocksUsed == dataBlock.Index)
                                    {
                                        system.DataBlocksUsed++;
                                        m_intercomFile.Write(1, system);
                                    }

                                    // Update the active data block index information.
                                    state.ActiveDataBlockIndex = dataBlock.Index;
                                }

                                // Keep in-memory reference to the data block for consecutive writes.
                                lock (m_dataBlocks)
                                {
                                    m_dataBlocks[dataPoint.HistorianID - 1] = dataBlock;
                                }

                                // Kick-off the rollover preparation when its threshold is reached.
                                if (Statistics.FileUsage >= m_rolloverPreparationThreshold && !File.Exists(StandbyArchiveFileName) && !m_rolloverPreparationThread.IsAlive)
                                {
                                    m_rolloverPreparationThread = new Thread(PrepareForRollover);
                                    m_rolloverPreparationThread.Priority = ThreadPriority.Lowest;
                                    m_rolloverPreparationThread.Start();
                                }
                            }

                            if ((object)dataBlock != null)
                            {
                                // Write data to the data block.
                                if (dataBlock.Write(dataPoint, out ex))
                                    m_fat.DataPointsArchived++;
                                else
                                    OnDataWriteException(ex);
                            }
                            else
                            {
                                OnFileFull();   // Current file is full.

                                m_fat.DataPointsReceived--;
                                while (true)
                                {
                                    Rollover(); // Rollover current file.
                                    if (m_rolloverWaitHandle.WaitOne(1, false))
                                        break;  // Rollover is successful.
                                }

                                i--;                                // Process current data point again.
                                system = m_intercomFile.Read(1);    // Re-read modified intercom record.
                                continue;
                            }
                        }
                        else
                        {
                            // Data is historic.
                            m_fat.DataPointsReceived--;
                            m_historicDataQueue.Add(dataPoint);
                            OnHistoricDataReceived(dataPoint);
                        }

                        state.ArchivedData = new StateRecordDataPoint(dataPoint);
                    }

                    if (calculateSlopes)
                    {
                        if (state.CurrentData.Time.Value != state.ArchivedData.Time.Value)
                        {
                            state.Slope1 = (state.CurrentData.Value - (state.ArchivedData.Value + compressionLimit)) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                            state.Slope2 = (state.CurrentData.Value - (state.ArchivedData.Value - compressionLimit)) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                        }
                        else
                        {
                            state.Slope1 = 0;
                            state.Slope2 = 0;
                        }
                    }

                    state.PreviousData = state.CurrentData;

                    // Write state information to the file.
                    m_stateFile.Write(state.HistorianID, state);
                    // [END]     Data archival
                }
            }
        }

        private void WriteToHistoricArchiveFile(IDataPoint[] items)
        {
            // Wait until the historic file list has been built.
            if (m_buildHistoricFileListThread.IsAlive)
                m_buildHistoricFileListThread.Join();

            Dictionary<Info, Dictionary<int, List<IDataPoint>>> historicFileData = new Dictionary<Info, Dictionary<int, List<IDataPoint>>>();

            // Separate all point data into bins by historic file and point ID
            foreach (IDataPoint dataPoint in items) {
                try
                {
                    Info historicFileInfo;
                    Dictionary<int, List<IDataPoint>> sortedPointData;
                    List<IDataPoint> pointData;

                    lock (m_historicArchiveFiles)
                    {
                        // Attempt to find a historic archive file where the data point belongs
                        historicFileInfo = m_historicArchiveFiles.Find(info => FindHistoricArchiveFileForWrite(info, dataPoint.Time));
                    }

                    // If a historic file exists, sort the data point into the proper bin
                    if ((object)historicFileInfo != null)
                    {
                        sortedPointData = historicFileData.GetOrAdd(historicFileInfo, info => new Dictionary<int, List<IDataPoint>>());
                        pointData = sortedPointData.GetOrAdd(dataPoint.HistorianID, id => new List<IDataPoint>());
                        pointData.Add(dataPoint);
                    }
                }
                catch (Exception ex)
                {
                    // Notify of the exception
                    OnDataWriteException(ex);
                }
            }

            foreach (Info historicFileInfo in historicFileData.Keys)
            {
                Dictionary<int, List<IDataPoint>> sortedPointData = historicFileData[historicFileInfo];
                int overflowBlocks = 0;

                using (ArchiveFile historicFile = new ArchiveFile())
                {
                    try
                    {
                        // Open the historic file
                        historicFile.FileName = historicFileInfo.FileName;
                        historicFile.StateFile = m_stateFile;
                        historicFile.IntercomFile = m_intercomFile;
                        historicFile.MetadataFile = m_metadataFile;
                        historicFile.Open();
                    }
                    catch (Exception ex)
                    {
                        // Notify of the exception
                        OnDataWriteException(ex);

                        // If we fail to open the historic file,
                        // then we cannot write any of these data
                        // points to it so we might as well move on
                        continue;
                    }

                    // Calculate the number of additional data blocks needed to store all the data
                    foreach (int pointID in sortedPointData.Keys)
                    {
                        try
                        {
                            ArchiveDataBlock lastDataBlock = historicFile.Fat.FindLastDataBlock(pointID);
                            int blockCapacity = historicFile.DataBlockSize * 1024 / ArchiveDataPoint.FixedLength;
                            int overflowPoints = sortedPointData[pointID].Count + (lastDataBlock?.SlotsUsed ?? 0) - (lastDataBlock?.Capacity ?? 0);
                            overflowBlocks += (overflowPoints + blockCapacity - 1) / blockCapacity;
                        }
                        catch (Exception ex)
                        {
                            // Notify of the exception
                            OnDataWriteException(ex);
                        }
                    }

                    try
                    {
                        // Extend the file by the needed amount
                        if (overflowBlocks > 0)
                            historicFile.Fat.Extend(overflowBlocks);
                    }
                    catch (Exception ex)
                    {
                        // Notify of the exception
                        OnDataWriteException(ex);
                    }

                    foreach (int pointID in sortedPointData.Keys)
                    {
                        try
                        {
                            ArchiveDataBlock historicFileBlock = null;

                            // Sort the point data for the current point ID by time
                            sortedPointData[pointID].Sort();

                            foreach (IDataPoint dataPoint in sortedPointData[pointID])
                            {
                                if ((object)historicFileBlock == null || historicFileBlock.SlotsAvailable == 0)
                                {
                                    // Request a new or previously used data block for point data
                                    historicFileBlock = historicFile.Fat.RequestDataBlock(pointID, dataPoint.Time, -1);
                                }

                                // Write the data point into the data block
                                Exception ex;

                                if (historicFileBlock.Write(dataPoint, out ex))
                                {
                                    historicFile.Fat.DataPointsReceived++;
                                    historicFile.Fat.DataPointsArchived++;
                                }
                                else
                                {
                                    // Suppress exceptions related to bad timestamps - this handles the case where the system is attempting to write
                                    // a historical point with a timestamp that is less than 01/01/1995, the minimum value for a TimeTag instance
                                    if (!(ex is TimeTagException))
                                        OnDataWriteException(ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Notify of the exception
                            OnDataWriteException(ex);
                        }
                    }

                    try
                    {
                        // Save the file after all
                        // data has been written to it
                        historicFile.Save();
                    }
                    catch (Exception ex)
                    {
                        // Notify of the exception
                        OnDataWriteException(ex);
                    }
                }
            }
        }

        private void InsertInCurrentArchiveFile(IDataPoint[] items)
        {
            // TODO: Implement archival of out-of-sequence data.
        }

        #endregion

        #region [ Event Handlers ]

        private void MetadataFile_FileModified(object sender, EventArgs e)
        {
            OnMetadataUpdated();
            SyncStateFile(null);
        }

        private void ConserveMemoryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (m_dataBlocks)
            {
                // Go through all data blocks and remove that are inactive.
                for (int i = 0; i < m_dataBlocks.Count; i++)
                {
                    if ((object)m_dataBlocks[i] != null && !m_dataBlocks[i].IsActive)
                    {
                        m_dataBlocks[i] = null;
                        //Trace.WriteLine(string.Format("Inactive block for Point ID {0} disposed", i + 1));
                    }
                }
            }
        }

        private void CurrentDataQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnDataWriteException(e.Argument);
        }

        private void HistoricDataQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnDataWriteException(e.Argument);
        }

        private void OutOfSequenceDataQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnDataWriteException(e.Argument);
        }

        // File.Delete proxy function that will manually invoke file watcher handlers when file watchers are disabled
        private void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            // Manually call file monitoring events if file watchers are not enabled
            if (!m_monitorNewArchiveFiles)
                FileWatcher_Deleted(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, FilePath.GetDirectoryName(FilePath.GetAbsolutePath(fileName)), FilePath.GetFileName(fileName)));
        }

        // File.Move proxy function that will manually invoke file watcher handlers when file watchers are disabled
        private void MoveFile(string sourceFileName, string destinationFileName)
        {
            File.Move(sourceFileName, destinationFileName);

            // Manually call file monitoring events if file watchers are not enabled
            if (!m_monitorNewArchiveFiles)
            {
                if (string.Compare(FilePath.GetDirectoryName(FilePath.GetAbsolutePath(sourceFileName)).Trim(), FilePath.GetDirectoryName(FilePath.GetAbsolutePath(destinationFileName)).Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    //FileWatcher_Renamed(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, FilePath.GetDirectoryName(FilePath.GetAbsolutePath(sourceFileName)), FilePath.GetFileName(sourceFileName), FilePath.GetFileName(destinationFileName)));
                    FileWatcher_Renamed(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, FilePath.GetDirectoryName(FilePath.GetAbsolutePath(destinationFileName)), FilePath.GetFileName(destinationFileName), FilePath.GetFileName(sourceFileName)));
                }
                else
                {
                    FileWatcher_Deleted(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, FilePath.GetDirectoryName(FilePath.GetAbsolutePath(sourceFileName)), FilePath.GetFileName(sourceFileName)));
                    FileWatcher_Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FilePath.GetDirectoryName(FilePath.GetAbsolutePath(destinationFileName)), FilePath.GetFileName(destinationFileName)));
                }
            }
        }

        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if ((object)m_historicArchiveFiles != null)
            {
                bool historicFileListUpdated = false;
                Info historicFileInfo = GetHistoricFileInfo(e.FullPath);

                lock (m_historicArchiveFiles)
                {
                    if ((object)historicFileInfo != null && !m_historicArchiveFiles.Contains(historicFileInfo))
                    {
                        m_historicArchiveFiles.Add(historicFileInfo);
                        historicFileListUpdated = true;
                    }
                }

                if (historicFileListUpdated)
                    OnHistoricFileListUpdated();
            }
        }

        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if ((object)m_historicArchiveFiles != null)
            {
                bool historicFileListUpdated = false;
                Info historicFileInfo = GetHistoricFileInfo(e.FullPath);

                lock (m_historicArchiveFiles)
                {
                    if ((object)historicFileInfo != null && m_historicArchiveFiles.Contains(historicFileInfo))
                    {
                        m_historicArchiveFiles.Remove(historicFileInfo);
                        historicFileListUpdated = true;
                    }
                }

                if (historicFileListUpdated)
                    OnHistoricFileListUpdated();
            }
        }

        private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if ((object)m_historicArchiveFiles != null)
            {
                if (string.Compare(FilePath.GetExtension(e.OldFullPath), FileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    try
                    {
                        bool historicFileListUpdated = false;
                        Info oldFileInfo = GetHistoricFileInfo(e.OldFullPath);

                        lock (m_historicArchiveFiles)
                        {
                            if ((object)oldFileInfo != null && m_historicArchiveFiles.Contains(oldFileInfo))
                            {
                                m_historicArchiveFiles.Remove(oldFileInfo);
                                historicFileListUpdated = true;
                            }
                        }

                        if (historicFileListUpdated)
                            OnHistoricFileListUpdated();
                    }
                    catch
                    {
                        // Ignore any exception we might encounter here if an archive file being renamed to a
                        // historic archive file. This might happen if someone is renaming files manually.
                    }
                }

                if (string.Compare(FilePath.GetExtension(e.FullPath), FileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    try
                    {
                        bool historicFileListUpdated = false;
                        Info newFileInfo = GetHistoricFileInfo(e.FullPath);

                        lock (m_historicArchiveFiles)
                        {
                            if ((object)newFileInfo != null && !m_historicArchiveFiles.Contains(newFileInfo))
                            {
                                m_historicArchiveFiles.Add(newFileInfo);
                                historicFileListUpdated = true;
                            }
                        }

                        if (historicFileListUpdated)
                            OnHistoricFileListUpdated();
                    }
                    catch
                    {
                        // Ignore any exception we might encounter if a historic archive file is being renamed to
                        // something else. This might happen if someone is renaming files manually.
                    }
                }
            }
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Returns the number of <see cref="ArchiveDataBlock"/>s an <see cref="ArchiveFile"/> can have.
        /// </summary>
        /// <param name="fileSize">Size (in MB) of the <see cref="ArchiveFile"/>.</param>
        /// <param name="blockSize">Size (in KB) of the <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.</param>
        /// <returns>A 32-bit signed integer for the number of <see cref="ArchiveDataBlock"/>s an <see cref="ArchiveFile"/> can have.</returns>
        public static int MaximumDataBlocks(double fileSize, int blockSize)
        {
            return (int)((fileSize * 1024) / blockSize);
        }

        #endregion
    }
}
