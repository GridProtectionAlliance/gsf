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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using AdapterExplorer.Model;
using GSF;
using GSF.Communication;
using GSF.ComponentModel;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.ServiceProcess;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries.UI;
using GSF.Windows.Forms;
using Measurement = AdapterExplorer.Model.Measurement;

namespace AdapterExplorer
{
    public partial class MainForm : SecureForm
    {
        private DataSet m_dataSource;
        private readonly LogPublisher m_log;
        private AdoDataConnection m_database;
        private WindowsServiceClient m_windowsServiceClient;
        private Settings m_settings;
        private readonly ConcurrentQueue<Action> m_subscriberOperationQueue;
        private readonly ShortSynchronizedOperation m_subscriberOperations;
        private DataSubscriber m_subscriber;
        private UnsynchronizedSubscriptionInfo m_throttledSubscription;
        private Guid[] m_inputSignalIDs;
        private Guid[] m_outputSignalIDs;
        private bool m_formLoaded;
        private volatile bool m_formClosing;

        public MainForm()
        {
            InitializeComponent();
            
            m_log = Program.Log;

            m_subscriberOperationQueue = new ConcurrentQueue<Action>();
            m_subscriberOperations = new ShortSynchronizedOperation(() =>
            {
                while (!m_formClosing && m_subscriberOperationQueue.TryDequeue(out Action operation))
                {
                    if (m_subscriber.IsConnected)
                        operation();
                }
            },
            ex => ShowUpdateMessage($"ERROR: Operations queue exception: {ex.Message}"));
        }

        private void QueueSubscriberOperation(Action operation)
        {
            m_subscriberOperationQueue.Enqueue(operation);
            m_subscriberOperations.RunOnceAsync();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Set current principal and attempt host service connection
                CommonFunctions.CurrentPrincipal = SecurityPrincipal;
                CommonFunctions.ServiceConnectionRefreshed += CommonFunctions_ServiceConnectionRefreshed;
                Program.HostNodeID.SetAsCurrentNodeID();
            }
            catch (Exception ex)
            {
                ShowUpdateMessage($"ERROR: Failed to initialize service connection: {ex.Message}");
                m_log.Publish(MessageLevel.Error, "Initialize Service Connection", exception: ex);
            }

