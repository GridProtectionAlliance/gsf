﻿//******************************************************************************************************
//  MainWindow.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/15/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GSF;
using GSF.Configuration;
using GSF.Data;

namespace CSVDataManager
{
    public partial class MainWindow : Form
    {
        #region [ Constructors ]

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        private Dictionary<string, string> DataProviderLookup { get; } = new Dictionary<string, string>()
        {
            { "SQL Server", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter" },
            { "SQLite", "" },
            { "PostgreSQL", "AssemblyName={Npgsql, Version=4.0.11.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7}; ConnectionType=Npgsql.NpgsqlConnection; AdapterType=Npgsql.NpgsqlDataAdapter" },
            { "MySQL", "AssemblyName={MySql.Data, Version=?.?.?.?, Culture=neutral, PublicKeyToken=c5687fc88969c44d}; ConnectionType=MySql.Data.MySqlClient.MySqlConnection; AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter" },
            { "Oracle", "AssemblyName={Oracle.DataAccess, Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342}; ConnectionType=Oracle.DataAccess.Client.OracleConnection; AdapterType=Oracle.DataAccess.Client.OracleDataAdapter" }
        };

        private Schema MemSchema { get; set; }
        private Schema DBSchema { get; set; }

        private string DBConnectionString { get; set; }
        private string DBDataProviderString { get; set; }
        private string SerializedSchema { get; set; }

        private string HostConfigurationFile { get; set; }
        private string HostConnectionString { get; set; }
        private string HostDataProviderString { get; set; }

        private TabPage PreviouslySelectedTab { get; set; }

        #endregion

        #region [ Methods ]

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Type sqliteConnectionType = typeof(System.Data.SQLite.SQLiteConnection);
            Type sqliteAdapterType = typeof(System.Data.SQLite.SQLiteDataAdapter);
            string assemblyName = sqliteConnectionType.Assembly.FullName;
            string memConnectionString = $"Data Source=:memory:; Version=3; Foreign Keys=True; FailIfMissing=True";
            string memDataProviderString = $"AssemblyName={{{assemblyName}}}; ConnectionType={sqliteConnectionType.FullName}; AdapterType={sqliteAdapterType.FullName}";
            DataProviderLookup["SQLite"] = memDataProviderString;

            ConfigurationFile configurationFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection applicationSettings = configurationFile.Settings["applicationSettings"];
            DBConnectionString = applicationSettings["ConnectionString"]?.Value;
            DBDataProviderString = applicationSettings["DataProviderString"]?.Value;
            SerializedSchema = applicationSettings["SerializedSchema"]?.Value;
            HostConfigurationFile = applicationSettings["HostConfigurationFile"]?.Value;

            if (!string.IsNullOrEmpty(HostConfigurationFile))
            {
                ConfigurationFile hostConfigurationFile = ConfigurationFile.Open(HostConfigurationFile);
                CategorizedSettingsElementCollection systemSettings = hostConfigurationFile.Settings["systemSettings"];
                HostConnectionString = systemSettings["ConnectionString"]?.Value;
                HostDataProviderString = systemSettings["DataProviderString"]?.Value;

                if (string.IsNullOrEmpty(DBConnectionString))
                    DBConnectionString = HostConnectionString;

                if (string.IsNullOrEmpty(DBDataProviderString))
                    DBDataProviderString = HostDataProviderString;
            }

            ConnectionStringTextBox.Text = DBConnectionString;
            DataProviderTextBox.Text = DBDataProviderString;
            SerializedSchemaTextBox.Text = SerializedSchema;

            MemSchema = new Schema($"{memConnectionString}; DataProviderString={{{memDataProviderString}}}", analyzeNow: false);
            DBSchema = new Schema();

            try
            {
                UpdateDBSchema();
            }
            catch (Exception ex)
            {
                string message = $"Error opening connection to database: {ex.Message}";
                MessageBox.Show(message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainTabControl.SelectedTab = ConfigurationTabPage;
            }
        }

        private void MainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            bool configurationChanged =
                DBConnectionString != ConnectionStringTextBox.Text ||
                DBDataProviderString != DataProviderTextBox.Text ||
                SerializedSchema != SerializedSchemaTextBox.Text;

            if (configurationChanged)
            {
                Cursor cursor = Cursor;

                try
                {
                    Cursor = Cursors.WaitCursor;

                    if (string.IsNullOrEmpty(ConnectionStringTextBox.Text))
                        ConnectionStringTextBox.Text = HostConnectionString;

                    if (string.IsNullOrEmpty(DataProviderTextBox.Text))
                        DataProviderTextBox.Text = HostDataProviderString;

                    if (string.IsNullOrEmpty(SerializedSchemaTextBox.Text))
                        SerializedSchemaTextBox.Text = "SerializedSchema.bin";

                    DBConnectionString = ConnectionStringTextBox.Text;
                    DBDataProviderString = DataProviderTextBox.Text;
                    SerializedSchema = SerializedSchemaTextBox.Text;

                    UpdateDBSchema();

                    string connectionString = string.Empty;
                    string dataProviderString = string.Empty;

                    if (DBConnectionString != HostConnectionString)
                        connectionString = DBConnectionString;

                    if (DBDataProviderString != HostDataProviderString)
                        dataProviderString = DBDataProviderString;

                    ConfigurationFile configurationFile = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection applicationSettings = configurationFile.Settings["applicationSettings"];
                    applicationSettings.Add("ConnectionString", connectionString, "Database connection string.", false, SettingScope.User);
                    applicationSettings.Add("DataProviderString", dataProviderString, "Configuration database ADO.NET data provider assembly type creation string.", false, SettingScope.User);
                    applicationSettings.Add("SerializedSchema", SerializedSchema, "File containing binary-serialized schema information about the database.", false, SettingScope.User);
                    applicationSettings["ConnectionString"].Value = connectionString;
                    applicationSettings["DataProviderString"].Value = dataProviderString;
                    applicationSettings["SerializedSchema"].Value = SerializedSchema;
                    configurationFile.Save();
                }
                catch(Exception ex)
                {
                    string message = $"Error updating configuration: {ex.Message}";
                    MessageBox.Show(message, "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
                finally
                {
                    Cursor = cursor;
                }
            }
        }

        private void TableComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Keep the import and export combo boxes in sync
            ComboBox tableComboBox = (ComboBox)sender;

            if (ImportTableComboBox.SelectedIndex != tableComboBox.SelectedIndex)
                ImportTableComboBox.SelectedIndex = tableComboBox.SelectedIndex;

            if (ExportTableComboBox.SelectedIndex != tableComboBox.SelectedIndex)
                ExportTableComboBox.SelectedIndex = tableComboBox.SelectedIndex;

            // To prevent superfluous updates, only update check
            // boxes when the combo box on the export tab changes
            if (tableComboBox == ExportTableComboBox)
            {
                Table table = (Table)ExportTableComboBox.SelectedItem;
                List<CheckBox> checkBoxes = new List<CheckBox>();

                foreach (Field field in table.Fields)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Text = field.Name;
                    checkBox.Checked = true;
                    checkBox.Enabled = !field.IsPrimaryKey;
                    checkBox.Dock = DockStyle.Bottom;
                    checkBox.AutoSize = true;
                    checkBox.Margin = new Padding(0);
                    checkBox.Tag = field;

                    checkBoxes.Add(checkBox);
                }

                // Adjust the margin of the last check
                // box to make extra room for scrolling
                checkBoxes.Last().Margin = new Padding(0, 0, 0, 20);

                // Add all the check boxes at once for a more responsive layout
                ExportFieldsPanel.Controls.Clear();
                ExportFieldsPanel.Controls.AddRange(checkBoxes.ToArray());

                // Update count labels to reflect number of records in selected table
                OpenDBConnectionAndExecute(UpdateCountLabels);
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (ExportTableComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a table from the dropdown.");
                return;
            }

            Table table = (Table)ExportTableComboBox.SelectedItem;
            ExportFileDialog.FileName = table.Name;

            DialogResult result = ExportFileDialog.ShowDialog();

            if (result != DialogResult.OK)
                return;

            Field[] fields = ExportFieldsPanel.Controls
                .OfType<CheckBox>()
                .Where(checkBox => checkBox.Checked)
                .Select(checkBox => (Field)checkBox.Tag)
                .ToArray();

            // Disabling the tab control makes it so the user cannot interact with any
            // controls while the export is executing, but running in a separate thread
            // does allow the user to move the window around while the export executes
            Cursor cursor = Cursor;
            BeginExport();

            Thread t = new Thread(() =>
            {
                try { DoExport(); }
                finally { EndExport(); }
            });

            t.IsBackground = true;
            t.Start();

            void BeginExport()
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(BeginExport));
                    return;
                }

