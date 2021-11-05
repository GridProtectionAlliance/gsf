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
using GSF.Configuration;
using GSF.Data.Model;
using GSF.PhasorProtocols;
using SELPDCImporter.Model;

namespace SELPDCImporter
{
    public partial class EditDetails : Form
    {
        private readonly Color m_matchedColor = Color.FromArgb(192, 255, 192);
        private readonly Color m_unmatchedColor = SystemColors.Control;
        private readonly List<CheckBox> m_deleteCheckBoxes = new();
        private readonly List<Control> m_validatedControls = new();

        public EditDetails()
        {
            InitializeComponent();
        }

        public ImportParameters ImportParams { get; set; }

        public ConfigurationFrame TargetConfigFrame { get; private set; }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void LoadHistorians()
        {
            ConfigurationFile configurationFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection systemSettings = configurationFile.Settings["systemSettings"];
            string targetHistorianIDValue = systemSettings["TargetHistorianID"]?.Value ?? "0";

            TableOperations<Historian> historianTable = new(ImportParams.Connection);
            Dictionary<int, string> historians = historianTable.QueryHistorians().ToDictionary(historian => historian.ID, historian => historian.Acronym);
            historians.Add(0, "None");

            comboBoxHistorian.DataSource = new BindingSource(historians, null);
            comboBoxHistorian.ValueMember = "Key";
            comboBoxHistorian.DisplayMember = "Value";

            void updateSelectedHistorian()
            {
                if (!int.TryParse(comboBoxHistorian.SelectedValue.ToString(), out int historianID))
                    historianID = -1;

                TargetConfigFrame.HistorianID = historianID <= 0 ? null : historianID;
                systemSettings["TargetHistorianID", true].Update(historianID, "Target historian ID.", false, SettingScope.User);
            }

            comboBoxHistorian.SelectedValueChanged += (_, _) => updateSelectedHistorian();
            
            if (!int.TryParse(targetHistorianIDValue, out int targetHistorianID) || !historians.ContainsKey(targetHistorianID))
                targetHistorianID = -1;

            if (targetHistorianID == -1)
            {
                // Attempt to select an ideal initial target historian
                if (historians.ContainsValue("PPA"))
                    targetHistorianID = historians.FirstOrDefault(kvp => string.Equals(kvp.Value, "PPA")).Key;
                else if (historians.Count > 1)
                    targetHistorianID = historians.FirstOrDefault(kvp => kvp.Key > 0 && !string.Equals(kvp.Value, "STAT")).Key;

                if (targetHistorianID == -1)
                    targetHistorianID = 0;
            }

            comboBoxHistorian.SelectedValue = targetHistorianID;
            updateSelectedHistorian();
        }

