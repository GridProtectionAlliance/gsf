//******************************************************************************************************
//  InputWizardDevice.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  05/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using GSF.Diagnostics;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a record from IConfigurationFrame.
    /// </summary>
    public class InputWizardDevice : DataModelBase
    {
        #region [ Members ]

        // Fields
        private string m_acronym;
        private string m_name;
        private string m_configAcronym;
        private string m_configName;
        private decimal m_longitude;
        private decimal m_latitude;
        private int? m_vendorDeviceId;
        private int m_accessId = 1;
        private int m_parentAccessId;
        private bool m_include;
        private int m_digitalCount;
        private int m_analogCount;
        private bool m_addDigitals;
        private bool m_addAnalogs;
        private bool m_existing;
        private string m_statusColor;
        private ObservableCollection<InputWizardDevicePhasor> m_phasorList;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets existing device ID, if any.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets and provided unique ID for the device.
        /// </summary>
        public Guid? UniqueID { get; set; }

        /// <summary>
        /// Gets or sets acronym of the <see cref="InputWizardDevice"/>.
        /// </summary>
        [Required(ErrorMessage = "Device acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Device Acronym cannot exceed 200 characters.")]
        [AcronymValidation]
        public string Acronym
        {
            get => m_acronym;
            set
            {
                m_acronym = value.Replace("'", "").Replace(" ", "_").ToUpper();

                if (m_acronym.Length > 200)
                    m_acronym = m_acronym.Substring(0, 200);

                OnPropertyChanged(nameof(Acronym));
                Existing = Device.GetDevice(null, $"WHERE Acronym = '{m_acronym.ToUpper()}'") is not null;
            }
        }

        /// <summary>
        /// Gets or sets name of the <see cref="InputWizardDevice"/>.
        /// </summary>
        [StringLength(200, ErrorMessage = "Device Name cannot exceed 200 characters.")]
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value is null || value.Length <= 200 ? value : value.Substring(0, 200);
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets acronym from configuration frame.
        /// </summary>
        public string ConfigAcronym
        {
            get => m_configAcronym;
            set
            {
                m_configAcronym = value;
                OnPropertyChanged(nameof(ConfigAcronym));
            }
        }

        /// <summary>
        /// Gets or sets name from configuration frame.
        /// </summary>
        public string ConfigName
        {
            get => m_configName;
            set
            {
                m_configName = value;
                OnPropertyChanged(nameof(ConfigName));
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="InputWizardDevice"/> Longitude.
        /// </summary>
        public decimal Longitude
        {
            get => m_longitude;
            set
            {
                m_longitude = value;
                OnPropertyChanged(nameof(Longitude));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> Latitude.
        /// </summary>
        public decimal Latitude
        {
            get => m_latitude;
            set
            {
                m_latitude = value;
                OnPropertyChanged(nameof(Latitude));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> vendor device id.
        /// </summary>
        public int? VendorDeviceID
        {
            get => m_vendorDeviceId;
            set
            {
                m_vendorDeviceId = value;
                OnPropertyChanged(nameof(VendorDeviceID));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> access id.
        /// </summary>
        public int AccessID
        {
            get => m_accessId;
            set
            {
                m_accessId = value;
                OnPropertyChanged(nameof(AccessID));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> parent access id.
        /// </summary>
        public int ParentAccessID
        {
            get => m_parentAccessId;
            set
            {
                m_parentAccessId = value;
                OnPropertyChanged(nameof(ParentAccessID));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> include flag.
        /// </summary>
        public bool Include
        {
            get => m_include;
            set
            {
                m_include = value;
                OnPropertyChanged(nameof(Include));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> digital count.
        /// </summary>
        public int DigitalCount
        {
            get => m_digitalCount;
            set
            {
                m_digitalCount = value;
                OnPropertyChanged(nameof(DigitalCount));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> analog count.
        /// </summary>
        public int AnalogCount
        {
            get => m_analogCount;
            set
            {
                m_analogCount = value;
                OnPropertyChanged(nameof(AnalogCount));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> add digitals flag.
        /// </summary>
        public bool AddDigitals
        {
            get => m_addDigitals;
            set
            {
                m_addDigitals = value;
                OnPropertyChanged(nameof(AddDigitals));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> add analog flag.
        /// </summary>
        public bool AddAnalogs
        {
            get => m_addAnalogs;
            set
            {
                m_addAnalogs = value;
                OnPropertyChanged(nameof(AddAnalogs));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> existing flag.
        /// </summary>
        public bool Existing
        {
            get => m_existing;
            set
            {
                m_existing = value;
                OnPropertyChanged(nameof(Existing));
                StatusColor = m_existing ? "green" : "transparent";
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> status color based on existing flag.
        /// </summary>
        public string StatusColor
        {
            get => m_statusColor;
            set
            {
                m_statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> phasor list.
        /// </summary>
        public ObservableCollection<InputWizardDevicePhasor> PhasorList
        {
            get => m_phasorList;
            set
            {
                m_phasorList = value;
                OnPropertyChanged(nameof(PhasorList));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> DigitalLabels.
        /// </summary>
        public List<string> DigitalLabels { get; set; }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> AnalogLabels.
        /// </summary>
        public List<string> AnalogLabels { get; set; }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> analog scalars, i.e., adders and multipliers.
        /// </summary>
        public Tuple<float, float>[] AnalogScalars { get; set; }

        /// <summary>
        /// Gets a tool tip preview of <see cref="DigitalLabels"/>.
        /// </summary>
        public string DigitalLabelsPreview
        {
            get
            {
                try
                {
                    string[] labels = DigitalLabels?.ToArray();

                    if (labels is null || labels.Length == 0)
                        return string.Empty;

                    for (int i = 0; i < labels.Length; i++)
                    {
                        string label = labels[i];

                        // Check for a config frame 3 style digital label
                        string[] cfg3BitLabels = label.Split('|');

                        if (cfg3BitLabels.Length == 16)
                        {
                            labels[i] = $"Digital {i}: [{string.Join(", ", cfg3BitLabels.Select((bitLabel, bitIndex) => $"{bitIndex}: {bitLabel.GetValidLabel()}"))}]";
                        }
                        else
                        {
                            List<string> bitLabels = new();

                            for (int j = 0; j < label.Length; j += 16)
                            {
                                int bitIndex = j / 16;

                                if (bitIndex > 15)
                                    break;

                                bitLabels.Add($"{bitIndex}: " + (label.Length - j < 16 || bitIndex == 15 ?
                                    $"{label.Substring(j).GetValidLabel().Trim()}" :
                                    $"{label.Substring(j, 16).GetValidLabel().Trim()}"));
                            }

                            labels[i] = $"Digital {i}: [{string.Join(", ", bitLabels)}]";
                        }
                    }

                    return string.Join(Environment.NewLine, labels);
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets a tool tip preview of <see cref="AnalogLabels"/>.
        /// </summary>
        public string AnalogLabelsPreview => AnalogLabels is not null ? string.Join(Environment.NewLine, AnalogLabels.Select((label, index) => $"Analog {index}: {label}")) : string.Empty;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Retrieves <see cref="ObservableCollection{T}"/> type collection of <see cref="InputWizardDevice"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="InputWizardDevice"/>.</returns>
        public static ObservableCollection<InputWizardDevice> Load(AdoDataConnection database) => 
            new();

        /// <summary>
        /// Saves <see cref="InputWizardDevice"/> information in to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="inputWizardDevice">Information about <see cref="InputWizardDevice"/>.</param>
        /// <returns>string, indicating success for UI display.</returns>
        public static string Save(AdoDataConnection database, InputWizardDevice inputWizardDevice) => 
            string.Empty;

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="InputWizardDevice"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false) => 
            new();

        /// <summary>
        /// Deletes specified record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int deviceID) => 
            string.Empty;

        #endregion
    }

    /// <summary>
    /// Represents phasor information for <see cref="InputWizardDevice"/> from IConfigurationFrame.
    /// </summary>
    public class InputWizardDevicePhasor : DataModelBase
    {
        #region [ Members ]

        // Fields
        private string m_label;
        private string m_type;
        private string m_configLabel;
        private string m_configType;
        private string m_phase;
        private string m_baseKVInput;
        //private string m_destinationLabel;
        private bool m_include;

        internal string DatabaseLabel;
        internal string ConfigFrameLabel;
        internal string DatabaseType;
        internal string ConfigFrameType;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="InputWizardDevicePhasor"/> Label.
        /// </summary>
        [Required(ErrorMessage = "Phasor label is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Phasor label must not exceed 200 characters.")]
        public string Label
        {
            get => m_label;
            set
            {
                m_label = value is null || value.Length <= 200 ? value : value.Substring(0, 200);
                OnPropertyChanged("Label");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="InputWizardDevicePhasor"/> Type.
        /// </summary>
        [DefaultValue("V")]
        [StringLength(1, ErrorMessage = "Phasor type must be 1 character.")]
        public string Type
        {
            get => m_type;
            set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }
        
        /// <summary>
        /// Gets or sets label from configuration frame.
        /// </summary>
        public string ConfigLabel
        {
            get => m_configLabel;
            set
            {
                m_configLabel = value;
                OnPropertyChanged(nameof(ConfigLabel));
            }
        }

        /// <summary>
        /// Gets or sets type from configuration frame.
        /// </summary>
        public string ConfigType
        {
            get => m_configType;
            set
            {
                m_configType = value;
                OnPropertyChanged(nameof(ConfigType));
            }
        }

        /// <summary>
        /// Gets or sets the Phase of the current <see cref="InputWizardDevicePhasor"/>.
        /// </summary>
        [DefaultValue("+")]
        [StringLength(1, ErrorMessage = "Phasor phase must be 1 character.")]
        public string Phase
        {
            get => m_phase;
            set
            {
                m_phase = value;
                OnPropertyChanged("Phase");
            }
        }

        /// <summary>
        /// Gets base kV integer value, i.e., nominal voltage level of line/bus associated with phasor.
        /// </summary>
        public int BaseKV { get; private set; }

        /// <summary>
        /// Gets or sets basekV string based binding value.
        /// </summary>
        [DefaultValue("0")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Base kV must be an integer")]
        public string BaseKVInput
        {
            get => m_baseKVInput;
            set
            {
                BaseKV = int.TryParse(value, out int baseKV) ? baseKV : 0;
                m_baseKVInput = value;
                OnPropertyChanged("BaseKVInput");
            }
        }

        //public string DestinationLabel
        //{
        //    get
        //    {
        //        return m_destinationLabel;
        //    }
        //    set
        //    {
        //        m_destinationLabel = value;
        //        OnPropertyChanged("DestinationLabel");
        //    }
        //}

        /// <summary>
        /// Gets or sets boolean flag indicating if <see cref="InputWizardDevicePhasor"/> needs to be saved into database.
        /// </summary>
        public bool Include
        {
            get => m_include;
            set
            {
                m_include = value;
                OnPropertyChanged("Include");

                if (!m_include && Phase.Length > 1)
                    Phase = Phase.Substring(0, 1);
            }
        }

        /// <summary>
        /// Gets or sets any multiplier to be applied to the phasor magnitudes.
        /// </summary>
        public float MagnitudeMultiplier { get; set; } = 1.0F;

        /// <summary>
        /// Gets or sets and adder to be applied to the phasor angle, in radians.
        /// </summary>
        public float AngleAdder { get; set; }

        #endregion
    }
}
