//******************************************************************************************************
//  UserAccount.cs - Gbtc
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
//  04/13/2011 - Aniket Salver
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
// 05/13/2011 - Aniket Salver
//                  Modified the way Guid is retrived from the Data Base.
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
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using GSF.Identity;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    ///  Represents a record of <see cref="UserAccount"/> information as defined in the database.
    /// </summary>
    public class UserAccount : DataModelBase
    {
        #region [ Members ]

        private Guid m_id;
        private string m_name;
        private string m_password;
        private string m_firstName;
        private string m_lastName;
        private Guid m_defaultNodeID;
        private string m_phone;
        private string m_email;
        private bool m_lockedOut;
        private bool m_useADAuthentication;
        private DateTime m_changePasswordOn;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public Guid ID
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
        /// Gets or sets <see cref="UserAccount"/> Name.
        /// </summary>
        [Required(ErrorMessage = " User account name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "User account name cannot exceed 200 characters.")]
        [DefaultValue("Add New User")]
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
        /// Gets or sets <see cref="UserAccount"/> Password.
        /// </summary>  
        [StringLength(200, ErrorMessage = "User Account password cannot exceed 200 characters.")]
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
                OnPropertyChanged("Password");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> FirstName.
        /// </summary>       
        [StringLength(200, ErrorMessage = "User Account password cannot exceed 200 characters.")]
        public string FirstName
        {
            get
            {
                return m_firstName;
            }
            set
            {
                m_firstName = value;
                OnPropertyChanged("FirstName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> LastName.
        /// </summary>
        [StringLength(200, ErrorMessage = "User Account password cannot exceed 200 characters.")]
        public string LastName
        {
            get
            {
                return m_lastName;
            }
            set
            {
                m_lastName = value;
                OnPropertyChanged("LastName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> DefaultNodeID.
        /// </summary>
        [Required(ErrorMessage = "User account default node ID is a required field, please select a value.")]
        public Guid DefaultNodeID
        {
            get
            {
                return m_defaultNodeID;
            }
            set
            {
                m_defaultNodeID = value;
                OnPropertyChanged("DefaultNodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> Phone
        /// </summary>
        [StringLength(200, ErrorMessage = "User Account password cannot exceed 200 characters.")]
        public string Phone
        {
            get
            {
                return m_phone;
            }
            set
            {
                m_phone = value;
                OnPropertyChanged("Phone");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> Email
        /// </summary>
        [StringLength(200, ErrorMessage = "User Account password cannot exceed 200 characters.")]
        [EmailValidation]
        public string Email
        {
            get
            {
                return m_email;
            }
            set
            {
                m_email = value;
                OnPropertyChanged("Email");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> LockedOut.
        /// </summary>
        [DefaultValue(false)]
        public bool LockedOut
        {
            get
            {
                return m_lockedOut;
            }
            set
            {
                m_lockedOut = value;
                OnPropertyChanged("LockedOut");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> UseADAuthentication.
        /// </summary>
        [DefaultValue(true)]
        public bool UseADAuthentication
        {
            get
            {
                return m_useADAuthentication;
            }
            set
            {
                m_useADAuthentication = value;
                OnPropertyChanged("UseADAuthentication");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> ChangePasswordOn.
        /// </summary>
        // Because of database design no validation attributes are applied.
        public DateTime ChangePasswordOn
        {
            get
            {
                return m_changePasswordOn;
            }
            set
            {
                m_changePasswordOn = value;
                OnPropertyChanged("ChangePasswordOn");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> CreatedOn
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
        /// Gets or sets <see cref="UserAccount"/> CreatedBy
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
        /// Gets or sets <see cref="UserAccount"/> UpdatedOn
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
        /// Gets or sets <see cref="UserAccount"/> UpdatedBy
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
        /// Loads <see cref="UserAccount"/> information as an OberservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="UserAccount"/></returns>
        public static ObservableCollection<UserAccount> Load(AdoDataConnection database)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<UserAccount> userAccountList = new ObservableCollection<UserAccount>();
                DataTable userAccountTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * From UserAccount WHERE DefaultNodeID = '" + database.CurrentNodeID() + "'  ORDER BY Name");

                foreach (DataRow row in userAccountTable.Rows)
                {
                    userAccountList.Add(new UserAccount()
                    {
                        ID = database.Guid(row, "ID"),
                        Name = UserInfo.SIDToAccountName(row.Field<string>("Name")),
                        Password = row.Field<object>("Password") == null ? string.Empty : row.Field<string>("Password"),
                        FirstName = row.Field<object>("FirstName") == null ? string.Empty : row.Field<string>("FirstName"),
                        LastName = row.Field<object>("LastName") == null ? string.Empty : row.Field<string>("LastName"),
                        DefaultNodeID = database.Guid(row, "DefaultNodeID"),
                        Phone = row.Field<object>("Phone") == null ? string.Empty : row.Field<string>("Phone"),
                        Email = row.Field<object>("Email") == null ? string.Empty : row.Field<string>("Email"),
                        LockedOut = Convert.ToBoolean(row.Field<object>("LockedOut")),
                        UseADAuthentication = Convert.ToBoolean(row.Field<object>("UseADAuthentication")),
                        ChangePasswordOn = row.Field<object>("ChangePasswordOn") == null ? DateTime.MinValue : Convert.ToDateTime(row.Field<object>("ChangePasswordOn")),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"]),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        UpdatedOn = Convert.ToDateTime(row.Field<object>("UpdatedOn")),
                        UpdatedBy = row.Field<string>("UpdatedBy")
                    });
                }

                userAccountList.Insert(0, new UserAccount
                {
                    ID = Guid.Empty,
                    ChangePasswordOn = DateTime.Now.AddDays(90)
                });

                return userAccountList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="UserAccount"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of user accounts defined in the database.</returns>
        public static Dictionary<Guid, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> userAccountList = new Dictionary<Guid, string>();
                if (isOptional)
                    userAccountList.Add(Guid.Empty, "Select UserAccount");

                DataTable userAccountTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM UserAccount ORDER BY Name");

                foreach (DataRow row in userAccountTable.Rows)
                    userAccountList[database.Guid(row, "ID")] = UserInfo.SIDToAccountName(row.Field<string>("Name"));

                return userAccountList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="UserAccount"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="userAccount">Information about <see cref="UserAccount"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, UserAccount userAccount)
        {
            const string ErrorMessage = "User name already exists.";

            bool createdConnection = false;
            string query;
            string userAccountSID;
            int existing;

            try
            {
                createdConnection = CreateConnection(ref database);

                string pColumn = "Password";

                if (database.IsJetEngine)
                    pColumn = "[Password]";

                object changePasswordOn = userAccount.ChangePasswordOn;

                if (userAccount.ChangePasswordOn == DateTime.MinValue)
                    changePasswordOn = (object)DBNull.Value;
                else if (database.IsJetEngine)
                    changePasswordOn = userAccount.ChangePasswordOn.ToOADate();

                userAccountSID = UserInfo.UserNameToSID(userAccount.Name);

                if (!userAccount.UseADAuthentication || !UserInfo.IsUserSID(userAccountSID))
                    userAccountSID = userAccount.Name;

                if (userAccount.ID == Guid.Empty)
                {
                    existing = Convert.ToInt32(database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT COUNT(*) FROM UserAccount WHERE Name = {0}", "name"), DefaultTimeout, userAccountSID));

                    if (existing > 0)
                        throw new InvalidOperationException(ErrorMessage);

                    query = database.ParameterizedQueryString("INSERT INTO UserAccount (Name, " + pColumn + ", FirstName, LastName, DefaultNodeID, Phone, Email, " +
                        "LockedOut, UseADAuthentication, ChangePasswordOn, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, " +
                        "{9}, {10}, {11}, {12}, {13})", "name", "password", "firstName", "lastName", "defaultNodeID", "phone", "email", "lockedOut", "useADAuthentication",
                        "changePasswordOn", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, userAccountSID,
                        userAccount.Password.ToNotNull(), userAccount.FirstName.ToNotNull(), userAccount.LastName.ToNotNull(), database.CurrentNodeID(),
                        userAccount.Phone.ToNotNull(), userAccount.Email.ToNotNull(), database.Bool(userAccount.LockedOut), database.Bool(userAccount.UseADAuthentication),
                        changePasswordOn, CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);

                    CommonFunctions.LogEvent(string.Format("New user \"{0}\" created successfully by user \"{1}\".", userAccount.Name, CommonFunctions.CurrentUser), 2);
                }
                else
                {
                    existing = database.ExecuteScalar<int>("SELECT COUNT(*) FROM UserAccount WHERE Name = {0} AND ID <> {1}", userAccountSID, userAccount.ID);

                    if (existing > 0)
                        throw new InvalidOperationException(ErrorMessage);

                    query = database.ParameterizedQueryString("UPDATE UserAccount SET Name = {0}, " + pColumn + " = {1}, FirstName = {2}, LastName = {3}, " +
                            "DefaultNodeID = {4}, Phone = {5}, Email = {6}, LockedOut = {7}, UseADAuthentication = {8}, ChangePasswordOn = {9}, UpdatedBy = {10}, " +
                            "UpdatedOn = {11} WHERE ID = {12}", "name", "password", "firstName", "lastName", "defaultNodeID", "phone", "email", "lockedOut",
                            "useADAuthentication", "changePasswordOn", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, userAccountSID,
                            userAccount.Password.ToNotNull(), userAccount.FirstName.ToNotNull(), userAccount.LastName.ToNotNull(), database.Guid(userAccount.DefaultNodeID),
                            userAccount.Phone.ToNotNull(), userAccount.Email.ToNotNull(), database.Bool(userAccount.LockedOut), database.Bool(userAccount.UseADAuthentication),
                            changePasswordOn, CommonFunctions.CurrentUser, database.UtcNow, database.Guid(userAccount.ID));

                    CommonFunctions.LogEvent(string.Format("Information about user \"{0}\" updated successfully by user \"{1}\".", userAccount.Name, CommonFunctions.CurrentUser), 3);
                }

                return "User account information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="UserAccount"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="userAccountID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, Guid userAccountID)
        {
            bool createdConnection = false;
            string userName;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Get the name of the user to be deleted
                userName = database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT Name FROM UserAccount WHERE ID = {0}", "userAccountID"), DefaultTimeout, database.Guid(userAccountID)).ToNonNullString();

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                // Delete the user from the database
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM UserAccount WHERE ID = {0}", "userAccountID"), DefaultTimeout, database.Guid(userAccountID));

                // Write to the event log
                CommonFunctions.LogEvent(string.Format("User \"{0}\" deleted successfully by user \"{1}\".", UserInfo.SIDToAccountName(userName), CommonFunctions.CurrentUser), 12);

                return "User account deleted successfully";
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
