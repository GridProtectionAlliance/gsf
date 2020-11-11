//******************************************************************************************************
//  MainForm.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  11/06/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using AdapterExplorer.Model;
using GSF;
using GSF.ComponentModel;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;
using GSF.Windows.Forms;
using Measurement = AdapterExplorer.Model.Measurement;

namespace AdapterExplorer
{
    public partial class MainForm : Form
    {
        private DataSet m_dataSource;
        private readonly LogPublisher m_log;
        private AdoDataConnection m_connection;
        private Settings m_settings;
        private DataSubscriber m_subscriber;
        private UnsynchronizedSubscriptionInfo m_throttledSubscription;
        private Guid[] m_inputSignalIDs;
        private Guid[] m_outputSignalIDs;
        private volatile bool m_formClosing;

        public MainForm()
        {
            InitializeComponent();
            m_log = Program.Log;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Load current settings registering a symbolic reference to this form instance for use by default value expressions
                m_settings = new Settings(new Dictionary<string, object> { { "Form", this } }.RegisterSymbols());

                // Restore last window size/location
                this.RestoreLayout();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormLoad", exception: ex);

            #if DEBUG
                throw;
            #endif
            }

            try
            {
                m_connection = Program.GetDatabaseConnection();

                LoadDataSource();

                InitializeDataGrid(dataGridViewInputMeasurements);
                InitializeDataGrid(dataGridViewOutputMeasurements);

                InitializeDataSubscriber();
                LoadAdapters();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormLoad: Database Connection", exception: ex);

            #if DEBUG
                throw;
            #else
                MessageBox.Show(this, $"Failed to connect to database defined in \"{Program.HostConfigFileName}\":{Environment.NewLine}{ex.Message}", "Database Connection Error", MessageBoxButtons.OK);
            #endif
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                m_formClosing = true;

                // Save current window size/location
                this.SaveLayout();

                // Save any updates to current screen values
                m_settings.Save();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormClosing", "Failed while saving settings", exception: ex);

            #if DEBUG
                throw;
            #endif
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!(m_subscriber is null))
            {
                m_subscriber.Stop();
                m_subscriber.StatusMessage -= Subscriber_StatusMessage;
                m_subscriber.ProcessException -= Subscriber_ProcessException;
                m_subscriber.ConnectionTerminated -= Subscriber_ConnectionTerminated;
                m_subscriber.NewMeasurements -= Subscriber_NewMeasurements;
                m_subscriber.Dispose();
            }

            m_settings?.Dispose();
        }

        private void checkBoxAdapters_CheckedChanged(object sender, EventArgs e)
        {
            LoadAdapters();
        }

        private void InitializeDataSubscriber()
        {
            m_throttledSubscription = new UnsynchronizedSubscriptionInfo(true)
            {
                LagTime = 0.5D,
                LeadTime = 1.0D
            };

            m_subscriber = new DataSubscriber
            {
                ConnectionString = $"server=localhost:{Program.GetGEPPort()}",
                CompressionModes = CompressionModes.TSSC | CompressionModes.GZip,
                OperationalModes = OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressMetadata | OperationalModes.CompressSignalIndexCache | OperationalModes.CompressPayloadData
            };

            m_subscriber.StatusMessage += Subscriber_StatusMessage;
            m_subscriber.ProcessException += Subscriber_ProcessException;
            m_subscriber.ConnectionEstablished += Subscriber_ConnectionEstablished;
            m_subscriber.ConnectionTerminated += Subscriber_ConnectionTerminated;
            m_subscriber.NewMeasurements += Subscriber_NewMeasurements;

            m_subscriber.Initialize();
            m_subscriber.Start();
        }

        private void Subscriber_StatusMessage(object sender, EventArgs<string> e)
        {
            ShowUpdateMessage(e.Argument);
        }

        private void Subscriber_ProcessException(object sender, EventArgs<Exception> e)
        {
            ShowUpdateMessage($"ERROR: {e.Argument}", false);
            m_log.Publish(MessageLevel.Info, "GEP Process Exception", exception: e.Argument);
        }

        private void Subscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            if (m_formClosing)
                return;

