//******************************************************************************************************
//  FileDialogEditor.xaml.cs - Gbtc
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
//  08/02/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GSF.TimeSeries.UI.DataModels;
using Microsoft.Win32;

namespace GSF.TimeSeries.UI.Editors
{
    /// <summary>
    /// Interaction logic for FileDialogEditor.xaml
    /// </summary>
    public partial class FileDialogEditor : UserControl
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
        /// <param name="connectionString">Parameters for the file dialog.</param>
        public FileDialogEditor(Adapter adapter, string parameterName, string connectionString)
        {
            InitializeComponent();
            m_adapter = adapter;
            m_parameterName = parameterName;
            m_connectionString = connectionString;
        }

        #endregion

        #region [ Methods ]

        private void FileDialogEditor_Loaded(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog;
            Popup parentPopup;

            Dictionary<string, string> settings;
            string setting;
            int filterIndex;

            // Parse connection string
            settings = m_connectionString.ParseKeyValuePairs();

            if (!settings.TryGetValue("type", out setting))
                return;

            // Set up the file dialog
            fileDialog = setting.Equals("save", StringComparison.CurrentCultureIgnoreCase) ? (FileDialog)new SaveFileDialog() : (FileDialog)new OpenFileDialog();
            fileDialog.FileName = GetCurrentParameterValue();

            // Parse file dialog parameters
            if (settings.TryGetValue("checkFileExists", out setting))
                fileDialog.CheckFileExists = setting.ParseBoolean();

            if (settings.TryGetValue("checkPathExists", out setting))
                fileDialog.CheckPathExists = setting.ParseBoolean();

            if (settings.TryGetValue("defaultExt", out setting))
                fileDialog.DefaultExt = setting;

            if (settings.TryGetValue("filter", out setting))
                fileDialog.Filter = setting;

            if (settings.TryGetValue("filterIndex", out setting) && int.TryParse(setting, out filterIndex))
                fileDialog.FilterIndex = filterIndex;

            if (settings.TryGetValue("title", out setting))
                fileDialog.Title = setting;

            if (settings.TryGetValue("validateNames", out setting))
                fileDialog.ValidateNames = setting.ParseBoolean();

            // Show the dialog and update the parameter
            if (fileDialog.ShowDialog() == true)
                UpdateParameterValue(fileDialog.FileName);

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
