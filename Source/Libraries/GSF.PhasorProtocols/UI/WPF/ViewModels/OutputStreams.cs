//******************************************************************************************************
//  OutputStreams.cs - Gbtc
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
//  09/09/2011 - Mehulbhai Thakkar
//       Generated original version of source code.
//  09/14/2012 - Aniket Salver 
//          Added paging and sorting technique. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GSF.Data;
using GSF.Diagnostics;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.Modal;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.Threading;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;
using C37Concentrator = PhasorProtocolAdapters.IeeeC37_118.Concentrator;
using BPAConcentrator = PhasorProtocolAdapters.BpaPdcStream.Concentrator;
using IECConcentrator = PhasorProtocolAdapters.Iec61850_90_5.Concentrator;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    internal class OutputStreams : PagedViewModelBase<OutputStream, int>
    {
        #region [ Members ]
        
        private Dictionary<string, string> m_mirroringSourceLookupList;
        
        private RelayCommand m_initializeCommand;
        private RelayCommand m_copyCommand;
        private RelayCommand m_updateConfigurationCommand;
        private RelayCommand m_deviceCommand;
        private RelayCommand m_measurementCommand;
        private RelayCommand m_wizardCommand;
        private RelayCommand m_buildCommandChannelCommand;
        private RelayCommand m_buildDataChannelCommand;
        
        private LongSynchronizedOperation m_configFrameSizeCalculation;

        private string m_runtimeID;
        private bool m_mirrorMode;
        private string m_frameSizeLabel;
        private string m_frameSizeText;
        private Brush m_frameSizeColor;

        #endregion

        #region [ Constructors ]

        public OutputStreams(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_frameSizeLabel = "Config Frame Size";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="ICommand"/> object for update operation.
        /// </summary>
        public ICommand UpdateConfigurationCommand => 
            m_updateConfigurationCommand ?? (m_updateConfigurationCommand = new RelayCommand(UpdateConfiguration));

        /// <summary>
        /// Gets <see cref="ICommand"/> object for copy operation.
        /// </summary>
        public ICommand CopyCommand => 
            m_copyCommand ?? (m_copyCommand = new RelayCommand(MakeCopy));

        /// <summary>
        /// Gets <see cref="ICommand"/> object for Initialize operation.
        /// </summary>
        public ICommand InitializeCommand => 
            m_initializeCommand ?? (m_initializeCommand = new RelayCommand(InitializeOutputStream));

        /// <summary>
        /// Gets <see cref="ICommand"/> object to pop open connection string builder for command channel configuration.
        /// </summary>
        public ICommand BuildCommandChannelCommand => 
            m_buildCommandChannelCommand ?? (m_buildCommandChannelCommand = new RelayCommand(BuildCommandChannel, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> object to pop open connection string builder for data channel configuration.
        /// </summary>
        public ICommand BuildDataChannelCommand => 
            m_buildDataChannelCommand ?? (m_buildDataChannelCommand = new RelayCommand(BuildDataChannel, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to go to Devices configuration.
        /// </summary>
        public ICommand DeviceCommand => 
            m_deviceCommand ?? (m_deviceCommand = new RelayCommand(GoToDevices));

        /// <summary>
        /// Gets <see cref="ICommand"/> to go to Measurements configuration.
        /// </summary>
        public ICommand MeasurementCommand => 
            m_measurementCommand ?? (m_measurementCommand = new RelayCommand(GoToMeasurements));

        /// <summary>
        /// Gets <see cref="ICommand"/> to launch device wizard.
        /// </summary>
        public ICommand WizardCommand => m_wizardCommand ?? (m_wizardCommand = new RelayCommand(LaunchDeviceWizard));

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Node"/> defined in the database.
        /// </summary>
        public Dictionary<Guid, string> NodeLookupList { get; private set; }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="OutputStream"/> types.
        /// </summary>
        public Dictionary<OutputProtocol, string> TypeLookupList
        {
            get
            {
                // Dynamically load output type lookup list from static OutputProtocolNames map
                return OutputStream.OutputProtocolNames.ToDictionary
                (
                    item => item.Key,
                    item => item.Value
                );
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="OutputStream"/> down sampling methods.
        /// </summary>
        public Dictionary<string, string> DownSamplingMethodLookupList { get; private set; }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="OutputStream"/> data formats.
        /// </summary>
        public Dictionary<string, string> DataFormatLookupList { get; private set; }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="OutputStream"/> coordinate formats.
        /// </summary>
        public Dictionary<string, string> CoordinateFormatLookupList { get; private set; }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord => 
            CurrentItem.ID == 0;

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/> RuntimeID.
        /// </summary>
        public string RuntimeID
        {
            get => m_runtimeID;
            set
            {
                m_runtimeID = value;
                OnPropertyChanged("RuntimeID");
            }
        }

        /// <summary>
        /// Gets a boolean flag indicating if this screen is loaded for IEEE C37.118 mirroring.
        /// </summary>
        public bool MirrorMode => m_mirrorMode;

        /// <summary>
        /// Gets or sets list of devices to use for mirroring.
        /// </summary>
        public Dictionary<string, string> MirroringSourceLookupList
        {
            get => m_mirroringSourceLookupList;
            set
            {
                m_mirroringSourceLookupList = value;
                OnPropertyChanged("MirroringSourceLookupList");
            }
        }

        /// <summary>
        /// Gets or sets the label for frame size text warnings.
        /// </summary>
        public string FrameSizeLabel
        {
            get => m_frameSizeLabel;
            set
            {
                m_frameSizeLabel = value;
                OnPropertyChanged("FrameSizeLabel");
            }
        }

        /// <summary>
        /// Gets or sets the text to be displayed as the frame size.
        /// </summary>
        public string FrameSizeText
        {
            get => m_frameSizeText;
            set
            {
                m_frameSizeText = value;
                OnPropertyChanged("FrameSizeText");
            }
        }

        /// <summary>
        /// Gets or sets the color of the text to be displayed as the frame size.
        /// </summary>
        public Brush FrameSizeColor
        {
            get => m_frameSizeColor;
            set
            {
                m_frameSizeColor = value;
                OnPropertyChanged("FrameSizeColor");
            }
        }

        #endregion

        #region [ Methods ]

        public override int GetCurrentItemKey() => 
            CurrentItem.ID;

        public override string GetCurrentItemName() => 
            CurrentItem.Name;

        public override void Clear()
        {
            base.Clear();
            CurrentItem.NodeID = NodeLookupList.First().Key;
        }

        public override void Initialize()
        {
            m_configFrameSizeCalculation = new LongSynchronizedOperation(UpdateConfigFrameSize, UpdateConfigFrameSizeError)
            {
                IsBackground = true
            };

            base.Initialize();

            NodeLookupList = Node.GetLookupList(null);

            bool.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("MirrorMode").ToString(), out m_mirrorMode);

            if (m_mirrorMode)
                MirroringSourceLookupList = Device.GetDevicesForMirroringOutputStream(null);

            FrameSizeColor = new SolidColorBrush(Colors.Black);
            FrameSizeText = "Calculating...";

            DownSamplingMethodLookupList = new Dictionary<string, string> 
            {
                {"LastReceived", "LastReceived"}, 
                {"Closest", "Closest"}, 
                {"Filtered", "Filtered"}, 
                {"BestQuality", "BestQuality"}
            };

            DataFormatLookupList = new Dictionary<string, string> 
            {
                {"FloatingPoint", "FloatingPoint"}, 
                {"FixedInteger", "FixedInteger"}
            };

            CoordinateFormatLookupList = new Dictionary<string, string> 
            {
                {"Polar", "Polar"}, 
                {"Rectangular", "Rectangular"}
            };
        }

        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (OnBeforeLoadCanceled())
                    throw new OperationCanceledException("Load was canceled.");

                if (ItemsKeys is null)
                {
                    ItemsKeys = OutputStream.LoadKeys(null, false, SortMember, SortDirection);

                    if (!(SortSelector is null))
                    {
                        ItemsKeys = SortDirection == "ASC" ? 
                            ItemsKeys.OrderBy(SortSelector).ToList() : 
                            ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }
                }

                List<int> pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();

                ItemsSource = OutputStream.Load(null, pageKeys);
                OnLoaded();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    Popup(ex.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex);
                }
                else
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex.InnerException);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void InitializeOutputStream()
        {
            if (!CommonFunctions.CurrentPrincipal.IsInRole("Administrator, Editor"))
                return;

            if (Confirm("Do you want to send Initialize Command?", "Output Stream: " + CurrentItem.Acronym))
            {
                try
                {
                    string result = CommonFunctions.SendCommandToService("Initialize " + RuntimeID);
                    Popup(result, "", MessageBoxImage.Information);
                    CommonFunctions.SendCommandToService("Invoke 0 ReloadStatistics");
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(null, "WPF.SendInitialize", ex);
                    Popup("Failed to Send Initialize Command", ex.Message, MessageBoxImage.Error);
                }
            }
        }

        private void MakeCopy(object parameter)
        {
            try
            {
                if (!Confirm("Do you want to make a copy of " + CurrentItem.Acronym + " output stream?", "This will only copy the output stream configuration, not associated devices."))
                    return;

                OutputStream newOutputStream = new OutputStream
                {
                    ID = 0, // Set it to zero so it will be inserted instead of updated.
                    Enabled = false,
                    Acronym = CurrentItem.Acronym,
                    Name = "Copy of " + CurrentItem.Name,
                    AllowPreemptivePublishing = CurrentItem.AllowPreemptivePublishing,
                    AllowSortsByArrival = CurrentItem.AllowSortsByArrival,
                    AnalogScalingValue = CurrentItem.AnalogScalingValue,
                    AutoPublishConfigFrame = CurrentItem.AutoPublishConfigFrame,
                    AutoStartDataChannel = CurrentItem.AutoStartDataChannel,
                    CommandChannel = CurrentItem.CommandChannel,
                    ConnectionString = CurrentItem.ConnectionString,
                    CoordinateFormat = CurrentItem.CoordinateFormat,
                    CurrentScalingValue = CurrentItem.CurrentScalingValue,
                    DataChannel = CurrentItem.DataChannel,
                    DataFormat = CurrentItem.DataFormat,
                    DigitalMaskValue = CurrentItem.DigitalMaskValue,
                    DownSamplingMethod = CurrentItem.DownSamplingMethod,
                    FramesPerSecond = CurrentItem.FramesPerSecond,
                    IDCode = CurrentItem.IDCode,
                    IgnoreBadTimeStamps = CurrentItem.IgnoreBadTimeStamps,
                    LagTime = CurrentItem.LagTime,
                    LeadTime = CurrentItem.LeadTime,
                    LoadOrder = CurrentItem.LoadOrder,
                    NodeID = CurrentItem.NodeID,
                    NominalFrequency = CurrentItem.NominalFrequency,
                    PerformTimestampReasonabilityCheck = CurrentItem.PerformTimestampReasonabilityCheck,
                    TimeResolution = CurrentItem.TimeResolution,
                    Type = CurrentItem.Type,
                    UseLocalClockAsRealTime = CurrentItem.UseLocalClockAsRealTime,
                    VoltageScalingValue = CurrentItem.VoltageScalingValue
                };

                string originalAcronym = newOutputStream.Acronym;
                int i = 1;

                do
                {
                    newOutputStream.Acronym = $"{originalAcronym}{i++}";
                }
                while (!(OutputStream.GetOutputStream(null, " WHERE Acronym = '" + newOutputStream.Acronym + "'") is null));

                CurrentItem = newOutputStream;
            }
            catch (Exception ex)
            {
                Popup("Failed to copy output stream.", ex.Message, MessageBoxImage.Error);
            }
        }

        private void UpdateConfiguration()
        {
            if (!CommonFunctions.CurrentPrincipal.IsInRole("Administrator, Editor"))
                return;

            try
            {
                if (Confirm("Do you want to update configuration?", ""))
                {
                    string runtimeID = CommonFunctions.GetRuntimeID("OutputStream", CurrentItem.ID);
                    string result = CommonFunctions.SendCommandToService($"ReloadConfig -Invoke=\"Invoke {runtimeID} UpdateConfiguration\"");
                    Popup(result, "", MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Popup("Failed to UpdateConfiguration", ex.Message, MessageBoxImage.Error);
            }
        }

        private void UpdateConfigFrameSize()
        {
            const int MaxFrameSize = ushort.MaxValue;

            if (IsNewRecord)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FrameSizeColor = Brushes.Gray;
                    FrameSizeText = "You must save the new record to calculate frame size.";
                });

                return;
            }

            DataSet dataSource;

            using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
            {
                dataSource = new DataSet();

                DataTable table = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM RuntimeOutputStreamDevice");
                table.TableName = "OutputStreamDevices";
                dataSource.Tables.Add(table.Copy());

                // We just need the schema information from this table - not the actual data
                table = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM RuntimeOutputStreamMeasurement WHERE AdapterID = 0");
                table.TableName = "OutputStreamMeasurements";
                dataSource.Tables.Add(table.Copy());

                table = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM OutputStreamDevicePhasor");
                table.TableName = "OutputStreamDevicePhasors";
                dataSource.Tables.Add(table.Copy());

                table = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM OutputStreamDeviceAnalog");
                table.TableName = "OutputStreamDeviceAnalogs";
                dataSource.Tables.Add(table.Copy());

                table = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM OutputStreamDeviceDigital");
                table.TableName = "OutputStreamDeviceDigitals";
                dataSource.Tables.Add(table.Copy());
            }

            int frameSize = -1;
            int configSize = 0;
            int configFrames = 0;

            switch (CurrentItem.Type)
            {
                case OutputProtocol.IEEE_C37_118_2011:
                    // 2011 version includes a config frame fragmentation feature, so all practical frame
                    // size limitations are now bound to data frame instead of configuration frame
                    using (C37Concentrator concentrator = new C37Concentrator { TargetConfigurationType = IEEEC37_118.DraftRevision.Std2011 })
                    {
                        concentrator.ID = Convert.ToUInt32(RuntimeID);
                        concentrator.DataSource = dataSource;
                        concentrator.UpdateConfiguration();
                        frameSize = C37Concentrator.CreateDataFrame(DateTime.UtcNow.Ticks, concentrator.ConfigurationFrame as IEEEC37_118.ConfigurationFrame1).BinaryLength;

                        byte[][] images = concentrator.ConfigurationFrame3?.BinaryImageFrames.ToArray();
                        configSize = images?.Sum(image => image.Length) ?? concentrator.ConfigurationFrame.BinaryLength;
                        configFrames = images?.Length ?? 0;
                    }
                    break;
                case OutputProtocol.IEEE_C37_118_2005:
                    using (C37Concentrator concentrator = new C37Concentrator { TargetConfigurationType = IEEEC37_118.DraftRevision.Std2005 })
                    {
                        concentrator.ID = Convert.ToUInt32(RuntimeID);
                        concentrator.DataSource = dataSource;
                        concentrator.UpdateConfiguration();
                        frameSize = concentrator.ConfigurationFrame.BinaryLength;
                    }
                    break;
                case OutputProtocol.BPA_PDCSTREAM:
                    // BPA PDCstream config frames are based on an INI file, so all practical frame
                    // size limitations are bound to data frame instead of configuration frame
                    string iniFilePath = Path.GetTempFileName();
                    
                    using (BPAConcentrator concentrator = new BPAConcentrator { IniFileName = iniFilePath, FramesPerSecond = CurrentItem.FramesPerSecond })
                    {
                        concentrator.ID = Convert.ToUInt32(RuntimeID);
                        concentrator.DataSource = dataSource;
                        concentrator.UpdateConfiguration();
                        frameSize = BPAConcentrator.CreateDataFrame(DateTime.UtcNow.Ticks, concentrator.ConfigurationFrame as BPAPDCstream.ConfigurationFrame).BinaryLength;
                    }

                    try
                    {
                        File.Delete(iniFilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.SwallowException(ex);
                    }

                    break;
                case OutputProtocol.IEC_61850_90_5:
                    using (IECConcentrator concentrator = new IECConcentrator())
                    {
                        concentrator.ID = Convert.ToUInt32(RuntimeID);
                        concentrator.DataSource = dataSource;
                        concentrator.UpdateConfiguration();
                        frameSize = concentrator.ConfigurationFrame.BinaryLength;
                    }
                    break;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                bool dataFrameTarget = CurrentItem.Type == OutputProtocol.IEEE_C37_118_2011 || CurrentItem.Type == OutputProtocol.BPA_PDCSTREAM;

                FrameSizeColor = new SolidColorBrush(Colors.Black);
                FrameSizeText = $"{frameSize:N0} bytes ({frameSize / (double)MaxFrameSize:0.##%} of maximum)";
                FrameSizeLabel = (dataFrameTarget ? "Data" : "Config") + " Frame Size";

                if (frameSize > MaxFrameSize)
                {
                    string frameTypeSizeTarget = $"{(dataFrameTarget ? "data" : "config")} frame";
                    FrameSizeColor = new SolidColorBrush(Colors.Red);
                    FrameSizeText += $" - WARNING: Exceeds maximum {frameTypeSizeTarget} size for {CurrentItem.OutputProtocolName}. Remove devices.";
                }
                else if (dataFrameTarget)
                {

                    if (CurrentItem.Type == OutputProtocol.IEEE_C37_118_2011)
                        FrameSizeText += $" - Config Frame Size: {configSize:N0} bytes, spanning {configFrames:N0} frames";
                    else
                        FrameSizeText += $" - {CurrentItem.OutputProtocolName} config frame has no practical limit";
                }
            });
        }

        private void UpdateConfigFrameSizeError(Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FrameSizeColor = new SolidColorBrush(Colors.Gray);
                FrameSizeText = $"Error calculating frame size: {ex.Message}";
            });
        }

        private void BuildCommandChannel()
        {
            if (CurrentItem is null)
                return;

            ConnectionStringBuilder csb = new ConnectionStringBuilder(ConnectionStringBuilder.ConnectionType.CommandChannel);

            if (!string.IsNullOrEmpty(CurrentItem.CommandChannel))
                csb.ConnectionString = CurrentItem.CommandChannel;

            csb.Closed += delegate
            {
                if (csb.DialogResult.GetValueOrDefault())
                    CurrentItem.CommandChannel = csb.ConnectionString;
            };

            csb.Owner = Application.Current.MainWindow;
            csb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            csb.ShowDialog();
        }

        private void BuildDataChannel()
        {
            if (CurrentItem is null)
                return;

            ConnectionStringBuilder csb = new ConnectionStringBuilder(ConnectionStringBuilder.ConnectionType.DataChannel);

            if (!string.IsNullOrEmpty(CurrentItem.DataChannel))
                csb.ConnectionString = CurrentItem.DataChannel;

            csb.Closed += delegate
            {
                if (csb.DialogResult.GetValueOrDefault())
                    CurrentItem.DataChannel = csb.ConnectionString;
            };

            csb.Owner = Application.Current.MainWindow;
            csb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            csb.ShowDialog();
        }

        private void GoToDevices(object parameter) => 
            CommonFunctions.LoadUserControl("Manage Devices for " + CurrentItem.Acronym, typeof(OutputStreamDeviceUserControl), CurrentItem.ID, MirrorMode);

        private void LaunchDeviceWizard(object parameter)
        {
            if (!CommonFunctions.CurrentPrincipal.IsInRole("Administrator,Editor"))
                return;

            if (!MirrorMode)
                CommonFunctions.LoadUserControl("Current Devices for " + CurrentItem.Acronym, typeof(OutputStreamCurrentDeviceUserControl), CurrentItem.ID);
        }

        private void GoToMeasurements(object parameter) => 
            CommonFunctions.LoadUserControl("Manage Measurements for " + CurrentItem.Acronym, typeof(OutputStreamMeasurementUserControl), CurrentItem.ID);

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "CurrentItem")
            {
                ExecuteConfigFrameSizeCalculation();
                CurrentItem.UpdateConfigFrameSize = ExecuteConfigFrameSizeCalculation;
            }
        }

        private void ExecuteConfigFrameSizeCalculation()
        {
            try
            {
                bool dataFrameTarget = !(CurrentItem is null) && (CurrentItem.Type == OutputProtocol.IEEE_C37_118_2011 || CurrentItem.Type == OutputProtocol.BPA_PDCSTREAM);

                RuntimeID = CurrentItem is null ? string.Empty : CommonFunctions.GetRuntimeID("OutputStream", CurrentItem.ID);
                FrameSizeLabel = (dataFrameTarget ? "Data" : "Config") + " Frame Size";
                FrameSizeColor = new SolidColorBrush(Colors.Black);
                FrameSizeText = "Calculating...";

                m_configFrameSizeCalculation.RunOnceAsync();
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "ExecuteConfigFrameSizeCalculation " + DataModelName, ex);
            }
        }

        public override void Save()
        {
            if (!CanSave)
                return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (OnBeforeSaveCanceled())
                    throw new OperationCanceledException("Save was canceled.");

                bool isNewRecord = IsNewRecord;
                string result = OutputStream.Save(null, CurrentItem, m_mirrorMode);

                OnSaved();

                CurrentItemPropertyChanged = false;

                if (!DisplayStatusMessage(result))
                    Popup(result, "Save " + DataModelName, MessageBoxImage.Information);

                if (isNewRecord)
                {
                    ItemsKeys = null;
                    Load();
                }
                else
                {
                    FrameSizeColor = new SolidColorBrush(Colors.Black);
                    FrameSizeText = "Calculating...";
                    m_configFrameSizeCalculation.RunOnceAsync();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    Popup(ex.Message, "Save " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Save " + DataModelName, ex);
                }
                else
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Save " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Save " + DataModelName, ex.InnerException);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion
    }
}
