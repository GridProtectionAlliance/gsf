//******************************************************************************************************
//  FilterEditorDialog.cs - Gbtc
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
//  12/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Windows.Forms;
using GSF.Diagnostics;
using LogFileViewer.Filters;

namespace LogFileViewer
{
    public partial class FilterEditorDialog : Form
    {
        private LogMessage m_sampleMessage;
        private LogMessageFilter m_filter;

        public LogMessageFilter FilterResult;

        public FilterEditorDialog(LogMessage message, LogMessageFilter filter)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            m_sampleMessage = message;
            m_filter = new LogMessageFilter();
            m_filter.FilterLevel = filter.FilterLevel;
            m_filter.TimeFilter = filter.TimeFilter ?? new TimestampMatching(TimestampMatchingMode.Inside, message.UtcTime.AddMinutes(-1), message.UtcTime.AddMinutes(1));
            m_filter.Classification = filter.Classification ?? new EnumMatchingFlags(EnumMatchingFlagsMode.Any, 1 << (int)message.Classification, message.Classification.ToString());
            m_filter.Level = filter.Level ?? new EnumMatchingFlags(EnumMatchingFlagsMode.Any, 1 << (int)message.Level, message.Level.ToString());
            m_filter.Flags = filter.Flags ?? new EnumMatchingFlags(EnumMatchingFlagsMode.All, (int)message.Flags, message.Flags.ToString());
            m_filter.Assembly = filter.Assembly ?? new StringMatching(StringMatchingMode.Exact, message.AssemblyName);
            m_filter.Type = filter.Type ?? new StringMatching(StringMatchingMode.Exact, message.TypeName);
            m_filter.RelatedType = filter.RelatedType ?? new StringMatching(StringMatchingMode.Exact, message.TypeName);
            m_filter.StackDetails = filter.StackDetails ?? new StackDetailsMatching();
            m_filter.StackTraceDetails = filter.StackTraceDetails ?? new StackTraceMatching();
            m_filter.EventName = filter.EventName ?? new StringMatching(StringMatchingMode.Exact, message.EventName);
            m_filter.MessageText = filter.MessageText ?? new StringMatching(StringMatchingMode.Exact, message.Message);
            m_filter.DetailsText = filter.DetailsText ?? new StringMatching(StringMatchingMode.Exact, message.Details);
            m_filter.ExceptionText = filter.ExceptionText ?? new StringMatching(StringMatchingMode.Exact, message.ExceptionString);

            InitializeComponent();

            chkTime.Checked = filter.TimeFilter != null;
            chkClass.Checked = filter.Classification != null;
            chkLevel.Checked = filter.Level != null;
            chkFlags.Checked = filter.Flags != null;
            chkAssembly.Checked = filter.Assembly != null;
            chkType.Checked = filter.Type != null;
            chkRelatedType.Checked = filter.RelatedType != null;
            chkStackDetails.Checked = filter.StackDetails != null;
            chkStackTrace.Checked = filter.StackTraceDetails != null;
            chkEventName.Checked = filter.EventName != null;
            chkMessage.Checked = filter.MessageText != null;
            chkDetails.Checked = filter.DetailsText != null;
            chkException.Checked = filter.ExceptionText != null;

            lblTime.Text = m_filter.TimeFilter.ToString();
            lblClass.Text = m_filter.Classification.ToString();
            lblLevel.Text = m_filter.Level.ToString();
            lblFlags.Text = m_filter.Flags.ToString();
            lblAssembly.Text = m_filter.Assembly.ToString();
            lblType.Text = m_filter.Type.ToString();
            lblRelatedType.Text = m_filter.RelatedType.ToString();
            lblStackDetails.Text = m_filter.StackDetails.ToString();
            lblStackTrace.Text = m_filter.StackTraceDetails.ToString();
            lblEventName.Text = m_filter.EventName.ToString();
            lblMessage.Text = m_filter.MessageText.ToString();
            lblDetails.Text = m_filter.DetailsText.ToString();
            lblException.Text = m_filter.ExceptionText.ToString();

            cmbVerbose.SelectedIndex = (int)m_filter.FilterLevel;
            chkMessage.Checked = filter.MessageText != null;
            chkMessage.Tag = filter.MessageText ?? new StringMatching(StringMatchingMode.Exact, message.Message);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            FilterResult = new LogMessageFilter();
            FilterResult.FilterLevel = (FilterLevel)cmbVerbose.SelectedIndex;

            if (chkTime.Checked) FilterResult.TimeFilter = m_filter.TimeFilter;
            if (chkClass.Checked) FilterResult.Classification = m_filter.Classification;
            if (chkLevel.Checked) FilterResult.Level = m_filter.Level;
            if (chkFlags.Checked) FilterResult.Flags = m_filter.Flags;
            if (chkAssembly.Checked) FilterResult.Assembly = m_filter.Assembly;
            if (chkType.Checked) FilterResult.Type = m_filter.Type;
            if (chkRelatedType.Checked) FilterResult.RelatedType = m_filter.RelatedType;
            if (chkStackDetails.Checked) FilterResult.StackDetails = m_filter.StackDetails;
            if (chkStackTrace.Checked) FilterResult.StackTraceDetails = m_filter.StackTraceDetails;
            if (chkEventName.Checked) FilterResult.EventName = m_filter.EventName;
            if (chkMessage.Checked) FilterResult.MessageText = m_filter.MessageText;
            if (chkDetails.Checked) FilterResult.DetailsText = m_filter.DetailsText;
            if (chkException.Checked) FilterResult.ExceptionText = m_filter.ExceptionText;

            DialogResult = DialogResult.OK;
        }

        private void btnAssembly_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(m_filter.Assembly))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.Assembly = win.ResultFilter;
                    chkAssembly.Checked = true;
                    lblAssembly.Text = win.ResultFilter.ToString();
                }
            }
        }

        private void btnType_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(m_filter.Type))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.Type = win.ResultFilter;
                    chkType.Checked = true;
                    lblType.Text = win.ResultFilter.ToString();
                }
            }
        }

        private void btnEventName_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(m_filter.EventName))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.EventName = win.ResultFilter;
                    chkEventName.Checked = true;
                    lblEventName.Text = win.ResultFilter.ToString();
                }
            }
        }

        private void benMessage_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(m_filter.MessageText))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.MessageText = win.ResultFilter;
                    chkMessage.Checked = true;
                    lblMessage.Text = win.ResultFilter.ToString();
                }
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(m_filter.DetailsText))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.DetailsText = win.ResultFilter;
                    chkDetails.Checked = true;
                    lblDetails.Text = win.ResultFilter.ToString();
                }
            }
        }

        private void btnException_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(m_filter.ExceptionText))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.ExceptionText = win.ResultFilter;
                    chkException.Checked = true;
                    lblException.Text = win.ResultFilter.ToString();
                }
            }
        }

        private void btnRelatedType_Click(object sender, EventArgs e)
        {
            using (var win = new StringMatchingFilterDialog(new StringMatching(StringMatchingMode.Exact,
                string.Join(Environment.NewLine,
                m_sampleMessage.RelatedTypes.Union(new string[] { m_sampleMessage.TypeName }).OrderBy(x => x)))))
            {
                if (win.ShowDialog() == DialogResult.OK)
                {
                    m_filter.RelatedType = win.ResultFilter;
                    chkRelatedType.Checked = true;
                    lblRelatedType.Text = win.ResultFilter.ToString();
                }
            }
        }
    }
}
