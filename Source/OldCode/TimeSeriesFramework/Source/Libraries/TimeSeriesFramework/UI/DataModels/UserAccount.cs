//******************************************************************************************************
//  UserAccount.cs - Gbtc
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
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
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
                DataTable userAccountTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * From UserAccount ORDER BY Name");

                foreach (DataRow row in userAccountTable.Rows)
                {
                    userAccountList.Add(new UserAccount()
                    {
                        ID = Guid.Parse(row.Field<object>("ID").ToString()),
                        Name = row.Field<string>("Name"),
                        Password = row.Field<object>("Password") == null ? string.Empty : row.Field<string>("Password"),
                        FirstName = row.Field<object>("FirstName") == null ? string.Empty : row.Field<string>("FirstName"),
                        LastName = row.Field<object>("LastName") == null ? string.Empty : row.Field<string>("LastName"),
                        DefaultNodeID = Guid.Parse(row.Field<object>("DefaultNodeID").ToString()),
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

                userAccountList.Insert(0, new UserAccount() { ID = Guid.Empty, ChangePasswordOn = DateTime.Now.AddDays(90) });

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
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, UserAccount userAccount)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                string passwordColumn = "Password";

                if (database.IsJetEngine())
                    passwordColumn = "[Password]";

                object changePasswordOn = userAccount.ChangePasswordOn;
                if (userAccount.ChangePasswordOn == DateTime.MinValue)
                    changePasswordOn = (object)DBNull.Value;
                else if (database.IsJetEngine())
                    changePasswordOn = userAccount.ChangePasswordOn.ToOADate();

                if (userAccount.ID == null || userAccount.ID == Guid.Empty)
                    database.Connection.ExecuteNonQuery("INSERT INTO UserAccount (Name, " + passwordColumn + ", FirstName, LastName, DefaultNodeID, Phone, Email, LockedOut, UseADAuthentication, " +
                        "ChangePasswordOn, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@name, @password, @firstName, @lastName, @defaultNodeID, @phone, " +
                        "@email, @lockedOut, @useADAuthentication, @changePasswordOn, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, userAccount.Name,
                        userAccount.Password.ToNotNull(), userAccount.FirstName.ToNotNull(), userAccount.LastName.ToNotNull(), database.Guid(userAccount.DefaultNodeID),
                        userAccount.Phone.ToNotNull(), userAccount.Email.ToNotNull(), userAccount.LockedOut, userAccount.UseADAuthentication, changePasswordOn,
                        CommonFunctions.CurrentUser, database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE UserAccount SET Name = @name, " + passwordColumn + " = @password, FirstName = @firstName, LastName = @lastName, " +
                            "DefaultNodeID = @defaultNodeID, Phone = @phone, Email = @email, LockedOut = @lockedOut, UseADAuthentication = @useADAuthentication, " +
                            "ChangePasswordOn = @changePasswordOn, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, userAccount.Name,
                            userAccount.Password.ToNotNull(), userAccount.FirstName.ToNotNull(), userAccount.LastName.ToNotNull(), database.Guid(userAccount.DefaultNodeID),
                            userAccount.Phone.ToNotNull(), userAccount.Email.ToNotNull(), userAccount.LockedOut, userAccount.UseADAuthentication, changePasswordOn,
                            CommonFunctions.CurrentUser, database.UtcNow(), database.Guid(userAccount.ID));

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

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM UserAccount WHERE ID = @userAccountID", DefaultTimeout, database.Guid(userAccountID));

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
