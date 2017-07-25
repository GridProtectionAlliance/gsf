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
using System.Reflection;
using System.Runtime.InteropServices;
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

        private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
#if !DEBUG
            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = m_browser.Document as IOleServiceProvider;

            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;

                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);

                if (webBrowser != null)
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { true });
            }
#endif
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}
