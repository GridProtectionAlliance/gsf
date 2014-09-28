//******************************************************************************************************
//  ConnectionStringBuilder.xaml.cs - Gbtc
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
//  06/10/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.Modal
{
    /// <summary>
    /// Interaction logic for ConnectionStringBuilder.xaml
    /// </summary>
    public partial class ConnectionStringBuilder : Window
    {
        #region [ Enumerations ]

        /// <summary>
        /// Type of transport protocol enumeration.
        /// </summary>
        public enum TransportProtocol
        {
            /// <summary>
            /// Defines TCP tranasport protocol.
            /// </summary>
            /// <remarks>
            /// Use this to create connection string for TCP connection.
            /// </remarks>
            tcp,
            /// <summary>
            /// Defines UDP tranasport protocol.
            /// </summary>
            /// <remarks>
            /// Use this to create connection string for UDP connection.
            /// </remarks>
            udp,
            /// <summary>
            /// Defines Serial tranasport protocol.
            /// </summary>
            /// <remarks>
            /// Use this to create connection string for Serial connection.
            /// </remarks>
            serial,
            /// <summary>
            /// Defines file based connection.
            /// </summary>
            /// <remarks>
            /// Use this to create connection string for File based connection.
            /// </remarks>
            file,
            /// <summary>
            /// Defines UDP Server connection.
            /// </summary>
            /// <remarks>
            /// Use this to create connection string for TCP Server connection.
            /// </remarks>
            udpserver
        }

        /// <summary>
        /// Type of connection enumeration.
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// Defines connection to a device.
            /// </summary>
            /// <remarks>
            /// Use this to connect to a device.
            /// </remarks>
            DeviceConnection,
            /// <summary>
            /// Defines data channel for communication.
            /// </summary>
            /// <remarks>
            /// Use this to generate string to setup data channel.
            /// </remarks>
            DataChannel,
            /// <summary>
            /// Defines command channel for communication.
            /// </summary>
            /// <remarks>
            /// Use this to generate string to setup command channel.
            /// </remarks>
            CommandChannel,
            /// <summary>
            /// Defines alternate command channel.
            /// </summary>
            /// <remarks>
            /// Use this to generate string to setup alternate command channel.
            /// </remarks>
            AlternateCommandChannel
        }

        #endregion

        #region [ Members ]

        private readonly ConnectionType m_connectionType;
        private string m_connectionString;
        private Dictionary<string, string> m_keyvaluepairs;
        private bool m_pgConnection;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a flag indicating if the connection is to Phasor Gateway.
        /// </summary>
        public bool PgConnection
        {
            get
            {
                return m_pgConnection;
            }
            set
            {
                m_pgConnection = value;
            }
        }

        /// <summary>
        /// Gets or sets connection string from the parameters defined on the UI.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
            }
        }

        #endregion

        #region [ Constructor]

        /// <summary>
        /// Creates a new instance of <see cref="ConnectionStringBuilder"/> class.
        /// </summary>
        public ConnectionStringBuilder(ConnectionType connectionType)
        {
            InitializeComponent();
            m_connectionType = connectionType;
            this.Loaded += ConnectionStringBuilder_Loaded;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles loaded event of the window.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ConnectionStringBuilder_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_connectionType == ConnectionType.CommandChannel)
            {
                TabItemTCP.Visibility = Visibility.Visible;
                TabItemUDP.Visibility = Visibility.Collapsed;
                TabItemSerial.Visibility = Visibility.Collapsed;
                TabItemFile.Visibility = Visibility.Collapsed;
                TabItemUdpServer.Visibility = Visibility.Collapsed;
                TextBoxHostIP.Visibility = Visibility.Collapsed;
                CheckboxEstablishServer.Visibility = Visibility.Collapsed;
            }
            else if (m_connectionType == ConnectionType.AlternateCommandChannel)
            {
                TabItemTCP.Visibility = Visibility.Visible;
                TabItemUDP.Visibility = Visibility.Collapsed;
                if (PgConnection)
                    TabItemSerial.Visibility = Visibility.Collapsed;
                else
                    TabItemSerial.Visibility = Visibility.Visible;
                TabItemFile.Visibility = Visibility.Collapsed;
                TabItemUdpServer.Visibility = Visibility.Collapsed;
            }
            else if (m_connectionType == ConnectionType.DataChannel)
            {
                TabControlOptions.SelectedIndex = 4;
                TabItemTCP.Visibility = Visibility.Collapsed;
                TabItemUDP.Visibility = Visibility.Collapsed;
                TabItemSerial.Visibility = Visibility.Collapsed;
                TabItemFile.Visibility = Visibility.Collapsed;
                TabItemUdpServer.Visibility = Visibility.Visible;
            }
            else
            {
                TabItemTCP.Visibility = Visibility.Visible;
                TabItemUDP.Visibility = Visibility.Visible;
                TabItemSerial.Visibility = Visibility.Visible;
                TabItemFile.Visibility = Visibility.Visible;
                TabItemUdpServer.Visibility = Visibility.Collapsed;
            }

            m_keyvaluepairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            ComboboxParity.ItemsSource = CommonFunctions.GetParities();
            ComboboxStopBits.ItemsSource = CommonFunctions.GetStopBits();

            if (ComboboxParity.Items.Count > 0)
                ComboboxParity.SelectedIndex = 0;
            if (ComboboxStopBits.Items.Count > 1)
                ComboboxStopBits.SelectedIndex = 1;
            else
                ComboboxStopBits.SelectedIndex = 0;

            ComboboxPort.Items.Add("COM1");
            ComboboxPort.Items.Add("COM2");
            ComboboxPort.Items.Add("COM3");
            ComboboxPort.Items.Add("COM4");
            ComboboxPort.Items.Add("COM5");
            ComboboxPort.Items.Add("COM6");
            ComboboxPort.Items.Add("COM7");
            ComboboxPort.Items.Add("COM8");
            ComboboxPort.Items.Add("COM9");
            ComboboxPort.Items.Add("COM10");
            ComboboxPort.SelectedIndex = 0;

            // Populate Baud Rate Dropdown in Serial Tab
            ComboboxBaudRate.Items.Add(115200);
            ComboboxBaudRate.Items.Add(57600);
            ComboboxBaudRate.Items.Add(38400);
            ComboboxBaudRate.Items.Add(19200);
            ComboboxBaudRate.Items.Add(9600);
            ComboboxBaudRate.Items.Add(4800);
            ComboboxBaudRate.Items.Add(2400);
            ComboboxBaudRate.Items.Add(1200);
            ComboboxBaudRate.SelectedIndex = 0;

            CheckboxForceIPv4.IsChecked = IsolatedStorageManager.ReadFromIsolatedStorage("ForceIPv4") == null ? true : Convert.ToBoolean(IsolatedStorageManager.ReadFromIsolatedStorage("ForceIPv4"));

            // populate connection info	if already provided from the parent window
            ParseConnectionString();
        }

        /// <summary>
        /// Hanldes click event of the save button on UdpServer tab.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSaveUdpServer_Click(object sender, RoutedEventArgs e)
        {

            m_keyvaluepairs["port"] = "-1";

            string clients = string.Empty;
            if (!string.IsNullOrEmpty(TextBoxHostIP1.Text))
                clients += FormatIP(TextBoxHostIP1.Text) + ":" + (string.IsNullOrEmpty(TextBoxHostPort1.Text) ? "4712" : TextBoxHostPort1.Text) + ",";

            if (!string.IsNullOrEmpty(TextBoxHostIP2.Text))
                clients += FormatIP(TextBoxHostIP2.Text) + ":" + (string.IsNullOrEmpty(TextBoxHostPort2.Text) ? "4712" : TextBoxHostPort2.Text) + ",";

            if (!string.IsNullOrEmpty(TextBoxHostIP3.Text))
                clients += FormatIP(TextBoxHostIP3.Text) + ":" + (string.IsNullOrEmpty(TextBoxHostPort3.Text) ? "4712" : TextBoxHostPort3.Text) + ",";

            while (clients.EndsWith(","))
                clients = clients.Substring(0, clients.LastIndexOf(','));

            m_keyvaluepairs["clients"] = clients;

            SetConnectionString(TransportProtocol.udpserver);
            this.DialogResult = true;
        }

        /// <summary>
        /// Hanldes click event of the save button on File tab.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            CheckboxForceIPv4.IsChecked = false;	//as IPv4 doesn't matter for file protocol
            m_keyvaluepairs["file"] = TextBoxFile.Text;
            m_keyvaluepairs["definedframerate"] = String.IsNullOrEmpty(TextBoxFrameRate.Text) ? "30" : TextBoxFrameRate.Text;
            m_keyvaluepairs["simulatetimestamp"] = CheckboxSimulateTimeStamp.IsChecked.ToString().ToLower();
            m_keyvaluepairs["autorepeatfile"] = CheckboxAutoRepeat.IsChecked.ToString().ToLower();

            SetConnectionString(TransportProtocol.file);
            this.DialogResult = true;
        }

        /// <summary>
        /// Hanldes click event of the browse button on File tab.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PMU Capture File (*.PmuCapture)|*.PmuCapture|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TextBoxFile.Text = openFileDialog.FileName;
        }

        /// <summary>
        /// Hanldes click event of the save button on Serial tab.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSaveSerial_Click(object sender, RoutedEventArgs e)
        {
            m_keyvaluepairs["port"] = ComboboxPort.SelectedItem.ToString();
            m_keyvaluepairs["baudrate"] = ComboboxBaudRate.SelectedItem.ToString();
            m_keyvaluepairs["parity"] = ComboboxParity.SelectedItem.ToString();
            m_keyvaluepairs["stopbits"] = ComboboxStopBits.SelectedItem.ToString();
            m_keyvaluepairs["databits"] = String.IsNullOrEmpty(TextBoxDataBits.Text) ? "8" : TextBoxDataBits.Text;
            m_keyvaluepairs["dtrenable"] = CheckboxDTR.IsChecked.ToString().ToLower();
            m_keyvaluepairs["rtsenable"] = CheckboxRTS.IsChecked.ToString().ToLower();

            SetConnectionString(TransportProtocol.serial);
            this.DialogResult = true;
        }

        /// <summary>
        /// Hanldes click event of the save button on UDP tab.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSaveUDP_Click(object sender, RoutedEventArgs e)
        {

            m_keyvaluepairs["localport"] = String.IsNullOrEmpty(TextBoxLocalPort.Text) ? "4712" : TextBoxLocalPort.Text;

            if ((bool)CheckboxEnableMulticast.IsChecked)
            {
                m_keyvaluepairs["server"] = FormatIP(TextBoxHostIPUdp.Text);
                m_keyvaluepairs["remoteport"] = TextBoxRemotePort.Text;
            }
            else
            {
                if (m_keyvaluepairs.ContainsKey("server"))
                    m_keyvaluepairs.Remove("server");

                if (m_keyvaluepairs.ContainsKey("remoteport"))
                    m_keyvaluepairs.Remove("remoteport");
            }

            SetConnectionString(TransportProtocol.udp);
            this.DialogResult = true;
        }

        /// <summary>
        /// Hanldes click event of the save button on TCP tab.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSaveTCP_Click(object sender, RoutedEventArgs e)
        {
            m_keyvaluepairs["port"] = String.IsNullOrEmpty(TextBoxPort.Text) ? "4712" : TextBoxPort.Text;
            m_keyvaluepairs["maxSendQueueSize"] = String.IsNullOrEmpty(TextBoxMaxSendQueueSize.Text) ? "-1" : TextBoxMaxSendQueueSize.Text;

            if (m_connectionType != ConnectionType.CommandChannel)
            {
                string hostIP = String.IsNullOrEmpty(TextBoxHostIP.Text) ? "127.0.0.1" : TextBoxHostIP.Text;
                m_keyvaluepairs["server"] = FormatIP(hostIP);
                m_keyvaluepairs["islistener"] = CheckboxEstablishServer.IsChecked.ToString().ToLower();

                if (PgConnection)
                {
                    m_keyvaluepairs["server"] = m_keyvaluepairs["server"] + ":" + m_keyvaluepairs["port"];
                    m_keyvaluepairs.Remove("port");
                }
            }

            SetConnectionString(TransportProtocol.tcp);
            this.DialogResult = true;
        }

        /// <summary>
        /// Parses <see cref="ConnectionString"/> and creates <see cref="Dictionary{T1,T2}"/> type collection.
        /// </summary>
        void ParseConnectionString()
        {
            if (!string.IsNullOrEmpty(this.ConnectionString))
            {
                //string[] keyvalues = this.ConnectionString.Replace("[", "").Replace("]", "").Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                //foreach (string keyvalue in keyvalues)
                //{
                //    string[] keyvaluepair = keyvalue.Split('=');
                //    if (keyvaluepair.GetLength(0) == 2)
                //    {
                //        m_keyvaluepairs.Add(keyvaluepair[0].Trim(), keyvaluepair[1].Trim());
                //    }
                //}

                m_keyvaluepairs = ConnectionString.Replace("[", "").Replace("]", "").ParseKeyValuePairs();

                if (m_connectionType == ConnectionType.CommandChannel)  //command channel is TCP
                {
                    if (!m_keyvaluepairs.ContainsKey("transportprotocol") && !m_keyvaluepairs.ContainsKey("protocol"))
                        m_keyvaluepairs.Add("transportprotocol", "tcp");
                    else if (m_keyvaluepairs.ContainsKey("transportprotocol"))
                        m_keyvaluepairs["transportprotocol"] = "tcp";
                    else if (m_keyvaluepairs.ContainsKey("protocol"))
                        m_keyvaluepairs["protocol"] = "tcp";
                }

                if (m_connectionType == ConnectionType.DataChannel) //then it is UDP server.
                {
                    TabControlOptions.SelectedIndex = 4;
                    if (m_keyvaluepairs.ContainsKey("clients"))
                    {
                        string[] clients = m_keyvaluepairs["clients"].Split(',');
                        int count = 0;
                        foreach (string client in clients)
                        {
                            string port = "4712";
                            string hostIp = string.Empty;
                            if (client.Contains(":"))
                            {
                                hostIp = client.Substring(0, client.LastIndexOf(':'));
                                port = client.Substring(client.LastIndexOf(':') + 1);
                            }
                            else
                                hostIp = client;

                            if (count == 0)
                            {
                                TextBoxHostIP1.Text = hostIp;
                                TextBoxHostPort1.Text = port;
                            }
                            else if (count == 1)
                            {
                                TextBoxHostIP2.Text = hostIp;
                                TextBoxHostPort2.Text = port;
                            }
                            else if (count == 2)
                            {
                                TextBoxHostIP3.Text = hostIp;
                                TextBoxHostPort3.Text = port;
                            }
                            count += 1;
                        }
                    }
                }
                else if ((m_keyvaluepairs.ContainsKey("transportprotocol") && m_keyvaluepairs["transportprotocol"].ToLower() == "tcp") || (m_keyvaluepairs.ContainsKey("protocol") && m_keyvaluepairs["protocol"].ToLower() == "tcp"))
                {
                    TabControlOptions.SelectedIndex = 0;
                    if (m_keyvaluepairs.ContainsKey("server"))
                    {
                        if (m_keyvaluepairs["server"].Contains(":") && PgConnection)
                        {
                            m_keyvaluepairs["port"] = m_keyvaluepairs["server"].Substring(m_keyvaluepairs["server"].LastIndexOf(":") + 1);
                            m_keyvaluepairs["server"] = m_keyvaluepairs["server"].Substring(0, m_keyvaluepairs["server"].LastIndexOf(":"));
                        }
                        TextBoxHostIP.Text = m_keyvaluepairs["server"];

                    }
                    if (m_keyvaluepairs.ContainsKey("port"))
                        TextBoxPort.Text = m_keyvaluepairs["port"];
                    if (m_keyvaluepairs.ContainsKey("maxSendQueueSize"))
                        TextBoxMaxSendQueueSize.Text = m_keyvaluepairs["maxSendQueueSize"];
                    if (m_keyvaluepairs.ContainsKey("islistener") && m_keyvaluepairs["islistener"].ToLower() == "true")
                        CheckboxEstablishServer.IsChecked = true;
                    else
                        CheckboxEstablishServer.IsChecked = false;
                }
                else if ((m_keyvaluepairs.ContainsKey("transportprotocol") && m_keyvaluepairs["transportprotocol"].ToLower() == "udp") ||
                            (m_keyvaluepairs.ContainsKey("protocol") && m_keyvaluepairs["protocol"].ToLower() == "udp"))
                {
                    TabControlOptions.SelectedIndex = 1;
                    if (m_keyvaluepairs.ContainsKey("localport"))
                        TextBoxLocalPort.Text = m_keyvaluepairs["localport"];
                    if (m_keyvaluepairs.ContainsKey("server"))
                    {
                        CheckboxEnableMulticast.IsChecked = true;
                        TextBoxHostIPUdp.Text = m_keyvaluepairs["server"];
                        if (m_keyvaluepairs.ContainsKey("remoteport"))
                            TextBoxRemotePort.Text = m_keyvaluepairs["remoteport"];
                    }
                    else
                    {
                        CheckboxEnableMulticast.IsChecked = false;
                        TextBoxHostIPUdp.Text = string.Empty;
                        TextBoxRemotePort.Text = string.Empty;
                    }
                }
                else if ((m_keyvaluepairs.ContainsKey("transportprotocol") && m_keyvaluepairs["transportprotocol"].ToLower() == "serial") || (m_keyvaluepairs.ContainsKey("protocol") && m_keyvaluepairs["protocol"].ToLower() == "serial"))
                {
                    TabControlOptions.SelectedIndex = 2;
                    if (m_keyvaluepairs.ContainsKey("port"))
                    {
                        foreach (object item in ComboboxPort.Items)
                        {
                            if (item.ToString().ToLower() == m_keyvaluepairs["port"].ToLower())
                            {
                                ComboboxPort.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (m_keyvaluepairs.ContainsKey("baudrate"))
                    {
                        foreach (object item in ComboboxBaudRate.Items)
                        {
                            if (item.ToString().ToLower() == m_keyvaluepairs["baudrate"].ToLower())
                            {
                                ComboboxBaudRate.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (m_keyvaluepairs.ContainsKey("parity"))
                    {
                        foreach (object item in ComboboxParity.Items)
                        {
                            if (item.ToString().ToLower() == m_keyvaluepairs["parity"].ToLower())
                            {
                                ComboboxParity.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (m_keyvaluepairs.ContainsKey("stopbits"))
                    {
                        foreach (object item in ComboboxStopBits.Items)
                        {
                            if (item.ToString().ToLower() == m_keyvaluepairs["stopbits"].ToLower())
                            {
                                ComboboxStopBits.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (m_keyvaluepairs.ContainsKey("databits"))
                        TextBoxDataBits.Text = m_keyvaluepairs["databits"];
                    if (m_keyvaluepairs.ContainsKey("dtrenable") && m_keyvaluepairs["dtrenable"].ToLower() == "true")
                        CheckboxDTR.IsChecked = true;
                    else
                        CheckboxDTR.IsChecked = false;
                    if (m_keyvaluepairs.ContainsKey("rtsenable") && m_keyvaluepairs["rtsenable"].ToLower() == "true")
                        CheckboxRTS.IsChecked = true;
                    else
                        CheckboxRTS.IsChecked = false;
                }
                else if ((m_keyvaluepairs.ContainsKey("transportprotocol") && m_keyvaluepairs["transportprotocol"].ToLower() == "file") || (m_keyvaluepairs.ContainsKey("protocol") && m_keyvaluepairs["protocol"].ToLower() == "file"))
                {
                    TabControlOptions.SelectedIndex = 3;
                    if (m_keyvaluepairs.ContainsKey("file"))
                        TextBoxFile.Text = m_keyvaluepairs["file"];

                    if (m_keyvaluepairs.ContainsKey("definedframerate"))
                        TextBoxFrameRate.Text = m_keyvaluepairs["definedframerate"];

                    if (m_keyvaluepairs.ContainsKey("simulatetimestamp") && m_keyvaluepairs["simulatetimestamp"].ToLower() == "false")
                        CheckboxSimulateTimeStamp.IsChecked = false;	//by default it is true

                    if (m_keyvaluepairs.ContainsKey("autorepeatfile") && m_keyvaluepairs["autorepeatfile"].ToLower() == "false")
                        CheckboxAutoRepeat.IsChecked = false;
                }
                else
                    TabControlOptions.SelectedIndex = 0;


                if (m_keyvaluepairs.ContainsKey("interface"))
                    CheckboxForceIPv4.IsChecked = true;
                else
                    CheckboxForceIPv4.IsChecked = false;
            }
        }

        /// <summary>
        /// Creates connection string by merging keyvaluepairs defined in <see cref="Dictionary{T1,T2}"/>.
        /// </summary>
        /// <param name="transportProtocol"><see cref="TransportProtocol"/> to format connection string.</param>
        void SetConnectionString(TransportProtocol transportProtocol)
        {
            if (m_connectionType == ConnectionType.DataChannel || m_connectionType == ConnectionType.CommandChannel)	// don't need transport protocol if it is a data channel or command channel.
            {
                if (m_keyvaluepairs.ContainsKey("transportprotocol"))
                    m_keyvaluepairs.Remove("transportprotocol");

                if (m_keyvaluepairs.ContainsKey("protocol"))
                    m_keyvaluepairs.Remove("protocol");
            }
            else
            {
                if (!m_keyvaluepairs.ContainsKey("transportprotocol") && !m_keyvaluepairs.ContainsKey("protocol"))
                    m_keyvaluepairs.Add("transportprotocol", transportProtocol.ToString());
                else if (m_keyvaluepairs.ContainsKey("transportprotocol"))
                    m_keyvaluepairs["transportprotocol"] = transportProtocol.ToString();
                else if (m_keyvaluepairs.ContainsKey("protocol"))
                    m_keyvaluepairs["protocol"] = transportProtocol.ToString();
            }

            if ((bool)CheckboxForceIPv4.IsChecked)
                m_keyvaluepairs["interface"] = "0.0.0.0";
            else
                m_keyvaluepairs.Remove("interface");

            m_connectionString = m_keyvaluepairs.JoinKeyValuePairs();
            //m_connectionString = string.Empty;
            //foreach (KeyValuePair<string, string> keyvalue in m_keyvaluepairs)
            //{
            //    m_connectionString += keyvalue.Key + "=" + keyvalue.Value + "; ";
            //}
        }

        /// <summary>
        /// Formats IPv6 address.
        /// </summary>
        /// <param name="ipAddress">IP address to be formatted.</param>
        /// <returns>string, formatted IP address.</returns>
        private string FormatIP(string ipAddress)
        {
            if (ipAddress.Contains(":"))
                ipAddress = "[" + ipAddress + "]";

            return ipAddress;
        }

        #endregion
    }
}
