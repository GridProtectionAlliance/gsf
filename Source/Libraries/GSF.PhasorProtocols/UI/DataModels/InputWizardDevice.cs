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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
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
        private decimal m_longitude;
        private decimal m_latitude;
        private int? m_vendorDeviceId;
        private int m_accessId;
        private int m_parentAccessId;
        private bool m_include;
        private int m_digitalCount;
        private int m_analogCount;
        private bool m_addDigitals;
        private bool m_addAnalogs;
        private bool m_existing;
        private string m_statusColor;
        private ObservableCollection<InputWizardDevicePhasor> m_phasorList;
        private List<string> m_digitalLabels;
        private List<string> m_analogLabels;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets acronym of the <see cref="InputWizardDevice"/>.
        /// </summary>
        [Required(ErrorMessage = "Device acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Device Acronym cannot exceed 200 characters.")]
        [AcronymValidation]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value.Replace("'", "").Replace(" ", "_").ToUpper();

                if (m_acronym.Length > 200)
                    m_acronym = m_acronym.Substring(0, 200);

                OnPropertyChanged(nameof(Acronym));
                Existing = (Device.GetDevice(null, "WHERE Acronym = '" + m_acronym.ToUpper() + "'") != null);
            }
        }

        /// <summary>
        /// Gets or sets name of the <see cref="InputWizardDevice"/>.
        /// </summary>
        [StringLength(200, ErrorMessage = "Device Name cannot exceed 200 characters.")]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if ((object)value != null && value.Length > 200)
                    m_name = value.Substring(0, 200);
                else
                    m_name = value;

                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="InputWizardDevice"/> Longitude.
        /// </summary>
        public decimal Longitude
        {
            get
            {
                return m_longitude;
            }
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
            get
            {
                return m_latitude;
            }
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
            get
            {
                return m_vendorDeviceId;
            }
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
            get
            {
                return m_accessId;
            }
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
            get
            {
                return m_parentAccessId;
            }
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
            get
            {
                return m_include;
            }
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
            get
            {
                return m_digitalCount;
            }
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
            get
            {
                return m_analogCount;
            }
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
            get
            {
                return m_addDigitals;
            }
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
            get
            {
                return m_addAnalogs;
            }
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
            get
            {
                return m_existing;
            }
            set
            {
                m_existing = value;
                OnPropertyChanged(nameof(Existing));
                if (m_existing)
                    StatusColor = "green";
                else
                    StatusColor = "transparent";
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> status color based on existing flag.
        /// </summary>
        public string StatusColor
        {
            get
            {
                return m_statusColor;
            }
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
            get
            {
                return m_phasorList;
            }
            set
            {
                m_phasorList = value;
                OnPropertyChanged(nameof(PhasorList));
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> DigitalLabels.
        /// </summary>
        public List<string> DigitalLabels
        {
            get
            {
                return m_digitalLabels;
            }
            set
            {
                m_digitalLabels = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="InputWizardDevice"/> AnalogLabels.
        /// </summary>
        public List<string> AnalogLabels
        {
            get
            {
                return m_analogLabels;
            }
            set
            {
                m_analogLabels = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Retrieves <see cref="ObservableCollection{T}"/> type collection of <see cref="InputWizardDevice"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="InputWizardDevice"/>.</returns>
        public static ObservableCollection<InputWizardDevice> Load(AdoDataConnection database)
        {
            return new ObservableCollection<InputWizardDevice>();
        }

        /// <summary>
        /// Saves <see cref="InputWizardDevice"/> information in to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="inputWizardDevice">Information about <see cref="InputWizardDevice"/>.</param>
        /// <returns>string, indicating success for UI display.</returns>
        public static string Save(AdoDataConnection database, InputWizardDevice inputWizardDevice)
        {
            return "";
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="InputWizardDevice"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            return new Dictionary<int, string>();
        }

        /// <summary>
        /// Deletes specified record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int deviceID)
        {
            return "";
        }

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
        private string m_phase;
        private string m_baseKVInput;
        //private string m_destinationLabel;
        private bool m_include;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="InputWizardDevicePhasor"/> Label.
        /// </summary>
        [Required(ErrorMessage = "Phasor label is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Phasor label must not exceed 200 characters.")]
        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                if ((object)value != null && value.Length > 200)
                    m_label = value.Substring(0, 200);
                else
                    m_label = value;

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
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Gets or sets the Phase of the current <see cref="InputWizardDevicePhasor"/>.
        /// </summary>
        [DefaultValue("+")]
        [StringLength(1, ErrorMessage = "Phasor phase must be 1 character.")]
        public string Phase
        {
            get
            {
                return m_phase;
            }
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
            get
            {
                return m_baseKVInput;
            }
            set
            {
                if (int.TryParse(value, out int baseKV))
                    BaseKV = baseKV;
                else
                    BaseKV = 0;

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
            get
            {
                return m_include;
            }
            set
            {
                m_include = value;
                OnPropertyChanged("Include");
            }
        }

        #endregion
    }
}
