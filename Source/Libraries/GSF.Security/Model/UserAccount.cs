//******************************************************************************************************
//  UserAccount.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/27/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;

namespace GSF.Security.Model
{
    /// <summary>
    /// Model for UserAccount table.
    /// </summary>
    /// <remarks>
    /// A record in this table can represent a database defined user with associated detail
    /// or an Active Directory user that maintains its own user details.
    /// </remarks>
    [PrimaryLabel("Name")]
    public class UserAccount
    {
        /// <summary>
        /// Unique user account ID field.
        /// </summary>
        [PrimaryKey(true)]
        public Guid ID { get; set; }

        /// <summary>
        /// User name field - stores SID for AD authentication.
        /// </summary>
        [Label("User Name")]
        [Required]
        [StringLength(200)]
        [Searchable]
        public string Name { get; set; }

        /// <summary>
        /// Password field - stores password hash for DB authentication only.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Password { get; set; }

        /// <summary>
        /// First name field - for DB authentication only.
        /// </summary>
        [Label("First Name")]
        [StringLength(200)]
        [Searchable]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name field - for DB authentication only.
        /// </summary>
        [Label("Last Name")]
        [StringLength(200)]
        [Searchable]
        public string LastName { get; set; }

        /// <summary>
        /// Default Node ID field.
        /// </summary>
        [Required]
        public Guid DefaultNodeID { get; set; }

        /// <summary>
        /// Phone number field - for DB authentication only.
        /// </summary>
        [StringLength(200)]
        public string Phone { get; set; }

        /// <summary>
        /// E-mail field - for DB authentication only.
        /// </summary>
        [Label("E-Mail")]
        [StringLength(200)]
        [EmailValidation]
        public string Email { get; set; }

        /// <summary>
        /// Change password on date field - for DB authentication only.
        /// </summary>
        public DateTime? ChangePasswordOn
        {
            get; set;
        }

        /// <summary>
        /// Use Active Directory authentication field.
        /// </summary>
        [Label("Use Active Directory Authentication")]
        [DefaultValue(true)]
        public bool UseADAuthentication { get; set; }

        /// <summary>
        /// User locked-out field.
        /// </summary>
        [Label("Locked Out")]
        public bool LockedOut
        {
            get; set;
        }

        /// <summary>
        /// Created on field.
        /// </summary>
        [DefaultValueExpression("DateTime.UtcNow")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Created by field.
        /// </summary>
        [Required]
        [StringLength(200)]
        [DefaultValueExpression("UserInfo.CurrentUserID")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Updated on field.
        /// </summary>
        [DefaultValueExpression("this.CreatedOn", EvaluationOrder = 1)]
        [UpdateValueExpression("DateTime.UtcNow")]
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Updated by field.
        /// </summary>
        [Required]
        [StringLength(200)]
        [DefaultValueExpression("this.CreatedBy", EvaluationOrder = 1)]
        [UpdateValueExpression("UserInfo.CurrentUserID")]
        public string UpdatedBy { get; set; }
    }
}
