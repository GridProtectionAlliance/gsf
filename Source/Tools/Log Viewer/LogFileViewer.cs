using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace GSF.Diagnostics.UI
{
    internal partial class LogFileViewer : Form
    {
        private List<LogMessage> m_messages;
        private List<IMessageMatch> m_filters;
        private string m_logPath;

        public LogFileViewer(string logPath = null)
        {
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

        private void FrmMain_Load(object sender, EventArgs e)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dgvResults, new object[] { true });
        }

        private void RefreshFilters()
        {
            var dt = new DataTable();
            dt.Columns.Add("Object", typeof(LogMessage));
            dt.Columns.Add("Time", typeof(DateTime));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("EventName", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("Details", typeof(string));
            dt.Columns.Add("Exception", typeof(string));

            foreach (var message in m_messages)
            {
                if (m_filters.All(x => x.IsIncluded(message)))
                {
                    dt.Rows.Add(message, message.UtcTime.ToLocalTime(), message.Classification.ToString() + " - " + message.Level.ToString(),
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
                        m_messages.AddRange(messages);
                    }
                }
                RefreshFilters();
            }
        }

        private void dgvResults_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >=0)
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
            foreach (var item in filter.GetMenuButtons())
            {
                var button = new ToolStripButton(item.Item1);
                button.Click += (send1, e1) =>
                    {
                        item.Item2();
                        LstFilters.Items.Add(filter);
                        m_filters.Add(filter);
                        RefreshFilters();
                    };
                menu.Items.Add(button);
            }

            menu.Width = 150;
            menu.Show(dgvResults, dgvResults.PointToClient(Cursor.Position));
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (var item in LstFilters.SelectedItems.Cast<IMessageMatch>().ToArray())
            {
                m_filters.Remove(item);
                LstFilters.Items.Remove(item);
            }
            RefreshFilters();
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            foreach (var item in LstFilters.SelectedItems.Cast<IMessageMatch>().ToArray())
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
                FrmShowError win = new FrmShowError();
                win.TxtMessage.Text = item.ToString();
                win.Show();
                win.TxtMessage.Select(0, 0);
            }
        }

        private void BtnCompactFiles_Click(object sender, EventArgs e)
        {
            using (var dlgLoad = new OpenFileDialog())
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
                    using (var dlgSave = new SaveFileDialog())
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
    }
}
