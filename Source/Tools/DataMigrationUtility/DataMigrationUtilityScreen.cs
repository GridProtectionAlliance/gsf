//******************************************************************************************************
//  DataMigrationUtility.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  06/28/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/27/2010 - Mihir Brahmbhatt
//       Convert code from VB to C#
//  02/07/2011 - Aniket Salver
//       Added final message box to indicate activity completion.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GSF;
using GSF.Data;
using GSF.IO;
using GSF.Reflection;
using GSF.Windows.Forms;
using SerializationFormat = GSF.SerializationFormat;

namespace DataMigrationUtility
{
    public partial class DataMigrationUtilityScreen : Form
    {
        internal string AppName = AssemblyInfo.ExecutingAssembly.Name;

        private readonly Schema m_fromSchema;
        private readonly Schema m_toSchema;
        private readonly DataInserter m_dataInserter;

        private ApplicationSettings m_applicationSettings;
        private bool m_exceptionsEncountered;
        private bool m_verifiedConnectionStringReplacement;
        private long m_yieldIndex;
        private bool m_screenLoaded;

        public DataMigrationUtilityScreen()
        {
            InitializeComponent();

            m_fromSchema = new Schema();
            m_toSchema = new Schema();
            m_dataInserter = new DataInserter();

            m_fromSchema.ConnectionString = "";
            m_fromSchema.ImmediateClose = false;
            m_fromSchema.Tables = null;
            m_fromSchema.TableTypeRestriction = TableType.Table;
            m_fromSchema.AllowNumericNulls = true;
            m_fromSchema.AllowTextNulls = true;

            m_toSchema.ConnectionString = "";
            m_toSchema.ImmediateClose = false;
            m_toSchema.Tables = null;
            m_toSchema.TableTypeRestriction = TableType.Table;
            m_toSchema.AllowNumericNulls = true;
            m_toSchema.AllowTextNulls = true;

            m_dataInserter.BulkInsertFilePath = "c:\\";
            m_dataInserter.BulkInsertSettings = "FIELDTERMINATOR = \'\\t\', ROWTERMINATOR = \'\\n\', CODEPAGE = \'OEM\', FIRE_TRIGGERS, KEEPNULLS";
            m_dataInserter.DelimiterReplacement = " - ";
            m_dataInserter.FromSchema = m_fromSchema;
            m_dataInserter.ToSchema = m_toSchema;
            m_dataInserter.RowReportInterval = 10;

            m_dataInserter.OverallProgress += DataInserter_OverallProgress;
            m_dataInserter.RowProgress += DataInserter_RowProgress;
            m_dataInserter.TableProgress += DataInserter_TableProgress;
            m_dataInserter.TableCleared += DataInserter_TableCleared;
            m_dataInserter.SQLFailure += DataInserter_SQLFailure;
            m_dataInserter.BulkInsertExecuting += DataInserter_BulkInsertExecuting;
            m_dataInserter.BulkInsertCompleted += DataInserter_BulkInsertCompleted;
            m_dataInserter.BulkInsertException += DataInserter_BulkInsertException;
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
            AddMessage(ErrorText + "\r\n" + "Caused by: " + SQL);
        }

