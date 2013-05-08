//******************************************************************************************************
//  InputWizardUserControl.xaml.cs - Gbtc
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
//  05/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.ViewModels;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for InputWizardUserControl.xaml
    /// </summary>
    public partial class InputWizardUserControl : UserControl
    {
        #region [ Members ]

        private readonly InputWizardDevices m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="InputWizardUserControl"/> class.
        /// </summary>
        public InputWizardUserControl()
        {
            InitializeComponent();
            this.Unloaded += InputWizardUserControl_Unloaded;
            m_dataContext = new InputWizardDevices(1);
            StackPanelRoot.DataContext = m_dataContext;
        }



        /// <summary>
        /// Creates an instance of <see cref="InputWizardUserControl"/> class for the specified <see cref="Device"/>.
        /// </summary>
        /// <param name="device"></param>
        public InputWizardUserControl(Device device)
            : this()
        {
            if (device != null)
            {
                m_dataContext.SkipDisableRealTimeData = device.SkipDisableRealTimeData;

                Dictionary<string, string> connectionSettings = device.ConnectionString.ToLower().ParseKeyValuePairs();

                m_dataContext.ConnectionString = device.ConnectionString;
                m_dataContext.AlternateCommandChannel = device.AlternateCommandChannel;
                m_dataContext.AccessID = device.AccessID;
                m_dataContext.ProtocolID = device.ProtocolID ?? 0;
                m_dataContext.CompanyID = device.CompanyID ?? 0;
                m_dataContext.HistorianID = device.HistorianID ?? 0;
                m_dataContext.InterconnectionID = device.InterconnectionID ?? 0;

                if (device.IsConcentrator)
                {
                    m_dataContext.ConnectToConcentrator = true;
                    m_dataContext.PdcAcronym = device.Acronym;
                    m_dataContext.PdcName = device.Name;
                    m_dataContext.PdcVendorDeviceID = device.VendorDeviceID ?? 0;
                }

                ExpanderStep2.IsExpanded = true;
                m_dataContext.CurrentDeviceRuntimeID = Convert.ToInt32(CommonFunctions.GetRuntimeID("Device", device.ID));
                m_dataContext.NewDeviceConfiguration = false;
            }
        }

        #endregion

        #region [ Methods ]

        private void InputWizardUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (m_dataContext.DisconnectedCurrentDevice && m_dataContext.CurrentDeviceRuntimeID > 0)
                CommonFunctions.SendCommandToService("Connect " + m_dataContext.CurrentDeviceRuntimeID);

            m_dataContext.CurrentDeviceRuntimeID = 0;
        }

        /// <summary>
        /// Hanldes checked event on the select all check box.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBoxAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (InputWizardDevice device in m_dataContext.ItemsSource)
            {
                device.Include = true;
                foreach (InputWizardDevicePhasor phasor in device.PhasorList)
                    phasor.Include = true;
            }
        }

        /// <summary>
        /// Handles unchecked event on the select all check box.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBoxAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (InputWizardDevice device in m_dataContext.ItemsSource)
            {
                device.Include = false;
                foreach (InputWizardDevicePhasor phasor in device.PhasorList)
                    phasor.Include = false;
            }
        }

        private void CheckBoxDevice_Checked(object sender, RoutedEventArgs e)
        {
            foreach (InputWizardDevicePhasor phasor in ((InputWizardDevice)((CheckBox)sender).DataContext).PhasorList)
                phasor.Include = true;
        }

        private void CheckBoxDevice_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (InputWizardDevicePhasor phasor in ((InputWizardDevice)((CheckBox)sender).DataContext).PhasorList)
                phasor.Include = false;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (ExpanderStep2.IsExpanded)
                ExpanderStep1.IsExpanded = true;
            else if (ExpanderStep3.IsExpanded)
                ExpanderStep2.IsExpanded = true;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (ExpanderStep1.IsExpanded)
            {
                ExpanderStep2.IsExpanded = true;
            }
            else if (ExpanderStep2.IsExpanded)
            {
                ExpanderStep3.IsExpanded = true;
            }
            else if (ExpanderStep3.IsExpanded)
            {
                m_dataContext.SavePDC();
                m_dataContext.SaveConfiguration();
            }
        }

        private void ExpanderStep1_Expanded(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                ExpanderStep2.IsExpanded = false;
                ExpanderStep3.IsExpanded = false;
                ButtonNext.Content = "Next";
                ButtonPrevious.IsEnabled = false;
            }
        }

        private void ExpanderStep2_Expanded(object sender, RoutedEventArgs e)
        {
            ExpanderStep1.IsExpanded = false;
            ExpanderStep3.IsExpanded = false;
            ButtonNext.Content = "Next";
            ButtonPrevious.IsEnabled = true;
        }

        private void ExpanderStep3_Expanded(object sender, RoutedEventArgs e)
        {
            string errorMessage;

            if (!m_dataContext.ValidatePDCDetails(out errorMessage))
            {
                MessageBox.Show(errorMessage, "PDC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExpanderStep3.IsExpanded = false;
            }
            else
            {
                ExpanderStep1.IsExpanded = false;
                ExpanderStep2.IsExpanded = false;
                ButtonNext.Content = "Finish";
                ButtonPrevious.IsEnabled = true;
            }
        }

        #endregion
    }
}
