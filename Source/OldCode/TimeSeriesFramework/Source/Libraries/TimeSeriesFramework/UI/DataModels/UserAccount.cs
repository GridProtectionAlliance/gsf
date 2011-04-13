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
                m_defaultNodeID = Email;
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
    }
}
