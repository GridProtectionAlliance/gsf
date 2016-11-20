//******************************************************************************************************
//  LogFileViewer.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GSF.Diagnostics;
using GSF.IO;
using LogFileViewer.Filters;
using LogFileViewer.Properties;
using Microsoft.VisualBasic;

namespace LogFileViewer
{
    internal partial class LogFileViewer : Form
    {
        private List<LogMessage> m_messages;
        private List<IMessageMatch> m_filters;
        private string m_logPath;
        SortedList<string, byte[]> m_savedFilters = new SortedList<string, byte[]>();

        public LogFileViewer(string logPath = null)
        {
            if (!Settings.Default.HasBeenUpgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.HasBeenUpgraded = true;
                Settings.Default.Save();
            }

            if (logPath != null)
            {
                try
                {
                    if (!Directory.Exists(logPath))
                    {
                        logPath = null;
                    }
                }
                catch (Exception)
                {
                    logPath = null;
                }
            }

            m_logPath = logPath;
            m_filters = new List<IMessageMatch>();
            m_messages = new List<LogMessage>();

            InitializeComponent();
        }

        private void LogFileViewer_Load(object sender, EventArgs e)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dgvResults, new object[] { true });
            LoadFilters();
        }

        private void RefreshFilters()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Object", typeof(LogMessage));
            dt.Columns.Add("Time", typeof(DateTime));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("EventName", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("Details", typeof(string));
            dt.Columns.Add("Exception", typeof(string));

            foreach (LogMessage message in m_messages)
            {
                if (m_filters.All(x => x.IsIncluded(message)))
                {
                    dt.Rows.Add(message, message.UtcTime.ToLocalTime(), $"{message.Classification} - {message.Level}",
                        message.EventPublisherDetails.TypeName, message.EventPublisherDetails.EventName, message.Message,
                        message.Details, message.ExceptionString);
                }
            }

            dgvResults.DataSource = dt;
            dgvResults.Columns["Object"].Visible = false;

            foreach (DataGridViewColumn dc in dgvResults.Columns)
            {
                if (dc.ValueType == typeof(DateTime))
                {
                    dc.DefaultCellStyle.Format = "MM/dd/yyyy HH:mm:ss.fff";
                }
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (m_logPath != null)
                {
                    dlg.InitialDirectory = m_logPath;
                    dlg.RestoreDirectory = true;
                }

                dlg.Filter = "Log Files (*.logz)|*.logz|All Files (*.*)|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    m_messages.Clear();
                    foreach (string file in dlg.FileNames)
                    {
                        List<LogMessage> messages = LogFileReader.Read(file);
                        m_messages.AddRange(messages);
                    }
                }
                RefreshFilters();
            }
        }

        private void dgvResults_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                LogMessage item = (LogMessage)dgvResults.Rows[e.RowIndex].Cells["Object"].Value;

                switch (dgvResults.Columns[e.ColumnIndex].Name)
                {
                    case "Type":
                        MakeMenu(e, new MatchType(item));
                        break;
                    case "EventName":
                        MakeMenu(e, new MatchEventName(item));
                        break;
                    case "Time":
                        MakeMenu(e, new MatchTimestamp(item));
                        break;
                    case "Level":
                        MakeMenu(e, new MatchVerbose(item));
                        break;
                    case "Exception":
                        MakeMenu(e, new MatchErrorName(item));
                        break;
                    case "Message":
                        MakeMenu(e, new MatchMessageName(item));
                        break;
                    default:
                        return;
                }
            }
        }

        private void MakeMenu(DataGridViewCellMouseEventArgs e, IMessageMatch filter)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.ShowImageMargin = false;

            foreach (Tuple<string, Func<bool>> item in filter.GetMenuButtons())
            {
                ToolStripButton button = new ToolStripButton(item.Item1);
                button.Click += (send1, e1) =>
                    {
                        if (item.Item2())
                        {
                            LstFilters.Items.Add(filter);
                            m_filters.Add(filter);
                            RefreshFilters();
                        }
                    };
                menu.Items.Add(button);
            }

            menu.Width = 150;
            menu.Show(dgvResults, dgvResults.PointToClient(Cursor.Position));
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (IMessageMatch item in LstFilters.SelectedItems.Cast<IMessageMatch>().ToArray())
            {
                m_filters.Remove(item);
                LstFilters.Items.Remove(item);
            }

            RefreshFilters();
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            foreach (IMessageMatch item in LstFilters.SelectedItems.Cast<IMessageMatch>().ToArray())
            {
                item.ToggleResult();
            }

            LstFilters.DisplayMember = string.Empty;
            LstFilters.DisplayMember = "Description";

            RefreshFilters();
        }

        private void dgvResults_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0)
            {
                LogMessage item = (LogMessage)dgvResults.Rows[e.RowIndex].Cells["Object"].Value;
                ShowError win = new ShowError();

                win.TxtMessage.Text = item.ToString();
                win.Show();
                win.TxtMessage.Select(0, 0);
            }
        }

        private void BtnCompactFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlgLoad = new OpenFileDialog())
            {
                if (m_logPath != null)
                {
                    dlgLoad.InitialDirectory = m_logPath;
                    dlgLoad.RestoreDirectory = true;
                }

                dlgLoad.Filter = "Log File (Compressed)|*.Logz";
                dlgLoad.Multiselect = true;

                if (dlgLoad.ShowDialog() == DialogResult.OK)
                {
                    using (SaveFileDialog dlgSave = new SaveFileDialog())
                    {
                        if (m_logPath != null)
                        {
                            dlgLoad.InitialDirectory = m_logPath;
                            dlgLoad.RestoreDirectory = true;
                        }

                        dlgSave.Filter = "Log File (Compressed)|*.Logz";

                        if (dlgSave.ShowDialog() == DialogResult.OK)
                        {
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            LogFileCompactor.Compact(dlgLoad.FileNames, dlgSave.FileName);
                            sw.Stop();
                            MessageBox.Show(sw.Elapsed.TotalSeconds.ToString());
                        }
                    }
                }
            }
        }

        private void cmsFilters_Opening(object sender, CancelEventArgs e)
        {
            FillMenu();
        }

        public void LoadFilters()
        {
            try
            {
                string data = Settings.Default.SavedFilters;
                if (string.IsNullOrEmpty(data))
                    return;
                byte[] dataBytes = Convert.FromBase64String(data);
                MemoryStream ms = new MemoryStream(dataBytes);

                byte version = ms.ReadNextByte();
                switch (version)
                {
                    case 1:
                        SortedList<string, byte[]> filterSets = new SortedList<string, byte[]>();
                        int count = ms.ReadInt32();
                        while (count > 0)
                        {
                            count--;
                            string name = ms.ReadString();
                            byte[] filter = ms.ReadBytes();
                            filterSets[name] = filter;
                        }

                        m_savedFilters = filterSets;
                        break;
                    default:
                        throw new VersionNotFoundException();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveFilters()
        {
            var ms = new MemoryStream();
            ms.Write((byte)1);
            ms.Write(m_savedFilters.Count);
            foreach (var filter in m_savedFilters)
            {
                ms.Write(filter.Key);
                ms.WriteWithLength(filter.Value);
            }

            Settings.Default.SavedFilters = Convert.ToBase64String(ms.ToArray());
            Settings.Default.Save();
        }

        public void FillMenu()
        {
            overwriteToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in m_savedFilters)
            {
                var button = new ToolStripButton(item.Key);
                button.Width = 300;
                button.Click += (send1, e1) =>
                {
                    SaveCurrentTemplete(item.Key);
                };
                overwriteToolStripMenuItem.DropDownItems.Add(button);
            }

            loadToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in m_savedFilters)
            {
                var button = new ToolStripButton(item.Key);
                button.Width = 300;
                button.Click += (send1, e1) =>
                {
                    LoadCurrentTemplete(item.Key);
                };
                loadToolStripMenuItem.DropDownItems.Add(button);
            }

            deleteToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in m_savedFilters)
            {
                var button = new ToolStripButton(item.Key);
                button.Width = 300;
                button.Click += (send1, e1) =>
                {
                    m_savedFilters.Remove(item.Key);
                    SaveFilters();

                };
                deleteToolStripMenuItem.DropDownItems.Add(button);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string item = InputBox.Show("Provide a name.", "Save As", "Name");
            if (string.IsNullOrWhiteSpace(item))
                return;

            SaveCurrentTemplete(item);
        }

        private void SaveCurrentTemplete(string item)
        {
            var ms = new MemoryStream();
            ms.Write((byte)1);
            ms.Write(m_filters.Count);
            foreach (var filter in m_filters)
            {
                ms.Write((byte)filter.TypeCode);
                filter.Save(ms);
            }

            m_savedFilters[item] = ms.ToArray();

            SaveFilters();
        }

        private void LoadCurrentTemplete(string item)
        {
            List<IMessageMatch> filters = new List<IMessageMatch>();
            var ms = new MemoryStream(m_savedFilters[item]);
            byte version = ms.ReadNextByte();
            switch (version)
            {
                case 1:
                    int count = ms.ReadInt32();
                    while (count > 0)
                    {
                        count--;
                        switch ((FilterType)ms.ReadNextByte())
                        {
                            case FilterType.Timestamp:
                                filters.Add(new MatchTimestamp(ms));
                                break;
                            case FilterType.Verbose:
                                filters.Add(new MatchVerbose(ms));
                                break;
                            case FilterType.Type:
                                filters.Add(new MatchType(ms));
                                break;
                            case FilterType.Event:
                                filters.Add(new MatchEventName(ms));
                                break;
                            case FilterType.Description:
                                filters.Add(new MatchMessageName(ms));
                                break;
                            case FilterType.Error:
                                filters.Add(new MatchErrorName(ms));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    m_filters = filters;
                    LstFilters.Items.Clear();
                    LstFilters.Items.AddRange(filters.Cast<object>().ToArray());
                    RefreshFilters();
                    break;
                default:
                    throw new NotSupportedException();
            }

        }

        private void btnFilteredLoad_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                if (m_logPath != null)
                {
                    dlg.InitialDirectory = m_logPath;
                    dlg.RestoreDirectory = true;
                }
                dlg.Filter = "Log File (Compressed)|*.Logz";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    m_messages.Clear();
                    foreach (var file in dlg.FileNames)
                    {
                        var messages = LogFileReader.Read(file);
                        m_messages.AddRange(messages.Where(y => m_filters.All(x => x.IsIncluded(y))));
                    }
                }
                RefreshFilters();
            }
        }

        private void btnSaveSelected_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlgSave = new SaveFileDialog())
            {
                dlgSave.Filter = "Log File (Compressed)|*.Logz";

                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    using (var fileWriter = new LogFileWriter(dlgSave.FileName))
                    {
                        foreach (LogMessage message in m_messages)
                        {
                            if (m_filters.All(x => x.IsIncluded(message)))
                            {
                                fileWriter.Write(message, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
