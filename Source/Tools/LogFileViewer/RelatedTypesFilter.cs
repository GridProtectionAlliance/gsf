//******************************************************************************************************
//  RelatedTypesFilter.cs - Gbtc
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

namespace LogFileViewer
{
    public partial class RelatedTypesFilter : Form
    {
        public string SelectedItem;

        public RelatedTypesFilter(PublisherTypeDefinition types)
        {
            InitializeComponent();
            listBox1.Items.Add(types.TypeName);
            listBox1.Items.AddRange(types.RelatedTypes.Cast<object>().ToArray());
            listBox1.Sorted = true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SelectedItem = (string)listBox1.SelectedItem;
            if (SelectedItem == null)
            {
                MessageBox.Show("Select an item");
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

        }
    }
}
