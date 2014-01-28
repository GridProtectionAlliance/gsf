//******************************************************************************************************
//  SecurityUserControl.xaml.cs - Gbtc
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
//  05/10/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using GSF.Configuration;
using GSF.Security;
using GSF.TimeSeries.UI.ViewModels;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SecurityUserControl.xaml
    /// </summary>
    public partial class SecurityUserControl : UserControl
    {
        #region [ Members ]

        // Fields
        private UserAccounts m_userAccounts;
        private SecurityGroups m_securityGroups;
        private ApplicationRoles m_applicationRoles;
        private string m_passwordRequirementsRegex;
        private string m_passwordRequirementsError;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SecurityUserControl"/> class.
        /// </summary>
        public SecurityUserControl()
        {
            ConfigurationFile config;
            CategorizedSettingsElementCollection settings;

            InitializeComponent();

            config = ConfigurationFile.Current;
            settings = config.Settings["securityProvider"];

            // Set up defaults for config file settings
            m_passwordRequirementsRegex = AdoSecurityProvider.DefaultPasswordRequirementsRegex;
            m_passwordRequirementsError = AdoSecurityProvider.DefaultPasswordRequirementsError;

            // Make sure default config file settings exist
            settings.Add("PasswordRequirementsRegex", m_passwordRequirementsRegex, "Regular expression used to validate new passwords for database users.");
            settings.Add("PasswordRequirementsError", m_passwordRequirementsError, "Error message to be displayed when new database user password fails regular expression test.");

            // Read settings from config file
            m_passwordRequirementsRegex = settings["PasswordRequirementsRegex"].ValueAs(m_passwordRequirementsRegex);
            m_passwordRequirementsError = settings["PasswordRequirementsError"].ValueAs(m_passwordRequirementsError);

            // Initialize view models
            m_userAccounts = new UserAccounts(1, false);
            m_securityGroups = new SecurityGroups(1, false);
            m_applicationRoles = new ApplicationRoles(1, true);

            RefreshBindings();

            // Attach to events
            Unloaded += SecurityUserControl_Unloaded;
            m_userAccounts.BeforeSave += m_userAccounts_BeforeSave;
            m_userAccounts.Saved += m_userAccounts_Changed;
            m_userAccounts.Deleted += m_userAccounts_Changed;
            m_securityGroups.Saved += m_securityGroups_Changed;
            m_securityGroups.Deleted += m_securityGroups_Changed;
        }

        #endregion

        #region [ Methods ]

        private void m_userAccounts_BeforeSave(object sender, CancelEventArgs e)
        {
            m_userAccounts.CurrentItem.UseADAuthentication = RadioButtonWindows.IsChecked.GetValueOrDefault();

            if (m_userAccounts.IsNewRecord)
            {
                if (RadioButtonDatabase.IsChecked.GetValueOrDefault())
                {
                    if (string.IsNullOrEmpty(TextBoxPassword.Password) || !Regex.IsMatch(TextBoxPassword.Password, m_passwordRequirementsRegex))
                    {
                        MessageBox.Show(m_passwordRequirementsError, "Invalid Password", MessageBoxButton.OK);
                        e.Cancel = true;
                    }
                    else
                    {
                        m_userAccounts.CurrentItem.Password = SecurityProviderUtility.EncryptPassword(TextBoxPassword.Password);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(TextBoxPassword.Password))
                {
                    if (Regex.IsMatch(TextBoxPassword.Password, m_passwordRequirementsRegex))
                    {
                        m_userAccounts.CurrentItem.Password = SecurityProviderUtility.EncryptPassword(TextBoxPassword.Password);
                    }
                    else
                    {
                        MessageBox.Show("Please provide valid password value or leave it blank to retain old password." + Environment.NewLine + m_passwordRequirementsError, "Invalid Password", MessageBoxButton.OK);
                        e.Cancel = true;
                    }
                }
            }
        }

        private void SecurityUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_userAccounts != null)
                {
                    m_userAccounts.BeforeSave -= m_userAccounts_BeforeSave;
                    m_userAccounts.Saved -= m_userAccounts_Changed;
                    m_userAccounts.Deleted -= m_userAccounts_Changed;
                }
                if (m_securityGroups != null)
                {
                    m_securityGroups.Saved -= m_securityGroups_Changed;
                    m_securityGroups.Deleted -= m_securityGroups_Changed;
                }
            }
            finally
            {
                m_userAccounts = null;
                m_securityGroups = null;
                m_applicationRoles = null;
            }
        }

        private void m_securityGroups_Changed(object sender, EventArgs e)
        {
            m_applicationRoles = new ApplicationRoles(1, true);
            RefreshBindings();
        }

        private void m_userAccounts_Changed(object sender, EventArgs e)
        {
            TextBoxPassword.Password = string.Empty;
            m_securityGroups = new SecurityGroups(1, false);
            m_applicationRoles = new ApplicationRoles(1, true);
            RefreshBindings();
        }

        private void RefreshBindings()
        {
            GroupBoxManageUsers.DataContext = m_userAccounts;
            GroupBoxManageGroups.DataContext = m_securityGroups;
            GroupBoxManageRoles.DataContext = m_applicationRoles;
        }

        #endregion
    }
}
