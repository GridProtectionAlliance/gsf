//******************************************************************************************************
//  Interconnection.cs - Gbtc
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
//  05/06/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added regular expression validator for Acronym.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Interconnection"/> information as defined in the database.
    /// </summary>
    public class Interconnection : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_acronym;
        private string m_name;
        private int m_loadOrder;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Interconnection"/> ID.
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
        /// Gets or sets <see cref="Interconnection"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Interconnection acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Interconnection Acronym cannot exceed 200 characters.")]
        [AcronymValidation]
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
        /// Gets or sets <see cref="Interconnection"/> Name.
        /// </summary>
        [Required(ErrorMessage = "Interconnection name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Interconnection Name cannot exceed 200 characters.")]
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
        /// Gets or sets <see cref="Interconnection"/> LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "Interconnection load order is a required field, please provide value.")]
        [DefaultValue(0)]
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

        #endregion

        #region [ Static ]

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Interconnection"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>        
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> interconnectionList = new Dictionary<int, string>();
                DataTable interconnectionTable;

                if (isOptional)
                    interconnectionList.Add(0, "Select Interconnection");

                interconnectionTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Interconnection ORDER BY LoadOrder");

                foreach (DataRow row in interconnectionTable.Rows)
                    interconnectionList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return interconnectionList;
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
