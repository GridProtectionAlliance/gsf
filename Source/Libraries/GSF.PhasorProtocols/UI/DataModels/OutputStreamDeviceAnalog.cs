//******************************************************************************************************
//  OutputStreamDeviceAnalog.cs - Gbtc
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
//  08/5/2011 - Aniket Salver
//       Generated original version of source code.
//  09/16/2011 - Mehulbhai P Thakkar
//       Fixed load method to filter data correctly.
//  09/19/2011 - Mehulbhai P Thakkar
//       Added OnPropertyChanged() on all properties to reflect changes on UI.
//       Fixed database queries and collection population.
//       Fixed Load() and GetLookupList() static methods.
//  06/27/2012- Vijay Sukhavasi
//       Modified Delete() to delete measurements associated with analog
//  08/14/2012 - Aniket Salver 
//       Added paging and sorting technique.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Represents a record of <see cref="OutputStreamDeviceAnalog"/> information as defined in the database.
    /// </summary>
    public class OutputStreamDeviceAnalog : DataModelBase
    {
        #region [ Members ]

        private Guid m_nodeID;
        private int m_outputStreamDeviceID;
        private int m_id;
        private string m_label;
        private int m_type;
        private int m_scalingValue;
        private int m_loadOrder;
        private string m_typeName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s NodeID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceAnalog NodeID is a required field, please provide value.")]
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
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s OutputStreamDeviceID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceAnalog OutputStreamDeviceID is a required field, please provide value.")]
        public int OutputStreamDeviceID
        {
            get
            {
                return m_outputStreamDeviceID;
            }
            set
            {
                m_outputStreamDeviceID = value;
                OnPropertyChanged("OutputStreamDeviceID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s ID.
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
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s Label.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceAnalog Lable is a required field, please provide value.")]
        [StringLength(16, ErrorMessage = "OutputStreamDeviceAnalog Label cannot exceed 16 characters.")]
        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                if (value.Length > 16)
                    m_label = value.Substring(0, 16);
                else
                    m_label = value;
                OnPropertyChanged("Label");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s Type.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceAnalog Type is a required field, please provide value.")]
        public int Type
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
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s ScalingValue.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceAnalog ScalingValue is a required field, please provide value.")]
        public int ScalingValue
        {
            get
            {
                return m_scalingValue;
            }
            set
            {
                m_scalingValue = value;
                OnPropertyChanged("ScalingValue");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamDeviceAnalog"/>'s LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceAnalog LoadOrder is a required field, please provide value.")]
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
        /// Gets the current <see cref="OutputStreamDeviceAnalog"/>'s TypeName.
        /// </summary>
        public string TypeName
        {
            get
            {
                return m_typeName;
            }
        }

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="OutputStreamDeviceAnalog"/> was created on.
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
        /// Gets or sets who the current <see cref="OutputStreamDeviceAnalog"/> was created by.
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
        /// Gets or sets the Date or Time when the current <see cref="OutputStreamDeviceAnalog"/> was updated on.
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
        /// Gets or sets who the current <see cref="OutputStreamDeviceAnalog"/> was updated by.
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

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// LoadKeys <see cref="OutputStreamDeviceAnalog"/> information as an <see cref="ObservableCollection{T}"/> style list.
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

                IList<int> outputStreamDeviceAnalogList = new List<int>();
                DataTable outputStreamDeviceAnalogTable;

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                outputStreamDeviceAnalogTable = database.Connection.RetrieveData(database.AdapterType, string.Format("SELECT ID FROM OutputStreamDeviceAnalog WHERE OutputStreamDeviceID = {0} {1}", outputStreamDeviceID, sortClause));

                foreach (DataRow row in outputStreamDeviceAnalogTable.Rows)
                {
                    outputStreamDeviceAnalogList.Add((row.ConvertField<int>("ID")));
                }

                return outputStreamDeviceAnalogList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="OutputStreamDeviceAnalog"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measuremnets to be loaded from the database</param>
        /// <returns>Collection of <see cref="OutputStreamDeviceAnalog"/>.</returns>
        public static ObservableCollection<OutputStreamDeviceAnalog> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                OutputStreamDeviceAnalog[] outputStreamDeviceAnalogList = null;
                DataTable outputStreamDeviceAnalogTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "" + key.ToString() + "").Aggregate((str1, str2) => str1 + "," + str2);
                    query = database.ParameterizedQueryString(string.Format("SELECT NodeID, OutputStreamDeviceID, ID, Label, Type, ScalingValue, LoadOrder " +
                                        "FROM OutputStreamDeviceAnalog WHERE ID IN ({0})", commaSeparatedKeys));
                    outputStreamDeviceAnalogTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
                    outputStreamDeviceAnalogList = new OutputStreamDeviceAnalog[outputStreamDeviceAnalogTable.Rows.Count];

                    foreach (DataRow row in outputStreamDeviceAnalogTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        outputStreamDeviceAnalogList[keys.IndexOf(id)] = new OutputStreamDeviceAnalog()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            OutputStreamDeviceID = row.ConvertField<int>("OutputStreamDeviceID"),
                            ID = id,
                            Label = row.Field<string>("Label"),
                            Type = row.ConvertField<int>("Type"),
                            ScalingValue = row.ConvertField<int>("ScalingValue"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            m_typeName = row.ConvertField<int>("Type") == 0 ? "Single point-on-wave" : row.ConvertField<int>("Type") == 1 ? "RMS of analog input" : "Peak of analog input"
                        };
                    }
                }

                return new ObservableCollection<OutputStreamDeviceAnalog>(outputStreamDeviceAnalogList ?? new OutputStreamDeviceAnalog[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="OutputStreamDeviceAnalog"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceID">ID of the output stream device to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of OutputStreamDeviceAnalog defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int outputStreamDeviceID, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> OutputStreamDeviceAnalogList = new Dictionary<int, string>();
                if (isOptional)
                    OutputStreamDeviceAnalogList.Add(0, "Select OutputStreamDeviceAnalog");

                string query = database.ParameterizedQueryString("SELECT ID, Label FROM OutputStreamDeviceAnalog WHERE OutputStreamDeviceID = {0} ORDER BY LoadOrder", "outputStreamDeviceID");
                DataTable OutputStreamDeviceAnalogTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, outputStreamDeviceID);

                foreach (DataRow row in OutputStreamDeviceAnalogTable.Rows)
                    OutputStreamDeviceAnalogList[row.ConvertField<int>("ID")] = row.Field<string>("Label");

                return OutputStreamDeviceAnalogList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="OutputStreamDeviceAnalog"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceAnalog">Information about <see cref="OutputStreamDeviceAnalog"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, OutputStreamDeviceAnalog outputStreamDeviceAnalog)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (outputStreamDeviceAnalog.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO OutputStreamDeviceAnalog (NodeID, OutputStreamDeviceID, Label, Type, ScalingValue, LoadOrder, " +
                        "UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})", "nodeID",
                        "outputStreamDeviceID", "label", "type", "scalingValue", "loadOrder", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    // TypeName, "typeName",

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.CurrentNodeID(), outputStreamDeviceAnalog.OutputStreamDeviceID,
                        outputStreamDeviceAnalog.Label, outputStreamDeviceAnalog.Type, outputStreamDeviceAnalog.ScalingValue, outputStreamDeviceAnalog.LoadOrder,
                        CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);

                    //outputStreamDeviceAnalog.TypeName,
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE OutputStreamDeviceAnalog SET NodeID = {0}, OutputStreamDeviceID = {1}, Label = {2}, Type = {3}, " +
                        "ScalingValue = {4}, LoadOrder = {5}, UpdatedBy = {6}, UpdatedOn = {7} WHERE ID = {8}", "nodeID", "outputStreamDeviceID",
                        "label", "type", "scalingValue", "loadOrder", "updatedBy", "updatedOn", "id");

                    //   TypeName= {6},  "typeName",

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, outputStreamDeviceAnalog.NodeID, outputStreamDeviceAnalog.OutputStreamDeviceID,
                        outputStreamDeviceAnalog.Label, outputStreamDeviceAnalog.Type, outputStreamDeviceAnalog.ScalingValue, outputStreamDeviceAnalog.LoadOrder,
                        CommonFunctions.CurrentUser, database.UtcNow, outputStreamDeviceAnalog.ID);
                }

                //  OutputStreamDeviceAnalog.TypeName,

                return "OutputStreamDeviceAnalog information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="OutputStreamDeviceAnalog"/> record and its associated measurements from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceAnalogID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int outputStreamDeviceAnalogID)
        {
            bool createdConnection = false;

            int adapterID;
            int outputStreamDeviceID;
            int deletedSignalReferenceIndex;
            int presentDeviceAnalogCount;

            string analogSignalReference;
            string signalReferenceBase;
            string previousAnalogSignalReference;
            string nextAnalogSignalReference = string.Empty;
            string lastAffectedMeasurementMessage = string.Empty;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                GetDeleteMeasurementDetails(database, outputStreamDeviceAnalogID, out analogSignalReference, out adapterID, out outputStreamDeviceID);
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamMeasurement WHERE SignalReference = {0} AND AdapterID = {1}", "signalReference", "adapterID"), DefaultTimeout, analogSignalReference, adapterID);
                presentDeviceAnalogCount = Convert.ToInt32(database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT COUNT(*) FROM OutputStreamDeviceAnalog WHERE OutputStreamDeviceID = {0}", "outputStreamDeviceID"), DefaultTimeout, outputStreamDeviceID));

                // Using signal reference of measurement deleted build the next signal reference (increment by 1)
                int.TryParse(Regex.Match(analogSignalReference, @"\d+$").Value, out deletedSignalReferenceIndex);
                signalReferenceBase = Regex.Replace(analogSignalReference, @"\d+$", "");

                for (int i = deletedSignalReferenceIndex; i <= presentDeviceAnalogCount; i++)
                {
                    // We will be modifying the measurement with signal reference index i+1 to have signal reference index i.
                    previousAnalogSignalReference = string.Format("{0}{1}", signalReferenceBase, i);
                    nextAnalogSignalReference = string.Format("{0}{1}", signalReferenceBase, i + 1);

                    // Obtain details of measurements of the deleted measurements, then modify the signal reference (decrement by 1) and put it back
                    OutputStreamMeasurement outputStreamMeasurement = GetOutputMeasurementDetails(database, nextAnalogSignalReference, adapterID);
                    outputStreamMeasurement.SignalReference = previousAnalogSignalReference;
                    OutputStreamMeasurement.Save(database, outputStreamMeasurement);
                }

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDeviceAnalog WHERE ID = {0}", "outputStreamDeviceAnalogID"), DefaultTimeout, outputStreamDeviceAnalogID);

                return "OutputStreamDeviceAnalog deleted successfully";
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(nextAnalogSignalReference))
                    lastAffectedMeasurementMessage = string.Format("{0}(Last affected measurement: {1})", Environment.NewLine, nextAnalogSignalReference);

                CommonFunctions.LogException(database, "OutputStreamDeviceAnalog.Delete", ex);
                MessageBoxResult dialogResult = MessageBox.Show(string.Format("Could not delete or modify measurements.{0}Do you still wish to delete this Analog?{1}", Environment.NewLine, lastAffectedMeasurementMessage), "", MessageBoxButton.YesNo);

                if ((dialogResult == MessageBoxResult.Yes))
                {
                    database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDeviceAnalog WHERE ID = {0}", "outputStreamDeviceAnalogID"), DefaultTimeout, outputStreamDeviceAnalogID);
                    return "OutputStreamDeviceAnalog deleted successfully but failed to delete all measurements";
                }
                else
                {
                    Exception exception = ex.InnerException ?? ex;
                    return string.Format("Delete OutputStreamDeviceAnalog was unsuccessful: {0}", exception.Message);
                }
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
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

                OutputStreamMeasurement outputStreamMeasurement = new OutputStreamMeasurement
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
                CommonFunctions.LogException(database, "OutputStreamDeviceAnalog.GetOutputMeasurementDetails", ex);
                throw;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        private static void GetDeleteMeasurementDetails(AdoDataConnection database, int outputStreamDeviceAnalogID, out string analogSignalReference, out int adapterID, out int outputStreamDeviceID)
        {
            const string outputAnalogFormat = "SELECT Label, OutputStreamDeviceID FROM OutputStreamDeviceAnalog WHERE ID = {0}";
            const string outputDeviceFormat = "SELECT Acronym, AdapterID FROM OutputStreamDevice WHERE ID = {0}";
            const string measurementDetailFormat = "SELECT PointTag FROM MeasurementDetail WHERE DeviceAcronym = '{0}' AND AlternateTag = '{1}' AND SignalTypeSuffix = 'AV'";
            const string outputMeasurementDetailFormat = "SELECT SignalReference FROM OutputStreamMeasurementDetail WHERE SourcePointTag = '{0}'";

            bool createdConnection = false;

            try
            {
                DataRow outputAnalogRecord;
                DataRow outputDeviceRecord;

                string labelName;
                string deviceName;
                string analogPointTag;

                createdConnection = CreateConnection(ref database);

                outputAnalogRecord = database.Connection.RetrieveData(database.AdapterType, string.Format(outputAnalogFormat, outputStreamDeviceAnalogID)).Rows[0];
                labelName = outputAnalogRecord.Field<string>("Label");
                outputStreamDeviceID = outputAnalogRecord.ConvertField<int>("OutputStreamDeviceID");

                outputDeviceRecord = database.Connection.RetrieveData(database.AdapterType, string.Format(outputDeviceFormat, outputStreamDeviceID)).Rows[0];
                deviceName = outputDeviceRecord.Field<string>("Acronym");
                adapterID = outputDeviceRecord.ConvertField<int>("AdapterID");

                analogPointTag = database.Connection.ExecuteScalar(string.Format(measurementDetailFormat, deviceName, labelName)).ToNonNullString();
                analogSignalReference = database.Connection.ExecuteScalar(string.Format(outputMeasurementDetailFormat, analogPointTag)).ToNonNullString();
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(database, "OutputStreamDeviceAnalog.GetDeleteMeasurementDetails", ex);
                throw;
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
