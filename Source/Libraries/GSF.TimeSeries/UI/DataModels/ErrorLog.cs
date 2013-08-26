//******************************************************************************************************
//  ErrorLog.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
// 09/10/2012 - Aniket Salver
//       Added paging technique and implemented sorting.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
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
        [StringLength(1024, ErrorMessage = "Error log message cannot exceed 1024 characters.")]
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

        #region[ Static ]

        /// <summary>
        /// Loads <see cref="ErrorLog"/> IDs as an <see cref="IList{T}"/>.
        /// </summary>        
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Int32"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> ErrorLogList = new List<int>();
                string sortClause = string.Empty;
                DataTable ErrorLogTable;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                ErrorLogTable = database.Connection.RetrieveData(database.AdapterType, string.Format("SELECT ID FROM ErrorLog {0} ", sortClause));

                foreach (DataRow row in ErrorLogTable.Rows)
                {
                    ErrorLogList.Add(row.ConvertField<int>("ID"));
                }

                return ErrorLogList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="ErrorLog"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the ErrorLogs to be loaded from the database.</param>
        /// <returns>Collection of <see cref="ErrorLog"/>.</returns>
        public static ObservableCollection<ErrorLog> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                ErrorLog[] errorLogList = null;
                DataTable errorLogTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "" + key.ToString() + "").Aggregate((str1, str2) => str1 + "," + str2);
                    query = string.Format("SELECT ID, Source, Type, Message, Detail, CreatedOn  FROM ErrorLog WHERE ID IN ({0})", commaSeparatedKeys);
                    errorLogTable = database.Connection.RetrieveData(database.AdapterType, query);
                    errorLogList = new ErrorLog[errorLogTable.Rows.Count];

                    foreach (DataRow row in errorLogTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        errorLogList[keys.IndexOf(id)] = new ErrorLog()
                        {
                            ID = id,
                            Source = row.Field<String>("Source"),
                            Type = row.Field<String>("Type"),
                            Message = row.Field<String>("Message"),
                            Detail = row.Field<String>("Detail"),
                            CreatedOn = row.Field<DateTime>("CreatedOn")
                        };
                    }
                }

                return new ObservableCollection<ErrorLog>(errorLogList ?? new ErrorLog[0]);
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
