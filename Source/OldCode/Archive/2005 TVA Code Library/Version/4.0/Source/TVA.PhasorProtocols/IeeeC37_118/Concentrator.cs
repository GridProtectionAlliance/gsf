//*******************************************************************************************************
//  IeeeC37_118Concentrator.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/20/2007 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using TVA.Communication;
using TVA.Measurements;
using TVA.Measurements.Routing;

namespace TVA.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents an IEEE C37.118 phasor data concentrator.
    /// </summary>
	public class Concentrator : PhasorDataConcentratorBase
	{
        #region [ Members ]

        // Fields
        private ConfigurationFrame2 m_configurationFrame;
        private uint m_timeBase;
        private bool m_configurationChanged;
        private Ticks m_notificationStartTime;

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
            if (settings.TryGetValue("timebase", out setting))
                m_timeBase = uint.Parse(setting);
            else
                m_timeBase = 16777215;

            // Start base class initialization
            base.Initialize();
        }

        /// <summary>
        /// Creates a new IEEE C37.118 specific <see cref="IConfigurationFrame"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <paramref name="IConfigurationFrame"/>.</param>
        /// <returns>A new IEEE C37.118 specific <see cref="IConfigurationFrame"/>.</returns>
        protected override IConfigurationFrame CreateNewConfigurationFrame(TVA.PhasorProtocols.Anonymous.ConfigurationFrame baseConfigurationFrame)
        {
            ConfigurationCell newCell;

            // Create a new IEEE C37.118 configuration frame 2 using base configuration
            ConfigurationFrame2 configurationFrame = new ConfigurationFrame2(m_timeBase, baseConfigurationFrame.IDCode, DateTime.UtcNow.Ticks, baseConfigurationFrame.FrameRate);

            foreach (TVA.PhasorProtocols.Anonymous.ConfigurationCell baseCell in baseConfigurationFrame.Cells)
            {
                // Create a new IEEE C37.118 configuration cell (i.e., a PMU configuration)
                newCell = new ConfigurationCell(configurationFrame, baseCell.IDCode, base.NominalFrequency);

                // Update other cell level attributes
                newCell.StationName = baseCell.StationName;
                newCell.IDLabel = baseCell.IDLabel;
                newCell.PhasorDataFormat = baseCell.PhasorDataFormat;
                newCell.PhasorCoordinateFormat = baseCell.PhasorCoordinateFormat;
                newCell.FrequencyDataFormat = baseCell.FrequencyDataFormat;
                newCell.AnalogDataFormat = baseCell.AnalogDataFormat;
                newCell.Tag = baseCell.IsVirtual;

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
                    newCell.DigitalDefinitions.Add(new DigitalDefinition(newCell, digitalDefinition.Label));
                }

                // Add new PMU configuration (cell) to protocol specific configuration frame
                configurationFrame.Cells.Add(newCell);
            }

            bool configurationChanged = false;

            // After system has started any subsequent changes in configuration get indicated in the outgoing data stream
            if (m_configurationFrame != null)
                configurationChanged = true;

            // Cache new IEEE C7.118 for later use
            Interlocked.Exchange(ref m_configurationFrame, configurationFrame);

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
            DataFrame dataFrame = new DataFrame(timestamp, m_configurationFrame);
            DataCell dataCell;
            bool configurationChanged = false;

            if (m_configurationChanged)
            {
                // Change notifications should only last for one minute
                if ((DateTime.UtcNow.Ticks - m_notificationStartTime).ToSeconds() <= 60.0D)
                    configurationChanged = true;
                else
                    m_configurationChanged = false;
            }

            foreach (ConfigurationCell configurationCell in m_configurationFrame.Cells)
            {
                // Create a new IEEE C37.118 data cell (i.e., a PMU entry for this frame)
                dataCell = new DataCell(dataFrame, configurationCell, true);

                // Assume good status flags for virtual devices
                if ((bool)configurationCell.Tag)
                    dataCell.StatusFlags = 0;

                // Mark cells with configuration changed flag if configuration was reloaded
                if (configurationChanged)
                    dataCell.ConfigurationChangeDetected = true;

                // Add data cell to the frame
                dataFrame.Cells.Add(dataCell);
            }

            return dataFrame;
        }

        /// <summary>
        /// Handles incoming commands from devices connected over the command channel.
        /// </summary>
        /// <param name="clientID">Guid of client that sent the command.</param>
        /// <param name="commandBuffer">Data buffer received from connected client device.</param>
        /// <param name="length">Valid length of data within the buffer.</param>
        protected override void DeviceCommandHandler(Guid clientID, byte[] commandBuffer, int length)
        {
            try
            {
                // Interpret data received from a client as a command frame
                CommandFrame commandFrame = new CommandFrame(commandBuffer, 0, length);
                TcpServer commandChannel = CommandChannel;

                switch (commandFrame.Command)
                {
                    case DeviceCommand.SendConfigurationFrame1:
                    case DeviceCommand.SendConfigurationFrame2:
                        if (commandChannel != null)
                        {
                            commandChannel.SendToAsync(clientID, m_configurationFrame.BinaryImage);
                            OnStatusMessage("Received device command \"{0}\" - frame was returned.", commandFrame.Command);
                        }
                        break;
                    case DeviceCommand.SendHeaderFrame:
                        if (commandChannel != null)
                        {
                            HeaderFrame headerFrame = new HeaderFrame("IEEE C37.118 Concentrator Status:\r\n\r\n" + Status);
                            commandChannel.SendToAsync(clientID, headerFrame.BinaryImage);
                            OnStatusMessage("Received device command \"SendHeaderFrame\" - frame was returned.");
                        }
                        break;
                    case DeviceCommand.EnableRealTimeData:
                        // Only responding to stream control command if auto-start data channel is false
                        if (!AutoStartDataChannel)
                        {
                            StartDataChannel();
                            OnStatusMessage("Received device command \"EnableRealTimeData\" - concentrator real-time data stream was started.");
                        }
                        break;
                    case DeviceCommand.DisableRealTimeData:
                        // Only responding to stream control command if auto-start data channel is false
                        if (!AutoStartDataChannel)
                        {
                            StopDataChannel();
                            OnStatusMessage("Received device command \"DisableRealTimeData\" - concentrator real-time data stream was stopped.");
                        }
                        break;
                    default:
                        OnStatusMessage("Received unsupported device command: {0}", commandFrame.Command);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Encountered an exception while processing received client data: {0}", ex.Message), ex));
            }
        }

        #endregion
	}
}