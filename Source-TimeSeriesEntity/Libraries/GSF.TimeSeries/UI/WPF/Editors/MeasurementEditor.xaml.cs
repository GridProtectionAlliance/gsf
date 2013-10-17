//******************************************************************************************************
//  MeasurementEditor.xaml.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  08/02/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GSF.Data;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.UI.DataModels;

namespace GSF.TimeSeries.UI.Editors
{
    /// <summary>
    /// Interaction logic for MeasurementEditor.xaml
    /// </summary>
    public partial class MeasurementEditor : UserControl
    {
        #region [ Members ]

        // Fields
        private Adapter m_adapter;
        private string m_parameterName;
        private string m_connectionString;
        private bool m_updatingSelectedMeasurements;
        private DispatcherTimer m_dispatcherTimer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MeasurementEditor"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to be configured.</param>
        /// <param name="parameterName">The name of the parameter being configured.</param>
        public MeasurementEditor(Adapter adapter, string parameterName)
            : this(adapter, parameterName, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MeasurementEditor"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to be configured.</param>
        /// <param name="parameterName">The name of the parameter being configured.</param>
        /// <param name="connectionString">Parameters which define how this control interacts with the user.</param>
        public MeasurementEditor(Adapter adapter, string parameterName, string connectionString)
        {
            InitializeComponent();
            m_adapter = adapter;
            m_parameterName = parameterName;
            m_connectionString = connectionString;
            m_dispatcherTimer = new DispatcherTimer();
            m_dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            m_dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        #endregion

        #region [ Methods ]

        private void MeasurementPager_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> settings;
            string setting;

            if ((object)m_connectionString != null)
            {
                settings = m_connectionString.ParseKeyValuePairs();

                if (settings.TryGetValue("filterExpression", out setting))
                {
                    MeasurementPager.FilterExpression = setting;
                    MeasurementPager.ReloadDataGrid();
                }

                if (settings.TryGetValue("selectable", out setting))
                {
                    MeasurementPager.Selectable = setting.ParseBoolean();
                    FilterExpressionTextBox.Visibility = MeasurementPager.Selectable ? Visibility.Visible : Visibility.Collapsed;
                }

                if (settings.TryGetValue("searchable", out setting))
                    MeasurementPager.Searchable = setting.ParseBoolean();
            }

            FilterExpressionTextBox.Text = GetParameterValue();
        }

        private void MeasurementEditor_KeyDown(object sender, KeyEventArgs e)
        {
            bool setFocus;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                setFocus = true;

                if (e.Key == Key.Z && FilterExpressionTextBox.CanUndo)
                    FilterExpressionTextBox.Undo();
                else if (e.Key == Key.Y && FilterExpressionTextBox.CanRedo)
                    FilterExpressionTextBox.Redo();
                else
                    setFocus = false;

                if (setFocus)
                    FilterExpressionTextBox.Focus();
            }
        }

        private void FilterExpressionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!m_updatingSelectedMeasurements)
            {
                m_dispatcherTimer.Stop();
                m_dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs eventArgs)
        {
            try
            {
                m_updatingSelectedMeasurements = true;

                MeasurementPager.SelectedMeasurements.Clear();

                foreach (Guid signalID in AdapterBase.ParseOutputMeasurementKeys(GetDataSource(), false, FilterExpressionTextBox.Text).Select(key => key.SignalID))
                    MeasurementPager.SelectedMeasurements.Add(signalID);
            }
            catch
            {
                MeasurementPager.SelectedMeasurements.Clear();
            }
            finally
            {
                MeasurementPager.ReloadDataGrid();
                MeasurementPager.UpdateSelections();
                m_updatingSelectedMeasurements = false;
                m_dispatcherTimer.Stop();
            }
        }

        private void MeasurementPager_SelectedMeasurementsChanged(object sender, EventArgs e)
        {
            if (!m_updatingSelectedMeasurements)
            {
                try
                {
                    m_updatingSelectedMeasurements = true;
                    FilterExpressionTextBox.Text = string.Join(";", MeasurementPager.SelectedMeasurements.Select(signalID => signalID.ToString()));
                }
                finally
                {
                    m_updatingSelectedMeasurements = false;
                }
            }
        }

        private void MeasurementPager_ButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> settings = m_adapter.ConnectionString.ToNonNullString().ParseKeyValuePairs();

            if (MeasurementPager.Selectable)
                settings[m_parameterName] = FilterExpressionTextBox.Text;
            else
                settings[m_parameterName] = MeasurementPager.CurrentItem.SignalID.ToString();

            m_adapter.ConnectionString = settings.JoinKeyValuePairs();
        }

        private DataSet GetDataSource()
        {
            DataSet dataSource;

            using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
            {
                dataSource = database.Connection.RetrieveDataSet(database.AdapterType, "SELECT * FROM ActiveMeasurement");
                dataSource.Tables[0].TableName = "ActiveMeasurements";
                return dataSource;
            }
        }

        private string GetParameterValue()
        {
            Dictionary<string, string> settings = m_adapter.ConnectionString.ToNonNullString().ParseKeyValuePairs();
            string value;

            if (settings.TryGetValue(m_parameterName, out value))
                return value;

            return null;
        }

        #endregion
    }
}
