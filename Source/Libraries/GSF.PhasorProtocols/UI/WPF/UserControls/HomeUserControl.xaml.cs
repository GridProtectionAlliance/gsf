//******************************************************************************************************
//  HomeUserControl.xaml.cs - Gbtc
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
//  11/09/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
// 30/07/2012 - Aniket Salver
//       Remembers the last graph selection. 
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using GSF.Communication;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Identity;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.Reflection;
using GSF.ServiceProcess;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries.UI;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

#pragma warning disable 612,618

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for HomeUserControl.xaml
    /// </summary>
    public partial class HomeUserControl
    {
        #region [ Members ]

        // Fields
        private readonly ObservableCollection<MenuDataItem> m_menuDataItems;
        private WindowsServiceClient m_windowsServiceClient;
        private DispatcherTimer m_refreshTimer;

        // Subscription fields
        private DataSubscriber m_chartSubscription;
        private DataSubscriber m_statsSubscription;
        private bool m_chartSubscriptionConnected;
        private string m_signalID;
        private int m_processingUnsynchronizedMeasurements;
        private double m_refreshInterval = 0.25;
        private int[] m_xAxisDataCollection;                                // Source data for the binding collection.
        private EnumerableDataSource<int> m_xAxisBindingCollection;         // Values plotted on X-Axis.        
        private ConcurrentQueue<double> m_yAxisDataCollection;              // Source data for the binding collection. Format is <signalID, collection of values from subscription API>.
        private EnumerableDataSource<double> m_yAxisBindingCollection;      // Values plotted on Y-Axis.
        private LineGraph m_lineGraph;
        private int m_numberOfPointsToPlot = 60;
        private bool m_eventHandlerRegistered;
        private Measurement m_selectedMeasurement;
        private Guid[] m_statSignalIDs;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="HomeUserControl"/>.
        /// </summary>
        public HomeUserControl()
        {
            InitializeComponent();
            this.Loaded += HomeUserControl_Loaded;
            this.Unloaded += HomeUserControl_Unloaded;

            // Load Menu
            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("MenuDataItems");
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MenuDataItem>), xmlRootAttribute);

            using (XmlReader reader = XmlReader.Create(FilePath.GetAbsolutePath("Menu.xml")))
            {
                m_menuDataItems = (ObservableCollection<MenuDataItem>)serializer.Deserialize(reader);
            }
        }

        #endregion

        #region [ Methods ]

        private void HomeUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(m_windowsServiceClient?.Helper is null))
                    m_windowsServiceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;

                if (!(m_refreshTimer is null))
                    m_refreshTimer.Stop();

                UnsubscribeChartData();
                DisposeStatsSubscription();
            }
            finally
            {
                m_refreshTimer = null;
                m_chartSubscription = null;
                m_statsSubscription = null;
            }
        }

        private void HomeUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CommonFunctions.CurrentPrincipal.IsInRole("Administrator"))
                ButtonRestart.IsEnabled = false;

            if (!CommonFunctions.CurrentPrincipal.IsInRole("Administrator,Editor"))
                ButtonInputWizard.IsEnabled = false;

            m_windowsServiceClient = CommonFunctions.GetWindowsServiceClient();

            if (m_windowsServiceClient is null || m_windowsServiceClient.Helper.RemotingClient.CurrentState != ClientState.Connected)
            {
                ButtonRestart.IsEnabled = false;
                m_statsSubscription?.Stop();
            }
            else
            {
                m_windowsServiceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                CommonFunctions.SendCommandToService("Health -actionable");
                CommonFunctions.SendCommandToService("Version -actionable");
                CommonFunctions.SendCommandToService("Status -actionable");
                CommonFunctions.SendCommandToService("Time -actionable");
                m_eventHandlerRegistered = true;
            }

            m_refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            m_refreshTimer.Tick += RefreshTimer_Tick;
            m_refreshTimer.Start();

            TextBlockInstance.Text = IntPtr.Size == 8 ? "64-bit" : "32-bit";
            TextBlockLocalTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Initialize default brush
            if (s_defaultBrush is null)
                s_defaultBrush = TextBlockInstance.Background;

            // Get needed styles
            if (s_underlineStyle is null)
                s_underlineStyle = FindResource("Underline") as Style;

            if (s_shadowStyle is null)
                s_shadowStyle = FindResource("Shadow") as Style;

            if (s_underlineShadowStyle is null)
                s_underlineShadowStyle = FindResource("UnderlineShadow") as Style;

            Version appVersion = AssemblyInfo.EntryAssembly.Version;
            TextBlockManagerVersion.Text = $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.0";

            try
            {
                using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    TextBlockDatabaseType.Text = database.DatabaseType.ToString();

                    try
                    {
                        if (database.IsSqlite || database.IsJetEngine)
                        {
                            // Extract database file name from connection string for file centric databases
                            TextBlockDatabaseName.Text = FilePath.GetFileName(database.Connection.ConnectionString.ParseKeyValuePairs()["Data Source"]);
                        }
                        else if (database.IsOracle)
                        {
                            // Extract user name from connection string for Oracle databases
                            TextBlockDatabaseName.Text = database.Connection.ConnectionString.ParseKeyValuePairs()["User Id"];
                        }
                        else
                        {
                            TextBlockDatabaseName.Text = database.Connection.Database;
                        }
                    }
                    catch
                    {
                        // Fall back on database name if file anything fails
                        TextBlockDatabaseName.Text = database.Connection.Database;
                    }
                }
            }
            catch
            {
                TextBlockDatabaseName.Text = "Not Available";
            }

            try
            {
                using (UserInfo info = new UserInfo(CommonFunctions.CurrentUser))
                {
                    TextBlockUser.Text = info.Exists ? info.LoginID : CommonFunctions.CurrentUser;
                }
            }
            catch
            {
                TextBlockUser.Text = CommonFunctions.CurrentUser;
            }

            ((HorizontalAxis)ChartPlotterDynamic.MainHorizontalAxis).LabelProvider.LabelStringFormat = "";

            //Remove legend on the right.
            Panel legendParent = (Panel)ChartPlotterDynamic.Legend.ContentGrid.Parent;

            if (!(legendParent is null))
                legendParent.Children.Remove(ChartPlotterDynamic.Legend.ContentGrid);

            ChartPlotterDynamic.NewLegendVisible = false;

            m_xAxisDataCollection = new int[m_numberOfPointsToPlot];

            for (int i = 0; i < m_numberOfPointsToPlot; i++)
                m_xAxisDataCollection[i] = i;

            m_xAxisBindingCollection = new EnumerableDataSource<int>(m_xAxisDataCollection);
            m_xAxisBindingCollection.SetXMapping(x => x);

            LoadComboBoxDeviceAsync();
            InitializeStatsSubscription();
        }

        private void LoadComboBoxDeviceAsync()
        {
            Thread t = new Thread(() =>
            {
                Dictionary<int, string> deviceLookupList = Device.GetLookupList(null);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ComboBoxDevice.ItemsSource = deviceLookupList;

                    if (Application.Current.Resources.Contains("SelectedDevice_Home"))
                        ComboBoxDevice.SelectedIndex = (int)Application.Current.Resources["SelectedDevice_Home"];
                    else
                        ComboBoxDevice.SelectedIndex = 0;
                }));
            });

            t.IsBackground = true;
            t.Start();
        }

        private void LoadComboBoxMeasurementAsync()
        {
            int selectedDeviceID = ((KeyValuePair<int, string>)ComboBoxDevice.SelectedItem).Key;

            Thread t = new Thread(() =>
            {
                ObservableCollection<Measurement> measurements = Measurement.Load(null, selectedDeviceID);
                ObservableCollection<Measurement> itemsSource = new ObservableCollection<Measurement>(measurements.Where(m => m.SignalSuffix == "PA" || m.SignalSuffix == "FQ"));

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ComboBoxMeasurement.ItemsSource = itemsSource;

                    if (Application.Current.Resources.Contains("SelectedMeasurement_Home") && (int)Application.Current.Resources["SelectedMeasurement_Home"] != -1)
                        ComboBoxMeasurement.SelectedIndex = (int)Application.Current.Resources["SelectedMeasurement_Home"];
                    else
                        ComboBoxMeasurement.SelectedIndex = 0;
                }));
            });

            t.IsBackground = true;
            t.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (!(m_windowsServiceClient?.Helper?.RemotingClient is null) && m_windowsServiceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
            {
                try
                {
                    ButtonRestart.IsEnabled = CommonFunctions.CurrentPrincipal.IsInRole("Administrator");
                    ButtonInputWizard.IsEnabled = CommonFunctions.CurrentPrincipal.IsInRole("Administrator,Editor");

                    if (!m_eventHandlerRegistered)
                    {
                        m_windowsServiceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                        CommonFunctions.SendCommandToService("Version -actionable");
                        m_eventHandlerRegistered = true;
                    }

                    CommonFunctions.SendCommandToService("Health -actionable");
                    CommonFunctions.SendCommandToService("Time -actionable");

                    if (PopupStatus.IsOpen)
                        CommonFunctions.SendCommandToService("Status -actionable");

                    if (!(m_statsSubscription is null) && !m_statsSubscription.Enabled)
                        m_statsSubscription?.Start();
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex);
                }
            }
            else
            {
                ButtonRestart.IsEnabled = false;
            }

            TextBlockLocalTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private void ButtonQuickLink_Click(object sender, RoutedEventArgs e)
        {
            MenuDataItem item = new MenuDataItem();
            string stringToMatch = ((Button)sender).Tag.ToString();

            if (!string.IsNullOrEmpty(stringToMatch))
            {
                if (stringToMatch == "Restart")
                {
                    RestartService();
                }
                else
                {
                    GetMenuDataItem(m_menuDataItems, stringToMatch, ref item);

                    if (!(item.MenuText is null))
                        item.Command.Execute(null);
                }
            }
        }

        /// <summary>
        /// Recursively finds menu item to navigate to when a button is clicked on the UI.
        /// </summary>
        /// <param name="items">Collection of menu items.</param>
        /// <param name="stringToMatch">Item to search for in menu items collection.</param>
        /// <param name="item">Returns a menu item.</param>
        private void GetMenuDataItem(ObservableCollection<MenuDataItem> items, string stringToMatch, ref MenuDataItem item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].UserControlPath.ToLower() == stringToMatch.ToLower())
                {
                    item = items[i];
                    break;
                }
                else
                {
                    if (items[i].SubMenuItems.Count > 0)
                    {
                        GetMenuDataItem(items[i].SubMenuItems, stringToMatch, ref item);
                    }
                }
            }
        }

        private void RestartService()
        {
            try
            {
                if (MessageBox.Show("Do you want to restart service?", "Restart Service", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CommonFunctions.SendCommandToService("Restart");
                    MessageBox.Show("Successfully sent RESTART command to the service.", "Restart Service", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                MessageBox.Show($"Failed sent RESTART command to the service.{Environment.NewLine}Service is either offline or disconnected.", "Restart Service", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            if (ClientHelper.TryParseActionableResponse(e.Argument, out string sourceCommand, out bool _))
            {
                switch (sourceCommand.ToLowerInvariant())
                {
                    case "health":
                        this.Dispatcher.BeginInvoke((Action)delegate
                        {
                            string[] lines = e.Argument.Message.TrimEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            TextBlockSystemHealth.Text = string.Join(Environment.NewLine, lines.Take(21));
                            GroupBoxSystemHealth.Header = $"System Health (Last Refreshed: {DateTime.Now:HH:mm:ss.fff})";
                        });
                        break;
                    case "status":
                        this.Dispatcher.BeginInvoke((Action)delegate
                        {
                            GroupBoxStatus.Header = $"System Status (Last Refreshed: {DateTime.Now:HH:mm:ss.fff})";
                            TextBlockStatus.Text = e.Argument.Message.TrimEnd();
                        });
                        break;
                    case "version":
                        this.Dispatcher.BeginInvoke((Action)delegate
                        {
                            TextBlockVersion.Text = e.Argument.Message.Substring(e.Argument.Message.ToLower().LastIndexOf("version:", StringComparison.Ordinal) + 8).Trim();
                        });
                        break;
                    case "time":
                        this.Dispatcher.BeginInvoke((Action)delegate
                        {
                            string[] lines = e.Argument.Message.TrimEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                            if (lines.Length > 0)
                            {
                                string[] currentTimes = lines[0].Split(',');

                                if (currentTimes.Length > 0)
                                    TextBlockServerTime.Text = currentTimes[0].Substring(currentTimes[0].ToLower().LastIndexOf("system time:", StringComparison.Ordinal) + 12).Trim();
                            }
                        });
                        break;
                }
            }
        }

        private void ButtonStatus_Click(object sender, RoutedEventArgs e)
        {
            PopupStatus.IsOpen = true;

            if (!(m_windowsServiceClient?.Helper?.RemotingClient is null) && m_windowsServiceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
                CommonFunctions.SendCommandToService("Status -actionable");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            PopupStatus.IsOpen = false;
        }

        private void ComboBoxMeasurement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxMeasurement.Items.Count > 0)
            {
                m_selectedMeasurement = (Measurement)ComboBoxMeasurement.SelectedItem;

                // Capture's the Selected index of measurement Combo Box
                if (Application.Current.Resources.Contains("SelectedMeasurement_Home"))
                {
                    Application.Current.Resources.Remove("SelectedMeasurement_Home");
                    Application.Current.Resources.Add("SelectedMeasurement_Home", ComboBoxMeasurement.SelectedIndex);
                }
                else
                    Application.Current.Resources.Add("SelectedMeasurement_Home", ComboBoxMeasurement.SelectedIndex);

                if (!(m_selectedMeasurement is null))
                {
                    m_signalID = m_selectedMeasurement.SignalID.ToString();

                    switch (m_selectedMeasurement.SignalSuffix)
                    {
                        case "PA":
                            ChartPlotterDynamic.Visible = DataRect.Create(0, -180, m_numberOfPointsToPlot, 180);
                            break;
                        case "FQ":
                            {
                                double frequencyMin = Convert.ToDouble(IsolatedStorageManager.ReadFromIsolatedStorage("FrequencyRangeMin"));
                                double frequencyMax = Convert.ToDouble(IsolatedStorageManager.ReadFromIsolatedStorage("FrequencyRangeMax"));

                                ChartPlotterDynamic.Visible = DataRect.Create(0, Math.Min(frequencyMin, frequencyMax), m_numberOfPointsToPlot, Math.Max(frequencyMin, frequencyMax));
                                break;
                            }
                    }
                }
            }
            else
            {
                m_signalID = string.Empty;
            }

            SubscribeChartData();
        }

        private void ComboBoxDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Capture's the Selected index of Device Combo Box
            if (Application.Current.Resources.Contains("SelectedDevice_Home"))
            {
                Application.Current.Resources.Remove("SelectedDevice_Home");
                Application.Current.Resources.Add("SelectedDevice_Home", ComboBoxDevice.SelectedIndex);
            }
            else
            {
                Application.Current.Resources.Add("SelectedDevice_Home", ComboBoxDevice.SelectedIndex);
            }

            LoadComboBoxMeasurementAsync();
        }
        private void TimeLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Clicked!");
        }

        private void TimeLabel_MouseEnter(object sender, MouseEventArgs e) =>
            TimeLabel.FontWeight = FontWeights.Bold;

        private void TimeLabel_OnMouseLeave(object sender, MouseEventArgs e) =>
            TimeLabel.FontWeight = FontWeights.Normal;

        #region [ Chart Subscription ]

        private void ChartSubscriptionConnectionTerminated(object sender, EventArgs e)
        {
            m_chartSubscriptionConnected = false;
            UnsubscribeChartData();

            try
            {
                ChartPlotterDynamic.Dispatcher.BeginInvoke(new Action(() => ChartPlotterDynamic.Children.Remove(m_lineGraph)));
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            InitializeChartSubscription();
        }

        private void ChartSubscriptionNewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (0 != Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 1))
                return;

            try
            {
                foreach (IMeasurement measurement in e.Argument)
                {
                    double tempValue = measurement.AdjustedValue;

                    if (!double.IsNaN(tempValue) && !double.IsInfinity(tempValue)) // Process data only if it is not NaN or infinity.
                    {
                        ChartPlotterDynamic.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                        {
                            if (m_yAxisDataCollection.Count == 0)
                            {
                                for (int i = 0; i < m_numberOfPointsToPlot; i++)
                                    m_yAxisDataCollection.Enqueue(tempValue);

                                m_yAxisBindingCollection = new EnumerableDataSource<double>(m_yAxisDataCollection);
                                m_yAxisBindingCollection.SetYMapping(y => y);

                                m_lineGraph = ChartPlotterDynamic.AddLineGraph(new CompositeDataSource(m_xAxisBindingCollection, m_yAxisBindingCollection), Color.FromArgb(255, 25, 25, 200), 1, string.Empty);

                            }
                            else
                            {
                                if (m_yAxisDataCollection.TryDequeue(out double _))
                                    m_yAxisDataCollection.Enqueue(tempValue);
                            }
                            m_yAxisBindingCollection.RaiseDataChanged();
                        });
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 0);
            }
        }

        private void ChartSubscriptionConnectionEstablished(object sender, EventArgs e)
        {
            m_chartSubscriptionConnected = true;
            SubscribeChartData();
        }

        private void InitializeChartSubscription()
        {
            try
            {
                using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    m_chartSubscription = new DataSubscriber();
                    m_chartSubscription.ConnectionEstablished += ChartSubscriptionConnectionEstablished;
                    m_chartSubscription.NewMeasurements += ChartSubscriptionNewMeasurements;
                    m_chartSubscription.ConnectionTerminated += ChartSubscriptionConnectionTerminated;
                    m_chartSubscription.ConnectionString = database.DataPublisherConnectionString();
                    m_chartSubscription.Initialize();
                    m_chartSubscription.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize subscription.{Environment.NewLine}{ex.Message}", "Failed to Subscribe", MessageBoxButton.OK);
            }
        }

        private void DisposeChartSubscription()
        {
            if (m_chartSubscription is null)
                return;

            m_chartSubscription.ConnectionEstablished -= ChartSubscriptionConnectionEstablished;
            m_chartSubscription.NewMeasurements -= ChartSubscriptionNewMeasurements;
            m_chartSubscription.ConnectionTerminated -= ChartSubscriptionConnectionTerminated;
            m_chartSubscription.Stop();
            m_chartSubscription.Dispose();
            m_chartSubscription = null;
        }

        private void SubscribeChartData()
        {
            if (m_chartSubscription is null)
                InitializeChartSubscription();

            try
            {
                ChartPlotterDynamic.Dispatcher.BeginInvoke(new Action(() => ChartPlotterDynamic.Children.Remove(m_lineGraph)));
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            m_yAxisDataCollection = new ConcurrentQueue<double>();

            if (m_chartSubscriptionConnected && m_chartSubscription != null && !string.IsNullOrEmpty(m_signalID))
                m_chartSubscription.UnsynchronizedSubscribe(true, true, m_signalID, null, true, m_refreshInterval);
        }

        private void UnsubscribeChartData()
        {
            try
            {
                if (m_chartSubscription != null)
                {
                    m_chartSubscription.Unsubscribe();
                    DisposeChartSubscription();
                }
            }
            catch
            {
                m_chartSubscription = null;
            }
        }

        #endregion

        #region [ Stats Subscription ]

        private void InitializeStatsSubscription()
        {
            try
            {
                if (m_statsSubscription is null)
                {
                    using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                    {
                        m_statsSubscription = new DataSubscriber();
                        m_statsSubscription.ConnectionEstablished += StatsSubscriptionConnectionEstablished;
                        m_statsSubscription.NewMeasurements += StatsSubscriptionNewMeasurements;
                        m_statsSubscription.ConnectionTerminated += StatsSubscriptionConnectionTerminated;
                        m_statsSubscription.ConnectionString = database.DataPublisherConnectionString();
                        m_statsSubscription.Initialize();
                        m_statsSubscription.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        private void StatsSubscriptionConnectionEstablished(object sender, EventArgs e)
        {
            const string StatsFilterExpression = "SignalAcronym = 'STAT' AND (" +
                                                 "SignalReference LIKE '%SYSTEM-ST16' OR " + // [0] System CPU Usage
                                                 "SignalReference LIKE '%SYSTEM-ST20' OR " + // [1] System Memory Usage
                                                 "SignalReference LIKE '%SYSTEM-ST24' OR " + // [2] System Time Deviation
                                                 "SignalReference LIKE '%SYSTEM-ST25')";     // [3] Primary Disk Usage

            m_statSignalIDs = Measurement.LoadSignalIDs(null, StatsFilterExpression, "SignalReference").ToArray();
            m_statsSubscription.UnsynchronizedSubscribe(true, true, string.Join(";", m_statSignalIDs), lagTime: 5.0D);
        }

        private void StatsSubscriptionConnectionTerminated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CPUValue.Text = "...%";
                UpdateHighlighting(CPULabel, CPUValue, s_defaultBrush, null, null);

                MemoryValue.Text = "...%";
                UpdateHighlighting(MemoryLabel, MemoryValue, s_defaultBrush, null, null);

                DiskValue.Text = "...%";
                UpdateHighlighting(DiskLabel, DiskValue, s_defaultBrush, null, null);

                TimeValue.Text = "... seconds";
                UpdateHighlighting(TimeLabel, TimeValue, s_defaultBrush, s_underlineStyle, null);
            }));
        }

        private void StatsSubscriptionNewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            // [0] System CPU Usage
            // [1] System Memory Usage
            // [2] System Time Deviation From Average
            // [3] Primary Disk Usage

            foreach (IMeasurement stat in e.Argument)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    double value = stat.AdjustedValue;
                    string formattedValue = double.IsNaN(value) ? "..." : $"{value:N2}";

                    Guid signalID = stat.ID;

                    if (signalID == m_statSignalIDs[0])
                    {
                        CPUValue.Text = $"{formattedValue}%";

                        if (value > s_cpuAlarm)
                            UpdateHighlighting(CPULabel, CPUValue, s_alarmBrush, s_shadowStyle, s_shadowStyle);
                        else if (value > s_cpuWarning)
                            UpdateHighlighting(CPULabel, CPUValue, s_warningBrush, s_shadowStyle, s_shadowStyle);
                        else
                            UpdateHighlighting(CPULabel, CPUValue, s_defaultBrush, null, null);
                    }
                    else if (signalID == m_statSignalIDs[1])
                    {
                        MemoryValue.Text = $"{formattedValue}%";

                        if (value > s_memoryAlarm)
                            UpdateHighlighting(MemoryLabel, MemoryValue, s_alarmBrush, s_shadowStyle, s_shadowStyle);
                        else if (value > s_memoryWarning)
                            UpdateHighlighting(MemoryLabel, MemoryValue, s_warningBrush, s_shadowStyle, s_shadowStyle);
                        else
                            UpdateHighlighting(MemoryLabel, MemoryValue, s_defaultBrush, null, null);
                    }
                    else if (signalID == m_statSignalIDs[2])
                    {
                        TimeValue.Text = $"{formattedValue} seconds";

                        if (Math.Abs(value) > s_timeAlarm)
                            UpdateHighlighting(TimeLabel, TimeValue, s_alarmBrush, s_underlineShadowStyle, s_shadowStyle);
                        else if (Math.Abs(value) > s_timeWarning)
                            UpdateHighlighting(TimeLabel, TimeValue, s_warningBrush, s_underlineShadowStyle, s_shadowStyle);
                        else
                            UpdateHighlighting(TimeLabel, TimeValue, s_defaultBrush, s_underlineStyle, null);
                    }
                    else if (signalID == m_statSignalIDs[3])
                    {
                        DiskValue.Text = $"{formattedValue}%";

                        if (value > s_diskAlarm)
                            UpdateHighlighting(DiskLabel, DiskValue, s_alarmBrush, s_shadowStyle, s_shadowStyle);
                        else if (value > s_diskWarning)
                            UpdateHighlighting(DiskLabel, DiskValue, s_warningBrush, s_shadowStyle, s_shadowStyle);
                        else
                            UpdateHighlighting(DiskLabel, DiskValue, s_defaultBrush, null, null);
                    }
                }));
            }
        }

        private void DisposeStatsSubscription()
        {
            if (m_statsSubscription is null)
                return;

            m_statsSubscription.Stop();
            m_statsSubscription.ConnectionEstablished -= StatsSubscriptionConnectionEstablished;
            m_statsSubscription.NewMeasurements -= StatsSubscriptionNewMeasurements;
            m_statsSubscription.ConnectionTerminated -= StatsSubscriptionConnectionTerminated;
            m_statsSubscription.Dispose();
            m_statsSubscription = null;
        }

        #endregion

        #endregion

        #region [ Static ]

        private const string DefaultWarningBrush = "#FFCAD43E";
        private const string DefaultAlarmBrush = "#FFFF1111";

        // Static Fields
        private static readonly double s_cpuWarning = 75.0D;
        private static readonly double s_cpuAlarm = 95.0D;
        private static readonly double s_memoryWarning = 75.0D;
        private static readonly double s_memoryAlarm = 95.0D;
        private static readonly double s_diskWarning = 75.0D;
        private static readonly double s_diskAlarm = 95.0D;
        private static readonly double s_timeWarning = 3.0D;
        private static readonly double s_timeAlarm = 5.0D;
        private static readonly SolidColorBrush s_warningBrush;
        private static readonly SolidColorBrush s_alarmBrush;
        private static Brush s_defaultBrush;
        private static Style s_underlineStyle;
        private static Style s_shadowStyle;
        private static Style s_underlineShadowStyle;

        // Static Constructor
        static HomeUserControl()
        {
            try
            {
                // Load home screen threshold settings
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings["HomeScreenThresholds"];

                // Make sure needed settings exist
                settings.Add("CPUWarning", s_cpuWarning, "CPU utilization warning threshold, in percent.");
                settings.Add("CPUAlarm", s_cpuAlarm, "CPU utilization alarm threshold, in percent.");
                settings.Add("MemoryWarning", s_memoryWarning, "Memory utilization warning threshold, in percent.");
                settings.Add("MemoryAlarm", s_memoryAlarm, "Memory utilization alarm threshold, in percent.");
                settings.Add("DiskWarning", s_diskWarning, "Primary disk utilization warning threshold, in percent.");
                settings.Add("DiskAlarm", s_diskAlarm, "Primary disk utilization alarm threshold, in percent.");
                settings.Add("TimeWarning", s_timeWarning, "Local clock time deviation warning threshold, in seconds.");
                settings.Add("TimeAlarm", s_timeAlarm, "Local clock time deviation alarm threshold, in seconds.");
                settings.Add("WarningColor", DefaultWarningBrush, "Background color for warning labels, commonly yellow.");
                settings.Add("AlarmColor", DefaultAlarmBrush, "Background color for alarm labels, commonly red.");

                // Load configured settings
                s_cpuWarning = settings["CPUWarning"].ValueAs(s_cpuWarning);
                s_cpuAlarm = settings["CPUAlarm"].ValueAs(s_cpuAlarm);
                s_memoryWarning = settings["MemoryWarning"].ValueAs(s_memoryWarning);
                s_memoryAlarm = settings["MemoryAlarm"].ValueAs(s_memoryAlarm);
                s_diskWarning = settings["DiskWarning"].ValueAs(s_diskWarning);
                s_diskAlarm = settings["DiskAlarm"].ValueAs(s_diskAlarm);
                s_timeWarning = settings["TimeWarning"].ValueAs(s_timeWarning);
                s_timeAlarm = settings["TimeAlarm"].ValueAs(s_timeAlarm);
                s_warningBrush = new SolidColorBrush((Color)(ColorConverter.ConvertFromString(settings["WarningColor"].ValueAs(DefaultWarningBrush)) ?? Colors.Yellow));
                s_alarmBrush = new SolidColorBrush((Color)(ColorConverter.ConvertFromString(settings["AlarmColor"].ValueAs(DefaultWarningBrush)) ?? Colors.Red));

                // Save changes, if any
                config.Save();
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        // Static Methods
        private static void UpdateHighlighting(TextBlock statLabel, TextBlock statValue, Brush color, Style statLabelStyle, Style statValueStyle)
        {
            void setParentLabelBackground(TextBlock textBlock)
            {
                if (textBlock.Parent is Label label)
                    label.Background = color;
            }

            setParentLabelBackground(statLabel);
            statLabel.Style = statLabelStyle;

            setParentLabelBackground(statValue);
            statValue.Style = statValueStyle;
        }

        #endregion
    }
}
