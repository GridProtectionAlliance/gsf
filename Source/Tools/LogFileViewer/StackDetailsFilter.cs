//******************************************************************************************************
//  StackDetailsFilter.cs - Gbtc
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GSF.Diagnostics;
using LogFileViewer.Filters;

namespace LogFileViewer
{
    public partial class StackDetailsFilter : Form
    {
        private class KVP
        {
            public KeyValuePair<string, string> Values;
            public KVP(KeyValuePair<string, string> values)
            {
                Values = values;
            }
            public override string ToString()
            {
                return Values.Key + "=" + Values.Value;
            }
        }

        public StackDetailsMatching Matching;

        public StackDetailsFilter(LogMessage log)
        {
            InitializeComponent();

            for (int x = 0; x < log.CurrentStackMessages.Count; x++)
            {
                checkedListBox1.Items.Add(new KVP(log.CurrentStackMessages[x]));
            }
            for (int x = 0; x < log.InitialStackMessages.Count; x++)
            {
                checkedListBox1.Items.Add(new KVP(log.InitialStackMessages[x]));
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Select an item");
                return;
            }

            Matching = new StackDetailsMatching(checkedListBox1.CheckedItems.Cast<KVP>().Select(x => x.Values).ToList());
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
