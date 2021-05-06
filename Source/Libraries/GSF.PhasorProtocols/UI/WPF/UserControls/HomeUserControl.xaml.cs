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
//  30/07/2012 - Aniket Salver
//       Remembers the last graph selection. 
//  05/05/2021 - J.Ritchie Carroll
//       Added new alarm/warning stats row.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using GSF.Communication;
using GSF.ComponentModel;
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
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

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for HomeUserControl.xaml
    /// </summary>
    public partial class HomeUserControl
    {
        #region [ Members ]

        // Nested Types
        private class CustomActionAdapter
        {
            [PrimaryKey(true)]
            public int ID { get; set; }

            public string AdapterName { get; set; }

            public string AssemblyName { get; set; }

            public string ConnectionString { get; set; }

            [UpdateValueExpression("DateTime.UtcNow")]
            public DateTime UpdatedOn { get; set; }

            [UpdateValueExpression("UserInfo.CurrentUserID")]
            public string UpdatedBy { get; set; }
        }

        // Constants
        private const string SystemSettings = CommonFunctions.DefaultSettingsCategory;

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
        private bool m_serverIsLocal;
        private bool m_timeReasonabilityPopupIsForLocalTime;
        private double m_lastLocalToServerTimeDeviation;
        private double m_lastLocalTimeDeviation;
        private double m_lastServerTimeDeviation;

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
            {
                ButtonInputWizard.IsEnabled = false;
                ApplyToServer.IsEnabled = false;
                ApplyToServer.Foreground = new SolidColorBrush(Colors.Gray);
                ServerTimeLabel.Cursor = null;
            }

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
            InitializeTimeReasonabilitySettings();
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

        // Recursively finds menu item to navigate to when a button is clicked on the UI.
        private void GetMenuDataItem(ObservableCollection<MenuDataItem> dataItems, string stringToMatch, ref MenuDataItem item)
        {
            foreach (MenuDataItem dataItem in dataItems)
            {
                if (string.Equals(dataItem.UserControlPath, stringToMatch, StringComparison.OrdinalIgnoreCase))
                {
                    item = dataItem;
                    break;
                }

                if (dataItem.SubMenuItems.Count > 0)
                    GetMenuDataItem(dataItem.SubMenuItems, stringToMatch, ref item);
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
                            string trimEnd(string value)
                            {
                                const int MaxLength = 80;

                                if (string.IsNullOrEmpty(value))
                                    return "";

                                return value.Length <= MaxLength ? value : string.Concat(value.Substring(0, MaxLength - 3), "...");
                            }

                            string[] lines = e.Argument.Message.TrimEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            TextBlockStatus.Text = string.Join(Environment.NewLine, lines.Select(trimEnd));
                            GroupBoxStatus.Header = $"System Status (Last Refreshed: {DateTime.Now:HH:mm:ss.fff})";
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
                                string[] currentServiceTimes = lines[0].Split(',');

                                if (currentServiceTimes.Length > 0)
                                {
                                    string currentServiceTime = currentServiceTimes[0].Substring(currentServiceTimes[0].ToLower().LastIndexOf("system time:", StringComparison.Ordinal) + 12).Trim();

                                    if (DateTime.TryParse(currentServiceTime, out DateTime serviceTime) && DateTime.TryParse(TextBlockLocalTime.Text, out DateTime localTime))
                                        m_lastLocalToServerTimeDeviation = new Ticks(localTime.Ticks - serviceTime.Ticks).ToSeconds();
                                    
                                    TextBlockServerTime.Text = currentServiceTime;
                                }
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void LocalTimeLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double timeDeviation = m_lastLocalTimeDeviation;

            if (double.IsNaN(timeDeviation) || double.IsInfinity(timeDeviation) || timeDeviation == default)
                timeDeviation = s_timeAlarm;

            double lagTime = Math.Ceiling(timeDeviation * 1.2D);
            double leadTime = Math.Ceiling(lagTime * 1.2D);

            LagTime.Text = lagTime.ToString("N2");
            LeadTime.Text = leadTime.ToString("N2");

            TimeReasonabilityGroupBox.Header = "Change Local Time Reasonability Parameters";
            m_timeReasonabilityPopupIsForLocalTime = true;

            ApplyToManager.Visibility = m_serverIsLocal ? Visibility.Visible : Visibility.Collapsed;
            ApplyToManager.IsChecked = true;

            ApplyToServer.Visibility = m_serverIsLocal ? Visibility.Visible : Visibility.Collapsed;
            ApplyToServer.IsChecked = !m_serverIsLocal;

            LabelManagerUpdateNote.Visibility = Visibility.Visible;
            LabelServerUpdateNote.Visibility = Visibility.Collapsed;

            TimeReasonabilityPopup.IsOpen = true;
            LagTime.Focus();
        }

        private void LocalTimeLabel_MouseEnter(object sender, MouseEventArgs e) =>
            LocalTimeLabel.FontWeight = FontWeights.Bold;

        private void LocalTimeLabel_OnMouseLeave(object sender, MouseEventArgs e) =>
            LocalTimeLabel.FontWeight = FontWeights.Normal;

        private void ServerTimeLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!ApplyToServer.IsEnabled)
                return;

            double timeDeviation = m_lastServerTimeDeviation;

            if (double.IsNaN(timeDeviation) || double.IsInfinity(timeDeviation) || timeDeviation == default)
                timeDeviation = s_timeAlarm;

            double lagTime = Math.Ceiling(timeDeviation * 1.2D);
            double leadTime = Math.Ceiling(lagTime * 1.2D);

            LagTime.Text = lagTime.ToString("N2");
            LeadTime.Text = leadTime.ToString("N2");

            TimeReasonabilityGroupBox.Header = "Change Server Time Reasonability Parameters";
            m_timeReasonabilityPopupIsForLocalTime = false;

            ApplyToManager.Visibility = Visibility.Collapsed;
            ApplyToManager.IsChecked = true;

            ApplyToServer.Visibility = Visibility.Collapsed;
            ApplyToServer.IsChecked = true;

            LabelManagerUpdateNote.Visibility = Visibility.Collapsed;
            LabelServerUpdateNote.Visibility = Visibility.Visible;

            TimeReasonabilityPopup.IsOpen = true;
            LagTime.Focus();
        }

        private void ServerTimeLabel_MouseEnter(object sender, MouseEventArgs e) =>
            ServerTimeLabel.FontWeight = ApplyToServer.IsEnabled ? FontWeights.Bold : FontWeights.Normal;

        private void ServerTimeLabel_OnMouseLeave(object sender, MouseEventArgs e) =>
            ServerTimeLabel.FontWeight = FontWeights.Normal;

        private void ApplyToManager_OnChecked(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LabelManagerUpdateNote.Visibility = m_timeReasonabilityPopupIsForLocalTime ? Visibility.Visible : Visibility.Collapsed;
                VerifyButtonApplyEnabledState();
            }));
        }

        private void ApplyToManager_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LabelManagerUpdateNote.Visibility = Visibility.Collapsed;
                VerifyButtonApplyEnabledState();
            }));
        }

        private void ApplyToServer_OnChecked(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LabelServerUpdateNote.Visibility = !m_timeReasonabilityPopupIsForLocalTime || m_serverIsLocal ? Visibility.Visible : Visibility.Collapsed;
                VerifyButtonApplyEnabledState();
            }));
        }

        private void ApplyToServer_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LabelServerUpdateNote.Visibility = Visibility.Collapsed;
                VerifyButtonApplyEnabledState();
            }));
        }

        private void VerifyButtonApplyEnabledState() => 
            ButtonApply.IsEnabled = ApplyToManager.IsChecked.GetValueOrDefault() || ApplyToServer.IsChecked.GetValueOrDefault();

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e) =>
            TimeReasonabilityPopup.IsOpen = false;

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            TimeReasonabilityPopup.IsOpen = false;

            if (!double.TryParse(LagTime.Text, out double lagTime) || !double.TryParse(LeadTime.Text, out double leadTime))
            {
                MessageBox.Show($"Failed to parse lag time \"{LagTime.Text}\" and/or lead time \"{LeadTime.Text}\" as a floating point value", "Time Reasonability Parse Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string lagTimeText = lagTime.ToString(CultureInfo.InvariantCulture);
                string leadTimeText = leadTime.ToString(CultureInfo.InvariantCulture);

                if (m_timeReasonabilityPopupIsForLocalTime && ApplyToManager.IsChecked.GetValueOrDefault())
                {
                    try
                    {
                        // Update warning and alarm thresholds in manager configuration file
                        s_timeWarning = lagTime * 0.75D;
                        s_timeAlarm = lagTime;

                        // Load home screen threshold settings
                        ConfigurationFile config = ConfigurationFile.Current;
                        CategorizedSettingsElementCollection settings = config.Settings["HomeScreenThresholds"];

                        // Make sure needed settings exist
                        settings.Add("TimeWarning", s_timeWarning, "Local clock time deviation warning threshold, in seconds.");
                        settings.Add("TimeAlarm", s_timeAlarm, "Local clock time deviation alarm threshold, in seconds.");

                        // Update settings
                        settings["TimeWarning"].Update(s_timeWarning);
                        settings["TimeAlarm"].Update(s_timeAlarm);

                        // Save changes
                        config.Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update manager warning and alarm configuration thresholds: {ex.Message}", "Time Reasonability Manager Update Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    try
                    {
                        // Update internal lead/lag time settings used by manager screens
                        IsolatedStorageManager.WriteToIsolatedStorage("LagTime", lagTimeText);
                        IsolatedStorageManager.WriteToIsolatedStorage("LeadTime", leadTimeText);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update manager lead/lag times in isolated storage: {ex.Message}", "Time Reasonability Manager Update Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                if (ApplyToServer.IsEnabled && (!m_timeReasonabilityPopupIsForLocalTime || m_serverIsLocal) && ApplyToServer.IsChecked.GetValueOrDefault())
                {
                    try
                    {
                        // Define common custom action adapter assemblies for which to not apply lead/lag time updates
                        HashSet<string> excludedAssemblies = new HashSet<string>(new[]
                        { 
                            "GSF.TimeSeries.dll", 
                            "DataQualityMonitoring.dll", 
                            "FileAdapters.dll", 
                            "sttp.gsf.dll"
                        },
                        StringComparer.OrdinalIgnoreCase);

                        // Update lead/lag time values in connection strings for each custom action adapter
                        using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                        {
                            TableOperations<CustomActionAdapter> actionAdapterTable = new TableOperations<CustomActionAdapter>(database);

                            foreach (CustomActionAdapter actionAdapter in actionAdapterTable.QueryRecords())
                            {
                                if (excludedAssemblies.Contains(actionAdapter.AssemblyName))
                                    continue;

                                string connectionString = actionAdapter.ConnectionString;

                                if (string.IsNullOrWhiteSpace(connectionString))
                                    continue;

                                Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
                                
                                settings["LagTime"] = lagTimeText;
                                settings["LeadTime"] = leadTimeText;

                                actionAdapter.ConnectionString = settings.JoinKeyValuePairs();
                                actionAdapterTable.UpdateRecord(actionAdapter);

                                try
                                {
                                    if (m_windowsServiceClient?.Helper.RemotingClient.CurrentState == ClientState.Connected)
                                        CommonFunctions.SendCommandToService($"Init {actionAdapter.AdapterName}");
                                }
                                catch (Exception ex)
                                {
                                    Logger.SwallowException(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update host service custom action adapters lag/lead times: {ex.Message}", "Time Reasonability Server Update Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    try
                    {
                        // Update default lead/lag time config file values for calculations
                        XAttribute getElementValue(XElement[] elements, string name)
                        {
                            XElement element = elements.Elements("add").FirstOrDefault(elem =>
                                elem.Attributes("name").Any(nameAttribute => 
                                string.Compare(nameAttribute.Value, name, StringComparison.OrdinalIgnoreCase) == 0));

                            return element?.Attributes("value").FirstOrDefault();
                        }

                        string configPath = FilePath.GetAbsolutePath(ConfigurationFile.Current.Configuration.FilePath.Replace("Manager", ""));

                        if (File.Exists(configPath))
                        {
                            if (UserHasWriteAccess())
                            {
                                XDocument hostConfig = XDocument.Load(configPath);
                                XElement[] systemSettings = hostConfig.Descendants(SystemSettings).ToArray();

                                getElementValue(systemSettings, "DefaultCalculationLagTime").Value = lagTimeText;
                                getElementValue(systemSettings, "DefaultCalculationLeadTime").Value = leadTimeText;

                                hostConfig.Save(configPath);
                            }
                            else if (MessageBox.Show($"All custom action adapter lag/lead times were successfully updated, however, elevated privileges are required in order to update the service configuration file with new default calculation lag/lead time values.{Environment.NewLine}{Environment.NewLine}Do you want to relaunch the manager with elevated privileges so this task can be completed?", "Time Reasonability Server Update Requires Elevation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                if (!(Application.Current?.MainWindow is null))
                                    Application.Current.MainWindow.Visibility = Visibility.Hidden;

                                ProcessStartInfo startInfo = new ProcessStartInfo
                                {
                                    FileName = Environment.GetCommandLineArgs()[0],
                                    Arguments = $"{string.Join(" ", Environment.GetCommandLineArgs().Skip(1))} -elevated",
                                    UseShellExecute = true,
                                    Verb = "runas"
                                };

                                using (Process.Start(startInfo)) { }
                                Environment.Exit(0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update host service configuration default calculation lag/lead times: {ex.Message}", "Time Reasonability Server Update Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private bool UserHasWriteAccess()
        {
            try
            {
                // Validate that user has write access to the local folder
                string tempFile = FilePath.GetAbsolutePath(Guid.NewGuid() + ".tmp");

                using (File.Create(tempFile))
                {
                }

                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                return true;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            return false;
        }

        private void SetTimeReasonabilityLabelFontSizes(double fontSize)
        {
            LabelCPULabel.FontSize = LabelCPUValue.FontSize = 
            LabelMemoryLabel.FontSize = LabelMemoryValue.FontSize = 
            LabelDiskLabel.FontSize = LabelDiskValue.FontSize =
            LabelLocalTimeLabel.FontSize = LabelLocalTimeValue.FontSize =
            LabelServerTimeLabel.FontSize = LabelServerTimeValue.FontSize = 
                fontSize;
        }

        private void SetTimeReasonabilityLabelDefaults()
        {
            CPUValue.Text = "...%";
            UpdateHighlighting(CPULabel, CPUValue, s_defaultBrush, null, null);

            MemoryValue.Text = "...%";
            UpdateHighlighting(MemoryLabel, MemoryValue, s_defaultBrush, null, null);

            DiskValue.Text = "...%";
            UpdateHighlighting(DiskLabel, DiskValue, s_defaultBrush, null, null);

            LocalTimeValue.Text = $"... {(m_serverIsLocal ? "seconds" : "secs")}";
            UpdateHighlighting(LocalTimeLabel, LocalTimeValue, s_defaultBrush, s_underlineStyle, null);

            ServerTimeValue.Text = $"... {(m_serverIsLocal ? "seconds" : "secs")}";
            UpdateHighlighting(ServerTimeLabel, ServerTimeValue, s_defaultBrush, ApplyToServer.IsEnabled ? s_underlineStyle : null, null);
        }

        private void InitializeTimeReasonabilitySettings()
        {
            if (m_serverIsLocal)
            {
                LabelServerTimeSeparator.Visibility = Visibility.Collapsed;
                LabelServerTimeLabel.Visibility = Visibility.Collapsed;
                LabelServerTimeValue.Visibility = Visibility.Collapsed;
                CPULabel.Text = "System CPU Usage:";
                MemoryLabel.Text = "System Memory Usage:";
                DiskLabel.Text = "Primary Disk Usage:";
                SetTimeReasonabilityLabelFontSizes(13);
            }
            else
            {
                LabelServerTimeSeparator.Visibility = Visibility.Visible;
                LabelServerTimeLabel.Visibility = Visibility.Visible;
                LabelServerTimeValue.Visibility = Visibility.Visible;
                CPULabel.Text = "Sys CPU Usage:";
                MemoryLabel.Text = "Sys Mem Usage:";
                DiskLabel.Text = "Prim Disk Usage:";
                SetTimeReasonabilityLabelFontSizes(11);
            }

            SetTimeReasonabilityLabelDefaults();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Fetch the real key
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            
            // Check for Ctrl+Shift+S to toggle server local mode
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && 
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) &&
                key == Key.S)
            {
                TimeReasonabilityPopup.IsOpen = false;
                m_serverIsLocal = !m_serverIsLocal;
                InitializeTimeReasonabilitySettings();
            }
        }

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
                        string connectionString = database.DataPublisherConnectionString();

                        Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

                        if (settings.TryGetValue("server", out string server))
                        {
                            try
                            {
                                string[] parts = server.Split(':');

                                if (parts.Length > 1)
                                    server = parts[0];

                                m_serverIsLocal = Transport.IsLocalAddress(server);
                            }
                            catch (Exception ex)
                            {
                                Logger.SwallowException(ex);
                                m_serverIsLocal = false;
                            }
                        }
                        
                        m_statsSubscription = new DataSubscriber();
                        m_statsSubscription.ConnectionEstablished += StatsSubscriptionConnectionEstablished;
                        m_statsSubscription.NewMeasurements += StatsSubscriptionNewMeasurements;
                        m_statsSubscription.ConnectionTerminated += StatsSubscriptionConnectionTerminated;
                        m_statsSubscription.ConnectionString = connectionString;
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

        private void StatsSubscriptionConnectionTerminated(object sender, EventArgs e) => 
            Dispatcher.BeginInvoke(new Action(SetTimeReasonabilityLabelDefaults));

        private void StatsSubscriptionNewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            // [0] System CPU Usage
            // [1] System Memory Usage
            // [2] System Time Deviation
            // [3] Primary Disk Usage

            foreach (IMeasurement stat in e.Argument)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Guid signalID = stat.ID;
                    double value = stat.AdjustedValue;
                    string formattedValue = double.IsNaN(value) ? "..." : $"{value:N2}";

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
                        ServerTimeValue.Text = $"{formattedValue} {(m_serverIsLocal ? "seconds" : "secs")}";
                        m_lastServerTimeDeviation = Math.Abs(value);

                        if (m_lastServerTimeDeviation > s_timeAlarm)
                            UpdateHighlighting(ServerTimeLabel, ServerTimeValue, s_alarmBrush, ApplyToServer.IsEnabled ? s_underlineShadowStyle : s_shadowStyle, s_shadowStyle);
                        else if (m_lastServerTimeDeviation > s_timeWarning)
                            UpdateHighlighting(ServerTimeLabel, ServerTimeValue, s_warningBrush, ApplyToServer.IsEnabled ? s_underlineShadowStyle : s_shadowStyle, s_shadowStyle);
                        else
                            UpdateHighlighting(ServerTimeLabel, ServerTimeValue, s_defaultBrush, ApplyToServer.IsEnabled ? s_underlineStyle : null, null);

                        // Calculate local time deviation
                        value += m_lastLocalToServerTimeDeviation;
                        m_lastLocalTimeDeviation = Math.Abs(value);
                        formattedValue = double.IsNaN(value) ? "..." : $"{value:N2}";
                        LocalTimeValue.Text = $"{formattedValue} {(m_serverIsLocal ? "seconds" : "secs")}";

                        if (m_lastLocalTimeDeviation > s_timeAlarm)
                            UpdateHighlighting(LocalTimeLabel, LocalTimeValue, s_alarmBrush, s_underlineShadowStyle, s_shadowStyle);
                        else if (m_lastLocalTimeDeviation > s_timeWarning)
                            UpdateHighlighting(LocalTimeLabel, LocalTimeValue, s_warningBrush, s_underlineShadowStyle, s_shadowStyle);
                        else
                            UpdateHighlighting(LocalTimeLabel, LocalTimeValue, s_defaultBrush, s_underlineStyle, null);
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
        private static double s_timeWarning = 3.0D;
        private static double s_timeAlarm = 5.0D;
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