        private void EditDetails_Load(object sender, EventArgs e)
        {
            ConfigurationFrame selPDCConfigFrame = ImportParams.SELPDCConfigFrame;
            ConfigurationFrame gsfPDCConfigFrame = ImportParams.GSFPDCConfigFrame;

            TargetConfigFrame = ConfigurationFrame.Clone(gsfPDCConfigFrame ?? selPDCConfigFrame, false);

            checkBoxDeleteAll.CheckedChanged += (_, _) =>
            {
                foreach (CheckBox checkBox in m_deleteCheckBoxes)
                    checkBox.Checked = checkBoxDeleteAll.Checked;
            };

            buttonImport.Click += (_, _) =>
            {
                int validationErrors = m_validatedControls.Count(control => !string.IsNullOrWhiteSpace(errorProvider.GetError(control)));

                if (validationErrors > 0)
                {
                    MessageBox.Show(this, $"Cannot Import: There {(validationErrors == 1 ? "is" : "are")} {validationErrors:N0} validation error{(validationErrors == 1 ? "" : "s")} that must be corrected before PDC can be imported.", "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (m_deleteCheckBoxes.All(checkBox => checkBox.Checked))
                {
                    if (MessageBox.Show(this, $"All PMUs ({TargetConfigFrame.Cells.Count:N0} total) are marked for deletion, are you sure this is the desired operation?", "Delete All Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;

                    MessageBox.Show(this, $"All PMUs will now be deleted. Note that associated connection \"{textBoxTCFConnectionName.Text}\" will need to be manually removed from GSF host application.", "Deleting All PMUs", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                DialogResult = DialogResult.OK;
                ImportParams.TargetConfigFrame = TargetConfigFrame;
                Close();
            };

            textBoxSCFConnectionName.Text = selPDCConfigFrame.Acronym;
            textBoxSCFConnectionName.Click += ConnectionNameOnClick;

            textBoxGCFConnectionName.Text = gsfPDCConfigFrame?.Acronym;
            textBoxGCFConnectionName.Click += ConnectionNameOnClick;

            textBoxTCFConnectionName.TextChanged += (_, _) =>
            {
                TargetConfigFrame.Acronym = textBoxTCFConnectionName.Text;
                
                bool matchesSCF = string.Equals(textBoxTCFConnectionName.Text, textBoxSCFConnectionName.Text);
                bool matchesGSF = string.Equals(textBoxTCFConnectionName.Text, textBoxGCFConnectionName.Text);
                
                textBoxTCFConnectionName.BackColor = matchesSCF || matchesGSF ? m_matchedColor : m_unmatchedColor;
                textBoxSCFConnectionName.BackColor = matchesSCF ? m_matchedColor : m_unmatchedColor;
                textBoxGCFConnectionName.BackColor = matchesGSF ? m_matchedColor : m_unmatchedColor;

                ValidateChildren();
            };

            textBoxTCFConnectionName.Validated += (_, _) => errorProvider.SetError(textBoxTCFConnectionName, ParentDeviceIsUnique() ? string.Empty : "Acronym already exists with different ID Code!");

            m_validatedControls.Add(textBoxTCFConnectionName);

            textBoxTCFConnectionName.Leave += (_, _) => textBoxTCFConnectionName.Text = textBoxTCFConnectionName.Text.GetCleanAcronym();

            textBoxTCFConnectionName.Text = TargetConfigFrame.Acronym;

            TableLayoutPanel table = tableLayoutPanelConfigDetails;
            List<ConfigurationCell> matchedCells = new();

            table.SuspendLayout();

            for (int i = 0; i < selPDCConfigFrame.Cells.Count; i++)
            {
                ConfigurationCell selConfigCell = selPDCConfigFrame.Cells[i];
                ConfigurationCell gsfConfigCell = gsfPDCConfigFrame?.Cells.FirstOrDefault(cell => cell.IDCode == selConfigCell.IDCode) as ConfigurationCell;
                ConfigurationCell targetConfigCell = new(
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
                targetTextBox.Validated += (_, _) => ValidateChildDevice(targetTextBox, targetConfigCell);

                m_validatedControls.Add(targetTextBox);

                CheckBox deletedCheckBox = dataControls.Item2;
                deletedCheckBox.CheckedChanged += (_, _) => targetConfigCell.Delete = deletedCheckBox.Checked;
            }

            // Add unmatched cells
            if (gsfPDCConfigFrame is not null && gsfPDCConfigFrame.Cells.Count > 0)
            {
                HashSet<ConfigurationCell> unmatchedCells = new(gsfPDCConfigFrame.Cells.Select(cell => cell as ConfigurationCell));
                unmatchedCells.ExceptWith(matchedCells);
                int i = TargetConfigFrame.Cells.Count;

                foreach (ConfigurationCell gsfConfigCell in unmatchedCells)
                {
                    if (gsfConfigCell is null)
                        continue;

                    ConfigurationCell targetConfigCell = new(
                        TargetConfigFrame,
                        gsfConfigCell.StationName,
                        gsfConfigCell.IDCode,
                        gsfConfigCell.IDLabel)
                    {
                        FrequencyDefinition = gsfConfigCell.FrequencyDefinition,
                        Delete = true
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
                    targetTextBox.Validated += (_, _) => ValidateChildDevice(targetTextBox, targetConfigCell);

                    m_validatedControls.Add(targetTextBox);

                    CheckBox deletedCheckBox = dataControls.Item2;
                    deletedCheckBox.CheckedChanged += (_, _) => targetConfigCell.Delete = deletedCheckBox.Checked;
                }
            }

            // Add a final blank row
            table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            
            table.ResumeLayout();

            // Update tab ordering
            int tabIndex = 0;

            foreach (Control control in m_validatedControls)
                control.TabIndex = tabIndex++;

            buttonImport.TabIndex = tabIndex++;
            buttonCancel.TabIndex = tabIndex;

            // Load target historian list
            LoadHistorians();

            // Perform initial validation
            ValidateChildren();
            if (m_validatedControls.Count(control => !string.IsNullOrWhiteSpace(errorProvider.GetError(control))) > 0)
            {
                if ((gsfPDCConfigFrame?.ID ?? 0) == 0)
                    labelWarning1.Visible = true;
                else
                    labelWarning2.Visible = true;
            }

            textBoxTCFConnectionName.Focus();

            if (!tableLayoutPanelConfigDetails.VerticalScroll.Visible)
                return;

            Width += 20;
        }

        private void EditDetails_Resize(object sender, EventArgs e) =>
            panelHistorian.Width = Width - 180;

        private bool LocalAcronymIsUnique(string acronym) => 
            m_validatedControls.Count(control => string.Equals(control.Text, acronym)) == 1;

        private bool LocalIDCodeIsUnique(ushort idCode) =>
            TargetConfigFrame.Cells.Cast<ConfigurationCell>().Count(cell => cell.IDCode == idCode && !cell.Delete) < 2;

        private bool ParentDeviceIsUnique() =>
            ImportParams.DeviceTable.ParentDeviceIsUnique(textBoxTCFConnectionName.Text, TargetConfigFrame.IDCode) &&
            LocalAcronymIsUnique(textBoxTCFConnectionName.Text);

        private bool ChildDeviceIsUnique(TextBox targetTextBox, ConfigurationCell targetConfigCell) =>
            targetConfigCell.Delete ||
            ImportParams.DeviceTable.ChildDeviceIsUnique(TargetConfigFrame.ID, targetTextBox.Text, targetConfigCell.IDCode) &&
            LocalAcronymIsUnique(targetTextBox.Text);

        private void ValidateChildDevice(TextBox targetTextBox, ConfigurationCell targetConfigCell)
        {
            string errorMessage = "";

            if (!ChildDeviceIsUnique(targetTextBox, targetConfigCell))
                errorMessage = $"PMU acronym \"{targetTextBox.Text}\" already exists!";

            if (!LocalIDCodeIsUnique(targetConfigCell.IDCode))
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    errorMessage = $"{errorMessage.Substring(0, errorMessage.Length - 1)} and ";

                errorMessage = $"{errorMessage}PMU \"{targetTextBox.Text}\" ID Code \"{targetConfigCell.IDCode}\" is not unique for this connection!";
            }

            errorProvider.SetError(targetTextBox,  errorMessage);
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

            checkBox.CheckedChanged += (_, _) => ValidateChildren();

            TextBox selTextBox = NewTextBox(true);
            selTextBox.TabStop = false;
            selTextBox.Text = selValue;

            table.Controls.Add(selTextBox, 1, rowIndex);

            TextBox gsfTextBox = NewTextBox(true);
            gsfTextBox.TabStop = false;
            gsfTextBox.Text = gsfValue;

            table.Controls.Add(gsfTextBox, 2, rowIndex);

            TextBox targetTextBox = NewTextBox(false);
            errorProvider.SetIconPadding(targetTextBox, errorProvider.GetIconPadding(textBoxTCFConnectionName));

            targetTextBox.TextChanged += (_, _) =>
            {
                bool matchesSCF = string.Equals(targetTextBox.Text, selTextBox.Text);
                bool matchesGSF = string.Equals(targetTextBox.Text, gsfTextBox.Text);
                
                targetTextBox.BackColor = matchesSCF || matchesGSF ? m_matchedColor : m_unmatchedColor;
                selTextBox.BackColor = matchesSCF ? m_matchedColor : m_unmatchedColor;
                gsfTextBox.BackColor = matchesGSF ? m_matchedColor : m_unmatchedColor;

                ValidateChildren();
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
            Panel panel = new();
            panel.SuspendLayout();

            checkBox = NewCheckBox(deleted);

            panel.Controls.Add(checkBox);
            panel.Controls.Add(NewLabel(labelText));
            panel.Dock = DockStyle.Fill;
            panel.Margin = panelDataItem.Margin;

            panel.ResumeLayout();
            panel.PerformLayout();

            return panel;
        }

        private CheckBox NewCheckBox(bool @checked)
        {
            CheckBox checkBox = new()
            {
                AutoSize = true,
                Dock = checkBoxDeleteAll.Dock,
                Padding = new Padding(10, 2, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = true,
                Checked = @checked,
                TabStop = false
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
    }
}
