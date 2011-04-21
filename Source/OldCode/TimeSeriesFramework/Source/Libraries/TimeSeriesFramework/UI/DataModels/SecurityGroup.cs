//******************************************************************************************************
//  ErrorLog.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  04/13/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using TVA.Data;
using System.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Creates a new object that represents a SecurityGroup
    /// </summary>
    class SecurityGroup : DataModelBase
    {
        #region [Members]

        //Fileds
        private string m_ID;
        private string m_name;
        private string m_description;
        private DateTime m_createdOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;
        private string m_UpdatedBy;
        private ObservableCollection<SecurityGroup> m_CurrentGroupUsers;
        private ObservableCollection<SecurityGroup> m_PossibleGroupUsers;

        #endregion

        #region [properties]

        /// <summary>
        /// Gets and sets the current SecurityGroup ID
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup Name
        /// </summary>
        [Required(ErrorMessage = " SecurityGroup Name is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "SecurityGroup Name cannot exceed 50 characters.")]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup Description
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup CreatedOn
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get
            {
                return m_createdOn;
            }
            set
            {
                m_createdOn = value;
                OnPropertyChanged("CreatedOn");
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup CreatedBy
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_CreatedBy;
            }
            set
            {
                m_CreatedBy = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup UpdatedOn
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_UpdatedOn;
            }
            set
            {
                m_UpdatedOn = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup UpdatedBy
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_UpdatedBy;
            }
            set
            {
                m_UpdatedBy = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup CurrentGroupUsers
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<SecurityGroup> CurrentGroupUsers
        {
            get
            {
                return m_CurrentGroupUsers;
            }
            set
            {
                m_CurrentGroupUsers = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup PossibleGroupUsers
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<SecurityGroup> PossibleGroupUsers
        {
            get
            {
                return m_PossibleGroupUsers;
            }
            set
            {
                m_PossibleGroupUsers = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads <see cref="Company"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="Company"/>.</returns>
        public static ObservableCollection<SecurityGroup> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                ObservableCollection<SecurityGroup> securityGroupList = new ObservableCollection<SecurityGroup>();
                DataTable securityGroupTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym, MapAcronym, Name, URL, LoadOrder FROM SecurityGroup ORDER BY LoadOrder");

                foreach (DataRow row in securityGroupTable.Rows)
                {
                    securityGroupList.Add(new SecurityGroup()
                    {
                        ID = row.Field<object>("ID").ToString(),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<object>("Description") == null ? string.Empty : row.Field<string>("Description"),
                        CreatedOn = Convert.ToDateTime(row.Field<object>("CreatedOn")),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        UpdatedOn = Convert.ToDateTime(row.Field<object>("UpdatedOn")),
                        UpdatedBy = row.Field<string>("UpdatedBy")
                    });
                }

                return securityGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }


        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="SecurityGroup"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of security groups defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional)
        {
            bool createdConnection = false;
            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                Dictionary<int, string> securityGroupList = new Dictionary<int, string>();
                if (isOptional)
                    securityGroupList.Add(0, "Select Security Group");

                DataTable securityGroupTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM SecurityGroup ORDER BY Name");

                foreach (DataRow row in securityGroupTable.Rows)
                    securityGroupList[row.Field<int>("ID")] = row.Field<string>("Name");

                return securityGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="SecurityGroup"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="securityGroup">Information about <see cref="SecurityGroup"/>.</param>
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, SecurityGroup securityGroup, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                if (isNew)
                    database.Connection.ExecuteNonQuery("Insert Into SecurityGroup (Name, Description, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@name, @description, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout,
                             securityGroup.Name, securityGroup.Description, securityGroup.UpdatedBy, securityGroup.UpdatedOn, securityGroup.CreatedBy, securityGroup.CreatedOn);
                else
                    database.Connection.ExecuteNonQuery("Update SecurityGroup Set Name = @name, Description = @description, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn Where ID = @id", DefaultTimeout,
                             securityGroup.Name, securityGroup.Description, securityGroup.UpdatedBy, securityGroup.UpdatedOn, securityGroup.ID);

                return "Security Group information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="SecurityGroup"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="securityGroupID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int securityGroupID)
        {
            bool createdConnection = false;

            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM SecurityGroup WHERE ID = @securityGroupID", DefaultTimeout, securityGroupID);

                return "Security Group deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        #endregion
    }
}
