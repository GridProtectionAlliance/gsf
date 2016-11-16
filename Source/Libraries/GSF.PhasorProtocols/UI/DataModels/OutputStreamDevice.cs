//******************************************************************************************************
//  OutputStreamDevice.cs - Gbtc
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
//   08/04/2011 - Aniket Salver
//       Generated original version of source code.
//   09/19/2011 - Mehulbhai P Thakkar
//       Added OnPropertyChanged() on all properties to reflect changes on UI.
//       Fixed Load() and GetLookupList() static methods.
//   09/21/2011 - Aniket Salver
//       Fixed issue, which helps in enabling the save button on the screen
//   08/03/2012 - Vijay Sukhavasi
//       Fix add digitals/analogs check boxes while configuring output stream
//   08/14/2012 - Aniket Salver 
//       Added paging and sorting technique.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.Data;
using GSF.TimeSeries.UI;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="OutputStreamDevice"/> information as defined in the database.
    /// </summary>
    public class OutputStreamDevice : DataModelBase
    {
        #region [ Members ]

        private Guid m_nodeID;
        private int m_adapterID;
        private int m_id;
        private int m_idCode;
        private string m_acronym;
        private string m_bpaAcronym;
        private string m_name;
        private string m_phasorDataFormat;
        private string m_frequencyDataFormat;
        private string m_analogDataFormat;
        private string m_coordinateFormat;
        private int m_loadOrder;
        private bool m_enabled;
        private bool m_virtual;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        private bool m_selected;    //added this for use in output stream device wizard.

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s NodeID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevice NodeID is a required field, please provide value.")]
        public Guid NodeID
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
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s AdapterID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevice AdapterID is a required field, please provide value.")]
        public int AdapterID
        {
            get
            {
                return m_adapterID;
            }
            set
            {
                m_adapterID = value;
                OnPropertyChanged("AdapterID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s ID.
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
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s IDCode.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevice IdCode is a required field, please provide value.")]
        public int IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;
                OnPropertyChanged("IDCode");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s Acronym.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevice acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "OutputStreamDevice acronym cannot exceed 200 characters.")]
        [RegularExpression("^[A-Z0-9-'!'_''.' @#\\$]+$", ErrorMessage = "Only upper case letters, numbers, '!', '-', '@', '#', '_' , '.'and '$' are allowed.")]
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
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s BpaAcronym.
        /// </summary>
        [StringLength(4, ErrorMessage = "OutputStreamDevice BpaAcronym cannot exceed 4 characters.")]
        public string BpaAcronym
        {
            get
            {
                return m_bpaAcronym;
            }
            set
            {
                m_bpaAcronym = value;
                OnPropertyChanged("BpaAcronym");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s Name.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevice Name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "OutputStreamDevice Name cannot exceed 200 characters.")]
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
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s PhasorDataFormat.
        /// </summary>
        [StringLength(15, ErrorMessage = "OutputStreamDevice PhasorDataFormat cannot exceed 15 characters.")]
        public string PhasorDataFormat
        {
            get
            {
                return m_phasorDataFormat;
            }
            set
            {
                m_phasorDataFormat = value;
                OnPropertyChanged("PhasorDataFormat");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s FrequencyDataFormat.
        /// </summary>
        [StringLength(15, ErrorMessage = "OutputStreamDevice FrequencyDataFormat cannot exceed 15 characters.")]
        public string FrequencyDataFormat
        {
            get
            {
                return m_frequencyDataFormat;
            }
            set
            {
                m_frequencyDataFormat = value;
                OnPropertyChanged("FrequencyDataFormat");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s AnalogDataFormat.
        /// </summary>
        [StringLength(15, ErrorMessage = "OutputStreamDevice AnalogDataFormat cannot exceed 15 characters.")]
        public string AnalogDataFormat
        {
            get
            {
                return m_analogDataFormat;
            }
            set
            {
                m_analogDataFormat = value;
                OnPropertyChanged("AnalogDataFormat");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s CoordinateFormat.
        /// </summary>
        [StringLength(15, ErrorMessage = "OutputStreamDevice CoordinateFormat cannot exceed 15 characters.")]
        public string CoordinateFormat
        {
            get
            {
                return m_coordinateFormat;
            }
            set
            {
                m_coordinateFormat = value;
                OnPropertyChanged("CoordinateFormat");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevice LoadOrder is a required field, please provide value.")]
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
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s Enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDevice"/>'s Virtual.
        /// </summary>
        public bool Virtual
        {
            get
            {
                return m_virtual;
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="OutputStreamDevice"/> was created on.
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
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamDevice"/> was created by.
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
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time when the current <see cref="OutputStreamDevice"/> was updated on.
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
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamDevice"/> was updated by.
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
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevice"/>'s selected flag.
        /// </summary>
        /// <remarks>Used only for output stream device wizard to make <see cref="OutputStreamDevice"/> selectable via checkbox.</remarks>
        public bool Selected
        {
            get
            {
                return m_selected;
            }
            set
            {
                m_selected = value;
                OnPropertyChanged("Selected");
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// LoadKeys <see cref="OutputStreamDevice"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the OutputStream to filter data.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Phasor"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, int outputStreamID, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> outputStreamDeviceList = new List<int>();
                DataTable outputStreamDeviceTable;

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                outputStreamDeviceTable = database.Connection.RetrieveData(database.AdapterType, string.Format("SELECT ID From OutputStreamDeviceDetail WHERE AdapterID = {1} {0}", sortClause, outputStreamID));

                foreach (DataRow row in outputStreamDeviceTable.Rows)
                {
                    outputStreamDeviceList.Add(row.ConvertField<int>("ID"));
                }

                return outputStreamDeviceList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="OutputStreamDevice"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measurement to be loaded from the datbase</param>
        /// <returns>Collection of <see cref="OutputStreamDevice"/>.</returns>
        public static ObservableCollection<OutputStreamDevice> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                OutputStreamDevice[] outputStreamDeviceList = null;
                DataTable outputStreamDeviceTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);

                    query = string.Format("SELECT NodeID, AdapterID, ID, IDCode, Acronym, BpaAcronym, Name, PhasorDataFormat, " +
                        "FrequencyDataFormat, AnalogDataFormat, CoordinateFormat, LoadOrder, Enabled, Virtual " +
                        "FROM OutputStreamDeviceDetail WHERE ID IN ({0})", commaSeparatedKeys);

                    if (database.IsMySQL)
                    {
                        query = string.Format("SELECT NodeID, AdapterID, ID, IDCode, Acronym, BpaAcronym, Name, PhasorDataFormat, " +
                            "FrequencyDataFormat, AnalogDataFormat, CoordinateFormat, LoadOrder, Enabled, `Virtual` " +
                            "FROM OutputStreamDeviceDetail WHERE ID IN ({0})", commaSeparatedKeys);
                    }

                    outputStreamDeviceTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
                    outputStreamDeviceList = new OutputStreamDevice[outputStreamDeviceTable.Rows.Count];

                    foreach (DataRow row in outputStreamDeviceTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        outputStreamDeviceList[keys.IndexOf(id)] = new OutputStreamDevice()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            AdapterID = row.ConvertField<int>("AdapterID"),
                            ID = id,
                            IDCode = row.ConvertField<int>("IDCode"),
                            Acronym = row.Field<string>("Acronym"),
                            BpaAcronym = row.Field<string>("BpaAcronym"),
                            Name = row.Field<string>("Name"),
                            PhasorDataFormat = row.Field<string>("PhasorDataFormat"),
                            FrequencyDataFormat = row.Field<string>("FrequencyDataFormat"),
                            AnalogDataFormat = row.Field<string>("AnalogDataFormat"),
                            CoordinateFormat = row.Field<string>("CoordinateFormat"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            m_virtual = Convert.ToBoolean(row.Field<object>("Virtual"))
                        };
                    }
                }

                return new ObservableCollection<OutputStreamDevice>(outputStreamDeviceList ?? new OutputStreamDevice[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="OutputStreamDevice"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the output stream to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of OutputStreamDevice defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int outputStreamID, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> OutputStreamDeviceList = new Dictionary<int, string>();
                if (isOptional)
                    OutputStreamDeviceList.Add(0, "Select OutputStreamDevice");

                string query = database.ParameterizedQueryString("SELECT ID, Name FROM OutputStreamDevice WHERE AdapterID = {0} ORDER BY LoadOrder", "adapterID");
                DataTable OutputStreamDeviceTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, outputStreamID);

                foreach (DataRow row in OutputStreamDeviceTable.Rows)
                    OutputStreamDeviceList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return OutputStreamDeviceList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="OutputStreamDevice"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDevice">Information about <see cref="OutputStreamDevice"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, OutputStreamDevice outputStreamDevice)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (outputStreamDevice.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO OutputStreamDevice (NodeID, AdapterID, IDCode, Acronym, BpaAcronym, Name, " +
                        "PhasorDataFormat, FrequencyDataFormat, AnalogDataFormat, CoordinateFormat, LoadOrder, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn)" +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15})", "nodeID", "adapterID", "idCode", "acronym",
                        "bpaAcronym", "name", "phasorDataFormat", "frequencyDataFormat", "analogDataFormat", "coordinateFormat", "loadOrder", "enabled",
                        "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.CurrentNodeID(), outputStreamDevice.AdapterID, outputStreamDevice.IDCode,
                        outputStreamDevice.Acronym, outputStreamDevice.BpaAcronym.ToNotNull(), outputStreamDevice.Name, outputStreamDevice.PhasorDataFormat.ToNotNull(),
                        outputStreamDevice.FrequencyDataFormat.ToNotNull(), outputStreamDevice.AnalogDataFormat.ToNotNull(), outputStreamDevice.CoordinateFormat.ToNotNull(),
                        outputStreamDevice.LoadOrder, database.Bool(outputStreamDevice.Enabled), CommonFunctions.CurrentUser,
                        database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    OutputStreamDevice originalDevice = GetOutputStreamDevice(database, "WHERE ID = " + outputStreamDevice.ID);

                    query = database.ParameterizedQueryString("UPDATE OutputStreamDevice SET NodeID = {0}, AdapterID = {1}, IDCode = {2}, Acronym = {3}, BpaAcronym = {4}, " +
                        "Name = {5}, PhasorDataFormat = {6}, FrequencyDataFormat = {7}, AnalogDataFormat = {8}, CoordinateFormat = {9}, LoadOrder = {10}, Enabled = {11}, " +
                        " UpdatedBy = {12}, UpdatedOn = {13} WHERE ID = {14}", "nodeID", "adapterID", "idCode", "acronym", "bpaAcronym", "name",
                        "phasorDataFormat", "frequencyDataFormat", "analogDataFormat", "coordinateFormat", "loadOrder", "enabled", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.Guid(outputStreamDevice.NodeID), outputStreamDevice.AdapterID, outputStreamDevice.IDCode,
                        outputStreamDevice.Acronym, outputStreamDevice.BpaAcronym.ToNotNull(), outputStreamDevice.Name, outputStreamDevice.PhasorDataFormat.ToNotNull(),
                        outputStreamDevice.FrequencyDataFormat.ToNotNull(), outputStreamDevice.AnalogDataFormat.ToNotNull(), outputStreamDevice.CoordinateFormat.ToNotNull(),
                        outputStreamDevice.LoadOrder, database.Bool(outputStreamDevice.Enabled), CommonFunctions.CurrentUser,
                        database.UtcNow, outputStreamDevice.ID);

                    if (originalDevice != null && originalDevice.Acronym != outputStreamDevice.Acronym)
                    {

                        IList<int> keys = OutputStreamMeasurement.LoadKeys(database, originalDevice.AdapterID);

                        foreach (OutputStreamMeasurement measurement in OutputStreamMeasurement.Load(database, keys))
                        {
                            measurement.SignalReference = measurement.SignalReference.Replace(originalDevice.Acronym + "-", outputStreamDevice.Acronym + "-");
                            OutputStreamMeasurement.Save(database, measurement);
                        }
                    }
                }

                return "OutputStreamDevice information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="OutputStreamDevice"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the output stream to filter data.</param>
        /// <param name="outputStreamDeviceAcronym">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int outputStreamID, string outputStreamDeviceAcronym)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDevice WHERE AdapterID = {0} AND Acronym = {1}", "outputStreamID", "outputStreamDeviceAcronym"), DefaultTimeout, outputStreamID, outputStreamDeviceAcronym);

                try
                {
                    database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE From OutputStreamMeasurement WHERE AdapterID = {0} AND SignalReference LIKE {1}", "outputStreamID", "outputStreamDeviceAcronym"),
                        DefaultTimeout, outputStreamID, "%" + outputStreamDeviceAcronym + "%");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to delete measurements associated with output stream device.", ex);
                }

                return "OutputStreamDevice deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Adds multiple devices to output steam.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the output stream to which devices needs to be added.</param>
        /// <param name="devices">Collection of devices to be added.</param>
        /// <param name="addDigitals">Boolean flag indicating if analogs should be added.</param>
        /// <param name="addAnalogs">Boolean flag indicating if digirals should be added.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string AddDevices(AdoDataConnection database, int outputStreamID, ObservableCollection<Device> devices, bool addDigitals, bool addAnalogs)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                foreach (Device device in devices)
                {
                    OutputStreamDevice outputStreamDevice = new OutputStreamDevice();
                    outputStreamDevice.NodeID = device.NodeID;
                    outputStreamDevice.AdapterID = outputStreamID;
                    outputStreamDevice.Acronym = device.Acronym.Substring(device.Acronym.LastIndexOf("!", StringComparison.Ordinal) + 1);
                    outputStreamDevice.BpaAcronym = string.Empty;
                    outputStreamDevice.Name = device.Name;
                    outputStreamDevice.LoadOrder = device.LoadOrder;
                    outputStreamDevice.Enabled = true;
                    outputStreamDevice.IDCode = device.AccessID;
                    Save(database, outputStreamDevice);

                    outputStreamDevice = GetOutputStreamDevice(database, "WHERE Acronym = '" + outputStreamDevice.Acronym + "' AND AdapterID = " + outputStreamID);

                    if ((object)outputStreamDevice != null)
                    {
                        IList<int> keys = Phasor.LoadKeys(database, device.ID);
                        ObservableCollection<Phasor> phasors = Phasor.Load(database, keys);
                        foreach (Phasor phasor in phasors)
                        {
                            OutputStreamDevicePhasor outputStreamDevicePhasor = new OutputStreamDevicePhasor();
                            outputStreamDevicePhasor.NodeID = device.NodeID;
                            outputStreamDevicePhasor.OutputStreamDeviceID = outputStreamDevice.ID;
                            outputStreamDevicePhasor.Label = phasor.Label;
                            outputStreamDevicePhasor.Type = phasor.Type;
                            outputStreamDevicePhasor.Phase = phasor.Phase;
                            outputStreamDevicePhasor.LoadOrder = phasor.SourceIndex;
                            OutputStreamDevicePhasor.Save(database, outputStreamDevicePhasor);
                        }

                        ObservableCollection<Measurement> measurements = Measurement.Load(database, device.ID);
                        int analogIndex = 0;
                        foreach (Measurement measurement in measurements)
                        {
                            if (measurement.SignalAcronym != "STAT" && measurement.SignalAcronym != "QUAL")
                            {
                                measurement.SignalReference = measurement.SignalReference.Substring(measurement.SignalReference.LastIndexOf("!", StringComparison.Ordinal) + 1);

                                if ((measurement.SignalAcronym != "ALOG" && measurement.SignalAcronym != "DIGI") || (measurement.SignalAcronym == "ALOG" && addAnalogs) || (measurement.SignalAcronym == "DIGI" && addDigitals))
                                {
                                    OutputStreamMeasurement outputStreamMeasurement = new OutputStreamMeasurement();
                                    outputStreamMeasurement.NodeID = device.NodeID;
                                    outputStreamMeasurement.AdapterID = outputStreamID;
                                    outputStreamMeasurement.HistorianID = measurement.HistorianID;
                                    outputStreamMeasurement.PointID = measurement.PointID;
                                    outputStreamMeasurement.SignalReference = measurement.SignalReference;
                                    OutputStreamMeasurement.Save(database, outputStreamMeasurement);
                                }

                                if (addAnalogs && measurement.SignalAcronym == "ALOG")
                                {
                                    OutputStreamDeviceAnalog outputStreamDeviceAnalog = new OutputStreamDeviceAnalog();
                                    outputStreamDeviceAnalog.NodeID = device.NodeID;
                                    outputStreamDeviceAnalog.OutputStreamDeviceID = outputStreamDevice.ID;
                                    outputStreamDeviceAnalog.Label = string.IsNullOrEmpty(measurement.AlternateTag) ? device.Acronym.Length > 12 ? device.Acronym.Substring(0, 12) + ":A" + analogIndex : device.Acronym + ":A" + analogIndex : measurement.AlternateTag; // measurement.PointTag;                                    

                                    int charIndex = measurement.SignalReference.LastIndexOf("-", StringComparison.Ordinal);
                                    int signalIndex;

                                    if (charIndex >= 0 && charIndex + 3 < measurement.SignalReference.Length && int.TryParse(measurement.SignalReference.Substring(charIndex + 3), out signalIndex))
                                        outputStreamDeviceAnalog.LoadOrder = signalIndex;
                                    else
                                        outputStreamDeviceAnalog.LoadOrder = 999;

                                    OutputStreamDeviceAnalog.Save(database, outputStreamDeviceAnalog);
                                    analogIndex++;
                                }
                                else if (addDigitals && measurement.SignalAcronym == "DIGI")
                                {
                                    OutputStreamDeviceDigital outputStreamDeviceDigital = new OutputStreamDeviceDigital();
                                    outputStreamDeviceDigital.NodeID = device.NodeID;
                                    outputStreamDeviceDigital.OutputStreamDeviceID = outputStreamDevice.ID;
                                    outputStreamDeviceDigital.Label = string.IsNullOrEmpty(measurement.AlternateTag) ? DefaultDigitalLabel : measurement.AlternateTag;     // measurement.PointTag;

                                    int charIndex = measurement.SignalReference.LastIndexOf("-", StringComparison.Ordinal);
                                    int signalIndex;

                                    if (charIndex >= 0 && charIndex + 3 < measurement.SignalReference.Length && int.TryParse(measurement.SignalReference.Substring(charIndex + 3), out signalIndex))
                                        outputStreamDeviceDigital.LoadOrder = signalIndex;
                                    else
                                        outputStreamDeviceDigital.LoadOrder = 999;

                                    OutputStreamDeviceDigital.Save(database, outputStreamDeviceDigital);
                                }
                            }
                        }
                    }
                }

                return "Output Stream Device(s) added successfully!";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        //                                                   1         2         3         4         5         6         7         8         9         10        11        12
        //                                          12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678
        private const string DefaultDigitalLabel = "DIGITAL0        DIGITAL1        DIGITAL2        DIGITAL3        DIGITAL4        DIGITAL5        DIGITAL6        DIGITAL7        " +
                                                   "DIGITAL8        DIGITAL9        DIGITAL10       DIGITAL11       DIGITAL12       DIGITAL13       DIGITAL14       DIGITAL15       ";
        //                                          *23456789012345+*23456789012345+*23456789012345+*23456789012345+*23456789012345+*23456789012345+*23456789012345+*23456789012345+

        /// <summary>
        /// Gets output stream device from the database based on where condition provided.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="whereClause">query string to filter data.</param>
        /// <returns><see cref="OutputStreamDevice"/> information.</returns>
        public static OutputStreamDevice GetOutputStreamDevice(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                DataTable deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM OutputStreamDeviceDetail " + whereClause);

                if (deviceTable.Rows.Count == 0)
                    return null;

                DataRow row = deviceTable.Rows[0];

                OutputStreamDevice device = new OutputStreamDevice
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        AdapterID = row.ConvertField<int>("AdapterID"),
                        ID = row.ConvertField<int>("ID"),
                        IDCode = row.ConvertField<int>("IDCode"),
                        Acronym = row.Field<string>("Acronym"),
                        BpaAcronym = row.Field<string>("BpaAcronym"),
                        Name = row.Field<string>("Name"),
                        PhasorDataFormat = row.Field<string>("PhasorDataFormat"),
                        FrequencyDataFormat = row.Field<string>("FrequencyDataFormat"),
                        AnalogDataFormat = row.Field<string>("AnalogDataFormat"),
                        CoordinateFormat = row.Field<string>("CoordinateFormat"),
                        LoadOrder = row.ConvertField<int>("LoadOrder"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_virtual = Convert.ToBoolean(row.Field<object>("Virtual"))
                    };

                return device;

            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets collection of output stream devices.
        /// </summary>
        /// <param name="database">Source database connection.</param>
        /// <param name="whereClause">Where filter clause.</param>
        /// <returns>Collection of output stream devices.</returns>
        public static ObservableCollection<OutputStreamDevice> GetOutputStreamDevices(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<OutputStreamDevice> outputStreamDeviceList = new ObservableCollection<OutputStreamDevice>();
                DataTable outputStreamDeviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM OutputStreamDeviceDetail " + whereClause);

                foreach (DataRow row in outputStreamDeviceTable.Rows)
                {
                    outputStreamDeviceList.Add(new OutputStreamDevice
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            AdapterID = row.ConvertField<int>("AdapterID"),
                            ID = row.ConvertField<int>("ID"),
                            IDCode = row.ConvertField<int>("IDCode"),
                            Acronym = row.Field<string>("Acronym"),
                            BpaAcronym = row.Field<string>("BpaAcronym"),
                            Name = row.Field<string>("Name"),
                            PhasorDataFormat = row.Field<string>("PhasorDataFormat"),
                            FrequencyDataFormat = row.Field<string>("FrequencyDataFormat"),
                            AnalogDataFormat = row.Field<string>("AnalogDataFormat"),
                            CoordinateFormat = row.Field<string>("CoordinateFormat"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            m_virtual = Convert.ToBoolean(row.Field<object>("Virtual"))
                        });
                }

                return outputStreamDeviceList;
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


