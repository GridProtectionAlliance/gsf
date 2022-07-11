//******************************************************************************************************
//  OutputStream.cs - Gbtc
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
//  07/26/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  09/08/2011 - Mehulbhai Thakkar
//       Modified code to use SQL queries directly instead from script file resource.
//       Added comments to all properties and static methods.
//  09/14/2012 - Aniket Salver 
//       Added paging and sorting technique. 
//  03/08/2017 - J. Ritchie Carroll
//       Modified UI code to auto-set assign default auto publish config frame based on selections.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using GSF.Diagnostics;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;
using C37Concentrator = PhasorProtocolAdapters.IeeeC37_118.Concentrator;

// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ConstantConditionalAccessQualifier
namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Supported Synchrophasor Output Protocols.
    /// </summary>
    public enum OutputProtocol
    {
        /// <summary>
        /// IEEE C37.118-2011 Output.
        /// </summary>
        IEEE_C37_118_2011 = 3,
        /// <summary>
        /// IEEE C37.118-2005 Output.
        /// </summary>
        IEEE_C37_118_2005 = 0,
        /// <summary>
        /// BPA PDCstream Output.
        /// </summary>
        BPA_PDCSTREAM = 1,
        /// <summary>
        /// IEC 61850-90-5 Output.
        /// </summary>
        IEC_61850_90_5 = 2
    }

    /// <summary>
    /// Defines a value converter for the <see cref="OutputProtocol"/> enumeration.
    /// </summary>
    public class OutputProtocolValueConverter : IValueConverter
    {
        /// <summary>Converts a value.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if (parameter is not CollectionViewSource { Source: Dictionary<OutputProtocol, string> typeList })
                return null;

            string name = value.ToString();

            if (Enum.TryParse(name, out OutputProtocol type) && typeList.TryGetValue(type, out string typeName))
                return new KeyValuePair<OutputProtocol, string>(type, typeName);

            return null;
        }

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is KeyValuePair<OutputProtocol, string> pair ? pair.Key : null;
        }

        /// <summary>
        /// Gets default instance of the <see cref="OutputProtocolValueConverter"/>.
        /// </summary>
        public static readonly OutputProtocolValueConverter Default = new();
    }

    /// <summary>
    /// Represents a record of <see cref="OutputStream"/> as defined in the database.
    /// </summary>
    public class OutputStream : DataModelBase
    {
        #region [ Members ]

        private Guid m_nodeID;
        private int m_ID;
        private string m_acronym;
        private string m_name;
        private OutputProtocol m_type = OutputProtocol.IEEE_C37_118_2005;
        private string m_connectionString;
        private int m_idCode;
        private string m_commandChannel;
        private string m_dataChannel;
        private bool m_autoPublishConfigFrame;
        private bool m_autoStartDataChannel;
        private int m_nominalFrequency;
        private int m_framesPerSecond;
        private double m_lagTime;
        private double m_leadTime;
        private bool m_useLocalClockAsRealTime;
        private bool m_allowSortsByArrival;
        private int m_loadOrder;
        private bool m_enabled;
        private bool m_ignoreBadTimeStamps;
        private int m_timeResolution;
        private bool m_allowPreemptivePublishing;
        private string m_downsamplingMethod;
        private string m_dataFormat;
        private string m_coordinateFormat;
        private int m_currentScalingValue;
        private int m_voltageScalingValue;
        private int m_analogScalingValue;
        private int m_digitalMaskValue;
        private bool m_performTimestampReasonabilityCheck;
        private bool m_roundToNearestTimestamp;
        private string m_mirroringSourceDevice = "";

        // ReSharper disable once MemberInitializerValueIgnored
        // Assign initial changed state to true before calling base class constructor call to prevent unintended updates during default value initialization
        private bool m_autoPublishConfigFrameChangedByUser = true; // Called 1st

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutputStream"/>.
        /// </summary>
        public OutputStream() : base(false) // Called 2nd
        {
            // Post base class constructor call, reset changed state value back to false
            m_autoPublishConfigFrameChangedByUser = false; // Called 3rd

            try
            {
                // Set default lead/lag time settings to those used by manager screens
                object lagTimeValue = IsolatedStorageManager.ReadFromIsolatedStorage("LagTime");

                if (lagTimeValue is not null && double.TryParse(lagTimeValue.ToString(), out double lagTime) && lagTime > 0.0D)
                    m_lagTime = lagTime;

                object leadTimeValue = IsolatedStorageManager.ReadFromIsolatedStorage("LeadTime");

                if (leadTimeValue is not null && double.TryParse(leadTimeValue.ToString(), out double leadTime) && leadTime > 0.0D)
                    m_leadTime = leadTime;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets function to call when config frame size calculation should be updated.
        /// </summary>
        public Action UpdateConfigFrameSize { get; set; }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s NodeID.
        /// </summary>
        public Guid NodeID
        {
            get => m_nodeID;
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied.
        public int ID
        {
            get => m_ID;
            set
            {
                m_ID = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s Acronym.
        /// </summary>
        [Required(ErrorMessage = "Output stream acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Output stream acronym cannot exceed 200 characters.")]
        [AcronymValidation]
        public string Acronym
        {
            get => m_acronym;
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s Name.
        /// </summary>
        [StringLength(200, ErrorMessage = "Output stream name cannot exceed 200 characters.")]
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s output protocol type.
        /// </summary>
        [Required(ErrorMessage = "Output stream type is a required, please provide a value.")]
        public OutputProtocol Type
        {
            get => m_type;
            set
            {
                const string ConfigTypeKey = nameof(C37Concentrator.TargetConfigurationType);

                m_type = value;

                Dictionary<string, string> settings;

                try
                {
                    settings = ConnectionString?.ParseKeyValuePairs() ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    settings = new Dictionary<string, string>();
                }

                switch (m_type)
                {
                    case OutputProtocol.IEEE_C37_118_2011:
                        settings[ConfigTypeKey] = DraftRevision.Std2011.ToString();
                        break;
                    case OutputProtocol.IEEE_C37_118_2005:
                        settings[ConfigTypeKey] = DraftRevision.Std2005.ToString();
                        break;
                    default:
                        settings.Remove(ConfigTypeKey);
                        break;
                }

                ConnectionString = settings.JoinKeyValuePairs();

                OnPropertyChanged("Type");
                UpdateConfigFrameSize?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s ConnectionString.
        /// </summary>
        [DefaultValue("addPhaseLabelSuffix=false; replaceWithSpaceChar=_")]
        public string ConnectionString
        {
            get => m_connectionString;
            set
            {
                Dictionary<string, string> settings;

                try
                {
                    settings = value.ParseKeyValuePairs();
                }
                catch
                {
                    settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                if (settings.TryGetValue(nameof(RoundToNearestTimestamp), out string setting))
                {
                    RoundToNearestTimestamp = setting.ParseBoolean();
                    settings.Remove(nameof(RoundToNearestTimestamp));
                    m_connectionString = settings.JoinKeyValuePairs();
                }
                else
                {
                    m_connectionString = value;
                }

                OnPropertyChanged("ConnectionString");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s IDCode.
        /// </summary>
        [Required(ErrorMessage = "Output stream IDCode is a required field, please provide a value.")]
        public int IDCode
        {
            get => m_idCode;
            set
            {
                m_idCode = value;
                OnPropertyChanged("IDCode");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s CommandChannel.
        /// </summary>
        public string CommandChannel
        {
            get => m_commandChannel;
            set
            {
                m_commandChannel = value;
                SetDefaultAutoPublishConfigFrameState();
                OnPropertyChanged("CommandChannel");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s DataChannel.
        /// </summary>
        public string DataChannel
        {
            get => m_dataChannel;
            set
            {
                m_dataChannel = value;
                SetDefaultAutoPublishConfigFrameState();
                OnPropertyChanged("DataChannel");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s AutoPublishConfigFrame flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream auto publish config frame is a required field, please provide a value.")]
        [DefaultValue(false)]
        public bool AutoPublishConfigFrame
        {
            get => m_autoPublishConfigFrame;
            set
            {
                m_autoPublishConfigFrame = value;
                m_autoPublishConfigFrameChangedByUser = true;
                OnPropertyChanged("AutoPublishConfigFrame");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s AutoStartDataChannel flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream auto start data channel is a required field, please provide a value.")]
        [DefaultValue(true)]
        public bool AutoStartDataChannel
        {
            get => m_autoStartDataChannel;
            set
            {
                m_autoStartDataChannel = value;
                OnPropertyChanged("AutoStartDataChannel");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s Nominal Frequency.
        /// </summary>
        [Required(ErrorMessage = "Output stream nominal frequency is a required field, please provide a value.")]
        [DefaultValue(60)]
        public int NominalFrequency
        {
            get => m_nominalFrequency;
            set
            {
                m_nominalFrequency = value;
                OnPropertyChanged("NominalFrequency");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s Frames Per Second.
        /// </summary>
        [Required(ErrorMessage = "Output stream frames per second is a required field, please provide a value.")]
        [DefaultValue(30)]
        public int FramesPerSecond
        {
            get => m_framesPerSecond;
            set
            {
                m_framesPerSecond = value;
                OnPropertyChanged("FramesPerSecond");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s LagTime.
        /// </summary>
        [Required(ErrorMessage = "Output stream lag time is a required field, please provide a value.")]
        [DefaultValue(3.0)]
        public double LagTime
        {
            get => m_lagTime;
            set
            {
                m_lagTime = value;
                OnPropertyChanged("LagTime");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s LeadTime.
        /// </summary>
        [Required(ErrorMessage = "Output stream lead time is a required field, please provide a value.")]
        [DefaultValue(1.0)]
        public double LeadTime
        {
            get => m_leadTime;
            set
            {
                m_leadTime = value;
                OnPropertyChanged("LeadTime");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s UseLocalClockAsRealTime flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream use local clock as real-time is a required field, please provide a value.")]
        [DefaultValue(false)]
        public bool UseLocalClockAsRealTime
        {
            get => m_useLocalClockAsRealTime;
            set
            {
                m_useLocalClockAsRealTime = value;
                OnPropertyChanged("UseLocalClockAsRealTime");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s AllowSortsByArrival flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream allow sorts by arrival is a required field, please provide a value.")]
        [DefaultValue(true)]
        public bool AllowSortsByArrival
        {
            get => m_allowSortsByArrival;
            set
            {
                m_allowSortsByArrival = value;
                OnPropertyChanged("AllowSortsByArrival");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "Output stream load order is a required field, please provide a value.")]
        [DefaultValue(0)]
        public int LoadOrder
        {
            get => m_loadOrder;
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s Enabled flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream enabled is a required field, please provide a value.")]
        [DefaultValue(false)]
        public bool Enabled
        {
            get => m_enabled;
            set
            {
                m_enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s IgnoreBadTimeStamps flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream ignore bad timestamps is a required field, please provide a value.")]
        [DefaultValue(false)]
        public bool IgnoreBadTimeStamps
        {
            get => m_ignoreBadTimeStamps;
            set
            {
                m_ignoreBadTimeStamps = value;
                OnPropertyChanged("IgnoreBadTimeStamps");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s TimeResolution.
        /// </summary>
        [Required(ErrorMessage = "Output stream time resolution is a required field, please provide a value.")]
        [DefaultValue(330000)]
        public int TimeResolution
        {
            get => m_timeResolution;
            set
            {
                m_timeResolution = value;
                OnPropertyChanged("TimeResolution");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s AllowPreemptivePublishing flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream allow preemptive publishing flag is a required field, please provide a value.")]
        [DefaultValue(true)]
        public bool AllowPreemptivePublishing
        {
            get => m_allowPreemptivePublishing;
            set
            {
                m_allowPreemptivePublishing = value;
                OnPropertyChanged("AllowPreemptivePublishing");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s DownSamplingMethod.
        /// </summary>
        [Required(ErrorMessage = "Output stream down sampling method is a required field, please provide a value.")]
        [DefaultValue("LastReceived")]
        public string DownSamplingMethod
        {
            get => m_downsamplingMethod;
            set
            {
                m_downsamplingMethod = value;
                OnPropertyChanged("DownSamplingMethod");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s DataFormat.
        /// </summary>
        [Required(ErrorMessage = "Output stream data format is a required field, please provide a value.")]
        [DefaultValue("FloatingPoint")]
        public string DataFormat
        {
            get => m_dataFormat;
            set
            {
                m_dataFormat = value;
                OnPropertyChanged("DataFormat");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s CoordinateFormat.
        /// </summary>
        [Required(ErrorMessage = "Output stream coordinate format is a required field, please provide a value.")]
        [DefaultValue("Polar")]
        public string CoordinateFormat
        {
            get => m_coordinateFormat;
            set
            {
                m_coordinateFormat = value;
                OnPropertyChanged("CoordinateFormat");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s CurrentScalingValue.
        /// </summary>
        [Required(ErrorMessage = "Output stream current scaling value is a required field, please provide a value.")]
        [DefaultValue(2423)]
        public int CurrentScalingValue
        {
            get => m_currentScalingValue;
            set
            {
                m_currentScalingValue = value;
                OnPropertyChanged("CurrentScalingValue");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s VoltageScalingValue.
        /// </summary>
        [Required(ErrorMessage = "Output stream voltage scaling value is a required field, please provide a value.")]
        [DefaultValue(2725785)]
        public int VoltageScalingValue
        {
            get => m_voltageScalingValue;
            set
            {
                m_voltageScalingValue = value;
                OnPropertyChanged("VoltageScalingValue");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s AnalogScalingValue.
        /// </summary>
        [Required(ErrorMessage = "Output stream analog scaling value is a required field, please provide a value.")]
        [DefaultValue(1373291)]
        public int AnalogScalingValue
        {
            get => m_analogScalingValue;
            set
            {
                m_analogScalingValue = value;
                OnPropertyChanged("AnalogScalingValue");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s DigitalMaskValue.
        /// </summary>
        [Required(ErrorMessage = "Output stream digital mask value is a required field, please provide a value.")]
        [DefaultValue(-65536)]
        public int DigitalMaskValue
        {
            get => m_digitalMaskValue;
            set
            {
                m_digitalMaskValue = value;
                OnPropertyChanged("DigitalMaskValue");
            }
        }

        /// <summary>
        /// Gets <see cref="OutputStream"/>'s Node name.
        /// </summary>        
        public string NodeName { get; private set; }

        /// <summary>
        /// Gets <see cref="OutputStream"/>'s TypeName.
        /// </summary>
        public string OutputProtocolName => OutputProtocolNames[Type];

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s PerformTimestampReasonabilityCheck flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream perform timestamp reasonability check is a required field, please provide a value.")]
        [DefaultValue(true)]
        public bool PerformTimestampReasonabilityCheck
        {
            get => m_performTimestampReasonabilityCheck;
            set
            {
                m_performTimestampReasonabilityCheck = value;
                OnPropertyChanged("PerformTimestampReasonabilityCheck");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/>'s PerformTimestampReasonabilityCheck flag.
        /// </summary>
        [Required(ErrorMessage = "Output stream perform timestamp reasonability check is a required field, please provide a value.")]
        [DefaultValue(false)]
        public bool RoundToNearestTimestamp
        {
            get => m_roundToNearestTimestamp;
            set
            {
                m_roundToNearestTimestamp = value;
                OnPropertyChanged("RoundToNearestTimestamp");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/> CreatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/> CreatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/> UpdatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Gets or sets <see cref="OutputStream"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets id of the device used for mirroring output stream.
        /// </summary>  
        public string MirroringSourceDevice
        {
            get => m_mirroringSourceDevice;
            set
            {
                if (m_mirroringSourceDevice == value)
                    return;

                if (MessageBox.Show("WARNING: This will replace all existing devices and measurements associated with this output stream. Do you want to continue?", "IEEE C37.118 Mirroring", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    m_mirroringSourceDevice = value;
                    OnPropertyChanged("MirroringSourceDevice");
                }

            }
        }

        #endregion

        #region [ Methods ]

        private void SetDefaultAutoPublishConfigFrameState()
        {
            if (m_autoPublishConfigFrameChangedByUser)
                return;

            m_autoPublishConfigFrame = string.IsNullOrWhiteSpace(m_commandChannel) && !string.IsNullOrWhiteSpace(m_dataChannel);
            OnPropertyChanged("AutoPublishConfigFrame");
        }

        #endregion

        #region [ Static ]

        // Static Properties

        /// <summary>
        /// Gets an output stream type lookup list, maps <see cref="OutputStream"/> enum value to its display name.
        /// </summary>
        public static ReadOnlyDictionary<OutputProtocol, string> OutputProtocolNames { get; } = new
        (
            new Dictionary<OutputProtocol, string>
            {
                { OutputProtocol.IEEE_C37_118_2011, "IEEE C37.118-2011" },
                { OutputProtocol.IEEE_C37_118_2005, "IEEE C37.118-2005" },
                { OutputProtocol.BPA_PDCSTREAM, "BPA PDCstream" },
                { OutputProtocol.IEC_61850_90_5, "IEC 61850-90-5" }
            }
        );

        // Static Methods

        /// <summary>
        /// Loads <see cref="OutputStream"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="enabledOnly">Boolean flag indicating if only enabled <see cref="OutputStream"/>s needed.</param>   
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="OutputStream"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, bool enabledOnly, string sortMember, string sortDirection)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> outputStreamList = new List<int>();
                DataTable outputStreamTable;
                string query;

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = $"ORDER BY {sortMember} {sortDirection}";

                if (enabledOnly)
                {
                    query = database.ParameterizedQueryString($"SELECT ID FROM OutputStreamDetail WHERE NodeID = {{0}} AND Enabled = {{1}} {sortClause}", "nodeID", "enabled");
                    outputStreamTable = database.Connection.RetrieveData(database.AdapterType, query, database.CurrentNodeID(), database.Bool(true));
                }
                else
                {
                    query = database.ParameterizedQueryString($"SELECT * FROM OutputStreamDetail WHERE NodeID = {{0}} {sortClause}", "nodeID");
                    outputStreamTable = database.Connection.RetrieveData(database.AdapterType, query, database.CurrentNodeID());
                }

                foreach (DataRow row in outputStreamTable.Rows)
                {
                    outputStreamList.Add(row.ConvertField<int>("ID"));
                }

                return outputStreamList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="OutputStream"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measurements to be loaded from the database</param>
        /// <returns>Collection of <see cref="OutputStream"/>.</returns>
        public static ObservableCollection<OutputStream> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<OutputStream> outputStreamList = new();

                if (keys is not null && keys.Count > 0)
                {
                    string commaSeparatedKeys = keys.Select(key => $"{key}").Aggregate((str1, str2) => $"{str1},{str2}");
                    string query = $"SELECT * FROM OutputStreamDetail WHERE ID IN ({commaSeparatedKeys})";
                    DataTable outputStreamTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);

                    outputStreamList = new ObservableCollection<OutputStream>(
                        from item in outputStreamTable.AsEnumerable()
                        let id = item.ConvertField<int>("ID")
                        let type = (OutputProtocol)item.ConvertField<int>("Type")
                        orderby keys.IndexOf(id)
                        select new OutputStream
                        {
                            NodeID = database.Guid(item, "NodeID"),
                            ID = id,
                            Acronym = item.Field<string>("Acronym"),
                            Name = item.Field<string>("Name"),
                            Type = type,
                            ConnectionString = item.Field<string>("ConnectionString"),
                            IDCode = item.ConvertField<int>("IDCode"),
                            CommandChannel = item.Field<string>("CommandChannel"),
                            DataChannel = item.Field<string>("DataChannel"),
                            AutoPublishConfigFrame = Convert.ToBoolean(item.Field<object>("AutoPublishConfigFrame")),
                            AutoStartDataChannel = Convert.ToBoolean(item.Field<object>("AutoStartDataChannel")),
                            NominalFrequency = item.ConvertField<int>("NominalFrequency"),
                            FramesPerSecond = item.ConvertNullableField<int>("FramesPerSecond") ?? 30,
                            LagTime = item.ConvertField<double>("LagTime"),
                            LeadTime = item.ConvertField<double>("LeadTime"),
                            UseLocalClockAsRealTime = Convert.ToBoolean(item.Field<object>("UseLocalClockAsRealTime")),
                            AllowSortsByArrival = Convert.ToBoolean(item.Field<object>("AllowSortsByArrival")),
                            LoadOrder = item.ConvertField<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(item.Field<object>("Enabled")),
                            NodeName = item.Field<string>("NodeName"),
                            IgnoreBadTimeStamps = Convert.ToBoolean(item.Field<object>("IgnoreBadTimeStamps")),
                            TimeResolution = item.ConvertField<int>("TimeResolution"),
                            AllowPreemptivePublishing = Convert.ToBoolean(item.Field<object>("AllowPreemptivePublishing")),
                            DownSamplingMethod = item.Field<string>("DownsamplingMethod"),
                            DataFormat = item.Field<string>("DataFormat"),
                            CoordinateFormat = item.Field<string>("CoordinateFormat"),
                            CurrentScalingValue = item.ConvertField<int>("CurrentScalingValue"),
                            VoltageScalingValue = item.ConvertField<int>("VoltageScalingValue"),
                            AnalogScalingValue = item.ConvertField<int>("AnalogScalingValue"),
                            DigitalMaskValue = item.ConvertField<int>("DigitalMaskValue"),
                            PerformTimestampReasonabilityCheck = Convert.ToBoolean(item.Field<object>("PerformTimeReasonabilityCheck")),
                            m_mirroringSourceDevice = GetMirroringSource(database, id),
                            m_autoPublishConfigFrameChangedByUser = false
                        });

                    return outputStreamList;

                }
                return outputStreamList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        private static string GetMirroringSource(AdoDataConnection database, int outputStreamID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> keys = OutputStreamDevice.LoadKeys(database, outputStreamID);

                // Get first output stream device.
                ObservableCollection<OutputStreamDevice> outputStreamDevices = OutputStreamDevice.Load(database, keys);

                if (outputStreamDevices.Count == 0)
                    return "";

                OutputStreamDevice outputStreamDevice = outputStreamDevices[0];

                // Get OriginalSource value for the above OutputStreamDevice from the input Device table.
                Device device = Device.GetDevice(database, $" WHERE Acronym LIKE '%{outputStreamDevice.Acronym}'");

                return device is null ? "" : device.OriginalSource;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="OutputStream"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStream">Information about <see cref="OutputStream"/>.</param>        
        /// <param name="mirrorMode">Boolean value to use mirror mode when saving output stream.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, OutputStream outputStream, bool mirrorMode)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string connectionString = outputStream.ConnectionString;

                if (outputStream.RoundToNearestTimestamp)
                    connectionString += "; RoundToNearestTimestamp=True";

                string query;

                if (outputStream.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO OutputStream (NodeID, Acronym, Name,  Type, ConnectionString, IDCode, CommandChannel, DataChannel, " +
                        "AutoPublishConfigFrame, AutoStartDataChannel, NominalFrequency, FramesPerSecond, LagTime, LeadTime, UseLocalClockAsRealTime, AllowSortsByArrival, " +
                        "LoadOrder, Enabled, IgnoreBadTimeStamps, TimeResolution, AllowPreemptivePublishing, DownSamplingMethod, DataFormat, CoordinateFormat, " +
                        "CurrentScalingValue, VoltageScalingValue, AnalogScalingValue, DigitalMaskValue, PerformTimeReasonabilityCheck, UpdatedBy, UpdatedOn, " +
                        "CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, " +
                        "{21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32})", "nodeID", "acronym", "name", "type", "connectionString", "idCode",
                        "commandChannel", "dataChannel", "autoPublishConfigFrame", "autoStartDataChannel", "nominalFrequency", "framesPerSecond", "lagTime", "leadTime",
                        "useLocalClockAsRealTime", "allowSortsByArrival", "loadOrder", "enabled", "ignoreBadTimeStamps", "timeResolution", "allowPreemptivePublishing",
                        "downSamplingMethod", "dataFormat", "coordinateFormat", "currentScalingValue", "voltageScalingValue", "analogScalingValue", "digitalMaskValue",
                        "performTimeReasonabilityCheck", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query,
                        database.CurrentNodeID(), outputStream.Acronym.Replace(" ", "").ToUpper(), outputStream.Name.ToNotNull(), (int)outputStream.Type, connectionString.ToNotNull(),
                        outputStream.IDCode, outputStream.CommandChannel.ToNotNull(), outputStream.DataChannel.ToNotNull(), database.Bool(outputStream.AutoPublishConfigFrame), database.Bool(outputStream.AutoStartDataChannel),
                        outputStream.NominalFrequency, outputStream.FramesPerSecond, outputStream.LagTime, outputStream.LeadTime, database.Bool(outputStream.UseLocalClockAsRealTime), database.Bool(outputStream.AllowSortsByArrival),
                        outputStream.LoadOrder, database.Bool(outputStream.Enabled), database.Bool(outputStream.IgnoreBadTimeStamps), outputStream.TimeResolution, database.Bool(outputStream.AllowPreemptivePublishing),
                        outputStream.DownSamplingMethod.ToNotNull(), outputStream.DataFormat.ToNotNull(), outputStream.CoordinateFormat.ToNotNull(), outputStream.CurrentScalingValue,
                        outputStream.VoltageScalingValue, outputStream.AnalogScalingValue, outputStream.DigitalMaskValue, database.Bool(outputStream.PerformTimestampReasonabilityCheck),
                        CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    OutputStream oldOutputStream = GetOutputStream(database, $" WHERE ID = {outputStream.ID}");

                    query = database.ParameterizedQueryString("UPDATE OutputStream SET NodeID = {0}, Acronym = {1}, Name = {2}, Type = {3}, ConnectionString = {4}, " +
                        "IDCode = {5}, CommandChannel = {6}, DataChannel = {7}, AutoPublishConfigFrame = {8}, AutoStartDataChannel = {9}, NominalFrequency = {10}, " +
                        "FramesPerSecond = {11}, LagTime = {12}, LeadTime = {13}, UseLocalClockAsRealTime = {14}, AllowSortsByArrival = {15}, LoadOrder = {16}, " +
                        "Enabled = {17}, IgnoreBadTimeStamps = {18}, TimeResolution = {19}, AllowPreemptivePublishing = {20}, DownSamplingMethod = {21}, " +
                        "DataFormat = {22}, CoordinateFormat = {23}, CurrentScalingValue = {24}, VoltageScalingValue = {25}, AnalogScalingValue = {26}, " +
                        "DigitalMaskValue = {27}, PerformTimeReasonabilityCheck = {28}, UpdatedBy = {29}, UpdatedOn = {30} WHERE ID = {31}", "nodeID", "acronym", "name",
                        "type", "connectionString", "idCode", "commandChannel", "dataChannel", "autoPublishConfigFrame", "autoStartDataChannel", "nominalFrequency",
                        "framesPerSecond", "lagTime", "leadTime", "useLocalClockAsRealTime", "allowSortsByArrival", "loadOrder", "enabled", "ignoreBadTimeStamps",
                        "timeResolution", "allowPreemptivePublishing", "downsamplingMethod", "dataFormat", "coordinateFormat", "currentScalingValue", "voltageScalingValue",
                        "analogScalingValue", "digitalMaskValue", "performTimeReasonabilityCheck", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout,
                        database.Guid(outputStream.NodeID), outputStream.Acronym.Replace(" ", "").ToUpper(), outputStream.Name.ToNotNull(), (int)outputStream.Type, connectionString.ToNotNull(),
                        outputStream.IDCode, outputStream.CommandChannel.ToNotNull(), outputStream.DataChannel.ToNotNull(), database.Bool(outputStream.AutoPublishConfigFrame), database.Bool(outputStream.AutoStartDataChannel),
                        outputStream.NominalFrequency, outputStream.FramesPerSecond, outputStream.LagTime, outputStream.LeadTime, database.Bool(outputStream.UseLocalClockAsRealTime),
                        database.Bool(outputStream.AllowSortsByArrival), outputStream.LoadOrder, database.Bool(outputStream.Enabled), database.Bool(outputStream.IgnoreBadTimeStamps), outputStream.TimeResolution,
                        database.Bool(outputStream.AllowPreemptivePublishing), outputStream.DownSamplingMethod.ToNotNull(), outputStream.DataFormat.ToNotNull(), outputStream.CoordinateFormat.ToNotNull(),
                        outputStream.CurrentScalingValue, outputStream.VoltageScalingValue, outputStream.AnalogScalingValue, outputStream.DigitalMaskValue, database.Bool(outputStream.PerformTimestampReasonabilityCheck),
                        CommonFunctions.CurrentUser, database.UtcNow, outputStream.ID);

                    if (oldOutputStream is not null && oldOutputStream.Acronym != outputStream.Acronym.Replace(" ", "").ToUpper())
                    {
                        ObservableCollection<Measurement> measurementList = Measurement.GetOutputStatisticMeasurements(database, oldOutputStream.Acronym);

                        foreach (Measurement measurement in measurementList)
                        {
                            measurement.SignalReference = measurement.SignalReference.Replace(oldOutputStream.Acronym, outputStream.Acronym.Replace(" ", "").ToUpper());
                            measurement.PointTag = measurement.PointTag.Replace(oldOutputStream.Acronym, outputStream.Acronym.Replace(" ", "").ToUpper());
                            measurement.Description = Regex.Replace(measurement.Description, oldOutputStream.Name, outputStream.Name ?? outputStream.Acronym.Replace(" ", "").ToUpper(), RegexOptions.IgnoreCase);
                            Measurement.Save(database, measurement);
                        }

                        SignalType qualityType = SignalType.Load(database).FirstOrDefault(type => type.Acronym == "QUAL");

                        if (qualityType is not null)
                        {
                            IList<int> keys = database.Connection.RetrieveData(database.AdapterType, $"SELECT ID FROM OutputStreamMeasurement WHERE AdapterID = {outputStream.ID}")
                                .Select().Select(row => row.ConvertField<int>("ID")).ToList();

                            foreach (OutputStreamMeasurement measurement in OutputStreamMeasurement.Load(database, keys))
                            {
                                if (Regex.IsMatch(measurement.SignalReference, $"{oldOutputStream.Acronym}-{qualityType.Suffix}"))
                                {
                                    measurement.SignalReference = measurement.SignalReference.Replace(oldOutputStream.Acronym, outputStream.Acronym.Replace(" ", "").ToUpper());
                                    OutputStreamMeasurement.Save(database, measurement);
                                }
                            }
                        }
                    }
                }

                if (mirrorMode)
                {
                    // Get ID of the output stream if a new one was inserted above.
                    if (outputStream.ID == 0)
                        outputStream.ID = GetOutputStream(database, $" WHERE Acronym = '{outputStream.Acronym.Replace(" ", "").ToUpper()}'").ID;

                    IList<int> keys = OutputStreamDevice.LoadKeys(database, outputStream.ID);

                    // Get all existing devices associated with output stream and delete them.
                    ObservableCollection<OutputStreamDevice> outputStreamDevices = OutputStreamDevice.Load(database, keys);
                    foreach (OutputStreamDevice outputStreamDevice in outputStreamDevices)
                        OutputStreamDevice.Delete(database, outputStream.ID, outputStreamDevice.Acronym);

                    if (!string.IsNullOrEmpty(outputStream.MirroringSourceDevice))
                    {
                        // Get list of input devices, filter by original source = outputstream.MirrorSourceDevice.
                        ObservableCollection<Device> devices = Device.GetDevices(database, $"WHERE OriginalSource = '{outputStream.MirroringSourceDevice}'");

                        // Add these above input devices as output stream devices.
                        OutputStreamDevice.AddDevices(database, outputStream.ID, devices, true, true);
                    }
                }

                return "Output Stream Information Saved Successfully";
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="OutputStream"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of <see cref="OutputStream"/>s defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = true)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> osList = new();

                DataTable results = database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, Name FROM OutputStream WHERE NodeID = {0} ORDER BY Name", "nodeID"), DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in results.Rows)
                    osList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return osList;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="OutputStream"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int outputStreamID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Get Acronym of output stream we need to delete and save it
                DataTable outputStreamAcronym = database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT Acronym FROM OutputStream WHERE ID = {0}", "outputStreamID"), DefaultTimeout, outputStreamID);

                // Delete output stream from database
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStream WHERE ID = {0}", "outputStreamID"), DefaultTimeout, outputStreamID);

                // Delete statistic measurements from database using the output stream acronym we have just deleted
                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString($"DELETE FROM Measurement WHERE SignalReference LIKE '{outputStreamAcronym.Rows[0].Field<string>("Acronym")}!OS-ST%'"), DefaultTimeout);

                CommonFunctions.SendCommandToService("ReloadConfig");

                return "Output Stream Deleted Successfully";
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        /// <summary>
        /// Gets output stream.
        /// </summary>
        /// <param name="database">Source database connection.</param>
        /// <param name="whereClause">Where filter clause.</param>
        /// <returns>Output stream.</returns>
        public static OutputStream GetOutputStream(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                DataTable outputStreamTable = database.Connection.RetrieveData(database.AdapterType, $"SELECT * FROM OutputStreamDetail {whereClause}");

                if (outputStreamTable.Rows.Count == 0)
                    return null;

                DataRow row = outputStreamTable.Rows[0];
                OutputProtocol type = (OutputProtocol)Convert.ToInt32(row.Field<object>("Type"));

                OutputStream outputStream = new()
                {
                    NodeID = database.Guid(row, "NodeID"),
                    ID = Convert.ToInt32(row.Field<object>("ID")),
                    Acronym = row.Field<string>("Acronym"),
                    Name = row.Field<string>("Name"),
                    Type = type,
                    ConnectionString = row.Field<string>("ConnectionString"),
                    IDCode = Convert.ToInt32(row.Field<object>("IDCode")),
                    CommandChannel = row.Field<string>("CommandChannel"),
                    DataChannel = row.Field<string>("DataChannel"),
                    AutoPublishConfigFrame = Convert.ToBoolean(row.Field<object>("AutoPublishConfigFrame")),
                    AutoStartDataChannel = Convert.ToBoolean(row.Field<object>("AutoStartDataChannel")),
                    NominalFrequency = Convert.ToInt32(row.Field<object>("NominalFrequency")),
                    FramesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                    LagTime = row.ConvertField<double>("LagTime"),
                    LeadTime = row.ConvertField<double>("LeadTime"),
                    UseLocalClockAsRealTime = Convert.ToBoolean(row.Field<object>("UseLocalClockAsRealTime")),
                    AllowSortsByArrival = Convert.ToBoolean(row.Field<object>("AllowSortsByArrival")),
                    LoadOrder = Convert.ToInt32(row.Field<object>("LoadOrder")),
                    Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                    NodeName = row.Field<string>("NodeName"),
                    IgnoreBadTimeStamps = Convert.ToBoolean(row.Field<object>("IgnoreBadTimeStamps")),
                    TimeResolution = Convert.ToInt32(row.Field<object>("TimeResolution")),
                    AllowPreemptivePublishing = Convert.ToBoolean(row.Field<object>("AllowPreemptivePublishing")),
                    DownSamplingMethod = row.Field<string>("DownsamplingMethod"),
                    DataFormat = row.Field<string>("DataFormat"),
                    CoordinateFormat = row.Field<string>("CoordinateFormat"),
                    CurrentScalingValue = Convert.ToInt32(row.Field<object>("CurrentScalingValue")),
                    VoltageScalingValue = Convert.ToInt32(row.Field<object>("VoltageScalingValue")),
                    AnalogScalingValue = Convert.ToInt32(row.Field<object>("AnalogScalingValue")),
                    DigitalMaskValue = Convert.ToInt32(row.Field<object>("DigitalMaskValue")),
                    PerformTimestampReasonabilityCheck = Convert.ToBoolean(row.Field<object>("PerformTimeReasonabilityCheck")),
                    m_autoPublishConfigFrameChangedByUser = false
                };

                return outputStream;
            }
            finally
            {
                if (createdConnection)
                    database?.Dispose();
            }
        }

        #endregion
    }
}
