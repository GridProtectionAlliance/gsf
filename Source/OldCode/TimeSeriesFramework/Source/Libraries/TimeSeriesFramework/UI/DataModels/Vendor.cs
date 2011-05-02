//******************************************************************************************************
//  Vendor.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  04/14/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
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
        [StringLength(3, ErrorMessage = "Vendor acronym cannot exceed 3 characters")]
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
        [StringLength(100, ErrorMessage = "Vendor name cannot exceed 100 characters.")]
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
        [StringLength(100, ErrorMessage = "Vendor phone number cannot exceed 100 characters.")]
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
        [StringLength(100, ErrorMessage = "Vendor contact e-mail cannot exceed 100 characters.")]
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
        /// Loads <see cref="Vendor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="Vendor"/>.</returns>
        public static ObservableCollection<Vendor> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Vendor> vendorList = new ObservableCollection<Vendor>();
                DataTable vendorTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym, Name, PhoneNumber, ContactEmail, URL " +
                    "FROM VendorDetail ORDER BY Name");

                foreach (DataRow row in vendorTable.Rows)
                {
                    vendorList.Add(new Vendor()
                        {
                            ID = row.Field<int>("ID"),
                            Acronym = row.Field<string>("Acronym"),
                            Name = row.Field<string>("Name"),
                            PhoneNumber = row.Field<string>("PhoneNumber"),
                            ContactEmail = row.Field<string>("ContactEmail"),
                            URL = row.Field<string>("URL")
                        });
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
                    vendorList[row.Field<int>("ID")] = row.Field<string>("Name");

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
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Vendor vendor, bool isNew)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (isNew)
                    database.Connection.ExecuteNonQuery("INSERT INTO Vendor (Acronym, Name, PhoneNumber, ContactEmail, URL, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES (@acronym, @name, @phoneNumber, @contactEmail, @url, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, vendor.Acronym.Replace(" ", "").ToUpper() ?? (object)DBNull.Value,
                        vendor.Name, vendor.PhoneNumber ?? (object)DBNull.Value, vendor.ContactEmail ?? (object)DBNull.Value, vendor.URL ?? (object)DBNull.Value,
                        CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, CommonFunctions.CurrentUser,
                        database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("UPDATE Vendor SET Acronym = @acronym, Name = @name, PhoneNumber = @phoneNumber, ContactEmail = @contactEmail, " +
                        "URL = @url, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, vendor.Acronym.Replace(" ", "").ToUpper(), vendor.Name,
                        vendor.PhoneNumber, vendor.ContactEmail, vendor.URL, CommonFunctions.CurrentUser,
                        database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, vendor.ID);

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

                database.Connection.ExecuteNonQuery("DELETE FROM Vendor WHER ID = @vendorID", DefaultTimeout, vendorID);

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
