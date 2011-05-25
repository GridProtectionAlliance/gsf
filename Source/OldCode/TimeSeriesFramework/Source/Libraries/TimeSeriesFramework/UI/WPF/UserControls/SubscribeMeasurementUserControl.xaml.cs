//******************************************************************************************************
//  SubscribeMeasurementUserControl.xaml.cs - Gbtc
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
//  05/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows.Controls;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscribeMeasurementUserControl.xaml
    /// </summary>
    public partial class SubscribeMeasurementUserControl : UserControl
    {
        #region [ Members ]

        private SubscribeMeasurements m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscribeMeasurementUserControl"/> class.
        /// </summary>
        public SubscribeMeasurementUserControl()
        {
            InitializeComponent();
            m_dataContext = new SubscribeMeasurements(1);
            m_dataContext.SubscriptionChanged += new SubscribeMeasurements.OnSubscriptionChanged(m_dataContext_MeasurementsSubscribed);
            m_dataContext.CurrentDeviceChanged += new SubscribeMeasurements.OnCurrentDeviceChanged(m_dataContext_CurrentDeviceChanged);
            StackPanelSubscribeMeasurements.DataContext = m_dataContext;
            this.Loaded += new System.Windows.RoutedEventHandler(SubscribeMeasurementUserControl_Loaded);
        }

        #endregion

        #region [ Methods ]

        private void SubscribeMeasurementUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UserControlSelectMeasurements.SourceCollectionChanged += new SelectMeasurementUserControl.OnSourceCollectionChanged(UserControlSelectMeasurements_SourceCollectionChanged);
        }

        private void UserControlSelectMeasurements_SourceCollectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            m_dataContext.MeasurementsToBeSubscribed = UserControlSelectMeasurements.UpdatedMeasurements;
        }

        private void m_dataContext_CurrentDeviceChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            UserControlSelectMeasurements.Refresh(m_dataContext.CurrentDeviceID);
        }

        private void m_dataContext_MeasurementsSubscribed(object sender, System.Windows.RoutedEventArgs e)
        {
            UserControlSelectMeasurements.UncheckSelection();
            UserControlSelectMeasurements.Refresh();
        }

        #endregion
    }
}
