//*******************************************************************************************************
//  Main.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/05/2010 - Pinal C. Patel
//       Generated original version of source code.
//  03/17/2010 - Pinal C. Patel
//       Updated to include File and Serial output options and option to format plain text output.
//  03/19/2010 - Pinal C. Patel
//       Modified to work with a live archive.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TVA.Communication;
using TVA.Configuration;
using TVA.Historian;
using TVA.Historian.Files;
using TVA.Historian.Packets;
using TVA.IO;
using TVA.Reflection;

namespace HistorianPlaybackUtility
{
    public partial class Main : Form
    {
        #region [ Members ]

        // Nested Types
        private class Metadata
        {
            public string Instance;
            public int PointID;
            public string PointName;
            public string PointDescription;

            public Metadata(MetadataRecord metadata)
            {
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
        private const string ArchiveFileName = "{0}{1}_archive.d";
        private const string MetadataFileName = "{0}{1}_dbase.dat";
        private const string StateFileName = "{0}{1}_startup.dat";
        private const string IntercomFileName = "{0}scratch.dat";
        private const string WatermarkText = "Enter search phrase";

        // Fields
        private bool m_watermarkEnabled;
        private List<Thread> m_activeThreads;
        private ArchiveFile m_archiveFile;
        private IClient m_transmitClient;
        private System.Timers.Timer m_rollverWatcher;
        private ManualResetEvent m_rolloverWaitHandle;

        #endregion

        #region [ Constructors ]

        public Main()
        {
            InitializeComponent();

            // Initialize UI.
            EnableWatermark();
            StartTimeInput.Value = DateTime.UtcNow.AddMinutes(-5);
            EndTimeInput.Value = DateTime.UtcNow;
            foreach (string port in SerialPort.GetPortNames())
            {
                SerialPortInput.Items.Add(port);
            }
            SerialPortInput.SelectedIndex = 0;
            SerialBaudRateInput.SelectedIndex = 4;
            SerialParityInput.SelectedIndex = 0;
            SerialStopBitsInput.SelectedIndex = 1;
            this.Text = string.Format(this.Text, AssemblyInfo.EntryAssembly.Version.ToString(3));

            // Initialize member variables.
            m_activeThreads = new List<Thread>();
            m_archiveFile = new ArchiveFile();
            m_archiveFile.StateFile = new StateFile();
            m_archiveFile.StateFile.FileAccessMode = FileAccess.Read;
            m_archiveFile.IntercomFile = new IntercomFile();
            m_archiveFile.IntercomFile.FileAccessMode = FileAccess.Read;
            m_archiveFile.MetadataFile = new MetadataFile();
            m_archiveFile.MetadataFile.FileAccessMode = FileAccess.Read;
            m_archiveFile.FileAccessMode = FileAccess.Read;
            m_archiveFile.HistoricFileListBuildStart += ArchiveFile_HistoricFileListBuildStart;
            m_archiveFile.HistoricFileListBuildComplete += ArchiveFile_HistoricFileListBuildComplete;
            m_rollverWatcher = new System.Timers.Timer();
            m_rollverWatcher.Interval = 1000;
            m_rollverWatcher.Elapsed += RollverWatcher_Elapsed;
            m_rollverWatcher.Start();
            m_rolloverWaitHandle = new ManualResetEvent(true);
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

        private void Process(object state)
        {
            object[] info = (object[])state;
            string ids = (string)info[0];
            DateTime startTime = (DateTime)info[1];
            DateTime endTime = (DateTime)info[2];
            bool repeatTransmit = (bool)info[3];
            string dataFormat = (string)info[4];
            int sampleRate = (int)info[5];

            int sleepTime = 0;
            if (sampleRate > 0)
                sleepTime = 1000 / sampleRate;

            try
            {
                lock (m_activeThreads)
                {
                    m_activeThreads.Add(Thread.CurrentThread);
                }

                ShowUpdateMessage("Started processing point {0} on thread {1}...", ids, Thread.CurrentThread.ManagedThreadId);
                while (true)
                {
                    foreach (string id in ids.Split(','))
                    {
                        IEnumerable<IDataPoint> data = m_archiveFile.ReadData(int.Parse(id), startTime, endTime);
                        ShowUpdateMessage("Processing measurements for point {0}...", id);
                        int count = 0;
                        byte[] buffer = null;
                        if (string.IsNullOrEmpty(dataFormat))
                        {
                            // Output in binary format.
                            foreach (IDataPoint sample in data)
                            {
                                m_transmitClient.SendAsync(new PacketType1(sample).BinaryImage, 0, PacketType1.ByteCount);
                                count++;
                                if (!m_rolloverWaitHandle.WaitOne(0))
                                    break;                  // Abort for rollover.
                                Thread.Sleep(sleepTime);    // Sleep for throttling.
                            }
                        }
                        else
                        {
                            // Output in plain-text format.
                            foreach (IDataPoint sample in data)
                            {
                                buffer = Encoding.ASCII.GetBytes(string.Format(dataFormat, sample, sample, sample, sample));
                                m_transmitClient.SendAsync(buffer, 0, buffer.Length);
                                count++;
                                if (!m_rolloverWaitHandle.WaitOne(0))
                                    break;                  // Abort for rollover.
                                Thread.Sleep(sleepTime);    // Sleep for throttling.
                            }
                        }
                        ShowUpdateMessage("Processed {0} measurements for point {1}.", count, id);
                    }

                    if (!repeatTransmit)
                        break;
                }
                ShowUpdateMessage("Completed processing point {0} on thread {1}.", ids, Thread.CurrentThread.ManagedThreadId);
            }
            catch (ThreadAbortException)
            {
                ShowUpdateMessage("Aborted processing point {0} on thread {1}.", ids, Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                ShowUpdateMessage("Error processing point {0} on thread {1} - {2}", ids, Thread.CurrentThread.ManagedThreadId, ex.Message);
            }
            finally
            {
                lock (m_activeThreads)
                {
                    m_activeThreads.Remove(Thread.CurrentThread);
                    if (m_activeThreads.Count == 0)
                        this.BeginInvoke((ThreadStart)delegate()
                        {
                            StopProcessing.Visible = false;
                            StartProcessing.Visible = true;
                            SplitContainerTop.Enabled = true;
                        });
                }
            }
        }

        private void ShowUpdateMessage(string message, params object[] args)
        {
            this.BeginInvoke((ThreadStart)delegate()
            {
                MessagesOutput.AppendText(string.Format("[{0}] ", DateTime.Now.ToString()));
                MessagesOutput.AppendText(string.Format(message, args));
                MessagesOutput.AppendText("\r\n");
                Application.DoEvents();
            });
        }

        #region [ Handlers ]

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_archiveFile.StateFile != null)
                m_archiveFile.StateFile.Dispose();

            if (m_archiveFile.IntercomFile != null)
                m_archiveFile.IntercomFile.Dispose();

            if (m_archiveFile.MetadataFile != null)
                m_archiveFile.MetadataFile.Dispose();

            if (m_archiveFile != null)
                m_archiveFile.Dispose();

            if (m_transmitClient != null)
                m_transmitClient.Dispose();

            if (m_rollverWatcher != null)
                m_rollverWatcher.Dispose();

            if (m_rolloverWaitHandle != null)
                m_rolloverWaitHandle.Close();
        }

        private void ArchiveLocationBrowse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Prompt user for primary archive location.
            if (FolderBrowser.ShowDialog(this) == DialogResult.OK)
                ArchiveLocationInput.Text = FolderBrowser.SelectedPath;
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
                        // Capture the instance name.
                        string folder = FilePath.GetDirectoryName(matches[0]);
                        string instance = FilePath.GetFileName(matches[0]).Split('_')[0];

                        // Capture active archive.
                        m_archiveFile.FileName = matches[0];
                        m_archiveFile.StateFile.FileName = string.Format(StateFileName, folder, instance);
                        m_archiveFile.IntercomFile.FileName = string.Format(IntercomFileName, folder);
                        m_archiveFile.MetadataFile.FileName = string.Format(MetadataFileName, folder, instance);

                        // Open the active archive.
                        m_archiveFile.Open();

                        MetadataRecord definition;
                        List<string> previousSelection = new List<string>(ConfigurationFile.Current.Settings.General["Selection", true].ValueAs("").Split(','));
                        IDInput.Items.Clear();
                        for (int i = 1; i <= m_archiveFile.MetadataFile.RecordsOnDisk; i++)
                        {
                            definition = m_archiveFile.MetadataFile.Read(i);
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
                int pointID = -1;
                Metadata definition = null;
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

            // Capture selection.
            DateTime startTime = DateTime.Parse(StartTimeInput.Text);
            DateTime endTime = DateTime.Parse(EndTimeInput.Text);

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
                state.Add(null);
                state.Add(startTime);
                state.Add(endTime);
                state.Add(RepeatDataProcessing.Checked);
                state.Add(OutputPlainTextDataFormat.Text);
                state.Add(int.Parse(ProcessDataAtIntervalSampleRate.Text));
                switch (OutputCannelTabs.SelectedIndex)
                {
                    case 0: // TCP
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=TCP;Server={0}:{1}", TCPServerInput.Text, TCPPortInput.Text));
                        break;
                    case 1: // UDP
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=UDP;Server={0}:{1};Port=-1", UDPServerInput.Text, UDPPortInput.Text));
                        break;
                    case 2: // File
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=File;File={0}", FileNameInput.Text));
                        break;
                    case 3: // Serial
                        m_transmitClient = ClientBase.Create(string.Format("Protocol=Serial;Port={0};BaudRate={1};Parity={2};StopBits={3};DataBits={4};DtrEnable={5};RtsEnable={6}", SerialPortInput.Text, SerialBaudRateInput.Text, SerialParityInput.Text, SerialStopBitsInput.Text, SerialDataBitsInput.Text, SerialDtrEnable.Checked, SerialRtsEnable.Checked));
                        break;
                }
                m_transmitClient.Handshake = false;
                m_transmitClient.MaxConnectionAttempts = 10;
                ShowUpdateMessage("Client initialized!");

                // Connect the newly created client.
                ShowUpdateMessage("Connecting client...");
                m_transmitClient.Connect();
                if (m_transmitClient.CurrentState == ClientState.Connected)
                {
                    // Client connected successfully.
                    ShowUpdateMessage("Client connected!");

                    // Queue all selected points for processing.
                    string selection = "";
                    Metadata definition = null;
                    for (int i = 0; i < IDInput.CheckedItems.Count; i++)
                    {
                        definition = (Metadata)IDInput.CheckedItems[i];
                        if (ProcessDataInParallel.Checked)
                        {
                            ShowUpdateMessage("Queuing processing request for point {0}...", definition.PointID);
                            state[0] = definition.PointID.ToString();
                            ThreadPool.QueueUserWorkItem(Process, state.ToArray());
                        }
                        selection += definition.PointID + ",";   // Update information to be persisted.
                    }
                    selection = selection.TrimEnd(',');

                    if (!ProcessDataInParallel.Checked)
                    {
                        state[0] = selection;
                        ThreadPool.QueueUserWorkItem(Process, state.ToArray());
                    }

                    ConfigurationFile.Current.Settings.General["Selection", true].Value = selection.TrimEnd(',');
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
            OutputPlainTextDataFormat.Text = "{0:I},{1:T},{2:V},{3:Q}";
            OutputPlainTextDataFormat.Enabled = true;
        }

        private void ArchiveFile_HistoricFileListBuildStart(object sender, EventArgs e)
        {
            ShowUpdateMessage("Building list of historic archive files...");
        }

        private void ArchiveFile_HistoricFileListBuildComplete(object sender, EventArgs e)
        {
            ShowUpdateMessage("Completed building list of historic archive files.");
        }

        private void RollverWatcher_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_archiveFile.IntercomFile.IsOpen)
            {
                IntercomRecord record = m_archiveFile.IntercomFile.Read(1);
                // Pause processing.
                if (record.RolloverInProgress && m_archiveFile.IsOpen)
                {
                    m_rolloverWaitHandle.Reset();
                    m_archiveFile.Close();
                    ShowUpdateMessage("Archive rollover in progress...");
                }
                // Resume processing.
                if (!record.RolloverInProgress && !m_archiveFile.IsOpen)
                {
                    m_archiveFile.Open();
                    m_rolloverWaitHandle.Set();
                    ShowUpdateMessage("Archive rollover complete!");
                }
            }
        }
        
        #endregion

        #endregion
    }
}
