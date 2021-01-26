//******************************************************************************************************
//  EditDetails.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  01/25/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GSF.PhasorProtocols;

namespace SELPDCImporter
{
    public partial class EditDetails : Form
    {
        public EditDetails()
        {
            InitializeComponent();
        }

        public ImportParameters ImportParams { get; set; }

        public ConfigurationFrame TargetConfigFrame { get; private set; }

        private void EditDetails_Load(object sender, System.EventArgs e)
        {
            ConfigurationFrame selPDCConfigFrame = ImportParams.SELPDCConfigFrame;
            ConfigurationFrame gsfPDCConfigFrame = ImportParams.GSFPDCConfigFrame;
            
            TargetConfigFrame = new ConfigurationFrame(
                gsfPDCConfigFrame?.IDCode ?? selPDCConfigFrame.IDCode,
                gsfPDCConfigFrame?.FrameRate ?? selPDCConfigFrame.FrameRate,
                gsfPDCConfigFrame?.Name ?? selPDCConfigFrame.Name,
                gsfPDCConfigFrame?.Acronym ?? selPDCConfigFrame.Acronym);

            textBoxSCFConnectionName.Text = selPDCConfigFrame.Acronym;
            textBoxSCFConnectionName.Click += ConnectionNameOnClick;
            
            textBoxGCFConnectionName.Text = gsfPDCConfigFrame?.Acronym;
            textBoxGCFConnectionName.Click += ConnectionNameOnClick;
            
            textBoxTCFConnectionName.Text = TargetConfigFrame.Acronym;
            textBoxTCFConnectionName.TextChanged += (_, _) => TargetConfigFrame.Acronym = textBoxTCFConnectionName.Text;
            textBoxTCFConnectionName.Leave += (_, _) => textBoxTCFConnectionName.Text = textBoxTCFConnectionName.Text.GetCleanAcronym();

            HashSet<ConfigurationCell> matchedCells = new HashSet<ConfigurationCell>();

            for (int i = 0; i < selPDCConfigFrame.Cells.Count; i++)
            {
                ConfigurationCell selConfigCell = selPDCConfigFrame.Cells[i];
                ConfigurationCell gsfConfigCell = gsfPDCConfigFrame?.Cells.FirstOrDefault(cell => cell.IDCode == selConfigCell.IDCode) as ConfigurationCell;
                ConfigurationCell targetConfigCell = new ConfigurationCell(
                    TargetConfigFrame, 
                    gsfConfigCell?.StationName ?? selConfigCell.StationName, 
                    gsfConfigCell?.IDCode ?? selConfigCell.IDCode, 
                    gsfConfigCell?.IDLabel ?? selConfigCell.IDLabel)
                {
                    FrequencyDefinition = selConfigCell.FrequencyDefinition
                };

                foreach (IPhasorDefinition phasorDefinition in selConfigCell.PhasorDefinitions)
                    targetConfigCell.PhasorDefinitions.Add(phasorDefinition);

                foreach (IAnalogDefinition analogDefinition in selConfigCell.AnalogDefinitions)
                    targetConfigCell.AnalogDefinitions.Add(analogDefinition);
                
                foreach (IDigitalDefinition digitalDefinition in selConfigCell.DigitalDefinitions)
                    targetConfigCell.DigitalDefinitions.Add(digitalDefinition);

                TargetConfigFrame.Cells.Add(targetConfigCell);

                if (gsfConfigCell is not null)
                    matchedCells.Add(gsfConfigCell);

                TextBox targetAcronymTextBox = AddRow($"PMU {i + 1:N0} Acronym", selConfigCell.IDLabel, gsfConfigCell?.IDLabel);
                targetAcronymTextBox.TextChanged += (_, _) => targetConfigCell.IDLabel = targetAcronymTextBox.Text;
            }

            // TODO: Add unmatched cells
        }

        private void ConnectionNameOnClick(object sender, EventArgs _)
        {
            if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                textBoxTCFConnectionName.Text = textBox.Text;
        }

        private TextBox AddRow(string dataItemLabel, string selValue, string gsfValue)
        {
            TableLayoutPanel table = tableLayoutPanelConfigDetails;

            int rowIndex = table.RowCount++ - 1;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));

            Label dataItem = NewLabel();
            dataItem.Text = dataItemLabel;
            table.Controls.Add(dataItem, 0, rowIndex);

            TextBox selTextBox = NewTextBox(true);
            selTextBox.Text = selValue;
            table.Controls.Add(selTextBox, 1, rowIndex);

            TextBox gsfTextBox = NewTextBox(true);
            gsfTextBox.Text = gsfValue;
            table.Controls.Add(gsfTextBox, 2, rowIndex);

            TextBox targetTextBox = NewTextBox(false);
            targetTextBox.Text = gsfValue ?? selValue;
            targetTextBox.Leave += (_, _) => targetTextBox.Text = targetTextBox.Text.GetCleanAcronym();
            table.Controls.Add(targetTextBox, 3, rowIndex);

            void textBoxOnClick(object sender, EventArgs _)
            {
                if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                    targetTextBox.Text = textBox.Text;
            }

            selTextBox.Click += textBoxOnClick;
            gsfTextBox.Click += textBoxOnClick;

            return targetTextBox;
        }

        private Label NewLabel()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Font = labelConnectionName.Font,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private TextBox NewTextBox(bool readOnly)
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                CharacterCasing = CharacterCasing.Upper,
                ReadOnly = readOnly
            };
        }
    }
}
