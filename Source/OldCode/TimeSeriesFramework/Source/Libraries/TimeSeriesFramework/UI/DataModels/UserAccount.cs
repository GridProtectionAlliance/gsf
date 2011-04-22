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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    ///  Represents a record of UserAccount information as defined in the database.
    /// </summary>
    public class UserAccount : DataModelBase
    {
        #region [ Members ]

        private string m_ID;
        private string m_name;
        private string m_password;
        private string m_frstName;
        private string m_lastName;
        private string m_defaultNodeID;
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
        /// Gets or sets <see cref="UserAccount"/> Name.
        /// </summary>
        [Required(ErrorMessage = " UserAccount Name is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "UserAccount Name cannot exceed 50 characters.")]
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
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string FirstName
        {
            get
            {
                return m_frstName;
            }
            set
            {
                m_frstName = value;
                OnPropertyChanged("FirstName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> LastName.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        [Required(ErrorMessage = " UserAccount DefaultNodeID is a required field, please provide value.")]
        [StringLength(36, ErrorMessage = "UserAccount DefaultNodeID cannot exceed 36 characters.")]
        public string DefaultNodeID
        {
            get
            {
                return m_defaultNodeID;
            }
            set
            {
                m_defaultNodeID = value;
                OnPropertyChanged("DefaultNodeId");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> Phone
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string Email
        {
            get
            {
                return m_email;
            }
            set
            {
                m_email = Email;
                OnPropertyChanged("Email");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="UserAccount"/> LockedOut
        /// </summary>
        [Required(ErrorMessage = " UserAccount LockedOut is a required field, please provide value.")]
        [StringLength(3, ErrorMessage = "UserAccount LockedOut cannot exceed 3 characters.")]
        [DefaultValue(typeof(bool), "0")]
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
        /// Gets or sets <see cref="UserAccount"/> UseADAuthentication
        /// </summary>
        [Required(ErrorMessage = " UserAccount UseADAuthentication is a required field, please provide value.")]
        [StringLength(3, ErrorMessage = "UserAccount UseADAuthentication cannot exceed 3 characters.")]
        [DefaultValue(typeof(bool), "1")]
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
        /// Gets or sets <see cref="UserAccount"/> ChangePasswordOn
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
                OnPropertyChanged("CreatedOn");

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
                OnPropertyChanged("CreatedBy");

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
                OnPropertyChanged("UpdatedOn");
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
                OnPropertyChanged("UpdatedBy");
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
                DataTable userAccountTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * From UserAccount Order By Name");

                foreach (DataRow row in userAccountTable.Rows)
                {
                    userAccountList.Add(new UserAccount()
                    {
                        ID = row.Field<int>("ID").ToString(),
                        Name = row.Field<string>("Name"),
                        Password = row.Field<object>("Password") == null ? string.Empty : row.Field<string>("Password"),
                        FirstName = row.Field<object>("FirstName") == null ? string.Empty : row.Field<string>("FirstName"),
                        LastName = row.Field<object>("LastName") == null ? string.Empty : row.Field<string>("LastName"),
                        DefaultNodeID = row.Field<object>("DefaultNodeID").ToString(),
                        Phone = row.Field<object>("Phone") == null ? string.Empty : row.Field<string>("Phone"),
                        Email = row.Field<object>("Email") == null ? string.Empty : row.Field<string>("Email"),
                        LockedOut = Convert.ToBoolean(row.Field<object>("LockedOut")),
                        UseADAuthentication = Convert.ToBoolean(row.Field<object>("UseADAuthentication")),
                        ChangePasswordOn = row.Field<object>("ChangePasswordOn") == null ? DateTime.MinValue : Convert.ToDateTime(row.Field<object>("ChangePasswordOn")),
                        CreatedOn = Convert.ToDateTime(row.Field<object>("CreatedOn")),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        UpdatedOn = Convert.ToDateTime(row.Field<object>("UpdatedOn")),
                        UpdatedBy = row.Field<string>("UpdatedBy")
                    });
                }

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
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> userAccountList = new Dictionary<int, string>();
                if (isOptional)
                    userAccountList.Add(0, "Select UserAccount");

                DataTable userAccountTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM UserAccount ORDER BY Name");

                foreach (DataRow row in userAccountTable.Rows)
                    userAccountList[row.Field<int>("ID")] = row.Field<string>("Name");

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
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, UserAccount userAccount, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                string passwordColumn = "Password";
                if (database.IsJetEngine())
                {
                    userAccount.DefaultNodeID = "{" + userAccount.DefaultNodeID + "}";
                    passwordColumn = "[Password]";
                }

                object changePasswordOn = userAccount.ChangePasswordOn;
                if (userAccount.ChangePasswordOn == DateTime.MinValue)
                    changePasswordOn = (object)DBNull.Value;
                else if (database.IsJetEngine())
                    changePasswordOn = userAccount.ChangePasswordOn.Date;

                if (isNew)
                    database.Connection.ExecuteNonQuery("Insert Into UserAccount (Name, " + passwordColumn + ", FirstName, LastName, DefaultNodeID, Phone, Email, LockedOut, UseADAuthentication, " +
                        "ChangePasswordOn, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@name, @password, @firstName, @lastName, @defaultNodeID, @phone, " +
                        "@email, @lockedOut, @useADAuthentication, @changePasswordOn, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, userAccount.Name,
                        userAccount.Password, userAccount.FirstName, userAccount.LastName, userAccount.DefaultNodeID, userAccount.Phone, userAccount.Email, userAccount.LockedOut,
                        userAccount.UseADAuthentication, changePasswordOn, CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow,
                        CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("Update UserAccount Set Name = @name, " + passwordColumn + " = @password, FirstName = @firstName, LastName = @lastName, " +
                            "DefaultNodeID = @defaultNodeID, Phone = @phone, Email = @email, LockedOut = @lockedOut, UseADAuthentication = @useADAuthentication, " +
                            "ChangePasswordOn = @changePasswordOn, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn Where ID = @id", DefaultTimeout, userAccount.Name,
                            userAccount.Password, userAccount.FirstName, userAccount.LastName, userAccount.DefaultNodeID, userAccount.Phone, userAccount.Email, userAccount.LockedOut,
                            userAccount.UseADAuthentication, changePasswordOn, CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow,
                            database.IsJetEngine() ? "{" + userAccount.ID + "}" : userAccount.ID);

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
        public static string Delete(AdoDataConnection database, int userAccountID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM UserAccount WHERE ID = @userAccountID", DefaultTimeout, userAccountID);

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
