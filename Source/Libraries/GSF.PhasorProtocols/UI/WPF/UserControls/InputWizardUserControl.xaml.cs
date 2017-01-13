//******************************************************************************************************
//  InputWizardUserControl.xaml.cs - Gbtc
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
//  05/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

                m_dataContext.StepTwoExpanded = true;
                m_dataContext.CurrentDeviceRuntimeID = Convert.ToInt32(CommonFunctions.GetRuntimeID("Device", device.ID));
                m_dataContext.NewDeviceConfiguration = false;
            }
        }

        #endregion

        #region [ Methods ]

        private void InputWizardUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bool showWalkthrough = true;

            if (IsolatedStorageManager.SettingExists("ShowWalkthroughAtStartup"))
                showWalkthrough = Convert.ToBoolean(IsolatedStorageManager.ReadFromIsolatedStorage("ShowWalkthroughAtStartup"));

            if (showWalkthrough)
                m_dataContext.LaunchWalkthroughCommand.Execute(null);
        }

        private void InputWizardUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (m_dataContext.DisconnectedCurrentDevice && m_dataContext.CurrentDeviceRuntimeID > 0)
                CommonFunctions.SendCommandToService("Connect " + m_dataContext.CurrentDeviceRuntimeID);

            m_dataContext.CurrentDeviceRuntimeID = 0;

            if (m_dataContext.RequestConfigurationPopupIsOpen)
                m_dataContext.CancelConfigurationRequestCommand.Execute(null);
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
            if (m_dataContext.StepTwoExpanded)
                m_dataContext.StepOneExpanded = true;
            else if (m_dataContext.StepThreeExpanded)
                m_dataContext.StepTwoExpanded = true;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (m_dataContext.StepOneExpanded)
            {
                m_dataContext.StepTwoExpanded = true;
            }
            else if (m_dataContext.StepTwoExpanded)
            {
                m_dataContext.StepThreeExpanded = true;
            }
            else if (m_dataContext.StepThreeExpanded)
            {
                if (!m_dataContext.ConnectToConcentrator || ValidatePMUNames())
                {
                    m_dataContext.SavePDC();
                    m_dataContext.SaveConfiguration();
                }
                else
                {
                    m_dataContext.Popup($"One of the device acronyms is set to the PDC acronym \"{m_dataContext.PdcAcronym}\", device cannot be saved.", "Validation Error", MessageBoxImage.Error);
                }
            }
        }

        private bool ValidatePMUNames()
        {
            string pdcAcronym = m_dataContext.PdcAcronym.Trim();

            foreach (InputWizardDevice device in m_dataContext.ItemsSource)
            {
                if (device.Acronym.Trim().Equals(pdcAcronym, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private void ExpanderStep1_Expanded(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                m_dataContext.StepTwoExpanded = false;
                m_dataContext.StepThreeExpanded = false;
                ButtonNext.Content = "Next";
                ButtonPrevious.IsEnabled = false;
            }
        }

        private void ExpanderStep2_Expanded(object sender, RoutedEventArgs e)
        {
            m_dataContext.StepOneExpanded = false;
            m_dataContext.StepThreeExpanded = false;
            ButtonNext.Content = "Next";
            ButtonPrevious.IsEnabled = true;
        }

        private void ExpanderStep3_Expanded(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Controls.Validation.GetHasError(PdcAcronymTextBox))
            {
                ReadOnlyObservableCollection<ValidationError> errors = System.Windows.Controls.Validation.GetErrors(PdcAcronymTextBox);
                IEnumerable<string> errorMessages = errors.Select(error => error.ErrorContent).OfType<string>();
                string errorMessage = string.Join(Environment.NewLine, errorMessages);

                MessageBox.Show(errorMessage, "PDC Acronym Error", MessageBoxButton.OK, MessageBoxImage.Error);
                m_dataContext.StepThreeExpanded = false;
                m_dataContext.StepTwoExpanded = true;
                PdcAcronymTextBox.Focus();
            }
            else
            {
                m_dataContext.StepOneExpanded = false;
                m_dataContext.StepTwoExpanded = false;
                ButtonNext.Content = "Finish";
                ButtonPrevious.IsEnabled = true;
            }
        }

        private void RowDetailsDataGrid_Initialized(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;

            if ((object)grid != null)
                grid.Visibility = (grid.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RowDetailsDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataGrid grid;
            ScrollViewer scrollViewer;

            if (e.PreviousSize != e.NewSize)
            {
                grid = sender as DataGrid;

                if ((object)grid != null)
                {
                    scrollViewer = grid.Template.FindName("DG_ScrollViewer", grid) as ScrollViewer;

                    if ((object)scrollViewer != null)
                    {
                        scrollViewer.Width = double.NaN;
                        scrollViewer.UpdateLayout();
                        scrollViewer.Width = scrollViewer.ActualWidth;
                        scrollViewer.UpdateLayout();
                    }
                }
            }
        }

        #endregion
    }
}
