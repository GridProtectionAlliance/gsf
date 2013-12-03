//******************************************************************************************************
//  MonitoringUserControl.xaml.cs - Gbtc
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
//  06/08/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GSF.Communication;
using GSF.ServiceProcess;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for MonitoringUserControl.xaml
    /// </summary>
    public partial class MonitoringUserControl : UserControl
    {
        #region [ Members ]

        // Fields
        private WindowsServiceClient m_serviceClient;
        private int m_numberOfMessages;

        // Delegates
        private delegate void DisplayHelper(UpdateType updateType, string message);

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="MonitoringUserControl"/>.
        /// </summary>
        public MonitoringUserControl()
        {
            InitializeComponent();
            this.Loaded += MonitoringUserControl_Loaded;
            this.Unloaded += MonitoringUserControl_Unloaded;
            this.KeyUp += MonitoringUserControl_KeyUp;
            PopupSettings.Closed += PopupSettings_Closed;
        }

        private void PopupSettings_Closed(object sender, EventArgs e)
        {
            TextBoxServiceRequest.Focus();
        }

        #endregion

        #region [ Methods ]

        private void MonitoringUserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && PopupSettings.IsOpen)
                PopupSettings.IsOpen = false;
        }

        /// <summary>
        /// Hanldes loaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MonitoringUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxServiceRequest.Focus();
            SetupServiceConnection();
            if (!int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("NumberOfMessages").ToString(), out m_numberOfMessages))
                m_numberOfMessages = 75;

            TextBoxNumberOfMessages.Text = m_numberOfMessages.ToString();
        }

        /// <summary>
        /// Handles unloaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MonitoringUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (m_serviceClient != null && m_serviceClient.Helper != null)
            {
                m_serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                m_serviceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
            }
        }

        /// <summary>
        /// Connects to backend windows service and attaches to it's events.
        /// </summary>
        private void SetupServiceConnection()
        {
            m_serviceClient = null;
            try
            {
                m_serviceClient = CommonFunctions.GetWindowsServiceClient();
                if (m_serviceClient != null && m_serviceClient.Helper != null &&
                   m_serviceClient.Helper.RemotingClient != null && m_serviceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
                {
                    TextBlockServiceStatus.Dispatcher.BeginInvoke(new DisplayHelper(RefreshStatusText), new object[] { UpdateType.Information, m_serviceClient.CachedStatus });
                    m_serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    m_serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;
                }
            }
            catch (Exception ex)
            {
                TextBlockServiceStatus.Dispatcher.BeginInvoke(new DisplayHelper(RefreshStatusText), new object[] { UpdateType.Alarm, "Failed to connect to windows service." + Environment.NewLine + ex.Message });
            }
        }

        /// <summary>
        /// Handles ReceivedServiceUpdate event of the <see cref="WindowsServiceClient"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments containing <see cref="UpdateType"/> and Message.</param>
        private void Helper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            TextBlockServiceStatus.Dispatcher.BeginInvoke(new DisplayHelper(RefreshStatusText), new object[] { e.Argument1, e.Argument2 });
        }

        /// <summary>
        /// Hanldes ReceivedServiceResponse event of the <see cref="WindowsServiceClient"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Response received from the service.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            string response = e.Argument.Type;
            string message = e.Argument.Message;
            string responseToClient = string.Empty;
            UpdateType responseType = UpdateType.Information;

            if (!string.IsNullOrEmpty(response))
            {
                // Reponse types are formatted as "Command:Success" or "Command:Failure"
                string[] parts = response.Split(':');
                string action;
                bool success;

                if (parts.Length > 1)
                {
                    action = parts[0].Trim().ToTitleCase();
                    success = (string.Compare(parts[1].Trim(), "Success", true) == 0);
                }
                else
                {
                    action = response;
                    success = true;
                }

                if (success)
                {
                    if (string.IsNullOrEmpty(message))
                        responseToClient = string.Format("{0} command processed successfully.\r\n\r\n", action);
                    else
                        responseToClient = string.Format("{0}\r\n\r\n", message);
                }
                else
                {
                    responseType = UpdateType.Alarm;
                    if (string.IsNullOrEmpty(message))
                        responseToClient = string.Format("{0} failure.\r\n\r\n", action);
                    else
                        responseToClient = string.Format("{0} failure: {1}\r\n\r\n", action, message);
                }

                TextBlockServiceStatus.Dispatcher.BeginInvoke(new DisplayHelper(RefreshStatusText), new object[] { responseType, responseToClient });
            }
        }

        /// <summary>
        /// Refreshes status text being displayed on the screen.
        /// </summary>
        /// <param name="updateType">Type of update for the new status message received from backend service.</param>
        /// <param name="message">Actual message received from the backend service.</param>
        private void RefreshStatusText(UpdateType updateType, string message)
        {
            Run run;
            if (updateType == UpdateType.Information)
            {
                run = new Run();
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                run.Text = message;
            }
            else if (updateType == UpdateType.Warning)
            {
                run = new Run();
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                run.Text = message;
            }
            else
            {
                run = new Run();
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 10, 10));
                run.Text = message;
            }

            TextBlockServiceStatus.Inlines.Add(run);
            if (TextBlockServiceStatus.Inlines.Count > m_numberOfMessages)
                TextBlockServiceStatus.Inlines.Remove(TextBlockServiceStatus.Inlines.FirstInline);

            TextBlockServiceStatus.UpdateLayout();
            ScrollViewerMonitor.UpdateLayout(); //this is require to keep scroll-bar at the bottom.
            ScrollViewerMonitor.ScrollToVerticalOffset(TextBlockServiceStatus.ActualHeight * 2);
        }

        /// <summary>
        /// Handles Click event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSendServiceRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxServiceRequest.Text) &&
                m_serviceClient != null && m_serviceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
            {
                CommonFunctions.SendCommandToService(TextBoxServiceRequest.Text);
                TextBoxServiceRequest.Focus();
                TextBoxServiceRequest.SelectAll();
            }
        }

        /// <summary>
        /// Handles GotFocus event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void TextBoxServiceRequest_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxServiceRequest.SelectAll();
        }

        private void ButtonDisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.Placement = PlacementMode.Center;
            PopupSettings.IsOpen = true;
        }

        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageManager.InitializeStorageForRemoteConsole(true);
            if (int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("NumberOfMessages").ToString(), out m_numberOfMessages))
            {
                TextBoxNumberOfMessages.Text = m_numberOfMessages.ToString();
            }
            PopupSettings.IsOpen = false;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = false;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBoxNumberOfMessages.Text, out m_numberOfMessages))
            {
                IsolatedStorageManager.WriteToIsolatedStorage("NumberOfMessages", m_numberOfMessages);
                PopupSettings.IsOpen = false;
            }
            else
            {
                MessageBox.Show("Please provide integer value.", "ERROR: Invalid Value", MessageBoxButton.OK);
            }
        }

        #endregion

    }
}
