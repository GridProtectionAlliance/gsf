//*******************************************************************************************************
//  ArchiveFile.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
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
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using TVA.Collections;
using TVA.Configuration;
using TVA.IO;
using TVA.Units;

namespace TVA.Historian.Files
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
    /// Represents a file that contains <see cref="ArchiveData"/>.
    /// </summary>
    /// <seealso cref="ArchiveData"/>
    /// <seealso cref="ArchiveFileAllocationTable"/>
    [ToolboxBitmap(typeof(ArchiveFile))]
    public class ArchiveFile : Component, IArchive, ISupportLifecycle, ISupportInitialize, IPersistSettings
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents information about an <see cref="ArchiveFile"/>.
        /// </summary>
        private class Info : IComparable
        {
            /// <summary>
            /// Name of the <see cref="ArchiveFile"/>.
            /// </summary>
            public string FileName;

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
                if (other == null)
                {
                    return 1;
                }
                else
                {
                    int result = StartTimeTag.CompareTo(other.StartTimeTag);
                    if (result != 0)
                        return result;
                    else
                        return EndTimeTag.CompareTo(other.EndTimeTag);
                }
            }

            public override bool Equals(object obj)
            {
                Info other = obj as Info;
                if (other == null)
                {
                    return false;
                }
                else
                {
                    // We will only compare file name for equality because the result will be incorrent if one of
                    // the ArchiveFileInfo instance is created from the filename by GetHistoricFileInfo() function.
                    return string.Compare(FilePath.GetFileName(FileName), FilePath.GetFileName(other.FileName), true) == 0;
                }
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
        /// Specifies the default value for the <see cref="RolloverOnFull"/> property.
        /// </summary>
        public const bool DefaultRolloverOnFull = true;

        /// <summary>
        /// Specifies the default value for the <see cref="RolloverPreparationThreshold"/> property.
        /// </summary>
        public const int DefaultRolloverPreparationThreshold = 75;

        /// <summary>
        /// Gets or sets the default value for the <see cref="FileOffloadLocation"/> property.
        /// </summary>
        public const string DefaultFileOffloadLocation = "";

        /// <summary>
        /// Specifies the default value for the <see cref="FileOffloadCount"/> property.
        /// </summary>
        public const int DefaultFileOffloadCount = 5;

        /// <summary>
        /// Specifies the default value for the <see cref="FileOffloadThreshold"/> property.
        /// </summary>
        public const int DefaultFileOffloadThreshold = 90;

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
        /// Occurs when the process of offloading historic <see cref="ArchiveFile"/>s is started.
        /// </summary>
        [Category("Offload"),
        Description("Occurs when the process of offloading historic ArchiveFiles is started.")]
        public event EventHandler OffloadStart;

        /// <summary>
        /// Occurs when the process of offloading historic <see cref="ArchiveFile"/>s is complete.
        /// </summary>
        [Category("Offload"),
        Description("Occurs when the process of offloading historic ArchiveFiles is complete.")]
        public event EventHandler OffloadComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during the historic <see cref="ArchiveFile"/> offload process.
        /// </summary>
        [Category("Offload"),
        Description("Occurs when an Exception is encountered during the historic ArchiveFile offload process.")]
        public event EventHandler<EventArgs<Exception>> OffloadException;

        /// <summary>
        /// Occurs when historic <see cref="ArchiveFile"/>s are being offloaded.
        /// </summary>
        [Category("Offload"),
        Description("Occurs when historic ArchiveFiles are being offloaded.")]
        public event EventHandler<EventArgs<ProcessProgress<int>>> OffloadProgress;

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
        /// Occurs when <see cref="ArchiveData"/> is received whose <see cref="MetadataRecord"/> does not exist or is marked as disabled.
        /// </summary>
        [Category("Data"),
        Description("Occurs when ArchiveData is received whose MetadataRecord does not exist or is marked as disabled.")]
        public event EventHandler<EventArgs<ArchiveData>> OrphanDataReceived;

        /// <summary>
        /// Occurs when <see cref="ArchiveData"/> is received with <see cref="TimeTag"/> ahead of the local clock by more than the <see cref="LeadTimeTolerance"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when ArchiveDatais received with TimeTag ahead of the local clock by more than the LeadTimeTolerance.")]
        public event EventHandler<EventArgs<ArchiveData>> FutureDataReceived;

        /// <summary>
        /// Occurs when <see cref="ArchiveData"/> that belongs to a historic <see cref="ArchiveFile"/> is received for archival.
        /// </summary>
        [Category("Data"),
        Description("Occurs when ArchiveData that belongs to a historic ArchiveFile is received for archival.")]
        public event EventHandler<EventArgs<ArchiveData>> HistoricDataReceived;

        /// <summary>
        /// Occurs when the process of archiving <see cref="ArchiveData"/> to historic <see cref="ArchiveFile"/> is started.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the process of archiving ArchiveData to historic ArchiveFile is started.")]
        public event EventHandler HistoricDataWriteStart;

        /// <summary>
        /// Occurs when the process of archiving <see cref="ArchiveData"/> to historic <see cref="ArchiveFile"/> is complete.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the process of archiving ArchiveData to historic ArchiveFile is complete.")]
        public event EventHandler HistoricDataWriteComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while archiving <see cref="ArchiveData"/> to historic <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while writting ArchiveData to historic ArchiveFile.")]
        public event EventHandler<EventArgs<Exception>> HistoricDataWriteException;

        /// <summary>
        /// Occurs when <see cref="ArchiveData"/> is being archived to historic <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs to when ArchievData is being written to historic ArchiveFile.")]
        public event EventHandler<EventArgs<ProcessProgress<int>>> HistoricDataWriteProgress;

        /// <summary>
        /// Occurs when misaligned <see cref="ArchiveData"/> is received for archival.
        /// </summary>
        [Category("Data"),
        Description("Occurs when misaligned ArchiveData is received for archival.")]
        public event EventHandler<EventArgs<ArchiveData>> OutOfSequenceDataReceived;

        /// <summary>
        /// Occurs when the process of archiving misaligned (by time) <see cref="ArchiveData"/> is started.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the process of archiving misaligned ArchiveData is started.")]
        public event EventHandler OutOfSequenceDataWriteStart;

        /// <summary>
        /// Occurs when the process of archiving misaligned (by time) <see cref="ArchiveData"/> is complete.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the process of archiving misaligned ArchiveData is complete.")]
        public event EventHandler OutOfSequenceDataWriteComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while archiving misaligned (by time) <see cref="ArchiveData"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while archiving misaligned ArchiveData.")]
        public event EventHandler<EventArgs<Exception>> OutOfSequenceDataWriteException;

        /// <summary>
        /// Occurs when misaligned (by time) <see cref="ArchiveData"/> is being archived.
        /// </summary>
        [Category("Data"),
        Description("Occurs when misaligned ArchievData is being archived.")]
        public event EventHandler<EventArgs<ProcessProgress<int>>> OutOfSequenceDataWriteProgress;

        /// <summary>
        /// Occurs when <see cref="ArchiveData"/> triggers an alarm notification.
        /// </summary>
        [Category("File"),
        Description("Occurs when ArchiveData triggers an alarm notification.")]
        public event EventHandler<EventArgs<StateRecord>> ProcessAlarmNotification;

        // Fields

        // Component
        private string m_fileName;
        private ArchiveFileType m_fileType;
        private double m_fileSize;
        private FileAccess m_fileAccessMode;
        private int m_dataBlockSize;
        private bool m_rolloverOnFull;
        private short m_rolloverPreparationThreshold;
        private string m_fileOffloadLocation;
        private int m_fileOffloadCount;
        private short m_fileOffloadThreshold;
        private double m_leadTimeTolerance;
        private bool m_compressData;
        private bool m_discardOutOfSequenceData;
        private bool m_cacheWrites;
        private bool m_conserveMemory;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private ArchiveFileAllocationTable m_fat;
        // Operational
        private bool m_disposed;
        private bool m_initialized;
        private FileStream m_fileStream;
        private List<ArchiveDataBlock> m_dataBlocks;
        private List<Info> m_historicArchiveFiles;
        private Dictionary<int, double> m_delayedAlarmProcessing;
        // Searching
        private TimeTag m_writeSearchTimeTag;
        private TimeTag m_readSearchStartTimeTag;
        private TimeTag m_readSearchEndTimeTag;
        // Threading
        private Thread m_rolloverPreparationThread;
        private Thread m_buildHistoricFileListThread;
        private ManualResetEvent m_rolloverWaitHandle;
        // Components
        private StateFile m_stateFile;
        private IntercomFile m_intercomFile;
        private MetadataFile m_metadataFile;
        private System.Timers.Timer m_conserveMemoryTimer;
        private ProcessQueue<ArchiveData> m_historicDataQueue;
        private ProcessQueue<ArchiveData> m_outOfSequenceDataQueue;
        private FileSystemWatcher m_currentLocationFileWatcher;
        private FileSystemWatcher m_offloadLocationFileWatcher;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFile"/> class.
        /// </summary>
        public ArchiveFile()
            : base()
        {
            m_fileName = DefaultFileName;
            m_fileType = DefaultFileType;
            m_fileSize = DefaultFileSize;
            m_fileAccessMode = DefaultFileAccessMode;
            m_dataBlockSize = DefaultDataBlockSize;
            m_rolloverOnFull = DefaultRolloverOnFull;
            m_rolloverPreparationThreshold = DefaultRolloverPreparationThreshold;
            m_fileOffloadLocation = DefaultFileOffloadLocation;
            m_fileOffloadCount = DefaultFileOffloadCount;
            m_fileOffloadThreshold = DefaultFileOffloadThreshold;
            m_leadTimeTolerance = DefaultLeadTimeTolerance;
            m_compressData = DefaultCompressData;
            m_discardOutOfSequenceData = DefaultDiscardOutOfSequenceData;
            m_cacheWrites = DefaultCacheWrites;
            m_conserveMemory = DefaultConserveMemory;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;

            m_delayedAlarmProcessing = new Dictionary<int, double>();
            m_rolloverWaitHandle = new ManualResetEvent(true);
            m_rolloverPreparationThread = new Thread(PrepareForRollover);
            m_buildHistoricFileListThread = new Thread(BuildHistoricFileList);

            m_conserveMemoryTimer = new System.Timers.Timer(DataBlockCheckInterval);
            m_conserveMemoryTimer.Elapsed += ConserveMemoryTimer_Elapsed;

            m_historicDataQueue = ProcessQueue<ArchiveData>.CreateRealTimeQueue(WriteToHistoricArchiveFile);
            m_historicDataQueue.ProcessException += HistoricDataQueue_ProcessException;

            m_outOfSequenceDataQueue = ProcessQueue<ArchiveData>.CreateRealTimeQueue(InsertInCurrentArchiveFile);
            m_outOfSequenceDataQueue.ProcessException += OutOfSequenceDataQueue_ProcessException;

            m_currentLocationFileWatcher = new FileSystemWatcher();
            m_currentLocationFileWatcher.IncludeSubdirectories = true;
            m_currentLocationFileWatcher.Renamed += FileWatcher_Renamed;
            m_currentLocationFileWatcher.Deleted += FileWatcher_Deleted;
            m_currentLocationFileWatcher.Created += FileWatcher_Created;

            m_offloadLocationFileWatcher = new FileSystemWatcher();
            m_offloadLocationFileWatcher.IncludeSubdirectories = true;
            m_offloadLocationFileWatcher.Renamed += FileWatcher_Renamed;
            m_offloadLocationFileWatcher.Deleted += FileWatcher_Deleted;
            m_offloadLocationFileWatcher.Created += FileWatcher_Created;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ArchiveFile"/>.</param>
        public ArchiveFile(IContainer container)
            : this()
        {
            if (container != null)
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
                    throw new ArgumentNullException();

                if (string.Compare(FilePath.GetExtension(value), FileExtension, true) != 0)
                    throw (new ArgumentException(string.Format("{0} must have an extension of {1}.", this.GetType().Name, FileExtension)));

                m_fileName = value;
                if (IsOpen)
                {
                    Close();
                    Open();
                }
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
                    throw new ArgumentException("Value must be positive.");

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
                if (m_fat == null)
                {
                    // Design time.
                    return m_dataBlockSize;
                }
                else
                {
                    // Run time.
                    return m_fat.DataBlockSize;
                }
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_dataBlockSize = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the current <see cref="ArchiveFile"/> will rollover to a new <see cref="ArchiveFile"/> when full.
        /// </summary>
        [Category("Rollover"),
        DefaultValue(DefaultRolloverOnFull),
        Description("Indicates whether the current ArchiveFile will rollover to a new ArchiveFile when full.")]
        public bool RolloverOnFull
        {
            get
            {
                return m_rolloverOnFull;
            }
            set
            {
                m_rolloverOnFull = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ArchiveFile"/> usage (in %) that will trigger the creation of an empty <see cref="ArchiveFile"/> for rollover.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 1 and 95.</exception>
        [Category("Rollover"),
        DefaultValue(DefaultRolloverPreparationThreshold),
        Description("ArchiveFile usage (in %) that will trigger the creation of an empty ArchiveFile for rollover.")]
        public short RolloverPreparationThreshold
        {
            get
            {
                return m_rolloverPreparationThreshold;
            }
            set
            {
                if (value < 1 || value > 95)
                    throw new ArgumentOutOfRangeException("RolloverPreparationThreshold", "Value must be between 1 and 95.");

                m_rolloverPreparationThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the directory where historic <see cref="ArchiveFile"/>s are to be offloaded to make space in the primary archive location.
        /// </summary>
        [Category("Offload"),
        DefaultValue(DefaultFileOffloadLocation),
        Description("Path to the directory where historic ArchiveFiles are to be offloaded to make space in the primary archive location.")]
        public string FileOffloadLocation
        {
            get
            {
                return m_fileOffloadLocation;
            }
            set
            {
                m_fileOffloadLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of historic <see cref="ArchiveFile"/>s to be offloaded to the <see cref="FileOffloadLocation"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        [Category("Offload"),
        DefaultValue(DefaultFileOffloadCount),
        Description("Number of historic ArchiveFiles to be offloaded to the FileOffloadPath.")]
        public int FileOffloadCount
        {
            get
            {
                return m_fileOffloadCount;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_fileOffloadCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the free disk space (in %) of the primary archive location that triggers the offload of historic <see cref="ArchiveFile"/>s.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is not between 1 and 99.</exception>
        [Category("Offload"),
        DefaultValue(DefaultFileOffloadThreshold),
        Description("Free disk space (in %) of the primary archive location that triggers the offload of historic ArchiveFiles.")]
        public short FileOffloadThreshold
        {
            get
            {
                return m_fileOffloadThreshold;
            }
            set
            {
                if (value < 1 || value > 99)
                    throw new ArgumentOutOfRangeException("FileOffloadThreshold", "Value must be between 1 and 99.");

                m_fileOffloadThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of minutes by which incoming <see cref="ArchiveData"/> can be ahead of local system clock and still be considered valid.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not zero or positive.</exception>
        [Category("Data"),
        DefaultValue(DefaultLeadTimeTolerance),
        Description("Number of minutes by which incoming ArchiveData can be ahead of local system clock and still be considered valid.")]
        public double LeadTimeTolerance
        {
            get
            {
                return m_leadTimeTolerance;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be zero or positive.");

                m_leadTimeTolerance = value;
            }
        }

        /// <summary>
        /// Gets or set a boolean value that indicates whether incoming <see cref="ArchiveData"/> is to be compressed to save space.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultCompressData),
        Description("Indicates whether incoming ArchiveData is to be compressed to save space.")]
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
        /// Gets or sets a boolean value that indicates whether incoming <see cref="ArchiveData"/> with out-of-sequence <see cref="TimeTag"/> is to be discarded.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultDiscardOutOfSequenceData),
        Description("Indicates whether incoming ArchiveData with out-of-sequence TimeTag is to be discarded.")]
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
                if (m_stateFile != null)
                {
                    // Detach events from any existing instance
                    m_stateFile.FileModified -= StateFile_FileModified;
                }

                m_stateFile = value;

                if (m_stateFile != null)
                {
                    // Attach events to new instance
                    m_stateFile.FileModified += StateFile_FileModified;
                }
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
                if (m_metadataFile != null)
                {
                    // Detach events from any existing instance
                    m_metadataFile.FileModified -= MetadataFile_FileModified;
                }

                m_metadataFile = value;

                if (m_stateFile != null)
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
        [Category("Persistance"),
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
        [Category("Persistance"),
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
                    throw new ArgumentNullException();

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
        /// Gets a boolean value that indicates whether the <see cref="ArchiveFile"/> is currently open.
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
        public ArchiveFileStatistics Statistics
        {
            get
            {
                ArchiveFileStatistics statistics = new ArchiveFileStatistics();

                // Calculate file usage.
                if (m_fileType == ArchiveFileType.Active)
                    statistics.FileUsage = ((float)m_intercomFile.Read(1).DataBlocksUsed / (float)m_fat.DataBlockCount) * 100;
                else
                    statistics.FileUsage = ((float)m_fat.DataBlocksUsed / (float)m_fat.DataBlockCount) * 100;

                // Calculate compression rate.
                if (m_fat.DataPointsReceived > 0)
                    statistics.CompressionRate = ((float)(m_fat.DataPointsReceived - m_fat.DataPointsArchived) / (float)m_fat.DataPointsReceived) * 100;
                else
                    statistics.CompressionRate = 0f;

                // Calculate write speed averaging window.
                if (m_fat.FileStartTime != TimeTag.MinValue)
                    statistics.AveragingWindow = new Time((DateTime.UtcNow - m_fat.FileStartTime.ToDateTime()).TotalSeconds);
                else
                    statistics.AveragingWindow = Time.MinValue;

                // Calculate average write speed.
                if (m_fat.DataPointsArchived > 0)
                    statistics.AverageWriteSpeed = m_fat.DataPointsArchived / (int)statistics.AveragingWindow;
                else
                    statistics.AverageWriteSpeed = 0;

                return statistics;
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
                    m_currentLocationFileWatcher.BeginInit();
                    m_offloadLocationFileWatcher.BeginInit();
                }
                catch (Exception)
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
                    m_currentLocationFileWatcher.EndInit();
                    m_offloadLocationFileWatcher.EndInit();
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="ArchiveFile"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
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
                element = settings["FileName"];
                element.Update(m_fileName, element.Description, element.Encrypted);
                element = settings["FileType"];
                element.Update(m_fileType, element.Description, element.Encrypted);
                element = settings["FileSize"];
                element.Update(m_fileSize, element.Description, element.Encrypted);
                element = settings["DataBlockSize"];
                element.Update(m_dataBlockSize, element.Description, element.Encrypted);
                element = settings["RolloverOnFull"];
                element.Update(m_rolloverOnFull, element.Description, element.Encrypted);
                element = settings["RolloverPreparationThreshold"];
                element.Update(m_rolloverPreparationThreshold, element.Description, element.Encrypted);
                element = settings["FileOffloadLocation"];
                element.Update(m_fileOffloadLocation, element.Description, element.Encrypted);
                element = settings["FileOffloadCount"];
                element.Update(m_fileOffloadCount, element.Description, element.Encrypted);
                element = settings["FileOffloadThreshold"];
                element.Update(m_fileOffloadThreshold, element.Description, element.Encrypted);
                element = settings["LeadTimeTolerance"];
                element.Update(m_leadTimeTolerance, element.Description, element.Encrypted);
                element = settings["CompressData"];
                element.Update(m_compressData, element.Description, element.Encrypted);
                element = settings["DiscardOutOfSequenceData"];
                element.Update(m_discardOutOfSequenceData, element.Description, element.Encrypted);
                element = settings["CacheWrites"];
                element.Update(m_cacheWrites, element.Description, element.Encrypted);
                element = settings["ConserveMemory"];
                element.Update(m_conserveMemory, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="ArchiveFile"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
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
                settings.Add("FileName", m_fileName, "Name of the file including its path.");
                settings.Add("FileType", m_fileType, "Type (Active; Standby; Historic) of the file.");
                settings.Add("FileSize", m_fileSize, "Size (in MB) of the file.");
                settings.Add("DataBlockSize", m_dataBlockSize, "Size (in KB) of the data blocks in the file.");
                settings.Add("RolloverOnFull", m_rolloverOnFull, "True if rollover of the file is to be performed when it is full; otherwise False.");
                settings.Add("RolloverPreparationThreshold", m_rolloverPreparationThreshold, "Percentage file full when the rollover preparation should begin.");
                settings.Add("FileOffloadLocation", m_fileOffloadLocation, "Path to the location where historic files are to be moved when disk start getting full.");
                settings.Add("FileOffloadCount", m_fileOffloadCount, "Number of files that are to be moved to the offload location when the disk starts getting full.");
                settings.Add("FileOffloadThreshold", m_fileOffloadThreshold, "Percentage disk full when the historic files should be moved to the offload location.");
                settings.Add("LeadTimeTolerance", m_leadTimeTolerance, "Number of minutes by which incoming data points can be ahead of local system clock and still be considered valid.");
                settings.Add("CompressData", m_compressData, "True if compression is to be performed on the incoming data points; otherwise False.");
                settings.Add("DiscardOutOfSequenceData", m_discardOutOfSequenceData, "True if out-of-sequence data points are to be discarded; otherwise False.");
                settings.Add("CacheWrites", m_cacheWrites, "True if writes are to be cached for performance; otherwise False.");
                settings.Add("ConserveMemory", m_conserveMemory, "True if attempts are to be made to conserve memory; otherwise False.");
                FileName = settings["FileName"].ValueAs(FileName);
                FileType = settings["FileType"].ValueAs(FileType);
                FileSize = settings["FileSize"].ValueAs(FileSize);
                DataBlockSize = settings["DataBlockSize"].ValueAs(DataBlockSize);
                RolloverOnFull = settings["RolloverOnFull"].ValueAs(RolloverOnFull);
                RolloverPreparationThreshold = settings["RolloverPreparationThreshold"].ValueAs(RolloverPreparationThreshold);
                FileOffloadLocation = settings["FileOffloadLocation"].ValueAs(FileOffloadLocation);
                FileOffloadCount = settings["FileOffloadCount"].ValueAs(FileOffloadCount);
                FileOffloadThreshold = settings["FileOffloadThreshold"].ValueAs(FileOffloadThreshold);
                LeadTimeTolerance = settings["LeadTimeTolerance"].ValueAs(LeadTimeTolerance);
                CompressData = settings["CompressData"].ValueAs(CompressData);
                DiscardOutOfSequenceData = settings["DiscardOutOfSequenceData"].ValueAs(DiscardOutOfSequenceData);
                CacheWrites = settings["CacheWrites"].ValueAs(CacheWrites);
                ConserveMemory = settings["ConserveMemory"].ValueAs(ConserveMemory);
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
                // Check for the existance of dependencies.
                if (m_stateFile == null || m_intercomFile == null | m_metadataFile == null)
                    throw (new InvalidOperationException("One or more of the dependency files are not specified."));

                // Change the file name for a standby file.
                if (m_fileType == ArchiveFileType.Standby)
                    m_fileName = StandbyArchiveFileName;
                // Get the absolute path for the file name.
                m_fileName = FilePath.GetAbsolutePath(m_fileName);
                // Create the directory if it does not exist.
                if (!Directory.Exists(FilePath.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(FilePath.GetDirectoryName(m_fileName));

                if (File.Exists(m_fileName))
                {
                    // File has been created already, so we just need to read it.
                    m_fileStream = new FileStream(m_fileName, FileMode.Open, m_fileAccessMode, FileShare.ReadWrite);
                    m_fat = new ArchiveFileAllocationTable(this);
                }
                else
                {
                    // File does not exist, so we have to create it and initialize it.
                    m_fileStream = new FileStream(m_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    m_fat = new ArchiveFileAllocationTable(this);
                    m_fat.Save();
                }

                // Don't proceed further for standby and historic files.
                if (m_fileType != ArchiveFileType.Active)
                    return;

                // Start internal process queues.
                m_historicDataQueue.Start();
                m_outOfSequenceDataQueue.Start();

                // Open state file if closed.
                if (!m_stateFile.IsOpen)
                    m_stateFile.Open();
                //Open intercom file if closed.
                if (!m_intercomFile.IsOpen)
                    m_intercomFile.Open();
                // Open metadata file if closed.
                if (!m_metadataFile.IsOpen)
                    m_metadataFile.Open();

                // Create data block lookup list.
                if (m_stateFile.RecordsInMemory > 0)
                    m_dataBlocks = new List<ArchiveDataBlock>(new ArchiveDataBlock[m_stateFile.RecordsInMemory]);
                else
                    m_dataBlocks = new List<ArchiveDataBlock>(new ArchiveDataBlock[m_stateFile.RecordsOnDisk]);

                // Start the memory conservation process.
                if (m_conserveMemory)
                    m_conserveMemoryTimer.Start();

                // Ensure that "rollover in progress" is not set.
                IntercomRecord system = m_intercomFile.Read(1);
                system.RolloverInProgress = false;
                m_intercomFile.Write(1, system);

                // Start preparing the list of historic files.
                m_buildHistoricFileListThread = new Thread(BuildHistoricFileList);
                m_buildHistoricFileListThread.Priority = ThreadPriority.Lowest;
                m_buildHistoricFileListThread.Start();

                // Start file watchers to monitor file system changes.
                m_currentLocationFileWatcher.Filter = HistoricFilesSearchPattern;
                m_currentLocationFileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                m_currentLocationFileWatcher.EnableRaisingEvents = true;
                if (Directory.Exists(m_fileOffloadLocation))
                {
                    m_offloadLocationFileWatcher.Filter = HistoricFilesSearchPattern;
                    m_offloadLocationFileWatcher.Path = m_fileOffloadLocation;
                    m_offloadLocationFileWatcher.EnableRaisingEvents = true;
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
                // Abort any asynchronous processing.
                m_rolloverPreparationThread.Abort();
                m_buildHistoricFileListThread.Abort();

                // Stop checking allocated data blocks for activity.
                m_conserveMemoryTimer.Stop();

                // Stop the historic and out-of-sequence data queues.
                m_historicDataQueue.Stop();
                m_outOfSequenceDataQueue.Stop();

                // Dispose the underlying file stream object.
                m_fat = null;
                if (m_fileStream != null)
                {
                    lock (m_fileStream)
                    {
                        m_fileStream.Dispose();
                    }
                    m_fileStream = null;
                }

                if (m_dataBlocks != null)
                {
                    lock (m_dataBlocks)
                    {
                        m_dataBlocks.Clear();
                    }
                    m_dataBlocks = null;
                }

                // Stop watching for historic archive files.
                m_currentLocationFileWatcher.EnableRaisingEvents = false;
                m_offloadLocationFileWatcher.EnableRaisingEvents = false;

                // Clear the list of historic archive files.
                if (m_historicArchiveFiles != null)
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
                throw new InvalidOperationException(string.Format("\"{0}\" is not open.", m_fileName));
            }
        }

        /// <summary>
        /// Performs rollover of active <see cref="ArchiveFile"/> to a new <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="ArchiveFile"/> is not <see cref="ArchiveFileType.Active"/>.</exception>
        public void Rollover()
        {
            if (m_fileType != ArchiveFileType.Active)
                throw new InvalidOperationException("Cannot rollover a file that is not active.");

            try
            {
                OnRolloverStart();

                // Notify other threads that rollover is in progress.
                m_rolloverWaitHandle.Reset();

                // Notify server that rollover is in progress.
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
                    if (state.ArchivedData.Time > endTime)
                        endTime = state.ArchivedData.Time;

                    m_stateFile.Write(state.HistorianID, state);
                }
                m_fat.FileEndTime = endTime;
                Save();

                List<Info> historicFiles = new List<Info>();
                string historyFileName = HistoryArchiveFileName;
                string standbyFileName = StandbyArchiveFileName;
                // We get a local copy of the list of historic archive files before closing as closing the
                // file clears the list. After file's been closed, we re-assign the local copy to the list
                // in order to avoid this list from being created over and over again after rollovers.
                lock (m_historicArchiveFiles)
                {
                    historicFiles.AddRange(m_historicArchiveFiles);
                }
                Close();
                m_historicArchiveFiles = historicFiles;

                // CRITICAL: Exception can be encountered if exclusive lock to the current file cannot be obtained.
                //           Possible if the server fails to give up the file or for some reason the current file
                //           doesn't release all locks on the file.
                if (File.Exists(m_fileName))
                {
                    try
                    {
                        FilePath.WaitForWriteLock(m_fileName, 60); // Wait for the server to release the file.
                        File.Move(m_fileName, historyFileName); // Make the active archive file historic.
                        // We add the file that we just rolled over from to the list of historic files.
                        lock (m_historicArchiveFiles)
                        {
                            m_historicArchiveFiles.Add(GetHistoricFileInfo(historyFileName));
                        }

                        if (File.Exists(standbyFileName))
                        {
                            // We have a "standby" archive file for us to use, so we'll use it. It is possible that
                            // the "standby" file may not be available for use if it could not be created due to
                            // insufficient disk space during the "rollover preparation stage". If that's the case,
                            // Open() below will try to create a new archive file, but will only succeed if there
                            // is enough disk space.
                            File.Move(standbyFileName, m_fileName); // Make the standby archive file active.
                        }
                    }
                    catch (Exception)
                    {
                        Open();
                        throw;
                    }
                }

                // CRITICAL: Exception can be encountered if a "standby" archive is not present for us to use and
                //           we cannot create a new archive file probably because there isn't enough disk space.
                try
                {
                    Open();
                    m_fat.FileStartTime = endTime;

                    // Notify server that rollover is complete.
                    system.RolloverInProgress = false;
                    m_intercomFile.Write(1, system);
                    m_intercomFile.Save();

                    // Notify other threads that rollover is complete.
                    m_rolloverWaitHandle.Set();

                    OnRolloverComplete();
                }
                catch (Exception)
                {
                    Close(); // Close the file if we fail to open it.
                    File.Delete(m_fileName);
                    throw; // Rethrow the exception so that the exception event can be raised.
                }
            }
            catch (Exception ex)
            {
                OnRolloverException(ex);
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="dataPoint"/> to the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="dataPoint"><see cref="ArchiveData"/> to be written.</param>
        public void WriteData(IDataPoint dataPoint)
        {
            // Ensure that the current file is open.
            if (!IsOpen)
                throw new InvalidOperationException(string.Format("\"{0}\" file is not open.", m_fileName));

            // Ensure that the current file is active.
            if (m_fileType != ArchiveFileType.Active)
                throw new InvalidOperationException("Data can only be directly written to files that are Active.");

            // Yeild to the rollover process if it is in progress.
            m_rolloverWaitHandle.WaitOne();

            // Initialize local variables.
            ArchiveData data = (ArchiveData)dataPoint;
            MetadataRecord metadata = m_metadataFile.Read(data.HistorianID);
            StateRecord state = m_stateFile.Read(data.HistorianID);
            IntercomRecord system = m_intercomFile.Read(1);

            // Ensure that the received data is to be archived.
            if (metadata == null || !metadata.GeneralFlags.Enabled)
            {
                OnOrphanDataReceived(data);
                return;
            }

            // Ensure that data is not far out in to the future.
            if (data.Time > DateTime.UtcNow.AddMinutes(m_leadTimeTolerance))
            {
                OnFutureDataReceived(data);
                return;
            }

            // Perform quality check if data quality is not set.
            if ((int)data.Quality == 31)
            {
                // Note: Here we're checking if the Quality is 31 instead of -1 because the quality value is stored
                // in the first 5 bits (QualityMask = 31) of Flags in the point data. Initially when the Quality is
                // set to -1, all the bits Flags (a 32-bit integer) are set to 1. And therefore, when we get the
                // Quality, which is a masked value of Flags, we get 31 and not -1.
                switch (metadata.GeneralFlags.DataType)
                {
                    case DataType.Analog:
                        if (data.Value >= metadata.AnalogFields.HighRange)
                            data.Quality = Quality.UnreasonableHigh;
                        else if (data.Value >= metadata.AnalogFields.HighAlarm)
                            data.Quality = Quality.ValueAboveHiHiAlarm;
                        else if (data.Value >= metadata.AnalogFields.HighWarning)
                            data.Quality = Quality.ValueAboveHiAlarm;
                        else if (data.Value <= metadata.AnalogFields.LowRange)
                            data.Quality = Quality.UnreasonableLow;
                        else if (data.Value <= metadata.AnalogFields.LowAlarm)
                            data.Quality = Quality.ValueBelowLoLoAlarm;
                        else if (data.Value <= metadata.AnalogFields.LowWarning)
                            data.Quality = Quality.ValueBelowLoAlarm;
                        else
                            data.Quality = Quality.Good;
                        break;
                    case DataType.Digital:
                        if (data.Value == metadata.DigitalFields.AlarmState)
                            data.Quality = Quality.LogicalAlarm;
                        else
                            data.Quality = Quality.Good;
                        break;
                }
            }

            // Update information about the latest data point received.
            if (data.Time > system.LatestDataTime)
            {
                system.LatestDataID = data.HistorianID;
                system.LatestDataTime = data.Time;
                m_intercomFile.Write(1, system);
            }

            // Check for data that out-of-sequence based on it's time.
            if (data.Time <= state.PreviousData.Time)
            {
                if (data.Time == state.PreviousData.Time)
                {
                    // Discard data that is an exact duplicate of data in line for archival.
                    if (data.Value == state.PreviousData.Value && data.Quality == state.PreviousData.Quality)
                        return;
                }
                else
                {
                    // Queue out-of-sequence data for processing if it is not be discarded.
                    if (!m_discardOutOfSequenceData)
                        m_outOfSequenceDataQueue.Add(data);

                    OnOutOfSequenceDataReceived(data);
                    return;
                }
            }

            // [BEGIN]   Data compression
            bool archiveData = false;
            bool calculateSlopes = false;
            float compressionLimit = metadata.AnalogFields.CompressionLimit;

            // Set the compression limit to a very low number for digital points.
            if (metadata.GeneralFlags.DataType == DataType.Digital)
                compressionLimit = 0.000000001f;

            state.CurrentData = new StateRecordData(data);
            if (state.ArchivedData.IsEmpty)
            {
                // This is the first time data is received.
                state.CurrentData = new StateRecordData(-1);
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
                    if ((metadata.AlarmFlags.Value & (2 ^ (int)state.CurrentData.Quality)) != 0)
                    {
                        // Current data quality warrants alarming based on the alarming settings.
                        float delay = 0;
                        switch (metadata.GeneralFlags.DataType)
                        {
                            case DataType.Analog:
                                delay = metadata.AnalogFields.AlarmDelay;
                                break;
                            case DataType.Digital:
                                delay = metadata.DigitalFields.AlarmDelay;
                                break;
                        }

                        // Dispatch the alarm immediately or after a given time based on settings.
                        if (delay > 0)
                        {
                            // Wait before dispatching alarm.
                            double first;
                            if (m_delayedAlarmProcessing.TryGetValue(data.HistorianID, out first))
                            {
                                if (state.CurrentData.Time.Value - first > delay)
                                {
                                    // Wait is now over, dispatch the alarm.
                                    m_delayedAlarmProcessing.Remove(data.HistorianID);
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
                        m_delayedAlarmProcessing.Remove(data.HistorianID);
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
                        data = new ArchiveData(state.PreviousData);
                        archiveData = true;
                        calculateSlopes = true;
                    }
                    else
                    {
                        // Perform a compression test.
                        double slope1;
                        double slope2;
                        double currentSlope;

                        slope1 = (state.CurrentData.Value - (state.ArchivedData.Value + compressionLimit)) / (state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                        slope2 = (state.CurrentData.Value - (state.ArchivedData.Value - compressionLimit)) / (state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                        currentSlope = (state.CurrentData.Value - state.ArchivedData.Value) / (state.CurrentData.Time.Value - state.ArchivedData.Time.Value);

                        if (slope1 >= state.Slope1)
                            state.Slope1 = slope1;
                        
                        if (slope2 <= state.Slope2)
                            state.Slope2 = slope2;
                        
                        if (currentSlope <= state.Slope1 || currentSlope >= state.Slope2)
                        {
                            data = new ArchiveData(state.PreviousData);
                            archiveData = true;
                            calculateSlopes = true;
                        }
                    }
                }
                else
                {
                    // Data is not to be compressed.
                    data = new ArchiveData(state.PreviousData);
                    archiveData = true;
                }
            }
            // [END]     Data compression

            // [BEGIN]   Data archival
            m_fat.DataPointsReceived++;
            if (archiveData)
            {
                if (data.Time >= m_fat.FileStartTime)
                {
                    // Data belongs to this file.
                    ArchiveDataBlock dataBlock;
                    lock (m_dataBlocks)
                    {
                        dataBlock = m_dataBlocks[data.HistorianID - 1];
                    }

                    if (dataBlock == null || dataBlock.SlotsAvailable == 0)
                    {
                        // Need to find a data block for writting the data.
                        if (dataBlock != null)
                        {
                            dataBlock = null;
                            state.ActiveDataBlockIndex = -1;
                        }

                        if (state.ActiveDataBlockIndex >= 0)
                        {
                            // Retrieve previously used data block.
                            dataBlock = m_fat.RequestDataBlock(data.HistorianID, data.Time, state.ActiveDataBlockIndex);
                        }
                        else
                        {
                            // Time to request a brand new data block.
                            dataBlock = m_fat.RequestDataBlock(data.HistorianID, data.Time, system.DataBlocksUsed);
                        }

                        if (dataBlock != null)
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
                            m_dataBlocks[data.HistorianID - 1] = dataBlock;
                        }

                        // Kick-off the rollover preparation when its threshold is reached.
                        if (Statistics.FileUsage >= m_rolloverPreparationThreshold && !File.Exists(StandbyArchiveFileName) && !m_rolloverPreparationThread.IsAlive)
                        {
                            m_rolloverPreparationThread = new Thread(new ThreadStart(PrepareForRollover));
                            m_rolloverPreparationThread.Priority = ThreadPriority.Lowest;
                            m_rolloverPreparationThread.Start();
                        }
                    }

                    if (dataBlock != null)
                    {
                        // Write data to the data block.
                        dataBlock.Write(data);
                        m_fat.DataPointsArchived++;
                    }
                    else
                    {
                        // File is full, rollover if configured.
                        OnFileFull();

                        if (m_rolloverOnFull)
                        {
                            while (true)
                            {
                                Rollover(); // Start rollover.
                                if (m_rolloverWaitHandle.WaitOne(1, false))
                                {
                                    break; // Rollover is successful.
                                }
                            }
                        }

                        // Re-read the state information since it is modified during the rollover.
                        state = m_stateFile.Read(data.HistorianID);
                    }
                }
                else
                {
                    // Data is historic.
                    m_fat.DataPointsReceived--;
                    m_historicDataQueue.Add(data);
                    OnHistoricDataReceived(data);
                }

                state.ArchivedData = new StateRecordData(data);
            }

            if (calculateSlopes)
            {
                if (state.CurrentData.Time.Value != state.ArchivedData.Time.Value)
                {
                    state.Slope1 = (state.CurrentData.Value - (state.ArchivedData.Value + compressionLimit)) / (state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                    state.Slope2 = (state.CurrentData.Value - (state.ArchivedData.Value - compressionLimit)) / (state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
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

        /// <summary>
        /// Writes the specified <paramref name="dataPoints"/> to the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="dataPoints"><see cref="ArchiveData"/> points to be written.</param>
        public void WriteData(IEnumerable<IDataPoint> dataPoints)
        {
            if (IsOpen)
            {
                foreach (IDataPoint dataPoint in dataPoints)
                {
                    WriteData(dataPoint);
                }
            }
            else
            {
                throw (new InvalidOperationException(string.Format("{0} '{1}' is not open.", this.GetType().Name, m_fileName)));
            }
        }

        /// <summary>
        /// Writes <paramref name="metadata"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="metadata"><see cref="MetadataRecord.BinaryImage"/> data.</param>
        public void WriteMetaData(int historianID, byte[] metadata)
        {
            MetadataFile.Write(historianID, new MetadataRecord(historianID, metadata, 0, metadata.Length));
            MetadataFile.Save();
        }

        /// <summary>
        /// Writes <paramref name="statedata"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="statedata"><see cref="StateRecord.BinaryImage"/> data.</param>
        public void WriteStateData(int historianID, byte[] statedata)
        {
            StateFile.Write(historianID, new StateRecord(historianID, statedata, 0, statedata.Length));
            StateFile.Save();
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID)
        {
            return ReadData(historianID, TimeTag.MinValue);
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, string startTime)
        {
            return ReadData(historianID, startTime, TimeTag.MinValue.ToString());
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <param name="endTime"><see cref="String"/> representation of the end time (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, string startTime, string endTime)
        {
            return ReadData(historianID, Convert.ToDateTime(startTime), Convert.ToDateTime(endTime));
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, DateTime startTime)
        {
            return ReadData(historianID, startTime, TimeTag.MinValue.ToDateTime());
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <param name="endTime">End <see cref="DateTime"/> (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, DateTime startTime, DateTime endTime)
        {
            return ReadData(historianID, new TimeTag(startTime), new TimeTag(endTime));
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime)
        {
            return ReadData(historianID, startTime, TimeTag.MaxValue);
        }

        /// <summary>
        /// Reads <see cref="ArchiveData"/> points.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveData"/> points are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <param name="endTime">End <see cref="TimeTag"/> (in GMT) for the <see cref="ArchiveData"/> points to be retrieved.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveData"/> points.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime, TimeTag endTime)
        {
            // Ensure that the current file is open.
            if (!IsOpen)
                throw new InvalidOperationException(string.Format("\"{0}\" file is not open.", m_fileName));

            // Ensure that the current file is active.
            if (m_fileType != ArchiveFileType.Active)
                throw new InvalidOperationException("Data can only be directly read from files that are Active.");

            // Ensure that the start and end time are valid.
            if (startTime > endTime)
                throw new ArgumentException("End Time preceeds Start Time in the specified timespan.");

            // Yeild to the rollover process if it is in progress.
            m_rolloverWaitHandle.WaitOne();

            List<Info> dataFiles = new List<Info>();
            if (startTime < m_fat.FileStartTime)
            {
                // Data is to be read from historic file(s).
                if (m_buildHistoricFileListThread.IsAlive)
                    m_buildHistoricFileListThread.Join();

                m_readSearchStartTimeTag = startTime;
                m_readSearchEndTimeTag = endTime;
                lock (m_historicArchiveFiles)
                {
                    dataFiles.AddRange(m_historicArchiveFiles.FindAll(FindHistoricArchiveFileForRead));
                }
            }

            if (endTime >= m_fat.FileStartTime)
            {
                // Data is to be read from the active file.
                Info activeFileInfo = new Info();
                activeFileInfo.FileName = m_fileName;
                activeFileInfo.StartTimeTag = m_fat.FileStartTime;
                activeFileInfo.EndTimeTag = m_fat.FileEndTime;
                dataFiles.Add(activeFileInfo);
            }

            // Read data from all qualifying files.
            foreach (Info dataFile in dataFiles)
            {
                ArchiveFile file = new ArchiveFile();
                IList<ArchiveDataBlock> dataBlocks;
                try
                {
                    file.FileName = dataFile.FileName;
                    file.FileType = ArchiveFileType.Historic;
                    file.StateFile = m_stateFile;
                    file.IntercomFile = m_intercomFile;
                    file.MetadataFile = m_metadataFile;
                    file.Open();

                    dataBlocks = file.Fat.FindDataBlocks(historianID, startTime, endTime);
                    if (dataBlocks.Count > 0)
                    {
                        // Data block before the first data block matching the search criteria might contain some data 
                        // for the specified search criteria, so look for such a data block and process its data.
                        lock (file.Fat.DataBlockPointers)
                        {
                            for (int i = dataBlocks[0].Index - 1; i >= 0; i--)
                            {
                                if (file.Fat.DataBlockPointers[i].HistorianID == historianID)
                                {
                                    foreach (ArchiveData data in file.Fat.DataBlockPointers[i].DataBlock.Read())
                                    {
                                        if (data.Time >= startTime)
                                            yield return data;
                                    }

                                    break;
                                }
                            }
                        }

                        // Read data from rest of the data blocks and scan the last data block for data matching the
                        // the search criteria as it may contain data beyond the timespan specified in the search.
                        for (int i = 0; i < dataBlocks.Count; i++)
                        {
                            if (i < dataBlocks.Count - 1)
                            {
                                // Read all the data.
                                foreach (ArchiveData data in dataBlocks[i].Read())
                                {
                                    yield return data;
                                }
                            }
                            else
                            {
                                // Scan through the data block.
                                foreach (ArchiveData data in dataBlocks[i].Read())
                                {
                                    if (data.Time <= endTime)
                                        yield return data;
                                    else
                                        yield break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (file.IsOpen)
                    {
                        file.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Reads <see cref="MetadataRecord.BinaryImage"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="MetadataRecord.BinaryImage"/>.</returns>
        public byte[] ReadMetaData(int historianID)
        {
            return MetadataFile.Read(historianID).BinaryImage;
        }

        /// <summary>
        /// Reads <see cref="StateRecord.BinaryImage"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="StateRecord.BinaryImage"/>.</returns>
        public byte[] ReadStateData(int historianID)
        {
            return StateFile.Read(historianID).BinaryImage;
        }

        /// <summary>
        /// Reads <see cref="MetadataRecordSummary.BinaryImage"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="MetadataRecordSummary.BinaryImage"/>.</returns>
        public byte[] ReadMetaDataSummary(int historianID)
        {
            return MetadataFile.Read(historianID).Summary.BinaryImage;
        }

        /// <summary>
        /// Reads <see cref="StateRecordSummary.BinaryImage"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing <see cref="StateRecordSummary.BinaryImage"/>.</returns>
        public byte[] ReadStateDataSummary(int historianID)
        {
            return StateFile.Read(historianID).Summary.BinaryImage;
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
        /// Raises the <see cref="RolloverStart"/> event.
        /// </summary>
        protected virtual void OnRolloverStart()
        {
            if (RolloverStart != null)
                RolloverStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverComplete"/> event.
        /// </summary>
        protected virtual void OnRolloverComplete()
        {
            if (RolloverComplete != null)
                RolloverComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="RolloverException"/> event.</param>
        protected virtual void OnRolloverException(Exception ex)
        {
            if (RolloverException != null)
                RolloverException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="OffloadStart"/> event.
        /// </summary>
        protected virtual void OnOffloadStart()
        {
            if (OffloadStart != null)
                OffloadStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OffloadComplete"/> event.
        /// </summary>
        protected virtual void OnOffloadComplete()
        {
            if (OffloadComplete != null)
                OffloadComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OffloadException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="OffloadException"/> event.</param>
        protected virtual void OnOffloadException(Exception ex)
        {
            if (OffloadException != null)
                OffloadException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="OffloadProgress"/> event.
        /// </summary>
        /// <param name="offloadProgress"><see cref="ProcessProgress{T}"/> to send to <see cref="OffloadProgress"/> event.</param>
        protected virtual void OnOffloadProgress(ProcessProgress<int> offloadProgress)
        {
            if (OffloadProgress != null)
                OffloadProgress(this, new EventArgs<ProcessProgress<int>>(offloadProgress));
        }

        /// <summary>
        /// Raises the <see cref="RolloverPreparationStart"/> event.
        /// </summary>
        protected virtual void OnRolloverPreparationStart()
        {
            if (RolloverPreparationStart != null)
                RolloverPreparationStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverPreparationComplete"/> event.
        /// </summary>
        protected virtual void OnRolloverPreparationComplete()
        {
            if (RolloverPreparationComplete != null)
                RolloverPreparationComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="RolloverPreparationException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="RolloverPreparationException"/> event.</param>
        protected virtual void OnRolloverPreparationException(Exception ex)
        {
            if (RolloverPreparationException != null)
                RolloverPreparationException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="HistoricFileListBuildStart"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListBuildStart()
        {
            if (HistoricFileListBuildStart != null)
                HistoricFileListBuildStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HistoricFileListBuildComplete"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListBuildComplete()
        {
            if (HistoricFileListBuildComplete != null)
                HistoricFileListBuildComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raise the <see cref="HistoricFileListBuildException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="HistoricFileListBuildException"/> event.</param>
        protected virtual void OnHistoricFileListBuildException(Exception ex)
        {
            if (HistoricFileListBuildException != null)
                HistoricFileListBuildException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="HistoricFileListUpdated"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListUpdated()
        {
            if (HistoricFileListUpdated != null)
                HistoricFileListUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OrphanDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="ArchiveData"/> to send to <see cref="OrphanDataReceived"/> event.</param>
        protected virtual void OnOrphanDataReceived(ArchiveData dataPoint)
        {
            if (OrphanDataReceived != null)
                OrphanDataReceived(this, new EventArgs<ArchiveData>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="FutureDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="ArchiveData"/> to send to <see cref="FutureDataReceived"/> event.</param>
        protected virtual void OnFutureDataReceived(ArchiveData dataPoint)
        {
            if (FutureDataReceived != null)
                FutureDataReceived(this, new EventArgs<ArchiveData>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="HistoricDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="ArchiveData"/> to send to <see cref="HistoricDataReceived"/> event.</param>
        protected virtual void OnHistoricDataReceived(ArchiveData dataPoint)
        {
            if (HistoricDataReceived != null)
                HistoricDataReceived(this, new EventArgs<ArchiveData>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="HistoricDataWriteStart"/> event.
        /// </summary>
        protected virtual void OnHistoricDataWriteStart()
        {
            if (HistoricDataWriteStart != null)
                HistoricDataWriteStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HistoricDataWriteComplete"/> event.
        /// </summary>
        protected virtual void OnHistoricDataWriteComplete()
        {
            if (HistoricDataWriteComplete != null)
                HistoricDataWriteComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HistoricDataWriteException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="HistoricDataWriteException"/> event.</param>
        protected virtual void OnHistoricDataWriteException(Exception ex)
        {
            if (HistoricDataWriteException != null)
                HistoricDataWriteException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="HistoricDataWriteProgress"/> event.
        /// </summary>
        /// <param name="historicWriteProgress"><see cref="ProcessProgress{T}"/> to send to <see cref="HistoricDataWriteProgress"/> event.</param>
        protected virtual void OnHistoricDataWriteProgress(ProcessProgress<int> historicWriteProgress)
        {
            if (HistoricDataWriteProgress != null)
                HistoricDataWriteProgress(this, new EventArgs<ProcessProgress<int>>(historicWriteProgress));
        }

        /// <summary>
        /// Raises the <see cref="OutOfSequenceDataReceived"/> event.
        /// </summary>
        /// <param name="dataPoint"><see cref="ArchiveData"/> to send to <see cref="OutOfSequenceDataReceived"/> event.</param>
        protected virtual void OnOutOfSequenceDataReceived(ArchiveData dataPoint)
        {
            if (OutOfSequenceDataReceived != null)
                OutOfSequenceDataReceived(this, new EventArgs<ArchiveData>(dataPoint));
        }

        /// <summary>
        /// Raises the <see cref="OutOfSequenceDataWriteStart"/> event.
        /// </summary>
        protected virtual void OnOutOfSequenceDataWriteStart()
        {
            if (OutOfSequenceDataWriteStart != null)
                OutOfSequenceDataWriteStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OutOfSequenceDataWriteComplete"/> event.
        /// </summary>
        protected virtual void OnOutOfSequenceDataWriteComplete()
        {
            if (OutOfSequenceDataWriteComplete != null)
                OutOfSequenceDataWriteComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="OutOfSequenceDataWriteException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="OutOfSequenceDataWriteException"/> event.</param>
        protected virtual void OnOutOfSequenceDataWriteException(Exception ex)
        {
            if (OutOfSequenceDataWriteException != null)
                OutOfSequenceDataWriteException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="OutOfSequenceDataWriteProgress"/> event.
        /// </summary>
        /// <param name="historicWriteProgress"><see cref="ProcessProgress{T}"/> to send to <see cref="OutOfSequenceDataWriteProgress"/> event.</param>
        protected virtual void OnOutOfSequenceDataWriteProgress(ProcessProgress<int> historicWriteProgress)
        {
            if (OutOfSequenceDataWriteProgress != null)
                OutOfSequenceDataWriteProgress(this, new EventArgs<ProcessProgress<int>>(historicWriteProgress));
        }

        /// <summary>
        /// Raises the <see cref="ProcessAlarmNotification"/> event.
        /// </summary>
        /// <param name="pointState"><see cref="StateRecord"/> to send to <see cref="ProcessAlarmNotification"/> event.</param>
        protected virtual void OnProcessAlarmNotification(StateRecord pointState)
        {
            if (ProcessAlarmNotification != null)
                ProcessAlarmNotification(this, new EventArgs<StateRecord>(pointState));
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

                        if (m_rolloverWaitHandle != null)
                            m_rolloverWaitHandle.Close();

                        if (m_historicDataQueue != null)
                            m_historicDataQueue.Dispose();

                        if (m_outOfSequenceDataQueue != null)
                            m_outOfSequenceDataQueue.Dispose();

                        if (m_currentLocationFileWatcher != null)
                            m_currentLocationFileWatcher.Dispose();

                        if (m_offloadLocationFileWatcher != null)
                            m_offloadLocationFileWatcher.Dispose();

                        // Detach from all of the dependency files.
                        StateFile = null;
                        MetadataFile = null;
                        IntercomFile = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        #region [ Helper Methods ]

        private void BuildHistoricFileList()
        {
            if (m_historicArchiveFiles == null)
            {
                // The list of historic files has not been created, so we'll create it.
                try
                {
                    m_historicArchiveFiles = new List<Info>();

                    OnHistoricFileListBuildStart();

                    // We can safely assume that we'll always get information about the historic file because, the
                    // the search pattern ensures that we only can a list of historic archive files and not all files.
                    Info historicFileInfo = null;
                    lock (m_historicArchiveFiles)
                    {
                        // Prevent the historic file list from being updated by the file watchers.
                        foreach (string historicFileName in Directory.GetFiles(FilePath.GetDirectoryName(m_fileName), HistoricFilesSearchPattern))
                        {
                            historicFileInfo = GetHistoricFileInfo(historicFileName);
                            if (historicFileInfo != null)
                            {
                                m_historicArchiveFiles.Add(historicFileInfo);
                            }
                        }

                        if (Directory.Exists(m_fileOffloadLocation))
                        {
                            foreach (string historicFileName in Directory.GetFiles(m_fileOffloadLocation, HistoricFilesSearchPattern))
                            {
                                historicFileInfo = GetHistoricFileInfo(historicFileName);
                                if (historicFileInfo != null)
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

        private void PrepareForRollover()
        {
            try
            {
                DriveInfo archiveDrive = new DriveInfo(Path.GetPathRoot(m_fileName));
                if (archiveDrive.AvailableFreeSpace < archiveDrive.TotalSize * (1 - ((double)m_fileOffloadThreshold / 100)))
                {
                    // We'll start offloading historic files if we've reached the offload threshold.
                    OffloadHistoricFiles();
                }

                OnRolloverPreparationStart();

                // Opening and closing a new archive file in "standby" mode will create a "standby" archive file.
                ArchiveFile standbyArchiveFile = new ArchiveFile();
                standbyArchiveFile.FileName = m_fileName;
                standbyArchiveFile.FileType = ArchiveFileType.Standby;
                standbyArchiveFile.FileSize = m_fileSize;
                standbyArchiveFile.DataBlockSize = m_dataBlockSize;
                standbyArchiveFile.StateFile = m_stateFile;
                standbyArchiveFile.IntercomFile = m_intercomFile;
                standbyArchiveFile.MetadataFile = m_metadataFile;
                try
                {
                    standbyArchiveFile.Open();
                }
                catch (Exception)
                {
                    string standbyFileName = standbyArchiveFile.FileName;
                    standbyArchiveFile.Close();
                    // We didn't succeed in creating a "standby" archive file, so we'll delete it if it was created
                    // partially (might happen if there isn't enough disk space or thread is aborted). This is to
                    // ensure that this preparation processes is kicked off again until a valid "standby" archive
                    // file is successfully created.
                    if (File.Exists(standbyFileName))
                    {
                        File.Delete(standbyFileName);
                    }

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

        private void OffloadHistoricFiles()
        {
            if (Directory.Exists(m_fileOffloadLocation))
            {
                if (m_buildHistoricFileListThread.IsAlive)
                {
                    // Wait until the historic file list has been built.
                    m_buildHistoricFileListThread.Join();
                }

                try
                {
                    OnOffloadStart();

                    // The offload path that is specified is a valid one so we'll gather a list of all historic
                    // files in the directory where the current (active) archive file is located.
                    List<Info> newHistoricFiles = null;
                    lock (m_historicArchiveFiles)
                    {
                        newHistoricFiles = m_historicArchiveFiles.FindAll(IsNewHistoricArchiveFile);
                    }

                    // Sorting the list will sort the historic files from oldest to newest.
                    newHistoricFiles.Sort();

                    // We'll offload the specified number of oldest historic files to the offload location if the
                    // number of historic files is more than the offload count or all of the historic files if the
                    // offload count is smaller the available number of historic files.
                    ProcessProgress<int> offloadProgress = new ProcessProgress<int>("FileOffload");
                    offloadProgress.Total = (newHistoricFiles.Count < m_fileOffloadCount ? newHistoricFiles.Count : m_fileOffloadCount);
                    for (int i = 0; i < offloadProgress.Total; i++)
                    {
                        string destinationFileName = FilePath.AddPathSuffix(m_fileOffloadLocation) + FilePath.GetFileName(newHistoricFiles[i].FileName);
                        if (File.Exists(destinationFileName))
                        {
                            // Delete the destination file if it already exists.
                            File.Delete(destinationFileName);
                        }

                        File.Move(newHistoricFiles[i].FileName, destinationFileName);

                        offloadProgress.Complete++;
                        offloadProgress.ProgressMessage = string.Format("Offloaded historic file {0} ({1} of {2}).", FilePath.GetFileName(newHistoricFiles[i].FileName), "{0}", "{1}");
                        OnOffloadProgress(offloadProgress);
                    }

                    OnOffloadComplete();
                }
                catch (ThreadAbortException)
                {
                    throw; // Bubble up the ThreadAbortException.
                }
                catch (Exception ex)
                {
                    OnOffloadException(ex);
                }
            }
        }

        private Info GetHistoricFileInfo(string fileName)
        {
            Info fileInfo = null;
            try
            {
                if (File.Exists(fileName))
                {
                    // We'll open the file and get relevant information about it.
                    ArchiveFile historicArchiveFile = new ArchiveFile();
                    historicArchiveFile.FileName = fileName;
                    historicArchiveFile.FileType = ArchiveFileType.Historic;
                    historicArchiveFile.StateFile = m_stateFile;
                    historicArchiveFile.IntercomFile = m_intercomFile;
                    historicArchiveFile.MetadataFile = m_metadataFile;
                    try
                    {
                        historicArchiveFile.Open();
                        fileInfo = new Info();
                        fileInfo.FileName = fileName;
                        fileInfo.StartTimeTag = historicArchiveFile.Fat.FileStartTime;
                        fileInfo.EndTimeTag = historicArchiveFile.Fat.FileEndTime;
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        historicArchiveFile.Dispose();
                        historicArchiveFile = null;
                    }
                }
                else
                {
                    // We'll resolve to getting the file information from its name only if the file no longer exists
                    // at the location. This will be the case when file is moved to a different location. In this
                    // case the file information we provide is only as good as the file name.
                    string datesString = FilePath.GetFileNameWithoutExtension(fileName).Substring((FilePath.GetFileNameWithoutExtension(m_fileName) + "_").Length);
                    string[] fileStartEndDates = datesString.Split(new string[] { "_to_" }, StringSplitOptions.None);

                    fileInfo = new Info();
                    fileInfo.FileName = fileName;
                    if (fileStartEndDates.Length == 2)
                    {
                        fileInfo.StartTimeTag = new TimeTag(Convert.ToDateTime(fileStartEndDates[0].Replace('!', ':')));
                        fileInfo.EndTimeTag = new TimeTag(Convert.ToDateTime(fileStartEndDates[1].Replace('!', ':')));
                    }
                }
            }
            catch (Exception)
            {

            }

            return fileInfo;
        }

        #endregion

        #region [ Find Predicates ]

        private bool FindHistoricArchiveFileForRead(Info fileInfo)
        {
            return (fileInfo != null && ((m_readSearchStartTimeTag >= fileInfo.StartTimeTag && m_readSearchStartTimeTag <= fileInfo.EndTimeTag) ||
                                         (m_readSearchEndTimeTag >= fileInfo.StartTimeTag && m_readSearchEndTimeTag <= fileInfo.EndTimeTag) ||
                                         (m_readSearchStartTimeTag < fileInfo.StartTimeTag && m_readSearchEndTimeTag > fileInfo.EndTimeTag)));
        }

        private bool FindHistoricArchiveFileForWrite(Info fileInfo)
        {
            return (fileInfo != null &&
                    m_writeSearchTimeTag >= fileInfo.StartTimeTag &&
                    m_writeSearchTimeTag <= fileInfo.EndTimeTag);
        }

        private bool IsNewHistoricArchiveFile(Info fileInfo)
        {
            return (fileInfo != null &&
                    string.Compare(FilePath.GetDirectoryName(m_fileName), FilePath.GetDirectoryName(fileInfo.FileName), true) == 0);
        }

        private bool IsOldHistoricArchiveFile(Info fileInfo)
        {
            return (fileInfo != null &&
                    !string.IsNullOrEmpty(m_fileOffloadLocation) &&
                    string.Compare(FilePath.GetDirectoryName(m_fileOffloadLocation), FilePath.GetDirectoryName(fileInfo.FileName), true) == 0);
        }

        #endregion

        #region [ Queue Delegates ]

        private void WriteToHistoricArchiveFile(ArchiveData[] items)
        {
            if (m_buildHistoricFileListThread.IsAlive)
                // Wait until the historic file list has been built.
                m_buildHistoricFileListThread.Join();

            OnHistoricDataWriteStart();

            Dictionary<int, List<ArchiveData>> sortedPointData = new Dictionary<int, List<ArchiveData>>();
            // First we'll seperate all point data by ID.
            for (int i = 0; i < items.Length; i++)
            {
                if (!sortedPointData.ContainsKey(items[i].HistorianID))
                {
                    sortedPointData.Add(items[i].HistorianID, new List<ArchiveData>());
                }

                sortedPointData[items[i].HistorianID].Add(items[i]);
            }

            ProcessProgress<int> historicWriteProgress = new ProcessProgress<int>("HistoricWrite");
            historicWriteProgress.Total = items.Length;
            foreach (int pointID in sortedPointData.Keys)
            {
                // We'll sort the point data for the current point ID by time.
                sortedPointData[pointID].Sort();

                ArchiveFile historicFile = null;
                ArchiveDataBlock historicFileBlock = null;
                try
                {
                    for (int i = 0; i < sortedPointData[pointID].Count; i++)
                    {
                        if (historicFile == null)
                        {
                            // We'll try to find a historic file when the current point data belongs.
                            Info historicFileInfo;
                            m_writeSearchTimeTag = sortedPointData[pointID][i].Time;
                            lock (m_historicArchiveFiles)
                            {
                                historicFileInfo = m_historicArchiveFiles.Find(FindHistoricArchiveFileForWrite);
                            }

                            if (historicFileInfo != null)
                            {
                                // Found a historic file where the data can be written.
                                historicFile = new ArchiveFile();
                                historicFile.FileName = historicFileInfo.FileName;
                                historicFile.FileType = ArchiveFileType.Historic;
                                historicFile.StateFile = m_stateFile;
                                historicFile.IntercomFile = m_intercomFile;
                                historicFile.MetadataFile = m_metadataFile;
                                historicFile.Open();
                            }
                        }

                        if (historicFile != null)
                        {
                            if (sortedPointData[pointID][i].Time.CompareTo(historicFile.Fat.FileStartTime) >= 0 && sortedPointData[pointID][i].Time.CompareTo(historicFile.Fat.FileEndTime) <= 0)
                            {
                                // The current point data belongs to the current historic archive file.
                                if (historicFileBlock == null || historicFileBlock.SlotsAvailable == 0)
                                {
                                    // Request a new or previously used data block for point data.
                                    historicFileBlock = historicFile.Fat.RequestDataBlock(pointID, sortedPointData[pointID][i].Time, -1);
                                }
                                historicFileBlock.Write(sortedPointData[pointID][i]);
                                historicFile.Fat.DataPointsReceived++;
                                historicFile.Fat.DataPointsArchived++;
                                if (i == sortedPointData[pointID].Count() - 1)
                                {
                                    // Last piece of data for the point, so we close the currently open file.
                                    historicFile.Save();
                                    historicFile.Dispose();
                                    historicFile = null;
                                    historicFileBlock = null;
                                }

                                historicWriteProgress.Complete++;
                            }
                            else
                            {
                                // The current point data doesn't belong to the current historic archive file, so we have
                                // to write all the point data we have so far for the current historic archive file to it.
                                i--;
                                historicFile.Dispose();
                                historicFile = null;
                                historicFileBlock = null;
                            }
                        }
                    }

                    // Notify of progress per point.
                    historicWriteProgress.ProgressMessage = string.Format("Wrote historic data for point id {0} ({1} of {2}).", pointID, "{0}", "{1}");
                    OnHistoricDataWriteProgress(historicWriteProgress);
                }
                catch (Exception ex)
                {
                    // Free-up used memory.
                    if (historicFile != null)
                    {
                        try
                        {
                            historicFile.Dispose();
                            historicFile = null;
                        }
                        catch
                        {

                        }
                    }

                    // Notify of the exception.
                    OnHistoricDataWriteException(ex);
                }
            }

            OnHistoricDataWriteComplete();
        }

        private void InsertInCurrentArchiveFile(ArchiveData[] items)
        {
            // TODO: Implement archival of out-of-sequence data.
        }

        #endregion

        #region [ Event Handlers ]

        private void StateFile_FileModified(object sender, EventArgs e)
        {
            if (m_stateFile.RecordsOnDisk > m_dataBlocks.Count)
            {
                // We synchronize the block list with the number of state records physically present on the disk.
                // This count goes up when new metadata records are added which in turn causes new state records
                // to be added to match with the metadata records.
                lock (m_dataBlocks)
                {
                    m_dataBlocks.AddRange(new ArchiveDataBlock[m_stateFile.RecordsOnDisk - m_dataBlocks.Count]);
                }
            }
        }

        private void MetadataFile_FileModified(object sender, System.EventArgs e)
        {
            if (m_metadataFile.RecordsOnDisk > m_stateFile.RecordsOnDisk)
            {
                // Since we have more number of records in the Metadata File than in the State File we'll synchronize
                // the number of records in both the files (very important) by writting a new records to the State
                // File with an ID same as the number of records on disk for Metadata File. Doing so will cause the
                // State File to grow in-memory or on-disk depending on how it's configured.
                m_stateFile.Write(m_metadataFile.RecordsOnDisk, new StateRecord(m_metadataFile.RecordsOnDisk));
                m_stateFile.Save();
            }
        }

        private void ConserveMemoryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_dataBlocks)
            {
                // Go through all data blocks and remove that are inactive.
                for (int i = 0; i < m_dataBlocks.Count; i++)
                {
                    if ((m_dataBlocks[i] != null) && !(m_dataBlocks[i].IsActive))
                    {
                        m_dataBlocks[i] = null;
                        Trace.WriteLine(string.Format("Inactive block for Point ID {0} disposed", i + 1));
                    }
                }
            }
        }

        private void HistoricDataQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnHistoricDataWriteException(e.Argument);
        }

        private void OutOfSequenceDataQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnOutOfSequenceDataWriteException(e.Argument);
        }

        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (IsOpen)
            {
                // Attempt to update the historic file list only if the current file is open.
                bool historicFileListUpdated = false;
                Info historicFileInfo = GetHistoricFileInfo(e.FullPath);
                lock (m_historicArchiveFiles)
                {
                    if ((historicFileInfo != null) && !m_historicArchiveFiles.Contains(historicFileInfo))
                    {
                        m_historicArchiveFiles.Add(historicFileInfo);
                        historicFileListUpdated = true;
                    }
                }
                if (historicFileListUpdated)
                {
                    OnHistoricFileListUpdated();
                }
            }
        }

        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (IsOpen)
            {
                // Attempt to update the historic file list only if the current file is open.
                bool historicFileListUpdated = false;
                Info historicFileInfo = GetHistoricFileInfo(e.FullPath);
                lock (m_historicArchiveFiles)
                {
                    if ((historicFileInfo != null) && m_historicArchiveFiles.Contains(historicFileInfo))
                    {
                        m_historicArchiveFiles.Remove(historicFileInfo);
                        historicFileListUpdated = true;
                    }
                }
                if (historicFileListUpdated)
                {
                    OnHistoricFileListUpdated();
                }
            }
        }

        private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (IsOpen)
            {
                // Attempt to update the historic file list only if the current file is open.
                if (string.Compare(FilePath.GetExtension(e.OldFullPath), FileExtension, true) == 0)
                {
                    try
                    {
                        bool historicFileListUpdated = false;
                        Info oldFileInfo = GetHistoricFileInfo(e.OldFullPath);
                        lock (m_historicArchiveFiles)
                        {
                            if ((oldFileInfo != null) && m_historicArchiveFiles.Contains(oldFileInfo))
                            {
                                m_historicArchiveFiles.Remove(oldFileInfo);
                                historicFileListUpdated = true;
                            }
                        }
                        if (historicFileListUpdated)
                        {
                            OnHistoricFileListUpdated();
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore any exception we might encounter here if an archive file being renamed to a
                        // historic archive file. This might happen if someone is renaming files manually.
                    }
                }

                if (string.Compare(FilePath.GetExtension(e.FullPath), FileExtension, true) == 0)
                {
                    try
                    {
                        bool historicFileListUpdated = false;
                        Info newFileInfo = GetHistoricFileInfo(e.FullPath);
                        lock (m_historicArchiveFiles)
                        {
                            if ((newFileInfo != null) && !m_historicArchiveFiles.Contains(newFileInfo))
                            {
                                m_historicArchiveFiles.Add(newFileInfo);
                                historicFileListUpdated = true;
                            }
                        }
                        if (historicFileListUpdated)
                        {
                            OnHistoricFileListUpdated();
                        }
                    }
                    catch (Exception)
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
