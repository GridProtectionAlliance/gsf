//******************************************************************************************************
//  SubscriberMeasurementUserControl.xaml.cs - Gbtc
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
//  05/20/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberMeasurementUserControl.xaml
    /// </summary>
    public partial class SubscriberMeasurementUserControl : UserControl
    {
        #region [ Members ]

        private Subscribers m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscriberMeasurementUserControl"/>.
        /// </summary>
        public SubscriberMeasurementUserControl()
        {
            InitializeComponent();
            m_dataContext = new Subscribers(1);
            m_dataContext.MeasurementsAdded += new Subscribers.OnMeasurementsAdded(m_dataContext_MeasurementsAdded);
            StackPanelManageSubscriberMeasurements.DataContext = m_dataContext;
            this.Loaded += new System.Windows.RoutedEventHandler(SubscriberMeasurementUserControl_Loaded);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles MeasurementsAdded event raised by MeasurementGroups view model class.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        void m_dataContext_MeasurementsAdded(object sender, System.Windows.RoutedEventArgs e)
        {
            UserControlSelectMeasurements.UncheckSelection();
        }

        /// <summary>
        /// Hanldes Loaded event of <see cref="SubscriberMeasurementUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments</param>
        void SubscriberMeasurementUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UserControlSelectMeasurements.SourceCollectionChanged += new SelectMeasurementUserControl.OnSourceCollectionChanged(UpdatePossibleMeasurements);
        }

        /// <summary>
        /// Method to retireve user's selection in the SelectMeasurementUserControl.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments</param>
        private void UpdatePossibleMeasurements(object sender, RoutedEventArgs e)
        {
            m_dataContext.CurrentItem.AvailableMeasurements = UserControlSelectMeasurements.UpdatedMeasurements;
        }

        #endregion
    }
}
