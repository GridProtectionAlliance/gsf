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
//  02/05/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GSF;
using GSF.Communication;
using GSF.Diagnostics;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.IEC61850_90_5;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units.EE;
using DigitalDefinition = GSF.PhasorProtocols.Anonymous.DigitalDefinition;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
namespace PhasorProtocolAdapters.Iec61850_90_5
{
    /// <summary>
    /// Represents an IEC 61850-90-5 phasor data concentrator.
    /// </summary>
    public class Concentrator : PhasorDataConcentratorBase
    {
        #region [ Members ]

        // Fields
        private ConfigurationFrame m_configurationFrame;
        private uint m_timeBase;
        private bool m_configurationChanged;
        private uint m_configurationRevision;
        private Ticks m_notificationStartTime;
        private bool m_validateIDCode;
        private string m_msvid;
        private int m_asduCount;
        private ushort m_sampleCount;
        private uint m_packetNumber;
        private byte[][] m_asduImages;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets IEEE C37.118 time base for this concentrator instance.
        /// </summary>
        public uint TimeBase
        {
            get
            {
                return m_timeBase;
            }
            set
            {
                m_timeBase = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if concentrator will validate ID code before processing commands.
        /// </summary>
        public bool ValidateIDCode
        {
            get
            {
                return m_validateIDCode;
            }
            set
            {
                m_validateIDCode = value;
            }
        }

        /// <summary>
        /// Returns the detailed status of this <see cref="Concentrator"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine("           Output protocol: IEC 61850-90-5");
                status.AppendFormat("      Configured time base: {0}", m_timeBase);
                status.AppendLine();
                status.AppendFormat("        Validating ID code: {0}", m_validateIDCode);
                status.AppendLine();
                status.AppendFormat("                     MSVID: {0}", m_msvid);
                status.AppendLine();
                status.AppendFormat("                ASDU Count: {0}", m_asduCount);
                status.AppendLine();
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
            string setting;

            // Load optional parameters
            if (settings.TryGetValue("validateIDCode", out setting))
                m_validateIDCode = setting.ParseBoolean();
            else
                m_validateIDCode = false;

            // Pre-fetch ID code to create default MSVID
            ushort idCode;

            if (settings.TryGetValue("IDCode", out setting) && ushort.TryParse(setting, out idCode))
            {
                if (settings.TryGetValue("msvid", out setting))
                    m_msvid = setting;
                else
                    m_msvid = $"{idCode}_{Name}";
            }

            // Get ASDU count
            if (!(settings.TryGetValue("asduCount", out setting) && int.TryParse(setting, out m_asduCount) && m_asduCount > 0))
                m_asduCount = 1;

            if (m_asduCount > 32)
                throw new InvalidOperationException("ASDU count current limited to 32 images.");

            // Establish ASDU image cache
            m_asduImages = new byte[m_asduCount][];

            // Start base class initialization
            base.Initialize();
        }

        /// <summary>
        /// Creates a new IEEE C37.118 specific <see cref="IConfigurationFrame"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <see cref="GSF.PhasorProtocols.Anonymous.ConfigurationFrame"/>.</param>
        /// <returns>A new IEEE C37.118 specific <see cref="IConfigurationFrame"/>.</returns>
        protected override IConfigurationFrame CreateNewConfigurationFrame(GSF.PhasorProtocols.Anonymous.ConfigurationFrame baseConfigurationFrame)
        {
            // Create a new IEEE C37.118 configuration frame 2 using base configuration
            ConfigurationFrame configurationFrame = CreateConfigurationFrame(baseConfigurationFrame, m_timeBase, base.NominalFrequency);

            // After system has started any subsequent changes in configuration get indicated in the outgoing data stream
            bool configurationChanged = m_configurationFrame != null;

            // Cache new configuration frame for later use
            Interlocked.Exchange(ref m_configurationFrame, configurationFrame);
            m_configurationRevision++;

            if (configurationChanged)
            {
                // Start adding configuration changed notification flag to data cells
                m_configurationChanged = true;
                m_notificationStartTime = DateTime.UtcNow.Ticks;
            }

            return configurationFrame;
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
            DataFrame dataFrame = CreateDataFrame(timestamp, m_configurationFrame, m_msvid, m_asduCount, m_asduImages, m_configurationRevision);
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
            }

            // Increment IEC 61850-90-5 sequence numbers
            unchecked
            {
                m_sampleCount++;
                m_packetNumber++;
            }

            dataFrame.SampleCount = m_sampleCount;
            dataFrame.CommonHeader.PacketNumber = m_packetNumber;

            return dataFrame;
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
                if (!m_validateIDCode || commandFrame.IDCode == this.IDCode)
                {
                    switch (commandFrame.Command)
                    {
                        case DeviceCommand.SendConfigurationFrame1:
                        case DeviceCommand.SendConfigurationFrame2:
                            if (commandChannel != null)
                            {
                                commandChannel.SendToAsync(clientID, m_configurationFrame.BinaryImage, 0, m_configurationFrame.BinaryLength);
                                OnStatusMessage(MessageLevel.Info, "Concentrator", "Received request for \"{0}\" from \"{1}\" - frame was returned.", commandFrame.Command, connectionID);
                            }
                            break;
                        case DeviceCommand.EnableRealTimeData:
                            // Only responding to stream control command if auto-start data channel is false
                            if (!AutoStartDataChannel)
                            {
                                StartDataChannel();
                                OnStatusMessage(MessageLevel.Info, "Concentrator", "Received request for \"EnableRealTimeData\" from \"{0}\" - concentrator real-time data stream was started.", connectionID);
                            }
                            else
                            {
                                OnStatusMessage(MessageLevel.Info, "Concentrator", "Request for \"EnableRealTimeData\" from \"{0}\" was ignored - concentrator data channel is set for auto-start.", connectionID);                                
                            }
                            break;
                        case DeviceCommand.DisableRealTimeData:
                            // Only responding to stream control command if auto-start data channel is false
                            if (!AutoStartDataChannel)
                            {
                                StopDataChannel();
                                OnStatusMessage(MessageLevel.Info, "Concentrator", "Received request for \"DisableRealTimeData\" from \"{0}\" - concentrator real-time data stream was stopped.", connectionID);
                            }
                            else
                            {
                                OnStatusMessage(MessageLevel.Info, "Concentrator", "Request for \"DisableRealTimeData\" from \"{0}\" was ignored - concentrator data channel is set for auto-start.", connectionID);                                
                            }
                            break;
                        default:
                            OnStatusMessage(MessageLevel.Info, "Concentrator", "Request for \"{0}\" from \"{1}\" was ignored - device command is unsupported.", commandFrame.Command, connectionID);
                            break;
                    }
                }
                else
                    OnStatusMessage(MessageLevel.Info, "Concentrator", "WARNING: Concentrator ID code validation failed for device command \"{0}\" from \"{1}\" - no action was taken.", commandFrame.Command, connectionID);
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, "Concentrator", new InvalidOperationException($"Remotely connected device \"{connectionID}\" sent an unrecognized data sequence to the concentrator, no action was taken. Exception details: {ex.Message}", ex));
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods       

