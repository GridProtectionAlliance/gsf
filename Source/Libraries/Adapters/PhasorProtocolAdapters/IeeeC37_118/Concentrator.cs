//******************************************************************************************************
//  Concentrator.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/20/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/27/2010 - J. Ritchie Carroll
//       Added optional ID code validation for command requests.
//  12/04/2012 - J. Ritchie Carroll
//       Migrated to PhasorProtocolAdapters project.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF;
using GSF.Communication;
using GSF.Diagnostics;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Model;
using GSF.Units.EE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ExpressionEvaluator;
using GSF.ComponentModel;
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
using AnonymousPhasorDefinition = GSF.PhasorProtocols.Anonymous.PhasorDefinition;
using AnonymousDigitalDefinition = GSF.PhasorProtocols.Anonymous.DigitalDefinition;
using AnonymousConfigurationCell = GSF.PhasorProtocols.Anonymous.ConfigurationCell;
using AnonymousConfigurationFrame = GSF.PhasorProtocols.Anonymous.ConfigurationFrame;
using Measurement = GSF.TimeSeries.Model.Measurement;
using Phasor = GSF.TimeSeries.Model.Phasor;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
namespace PhasorProtocolAdapters.IeeeC37_118
{
    /// <summary>
    /// Represents an IEEE C37.118 phasor data concentrator.
    /// </summary>
    public class Concentrator : PhasorDataConcentratorBase
    {
        #region [ Members ]

        // Nested Types
        private class GlobalSettings
        {
            // ReSharper disable once MemberCanBePrivate.Local
            public static string CompanyAcronym { get; }

            static GlobalSettings()
            {
                try
                {
                    CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
                    CompanyAcronym = systemSettings["CompanyAcronym"]?.Value;
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex, "Failed to initialize default company acronym");
                }

                if (string.IsNullOrWhiteSpace(CompanyAcronym))
                    CompanyAcronym = "GPA";
            }
        }

        // Constants
        private const string ConfigFrame3CacheName = "{0}-CFG3";

        // Fields
        private ConfigurationFrame2 m_configurationFrame2;
        private ConfigurationFrame3 m_configurationFrame3;
        private bool m_configurationChanged;
        private Ticks m_notificationStartTime;
        private ushort m_revisionCount;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets IEEE C37.118 time base for this concentrator instance.
        /// </summary>
        public uint TimeBase { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if concentrator will validate ID code before processing commands.
        /// </summary>
        public bool ValidateIDCode { get; set; }

        /// <summary>
        /// Gets or sets default PMU_ELEV value to assign to PMUs in configuration 3 frames.
        /// </summary>
        public float Elevation { get; set; } = float.PositiveInfinity;

        /// <summary>
        /// Gets or sets default SVC_CLASS value, 'M' or 'P', to assign to PMUs in configuration 3 frames.
        /// </summary>
        public char ServiceClass { get; set; } = 'M';

        /// <summary>
        /// Gets or sets default WINDOW value to assign to PMUs in configuration 3 frames.
        /// </summary>
        public int Window { get; set; }

        /// <summary>
        /// Gets or sets default GRP_DLY value to assign to PMUs in configuration 3 frames.
        /// </summary>
        public int GroupDelay { get; set; }

        /// <summary>
        /// Gets or sets target output type for configuration frames. For example, setting property to
        /// <see cref="DraftRevision.Std2011"/> will target <see cref="ConfigurationFrame3"/> outputs.
        /// </summary>
        public DraftRevision TargetConfigurationType { get; set; } = DraftRevision.Std2005;

        /// <summary>
        /// Gets the maximum label length for string fields in configuration frames.
        /// </summary>
        public override int MaximumLabelLength => TargetConfigurationType >= DraftRevision.Std2011 ? byte.MaxValue : base.MaximumLabelLength;

        /// <summary>
        /// Gets the CFG-2 frame instance.
        /// </summary>
        public ConfigurationFrame2 ConfigurationFrame2 => m_configurationFrame2;

        /// <summary>
        /// Gets the CFG-3 frame instance.
        /// </summary>
        public ConfigurationFrame3 ConfigurationFrame3 => m_configurationFrame3;

        /// <summary>
        /// Returns the detailed status of this <see cref="Concentrator"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine($"           Output Protocol: IEEE C37.118-{TargetConfigurationType.ToVersionString()}");
                status.AppendLine($"      Configured Time Base: {TimeBase}");
                status.AppendLine($"        Validating ID Code: {ValidateIDCode}");

                if (TargetConfigurationType >= DraftRevision.Std2011)
                {
                    status.AppendLine($"     Default PMU Elevation: {Elevation}");
                    status.AppendLine($" Default PMU Service Class: {ServiceClass}");
                    status.AppendLine($" Default PMU Window Length: {Window:N0}µs");
                    status.AppendLine($"   Default PMU Group Delay: {GroupDelay:N0}µs");
                }
                
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="Concentrator"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;

            // Load optional parameters
            TimeBase = settings.TryGetValue(nameof(TimeBase), out string setting) ? uint.Parse(setting) : 16777215U;
            ValidateIDCode = settings.TryGetValue(nameof(ValidateIDCode), out setting) && setting.ParseBoolean();

            if (settings.TryGetValue(nameof(TargetConfigurationType), out setting) && Enum.TryParse(setting, out DraftRevision revision))
                TargetConfigurationType = revision;

            // Parse default IEEE C37.118 Configuration 3 parameters
            if ((settings.TryGetValue(nameof(Elevation), out setting) || settings.TryGetValue("PMU_ELEV", out setting)) && float.TryParse(setting, out float elevation))
                Elevation = elevation;

            if ((settings.TryGetValue(nameof(ServiceClass), out setting) || settings.TryGetValue("SVC_CLASS", out setting)) && char.TryParse(setting, out char serviceClass))
                ServiceClass = serviceClass;

            if (!ServiceClass.IsAnyOf(new[] { 'M', 'P' }))
                ServiceClass = 'M';

            if (settings.TryGetValue(nameof(Window), out setting) && int.TryParse(setting, out int window))
                Window = window;

            if ((settings.TryGetValue(nameof(GroupDelay), out setting) || settings.TryGetValue("GRP_DLY", out setting)) && int.TryParse(setting, out int groupDelay))
                GroupDelay = groupDelay;

            // Start base class initialization
            base.Initialize();
        }

