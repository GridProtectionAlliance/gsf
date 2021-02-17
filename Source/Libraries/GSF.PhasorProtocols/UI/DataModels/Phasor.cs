//******************************************************************************************************
//  Phasor.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
//  05/13/2011 - Mehulbhai P Thakkar
//       Modified static methods to filter data by device.
//  09/15/2012 - Aniket Salver 
//          Added paging and sorting technique. 
//******************************************************************************************************

using GSF.Data;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;
using PhasorProtocolAdapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Phasor"/> information as defined in the database.
    /// </summary>
    public class Phasor : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_deviceID;
        private string m_label;
        private string m_type;
        private string m_phase;
        private int m_baseKV;
        private int? m_destinationPhasorID;
        private int m_sourceIndex;
        //private string m_destinationPhasorLabel;
        //private string m_deviceAcronym;
        //private string m_phasorType;
        //private string m_phaseType;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied.
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> DeviceID.
        /// </summary>
        [Required(ErrorMessage = "Phasor device ID is a required field, please provide value.")]
        public int DeviceID
        {
            get => m_deviceID;
            set
            {
                m_deviceID = value;
                OnPropertyChanged("DeviceID");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> Label.
        /// </summary>
        [Required(ErrorMessage = "Phasor label is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Phasor label must not exceed 200 characters.")]
        public string Label
        {
            get => m_label;
            set
            {
                if (!(value is null) && value.Length > 200)
                    m_label = value.Substring(0, 200);
                else
                    m_label = value;

                OnPropertyChanged("Label");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Phasor"/> Type.
        /// </summary>
        [DefaultValue("V")]
        [StringLength(1, ErrorMessage = "Phasor type must be 1 character.")]
        public string Type
        {
            get => m_type;
            set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Gets or sets the Phase of the current <see cref="Phasor"/>.
        /// </summary>
        [DefaultValue("+")]
        [StringLength(1, ErrorMessage = "Phasor phase must be 1 character.")]
        public string Phase
        {
            get => m_phase;
            set
            {
                m_phase = value;
                OnPropertyChanged("Phase");
            }
        }

        /// <summary>
        /// Gets or sets nominal voltage level of line/bus associated with phasor.
        /// </summary>
        [DefaultValue(0)]
        public int BaseKV
        {
            get => m_baseKV;
            set
            {
                m_baseKV = value;
                OnPropertyChanged("BaseKV");
            }
        }

        /// <summary>
        /// Gets or sets Destination <see cref="Phasor"/> ID for the current Phasor.
        /// </summary>
        // Because of Database design, no validation attributes are applied
        public int? DestinationPhasorID
        {
            get => m_destinationPhasorID;
            set
            {
                m_destinationPhasorID = value;
                OnPropertyChanged("DestinationPhasorID");
            }
        }

        /// <summary>
        /// Gets or sets Source Index for the current Phasor.
        /// </summary>
        [DefaultValue(typeof(int), "0")]
        public int SourceIndex
        {
            get => m_sourceIndex;
            set
            {
                m_sourceIndex = value;
                OnPropertyChanged("SourceIndex");
            }
        }

        ///// <summary>
        ///// Gets the Destination <see cref="Phasor"/> Label for the current Phasor.
        ///// </summary>
        //public string DestinationPhasorLabel
        //{
        //    get
        //    {
        //        return m_destinationPhasorLabel;
        //    }
        //}

        ///// <summary>
        ///// Gets the Device Acronym for the current <see cref="Phasor"/>.
        ///// </summary>
        //public string DeviceAcronym
        //{
        //    get
        //    {
        //        return m_deviceAcronym;
        //    }
        //}

        ///// <summary>
        ///// Gets <see cref="Phasor"/> Type for the current Phasor.
        ///// </summary>
        //public string PhasorType
        //{
        //    get
        //    {
        //        return m_phasorType;
        //    }
        //}

        ///// <summary>
        ///// Gets Phase Type for the current <see cref="Phasor"/>.
        ///// </summary>
        //public string PhaseType
        //{
        //    get
        //    {
        //        return m_phaseType;
        //    }
        //}

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="Phasor"/> was created on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets who the current <see cref="Phasor"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="Phasor"/> was updated on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Gets or sets who the current <see cref="Phasor"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy { get; set; }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// LoadKeys <see cref="Phasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the <see cref="Device"/> to filter data.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Phasor"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, int deviceID, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> phasorList = new List<int>();

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = $"ORDER BY {sortMember} {sortDirection}";

                string query = database.ParameterizedQueryString($"SELECT ID From PhasorDetail WHERE DeviceID = {{0}} {sortClause}", "deviceID");
                DataTable phasorTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, deviceID);

                foreach (DataRow row in phasorTable.Rows)
                    phasorList.Add(row.ConvertField<int>("ID"));

                return phasorList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Phasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the phasors to be loaded from the database</param>
        /// <returns>Collection of <see cref="Phasor"/>.</returns>
        public static ObservableCollection<Phasor> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Phasor[] phasorList = null;

                if (!(keys is null) && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "" + key.ToString() + "").Aggregate((str1, str2) => str1 + "," + str2);
                    query = $"SELECT ID, DeviceID, Label, Type, Phase, BaseKV, DestinationPhasorID, SourceIndex, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn FROM Phasor WHERE ID IN ({commaSeparatedKeys})";

                    DataTable phasorTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
                    phasorList = new Phasor[phasorTable.Rows.Count];

                    foreach (DataRow row in phasorTable.Rows)
                    {
                        int id = row.ConvertField<int>("ID");

                        phasorList[keys.IndexOf(id)] = new Phasor()
                        {
                            ID = id,
                            DeviceID = row.ConvertField<int>("DeviceID"),
                            Label = row.Field<string>("Label"),
                            Type = row.Field<string>("Type"),
                            Phase = row.Field<string>("Phase"),
                            BaseKV = row.ConvertField<int>("BaseKV"),
                            SourceIndex = row.ConvertField<int>("SourceIndex")
                        };
                    }
                }

                return new ObservableCollection<Phasor>(phasorList ?? new Phasor[0]);
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Phasor"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the <see cref="Device"/> to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Label of phasors defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int deviceID, bool isOptional = true)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> phasorList = new Dictionary<int, string>();
                if (isOptional)
                    phasorList.Add(0, "Select Phasor");

                if (deviceID == 0)
                    return phasorList;

                string query = database.ParameterizedQueryString("SELECT ID, Label FROM Phasor WHERE DeviceID = {0} ORDER BY SourceIndex", "deviceID");
                DataTable phasorTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, deviceID);

                foreach (DataRow row in phasorTable.Rows)
                    phasorList[row.ConvertField<int>("ID")] = row.Field<string>("Label");

                return phasorList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Phasor"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="phasor">Information about <see cref="Phasor"/>.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Phasor phasor)
        {
            return SaveAndReorder(database, phasor, phasor.SourceIndex);
        }

        /// <summary>
        /// Saves <see cref="Phasor"/> information to database and skips associated measurement update.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="phasor">Information about <see cref="Phasor"/>.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string SaveWithoutMeasurementUpdate(AdoDataConnection database, Phasor phasor)
        {
            return SaveAndReorder(database, phasor, phasor.SourceIndex, true);
        }

        /// <summary>
        /// Saves <see cref="Phasor"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="phasor">Information about <see cref="Phasor"/>.</param>
        /// <param name="oldSourceIndex">The old source index of the phasor.</param>
        /// <param name="skipMeasurementUpdate">Skips associated measurement update if this is already being handled.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string SaveAndReorder(AdoDataConnection database, Phasor phasor, int oldSourceIndex, bool skipMeasurementUpdate = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (phasor.SourceIndex == 0)
                    phasor.SourceIndex = database.ExecuteScalar<int>("SELECT MAX(SourceIndex) FROM Phasor WHERE DeviceID = {0}", phasor.DeviceID) + 1;

                // Since phasors could be reordered in the source device, this test could inadvertently throw an exception when it should not - so the validation has been removed
                //if (database.ExecuteScalar<int>("SELECT COUNT(*) FROM Phasor WHERE ID <> {0} AND DeviceID = {1} AND SourceIndex = {2}", phasor.ID, phasor.DeviceID, phasor.SourceIndex) > 0)
                //    throw new InvalidOperationException("Phasor source index must be unique per device.");

                string query;
                
                if (phasor.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO Phasor (DeviceID, Label, Type, Phase, BaseKV, SourceIndex, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})", "deviceID", "label", "type", "phase", "baseKV", "sourceIndex", "updatedBy", "updatedOn", "createdBy",
                        "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, phasor.DeviceID, phasor.Label, phasor.Type, phasor.Phase, phasor.BaseKV, phasor.SourceIndex,
                        CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE Phasor SET DeviceID = {0}, Label = {1}, Type = {2}, Phase = {3}, BaseKV = {4}, SourceIndex = {5}, " +
                        "UpdatedBy = {6}, UpdatedOn = {7} WHERE ID = {8}", "deviceID", "label", "type", "phase", "baseKV", "sourceIndex", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, phasor.DeviceID, phasor.Label, phasor.Type,
                        phasor.Phase, phasor.BaseKV, phasor.SourceIndex, CommonFunctions.CurrentUser, database.UtcNow, phasor.ID);
                }

                // Get reference to the device to which phasor is being added.
                Device device = Device.GetDevice(database, "WHERE ID = " + phasor.DeviceID);

                // Get Phasor signal types.
                ObservableCollection<SignalType> signals;

                if (phasor.Type == "V")
                    signals = SignalType.GetVoltagePhasorSignalTypes();
                else
                    signals = SignalType.GetCurrentPhasorSignalTypes();

                // Get reference to phasor which has just been added.
                Phasor addedPhasor = GetPhasor(database, "WHERE DeviceID = " + phasor.DeviceID + " AND SourceIndex = " + phasor.SourceIndex);

                foreach (SignalType signal in signals)
                {
                    Measurement measurement = Measurement.GetMeasurement(database, "WHERE DeviceID = " + phasor.DeviceID + " AND SignalTypeSuffix = '" + signal.Suffix + "' AND PhasorSourceIndex = " + oldSourceIndex);

                    if (measurement is null)
                    {
                        measurement = new Measurement
                        {
                            DeviceID = device.ID,
                            HistorianID = device.HistorianID,
                            PointTag = CommonPhasorServices.CreatePointTag(device.CompanyAcronym, device.Acronym, device.VendorAcronym, signal.Acronym, addedPhasor.Label, addedPhasor.SourceIndex, addedPhasor.Phase[0], addedPhasor.BaseKV),
                            SignalReference = device.Acronym + "-" + signal.Suffix + addedPhasor.SourceIndex,
                            SignalTypeID = signal.ID,
                            Description = device.Name + " " + addedPhasor.Label + " " + device.VendorDeviceName + " " + addedPhasor.Phase + " " + signal.Name,
                            PhasorSourceIndex = addedPhasor.SourceIndex,
                            Enabled = true
                        };

                        Measurement.Save(database, measurement);
                    }
                    else if (!skipMeasurementUpdate || addedPhasor.SourceIndex != oldSourceIndex) //  || measurement.SignalTypeID != signal.ID
                    {
                        // Update existing record when needed or when phasor source index has changed
                        measurement.HistorianID = device.HistorianID;
                        measurement.PointTag = CommonPhasorServices.CreatePointTag(device.CompanyAcronym, device.Acronym, device.VendorAcronym, signal.Acronym, addedPhasor.Label, addedPhasor.SourceIndex, addedPhasor.Phase[0], addedPhasor.BaseKV);
                        measurement.SignalReference = device.Acronym + "-" + signal.Suffix + addedPhasor.SourceIndex;
                        measurement.SignalTypeID = signal.ID;
                        measurement.Description = device.Name + " " + addedPhasor.Label + " " + device.VendorDeviceName + " " + addedPhasor.Phase + " " + signal.Name;
                        measurement.PhasorSourceIndex = addedPhasor.SourceIndex;

                        Measurement.Save(database, measurement);
                    }
                }

                return "Phasor information saved successfully";
            }
            finally
            {
                // ReSharper disable once ConstantConditionalAccessQualifier
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Phasor"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="phasorID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int phasorID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                Phasor phasor = GetPhasor(database, "WHERE ID = " + phasorID);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Phasor WHERE ID = {0}", "phasorID"), DefaultTimeout, phasorID);

                if (!(phasor is null))
                {
                    try
                    {
                        database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Measurement WHERE DeviceID = {0} AND PhasorSourceIndex = {1}", "deviceID", "phasorSourceIndex"), DefaultTimeout, phasor.DeviceID, phasor.SourceIndex);
                    }
                    catch (Exception ex)
                    {
                        CommonFunctions.LogException(database, "Phasor.Delete", ex);
                        throw new Exception("Phasor deleted successfully but failed to delete measurements. " + Environment.NewLine + ex.Message);
                    }
                }

                return "Phasor deleted successfully";
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Retrieves <see cref="Phasor"/> information based on query string filter.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="whereClause">query string to filter data.</param>
        /// <returns><see cref="Phasor"/> information.</returns>
        public static Phasor GetPhasor(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                DataTable phasorTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM PhasorDetail " + whereClause);

                if (phasorTable.Rows.Count == 0)
                    return null;

                DataRow row = phasorTable.Rows[0];
                Phasor phasor = new Phasor
                    {
                        ID = row.ConvertField<int>("ID"),
                        DeviceID = row.ConvertField<int>("DeviceID"),
                        Label = row.Field<string>("Label"),
                        Type = row.Field<string>("Type"),
                        Phase = row.Field<string>("Phase"),
                        BaseKV = row.ConvertField<int>("BaseKV"),
                        SourceIndex = row.ConvertField<int>("SourceIndex")
                    };

                return phasor;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        #endregion
    }
}
