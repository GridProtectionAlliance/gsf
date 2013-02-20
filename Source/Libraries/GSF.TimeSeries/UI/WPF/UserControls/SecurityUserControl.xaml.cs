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

using GSF.TimeSeries.UI.ViewModels;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Windows;
using System.Windows.Controls;

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
        private string m_strongPasswordRegex = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";
        StringBuilder m_invalidPasswordMessage;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SecurityUserControl"/> class.
        /// </summary>
        public SecurityUserControl()
        {
            InitializeComponent();
            m_userAccounts = new UserAccounts(1, false);
            m_securityGroups = new SecurityGroups(1, false);
            m_applicationRoles = new ApplicationRoles(1, true);
            RefreshBindings();
            this.Unloaded += new RoutedEventHandler(SecurityUserControl_Unloaded);
            m_userAccounts.BeforeSave += new System.ComponentModel.CancelEventHandler(m_userAccounts_BeforeSave);
            m_userAccounts.Saved += new System.EventHandler(m_userAccounts_Changed);
            m_userAccounts.Deleted += new System.EventHandler(m_userAccounts_Changed);
            m_securityGroups.Saved += new System.EventHandler(m_securityGroups_Changed);
            m_securityGroups.Deleted += new System.EventHandler(m_securityGroups_Changed);

            m_invalidPasswordMessage = new StringBuilder();
            m_invalidPasswordMessage.Append("Password does not meet the following criteria:");
            m_invalidPasswordMessage.AppendLine();
            m_invalidPasswordMessage.Append("- Password must be at least 8 characters");
            m_invalidPasswordMessage.AppendLine();
            m_invalidPasswordMessage.Append("- Password must contain at least 1 digit");
            m_invalidPasswordMessage.AppendLine();
            m_invalidPasswordMessage.Append("- Password must contain at least 1 upper case letter");
            m_invalidPasswordMessage.AppendLine();
            m_invalidPasswordMessage.Append("- Password must contain at least 1 lower case letter");
        }

        #endregion

        #region [ Methods ]

        private void m_userAccounts_BeforeSave(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_userAccounts.CurrentItem.UseADAuthentication = (bool)RadioButtonWindows.IsChecked;
            if (m_userAccounts.IsNewRecord)
            {
                if ((bool)RadioButtonDatabase.IsChecked)
                {
                    if (string.IsNullOrEmpty(TextBoxPassword.Password) || !Regex.IsMatch(TextBoxPassword.Password, m_strongPasswordRegex))
                    {
                        MessageBox.Show("Please provide valid password value." + Environment.NewLine + m_invalidPasswordMessage, "Invalid Password", MessageBoxButton.OK);
                        e.Cancel = true;
                    }
                    else
                    {
                        #pragma warning disable 612, 618
                        m_userAccounts.CurrentItem.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(@"O3990\P78f9E66b:a35_V©6M13©6~2&[" + TextBoxPassword.Password, "SHA1");
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(TextBoxPassword.Password))
                {
                    if (Regex.IsMatch(TextBoxPassword.Password, m_strongPasswordRegex))
                    {
                        #pragma warning disable 612, 618
                        m_userAccounts.CurrentItem.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(@"O3990\P78f9E66b:a35_V©6M13©6~2&[" + TextBoxPassword.Password, "SHA1");
                    }
                    else
                    {
                        MessageBox.Show("Please provide valid password value or leave it blank to retain old password." + Environment.NewLine + m_invalidPasswordMessage, "Invalid Password", MessageBoxButton.OK);
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

        private void m_securityGroups_Changed(object sender, System.EventArgs e)
        {
            m_applicationRoles = new ApplicationRoles(1, true);
            RefreshBindings();
        }

        private void m_userAccounts_Changed(object sender, System.EventArgs e)
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