        /// <summary>
        /// Creates a new IEEE C37.118 specific <see cref="IConfigurationFrame"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <see cref="AnonymousConfigurationFrame"/>.</param>
        /// <returns>A new IEEE C37.118 specific <see cref="IConfigurationFrame"/>.</returns>
        protected override IConfigurationFrame CreateNewConfigurationFrame(AnonymousConfigurationFrame baseConfigurationFrame)
        {
            // Create a new IEEE C37.118 configuration frame 2 using base configuration
            ConfigurationFrame2 configurationFrame2 = CreateConfigurationFrame2(baseConfigurationFrame, TimeBase, NominalFrequency);

            // Create a new IEEE C37.118 configuration frame 3 using base configuration
            ConfigurationFrame3 configurationFrame3 = CreateConfigurationFrame3(baseConfigurationFrame, TimeBase, NominalFrequency, this);

            // After system has started any subsequent changes in configuration get indicated in the outgoing data stream
            bool configurationChanged = false;

            if (!(m_configurationFrame2 is null))
            {
                // Get a clone of the current config frame and set its header match the cached config frame for a clean compare
                ConfigurationFrame1 cacheMatch = configurationFrame2.Clone(m_configurationFrame2.CommonHeader);
                configurationChanged = cacheMatch.Checksum != m_configurationFrame2.Checksum;
            }

            // Apply tracked revision counts
            if (configurationChanged)
            {
                unchecked { m_revisionCount++; }

                foreach (IConfigurationCell cell in configurationFrame2.Cells)
                    cell.RevisionCount = m_revisionCount;

                foreach (IConfigurationCell cell in configurationFrame3.Cells)
                    cell.RevisionCount = m_revisionCount;
            }

            // Cache new IEEE C7.118 configuration frames for later use
            Interlocked.Exchange(ref m_configurationFrame2, configurationFrame2);
            Interlocked.Exchange(ref m_configurationFrame3, configurationFrame3);

            if (configurationChanged)
            {
                // Start adding configuration changed notification flag to data cells
                m_configurationChanged = true;
                m_notificationStartTime = DateTime.UtcNow.Ticks;
            }

            return configurationFrame2;
        }

        /// <summary>
        /// Creates a new IEEE C37.118 specific <see cref="DataFrame"/> for the given <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp for new <see cref="IFrame"/> in <see cref="Ticks"/>.</param>
        /// <returns>New IEEE C37.118 <see cref="DataFrame"/> at given <paramref name="timestamp"/>.</returns>
        /// <remarks>
        /// Note that the <see cref="ConcentratorBase"/> class (which the <see cref="ActionAdapterBase"/> is derived from)
        /// is designed to sort <see cref="IMeasurement"/> implementations into an <see cref="IFrame"/> which represents
        /// a collection of measurements at a given timestamp. The <c>CreateNewFrame</c> method allows consumers to create
        /// their own <see cref="IFrame"/> implementations, in our case this will be an IEEE C37.118 data frame.
        /// </remarks>
        protected override IFrame CreateNewFrame(Ticks timestamp)
        {
            // We create a new IEEE C37.118 data frame based on current configuration frame
            DataFrame dataFrame = CreateDataFrame(timestamp, m_configurationFrame2);
            bool configurationChanged = false;

            if (m_configurationChanged)
            {
                // Change notifications should only last for one minute
                if ((DateTime.UtcNow.Ticks - m_notificationStartTime).ToSeconds() <= 60.0D)
                    configurationChanged = true;
                else
                    m_configurationChanged = false;
            }

            foreach (DataCell dataCell in dataFrame.Cells)
            {
                // Mark cells with configuration changed flag if configuration was reloaded
                if (configurationChanged)
                    dataCell.ConfigurationChangeDetected = true;

                // If the associated parent configuration for this data cell (PMU/device)
                // has identified data modifications through configuration, mark the data
                // modification bit of the data cell (Bit 9)
                if (dataCell.Parent3?.DataModified ?? false)
                    dataCell.DataModified = true;
            }

            return dataFrame;
        }

