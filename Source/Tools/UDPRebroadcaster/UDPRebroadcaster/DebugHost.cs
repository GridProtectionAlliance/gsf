//******************************************************************************************************
//  DebugHost.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  08/20/2009 - Paul B. Trachian
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Windows.Forms;
using GSF.Reflection;

namespace UDPRebroadcaster
{
    public partial class DebugHost : Form
    {
        #region [ Members ]

        // Fields
        private string m_productName;

        #endregion

        #region [ Constructors ]

        public DebugHost()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        private void DebugHost_Load(object sender, EventArgs e)
        {
            // Initialize text.
            m_productName = AssemblyInfo.EntryAssembly.Title;
            this.Text = string.Format(this.Text, m_productName);
            m_notifyIcon.Text = string.Format(m_notifyIcon.Text, m_productName);
            LabelNotice.Text = string.Format(LabelNotice.Text, m_productName);
            m_exitToolStripMenuItem.Text = string.Format(m_exitToolStripMenuItem.Text, m_productName);

            // Minimize the window.
            this.WindowState = FormWindowState.Minimized;

            // Start the windows service.
            m_serviceHost.StartDebugging(Environment.CommandLine.Split(' '));
        }

        private void DebugHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(string.Format("Are you sure you want to stop {0} windows service? ",
                m_productName), "Stop Service", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Stop the windows service.
                m_serviceHost.StopDebugging();
            }
            else
            {
                // Stop the application from exiting.
                e.Cancel = true;
            }
        }

        private void DebugHost_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                // Don't show the window in taskbar when minimized.
                this.ShowInTaskbar = false;
            }
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the window in taskbar the in normal mode (visible).
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Close this window which will cause the application to exit.
            this.Close();
        }

        #endregion
    }
}
