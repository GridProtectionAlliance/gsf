//******************************************************************************************************
//  OutputStreamDevicePhasor.cs - Gbtc
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
//  08/15/2011 - Aniket Salver
//       Generated original version of source code.
//  09/16/2011 - Mehulbhai P Thakkar
//       Fixed issue in Load() method.
//   09/19/2011 - Mehulbhai P Thakkar
//       Added OnPropertyChanged() on all properties to reflect changes on UI.
//       Fixed Load() and GetLookupList() static methods.
//  06/27/2012- Vijay Sukhavasi
//       Modified Delete() to delete measurements associated with phasor  
//  08/15/2012 - Aniket Salver 
//       Added paging and sorting technique. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using GSF.Data;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="OutputStreamDevicePhasor"/> information as defined in the database.
    /// </summary>
    public class OutputStreamDevicePhasor : DataModelBase
    {
        #region [ Members ]

        private Guid m_nodeID;
        private int m_outputStreamDeviceID;
        private int m_id;
        private string m_label;
        private string m_type;
        private string m_phase;
        private int m_scalingValue;
        private int m_loadOrder;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor NodeID is a required field, please provide value.")]
        public Guid NodeID
        {
            get => m_nodeID;
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> OutputStreamDeviceID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor OutputStreamDeviceID is a required field, please provide value.")]
        public int OutputStreamDeviceID
        {
            get => m_outputStreamDeviceID;
            set
            {
                m_outputStreamDeviceID = value;
                OnPropertyChanged("OutputStreamDeviceID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        [Required(ErrorMessage = "OutputStreamDevicePhasor ID is a required field, please provide value.")]
        public int ID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> Label.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor Label is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "OutputStreamDevicePhasor Label cannot exceed 200 characters.")]
        public string Label
        {
            get => m_label;
            set
            {
                m_label = value;
                OnPropertyChanged("Label");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> Type.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor Type is a required field, please provide value.")]
        [DefaultValue("V")]
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
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> Phase.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor Phase is a required field, please provide value.")]
        [DefaultValue("+")]
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
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> ScalingValue .
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor ScalingValue  is a required field, please provide value.")]
        public int ScalingValue
        {
            get => m_scalingValue;
            set
            {
                m_scalingValue = value;
                OnPropertyChanged("ScalingValue");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> LoadOrder .
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDevicePhasor LoadOrder  is a required field, please provide value.")]
        public int LoadOrder
        {
            get => m_loadOrder;
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> PhasorType.
        /// </summary>        
        public string PhasorType { get; private set; }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDevicePhasor"/> PhaseType.
        /// </summary>        
        public string PhaseType { get; private set; }

        /// <summary>
        /// Gets or sets when the current <see cref="OutputStreamDevicePhasor"/> was created.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamDevicePhasor"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets when the current <see cref="OutputStreamDevicePhasor"/> was updated.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamDevicePhasor"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy { get; set; }

        #endregion

        #region [ Static ]

        // Static Methods      

        /// <summary>
        /// LoadKeys <see cref="OutputStreamDevicePhasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceID">ID of the output stream device to filter data.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="OutputStreamDevicePhasor"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, int outputStreamDeviceID, string sortMember, string sortDirection)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> outputStreamDevicePhasorList = new List<int>();

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = $"ORDER BY {sortMember} {sortDirection}";

                DataTable OutputStreamDevicePhasorTable = database.Connection.RetrieveData(database.AdapterType, $"SELECT ID FROM OutputStreamDevicePhasor WHERE OutputStreamDeviceID = {outputStreamDeviceID} {sortClause}");

                foreach (DataRow row in OutputStreamDevicePhasorTable.Rows)
                    outputStreamDevicePhasorList.Add((row.ConvertField<int>("ID")));

                return outputStreamDevicePhasorList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="OutputStreamDevicePhasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measurements to be loaded from the database</param>
        /// <returns>Collection of <see cref="OutputStreamDevicePhasor"/>.</returns>
        public static ObservableCollection<OutputStreamDevicePhasor> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                OutputStreamDevicePhasor[] outputStreamDevicePhasorList = null;

                if (keys is not null && keys.Count > 0)
                {
                    string commaSeparatedKeys = keys.Select(key => "" + key.ToString() + "").Aggregate((str1, str2) => str1 + "," + str2);
                    string query = database.ParameterizedQueryString("SELECT NodeID, OutputStreamDeviceID, ID, Label, Type, Phase, ScalingValue, LoadOrder " + $"FROM OutputStreamDevicePhasor WHERE ID IN ({commaSeparatedKeys})");

                    DataTable outputStreamDevicePhasorTable = database.Connection.RetrieveData(database.AdapterType, query);
                    outputStreamDevicePhasorList = new OutputStreamDevicePhasor[outputStreamDevicePhasorTable.Rows.Count];

                    foreach (DataRow row in outputStreamDevicePhasorTable.Rows)
                    {
                        int id = row.ConvertField<int>("ID");

                        outputStreamDevicePhasorList[keys.IndexOf(id)] = new OutputStreamDevicePhasor()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            OutputStreamDeviceID = row.ConvertField<int>("OutputStreamDeviceID"),
                            ID = id,
                            Label = row.Field<string>("Label"),
                            Type = row.Field<string>("Type"),
                            Phase = row.Field<string>("Phase"),
                            ScalingValue = row.ConvertField<int>("ScalingValue"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            PhaseType = row.Field<string>("Phase") == "+" ? "Positive Sequence" : row.Field<string>("Phase") == "-" ? "Negative Sequence" :
                                                                    row.Field<string>("Phase") == "0" ? "Zero Sequence" : row.Field<string>("Phase") == "A" ? "Phase A" :
                                                                    row.Field<string>("Phase") == "B" ? "Phase B" : "Phase C",
                            PhasorType = row.Field<string>("Type") == "V" ? "Voltage" : "Current"
                        };
                    }
                }

                return new ObservableCollection<OutputStreamDevicePhasor>(outputStreamDevicePhasorList ?? new OutputStreamDevicePhasor[0]);
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="OutputStreamDevicePhasor"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceID">ID of the output stream device to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Label of OutputStreamDevicePhasors defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int outputStreamDeviceID, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> OutputStreamDevicePhasorList = new();

                if (isOptional)
                    OutputStreamDevicePhasorList.Add(0, "Select OutputStreamDevicePhasor");

                string query = database.ParameterizedQueryString("SELECT ID, Label FROM OutputStreamDevicePhasor " +
                    "WHERE OutputStreamDeviceID = {0} ORDER BY LoadOrder", "outputStreamDeviceID");
                
                DataTable OutputStreamDevicePhasorTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, outputStreamDeviceID);

                foreach (DataRow row in OutputStreamDevicePhasorTable.Rows)
                    OutputStreamDevicePhasorList[row.ConvertField<int>("ID")] = row.Field<string>("Label");

                return OutputStreamDevicePhasorList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="OutputStreamDevicePhasor"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDevicePhasor">Information about <see cref="OutputStreamDevicePhasor"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, OutputStreamDevicePhasor outputStreamDevicePhasor)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;

                if (outputStreamDevicePhasor.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO OutputStreamDevicePhasor (NodeID, OutputStreamDeviceID, Label, Type, Phase, ScalingValue, " +
                        "LoadOrder, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})", "nodeID",
                        "outputStreamDeviceID", "label", "type", "phase", "scalingValue", "loadOrder", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.CurrentNodeID(), outputStreamDevicePhasor.OutputStreamDeviceID,
                        outputStreamDevicePhasor.Label, outputStreamDevicePhasor.Type, outputStreamDevicePhasor.Phase, outputStreamDevicePhasor.ScalingValue,
                        outputStreamDevicePhasor.LoadOrder, CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);

                    //    PhasorType, PhaseType @phasorName, @phaseType, OutputStreamDevicePhasor.PhasorType, OutputStreamDevicePhasor.PhaseType
                }
                else
                {
                    query = "UPDATE OutputStreamDevicePhasor SET NodeID = {0}, OutputStreamDeviceID = {1}, Label = {2}, Type = {3}, " +
                        "Phase = {4}, ScalingValue = {5}, LoadOrder = {6}, UpdatedBy = {7}, UpdatedOn = {8} WHERE ID = {9}";

                    database.ExecuteNonQuery(DefaultTimeout, query, outputStreamDevicePhasor.NodeID, outputStreamDevicePhasor.OutputStreamDeviceID,
                        outputStreamDevicePhasor.Label, outputStreamDevicePhasor.Type, outputStreamDevicePhasor.Phase, outputStreamDevicePhasor.ScalingValue,
                        outputStreamDevicePhasor.LoadOrder, CommonFunctions.CurrentUser, database.UtcNow, outputStreamDevicePhasor.ID);

                    //PhasorType = @typeName, PhaseType = @PhaseType" OutputStreamDevicePhasor.PhasorType, OutputStreamDevicePhasor.PhaseType,
                }

                return "OutputStreamDevicePhasor information saved successfully";
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="OutputStreamDevicePhasor"/> record and its associated measurements from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDevicePhasorID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int outputStreamDevicePhasorID)
        {
            bool createdConnection = false;
            string nextAngleSignalReference = string.Empty;
            string nextMagnitudeSignalReference = string.Empty;
            string lastAffectedMeasurementsMessage = string.Empty;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                GetDeleteMeasurementDetails(database, outputStreamDevicePhasorID, out string angleSignalReference, out string magnitudeSignalReference, out int adapterID, out int outputStreamDeviceID);

                // Delete angle/magnitude of measurements if they exist
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamMeasurement WHERE SignalReference = {0} AND AdapterID = {1}", "signalReference", "adapterID"), DefaultTimeout, angleSignalReference, adapterID);
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamMeasurement WHERE SignalReference = {0} AND AdapterID = {1}", "signalReference", "adapterID"), DefaultTimeout, magnitudeSignalReference, adapterID);
                int presentDevicePhasorCount = Convert.ToInt32(database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT COUNT(*) FROM OutputStreamDevicePhasor WHERE OutputStreamDeviceID = {0}", "outputStreamDeviceID"), DefaultTimeout, outputStreamDeviceID));

                // Using signal reference angle/mag deleted build the next signal reference (increment by 1)
                int.TryParse(Regex.Match(magnitudeSignalReference, @"\d+$").Value, out int deletedSignalReferenceIndex);
                string angleSignalReferenceBase = Regex.Replace(angleSignalReference, @"\d+$", "");
                string magnitudeSignalReferenceBase = Regex.Replace(magnitudeSignalReference, @"\d+$", "");

                for (int i = deletedSignalReferenceIndex; i < presentDevicePhasorCount; i++)
                {
                    // We will be modifying the measurements with signal reference index i+1 to have signal reference index i.
                    string previousAngleSignalReference = $"{angleSignalReferenceBase}{i}";
                    nextAngleSignalReference = $"{angleSignalReferenceBase}{i + 1}";

                    string previousMagnitudeSignalReference = $"{magnitudeSignalReferenceBase}{i}";
                    nextMagnitudeSignalReference = $"{magnitudeSignalReferenceBase}{i + 1}";

                    // For angle...
                    // Obtain details of measurements after the deleted measurements, then modify the signal reference (decrement by 1) and put it back
                    OutputStreamMeasurement outputStreamMeasurement = GetOutputMeasurementDetails(database, nextAngleSignalReference, adapterID);
                    outputStreamMeasurement.SignalReference = previousAngleSignalReference;
                    OutputStreamMeasurement.Save(database, outputStreamMeasurement);

                    // For magnitude...
                    outputStreamMeasurement = GetOutputMeasurementDetails(database, nextMagnitudeSignalReference, adapterID);
                    outputStreamMeasurement.SignalReference = previousMagnitudeSignalReference;
                    OutputStreamMeasurement.Save(database, outputStreamMeasurement);
                }

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDevicePhasor WHERE ID = {0}", "outputStreamDevicePhasorID"), DefaultTimeout, outputStreamDevicePhasorID);

                return "OutputStreamDevicePhasor deleted successfully";
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(nextMagnitudeSignalReference))
                    lastAffectedMeasurementsMessage = $"{Environment.NewLine}(Last affected measurements: {nextMagnitudeSignalReference} {nextAngleSignalReference})";

                CommonFunctions.LogException(database, "OutputStreamDevicePhasor.Delete", ex);
                MessageBoxResult dialogResult = MessageBox.Show($"Could not delete or modify measurements.{Environment.NewLine}Do you still wish to delete this Phasor?{lastAffectedMeasurementsMessage}", "", MessageBoxButton.YesNo);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDevicePhasor WHERE ID = {0}", "outputStreamDevicePhasorID"), DefaultTimeout, outputStreamDevicePhasorID);
                    return "OutputStreamDevicePhasor deleted successfully but failed to modify all measurements ";
                }
                else
                {
                    Exception exception = ex.InnerException ?? ex;
                    return $"Delete OutputStreamDevicePhasor was unsuccessful: {exception.Message}";
                }
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        private static OutputStreamMeasurement GetOutputMeasurementDetails(AdoDataConnection database, string signalReference, int adapterID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query = database.ParameterizedQueryString("SELECT * FROM OutputStreamMeasurement WHERE SignalReference = {0} AND AdapterID = {1}", "signalReference", "adapterID");
                DataRow row = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, signalReference, adapterID).Rows[0];

                OutputStreamMeasurement outputStreamMeasurement = new()
                {
                    NodeID = row.ConvertField<Guid>("NodeID"),
                    AdapterID = row.Field<int>("AdapterID"),
                    ID = row.Field<int>("ID"),
                    HistorianID = row.Field<int>("HistorianID"),
                    PointID = row.Field<int>("PointID"),
                    SignalReference = row.ConvertField<string>("SignalReference"),
                    CreatedOn = row.ConvertField<DateTime>("CreatedOn"),
                    CreatedBy = row.Field<string>("CreatedBy"),
                    UpdatedOn = row.ConvertField<DateTime>("UpdatedOn"),
                    UpdatedBy = row.Field<string>("UpdatedBy")
                };

                return outputStreamMeasurement;
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(database, "OutputStreamDevicePhasor.GetOutputMeasurementDetails", ex);
                throw;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        private static void GetDeleteMeasurementDetails(AdoDataConnection database, int outputStreamDevicePhasorID, out string angleSignalReference, out string magnitudeSignalReference, out int adapterID, out int outputStreamDeviceID)
        {
            const string outputPhasorFormat = "SELECT Label, OutputStreamDeviceID FROM OutputStreamDevicePhasor WHERE ID = {0}";
            const string outputDeviceFormat = "SELECT Acronym, AdapterID FROM OutputStreamDevice WHERE ID = {0}";
            const string measurementDetailFormat = "SELECT PointTag FROM MeasurementDetail WHERE DeviceAcronym = '{0}' AND PhasorLabel = '{1}' AND SignalTypeSuffix = '{2}'";
            const string outputMeasurementDetailFormat = "SELECT SignalReference FROM OutputStreamMeasurementDetail WHERE SourcePointTag = '{0}'";

            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                DataRow outputPhasorRecord = database.Connection.RetrieveData(database.AdapterType, string.Format(outputPhasorFormat, outputStreamDevicePhasorID)).Rows[0];
                string labelName = outputPhasorRecord.Field<string>("Label");
                outputStreamDeviceID = outputPhasorRecord.ConvertField<int>("OutputStreamDeviceID");

                DataRow outputDeviceRecord = database.Connection.RetrieveData(database.AdapterType, string.Format(outputDeviceFormat, outputStreamDeviceID)).Rows[0];
                string deviceName = outputDeviceRecord.Field<string>("Acronym");
                adapterID = outputDeviceRecord.ConvertField<int>("AdapterID");

                string anglePointTag = database.Connection.ExecuteScalar(string.Format(measurementDetailFormat, deviceName, labelName, "PA")).ToNonNullString();
                angleSignalReference = database.Connection.ExecuteScalar(string.Format(outputMeasurementDetailFormat, anglePointTag)).ToNonNullString();

                string magnitudePointTag = database.Connection.ExecuteScalar(string.Format(measurementDetailFormat, deviceName, labelName, "PM")).ToNonNullString();
                magnitudeSignalReference = database.Connection.ExecuteScalar(string.Format(outputMeasurementDetailFormat, magnitudePointTag)).ToNonNullString();
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(database, "OutputStreamDevicePhasor.GetDeleteMeasurementDetails", ex);
                throw;
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