            InitiateSubscribe();
        }

        private void Subscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            if (m_formClosing)
                return;

            ShowUpdateMessage("Connection to publisher was terminated, restarting connection cycle...");
            m_subscriber.Start();
        }

        private void Subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (m_formClosing)
                return;

            Dictionary<Guid, IMeasurement> receivedMeasurements = e.Argument.ToDictionary(m => m.ID);

            AssignMeasurements(receivedMeasurements, dataGridViewInputMeasurements);
            AssignMeasurements(receivedMeasurements, dataGridViewOutputMeasurements);
        }

        private void AssignMeasurements(Dictionary<Guid, IMeasurement> receivedMeasurements, DataGridView dataGridView)
        {
            if (!(dataGridView.DataSource is IList source))
                return;

            foreach (Measurement measurement in source)
            {
                if (!receivedMeasurements.TryGetValue(measurement.SignalID, out IMeasurement receivedMeasurement))
                    continue;

                measurement.Value = receivedMeasurement.AdjustedValue;
                measurement.Timestamp = receivedMeasurement.Timestamp;
            }

            BeginInvoke(new Action(dataGridView.Refresh));
        }

        private void LoadDataSource()
        {
            m_dataSource = m_connection.RetrieveDataSet("SELECT * FROM ActiveMeasurement");
            m_dataSource.Tables[0].TableName = "ActiveMeasurements";
        }

        private void LoadAdapters()
        {
            comboBoxAdapters.Items.Clear();
            comboBoxAdapters.Text = "";

            void loadAdapters(ITableOperations table)
            {
                foreach (IIaonAdapter iaonAdapter in table.QueryRecords("AdapterName"))
                {
                    if (iaonAdapter is null)
                        continue;

                    comboBoxAdapters.Items.Add(iaonAdapter);
                }
            }

            if (checkBoxActionAdapters.Checked)
                loadAdapters(new TableOperations<IaonActionAdapter>(m_connection));

            if (checkBoxInputAdapters.Checked)
                loadAdapters(new TableOperations<IaonInputAdapter>(m_connection));

            if (checkBoxOutputAdapters.Checked)
                loadAdapters(new TableOperations<IaonOutputAdapter>(m_connection));

            if (comboBoxAdapters.Items.Count > 0)
                comboBoxAdapters.SelectedIndex = 0;
        }

        private void AssignInputMeasurements(Dictionary<string, string> settings)
        {
            MeasurementKey[] inputMeasurementKeys;

            if (!settings.ContainsKey("inputMeasurementKeys") && settings.TryGetValue("variableList", out string variableList))
            {
                Dictionary<string, string> variables = variableList.ParseKeyValuePairs();

                if (variables.Count > 0)
                    settings["inputMeasurementKeys"] = string.Join(";", variables.Values);
            }

            inputMeasurementKeys = settings.TryGetValue("inputMeasurementKeys", out string setting) ?
                AdapterBase.ParseInputMeasurementKeys(m_dataSource, false, setting) :
                Array.Empty<MeasurementKey>();

            m_inputSignalIDs = inputMeasurementKeys.Select(k => k.SignalID).ToArray();

            string inputSignalIDQuery = m_inputSignalIDs.Length > 0 ?
                $"SignalID IN ({string.Join(", ", m_inputSignalIDs.Select(id => $"'{id}'"))})" :
                null;

            LoadMeasurements(inputSignalIDQuery, dataGridViewInputMeasurements);
        }

        private void AssignOutputMeasurements(Dictionary<string, string> settings)
        {
            MeasurementKey[] outputMeasurementKeys;

            outputMeasurementKeys = settings.TryGetValue("outputMeasurements", out string setting) ?
                AdapterBase.ParseOutputMeasurements(m_dataSource, false, setting).MeasurementKeys() :
                Array.Empty<MeasurementKey>();

            m_outputSignalIDs = outputMeasurementKeys.Select(k => k.SignalID).ToArray();

            string outputSignalIDQuery = m_outputSignalIDs.Length > 0 ?
                $"SignalID IN ({string.Join(", ", m_outputSignalIDs.Select(id => $"'{id}'"))})" :
                null;

            LoadMeasurements(outputSignalIDQuery, dataGridViewOutputMeasurements);
        }

        private void LoadMeasurements(string signalIDQuery, DataGridView dataGridView)
        {
            TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(m_connection);

            Measurement[] measurements = signalIDQuery is null ?
                Array.Empty<Measurement>() :
                measurementTable.QueryRecords("PointTag", new RecordRestriction(signalIDQuery)).ToArray();

            BindingSource source = new BindingSource();

            foreach (Measurement measurement in measurements)
                source.Add(measurement);

            dataGridView.DataSource = null;
            dataGridView.Rows.Clear();
            dataGridView.Refresh();
            dataGridView.DataSource = source;
        }

        private void comboBoxAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_formClosing)
                return;

            if (comboBoxAdapters.SelectedIndex < 0 || !(comboBoxAdapters.SelectedItem is IIaonAdapter iaonAdapter))
                return;

            Dictionary<string, string> settings = iaonAdapter.ConnectionString.ParseKeyValuePairs();

            m_subscriber.Unsubscribe();
            ClearUpdateMessages();

            AssignInputMeasurements(settings);
            AssignOutputMeasurements(settings);
            
            InitiateSubscribe();

            labelAdapterInfo.Text = $"Adapter Info: {iaonAdapter.TypeName} [{iaonAdapter.AssemblyName}]{Environment.NewLine}{Environment.NewLine}Connection String:{Environment.NewLine}{iaonAdapter.ConnectionString}";
        }

        private void InitiateSubscribe()
        {
            bool hasInputIDs = m_inputSignalIDs?.Length > 0;
            bool hasOutputIDs = m_outputSignalIDs?.Length > 0;

            if (!hasInputIDs && !hasOutputIDs)
                return;

            HashSet<Guid> signalIDs = new HashSet<Guid>();

            if (hasInputIDs)
                signalIDs.UnionWith(m_inputSignalIDs);

            if (hasOutputIDs)
                signalIDs.UnionWith(m_outputSignalIDs);

            m_throttledSubscription.FilterExpression = string.Join(";", signalIDs);
            m_subscriber.UnsynchronizedSubscribe(m_throttledSubscription);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            splitContainerMeasurements.SplitterDistance = (int)(splitContainerMeasurements.ClientSize.Width * 0.5);
        }

        private void splitContainerMeasurements_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            textBoxMessageOutput.Width = groupBoxOutputMeasurements.Width;
            labelAdapterInfo.Width = groupBoxInputMeasurements.Width;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearUpdateMessages();
        }

        private void InitializeDataGrid(DataGridView dataGridView)
        {
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();

            dataGridView.Columns.Clear();
            dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView.AutoGenerateColumns = false;
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.ColumnHeadersDefaultCellStyle = cellStyle;
            dataGridView.RowsDefaultCellStyle = cellStyle;
            dataGridView.AlternatingRowsDefaultCellStyle = cellStyle;
            dataGridView.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

            dataGridView.Columns.Add(new DataGridViewColumn
            {
                Name = "PointTag",
                DataPropertyName = "PointTag",
                HeaderText = "Point Tag",
                DefaultCellStyle = cellStyle,
                ReadOnly = true,
                Resizable = DataGridViewTriState.True,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 50,
                CellTemplate = new DataGridViewTextBoxCell()
            });

            dataGridView.Columns.Add(new DataGridViewColumn
            {
                Name = "Value",
                DataPropertyName = "Value",
                DefaultCellStyle = new DataGridViewCellStyle(cellStyle) { Format = "N3" },
                ReadOnly = true,
                Resizable = DataGridViewTriState.True,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 20,
                CellTemplate = new DataGridViewTextBoxCell(),
            });

            dataGridView.Columns.Add(new DataGridViewColumn
            {
                Name = "Timestamp",
                DataPropertyName = "Timestamp",
                DefaultCellStyle = new DataGridViewCellStyle(cellStyle) { Format = TimeTagBase.DefaultFormat },
                ReadOnly = true,
                Resizable = DataGridViewTriState.True,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 30,
                CellTemplate = new DataGridViewTextBoxCell()
            });
        }

        private void ShowUpdateMessage(string message, bool log = true)
        {
            if (m_formClosing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, bool>(ShowUpdateMessage), message, log);
            }
            else
            {
                lock (textBoxMessageOutput)
                    textBoxMessageOutput.AppendText($"{message}{Environment.NewLine}");

                if (log)
                    m_log.Publish(MessageLevel.Info, "StatusMessage", message);
            }
        }

        private void ClearUpdateMessages()
        {
            if (m_formClosing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(ClearUpdateMessages));
            }
            else
            {
                lock (textBoxMessageOutput)
                    textBoxMessageOutput.Text = "";
            }
        }
    }
}
