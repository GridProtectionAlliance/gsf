//******************************************************************************************************
//  InternalSubscriptionUserControl.xaml.cs - Gbtc
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
//  08/23/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.TimeSeries.Transport.UI.ViewModels;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for InternalSubscriptionUserControl.xaml
    /// </summary>
    public partial class InternalSubscriptionUserControl
    {
        #region [ Members ]

        // Fields
        private bool m_defaultsInitialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InternalSubscriptionUserControl"/>.
        /// </summary>
        public InternalSubscriptionUserControl()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        private SubscriberRequestViewModel ViewModel
        {
            get
            {
                return Resources["ViewModel"] as SubscriberRequestViewModel;
            }
        }

        #endregion

        #region [ Methods ]

        private void InternalSubscriptionUserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SubscriberRequestViewModel model = ViewModel;

            if ((object)model != null)
                model.Dispose();
        }

        #endregion

        private void EnableDataGapRecovery_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!m_defaultsInitialized && EnableDataGapRecovery.IsChecked.GetValueOrDefault())
            {
                SubscriberRequestViewModel model = ViewModel;

                if ((object)model != null)
                    model.InitializeDefaultInternalSubscriptionSettings();

                m_defaultsInitialized = true;
            }
        }
    }
}
