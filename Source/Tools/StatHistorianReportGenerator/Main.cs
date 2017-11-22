//******************************************************************************************************
//  MainWindow.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/10/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Windows.Forms;

namespace StatHistorianReportGenerator
{
    public partial class Main : Form
    {
        #region [ Constructors ]

        public Main()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        private void Main_Load(object sender, EventArgs e)
        {
            ReportDateDateTimePicker.Value = DateTime.UtcNow.Date;
        }

        private void GenerateReportButton_Click(object sender, EventArgs e)
        {
            CompletenessReportGenerator completenessReportGenerator;
            double level4Threshold;
            double level3Threshold;

            using (FileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.DefaultExt = "pdf";
                fileDialog.Filter = "PDF files|*.pdf|All files|*.*";

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    completenessReportGenerator = new CompletenessReportGenerator()
                    {
                        TitleText = TitleTextTextBox.Text,
                        CompanyText = CompanyTextTextBox.Text,
                        ReportDate = new DateTime(ReportDateDateTimePicker.Value.Ticks, DateTimeKind.Utc),
                        Level4Alias = Level4AliasTextBox.Text,
                        Level3Alias = Level3AliasTextBox.Text,
                        GenerateCsvReport = true,
                        ReportFilePath = fileDialog.FileName
                    };

                    if (double.TryParse(Level4ThresholdTextBox.Text, out level4Threshold))
                        completenessReportGenerator.Level4Threshold = level4Threshold;

                    if (double.TryParse(Level3ThresholdTextBox.Text, out level3Threshold))
                        completenessReportGenerator.Level3Threshold = level3Threshold;

                    completenessReportGenerator.GenerateReport().Save(fileDialog.FileName);
                }
            }
        }

        #endregion
    }
}
