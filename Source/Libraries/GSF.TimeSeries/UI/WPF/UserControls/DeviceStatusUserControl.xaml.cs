//******************************************************************************************************
//  DeviceStatusUserControl.xaml.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/25/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows.Controls;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceStatusUserControl.xaml
    /// </summary>
    public partial class DeviceStatusUserControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DeviceStatusUserControl"/> class.
        /// </summary>
        public DeviceStatusUserControl()
        {
            InitializeComponent();
            AddChild(new WebBrowserUserControl("Status.cshtml?ShowMenu=false"));
        }
    }
}
