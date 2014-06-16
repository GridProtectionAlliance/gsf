//******************************************************************************************************
//  AverageFrequencyViewModel.cs - Gbtc
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
//  08/20/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GSF.Data;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    /// <summary>
    /// View model for the <see cref="AverageFrequencyUserControl"/>.
    /// </summary>
    public class AverageFrequencyViewModel : DependencyObject, INotifyPropertyChanged
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents a mapping between a frequency input definition
        /// and an average frequency output definition.
        /// </summary>
        public class IOMapping
        {
            private string m_signalReference;
            private string m_inputKey;
            private string m_outputKey;

            /// <summary>
            /// Gets or sets the signal reference of the frequency input.
            /// </summary>
            public string SignalReference
            {
                get
                {
                    return m_signalReference;
                }
                set
                {
                    m_signalReference = value;
                }
            }

            /// <summary>
            /// Gets or sets the measurement key of the frequency input.
            /// </summary>
            public string InputKey
            {
                get
                {
                    return m_inputKey;
                }
                set
                {
                    m_inputKey = value;
                }
            }

            /// <summary>
            /// Gets or sets the measurement key of the frequency output.
            /// </summary>
            public string OutputKey
            {
                get
                {
                    return m_outputKey;
                }
                set
                {
                    m_outputKey = value;
                }
            }
        }

        /// <summary>
        /// Represents a frequency measurement which can be selected
        /// as an input to the average frequency calculator.
        /// </summary>
        public class FrequencyMeasurement
        {
            private string m_signalReference;
            private string m_key;

            /// <summary>
            /// Gets or sets the signal reference of the frequency measurement.
            /// </summary>
            public string SignalReference
            {
                get
                {
                    return m_signalReference;
                }
                set
                {
                    m_signalReference = value;
                }
            }

            /// <summary>
            /// Gets or sets the measurement key of the frequency measurement.
            /// </summary>
            public string Key
            {
                get
                {
                    return m_key;
                }
                set
                {
                    m_key = value;
                }
            }
        }

        // Data model for the average frequency user interface.
        private class DataModel
        {
            public readonly List<Adapter> Adapters;
            public readonly List<Device> Devices;
            public readonly List<Measurement> Measurements;

            public DataModel()
            {
                using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    Adapters = Adapter.Load(database, AdapterType.Action, Adapter.LoadIDs(database, AdapterType.Action, "", "")).ToList();
                    Devices = Device.Load(database, Device.LoadKeys(database)).ToList();
                    Measurements = Measurement.GetMeasurements(database, "WHERE SignalAcronym = 'FREQ'").ToList();
                }
            }
        }

        // Events

        /// <summary>
        /// Defines an event for when a property has changed in the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Fields
        private ObservableCollection<string> m_calculatorNames;
        private ObservableCollection<string> m_virtualDeviceNames;
        private ObservableCollection<IOMapping> m_ioMappings;
        private ObservableCollection<FrequencyMeasurement> m_frequencyMeasurements;

        private string m_selectedCalculatorName;
        private string m_selectedVirtualDeviceName;
        private string m_oldVirtualDeviceName;
        private IOMapping m_selectedIOMapping;
        private FrequencyMeasurement m_selectedFrequencyMeasurement;

        private readonly RelayCommand m_addSelectedCommand;
        private readonly RelayCommand m_removeSelectedCommand;
        private DataModel m_dataModel;
        private bool m_ignorePropertyNotify;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AverageFrequencyViewModel"/> class.
        /// </summary>
        public AverageFrequencyViewModel()
        {
            m_addSelectedCommand = new RelayCommand(AddSelected);
            m_removeSelectedCommand = new RelayCommand(RemoveSelected);
            PropertyChanged += (sender, args) => HandlePropertyChanged(args.PropertyName);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets calculator names for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public ObservableCollection<string> CalculatorNames
        {
            get
            {
                return m_calculatorNames;
            }
            set
            {
                m_calculatorNames = value;
                OnPropertyChanged("CalculatorNames");
            }
        }

        /// <summary>
        /// Gets or sets the virtual device names for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public ObservableCollection<string> VirtualDeviceNames
        {
            get
            {
                return m_virtualDeviceNames;
            }
            set
            {
                m_virtualDeviceNames = value;
                OnPropertyChanged("VirtualDeviceNames");
            }
        }

        /// <summary>
        /// Gets or sets the Input/Output mappings for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public ObservableCollection<IOMapping> IOMappings
        {
            get
            {
                return m_ioMappings;
            }
            set
            {
                m_ioMappings = value;
                OnPropertyChanged("IOMappings");
            }
        }

        /// <summary>
        /// Gets or sets the frequency measurements for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public ObservableCollection<FrequencyMeasurement> FrequencyMeasurements
        {
            get
            {
                return m_frequencyMeasurements;
            }
            set
            {
                m_frequencyMeasurements = value;
                OnPropertyChanged("FrequencyMeasurements");
            }
        }

        /// <summary>
        /// Gets or sets the selected calculator name for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public string SelectedCalculatorName
        {
            get
            {
                return m_selectedCalculatorName;
            }
            set
            {
                m_selectedCalculatorName = value;
                OnPropertyChanged("SelectedCalculatorName");
            }
        }

        /// <summary>
        /// Gets or sets the selected virtual device name for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public string SelectedVirtualDeviceName
        {
            get
            {
                return m_selectedVirtualDeviceName;
            }
            set
            {
                m_oldVirtualDeviceName = m_selectedVirtualDeviceName;
                m_selectedVirtualDeviceName = value;
                OnPropertyChanged("SelectedVirtualDeviceName");
            }
        }

        /// <summary>
        /// Gets or sets the selected Input/Output mapping for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public IOMapping SelectedIOMapping
        {
            get
            {
                return m_selectedIOMapping;
            }
            set
            {
                m_selectedIOMapping = value;
                OnPropertyChanged("SelectedIOMapping");
            }
        }

        /// <summary>
        /// Gets or sets the selected frequency measurement for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public FrequencyMeasurement SelectedFrequencyMeasurement
        {
            get
            {
                return m_selectedFrequencyMeasurement;
            }
            set
            {
                m_selectedFrequencyMeasurement = value;
                OnPropertyChanged("SelectedFrequencyMeasurement");
            }
        }

        /// <summary>
        /// Gets the add selected relay command for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public RelayCommand AddSelectedCommand
        {
            get
            {
                return m_addSelectedCommand;
            }
        }

        /// <summary>
        /// Gets the remove selected relay command for the <see cref="AverageFrequencyViewModel"/>.
        /// </summary>
        public RelayCommand RemoveSelectedCommand
        {
            get
            {
                return m_removeSelectedCommand;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Called when the view is loaded.
        /// </summary>
        public virtual void Load()
        {
            IEnumerable<string> calculatorNames;
            IEnumerable<string> virtualDeviceNames;

            try
            {
                m_dataModel = new DataModel();

                calculatorNames = m_dataModel.Adapters
                    .Where(adapter => adapter.AssemblyName == "OneSecondFrequencyAverager.dll")
                    .Where(adapter => adapter.TypeName == "OneSecondFrequencyAverager.OneSecondFrequencyAverager")
                    .Select(adapter => adapter.AdapterName);

                virtualDeviceNames = m_dataModel.Devices
                    .Where(device => device.ProtocolName == "Virtual Device")
                    .Select(device => device.Acronym);

                CalculatorNames = new ObservableCollection<string>(calculatorNames);
                VirtualDeviceNames = new ObservableCollection<string>(virtualDeviceNames);

                SelectCalculatorName();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR - Unable to load measurements from database due to exception: " + ex.Message, "Load", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Called when the view is unloaded.
        /// </summary>
        public virtual void Unload()
        {
            const string calculatorNameKey = "AverageFrequency.CalculatorName";
            string virtualDeviceNameKey = string.Format("AverageFrequency.{0}.VirtualDeviceName", m_selectedCalculatorName);

            if (!string.IsNullOrEmpty(m_selectedCalculatorName))
                IsolatedStorageManager.WriteToIsolatedStorage(calculatorNameKey, m_selectedCalculatorName);

            if (!string.IsNullOrEmpty(m_selectedVirtualDeviceName))
                IsolatedStorageManager.WriteToIsolatedStorage(virtualDeviceNameKey, m_selectedVirtualDeviceName);
        }

        /// <summary>
        /// Handles change in a property once per call stack.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void HandlePropertyChangedOnce(string propertyName)
        {
            switch (propertyName)
            {
                case "SelectedCalculatorName":
                    SelectVirtualDeviceName();
                    UpdateIOMappings();
                    UpdateFrequencyMeasurements();
                    break;

                case "SelectedVirtualDeviceName":
                    // Check to see if a change was made to the value of the property
                    if (m_selectedVirtualDeviceName == m_oldVirtualDeviceName)
                        return;

                    // Ask the user to confirm their selection
                    if (ConfirmVirtualDeviceSelection())
                    {
                        UpdateVirtualDevice();
                    }
                    else
                    {
                        // User chose not to confirm; restore the old name and notify the view
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            m_selectedVirtualDeviceName = m_oldVirtualDeviceName;
                            OnPropertyChanged("SelectedVirtualDeviceName");
                        }));
                    }

                    break;
            }
        }

        // Calls HandlePropertyChangedOnce in such a way that prevents infinite recursion when modifying other properties.
        private void HandlePropertyChanged(string propertyName)
        {
            if (m_ignorePropertyNotify)
                return;

            m_ignorePropertyNotify = true;
            HandlePropertyChangedOnce(propertyName);
            m_ignorePropertyNotify = false;
        }

        #region [ User adds a frequency ]

        // Adds the selected measurement to the average calculator.
        private void AddSelected()
        {
            if (string.IsNullOrEmpty(m_selectedCalculatorName))
                return;

            if ((object)m_selectedFrequencyMeasurement == null)
                return;

            CreateOutputMeasurement();
            AddNewMapping();
            Load();
        }

        // Creates a measurement to define the average frequency signal.
        private void CreateOutputMeasurement()
        {
            Device virtualDevice = null;
            Measurement inputMeasurement;
            Measurement outputMeasurement;

            if (!string.IsNullOrEmpty(m_selectedVirtualDeviceName))
            {
                virtualDevice = m_dataModel.Devices
                    .Single(dev => dev.Acronym == m_selectedVirtualDeviceName);
            }

            inputMeasurement = m_dataModel.Measurements
                .Single(measurement => measurement.ID == m_selectedFrequencyMeasurement.Key);

            outputMeasurement = new Measurement
                {
                    HistorianID = inputMeasurement.HistorianID,
                    DeviceID = ((object)virtualDevice != null) ? virtualDevice.ID : (int?)null,
                    PointTag = string.Format("{0}_{1}", m_selectedCalculatorName, inputMeasurement.PointTag),
                    SignalTypeID = inputMeasurement.SignalTypeID,
                    SignalReference = string.Format("{0}-FQ", m_selectedVirtualDeviceName ?? m_selectedCalculatorName),
                    Description = string.Format("Average {0}", inputMeasurement.Description),
                    Internal = true,
                    Subscribed = false,
                    Enabled = true
                };

            Measurement.Save(null, outputMeasurement);
        }

        // Adds a new mapping between the input and output measurements to the connection string.
        private void AddNewMapping()
        {
            Adapter calculator;
            Dictionary<string, string> settings;
            string inputMeasurementKeys;
            string outputMeasurements;

            calculator = m_dataModel.Adapters
                .Single(adapter => adapter.AdapterName == m_selectedCalculatorName);

            settings = calculator.ConnectionString.ToNonNullString().ParseKeyValuePairs();

            if (!settings.TryGetValue("inputMeasurementKeys", out inputMeasurementKeys))
                inputMeasurementKeys = string.Empty;

            if (!settings.TryGetValue("outputMeasurements", out outputMeasurements))
                outputMeasurements = string.Empty;

            inputMeasurementKeys += ";" + m_selectedFrequencyMeasurement.Key;
            outputMeasurements += ";" + GetNewMeasurementKey();

            settings["inputMeasurementKeys"] = inputMeasurementKeys;
            settings["outputMeasurements"] = outputMeasurements;

            calculator.ConnectionString = settings.JoinKeyValuePairs();
            Adapter.Save(null, calculator);
        }

        // Gets the measurement key of the newly created measurement.
        private string GetNewMeasurementKey()
        {
            DataModel tempDataModel = new DataModel();

            IEnumerable<string> oldKeys = m_dataModel.Measurements
                .Select(measurement => measurement.ID);

            return tempDataModel.Measurements
                .Select(measurement => measurement.ID)
                .Except(oldKeys)
                .First();
        }

        #endregion

        #region [ User removes a mapping ]

        // Removes the selected item.
        private void RemoveSelected()
        {
            IOMapping selected = m_selectedIOMapping;

            if (string.IsNullOrEmpty(m_selectedCalculatorName))
                return;

            if ((object)selected == null)
                return;

            RemoveMapping();
            DeleteOutputMeasurement(m_selectedIOMapping.OutputKey);
            Load();
        }

        // Removes the selected mapping from the connection string.
        private void RemoveMapping()
        {
            Adapter calculator;
            Dictionary<string, string> settings;
            string inputMeasurementKeys;
            string outputMeasurements;

            calculator = m_dataModel.Adapters
                .Single(adapter => adapter.AdapterName == m_selectedCalculatorName);

            settings = calculator.ConnectionString.ToNonNullString().ParseKeyValuePairs();

            if (!settings.TryGetValue("inputMeasurementKeys", out inputMeasurementKeys))
                inputMeasurementKeys = string.Empty;

            if (!settings.TryGetValue("outputMeasurements", out outputMeasurements))
                outputMeasurements = string.Empty;

            settings["inputMeasurementKeys"] = inputMeasurementKeys.Split(';')
                .Where(key => key != m_selectedIOMapping.InputKey)
                .Aggregate((keys, nextKey) => string.Format("{0};{1}", keys, nextKey));

            settings["outputMeasurements"] = outputMeasurements.Split(';')
                .Where(def => def.Split(',')[0] != m_selectedIOMapping.OutputKey)
                .Aggregate((defs, nextDef) => string.Format("{0};{1}", defs, nextDef));

            calculator.ConnectionString = settings.JoinKeyValuePairs();
            Adapter.Save(null, calculator);
        }

        // Deletes the output measurement definition for the mapping that was removed.
        private void DeleteOutputMeasurement(string outKey)
        {
            Measurement measurement = m_dataModel.Measurements
                .SingleOrDefault(m => m.ID == outKey);

            if ((object)measurement != null)
            {
                Measurement.Delete(null, measurement.SignalID);
            }
        }

        #endregion

        #region [ Operations to populate view-model properties ]

        // Selects the calculator name that was defined in isolated storage.
        private void SelectCalculatorName()
        {
            const string isolatedStorageKey = "AverageFrequency.CalculatorName";
            string isolatedName = IsolatedStorageManager.ReadFromIsolatedStorage(isolatedStorageKey).ToNonNullString();
            SelectedCalculatorName = m_calculatorNames.Contains(isolatedName) ? isolatedName : m_calculatorNames.FirstOrDefault();
        }

        // Selects the virtual device name that was defined in isolated storage.
        private void SelectVirtualDeviceName()
        {
            if (!string.IsNullOrWhiteSpace(m_selectedCalculatorName))
            {
                string isolatedStorageKey = string.Format("AverageFrequency.{0}.VirtualDeviceName", m_selectedCalculatorName);
                string isolatedName = IsolatedStorageManager.ReadFromIsolatedStorage(isolatedStorageKey).ToNonNullString();
                SelectedVirtualDeviceName = m_virtualDeviceNames.Contains(isolatedName) ? isolatedName : m_virtualDeviceNames.FirstOrDefault();
            }
        }

        // Updates the I/O mappings when changes are made to selections.
        private void UpdateIOMappings()
        {
            Dictionary<string, string> settings;
            string setting;

            IEnumerable<string> inputKeys;
            IEnumerable<string> outputKeys;
            IEnumerable<IOMapping> ioMappings;

            if ((object)m_selectedCalculatorName == null)
            {
                IOMappings = null;
                return;
            }

            settings = m_dataModel.Adapters
                .Single(adapter => adapter.AdapterName == m_selectedCalculatorName)
                .ConnectionString.ToNonNullString().ParseKeyValuePairs();

            if (settings.TryGetValue("inputMeasurementKeys", out setting))
            {
                inputKeys = setting.Split(';').Where(key => !string.IsNullOrWhiteSpace(key));
            }
            else
            {
                inputKeys = new List<string>();
            }

            if (settings.TryGetValue("outputMeasurements", out setting))
            {
                outputKeys = setting.Split(';').Select(key => key.Split(',')[0])
                    .Where(key => !string.IsNullOrWhiteSpace(key));
            }
            else
            {
                outputKeys = new List<string>();
            }

            ioMappings = inputKeys.Zip(outputKeys, MapKeys)
                .Where(mapping => (object)mapping.SignalReference != null);

            IOMappings = new ObservableCollection<IOMapping>(ioMappings);
        }

        // Updates the frequency measurements when another virtual device is selected.
        private void UpdateFrequencyMeasurements()
        {
            IEnumerable<FrequencyMeasurement> frequencyMeasurements;

            frequencyMeasurements = m_dataModel.Measurements
                .Select(measurement => new FrequencyMeasurement
                    {
                        SignalReference = measurement.SignalReference,
                        Key = measurement.ID
                    });

            if ((object)m_ioMappings != null)
            {
                frequencyMeasurements = frequencyMeasurements
                    .Where(freq => m_ioMappings.All(mapping => freq.Key != mapping.InputKey && freq.Key != mapping.OutputKey));
            }

            FrequencyMeasurements = new ObservableCollection<FrequencyMeasurement>(frequencyMeasurements);
        }

        #endregion

        #region [ Logic for HandlePropertyChangedOnce ]

        // Creates an IOMapping object between the given input and output measurement keys.
        private IOMapping MapKeys(string inKey, string outKey)
        {
            Measurement freq = m_dataModel.Measurements.SingleOrDefault(m => m.ID == inKey);
            string signalReference = ((object)freq != null) ? freq.SignalReference : null;

            return new IOMapping
                {
                    SignalReference = signalReference,
                    InputKey = inKey,
                    OutputKey = outKey
                };
        }

        // Confirms that the user wishes to modify the parent device of the
        // output measurements defined for the currently selected calculator
        // when the user changes their virtual device selection.
        private bool ConfirmVirtualDeviceSelection()
        {
            const string caption = "Change virtual device";
            const string text = "Output measurements will be updated. Continue?";
            MessageBoxResult result = MessageBox.Show(text, caption, MessageBoxButton.YesNo);

            return result == MessageBoxResult.Yes;
        }

        // Updates the parent device of the output measurements
        // to the currently selected virtual device.
        private void UpdateVirtualDevice()
        {
            Device virtualDevice = m_dataModel.Devices.Single(device => device.Acronym == m_selectedVirtualDeviceName);

            IEnumerable<Measurement> outputMeasurements = m_dataModel.Measurements
                .Where(measurement => m_ioMappings.Any(mapping => measurement.ID == mapping.OutputKey));

            using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
            {
                foreach (Measurement outputMeasurement in outputMeasurements)
                {
                    outputMeasurement.DeviceID = virtualDevice.ID;
                    Measurement.Save(database, outputMeasurement);
                }
            }
        }

        #endregion

        #region [ Event triggers ]

        // Triggers the PropertyChanged event.
        private void OnPropertyChanged(string propertyName)
        {
            if ((object)PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }
}