            try
            {
                Text += " - " + SecurityPrincipal.Identity.Provider.UserData.LoginID;
                
                m_database = Program.GetDatabaseConnection();

                if (m_database is null)
                    throw new InvalidOperationException("Failed to connect to database.");

                MeasurementKey.EstablishDefaultCache(m_database.Connection, m_database.AdapterType);

                // Load current settings registering a symbolic reference to this form instance for use by default value expressions
                m_settings = new Settings(new Dictionary<string, object> { { "Form", this } }.RegisterSymbols());

                // Restore last window size/location
                this.RestoreLayout();

                LoadDataSource();

                InitializeDataGrid(dataGridViewInputMeasurements);
                InitializeDataGrid(dataGridViewOutputMeasurements);

                InitializeDataSubscriber();
                LoadAdapters();

                SetBottomPanelWidths();

                m_formLoaded = true;
            }
            catch (Exception ex)
            {
                ShowUpdateMessage($"ERROR: Failed during load: {ex.Message}");
                m_log.Publish(MessageLevel.Error, "FormLoad", exception: ex);

            #if DEBUG
                throw;
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
            CommonFunctions.DisconnectWindowsServiceClient();

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

        private void FormElementChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, EventArgs>(FormElementChanged), sender, e);
            }
            else
            {
                if (Visible && m_formLoaded)
                    m_settings?.UpdateProperties();
            }
        }

        private void checkBoxAdapters_CheckedChanged(object sender, EventArgs e)
        {
            if (!Visible || !m_formLoaded)
                return;

            LoadAdapters();
            FormElementChanged(sender, e);
        }

        private void CommonFunctions_ServiceConnectionRefreshed(object sender, EventArgs e)
        {
            ConnectToService();
        }

        private bool IsConnected { get; set; }

        private void ConnectToService()
        {
            ClientHelper helper = m_windowsServiceClient?.Helper;
            ClientBase remotingClient = m_windowsServiceClient?.Helper?.RemotingClient;

            if (!(helper is null))
                helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;

            if (!(remotingClient is null))
            {
                remotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;
                remotingClient.ConnectionEstablished -= RemotingClient_ConnectionEstablished;
            }

            m_windowsServiceClient = CommonFunctions.GetWindowsServiceClient();
            helper = m_windowsServiceClient?.Helper;
            remotingClient = m_windowsServiceClient?.Helper?.RemotingClient;

            if (m_windowsServiceClient is null || helper is null || remotingClient is null)
                return;

            helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
            remotingClient.ConnectionEstablished += RemotingClient_ConnectionEstablished;
            remotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;

            SetConnectionStatus(remotingClient.CurrentState == ClientState.Connected ? Status.Connected : Status.Disconnected);
        }

        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            if (!ClientHelper.TryParseActionableResponse(e.Argument, out string sourceCommand, out _))
                return;

            Guid[] parseSignalIDs()
            {
                string[] parts = (e.Argument?.Message ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    return Array.Empty<Guid>();

                List<Guid> signalIDs = new List<Guid>(parts.Length);

                foreach (string part in parts)
                {
                    if (Guid.TryParse(part, out Guid signalID))
                        signalIDs.Add(signalID);
                }

                return signalIDs.ToArray();
            }

            string command = sourceCommand.ToLower().Trim();

            if (command.Equals("getinputmeasurements"))
            {
                m_inputSignalIDs = parseSignalIDs();
                LoadMeasurements(m_inputSignalIDs, dataGridViewInputMeasurements);
            }
            else if (command.Equals("getoutputmeasurements"))
            {
                m_outputSignalIDs = parseSignalIDs();
                LoadMeasurements(m_outputSignalIDs, dataGridViewOutputMeasurements);
            }
        }

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs e)
        {
            SetConnectionStatus(Status.Disconnected);
        }

        private void RemotingClient_ConnectionEstablished(object sender, EventArgs e)
        {
            SetConnectionStatus(Status.Connected);
        }

        private void SetConnectionStatus(Status status)
        {
            if (m_formClosing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<Status>(SetConnectionStatus), status);
            }
            else
            {
                IsConnected = status == Status.Connected;
                pictureBoxStatus.Image = imageListStatus.Images[(int)status];
                toolTip.SetToolTip(pictureBoxStatus, status.ToString());
            }
        }

        private void LoadDataSource()
        {
            m_dataSource = m_database.RetrieveDataSet("SELECT * FROM ActiveMeasurement");
            m_dataSource.Tables[0].TableName = "ActiveMeasurements";
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
            QueueSubscriberOperation(() => m_subscriber.Start());
        }

        private void Subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (m_formClosing)
                return;

            Dictionary<Guid, IMeasurement> receivedMeasurements = e.Argument.ToDictionary(m => m.ID);

            AssignMeasurements(receivedMeasurements, dataGridViewInputMeasurements);
            AssignMeasurements(receivedMeasurements, dataGridViewOutputMeasurements);
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

            UnsynchronizedSubscriptionInfo subscription = (UnsynchronizedSubscriptionInfo)m_throttledSubscription.Copy();
            subscription.FilterExpression = string.Join(";", signalIDs);

            QueueSubscriberOperation(() => m_subscriber.UnsynchronizedSubscribe(subscription));
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
                loadAdapters(new TableOperations<IaonActionAdapter>(m_database));

            if (checkBoxInputAdapters.Checked)
                loadAdapters(new TableOperations<IaonInputAdapter>(m_database));

            if (checkBoxOutputAdapters.Checked)
                loadAdapters(new TableOperations<IaonOutputAdapter>(m_database));

            if (comboBoxAdapters.Items.Count > 0)
                comboBoxAdapters.SelectedIndex = 0;
        }

        private void AssignInputMeasurements(Dictionary<string, string> settings)
        {
            if (!settings.ContainsKey("inputMeasurementKeys") && settings.TryGetValue("variableList", out string variableList))
            {
                Dictionary<string, string> variables = variableList.ParseKeyValuePairs();

                if (variables.Count > 0)
                    settings["inputMeasurementKeys"] = string.Join(";", variables.Values);
            }

            MeasurementKey[] inputMeasurementKeys = settings.TryGetValue("inputMeasurementKeys", out string setting) ?
                AdapterBase.ParseInputMeasurementKeys(m_dataSource, false, setting) :
                Array.Empty<MeasurementKey>();

            m_inputSignalIDs = inputMeasurementKeys.Select(k => k.SignalID).ToArray();
            LoadMeasurements(m_inputSignalIDs, dataGridViewInputMeasurements);
        }

        private void AssignOutputMeasurements(Dictionary<string, string> settings)
        {
            MeasurementKey[] outputMeasurementKeys = settings.TryGetValue("outputMeasurements", out string setting) ?
                AdapterBase.ParseOutputMeasurements(m_dataSource, false, setting).MeasurementKeys() :
                Array.Empty<MeasurementKey>();

            m_outputSignalIDs = outputMeasurementKeys.Select(k => k.SignalID).ToArray();
            LoadMeasurements(m_outputSignalIDs, dataGridViewOutputMeasurements);
        }

        private void LoadMeasurements(Guid[] signalIDs, DataGridView dataGridView)
        {
            if (m_formClosing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<Guid[], DataGridView>(LoadMeasurements), signalIDs, dataGridView);
            }
            else
            {
                TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(m_database);

                string signalIDQuery = signalIDs.Length == 0 ? null :
                    $"SignalID IN ({string.Join(",", signalIDs.Select(id => $"'{id}'"))})";

                IEnumerable<Measurement> measurements = signalIDQuery is null ?
                    Enumerable.Empty<Measurement>() :
                    measurementTable.QueryRecords("PointTag", new RecordRestriction(signalIDQuery));

                dataGridView.DataSource = null;
                dataGridView.Rows.Clear();
                dataGridView.Refresh();

                // ReSharper disable PossibleMultipleEnumeration
                if (measurements.Any())
                {
                    BindingSource source = new BindingSource();

                    foreach (Measurement measurement in measurements)
                        source.Add(measurement);

                    dataGridView.DataSource = source;
                }
                // ReSharper restore PossibleMultipleEnumeration

                InitiateSubscribe();
            }
        }

        private void comboBoxAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_formClosing)
                return;

            if (comboBoxAdapters.SelectedIndex < 0 || !(comboBoxAdapters.SelectedItem is IIaonAdapter iaonAdapter))
                return;

            Dictionary<string, string> settings = iaonAdapter.ConnectionString?.ParseKeyValuePairs();

            QueueSubscriberOperation(() => m_subscriber.Unsubscribe());
            ClearUpdateMessages();

            if (IsConnected)
            {
                try
                {
                    // When connected to service, request dynamically assigned runtime measurements directly from adapter
                    CommonFunctions.SendCommandToService($"GetInputMeasurements {iaonAdapter.AdapterName} -actionable");
                    CommonFunctions.SendCommandToService($"GetOutputMeasurements {iaonAdapter.AdapterName} -actionable");
                }
                catch (Exception ex)
                {
                    ShowUpdateMessage($"ERROR: Failed to send command to service: {ex.Message}");
                }
            }
            else
            {
                if (!(settings is null))
                {
                    // If service is not running, load configured measurements from database - this will work when service is not connected, but is not exhaustive
                    AssignInputMeasurements(settings);
                    AssignOutputMeasurements(settings);
                }
            }

            textBoxAdapterInfo.Text = $"Adapter Info: {iaonAdapter.TypeName} [{iaonAdapter.AssemblyName}]{Environment.NewLine}{Environment.NewLine}Connection String:{Environment.NewLine}{Environment.NewLine}{iaonAdapter.ConnectionString}";
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            splitContainerMeasurements.SplitterDistance = (int)(splitContainerMeasurements.ClientSize.Width * 0.5);
            SetBottomPanelWidths();
        }

        private void splitContainerMeasurements_SplitterMoved(object sender, SplitterEventArgs e)
        {
            SetBottomPanelWidths();
        }

        private void SetBottomPanelWidths()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            textBoxMessageOutput.Width = groupBoxOutputMeasurements.Width;
            textBoxAdapterInfo.Width = groupBoxInputMeasurements.Width;
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

        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            ShowUpdateMessage($"ERROR: Data grid view exception: {e.Exception?.Message}");
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
