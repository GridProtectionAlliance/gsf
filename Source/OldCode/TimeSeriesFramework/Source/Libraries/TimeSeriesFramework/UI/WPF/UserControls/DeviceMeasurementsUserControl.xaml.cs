//******************************************************************************************************
//  DeviceMeasurementsUserControl.xaml.cs - Gbtc
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
//  06/29/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

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
using TimeSeriesFramework.Transport;
using TVA.Data;
using System.Threading;
using TVA;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceMeasurementsUserControl.xaml
    /// </summary>
    public partial class DeviceMeasurementsUserControl : UserControl
    {

        #region [ Members ]

        ObservableCollection<DeviceMeasurementData> m_deviceMeasurementDataList;
        DeviceMeasurementDataForBinding m_dataForBinding;
        int m_refreshInterval;
        AdoDataConnection database;
        

        //Subscription API related declarations.
        DataSubscriber m_dataSubscriber;
        bool m_subscribed;
        int m_processing;
        bool m_restartConnectionCycle = true;
        string m_measurementForSubscription;

        #endregion

        #region [ Constructor ]

        public DeviceMeasurementsUserControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DeviceMeasurementsUserControl_Loaded);
            this.Unloaded += new RoutedEventHandler(DeviceMeasurementsUserControl_Unloaded);
            m_dataForBinding = new DeviceMeasurementDataForBinding();
            m_deviceMeasurementDataList = new ObservableCollection<DeviceMeasurementData>();
            database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
            m_refreshInterval = 10;
        }

        #endregion

        #region [ Page Event Handlers ]

        void DeviceMeasurementsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetDeviceMeasurementData();
            TextBlockRefreshInterval.Text = "Refresh Interval: " + m_refreshInterval.ToString() + " sec";
        }

        void DeviceMeasurementsUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_restartConnectionCycle = false;
            UnsubscribeData();  
        }

        #endregion

        #region [ Methods ]

        void GetDeviceMeasurementData()
        {
            try
            {
                m_deviceMeasurementDataList = DeviceMeasurementData.Load(null, (Guid)database.CurrentNodeID());
                m_dataForBinding.DeviceMeasurementDataList = m_deviceMeasurementDataList;

                StringBuilder sb = new StringBuilder();
                foreach (DeviceMeasurementData deviceMeasurementData in m_deviceMeasurementDataList)
                {
                    foreach (DeviceInfo deviceInfo in deviceMeasurementData.DeviceList)
                    {
                        foreach (MeasurementInfo measurementInfo in deviceInfo.MeasurementList)
                            sb.Append(measurementInfo.HistorianAcronym + ":" + measurementInfo.PointID + ";");
                    }
                }

                m_measurementForSubscription = sb.ToString();
                if (m_measurementForSubscription.Length > 0)
                    m_measurementForSubscription = m_measurementForSubscription.Substring(0, m_measurementForSubscription.Length - 1);

                m_dataForBinding.IsExpanded = false;
                TreeViewDeviceMeasurements.DataContext = m_dataForBinding;

                SubscribeData();
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "WPF.DeviceMeasurementUserControl", ex);
            }
        }

        #endregion

        #region [ Subscription API Code ]

        #region [ Methods ]

        void StartSubscription()
        {
            // Create server connection string
            string server;
            StringBuilder b = new StringBuilder();
            string serverTemp = database.RemoteStatusServerConnectionString();
            var temp = serverTemp.Split(';');
            b.Append(temp[0].Substring(0,temp[0].Length - 4));
            b.Append(temp[1].Substring(temp[1].Length - 4));
            server = b.ToString();

            m_dataSubscriber = new DataSubscriber();
            m_dataSubscriber.StatusMessage += dataSubscriber_StatusMessage;
            m_dataSubscriber.ProcessException += dataSubscriber_ProcessException;
            m_dataSubscriber.ConnectionEstablished += dataSubscriber_ConnectionEstablished;
            m_dataSubscriber.NewMeasurements += dataSubscriber_NewMeasurements;
            m_dataSubscriber.ConnectionTerminated += dataSubscriber_ConnectionTerminated;
            m_dataSubscriber.ConnectionString = server;
            m_dataSubscriber.Initialize();
            m_dataSubscriber.Start();
        }

        void SubscribeData()
        {
            if (m_dataSubscriber == null)
                StartSubscription();

            m_subscribed = true;
        }

        void UnsubscribeData()
        {
            try
            {
                if (m_dataSubscriber != null)
                {
                    m_dataSubscriber.Unsubscribe();
                    StopSubscription();
                }
            }
            catch
            {
                m_dataSubscriber = null;
            }
        }

        void StopSubscription()
        {
            if (m_dataSubscriber != null)
            {
                m_dataSubscriber.StatusMessage -= dataSubscriber_StatusMessage;
                m_dataSubscriber.ProcessException -= dataSubscriber_ProcessException;
                m_dataSubscriber.ConnectionEstablished -= dataSubscriber_ConnectionEstablished;
                m_dataSubscriber.NewMeasurements -= dataSubscriber_NewMeasurements;
                m_dataSubscriber.Stop();
                m_dataSubscriber.Dispose();
                m_dataSubscriber = null;
            }
        }

        #endregion

        #region [ Event Handlers ]

        void dataSubscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (0 == Interlocked.Exchange(ref m_processing, 1))
            {
                try
                {
                    foreach (DeviceMeasurementData deviceMeasurementData in m_deviceMeasurementDataList)
                    {
                        foreach (DeviceInfo deviceInfo in deviceMeasurementData.DeviceList)
                        {
                            foreach (MeasurementInfo measurementInfo in deviceInfo.MeasurementList)
                            {
                                foreach (IMeasurement measurement in e.Argument)
                                {
                                    if (measurement.ID.ToString().ToUpper() == measurementInfo.SignalID.ToUpper())
                                    {
                                        measurementInfo.CurrentQuality = measurement.ValueQualityIsGood() ? "GOOD" : "BAD";
                                        measurementInfo.CurrentTimeTag = measurement.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        measurementInfo.CurrentValue = measurement.Value.ToString("0.###");
                                    }
                                }
                            }
                        }
                    }
                    TreeViewDeviceMeasurements.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        TreeViewDeviceMeasurements.Items.Refresh();
                        m_dataForBinding.IsExpanded = true;
                        m_dataForBinding.DeviceMeasurementDataList = m_deviceMeasurementDataList;
                        TreeViewDeviceMeasurements.DataContext = m_dataForBinding;
                    });
                }
                finally
                {
                    Interlocked.Exchange(ref m_processing, 0);
                }
            }
        }

        void dataSubscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            m_subscribed = true;
            if (m_subscribed && !string.IsNullOrEmpty(m_measurementForSubscription))
            {
                m_dataSubscriber.UnsynchronizedSubscribe(true, true, m_measurementForSubscription);
            }
            SubscribeData();
        }

        void dataSubscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            m_subscribed = false;
            UnsubscribeData();
            if (m_restartConnectionCycle)
                StartSubscription();
        }

        void dataSubscriber_ProcessException(object sender, TVA.EventArgs<Exception> e)
        {
            //System.Diagnostics.Debug.WriteLine("SUBSCRIPTION: EXCEPTION: " + e.Argument.Message);
        }

        void dataSubscriber_StatusMessage(object sender, TVA.EventArgs<string> e)
        {
            //System.Diagnostics.Debug.WriteLine("SUBSCRIPTION: " + e.Argument);
        }

        #endregion

        #endregion
    }
}
