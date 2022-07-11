//******************************************************************************************************
//  OutputStreamDeviceDigital.cs - Gbtc
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
//  08/10/2011 - Aniket Salver
//       Generated original version of source code.
//  09/19/2011 - Mehulbhai P Thakkar
//       Added OnPropertyChanged() on all properties to reflect changes on UI.
//       Fixed Load() and GetLookupList() static methods.
//  06/27/2012- Vijay Sukhavasi
//        Modified Delete() to delete measurements associated with digital 
//  08/15/2012 - Aniket Salver 
//          Added paging and sorting technique. 
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
    /// Represents a record of <see cref="OutputStreamDeviceDigital"/> information as defined in the database.
    /// </summary>
    public class OutputStreamDeviceDigital : DataModelBase
    {
        #region [ Members ]

        private Guid m_nodeID;
        private int m_outputStreamDeviceID;
        private int m_id;
        private string m_label;
        private int m_maskValue;
        private int m_loadOrder;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceDigital NodeID is a required field, please provide value.")]
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
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> OutputStreamDeviceID.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceDigital OutputStreamDeviceID is a required field, please provide value.")]
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
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        [Required(ErrorMessage = "OutputStreamDeviceDigital ID is a required field, please provide value.")]
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
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> Label.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceDigital Label is a required field, please provide value.")]
        public string Label
        {
            get => m_label;
            set
            {
                m_label = value.TruncateRight(256).PadRight(256);
                OnPropertyChanged("Label");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> Display Label.
        /// </summary>                
        public string DisplayLabel
        {
            get => string.Concat(Label.GetSegments(16).Select(label => $"{label.Trim()}\r\n").Take(16));
            set
            {
                Label = string.Concat(value.Split(new[] { "\r\n" }, StringSplitOptions.None).Select(label => label.TruncateRight(16).PadRight(16)).Take(16));
                OnPropertyChanged("DisplayLabel");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> MaskValue.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceDigital MaskValue is a required field, please provide value.")]
        public int MaskValue
        {
            get => m_maskValue;
            set
            {
                m_maskValue = value;
                OnPropertyChanged("MaskValue");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStreamDeviceDigital"/> LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "OutputStreamDeviceDigital LoadOrder is a required field, please provide value.")]
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

        /// <summary>
        /// Gets or sets when the current <see cref="OutputStreamDeviceDigital"/> was created.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get => m_createdOn;
            set => m_createdOn = value;
        }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamDeviceDigital"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get => m_createdBy;
            set => m_createdBy = value;
        }

        /// <summary>
        /// Gets or sets when the current <see cref="OutputStreamDeviceDigital"/> was updated.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get => m_updatedOn;
            set => m_updatedOn = value;
        }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamDeviceDigital"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get => m_updatedBy;
            set => m_updatedBy = value;
        }

        #endregion

        #region [ Static ]

        // Static Methods      

        /// <summary>
        /// LoadKeys <see cref="OutputStreamDeviceDigital"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceID">ID of the output stream device to filter data.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="OutputStreamDeviceDigital"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, int outputStreamDeviceID, string sortMember, string sortDirection)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> outputStreamDeviceDigitalList = new List<int>();

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = $"ORDER BY {sortMember} {sortDirection}";

                DataTable outputStreamDeviceDigitalTable = database.Connection.RetrieveData(database.AdapterType, $"SELECT ID From OutputStreamDeviceDigital WHERE OutputStreamDeviceID = {outputStreamDeviceID} {sortClause}");

                foreach (DataRow row in outputStreamDeviceDigitalTable.Rows)
                {
                    outputStreamDeviceDigitalList.Add(row.ConvertField<int>("ID"));
                }

                return outputStreamDeviceDigitalList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="OutputStreamDeviceDigital"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measurement to be loaded from  the database</param>
        /// <returns>Collection of <see cref="OutputStreamDeviceDigital"/>.</returns>
        public static ObservableCollection<OutputStreamDeviceDigital> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                OutputStreamDeviceDigital[] outputStreamDeviceDigitalList = null;
                DataTable outputStreamDeviceDigitalTable;
                int id;

                if (keys is not null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => $"{key}").Aggregate((str1, str2) => $"{str1},{str2}");
                    query = database.ParameterizedQueryString($"SELECT NodeID, OutputStreamDeviceID, ID, Label, MaskValue, LoadOrder FROM OutputStreamDeviceDigital WHERE ID IN ({commaSeparatedKeys})");

                    outputStreamDeviceDigitalTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
                    outputStreamDeviceDigitalList = new OutputStreamDeviceDigital[outputStreamDeviceDigitalTable.Rows.Count];

                    foreach (DataRow row in outputStreamDeviceDigitalTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        outputStreamDeviceDigitalList[keys.IndexOf(id)] = new OutputStreamDeviceDigital()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            OutputStreamDeviceID = row.ConvertField<int>("OutputStreamDeviceID"),
                            ID = id,
                            Label = row.Field<string>("Label"),
                            MaskValue = row.ConvertField<int>("MaskValue"),
                            LoadOrder = row.ConvertField<int>("LoadOrder")
                        };
                    }
                }

                return new ObservableCollection<OutputStreamDeviceDigital>(outputStreamDeviceDigitalList ?? Array.Empty<OutputStreamDeviceDigital>());
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="OutputStreamDeviceDigital"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceID">ID of the output stream device to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Label of OutputStreamDeviceDigitals defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int outputStreamDeviceID, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> OutputStreamDeviceDigitalList = new();

                if (isOptional)
                    OutputStreamDeviceDigitalList.Add(0, "Select OutputStreamDeviceDigital");

                string query = database.ParameterizedQueryString("SELECT ID, Label FROM OutputStreamDeviceDigital " +
                    "WHERE OutputStreamDeviceID = {0} ORDER BY LoadOrder", "outputStreamDeviceID");
                
                DataTable OutputStreamDeviceDigitalTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, outputStreamDeviceID);

                foreach (DataRow row in OutputStreamDeviceDigitalTable.Rows)
                    OutputStreamDeviceDigitalList[row.ConvertField<int>("ID")] = row.Field<string>("Label");

                return OutputStreamDeviceDigitalList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="OutputStreamDeviceDigital"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceDigital">Information about <see cref="OutputStreamDeviceDigital"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, OutputStreamDeviceDigital outputStreamDeviceDigital)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (outputStreamDeviceDigital.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO OutputStreamDeviceDigital (NodeID, OutputStreamDeviceID, Label, MaskValue, LoadOrder, " +
                        "UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", "nodeID", "outputStreamDeviceID", "label",
                        "maskValue", "loadOrder", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.CurrentNodeID(), outputStreamDeviceDigital.OutputStreamDeviceID,
                        outputStreamDeviceDigital.Label, outputStreamDeviceDigital.MaskValue, outputStreamDeviceDigital.LoadOrder, CommonFunctions.CurrentUser,
                        database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE OutputStreamDeviceDigital SET NodeID = {0}, OutputStreamDeviceID = {1}, Label = {2}, MaskValue = {3}, " +
                        "LoadOrder = {4}, UpdatedBy = {5}, UpdatedOn = {6} WHERE ID = {7}", "nodeID", "outputStreamDeviceID", "label", "maskValue", "loadOrder", "updatedBy",
                        "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, outputStreamDeviceDigital.NodeID, outputStreamDeviceDigital.OutputStreamDeviceID,
                        outputStreamDeviceDigital.Label, outputStreamDeviceDigital.MaskValue, outputStreamDeviceDigital.LoadOrder, CommonFunctions.CurrentUser,
                        database.UtcNow, outputStreamDeviceDigital.ID);
                }

                return "OutputStreamDeviceDigital information saved successfully";
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="OutputStreamDeviceDigital"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamDeviceDigitalID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int outputStreamDeviceDigitalID)
        {
            bool createdConnection = false;

            string nextDigitalSignalReference = string.Empty;
            string lastAffectedMeasurementMessage = string.Empty;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                GetDeleteMeasurementDetails(database, outputStreamDeviceDigitalID, out string digitalSignalReference, out int adapterID, out int outputStreamDeviceID);
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamMeasurement WHERE SignalReference = {0} AND AdapterID = {1}", "signalReference", "adapterID"), DefaultTimeout, digitalSignalReference, adapterID);
                int presentDeviceDigitalCount = Convert.ToInt32(database.Connection.ExecuteScalar(database.ParameterizedQueryString("SELECT COUNT(*) FROM OutputStreamDeviceDigital WHERE OutputStreamDeviceID = {0}", "outputStreamDeviceID"), DefaultTimeout, outputStreamDeviceID));

                // Using signal reference of measurement deleted build the next signal reference (increment by 1)\
                int.TryParse(Regex.Match(digitalSignalReference, @"\d+$").Value, out int deletedSignalReferenceIndex);
                string signalReferenceBase = Regex.Replace(digitalSignalReference, @"\d+$", "");

                for (int i = deletedSignalReferenceIndex; i < presentDeviceDigitalCount; i++)
                {
                    // We will be modifying the measurement with signal reference index i+1 to have signal reference index i.
                    string previousDigitalSignalReference = $"{signalReferenceBase}{i}";
                    nextDigitalSignalReference = $"{signalReferenceBase}{i + 1}";

                    // Obtain details of measurements of the deleted measurements, then modify the signal reference (decrement by 1) and put it back
                    OutputStreamMeasurement outputStreamMeasurement = GetOutputMeasurementDetails(database, nextDigitalSignalReference, adapterID);
                    outputStreamMeasurement.SignalReference = previousDigitalSignalReference;
                    OutputStreamMeasurement.Save(database, outputStreamMeasurement);
                }

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDeviceDigital WHERE ID = {0}", "outputStreamDeviceDigitalID"), DefaultTimeout, outputStreamDeviceDigitalID);
                
                return "OutputStreamDeviceDigital and its Measurements deleted successfully";
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(nextDigitalSignalReference))
                    lastAffectedMeasurementMessage = $"{Environment.NewLine}(Last affected measurement: {nextDigitalSignalReference})";

                CommonFunctions.LogException(database, "OutputStreamDeviceDigital.Delete", ex);
                MessageBoxResult dialogResult = MessageBox.Show($"Could not delete or modify measurements.{Environment.NewLine}Do you still wish to delete this Digital?{lastAffectedMeasurementMessage}", "", MessageBoxButton.YesNo);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamDeviceDigital WHERE ID = {0}", "outputStreamDeviceDigitalID"), DefaultTimeout, outputStreamDeviceDigitalID);
                    return "OutputStreamDeviceDigital deleted successfully but failed to modify all measurements ";

                }
                else
                {
                    Exception exception = ex.InnerException ?? ex;
                    return $"Delete OutputStreamDeviceDigital was unsuccessful: {exception.Message}";
                }
            }
            finally
            {
                if (createdConnection && database is not null)
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
                CommonFunctions.LogException(database, "OutputStreamDeviceDigital.GetOutputMeasurementDetails", ex);
                throw;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }

        }

        private static void GetDeleteMeasurementDetails(AdoDataConnection database, int outputStreamDeviceDigitalID, out string digitalSignalReference, out int adapterID, out int outputStreamDeviceID)
        {
            const string outputDigitalFormat = "SELECT Label, OutputStreamDeviceID FROM OutputStreamDeviceDigital ID = {0}";
            const string outputDeviceFormat = "SELECT Acronym, AdapterID FROM OutputStreamDevice WHERE ID = {0}";
            const string measurementDetailFormat = "SELECT PointTag FROM MeasurementDetail WHERE DeviceAcronym = '{0}' AND AlternateTag = '{1}' AND SignalTypeSuffix = 'DV'";
            const string outputMeasurementDetailFormat = "SELECT SignalReference FROM OutputStreamMeasurementDetail WHERE SourcePointTag = '{0}'";

            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                DataRow outputDigitalRecord = database.Connection.RetrieveData(database.AdapterType, string.Format(outputDigitalFormat, outputStreamDeviceDigitalID)).Rows[0];
                string labelName = outputDigitalRecord.Field<string>("Label");
                outputStreamDeviceID = outputDigitalRecord.Field<int>("OutputStreamDeviceID");

                DataRow outputDeviceRecord = database.Connection.RetrieveData(database.AdapterType, string.Format(outputDeviceFormat, outputStreamDeviceID)).Rows[0];
                string deviceName = outputDeviceRecord.Field<string>("Acronym");
                adapterID = outputDeviceRecord.ConvertField<int>("AdapterID");

                string digitalPointTag = database.Connection.ExecuteScalar(string.Format(measurementDetailFormat, deviceName, labelName)).ToNonNullString();
                digitalSignalReference = database.Connection.ExecuteScalar(string.Format(outputMeasurementDetailFormat, digitalPointTag)).ToNonNullString();
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(database, "OutputStreamDeviceDigital.GetDeleteMeasurementDetails", ex);
                throw;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        #endregion
    }
}
