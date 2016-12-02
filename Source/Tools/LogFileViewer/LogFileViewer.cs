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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GSF.Diagnostics;
using GSF.IO;
using LogFileViewer.Filters;
using LogFileViewer.Menu;
using LogFileViewer.Properties;

namespace LogFileViewer
{
    internal partial class LogFileViewer : Form
    {
        private BindingSource m_bindingSource;
        private DataTable m_dataTable;
        private bool[] m_visibleFlags;
        private List<LogMessageFilter> m_filters;

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

            m_visibleFlags = new bool[9];

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
            m_filters = new List<LogMessageFilter>();

            InitializeComponent();

            m_bindingSource = new BindingSource();
            m_dataTable = new DataTable();
            m_dataTable.Columns.Add("Object", typeof(LogMessage));
            m_dataTable.Columns.Add("File Name", typeof(string));
            m_dataTable.Columns.Add("Time", typeof(DateTime));
            m_dataTable.Columns.Add("Level", typeof(string));
            m_dataTable.Columns.Add("Flags", typeof(string));
            m_dataTable.Columns.Add("Type", typeof(string));
            m_dataTable.Columns.Add("Stack Details", typeof(string));
            m_dataTable.Columns.Add("Event Name", typeof(string));
            m_dataTable.Columns.Add("Message", typeof(string));
            m_dataTable.Columns.Add("Details", typeof(string));
            m_dataTable.Columns.Add("Exception", typeof(string));
            m_dataTable.Columns.Add("_Show", typeof(bool));
            m_dataTable.Columns.Add("_Level", typeof(int));

            m_bindingSource.Filter = "_Show";
            m_bindingSource.DataSource = m_dataTable;
            dgvResults.DataSource = m_bindingSource;

            dgvResults.Columns["Time"].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm:ss.fff";
            dgvResults.Columns["Object"].Visible = false;
            dgvResults.Columns["_Show"].Visible = false;
            dgvResults.Columns["_Level"].Visible = false;
            m_visibleFlags = GetVisibleFlags();
        }

