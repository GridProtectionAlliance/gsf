//******************************************************************************************************
//  MainWindow.xaml.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  03/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using GSF.Communication;
using GSF.IO;
using GSF.Reflection;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;

namespace TsfManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region [ Members ]

        // Fields
        private ObservableCollection<MenuDataItem> m_menuDataItems;
        private WindowsServiceClient m_windowsServiceClient;
        private AlarmMonitor m_alarmMonitor;
        private Guid m_selectedNodeId;

        #endregion

        #region [ Properties ]

        public ObservableCollection<MenuDataItem> MenuDataItems => m_menuDataItems;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            if (SecurityPrincipal is null)
            {
                Environment.Exit(403);
                return;
            }

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
            Title = ((App)Application.Current).Title;
            TextBoxTitle.Text = AssemblyInfo.EntryAssembly.Title;

            CommonFunctions.CurrentPrincipal = SecurityPrincipal;
            Title += " - " + SecurityPrincipal.Identity.Provider.UserData.LoginID;

            CommonFunctions.SetRetryServiceConnection(true);
            CommonFunctions.ServiceConnectionRefreshed += CommonFunctions_ServiceConnectionRefreshed;
            CommonFunctions.CanGoForwardChanged += (_, _) => ForwardButton.IsEnabled = CommonFunctions.CanGoForward;
            CommonFunctions.CanGoBackChanged += (_, _) => BackButton.IsEnabled = CommonFunctions.CanGoBack;
        }

        #endregion

        #region [ Methods ]

        private void CommonFunctions_ServiceConnectionRefreshed(object sender, EventArgs e)
        {
            try
            {
                KeyValuePair<Guid, string> currentNode = (KeyValuePair<Guid, string>)ComboboxNode.SelectedItem;
                m_selectedNodeId = currentNode.Key;

                Dictionary<Guid, string> updatedNodeList = Node.GetLookupList(null);
                ComboboxNode.ItemsSource = updatedNodeList;

                if (ComboboxNode.Items.Count <= 0)
                    return;

                if (!updatedNodeList.ContainsKey(m_selectedNodeId))
                    return;

                KeyValuePair<Guid, string> first = new();

                foreach (KeyValuePair<Guid, string> pair in updatedNodeList.Where(pair => pair.Key == m_selectedNodeId))
                {
                    first = pair;
                    break;
                }

                ComboboxNode.SelectedItem = first;
            }
            finally
            {
                ConnectToService();
            }
        }

        /// <summary>
        /// Method to handle window loaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Menu
            XmlRootAttribute xmlRootAttribute = new("MenuDataItems");
            XmlSerializer serializer = new(typeof(ObservableCollection<MenuDataItem>), xmlRootAttribute);

            using (XmlReader reader = XmlReader.Create(FilePath.GetAbsolutePath("Menu.xml")))
            {
                m_menuDataItems = (ObservableCollection<MenuDataItem>)serializer.Deserialize(reader);
            }

            MenuMain.DataContext = m_menuDataItems;

            // Populate Node Dropdown
            ComboboxNode.ItemsSource = Node.GetLookupList(null);
            if (ComboboxNode.Items.Count > 0)
                ComboboxNode.SelectedIndex = 0;

            // Create alarm monitor as singleton
            m_alarmMonitor = new AlarmMonitor(true);
            m_alarmMonitor.Start();

            IsolatedStorageManager.InitializeIsolatedStorage(false);
        }

        /// <summary>
        /// Method to handle window unloaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonFunctions.SetRetryServiceConnection(false);
            m_alarmMonitor.Dispose();
        }

        /// <summary>
        /// Handles selectionchanged event on node selection combobox.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event argument.</param>
        private void ComboboxNode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((App)Application.Current).NodeID = string.IsNullOrEmpty(ComboboxNode.SelectionBoxItem.ToString()) ? 
                ((KeyValuePair<Guid, string>)ComboboxNode.SelectedItem).Key : 
                ((KeyValuePair<Guid, string>)ComboboxNode.SelectionBoxItem).Key;

            m_menuDataItems[0].Command.Execute(null);
        }

        private void ConnectToService()
        {
            if (m_windowsServiceClient != null && m_windowsServiceClient.Helper is not null && m_windowsServiceClient.Helper.RemotingClient is not null)
            {
                m_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished -= RemotingClient_ConnectionEstablished;
                m_windowsServiceClient.Helper.RemotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;
            }

            m_windowsServiceClient = CommonFunctions.GetWindowsServiceClient();

            if (m_windowsServiceClient is null)
                return;

            m_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished += RemotingClient_ConnectionEstablished;
            m_windowsServiceClient.Helper.RemotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;

            if (m_windowsServiceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
            {
                EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate
                {
                    EllipseConnectionState.Fill = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                    ToolTipService.SetToolTip(EllipseConnectionState, "Connected to the service");
                });
            }
            else
            {
                EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate
                {
                    EllipseConnectionState.Fill = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                    ToolTipService.SetToolTip(EllipseConnectionState, "Disconnected from the service");
                });
            }
        }

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs e)
        {
            EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate
            {
                EllipseConnectionState.Fill = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                ToolTipService.SetToolTip(EllipseConnectionState, "Disconnected from the service");
            });
        }

        private void RemotingClient_ConnectionEstablished(object sender, EventArgs e)
        {
            EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate
            {
                EllipseConnectionState.Fill = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                ToolTipService.SetToolTip(EllipseConnectionState, "Connected to the service");
            });
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e) => 
            CommonFunctions.GoBack();

        private void ButtonForward_Click(object sender, RoutedEventArgs e) => 
            CommonFunctions.GoForward();

        #endregion
    }
}
