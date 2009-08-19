//*******************************************************************************************************
//  OleDbMetadataProvider.cs
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
//  07/20/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Data.OleDb;
using TVA.Configuration;

namespace TVA.Historian.MetadataProviders
{
    /// <summary>
    /// Represents a provider of data to a <see cref="TVA.Historian.Files.MetadataFile"/> from any OLE DB data store.
    /// </summary>
    public class OleDbMetadataProvider : MetadataProviderBase
    {
        #region [ Members ]

        // Fields
        private string m_connectString;
        private string m_selectString;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbMetadataProvider"/> class.
        /// </summary>
        public OleDbMetadataProvider()
            : base()
        {
            m_connectString = string.Empty;
            m_selectString = string.Empty;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the connection string for connecting to the OLE DB data store of metadata.
        /// </summary>
        public string ConnectString
        {
            get
            {
                return m_connectString;
            }
            set
            {
                m_connectString = value;
            }
        }

        /// <summary>
        /// Gets or sets the SELECT statement for retrieving metadata from the OLE DB data store.
        /// </summary>
        public string SelectString
        {
            get
            {
                return m_selectString;
            }
            set
            {
                m_selectString = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves <see cref="OleDbMetadataProvider"/> settings to the config file if the <see cref="MetadataProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                element = settings["ConnectString", true];
                element.Update(m_connectString, element.Description, element.Encrypted);
                element = settings["SelectString", true];
                element.Update(m_selectString, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="OleDbMetadataProvider"/> settings from the config file if the <see cref="MetadataProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("ConnectString", m_connectString, "Connection string for connecting to the OLE DB data store of metadata.", true);
                settings.Add("SelectString", m_selectString, "SELECT statement for retrieving metadata from the OLE DB data store.");
                ConnectString = settings["ConnectString"].ValueAs(m_connectString);
                SelectString = settings["SelectString"].ValueAs(m_selectString);
            }
        }

        /// <summary>
        /// Refreshes the <see cref="MetadataProviderBase.Metadata"/> from an OLE DB data store.
        /// </summary>
        /// <exception cref="ArgumentNullException"><see cref="ConnectString"/> or <see cref="SelectString"/> is set to a null or empty string.</exception>
        protected override void RefreshMetadata()
        {
            if (string.IsNullOrEmpty(m_connectString))
                throw new ArgumentNullException("ConnectString");

            if (string.IsNullOrEmpty(m_selectString))
                throw new ArgumentNullException("SelectString");


            // Update existing metadata with retrieved metadata.
            OleDbDataReader reader = null;
            OleDbConnection connection = new OleDbConnection(m_connectString);
            OleDbCommand command = new OleDbCommand(m_selectString, connection);
            try
            {
                connection.Open();
                reader = command.ExecuteReader();

                MetadataUpdater metadataUpdater = new MetadataUpdater(Metadata);
                metadataUpdater.UpdateMetadata(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();

                if (command != null)
                    command.Dispose();

                if (connection != null)
                    connection.Dispose();
            }
        }

        #endregion
    }
}