        private void LogFileViewer_Load(object sender, EventArgs e)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dgvResults, new object[] { true });
            LoadFilters();
        }

        private void RefreshDataSource()
        { }

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
                    LoadFiles(dlg.FileNames, false);
                }
            }
        }

        private void LoadFiles(string[] files, bool loadFiltered)
        {
            m_bindingSource.RaiseListChangedEvents = false;
            m_dataTable.Rows.Clear();
            foreach (string file in files)
            {
                string fileWithoutExtension = Path.GetFileNameWithoutExtension(file);
                List<LogMessage> messages = LogFileReader.Read(file);
                FilterFirstChanceExceptions(messages);
                foreach (LogMessage message in messages)
                {
                    if (!loadFiltered || IsShown(message))
                        AddRowToDataTable(message, fileWithoutExtension);
                }
            }
            m_bindingSource.RaiseListChangedEvents = true;
            m_bindingSource.ResetBindings(false);
            RefreshFilters();
        }

        private void AddRowToDataTable(LogMessage message, string fileWithoutExtension)
        {
            object[] items = new object[13];
            items[0] = message;
            items[1] = fileWithoutExtension;
            items[2] = message.UtcTime.ToLocalTime();
            items[3] = $"{message.Classification} - {message.Level}";
            items[4] = message.Flags.ToString();
            items[5] = message.TypeName;
            items[6] = message.InitialStackMessages.Union(message.CurrentStackMessages).ToString();
            items[7] = message.EventName;
            items[8] = message.Message;
            items[9] = message.Details;
            items[10] = message.ExceptionString;
            items[11] = true;
            items[12] = 4;
            m_dataTable.Rows.Add(items);
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
                dlg.Filter = "Log Files (Compressed)|*.logz";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadFiles(dlg.FileNames, true);
                }
            }
        }

        private void dgvResults_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                LogMessage item = (LogMessage)dgvResults.Rows[e.RowIndex].Cells["Object"].Value;

                List<LogMessage> messages = new List<LogMessage>();
                messages.Add(item);

                List<Tuple<string, Func<LogMessageFilter>>> items = new List<Tuple<string, Func<LogMessageFilter>>>();

                switch (dgvResults.Columns[e.ColumnIndex].Name)
                {
                    case "Type":
                        items.AddRange(new TypeMenu(messages).GetMenuButtons());
                        break;
                    case "Event Name":
                        items.AddRange(new EventMenu(messages).GetMenuButtons());
                        break;
                    case "Time":
                        items.AddRange(new TimestampMenu(messages).GetMenuButtons());
                        break;
                    case "Level":
                        //MakeMenu(e, new MatchVerbose(item));
                        break;
                    case "Exception":
                        items.AddRange(new ExceptionMenu(messages).GetMenuButtons());
                        break;
                    case "Message":
                        items.AddRange(new MessageMenu(messages).GetMenuButtons());
                        break;
                    case "Stack Details":
                        //MakeMenu(e, new MatchStackMessages(item));
                        break;
                    default:
                        return;
                }

                if (items.Count > 0)
                    MakeMenu(e, items);

            }
        }

        private void MakeMenu(DataGridViewCellMouseEventArgs e, List<Tuple<string, Func<LogMessageFilter>>> items)
        {
            ContextMenuStrip menu = new ContextMenuStrip();


            foreach (Tuple<string, Func<LogMessageFilter>> item in items)
            {
                var subMenu = new ToolStripMenuItem(item.Item1);

                for (int x = 0; x <= 6; x++)
                {
                    ToolStripButton button = new ToolStripButton(((FilterLevel)x).Name());
                    button.Tag = (FilterLevel)x;
                    button.Click += (send1, e1) =>
                                    {
                                        LogMessageFilter filter = item.Item2();
                                        if (filter != null)
                                        {
                                            filter.FilterLevel = (FilterLevel)((ToolStripButton)send1).Tag;
                                            LstFilters.Items.Add(filter);
                                            m_filters.Add(filter);
                                            RefreshFilters();
                                        }
                                    };
                    subMenu.DropDownItems.Add(button);
                }
                menu.Items.Add(subMenu);

                ((ToolStripDropDownMenu)subMenu.DropDown).ShowImageMargin = false;
                subMenu.DropDown.Width = 150;


            }

            menu.ShowImageMargin = false;
            menu.Width = 150;
            menu.Show(dgvResults, dgvResults.PointToClient(Cursor.Position));
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

                dlgLoad.Filter = "Log File (Compressed)|*.logz";
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

                        dlgSave.Filter = "Log File (Compressed)|*.logz";

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
                                { SaveCurrentTemplete(item.Key); };
                overwriteToolStripMenuItem.DropDownItems.Add(button);
            }

            loadToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in m_savedFilters)
            {
                var button = new ToolStripButton(item.Key);
                button.Width = 300;
                button.Click += (send1, e1) =>
                                { LoadCurrentTemplete(item.Key); };
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
            ms.Write((byte)2);
            ms.Write(m_filters.Count);
            foreach (var filter in m_filters)
            {
                filter.Save(ms);
            }

            m_savedFilters[item] = ms.ToArray();

            SaveFilters();
        }

        private void LoadCurrentTemplete(string item)
        {
            List<LogMessageFilter> filters = new List<LogMessageFilter>();
            var ms = new MemoryStream(m_savedFilters[item]);
            byte version = ms.ReadNextByte();
            switch (version)
            {
                case 1:
                    return;
                case 2:
                    int count = ms.ReadInt32();
                    while (count > 0)
                    {
                        count--;
                        filters.Add(new LogMessageFilter(ms));
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

        private void btnSaveSelected_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlgSave = new SaveFileDialog())
            {
                dlgSave.Filter = "Log File (Compressed)|*.logz";

                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    using (var fileWriter = new LogFileWriter(dlgSave.FileName))
                    {
                        foreach (DataRow row in m_dataTable.Rows)
                        {
                            LogMessage message = row["Object"] as LogMessage;
                            bool shown = (bool)row["_Show"];

                            if (shown)
                            {
                                fileWriter.Write(message, false);
                            }
                        }
                    }
                }
            }
        }

        private void FilterFirstChanceExceptions(List<LogMessage> messages)
        {
            for (int x = 0; x < messages.Count; x++)
            {
                //Only first chance exceptions can be filtered.
                LogMessage firstMessage = messages[x];
                if (firstMessage.ExceptionString.Length > 0 && firstMessage.TypeName == "GSF.Diagnostics.Logger" && firstMessage.EventName == "First Chance App Domain Exception")
                {
                    //don't look ahead for more than 100ms.
                    DateTime stopAfter = firstMessage.UtcTime.AddTicks(100 * TimeSpan.TicksPerMillisecond);

                    bool foundMatch = false;
                    for (int y = x + 1; y < messages.Count; y++)
                    {
                        LogMessage futureMessage = messages[y];
                        if (futureMessage.UtcTime > stopAfter)
                            break;

                        if (futureMessage.ManagedThreadID == firstMessage.ManagedThreadID && futureMessage.ExceptionString.Length > 0 && futureMessage.TypeName != "GSF.Diagnostics.Logger")
                        {
                            if (futureMessage.ExceptionString.StartsWith(firstMessage.ExceptionString))
                            {
                                foundMatch = true;
                                break;
                            }
                            else if (futureMessage.ExceptionString.StartsWith("System.InvalidOperationException:") && futureMessage.ExceptionString.Contains(" ---> "))
                            {
                                string exception = firstMessage.ExceptionString;
                                if (exception.StartsWith("System.Net.Sockets.SocketException (0x") && exception.Contains("):"))
                                {
                                    exception = "System.Net.Sockets.SocketException" + exception.Substring(exception.IndexOf(")") + 1) + Environment.NewLine + "   --- End of inner exception stack trace ---";
                                }

                                if (futureMessage.ExceptionString.EndsWith(exception))
                                {
                                    foundMatch = true;
                                    break;
                                }

                            }

                        }
                    }

                    if (foundMatch)
                    {
                        messages.RemoveAt(x);
                        x--;
                    }
                }
            }

        }

        private void LogFileViewer_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void LogFileViewer_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFiles(files, (e.KeyState & 8) == 8); //CTRL is pushed.
        }

        private void LstFilters_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (LogMessageFilter item in LstFilters.SelectedItems.Cast<LogMessageFilter>().ToArray())
                {
                    m_filters.Remove(item);
                    LstFilters.Items.Remove(item);
                }

                RefreshFilters();
            }
        }

        private void VisibleCheckedChanged(object sender, EventArgs e)
        {
            m_visibleFlags = GetVisibleFlags();
            RefreshVisible();
        }

        private void RefreshVisible()
        {
            var visible = GetVisibleFlags();

            m_bindingSource.RaiseListChangedEvents = false;
            foreach (DataRow row in m_dataTable.Rows)
            {
                int level = (int)row["_Level"];
                bool show = (bool)row["_Show"];

                if (show ^ visible[level])
                {
                    row["_show"] = !show;
                }
            }
            m_bindingSource.RaiseListChangedEvents = true;
            m_bindingSource.ResetBindings(false);
        }

        private bool[] GetVisibleFlags()
        {
            bool[] visible = new bool[7];
            visible[0] = RdoLowest.Checked;
            visible[1] = visible[0] || RdoLow.Checked;
            visible[2] = visible[1] || RdoBelowNormal.Checked;
            visible[3] = visible[2] || RdoNormal.Checked;
            visible[4] = visible[3] || RdoAboveNormal.Checked;
            visible[5] = visible[4] || RdoHigh.Checked;
            visible[6] = visible[5] || RdoHighest.Checked;
            return visible;
        }

        private void RefreshFilters()
        {
            var visible = GetVisibleFlags();
            m_bindingSource.RaiseListChangedEvents = false;
            foreach (DataRow row in m_dataTable.Rows)
            {
                LogMessage message = row["Object"] as LogMessage;
                int level = (int)row["_Level"];
                bool show = (bool)row["_Show"];
                int assignedLevel = GetLevel(message);

                if (level != assignedLevel)
                {
                    row["_Level"] = assignedLevel;
                }
                if (show ^ visible[assignedLevel])
                {
                    row["_show"] = !show;
                }
            }
            m_bindingSource.RaiseListChangedEvents = true;
            m_bindingSource.ResetBindings(false);
        }

        private int GetLevel(LogMessage message)
        {
            //Match is in reverse order.
            for (int x = m_filters.Count - 1; x >= 0; x--)
            {
                var filter = m_filters[x];
                if (filter.IsMatch(message))
                {
                    return (int)filter.FilterLevel;
                }
            }
            return (int)(FilterLevel.Normal);
        }

        private bool IsShown(LogMessage message)
        {
            return m_visibleFlags[GetLevel(message)];
        }

        private void dgvResults_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex != -1 && (e.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.Selected)
            {
                int value = (int)dgvResults["_Level", e.RowIndex].Value;
                Color color;
                switch (value)
                {
                    case 0:
                        color = Color.Silver;
                        break;
                    case 1:
                        color = Color.FromArgb(255, 128, 128);
                        break;
                    case 2:
                        color = Color.FromArgb(255, 128, 255);
                        break;
                    case 4:
                        color = Color.FromArgb(255, 255, 128);
                        break;
                    case 5:
                        color = Color.FromArgb(128, 255, 128);
                        break;
                    case 6:
                        color = Color.FromArgb(128, 255, 255);
                        break;
                    default:
                        return;
                }


                //fill gradient background
                using (SolidBrush gradientBrush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(gradientBrush, e.CellBounds);
                }

                // paint rest of cell
                e.Paint(e.CellBounds, DataGridViewPaintParts.Border | DataGridViewPaintParts.ContentForeground);
                e.Handled = true;
            }
        }

        private void BtnAddFilter_Click(object sender, EventArgs e)
        {
            DataGridViewCell cell = dgvResults.SelectedCells.Cast<DataGridViewCell>().FirstOrDefault();
            if (cell == null)
            {
                MessageBox.Show("Select a message to base the filter on");
                return;
            }
            LogMessage message = dgvResults["Object", cell.RowIndex].Value as LogMessage;

            using (var win = new FilterEditorDialog(message, new LogMessageFilter()))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    LstFilters.Items.Add(win.FilterResult);
                    m_filters.Add(win.FilterResult);
                    RefreshFilters();
                }
            }
        }

        private void LstFilters_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (LstFilters.SelectedItem != null)
            {
                DataGridViewCell cell = dgvResults.SelectedCells.Cast<DataGridViewCell>().FirstOrDefault();
                if (cell == null)
                {
                    MessageBox.Show("Select a message to base the edits on");
                    return;
                }
                LogMessage message = dgvResults["Object", cell.RowIndex].Value as LogMessage;

                using (var win = new FilterEditorDialog(message, (LogMessageFilter)LstFilters.SelectedItem))
                {
                    if (win.ShowDialog() == DialogResult.OK)
                    {
                        m_filters[LstFilters.SelectedIndex] = win.FilterResult;
                        LstFilters.Items[LstFilters.SelectedIndex] = win.FilterResult;
                        RefreshFilters();
                    }
                }

            }
        }
    }
}