//******************************************************************************************************
//  MainWindow.xaml.cs - Gbtc
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
//  03/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.IO;
using GSF.Reflection;
using GSF.Security;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Serialization;

namespace TsfManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ResizableWindow
    {
        #region [ Members ]

        // Fields
        private ObservableCollection<MenuDataItem> m_menuDataItems;
        private WindowsServiceClient m_windowsServiceClient;
        private LinkedList<TextBlock> m_navigationList;
        private LinkedListNode<TextBlock> m_currentNode;
        private AlarmMonitor m_alarmMonitor;
        private bool m_navigationProcessed;

        #endregion

        #region [ Properties ]

        public ObservableCollection<MenuDataItem> MenuDataItems
        {
            get
            {
                return m_menuDataItems;
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
            Title = ((App)Application.Current).Title;
            TextBoxTitle.Text = AssemblyInfo.EntryAssembly.Title;

            CommonFunctions.CurrentUser = Thread.CurrentPrincipal.Identity.Name;
            CommonFunctions.CurrentPrincipal = Thread.CurrentPrincipal as SecurityPrincipal;

            if (!string.IsNullOrEmpty(CommonFunctions.CurrentUser))
                Title += " Current User: " + CommonFunctions.CurrentUser;

            CommonFunctions.SetRetryServiceConnection(true);

            CommonFunctions.ServiceConnectionRefreshed += CommonFunctions_ServiceConnectionRefreshed;

            m_navigationProcessed = false;
            m_navigationList = new LinkedList<TextBlock>();
            FrameContent.Navigated += new NavigatedEventHandler(FrameContent_Navigated);
        }

        #endregion

        #region [ Methods ]

        private void CommonFunctions_ServiceConnectionRefreshed(object sender, EventArgs e)
        {
            try
            {
                KeyValuePair<Guid, string> currentNode = (KeyValuePair<Guid, string>)ComboboxNode.SelectedItem;
                ComboboxNode.ItemsSource = Node.GetLookupList(null);
                if (ComboboxNode.Items.Count > 0)
                {
                    ComboboxNode.SelectedItem = currentNode;
                }
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
            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("MenuDataItems");
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MenuDataItem>), xmlRootAttribute);

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
        private void ComboboxNode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ((App)Application.Current).NodeID = ((KeyValuePair<Guid, string>)ComboboxNode.SelectedItem).Key;
            m_menuDataItems[0].Command.Execute(null);
            ConnectToService();
        }

        private void ConnectToService()
        {
            if (m_windowsServiceClient != null)
            {
                try
                {
                    m_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished -= RemotingClient_ConnectionEstablished;
                    m_windowsServiceClient.Helper.RemotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;
                }
                catch
                {
                }
            }

            m_windowsServiceClient = CommonFunctions.GetWindowsServiceClient();

            if (m_windowsServiceClient != null)
            {
                m_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished += RemotingClient_ConnectionEstablished;
                m_windowsServiceClient.Helper.RemotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;

                if (m_windowsServiceClient.Helper.RemotingClient.CurrentState == GSF.Communication.ClientState.Connected)
                {
                    EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        EllipseConnectionState.Fill = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                        ToolTipService.SetToolTip(EllipseConnectionState, "Connected to the service");
                    });
                }
                else
                {
                    EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        EllipseConnectionState.Fill = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                        ToolTipService.SetToolTip(EllipseConnectionState, "Disconnected from the service");
                    });
                }
            }
        }

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs e)
        {
            EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
                {
                    EllipseConnectionState.Fill = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                    ToolTipService.SetToolTip(EllipseConnectionState, "Disconnected from the service");
                });
        }

        private void RemotingClient_ConnectionEstablished(object sender, EventArgs e)
        {
            EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
                {
                    EllipseConnectionState.Fill = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                    ToolTipService.SetToolTip(EllipseConnectionState, "Connected to the service");
                });
        }

        private void FrameContent_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                if (!m_navigationProcessed)
                {
                    if (m_currentNode != null)
                    {
                        while (m_currentNode.Next != null)
                            m_navigationList.Remove(m_currentNode.Next.Value);
                    }
                    m_navigationList.AddLast((TextBlock)GroupBoxMain.Header);
                    m_currentNode = m_navigationList.Last;
                }
            }
            catch
            {
            }
            finally
            {
                updateButtons();
                m_navigationProcessed = false;
            }
        }

        private void updateButtons()
        {

            if (FrameContent.CanGoBack)
            {
                backDisabled.Visibility = Visibility.Collapsed;
                backEnabled.Visibility = Visibility.Visible;
            }
            else
            {
                backEnabled.Visibility = Visibility.Collapsed;
                backDisabled.Visibility = Visibility.Visible;
            }

            if (FrameContent.CanGoForward)
            {
                forwardDisabled.Visibility = Visibility.Collapsed;
                forwardEnabled.Visibility = Visibility.Visible;
            }
            else
            {
                forwardEnabled.Visibility = Visibility.Collapsed;
                forwardDisabled.Visibility = Visibility.Visible;
            }

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (FrameContent.CanGoBack)
            {
                m_currentNode = m_currentNode.Previous;
                m_navigationProcessed = true;
                FrameContent.GoBack();
                if (m_currentNode != null)
                    GroupBoxMain.Header = m_currentNode.Value;
            }
        }

        private void ButtonForward_Click(object sender, RoutedEventArgs e)
        {
            if (FrameContent.CanGoForward)
            {
                m_currentNode = m_currentNode.Next;
                m_navigationProcessed = true;
                FrameContent.GoForward();
                if (m_currentNode != null)
                    GroupBoxMain.Header = m_currentNode.Value;
            }
        }

        #endregion
    }
}