        /// <summary>
        /// Creates a new IEC 61850-90-5 (i.e., IEEE C37.118) <see cref="ConfigurationFrame"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <see cref="GSF.PhasorProtocols.Anonymous.ConfigurationFrame"/>.</param>
        /// <param name="timeBase">Timebase to use for fraction second resolution.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> to use for the new <see cref="ConfigurationFrame"/></param>.
        /// <returns>A new IEEE C37.118 <see cref="ConfigurationFrame"/>.</returns>
        public static ConfigurationFrame CreateConfigurationFrame(GSF.PhasorProtocols.Anonymous.ConfigurationFrame baseConfigurationFrame, uint timeBase, LineFrequency nominalFrequency)
        {
            ConfigurationCell newCell;
            uint maskValue;

            // Create a new IEEE C37.118 configuration frame 2 using base configuration
            ConfigurationFrame configurationFrame = new ConfigurationFrame(timeBase, baseConfigurationFrame.IDCode, DateTime.UtcNow.Ticks, baseConfigurationFrame.FrameRate);

            foreach (GSF.PhasorProtocols.Anonymous.ConfigurationCell baseCell in baseConfigurationFrame.Cells)
            {
                // Create a new IEEE C37.118 configuration cell (i.e., a PMU configuration)
                newCell = new ConfigurationCell(configurationFrame, baseCell.IDCode, nominalFrequency);

                // Update other cell level attributes
                newCell.StationName = baseCell.StationName;
                newCell.IDLabel = baseCell.IDLabel;
                newCell.PhasorDataFormat = baseCell.PhasorDataFormat;
                newCell.PhasorCoordinateFormat = baseCell.PhasorCoordinateFormat;
                newCell.FrequencyDataFormat = baseCell.FrequencyDataFormat;
                newCell.AnalogDataFormat = baseCell.AnalogDataFormat;

                // Add phasor definitions
                foreach (IPhasorDefinition phasorDefinition in baseCell.PhasorDefinitions)
                {
                    newCell.PhasorDefinitions.Add(new PhasorDefinition(newCell, phasorDefinition.Label, phasorDefinition.ScalingValue, phasorDefinition.Offset, phasorDefinition.PhasorType, null));
                }

                // Add frequency definition
                newCell.FrequencyDefinition = new FrequencyDefinition(newCell, baseCell.FrequencyDefinition.Label);

                // Add analog definitions
                foreach (IAnalogDefinition analogDefinition in baseCell.AnalogDefinitions)
                {
                    newCell.AnalogDefinitions.Add(new AnalogDefinition(newCell, analogDefinition.Label, analogDefinition.ScalingValue, analogDefinition.Offset, analogDefinition.AnalogType));
                }

                // Add digital definitions
                foreach (IDigitalDefinition digitalDefinition in baseCell.DigitalDefinitions)
                {
                    // Attempt to derive user defined mask value if available
                    DigitalDefinition anonymousDigitalDefinition = digitalDefinition as DigitalDefinition;

                    if (anonymousDigitalDefinition != null)
                        maskValue = anonymousDigitalDefinition.MaskValue;
                    else
                        maskValue = 0U;

                    newCell.DigitalDefinitions.Add(new GSF.PhasorProtocols.IEC61850_90_5.DigitalDefinition(newCell, digitalDefinition.Label, maskValue.LowWord(), maskValue.HighWord()));
                }

                // Add new PMU configuration (cell) to protocol specific configuration frame
                configurationFrame.Cells.Add(newCell);
            }

            return configurationFrame;
        }