        private bool TestConnection(string connectionString, string title, out DatabaseType databaseType, out bool isAdoConnection)
        {
            isAdoConnection = false;

            try
            {
                Schema schema;

                IDbConnection connection = Schema.OpenConnection(connectionString, out databaseType, out schema, out isAdoConnection);
                connection.Close();

                MessageBox.Show(this, (isAdoConnection ? "ADO" : "OLE DB") + " Connection Succeeded!" + ((object)schema == null ? "" : " Serialized schema successfully deserialized."), title, MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (!Serialize.Visible && !isAdoConnection)
                    Serialize.Visible = true;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, (isAdoConnection ? "ADO" : "OLE DB") + " Connection Failed: " + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                databaseType = DatabaseType.Other;
                return false;
            }
        }

        private void DataMigrationUtility_Load(object sender, EventArgs e)
        {
            m_applicationSettings = new ApplicationSettings();

            Version.Text = "Version " + AssemblyInfo.EntryAssembly.Version.Major + "." + AssemblyInfo.EntryAssembly.Version.Minor + "." + AssemblyInfo.EntryAssembly.Version.Build;

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
            PreserveAutoIncValues.Checked = m_applicationSettings.PreserveAutoIncValues;
            ClearDestinationTables.Checked = m_applicationSettings.ClearDestinationTables;

            if (m_applicationSettings.UseFromConnectionForRI)
            {
                UseToForRI.Checked = false;
                UseFromForRI.Checked = true;
            }
            else
            {
                UseFromForRI.Checked = false;
                UseToForRI.Checked = true;
            }

            this.RestoreLocation();

            if (!GSF.Common.IsPosixEnvironment)
            {
                Show();
                BringToFront();
            }

            string[] args = Environment.GetCommandLineArgs();
            bool installFlag = args.Contains("-install", StringComparer.CurrentCultureIgnoreCase);

            if (installFlag)
                MessageBox.Show(this, "Setup is now ready to migrate your existing configuration database to the new schema.\r\n\r\nYou should now have a new blank \"destination\" database ready for this data transfer and the connection settings should show your original \"source\" database in the \"From Connect String\" and the new \"destination\" database in the \"To Connect String\".\r\n\r\nVerify and test the connection settings and make sure the \"To Connect String\" is selected for referential integrity (i.e., \"Use for RI\") before you begin your data operation.\r\n\r\nClick the \"Migrate\" button to begin the data transfer.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                MessageBox.Show(this, "IMPORTANT: Always backup database before any mass database update and remember to select the proper data source to use for referential integrity BEFORE you begin your data operation!", AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            m_screenLoaded = true;
        }

        private void DataMigrationUtility_Closed(Object eventSender, EventArgs eventArgs)
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
                m_applicationSettings.PreserveAutoIncValues = PreserveAutoIncValues.Checked;
                m_applicationSettings.ClearDestinationTables = ClearDestinationTables.Checked;

                // Save application settings
                m_applicationSettings.Save();

                // Save current window location (size is fixed)
                this.SaveLocation();
            }
            catch
            {
                // Don't want any possible failures during this event to prevent shutdown :)
            }

            Environment.Exit(0);
        }

