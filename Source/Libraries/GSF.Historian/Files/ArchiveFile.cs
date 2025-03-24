//******************************************************************************************************
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
//       Fixed an issue in quality-based alarm processing.
//       Removed unused/unnecessary event raised during the write process.
//  11/06/2009 - Pinal C. Patel
//       Modified Read() and Write() methods to wait on the rollover process.
//  12/01/2009 - Pinal C. Patel
//       Removed unused RolloverOnFull property.
//       Fixed an issue in the rollover process that is encountered only when dependency files are 
//       configured to not load records in memory.
//  12/02/2009 - Pinal C. Patel
//       Modified Status property to show the total number of historic archive files.
//       Fixed an issue in the update of historic archive file list.
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
//       Added an exception handler for reading (exposed via DataReadException event) to make sure
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

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable PossibleMultipleEnumeration

namespace GSF.Historian.Files;

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
            if (obj is not Info other)
                return 1;

            int result = StartTimeTag.CompareTo(other.StartTimeTag);
            return result == 0 ? EndTimeTag.CompareTo(other.EndTimeTag) : result;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Info other)
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
    public const string DefaultFileName = $"ArchiveFile{FileExtension}";

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
    public const string DefaultSettingsCategory = nameof(ArchiveFile);

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
    [Category(nameof(File))]
    [Description("Occurs when the active ArchiveFile if full.")]
    public event EventHandler FileFull;

    /// <summary>
    /// Occurs when the process of offloading historic <see cref="ArchiveFile"/>s is started.
    /// </summary>
    [Category("Archive")]
    [Description("Occurs when the process of offloading historic ArchiveFiles is started.")]
    public event EventHandler OffloadStart;

    /// <summary>
    /// Occurs when the process of offloading historic <see cref="ArchiveFile"/>s is complete.
    /// </summary>
    [Category("Archive")]
    [Description("Occurs when the process of offloading historic ArchiveFiles is complete.")]
    public event EventHandler OffloadComplete;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered during the historic <see cref="ArchiveFile"/> offload process.
    /// </summary>
    [Category("Archive")]
    [Description("Occurs when an Exception is encountered during the historic ArchiveFile offload process.")]
    public event EventHandler<EventArgs<Exception>> OffloadException;

    /// <summary>
    /// Occurs when an historic <see cref="ArchiveFile"/> is being offloaded.
    /// </summary>
    [Category("Archive")]
    [Description("Occurs when an historic ArchiveFile is being offloaded.")]
    public event EventHandler<EventArgs<ProcessProgress<int>>> OffloadProgress;

    /// <summary>
    /// Occurs when <see cref="Rollover()"/> to a new <see cref="ArchiveFile"/> is started.
    /// </summary>
    [Category(nameof(Rollover))]
    [Description("Occurs when Rollover to a new ArchiveFile is started.")]
    public event EventHandler RolloverStart;

    /// <summary>
    /// Occurs when <see cref="Rollover()"/> to a new <see cref="ArchiveFile"/> is complete.
    /// </summary>
    [Category(nameof(Rollover))]
    [Description("Occurs when Rollover to a new ArchiveFile is complete.")]
    public event EventHandler RolloverComplete;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered during the <see cref="Rollover()"/> process.
    /// </summary>
    [Category(nameof(Rollover))]
    [Description("Occurs when an Exception is encountered during the Rollover() process.")]
    public event EventHandler<EventArgs<Exception>> RolloverException;

    /// <summary>
    /// Occurs when the process of creating a standby <see cref="ArchiveFile"/> is started.
    /// </summary>
    [Category(nameof(Rollover))]
    [Description("Occurs when the process of creating a standby ArchiveFile is started.")]
    public event EventHandler RolloverPreparationStart;

    /// <summary>
    /// Occurs when the process of creating a standby <see cref="ArchiveFile"/> is complete.
    /// </summary>
    [Category(nameof(Rollover))]
    [Description("Occurs when the process of creating a standby ArchiveFile is complete.")]
    public event EventHandler RolloverPreparationComplete;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered during the standby <see cref="ArchiveFile"/> creation process.
    /// </summary>
    [Category(nameof(Rollover))]
    [Description("Occurs when an Exception is encountered during the standby ArchiveFile creation process.")]
    public event EventHandler<EventArgs<Exception>> RolloverPreparationException;

    /// <summary>
    /// Occurs when the process of building historic <see cref="ArchiveFile"/> list is started.
    /// </summary>
    [Category(nameof(File))]
    [Description("Occurs when the process of building historic ArchiveFile list is started.")]
    public event EventHandler HistoricFileListBuildStart;

    /// <summary>
    /// Occurs when the process of building historic <see cref="ArchiveFile"/> list is complete.
    /// </summary>
    [Category(nameof(File))]
    [Description("Occurs when the process of building historic ArchiveFile list is complete.")]
    public event EventHandler HistoricFileListBuildComplete;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered in historic <see cref="ArchiveFile"/> list building process.
    /// </summary>
    [Category(nameof(File))]
    [Description("Occurs when an Exception is encountered in historic ArchiveFile list building process.")]
    public event EventHandler<EventArgs<Exception>> HistoricFileListBuildException;

    /// <summary>
    /// Occurs when the historic <see cref="ArchiveFile"/> list is updated to reflect addition or deletion of historic <see cref="ArchiveFile"/>s.
    /// </summary>
    [Category(nameof(File))]
    [Description("Occurs when the historic ArchiveFile list is updated to reflect addition or deletion of historic ArchiveFiles.")]
    public event EventHandler HistoricFileListUpdated;

    /// <summary>
    /// Occurs when <see cref="IDataPoint"/> is received for which a <see cref="StateRecord"/> or <see cref="MetadataRecord"/> does not exist or is marked as disabled.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when IDataPoint is received for which a StateRecord or MetadataRecord does not exist or is marked as disabled.")]
    public event EventHandler<EventArgs<IDataPoint>> OrphanDataReceived;

    /// <summary>
    /// Occurs when <see cref="IDataPoint"/> is received with <see cref="TimeTag"/> ahead of the local clock by more than the <see cref="LeadTimeTolerance"/>.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when IDataPoint is received with TimeTag ahead of the local clock by more than the LeadTimeTolerance.")]
    public event EventHandler<EventArgs<IDataPoint>> FutureDataReceived;

    /// <summary>
    /// Occurs when <see cref="IDataPoint"/> that belongs to a historic <see cref="ArchiveFile"/> is received for archival.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when IDataPoint that belongs to a historic ArchiveFile is received for archival.")]
    public event EventHandler<EventArgs<IDataPoint>> HistoricDataReceived;

    /// <summary>
    /// Occurs when misaligned (by time) <see cref="IDataPoint"/> is received for archival.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when misaligned (by time) IDataPoint is received for archival.")]
    public event EventHandler<EventArgs<IDataPoint>> OutOfSequenceDataReceived;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered while reading <see cref="IDataPoint"/> from the current or historic <see cref="ArchiveFile"/>.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when an Exception is encountered while reading IDataPoint from the current or historic ArchiveFile.")]
    public event EventHandler<EventArgs<Exception>> DataReadException;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered while writing <see cref="IDataPoint"/> to the current or historic <see cref="ArchiveFile"/>.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when an Exception is encountered while writing IDataPoint to the current or historic ArchiveFile.")]
    public event EventHandler<EventArgs<Exception>> DataWriteException;

    /// <summary>
    /// Occurs when <see cref="IDataPoint"/> triggers an alarm notification.
    /// </summary>
    [Category(nameof(File))]
    [Description("Occurs when IDataPoint triggers an alarm notification.")]
    public event EventHandler<EventArgs<StateRecord>> ProcessAlarmNotification;

    /// <summary>
    /// Occurs when associated Metadata file is updated.
    /// </summary>
    [Category("Metadata")]
    [Description("Occurs when associated Metadata file is updated.")]
    public event EventHandler MetadataUpdated;

    // Fields

    // Component
    private string m_fileName;
    private ArchiveFileType m_fileType;
    private double m_fileSize;
    private FileAccess m_fileAccessMode;
    private int m_dataBlockSize;
    private double m_rolloverPreparationThreshold;
    private int m_archiveOffloadCount;
    private double m_archiveOffloadThreshold;
    private int m_maxHistoricArchiveFiles;
    private double m_leadTimeTolerance;
    private bool m_conserveMemory;

    // Operational
    private bool m_initialized;
    private List<ArchiveDataBlock> m_dataBlocks;
    private List<Info> m_historicArchiveFiles;
    private readonly Dictionary<int, decimal> m_delayedAlarmProcessing;
    private volatile bool m_rolloverInProgress;
    private long m_activeFileReaders;

    // Threading
    private Thread m_rolloverPreparationThread;
    private Thread m_buildHistoricFileListThread;

    // Components
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
        ArchiveOffloadLocation = DefaultArchiveOffloadLocation;
        m_archiveOffloadCount = DefaultArchiveOffloadCount;
        m_archiveOffloadThreshold = DefaultArchiveOffloadThreshold;
        ArchiveOffloadMaxAge = DefaultArchiveOffloadMaxAge;
        m_maxHistoricArchiveFiles = DefaultMaxHistoricArchiveFiles;
        m_leadTimeTolerance = DefaultLeadTimeTolerance;
        CompressData = DefaultCompressData;
        DiscardOutOfSequenceData = DefaultDiscardOutOfSequenceData;
        CacheWrites = DefaultCacheWrites;
        m_conserveMemory = DefaultConserveMemory;
        MonitorNewArchiveFiles = DefaultMonitorNewArchiveFiles;
        PersistSettings = DefaultPersistSettings;
        Name = DefaultSettingsCategory;

        m_delayedAlarmProcessing = new Dictionary<int, decimal>();
        RolloverWaitHandle = new ManualResetEvent(true);
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
        container?.Add(this);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the name of the <see cref="ArchiveFile"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
    /// <exception cref="ArgumentException">The value being assigned contains an invalid file extension.</exception>
    [Category(nameof(Configuration))]
    [DefaultValue(DefaultFileName)]
    [Description("Name of the ArchiveFile.")]
    public string FileName
    {
        get => m_fileName;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            if (string.Compare(FilePath.GetExtension(value), FileExtension, StringComparison.OrdinalIgnoreCase) != 0 &&
                string.Compare(FilePath.GetExtension(value), StandbyFileExtension, StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException($"{GetType().Name} must have an extension of {FileExtension} or {StandbyFileExtension}.");

            m_fileName = value;
            ReOpen();
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="ArchiveFileType"/> of the <see cref="ArchiveFile"/>.
    /// </summary>
    [Category(nameof(Configuration))]
    [DefaultValue(DefaultFileType)]
    [Description("Type of the ArchiveFile.")]
    public ArchiveFileType FileType
    {
        get => m_fileType;
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
    [Category(nameof(Configuration))]
    [DefaultValue(DefaultFileSize)]
    [Description("Size (in MB) of the ArchiveFile.")]
    public double FileSize
    {
        get => m_fileSize;
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
    [Category(nameof(Configuration))]
    [DefaultValue(DefaultFileAccessMode)]
    [Description("System.IO.FileAccess value to use when opening the ArchiveFile.")]
    public FileAccess FileAccessMode
    {
        get => m_fileAccessMode;
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
    [Category(nameof(Configuration))]
    [DefaultValue(DefaultDataBlockSize)]
    [Description("Size (in KB) of the ArchiveDataBlocks.")]
    public int DataBlockSize
    {
        get
        {
            // This is the only redundant property between the ArchiveFile component and the FAT, so
            // we ensure that this information is synced at least at run time if not at design time.
            if (Fat is null)
                return m_dataBlockSize; // Design time.

            return Fat.DataBlockSize; // Run time.
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
    [Category(nameof(Rollover))]
    [DefaultValue(DefaultRolloverPreparationThreshold)]
    [Description("ArchiveFile usage (in %) that will trigger the creation of an empty ArchiveFile for rollover.")]
    public double RolloverPreparationThreshold
    {
        get => m_rolloverPreparationThreshold;
        set
        {
            if (value is < 1.0D or > 95.0D)
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(RolloverPreparationThreshold)} value must be between 1 and 95");

            m_rolloverPreparationThreshold = value;
        }
    }

    /// <summary>
    /// Gets or sets the number of historic <see cref="ArchiveFile"/>s to be offloaded to the <see cref="ArchiveOffloadLocation"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
    [Category("Archive")]
    [DefaultValue(DefaultArchiveOffloadCount)]
    [Description("Number of historic ArchiveFiles to be offloaded to the ArchiveOffloadLocation.")]
    public int ArchiveOffloadCount
    {
        get => m_archiveOffloadCount;
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
    [Category("Archive")]
    [DefaultValue(DefaultArchiveOffloadLocation)]
    [Description("Path to the directory where historic ArchiveFiles are to be offloaded to make space in the primary archive location. Set to *DELETE* to remove files instead of offloading.")]
    public string ArchiveOffloadLocation { get; set; }

    /// <summary>
    /// Gets or sets the free disk space (in %) of the primary archive location that triggers the offload of historic <see cref="ArchiveFile"/>s.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value being assigned is not between 0 and 99.</exception>
    [Category("Archive")]
    [DefaultValue(DefaultArchiveOffloadThreshold)]
    [Description("Free disk space (in %) of the primary archive location that triggers the offload of historic ArchiveFiles.")]
    public double ArchiveOffloadThreshold
    {
        get => m_archiveOffloadThreshold;
        set
        {
            if (value is <= 0.0D or > 99.0D)
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(ArchiveOffloadThreshold)} value must be between 0 and 99");

            m_archiveOffloadThreshold = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of days before an archive file triggers the offload of historic <see cref="ArchiveFile"/>s.
    /// </summary>
    [Category("Archive")]
    [DefaultValue(DefaultArchiveOffloadMaxAge)]
    [Description("Maximum number of days before an archive file will be offloaded.")]
    public int ArchiveOffloadMaxAge { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of historic <see cref="ArchiveFile"/>s to be kept at both the primary and offload locations combined.
    /// </summary>
    /// <remarks>
    /// Set <see cref="MaxHistoricArchiveFiles"/> to -1 to keep historic <see cref="ArchiveFile"/>s indefinitely.
    /// </remarks>
    [Category("Archive")]
    [DefaultValue(DefaultMaxHistoricArchiveFiles)]
    [Description("Gets or sets the maximum number of historic ArchiveFiles to be kept at both the primary and offload locations combined.")]
    public int MaxHistoricArchiveFiles
    {
        get => m_maxHistoricArchiveFiles;
        set => m_maxHistoricArchiveFiles = value < 1 ? -1 : value;
    }

    /// <summary>
    /// Gets or sets the number of minutes by which incoming <see cref="ArchiveDataPoint"/> can be ahead of local system clock and still be considered valid.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not zero or positive.</exception>
    [Category(nameof(Data))]
    [DefaultValue(DefaultLeadTimeTolerance)]
    [Description("Number of minutes by which incoming ArchiveDataPoint can be ahead of local system clock and still be considered valid.")]
    public double LeadTimeTolerance
    {
        get => m_leadTimeTolerance;
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
    [Category(nameof(Data))]
    [DefaultValue(DefaultCompressData)]
    [Description("Indicates whether incoming ArchiveDataPoints are to be compressed to save space.")]
    public bool CompressData { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether incoming <see cref="ArchiveDataPoint"/>s with out-of-sequence <see cref="TimeTag"/> are to be discarded.
    /// </summary>
    [Category(nameof(Data))]
    [DefaultValue(DefaultDiscardOutOfSequenceData)]
    [Description("Indicates whether incoming ArchiveDataPoints with out-of-sequence TimeTag are to be discarded.")]
    public bool DiscardOutOfSequenceData { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether writes to the disk are to be cached for performance efficiency.
    /// </summary>
    [Category("Performance")]
    [DefaultValue(DefaultCacheWrites)]
    [Description("indicates whether writes to the disk are to be cached for performance efficiency.")]
    public bool CacheWrites { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether memory usage is to be kept low for performance efficiency.
    /// </summary>
    [Category("Performance")]
    [DefaultValue(DefaultConserveMemory)]
    [Description("Indicates whether memory usage is to be kept low for performance efficiency.")]
    public bool ConserveMemory
    {
        get => m_conserveMemory;
        set
        {
            m_conserveMemory = value;
            ReOpen();
        }
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether to monitor and load newly encountered archive files.
    /// </summary>
    [Category(nameof(Data))]
    [DefaultValue(DefaultMonitorNewArchiveFiles)]
    [Description("Indicates whether to monitor and load newly encountered archive files.")]
    public bool MonitorNewArchiveFiles { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="StateFile"/> used by the <see cref="ArchiveFile"/>.
    /// </summary>
    [Category("Dependencies")]
    [Description("StateFile used by the ArchiveFile.")]
    public StateFile StateFile { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="MetadataFile"/> used by the <see cref="ArchiveFile"/>.
    /// </summary>
    [Category("Dependencies")]
    [Description("MetadataFile used by the ArchiveFile.")]
    public MetadataFile MetadataFile
    {
        get => m_metadataFile;
        set
        {
            // Detach events from any existing instance
            if (m_metadataFile is not null)
                m_metadataFile.FileModified -= MetadataFile_FileModified;

            m_metadataFile = value;

            // Attach events to new instance
            if (m_metadataFile is not null)
                m_metadataFile.FileModified += MetadataFile_FileModified;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="IntercomFile"/> used by the <see cref="ArchiveFile"/>.
    /// </summary>
    [Category("Dependencies")]
    [Description("IntercomFile used by the ArchiveFile.")]
    public IntercomFile IntercomFile { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the settings of <see cref="ArchiveFile"/> are to be saved to the config file.
    /// </summary>
    [Category("Persistence")]
    [DefaultValue(DefaultPersistSettings)]
    [Description("Indicates whether the settings of ArchiveFile are to be saved to the config file.")]
    public bool PersistSettings { get; set; }

    /// <summary>
    /// Gets or sets the category under which the settings of <see cref="ArchiveFile"/> are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
    [Category("Persistence")]
    [DefaultValue(DefaultSettingsCategory)]
    [Description("Category under which the settings of ArchiveFile are to be saved to the config file if the PersistSettings property is set to true.")]
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
    /// Gets or sets a boolean value that indicates whether the <see cref="ArchiveFile"/> is currently enabled.
    /// </summary>
    /// <remarks>
    /// <see cref="Enabled"/> property is not be set by user-code directly.
    /// </remarks>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the <see cref="ArchiveFile"/>.
    /// </summary>
    [Browsable(false)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets the descriptive status of the <see cref="ArchiveFile"/>.
    /// </summary>
    [Browsable(false)]
    public string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendLine($"                 File name: {FilePath.TrimFileName(m_fileName, 30)}");
            status.AppendLine($"                File state: {(IsOpen ? nameof(Open) : "Closed")}");
            status.AppendLine($"          File access mode: {m_fileAccessMode}");

            if (IsOpen)
            {
                ArchiveFileStatistics statistics = Statistics;

                status.AppendLine($"                File usage: {statistics.FileUsage:0.00}%");
                status.AppendLine($"          Compression rate: {statistics.CompressionRate:0.00}%");
                status.AppendLine($"      Data points received: {Fat.DataPointsReceived:N0}");
                status.AppendLine($"      Data points archived: {Fat.DataPointsArchived:N0}");
                status.AppendLine($"       Average write speed: {statistics.AverageWriteSpeed:N0} per Second");
                status.AppendLine($"          Averaging window: {statistics.AveragingWindow.ToString(3)}");

                // ReSharper disable once InconsistentlySynchronizedField
                if (m_historicArchiveFiles is not null)
                {
                    status.Append("    Historic archive files: ");

                    lock (m_historicArchiveFiles)
                        status.Append(m_historicArchiveFiles.Count);

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
    public bool IsOpen => FileData is not null && Fat is not null;

    /// <summary>
    /// Gets the underlying <see cref="FileStream"/> of the <see cref="ArchiveFile"/>.
    /// </summary>
    [Browsable(false)]
    public FileStream FileData { get; private set; }

    /// <summary>
    /// Gets the <see cref="ArchiveFileAllocationTable"/> of the <see cref="ArchiveFile"/>.
    /// </summary>
    [Browsable(false)]
    public ArchiveFileAllocationTable Fat { get; private set; }

    /// <summary>
    /// Gets the <see cref="ArchiveFileStatistics"/> object of the <see cref="ArchiveFile"/>.
    /// </summary>
    [Browsable(false)]
    public ArchiveFileStatistics Statistics
    {
        get
        {
            if (!IsOpen)
                return new ArchiveFileStatistics();
            
            ArchiveFileStatistics statistics = new();

            // Calculate file usage.
            IntercomRecord system = IntercomFile.Read(1);

            if (m_fileType == ArchiveFileType.Active && system is not null)
                statistics.FileUsage = system.DataBlocksUsed / (float)Fat.DataBlockCount * 100;
            else
                statistics.FileUsage = Fat.DataBlocksUsed / (float)Fat.DataBlockCount * 100;

            // Calculate compression rate.
            if (Fat.DataPointsReceived >= 1)
                statistics.CompressionRate = (Fat.DataPointsReceived - Fat.DataPointsArchived) / (float)Fat.DataPointsReceived * 100;

            if (m_currentDataQueue.RunTime >= 1.0D)
            {
                statistics.AveragingWindow = m_currentDataQueue.RunTime;

                statistics.AverageWriteSpeed = (int)(
                    (m_currentDataQueue.CurrentStatistics.TotalProcessedItems -
                     (m_historicDataQueue.CurrentStatistics.TotalProcessedItems +
                      m_historicDataQueue.CurrentStatistics.QueueCount +
                      m_historicDataQueue.CurrentStatistics.ItemsBeingProcessed +
                      m_outOfSequenceDataQueue.CurrentStatistics.TotalProcessedItems +
                      m_outOfSequenceDataQueue.CurrentStatistics.QueueCount +
                      m_outOfSequenceDataQueue.CurrentStatistics.ItemsBeingProcessed)) / (long)statistics.AveragingWindow);
            }

            return statistics;
        }
    }

    /// <summary>
    /// Gets the <see cref="ProcessQueueStatistics"/> for the internal current data write <see cref="ProcessQueue{T}"/>.
    /// </summary>
    [Browsable(false)]
    public ProcessQueueStatistics CurrentWriteStatistics => m_currentDataQueue.CurrentStatistics;

    /// <summary>
    /// Gets the <see cref="ProcessQueueStatistics"/> for the internal historic data write <see cref="ProcessQueue{T}"/>.
    /// </summary>
    [Browsable(false)]
    public ProcessQueueStatistics HistoricWriteStatistics => m_historicDataQueue.CurrentStatistics;

    /// <summary>
    /// Gets the <see cref="ProcessQueueStatistics"/> for the internal out-of-sequence data write <see cref="ProcessQueue{T}"/>.
    /// </summary>
    [Browsable(false)]
    public ProcessQueueStatistics OutOfSequenceWriteStatistics => m_outOfSequenceDataQueue.CurrentStatistics;

    /// <summary>
    /// Gets wait handle used to synchronize roll-over access.
    /// </summary>
    protected internal ManualResetEvent RolloverWaitHandle { get; }

    /// <summary>
    /// Gets the name of the standby <see cref="ArchiveFile"/>.
    /// </summary>
    private string StandbyArchiveFileName => Path.ChangeExtension(m_fileName, StandbyFileExtension);

    /// <summary>
    /// Gets the name to be used when promoting an active <see cref="ArchiveFile"/> to historic <see cref="ArchiveFile"/>.
    /// </summary>
    private string HistoryArchiveFileName => FilePath.GetDirectoryName(m_fileName) + (FilePath.GetFileNameWithoutExtension(m_fileName) + "_" + Fat.FileStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "_to_" + Fat.FileEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + FileExtension).Replace(':', '!');

    /// <summary>
    /// Gets the pattern to be used when searching for historic <see cref="ArchiveFile"/>s.
    /// </summary>
    private string HistoricFilesSearchPattern => $"{FilePath.GetFileNameWithoutExtension(m_fileName)}_*_to_*{FileExtension}";

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
        if (m_initialized)
            return;

        LoadSettings();         // Load settings from the config file.

        m_initialized = true;   // Initialize only once.

        if (!MonitorNewArchiveFiles)
            return;
        
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
                //if (m_currentLocationFileWatcher is not null)
                //    m_currentLocationFileWatcher.BeginInit();

                //if (m_offloadLocationFileWatcher is not null)
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
        if (!PersistSettings)
            return;
        
        // Ensure that settings category is specified.
        if (string.IsNullOrEmpty(Name))
            throw new ConfigurationErrorsException("SettingsCategory property has not been set");

        // Save settings under the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[Name];

        settings[nameof(FileName), true].Update(m_fileName);
        settings[nameof(FileType), true].Update(m_fileType);
        settings[nameof(FileSize), true].Update(m_fileSize);
        settings[nameof(DataBlockSize), true].Update(m_dataBlockSize);
        settings[nameof(RolloverPreparationThreshold), true].Update(m_rolloverPreparationThreshold);
        settings[nameof(ArchiveOffloadLocation), true].Update(ArchiveOffloadLocation);
        settings[nameof(ArchiveOffloadCount), true].Update(m_archiveOffloadCount);
        settings[nameof(ArchiveOffloadThreshold), true].Update(m_archiveOffloadThreshold);
        settings[nameof(ArchiveOffloadMaxAge), true].Update(ArchiveOffloadMaxAge);
        settings[nameof(MaxHistoricArchiveFiles), true].Update(m_maxHistoricArchiveFiles);
        settings[nameof(LeadTimeTolerance), true].Update(m_leadTimeTolerance);
        settings[nameof(CompressData), true].Update(CompressData);
        settings[nameof(DiscardOutOfSequenceData), true].Update(DiscardOutOfSequenceData);
        settings[nameof(CacheWrites), true].Update(CacheWrites);
        settings[nameof(ConserveMemory), true].Update(m_conserveMemory);
        settings[nameof(MonitorNewArchiveFiles), true].Update(MonitorNewArchiveFiles);

        config.Save();
    }

    /// <summary>
    /// Loads saved settings for the <see cref="ArchiveFile"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
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

        settings.Add(nameof(FileName), m_fileName, "Name of the file including its path.");
        settings.Add(nameof(FileType), m_fileType, "Type (Active; Standby; Historic) of the file.");
        settings.Add(nameof(FileSize), m_fileSize, "Size (in MB) of the file. Typical size = 100.");
        settings.Add(nameof(DataBlockSize), m_dataBlockSize, "Size (in KB) of the data blocks in the file.");
        settings.Add(nameof(RolloverPreparationThreshold), m_rolloverPreparationThreshold, "Percentage file full when the rollover preparation should begin.");
        settings.Add(nameof(ArchiveOffloadLocation), ArchiveOffloadLocation, "Path to the location where historic files are to be moved when disk start getting full. Set to *DELETE* to remove files instead of offloading.");
        settings.Add(nameof(ArchiveOffloadCount), m_archiveOffloadCount, "Number of files that are to be moved to the offload location when the disk starts getting full.");
        settings.Add(nameof(ArchiveOffloadThreshold), m_archiveOffloadThreshold, "Percentage disk full when the historic files should be moved to the offload location.");
        settings.Add(nameof(ArchiveOffloadMaxAge), ArchiveOffloadMaxAge, "Maximum number of days before an archive file will be offloaded.");
        settings.Add(nameof(MaxHistoricArchiveFiles), m_maxHistoricArchiveFiles, "Maximum number of historic files to be kept at both the primary and offload locations combined.");
        settings.Add(nameof(LeadTimeTolerance), m_leadTimeTolerance, "Number of minutes by which incoming data points can be ahead of local system clock and still be considered valid.");
        settings.Add(nameof(CompressData), CompressData, "True if compression (swinging door - lossy) is to be performed on the incoming data points; otherwise False.");
        settings.Add(nameof(DiscardOutOfSequenceData), DiscardOutOfSequenceData, "True if out-of-sequence data points are to be discarded; otherwise False.");
        settings.Add(nameof(CacheWrites), CacheWrites, "True if writes are to be cached for performance; otherwise False.");
        settings.Add(nameof(ConserveMemory), m_conserveMemory, "True if attempts are to be made to conserve memory; otherwise False.");
        settings.Add(nameof(MonitorNewArchiveFiles), MonitorNewArchiveFiles, "True to monitor and load newly encountered archive files; otherwise False.");

        FileName = settings[nameof(FileName)].ValueAs(m_fileName);
        FileType = settings[nameof(FileType)].ValueAs(m_fileType);
        FileSize = settings[nameof(FileSize)].ValueAs(m_fileSize);
        DataBlockSize = settings[nameof(DataBlockSize)].ValueAs(m_dataBlockSize);
        RolloverPreparationThreshold = settings[nameof(RolloverPreparationThreshold)].ValueAs(m_rolloverPreparationThreshold);
        ArchiveOffloadLocation = settings[nameof(ArchiveOffloadLocation)].ValueAs(ArchiveOffloadLocation);
        ArchiveOffloadCount = settings[nameof(ArchiveOffloadCount)].ValueAs(m_archiveOffloadCount);
        ArchiveOffloadThreshold = settings[nameof(ArchiveOffloadThreshold)].ValueAs(m_archiveOffloadThreshold);
        ArchiveOffloadMaxAge = settings[nameof(ArchiveOffloadMaxAge)].ValueAs(ArchiveOffloadMaxAge);
        MaxHistoricArchiveFiles = settings[nameof(MaxHistoricArchiveFiles)].ValueAs(m_maxHistoricArchiveFiles);
        LeadTimeTolerance = settings[nameof(LeadTimeTolerance)].ValueAs(m_leadTimeTolerance);
        CompressData = settings[nameof(CompressData)].ValueAs(CompressData);
        DiscardOutOfSequenceData = settings[nameof(DiscardOutOfSequenceData)].ValueAs(DiscardOutOfSequenceData);
        CacheWrites = settings[nameof(CacheWrites)].ValueAs(CacheWrites);
        ConserveMemory = settings[nameof(ConserveMemory)].ValueAs(m_conserveMemory);
    }

    /// <summary>
    /// Opens the <see cref="ArchiveFile"/> for use.
    /// </summary>
    /// <exception cref="InvalidOperationException">One or all of the <see cref="StateFile"/>, <see cref="IntercomFile"/> or <see cref="MetadataFile"/> properties are not set.</exception>
    public void Open()
    {
        if (IsOpen)
            return;

        // Check for the existence of dependencies.
        if (StateFile is null || IntercomFile is null || m_metadataFile is null || m_fileName is null)
            throw new InvalidOperationException("One or more of the dependency files are not specified.");

        // Validate file type against its name.
        if (Path.GetExtension(m_fileName).ToNonNullString().ToLower() == StandbyFileExtension)
            m_fileType = ArchiveFileType.Standby;
        else if (Regex.IsMatch(m_fileName.ToLower(), $".+_.+_to_.+\\{FileExtension}$"))
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
            if (!IntercomFile.IsOpen)
                IntercomFile.Open();

            IntercomFile.Load();
            IntercomRecord record = IntercomFile.Read(1);
            int waitCount = 0;

            while (record is not null && record.RolloverInProgress && waitCount < 30)
            {
                Thread.Sleep(1000);
                IntercomFile.Load();
                record = IntercomFile.Read(1);
                waitCount++;
            }
        }

        OpenStream();

        // Open state file if closed.
        if (!StateFile.IsOpen)
            StateFile.Open();

        // Open intercom file if closed.
        if (!IntercomFile.IsOpen)
            IntercomFile.Open();

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
        if (StateFile.RecordsInMemory > 0)
            m_dataBlocks = [..new ArchiveDataBlock[StateFile.RecordsInMemory]];
        else
            m_dataBlocks = [..new ArchiveDataBlock[StateFile.RecordsOnDisk]];

        // Validate the dependency files.
        SyncStateFile(null);

        if (IntercomFile.FileAccessMode != FileAccess.Read)
        {
            // Ensure that "rollover in progress" is not set.
            IntercomRecord system = IntercomFile.Read(1) ?? new IntercomRecord(1);
            system.RolloverInProgress = false;
            IntercomFile.Write(1, system);
        }

        // Start the memory conservation process.
        if (m_conserveMemory)
        {
            m_conserveMemoryTimer.Interval = DataBlockCheckInterval;
            m_conserveMemoryTimer.Start();
        }

        if (m_fileType != ArchiveFileType.Active)
            return;

        // Start preparing the list of historic files.
        m_buildHistoricFileListThread = new Thread(BuildHistoricFileList) { Priority = ThreadPriority.Lowest };
        m_buildHistoricFileListThread.Start();

        // Start file watchers to monitor file system changes.
        if (!MonitorNewArchiveFiles)
            return;

        if (m_currentLocationFileWatcher is not null)
        {
            m_currentLocationFileWatcher.Filter = HistoricFilesSearchPattern;
            m_currentLocationFileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
            m_currentLocationFileWatcher.EnableRaisingEvents = true;
        }

        if (Directory.Exists(ArchiveOffloadLocation) && m_offloadLocationFileWatcher is not null)
        {
            m_offloadLocationFileWatcher.Filter = HistoricFilesSearchPattern;
            m_offloadLocationFileWatcher.Path = ArchiveOffloadLocation;
            m_offloadLocationFileWatcher.EnableRaisingEvents = true;
        }
    }

    /// <summary>
    /// Closes the <see cref="ArchiveFile"/> if it <see cref="IsOpen"/>.
    /// </summary>
    public void Close()
    {
        if (!IsOpen)
            return;

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

        if (m_dataBlocks is not null)
        {
            lock (m_dataBlocks)
                m_dataBlocks.Clear();
            
            m_dataBlocks = null;
        }

        // Stop watching for historic archive files.
        if (m_currentLocationFileWatcher is not null)
            m_currentLocationFileWatcher.EnableRaisingEvents = false;

        if (m_offloadLocationFileWatcher is not null)
            m_offloadLocationFileWatcher.EnableRaisingEvents = false;

        // Clear the list of historic archive files.
        if (m_historicArchiveFiles is not null)
        {
            lock (m_historicArchiveFiles)
                m_historicArchiveFiles.Clear();
            
            m_historicArchiveFiles = null;
        }
    }

    /// <summary>
    /// Saves the <see cref="ArchiveFile"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="ArchiveFile"/> is not open.</exception>
    public void Save()
    {
        if (IsOpen)
            Fat.Save();
        else
            throw new InvalidOperationException($"\"{m_fileName}\" is not open");
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
            RolloverWaitHandle.Reset();

            // Raise roll-over start event - this also sets m_rolloverInProgress flag
            OnRolloverStart();

            // Notify external components about the rollover.
            IntercomRecord system = IntercomFile.Read(1);
            system.DataBlocksUsed = 0;
            system.RolloverInProgress = true;
            system.LatestDataID = -1;
            system.LatestDataTime = TimeTag.MinValue;
            IntercomFile.Write(1, system);
            IntercomFile.Save();

            // Figure out the end date for this file.
            TimeTag endTime = Fat.FileEndTime;

            for (int i = 1; i <= StateFile.RecordsOnDisk; i++)
            {
                StateRecord state = StateFile.Read(i);
                state.ActiveDataBlockIndex = -1;
                state.ActiveDataBlockSlot = 1;

                if (state.ArchivedData.Time.CompareTo(endTime) > 0)
                    endTime = state.ArchivedData.Time;

                StateFile.Write(state.HistorianID, state);
            }

            Fat.FileEndTime = endTime;

            StateFile.Save();
            Save();

            // Wait for any pending readers to release
            WaitForReadersRelease();

            // Clear all the cached data blocks.
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

            // CRITICAL: Exception can be encountered if a "standby" archive is not present for us to use, and
            //           we cannot create a new archive file probably because there isn't enough disk space.
            try
            {
                OpenStream();

                Fat.FileStartTime = endTime;
                Fat.Save();

                // Notify server that rollover is complete.
                system.RolloverInProgress = false;
                IntercomFile.Write(1, system);
                IntercomFile.Save();

                // Raise roll-over complete event - this also resets m_rolloverInProgress flag
                OnRolloverComplete();

                // Notify other threads that rollover is complete.
                RolloverWaitHandle.Set();
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
        // Yield to archive rollover process.
        RolloverWaitHandle.WaitOne();

        // Ensure that the current file is open.
        if (!IsOpen)
            throw new InvalidOperationException($"\"{m_fileName}\" file is not open");

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
            WriteData(dataPoint);
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
    /// <remarks>
    /// Data is always read from <paramref name="startTime"/> to <paramref name="endTime"/>. If <paramref name="startTime"/> is
    /// greater than <paramref name="endTime"/>, query data will be read from the archive in reverse time order.
    /// </remarks>
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
    /// <remarks>
    /// Data is always read from <paramref name="startTime"/> to <paramref name="endTime"/>. If <paramref name="startTime"/> is
    /// greater than <paramref name="endTime"/>, query data will be read from the archive in reverse time order.
    /// </remarks>
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
    /// <remarks>
    /// Data is always read from <paramref name="startTime"/> to <paramref name="endTime"/>. If <paramref name="startTime"/> is
    /// greater than <paramref name="endTime"/>, query data will be read from the archive in reverse time order.
    /// </remarks>
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
    /// <remarks>
    /// Data is always read from <paramref name="startTime"/> to <paramref name="endTime"/>. If <paramref name="startTime"/> is
    /// greater than <paramref name="endTime"/>, query data will be read from the archive in reverse time order.
    /// </remarks>
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
    /// <remarks>
    /// Data is always read from <paramref name="startTime"/> to <paramref name="endTime"/>. If <paramref name="startTime"/> is
    /// greater than <paramref name="endTime"/>, query data will be read from the archive in reverse time order.
    /// </remarks>
    public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime, TimeTag endTime, bool timeSorted = true)
    {
        return ReadData([historianID], startTime, endTime, timeSorted);
    }

    /// <summary>
    /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
    /// </summary>
    /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
    /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
    /// <param name="endTime">End <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
    /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
    /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
    /// <remarks>
    /// Data is always read from <paramref name="startTime"/> to <paramref name="endTime"/>. If <paramref name="startTime"/> is
    /// greater than <paramref name="endTime"/>, query data will be read from the archive in reverse time order.
    /// </remarks>
    public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, TimeTag endTime, bool timeSorted = true)
    {
        return ReadData(historianIDs, startTime, endTime, null, timeSorted);
    }

    // Read data implementation
    private IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, TimeTag endTime, IDataPoint resumeFrom, bool timeSorted)
    {
        // Yield to archive rollover process.
        RolloverWaitHandle.WaitOne();

        // Ensure that the current file is open.
        if (!IsOpen)
            throw new InvalidOperationException($"\"{m_fileName}\" file is not open");

        // Ensure that the current file is active.
        if (m_fileType != ArchiveFileType.Active)
            throw new InvalidOperationException("Data can only be directly read from files that are Active");

        bool reverseQuery = false;

        // If start time is greater than end time, assuming reverse query is requested
        if (startTime.CompareTo(endTime) > 0)
        {
            reverseQuery = true;
            (startTime, endTime) = (endTime, startTime); // Swap start and end time
        }

        List<Info> dataFiles = [];
        bool pendingRollover = false;
        bool usingActiveFile = false;

        if (startTime.CompareTo(Fat.FileStartTime) < 0)
        {
            // Data is to be read from historic file(s) - make sure that the list has been built
            if (m_buildHistoricFileListThread.IsAlive)
                m_buildHistoricFileListThread.Join();

            lock (m_historicArchiveFiles)
                dataFiles.AddRange(m_historicArchiveFiles.FindAll(info => FindHistoricArchiveFileForRead(info, startTime, endTime)));
        }

        if (endTime.CompareTo(Fat.FileStartTime) >= 0)
        {
            // Data is to be read from the active file.
            Info activeFileInfo = new(m_fileName)
            {
                StartTimeTag = Fat.FileStartTime,
                EndTimeTag = Fat.FileEndTime
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
                        RolloverWaitHandle.WaitOne();
                        Interlocked.Increment(ref m_activeFileReaders);
                    }
                }
                else
                {
                    // Read data from historic file.
                    usingActiveFile = false;
                    file = new ArchiveFile();
                    file.FileName = dataFile.FileName;
                    file.StateFile = StateFile;
                    file.IntercomFile = IntercomFile;
                    file.MetadataFile = m_metadataFile;
                    file.FileAccessMode = FileAccess.Read;
                    file.Open();
                }

                // Create new data point scanner for the desired points in this file and given time range
                IArchiveFileScanner scanner;

                if (timeSorted)
                    scanner = new TimeSortedArchiveFileScanner(reverseQuery);
                else
                    scanner = new ArchiveFileScanner(reverseQuery);

                scanner.FileAllocationTable = file.Fat;
                scanner.HistorianIDs = historianIDs;
                scanner.StartTime = startTime;
                scanner.EndTime = endTime;
                scanner.ResumeFrom = resumeFrom;
                scanner.DataReadExceptionHandler = (_, e) => OnDataReadException(e.Argument);

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
                else
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
                yield return dataPoint;
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
        return record?.BinaryImage();
    }

    /// <summary>
    /// Reads <see cref="StateRecord"/> for the specified <paramref name="historianID"/>.
    /// </summary>
    /// <param name="historianID">Historian identifier.</param>
    /// <returns>A <see cref="byte"/> array containing <see cref="StateRecord"/> of <see cref="StateRecord"/> if found; otherwise null.</returns>
    public byte[] ReadStateData(int historianID)
    {
        StateRecord record = StateFile.Read(historianID);
        return record?.BinaryImage();
    }

    /// <summary>
    /// Reads <see cref="MetadataRecordSummary"/> for the specified <paramref name="historianID"/>.
    /// </summary>
    /// <param name="historianID">Historian identifier.</param>
    /// <returns>A <see cref="byte"/> array containing <see cref="MetadataRecordSummary"/> of <see cref="MetadataRecordSummary"/> if found; otherwise null.</returns>
    public byte[] ReadMetaDataSummary(int historianID)
    {
        MetadataRecord record = MetadataFile.Read(historianID);
        return record?.Summary.BinaryImage();
    }

    /// <summary>
    /// Reads <see cref="StateRecordSummary"/> for the specified <paramref name="historianID"/>.
    /// </summary>
    /// <param name="historianID">Historian identifier.</param>
    /// <returns>A <see cref="byte"/> array containing <see cref="StateRecordSummary"/> of <see cref="StateRecordSummary"/> if found; otherwise null.</returns>
    public byte[] ReadStateDataSummary(int historianID)
    {
        StateRecord record = StateFile.Read(historianID);
        return record?.Summary.BinaryImage();
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

        return Interlocked.Read(ref m_activeFileReaders) <= 0;
    }

    /// <summary>
    /// Raises the <see cref="FileFull"/> event.
    /// </summary>
    protected virtual void OnFileFull()
    {
        FileFull?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="RolloverStart"/> event.
    /// </summary>
    protected internal virtual void OnRolloverStart()
    {
        m_rolloverInProgress = true;
        RolloverStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="RolloverComplete"/> event.
    /// </summary>
    protected internal virtual void OnRolloverComplete()
    {
        m_rolloverInProgress = false;
        RolloverComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="RolloverException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="RolloverException"/> event.</param>
    protected internal virtual void OnRolloverException(Exception ex)
    {
        m_rolloverInProgress = false;
        RolloverException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="OffloadStart"/> event.
    /// </summary>
    protected virtual void OnOffloadStart()
    {
        OffloadStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="OffloadComplete"/> event.
    /// </summary>
    protected virtual void OnOffloadComplete()
    {
        OffloadComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="OffloadException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="OffloadException"/> event.</param>
    protected virtual void OnOffloadException(Exception ex)
    {
        OffloadException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="OffloadProgress"/> event.
    /// </summary>
    /// <param name="offloadProgress"><see cref="ProcessProgress{T}"/> to send to <see cref="OffloadProgress"/> event.</param>
    protected virtual void OnOffloadProgress(ProcessProgress<int> offloadProgress)
    {
        OffloadProgress?.Invoke(this, new EventArgs<ProcessProgress<int>>(offloadProgress));
    }

    /// <summary>
    /// Raises the <see cref="RolloverPreparationStart"/> event.
    /// </summary>
    protected virtual void OnRolloverPreparationStart()
    {
        RolloverPreparationStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="RolloverPreparationComplete"/> event.
    /// </summary>
    protected virtual void OnRolloverPreparationComplete()
    {
        RolloverPreparationComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="RolloverPreparationException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="RolloverPreparationException"/> event.</param>
    protected virtual void OnRolloverPreparationException(Exception ex)
    {
        RolloverPreparationException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="HistoricFileListBuildStart"/> event.
    /// </summary>
    protected virtual void OnHistoricFileListBuildStart()
    {
        HistoricFileListBuildStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="HistoricFileListBuildComplete"/> event.
    /// </summary>
    protected virtual void OnHistoricFileListBuildComplete()
    {
        HistoricFileListBuildComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raise the <see cref="HistoricFileListBuildException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="HistoricFileListBuildException"/> event.</param>
    protected virtual void OnHistoricFileListBuildException(Exception ex)
    {
        HistoricFileListBuildException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="HistoricFileListUpdated"/> event.
    /// </summary>
    protected virtual void OnHistoricFileListUpdated()
    {
        HistoricFileListUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="OrphanDataReceived"/> event.
    /// </summary>
    /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="OrphanDataReceived"/> event.</param>
    protected virtual void OnOrphanDataReceived(IDataPoint dataPoint)
    {
        OrphanDataReceived?.Invoke(this, new EventArgs<IDataPoint>(dataPoint));
    }

    /// <summary>
    /// Raises the <see cref="FutureDataReceived"/> event.
    /// </summary>
    /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="FutureDataReceived"/> event.</param>
    protected virtual void OnFutureDataReceived(IDataPoint dataPoint)
    {
        FutureDataReceived?.Invoke(this, new EventArgs<IDataPoint>(dataPoint));
    }

    /// <summary>
    /// Raises the <see cref="HistoricDataReceived"/> event.
    /// </summary>
    /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="HistoricDataReceived"/> event.</param>
    protected virtual void OnHistoricDataReceived(IDataPoint dataPoint)
    {
        HistoricDataReceived?.Invoke(this, new EventArgs<IDataPoint>(dataPoint));
    }

    /// <summary>
    /// Raises the <see cref="OutOfSequenceDataReceived"/> event.
    /// </summary>
    /// <param name="dataPoint"><see cref="IDataPoint"/> to send to <see cref="OutOfSequenceDataReceived"/> event.</param>
    protected virtual void OnOutOfSequenceDataReceived(IDataPoint dataPoint)
    {
        OutOfSequenceDataReceived?.Invoke(this, new EventArgs<IDataPoint>(dataPoint));
    }

    /// <summary>
    /// Raises the <see cref="DataReadException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataReadException"/> event.</param>
    protected virtual void OnDataReadException(Exception ex)
    {
        DataReadException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="DataWriteException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataWriteException"/> event.</param>
    protected virtual void OnDataWriteException(Exception ex)
    {
        DataWriteException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="ProcessAlarmNotification"/> event.
    /// </summary>
    /// <param name="pointState"><see cref="StateRecord"/> to send to <see cref="ProcessAlarmNotification"/> event.</param>
    protected virtual void OnProcessAlarmNotification(StateRecord pointState)
    {
        ProcessAlarmNotification?.Invoke(this, new EventArgs<StateRecord>(pointState));
    }

    /// <summary>
    /// Raises the <see cref="MetadataUpdated"/> event.
    /// </summary>
    protected virtual void OnMetadataUpdated()
    {
        MetadataUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ArchiveFile"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;
        
        try
        {
            // This will be done regardless of whether the object is finalized or disposed.
            if (!disposing)
                return;
            
            // This will be done only when the object is disposed by calling Dispose().
            Close();
            SaveSettings();

            RolloverWaitHandle?.Close();

            if (m_conserveMemoryTimer is not null)
            {
                m_conserveMemoryTimer.Elapsed -= ConserveMemoryTimer_Elapsed;
                m_conserveMemoryTimer.Dispose();
            }

            if (m_currentDataQueue is not null)
            {
                m_currentDataQueue.ProcessException -= CurrentDataQueue_ProcessException;
                m_currentDataQueue.Dispose();
            }

            if (m_historicDataQueue is not null)
            {
                m_historicDataQueue.ProcessException -= HistoricDataQueue_ProcessException;
                m_historicDataQueue.Dispose();
            }

            if (m_outOfSequenceDataQueue is not null)
            {
                m_outOfSequenceDataQueue.ProcessException -= OutOfSequenceDataQueue_ProcessException;
                m_outOfSequenceDataQueue.Dispose();
            }

            if (m_currentLocationFileWatcher is not null)
            {
                m_currentLocationFileWatcher.Renamed -= FileWatcher_Renamed;
                m_currentLocationFileWatcher.Deleted -= FileWatcher_Deleted;
                m_currentLocationFileWatcher.Created -= FileWatcher_Created;
                m_currentLocationFileWatcher.Dispose();
            }

            if (m_offloadLocationFileWatcher is not null)
            {
                m_offloadLocationFileWatcher.Renamed -= FileWatcher_Renamed;
                m_offloadLocationFileWatcher.Deleted -= FileWatcher_Deleted;
                m_offloadLocationFileWatcher.Created -= FileWatcher_Created;
                m_offloadLocationFileWatcher.Dispose();
            }

            // Detach from all the dependency files.
            StateFile = null;
            MetadataFile = null;
            IntercomFile = null;
        }
        finally
        {
            IsDisposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    #region [ Helper Methods ]

    private void ReOpen()
    {
        if (!IsOpen)
            return;
        
        Close();
        Open();
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
                        FileData = new FileStream(m_fileName, FileMode.Open, m_fileAccessMode, FileShare.ReadWrite);
                        Fat = new ArchiveFileAllocationTable(this);
                        break;
                    }
                    catch
                    {
                        FileData?.Dispose();

                        if (attempts >= 4)
                            throw;

                        Thread.Sleep(500);
                    }
                }
            }
            else if (m_fileAccessMode == FileAccess.Read)
            {
                // File does not exist, so we create a placeholder file in the temp directory
                FileData = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                Fat = new ArchiveFileAllocationTable(this)
                { 
                    FileStartTime = TimeTag.MaxValue,
                    FileEndTime = TimeTag.MaxValue
                };

                // Manually call file monitoring event if file watchers are not enabled
                if (!MonitorNewArchiveFiles)
                    FileWatcher_Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FilePath.GetAbsolutePath(m_fileName), FilePath.GetFileName(m_fileName)));
            }
            else
            {
                // File does not exist, so we have to create it and initialize it.
                FileData = new FileStream(m_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                Fat = new ArchiveFileAllocationTable(this);
                Fat.Save(true);

                // Manually call file monitoring event if file watchers are not enabled
                if (!MonitorNewArchiveFiles)
                    FileWatcher_Created(this, new FileSystemEventArgs(WatcherChangeTypes.Created, FilePath.GetAbsolutePath(m_fileName), FilePath.GetFileName(m_fileName)));
            }
        }
        catch
        {
            FileData?.Dispose();
            throw;
        }
    }

    internal void CloseStream()
    {
        Fat = null;

        if (FileData is null)
            return;
        
        lock (FileData)
        {
            FileData.Flush();
            FileData.Close();
            FileData.Dispose();
        }
        
        FileData = null;
    }

    private void SyncStateFile(object state)
    {
        lock (this)
        {
            if (m_dataBlocks is null || 
                !StateFile.IsOpen || 
                !m_metadataFile.IsOpen || 
                StateFile.FileAccessMode == FileAccess.Read || 
                m_metadataFile.RecordsOnDisk <= StateFile.RecordsOnDisk)
                return;

            // Since we have more records in the Metadata File than in the State File we'll synchronize
            // the number of records in both the files (very important) by writing a new record to the State
            // File with an ID same as the number of records on disk for Metadata File. Doing so will cause the
            // State File to grow in-memory or on-disk depending on how it's configured.
            StateFile.Write(m_metadataFile.RecordsOnDisk, new StateRecord(m_metadataFile.RecordsOnDisk));
            StateFile.Save();

            // We synchronize the block list with the number of state records physically present on the disk.
            lock (m_dataBlocks)
                m_dataBlocks.AddRange(new ArchiveDataBlock[StateFile.RecordsOnDisk - m_dataBlocks.Count]);
        }
    }

    private void BuildHistoricFileList()
    {
        if (m_historicArchiveFiles is not null)
            return;
        
        // The list of historic files has not been created, so we'll create it.
        try
        {
            m_historicArchiveFiles = [];

            // We can safely assume that we'll always get information about the historic file because, the
            // search pattern ensures that we only can a list of historic archive files and not all files.
            OnHistoricFileListBuildStart();

            // Prevent the historic file list from being updated by the file watchers.
            lock (m_historicArchiveFiles)
            {
                Info historicFileInfo;

                foreach (string historicFileName in Directory.GetFiles(FilePath.GetDirectoryName(m_fileName), HistoricFilesSearchPattern))
                {
                    historicFileInfo = GetHistoricFileInfo(historicFileName);
                    
                    if (historicFileInfo is not null)
                        m_historicArchiveFiles.Add(historicFileInfo);
                }

                if (Directory.Exists(ArchiveOffloadLocation))
                {
                    foreach (string historicFileName in Directory.GetFiles(ArchiveOffloadLocation, HistoricFilesSearchPattern))
                    {
                        historicFileInfo = GetHistoricFileInfo(historicFileName);
                        
                        if (historicFileInfo is not null)
                            m_historicArchiveFiles.Add(historicFileInfo);
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

    [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Implementation is correct")]
    private void PrepareForRollover()
    {
        try
        {
            DriveInfo archiveDrive = new(Path.GetPathRoot(m_fileName).ToNonNullString());

            // We'll start offloading historic files if we've reached the offload threshold.
            if (ArchiveOffloadMaxAge > 0)
                OffloadMaxAgedFiles();

            if (archiveDrive.AvailableFreeSpace < archiveDrive.TotalSize * (1 - m_archiveOffloadThreshold / 100))
                OffloadHistoricFiles();

            // Maintain maximum number of historic files, if configured to do so
            MaintainMaximumNumberOfHistoricFiles();

            OnRolloverPreparationStart();

            // Opening and closing a new archive file in "standby" mode will create a "standby" archive file.
            ArchiveFile standbyArchiveFile = new();
            standbyArchiveFile.FileName = StandbyArchiveFileName;
            standbyArchiveFile.FileSize = m_fileSize;
            standbyArchiveFile.DataBlockSize = m_dataBlockSize;
            standbyArchiveFile.StateFile = StateFile;
            standbyArchiveFile.IntercomFile = IntercomFile;
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
        if (Directory.Exists(ArchiveOffloadLocation))
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
                    newHistoricFiles = m_historicArchiveFiles.FindAll(info => IsNewHistoricArchiveFile(info, m_fileName));

                // Sorting the list will sort the historic files from oldest to newest.
                newHistoricFiles.Sort();

                List<Info> filesToOffload = [];

                foreach (Info file in newHistoricFiles)
                {
                    if ((DateTime.UtcNow - file.StartTimeTag.ToDateTime()).TotalDays > ArchiveOffloadMaxAge)
                        filesToOffload.Add(file);
                }

                // We'll offload the specified number of oldest historic files to the offload location if the
                // number of historic files is more than the offload count or all of the historic files if the
                // offload count is smaller the available number of historic files.
                ProcessProgress<int> offloadProgress = new("FileOffload") { Total = filesToOffload.Count };

                for (int i = 0; i < offloadProgress.Total; i++)
                {
                    // Don't attempt to offload active file
                    if (string.Compare(filesToOffload[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) == 0)
                        continue;

                    string destinationFileName = FilePath.AddPathSuffix(ArchiveOffloadLocation) + FilePath.GetFileName(filesToOffload[i].FileName);

                    try
                    {
                        DeleteFile(destinationFileName);
                        MoveFile(filesToOffload[i].FileName, destinationFileName);
                    }
                    catch (Exception ex)
                    {
                        OnOffloadException(new InvalidOperationException($"Failed to offload historic file \"{FilePath.GetFileName(filesToOffload[i].FileName)}\": {ex.Message}", ex));
                    }

                    offloadProgress.Complete++;
                    offloadProgress.ProgressMessage = FilePath.GetFileName(filesToOffload[i].FileName);

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
        else if (string.Compare(ArchiveOffloadLocation.ToNonNullString().Trim(), "*DELETE*", StringComparison.OrdinalIgnoreCase) == 0)
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
                    allHistoricFiles = [..m_historicArchiveFiles];

                // Start deleting historic files from oldest to newest.
                allHistoricFiles.Sort();

                List<Info> filesToDelete = [];

                foreach (Info file in allHistoricFiles)
                {
                    if ((DateTime.UtcNow - file.StartTimeTag.ToDateTime()).TotalDays > ArchiveOffloadMaxAge)
                        filesToDelete.Add(file);
                }

                ProcessProgress<int> offloadProgress = new("FileOffload") { Total = filesToDelete.Count };

                for (int i = 0; i < filesToDelete.Count; i++)
                {
                    // Don't attempt to offload active file
                    if (string.Compare(filesToDelete[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) == 0)
                        continue;

                    try
                    {
                        DeleteFile(filesToDelete[i].FileName);
                    }
                    catch (Exception ex)
                    {
                        OnOffloadException(new InvalidOperationException($"Failed to remove historic file \"{FilePath.GetFileName(filesToDelete[i].FileName)}\": {ex.Message}", ex));
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
        if (Directory.Exists(ArchiveOffloadLocation))
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
                    newHistoricFiles = m_historicArchiveFiles.FindAll(info => IsNewHistoricArchiveFile(info, m_fileName));

                // Sorting the list will sort the historic files from oldest to newest.
                newHistoricFiles.Sort();

                // We'll offload the specified number of oldest historic files to the offload location if the
                // number of historic files is more than the offload count or all of the historic files if the
                // offload count is smaller the available number of historic files.
                ProcessProgress<int> offloadProgress = new("FileOffload") { Total = newHistoricFiles.Count < m_archiveOffloadCount ? newHistoricFiles.Count : m_archiveOffloadCount };

                for (int i = 0; i < offloadProgress.Total; i++)
                {
                    // Don't attempt to offload active file
                    if (string.Compare(newHistoricFiles[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) == 0)
                        continue;
                    
                    string destinationFileName = FilePath.AddPathSuffix(ArchiveOffloadLocation) + FilePath.GetFileName(newHistoricFiles[i].FileName);

                    try
                    {
                        DeleteFile(destinationFileName);
                        MoveFile(newHistoricFiles[i].FileName, destinationFileName);
                    }
                    catch (Exception ex)
                    {
                        OnOffloadException(new InvalidOperationException($"Failed to offload historic file \"{FilePath.GetFileName(newHistoricFiles[i].FileName)}\": {ex.Message}", ex));
                    }

                    offloadProgress.Complete++;
                    offloadProgress.ProgressMessage = FilePath.GetFileName(newHistoricFiles[i].FileName);

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
        else if (string.Compare(ArchiveOffloadLocation.ToNonNullString().Trim(), "*DELETE*", StringComparison.OrdinalIgnoreCase) == 0)
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
                    allHistoricFiles = [..m_historicArchiveFiles];

                // Determine total number of files to remove
                int filesToDelete = Common.Min(m_archiveOffloadCount, allHistoricFiles.Count);

                ProcessProgress<int> offloadProgress = new("FileOffload") { Total = filesToDelete };

                // Start deleting historic files from oldest to newest.
                allHistoricFiles.Sort();

                for (int i = 0; i < filesToDelete; i++)
                {
                    // Don't attempt to offload active file
                    if (string.Compare(allHistoricFiles[i].FileName, m_fileName, StringComparison.OrdinalIgnoreCase) == 0)
                        continue;
                    
                    try
                    {
                        DeleteFile(allHistoricFiles[i].FileName);
                    }
                    catch (Exception ex)
                    {
                        OnOffloadException(new InvalidOperationException($"Failed to remove historic file \"{FilePath.GetFileName(allHistoricFiles[i].FileName)}\": {ex.Message}", ex));
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
        if (m_maxHistoricArchiveFiles < 1)
            return;
        
        // Wait until the historic file list has been built.
        if (m_buildHistoricFileListThread.IsAlive)
            m_buildHistoricFileListThread.Join();

        // Get a local copy of all the historic archive files.
        List<Info> allHistoricFiles;

        lock (m_historicArchiveFiles)
            allHistoricFiles = [..m_historicArchiveFiles];

        if (allHistoricFiles.Count <= m_maxHistoricArchiveFiles)
            return;

        // Start deleting historic files from oldest to newest.
        allHistoricFiles.Sort();

        while (allHistoricFiles.Count > m_maxHistoricArchiveFiles)
        {
            try
            {
                DeleteFile(allHistoricFiles[0].FileName);
            }
            catch (Exception ex)
            {
                OnOffloadException(new InvalidOperationException($"Failed during attempt to maintain maximum number of historic files - file \"{FilePath.GetFileName(allHistoricFiles[0].FileName)}\" could not be deleted: {ex.Message}", ex));
            }
            finally
            {
                allHistoricFiles.RemoveAt(0);
            }
        }
    }

    private Info GetHistoricFileInfo(string fileName)
    {
        Info fileInfo = null;

        try
        {
            // Validate the file name to determine whether the given file is actually a historic file
            if (!Regex.IsMatch(fileName, $".+_.+_to_.+\\{FileExtension}$"))
                return null;

            if (File.Exists(fileName))
            {
                // We'll open the file and get relevant information about it.
                ArchiveFile historicArchiveFile = new();

                historicArchiveFile.FileName = fileName;
                historicArchiveFile.StateFile = StateFile;
                historicArchiveFile.IntercomFile = IntercomFile;
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
                    OnHistoricFileListBuildException(new InvalidOperationException($"Failed to open historic data file \"{FilePath.GetFileName(fileName)}\" due to exception: {ex.Message}", ex));
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
                string[] fileStartEndDates = datesString.Split(["_to_"], StringSplitOptions.None);

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
            OnHistoricFileListBuildException(new InvalidOperationException($"Failed during information access attempt for historic data file \"{FilePath.GetFileName(fileName)}\" due to exception: {ex.Message}", ex));
        }

        return fileInfo;
    }

    #endregion

    #region [ Queue Delegates ]

    private void WriteToCurrentArchiveFile(IDataPoint[] items)
    {
        Dictionary<int, List<IDataPoint>> sortedDataPoints = new();

        // First we'll separate all point data by ID.
        foreach (IDataPoint dataPoint in items)
        {
            if (!sortedDataPoints.ContainsKey(dataPoint.HistorianID))
                sortedDataPoints.Add(dataPoint.HistorianID, []);

            sortedDataPoints[dataPoint.HistorianID].Add(dataPoint);
        }

        IntercomRecord system = IntercomFile.Read(1);

        foreach (int pointID in sortedDataPoints.Keys)
        {
            // Initialize local variables.
            StateRecord state = StateFile.Read(pointID);
            MetadataRecord metadata = m_metadataFile.Read(pointID);

            for (int i = 0; i < sortedDataPoints[pointID].Count; i++)
            {
                IDataPoint dataPoint = sortedDataPoints[pointID][i];

                // Ensure that the received data is to be archived.
                if (state is null || metadata is null || !metadata.GeneralFlags.Enabled)
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
                            dataPoint.Quality = (int)dataPoint.Value == metadata.DigitalFields.AlarmState ? Quality.LogicalAlarm : Quality.Good;
                            break;
                    }
                }

                // Update information about the latest data point received.
                if (dataPoint.Time.CompareTo(system.LatestDataTime) > 0)
                {
                    system.LatestDataID = dataPoint.HistorianID;
                    system.LatestDataTime = dataPoint.Time;
                    IntercomFile.Write(1, system);
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
                        if (!DiscardOutOfSequenceData)
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

                            decimal delay = metadata.GeneralFlags.DataType switch
                            {
                                DataType.Analog => (decimal)metadata.AnalogFields.AlarmDelay,
                                DataType.Digital => (decimal)metadata.DigitalFields.AlarmDelay,
                                _ => 0
                            };

                            // Dispatch the alarm immediately or after a given time based on settings.
                            if (delay > 0)
                            {
                                // Wait before dispatching alarm.
                                if (m_delayedAlarmProcessing.TryGetValue(dataPoint.HistorianID, out decimal first))
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

                    if (CompressData)
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
                            double slope1 = (state.CurrentData.Value - (state.ArchivedData.Value + compressionLimit)) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                            double slope2 = (state.CurrentData.Value - (state.ArchivedData.Value - compressionLimit)) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);
                            double currentSlope = (state.CurrentData.Value - state.ArchivedData.Value) / (double)(state.CurrentData.Time.Value - state.ArchivedData.Time.Value);

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
                Fat.DataPointsReceived++;

                if (archiveData)
                {
                    if (dataPoint.Time.CompareTo(Fat.FileStartTime) >= 0)
                    {
                        // Data belongs to this file.
                        ArchiveDataBlock dataBlock;

                        lock (m_dataBlocks)
                            dataBlock = m_dataBlocks[dataPoint.HistorianID - 1];

                        if (dataBlock is null || dataBlock.SlotsAvailable == 0)
                        {
                            // Need to find a data block for writing the data.
                            if (dataBlock is not null)
                                state.ActiveDataBlockIndex = -1;
                            
                            dataBlock = Fat.RequestDataBlock(dataPoint.HistorianID, dataPoint.Time, state.ActiveDataBlockIndex >= 0 ? 
                                state.ActiveDataBlockIndex : // Retrieve previously used data block.
                                system.DataBlocksUsed);      // Time to request a new data block.

                            if (dataBlock is not null)
                            {
                                // Update the total number of data blocks used.
                                if (dataBlock.SlotsUsed == 0 && system.DataBlocksUsed == dataBlock.Index)
                                {
                                    system.DataBlocksUsed++;
                                    IntercomFile.Write(1, system);
                                }

                                // Update the active data block index information.
                                state.ActiveDataBlockIndex = dataBlock.Index;
                            }

                            // Keep in-memory reference to the data block for consecutive writes.
                            lock (m_dataBlocks)
                                m_dataBlocks[dataPoint.HistorianID - 1] = dataBlock;

                            // Kick-off the rollover preparation when its threshold is reached.
                            if (Statistics.FileUsage >= m_rolloverPreparationThreshold && !File.Exists(StandbyArchiveFileName) && !m_rolloverPreparationThread.IsAlive)
                            {
                                m_rolloverPreparationThread = new Thread(PrepareForRollover) { Priority = ThreadPriority.Lowest };
                                m_rolloverPreparationThread.Start();
                            }
                        }

                        if (dataBlock is not null)
                        {
                            // Write data to the data block.
                            if (dataBlock.Write(dataPoint, out Exception ex))
                                Fat.DataPointsArchived++;
                            else
                                OnDataWriteException(ex);
                        }
                        else
                        {
                            OnFileFull();   // Current file is full.

                            Fat.DataPointsReceived--;

                            while (true)
                            {
                                Rollover(); // Rollover current file.
                                if (RolloverWaitHandle.WaitOne(1, false))
                                    break;  // Rollover is successful.
                            }

                            i--;                              // Process current data point again.
                            system = IntercomFile.Read(1);    // Re-read modified intercom record.
                            continue;
                        }
                    }
                    else
                    {
                        // Data is historic.
                        Fat.DataPointsReceived--;
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
                StateFile.Write(state.HistorianID, state);
                // [END]     Data archival
            }
        }
    }

    private void WriteToHistoricArchiveFile(IDataPoint[] items)
    {
        // Wait until the historic file list has been built.
        if (m_buildHistoricFileListThread.IsAlive)
            m_buildHistoricFileListThread.Join();

        Dictionary<Info, Dictionary<int, List<IDataPoint>>> historicFileData = new();

        // Separate all point data into bins by historic file and point ID
        foreach (IDataPoint dataPoint in items)
        {
            try
            {
                Info historicFileInfo;

                // Attempt to find a historic archive file where the data point belongs
                lock (m_historicArchiveFiles)
                    historicFileInfo = m_historicArchiveFiles.Find(info => FindHistoricArchiveFileForWrite(info, dataPoint.Time));

                // If a historic file exists, sort the data point into the proper bin
                if (historicFileInfo is null)
                    continue;
                
                Dictionary<int, List<IDataPoint>> sortedPointData = historicFileData.GetOrAdd(historicFileInfo, _ => new Dictionary<int, List<IDataPoint>>());
                List<IDataPoint> pointData = sortedPointData.GetOrAdd(dataPoint.HistorianID, _ => []);
                pointData.Add(dataPoint);
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

            using ArchiveFile historicFile = new();
            
            try
            {
                // Open the historic file
                historicFile.FileName = historicFileInfo.FileName;
                historicFile.StateFile = StateFile;
                historicFile.IntercomFile = IntercomFile;
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
                        // Request a new or previously used data block for point data
                        if (historicFileBlock is null || historicFileBlock.SlotsAvailable == 0)
                            historicFileBlock = historicFile.Fat.RequestDataBlock(pointID, dataPoint.Time, -1);

                        // Write the data point into the data block
                        if (historicFileBlock.Write(dataPoint, out Exception ex))
                        {
                            historicFile.Fat.DataPointsReceived++;
                            historicFile.Fat.DataPointsArchived++;
                        }
                        else
                        {
                            // Suppress exceptions related to bad timestamps - this handles the case where the system is attempting to write
                            // a historical point with a timestamp that is less than 01/01/1995, the minimum value for a TimeTag instance
                            if (ex is not TimeTagException)
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
                if (m_dataBlocks[i] is not null && !m_dataBlocks[i].IsActive)
                    m_dataBlocks[i] = null;
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
        if (!MonitorNewArchiveFiles)
            FileWatcher_Deleted(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, FilePath.GetDirectoryName(FilePath.GetAbsolutePath(fileName)), FilePath.GetFileName(fileName)));
    }

    // File.Move proxy function that will manually invoke file watcher handlers when file watchers are disabled
    private void MoveFile(string sourceFileName, string destinationFileName)
    {
        File.Move(sourceFileName, destinationFileName);

        // Manually call file monitoring events if file watchers are not enabled
        if (MonitorNewArchiveFiles)
            return;
        
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

    private void FileWatcher_Created(object sender, FileSystemEventArgs e)
    {
        if (m_historicArchiveFiles is null)
            return;
        
        bool historicFileListUpdated = false;
        Info historicFileInfo = GetHistoricFileInfo(e.FullPath);

        lock (m_historicArchiveFiles)
        {
            if (historicFileInfo is not null && !m_historicArchiveFiles.Contains(historicFileInfo))
            {
                m_historicArchiveFiles.Add(historicFileInfo);
                historicFileListUpdated = true;
            }
        }

        if (historicFileListUpdated)
            OnHistoricFileListUpdated();
    }

    private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (m_historicArchiveFiles is null)
            return;
        
        bool historicFileListUpdated = false;
        Info historicFileInfo = GetHistoricFileInfo(e.FullPath);

        lock (m_historicArchiveFiles)
        {
            if (historicFileInfo is not null && m_historicArchiveFiles.Contains(historicFileInfo))
            {
                m_historicArchiveFiles.Remove(historicFileInfo);
                historicFileListUpdated = true;
            }
        }

        if (historicFileListUpdated)
            OnHistoricFileListUpdated();
    }

    private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        if (m_historicArchiveFiles is null)
            return;
        
        if (string.Compare(FilePath.GetExtension(e.OldFullPath), FileExtension, StringComparison.OrdinalIgnoreCase) == 0)
        {
            try
            {
                bool historicFileListUpdated = false;
                Info oldFileInfo = GetHistoricFileInfo(e.OldFullPath);

                lock (m_historicArchiveFiles)
                {
                    if (oldFileInfo is not null && m_historicArchiveFiles.Contains(oldFileInfo))
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

        if (string.Compare(FilePath.GetExtension(e.FullPath), FileExtension, StringComparison.OrdinalIgnoreCase) != 0)
            return;
        
        try
        {
            bool historicFileListUpdated = false;
            Info newFileInfo = GetHistoricFileInfo(e.FullPath);

            lock (m_historicArchiveFiles)
            {
                if (newFileInfo is not null && !m_historicArchiveFiles.Contains(newFileInfo))
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
        return (int)(fileSize * 1024 / blockSize);
    }

    // Find Predicates
    private static bool FindHistoricArchiveFileForRead(Info fileInfo, TimeTag startTime, TimeTag endTime)
    {
        return fileInfo is not null &&
               ((startTime.CompareTo(fileInfo.StartTimeTag) >= 0 && startTime.CompareTo(fileInfo.EndTimeTag) <= 0) ||
                (endTime.CompareTo(fileInfo.StartTimeTag) >= 0 && endTime.CompareTo(fileInfo.EndTimeTag) <= 0) ||
                (startTime.CompareTo(fileInfo.StartTimeTag) < 0 && endTime.CompareTo(fileInfo.EndTimeTag) > 0));
    }

    private static bool FindHistoricArchiveFileForWrite(Info fileInfo, TimeTag searchTime)
    {
        return fileInfo is not null &&
               searchTime.CompareTo(fileInfo.StartTimeTag) >= 0 &&
               searchTime.CompareTo(fileInfo.EndTimeTag) <= 0;
    }

    // Determines if the historic file is in the primary archive location (not offloaded).
    private static bool IsNewHistoricArchiveFile(Info fileInfo, string fileName)
    {
        return fileInfo is not null &&
               string.Compare(FilePath.GetDirectoryName(fileName), FilePath.GetDirectoryName(fileInfo.FileName), StringComparison.OrdinalIgnoreCase) == 0;
    }

    #endregion
}