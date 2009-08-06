//*******************************************************************************************************
//  MetadataProviderBase.cs
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
//  07/07/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/06/2009 - Pinal C. Patel
//       Made Initialize() virtual so inheriting classes can override the default behavior.
//
//*******************************************************************************************************

using System;
using System.Data;
using System.Threading;
using System.Timers;
using TVA.Configuration;
using TVA.Historian.Files;

namespace TVA.Historian.MetadataProviders
{
    /// <summary>
    /// A base class for a provider of updates to the data in a <see cref="MetadataFile"/>.
    /// </summary>
    public abstract class MetadataProviderBase : IMetadataProvider
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> is started.
        /// </summary>
        public event EventHandler MetadataRefreshStart;

        /// <summary>
        /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> is completed.
        /// </summary>
        public event EventHandler MetadataRefreshComplete;

        /// <summary>
        /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> times out.
        /// </summary>
        public event EventHandler MetadataRefreshTimeout;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during <see cref="Refresh()"/> of <see cref="Metadata"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered during <see cref="Refresh()"/>.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> MetadataRefreshException;

        // Fields
        private int m_refreshInterval;
        private int m_refreshTimeout;
        private MetadataFile m_metadata;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private System.Timers.Timer m_refreshTimer;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the metadata provider.
        /// </summary>
        protected MetadataProviderBase()
        {
            m_enabled = false;
            m_refreshInterval = -1;
            m_refreshTimeout = 60;
            m_persistSettings = true;
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the metadata provider is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~MetadataProviderBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of seconds to wait for the <see cref="Refresh()"/> to complete.
        /// </summary>
        /// <remarks>
        /// Set <see cref="RefreshTimeout"/> to -1 to wait indefinitely on <see cref="Refresh()"/>.
        /// </remarks>
        public int RefreshTimeout
        {
            get
            {
                return m_refreshTimeout;
            }
            set
            {
                if (value < 1)
                    m_refreshTimeout = -1;
                else
                    m_refreshTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of minutes at which the <see cref="Metadata"/> if to be refreshed automatically.
        /// </summary>
        /// <remarks>
        /// Set <see cref="RefreshInterval"/> to -1 to disable auto <see cref="Refresh()"/>.
        /// </remarks>
        public int RefreshInterval
        {
            get
            {
                return m_refreshInterval;
            }
            set
            {
                if (value < 1)
                    m_refreshInterval = -1;
                else
                    m_refreshInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MetadataFile"/> to be refreshed by the metadata provider.
        /// </summary>
        public MetadataFile Metadata
        {
            get
            {
                return m_metadata;
            }
            set
            {
                m_metadata = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the metadata provider is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the metadata provider settings are to be saved to the config file.
        /// </summary>
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
        /// Gets or sets the category under which the metadata provider settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
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

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, refreshes the <see cref="Metadata"/> from an external source.
        /// </summary>
        protected abstract void RefreshMetadata();

        #endregion

        /// <summary>
        /// Releases all the resources used by the metadata provider.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the metadata provider.
        /// </summary>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                // Load settings from the config file.
                LoadSettings();
                // Start refresh timer for auto-refresh.
                if (m_enabled && m_refreshInterval > 0)
                {
                    m_refreshTimer = new System.Timers.Timer(m_refreshInterval * 60000);
                    m_refreshTimer.Elapsed += RefreshTimer_Elapsed;
                    m_refreshTimer.Start();
                }
                // Initialize only once.
                m_initialized = true;
            }
        }

        /// <summary>
        /// Saves metadata provider settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        public virtual void SaveSettings()
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
                element = settings["Enabled", true];
                element.Update(m_enabled, element.Description, element.Encrypted);
                element = settings["RefreshTimeout", true];
                element.Update(m_refreshTimeout, element.Description, element.Encrypted);
                element = settings["RefreshInterval", true];
                element.Update(m_refreshInterval, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved metadata provider settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("Enabled", m_enabled, "True if this metadata provider is enabled; otherwise False.");
                settings.Add("RefreshTimeout", m_refreshTimeout, "Number of seconds to wait for metadata refresh to complete.");
                settings.Add("RefreshInterval", m_refreshInterval, "Number of minutes at which the metadata is to be refreshed.");
                Enabled = settings["Enabled"].ValueAs(m_enabled);
                RefreshTimeout = settings["RefreshTimeout"].ValueAs(m_refreshTimeout);
                RefreshInterval = settings["RefreshInterval"].ValueAs(m_refreshInterval);
            }
        }

        /// <summary>
        /// Refreshes the <see cref="Metadata"/> from an external source.
        /// </summary>
        /// <returns>true if the <see cref="Metadata"/> is refreshed; otherwise false.</returns>
        public bool Refresh()
        {
            if (!m_enabled)
                return false;

            Thread refreshThread = new Thread(RefreshInternal);
            refreshThread.Start();
            if (m_refreshTimeout < 1)
            {
                // Wait indefinetely on the refresh.
                refreshThread.Join(Timeout.Infinite);
            }
            else
            {
                // Wait for the specified time on refresh.
                if (!refreshThread.Join(m_refreshTimeout * 1000))
                {
                    refreshThread.Abort();                  

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the metadata provider and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
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

                        if (m_refreshTimer != null)
                        {
                            m_refreshTimer.Elapsed -= RefreshTimer_Elapsed;
                            m_refreshTimer.Dispose();
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="MetadataRefreshStart"/> event.
        /// </summary>
        protected virtual void OnMetadataRefreshStart()
        {
            if (MetadataRefreshStart != null)
                MetadataRefreshStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="MetadataRefreshComplete"/> event.
        /// </summary>
        protected virtual void OnMetadataRefreshComplete()
        {
            if (MetadataRefreshComplete != null)
                MetadataRefreshComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="MetadataRefreshTimeout"/> event.
        /// </summary>
        protected virtual void OnMetadataRefreshTimeout()
        {
            if (MetadataRefreshTimeout != null)
                MetadataRefreshTimeout(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="MetadataRefreshException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="MetadataRefreshException"/> event.</param>
        protected virtual void OnMetadataRefreshException(Exception ex)
        {
            if (MetadataRefreshException != null)
                MetadataRefreshException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Refresh <see cref="Metadata"/> from data in the specified <paramref name="dataTable"/>.
        /// </summary>
        /// <param name="dataTable"><see cref="DataTable"/> containing the new metadata.</param>
        protected virtual void RefreshMetadata(DataTable dataTable)
        {
            if (m_metadata == null)
                throw new ArgumentNullException("Metadata");

            if (dataTable == null)
                throw new ArgumentNullException("dataTable");

            if (dataTable.Rows.Count == 0)
                throw new ArgumentException("dataTable must contain data.");

            if (dataTable.Rows[0].ItemArray.Length != 43)
                throw new ArgumentException("dataTable must contain 43 columns.");

            // Column 00: HistorianID
            // Column 01: DataType
            // Column 02: Name
            // Column 03: Synonym1
            // Column 04: Synonym2
            // Column 05: Synonym3
            // Column 06: Description
            // Column 07: HardwareInfo
            // Column 08: Remarks
            // Column 09: PlantCode
            // Column 10: UnitNumber
            // Column 11: SystemName
            // Column 12: SourceID
            // Column 13: Enabled
            // Column 14: ScanRate
            // Column 15: CompressionMinTime
            // Column 16: CompressionMaxTime
            // Column 17: EngineeringUnits
            // Column 18: LowWarning
            // Column 19: HighWarning
            // Column 20: LowAlarm
            // Column 21: HighAlarm
            // Column 22: LowRange
            // Column 23: HighRange
            // Column 24: CompressionLimit
            // Column 25: ExceptionLimit
            // Column 26: DisplayedDigits
            // Column 27: SetDescription
            // Column 28: ClearDescription
            // Column 29: AlarmState
            // Column 30: ChangeSecurity
            // Column 31: AccessSecurity
            // Column 32: StepCheck
            // Column 33: AlarmEnabled
            // Column 34: AlarmFlags
            // Column 35: AlarmDelay
            // Column 36: AlarmToFile
            // Column 37: AlarmByEmail
            // Column 38: AlarmByPager
            // Column 39: AlarmByPhone
            // Column 40: AlarmEmails
            // Column 41: AlarmPagers
            // Column 42: AlarmPhones
            MetadataRecord metadata;
            foreach (DataRow row in dataTable.Rows)
            {
                metadata = new MetadataRecord(Convert.ToInt32(row[0]));
                metadata.GeneralFlags.DataType = (DataType)Convert.ToInt32(row[1]);
                metadata.Name = Convert.ToString(row[2]);
                metadata.Synonym1 = Convert.ToString(row[3]);
                metadata.Synonym2 = Convert.ToString(row[4]);
                metadata.Synonym3 = Convert.ToString(row[5]);
                metadata.Description = Convert.ToString(row[6]);
                metadata.HardwareInfo = Convert.ToString(row[7]);
                metadata.Remarks = Convert.ToString(row[8]);
                metadata.PlantCode = Convert.ToString(row[9]);
                metadata.UnitNumber = Convert.ToInt32(row[10]);
                metadata.System = Convert.ToString(row[11]);
                metadata.SourceID = Convert.ToInt32(row[12]);
                metadata.GeneralFlags.Enabled = Convert.ToBoolean(row[13]);
                metadata.ScanRate = Convert.ToSingle(row[14]);
                metadata.CompressionMinTime = Convert.ToInt32(row[15]);
                metadata.CompressionMaxTime = Convert.ToInt32(row[16]);
                metadata.SecurityFlags.ChangeSecurity = Convert.ToInt32(row[30]);
                metadata.SecurityFlags.AccessSecurity = Convert.ToInt32(row[31]);
                metadata.GeneralFlags.StepCheck = Convert.ToBoolean(row[32]);
                metadata.GeneralFlags.AlarmEnabled = Convert.ToBoolean(row[33]);
                metadata.AlarmFlags.Value = Convert.ToInt32(row[34]);
                metadata.GeneralFlags.LogToFile = Convert.ToBoolean(row[36]);
                metadata.GeneralFlags.NotifyByEmail = Convert.ToBoolean(row[37]);
                metadata.GeneralFlags.NotifyByPager = Convert.ToBoolean(row[38]);
                metadata.GeneralFlags.NotifyByPhone = Convert.ToBoolean(row[39]);
                metadata.EmailAddresses = Convert.ToString(row[40]);
                metadata.PagerNumbers = Convert.ToString(row[41]);
                metadata.PhoneNumbers = Convert.ToString(row[42]);
                if (metadata.GeneralFlags.DataType == DataType.Analog)
                {
                    metadata.AnalogFields.EngineeringUnits = Convert.ToString(row[17]);
                    metadata.AnalogFields.LowWarning = Convert.ToSingle(row[18]);
                    metadata.AnalogFields.HighWarning = Convert.ToSingle(row[19]);
                    metadata.AnalogFields.LowAlarm = Convert.ToSingle(row[20]);
                    metadata.AnalogFields.HighAlarm = Convert.ToSingle(row[21]);
                    metadata.AnalogFields.LowRange = Convert.ToSingle(row[22]);
                    metadata.AnalogFields.HighRange = Convert.ToSingle(row[23]);
                    metadata.AnalogFields.CompressionLimit = Convert.ToSingle(row[24]);
                    metadata.AnalogFields.ExceptionLimit = Convert.ToSingle(row[25]);
                    metadata.AnalogFields.DisplayDigits = Convert.ToInt32(row[26]);
                    metadata.AnalogFields.AlarmDelay = Convert.ToSingle(row[35]);
                }
                else if (metadata.GeneralFlags.DataType == DataType.Digital)
                {
                    metadata.DigitalFields.SetDescription = Convert.ToString(row[27]);
                    metadata.DigitalFields.ClearDescription = Convert.ToString(row[28]);
                    metadata.DigitalFields.AlarmState = Convert.ToInt32(row[29]);
                    metadata.DigitalFields.AlarmDelay = Convert.ToSingle(row[35]);
                }

                m_metadata.Write(metadata.HistorianID, metadata);
            }
        }

        private void RefreshInternal()
        {
            try
            {
                OnMetadataRefreshStart();
                RefreshMetadata();
                OnMetadataRefreshComplete();
            }
            catch (ThreadAbortException)
            {
                OnMetadataRefreshTimeout();
            }
            catch (Exception ex)
            {
                OnMetadataRefreshException(ex);
            }
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Refresh();
        }

        #endregion
    }
}
