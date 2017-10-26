//******************************************************************************************************
//  SignalType.cs - Gbtc
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
//  09/15/2011 - Mehulbhai P Thakkar
//       Hard coded signal types for PMU, Voltage Phasor and Current Phasor.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="SignalType"/> information as defined in the database.
    /// </summary>
    public class SignalType : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_name;
        private string m_acronym;
        private string m_suffix;
        private string m_abbreviation;
        private string m_source;
        private string m_engineeringUnits;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="SignalType"/> ID.
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
        /// Gets or sets <see cref="SignalType"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "SignalType acronym is a required field, please provide value.")]
        [StringLength(4, ErrorMessage = "SignalType Acronym cannot exceed 4 characters.")]
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
        /// Gets or sets <see cref="SignalType"/> Name.
        /// </summary>
        [Required(ErrorMessage = "SignalType name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "SignalType Name cannot exceed 200 characters.")]
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
        /// Gets or sets <see cref="SignalType"/> Suffix.
        /// </summary>
        [Required(ErrorMessage = "SignalType suffix is required field, please provide value.")]
        [StringLength(2, ErrorMessage = "SignalType Suffix cannot exceed 2 characters.")]
        public string Suffix
        {
            get
            {
                return m_suffix;
            }
            set
            {
                m_suffix = value;
                OnPropertyChanged("Suffix");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SignalType"/> Abbreviation.
        /// </summary>
        [Required(ErrorMessage = "SignalType Abbreviation is required field, please provide value.")]
        [StringLength(2, ErrorMessage = "SignalType abbreviation cannot exceed 2 characeters.")]
        public string Abbreviation
        {
            get
            {
                return m_abbreviation;
            }
            set
            {
                m_abbreviation = value;
                OnPropertyChanged("Abbreviation");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SignalType"/> Source.
        /// </summary>
        [Required(ErrorMessage = "SignalType Source is required field, please provide value.")]
        [StringLength(10, ErrorMessage = "SignalType source cannot exceed 10 characters.")]
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
        /// Gets or sets <see cref="SignalType"/> EngineeringUnits.
        /// </summary>
        [StringLength(10, ErrorMessage = "SignalType engineering units cannot exceed 10 characters.")]
        public string EngineeringUnits
        {
            get
            {
                return m_engineeringUnits;
            }
            set
            {
                m_engineeringUnits = value;
                OnPropertyChanged("EngineeringUnits");
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads <see cref="SignalType"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="source">Type if source to filter data.</param>
        /// <param name="phasorType">Type of phasor type to filter data.</param>
        /// <returns>Collection of <see cref="SignalType"/>.</returns>
        public static ObservableCollection<SignalType> Load(AdoDataConnection database, string source = "", string phasorType = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<SignalType> signalTypeList = new ObservableCollection<SignalType>();

                string query = "SELECT ID, Acronym, Name, Suffix, Abbreviation, Source, EngineeringUnits FROM SignalType ORDER BY Name";

                if (!string.IsNullOrEmpty(source) && source.ToUpper() == "PMU")
                {
                    query = "SELECT ID, Acronym, Name, Suffix, Abbreviation, Source, EngineeringUnits FROM SignalType Where Source = 'PMU' AND SUFFIX IN ('FQ','DF','SF','AV','DV','CV') ORDER BY Name";
                }
                else if (!string.IsNullOrEmpty(source) && source.ToUpper() == "PHASOR" && !string.IsNullOrEmpty(phasorType))
                {
                    if (phasorType.ToUpper() == "V")
                        query = "SELECT ID, Acronym, Name, Suffix, Abbreviation, Source, EngineeringUnits FROM SignalType Where Source = 'Phasor' " +
                            "AND Acronym IN ('VPHM', 'VPHA') ORDER BY Name";
                    else if (phasorType.ToUpper() == "I")
                        query = "SELECT ID, Acronym, Name, Suffix, Abbreviation, Source, EngineeringUnits FROM SignalType Where Source = 'Phasor' " +
                            "AND Acronym IN ('IPHM', 'IPHA') ORDER BY Name";
                }

                DataTable signalTypeTable = database.Connection.RetrieveData(database.AdapterType, query);

                foreach (DataRow row in signalTypeTable.Rows)
                {
                    signalTypeList.Add(new SignalType()
                    {
                        ID = row.ConvertField<int>("ID"),
                        Acronym = row.Field<string>("Acronym"),
                        Name = row.Field<string>("Name"),
                        Suffix = row.Field<string>("Suffix"),
                        Abbreviation = row.Field<string>("Abbreviation"),
                        Source = row.Field<string>("Source"),
                        EngineeringUnits = row["EngineeringUnits"].ToNonNullString()
                    });
                }

                return signalTypeList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="SignalType"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> signalTypeList = new Dictionary<int, string>();
                DataTable signalTypeTable;

                if (isOptional)
                    signalTypeList.Add(0, "Select SignalType");

                signalTypeTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM SignalType ORDER BY Name");

                foreach (DataRow row in signalTypeTable.Rows)
                    signalTypeList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return signalTypeList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Method to return signal types for PMU device.
        /// </summary>
        /// <returns>Returns <see cref="ObservableCollection{T}"/> type collection of signal types.</returns>
        public static ObservableCollection<SignalType> GetPmuSignalTypes()
        {
            if (s_pmuSignalTypes == null || s_pmuSignalTypes.Count == 0)
                s_pmuSignalTypes = Load(null, "PMU");

            return s_pmuSignalTypes;
        }

        /// <summary>
        /// Method to return signal types for voltage phasor.
        /// </summary>
        /// <returns>Returns <see cref="ObservableCollection{T}"/> type collection of signal types.</returns>
        public static ObservableCollection<SignalType> GetVoltagePhasorSignalTypes()
        {
            if (s_voltagePhasorSignalTypes == null || s_voltagePhasorSignalTypes.Count == 0)
                s_voltagePhasorSignalTypes = Load(null, "Phasor", "V");

            return s_voltagePhasorSignalTypes;
        }

        /// <summary>
        /// Method to return signal types for current phasor.
        /// </summary>
        /// <returns>Returns <see cref="ObservableCollection{T}"/> type collection of signal types.</returns>
        public static ObservableCollection<SignalType> GetCurrentPhasorSignalTypes()
        {
            if (s_currentPhasorSignalTypes == null || s_currentPhasorSignalTypes.Count == 0)
                s_currentPhasorSignalTypes = Load(null, "Phasor", "I");

            return s_currentPhasorSignalTypes;
        }

        // Static Fields

        private static ObservableCollection<SignalType> s_pmuSignalTypes;
        private static ObservableCollection<SignalType> s_voltagePhasorSignalTypes;
        private static ObservableCollection<SignalType> s_currentPhasorSignalTypes;

        #endregion
    }
}
