//******************************************************************************************************
//  OutputStreamDevices.cs - Gbtc
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
//  08/03/2011 - Aniket Salver
//       Generated original version of source code.
//  08/26/2011 - Aniket Salver
//       Added few Properties which help in binding the objects
//  09/16/2011 - Mehulbhai P Thakkar
//       Modified Load() method to display binding properly.
//       Added commands to go to other screens.
//  09/14/2012 - Aniket Salver 
//          Added paging and sorting technique. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="OutputStreamDevice"/> collection and selected OutputStreamDevice for UI.
    /// </summary>
    internal class OutputStreamDevices : PagedViewModelBase<OutputStreamDevice, int>
    {

        #region [ Members ]

        // Fields

        private int m_outputStreamID;
        private readonly Dictionary<string, string> m_phasorDataformatLookupList;
        private readonly Dictionary<string, string> m_frequencyDataformatLookupList;
        private readonly Dictionary<string, string> m_analogDataformatLookupList;
        private readonly Dictionary<string, string> m_coordinateDataformatLookupList;
        private RelayCommand m_phasorCommand;
        private RelayCommand m_deviceWizardCommand;
        private RelayCommand m_analogCommand;
        private RelayCommand m_digitalCommand;
        private readonly bool m_mirrorMode;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets Output Stream ID.
        /// </summary>
        public int OutputStreamID
        {
            get
            {
                return m_outputStreamID;
            }
            set
            {
                m_outputStreamID = value;
                OnPropertyChanged("OutputStreamID");
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> PhasorDataformat collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> PhasorDataFormatLookupList
        {
            get
            {
                return m_phasorDataformatLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> FrequencyDataformat collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> FrequencyDataFormatLookupList
        {
            get
            {
                return m_frequencyDataformatLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> AnalogDataformat collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> AnalogDataFormatLookupList
        {
            get
            {
                return m_analogDataformatLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> CoordinateDataformat collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> CoordinateFormatLookupList
        {
            get
            {
                return m_coordinateDataformatLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> object to go to Phasors screen.
        /// </summary>
        public ICommand PhasorCommand
        {
            get
            {
                if (m_phasorCommand == null)
                    m_phasorCommand = new RelayCommand(GoToPhasors);

                return m_phasorCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> object to go to Device Wizard screen.
        /// </summary>
        public ICommand DeviceWizardCommand
        {
            get
            {
                if (m_deviceWizardCommand == null)
                    m_deviceWizardCommand = new RelayCommand(GoToDeviceWizard, () => CanGoToDeviceWizard);

                return m_deviceWizardCommand;
            }
        }

        /// <summary>
        /// Gets a boolean flag indicating if Phasors and Measurements button are visible or not.
        /// </summary>
        public bool CanGoToDeviceWizard
        {
            get
            {
                return m_outputStreamID > 0;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> object to go to Analogs screen.
        /// </summary>
        public ICommand AnalogCommand
        {
            get
            {
                if (m_analogCommand == null)
                    m_analogCommand = new RelayCommand(GoToAnalogs);

                return m_analogCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> object to go to Digitals screen.
        /// </summary>
        public ICommand DigitalCommand
        {
            get
            {
                if (m_digitalCommand == null)
                    m_digitalCommand = new RelayCommand(GoToDigitals);

                return m_digitalCommand;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        public override bool CanSave
        {
            get
            {
                return (base.CanSave && !Convert.ToBoolean(IsolatedStorageManager.ReadFromIsolatedStorage("MirrorMode").ToString()));
            }
        }

        /// <summary>
        /// Gets a boolean flag indicating if this screen is loaded for IEEE C37.118 mirroring.
        /// </summary>
        public bool MirrorMode
        {
            get
            {
                return m_mirrorMode;
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="OutputStreamDevices"/> class.
        /// </summary>
        /// <param name="outputStreamID">ID of the output stream to filter data.</param>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public OutputStreamDevices(int outputStreamID, int itemsPerPage, bool autoSave = true)
            : base(0, autoSave)
        {
            ItemsPerPage = itemsPerPage;
            OutputStreamID = outputStreamID;
            Load();

            m_phasorDataformatLookupList = new Dictionary<string, string>();
            m_phasorDataformatLookupList.Add("", "Select Phasor Data Format");
            m_phasorDataformatLookupList.Add("FloatingPoint", "FloatingPoint");
            m_phasorDataformatLookupList.Add("FixedInteger", "FixedInteger");

            m_frequencyDataformatLookupList = new Dictionary<string, string>();
            m_frequencyDataformatLookupList.Add("", "Select Frequency Data Format");
            m_frequencyDataformatLookupList.Add("FloatingPoint", "FloatingPoint");
            m_frequencyDataformatLookupList.Add("FixedInteger", "FixedInteger");

            m_analogDataformatLookupList = new Dictionary<string, string>();
            m_analogDataformatLookupList.Add("", "Select Frequency Data Format");
            m_analogDataformatLookupList.Add("FloatingPoint", "FloatingPoint");
            m_analogDataformatLookupList.Add("FixedInteger", "FixedInteger");

            m_coordinateDataformatLookupList = new Dictionary<string, string>();
            m_coordinateDataformatLookupList.Add("", "Select Coordinate Format");
            m_coordinateDataformatLookupList.Add("Polar", "Polar");
            m_coordinateDataformatLookupList.Add("Rectangular", "Rectangular");

            bool.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("MirrorMode").ToString(), out m_mirrorMode);
        }


        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads output stream devices.
        /// </summary>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<int> pageKeys;

            try
            {
                if (OnBeforeLoadCanceled())
                    throw new OperationCanceledException("Load was canceled.");

                if ((object)ItemsKeys == null)
                {
                    ItemsKeys = OutputStreamDevice.LoadKeys(null, OutputStreamID, SortMember, SortDirection);

                    if ((object)SortSelector != null)
                    {
                        if (SortDirection == "ASC")
                            ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
                        else
                            ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }
                }

                pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
                ItemsSource = OutputStreamDevice.Load(null, pageKeys);
                CurrentItem.AdapterID = m_outputStreamID;
                OnLoaded();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="OutputStreamDevice"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.AdapterID = m_outputStreamID;
        }

        /// <summary>
        /// Deletes <see cref="OutputStreamDevice"/>.
        /// </summary>
        public override void Delete()
        {
            //base.Delete();
            if (CanDelete && Confirm("Are you sure you want to delete \'" + GetCurrentItemName() + "\'?", "Delete " + DataModelName))
            {
                try
                {
                    if (OnBeforeDeleteCanceled())
                        throw new OperationCanceledException("Delete was canceled.");

                    int currentItemKey = GetCurrentItemKey();
                    string result = OutputStreamDevice.Delete(null, m_outputStreamID, CurrentItem.Acronym);
                    ItemsKeys.Remove(currentItemKey);

                    OnDeleted();

                    Load();

                    Popup(result, "Delete " + DataModelName, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Delete " + DataModelName + " Exception:", MessageBoxImage.Error);
                        CommonFunctions.LogException(null, "Delete " + DataModelName, ex.InnerException);
                    }
                    else
                    {
                        Popup(ex.Message, "Delete " + DataModelName + " Exception:", MessageBoxImage.Error);
                        CommonFunctions.LogException(null, "Delete " + DataModelName, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.Name;
        }

        private void GoToPhasors()
        {
            if (!m_mirrorMode)
                CommonFunctions.LoadUserControl("Manage Phasors for " + CurrentItem.Acronym, typeof(OutputStreamDevicePhasorUserControl), CurrentItem.ID);
        }

        /// <summary>
        /// Handles <see cref="DeviceWizardCommand"/>.
        /// </summary>        
        private void GoToDeviceWizard()
        {
            if (!m_mirrorMode)
            {
                OutputStream outputStream = OutputStream.GetOutputStream(null, " WHERE ID = " + m_outputStreamID);
                CommonFunctions.LoadUserControl("Manage Devices for " + outputStream.Acronym, typeof(OutputStreamCurrentDeviceUserControl), m_outputStreamID);
            }
        }

        private void GoToAnalogs()
        {
            if (!m_mirrorMode)
                CommonFunctions.LoadUserControl("Manage Analogs for " + CurrentItem.Acronym, typeof(OutputStreamDeviceAnalogUserControl), CurrentItem.ID);
        }

        private void GoToDigitals()
        {
            if (!m_mirrorMode)
                CommonFunctions.LoadUserControl("Manage Digitals for " + CurrentItem.Acronym, typeof(OutputStreamDeviceDigitalUserControl), CurrentItem.ID);
        }

        #endregion
    }
}
