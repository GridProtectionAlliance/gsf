//******************************************************************************************************
//  Statistic.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  07/20/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using GSF.Data;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Statistic"/> information as defined in the database.
    /// </summary>
    public class Statistic : DataModelBase
    {

        #region [ Members ]

        // Fields
        private int m_id;
        private string m_source;
        private int m_signalIndex;
        private string m_name;
        private string m_description;
        private string m_assemblyName;
        private string m_typeName;
        private string m_methodName;
        private string m_arguments;
        private bool m_enabled;
        private string m_dataType;
        private string m_displayFormat;
        private bool m_isConnectedState;
        private int m_loadOrder;
        //private string m_value;
        //private string m_timeTag;
        //private string m_quality;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int ID
        {
            get => m_id;
            set => m_id = value;
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s Source.
        /// </summary>
        [Required(ErrorMessage = "Statistic Source is a required field, please provide a value.")]
        [StringLength(20, ErrorMessage = "Statistic Source cannot exceed 20 characters.")]
        public string Source
        {
            get => m_source;
            set
            {
                m_source = value;
                OnPropertyChanged("Source");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s SignalIndex.
        /// </summary>
        [Required(ErrorMessage = "Statistic Signal Index is a required field, please provice a value.")]
        public int SignalIndex
        {
            get => m_signalIndex;
            set
            {
                m_signalIndex = value;
                OnPropertyChanged("SignalIndex");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s Name.
        /// </summary>
        [Required(ErrorMessage = "Statistic Name is a required field, please provide a value.")]
        [StringLength(200, ErrorMessage = "Statistic Name cannot exceed 200 characters.")]
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s Description.
        /// </summary>
        public string Description
        {
            get => m_description;
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s AssemblyName.
        /// </summary>
        [Required(ErrorMessage = "Statistic Assembly Name is a required field, please provide a value.")]
        public string AssemblyName
        {
            get => m_assemblyName;
            set
            {
                m_assemblyName = value;
                OnPropertyChanged("AssemblyName");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s TypeName.
        /// </summary>
        [Required(ErrorMessage = "Statistic Type Name is a required field, please provide a value.")]
        public string TypeName
        {
            get => m_typeName;
            set
            {
                m_typeName = value;
                OnPropertyChanged("TypeName");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s MethodName.
        /// </summary>
        [Required(ErrorMessage = "Statistic Method Name is a required field, please provide a value.")]
        public string MethodName
        {
            get => m_methodName;
            set
            {
                m_methodName = value;
                OnPropertyChanged("MethodName");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s Arguments.
        /// </summary>
        public string Arguments
        {
            get => m_arguments;
            set
            {
                m_arguments = value;
                OnPropertyChanged("Arguments");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/> Enabled.
        /// </summary>
        [DefaultValue(true)]
        public bool Enabled
        {
            get => m_enabled;
            set
            {
                m_enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s DataType.
        /// </summary>
        [StringLength(200, ErrorMessage = "Statistic DataType cannot exceed 200 characters.")]
        public string DataType
        {
            get => m_dataType;
            set
            {
                m_dataType = value;
                OnPropertyChanged("DataType");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s DisplayFormat.
        /// </summary>
        [StringLength(200, ErrorMessage = "Statistic DisplayFormat cannot exceed 200 characters.")]
        public string DisplayFormat
        {
            get => m_displayFormat;
            set
            {
                m_displayFormat = value;
                OnPropertyChanged("DisplayFormat");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s IsConnectedState.
        /// </summary>
        public bool IsConnectedState
        {
            get => m_isConnectedState;
            set
            {
                m_isConnectedState = value;
                OnPropertyChanged("IsConnectedState");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Statistic"/>'s LoadOrder.
        /// </summary>
        [DefaultValue(0)]
        public int LoadOrder
        {
            get => m_loadOrder;
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads <see cref="Statistic"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="Statistic"/>.</returns>        
        public static ObservableCollection<Statistic> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Statistic> statisticList = new();

                DataTable statisticTable = database.Connection.RetrieveData(database.AdapterType, "SELECT Source, SignalIndex, Name, Description, DataType, DisplayFormat, " +
                    "IsConnectedState, LoadOrder FROM Statistic ORDER BY Source, SignalIndex");

                foreach (DataRow row in statisticTable.Rows)
                {
                    statisticList.Add(new Statistic()
                    {
                        Source = row.Field<string>("Source"),
                        SignalIndex = row.ConvertField<int>("SignalIndex"),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<string>("Description"),
                        DataType = row.Field<string>("DataType"),
                        DisplayFormat = row.Field<string>("DisplayFormat"),
                        IsConnectedState = Convert.ToBoolean(row.Field<object>("IsConnectedState")),
                        LoadOrder = row.ConvertField<int>("LoadOrder")
                    });
                }

                return statisticList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Statistic"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection of statistics defined in the database.</returns>
        // TODO: For now, this method is just a place holder. In future when we create screen to manage Statistic
        // definitions, we will need to add code to it.
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            return null;
        }

        /// <summary>
        /// Saves <see cref="Statistic"/> information into the database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="statistic">Information about <see cref="Statistic"/>.</param>
        /// <returns>String, for display use, indicating success.</returns>
        // TODO: For now, this method is just a place holder. In future when we create screen to manage Statistic
        // definitions, we will need to add code to it.
        public static string Save(AdoDataConnection database, Statistic statistic)
        {
            return string.Empty;
        }

        /// <summary>
        /// Deletes specified <see cref="Statistic"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="statisticID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        // TODO: For now, this method is just a place holder. In future when we create screen to manage Statistic
        // definitions, we will need to add code to it.
        public static string Delete(AdoDataConnection database, int statisticID)
        {
            return string.Empty;
        }

        #endregion

    }
}
