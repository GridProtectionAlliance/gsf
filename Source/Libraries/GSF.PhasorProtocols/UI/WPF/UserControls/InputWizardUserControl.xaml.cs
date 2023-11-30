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
    public partial class InputWizardUserControl
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
            if (device is null)
                return;
            
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

                ObservableCollection<Device> devices = Device.GetDevices(null, "WHERE ParentID = " + device.ID);
                m_dataContext.DeviceIDs = devices.Select(childDevice => childDevice.ID).ToArray();
                m_dataContext.DeviceAcronyms = devices.Select(childDevice => childDevice.Acronym).ToArray();
            }
            else
            {
                m_dataContext.DeviceIDs = new[] { device.ID };
                m_dataContext.DeviceAcronyms = new[] { device.Acronym };
            }

            m_dataContext.StepTwoExpanded = true;
            m_dataContext.CurrentDeviceRuntimeID = Convert.ToInt32(CommonFunctions.GetRuntimeID("Device", device.ID));
            m_dataContext.NewDeviceConfiguration = false;
        }

        #endregion

        #region [ Properties ]

        private bool HasPhaseErrors => m_dataContext.ItemsSource.Any(inputDevice => inputDevice.PhasorList.Any(phasor => !string.IsNullOrEmpty(phasor["Phase"])));

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
        /// Handles checked event on the select all check box.
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

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            // Accept all phase guesses
            foreach (InputWizardDevice device in m_dataContext.ItemsSource)
            {
                foreach (InputWizardDevicePhasor phasor in device.PhasorList)
                {
                    if (phasor.Phase.EndsWith("?"))
                        phasor.Phase = phasor.Phase[0].ToString();
                }
            }

            ButtonAccept.Visibility = Visibility.Hidden;
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
                if (m_dataContext.ItemsSource.Any(inputDevice => inputDevice.PhasorList.Any(phasor => !phasor.IsValid)))
                {
                    ButtonAccept.Visibility = HasPhaseErrors ? Visibility.Visible : Visibility.Hidden;
                    m_dataContext.Popup("Fix all validation errors before device can be saved.", "Validation Error", MessageBoxImage.Error);
                }
                else
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
        }

        private bool ValidatePMUNames()
        {
            string pdcAcronym = m_dataContext.PdcAcronym.Trim();
            return m_dataContext.ItemsSource.All(device => !device.Acronym.Trim().Equals(pdcAcronym, StringComparison.OrdinalIgnoreCase));
        }

        private void ExpanderStep1_Expanded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;
            
            m_dataContext.StepTwoExpanded = false;
            m_dataContext.StepThreeExpanded = false;

            ButtonNext.Content = "Next";
            ButtonPrevious.IsEnabled = false;
            ButtonAccept.Visibility = Visibility.Hidden;
        }

        private void ExpanderStep2_Expanded(object sender, RoutedEventArgs e)
        {
            m_dataContext.StepOneExpanded = false;
            m_dataContext.StepThreeExpanded = false;
            
            ButtonNext.Content = "Next";
            ButtonPrevious.IsEnabled = true;
            ButtonAccept.Visibility = Visibility.Hidden;
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

            ButtonAccept.Visibility = HasPhaseErrors ? Visibility.Visible : Visibility.Hidden;
        }

        private void RowDetailsDataGrid_Initialized(object sender, EventArgs e)
        {
            if (sender is DataGrid grid)
                grid.Visibility = grid.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RowDetailsDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize == e.NewSize)
                return;

            if (sender is not DataGrid grid)
                return;

            if (grid.Template.FindName("DG_ScrollViewer", grid) is not ScrollViewer scrollViewer)
                return;
            
            scrollViewer.Width = double.NaN;
            scrollViewer.UpdateLayout();
            scrollViewer.Width = scrollViewer.ActualWidth;
            scrollViewer.UpdateLayout();
        }

        private void PdcAcronymTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UseSourcePrefix.Content = $"Use Source Prefix: {PdcAcronymTextBox.Text}!";

            if (!m_dataContext.UseSourcePrefix)
                return;

            m_dataContext.UseSourcePrefix = false;
            m_dataContext.UseSourcePrefix = true;
        }

        #endregion
    }
}