                MainTabControl.Enabled = false;
                Cursor = Cursors.WaitCursor;
                ExportProgressBar.Value = 0;
            }

            void DoExport()
            {
                // The DBSchema database connection was closed after loading the schema so
                // it needs to be reopened before we can query the data to be exported
                OpenDBConnectionAndExecute(() => ExportSelectionToFile(table, fields));

                // Automatically open the exported file to upon completion
                using (Process.Start(ExportFileDialog.FileName)) { }
            }

            void EndExport()
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(EndExport));
                    return;
                }

                MainTabControl.Enabled = true;
                Cursor = cursor;
                ExportProgressBar.Value = 100;
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            Table table = (Table)ImportTableComboBox.SelectedItem;

            if ((object)table != null)
                ImportFileDialog.FileName = table.Name;

            DialogResult result = ImportFileDialog.ShowDialog();

            if (result != DialogResult.OK)
                return;

            string fileName = Path.GetFileName(ImportFileDialog.FileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            Table matchingTable = DBSchema.Tables[fileNameWithoutExtension];

            // Each of the import operations require slightly different wording
            Dictionary<object, string> importPhraseLookup = new Dictionary<object, string>()
            {
                { InsertButton, "insert data into" },
                { UpdateButton, "update" },
                { DeleteButton, "delete data from" }
            };

            if (!importPhraseLookup.TryGetValue(sender, out string importPhrase))
                importPhrase = "import data into";

            // Run sanity checks to make sure the user didn't make a mistake
            if ((object)table != null && (object)matchingTable != null && !ReferenceEquals(table, matchingTable))
            {
                DialogResult matchResult = MessageBox.Show($"File name matches another table in the database. Would you like to {importPhrase} the {matchingTable.Name} table instead?", "Matching table detected", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (matchResult == DialogResult.Cancel)
                    return;

                if (matchResult == DialogResult.Yes)
                    ImportTableComboBox.SelectedItem = table = matchingTable;
            }
            else if ((object)table != null)
            {

                DialogResult confirmResult = MessageBox.Show($"You are about to {importPhrase} the {table.Name} table. Are you sure you would like to proceed?", "Confirm import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult == DialogResult.No)
                    return;
            }
            else
            {
                MessageBox.Show("Please select a table to import to.");
                return;
            }

            // Disabling the tab control makes it so the user cannot interact with any
            // controls while the import is executing, but running in a separate thread
            // does allow the user to move the window around while the import executes
            Cursor cursor = Cursor;
            BeginImport();

            Thread t = new Thread(() =>
            {
                try { DoImport(); }
                finally { EndImport(); }
            });

            t.IsBackground = true;
            t.Start();

            void BeginImport()
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(BeginImport));
                    return;
                }

                MainTabControl.Enabled = false;
                Cursor = Cursors.WaitCursor;
                ImportProgressBar.Value = 0;
            }

            void DoImport()
            {
                using (BulkDataOperationBase importer = GetImporter(sender))
                {
                    OpenBothConnectionsAndExecute(() => ImportSelectionFromFile(importer, table));
                }
            }

            void EndImport()
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(EndImport));
                    return;
                }

                MainTabControl.Enabled = true;
                Cursor = cursor;
                ImportProgressBar.Value = 100;
                MessageBox.Show($"Completed import from {fileName} to the {table.Name} table.");
            }
        }

        private void Importer_OverallProgress(object sender, EventArgs<int, int> args)
        {
            int progress = args.Argument1;
            int total = args.Argument2;
            UpdateProgressBar(ImportProgressBar, 100 * progress / total);
        }

        private void DataProviderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selectedItem = DataProviderComboBox.SelectedItem;

            if (selectedItem == null)
                return;

            string dbType = selectedItem.ToString();
            string dataProviderString = DataProviderLookup[dbType];
            DataProviderTextBox.Text = dataProviderString;
        }

        private void ConfigurationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender == DataProviderTextBox)
            {
                foreach (KeyValuePair<string, string> dataProvider in DataProviderLookup)
                {
                    string dbType = dataProvider.Key;
                    string dataProviderString = dataProvider.Value;

                    if (dataProviderString == DataProviderTextBox.Text)
                    {
                        DataProviderComboBox.SelectedItem = dbType;
                        break;
                    }
                }
            }
        }

        private void SerializedSchemaBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = SerializedSchemaBrowseDialog.ShowDialog();

            if (result == DialogResult.OK)
                SerializedSchemaTextBox.Text = SerializedSchemaBrowseDialog.FileName;
        }

        private void UpdateDBSchema()
        {
            ImportTableComboBox.Items.Clear();
            ExportTableComboBox.Items.Clear();

            DBSchema.ConnectionString = $"{DBConnectionString}; DataProviderString={{{DBDataProviderString}}}; SerializedSchema={SerializedSchema}";
            DBSchema.Analyze();

            foreach (Table table in DBSchema.Tables)
            {
                ImportTableComboBox.Items.Add(table);
                ExportTableComboBox.Items.Add(table);
            }

            if (ImportTableComboBox.Items.Count > 0)
                ImportTableComboBox.SelectedIndex = 0;
        }

        private void UpdateCountLabels()
        {
            Table table = (Table)ExportTableComboBox.SelectedItem;
            object count = DBSchema.Connection.ExecuteScalar($"SELECT COUNT(*) FROM {table.SQLEscapedName}");
            string text = $"Count: {count}";
            ExportCountLabel.Text = text;
            ImportCountLabel.Text = text;
        }

        private void ExportSelectionToFile(Table table, Field[] fields)
        {
            string[] fieldNames = fields
                .Select(field => field.Name)
                .ToArray();

            string[] escapedFieldNames = fields
                .Select(field => field.SQLEscapedName)
                .ToArray();

            string csvHeader = CSVEncode(fieldNames);
            string fieldList = string.Join(",", escapedFieldNames);

            using (TextWriter writer = File.CreateText(ExportFileDialog.FileName))
            {
                writer.WriteLine(csvHeader);

                object result = DBSchema.Connection.ExecuteScalar($"SELECT COUNT(*) FROM {table.SQLEscapedName}");
                int count = Convert.ToInt32(result);
                UpdateProgressBar(ExportProgressBar, 0);

                using (IDataReader reader = DBSchema.Connection.ExecuteReader($"SELECT {fieldList} FROM {table.SQLEscapedName}"))
                {
                    int records = 0;

                    while (reader.Read())
                    {
                        string[] values = fields
                            .Select((field, index) => reader[index].ToNonNullString())
                            .ToArray();

                        string csvRecord = CSVEncode(values);
                        writer.WriteLine(csvRecord);
                        records++;

                        UpdateProgressBar(ExportProgressBar, 100 * records / count);
                    }

                    UpdateProgressBar(ExportProgressBar, 100);
                }
            }
        }

        private void ImportSelectionFromFile(BulkDataOperationBase importer, Table dbTable)
        {
            CopyCSVToMemSchema(dbTable);

            foreach (Table memTable in MemSchema.Tables)
            {
                importer.WorkTables.Add(memTable);
                memTable.Process = true;
            }

            UpdateProgressBar(ImportProgressBar, 0);
            importer.OverallProgress += Importer_OverallProgress;
            importer.UseFromSchemaReferentialIntegrity = false;
            importer.Execute();
            UpdateProgressBar(ImportProgressBar, 100);
        }

        private void CopyCSVToMemSchema(Table dbTable)
        {
            Table memTable = new Table(dbTable.Name);
            MemSchema.Tables.Clear();
            MemSchema.Tables.Add(memTable);

            using (TextReader reader = File.OpenText(ImportFileDialog.FileName))
            {
                string line = reader.ReadLine();

                if ((object)line == null)
                    throw new InvalidOperationException("CSV file is empty.");

                if (line.Length == 0)
                    throw new InvalidOperationException("CSV header row is empty.");

                string[] csvHeader = CSVDecode(line);

                if (csvHeader.Any(header => string.IsNullOrWhiteSpace(header)))
                    throw new InvalidOperationException("Ensure there are no empty fields in the CSV file");

                foreach (string fieldName in csvHeader)
                    memTable.Fields.Add(new Field(fieldName, OleDbType.VarChar));

                string[] tableFields = memTable.Fields
                    .Select(field => field.SQLEscapedName + " TEXT NOT NULL")
                    .ToArray();

                string tableFieldList = string.Join(",", tableFields);
                string createTableSQL = $"CREATE TABLE {memTable.SQLEscapedName}({tableFieldList})";
                MemSchema.Connection.ExecuteNonQuery(createTableSQL);

                // Modify the tableFieldList for INSERT statements
                tableFields = memTable.Fields
                    .Select(field => field.SQLEscapedName)
                    .ToArray();

                tableFieldList = string.Join(",", tableFields);

                while (true)
                {
                    line = reader.ReadLine();

                    if ((object)line == null)
                        break;

                    if (line.Length == 0)
                        continue;

                    string[] csvValues = CSVDecode(line);

                    for (int i = 0; i < memTable.Fields.Count; i++)
                        memTable.Fields[i].Value = csvValues[i];

                    string[] tableValues = memTable.Fields
                        .Select(field => field.SQLEncodedValue)
                        .ToArray();

                    string tableValueList = string.Join(",", tableValues);

                    string insertSQL = $"INSERT INTO {memTable.SQLEscapedName}({tableFieldList}) VALUES({tableValueList})";
                    MemSchema.Connection.ExecuteNonQuery(insertSQL);
                }
            }
        }

        private BulkDataOperationBase GetImporter(object button)
        {
            if (button == InsertButton)
                return new DataInserter(MemSchema, DBSchema);

            if (button == UpdateButton)
                return new DataUpdater(MemSchema, DBSchema);

            if (button == DeleteButton)
                return new DataDeleter(MemSchema, DBSchema);

            return null;
        }

        private void OpenBothConnectionsAndExecute(Action action) =>
            OpenMemConnectionAndExecute(() => OpenDBConnectionAndExecute(action));

        private void OpenMemConnectionAndExecute(Action action) =>
            OpenConnectionAndExecute(MemSchema, action);

        private void OpenDBConnectionAndExecute(Action action) =>
            OpenConnectionAndExecute(DBSchema, action);

        private void OpenConnectionAndExecute(Schema schema, Action action)
        {
            try
            {
                schema.Connection = Schema.OpenConnection(schema.ConnectionString, out DatabaseType databaseType, out Schema deserializedSchema, out bool isAdoConnection);
                schema.DataSourceType = databaseType;
                action();
            }
            finally
            {
                schema.Close();
            }
        }

        private string CSVEncode(string[] data)
        {
            string Encode(string text)
            {
                if (!text.Contains(','))
                    return text;

                return text
                    .Replace("\"", "\"\"")
                    .QuoteWrap();
            }

            string[] encodedData = data
                .Select(Encode)
                .ToArray();

            return string.Join(",", encodedData);
        }

        private string[] CSVDecode(string line)
        {
            List<string> fields = new List<string>();
            int i = 0;

            string ReadToComma()
            {
                StringBuilder token = new StringBuilder();

                while (i < line.Length && line[i] != ',')
                    token.Append(line[i++]);

                return token.ToString();
            }

            string ReadEscaped()
            {
                StringBuilder token = new StringBuilder();

                i++;

                while (true)
                {
                    while (i < line.Length && line[i] != '"')
                        token.Append(line[i++]);

                    i++;

                    if (i < line.Length && line[i] == '"')
                        token.Append(line[i++]);
                    else
                        break;
                }

                token.Append(ReadToComma());
                return token.ToString();
            }

            while (i < line.Length)
            {
                if (line[i] == '"')
                    fields.Add(ReadEscaped());
                else
                    fields.Add(ReadToComma());

                i++;
            }

            return fields.ToArray();
        }

        private void UpdateProgressBar(ProgressBar progressBar, int progress)
        {
            Action<ProgressBar, int> updateAction = (bar, val) => bar.Value = val;

            if (InvokeRequired)
                Invoke(updateAction, progressBar, progress);
            else
                updateAction(progressBar, progress);
        }

        #endregion
    }
}
