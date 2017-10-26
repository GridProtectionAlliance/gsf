//******************************************************************************************************
//  VendorDevice.cs - Gbtc
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
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
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
    /// Represents a record of <see cref="VendorDevice"/> information as defined in the database.
    /// </summary>
    public class VendorDevice : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private int m_vendorID;
        private string m_name;
        private string m_description;
        private string m_url;
        private string m_vendorName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="VendorDevice"/>'s ID.
        /// </summary>
        // Field is populated by database via auto-increment , so no validation attributes are applied.
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
        /// Gets or sets the VendorID.
        /// </summary>
        [DefaultValue(10)]
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
        [Required(ErrorMessage = "Vendor device name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Vendor device name cannot exceed 200 characters.")]
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
        [DataType(DataType.Url, ErrorMessage = "Vendor device URL is not formatted properly.")]
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
        /// Gets the VendorName of this <see cref="VendorDevice"/>.
        /// </summary>
        public string VendorName
        {
            get
            {
                return m_vendorName;
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
                return m_updatedOn;
            }
            set
            {
                m_updatedOn = value;
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
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// LoadKeys <see cref="VendorDevice"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of vendor device IDs.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, string sortMember, string sortDirection)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> vendorDeviceList = new List<int>();
                DataTable vendorDeviceTable;

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                // check the query once again , Does it have to be details or somethng else
                vendorDeviceTable = database.Connection.RetrieveData(database.AdapterType, string.Format("SELECT ID From VendorDeviceDetail {0}", sortClause));

                foreach (DataRow row in vendorDeviceTable.Rows)
                {
                    vendorDeviceList.Add(row.ConvertField<int>("ID"));
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
        /// Loads <see cref="VendorDevice"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the Measurement to be loaded from the database</param>
        /// <returns>Collection of <see cref="VendorDevice"/>.</returns>
        public static ObservableCollection<VendorDevice> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                VendorDevice[] vendorDeviceList = null;
                DataTable vendorDeviceTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);
                    query = string.Format("SELECT * FROM VendorDeviceDetail WHERE ID IN ({0})", commaSeparatedKeys);
                    vendorDeviceTable = database.Connection.RetrieveData(database.AdapterType, query);
                    vendorDeviceList = new VendorDevice[vendorDeviceTable.Rows.Count];

                    foreach (DataRow row in vendorDeviceTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        vendorDeviceList[keys.IndexOf(id)] = new VendorDevice()
                        {
                            ID = id,
                            VendorID = row.ConvertField<int>("VendorID"),
                            Name = row.Field<string>("Name"),
                            Description = row.Field<string>("Description"),
                            URL = row.Field<string>("URL"),
                            m_vendorName = row.Field<string>("VendorName")
                        };
                    }
                }

                return new ObservableCollection<VendorDevice>(vendorDeviceList ?? new VendorDevice[0]);
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
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
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
                    vendorDeviceList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

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
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, VendorDevice vendorDevice)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (vendorDevice.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO VendorDevice (VendorID, Name, Description, URL, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "Values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", "vendorID", "name", "description", "url", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, vendorDevice.VendorID, vendorDevice.Name, vendorDevice.Description.ToNotNull(),
                        vendorDevice.URL.ToNotNull(), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE VendorDevice SET VendorID = {0}, Name = {1}, Description = {2}, URL = {3}, UpdatedBy = {4}, " +
                        "UpdatedOn = {5} WHERE ID = {6}", "vendorID", "name", "description", "url", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, vendorDevice.VendorID, vendorDevice.Name, vendorDevice.Description.ToNotNull(),
                        vendorDevice.URL.ToNotNull(), CommonFunctions.CurrentUser, database.UtcNow, vendorDevice.ID);
                }

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

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM VendorDevice WHERE ID = {0}", "vendorDeviceID"), DefaultTimeout, vendorDeviceID);

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
