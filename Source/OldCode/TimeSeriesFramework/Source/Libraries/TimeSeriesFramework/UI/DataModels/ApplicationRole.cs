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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    ///  Represents a record of ApplicationRole information as defined in the database.
    /// </summary>
    class ApplicationRole : DataModelBase
    {
        #region [Members]

        //Fileds
        private string m_ID;
        private string m_nodeID;
        private string m_name;
        private string m_description;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_UpdatedOn;
        private string m_updatedBy;
        private ObservableCollection<ApplicationRole> m_currentRoleGroups;
        private ObservableCollection<ApplicationRole> m_possibleRoleGroups;
        private ObservableCollection<ApplicationRole> m_currentRoleUsers;
        private ObservableCollection<ApplicationRole> m_possibleRoleUsers;

        #endregion

        #region[Properties]

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> ID.
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
        /// Gets or sets <see cref="ApplicationRole"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = " ApplicationRole Name is a required field, please provide value.")]
        [StringLength(36, ErrorMessage = "ApplicationRole Name cannot exceed 36 characters.")]
        public string NodeID
        {
            get
            {
                return m_nodeID;
            }
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> Name.
        /// </summary>
        [Required(ErrorMessage = " ApplicationRole Name is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "ApplicationRole Name cannot exceed 50 characters.")]
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
        /// Gets or sets <see cref="ApplicationRole"/> Description.
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
        /// Gets or sets <see cref="ApplicationRole"/> CreatedOn.
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
        /// Gets or sets <see cref="ApplicationRole"/> CreatedBy.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="ApplicationRole"/> UpdatedOn.
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
                OnPropertyChanged("UpdateOn");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> CurrentRoleGroups.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<ApplicationRole> CurrentRoleGroups
        {
            get
            {
                return m_currentRoleGroups;
            }
            set
            {
                m_currentRoleGroups = value;
                OnPropertyChanged("CurrentRoleGroups");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> PossibleRoleGroups.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<ApplicationRole> PossibleRoleGroups
        {
            get
            {
                return m_possibleRoleGroups;
            }
            set
            {
                m_possibleRoleGroups = value;
                OnPropertyChanged("PossibleRoleGroups");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> CurrentRoleUsers.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<ApplicationRole> CurrentRoleUsers
        {
            get
            {
                return m_currentRoleUsers;
            }
            set
            {
                m_currentRoleUsers = value;
                OnPropertyChanged("CurrentRoleUsers");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ApplicationRole"/> PossibleRoleUsers.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public ObservableCollection<ApplicationRole> PossibleRoleUsers
        {
            get
            {
                return m_possibleRoleUsers;
            }
            set
            {
                m_possibleRoleUsers = value;
                OnPropertyChanged("PossibleRoleUsers");
            }
        }

        #endregion
    }
}
