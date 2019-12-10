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
//       Initial version of source generated.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/04/2012 - J. Ritchie Carroll
//       Migrated to PhasorProtocolAdapters project.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Threading;
using GSF;
using GSF.IO;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.BPAPDCstream;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
namespace PhasorProtocolAdapters.BpaPdcStream
{
    /// <summary>
    /// Represents a BPA PDCstream phasor data concentrator.
    /// </summary>
    public class Concentrator : PhasorDataConcentratorBase
    {
        #region [ Members ]

        // Fields
        private ConfigurationFrame m_configurationFrame;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the INI based configuration file name of this <see cref="Concentrator"/>.
        /// </summary>
        public string IniFileName { get; set; }

        /// <summary>
        /// Returns the detailed status of this <see cref="Concentrator"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine("           Output protocol: BPA PDCstream");
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
            string errorMessage = "{0} is missing from Settings - Example: iniFileName=TESTSTREAM.ini";

            // Load required parameters
            if (!Settings.TryGetValue("iniFileName", out string setting))
                throw new ArgumentException(string.Format(errorMessage, "iniFileName"));

            IniFileName = FilePath.GetAbsolutePath(setting);

            // Start base class initialization
            base.Initialize();

            // BPA PDCstream always publishes config frame over data channel
            AutoPublishConfigurationFrame = true;
            CommandChannel = null;

            // Coordinate format and data format are fixed for BPA PDCstream outgoing streams for now
            CoordinateFormat = CoordinateFormat.Rectangular;
            DataFormat = DataFormat.FixedInteger;
        }

        /// <summary>
        /// Creates a new BPA PDCstream specific <see cref="IConfigurationFrame"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <see cref="GSF.PhasorProtocols.Anonymous.ConfigurationFrame"/>.</param>
        /// <returns>A new BPA PDCstream specific <see cref="IConfigurationFrame"/>.</returns>
        protected override IConfigurationFrame CreateNewConfigurationFrame(GSF.PhasorProtocols.Anonymous.ConfigurationFrame baseConfigurationFrame)
        {
            int count = 0;

            // Fix ID labels to use BPA PDCstream 4 character label
            foreach (GSF.PhasorProtocols.Anonymous.ConfigurationCell baseCell in baseConfigurationFrame.Cells)
            {
                baseCell.StationName = baseCell.IDLabel.TruncateLeft(baseCell.MaximumStationNameLength);
                baseCell.IDLabel = DataSource.Tables["OutputStreamDevices"].Select($"IDCode={baseCell.IDCode}")[0]["BpaAcronym"].ToNonNullString(baseCell.IDLabel).TruncateLeft(4);

                // If no ID label was provided, we default to first 4 characters of station name
                if (string.IsNullOrEmpty(baseCell.IDLabel))
                {
                    string stationName = baseCell.StationName;
                    string pmuID = count.ToString();

                    if (string.IsNullOrEmpty(stationName))
                        stationName = "PMU";

                    baseCell.IDLabel = stationName.Substring(0, 4 - pmuID.Length).ToUpper() + pmuID;
                }

                count++;
            }

            // Create a default INI file if one doesn't exist
            if (!File.Exists(IniFileName))
            {
                using (StreamWriter iniFile = File.CreateText(IniFileName))
                {
                    iniFile.Write(GSF.PhasorProtocols.BPAPDCstream.ConfigurationFrame.GetIniFileImage(baseConfigurationFrame));
                }
            }

            // Create a new BPA PDCstream configuration frame using base configuration
            ConfigurationFrame configurationFrame = new ConfigurationFrame(DateTime.UtcNow.Ticks, IniFileName, 1, RevisionNumber.Revision2, StreamType.Compact);

            foreach (GSF.PhasorProtocols.Anonymous.ConfigurationCell baseCell in baseConfigurationFrame.Cells)
            {
                // Create a new BPA PDCstream configuration cell (i.e., a PMU configuration)
                ConfigurationCell newCell = new ConfigurationCell(configurationFrame, baseCell.IDCode, NominalFrequency)
                {
                    // Update other cell level attributes
                    StationName = baseCell.StationName,
                    IDLabel = baseCell.IDLabel,
                    PhasorDataFormat = DataFormat.FixedInteger,
                    PhasorCoordinateFormat = CoordinateFormat.Rectangular,
                    FrequencyDataFormat = DataFormat.FixedInteger,
                    AnalogDataFormat = DataFormat.FixedInteger
                };

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

            // Setup new configuration cells with their proper INI file settings
            configurationFrame.Refresh(true);

            // Cache new BPA PDCstream for later use
            Interlocked.Exchange(ref m_configurationFrame, configurationFrame);

            return configurationFrame;
        }

        /// <summary>
        /// Creates a new BPA PDCstream specific <see cref="DataFrame"/> for the given <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">Timestamp for new <see cref="IFrame"/> in <see cref="Ticks"/>.</param>
        /// <returns>New BPA PDCstream <see cref="DataFrame"/> at given <paramref name="timestamp"/>.</returns>
        /// <remarks>
        /// Note that the <see cref="ConcentratorBase"/> class (which the <see cref="ActionAdapterBase"/> is derived from)
        /// is designed to sort <see cref="IMeasurement"/> implementations into an <see cref="IFrame"/> which represents
        /// a collection of measurements at a given timestamp. The <c>CreateNewFrame</c> method allows consumers to create
        /// their own <see cref="IFrame"/> implementations, in our case this will be a BPA PDCstream data frame.
        /// </remarks>
        protected override IFrame CreateNewFrame(Ticks timestamp)
        {
            // We create a new BPA PDCstream data frame based on current configuration frame
            ushort sampleNumber = (ushort)((timestamp.DistanceBeyondSecond() + 1.0D) / TicksPerFrame);

            DataFrame dataFrame = new DataFrame(timestamp, m_configurationFrame, 1, sampleNumber);

            foreach (ConfigurationCell configurationCell in m_configurationFrame.Cells)
            {
                // Create a new BPA PDCstream data cell (i.e., a PMU entry for this frame)
                DataCell dataCell = new DataCell(dataFrame, configurationCell, true);

                // Add data cell to the frame
                dataFrame.Cells.Add(dataCell);
            }

            return dataFrame;
        }

        #endregion
    }
}
