////*******************************************************************************************************
////  MetadataProviderBase.cs
////  Copyright © 2009 - TVA, all rights reserved - Gbtc
////
////  Build Environment: C#, Visual Studio 2008
////  Primary Developer: Pinal C. Patel
////      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
////       Phone: 423/751-3024
////       Email: pcpatel@tva.gov
////
////  Code Modification History:
////  -----------------------------------------------------------------------------------------------------
////  07/07/2009 - Pinal C. Patel
////       Generated original version of source code.
////
////*******************************************************************************************************

//using System;
//using TVA.Configuration;
//using TVA.Historian.Files;

//namespace TVA.Historian.MetadataProviders
//{
//    public abstract class MetadataProviderBase : IMetadataProvider
//    {
//        #region [ Members ]

//        // Fields
//        private int m_refreshInterval;
//        private MetadataFile m_metadata;
//        private bool m_persistSettings;
//        private string m_settingsCategory;
//        private bool m_enabled;
//        private bool m_disposed;
//        private bool m_initialized;

//        #endregion

//        #region [ Constructors ]

//        /// <summary>
//        /// Initializes a new instance of the metadata provider.
//        /// </summary>
//        protected MetadataProviderBase()
//        {
//            m_enabled = false;
//            m_persistSettings = true;
//            m_settingsCategory = this.GetType().Name;
//        }

//        /// <summary>
//        /// Releases the unmanaged resources before the metadata provider is reclaimed by <see cref="GC"/>.
//        /// </summary>
//        ~MetadataProviderBase()
//        {
//            Dispose(false);
//        }

//        #endregion

//        #region [ Properties ]

//        /// <summary>
//        /// Gets or sets the interval (in minutes) at which the <see cref="Metadata"/> if to be refreshed.
//        /// </summary>
//        public int RefreshInterval
//        {
//            get
//            {
//                return m_refreshInterval;
//            }
//            set
//            {
//                m_refreshInterval = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the <see cref="MetadataFile"/> to be refreshed by the metadata provider.
//        /// </summary>
//        public MetadataFile Metadata
//        {
//            get
//            {
//                return m_metadata;
//            }
//            set
//            {
//                m_metadata = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value that indicates whether the metadata provider settings are to be saved to the config file.
//        /// </summary>
//        public bool PersistSettings
//        {
//            get
//            {
//                return m_persistSettings;
//            }
//            set
//            {
//                m_persistSettings = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the category under which the metadata provider settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
//        /// </summary>
//        /// <exception cref="ArgumentNullException">The value specified is a null or empty string.</exception>
//        public string SettingsCategory
//        {
//            get
//            {
//                return m_settingsCategory;
//            }
//            set
//            {
//                if (string.IsNullOrEmpty(value))
//                    throw (new ArgumentNullException());

//                m_settingsCategory = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value that indicates whether the metadata provider is currently enabled.
//        /// </summary>
//        public bool Enabled
//        {
//            get
//            {
//                return m_enabled;
//            }
//            set
//            {
//                m_enabled = value;
//            }
//        }

//        #endregion

//        #region [ Methods ]

//        #region [ Abstract ]

//        /// <summary>
//        /// When overridden in a derived class, refreshes the <see cref="MetadataFile"/> from an external source.
//        /// </summary>
//        /// <returns>true if the <see cref="Metadata"/> is refreshed; otherwise false.</returns>
//        public abstract bool Refresh();

//        #endregion

//        /// <summary>
//        /// Releases all the resources used by the metadata provider.
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        /// <summary>
//        /// Initializes the metadata provider.
//        /// </summary>
//        public void Initialize()
//        {
//            if (!m_initialized)
//            {
//                LoadSettings();         // Load settings from the config file.
//                m_initialized = true;   // Initialize only once.
//            }
//        }

//        /// <summary>
//        /// Saves metadata provider settings to the config file if the <see cref="PersistSettings"/> property is set to true.
//        /// </summary>        
//        public virtual void SaveSettings()
//        {
//            if (m_persistSettings)
//            {
//                // Ensure that settings category is specified.
//                if (string.IsNullOrEmpty(m_settingsCategory))
//                    throw new InvalidOperationException("SettingsCategory property has not been set.");

//                // Save settings under the specified category.
//                ConfigurationFile config = ConfigurationFile.Current;
//                CategorizedSettingsElement element = null;
//                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
//                element = settings["Enabled"];
//                element.Update(m_enabled, element.Description, element.Encrypted);
//                config.Save();
//            }
//        }

//        /// <summary>
//        /// Loads saved metadata provider settings from the config file if the <see cref="PersistSettings"/> property is set to true.
//        /// </summary>        
//        public virtual void LoadSettings()
//        {
//            if (m_persistSettings)
//            {
//                // Ensure that settings category is specified.
//                if (string.IsNullOrEmpty(m_settingsCategory))
//                    throw new InvalidOperationException("SettingsCategory property has not been set.");

//                // Load settings from the specified category.
//                ConfigurationFile config = ConfigurationFile.Current;
//                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
//                settings.Add("Enabled", m_enabled, "True if this metadata provider is enabled; otherwise False.");
//                Enabled = settings["Enabled"].ValueAs(m_enabled);
//            }
//        }

//        /// <summary>
//        /// Releases the unmanaged resources used by the metadata provider and optionally releases the managed resources.
//        /// </summary>
//        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
//        protected virtual void Dispose(bool disposing)
//        {
//            if (!m_disposed)
//            {
//                try
//                {
//                    // This will be done regardless of whether the object is finalized or disposed.				
//                    if (disposing)
//                    {
//                        // This will be done only when the object is disposed by calling Dispose().
//                        SaveSettings();
//                    }
//                }
//                finally
//                {
//                    m_disposed = true;  // Prevent duplicate dispose.
//                }
//            }
//        }

//        #endregion
//    }
//}
