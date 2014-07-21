//******************************************************************************************************
//  ConfigurationCreator.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/13/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.PhasorProtocols.Anonymous;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.UI.Modal
{
    /// <summary>
    /// Interaction logic for ConfigurationCreator.xaml
    /// </summary>
    public partial class ConfigurationCreator : Window
    {
        #region [ Members ]

        // Fields
        private ConfigurationFrame m_configurationFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCreator"/>.
        /// </summary>
        public ConfigurationCreator()
        {
            InitializeComponent();

            m_configurationFrame = new ConfigurationFrame((ushort)0, (Ticks)0, (ushort)30);

            listBoxDevices.ItemsSource = m_configurationFrame.Cells;
            listBoxDevices.SelectedValuePath = "@IDCode";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the manually tweaked configuration frame.
        /// </summary>
        public IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
            set
            {
                IConfigurationFrame sourceFrame = value;

                // Make sure consumer actually provided a valid configuration frame
                if (sourceFrame != null)
                {
                    // Use configuration frame as-is if it already anonymous
                    ConfigurationFrame derivedFrame = value as ConfigurationFrame;

                    if (derivedFrame == null)
                    {
                        // Create a new anonymous configuration frame converted from equivalent configuration information
                        ConfigurationCell derivedCell;
                        IFrequencyDefinition sourceFrequency;

                        derivedFrame = new ConfigurationFrame(sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

                        foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
                        {
                            // Create new derived configuration cell
                            derivedCell = new ConfigurationCell(derivedFrame, sourceCell.IDCode);

                            derivedCell.StationName = sourceCell.StationName;
                            derivedCell.IDLabel = sourceCell.IDLabel;

                            // Create equivalent derived phasor definitions
                            foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                            {
                                derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.PhasorType, null));
                            }

                            // Create equivalent derived frequency definition
                            sourceFrequency = sourceCell.FrequencyDefinition;

                            if (sourceFrequency != null)
                                derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                            // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
                            foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                            {
                                derivedCell.AnalogDefinitions.Add(new AnalogDefinition(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.AnalogType));
                            }

                            // Create equivalent derived digital definitions
                            foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                            {
                                derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label, 0));
                            }

                            // Add cell to frame
                            derivedFrame.Cells.Add(derivedCell);
                        }
                    }

                    // Update working configuration frame and refresh the screen binding
                    m_configurationFrame = derivedFrame;

                    // Update binding
                    listBoxDevices.ItemsSource = m_configurationFrame.Cells;
                    listBoxDevices.SelectedValuePath = "@IDCode";

                    m_configurationFrame.Cells.RefreshBinding();

                    if (m_configurationFrame.Cells.Count > 0)
                        listBoxDevices.SelectedIndex = 0;
                }
            }
        }

        // Gets the currently selected device, no null if no devices exist or are selected
        private ConfigurationCell SelectedDevice
        {
            get
            {
                if (listBoxDevices != null && m_configurationFrame != null && m_configurationFrame.Cells.Count > 0)
                {
                    int selectedIndex = listBoxDevices.SelectedIndex;

                    if (selectedIndex >= 0 && selectedIndex < m_configurationFrame.Cells.Count)
                        return m_configurationFrame.Cells[selectedIndex];
                }

                return null;
            }
        }

        // Gets the currently selected phasor, no null if no phasors exist or are selected
        private PhasorDefinition SelectedPhasor
        {
            get
            {
                ConfigurationCell selectedDevice = this.SelectedDevice;

                if (listBoxPhasors != null && selectedDevice != null && selectedDevice.PhasorDefinitions.Count > 0)
                {
                    int selectedIndex = listBoxPhasors.SelectedIndex;

                    if (selectedIndex >= 0 && selectedIndex < selectedDevice.PhasorDefinitions.Count)
                        return selectedDevice.PhasorDefinitions[selectedIndex] as PhasorDefinition;
                }

                return null;
            }
        }

        #endregion

        #region [ Methods ]

        private void buttonDeviceAdd_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationCell device = new ConfigurationCell(m_configurationFrame, 0);
            device.IDCode = (ushort)m_configurationFrame.Cells.Count;
            device.StationName = "Device " + (device.IDCode + 1);

            m_configurationFrame.Cells.Add(device);
            listBoxDevices.SelectedIndex = (m_configurationFrame.Cells.Count - 1);
        }

        private void buttonDeviceDelete_Click(object sender, RoutedEventArgs e)
        {
            m_configurationFrame.Cells.Remove(listBoxDevices.SelectedItem as IConfigurationCell);

            if (m_configurationFrame.Cells.Count > 0)
                listBoxDevices.SelectedIndex = 0;
        }

        private void buttonDeviceCopy_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDevices.SelectedItems.Count > 0)
            {
                ConfigurationCell[] selectedDevices = listBoxDevices.SelectedItems.Cast<ConfigurationCell>().ToArray();

                foreach (ConfigurationCell selectedDevice in selectedDevices)
                {
                    if (selectedDevice != null)
                        CopyDevice(selectedDevice);
                }
            }
            else
                MessageBox.Show("No items were selected to copy.");
        }

        private void CopyDevice(ConfigurationCell sourceDevice)
        {
            // Create a new configuration cell to hold copied information
            ConfigurationCell copiedDevice = new ConfigurationCell(m_configurationFrame, 0);

            copiedDevice.IDCode = (ushort)m_configurationFrame.Cells.Count;
            copiedDevice.StationName = "Device " + (copiedDevice.IDCode + 1);

            // Create equivalent derived phasor definitions
            foreach (PhasorDefinition sourcePhasor in sourceDevice.PhasorDefinitions)
            {
                copiedDevice.PhasorDefinitions.Add(new PhasorDefinition(copiedDevice, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.PhasorType, null));
            }

            // Create equivalent derived frequency definition
            IFrequencyDefinition sourceFrequency = sourceDevice.FrequencyDefinition;

            if (sourceFrequency != null)
                copiedDevice.FrequencyDefinition = new FrequencyDefinition(copiedDevice, sourceFrequency.Label);

            // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
            foreach (AnalogDefinition sourceAnalog in sourceDevice.AnalogDefinitions)
            {
                copiedDevice.AnalogDefinitions.Add(new AnalogDefinition(copiedDevice, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.AnalogType));
            }

            // Create equivalent derived digital definitions
            foreach (DigitalDefinition sourceDigital in sourceDevice.DigitalDefinitions)
            {
                copiedDevice.DigitalDefinitions.Add(new DigitalDefinition(copiedDevice, sourceDigital.Label, sourceDigital.MaskValue));
            }

            // Add new copied cell to the list and select it
            m_configurationFrame.Cells.Add(copiedDevice);
            listBoxDevices.SelectedIndex = (m_configurationFrame.Cells.Count - 1);
        }

        private void buttonDeviceMoveUp_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = listBoxDevices.SelectedIndex;

            if (selectedIndex > 0 && selectedIndex < m_configurationFrame.Cells.Count)
            {
                ConfigurationCell selectedDevice = m_configurationFrame.Cells[selectedIndex];
                m_configurationFrame.Cells.RemoveAt(selectedIndex);
                m_configurationFrame.Cells.Insert(selectedIndex - 1, selectedDevice);
                listBoxDevices.SelectedIndex = selectedIndex - 1;
            }
        }

        private void buttonDeviceMoveDown_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = listBoxDevices.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < m_configurationFrame.Cells.Count - 1)
            {
                ConfigurationCell selectedDevice = m_configurationFrame.Cells[selectedIndex];
                m_configurationFrame.Cells.RemoveAt(selectedIndex);
                m_configurationFrame.Cells.Insert(selectedIndex + 1, selectedDevice);
                listBoxDevices.SelectedIndex = selectedIndex + 1;
            }
        }

        private void listBoxDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                textBoxDeviceName.Text = selectedDevice.StationName;
                textBoxDeviceIDCode.Text = selectedDevice.IDCode.ToString();

                if (selectedDevice.PhasorCoordinateFormat == CoordinateFormat.Polar)
                    radioButtonPhasorPolar.IsChecked = true;
                else
                    radioButtonPhasorRectangular.IsChecked = true;

                if (selectedDevice.PhasorDataFormat == DataFormat.FloatingPoint)
                    radioButtonPhasorFloatingPoint.IsChecked = true;
                else
                    radioButtonPhasorScaledInteger.IsChecked = true;

                if (selectedDevice.FrequencyDataFormat == DataFormat.FloatingPoint)
                    radioButtonFrequencyFloatingPoint.IsChecked = true;
                else
                    radioButtonFrequencyScaledInteger.IsChecked = true;

                if (selectedDevice.AnalogDataFormat == DataFormat.FloatingPoint)
                    radioButtonAnalogFloatingPoint.IsChecked = true;
                else
                    radioButtonAnalogScaledInteger.IsChecked = true;

                textBoxAnalogs.Text = selectedDevice.AnalogDefinitions.Count.ToString();

                textBoxDigitals.Text = selectedDevice.DigitalDefinitions.Count.ToString();

                listBoxPhasors.ItemsSource = selectedDevice.PhasorDefinitions;
                listBoxPhasors.SelectedValuePath = "@Index";

                if (selectedDevice.PhasorDefinitions.Count > 0)
                    listBoxPhasors.SelectedIndex = 0;
            }
        }

        private void textBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                selectedDevice.StationName = textBoxDeviceName.Text;
                m_configurationFrame.Cells.RefreshBinding();
            }
        }

        private void textBoxDeviceIDCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxDeviceIDCode.Text = textBoxDeviceIDCode.Text.RemoveCharacters(c => !Char.IsDigit(c));

            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                ushort idCode;

                if (ushort.TryParse(textBoxDeviceIDCode.Text, out idCode))
                    selectedDevice.IDCode = GenerateUniqueIDCode(idCode);
                else
                    textBoxDeviceIDCode.Text = GenerateUniqueIDCode((ushort)m_configurationFrame.Cells.Count).ToString();
            }
        }

        private ushort GenerateUniqueIDCode(ushort suggested)
        {
            IConfigurationCell cell;

            // If suggested ID code already exists, try another
            if (m_configurationFrame.Cells.TryGetByIDCode(suggested, out cell) && !Equals(cell, this.SelectedDevice))
                return GenerateUniqueIDCode((ushort)(suggested + 1));

            return suggested;
        }

        private void textBoxDeviceIDCode_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxDeviceIDCode.Text = textBoxDeviceIDCode.Text.RemoveCharacters(c => !Char.IsDigit(c));
        }

        private void radioButtonPhasorPolar_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.PhasorCoordinateFormat = CoordinateFormat.Polar;
        }

        private void radioButtonPhasorRectangular_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.PhasorCoordinateFormat = CoordinateFormat.Rectangular;
        }

        private void radioButtonPhasorFloatingPoint_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.PhasorDataFormat = DataFormat.FloatingPoint;
        }

        private void radioButtonPhasorScaledInteger_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.PhasorDataFormat = DataFormat.FixedInteger;
        }

        private void radioButtonFrequencyFloatingPoint_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.FrequencyDataFormat = DataFormat.FloatingPoint;
        }

        private void radioButtonFrequencyScaledInteger_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.FrequencyDataFormat = DataFormat.FixedInteger;
        }

        private void radioButtonAnalogFloatingPoint_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.AnalogDataFormat = DataFormat.FloatingPoint;
        }

        private void radioButtonAnalogScaledInteger_Checked(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.AnalogDataFormat = DataFormat.FixedInteger;
        }

        private void textBoxAnalogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxAnalogs.Text = textBoxAnalogs.Text.RemoveCharacters(c => !Char.IsDigit(c));

            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                ushort analogCount;

                if (ushort.TryParse(textBoxAnalogs.Text, out analogCount))
                {
                    int difference = analogCount - selectedDevice.AnalogDefinitions.Count;

                    if (difference > 0)
                    {
                        for (int i = 0; i < difference; i++)
                        {
                            selectedDevice.AnalogDefinitions.Add(new AnalogDefinition(selectedDevice, "Analog " + selectedDevice.AnalogDefinitions.Count, 0, AnalogType.SinglePointOnWave));
                        }
                    }
                    else if (difference < 0)
                    {
                        for (int i = 0; i < difference; i++)
                        {
                            selectedDevice.AnalogDefinitions.RemoveAt(selectedDevice.AnalogDefinitions.Count - 1);
                        }
                    }
                }
                else
                    selectedDevice.AnalogDefinitions.Clear();
            }
        }

        private void textBoxDigitals_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxDigitals.Text = textBoxDigitals.Text.RemoveCharacters(c => !Char.IsDigit(c));

            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                ushort digitalCount;

                if (ushort.TryParse(textBoxDigitals.Text, out digitalCount))
                {
                    int difference = digitalCount - selectedDevice.DigitalDefinitions.Count;

                    if (difference > 0)
                    {
                        for (int i = 0; i < difference; i++)
                        {
                            selectedDevice.DigitalDefinitions.Add(new DigitalDefinition(selectedDevice, "Digital " + selectedDevice.DigitalDefinitions.Count, 0));
                        }
                    }
                    else if (difference < 0)
                    {
                        for (int i = 0; i < difference; i++)
                        {
                            selectedDevice.DigitalDefinitions.RemoveAt(selectedDevice.DigitalDefinitions.Count - 1);
                        }
                    }
                }
                else
                    selectedDevice.DigitalDefinitions.Clear();
            }
        }

        private void textBoxAnalogs_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxAnalogs.Text = textBoxAnalogs.Text.RemoveCharacters(c => !Char.IsDigit(c));
        }

        private void textBoxDigitals_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxDigitals.Text = textBoxDigitals.Text.RemoveCharacters(c => !Char.IsDigit(c));
        }

        private void listBoxPhasors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PhasorDefinition selectedPhasor = this.SelectedPhasor;

            if (selectedPhasor != null)
            {
                textBoxPhasorLabel.Text = selectedPhasor.Label;
                textBoxPhasorScale.Text = selectedPhasor.ScalingValue.ToString();
                textBoxPhasorOffset.Text = selectedPhasor.Offset.ToString();

                if (selectedPhasor.PhasorType == PhasorType.Current)
                    radioButtonPhasorTypeCurrent.IsChecked = true;
                else
                    radioButtonPhasorTypeVoltage.IsChecked = true;
            }
        }

        private void buttonPhasorAdd_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                PhasorDefinition phasor = new PhasorDefinition(selectedDevice, "Phasor " + (selectedDevice.PhasorDefinitions.Count + 1), 1, PhasorType.Current, null);
                selectedDevice.PhasorDefinitions.Add(phasor);
                listBoxPhasors.SelectedIndex = (selectedDevice.PhasorDefinitions.Count - 1);
            }
        }

        private void buttonPhasorCopy_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {

                if (listBoxPhasors.SelectedItems.Count > 0)
                {
                    IPhasorDefinition[] selectedPhasors = listBoxPhasors.SelectedItems.Cast<IPhasorDefinition>().ToArray();

                    foreach (IPhasorDefinition selectedPhasor in selectedPhasors)
                    {
                        if (selectedPhasor != null)
                            selectedDevice.PhasorDefinitions.Add(new PhasorDefinition(selectedDevice, "Phasor " + (selectedDevice.PhasorDefinitions.Count + 1), selectedPhasor.ScalingValue, selectedPhasor.PhasorType, null));
                    }
                }
                else
                    MessageBox.Show("No items were selected to copy.");
            }
        }

        private void buttonPhasorDelete_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                selectedDevice.PhasorDefinitions.Remove(listBoxPhasors.SelectedItem as IPhasorDefinition);


                if (selectedDevice.PhasorDefinitions.Count > 0)
                    listBoxPhasors.SelectedIndex = 0;
            }
        }

        private void buttonPhasorMoveUp_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                int selectedIndex = listBoxPhasors.SelectedIndex;

                if (selectedIndex > 0 && selectedIndex < selectedDevice.PhasorDefinitions.Count)
                {
                    IPhasorDefinition selectedPhasor = selectedDevice.PhasorDefinitions[selectedIndex];
                    selectedDevice.PhasorDefinitions.RemoveAt(selectedIndex);
                    selectedDevice.PhasorDefinitions.Insert(selectedIndex - 1, selectedPhasor);
                    listBoxPhasors.SelectedIndex = selectedIndex - 1;

                }
            }
        }

        private void buttonPhasorMoveDown_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
            {
                int selectedIndex = listBoxPhasors.SelectedIndex;

                if (selectedIndex >= 0 && selectedIndex < selectedDevice.PhasorDefinitions.Count - 1)
                {
                    IPhasorDefinition selectedPhasor = selectedDevice.PhasorDefinitions[selectedIndex];
                    selectedDevice.PhasorDefinitions.RemoveAt(selectedIndex);
                    selectedDevice.PhasorDefinitions.Insert(selectedIndex + 1, selectedPhasor);
                    listBoxPhasors.SelectedIndex = selectedIndex + 1;

                }
            }
        }

        private void textBoxPhasorLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigurationCell selectedDevice = this.SelectedDevice;
            IPhasorDefinition selectedPhasor = this.SelectedPhasor;

            if (selectedDevice != null && selectedPhasor != null)
            {
                selectedPhasor.Label = textBoxPhasorLabel.Text;
                selectedDevice.PhasorDefinitions.RefreshBinding();
            }
        }

        private void textBoxPhasorScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxPhasorScale.Text = textBoxPhasorScale.Text.RemoveCharacters(c => !Char.IsDigit(c));

            IPhasorDefinition selectedPhasor = this.SelectedPhasor;

            if (selectedPhasor != null)
            {
                UInt24 scale;

                if (UInt24.TryParse(textBoxPhasorScale.Text, out scale))
                {
                    if (scale > selectedPhasor.MaximumScalingValue)
                        selectedPhasor.ScalingValue = selectedPhasor.MaximumScalingValue;
                    else
                        selectedPhasor.ScalingValue = scale;
                }
                else
                    textBoxPhasorScale.Text = "1";
            }
        }

        private void textBoxPhasorOffset_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxPhasorOffset.Text = textBoxPhasorOffset.Text.RemoveCharacters(c => !Char.IsNumber(c) && c != '.' && c != '+' && c != '-');

            IPhasorDefinition selectedPhasor = this.SelectedPhasor;

            if (selectedPhasor != null)
            {
                double offset;

                if (double.TryParse(textBoxPhasorOffset.Text, out offset))
                    selectedPhasor.Offset = offset;
                else
                    textBoxPhasorOffset.Text = "0";
            }
        }

        private void textBoxPhasorScale_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxPhasorScale.Text = textBoxPhasorScale.Text.RemoveCharacters(c => !Char.IsDigit(c));
        }

        private void textBoxPhasorOffset_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxPhasorOffset.Text = textBoxPhasorOffset.Text.RemoveCharacters(c => !Char.IsNumber(c) && c != '.' && c != '+' && c != '-');
        }

        private void radioButtonPhasorTypeCurrent_Checked(object sender, RoutedEventArgs e)
        {
            IPhasorDefinition selectedPhasor = this.SelectedPhasor;

            if (selectedPhasor != null)
                selectedPhasor.PhasorType = PhasorType.Current;

            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.PhasorDefinitions.RefreshBinding();
        }

        private void radioButtonPhasorTypeVoltage_Checked(object sender, RoutedEventArgs e)
        {
            IPhasorDefinition selectedPhasor = this.SelectedPhasor;

            if (selectedPhasor != null)
                selectedPhasor.PhasorType = PhasorType.Voltage;

            ConfigurationCell selectedDevice = this.SelectedDevice;

            if (selectedDevice != null)
                selectedDevice.PhasorDefinitions.RefreshBinding();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        #endregion
    }
}
