//******************************************************************************************************
//  ApplicationSettings.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/28/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/15/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.ComponentModel;
using System.Configuration;
using GSF.Configuration;
using GSF.Data;

namespace DataMigrationUtility
{
    /// <summary>
    /// Represent an Application Settings information
    /// </summary>
    public class ApplicationSettings : CategorizedSettingsBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Application settings category name.
        /// </summary>
        public const string ApplicationSettingsCategory = "Application Settings";

        // Fields
        private string m_fromConnectionString;
        private DatabaseType m_fromDataType;
        private string m_toConnectionString;
        private DatabaseType m_toDataType;
        private bool m_preserveAutoIncValues;
        private bool m_useFromConnectionForRI;
        private bool m_clearDestinationTables;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initialize a new instance of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        public ApplicationSettings()
            : base(ApplicationSettingsCategory)
        {
            // Specify default category
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set from connection string to this Data Migration Utility
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(""), Description("Source connection string.")]
        public string FromConnectionString
        {
            get
            {
                return m_fromConnectionString;
            }
            set
            {
                m_fromConnectionString = value;
            }
        }

        /// <summary>
        /// Get or set from data Type to this Data Migration Utility
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(typeof(DatabaseType), "Other"), Description("Source database type.")]
        public DatabaseType FromDataType
        {
            get
            {
                return m_fromDataType;
            }
            set
            {
                m_fromDataType = value;
            }
        }


        /// <summary>
        /// Get or set To connection string to this Data Migration Utility
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(""), Description("Destination connection string.")]
        public string ToConnectionString
        {
            get
            {
                return m_toConnectionString;
            }
            set
            {
                m_toConnectionString = value;
            }
        }

        /// <summary>
        /// Get or set To Data Type to this Data Migration Utility
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(typeof(DatabaseType), "Other"), Description("Destination database type.")]
        public DatabaseType ToDataType
        {
            get
            {
                return m_toDataType;
            }
            set
            {
                m_toDataType = value;
            }
        }

        /// <summary>
        /// Get or set user from connection for RI(referential integrity)
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(false), Description("Use source connection string for referential integrity analysis.")]
        public bool UseFromConnectionForRI
        {
            get
            {
                return m_useFromConnectionForRI;
            }
            set
            {
                m_useFromConnectionForRI = value;
            }
        }

        /// <summary>
        /// Get or set Preserve Auto-Increment Field Values flag
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(true), Description("Preserve value of auto-increment field values.")]
        public bool PreserveAutoIncValues
        {
            get
            {
                return m_preserveAutoIncValues;
            }
            set
            {
                m_preserveAutoIncValues = value;
            }
        }

        /// <summary>
        /// Get or set Clear Destination Tables flag
        /// </summary>
        [Category(ApplicationSettingsCategory), UserScopedSetting, DefaultValue(false), Description("Clear existing data in destination database before migration.")]
        public bool ClearDestinationTables
        {
            get
            {
                return m_clearDestinationTables;
            }
            set
            {
                m_clearDestinationTables = value;
            }
        }

        #endregion
    }
}