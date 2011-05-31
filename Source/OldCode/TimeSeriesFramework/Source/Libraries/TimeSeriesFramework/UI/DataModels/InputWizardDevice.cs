//******************************************************************************************************
//  InputWizardDevice.cs - Gbtc
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
//  05/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
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
        private ObservableCollection<InputWizardDevicePhasor> m_phasorList;

        #endregion

        #region [ Properties ]

        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
                Existing = (Device.GetDevice(null, "WHERE Acronym = '" + m_acronym.ToUpper() + "'") != null);
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        public decimal Longitude
        {
            get
            {
                return m_longitude;
            }
            set
            {
                m_longitude = value;
                OnPropertyChanged("Longitude");
            }
        }

        public decimal Latitude
        {
            get
            {
                return m_latitude;
            }
            set
            {
                m_latitude = value;
                OnPropertyChanged("Latitude");
            }
        }

        public int? VendorDeviceID
        {
            get
            {
                return m_vendorDeviceId;
            }
            set
            {
                m_vendorDeviceId = value;
                OnPropertyChanged("VendorDeviceID");
            }
        }

        public int AccessID
        {
            get
            {
                return m_accessId;
            }
            set
            {
                m_accessId = value;
                OnPropertyChanged("AccessID");
            }
        }

        public int ParentAccessID
        {
            get
            {
                return m_parentAccessId;
            }
            set
            {
                m_parentAccessId = value;
                OnPropertyChanged("ParentAccessID");
            }
        }

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

        public int DigitalCount
        {
            get
            {
                return m_digitalCount;
            }
            set
            {
                m_digitalCount = value;
                OnPropertyChanged("DigitalCount");
            }
        }

        public int AnalogCount
        {
            get
            {
                return m_analogCount;
            }
            set
            {
                m_analogCount = value;
                OnPropertyChanged("AnalogCount");
            }
        }

        public bool AddDigitals
        {
            get
            {
                return m_addDigitals;
            }
            set
            {
                m_addDigitals = value;
                OnPropertyChanged("AddDigitals");
            }
        }

        public bool AddAnalogs
        {
            get
            {
                return m_addAnalogs;
            }
            set
            {
                m_addAnalogs = value;
                OnPropertyChanged("AddAnalogs");
            }
        }

        public bool Existing
        {
            get
            {
                return m_existing;
            }
            set
            {
                m_existing = value;
                OnPropertyChanged("Existing");
            }
        }

        public ObservableCollection<InputWizardDevicePhasor> PhasorList
        {
            get
            {
                return m_phasorList;
            }
            set
            {
                m_phasorList = value;
                OnPropertyChanged("PhasorList");
            }
        }

        #endregion

        #region [ Methods ]

        public static ObservableCollection<InputWizardDevice> Load(AdoDataConnection database)
        {
            return new ObservableCollection<InputWizardDevice>();
        }

        public static string Save(AdoDataConnection database, InputWizardDevice inputWizardDevice)
        {
            return "";
            //if (inputWizardDevice.Include)
            //{
            //    bool createdConnection = false;
            //    try
            //    {
            //        createdConnection = CreateConnection(ref database);
            //    }
            //    finally
            //    {
            //        if (createdConnection && database != null)
            //            database.Dispose();
            //    }
            //}
        }

        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            return new Dictionary<int, string>();
        }

        public static string Delete(AdoDataConnection database, int deviceID)
        {
            return "";
        }

        #endregion

    }

    public class InputWizardDevicePhasor : DataModelBase
    {
        #region [ Members ]

        // Fields
        private string m_label;
        private string m_type;
        private string m_phase;
        private string m_destinationLabel;
        private bool m_include;

        #endregion

        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                m_label = value;
                OnPropertyChanged("Label");
            }
        }

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

        public string DestinationLabel
        {
            get
            {
                return m_destinationLabel;
            }
            set
            {
                m_destinationLabel = value;
                OnPropertyChanged("DestinationLabel");
            }
        }

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
    }
}
