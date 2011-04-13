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
    /// Represents a record of ErrorLog information as defined in the database.
    /// </summary>
    public class ErrorLog : DataModelBase
    {
        #region[Members]

        //Fields
        private int m_ID;
        private string m_source;
        private string m_message;
        private string m_detail;
        private DateTime m_createdOn;

        #endregion

        #region[Properties]

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int ID
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
        /// Gets or sets <see cref="ErrorLog"/> Source.
        /// </summary>
        [Required(ErrorMessage = "ErrorLog Source is a required field, please provide value.")]
        [StringLength(256, ErrorMessage = "ErrorLog acronym cannot exceed 256 characters.")]
        public string Source
        {
            get
            {
                return m_source;
            }
            set
            {
                m_source = value;
                OnPropertyChanged("Source");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> Message.
        /// </summary>
        [Required(ErrorMessage = "ErrorLog Message is a required field, please provide value.")]
        [StringLength(1024, ErrorMessage = "ErrorLog acronym cannot exceed 1024 characters.")]
        public string Message
        {
            get
            {
                return m_message;
            }
            set
            {
                m_message = value;
                OnPropertyChanged("Message");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> Detail.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string Detail
        {
            get
            {
                return m_detail;
            }
            set
            {
                m_detail = value;
                OnPropertyChanged("Detail");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> CreatedOn.
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

        #endregion
    }
}
