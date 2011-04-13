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
using System.Collections.ObjectModel;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Creates a new object that represents a SecurityGroup
    /// </summary>
    class SecurityGroup : DataModelBase
    {
        #region [Members]

        //Fileds
        private string m_ID;
        private string m_name;
        private string m_description;
        private DateTime m_createdOn;
        private string m_CreatedBy;
        private DateTime m_UpdatedOn;
        private string m_UpdatedBy;
        private ObservableCollection<SecurityGroup> m_CurrentGroupUsers;
        private ObservableCollection<SecurityGroup> m_PossibleGroupUsers;

        #endregion

        #region [properties]

        /// <summary>
        /// Gets and sets the current SecurityGroup ID
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
        /// Gets and sets the current SecurityGroup Name
        /// </summary>
        [Required(ErrorMessage = " SecurityGroup Name is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "SecurityGroup Name cannot exceed 50 characters.")]
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
        /// Gets and sets the current SecurityGroup Description
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets and sets the current SecurityGroup CreatedOn
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets and sets the current SecurityGroup CreatedBy
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_CreatedBy;
            }
            set
            {
                m_CreatedBy = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup UpdatedOn
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_UpdatedOn;
            }
            set
            {
                m_UpdatedOn = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup UpdatedBy
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_UpdatedBy;
            }
            set
            {
                m_UpdatedBy = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup CurrentGroupUsers
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<SecurityGroup> CurrentGroupUsers
        {
            get
            {
                return m_CurrentGroupUsers;
            }
            set
            {
                m_CurrentGroupUsers = value;
            }
        }

        /// <summary>
        /// Gets and sets the current SecurityGroup PossibleGroupUsers
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<SecurityGroup> PossibleGroupUsers
        {
            get
            {
                return m_PossibleGroupUsers;
            }
            set
            {
                m_PossibleGroupUsers = value;
            }
        }

        #endregion

    }
}
