//******************************************************************************************************
//  Vendor.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  04/14/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    ///  Represents a record of <see cref="Vendor"/> information as defined in the database.
    /// </summary>
    public class Vendor : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_phoneNumber;
        private string m_contactEmail;
        private string m_url;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
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
        /// Gets or sets the <see cref="Vendor"/> Acronym.
        /// </summary>
        [StringLength(200, ErrorMessage = "Vendor acronym cannot exceed 200 characters")]
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
        /// Gets or sets the <see cref="Vendor"/> Name.
        /// </summary>
        [Required(ErrorMessage = "The Vendor name is a required field, please provide value")]
        [StringLength(200, ErrorMessage = "Vendor name cannot exceed 200 characters.")]
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
        /// Gets or sets <see cref="Vendor"/> Phone Number.
        /// </summary>
        [StringLength(200, ErrorMessage = "Vendor phone number cannot exceed 200 characters.")]
        public string PhoneNumber
        {
            get
            {
                return m_phoneNumber;
            }
            set
            {
                m_phoneNumber = value;
                OnPropertyChanged("PhoneNumber");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> Contact Email.
        /// </summary>
        [StringLength(200, ErrorMessage = "Vendor contact e-mail cannot exceed 200 characters.")]
        [EmailValidation]
        public string ContactEmail
        {
            get
            {
                return m_contactEmail;
            }
            set
            {
                m_contactEmail = value;
                OnPropertyChanged("ContactEmail");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vendor"/> URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "Vendor URL is not formatted properly.")]
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
        /// Gets or sets the Date or Time this <see cref="Vendor"/> was created on.
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
        /// Gets or sets who this <see cref="Vendor"/> was created by.
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
        /// Gets or sets the Date Time this <see cref="Vendor"/> was updated on.
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
        /// Gets or sets who this <see cref="Vendor"/> was updated by.
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
        /// LoadKeys <see cref="Vendor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of vendor IDs.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, string sortMember, string sortDirection)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> vendorList = new List<int>();
                DataTable vendorTable;

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                // check the query once again , Does it have to be details or somethng else
                vendorTable = database.Connection.RetrieveData(database.AdapterType, string.Format("SELECT ID From VendorDetail {0}", sortClause));

                foreach (DataRow row in vendorTable.Rows)
                {
                    vendorList.Add(row.ConvertField<int>("ID"));
                }
                return vendorList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }


        /// <summary>
        /// Loads <see cref="Vendor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the Measurement to be loaded from the database</param>
        /// <returns>Collection of <see cref="Vendor"/>.</returns>
        public static ObservableCollection<Vendor> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Vendor[] vendorList = null;
                DataTable vendorTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);
                    query = string.Format("SELECT ID, Acronym, Name, PhoneNumber, ContactEmail, URL FROM VendorDetail WHERE ID IN ({0})", commaSeparatedKeys);
                    vendorTable = database.Connection.RetrieveData(database.AdapterType, query);
                    vendorList = new Vendor[vendorTable.Rows.Count];

                    foreach (DataRow row in vendorTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        vendorList[keys.IndexOf(id)] = new Vendor()
                        {
                            ID = id,
                            Acronym = row.Field<string>("Acronym"),
                            Name = row.Field<string>("Name"),
                            PhoneNumber = row.Field<string>("PhoneNumber"),
                            ContactEmail = row.Field<string>("ContactEmail"),
                            URL = row.Field<string>("URL")
                        };
                    }
                }

                return new ObservableCollection<Vendor>(vendorList ?? new Vendor[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Vendor"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of vendors defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> vendorList = new Dictionary<int, string>();
                if (isOptional)
                    vendorList.Add(0, "Select Vendor");

                DataTable vendorTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Vendor ORDER BY Name");

                foreach (DataRow row in vendorTable.Rows)
                    vendorList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return vendorList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Vendor"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="vendor">Information about <see cref="Vendor"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Vendor vendor)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (vendor.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO Vendor (Acronym, Name, PhoneNumber, ContactEmail, URL, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", "acronym", "name", "phoneNumber", "contactEmail", "url", "updatedBy", "updatedOn", "createdBy",
                        "createdOn");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, vendor.Acronym.Replace(" ", "").ToUpper(),
                        vendor.Name, vendor.PhoneNumber.ToNotNull(), vendor.ContactEmail.ToNotNull(), vendor.URL.ToNotNull(), CommonFunctions.CurrentUser, database.UtcNow,
                        CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE Vendor SET Acronym = {0}, Name = {1}, PhoneNumber = {2}, ContactEmail = {3}, " +
                        "URL = {4}, UpdatedBy = {5}, UpdatedOn = {6} WHERE ID = {7}", "acronym", "name", "phoneNumber", "contactEmail", "url", "updatedBy",
                        "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, vendor.Acronym.Replace(" ", "").ToUpper(), vendor.Name,
                        vendor.PhoneNumber.ToNotNull(), vendor.ContactEmail.ToNotNull(), vendor.URL.ToNotNull(), CommonFunctions.CurrentUser, database.UtcNow, vendor.ID);
                }

                return "Vendor information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Vendor"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="vendorID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int vendorID)
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

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Vendor WHERE ID = {0}", "vendorID"), DefaultTimeout, vendorID);

                return "Vendor deleted successfully";
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
