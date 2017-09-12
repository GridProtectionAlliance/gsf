//******************************************************************************************************
//  UserData.cs - Gbtc
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
//  05/10/2010 - Pinal C. Patel
//       Generated original version of source code.
//  05/24/2010 - Pinal C. Patel
//       Added non-serializable LoginID property to keep track of the NTID for internal users.
//  07/12/2010 - Pinal C. Patel
//       Added IsDisabled property.
//  01/14/2011 - Pinal C. Patel
//       Updated field attributes used by XmlSerializer.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GSF.Security
{
    /// <summary>
    /// A serializable class that contains information about a user defined in the security data store.
    /// </summary>
    [XmlType(Namespace = ""), DataContract(Namespace = ""), Serializable]
    public class UserData
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class.
        /// </summary>
        public UserData()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class.
        /// </summary>
        /// <param name="username">User's logon name.</param>
        public UserData(string username)
        {
            LoginID = username;
            Username = username;
            Groups = new List<string>();
            Roles = new List<string>();

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class by copying an existing instance.
        /// </summary>
        /// <param name="userData">The existing instance of the <see cref="UserData"/> class.</param>
        public UserData(UserData userData)
        {
            Clone(userData);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the user's login ID.
        /// </summary>
        public string LoginID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's login name.
        /// </summary>
        [XmlElement, DataMember(Order = 0)]
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's password.
        /// </summary>
        /// <remarks>
        /// This field is only used to store hashed user
        /// passwords which are stored in the database.
        /// </remarks>
        [XmlElement, DataMember(Order = 1)]
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's first name.
        /// </summary>
        [XmlElement, DataMember(Order = 2)]
        public string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's last name.
        /// </summary>
        [XmlElement, DataMember(Order = 3)]
        public string LastName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's company name.
        /// </summary>
        [XmlElement, DataMember(Order = 4)]
        public string CompanyName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's phone number.
        /// </summary>
        [XmlElement, DataMember(Order = 5)]
        public string PhoneNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        [XmlElement, DataMember(Order = 6)]
        public string EmailAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's security question.
        /// </summary>
        [XmlElement, DataMember(Order = 7)]
        public string SecurityQuestion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the user's security answer.
        /// </summary>
        [XmlElement, DataMember(Order = 8)]
        public string SecurityAnswer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the UTC date and time when user must change the password.
        /// </summary>
        [XmlElement, DataMember(Order = 9)]
        public DateTime PasswordChangeDateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the UTC date and time when user account was created.
        /// </summary>
        [XmlElement, DataMember(Order = 10)]
        public DateTime AccountCreatedDateTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the user is defined in the backend security data store.
        /// </summary>
        [XmlElement, DataMember(Order = 11)]
        public bool IsDefined
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the user is defined as an external user in the backend security data store.
        /// </summary>
        [XmlElement, DataMember(Order = 12)]
        public bool IsExternal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the user account has been disabled.
        /// </summary>
        [XmlElement, DataMember(Order = 13)]
        public bool IsDisabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the user account has been locked due to numerous unsuccessful login attempts.
        /// </summary>
        [XmlElement, DataMember(Order = 14)]
        public bool IsLockedOut
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a read-only list of all the groups the user belongs to.
        /// </summary>
        [XmlArray, XmlArrayItem("Group"), DataMember(Order = 15)]
        public List<string> Groups
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a read-only list of all the roles assigned to the user.
        /// </summary>
        [XmlArray, XmlArrayItem("Role"), DataMember(Order = 16)]
        public List<string> Roles
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="UserData"/> object.
        /// </summary>
        public void Initialize()
        {
            Password = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            CompanyName = string.Empty;
            PhoneNumber = string.Empty;
            EmailAddress = string.Empty;
            SecurityQuestion = string.Empty;
            SecurityAnswer = string.Empty;
            PasswordChangeDateTime = DateTime.MinValue;
            AccountCreatedDateTime = DateTime.MinValue;
            IsDefined = false;
            IsExternal = false;
            IsDisabled = false;
            IsLockedOut = false;
            Groups.Clear();
            Roles.Clear();
        }

        /// <summary>
        /// Copies a new instance of the <see cref="UserData"/> class by copying an existing instance.
        /// </summary>
        /// <param name="userData">The existing instance of the <see cref="UserData"/> class.</param>
        public void Clone(UserData userData)
        {
            LoginID = userData.LoginID;
            Username = userData.Username;
            Password = userData.Password;
            FirstName = userData.FirstName;
            LastName = userData.LastName;
            CompanyName = userData.CompanyName;
            PhoneNumber = userData.PhoneNumber;
            EmailAddress = userData.EmailAddress;
            SecurityQuestion = userData.SecurityQuestion;
            SecurityAnswer = userData.SecurityAnswer;
            PasswordChangeDateTime = userData.PasswordChangeDateTime;
            AccountCreatedDateTime = userData.AccountCreatedDateTime;
            IsDefined = userData.IsDefined;
            IsExternal = userData.IsExternal;
            IsDisabled = userData.IsDisabled;
            IsLockedOut = userData.IsLockedOut;
            Groups = new List<string>(userData.Groups);
            Roles = new List<string>(userData.Roles);
        }

        #endregion
    }
}
