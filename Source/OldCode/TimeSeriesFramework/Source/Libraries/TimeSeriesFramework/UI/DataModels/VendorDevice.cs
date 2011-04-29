//******************************************************************************************************
//  VendorDevice.cs - Gbtc
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
    /// Represents a record of vendor device information as defined in the database.
    /// </summary>
    public class VendorDevice : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_ID;
        private int m_vendorID;
        private string m_name;
        private string m_description;
        private string m_URL;
        private string m_vendorName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_UpdatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="VendorDevice"/>'s ID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
        public int ID
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
        /// Gets or sets the VendorID.
        /// </summary>
        [DefaultValue(typeof(int), "10")]
        public int VendorID
        {
            get
            {
                return m_vendorID;
            }
            set
            {
                m_vendorID = value;
                OnPropertyChanged("VendorID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="VendorDevice"/>'s Name.
        /// </summary>
        [Required(ErrorMessage = "VendorDevice Name is a required field, please provide value")]
        [StringLength(100, ErrorMessage = "VendorDevice Name cannot exceed 100 characters.")]
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
        /// Gets or sets the Description.
        /// </summary>
        // Because of database design, no validation attributes are applied
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
        /// Gets or sets the URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "URL is not formatted properly.")]
        public string URL
        {
            get
            {
                return m_URL;
            }
            set
            {
                m_URL = value;
                OnPropertyChanged("URL");
            }
        }

        /// <summary>
        /// Gets or sets the VendorName.
        /// </summary>
        // Because of database design, no validation attributes are applied
        public string VendorName
        {
            get
            {
                return m_vendorName;
            }
            set
            {
                m_vendorName = value;
                OnPropertyChanged("VendorName");
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time this <see cref="VendorDevice"/> was created.
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
        /// Gets or sets who this <see cref="VendorDevice"/> was created by.
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
        /// Gets or sets the Date or Time this <see cref="VendorDevice"/> was updated.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_UpdatedOn;
            }
            set
            {
                m_UpdatedOn = value;
                OnPropertyChanged("UpdatedOn");
            }
        }

        /// <summary>
        /// Gets or sets who this VenderDevice was updated by.
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

        /// <summary>
        /// Loads <see cref="VendorDevice"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="VendorDevice"/>.</returns>
        public static ObservableCollection<VendorDevice> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<VendorDevice> vendorDeviceList = new ObservableCollection<VendorDevice>();
                DataTable vendorDeviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM VendorDeviceDetail ORDER BY Name");

                foreach (DataRow row in vendorDeviceTable.Rows)
                {
                    vendorDeviceList.Add(new VendorDevice()
                    {
                        ID = row.Field<int>("ID"),
                        VendorID = row.Field<int>("VendorID"),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<string>("Description"),
                        URL = row.Field<string>("URL"),
                        VendorName = row.Field<string>("VendorName")
                    });
                }

                return vendorDeviceList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="VendorDevice"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of vendor devices defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> vendorDeviceList = new Dictionary<int, string>();
                if (isOptional)
                    vendorDeviceList.Add(0, "Select Vendor Device");

                DataTable vendorDeviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM VendorDevice ORDER BY Name");

                foreach (DataRow row in vendorDeviceTable.Rows)
                    vendorDeviceList[row.Field<int>("ID")] = row.Field<string>("Name");

                return vendorDeviceList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="VendorDevice"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="vendorDevice">Information about <see cref="VendorDevice"/>.</param>
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, VendorDevice vendorDevice, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                if (isNew)
                    database.Connection.ExecuteNonQuery("INSERT INTO VendorDevice (VendorID, Name, Description, URL, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@vendorID, @name, @description, @url, @updatedBy, @updatedOn, @createdBy, @createdOn)",
                        DefaultTimeout, vendorDevice.VendorID, vendorDevice.Name, vendorDevice.Description ?? (object)DBNull.Value, vendorDevice.URL ?? (object)DBNull.Value,
                        CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("Update VendorDevice Set VendorID = @vendorID, Name = @name, Description = @description, URL = @url, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn Where ID = @id",
                        DefaultTimeout, vendorDevice.VendorID, vendorDevice.Name, vendorDevice.Description ?? (object)DBNull.Value, vendorDevice.URL ?? (object)DBNull.Value,
                        CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, vendorDevice.ID);

                return "Vendor Device information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="VendorDevice"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="vendorDeviceID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int vendorDeviceID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM VendorDevice WHERE ID = @vendorDeviceID", DefaultTimeout, vendorDeviceID);

                return "Vendor Device deleted successfully";
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
