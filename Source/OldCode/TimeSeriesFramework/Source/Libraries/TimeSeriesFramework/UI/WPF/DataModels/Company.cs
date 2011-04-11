//******************************************************************************************************
//  Company.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of company information as defined in the database.
    /// </summary>
    public class Company : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_acronym;
        private string m_mapAcronym;
        private string m_name;
        private string m_url;
        private int m_loadOrder;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Company"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int ID
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
        /// Gets or sets <see cref="Company"/> acronym.
        /// </summary>
        [Required(ErrorMessage = "Company acronym is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "Company acronym cannot exceed 50 characters.")]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets map acronym used by <see cref="Company"/>.
        /// </summary>
        [Required(ErrorMessage = "Company map acronym is a required field, please provide value.")]
        [StringLength(3, ErrorMessage = "Company map acronym cannot exceed 3 characters.")]
        public string MapAcronym
        {
            get
            {
                return m_mapAcronym;
            }
            set
            {
                m_mapAcronym = value;
                OnPropertyChanged("MapAcronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Company"/> name.
        /// </summary>
        [Required(ErrorMessage = "Company name is a required field, please provide value.")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
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
        /// Gets or sets <see cref="Company"/> URL.
        /// </summary>
        [DataType(DataType.Url, ErrorMessage = "Company web site address URL is not formatted properly.")]
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
        /// Gets or sets desired load order of <see cref="Company"/> record.
        /// </summary>
        [Required(ErrorMessage = "Company load order value is required field, please provide value.")]
        [DefaultValue(typeof(int), "0")]
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Company"/> was created.
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
        /// Gets or sets who the current <see cref="Company"/> was created by.
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
        /// Gets or sets when the current <see cref="Company"/> was updated.
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
        /// Gets or sets who the current <see cref="Company"/> was updated by.
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