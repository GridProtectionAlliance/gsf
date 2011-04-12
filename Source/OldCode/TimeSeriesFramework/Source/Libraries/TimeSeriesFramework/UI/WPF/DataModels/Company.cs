//******************************************************************************************************
//  Company.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of company information as defined in the database.
    /// </summary>
    public class Company : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_acronym;
        private string m_mapAcronym;
        private string m_name;
        private string m_url;
        private int m_loadOrder;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Company"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Company"/> acronym.
        /// </summary>
        [Required(ErrorMessage = "Company acronym is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "Company acronym cannot exceed 50 characters.")]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets map acronym used by <see cref="Company"/>.
        /// </summary>
        [Required(ErrorMessage = "Company map acronym is a required field, please provide value.")]
        [StringLength(3, ErrorMessage = "Company map acronym cannot exceed 3 characters.")]
        public string MapAcronym
        {
            get
            {
                return m_mapAcronym;
            }
            set
            {
                m_mapAcronym = value;
                OnPropertyChanged("MapAcronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Company"/> name.
        /// </summary>
        [Required(ErrorMessage = "Company name is a required field, please provide value.")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
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
        /// Gets or sets <see cref="Company"/> URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "Company web site address URL is not formatted properly.")]
        public string URL
        {
            get
            {
                return m_url;
            }
            set
            {
                m_url = value;
                OnPropertyChanged("URL");
            }
        }

        /// <summary>
        /// Gets or sets desired load order of <see cref="Company"/> record.
        /// </summary>
        [Required(ErrorMessage = "Company load order value is required field, please provide value.")]
        [DefaultValue(typeof(int), "0")]
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Company"/> was created.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets who the current <see cref="Company"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_createdBy;
            }
            set
            {
                m_createdBy = value;
                OnPropertyChanged("CreatedBy");
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Company"/> was updated.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_updatedOn;
            }
            set
            {
                m_updatedOn = value;
                OnPropertyChanged("UpdatedOn");
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="Company"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_updatedBy;
            }
            set
            {
                m_updatedBy = value;
                OnPropertyChanged("UpdatedBy");
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
        public static ObservableCollection<Company> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                List<Company> companyList = new List<Company>();
                DataTable companyTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym, MapAcronym, Name, URL, LoadOrder FROM Company ORDER BY LoadOrder");

                foreach (DataRow row in companyTable.Rows)
                {
                    companyList.Add(new Company()
                    {
                        ID = row.Field<int>("ID"),
                        Acronym = row.Field<string>("Acronym"),
                        MapAcronym = row.Field<string>("MapAcronym"),
                        Name = row.Field<string>("Name"),
                        URL = row.Field<string>("URL"),
                        LoadOrder = row.Field<int>("LoadOrder")
                    });
                }

                return new ObservableCollection<Company>(companyList);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Company"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns>Dictionary<int, string> containing ID and Name of companies defined in the database.</returns>
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

                Dictionary<int, string> companyList = new Dictionary<int, string>();
                if (isOptional)
                    companyList.Add(0, "Select Company");

                IDbCommand command = database.Connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT ID, Name FROM Company ORDER BY LoadOrder";

                DataTable resultTable = new DataTable();
                resultTable.Load(command.ExecuteReader());

                int id;
                foreach (DataRow row in resultTable.Rows)
                {
                    id = int.Parse(row["ID"].ToString());

                    if (!companyList.ContainsKey(id))
                        companyList.Add(id, row["Name"].ToString());
                }
                return companyList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Company"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="company">Information about <see cref="Company"/>.</param>
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Company company, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                IDbCommand command = database.Connection.CreateCommand();
                command.CommandType = CommandType.Text;

                if (isNew)
                    command.CommandText = "INSERT INTO Company (Acronym, MapAcronym, Name, URL, LoadOrder, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@acronym, @mapAcronym, @name, @url, @loadOrder, @updatedBy, @updatedOn, @createdBy, @createdOn)";
                else
                    command.CommandText = "UPDATE Company SET Acronym = @acronym, MapAcronym = @mapAcronym, Name = @name, URL = @url, LoadOrder = @loadOrder, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id";

                command.AddParameterWithValue("@acronym", company.Acronym.Replace(" ", "").ToUpper());
                command.AddParameterWithValue("@mapAcronym", company.MapAcronym.Replace(" ", "").ToUpper());
                command.AddParameterWithValue("@name", company.Name);
                command.AddParameterWithValue("@url", company.URL ?? string.Empty);
                command.AddParameterWithValue("@loadOrder", company.LoadOrder);
                command.AddParameterWithValue("@updatedBy", CommonFunctions.CurrentUser);
                command.AddParameterWithValue("@updatedOn", command.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow);

                if (isNew)
                {
                    command.AddParameterWithValue("@createdBy", CommonFunctions.CurrentUser);
                    command.AddParameterWithValue("@createdOn", command.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB") ? DateTime.UtcNow.Date : DateTime.UtcNow);
                }
                else
                {
                    command.AddParameterWithValue("@id", company.ID);
                }

                command.ExecuteNonQuery();
                return "Company information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Company"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="companyID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int companyID)
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

                database.Connection.ExecuteNonQuery("DELETE FROM Company WHERE ID = @companyID", DefaultTimeout, companyID);

                return "Company deleted successfully";
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