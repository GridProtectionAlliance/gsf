//******************************************************************************************************
//  InputStatusMonitorUserControl.xaml.cs - Gbtc
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
//  08/15/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GSF.Data;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.ViewModels;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.UserControls;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

#pragma warning disable 612,618

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for InputStatusMonitorUserControl.xaml
    /// </summary>
    public partial class InputStatusMonitorUserControl
    {
        #region [ Members ]

        // Fields
        private RealTimeStreams m_dataContext;
        private List<Color> m_lineColors;

        // Synchronized subscription fields
        private bool m_restartConnectionCycle;
        private DataSubscriber m_subscriber;
        private bool m_subscribedSynchronized;
        private DispatcherTimer m_refreshTimer;
        private string m_selectedSignalIDs;
        private int m_processingSynchronizedMeasurements;

        private int[] m_xAxisDataCollection;                                                            // Source data for the binding collection.
        private EnumerableDataSource<int> m_xAxisBindingCollection;                                     // Values plotted on X-Axis.
        private ConcurrentQueue<string> m_timeStampList;
        private ConcurrentDictionary<Guid, ConcurrentQueue<double>> m_yAxisDataCollection;              // Source data for the binding collection. Format is <signalID, collection of values from subscription API>.
        private ConcurrentDictionary<Guid, EnumerableDataSource<double>> m_yAxisBindingCollection;      // Values plotted on Y-Axis.
        private ConcurrentDictionary<Guid, LineGraph> m_lineGraphCollection;                            // List of graphs plotted on the chart.
        private ConcurrentDictionary<Guid, RealTimeMeasurement> m_selectedMeasurements;                 // Measurements user have selected to plot.
        private ObservableCollection<RealTimeMeasurement> m_displayedMeasurement;

        private int m_numberOfDataPointsToPlot = 30;
        private int m_framesPerSecond = 30;                                                             // Sample rate of the data from subscription API/Data Resolution
        private double m_leadTime = 1.0;
        private double m_lagTime = 3.0;
        private bool m_useLocalClockAsRealtime = true;
        private bool m_ignoreBadTimestamps;
        private int m_chartRefreshInterval = 66;
        private int m_statisticsDataRefershInterval = 10;
        private int m_measurementsDataRefreshInterval = 10;
        private double m_frequencyRangeMin = 59.95;
        private double m_frequencyRangeMax = 60.05;
        private bool m_displayFrequencyYAxis;
        private bool m_displayPhaseAngleYAxis;
        private bool m_displayVoltageYAxis;
        private bool m_displayCurrentYAxis;
        private bool m_displayXAxis;
        private bool m_displayLegend;
        private long m_refreshRate = Ticks.FromMilliseconds(500);                                       // This is used to refresh real-time values below chart.
        private long m_lastRefreshTime;
        private bool m_historicalPlayback;
        private bool m_waitingForData;
        private bool m_forceIPv4 = true;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="InputStatusMonitorUserControl"/>.
        /// </summary>
        public InputStatusMonitorUserControl()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += InputStatusMonitorUserControl_Loaded;
            this.Unloaded += InputStatusMonitorUserControl_Unloaded;
            this.KeyUp += InputStatusMonitorUserControl_KeyUp;
        }

        #endregion

        #region [ Methods ]

        private void InputStatusMonitorUserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && PopupSettings.IsOpen)
                PopupSettings.IsOpen = false;
        }

        private void InputStatusMonitorUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_restartConnectionCycle = false;
            m_dataContext.RestartConnectionCycle = false;
            Unsubscribe();
            m_dataContext.UnsubscribeUnsynchronizedData();
            IsolatedStorageManager.WriteToIsolatedStorage("InputMonitoringPoints", m_selectedSignalIDs);
        }

        /// <summary>
        /// Handles loaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void InputStatusMonitorUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_historicalPlayback = false;
            m_waitingForData = false;
            ModeMessage.Text = "Real-time";
            ChartPlotterDynamic.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            InitializeUserControl();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RealTimeMeasurement measurement = (RealTimeMeasurement)((CheckBox)sender).DataContext;
            m_selectedMeasurements.TryAdd(measurement.SignalID, measurement);
            RefreshSelectedMeasurements();
            AddToDisplayedMeasurements(measurement);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RealTimeMeasurement measurement = (RealTimeMeasurement)((CheckBox)sender).DataContext;
            m_selectedMeasurements.TryRemove(measurement.SignalID, out measurement);
            RemoveLineGraph(measurement);
            RefreshSelectedMeasurements();
            RemoveFromDisplayedMeasurements(measurement);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save Current Display Settings";
                saveDialog.Filter = "Input Status Monitoring Display Settings (*.ismsettings)|*.ismsettings|All Files (*.*)|*.*";
                bool? result = saveDialog.ShowDialog(Window.GetWindow(this));
                if (result.GetValueOrDefault())
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                        writer.Write(m_selectedSignalIDs);
                }
            }
            catch (Exception)
            {
                // TODO: Turn this into popup error
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Multiselect = false;
                openDialog.Filter = "Input Status Monitoring Display Settings (*.ismsettings)|*.ismsettings|All Files (*.*)|*.*";
                bool? result = openDialog.ShowDialog(Window.GetWindow(this));
                if (result.GetValueOrDefault())
                {
                    using (StreamReader reader = new StreamReader(openDialog.OpenFile()))
                    {
                        string selection = reader.ReadLine().ToNonNullString();
                        string[] signalIDs = selection.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        AutoSelectMeasurements(signalIDs);
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Turn this into popup error
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void AddToDisplayedMeasurements(RealTimeMeasurement measurement)
        {
            bool addMeasurement = true;
            foreach (RealTimeMeasurement m in m_displayedMeasurement)
            {
                if (m.SignalID == measurement.SignalID)
                {
                    addMeasurement = false;
                    break;
                }
            }

            if (addMeasurement)
                m_displayedMeasurement.Add(measurement);
        }

        private void RemoveFromDisplayedMeasurements(RealTimeMeasurement measurement)
        {
            bool removeMeasurement = false;
            foreach (RealTimeMeasurement m in m_displayedMeasurement)
            {
                if (m.SignalID == measurement.SignalID)
                {
                    removeMeasurement = true;
                    break;
                }
            }

            if (removeMeasurement)
                m_displayedMeasurement.Remove(measurement);
        }

        private void Initialize()
        {
            RetrieveSettingsFromIsolatedStorage(false);
            m_timeStampList = new ConcurrentQueue<string>();
            m_yAxisDataCollection = new ConcurrentDictionary<Guid, ConcurrentQueue<double>>();
            m_yAxisBindingCollection = new ConcurrentDictionary<Guid, EnumerableDataSource<double>>();
            m_lineGraphCollection = new ConcurrentDictionary<Guid, LineGraph>();
            m_selectedMeasurements = new ConcurrentDictionary<Guid, RealTimeMeasurement>();
            m_displayedMeasurement = new ObservableCollection<RealTimeMeasurement>();
            m_displayedMeasurement.CollectionChanged += m_displayedMeasurement_CollectionChanged;
            m_restartConnectionCycle = true;
            m_xAxisDataCollection = new int[m_numberOfDataPointsToPlot];
            m_refreshRate = Ticks.FromMilliseconds(m_chartRefreshInterval);
            TextBlockMeasurementRefreshInterval.Text = m_measurementsDataRefreshInterval.ToString();
            TextBlockStatisticsRefreshInterval.Text = m_statisticsDataRefershInterval.ToString();
            TextBoxProcessInterval.Text = "33";
        }

        private void m_displayedMeasurement_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                    ((RealTimeMeasurement)item).Selected = false;
            }
        }

        private void InitializeUserControl()
        {
            m_dataContext = new RealTimeStreams(1, m_measurementsDataRefreshInterval);
            this.DataContext = m_dataContext;
            //ListBoxCurrentValues.ItemsSource = m_displayedMeasurement;
            DataGridCurrentValues.ItemsSource = m_displayedMeasurement;

            // Initialize Chart Properties.
            InitializeColors();
            InitializeChart();

            m_selectedSignalIDs = IsolatedStorageManager.ReadFromIsolatedStorage("InputMonitoringPoints").ToString();
            string[] signalIDs = m_selectedSignalIDs.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            AutoSelectMeasurements(signalIDs);

            PopulateSettings();

            foreach (RealTimeStream stream in m_dataContext.ItemsSource)
            {
                if (stream.ID > 0)
                {
                    GetStatistics(stream.Acronym);
                    break;
                }

                foreach (RealTimeDevice device in stream.DeviceList)
                {
                    if (device.ID > 0)
                    {
                        GetStatistics(device.Acronym);
                        break;
                    }
                }
            }
        }

        private void InitializeColors()
        {
            m_lineColors = new List<Color>();
            m_lineColors.Add(Colors.DarkGoldenrod);
            m_lineColors.Add(Colors.Blue);
            m_lineColors.Add(Colors.Green);
            m_lineColors.Add(Colors.Red);
            m_lineColors.Add(Colors.Purple);
            m_lineColors.Add(Colors.Brown);
            m_lineColors.Add(Colors.Magenta);
            m_lineColors.Add(Colors.Black);
            m_lineColors.Add(Colors.DarkCyan);
            m_lineColors.Add(Colors.Coral);
        }

        private void InitializeChart()
        {
            ((HorizontalAxis)ChartPlotterDynamic.MainHorizontalAxis).LabelProvider.LabelStringFormat = "";

            //Remove legend on the right.
            Panel legendParent = (Panel)ChartPlotterDynamic.Legend.ContentGrid.Parent;
            if (legendParent != null)
                legendParent.Children.Remove(ChartPlotterDynamic.Legend.ContentGrid);

            ChartPlotterDynamic.NewLegendVisible = m_displayLegend;
            ChartPlotterDynamic.MainVerticalAxisVisibility = FrequencyAxisTitle.Visibility = m_displayFrequencyYAxis ? Visibility.Visible : Visibility.Collapsed;
            ChartPlotterDynamic.MainHorizontalAxisVisibility = m_displayXAxis ? Visibility.Visible : Visibility.Collapsed;
            PhaseAngleYAxis.Visibility = PhaseAngleAxisTitle.Visibility = m_displayPhaseAngleYAxis ? Visibility.Visible : Visibility.Collapsed;
            VoltageYAxis.Visibility = VoltageAxisTitle.Visibility = m_displayVoltageYAxis ? Visibility.Visible : Visibility.Collapsed;
            CurrentYAxis.Visibility = CurrentAxisTitle.Visibility = m_displayCurrentYAxis ? Visibility.Visible : Visibility.Collapsed;

            // This check was added as there was an error reported where it was infinity.
            if (m_frequencyRangeMin.IsInfinite() || m_frequencyRangeMin.IsNaN())
                m_frequencyRangeMin = 59.95;

            if (m_frequencyRangeMax.IsInfinite() || m_frequencyRangeMax.IsNaN())
                m_frequencyRangeMax = 60.05;

            // DataRect.Create didn't like it when the min > max.
            if (m_frequencyRangeMin > m_frequencyRangeMax)
            {
                double temp = m_frequencyRangeMin;
                m_frequencyRangeMin = m_frequencyRangeMax;
                m_frequencyRangeMax = temp;
            }

            ChartPlotterDynamic.Visible = DataRect.Create(0, m_frequencyRangeMin, m_numberOfDataPointsToPlot, m_frequencyRangeMax);
            PhaseAnglePlotter.Visible = DataRect.Create(0, -180, m_numberOfDataPointsToPlot, 180);
            TextBlockLeft.Visibility = TextBlockRight.Visibility = ChartPlotterDynamic.MainHorizontalAxisVisibility;

            for (int i = 0; i < m_numberOfDataPointsToPlot; i++)
                m_xAxisDataCollection[i] = i;

            m_xAxisBindingCollection = new EnumerableDataSource<int>(m_xAxisDataCollection);
            m_xAxisBindingCollection.SetXMapping(x => x);
        }

        private void RetrieveSettingsFromIsolatedStorage(bool reloadSelectedMeasurements = true)
        {
            // Retrieve values from IsolatedStorage.            
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("NumberOfDataPointsToPlot").ToString(), out m_numberOfDataPointsToPlot);
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("DataResolution").ToString(), out m_framesPerSecond);
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("ChartRefreshInterval").ToString(), out m_chartRefreshInterval);
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("StatisticsDataRefreshInterval").ToString(), out m_statisticsDataRefershInterval);
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("MeasurementsDataRefreshInterval").ToString(), out m_measurementsDataRefreshInterval);
            double.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("LagTime").ToString(), out m_lagTime);
            double.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("LeadTime").ToString(), out m_leadTime);
            double.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("FrequencyRangeMin").ToString(), out m_frequencyRangeMin);
            double.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("FrequencyRangeMax").ToString(), out m_frequencyRangeMax);
            m_forceIPv4 = IsolatedStorageManager.ReadFromIsolatedStorage("ForceIPv4").ToString().ParseBoolean();
            m_displayXAxis = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayXAxis").ToString().ParseBoolean();
            m_displayLegend = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayLegend").ToString().ParseBoolean();
            m_displayFrequencyYAxis = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayFrequencyYAxis").ToString().ParseBoolean();
            m_displayPhaseAngleYAxis = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayPhaseAngleYAxis").ToString().ParseBoolean();
            m_displayCurrentYAxis = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayCurrentYAxis").ToString().ParseBoolean();
            m_displayVoltageYAxis = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayVoltageYAxis").ToString().ParseBoolean();
            m_useLocalClockAsRealtime = IsolatedStorageManager.ReadFromIsolatedStorage("UseLocalClockAsRealtime").ToString().ParseBoolean();
            m_ignoreBadTimestamps = IsolatedStorageManager.ReadFromIsolatedStorage("IgnoreBadTimestamps").ToString().ParseBoolean();
            ValidateSettingsAfterRead();

            if (reloadSelectedMeasurements)
            {
                m_selectedSignalIDs = IsolatedStorageManager.ReadFromIsolatedStorage("InputMonitoringPoints").ToString();
                string[] signalIDs = m_selectedSignalIDs.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                AutoSelectMeasurements(signalIDs);
            }
        }

        /// <summary>
        /// Makes sure the user doesn't enter values that will crash the system.
        /// </summary>
        private void ValidateSettingsAfterRead()
        {
            m_numberOfDataPointsToPlot = Math.Abs(m_numberOfDataPointsToPlot);
            //m_framesPerSecond = Math.Abs(m_framesPerSecond);
            //m_lagTime = Math.Abs(m_lagTime);
            //m_leadTime = Math.Abs(m_leadTime);
            //m_chartRefreshInterval = Math.Abs(m_chartRefreshInterval);
            //m_statisticsDataRefershInterval = Math.Abs(m_statisticsDataRefershInterval);
            //m_measurementsDataRefreshInterval = Math.Abs(m_measurementsDataRefreshInterval);
            m_frequencyRangeMin = Math.Abs(m_frequencyRangeMin);
            m_frequencyRangeMax = Math.Abs(m_frequencyRangeMax);

            if (m_frequencyRangeMin > m_frequencyRangeMax)
            {
                double temp = m_frequencyRangeMin;
                m_frequencyRangeMin = m_frequencyRangeMax;
                m_frequencyRangeMax = temp;
            }
        }

        private void AutoSelectMeasurements(string[] signalIDs)
        {
            if (signalIDs.Any())
            {
                foreach (RealTimeStream stream in m_dataContext.ItemsSource)
                {
                    stream.Expanded = false;
                    foreach (RealTimeDevice device in stream.DeviceList)
                    {
                        device.Expanded = false;
                        foreach (RealTimeMeasurement measurement in device.MeasurementList)
                        {
                            if (signalIDs.Contains(measurement.SignalID.ToString()))
                            {
                                measurement.Selected = true;
                                device.Expanded = true;
                                stream.Expanded = true;
                                m_dataContext.Expanded = true;
                                m_selectedMeasurements.TryAdd(measurement.SignalID, measurement);
                                AddToDisplayedMeasurements(measurement);
                            }
                            else
                            {
                                measurement.Selected = false;
                                RemoveFromDisplayedMeasurements(measurement);
                                RealTimeMeasurement tempMeasurement;
                                m_selectedMeasurements.TryRemove(measurement.SignalID, out tempMeasurement);
                                RemoveLineGraph(measurement);
                            }
                        }
                    }
                }

                if (m_selectedMeasurements.Count > 0)
                    Subscribe();
            }
        }

        private void RefreshSelectedMeasurements()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<Guid, RealTimeMeasurement> measurement in m_selectedMeasurements)
            {
                sb.Append(measurement.Value.SignalID);
                sb.Append(";");
            }

            m_selectedSignalIDs = sb.ToString();
            if (m_selectedSignalIDs.Length > 0)
                m_selectedSignalIDs = m_selectedSignalIDs.Substring(0, m_selectedSignalIDs.Length - 1);

            // once user has changed selection, resubscribe with new values.
            Subscribe();
        }

        private void StartRefreshTimer()
        {
            if (m_refreshTimer == null)
            {
                m_refreshTimer = new DispatcherTimer();
                m_refreshTimer.Interval = TimeSpan.FromMilliseconds(m_chartRefreshInterval);
                m_refreshTimer.Tick += m_refreshTimer_Tick;
                m_refreshTimer.Start();
            }
        }

        private void m_refreshTimer_Tick(object sender, EventArgs e)
        {
            foreach (KeyValuePair<Guid, EnumerableDataSource<double>> keyValuePair in m_yAxisBindingCollection)
            {
                try
                {
                    lock (keyValuePair.Value)
                        keyValuePair.Value.RaiseDataChanged();
                }
                catch
                {
                }
            }

            if (m_timeStampList.Count > 0)
            {
                TextBlockLeft.Text = m_timeStampList.First();
                TextBlockRight.Text = m_timeStampList.Last();
            }
        }

        private void StopRefreshTimer()
        {
            try
            {
                if (m_refreshTimer != null)
                {
                    m_refreshTimer.Stop();
                    m_refreshTimer.Tick -= m_refreshTimer_Tick;
                }
            }
            finally
            {
                m_refreshTimer = null;
            }
        }

        private void RemoveLineGraph(RealTimeMeasurement measurement)
        {
            LineGraph lineGraphToBeRemoved;
            EnumerableDataSource<double> bindingCollectionToBeRemoved;
            ConcurrentQueue<double> dataCollectionToBeRemoved;
            measurement.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            m_yAxisBindingCollection.TryRemove(measurement.SignalID, out bindingCollectionToBeRemoved);
            m_yAxisDataCollection.TryRemove(measurement.SignalID, out dataCollectionToBeRemoved);
            if (m_lineGraphCollection.TryRemove(measurement.SignalID, out lineGraphToBeRemoved))
            {
                if (measurement.SignalAcronym == "FREQ")
                    ChartPlotterDynamic.Children.Remove((IPlotterElement)lineGraphToBeRemoved);
                else if (measurement.SignalAcronym == "IPHA" || measurement.SignalAcronym == "VPHA")
                    PhaseAnglePlotter.Children.Remove((IPlotterElement)lineGraphToBeRemoved);
                else if (measurement.SignalAcronym == "VPHM")
                    VoltagePlotter.Children.Remove((IPlotterElement)lineGraphToBeRemoved);
                else if (measurement.SignalAcronym == "IPHM")
                    CurrentPlotter.Children.Remove((IPlotterElement)lineGraphToBeRemoved);
            }
        }

        private void ButtonGetStatistics_Click(object sender, RoutedEventArgs e)
        {
            GetStatistics(((Button)sender).Tag.ToString());
        }

        private void GetStatistics(string acronym)
        {
            Device device = Device.GetDevice(null, "WHERE Acronym = '" + acronym + "'");
            if (device != null)
            {
                TextBlockDevice.Text = device.Acronym;
                m_dataContext.GetStatistics(device);
                //ListBoxStatistics.ItemsSource = m_dataContext.StatisticMeasurements;
                DataGridStatistics.ItemsSource = m_dataContext.StatisticMeasurements;
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Device device = Device.GetDevice(null, "WHERE Acronym = '" + ((Button)sender).Tag + "'");
            CommonFunctions.LoadUserControl("Manage Device Configuration", typeof(DeviceUserControl), device);
        }

        private void ButtonMeasurement_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = sender as DependencyObject;

            TreeViewItem item;
            RealTimeMeasurement realTimeMeasurement;
            Measurement measurement;

            PhasorMeasurementUserControl phasorMeasurementUserControl;

            while ((object)obj != null && obj.GetType() != typeof(TreeViewItem))
                obj = VisualTreeHelper.GetParent(obj);

            item = (TreeViewItem)obj;
            realTimeMeasurement = (RealTimeMeasurement)item?.DataContext;
            measurement = Measurement.GetMeasurement(null, $"WHERE SignalReference = '{realTimeMeasurement?.SignalReference}'");

            if ((object)measurement != null && measurement.DeviceID.HasValue)
            {
                phasorMeasurementUserControl = CommonFunctions.LoadUserControl("Manage Measurements for " + measurement.DeviceAcronym, typeof(PhasorMeasurementUserControl), (int)measurement.DeviceID) as PhasorMeasurementUserControl;

                if (phasorMeasurementUserControl != null)
                    ((PhasorMeasurements)phasorMeasurementUserControl.DataContext).CurrentItem = measurement;
            }
        }

        private void m_subscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            m_subscribedSynchronized = false;
            Unsubscribe();
            if (m_restartConnectionCycle)
                InitializeSubscription();
        }

        private void m_subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (0 == Interlocked.Exchange(ref m_processingSynchronizedMeasurements, 1))
            {
                try
                {
                    if (m_historicalPlayback && m_waitingForData)
                    {
                        m_waitingForData = false;

                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ModeMessage.Text = "Historical";
                        }));
                    }

                    bool refreshMeasurementValueBelowChart = false;
                    if (DateTime.UtcNow.Ticks - m_lastRefreshTime > m_refreshRate)
                    {
                        m_lastRefreshTime = DateTime.UtcNow.Ticks;
                        refreshMeasurementValueBelowChart = true;
                    }

                    foreach (IMeasurement newMeasurement in e.Argument)
                    {
                        m_timeStampList.Enqueue(newMeasurement.Timestamp.ToString("HH:mm:ss.fff"));

                        if (m_timeStampList.Count > m_numberOfDataPointsToPlot)
                        {
                            string oldValue;
                            m_timeStampList.TryDequeue(out oldValue);
                        }

                        double tempValue = newMeasurement.AdjustedValue;
                        Guid tempSignalID = newMeasurement.ID;
                        if (!double.IsNaN(tempValue) && !double.IsInfinity(tempValue)) // Process data only if it is not NaN or infinity.
                        {
                            ConcurrentQueue<double> tempValueCollection;
                            if (m_yAxisDataCollection.TryGetValue(tempSignalID, out tempValueCollection)) // If value collection already exists, then just replace oldest value with newest.
                            {
                                double oldValue;
                                if (tempValueCollection.TryDequeue(out oldValue))
                                    tempValueCollection.Enqueue(tempValue);
                            }
                            else // It is probably a new measurement user wants to subscribe to.
                            {
                                RealTimeMeasurement measurement;

                                // Check if user has selected this measurement. Because user may have unselected this but request may not have been processed completely.
                                if (m_selectedMeasurements.TryGetValue(tempSignalID, out measurement))
                                {
                                    tempValueCollection = new ConcurrentQueue<double>();
                                    for (int i = 0; i < m_numberOfDataPointsToPlot; i++)
                                        tempValueCollection.Enqueue(tempValue);

                                    m_yAxisDataCollection.TryAdd(tempSignalID, tempValueCollection);
                                    EnumerableDataSource<double> tempDataSource = new EnumerableDataSource<double>(tempValueCollection);
                                    m_yAxisBindingCollection.TryAdd(tempSignalID, tempDataSource);
                                    tempDataSource.SetYMapping(y => y);

                                    ChartPlotterDynamic.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                                        {
                                            int colorIndex;
                                            Math.DivRem(m_yAxisBindingCollection.Count, 10, out colorIndex);
                                            LineGraph lineGraph = null;

                                            if (measurement.SignalAcronym == "FREQ")
                                                lineGraph = ChartPlotterDynamic.AddLineGraph(new CompositeDataSource(m_xAxisBindingCollection, tempDataSource), m_lineColors[colorIndex], 1, measurement.SignalReference);
                                            else if (measurement.SignalAcronym == "IPHA" || measurement.SignalAcronym == "VPHA")
                                                lineGraph = PhaseAnglePlotter.AddLineGraph(new CompositeDataSource(m_xAxisBindingCollection, tempDataSource), m_lineColors[colorIndex], 1, measurement.SignalReference);
                                            else if (measurement.SignalAcronym == "VPHM")
                                                lineGraph = VoltagePlotter.AddLineGraph(new CompositeDataSource(m_xAxisBindingCollection, tempDataSource), m_lineColors[colorIndex], 1, measurement.SignalReference);
                                            else if (measurement.SignalAcronym == "IPHM")
                                                lineGraph = CurrentPlotter.AddLineGraph(new CompositeDataSource(m_xAxisBindingCollection, tempDataSource), m_lineColors[colorIndex], 1, measurement.SignalReference);

                                            if ((object)lineGraph != null)
                                            {
                                                m_lineGraphCollection.TryAdd(tempSignalID, lineGraph);
                                                measurement.Foreground = (SolidColorBrush)lineGraph.LinePen.Brush;
                                            }
                                        });
                                }
                            }

                            if (refreshMeasurementValueBelowChart)
                            {
                                lock (m_selectedMeasurements)
                                {
                                    RealTimeMeasurement measurement;
                                    if (m_selectedMeasurements.TryGetValue(tempSignalID, out measurement))
                                    {
                                        measurement.Value = tempValue.ToString("0.###");
                                        measurement.LongTimeTag = newMeasurement.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        measurement.TimeTag = newMeasurement.Timestamp.ToString("HH:mm:ss.fff");
                                        measurement.Quality = newMeasurement.ValueQualityIsGood() ? "GOOD" : "UNKNOWN";
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref m_processingSynchronizedMeasurements, 0);
                }
            }
        }

        private void m_subscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            m_subscribedSynchronized = true;
            Subscribe();
        }

        private void m_subscriber_ProcessException(object sender, EventArgs<Exception> e)
        {

        }

        private void m_subscriber_StatusMessage(object sender, EventArgs<string> e)
        {

        }

        private void m_subscriber_ProcessingComplete(object sender, EventArgs<string> e)
        {
            Dispatcher.BeginInvoke(new Action(() => ButtonReturnToRealtime_Click(null, null)));
        }

        private void InitializeSubscription()
        {
            try
            {
                using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    m_subscriber = new DataSubscriber();
                    m_subscriber.StatusMessage += m_subscriber_StatusMessage;
                    m_subscriber.ProcessException += m_subscriber_ProcessException;
                    m_subscriber.ConnectionEstablished += m_subscriber_ConnectionEstablished;
                    m_subscriber.NewMeasurements += m_subscriber_NewMeasurements;
                    m_subscriber.ConnectionTerminated += m_subscriber_ConnectionTerminated;
                    m_subscriber.ProcessingComplete += m_subscriber_ProcessingComplete;
                    m_subscriber.ConnectionString = database.DataPublisherConnectionString();
                    m_subscriber.Initialize();
                    m_subscriber.Start();
                }
            }
            catch
            {
                // TODO: show error in popup window
                //Popup("Failed to initialize subscription." + Environment.NewLine + ex.Message, "Failed to Subscribe", MessageBoxImage.Error);
            }
        }

        private void DisposeSubscription()
        {
            if (m_subscriber != null)
            {
                m_subscriber.StatusMessage -= m_subscriber_StatusMessage;
                m_subscriber.ProcessException -= m_subscriber_ProcessException;
                m_subscriber.ConnectionEstablished -= m_subscriber_ConnectionEstablished;
                m_subscriber.NewMeasurements -= m_subscriber_NewMeasurements;
                m_subscriber.ConnectionTerminated -= m_subscriber_ConnectionTerminated;
                m_subscriber.ProcessingComplete -= m_subscriber_ProcessingComplete;
                m_subscriber.Stop();
                m_subscriber.Dispose();
                m_subscriber = null;
            }
        }

        private void Subscribe()
        {
            if (m_selectedMeasurements.Count == 0)
            {
                Unsubscribe();
            }
            else
            {
                if (m_subscriber == null)
                {
                    InitializeSubscription();
                }
                else
                {
                    if (m_subscribedSynchronized && !string.IsNullOrEmpty(m_selectedSignalIDs))
                    {
                        m_subscriber.Unsubscribe();

                        if (m_historicalPlayback)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    string startTime = TextBoxStartTime.Text;
                                    string stopTime = TextBoxStopTime.Text;
                                    int processingInterval = int.Parse(TextBoxProcessInterval.Text);

                                    if (DateTime.Compare(AdapterBase.ParseTimeTag(startTime),
                                                     AdapterBase.ParseTimeTag(stopTime)) < 0)
                                    {
                                        ModeMessage.Text = "Initializing historical playback...";

                                        //m_synchronizedSubscriber.SynchronizedSubscribe(true, m_framesPerSecond, m_lagTime, m_leadTime, m_selectedSignalIDs, null, m_useLocalClockAsRealtime, m_ignoreBadTimestamps, startTime: TextBoxStartTime.Text, stopTime: TextBoxStopTime.Text, processingInterval: (int)SliderProcessInterval.Value);
                                        m_subscriber.UnsynchronizedSubscribe(true, false, m_selectedSignalIDs, null, true,
                                                                             m_lagTime, m_leadTime,
                                                                             m_useLocalClockAsRealtime, startTime, stopTime,
                                                                             null, processingInterval);
                                        m_waitingForData = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Start time must precede end time");
                                    }
                                }));
                        }
                        else
                        {
                            //m_synchronizedSubscriber.SynchronizedSubscribe(true, m_framesPerSecond, m_lagTime, m_leadTime, m_selectedSignalIDs, null, m_useLocalClockAsRealtime, m_ignoreBadTimestamps);
                            m_subscriber.UnsynchronizedSubscribe(true, false, m_selectedSignalIDs, null, true, m_lagTime, m_leadTime, m_useLocalClockAsRealtime);
                        }
                    }

                    ChartPlotterDynamic.Dispatcher.BeginInvoke((Action)StartRefreshTimer);
                }
            }
        }

        private void Unsubscribe()
        {
            try
            {
                if (m_subscriber != null)
                {
                    m_subscriber.Unsubscribe();
                    DisposeSubscription();
                }
            }
            catch
            {
                m_subscriber = null;
            }

            StopRefreshTimer();
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            PanAndZoomViewer viewer = new PanAndZoomViewer(new BitmapImage(new Uri(@"/GSF.PhasorProtocols.UI;component/Images/" + ((Button)sender).Tag, UriKind.Relative)), "Help Me Choose");
            viewer.Owner = Window.GetWindow(this);
            viewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            viewer.Topmost = true;
            viewer.ShowDialog();
        }

        private void ButtonRestoreSettings_Click(object sender, RoutedEventArgs e)
        {
            m_restartConnectionCycle = false;
            m_dataContext.RestartConnectionCycle = false;
            Unsubscribe();
            m_dataContext.UnsubscribeUnsynchronizedData();
            IsolatedStorageManager.InitializeStorageForInputStatusMonitor(true);
            RetrieveSettingsFromIsolatedStorage();
            PopulateSettings();
            PopupSettings.IsOpen = false;
            CommonFunctions.LoadUserControl(CommonFunctions.GetHeaderText("Graph Real-time Measurements"), typeof(InputStatusMonitorUserControl));
        }

        private void ButtonSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            ValidateSettingsBeforeWrite();
            IsolatedStorageManager.WriteToIsolatedStorage("InputMonitoringPoints", TextBoxLastSelectedMeasurements.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("ForceIPv4", CheckBoxForceIPv4.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("NumberOfDataPointsToPlot", TextBoxNumberOFDataPointsToPlot.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("DataResolution", TextBoxDataResolution.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("LagTime", TextBoxLagTime.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("LeadTime", TextBoxLeadTime.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("UseLocalClockAsRealtime", CheckBoxUseLocalClockAsRealTime.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("IgnoreBadTimestamps", CheckBoxIgnoreBadTimestamps.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("ChartRefreshInterval", TextBoxChartRefreshInterval.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("StatisticsDataRefreshInterval", TextBoxStatisticDataRefreshInterval.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("MeasurementsDataRefreshInterval", TextBoxMeasurementDataRefreshInterval.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayXAxis", CheckBoxDisplayXAxis.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayFrequencyYAxis", CheckBoxDisplayFrequencyYAxis.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayPhaseAngleYAxis", CheckBoxDisplayPhaseAngleYAxis.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayVoltageYAxis", CheckBoxDisplayVoltageMagnitudeYAxis.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayCurrentYAxis", CheckBoxDisplayCurrentMagnitudeYAxis.IsChecked.GetValueOrDefault());
            IsolatedStorageManager.WriteToIsolatedStorage("FrequencyRangeMin", TextBoxFrequencyRangeMin.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("FrequencyRangeMax", TextBoxFrequencyRangeMax.Text);
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayLegend", CheckBoxDisplayLegend.IsChecked.GetValueOrDefault());
            RetrieveSettingsFromIsolatedStorage();

            PopulateSettings();

            PopupSettings.IsOpen = false;

            CommonFunctions.LoadUserControl(CommonFunctions.GetHeaderText("Graph Real-time Measurements"), typeof(InputStatusMonitorUserControl));
        }

        private void ValidateSettingsBeforeWrite()
        {
            double frequencyRangeMin, frequencyRangeMax;
            double.TryParse(TextBoxFrequencyRangeMin.Text, out frequencyRangeMin);
            double.TryParse(TextBoxFrequencyRangeMax.Text, out frequencyRangeMax);
            if (frequencyRangeMin > frequencyRangeMax)
            {
                string temp = TextBoxFrequencyRangeMin.Text;
                TextBoxFrequencyRangeMin.Text = TextBoxFrequencyRangeMax.Text;
                TextBoxFrequencyRangeMax.Text = temp;
            }
        }

        private void PopulateSettings()
        {
            // Populate settings popup control.
            TextBoxLastSelectedMeasurements.Text = m_selectedSignalIDs;
            TextBoxNumberOFDataPointsToPlot.Text = m_numberOfDataPointsToPlot.ToString();
            TextBoxDataResolution.Text = m_framesPerSecond.ToString();
            TextBoxChartRefreshInterval.Text = m_chartRefreshInterval.ToString();
            TextBoxStatisticDataRefreshInterval.Text = m_statisticsDataRefershInterval.ToString();
            TextBoxMeasurementDataRefreshInterval.Text = m_measurementsDataRefreshInterval.ToString();
            TextBoxLagTime.Text = m_lagTime.ToString();
            TextBoxLeadTime.Text = m_leadTime.ToString();
            TextBoxFrequencyRangeMin.Text = m_frequencyRangeMin.ToString();
            TextBoxFrequencyRangeMax.Text = m_frequencyRangeMax.ToString();
            CheckBoxDisplayXAxis.IsChecked = m_displayXAxis;
            CheckBoxDisplayLegend.IsChecked = m_displayLegend;
            CheckBoxDisplayFrequencyYAxis.IsChecked = m_displayFrequencyYAxis;
            CheckBoxDisplayPhaseAngleYAxis.IsChecked = m_displayPhaseAngleYAxis;
            CheckBoxDisplayCurrentMagnitudeYAxis.IsChecked = m_displayCurrentYAxis;
            CheckBoxDisplayVoltageMagnitudeYAxis.IsChecked = m_displayVoltageYAxis;
            CheckBoxUseLocalClockAsRealTime.IsChecked = m_useLocalClockAsRealtime;
            CheckBoxIgnoreBadTimestamps.IsChecked = m_ignoreBadTimestamps;
            CheckBoxForceIPv4.IsChecked = m_forceIPv4;
        }

        private void ButtonManageSettings_Click(object sender, RoutedEventArgs e)
        {
            TextBoxLastSelectedMeasurements.Text = m_selectedSignalIDs;
            PopupSettings.Placement = PlacementMode.Center;
            PopupSettings.IsOpen = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = false;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (ExpanderHistoricalPlayback.IsVisible)
                DataGridStatistics.Height = 170;
            else
                DataGridStatistics.Height = 190;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            DataGridStatistics.Height = 110;
        }

        private void ButtonStartPlayback_Click(object sender, RoutedEventArgs e)
        {
            ModeMessage.Text = "Initializing historical playback...";
            ChartPlotterDynamic.Background = new SolidColorBrush(Color.FromArgb(255, 225, 225, 225));
            m_timeStampList = new ConcurrentQueue<string>();
            m_historicalPlayback = true;
            m_waitingForData = false;
            Subscribe();
        }

        private void ButtonReturnToRealtime_Click(object sender, RoutedEventArgs e)
        {
            ModeMessage.Text = "Real-time";
            ChartPlotterDynamic.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            m_timeStampList = new ConcurrentQueue<string>();
            m_historicalPlayback = false;
            m_waitingForData = false;
            Subscribe();
        }

        private void TextBoxProcessInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Allow dynamic updates to processing interval during operation
            if (m_historicalPlayback && !m_waitingForData && m_subscriber != null && m_subscriber.IsConnected)
            {
                int processingInterval;

                if (int.TryParse(TextBoxProcessInterval.Text, out processingInterval))
                    m_subscriber.ProcessingInterval = processingInterval;
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((RealTimeMeasurement)((Button)sender).DataContext).Selected = false;
            }
            catch
            {
            }
        }

        private void ButtonStatusBitReference_Click(object sender, RoutedEventArgs e)//svk_7/23/12
        {
            ShowuserStatusDoc.Placement = PlacementMode.Center;
            ShowuserStatusDoc.IsOpen = true;
        }


        private void ButtonCancelShowuserStatusDoc_Click(object sender, RoutedEventArgs e)
        {
            ShowuserStatusDoc.IsOpen = false;
        }

        #endregion
    }
}
