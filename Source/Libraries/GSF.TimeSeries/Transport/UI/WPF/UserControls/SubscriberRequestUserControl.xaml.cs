//******************************************************************************************************
//  SubscriberRequestUserControl.xaml.cs - Gbtc
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
//  05/18/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using GSF.TimeSeries.Transport.UI.ViewModels;
using GSF.TimeSeries.UI;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberRequestUserControl.xaml
    /// </summary>
    public partial class SubscriberRequestUserControl : UserControl
    {
        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscriberRequestUserControl"/> class.
        /// </summary>     
        public SubscriberRequestUserControl()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        private void SecurityModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton tlsRadioButton = sender as RadioButton;
            SubscriberRequestViewModel viewModel = Resources["ViewModel"] as SubscriberRequestViewModel;
            SecurityMode securityMode;

            if ((object)tlsRadioButton != null && (object)viewModel != null)
            {
                if (Enum.TryParse(tlsRadioButton.Content.ToString(), out securityMode))
                    viewModel.SecurityMode = securityMode;
            }
        }

        private void SelfSignedCertificateGenerator_ProcessException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            Popup(ex.Message, "Certificate generation error", MessageBoxImage.Error);
            CommonFunctions.LogException(null, "Generate certificate", ex);
        }

        // Display popup message for the user
        private void Popup(string message, string caption, MessageBoxImage image)
        {
            MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, image);
        }

        #endregion
    }
}
