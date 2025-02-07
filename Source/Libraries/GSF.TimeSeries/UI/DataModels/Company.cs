//******************************************************************************************************
//  Company.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added regular expression validator for Acronym.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Company"/> information as defined in the database.
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
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Company"/> acronym.
        /// </summary>
        [Required(ErrorMessage = "Company acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Company acronym cannot exceed 200 characters.")]
        [AcronymValidation]
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
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
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
        [UrlValidation]
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
        [Required(ErrorMessage = "Company load order value is a required field, please provide value.")]
        [DefaultValue(0)]
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
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods      

        /// <summary>
        /// Loads <see cref="Company"/> IDs as an <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Int32"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> companyList = new List<int>();
                string sortClause = string.Empty;
                DataTable companyTable;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                companyTable = database.Connection.RetrieveData(database.AdapterType, string.Format("SELECT ID FROM Company {0}", sortClause));

                foreach (DataRow row in companyTable.Rows)
                {
                    companyList.Add(row.ConvertField<int>("ID"));
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
        /// Loads <see cref="Company"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the companies to be loaded from the database.</param>
        /// <returns>Collection of <see cref="Company"/>.</returns>
        public static ObservableCollection<Company> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Company[] companyList = null;
                DataTable companyTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);
                    query = string.Format("SELECT ID, Acronym, MapAcronym, Name, URL, LoadOrder FROM Company WHERE ID IN ({0})", commaSeparatedKeys);
                    companyTable = database.Connection.RetrieveData(database.AdapterType, query);
                    companyList = new Company[companyTable.Rows.Count];

                    foreach (DataRow row in companyTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        companyList[keys.IndexOf(id)] = new Company()
                        {
                            ID = id,
                            Acronym = row.Field<string>("Acronym"),
                            MapAcronym = row.Field<string>("MapAcronym"),
                            Name = row.Field<string>("Name"),
                            URL = row.Field<string>("URL"),
                            LoadOrder = row.ConvertField<int>("LoadOrder")
                        };
                    }
                }

                return new ObservableCollection<Company>(companyList ?? new Company[0]);
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
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of companies defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> companyList = new Dictionary<int, string>();
                if (isOptional)
                    companyList.Add(0, "Select Company");

                DataTable companyTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Company ORDER BY Name");

                foreach (DataRow row in companyTable.Rows)
                    companyList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

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
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Company company)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (company.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO Company (Acronym, MapAcronym, Name, URL, LoadOrder, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", "acronym", "mapAcronym", "name", "url", "loadOrder", "updatedBy", "updatedOn", "createdBy",
                        "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, company.Acronym.Replace(" ", "").ToUpper(), company.MapAcronym.Replace(" ", "").ToUpper(),
                        company.Name, company.URL.ToNotNull(), company.LoadOrder, CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser,
                        database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE Company SET Acronym = {0}, MapAcronym = {1}, Name = {2}, URL = {3}, LoadOrder = {4}, " +
                        "UpdatedBy = {5}, UpdatedOn = {6} WHERE ID = {7}", "acronym", "mapAcronym", "name", "url", "loadOrder", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, company.Acronym.Replace(" ", "").ToUpper(),
                        company.MapAcronym.Replace(" ", "").ToUpper(), company.Name, company.URL.ToNotNull(), company.LoadOrder, CommonFunctions.CurrentUser,
                        database.UtcNow, company.ID);
                }

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
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Company WHERE ID = {0}", "companyID"), DefaultTimeout, companyID);

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