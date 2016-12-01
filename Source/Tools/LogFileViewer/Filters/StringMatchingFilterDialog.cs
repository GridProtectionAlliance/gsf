//******************************************************************************************************
//  StringMatchingFilterDialog.cs - Gbtc
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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LogFileViewer.Filters;

namespace LogFileViewer
{
    public partial class StringMatchingFilterDialog : Form
    {
        private StringMatching m_existingFilter;

        public StringMatching ResultFilter;

        public StringMatchingFilterDialog(StringMatching existingFilter)
        {
            m_existingFilter = existingFilter;
            InitializeComponent();

            TxtUserInput.Text = m_existingFilter.MatchText;
            switch (m_existingFilter.MatchMode)
            {
                case StringMatchingMode.Exact:
                    rdoExact.Checked = true;
                    break;
                case StringMatchingMode.StartsWith:
                    RdoStartsWith.Checked = true;
                    break;
                case StringMatchingMode.Contains:
                    rdoContains.Checked = true;
                    break;
                case StringMatchingMode.EndsWith:
                    rdoEndsWith.Checked = true;
                    break;
                case StringMatchingMode.Regex:
                    rdoRegex.Checked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BtnDone_Click(object sender, EventArgs e)
        {
            if (rdoRegex.Checked)
            {
                try
                {
                    Regex r = new Regex(TxtUserInput.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Not a valid Regex");
                    return;
                }
            }

            if (rdoExact.Checked)
                ResultFilter = new StringMatching(StringMatchingMode.Exact, TxtUserInput.Text);
            else if (RdoStartsWith.Checked)
                ResultFilter = new StringMatching(StringMatchingMode.StartsWith, TxtUserInput.Text);
            else if (rdoContains.Checked)
                ResultFilter = new StringMatching(StringMatchingMode.Contains, TxtUserInput.Text);
            else if (rdoEndsWith.Checked)
                ResultFilter = new StringMatching(StringMatchingMode.EndsWith, TxtUserInput.Text);
            else if (rdoRegex.Checked)
                ResultFilter = new StringMatching(StringMatchingMode.Regex, TxtUserInput.Text);
            else
            {
                MessageBox.Show("Not a valid mode");
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
