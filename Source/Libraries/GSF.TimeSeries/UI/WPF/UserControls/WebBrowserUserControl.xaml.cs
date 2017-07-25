//******************************************************************************************************
//  AdapterUserControl.xaml.cs - Gbtc
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
//  07/24/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using GSF.Data;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for AdapterUserControl.xaml
    /// </summary>
    public partial class WebBrowserUserControl : UserControl
    {
        private string m_urlPath;

        /// <summary>
        /// Creates an instance of <see cref="WebBrowserUserControl"/> class.
        /// </summary>
        public WebBrowserUserControl(string urlPath)
        {
            m_urlPath = urlPath;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            string webHostURL;

            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                string nodeSettings = connection.ExecuteScalar<string>("SELECT Settings FROM Node WHERE ID = {0}", connection.CurrentNodeID());
                Dictionary<string, string> lookup = nodeSettings.ParseKeyValuePairs();

                if (!lookup.TryGetValue("WebHostURL", out webHostURL))
                {
                    webHostURL = "http://localhost:8180/";
                    lookup.Add("WebHostURL", webHostURL);
                    nodeSettings = lookup.JoinKeyValuePairs();
                    connection.ExecuteNonQuery("UPDATE Node SET Settings = {0} WHERE ID = {1}", nodeSettings, connection.CurrentNodeID());
                }
            }

            m_browser.Source = new Uri(webHostURL.TrimEnd('/') + "/" + m_urlPath.TrimStart('/'));
        }
    }
}
