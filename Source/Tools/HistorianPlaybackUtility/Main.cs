//******************************************************************************************************
//  Main.cs - Gbtc
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
//  03/05/2010 - Pinal C. Patel
//       Generated original version of source code.
//  03/17/2010 - Pinal C. Patel
//       Updated to include File and Serial output options and option to format plain text output.
//  03/19/2010 - Pinal C. Patel
//       Modified to work with a live archive.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/12/2010 - Pinal C. Patel
//       Fixed a issue that was preventing all of the exported data from being written to output file.
//  11/08/2011 - J. Ritchie Carroll
//       Added enhanced export formatting and time-sorted outputs.
//  12/18/2011 - J. Ritchie Carroll
//       Set likely default archive locations on initial startup and added check box to allow append to
//       or overwrite modes on file based exports.
//  09/29/2012 - J. Ritchie Carroll
//       Updated to code to use roll-over yielding ArchiveReader.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF;
using GSF.Communication;
using GSF.Configuration;
using GSF.Historian;
using GSF.Historian.Files;
using GSF.Historian.Packets;
using GSF.IO;
using GSF.Parsing;
using GSF.Reflection;
using GSF.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HistorianPlaybackUtility
{
    public partial class Main : Form
    {
        #region [ Members ]

        // Nested Types
        private class Metadata
        {
            public readonly MetadataRecord MetadataRecord;
            public readonly string Instance;
            public readonly int PointID;
            public readonly string PointName;
            public readonly string PointDescription;

            public Metadata(MetadataRecord metadata)
            {
                MetadataRecord = metadata;
                Instance = metadata.PlantCode;
                PointID = metadata.HistorianID;
                PointName = metadata.Name;
                PointDescription = metadata.Description;
            }

            public override string ToString()
            {
                return string.Format("({0}:{1}) {2} | {3}", Instance, PointID, PointName, PointDescription);
            }
        }

        // Constants
        private const string WatermarkText = "Enter search phrase";

        // Fields
        private bool m_watermarkEnabled;
        private long m_transmitStarts;
        private long m_transmitCompletes;
        private long m_transmitExceptions;
        private readonly List<Thread> m_activeThreads;
        private readonly ArchiveReader m_archiveReader;
        private IClient m_transmitClient;
        private string m_lastSelectedArchiveLocation;

        #endregion

        #region [ Constructors ]

        public Main()
        {
            InitializeComponent();

            // Initialize UI.
            EnableWatermark();
            StartTimeInput.Value = DateTime.UtcNow.AddMinutes(-5);
            EndTimeInput.Value = DateTime.UtcNow;

            // Add version number to title
            this.Text = string.Format(this.Text, AssemblyInfo.EntryAssembly.Version.ToString(3));

            foreach (string port in SerialPort.GetPortNames())
            {
                SerialPortInput.Items.Add(port);
            }

            if (SerialPortInput.Items.Count > 0)
            {
                SerialPortInput.SelectedIndex = 0;
                SerialBaudRateInput.SelectedIndex = 4;
                SerialParityInput.SelectedIndex = 0;
                SerialStopBitsInput.SelectedIndex = 1;
            }
            else
            {
                // No serial ports where found on this machine so the option for serial output will be removed
                OutputChannelTabs.TabPages.Remove(SerialSettingsTab);
            }

            Application.DoEvents();

            // Initialize member variables.
            m_activeThreads = new List<Thread>();
            m_archiveReader = new ArchiveReader();
            m_archiveReader.RolloverStart += m_archiveReader_RolloverStart;
            m_archiveReader.RolloverComplete += m_archiveReader_RolloverComplete;
            m_archiveReader.HistoricFileListBuildStart += m_archiveReader_HistoricFileListBuildStart;
            m_archiveReader.HistoricFileListBuildComplete += m_archiveReader_HistoricFileListBuildComplete;
            m_archiveReader.HistoricFileListBuildException += m_archiveReader_HistoricFileListBuildException;
            m_archiveReader.DataReadException += m_archiveReader_DataReadException;
            m_lastSelectedArchiveLocation = ConfigurationFile.Current.Settings.General["ArchiveLocation", true].ValueAs("");

            // If last selected archive is not defined, try to a few default selections
            if (string.IsNullOrWhiteSpace(m_lastSelectedArchiveLocation) || !Directory.Exists(m_lastSelectedArchiveLocation))
            {
                // See if a local archive folder exists with a valid archive
                m_lastSelectedArchiveLocation = FilePath.GetAbsolutePath("Archive");

                if (!Directory.Exists(m_lastSelectedArchiveLocation) || Directory.GetFiles(m_lastSelectedArchiveLocation, "*_archive.d").Length == 0)
                {
                    // See if a local statistics folder exists with a valid archive
                    m_lastSelectedArchiveLocation = FilePath.GetAbsolutePath("Statistics");

                    // If neither of these folders exist, just leave setting blank
                    if (!Directory.Exists(m_lastSelectedArchiveLocation) || Directory.GetFiles(m_lastSelectedArchiveLocation, "*_archive.d").Length == 0)
                        m_lastSelectedArchiveLocation = "";
                }
            }

            // Update archive location text box to contain the archive location from the configuration file.
            ArchiveLocationInput.Text = m_lastSelectedArchiveLocation;
        }

        #endregion

        #region [ Methods ]

        private void EnableWatermark()
        {
            if (SearchPhraseInput.Text == "")
            {
                SearchPhraseInput.Text = WatermarkText;
                SearchPhraseInput.ForeColor = SystemColors.GrayText;
                m_watermarkEnabled = true;
            }
        }

        private void DisableWatermark()
        {
            if (SearchPhraseInput.Text == WatermarkText)
            {
                SearchPhraseInput.Text = "";
                SearchPhraseInput.ForeColor = SystemColors.WindowText;
                m_watermarkEnabled = false;
            }
        }

        private void ProcessSequential(object state)
        {
            object[] info = (object[])state;
            string ids = (string)info[0];

            lock (m_activeThreads)
            {
                m_activeThreads.Add(Thread.CurrentThread);
            }

            try
            {
                foreach (string value in ids.Split(','))
                {
                    Thread workerThread = new Thread(Process);
                    info[0] = value;
                    workerThread.Start(info);
                    workerThread.Join();
                }
            }
            finally
            {
                lock (m_activeThreads)
                {
                    m_activeThreads.Remove(Thread.CurrentThread);
                    if (m_activeThreads.Count == 0)
                    {
                        ShowUpdateMessage("Waiting for pending transmissions to complete...");
                        while (m_transmitStarts != (m_transmitCompletes + m_transmitExceptions))
                        {
                            Thread.Sleep(1000);
                        }
                        ShowUpdateMessage("Transmissions complete ({0} errors).", m_transmitExceptions);

                        m_transmitClient.SendDataStart -= m_transmitClient_SendDataStart;
                        m_transmitClient.SendDataComplete -= m_transmitClient_SendDataComplete;
                        m_transmitClient.SendDataException -= m_transmitClient_SendDataException;
                        m_transmitClient.Dispose();

                        this.BeginInvoke((ThreadStart)delegate
                            {
                                StopProcessing.Visible = false;
                                StartProcessing.Visible = true;
                                SplitContainerTop.Enabled = true;
                            });
                    }
                }
            }
        }

        private void Process(object state)
        {
            object[] info = (object[])state;
            string ids = (string)info[0];
            DateTime startTime = (DateTime)info[1];
            DateTime endTime = (DateTime)info[2];
            bool repeatTransmit = (bool)info[3];
            string dataFormat = (string)info[4];
            int sampleRate = (int)info[5];
            Dictionary<int, Metadata> metadata = info[6] as Dictionary<int, Metadata>;

            int sleepTime = 0;
            if (sampleRate > 0)
                sleepTime = 1000 / sampleRate;

            try
            {
                lock (m_activeThreads)
                {
                    m_activeThreads.Add(Thread.CurrentThread);
                }

                ShowUpdateMessage("Processing \"{0}\"...", ids);

                while (true)
                {
                    int id;
                    List<int> historianIDs = new List<int>();

                    foreach (string value in ids.Split(','))
                    {
                        if (int.TryParse(value, out id))
                            historianIDs.Add(id);
                    }

                    ShowUpdateMessage("Reading measurements...");
                    IEnumerable<IDataPoint> data = m_archiveReader.ReadData(historianIDs, startTime, endTime);

                    int count = 0;
                    byte[] buffer;

                    if (string.IsNullOrEmpty(dataFormat))
                    {
                        // Output in binary format.
                        foreach (IDataPoint sample in data)
                        {
                            m_transmitClient.SendAsync(new PacketType1(sample).BinaryImage(), 0, PacketType1.FixedLength);
                            count++;

                            // Sleep for throttling, if requested
                            if (sleepTime > 0)
                                Thread.Sleep(sleepTime);
                        }
                    }
                    else
                    {
                        Metadata localMetadata;
                        object[] args = new object[dataFormat.Split('{').Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => int.Parse(value.Split(':')[0])).Max() + 1];

                        // Output in plain-text format.
                        foreach (IDataPoint sample in data)
                        {
                            // Associate metadata record with this data point if it's available                            
                            if (metadata != null && metadata.TryGetValue(sample.HistorianID, out localMetadata))
                                sample.Metadata = localMetadata.MetadataRecord;

                            // Initialize arguments array with current sample for formatting
                            for (int i = 0; i < args.Length; i++)
                            {
                                args[i] = sample;
                            }

                            buffer = Encoding.ASCII.GetBytes(string.Format(dataFormat, args));

                            m_transmitClient.SendAsync(buffer, 0, buffer.Length);
                            count++;

                            // Sleep for throttling, if requested
                            if (sleepTime > 0)
                                Thread.Sleep(sleepTime);
                        }
                    }
                    ShowUpdateMessage("Read {0} measurements.", count);

                    if (!repeatTransmit)
                        break;
                }

                ShowUpdateMessage("Completed processing \"{0}\".", ids);
            }
            catch (ThreadAbortException)
            {
                ShowUpdateMessage("Aborted processing \"{0}\".", ids);
            }
            catch (Exception ex)
            {
                ShowUpdateMessage("Error processing \"{0}\": {1}", ids, ex.Message);
            }
            finally
            {
                lock (m_activeThreads)
                {
                    m_activeThreads.Remove(Thread.CurrentThread);
                    if (m_activeThreads.Count == 0)
                    {
                        ShowUpdateMessage("Waiting for pending transmissions to complete...");
                        while (m_transmitStarts != (m_transmitCompletes + m_transmitExceptions))
                        {
                            Thread.Sleep(1000);
                        }
                        ShowUpdateMessage("Transmissions complete ({0} errors).", m_transmitExceptions);

                        m_transmitClient.SendDataStart -= m_transmitClient_SendDataStart;
                        m_transmitClient.SendDataComplete -= m_transmitClient_SendDataComplete;
                        m_transmitClient.SendDataException -= m_transmitClient_SendDataException;
                        m_transmitClient.Dispose();

                        this.BeginInvoke((ThreadStart)delegate
                            {
                                StopProcessing.Visible = false;
                                StartProcessing.Visible = true;
                                SplitContainerTop.Enabled = true;
                            });
                    }
                }
            }
        }

        private void ShowUpdateMessage(string message, params object[] args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((ThreadStart)delegate
                    {
                        StringBuilder outputText = new StringBuilder();

                        outputText.AppendFormat("[{0}] ", DateTime.Now.ToString());
                        outputText.AppendFormat(message, args);
                        outputText.Append("\r\n");

                        MessagesOutput.AppendText(outputText.ToString());
                        Application.DoEvents();
                    });
            }
        }

        private bool ValidateOutputFormat()
        {
            bool valid = false;
            try
            {
                object[] args = new object[OutputPlainTextDataFormat.Text.Split('{').Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => int.Parse(value.Split(':')[0])).Max() + 1];
                ArchiveDataPoint sample = new ArchiveDataPoint(1);

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = sample;
                }

                Encoding.ASCII.GetBytes(string.Format(OutputPlainTextDataFormat.Text, args));

                valid = true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Invalid Data Output Format: ");
                sb.AppendLine(ex.Message);
                sb.AppendLine();
                sb.AppendLine("A valid format, for example, is:");
                sb.AppendLine("{0:Source}:{1:ID},{2:Name},{3:Synonym1},{4:Time},{5:UnixTime},{6:Value},{7:Quality},{8:Description}");
                MessageBox.Show(sb.ToString(), "Invalid Output Data Format", MessageBoxButtons.OK);
                OutputPlainTextDataFormat.Focus();
            }

            return valid;
        }

        #region [ Handlers ]

        private void Main_Load(object sender, EventArgs e)
        {
            this.RestoreLayout();
            OutputPlainTextData.Checked = true;
            OutputChannelTabs.SelectedIndex = 2;
            FileNameInput.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Output.csv");
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopProcessing_Click(sender, EventArgs.Empty);

            if (m_lastSelectedArchiveLocation != null)
            {
                ConfigurationFile.Current.Settings.General["ArchiveLocation", true].Value = m_lastSelectedArchiveLocation;
                ConfigurationFile.Current.Save();
            }

            if (m_archiveReader.StateFile != null)
                m_archiveReader.StateFile.Dispose();

            if (m_archiveReader.IntercomFile != null)
                m_archiveReader.IntercomFile.Dispose();

            if (m_archiveReader.MetadataFile != null)
                m_archiveReader.MetadataFile.Dispose();

            if (m_archiveReader != null)
            {
                m_archiveReader.RolloverStart -= m_archiveReader_RolloverStart;
                m_archiveReader.RolloverComplete -= m_archiveReader_RolloverComplete;
                m_archiveReader.HistoricFileListBuildStart -= m_archiveReader_HistoricFileListBuildStart;
                m_archiveReader.HistoricFileListBuildComplete -= m_archiveReader_HistoricFileListBuildComplete;
                m_archiveReader.DataReadException -= m_archiveReader_DataReadException;
                m_archiveReader.Dispose();
            }

            this.SaveLayout();
        }

        private void ArchiveLocationBrowse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Restore the last selected archive location.
            FolderBrowser.SelectedPath = m_lastSelectedArchiveLocation;

            // Prompt user for primary archive location.
            if (FolderBrowser.ShowDialog(this) == DialogResult.OK)
            {
                m_lastSelectedArchiveLocation = FolderBrowser.SelectedPath;
                ArchiveLocationInput.Text = FolderBrowser.SelectedPath;
            }
        }

        private void ArchiveLocationInput_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ArchiveLocationInput.Text) && Directory.Exists(ArchiveLocationInput.Text))
            {
                // Specified directory is a valid one.
                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    string[] matches = Directory.GetFiles(ArchiveLocationInput.Text, "*_archive.d");

                    if (matches.Length > 0)
                    {
                        // Open the active archive
                        m_archiveReader.Open(matches[0]);

                        MetadataRecord definition;
                        List<string> previousSelection = new List<string>(ConfigurationFile.Current.Settings.General["Selection", true].ValueAs("").Split(','));

                        IDInput.Items.Clear();

                        for (int i = 1; i <= m_archiveReader.MetadataFile.RecordsOnDisk; i++)
                        {
                            definition = m_archiveReader.MetadataFile.Read(i);

                            if (definition.GeneralFlags.Enabled)
                            {
                                IDInput.Items.Add(new Metadata(definition));

                                if (previousSelection.Contains(definition.HistorianID.ToString()))
                                    IDInput.SetItemChecked(IDInput.Items.Count - 1, true);

                                Application.DoEvents();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowUpdateMessage("Error initializing application - {0}", ex.Message);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void SearchPhraseInput_MouseClick(object sender, MouseEventArgs e)
        {
            if (m_watermarkEnabled)
                DisableWatermark();
        }

        private void SearchPhraseInput_Leave(object sender, EventArgs e)
        {
            if (!m_watermarkEnabled)
                EnableWatermark();
        }

        private void SearchPhraseInput_TextChanged(object sender, EventArgs e)
        {
            if (!m_watermarkEnabled)
                EnableWatermark();
        }

        private void SearchPhraseFind_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                // Search for points matching the search criteria.
                int pointID;
                Metadata definition;
                string searchPhrase = SearchPhraseInput.Text.ToLower();
                int.TryParse(searchPhrase, out pointID);

                this.Cursor = Cursors.WaitCursor;

                ShowUpdateMessage("Searching for points matching \"{0}\"...", searchPhrase);

                for (int i = 0; i < IDInput.Items.Count; i++)
                {
                    definition = (Metadata)IDInput.Items[i];
                    if (definition.PointID == pointID ||
                        definition.PointName.ToLower().Contains(searchPhrase) ||
                        definition.PointDescription.ToLower().Contains(searchPhrase))
                    {
                        IDInput.SetItemChecked(i, true);
                    }
                }

                ShowUpdateMessage("Found {0} point(s) matching \"{1}\".", IDInput.CheckedIndices.Count, searchPhrase);
            }
            catch (Exception ex)
            {
                ShowUpdateMessage("Error finding points - {0}", ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void SearchPhraseClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (int index in IDInput.CheckedIndices)
            {
                IDInput.SetItemChecked(index, false);
            }
        }

        private void FileNameBrowse_Click(object sender, EventArgs e)
        {
            if (SaveFile.ShowDialog(this) == DialogResult.OK)
                FileNameInput.Text = SaveFile.FileName;
        }

        private void StartProcessing_Click(object sender, EventArgs e)
        {
            // Validate selection.
            MessagesOutput.Clear();

            if (IDInput.CheckedIndices.Count == 0)
            {
                ShowUpdateMessage("No points selected for processing.");
                return;
            }

            if (!ValidateOutputFormat())
                return;

            // Capture selection.
            DateTime startTime = DateTime.Parse(StartTimeInput.Text, CultureInfo.InvariantCulture.DateTimeFormat);
            DateTime endTime = DateTime.Parse(EndTimeInput.Text, CultureInfo.InvariantCulture.DateTimeFormat);

            try
            {
                StartProcessing.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Dispose previously created client.
                if (m_transmitClient != null)
                    m_transmitClient.Dispose();

                // Create new client.
                ShowUpdateMessage("Initializing client...");

                List<object> state = new List<object>();
                Dictionary<int, Metadata> metadata = new Dictionary<int, Metadata>();

                state.Add(null);
                state.Add(startTime);
                state.Add(endTime);
                state.Add(RepeatDataProcessing.Checked);
                state.Add(OutputPlainTextDataFormat.Text);
                state.Add(int.Parse(ProcessDataAtIntervalSampleRate.Text));
                state.Add(metadata);

                m_transmitStarts = 0;
                m_transmitCompletes = 0;
                m_transmitExceptions = 0;

                switch (OutputChannelTabs.SelectedIndex)
                {
                    case 0: // TCP
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=TCP;Server={0}:{1}", TCPServerInput.Text, TCPPortInput.Text));
                        break;
                    case 1: // UDP
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=UDP;Server={0}:{1};Port=-1", UDPServerInput.Text, UDPPortInput.Text));
                        break;
                    case 2: // File
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=File;File={0}", FileNameInput.Text));
                        if (!AppendToExisting.Checked)
                        {
                            FileClient client = m_transmitClient as FileClient;

                            if ((object)client != null)
                                client.FileOpenMode = FileMode.Create;
                        }
                        break;
                    case 3: // Serial
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=Serial;Port={0};BaudRate={1};Parity={2};StopBits={3};DataBits={4};DtrEnable={5};RtsEnable={6}", SerialPortInput.Text, SerialBaudRateInput.Text, SerialParityInput.Text, SerialStopBitsInput.Text, SerialDataBitsInput.Text, SerialDtrEnable.Checked, SerialRtsEnable.Checked));
                        break;
                }

                if ((object)m_transmitClient != null)
                {
                    m_transmitClient.MaxConnectionAttempts = 10;
                    m_transmitClient.SendDataStart += m_transmitClient_SendDataStart;
                    m_transmitClient.SendDataComplete += m_transmitClient_SendDataComplete;
                    m_transmitClient.SendDataException += m_transmitClient_SendDataException;

                    ShowUpdateMessage("Client initialized.");

                    // Connect the newly created client.
                    ShowUpdateMessage("Connecting client...");

                    m_transmitClient.Connect();

                    if (m_transmitClient.CurrentState == ClientState.Connected)
                    {
                        // Client connected successfully.
                        ShowUpdateMessage("Client connected.");

                        // Queue all selected points for processing.
                        StringBuilder selection = new StringBuilder();
                        Metadata definition;

                        for (int i = 0; i < IDInput.CheckedItems.Count; i++)
                        {
                            if (selection.Length > 0)
                                selection.Append(',');

                            definition = (Metadata)IDInput.CheckedItems[i];
                            selection.Append(definition.PointID);
                            metadata.Add(definition.PointID, definition);
                        }

                        state[0] = selection.ToString();

                        if (ProcessDataInParallel.Checked)
                        {
                            ThreadPool.QueueUserWorkItem(Process, state.ToArray());
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(ProcessSequential, state.ToArray());
                        }

                        ConfigurationFile.Current.Settings.General["Selection", true].Value = selection.ToString();
                        ConfigurationFile.Current.Save();

                        StopProcessing.Visible = true;
                        StartProcessing.Visible = false;
                        SplitContainerTop.Enabled = false;
                    }
                    else
                    {
                        ShowUpdateMessage("Connection timeout.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowUpdateMessage("Error starting processing - {0}", ex.Message);
            }
            finally
            {
                StartProcessing.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void StopProcessing_Click(object sender, EventArgs e)
        {
            lock (m_activeThreads)
            {
                foreach (Thread activeThread in m_activeThreads)
                {
                    activeThread.Abort();
                }
            }
        }

        private void ProcessDataFullSpeed_CheckedChanged(object sender, EventArgs e)
        {
            ProcessDataAtIntervalSampleRate.Text = "0";
            ProcessDataAtIntervalSampleRate.Enabled = false;
        }

        private void ProcessDataAtInterval_CheckedChanged(object sender, EventArgs e)
        {
            ProcessDataAtIntervalSampleRate.Text = "30";
            ProcessDataAtIntervalSampleRate.Enabled = true;
        }

        private void OutputBinaryData_CheckedChanged(object sender, EventArgs e)
        {
            OutputPlainTextDataFormat.Text = "";
            OutputPlainTextDataFormat.Enabled = false;
        }

        private void OutputPlainTextData_CheckedChanged(object sender, EventArgs e)
        {
            OutputPlainTextDataFormat.Text = "{0:Source}:{1:ID},{2:Name},{3:Synonym1},{4:Time},{5:UnixTime},{6:Value},{7:Quality},{8:Description}\r\n";
            OutputPlainTextDataFormat.Enabled = true;
        }

        private void m_archiveReader_HistoricFileListBuildStart(object sender, EventArgs e)
        {
            ShowUpdateMessage("Building list of historic archive files...");
        }

        private void m_archiveReader_HistoricFileListBuildComplete(object sender, EventArgs e)
        {
            ShowUpdateMessage("Completed building list of historic archive files.");
        }

        private void m_archiveReader_HistoricFileListBuildException(object sender, EventArgs<Exception> e)
        {
            ShowUpdateMessage(e.Argument.Message);
        }

        private void m_archiveReader_DataReadException(object sender, EventArgs<Exception> e)
        {
            ShowUpdateMessage("Exception encountered during data read: " + e.Argument.Message);
        }

        private void m_archiveReader_RolloverStart(object sender, EventArgs e)
        {
            ShowUpdateMessage("Archive rollover in progress...");
        }

        private void m_archiveReader_RolloverComplete(object sender, EventArgs e)
        {
            ShowUpdateMessage("Archive rollover complete.");
        }

        private void m_transmitClient_SendDataStart(object sender, EventArgs e)
        {
            m_transmitStarts++;
        }

        private void m_transmitClient_SendDataComplete(object sender, EventArgs e)
        {
            m_transmitCompletes++;
        }

        private void m_transmitClient_SendDataException(object sender, EventArgs<Exception> e)
        {
            m_transmitExceptions++;
        }

        #endregion

        #endregion
    }
}