        private void Serialize_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(this, "About serialize OLEDB obtained schema using \"From Connection String\" to \"SerializedSchema.bin\" - continue?", "Serialize Schema", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Schema schema = new Schema(FromConnectString.Text, TableType.Table);

                    using (FileStream stream = new FileStream(FilePath.GetAbsolutePath("SerializedSchema.bin"), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] schemaData = Serialization.Serialize(schema, SerializationFormat.Binary);
                        stream.Write(schemaData, 0, schemaData.Length);
                    }

                    MessageBox.Show(this, "Serialization Succeeded!", "Serialize Schema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, " Failed to serialize schema: " + ex.Message, "Serialize Schema", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Import_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FromConnectString.Text))
            {
                MessageBox.Show(this, "Cannot perform migration until source DSN is selected.", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                FromConnectString.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ToConnectString.Text))
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

                m_fromSchema.ConnectionString = FromConnectString.Text;
                m_fromSchema.DataSourceType = (DatabaseType)FromDataType.SelectedIndex;

                m_toSchema.ConnectionString = ToConnectString.Text;
                m_toSchema.DataSourceType = (DatabaseType)ToDataType.SelectedIndex;

                m_dataInserter.UseFromSchemaReferentialIntegrity = UseFromForRI.Checked;

                if (!string.IsNullOrEmpty(ExcludedTablesTextBox.Text))
                    m_dataInserter.ExcludedTables.AddRange(ExcludedTablesTextBox.Text.Split(','));

                m_dataInserter.PreserveAutoIncValues = PreserveAutoIncValues.Checked;
                m_dataInserter.ClearDestinationTables = ClearDestinationTables.Checked;

                if (m_dataInserter.ClearDestinationTables && m_toSchema.DataSourceType == DatabaseType.SQLServer)
                    m_dataInserter.AttemptTruncateTable = true;

                m_dataInserter.Execute();
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
            if (!m_screenLoaded)
                return;

            ComboBox source = (ComboBox)sender;
            TextBox destination;
            int index = source.SelectedIndex;

            if (source.Name == "FromDataType")
                destination = FromConnectString;
            else
                destination = ToConnectString;

            if (!string.IsNullOrWhiteSpace(destination.Text) && !m_verifiedConnectionStringReplacement && MessageBox.Show("Do you want to replace existing connection string with an example one for the selected database type?", "Verify Connection String Replacement", MessageBoxButtons.YesNo) == DialogResult.Yes)
                m_verifiedConnectionStringReplacement = true;

            if (m_verifiedConnectionStringReplacement)
            {
                bool useOleDB = MessageBox.Show("Do you want to show example standard ADO connections (select yes), or otherwise use example OLE DB style connection strings (select no)?", "Verify Connection String Type", MessageBoxButtons.YesNo) == DialogResult.No;

                switch ((DatabaseType)index)
                {
                    case DatabaseType.Access:
                        if (useOleDB)
                            destination.Text = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\\Program Files\\myApp\\myAppConfig.mdb";
                        else
                            destination.Text = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\\Program Files\\myApp\\myAppConfig.mdb; DataProviderString={AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.OleDb.OleDbConnection; AdapterType=System.Data.OleDb.OleDbDataAdapter}";
                        break;
                    case DatabaseType.SQLServer:
                        if (useOleDB)
                            destination.Text = "Provider=SQLOLEDB; Data Source=localhost; Initial Catalog=myApp; User Id=myUsername; Password=myPassword;";
                        else
                            destination.Text = "Data Source=localhost\\SQLEXPRESS; Initial Catalog=databaseName; User ID=userName; Password=password; DataProviderString={AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter}";
                        break;
                    case DatabaseType.MySQL:
                        if (useOleDB)
                            destination.Text = "Provider=MySQLProv; location=localhost; Data Source=myApp; User Id=root; Password=myPassword;";
                        else
                            destination.Text = "Server=localhost; Database=databaseName; Uid=root; Pwd=password; allow user variables = true; DataProviderString={AssemblyName={MySql.Data, Version=6.7.4.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d}; ConnectionType=MySql.Data.MySqlClient.MySqlConnection; AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter}";
                        break;
                    case DatabaseType.Oracle:
                        if (useOleDB)
                            destination.Text = "Provider=MySQLProv; location=MACHINE; Data Source=myApp; User Id=myUsername; Password=myPassword;";
                        else
                            destination.Text = "Data Source=tnsName; User ID=schemaUserName; Password=schemaPassword; DataProviderString={AssemblyName={Oracle.DataAccess, Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342}; ConnectionType=Oracle.DataAccess.Client.OracleConnection; AdapterType=Oracle.DataAccess.Client.OracleDataAdapter}";
                        break;
                    case DatabaseType.SQLite:
                        destination.Text = "Data Source=databaseName.db; Version=3; Foreign Keys=True; FailIfMissing=True; DataProviderString={AssemblyName={System.Data.SQLite, Version=1.0.109.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter}";
                        break;
                    default:
                        destination.Text = "";
                        break;
                }
            }
        }

        private void LinkFromTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DatabaseType databaseType;
            bool isAdoConnection;

            if (TestConnection(FromConnectString.Text, "From Connection Test Status", out databaseType, out isAdoConnection))
            {
                if (databaseType != (DatabaseType)FromDataType.SelectedIndex && isAdoConnection)
                    MessageBox.Show("WARNING: Selected database type '" + (DatabaseType)FromDataType.SelectedIndex + "' does not match detected database type in the connection string: " + databaseType, "Database Type Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LinkToTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DatabaseType databaseType;
            bool isAdoConnection;

            if (TestConnection(ToConnectString.Text, "To Connection Test Status", out databaseType, out isAdoConnection))
            {
                if (databaseType != (DatabaseType)ToDataType.SelectedIndex && isAdoConnection)
                    MessageBox.Show("WARNING: Selected database type '" + (DatabaseType)ToDataType.SelectedIndex + "' does not match detected database type in the connection string: " + databaseType, "Database Type Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExampleConnectionStringLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.connectionstrings.com/");
        }

        private void ClearDestinationTables_CheckedChanged(object sender, EventArgs e)
        {
            if (!m_screenLoaded)
                return;

            DatabaseType type = (DatabaseType)ToDataType.SelectedIndex;

            if (ClearDestinationTables.Checked && PreserveAutoIncValues.Checked && !(type == DatabaseType.SQLServer || type == DatabaseType.MySQL || type == DatabaseType.SQLite))
                MessageBox.Show("WARNING: If there is existing data, some databases may not be able to properly preserve auto-increment values.", "Configuration Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void DataInserter_OverallProgress(object sender, EventArgs<int, int> e) // Current, Total
        {
            OverallProgress.Minimum = 0;
            OverallProgress.Maximum = e.Argument2;

            if (OverallProgress.Value != e.Argument1)
            {
                OverallProgress.Value = e.Argument1;
                OverallProgress.Refresh();
            }

            if (m_yieldIndex++ % 5 == 0)
                Application.DoEvents();
        }

        private void DataInserter_RowProgress(object sender, EventArgs<string, int, int> e) // Name, Index, Total
        {
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = e.Argument3;

            int value = e.Argument2 < e.Argument3 ? e.Argument2 : e.Argument3;

            if (ProgressBar.Value != value)
            {
                ProgressBar.Value = (e.Argument2 < e.Argument3 ? e.Argument2 : e.Argument3);
                ProgressBar.Refresh();
            }

            if (m_yieldIndex++ % 5 == 0)
                Application.DoEvents();

            if (e.Argument2 == e.Argument3)
            {
                Application.DoEvents();
                Thread.Sleep(500);
            }
        }

        private void DataInserter_TableProgress(object sender, EventArgs<string, bool, int, int> e) // Name, Executed, CurrentTable, TotalTables
        {
            if (e.Argument3 == 0 && e.Argument4 == 0 && e.Argument1.Length > 0)
            {
                // When current/total = zero/zero - assuming request for a special message
                UpdateProgress(e.Argument1);
            }
            else
            {
                if (e.Argument3 == e.Argument4 && e.Argument1.Length == 0)
                {
                    UpdateProgress("Processing complete. (" + e.Argument3 + " / " + e.Argument4 + " )");
                }
                else
                {
                    if (e.Argument2)
                    {
                        Thread.Sleep(250);
                        UpdateProgress("Processing " + e.Argument1 + "... ( " + e.Argument3 + " / " + e.Argument4 + " )");
                    }
                    else
                    {
                        UpdateProgress("Skipped " + e.Argument1 + "... ( " + e.Argument3 + " / " + e.Argument4 + " )");
                    }
                }
            }

            Application.DoEvents();
        }

        private void DataInserter_TableCleared(object sender, EventArgs<string> e)
        {
            UpdateProgress("Cleared data from " + e.Argument + "...");
        }

        private void DataInserter_BulkInsertCompleted(object sender, EventArgs<string, int, int> e) //string TableName, int TotalRows, int TotalSeconds
        {
            AddMessage("Bulk insert of " + e.Argument2 + " rows into table " + e.Argument1 + " completed in " + Ticks.FromSeconds(e.Argument3).ToElapsedTimeString(2).ToLower());
        }

        private void DataInserter_BulkInsertException(object sender, EventArgs<string, string, Exception> e) // Name, SQL, ex
        {
            m_exceptionsEncountered = true;
            AddMessage("Exception occurred during bulk insert into table " + e.Argument1 + ": " + e.Argument3.Message);
            AddMessage("    Bulk Insert SQL: " + e.Argument2);
        }

        private void DataInserter_BulkInsertExecuting(object sender, EventArgs<string> e)
        {
            AddMessage("Executing bulk insert into table " + e.Argument + "...");
        }

        private void DataInserter_SQLFailure(object sender, EventArgs<string, Exception> e)
        {
            m_exceptionsEncountered = true;
            AddSQLErrorMessage(e.Argument1, e.Argument2.Message);
        }
    }
}
