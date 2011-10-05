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

using System.Windows.Controls;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
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
            m_userAccounts.Saved += new System.EventHandler(m_userAccounts_Changed);
            m_userAccounts.Deleted += new System.EventHandler(m_userAccounts_Changed);
            m_securityGroups.Saved += new System.EventHandler(m_securityGroups_Changed);
            m_securityGroups.Deleted += new System.EventHandler(m_securityGroups_Changed);
        }

        #endregion

        #region [ Methods ]

        private void m_securityGroups_Changed(object sender, System.EventArgs e)
        {
            m_applicationRoles = new ApplicationRoles(1, true);
            RefreshBindings();
        }

        private void m_userAccounts_Changed(object sender, System.EventArgs e)
        {
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
