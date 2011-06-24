using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using TimeSeriesFramework.UI.DataModels;
using System.Windows.Threading;
using TVA.Data;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for RealTimeStatisticsUserControl.xaml
    /// </summary>
    public partial class RealTimeStatisticsUserControl : UserControl
    {
        #region [ Members ] 

        ObservableCollection<StatisticMeasurementData> m_statisticMeasurementDataList;
        StatisticMeasurementDataForBinding m_dataForBinding;
        DispatcherTimer m_thirtySecondsTimer;
        KeyValuePair<int?, int?> m_minMaxPointIDs;
        string m_url;
        Guid m_nodeID;
        bool m_retrievingData;
        AdoDataConnection database; 

        #endregion

        public RealTimeStatisticsUserControl()
        {
            InitializeComponent();
            database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
            this.Loaded += new RoutedEventHandler(RealTimeStatistics_Loaded);
            this.Unloaded += new RoutedEventHandler(RealTimeStatisticsUserControl_Unloaded);
            m_dataForBinding = new StatisticMeasurementDataForBinding();
            m_statisticMeasurementDataList = new ObservableCollection<StatisticMeasurementData>();
            m_minMaxPointIDs = new KeyValuePair<int?, int?>();

            int interval = 10;

            m_thirtySecondsTimer = new DispatcherTimer();
            m_thirtySecondsTimer.Interval = TimeSpan.FromSeconds(interval);
            TextBlockRefreshInterval.Text = "Refresh Interval: " + interval.ToString() + " sec";
            m_thirtySecondsTimer.Tick += new EventHandler(thirtySecondsTimer_Tick);
            m_thirtySecondsTimer.Start();
        }

        #region [ Methods ]

        void RealTimeStatisticsUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                m_thirtySecondsTimer.Stop();
                m_thirtySecondsTimer = null;
            }
            catch
            {

            }
        }

        void GetMinMaxPointIDs()
        {
            m_minMaxPointIDs = CommonFunctions.GetMinMaxPointIDs(null, m_nodeID);
        }

        void GetTimeTaggedMeasurements(string url)
        {
            if (!string.IsNullOrEmpty(url) && !m_retrievingData)
            {
                try
                {
                    m_retrievingData = true;
                    Dictionary<int, TimeTaggedMeasurement> timeTaggedMeasurements = new Dictionary<int, TimeTaggedMeasurement>();
                    timeTaggedMeasurements = CommonFunctions.GetStatisticMeasurements(url, m_nodeID.ToString());

                    if (timeTaggedMeasurements != null)
                    {
                        TextBlockLastRefresh.Text = "Last Refresh:" + DateTime.Now.ToString();
                        foreach (StatisticMeasurementData statisticMeasurement in m_statisticMeasurementDataList)
                        {
                            foreach (StreamInfo streamInfo in statisticMeasurement.SourceStreamInfoList)
                            {
                                foreach (DeviceStatistic deviceStatistic in streamInfo.DeviceStatisticList)
                                {
                                    foreach (DetailStatisticInfo detailStatistic in deviceStatistic.StatisticList)
                                    {
                                        TimeTaggedMeasurement timeTaggedMeasurement;
                                        if (timeTaggedMeasurements.TryGetValue(detailStatistic.PointID, out timeTaggedMeasurement))
                                        {
                                            detailStatistic.Statistics.Value = timeTaggedMeasurement.CurrentValue;
                                            detailStatistic.Statistics.TimeTag = timeTaggedMeasurement.TimeTag;
                                            detailStatistic.Statistics.Quality = timeTaggedMeasurement.Quality;

                                            if (detailStatistic.Statistics.IsConnectedState == true)
                                            {
                                                DateTime sourceDateTime;
                                                if (DateTime.TryParseExact(timeTaggedMeasurement.TimeTag, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out sourceDateTime) && DateTime.UtcNow.Subtract(sourceDateTime).TotalSeconds > 30)
                                                    streamInfo.StatusColor = "Gray";
                                                else if (Convert.ToBoolean(timeTaggedMeasurement.CurrentValue))
                                                    streamInfo.StatusColor = "Green";
                                                else
                                                    streamInfo.StatusColor = "Red";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    m_dataForBinding.IsExpanded = true;
                    m_dataForBinding.StatisticMeasurementDataList = m_statisticMeasurementDataList;
                    TreeViewRealTimeStatistics.DataContext = m_dataForBinding;
                    TreeViewRealTimeStatistics.Items.Refresh();
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(null, "WPF.TimeTaggedMeasurements", ex);
                }
                finally
                {
                    m_retrievingData = false;
                }
            }
        }

        void GetStatisticMeasurementData()
        {

            try
            {
                m_statisticMeasurementDataList = CommonFunctions.GetStatisticMeasurementData(null, m_nodeID);
                m_dataForBinding.StatisticMeasurementDataList = m_statisticMeasurementDataList;
                m_dataForBinding.IsExpanded = false;
                TreeViewRealTimeStatistics.DataContext = m_dataForBinding;
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "WPF.GetStatisticMeasurementData", ex);
            }
        }

        #endregion

        #region [ Page Event Handlers ]

        void thirtySecondsTimer_Tick(object sender, EventArgs e)
        {
            GetTimeTaggedMeasurements(m_url);
        }

        void RealTimeStatistics_Loaded(object sender, RoutedEventArgs e)
        {
            GetStatisticMeasurementData();
            GetMinMaxPointIDs();
            m_nodeID = (Guid)database.CurrentNodeID();
            if (string.IsNullOrEmpty(database.RealTimeStatisticServiceUrl()))
            {
                m_url = string.Empty;
            }
            else
            {
                m_url = database.RealTimeStatisticServiceUrl() + "/timeseriesdata/read/current/" + m_minMaxPointIDs.Key.ToString() + "-" + m_minMaxPointIDs.Value.ToString() + "/XML";
            }
            GetTimeTaggedMeasurements(m_url);
        }

        #endregion
    }
}
