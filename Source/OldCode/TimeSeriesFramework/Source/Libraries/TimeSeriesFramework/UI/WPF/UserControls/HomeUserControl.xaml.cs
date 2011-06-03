//******************************************************************************************************
//  HomeUserControl.xaml.cs - Gbtc
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
//  06/01/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows.Controls;
using System.Windows;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for HomeUserControl.xaml
    /// </summary>
    public partial class HomeUserControl : UserControl
    {
        /// <summary>
        /// Creates an instance of <see cref="HomeUserControl"/> class.
        /// </summary>
        public HomeUserControl()
        {
            InitializeComponent();
        }

        private void ButtonBrowseDevices_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UIElement frame = null;
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
            DeviceListUserControl userControl = new DeviceListUserControl();

            ((System.Windows.Controls.Frame)frame).Navigate(userControl);
        }
    }
}