        /// <summary>
        /// Creates a new IEC 61850-90-5 specific <see cref="DataFrame"/> for the given <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp for new <see cref="IFrame"/> in <see cref="Ticks"/>.</param>
        /// <param name="configurationFrame">Associated <see cref="ConfigurationFrame"/> for the new <see cref="DataFrame"/>.</param>
        /// <param name="msvID">MSVID to use for <see cref="DataFrame"/>.</param>
        /// <param name="asduCount">ASDU count.</param>
        /// <param name="asduImages">Concentrator's ASDU image cache.</param>
        /// <param name="configurationRevision">Configuration revision.</param>
        /// <returns>New IEC 61850-90-5 <see cref="DataFrame"/> at given <paramref name="timestamp"/>.</returns>
        public static DataFrame CreateDataFrame(Ticks timestamp, ConfigurationFrame configurationFrame, string msvID, int asduCount, byte[][] asduImages, uint configurationRevision)
        {
            // We create a new IEC 61850-90-5 data frame based on current configuration frame
            DataFrame dataFrame = new DataFrame(timestamp, configurationFrame, msvID, asduCount, asduImages, configurationRevision);
            DataCell dataCell;

            foreach (ConfigurationCell configurationCell in configurationFrame.Cells)
            {
                // Create a new IEC 61850-90-5 data cell (i.e., a PMU entry for this frame)
                dataCell = new DataCell(dataFrame, configurationCell, true);

                // Add data cell to the frame
                dataFrame.Cells.Add(dataCell);
            }

            return dataFrame;
        }

        #endregion
    }
}