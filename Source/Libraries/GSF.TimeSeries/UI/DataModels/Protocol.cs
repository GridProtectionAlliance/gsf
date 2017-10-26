//******************************************************************************************************
//  Protocol.cs - Gbtc
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
// 05/13/2011 - Aniket Salver
//                  Modified the way Guid is retrived from the Data Base.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added regular expression validator for Acronym
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Protocol"/> information as defined in the database.
    /// </summary>
    public class Protocol : DataModelBase
    {

        #region [ Members ]

        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_type;
        private int m_loadOrder;
        private string m_category;
        private string m_assemblyName;
        private string m_typeName;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Protocol"/> ID.
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
        /// Gets or sets <see cref="Protocol"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Protocol acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Protocol Acronym cannot exceed 200 characters.")]
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
        /// Gets or sets <see cref="Protocol"/> Name.
        /// </summary>
        [Required(ErrorMessage = "Protocol name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Protocol Name cannot exceed 200 characters.")]
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
        /// Gets or sets <see cref="Protocol"/> Type.
        /// </summary>
        [StringLength(200, ErrorMessage = "Protocol Type cannot exceed 200 characters.")]
        [DefaultValue("Frame")]
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
        /// Gets or sets <see cref="Protocol"/> Category.
        /// </summary>
        [StringLength(200, ErrorMessage = "Protocol Category cannot exceed 200 characters.")]
        [DefaultValue("Phasor")]
        public string Category
        {
            get
            {
                return m_category;
            }
            set
            {
                m_category = value;
                OnPropertyChanged("Category");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Protocol"/> AssemblyName.
        /// </summary>
        [Required(ErrorMessage = "Protocol AssemblyName is a required field, please provide value.")]
        [DefaultValue("GSF.PhasorProtocols.dll")]
        public string AssemblyName
        {
            get
            {
                return m_assemblyName;
            }
            set
            {
                m_assemblyName = value;
                OnPropertyChanged("AssemblyName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Protocol"/> TypeName.
        /// </summary>
        [Required(ErrorMessage = "Protocol TypeName is a required field, please provide value.")]
        [DefaultValue("GSF.PhasorProtocols.PhasorMeasurementMapper")]
        public string TypeName
        {
            get
            {
                return m_typeName;
            }
            set
            {
                m_typeName = value;
                OnPropertyChanged("TypeName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Protocol"/> LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "Protocol load order is a required field, please provide value.")]
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
        /// Loads protocol list from database.
        /// </summary>
        /// <param name="database">ADO database connection.</param>
        /// <returns>List of protocols.</returns>
        public static ObservableCollection<Protocol> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<Protocol> protocolList = new ObservableCollection<Protocol>();
                DataTable protocolTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM Protocol ORDER BY LoadOrder");

                foreach (DataRow row in protocolTable.Rows)
                {
                    protocolList.Add(new Protocol()
                    {
                        ID = row.ConvertField<int>("ID"),
                        Acronym = row.Field<string>("Acronym"),
                        Name = row.Field<string>("Name"),
                        Type = row.Field<string>("Type"),
                        Category = row.Field<string>("Category"),
                        AssemblyName = row.Field<string>("AssemblyName"),
                        TypeName = row.Field<string>("TypeName"),
                        LoadOrder = row.ConvertField<int>("LoadOrder")
                    });
                }

                return protocolList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Protocol"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> protocolList = new Dictionary<int, string>();
                DataTable protocolTable;

                if (isOptional)
                    protocolList.Add(0, "Select Protocol");

                protocolTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM Protocol ORDER BY LoadOrder");

                foreach (DataRow row in protocolTable.Rows)
                    protocolList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return protocolList;
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
