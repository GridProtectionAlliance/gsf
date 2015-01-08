//******************************************************************************************************
//  CorrectnessReportUserControl.xaml.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  01/08/2015 - Jeff Walker
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using GSF.Communication;
using GSF.ServiceProcess;
using GSF.TimeSeries.UI.ViewModels;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for CorrectnessReportUserControl.xaml
    /// </summary>
    public partial class CorrectnessReportUserControl : UserControl
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CorrectnessReportUserControl"/> class.
        /// </summary>
        public CorrectnessReportUserControl()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        private CorrectnessReportViewModel ViewModel
        {
            get
            {
                return Resources["ViewModel"] as CorrectnessReportViewModel;
            }
        }

        #endregion

        #region [ Methods ]

        private void CorrectnessReportUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach to service connected events
            CommonFunctions.ServiceConnectionRefreshed += CommonFunctions_ServiceConnectionRefreshed;

            // Determine initial state of connectivity
            UpdateServiceConnectivity();
        }

        private void CorrectnessReportUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Detach from service connected events
            CommonFunctions.ServiceConnectionRefreshed -= CommonFunctions_ServiceConnectionRefreshed;

            if ((object)ViewModel != null)
                ViewModel.Dispose();
        }

        private void CommonFunctions_ServiceConnectionRefreshed(object sender, EventArgs eventArgs)
        {
            // Determine new state of connectivity
            UpdateServiceConnectivity();
        }

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs eventArgs)
        {
            IClient remotingClient = sender as IClient;

            // Attempt to detach from the event that just occurred
            if ((object)remotingClient != null)
                remotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;

            // Determine new state of connectivity
            UpdateServiceConnectivity();
        }

        private void UpdateServiceConnectivity()
        {
            WindowsServiceClient serviceClient = CommonFunctions.GetWindowsServiceClient();
            ClientHelper clientHelper = ((object)serviceClient != null) ? serviceClient.Helper : null;
            IClient remotingClient = ((object)clientHelper != null) ? clientHelper.RemotingClient : null;

            if ((object)remotingClient == null || remotingClient.CurrentState != ClientState.Connected)
            {
                // If remoting client is not connected, make the message visible
                Dispatcher.BeginInvoke(new Action(() => ViewModel.ServiceConnected = false));
            }
            else
            {
                // If remoting client is connected, hide the message and attach to connection terminated event
                remotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;
                Dispatcher.BeginInvoke(new Action(() => ViewModel.ServiceConnected = true));
            }
        }

        #endregion
    }
}
