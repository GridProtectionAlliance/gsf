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
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="ErrorLog"/> information as defined in the database.
    /// </summary>
    public class ErrorLog : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_source;
        private string m_type;
        private string m_message;
        private string m_detail;
        private DateTime m_createdOn;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> ID.
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
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> Source.
        /// </summary>
        [Required(ErrorMessage = "Error log source is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Error log source cannot exceed 200 characters.")]
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
        /// Gets or sets <see cref="ErrorLog"/> Type.
        /// </summary>
        [StringLength(200, ErrorMessage = "Error log type cannot exceed 200 characters.")]
        public string Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ErrorLog"/> Message.
        /// </summary>
        [Required(ErrorMessage = "Error log message is a required field, please provide value.")]
        [StringLength(1024, ErrorMessage = "Error mog message cannot exceed 1024 characters.")]
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
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
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

        #endregion
    }
}
