//******************************************************************************************************
//  DataMigrationUtility.cs - Gbtc
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
//  06/28/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/27/2010 - Mihir Brahmbhatt
//       Convert code from VB to C#
//  02/07/2011 - Aniket Salver
//       Added final message box to indicate activity completion.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using TVA;
using TVA.Configuration;
using TVA.Reflection;
using Database;
using TVA.Units;
using TVA.Windows;
using TVA.Windows.Forms;
using System.Threading;

namespace DataMigrationUtility
{
    public partial class DataMigrationUtility : Form
    {
        internal string AppName = AssemblyInfo.ExecutingAssembly.Name;
        private ApplicationSettings m_applicationSettings;
        private bool m_exceptionsEncountered;

        public DataMigrationUtility()
        {
            InitializeComponent();
        }

        private void UpdateProgress(string LabelText)
        {
            ProgressLabel.Text = "Progress: " + LabelText;
            ProgressLabel.Refresh();
        }

        private void AddMessage(string MessageText)
        {
            Messages.Text += "\n" + MessageText;
            Messages.SelectionStart = Messages.Text.Length;
            Messages.ScrollToCaret();
        }

        private void AddSQLErrorMessage(string SQL, string ErrorText)
        {
            AddMessage(ErrorText + "\n" + "Caused by: " + SQL);
        }