        /// <summary>
        /// Execute the publish operation for a configuration frame.
        /// </summary>
        /// <param name="timestamp">Timestamp to use for published configuration frame.</param>
        /// <returns>Total length of published bytes.</returns>
        /// <remarks>
        /// Overriding to publish desired target configuration frame, that is, type 2 or 3.
        /// </remarks>
        protected override int PublishConfigFrame(Ticks timestamp)
        {
            return TargetConfigurationType == DraftRevision.Std2011 ? 
                PublishConfigFrame3(frame => PublishChannel.MulticastAsync(frame, 0, frame.Length), timestamp) : 
                base.PublishConfigFrame(timestamp);
        }

        private int PublishConfigFrame3(Action<byte[]> publishFrame, long timestamp = 0)
        {
            int length = 0;

            if (timestamp > 0)
                m_configurationFrame3.Timestamp = timestamp;

            foreach (byte[] frame in m_configurationFrame3.BinaryImageFrames)
            {
                publishFrame(frame);
                length += frame.Length;
            }

            return length;
        }

        /// <summary>
        /// Handles incoming commands from devices connected over the command channel.
        /// </summary>
        /// <param name="clientID">Guid of client that sent the command.</param>
        /// <param name="connectionID">Remote client connection identification (i.e., IP:Port).</param>
        /// <param name="commandBuffer">Data buffer received from connected client device.</param>
        /// <param name="length">Valid length of data within the buffer.</param>
        protected override void DeviceCommandHandler(Guid clientID, string connectionID, byte[] commandBuffer, int length)
        {
            try
            {
                // Interpret data received from a client as a command frame
                CommandFrame commandFrame = new CommandFrame(commandBuffer, 0, length);
                IServer commandChannel = (IServer)CommandChannel ?? DataChannel;

                // Validate incoming ID code if requested
                if (ValidateIDCode && commandFrame.IDCode != IDCode)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Concentrator ID code validation failed for device command \"{commandFrame.Command}\" from \"{connectionID}\" - no action was taken.");
                }
                else
                {
                    switch (commandFrame.Command)
                    {
                        case DeviceCommand.SendConfigurationFrame1:
                            if (!(commandChannel is null))
                            {
                                ConfigurationFrame1 configFrame1 = CastToConfigurationFrame1(m_configurationFrame2);
                                commandChannel.SendToAsync(clientID, configFrame1.BinaryImage, 0, configFrame1.BinaryLength);
                                OnStatusMessage(MessageLevel.Info, $"Received request for \"{commandFrame.Command}\" from \"{connectionID}\" - type 1 config frame was returned.");
                            }

                            break;
                        case DeviceCommand.SendConfigurationFrame2:
                            if (!(commandChannel is null))
                            {
                                commandChannel.SendToAsync(clientID, m_configurationFrame2.BinaryImage, 0, m_configurationFrame2.BinaryLength);
                                OnStatusMessage(MessageLevel.Info, $"Received request for \"{commandFrame.Command}\" from \"{connectionID}\" - type 2 config frame was returned.");
                            }

                            break;
                        case DeviceCommand.SendConfigurationFrame3:
                            if (!(commandChannel is null))
                            {
                                PublishConfigFrame3(frame => commandChannel.SendToAsync(clientID, frame, 0, frame.Length));
                                OnStatusMessage(MessageLevel.Info, $"Received request for \"{commandFrame.Command}\" from \"{connectionID}\" - type 3 config frame was returned.");
                            }

                            break;
                        case DeviceCommand.SendHeaderFrame:
                            if (!(commandChannel is null))
                            {
                                StringBuilder status = new StringBuilder();
                                status.AppendLine($"IEEE C37.118 Concentrator:{Environment.NewLine}");
                                status.AppendLine($" Revision for config frame: {TargetConfigurationType}");
                                status.AppendLine($" Auto-publish config frame: {AutoPublishConfigurationFrame}");
                                status.AppendLine($"   Auto-start data channel: {AutoStartDataChannel}");
                                status.AppendLine($"       Data stream ID code: {IDCode:N0}");
                                status.AppendLine($"       Derived system time: {RealTime:yyyy-MM-dd HH:mm:ss.fff} UTC");

                                HeaderFrame headerFrame = new HeaderFrame(status.ToString());
                                commandChannel.SendToAsync(clientID, headerFrame.BinaryImage, 0, headerFrame.BinaryLength);
                                OnStatusMessage(MessageLevel.Info, $"Received request for \"SendHeaderFrame\" from \"{connectionID}\" - frame was returned.");
                            }

                            break;
                        case DeviceCommand.EnableRealTimeData:
                            // Only responding to stream control command if auto-start data channel is false
                            if (!AutoStartDataChannel)
                            {
                                StartDataChannel();
                                OnStatusMessage(MessageLevel.Info, $"Received request for \"EnableRealTimeData\" from \"{connectionID}\" - concentrator real-time data stream was started.");
                            }
                            else
                            {
                                OnStatusMessage(MessageLevel.Info, $"Request for \"EnableRealTimeData\" from \"{connectionID}\" was ignored - concentrator data channel is set for auto-start.");
                            }

                            break;
                        case DeviceCommand.DisableRealTimeData:
                            // Only responding to stream control command if auto-start data channel is false
                            if (!AutoStartDataChannel)
                            {
                                StopDataChannel();
                                OnStatusMessage(MessageLevel.Info, $"Received request for \"DisableRealTimeData\" from \"{connectionID}\" - concentrator real-time data stream was stopped.");
                            }
                            else
                            {
                                OnStatusMessage(MessageLevel.Info, $"Request for \"DisableRealTimeData\" from \"{connectionID}\" was ignored - concentrator data channel is set for auto-start.");
                            }

                            break;
                        default:
                            OnStatusMessage(MessageLevel.Info, $"Request for \"{commandFrame.Command}\" from \"{connectionID}\" was ignored - device command is unsupported.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Remotely connected device \"{connectionID}\" sent an unrecognized data sequence to the concentrator, no action was taken. Exception details: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Serialize configuration frames, types 2 and 3, to cache folder for later use (if needed).
        /// </summary>
        /// <param name="_"></param>
        /// <param name="name">Name to use when caching the configuration.</param>
        protected override void CacheConfigurationFrame(IConfigurationFrame _, string name)
        {
            // Cache configuration frame for reference
            OnStatusMessage(MessageLevel.Info, "Caching configuration frame...");

            try
            {
                void exceptionHandler(Exception ex) => OnProcessException(MessageLevel.Info, ex);

                // Cache both configuration 2 and 3 frames
                AnonymousConfigurationFrame.Cache(m_configurationFrame2, exceptionHandler, name);
                AnonymousConfigurationFrame.Cache(m_configurationFrame3, exceptionHandler, string.Format(ConfigFrame3CacheName, name));
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(MessageLevel.Info, new InvalidOperationException("Failed to queue caching of config frame due to exception: " + ex.Message, ex));
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(Concentrator), MessageClass.Component);

        // Static Methods       

        /// <summary>
        /// Creates a new IEEE C37.118 <see cref="ConfigurationFrame2"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <see cref="AnonymousConfigurationFrame"/>.</param>
        /// <param name="timeBase">Timebase to use for fraction second resolution.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> to use for the new <see cref="ConfigurationFrame2"/></param>.
        /// <returns>A new IEEE C37.118 <see cref="ConfigurationFrame2"/>.</returns>
        public static ConfigurationFrame2 CreateConfigurationFrame2(AnonymousConfigurationFrame baseConfigurationFrame, uint timeBase, LineFrequency nominalFrequency)
        {
            // Create a new IEEE C37.118 configuration frame 2 using base configuration
            ConfigurationFrame2 configurationFrame = new ConfigurationFrame2(timeBase, baseConfigurationFrame.IDCode, DateTime.UtcNow.Ticks, baseConfigurationFrame.FrameRate);

            foreach (AnonymousConfigurationCell baseCell in baseConfigurationFrame.Cells)
            {
                // Create a new IEEE C37.118 configuration cell (i.e., a PMU configuration)
                ConfigurationCell newCell = new ConfigurationCell(configurationFrame, baseCell.IDCode, nominalFrequency)
                {
                    // Update other cell level attributes
                    PhasorDataFormat = baseCell.PhasorDataFormat,
                    PhasorCoordinateFormat = baseCell.PhasorCoordinateFormat,
                    FrequencyDataFormat = baseCell.FrequencyDataFormat,
                    AnalogDataFormat = baseCell.AnalogDataFormat
                };

                newCell.StationName = baseCell.StationName.TruncateRight(newCell.MaximumStationNameLength);
                newCell.IDLabel = baseCell.IDLabel.TruncateRight(newCell.IDLabelLength);
                int maximumLabelLength = newCell.MaximumStationNameLength;

                // Add phasor definitions
                foreach (IPhasorDefinition phasorDefinition in baseCell.PhasorDefinitions)
                    newCell.PhasorDefinitions.Add(new PhasorDefinition(newCell, phasorDefinition.Label.TruncateRight(maximumLabelLength), phasorDefinition.ScalingValue, phasorDefinition.Offset, phasorDefinition.PhasorType, null));

                // Add frequency definition
                newCell.FrequencyDefinition = new FrequencyDefinition(newCell, $"{newCell.IDLabel.TruncateRight(maximumLabelLength - 5)} Freq".Trim());

                // Add analog definitions
                foreach (IAnalogDefinition analogDefinition in baseCell.AnalogDefinitions)
                    newCell.AnalogDefinitions.Add(new AnalogDefinition(newCell, analogDefinition.Label.TruncateRight(maximumLabelLength), analogDefinition.ScalingValue, analogDefinition.Offset, analogDefinition.AnalogType));

                // Add digital definitions
                foreach (AnonymousDigitalDefinition digitalDefinition in baseCell.DigitalDefinitions)
                {
                    uint maskValue = digitalDefinition.MaskValue;

                    // Check for a config frame 3 style digital label
                    if (digitalDefinition.Label.Contains("|"))
                    {
                        string[] labels = digitalDefinition.Label.Split('|');

                        if (labels.Length == 16)
                            digitalDefinition.Label = string.Join("", labels.Select(label => label.GetValidLabel().TruncateRight(16).PadRight(16)));
                    }

                    newCell.DigitalDefinitions.Add(new DigitalDefinition(newCell, digitalDefinition.Label, maskValue.LowWord(), maskValue.HighWord()));
                }

                // Add new PMU configuration (cell) to protocol specific configuration frame
                configurationFrame.Cells.Add(newCell);
            }

            return configurationFrame;
        }

        /// <summary>
        /// Creates a new IEEE C37.118 <see cref="ConfigurationFrame3"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <see cref="AnonymousConfigurationFrame"/>.</param>
        /// <param name="timeBase">Timebase to use for fraction second resolution.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> to use for the new <see cref="ConfigurationFrame3"/></param>.
        /// <param name="parent">Gets reference to parent <see cref="Concentrator"/> instance, if available.</param>
        /// <returns>A new IEEE C37.118 <see cref="ConfigurationFrame3"/>.</returns>
        /// <remarks>
        /// When <paramref name="parent"/> reference is not available, ancillary configuration frame 3 data will be attempted to be loaded from last cached
        /// instance of the configuration matching the defined <see cref="AnonymousConfigurationFrame.Name"/> of the <paramref name="baseConfigurationFrame"/>.
        /// </remarks>
        public static ConfigurationFrame3 CreateConfigurationFrame3(AnonymousConfigurationFrame baseConfigurationFrame, uint timeBase, LineFrequency nominalFrequency, Concentrator parent = null)
        {
            // Create a new IEEE C37.118 configuration frame 3 using base configuration
            ConfigurationFrame3 configurationFrame = new ConfigurationFrame3(timeBase, baseConfigurationFrame.IDCode, DateTime.UtcNow.Ticks, baseConfigurationFrame.FrameRate);

            foreach (AnonymousConfigurationCell baseCell in baseConfigurationFrame.Cells)
            {
                // Create a new IEEE C37.118 configuration cell3 (i.e., a PMU configuration)
                ConfigurationCell3 newCell = new ConfigurationCell3(configurationFrame, baseCell.IDCode, nominalFrequency)
                {
                    // Update other cell level attributes
                    PhasorDataFormat = baseCell.PhasorDataFormat,
                    PhasorCoordinateFormat = baseCell.PhasorCoordinateFormat,
                    FrequencyDataFormat = baseCell.FrequencyDataFormat,
                    AnalogDataFormat = baseCell.AnalogDataFormat
                };

                newCell.StationName = baseCell.StationName.TruncateRight(newCell.MaximumStationNameLength);
                newCell.IDLabel = baseCell.IDLabel.TruncateRight(newCell.IDLabelLength);

                // Add phasor definitions
                foreach (AnonymousPhasorDefinition phasorDefinition in baseCell.PhasorDefinitions)
                    newCell.PhasorDefinitions.Add(new PhasorDefinition3(newCell, phasorDefinition.Label, phasorDefinition.ScalingValue, phasorDefinition.Offset, phasorDefinition.PhasorType, null, phasorDefinition.Phase));

                // Add frequency definition
                newCell.FrequencyDefinition = new FrequencyDefinition(newCell, baseCell.FrequencyDefinition.Label);

                // Add analog definitions
                foreach (IAnalogDefinition analogDefinition in baseCell.AnalogDefinitions)
                    newCell.AnalogDefinitions.Add(new AnalogDefinition3(newCell, analogDefinition.Label, analogDefinition.ScalingValue, analogDefinition.Offset, analogDefinition.AnalogType));

                // Add digital definitions
                foreach (AnonymousDigitalDefinition digitalDefinition in baseCell.DigitalDefinitions)
                {
                    // Attempt to derive user defined mask value if available
                    uint maskValue = digitalDefinition.MaskValue;

                    // Check for a config frame 2 style digital label
                    if (!digitalDefinition.Label.Contains("|"))
                    {
                        string[] labels = new string[16];
                        string label = digitalDefinition.Label.PadRight(16 * 16);

                        for (int i = 0; i < 16; i++)
                            labels[i] = label.Substring(i * 16, 16).GetValidLabel().Trim();

                        digitalDefinition.Label = string.Join("|", labels);
                    }

                    newCell.DigitalDefinitions.Add(new DigitalDefinition3(newCell, digitalDefinition.Label, maskValue.LowWord(), maskValue.HighWord()));
                }

                // Add new PMU configuration (cell) to protocol specific configuration frame
                configurationFrame.Cells.Add(newCell);
            }

            bool loadFromCache = parent is null;

            // When parent reference is null, loading from cached configuration is still an option. This will allow the
            // CreateConfigurationFrame3 function to still be usable in a use cases outside of a concentrator context.
            if (!loadFromCache)
            {
                try
                {
                    TypeRegistry registry = ValueExpressionParser.DefaultTypeRegistry;

                    if (!registry.ContainsKey("Global"))
                        registry.RegisterSymbol("Global", new GlobalSettings()); // Needed by modeled Device records

                    // Populate extra fields for config frame 3, like the G_PMU_ID guid value. Note that all of this information can be
                    // derived from data in the database configuration, however it is not currently cached in the runtime configuration
                    // as defined through the ConfigurationEntity table. As a result it is necessary to open a database connection to
                    // acquire the needed data. Although this data is considered ancillary, it may be important in some configurations,
                    // so failures to connect to database will fall-back on info the previously cached IEEE C37.118-2011 config frame.
                    using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                    {
                        // Since output streams are completely virtualized, there is no guarantee that what is in an output stream will
                        // ever map back to a source device. They often do map back, but even the database makes no assumptions on device
                        // mappings, everything in an output stream is mapped by measurement and device information is considered temporal
                        // and dynamic. However, needed information needed for output devices, i.e, ConfigurationCell3 instances, from the
                        // database will be defined in the original source device. The only concrete mapping from source device to output
                        // devices are the measurements - so this will be the path to mapping back to the source. Since each device in an
                        // output stream will always have a single frequency measurement, this should be as good as any for finding our
                        // way back to the original associated source device, this assuming there was one. If no source device exists, we
                        // will fall back on default values.
                        TableOperations<Device> deviceTable = new TableOperations<Device>(database);
                        TableOperations<Measurement> measurementTable = new TableOperations<Measurement>(database);
                        TableOperations<Phasor> phasorTable = new TableOperations<Phasor>(database);

                        // Search signal reference map for frequencies
                        foreach (KeyValuePair<MeasurementKey, SignalReference[]> kvp in parent.SignalReferences)
                        {
                            MeasurementKey key = kvp.Key;
                            SignalReference[] signals = kvp.Value;

                            foreach (SignalReference signal in signals)
                            {
                                if (signal.Kind == SignalKind.Frequency)
                                {
                                    // Validate that signal reference has a matching device target, i.e., cell, in the configuration frame
                                    if (!(configurationFrame.Cells.FirstOrDefault(searchCell => 
                                        searchCell.StationName.Equals(signal.Acronym, StringComparison.OrdinalIgnoreCase)) is ConfigurationCell3 cell))
                                        continue;

                                    bool foundSource = false;

                                    // Lookup associated frequency measurement
                                    Measurement measurement = measurementTable.QueryRecordWhere("PointID = {0}", (long)key.ID);
                                    int? deviceID = measurement?.DeviceID;
                                    int framesPerSecond = parent.FramesPerSecond;

                                    if (!(deviceID is null))
                                    {
                                        // Lookup frequency's parent device
                                        Device device = deviceTable.QueryRecordWhere("ID = {0}", deviceID);

                                        if (!(device is null))
                                        {
                                            // Assign common time-series configuration values
                                            cell.GlobalID = device.UniqueID;
                                            cell.Latitude = (float?)device.Latitude ?? float.PositiveInfinity;
                                            cell.Longitude = (float?)device.Longitude ?? float.PositiveInfinity;

                                            // Assign new configuration frame 3 specific values
                                            Dictionary<string, string> settings = device.ConnectionString?.ParseKeyValuePairs() ?? new Dictionary<string, string>();

                                            cell.Elevation = (settings.TryGetValue(nameof(Elevation), out string setting) || settings.TryGetValue("PMU_ELEV", out setting)) && float.TryParse(setting, out float elevation) ?
                                                elevation : parent.Elevation;

                                            cell.ServiceClass = (settings.TryGetValue(nameof(ServiceClass), out setting) || settings.TryGetValue("SVC_CLASS", out setting)) && char.TryParse(setting, out char serviceClass) ?
                                                serviceClass : parent.ServiceClass;

                                            cell.Window = settings.TryGetValue(nameof(Window), out setting) && int.TryParse(setting, out int window) ?
                                                window : parent.Window;

                                            cell.GroupDelay = (settings.TryGetValue(nameof(GroupDelay), out setting) || settings.TryGetValue("GRP_DLY", out setting)) && int.TryParse(setting, out int groupDelay) ?
                                                groupDelay : parent.GroupDelay;

                                            if (device.FramesPerSecond.HasValue)
                                                framesPerSecond = device.FramesPerSecond.Value;

                                            foundSource = true;
                                        }
                                    }

                                    if (!foundSource)
                                    {
                                        // Assign default values
                                        cell.GlobalID = Guid.Empty;
                                        cell.Latitude = float.PositiveInfinity;
                                        cell.Longitude = float.PositiveInfinity;
                                        cell.Elevation = parent.Elevation;
                                        cell.ServiceClass = parent.ServiceClass;
                                        cell.Window = parent.Window;
                                        cell.GroupDelay = parent.GroupDelay;
                                    }

                                    bool findMeasurementKey(SignalReference signalReference, out MeasurementKey measurementKey)
                                    {
                                        foreach (KeyValuePair<MeasurementKey, SignalReference[]> pair in parent.SignalReferences)
                                        {
                                            if (pair.Value.Any(sourceSignal => sourceSignal == signalReference))
                                            {
                                                measurementKey = pair.Key;
                                                return true;
                                            }
                                        }

                                        measurementKey = MeasurementKey.Undefined;
                                        return false;
                                    }

                                    foreach (PhasorDefinition3 phasor in cell.PhasorDefinitions)
                                    {
                                        measurement = null;

                                        // Find the associated measurement key for the phasor angle
                                        SignalReference angleSignal = new SignalReference
                                        {
                                            Acronym = cell.StationName,
                                            Kind = SignalKind.Angle,
                                            Index = phasor.Index + 1
                                        };

                                        if (findMeasurementKey(angleSignal, out MeasurementKey angleKey))
                                        {
                                            measurement = measurementTable.QueryRecordWhere("PointID = {0}", (long)angleKey.ID);

                                            if (measurement?.Multiplier != 1.0F)
                                                phasor.PhasorDataModifications |= PhasorDataModifications.AngleCalibrationAdjustment;

                                            if (measurement?.Adder != 0.0F)
                                                phasor.PhasorDataModifications |= PhasorDataModifications.AngleRotationAdjustment;
                                        }

                                        // Find the associated measurement key for the phasor magnitude
                                        SignalReference magnitudeSignal = new SignalReference
                                        {
                                            Acronym = cell.StationName,
                                            Kind = SignalKind.Magnitude,
                                            Index = phasor.Index + 1
                                        };

                                        if (findMeasurementKey(magnitudeSignal, out MeasurementKey magnitudeKey))
                                        {
                                            measurement = measurementTable.QueryRecordWhere("PointID = {0}", (long)magnitudeKey.ID);

                                            if (measurement?.Multiplier != 1.0F || measurement.Adder != 0.0F)
                                                phasor.PhasorDataModifications |= PhasorDataModifications.MagnitudeCalibrationAdjustment;
                                        }

                                        // Assign transmission voltage level to user flags, when defined
                                        if (phasor.PhasorType == PhasorType.Voltage && !(deviceID is null) && !(measurement?.PhasorSourceIndex is null))
                                        {
                                            Phasor phasorRecord = phasorTable.QueryRecordWhere("DeviceID = {0} AND SourceIndex = {1} AND Type = 'V'", deviceID, measurement.PhasorSourceIndex);

                                            if (!(phasorRecord is null) && phasorRecord.BaseKV.TryGetVoltageLevel(out VoltageLevel level))
                                                phasor.UserFlags = (byte)level;
                                        }

                                        if (parent.FramesPerSecond < framesPerSecond)
                                        {
                                            switch (parent.DownsamplingMethod)
                                            {
                                                case DownsamplingMethod.LastReceived:
                                                case DownsamplingMethod.Closest:
                                                    phasor.PhasorDataModifications |= PhasorDataModifications.DownSampledByReselection;
                                                    break;
                                                case DownsamplingMethod.Filtered:
                                                    // Filtered down-sampling here simply uses a moving average which is a special case
                                                    // of the Finite Impulse Response filter
                                                    phasor.PhasorDataModifications |= PhasorDataModifications.DownSampledWithFIRFilter;
                                                    break;
                                                case DownsamplingMethod.BestQuality:
                                                    phasor.PhasorDataModifications |= PhasorDataModifications.DownSampledWithNonFIRFilter;
                                                    break;
                                                default:
                                                    phasor.PhasorDataModifications |= PhasorDataModifications.OtherModificationApplied;
                                                    break;
                                            }
                                        }

                                        if (!cell.DataModified && phasor.PhasorDataModifications != PhasorDataModifications.NoModifications)
                                            cell.DataModified = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    loadFromCache = true;

                    s_log.Publish(MessageLevel.Warning, MessageFlags.UsageIssue, "CFG3 Ancillary Data Load from Database",
                        "Failed while attempting load ancillary IEEE C37.118-2011 configuration frame 3 info from database",
                        "System will attempt to load info from any previously cached config frame", ex);
                }
            }

            if (loadFromCache)
            {
                try
                {
                    // If database load did not succeed, fall back on trying to load ancillary data from a previously cached config 3 frame
                    if (!(AnonymousConfigurationFrame.GetCachedConfiguration(string.Format(ConfigFrame3CacheName, baseConfigurationFrame.Name), true) is ConfigurationFrame3 cachedConfigFrame))
                        throw new NullReferenceException("Failed to load cached configuration frame.");

                    foreach (ConfigurationCell3 cell in configurationFrame.Cells)
                    {
                        // Try to match cached cell to target cell by ID code first
                        if (!(cachedConfigFrame.Cells.FirstOrDefault(searchCell => searchCell.IDCode == cell.IDCode) is ConfigurationCell3 cachedCell))
                        {
                            // If ID code match failed, try match by station name 
                            cachedCell = cachedConfigFrame.Cells.FirstOrDefault(searchCell => searchCell.StationName.Equals(cell.StationName, StringComparison.OrdinalIgnoreCase)) as ConfigurationCell3;

                            if (cachedCell is null)
                                continue;
                        }

                        // The following values have long been available in GSF time-series configuration
                        cell.GlobalID = cachedCell.GlobalID;
                        cell.Latitude = cachedCell.Latitude;
                        cell.Longitude = cachedCell.Longitude;

                        // The following values are new to GSF time-series configuration, so we fall back on configured defaults if needed
                        cell.Elevation = !double.IsInfinity(cachedCell.Elevation) || parent is null ? cachedCell.Elevation : parent.Elevation;
                        cell.ServiceClass = cachedCell.ServiceClass.IsAnyOf(new[] { 'M', 'P' }) || parent is null ? cachedCell.ServiceClass : parent.ServiceClass;
                        cell.Window = cachedCell.Window != 0 || parent is null ? cachedCell.Window : parent.Window;
                        cell.GroupDelay = cachedCell.GroupDelay != 0 || parent is null ? cachedCell.GroupDelay : parent.GroupDelay;
                        cell.DataModified = cachedCell.DataModified;

                        foreach (PhasorDefinition3 phasor in cell.PhasorDefinitions)
                        {
                            // Try to match by phasor label
                            if (cachedCell.PhasorDefinitions.FirstOrDefault(searchDefinition => searchDefinition.Label.Equals(phasor.Label, StringComparison.OrdinalIgnoreCase)) is PhasorDefinition3 cachedPhasor)
                            {
                                phasor.PhasorDataModifications = cachedPhasor.PhasorDataModifications;
                                phasor.UserFlags = cachedPhasor.UserFlags;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    s_log.Publish(MessageLevel.Error, MessageFlags.UsageIssue, "CFG3 Ancillary Data Load from Cached Config",
                        "Failed while attempting load ancillary IEEE C37.118-2011 configuration frame 3 from last cached instance",
                        "System cannot load ancillary data for configuration frame 3 instance", ex);
                }
            }

            return configurationFrame;
        }

        /// <summary>
        /// Creates a new IEEE C37.118 specific <see cref="DataFrame"/> for the given <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp for new <see cref="IFrame"/> in <see cref="Ticks"/>.</param>
        /// <param name="configurationFrame">Associated <see cref="ConfigurationFrame1"/> for the new <see cref="DataFrame"/>.</param>
        /// <returns>New IEEE C37.118 <see cref="DataFrame"/> at given <paramref name="timestamp"/>.</returns>
        public static DataFrame CreateDataFrame(Ticks timestamp, ConfigurationFrame1 configurationFrame)
        {
            // We create a new IEEE C37.118 data frame based on current configuration frame
            DataFrame dataFrame = new DataFrame(timestamp, configurationFrame);

            foreach (ConfigurationCell configurationCell in configurationFrame.Cells)
            {
                // Create a new IEEE C37.118 data cell (i.e., a PMU entry for this frame)
                DataCell dataCell = new DataCell(dataFrame, configurationCell, true);

                // Add data cell to the frame
                dataFrame.Cells.Add(dataCell);
            }

            return dataFrame;
        }

        /// <summary>
        /// Converts given IEEE C37.118 type 2 <paramref name="sourceFrame"/> into a type 1 configuration frame.
        /// </summary>
        /// <param name="sourceFrame">Source configuration frame.</param>
        /// <returns>New <see cref="ConfigurationFrame1"/> frame based on source configuration.</returns>
        /// <remarks>
        /// This function allow an explicit downcast of a typical IEEE C37.118 configuration type 2 frame to a type 1 frame.
        /// </remarks>
        public static ConfigurationFrame1 CastToConfigurationFrame1(ConfigurationFrame2 sourceFrame)
        {
            ConfigurationFrame1 derivedFrame;

            // Create a new IEEE C37.118 configuration frame converted from equivalent configuration information
            if (sourceFrame.DraftRevision == DraftRevision.Std2005)
                derivedFrame = new ConfigurationFrame1(sourceFrame.Timebase, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);
            else
                derivedFrame = new ConfigurationFrame1Draft6(sourceFrame.Timebase, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

            foreach (ConfigurationCell sourceCell in sourceFrame.Cells)
            {
                // Create new derived configuration cell
                ConfigurationCell derivedCell = new ConfigurationCell(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                string stationName = sourceCell.StationName;
                string idLabel = sourceCell.IDLabel;

                if (!string.IsNullOrWhiteSpace(stationName))
                    derivedCell.StationName = stationName.TruncateLeft(derivedCell.MaximumStationNameLength);

                if (!string.IsNullOrWhiteSpace(idLabel))
                    derivedCell.IDLabel = idLabel.TruncateLeft(derivedCell.IDLabelLength);

                derivedCell.PhasorCoordinateFormat = sourceCell.PhasorCoordinateFormat;
                derivedCell.PhasorAngleFormat = sourceCell.PhasorAngleFormat;
                derivedCell.PhasorDataFormat = sourceCell.PhasorDataFormat;
                derivedCell.FrequencyDataFormat = sourceCell.FrequencyDataFormat;
                derivedCell.AnalogDataFormat = sourceCell.AnalogDataFormat;

                // Create equivalent derived phasor definitions
                foreach (PhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null));

                // Create equivalent derived frequency definition
                FrequencyDefinition sourceFrequency = sourceCell.FrequencyDefinition as FrequencyDefinition;            
                derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency?.Label);

                // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
                foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                    derivedCell.AnalogDefinitions.Add(new AnalogDefinition(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.Offset, sourceAnalog.AnalogType));

                // Create equivalent derived digital definitions
                foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label, 0, 0));

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);
            }

            return derivedFrame;
        }

        #endregion
    }
}