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
        private readonly Color m_matchedColor = Color.FromArgb(192, 255, 192);
        private readonly Color m_unmatchedColor = SystemColors.Control;
        private readonly List<CheckBox> m_deleteCheckBoxes = new List<CheckBox>();

        public EditDetails()
        {
            InitializeComponent();
        }

        public ImportParameters ImportParams { get; set; }

        public ConfigurationFrame TargetConfigFrame { get; private set; }

        private void EditDetails_Load(object sender, EventArgs e)
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

            textBoxTCFConnectionName.TextChanged += (_, _) =>
            {
                TargetConfigFrame.Acronym = textBoxTCFConnectionName.Text;
                textBoxSCFConnectionName.BackColor = string.Equals(textBoxTCFConnectionName.Text, textBoxSCFConnectionName.Text) ? m_matchedColor : m_unmatchedColor;
                textBoxGCFConnectionName.BackColor = string.Equals(textBoxTCFConnectionName.Text, textBoxGCFConnectionName.Text) ? m_matchedColor : m_unmatchedColor;
            };

            textBoxTCFConnectionName.Leave += (_, _) => textBoxTCFConnectionName.Text = textBoxTCFConnectionName.Text.GetCleanAcronym();

            textBoxTCFConnectionName.Text = TargetConfigFrame.Acronym;

            TableLayoutPanel table = tableLayoutPanelConfigDetails;
            List<ConfigurationCell> matchedCells = new List<ConfigurationCell>();

            table.SuspendLayout();

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

                Tuple<TextBox, CheckBox> dataControls = AddRow(table, $"PMU {i + 1:N0} Acronym:", selConfigCell.IDLabel, gsfConfigCell?.IDLabel);

                TextBox targetTextBox = dataControls.Item1;
                targetTextBox.TextChanged += (_, _) => targetConfigCell.IDLabel = targetTextBox.Text;

                CheckBox deletedCheckBox = dataControls.Item2;
                deletedCheckBox.CheckedChanged += (_, _) => targetConfigCell.Delete = deletedCheckBox.Checked;
            }

            // Add unmatched cells
            if (gsfPDCConfigFrame is not null && gsfPDCConfigFrame.Cells.Count > 0)
            {
                HashSet<ConfigurationCell> unmatchedCells = new HashSet<ConfigurationCell>(gsfPDCConfigFrame.Cells.Select(cell => cell as ConfigurationCell));
                unmatchedCells.ExceptWith(matchedCells);
                int i = TargetConfigFrame.Cells.Count;

                foreach (ConfigurationCell gsfConfigCell in unmatchedCells)
                {
                    if (gsfConfigCell is null)
                        continue;

                    ConfigurationCell targetConfigCell = new ConfigurationCell(
                        TargetConfigFrame,
                        gsfConfigCell.StationName,
                        gsfConfigCell.IDCode,
                        gsfConfigCell.IDLabel)
                    {
                        FrequencyDefinition = gsfConfigCell.FrequencyDefinition
                    };

                    foreach (IPhasorDefinition phasorDefinition in gsfConfigCell.PhasorDefinitions)
                        targetConfigCell.PhasorDefinitions.Add(phasorDefinition);

                    foreach (IAnalogDefinition analogDefinition in gsfConfigCell.AnalogDefinitions)
                        targetConfigCell.AnalogDefinitions.Add(analogDefinition);

                    foreach (IDigitalDefinition digitalDefinition in gsfConfigCell.DigitalDefinitions)
                        targetConfigCell.DigitalDefinitions.Add(digitalDefinition);

                    TargetConfigFrame.Cells.Add(targetConfigCell);

                    Tuple<TextBox, CheckBox> dataControls = AddRow(table, $"PMU {i++:N0} Acronym:", "", gsfConfigCell.IDLabel, true);

                    TextBox targetTextBox = dataControls.Item1;
                    targetTextBox.TextChanged += (_, _) => targetConfigCell.IDLabel = targetTextBox.Text;

                    CheckBox deletedCheckBox = dataControls.Item2;
                    deletedCheckBox.CheckedChanged += (_, _) => targetConfigCell.Delete = deletedCheckBox.Checked;
                }
            }

            table.ResumeLayout();
        }

        private void ConnectionNameOnClick(object sender, EventArgs _)
        {
            if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                textBoxTCFConnectionName.Text = textBox.Text;
        }

        private Tuple<TextBox, CheckBox> AddRow(TableLayoutPanel table, string dataItemLabel, string selValue, string gsfValue, bool deleted = false)
        {
            int rowIndex = table.RowCount++ - 1;

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            table.Controls.Add(NewPanel(dataItemLabel, deleted, out CheckBox checkBox), 0, rowIndex);

            TextBox selTextBox = NewTextBox(true);
            selTextBox.Text = selValue;

            table.Controls.Add(selTextBox, 1, rowIndex);

            TextBox gsfTextBox = NewTextBox(true);
            gsfTextBox.Text = gsfValue;

            table.Controls.Add(gsfTextBox, 2, rowIndex);

            TextBox targetTextBox = NewTextBox(false);
            targetTextBox.TextChanged += (_, _) =>
            {
                selTextBox.BackColor = string.Equals(targetTextBox.Text, selTextBox.Text) ? m_matchedColor : m_unmatchedColor;
                gsfTextBox.BackColor = string.Equals(targetTextBox.Text, gsfTextBox.Text) ? m_matchedColor : m_unmatchedColor;
            };
            targetTextBox.Leave += (_, _) => targetTextBox.Text = targetTextBox.Text.GetCleanAcronym();
            targetTextBox.Text = gsfValue ?? selValue;

            table.Controls.Add(targetTextBox, 3, rowIndex);

            void textBoxOnClick(object sender, EventArgs _)
            {
                if (sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                    targetTextBox.Text = textBox.Text;
            }

            selTextBox.Click += textBoxOnClick;
            gsfTextBox.Click += textBoxOnClick;

            return new Tuple<TextBox, CheckBox>(targetTextBox, checkBox);
        }

        private Panel NewPanel(string labelText, bool deleted, out CheckBox checkBox)
        {
            Panel panel = new Panel();
            panel.SuspendLayout();

            checkBox = NewCheckBox(deleted);

            panel.Controls.Add(checkBox);
            panel.Controls.Add(NewLabel(labelText));
            panel.Margin = panelDataItem.Margin;

            panel.ResumeLayout();
            panel.PerformLayout();

            return panel;
        }

        private CheckBox NewCheckBox(bool deleted)
        {
            CheckBox checkBox = new CheckBox
            {
                AutoSize = true,
                Dock = checkBoxDeleteAll.Dock,
                Padding = new Padding(10, 2, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = true,
                Checked = deleted
            };

            m_deleteCheckBoxes.Add(checkBox);

            return checkBox;
        }

        private Label NewLabel(string labelText)
        {
            return new Label
            {
                Dock = labelConnectionName.Dock,
                Font = labelConnectionName.Font,
                Padding = new Padding(0, 0, 9, 0),
                TextAlign = ContentAlignment.MiddleRight,
                Text = labelText
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

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (m_deleteCheckBoxes.All(checkBox => checkBox.Checked))
            {
                if (MessageBox.Show(this, $"All {TargetConfigFrame.Cells.Count:N0} PMUs are marked for deletion, are you sure this is the desired operation?", "Delete All Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                MessageBox.Show(this, $"All PMUs will now be deleted. Note that assoicated connection \"{textBoxTCFConnectionName.Text}\" will need to manually removed from GSF host application.", "Deleting All PMUs", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            DialogResult = DialogResult.OK;
            ImportParams.TargetConfigFrame = TargetConfigFrame;
            Close();
        }

        private void checkBoxDeleteAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (CheckBox checkBox in m_deleteCheckBoxes)
                checkBox.Checked = checkBoxDeleteAll.Checked;
        }
    }
}
