//******************************************************************************************************
//  FolderBrowserEditor.xaml.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  08/01/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GSF.TimeSeries.UI.DataModels;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace GSF.TimeSeries.UI.Editors
{
    /// <summary>
    /// Interaction logic for FolderBrowserEditor.xaml
    /// </summary>
    public partial class FolderBrowserEditor : UserControl
    {
        #region [ Members ]

        // Fields
        private Adapter m_adapter;
        private string m_parameterName;
        private string m_connectionString;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FolderBrowserEditor"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to be configured.</param>
        /// <param name="parameterName">The name of the parameter to be configured.</param>
        public FolderBrowserEditor(Adapter adapter, string parameterName)
            : this(adapter, parameterName, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FolderBrowserEditor"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to be configured.</param>
        /// <param name="parameterName">The name of the parameter to be configured.</param>
        /// <param name="connectionString">Parameters for the folder browser.</param>
        public FolderBrowserEditor(Adapter adapter, string parameterName, string connectionString)
        {
            InitializeComponent();
            m_adapter = adapter;
            m_parameterName = parameterName;
            m_connectionString = connectionString;
        }

        #endregion

        #region [ Methods ]

        private void FolderBrowserEditor_Loaded(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser;
            Popup parentPopup;

            Dictionary<string, string> settings;
            string setting;

            // Set up folder browser
            folderBrowser = new FolderBrowserDialog();
            folderBrowser.SelectedPath = GetCurrentParameterValue();

            // Parse folder browser parameters if they have been defined
            if ((object)m_connectionString != null)
            {
                settings = m_connectionString.ParseKeyValuePairs();

                if (settings.TryGetValue("description", out setting))
                    folderBrowser.Description = setting;

                if (settings.TryGetValue("showNewFolderButton", out setting))
                    folderBrowser.ShowNewFolderButton = setting.ParseBoolean();
            }

            // Show the browser and update the parameter
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                UpdateParameterValue(folderBrowser.SelectedPath);

            // Close the popup that contains this control
            parentPopup = GetParentPopup();

            if ((object)parentPopup != null)
                parentPopup.IsOpen = false;
        }
        
        private string GetCurrentParameterValue()
        {
            Dictionary<string, string> settings = m_adapter.ConnectionString.ToNonNullString().ParseKeyValuePairs();
            string value;

            if (settings.TryGetValue(m_parameterName, out value))
                return value;

            return null;
        }

        private void UpdateParameterValue(string value)
        {
            Dictionary<string, string> settings = m_adapter.ConnectionString.ToNonNullString().ParseKeyValuePairs();
            settings[m_parameterName] = value;
            m_adapter.ConnectionString = settings.JoinKeyValuePairs();
        }

        private Popup GetParentPopup()
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(this);
            Popup parentPopup = parent as Popup;

            while ((object)parent != null && (object)parentPopup == null)
            {
                parent = LogicalTreeHelper.GetParent(parent);
                parentPopup = parent as Popup;
            }

            return parentPopup;
        }

        #endregion
    }
}
