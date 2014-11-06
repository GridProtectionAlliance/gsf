//******************************************************************************************************
//  InputWizardWalkthrough.xaml.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using GSF.IO;
using GSF.TimeSeries.UI;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace GSF.PhasorProtocols.UI.Modal
{
    /// <summary>
    /// Interaction logic for InputWizardWalkthrough.xaml
    /// </summary>
    public partial class InputWizardWalkthrough : Window
    {
        #region [ Members ]

        // Fields
        private List<string> m_history; 

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="InputWizardWalkthrough"/> class.
        /// </summary>
        public InputWizardWalkthrough()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Startup steps, when the page is loaded.
        /// </summary>
        private void InputWizardWalkthrough_Loaded(object sender, RoutedEventArgs e)
        {
            m_history = new List<string>() { "Welcome" };

            if (IsolatedStorageManager.SettingExists("ShowWalkthroughAtStartup"))
                DoNotShowCheckBox.IsChecked = !Convert.ToBoolean(IsolatedStorageManager.ReadFromIsolatedStorage("ShowWalkthroughAtStartup"));
        }

        /// <summary>
        /// Hides the window when executing a configuration request.
        /// </summary>
        private void HideInputWizardWalkthrough(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Unhides the window when the configuration request has completed.
        /// </summary>
        private void ShowInputWizardWalkthrough(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Visible;

            if (RequestConfigurationSuccessCheckBox.IsChecked == true)
            {
                if (ConnectionIsToConcentratorCheckBox.IsChecked == true)
                    TraverseDecisionTree("PDCNameInput");
                else
                    TraverseDecisionTree("HistorianInput");
            }
        }

        /// <summary>
        /// Detach the data context when the window is closed so that modifications
        /// to the data context by the wizard itself will not result in errors
        /// (attempting to make changes to a closed window).
        /// </summary>
        private void InputWizardWalkthrough_Closed(object sender, EventArgs e)
        {
            DataContext = null;
        }

        /// <summary>
        /// When the user checks the "Do not show at startup" checkbox.
        /// </summary>
        private void DoNotShowAtStartup_Checked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageManager.WriteToIsolatedStorage("ShowWalkthroughAtStartup", false);
        }

        /// <summary>
        /// When the user unchecks the "Do not show at startup" checkbox.
        /// </summary>
        private void DoNotShowAtStartup_Unchecked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageManager.WriteToIsolatedStorage("ShowWalkthroughAtStartup", true);
        }

        /// <summary>
        /// Attempts to run the PMU Connection Tester.
        /// </summary>
        private bool TryRunConnectionTester(string fullPath)
        {
            try
            {
                if (fullPath.EndsWith("PMUConnectionTester.exe") && File.Exists(fullPath))
                {
                    using (Process.Start(fullPath))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the text in the text box, and updates the source of
        /// the binding so that changes are reflected in the view model.
        /// </summary>
        private void UpdateText(TextBox textBox, string text)
        {
            BindingExpression bindingExpression;

            textBox.Text = text;
            bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);

            if ((object)bindingExpression != null)
                bindingExpression.UpdateSource();
        }

        /// <summary>
        /// Updates the is-checked state of the check box, and updates the source
        /// of the binding so that changes are reflected in the view model.
        /// </summary>
        private void UpdateIsChecked(CheckBox checkBox, bool isChecked)
        {
            BindingExpression bindingExpression;

            checkBox.IsChecked = isChecked;
            bindingExpression = checkBox.GetBindingExpression(ToggleButton.IsCheckedProperty);

            if ((object)bindingExpression != null)
                bindingExpression.UpdateSource();
        }

        #region [ Decision Tree Navigation ]

        /// <summary>
        /// Uses the sender's Tag property to move to the next page of the walkthrough.
        /// </summary>
        private void TraverseDecisionTree(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if ((object)button != null && button.Tag != null)
                TraverseDecisionTree(button.Tag.ToString());
        }

        /// <summary>
        /// Moves to the next page of the walkthrough.
        /// </summary>
        private void TraverseDecisionTree(string header)
        {
            TabItem navigatedItem;

            navigatedItem = TabSwitcher.Items
                .OfType<TabItem>()
                .FirstOrDefault(item => header.Equals(item.Header));

            if ((object)navigatedItem != null)
            {
                m_history.Add(header);
                TabSwitcher.SelectedItem = navigatedItem;
            }
        }

        /// <summary>
        /// Uses the sender's Tag property to move backwards through the page history.
        /// </summary>
        private void ReverseDecisionTree(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if ((object)button != null && button.Tag != null)
                ReverseDecisionTree(button.Tag.ToString());
        }

        /// <summary>
        /// Moves backwards through the page history to display the given page.
        /// </summary>
        private void ReverseDecisionTree(string header)
        {
            for (int i = m_history.Count - 1; i >= 0; i--)
            {
                if (m_history[i] == header)
                {
                    m_history.RemoveRange(i, m_history.Count - i);
                    TraverseDecisionTree(header);
                    break;
                }
            }
        }

        /// <summary>
        /// Moves backwards through the page history by a given number of pages.
        /// </summary>
        private void ReverseDecisionTree(int pages)
        {
            int pagesRemoved = Math.Min(m_history.Count, pages + 1);
            int headerIndex = m_history.Count - pagesRemoved;
            string header = m_history[headerIndex];

            m_history.RemoveRange(headerIndex, pagesRemoved);
            TraverseDecisionTree(header);
        }

        #endregion

        #region [ Page Selected Handlers ]

        /// <summary>
        /// Executes when the ConnectionFileInput page is selected to automatically
        /// search for a connection file that might contain the connection settings
        /// for the device that the user wants to connect to.
        /// </summary>
        private void ConnectionFileInput_Selected(object sender, RoutedEventArgs e)
        {
            Func<string, string> getConnectionFile = directory => Directory.EnumerateFiles(directory, "*.PmuConnection")
                .Select(fileName => new FileInfo(fileName))
                .OrderByDescending(fileInfo => fileInfo.CreationTimeUtc)
                .Select(fileInfo => fileInfo.Name)
                .FirstOrDefault();

            string currentDirectory = FilePath.GetAbsolutePath("");
            string openPDCDirectory = FilePath.GetAbsolutePath(@"..\openPDC");
            string pmuConnectionTesterDirectory = FilePath.GetAbsolutePath(@"..\PMU Connection Tester");

            string connectionFile = getConnectionFile(currentDirectory);

            if ((object)connectionFile == null && Directory.Exists(openPDCDirectory))
                connectionFile = getConnectionFile(openPDCDirectory);

            if ((object)connectionFile == null && Directory.Exists(pmuConnectionTesterDirectory))
                connectionFile = getConnectionFile(pmuConnectionTesterDirectory);

            if ((object)connectionFile == null)
                connectionFile = string.Empty;

            UpdateText(ConnectionFileTextBox, connectionFile);
            UpdateText(CommandChannelTextBox, string.Empty);
            UpdateText(DataChannelTextBox, string.Empty);
            ExpandStepOne(sender, e);
        }

        /// <summary>
        /// Executes when one of the connection string setup pages is displayed
        /// in order to clear out all the relevant text boxes beforehand.
        /// </summary>
        private void ConnectionStringInput_Selected(object sender, RoutedEventArgs e)
        {
            UpdateText(ConnectionFileTextBox, string.Empty);
            UpdateText(CommandChannelTextBox, string.Empty);
            UpdateText(DataChannelTextBox, string.Empty);
            ExpandStepOne(sender, e);
        }

        /// <summary>
        /// Expands step 1 of the input wizard.
        /// </summary>
        private void ExpandStepOne(object sender, RoutedEventArgs e)
        {
            UpdateIsChecked(StepOneExpandedCheckBox, true);
        }

        /// <summary>
        /// Expands step 1 of the input wizard.
        /// </summary>
        private void ExpandStepTwo(object sender, RoutedEventArgs e)
        {
            UpdateIsChecked(StepTwoExpandedCheckBox, true);
        }

        /// <summary>
        /// Expands step 1 of the input wizard.
        /// </summary>
        private void ExpandStepThree(object sender, RoutedEventArgs e)
        {
            UpdateIsChecked(StepThreeExpandedCheckBox, true);
        }

        #endregion

        #region [ Click Handlers ]

        /// <summary>
        /// Attempts to run the PMU Connection Tester from a number of different locations.
        /// </summary>
        private void RunConnectionTesterQuery_Yes(object sender, RoutedEventArgs e)
        {
            const string Executable = "PMUConnectionTester.exe";

            string currentDirectory = FilePath.GetAbsolutePath("");
            string openPDCDirectory = FilePath.GetAbsolutePath(@"..\openPDC");
            string pmuConnectionTesterDirectory = FilePath.GetAbsolutePath(@"..\PMU Connection Tester");
            string fullPath = null;
            bool failed = false;

            if (IsolatedStorageManager.SettingExists("PMUConnectionTester.exe"))
                fullPath = IsolatedStorageManager.ReadFromIsolatedStorage("PMUConnectionTester.exe").ToNonNullString();

            if (!TryRunConnectionTester(fullPath))
            {
                fullPath = Path.Combine(currentDirectory, Executable);

                if (!TryRunConnectionTester(fullPath))
                {
                    fullPath = Path.Combine(openPDCDirectory, Executable);

                    if (!TryRunConnectionTester(fullPath))
                    {
                        fullPath = Path.Combine(pmuConnectionTesterDirectory, Executable);

                        if (!TryRunConnectionTester(fullPath))
                            failed = true;
                    }
                }
            }

            if (failed)
                TraverseDecisionTree("ConnectionTesterPathInput");
            else
                TraverseDecisionTree("ConnectionFileQuery");
        }

        /// <summary>
        /// Enables the user to browse for the PMU Connection Tester executable.
        /// </summary>
        private void ConnectionTesterBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "PMU Connection Tester|PMUConnectionTester.exe";
            openFileDialog.CheckFileExists = true;

            if (openFileDialog.ShowDialog() == true)
                ConnectionTesterPathTextBox.Text = openFileDialog.FileName;
        }

        /// <summary>
        /// Attempts to run the PMU Connection Tester from the path given by the user.
        /// </summary>
        private void RunConnectionTesterQuery_OK(object sender, RoutedEventArgs e)
        {
            string connectionTesterPath = ConnectionTesterPathTextBox.Text;

            if (TryRunConnectionTester(connectionTesterPath))
            {
                IsolatedStorageManager.WriteToIsolatedStorage("PMUConnectionTester.exe", connectionTesterPath);
                TraverseDecisionTree("ConnectionFileQuery");
            }
            else
            {
                TraverseDecisionTree("NumberOfChannelsQuery");
            }
        }

        /// <summary>
        /// For single-channel devices, displays the data channel builder and
        /// navigates automatically once the connection settings have been saved.
        /// </summary>
        private void DataChannelBuilder_Launch(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ICommand command;

            if ((object)button != null)
            {
                command = button.Tag as ICommand;

                if ((object)command != null)
                    command.Execute(button.CommandParameter);
            }

            if (!string.IsNullOrEmpty(DataChannelTextBox.Text))
                TraverseDecisionTree("ProtocolInput");
        }

        /// <summary>
        /// For dual-channel devices, validates the connection
        /// strings and navigates to the next page.
        /// </summary>
        private void DualChannelInput_Done(object sender, RoutedEventArgs e)
        {
            string message = null;

            if (string.IsNullOrEmpty(CommandChannelTextBox.Text))
            {
                message = "The walkthrough has detected that your command channel" +
                          " has not been set up. Are you sure you would like to proceed?";
            }
            else if (string.IsNullOrEmpty(DataChannelTextBox.Text))
            {
                message = "The walkthrough has detected that your data channel" +
                          " has not been set up. Are you sure you would like to proceed?";
            }

            if ((object)message == null || MessageBox.Show(message, "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                TraverseDecisionTree("ProtocolInput");
        }

        /// <summary>
        /// Sets the selected protocol to the one the user selected and then
        /// navigates to the appropriate page based on the selected protocol.
        /// </summary>
        private void PhasorProtocol_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string protocolText;

            if ((object)button != null && (object)button.Content != null)
            {
                protocolText = button.Content.ToString();

                ProtocolComboBox.SelectedItem = ProtocolComboBox.Items
                    .OfType<KeyValuePair<int, string>>()
                    .FirstOrDefault(kvp => kvp.Value == protocolText);

                switch (protocolText)
                {
                    case "IEEE C37.118.2-2011":
                    case "IEEE C37.118-2005":
                    case "IEEE C37.118 Draft 6":
                    case "IEC 61850-90-5":
                    case "IEEE 1344-1995":
                        TraverseDecisionTree("AccessIDInput");
                        break;

                    case "BPA PDCstream":
                        TraverseDecisionTree("INIFileInput");
                        break;

                    default:
                        TraverseDecisionTree("RequestConfigurationQuery");
                        break;
                }
            }
        }

        /// <summary>
        /// Determines which page to navigate to when the OK
        /// button is clicked after entering a configuration file.
        /// </summary>
        private void ConfigurationFileInput_OK(object sender, RoutedEventArgs e)
        {
            if (ConnectionIsToConcentratorCheckBox.IsChecked == true)
                TraverseDecisionTree("PDCNameInput");
            else
                TraverseDecisionTree("HistorianInput");
        }

        /// <summary>
        /// Validates the PDC Acronym text box before continuing to the next page.
        /// </summary>
        private void PDCNameInput_OK(object sender, RoutedEventArgs e)
        {
            ReadOnlyObservableCollection<ValidationError> errors;
            IEnumerable<string> errorMessages;
            string errorMessage;

            if (System.Windows.Controls.Validation.GetHasError(PdcAcronymTextBox))
            {
                errors = System.Windows.Controls.Validation.GetErrors(PdcAcronymTextBox);
                errorMessages = errors.Select(error => error.ErrorContent).OfType<string>();
                errorMessage = string.Join(Environment.NewLine, errorMessages);

                MessageBox.Show(errorMessage, "PDC Acronym Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PdcAcronymTextBox.Focus();
            }
            else
            {
                TraverseDecisionTree("HistorianInput");
            }
        }

        /// <summary>
        /// Closes the window when the walkthrough is finished.
        /// </summary>
        private void FinishWalkthrough_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Goes back one page in the page history when the Back button is clicked.
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ReverseDecisionTree(1);
        }

        #endregion

        #endregion
    }
}
