//******************************************************************************************************
//  Alarms.cs - Gbtc
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
//  02/10/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using GSF.TimeSeries.UI.DataModels;

namespace GSF.TimeSeries.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="DataModels.Alarm"/> collection and current selection information for UI.
    /// </summary>
    internal class Alarms : PagedViewModelBase<DataModels.Alarm, int>
    {
        #region [ Members ]

        // Fields
        private Dictionary<Guid, string> m_nodeLookupList;
        private Dictionary<int, string> m_operationList;
        private Dictionary<int, string> m_severityList;
        private Dictionary<Guid, string> m_measurementLabels;
        private string m_selectedMeasurementLabel;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Alarms"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public Alarms(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        /// <summary>
        /// Gets the list of nodes.
        /// </summary>
        public Dictionary<Guid, string> NodeLookupList
        {
            get
            {
                return m_nodeLookupList;
            }
        }

        /// <summary>
        /// Gets a list of operations that can be performed to trigger an alarm.
        /// </summary>
        public Dictionary<int, string> OperationList
        {
            get
            {
                return m_operationList;
            }
        }

        /// <summary>
        /// Gets a list of levels of severity that can be assigned to alarms.
        /// </summary>
        public Dictionary<int, string> SeverityList
        {
            get
            {
                return m_severityList;
            }
        }

        /// <summary>
        /// Gets a list of the measurement labels for the alarms on the current page.
        /// </summary>
        public Dictionary<Guid, string> MeasurementLabels
        {
            get
            {
                return m_measurementLabels;
            }
        }

        /// <summary>
        /// Gets or sets the label of the measurement selected by the user.
        /// </summary>
        public string SelectedMeasurementLabel
        {
            get
            {
                return m_selectedMeasurementLabel;
            }
            set
            {
                m_selectedMeasurementLabel = value;
                OnPropertyChanged("SelectedMeasurementLabel");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initialization to be done before the initial call to <see cref="Load"/>.
        /// </summary>
        public override void Initialize()
        {
            m_measurementLabels = new Dictionary<Guid, string>();

            base.Initialize();

            m_nodeLookupList = Node.GetLookupList(null);
            m_operationList = CreateOperationList();
            m_severityList = CreateSeverityList();
        }

        /// <summary>
        /// Loads collection of <see cref="DataModels.Alarm"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            try
            {
                base.Load();

                foreach (DataModels.Alarm alarm in ItemsSource)
                {
                    alarm.OperationDescription = GetOperationDescription(alarm);
                    alarm.SeverityName = SeverityList[alarm.Severity];
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex);
                }
            }
        }

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.TagName;
        }

        /// <summary>
        /// Clears the record for the associated <see cref="IDataModel"/>.
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            SelectedMeasurementLabel = string.Empty;

            if (m_operationList.Count > 0)
                CurrentItem.Operation = m_operationList.First().Key;

            if (m_severityList.Count > 0)
                CurrentItem.Severity = m_severityList.First().Key;
        }

        /// <summary>
        /// Handles PropertyChanged event on CurrentItem.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        protected override void m_currentItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "OperationDescription":
                case "SeverityName":
                case "SetPointEnabled":
                case "ToleranceEnabled":
                case "HysteresisEnabled":
                case "DelayEnabled":
                    break;

                default:
                    base.m_currentItem_PropertyChanged(sender, e);
                    break;
            }

            if (e.PropertyName == "SignalID")
                m_measurementLabels = GetMeasurementLabels();

            if (e.PropertyName == "SignalID" || e.PropertyName == "Operation" || e.PropertyName == "SetPoint" || e.PropertyName == "Delay")
                CurrentItem.OperationDescription = GetOperationDescription(CurrentItem);

            if (e.PropertyName == "Severity")
                CurrentItem.SeverityName = SeverityList[CurrentItem.Severity];

            if (e.PropertyName == "Operation")
                UpdateEnableFlags();
        }

        /// <summary>
        /// Raises the <see cref="PagedViewModelBase{T1,T2}.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Property name that has changed.</param>
        protected override void OnPropertyChanged(string propertyName)
        {
            string selectedMeasurementLabel;

            if (propertyName == "ItemsSource")
                m_measurementLabels = GetMeasurementLabels();
            
            base.OnPropertyChanged(propertyName);

            if (propertyName == "CurrentItem")
            {
                m_measurementLabels.TryGetValue(CurrentItem.SignalID, out selectedMeasurementLabel);
                SelectedMeasurementLabel = selectedMeasurementLabel;
                UpdateEnableFlags();
            }
        }

        // Creates a list of operations that can be performed to trigger an alarm.
        private Dictionary<int, string> CreateOperationList()
        {
            Dictionary<int, string> operationList = new Dictionary<int, string>();

            operationList.Add((int)AlarmOperation.Equal, "Equal to");
            operationList.Add((int)AlarmOperation.NotEqual, "Not equal to");
            operationList.Add((int)AlarmOperation.GreaterOrEqual, "Greater than or equal to");
            operationList.Add((int)AlarmOperation.LessOrEqual, "Less than or equal to");
            operationList.Add((int)AlarmOperation.GreaterThan, "Greater than");
            operationList.Add((int)AlarmOperation.LessThan, "Less than");
            operationList.Add((int)AlarmOperation.Flatline, "Latched");

            return operationList;
        }

        // Creates a list of levels of severity that can be assigned to alarms.
        private Dictionary<int, string> CreateSeverityList()
        {
            Dictionary<int, string> severityList = new Dictionary<int, string>();

            severityList.Add((int)AlarmSeverity.None, "None");
            severityList.Add((int)AlarmSeverity.Information, "Information");
            severityList.Add((int)AlarmSeverity.Low, "Low");
            severityList.Add((int)AlarmSeverity.MediumLow, "Medium Low");
            severityList.Add((int)AlarmSeverity.Medium, "Medium");
            severityList.Add((int)AlarmSeverity.MediumHigh, "Medium High");
            severityList.Add((int)AlarmSeverity.High, "High");
            severityList.Add((int)AlarmSeverity.Critical, "Critical");
            severityList.Add((int)AlarmSeverity.Error, "Error");

            return severityList;
        }

        // Creates a list of measurement labels for the current page of alarms.
        private Dictionary<Guid, string> GetMeasurementLabels()
        {
            List<Guid> signals = ItemsSource.Select(alarm => alarm.SignalID).Distinct().ToList();
            IList<DataModels.Measurement> measurements = DataModels.Measurement.LoadFromKeys(null, signals);
            return measurements.ToDictionary(measurement => measurement.SignalID, measurement => measurement.PointTag);
        }

        // Generates a description of the operation that triggers the alarm.
        private string GetOperationDescription(DataModels.Alarm item)
        {
            if (item.ID > 0)
            {
                string measurementLabel;
                StringBuilder description;

                if (!m_measurementLabels.TryGetValue(item.SignalID, out measurementLabel))
                    measurementLabel = m_selectedMeasurementLabel;

                description = new StringBuilder(measurementLabel);

                switch ((AlarmOperation)item.Operation)
                {
                    case AlarmOperation.Equal:
                        description.Append(" = ");
                        break;

                    case AlarmOperation.NotEqual:
                        description.Append(" != ");
                        break;

                    case AlarmOperation.GreaterOrEqual:
                        description.Append(" >= ");
                        break;

                    case AlarmOperation.LessOrEqual:
                        description.Append(" <= ");
                        break;

                    case AlarmOperation.GreaterThan:
                        description.Append(" > ");
                        break;

                    case AlarmOperation.LessThan:
                        description.Append(" < ");
                        break;

                    case AlarmOperation.Flatline:
                        if ((object)item.Delay == null)
                            return string.Empty;

                        description.Append(" flatlined for ");
                        description.Append(item.Delay);
                        description.Append(" seconds");
                        return description.ToString();

                    default:
                        return string.Empty;
                }

                if ((object)item.SetPoint == null)
                    return string.Empty;

                description.Append(item.SetPoint);
                return description.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Updates the enable flags which determine whether
        /// or not certain input fields are enabled.
        /// </summary>
        private void UpdateEnableFlags()
        {
            bool[] enabledFlags = { true, true, true, true };

            switch ((AlarmOperation)CurrentItem.Operation)
            {
                case AlarmOperation.Equal:
                case AlarmOperation.NotEqual:
                    enabledFlags = new[] { true, true, true, false };
                    break;

                case AlarmOperation.GreaterOrEqual:
                case AlarmOperation.LessOrEqual:
                case AlarmOperation.GreaterThan:
                case AlarmOperation.LessThan:
                    enabledFlags = new[] { true, false, true, true };
                    break;

                case AlarmOperation.Flatline:
                    enabledFlags = new[] { false, false, true, false };
                    break;
            }

            CurrentItem.SetPointEnabled = enabledFlags[0];
            CurrentItem.ToleranceEnabled = enabledFlags[1];
            CurrentItem.DelayEnabled = enabledFlags[2];
            CurrentItem.HysteresisEnabled = enabledFlags[3];
        }

        #endregion
    }
}
