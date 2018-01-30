//******************************************************************************************************
//  RetransmissionStatPicker.xaml.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  07/30/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#if !MONO
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GSF;
using GSF.TimeSeries.UI.DataModels;

namespace FileAdapters
{
    /// <summary>
    /// Interaction logic for RetransmissionStatPicker.xaml
    /// </summary>
    public partial class RetransmissionStatPicker : UserControl
    {
        #region [ Members ]

        // Fields
        private Adapter m_fileBlockReader;
        private string m_parameterName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RetransmissionStatPicker"/> class.
        /// </summary>
        /// <param name="fileBlockReader">The file block reader adapter to be configured.</param>
        /// <param name="parameterName">Name of the connection string parameter to be modified.</param>
        public RetransmissionStatPicker(Adapter fileBlockReader, string parameterName)
        {
            InitializeComponent();
            m_fileBlockReader = fileBlockReader;
            m_parameterName = parameterName;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            OriginalValueTextBox.Text = GetParameterValue();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the retransmission view model statistic.
        /// </summary>
        public RetransmissionStatViewModel ViewModel
        {
            get
            {
                return (RetransmissionStatViewModel)Resources["ViewModel"];
            }
        }

        #endregion

        #region [ Methods ]

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "FilterExpression")
                MeasurementPager.ReloadDataGrid();
        }

        private void MeasurementPager_CurrentItemChanged(object sender, EventArgs e)
        {
            Guid signalID = MeasurementPager.CurrentItem.SignalID;

            if (signalID != Guid.Empty)
                NewValueTextBox.Text = signalID.ToString();
        }

        private void MeasurementPager_ButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (MeasurementPager.CurrentItem.SignalID != Guid.Empty)
            {
                Dictionary<string, string> settings = m_fileBlockReader.ConnectionString.ToNonNullString().ParseKeyValuePairs();
                settings[m_parameterName] = NewValueTextBox.Text;
                m_fileBlockReader.ConnectionString = settings.JoinKeyValuePairs();
            }
        }

        private string GetParameterValue()
        {
            Dictionary<string, string> settings = m_fileBlockReader.ConnectionString.ToNonNullString().ParseKeyValuePairs();
            string value;

            if (settings.TryGetValue(m_parameterName, out value))
                return value;

            return null;
        }

        #endregion
    }
}
#endif