        private void TestConnection(string ConnectString, string Title)
        {
            try
            {
                OleDbConnection cnn = new OleDbConnection(ConnectString);
                cnn.Open();
                cnn.Close();
                MessageBox.Show(this, "Connection Succeeded!", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Connection Failed: " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void DataMigrationUtility_Load(object sender, EventArgs e)
        {
            m_applicationSettings = new ApplicationSettings();

            Version.Text = "Version " + AssemblyInfo.EntryAssembly.Version.Major
                    + "." + AssemblyInfo.EntryAssembly.Version.Minor + "." + AssemblyInfo.EntryAssembly.Version.Build;


            // Load database types from enumeration values
            foreach (string databaseType in Enum.GetNames(typeof(DatabaseType)))
            {
                ToDataType.Items.Add(databaseType);
                FromDataType.Items.Add(databaseType);
            }

            // Restore last settings
            FromDataType.SelectedIndex = Convert.ToInt32(m_applicationSettings.FromDataType);
            ToDataType.SelectedIndex = Convert.ToInt32(m_applicationSettings.ToDataType);
            FromConnectString.Text = m_applicationSettings.FromConnectionString;
            ToConnectString.Text = m_applicationSettings.ToConnectionString;
            chkPreservePrimaryKey.Checked = m_applicationSettings.PreservePrimaryKeyValue;

            if (m_applicationSettings.UseFromConnectionForRI)
                UseFromForRI.Checked = true;
            else
                UseToForRI.Checked = true;

            this.RestoreLocation();

            Show();
            BringToFront();

            string[] args = Environment.GetCommandLineArgs();
            bool installFlag = args.Contains("-install", StringComparer.CurrentCultureIgnoreCase);

            if (installFlag)
                MessageBox.Show(this, "Setup is now ready to migrate your existing configuration database to the new schema.\r\n\r\nYou should now have a new blank \"destination\" database ready for this data transfer and the connection settings should show your original \"source\" database in the \"From Connect String\" and the new \"destination\" database in the \"To Connect String\".\r\n\r\nVerify and test the connection settings and make sure the \"To Connect String\" is selected for referential integrity (i.e., \"Use for RI\") before you begin your data operation.\r\n\r\nClick the \"Migrate\" button to begin the data transfer.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                MessageBox.Show(this, "IMPORTANT: Always backup database before any mass database update and remember to select the proper data source to use for referential integrity BEFORE you begin your data operation!", AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void DataMigrationUtility_Closed(System.Object eventSender, System.EventArgs eventArgs)
        {
            this.Hide();
            Application.DoEvents();

            try
            {
                m_applicationSettings.FromDataType = (DatabaseType)FromDataType.SelectedIndex;
                m_applicationSettings.ToDataType = (DatabaseType)ToDataType.SelectedIndex;
                m_applicationSettings.FromConnectionString = FromConnectString.Text;
                m_applicationSettings.ToConnectionString = ToConnectString.Text;
                m_applicationSettings.UseFromConnectionForRI = UseFromForRI.Checked;
                m_applicationSettings.PreservePrimaryKeyValue = chkPreservePrimaryKey.Checked;

                // Save application settings
                m_applicationSettings.Save();

                // Save current window location (size is fixed)
                this.SaveLocation();
            }
            catch
            {
                // Don't want any possible failures during this event to prevent shudown :)
            }
            System.Environment.Exit(0);


        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Import_Click(object sender, EventArgs e)
        {
            if (FromConnectString.Text.Length == 0)
            {
                MessageBox.Show(this, "Cannot perform migration until source DSN is selected.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                FromConnectString.Focus();
                return;
            }

            if (ToConnectString.Text.Length == 0)
            {
                MessageBox.Show(this, "Cannot perform migration until destination DSN is selected.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ToConnectString.Focus();
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Don't allow user to inadvertently click the import button again
                Import.Enabled = false;

                // Reset exception flag
                m_exceptionsEncountered = false;

                // Reset message text
                Messages.Text = "Messages:";

                UpdateProgress("Analyzing data structures, please wait...");

                FromSchema.ConnectString = FromConnectString.Text;
                FromSchema.DataSourceType = (DatabaseType)FromDataType.SelectedIndex;

                ToSchema.ConnectString = ToConnectString.Text;
                ToSchema.DataSourceType = (DatabaseType)ToDataType.SelectedIndex;

                DataInserter.UseFromSchemaReferentialIntegrity = UseFromForRI.Checked;

                if (!string.IsNullOrEmpty(ExcludedTablesTextBox.Text))
                {
                    DataInserter.ExcludedTables.AddRange(ExcludedTablesTextBox.Text.Split(','));
                }
                DataInserter.PreservePrimaryKeyValue = chkPreservePrimaryKey.Checked;
                DataInserter.Execute();
            }
            catch (Exception ex)
            {
                m_exceptionsEncountered = true;
                UpdateProgress("Exception - " + ex.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if (m_exceptionsEncountered)
            {
                MessageBox.Show(this, "There were errors during the data transfer. Click OK to review.", "Data Insert Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Import.Enabled = true; // Re-enable Migrate button, so user can correct issue and retry
            }
            else
            {
                MessageBox.Show(this, "Data transfer complete. Data Migration Utility will now exit.", "Data Migration Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void DataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox source = (ComboBox)sender;
            TextBox destination = default(TextBox);
            int index = source.SelectedIndex;

            if (source.Name == "FromDataType")
            {
                destination = FromConnectString;
            }
            else
            {
                destination = ToConnectString;
            }

            switch ((DatabaseType)index)
            {
                case DatabaseType.Access:
                    destination.Text = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\\Program Files\\openPDC\\openPDC.mdb";
                    break;
                case DatabaseType.SqlServer:
                    destination.Text = "Provider=SQLOLEDB; Data Source=localhost; Initial Catalog=openPDC; User Id=myUsername; Password=myPassword;";
                    break;
                case DatabaseType.MySQL:
                    destination.Text = "Provider=MySQLProv; location=MACHINE; Data Source=openPDC; User Id=myUsername; Password=myPassword;";
                    break;
                case DatabaseType.Oracle:
                    destination.Text = "Provider=msdaora; Data Source=openPDC; User Id=myUsername; Password=myPassword;";
                    break;
                default:
                    destination.Text = "";
                    break;
            }
        }

        private void LinkFromTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TestConnection(FromConnectString.Text, "From Connection Test Status");
        }

        private void LinkToTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TestConnection(ToConnectString.Text, "To Connection Test Status");
        }

        private void ExampleConnectionStringLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.connectionstrings.com/");
        }

        private void DataInserter_OverallProgress(object sender, OverallProgressEventHandler<int, int> e)
        {
            if (OverallProgress.Minimum != 0)
                OverallProgress.Minimum = 0;

            if (OverallProgress.Maximum != e.Total)
                OverallProgress.Maximum = e.Total;

            OverallProgress.Value = e.Current;
            OverallProgress.Refresh();

            if (e.Current == e.Total)
                Application.DoEvents();
        }

        private void DataHandler_RowProgress(object sender, RowProgressEventHandler<string, int, int> e)
        {
            if (ProgressBar.Minimum != 0)
                ProgressBar.Minimum = 0;

            if (ProgressBar.Maximum != e.TotalRows)
                ProgressBar.Maximum = e.TotalRows;

            ProgressBar.Value = (e.CurrentRow < e.TotalRows ? e.CurrentRow : e.TotalRows);
            ProgressBar.Refresh();

            if (e.CurrentRow == e.TotalRows)
            {
                Application.DoEvents();
                Thread.Sleep(500);
            }
        }

        private void DataHandler_TableProgress(object sender, TableProgressEventHandler<string, bool, int, int> e)
        {
            if (e.CurrentTable == e.TotalTables && e.TableName.Length == 0)
            {
                UpdateProgress("Processing complete. (" + e.CurrentTable + " / " + e.TotalTables + " )");
            }
            else
            {
                if (e.Executed)
                {
                    System.Threading.Thread.Sleep(250);
                    UpdateProgress("Processing " + e.TableName + "... ( " + e.CurrentTable.ToString() + " / " + e.TotalTables.ToString() + " )");
                }
                else
                {
                    UpdateProgress("Skipped " + e.TableName + "... ( " + e.CurrentTable.ToString() + " / " + e.TotalTables.ToString() + " )");
                }
            }
            Application.DoEvents();
        }

        private void DataHandler_TableCleared(string TableName)
        {
            UpdateProgress("Cleared data from " + TableName + "...");
        }

        private void DataHandler_BulkInsertCompleted(string TableName, int TotalRows, int TotalSeconds)
        {
            AddMessage("Bulk insert of " + TotalRows + " rows into table " + TableName + " completed in " + Ticks.FromSeconds(TotalSeconds).ToElapsedTimeString().ToLower());
        }

        private void DataHandler_BulkInsertException(string TableName, string SQL, System.Exception ex)
        {
            m_exceptionsEncountered = true;
            AddMessage("Exception occurred during bulk insert into table " + TableName + ": " + ex.Message);
            AddMessage("    Bulk Insert SQL: " + SQL);
        }

        private void DataHandler_BulkInsertExecuting(string TableName)
        {
            AddMessage("Executing bulk insert into table " + TableName + "...");
        }

        private void DataHandler_SqlFailure(object sender, SqlFailureEventHandler<string, Exception> e)
        {
            m_exceptionsEncountered = true;
            AddSQLErrorMessage(e.Sql, e.Ex.Message);
        }

        private void DataHandler_TableProgress(object sender)
        {

        }

        private void DataHandler_RowProgress(object sender)
        {

        }

        private void DataInserter_OverallProgress(object sender)
        {

        }

        private void DataHandler_SqlFailure(object sender)
        {

        }
    }
}
