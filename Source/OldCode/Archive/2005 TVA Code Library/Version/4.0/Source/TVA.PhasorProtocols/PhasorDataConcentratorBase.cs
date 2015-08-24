//*******************************************************************************************************
//  PhasorDataConcentratorBase.cs
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
//  05/26/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using TVA.Communication;
using TVA.Measurements;
using TVA.Measurements.Routing;
using TVA.PhasorProtocols.Anonymous;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents an <see cref="IActionAdapter"/> used to generate and transmit concentrated stream
    /// of phasor measurements in a specific phasor protocol.
    /// </summary>
    public abstract class PhasorDataConcentratorBase : ActionAdapterBase
    {
        #region [ Design Notes ]

        // We can't assume what devices will appear in the stream - therefore a device list is needed.
        // In addition, a list of active measurements needs to be loaded - that is, the measurements that
        // will make up the data in the device list.  Idealistically one could map any given number of
        // measurements to a device allowing a completely virtually defined device.

        // In the existing system the need for virtual devices was minimal - there is only one virtual
        // device, the EIRA PMU, which defines the calculated interconnection reference angle - this includes
        // an average interconnection frequency - hence you have a virtual device consisting entirely of
        // of composed measurement points. Normally you just want to retransmit the received device data
        // which is forwared as a cell in the combined outgoing data stream - this typically excludes any
        // digital or analog values - but there may be cases where this data should be retransmitted as well.

        // It is fairly straight forward to reverse the process of mapping device signals to measurements
        // using the existing signal references - this requires the definition of input devices to match
        // the definition of output devices. For ultimate flexibility however, you would allow any given
        // measurement to be mapped to a device definition created entirely by hand.

        // To further explore this idea, a normal case would be including a device in the outgoing data
        // stream as it is currently defined in the system. This would mean simply creating a measurement
        // list for this device based on its defined signal references - or one would just load the
        // measurements (filtered by need - i.e., excluding digitals and analogs if needed) for the device
        // as its currently defined. This seems fairly trivial - a simple check box to include an existing
        // device as-is in the outgoing data stream definition.

        // The interesting part will be tweaking the outgoing device definition - for simple definitions
        // the existing signal reference for a measurement will define its purpose in an outgoing device
        // device definition, but for ultimate flexibility any existing measurement can be mapped to a
        // any field in the device definition - this means the measurement will need a signal reference
        // that is defined when the measurement is mapped to the field - a new signal reference that exists
        // solely for this outgoing stream definition.

        // In the end a set of tables needs to exist that defines the outgoing data streams, the devices
        // that will appear in these streams (technically these do not need to already exist) and the
        // points that make up the field defintitions in these devices along with their signal references
        // that designate their destination field location - this will not necessarily be the perordained 
        // signal reference that was used to orginally map this field to a measurement - but rather an
        // outgoing data stream specific signal reference that exists for this measurement mapped into
        // this device.

        // This brings up an interesting notion - measurements in the system will not necessarily have a
        // one-to-one ratio with the outgoing field devices.  What this means is that a single measurement
        // point could be mapped to multiple locations within the same or multiple devices in any
        // variety of outgoing data streams. From a technical perspective as it relates to the measurement
        // concentration engine, a point will still have a destination frame based on its timestamp, but
        // it may have application at various locations (i.e., cell based devices) within that frame.

        // As a result a measurement will need to identify multiple destinations, that is, it may need to
        // track multiple signal references so that the measurement can be applied to all destination
        // field locations during the AssignMeasurementToFrame procedure of the data frame creation stage.

        // As the measurement is assigned to its destination frame by the concentration engine, the method
        // will need to loop through each signal reference assigned to the measurement. The method will then
        // obtain a reference to the data cell by its cell index and assign the measurement to the field
        // location based on the signal type and optional field index (e.g., phasor 1, 2, 3, etc.). This
        // should complete the operation of creating a data frame based on incoming measurements and leave
        // the data frame ready to publish in the next 1/30 of a second.

        // Suggested table definitions for the phasor data concentrator base class:

        //    - OutputStreams               Stream ID, Name, ID, Analog Count, Digital Count, etc.
        //    - OutputStreamPhasors         Device ID, Type (I or V), Name, Order, etc.
        //    - OutputStreamMeasurements    Device ID, MeasurementKey, Destination SignalReference

        // Proposed internal data structures used to collate information:

        // Protocol independent configuration frame that defines all output stream devices
        // Dictionary<MeasurementKey, SignalReference[]> <- multiple possible signal refs per measurement
        // SignalReference defines cell index of associated data cell and signal information

        #endregion

        #region [ Members ]

        // Fields
        private UdpServer m_dataChannel;
        private TcpServer m_commandChannel;
        private IConfigurationFrame m_configurationFrame;
        private ConfigurationFrame m_baseConfigurationFrame;
        private Dictionary<MeasurementKey, SignalReference[]> m_signalReferences;
        private LineFrequency m_nominalFrequency;
        private bool m_autoPublishConfigurationFrame;
        private bool m_autoStartDataChannel;
        private ushort m_idCode;
        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets ID code defined for this <see cref="PhasorDataConcentratorBase"/> parsed from the <see cref="ActionAdapterBase.ConnectionString"/>.
        /// </summary>
        public ushort IDCode
        {
            get
            {
                return m_idCode;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if configuration frame should be automatically published at the top
        /// of each minute on the data channel.
        /// </summary>
        /// <remarks>
        /// By default if no command channel is defined, this flag will be <c>true</c>; otherwise if command channel
        /// is defined the flag will be <c>false</c>.
        /// </remarks>
        public bool AutoPublishConfigurationFrame
        {
            get
            {
                return m_autoPublishConfigurationFrame;
            }
            set
            {
                m_autoPublishConfigurationFrame = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if concentrator will automatically start data channel.
        /// </summary>
        /// <remarks>
        /// By default data channel will be started automatically, setting this flag to <c>false</c> will
        /// allow alternate methods of enabling and disabling the real-time data stream (e.g., this can
        /// be used to allow a remote to device to enable and disable data stream if the protocol supports
        /// such commands).
        /// </remarks>
        public bool AutoStartDataChannel
        {
            get
            {
                return m_autoStartDataChannel;
            }
            set
            {
                m_autoStartDataChannel = value;
            }
        }

        /// <summary>
        /// Gets the nominal <see cref="LineFrequency"/> defined for this <see cref="PhasorDataConcentratorBase"/> parsed from the <see cref="ActionAdapterBase.ConnectionString"/>.
        /// </summary>
        public LineFrequency NominalFrequency
        {
            get
            {
                return m_nominalFrequency;
            }
        }

        /// <summary>
        /// Gets or sets the protocol specific <see cref="IConfigurationFrame"/> used to send to clients for protocol parsing.
        /// </summary>
        public virtual IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
            set
            {
                m_configurationFrame = value;
            }
        }

        /// <summary>
        /// Gets the protocol independent <see cref="Anonymous.ConfigurationFrame"/> defined for this <see cref="PhasorDataConcentratorBase"/>.
        /// </summary>
        public ConfigurationFrame BaseConfigurationFrame
        {
            get
            {
                return m_baseConfigurationFrame;
            }
        }
        
        /// <summary>
        /// Gets or sets reference to <see cref="UdpServer"/> data channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected UdpServer DataChannel
        {
            get
            {
                return m_dataChannel;
            }
            set
            {
                if (m_dataChannel != null)
                {
                    // Detach from events on existing data channel reference
                    m_dataChannel.SendClientDataException -= m_dataChannel_SendClientDataException;
                    m_dataChannel.ServerStarted -= m_dataChannel_ServerStarted;
                    m_dataChannel.ServerStopped -= m_dataChannel_ServerStopped;

                    if (m_dataChannel != value)
                        m_dataChannel.Dispose();
                }

                // Assign new data channel reference
                m_dataChannel = value;

                if (m_dataChannel != null)
                {
                    // Attach to events on new data channel reference
                    m_dataChannel.SendClientDataException += m_dataChannel_SendClientDataException;
                    m_dataChannel.ServerStarted += m_dataChannel_ServerStarted;
                    m_dataChannel.ServerStopped += m_dataChannel_ServerStopped;
                }
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="TcpServer"/> command channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected TcpServer CommandChannel
        {
            get
            {
                return m_commandChannel;
            }
            set
            {
                if (m_commandChannel != null)
                {
                    // Detach from events on existing command channel reference
                    m_commandChannel.ClientConnected -= m_commandChannel_ClientConnected;
                    m_commandChannel.ClientDisconnected -= m_commandChannel_ClientDisconnected;
                    m_commandChannel.ReceiveClientDataComplete -= m_commandChannel_ReceiveClientDataComplete;
                    m_commandChannel.SendClientDataException -= m_commandChannel_SendClientDataException;
                    m_commandChannel.ServerStarted -= m_commandChannel_ServerStarted;
                    m_commandChannel.ServerStopped -= m_commandChannel_ServerStopped;

                    if (m_commandChannel != value)
                        m_commandChannel.Dispose();
                }

                // Assign new command channel reference
                m_commandChannel = value;

                if (m_commandChannel != null)
                {
                    // Attach to events on new command channel reference
                    m_commandChannel.ClientConnected += m_commandChannel_ClientConnected;
                    m_commandChannel.ClientDisconnected += m_commandChannel_ClientDisconnected;
                    m_commandChannel.ReceiveClientDataComplete += m_commandChannel_ReceiveClientDataComplete;
                    m_commandChannel.SendClientDataException += m_commandChannel_SendClientDataException;
                    m_commandChannel.ServerStarted += m_commandChannel_ServerStarted;
                    m_commandChannel.ServerStopped += m_commandChannel_ServerStopped;
                }
            }
        }

        /// <summary>
        /// Returns the detailed status of this <see cref="PhasorDataConcentratorBase"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);

                if (m_baseConfigurationFrame != null && m_baseConfigurationFrame.Cells != null)
                {
                    status.AppendFormat("  Total configured devices: {0}", m_baseConfigurationFrame.Cells.Count);
                    status.AppendLine();
                }

                if (m_signalReferences != null)
                {
                    status.AppendFormat(" Total device measurements: {0}", m_signalReferences.Count);
                    status.AppendLine();
                }

                status.AppendFormat(" Auto-publish config frame: {0}", m_autoPublishConfigurationFrame);
                status.AppendLine();
                status.AppendFormat("   Auto-start data channel: {0}", m_autoStartDataChannel);
                status.AppendLine();
                status.AppendFormat("       Data stream ID code: {0}", m_idCode);
                status.AppendLine();
                status.AppendFormat("         Nomimal frequency: {0}Hz", (int)m_nominalFrequency);
                status.AppendLine();

                if (m_dataChannel != null)
                    status.Append(m_dataChannel.Status);

                if (m_commandChannel != null)
                    status.Append(m_commandChannel.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PhasorDataConcentratorBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // Dispose and detach from data and command channel events
                        this.DataChannel = null;
                        this.CommandChannel = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="PhasorDataConcentratorBase"/>, if it is not already running.
        /// </summary>
        public override void Start()
        {
            // Start communications servers
            if (m_dataChannel != null && m_autoStartDataChannel)
                m_dataChannel.Start();

            if (m_commandChannel != null)
                m_commandChannel.Start();

            // Base action adapter gets started once data channel has been started (see m_dataChannel_ServerStarted)
            // so that the system doesn't attempt to start frame publication without an operational output data channel
            // when m_autoStartDataChannel is set to false.
        }

        /// <summary>
        /// Stops the <see cref="PhasorDataConcentratorBase"/>.
        /// </summary>
        public override void Stop()
        {
            // Stop concentration engine
            base.Stop();

            // Stop communications servers
            if (m_dataChannel != null)
                m_dataChannel.Stop();

            if (m_commandChannel != null)
                m_commandChannel.Stop();
        }
        
        /// <summary>
        /// Starts the <see cref="PhasorDataConcentratorBase"/> real-time data stream.
        /// </summary>
        /// <remarks>
        /// If <see cref="AutoStartDataChannel"/> is <c>false</c>, this method will allow host administrator
        /// to manually start the data channel, thus enabling the real-time data stream. If command channel
        /// is defined, it will be unaffected. 
        /// </remarks>
        [AdapterCommand("Manually starts the real-time data stream, command channel is unaffected.")]
        public virtual void StartDataChannel()
        {
            // Start data channel
            if (m_dataChannel != null)
                m_dataChannel.Start();
        }

        /// <summary>
        /// Starts the <see cref="PhasorDataConcentratorBase"/> real-time data stream.
        /// </summary>
        /// <remarks>
        /// This method will allow host administrator to manually stop the data channel, thus disabling
        /// the real-time data stream. If command channel is defined, it will be unaffected.
        /// </remarks>
        [AdapterCommand("Manually stops the real-time data stream, command channel is unaffected.")]
        public virtual void StopDataChannel()
        {
            // Stop concentration engine - this is done before stopping data channel since frames may still be
            // publishing while engine is stopping...
            base.Stop();

            // Stop data channel
            if (m_dataChannel != null)
                m_dataChannel.Stop();
        }

        /// <summary>
        /// Initializes <see cref="PhasorDataConcentratorBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting, commandChannel;

            // Load required parameters
            m_idCode = ushort.Parse(settings["IDCode"]);

            // Load optional parameters
            settings.TryGetValue("commandChannel", out commandChannel);

            if (settings.TryGetValue("autoPublishConfigFrame", out setting))
                m_autoPublishConfigurationFrame = setting.ParseBoolean();
            else
                m_autoPublishConfigurationFrame = !string.IsNullOrEmpty(commandChannel);

            if (settings.TryGetValue("autoStartDataChannel", out setting))
                m_autoStartDataChannel = setting.ParseBoolean();
            else
                m_autoStartDataChannel = true;

            if (settings.TryGetValue("nominalFrequency", out setting))
                m_nominalFrequency = (LineFrequency)int.Parse(setting);
            else
                m_nominalFrequency = LineFrequency.Hz60;

            // Initialize data channel
            this.DataChannel = new UdpServer(ConnectionString);

            // Initialize command channel if defined
            if (!string.IsNullOrEmpty(commandChannel))
                this.CommandChannel = new TcpServer(commandChannel);
            else
                this.CommandChannel = null;

            // Create the configuration frame
            UpdateConfiguration();
        }

        /// <summary>
        /// Update the configuration frame for this <see cref="PhasorDataConcentratorBase"/>.
        /// </summary>
        [AdapterCommand("Updates the phasor data concentrator configuration frame.")]
        public void UpdateConfiguration()
        {
            const int labelLength = 16;
            Dictionary<string, int> signalCellIndexes = new Dictionary<string, int>();
            ConfigurationCell cell;
            SignalReference signal;
            SignalReference[] signals;
            MeasurementKey measurementKey;
            PhasorType phasorType;
            AnalogType analogType;
            char phaseType;
            string label;
            int order;
            uint scale;
            double offset;

            // Define a protocol independent configuration frame
            m_baseConfigurationFrame = new ConfigurationFrame(m_idCode, DateTime.UtcNow.Ticks, (ushort)base.FramesPerSecond);

            // Define configuration cells (i.e., PMU's that will appear in outgoing data stream)
            foreach (DataRow deviceRow in DataSource.Tables["OutputStreams"].Select(string.Format("StreamID={0}", ID)))
            {
                try
                {
                    // Create a new configuration cell
                    cell = new ConfigurationCell(m_baseConfigurationFrame, ushort.Parse(deviceRow["ID"].ToString()), deviceRow["IsVirtual"].ToNonNullString("false").ParseBoolean());

                    // The base class defaults to floating-point, polar values, derived classes can change
                    cell.PhasorDataFormat = DataFormat.FloatingPoint;
                    cell.PhasorCoordinateFormat = CoordinateFormat.Polar;
                    cell.FrequencyDataFormat = DataFormat.FloatingPoint;
                    cell.AnalogDataFormat = DataFormat.FloatingPoint;

                    cell.IDLabel = deviceRow["Acronym"].ToString().Trim();
                    cell.StationName = deviceRow["Name"].ToString().TruncateRight(cell.MaximumStationNameLength).Trim();

                    // Define all the phasors configured for this device
                    foreach (DataRow phasorRow in DataSource.Tables["OutputStreamPhasors"].Select(string.Format("DeviceID={0}", cell.IDCode), "Order"))
                    {
                        order = int.Parse(phasorRow["Order"].ToNonNullString("0"));
                        label = phasorRow["Label"].ToNonNullString("Phasor " + order).Trim().RemoveDuplicateWhiteSpace().TruncateRight(labelLength - 4);
                        phasorType = phasorRow["PhasorType"].ToNonNullString("V").Trim().ToUpper().StartsWith("V") ? PhasorType.Voltage : PhasorType.Current;
                        phaseType = phasorRow["PhaseType"].ToNonNullString("+").Trim().ToUpper()[0];
                        
                        cell.PhasorDefinitions.Add(
                            new PhasorDefinition(
                                cell,
                                GeneratePhasorLabel(label, phaseType, phasorType),
                                phasorType,
                                null));
                    }

                    // Add frequency definition
                    label = string.Format("{0} Freq", cell.IDLabel.TruncateRight(labelLength - 5)).Trim();
                    cell.FrequencyDefinition = new FrequencyDefinition(cell, label);
                    
                    // Optionally define all the analogs configured for this device
                    if (DataSource.Tables.Contains("OutputStreamAnalogs"))
                    {
                        foreach (DataRow analogRow in DataSource.Tables["OutputStreamAnalogs"].Select(string.Format("DeviceID={0}", cell.IDCode), "Order"))
                        {
                            order = int.Parse(analogRow["Order"].ToNonNullString("0"));
                            label = analogRow["Label"].ToNonNullString("Analog " + order).Trim().RemoveDuplicateWhiteSpace().TruncateRight(labelLength);
                            scale = uint.Parse(analogRow["Scale"].ToNonNullString("1"));
                            offset = double.Parse(analogRow["Offset"].ToNonNullString("0.0"));
                            analogType = analogRow["AnalogType"].ToNonNullString("SinglePointOnWave").ConvertToType<AnalogType>();

                            cell.AnalogDefinitions.Add(
                                new AnalogDefinition(
                                    cell,
                                    label,
                                    scale,
                                    offset,
                                    analogType));
                        }
                    }

                    // Optionally define all the digitals configured for this device
                    if (DataSource.Tables.Contains("OutputStreamDigitals"))
                    {
                        foreach (DataRow digitalRow in DataSource.Tables["OutputStreamDigitals"].Select(string.Format("DeviceID={0}", cell.IDCode), "Order"))
                        {
                            order = int.Parse(digitalRow["Order"].ToNonNullString("0"));
                            label = digitalRow["Label"].ToNonNullString("Digital " + order).Trim().RemoveDuplicateWhiteSpace().TruncateRight(labelLength);

                            cell.DigitalDefinitions.Add(
                                new DigitalDefinition(
                                    cell,
                                    label));
                        }
                    }

                    m_baseConfigurationFrame.Cells.Add(cell);
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Failed to define output stream device \"{0}\" due to exception: {1}", deviceRow["Acronym"].ToString().Trim(), ex.Message), ex));
                }
            }

            OnStatusMessage("Defined {0} output stream devices...", m_baseConfigurationFrame.Cells.Count);

            // Create a new signal reference dictionary indexed on measurement keys
            m_signalReferences = new Dictionary<MeasurementKey, SignalReference[]>();
            
            // Define measurement to signals cross reference dictionary
            foreach (DataRow measurementRow in DataSource.Tables["OutputStreamMeasurements"].Select(string.Format("StreamID={0}", ID)))
            {
                try
                {
                    // Create a new signal reference
                    signal = new SignalReference(measurementRow["SignalReference"].ToString());

                    // Lookup cell index by acronym - doing this work upfront will save a huge amount
                    // of work during primary measurement sorting
                    if (!signalCellIndexes.TryGetValue(signal.Acronym, out signal.CellIndex))
                    {
                        // We cache these indicies locally to speed up initialization as we'll be
                        // requesting them for the same devices over and over
                        signal.CellIndex = m_configurationFrame.Cells.IndexOfIDLabel(signal.Acronym);
                        signalCellIndexes.Add(signal.Acronym, signal.CellIndex);
                    }

                    // Define measurement key
                    measurementKey = new MeasurementKey(uint.Parse(measurementRow["PointID"].ToString()), measurementRow["Historian"].ToString());

                    // It is possible, but not as common, that a measurement will have multiple destinations
                    // within an outgoing data stream frame, hence the following
                    if (m_signalReferences.TryGetValue(measurementKey, out signals))
                    {
                        // Add a new signal to existing collection
                        List<SignalReference> signalList = new List<SignalReference>(signals);
                        signalList.Add(signal);
                        m_signalReferences[measurementKey] = signalList.ToArray();
                    }
                    else
                    {
                        // Add new signal to new collection
                        signals = new SignalReference[1];
                        signals[0] = signal;
                        m_signalReferences.Add(measurementKey, signals);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Failed to associate measurement key to signal reference \"{0}\" due to exception: {1}", measurementRow["SignalReference"].ToString().Trim(), ex.Message), ex));
                }
            }

            // Assign action adapter input measurement keys
            InputMeasurementKeys = m_signalReferences.Keys.ToArray();

            // Create a new protocol specific configuration frame
            m_configurationFrame = CreateNewConfigurationFrame(m_baseConfigurationFrame);

            // Cache new protocol specific configuration frame
            CacheConfigurationFrame(m_configurationFrame);
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        public override void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            QueueMeasurementsForProcessing(new IMeasurement[] { measurement });
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            List<IMeasurement> inputMeasurements = new List<IMeasurement>();
            SignalReference[] signals;

            foreach (IMeasurement measurement in measurements)
            {
                // We assign signal reference to measurement in advance since we are using this as a filter
                // anyway, this will save a lookup later during measurement assignment to frame...
                if (m_signalReferences.TryGetValue(measurement.Key, out signals))
                {
                    // Loop through each signal reference defined for this measurement - this handles
                    // the case where there can be more than one destination for a measurement within
                    // an outgoing phasor data frame
                    foreach (SignalReference signal in signals)
                    {
                        // No need to queue this measurement unless it has a destination in the outgoing frame
                        if (signal.CellIndex > -1)
                            inputMeasurements.Add(new SignalReferenceMeasurement(measurement, signal));
                    }
                }
            }

            if (inputMeasurements.Count > 0)
                SortMeasurements(inputMeasurements);
        }

        /// <summary>
        /// Assign <see cref="IMeasurement"/> to its <see cref="IFrame"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> to assign <paramref name="measurement"/> to.</param>
        /// <param name="measurement"><see cref="IMeasurement"/> to assign to <paramref name="frame"/>.</param>
        /// <returns><c>true</c> if <see cref="IMeasurement"/> was successfully assigned to its <see cref="IFrame"/>.</returns>
        /// <remarks>
        /// In simple concentration scenarios all you need to do is assign a measurement to its frame based on
        /// time. In the case of a phasor data concentrator you need to assign a measurement to its particular
        /// location in its <see cref="IDataFrame"/> - so this method overrides the default behavior in order
        /// to accomplish this task.
        /// </remarks>
        protected override bool AssignMeasurementToFrame(IFrame frame, IMeasurement measurement)
        {
            // Make sure the measurement is a "SignalReferenceMeasurement" (it should be)
            SignalReferenceMeasurement signalMeasurement = measurement as SignalReferenceMeasurement;
            IDataFrame dataFrame = frame as IDataFrame;

            if (signalMeasurement != null && dataFrame != null)
            {
                PhasorValueCollection phasorValues;
                DigitalValueCollection digitalValues;
                AnalogValueCollection analogValues;
                SignalReference signal = signalMeasurement.SignalReference;
                IDataCell dataCell = dataFrame.Cells[signal.CellIndex];
                int signalIndex = signal.Index;

                // Assign measurement to its destination field in the data cell based on signal type
                switch (signal.Type)
                {
                    case SignalType.Angle:
                        // Assign "phase angle" measurement to data cell
                        phasorValues = dataCell.PhasorValues;
                        if (phasorValues.Count >= signalIndex)
                            phasorValues[signalIndex - 1].Angle = signalMeasurement.AdjustedValue;
                        break;
                    case SignalType.Magnitude:
                        // Assign "phase magnitude" measurement to data cell
                        phasorValues = dataCell.PhasorValues;
                        if (phasorValues.Count >= signalIndex)
                            phasorValues[signalIndex - 1].Magnitude = signalMeasurement.AdjustedValue;
                        break;
                    case SignalType.Frequency:
                        // Assign "frequency" measurement to data cell
                        dataCell.FrequencyValue.Frequency = signalMeasurement.AdjustedValue;
                        break;
                    case SignalType.DfDt:
                        // Assign "dF/dt" measurement to data cell
                        dataCell.FrequencyValue.DfDt = signalMeasurement.AdjustedValue;
                        break;
                    case SignalType.Status:
                        // Assign "common status flags" measurement to data cell
                        dataCell.CommonStatusFlags = unchecked((uint)signalMeasurement.AdjustedValue);
                        break;
                    case SignalType.Digital:
                        // Assign "digital" measurement to data cell
                        digitalValues = dataCell.DigitalValues;
                        if (digitalValues.Count >= signalIndex)
                                digitalValues[signalIndex - 1].Value = unchecked((ushort)signalMeasurement.AdjustedValue);
                        break;
                    case SignalType.Analog:
                        // Assign "analog" measurement to data cell
                        analogValues = dataCell.AnalogValues;
                        if (analogValues.Count >= signalIndex)
                            analogValues[signalIndex - 1].Value = signalMeasurement.AdjustedValue;
                        break;
                }

                // Track total measurements sorted for frame - this will become total measurements published
                frame.PublishedMeasurements++;

                return true;
            }

            // This is not expected to occur - but just in case
            OnProcessException(new InvalidCastException(string.Format("Attempt was made to assign an invalid measurement to phasor data concentration frame, expected a \"SignalReferenceMeasurement\" but got a \"{0}\"", measurement.GetType().Name)));

            return false;
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IDataFrame dataFrame = frame as IDataFrame;

            if (dataFrame != null)
            {
                // Send the configuration frame at the top of each minute if the class has been configured
                // to automatically publish the configuration frame over the data channel
                if (m_autoPublishConfigurationFrame && index == 0 && ((DateTime)dataFrame.Timestamp).Second == 0)
                {
                    // Publish binary image over data channel
                    m_configurationFrame.Timestamp = dataFrame.Timestamp;
                    m_dataChannel.MulticastAsync(m_configurationFrame.BinaryImage);
                }

                // Publish data frame binary image over data channel
                m_dataChannel.MulticastAsync(dataFrame.BinaryImage);
            }
        }

        /// <summary>
        /// Handles incoming commands from devices connected over the command channel.
        /// </summary>
        /// <param name="clientID">Guid of client that sent the command.</param>
        /// <param name="commandBuffer">Data buffer received from connected client device.</param>
        /// <param name="length">Valid length of data within the buffer.</param>
        /// <remarks>
        /// This method should be overriden by derived classes in order to handle incoming commands,
        /// specifically handling requests for configuration frames.
        /// </remarks>
        protected virtual void DeviceCommandHandler(Guid clientID, byte[] commandBuffer, int length)
        {
            // This is optionally overridden to handle incoming data - such as IEEE commands
        }

        /// <summary>
        /// Creates a new protocol specific <see cref="IConfigurationFrame"/> based on provided protocol independent <paramref name="baseConfigurationFrame"/>.
        /// </summary>
        /// <param name="baseConfigurationFrame">Protocol independent <paramref name="IConfigurationFrame"/>.</param>
        /// <returns>A new protocol specific <see cref="IConfigurationFrame"/>.</returns>
        /// <remarks>
        /// Derived classes should notify consumers of change in configuration if system is active when
        /// new configuration frame is created if outgoing protocol allows such a notfication.
        /// </remarks>
        protected abstract IConfigurationFrame CreateNewConfigurationFrame(ConfigurationFrame baseConfigurationFrame);

        /// <summary>
        /// Serialize configuration frame to cache folder for later use (if needed).
        /// </summary>
        /// <param name="configurationFrame">New <see cref="IConfigurationFrame"/> to cache.</param>
        /// <remarks>
        /// Derived concentrators can call this method to manually serialize their protocol specific
        /// configuration frames. Note that after initial call to <see cref="CreateNewConfigurationFrame"/>
        /// this method will be call automatically.
        /// </remarks>
        protected void CacheConfigurationFrame(IConfigurationFrame configurationFrame)
		{		
			// Cache configuration frame for reference
			OnStatusMessage("Caching configuration frame...");
			
            // Cache configuration on an independent thread in case this takes some time
            ThreadPool.QueueUserWorkItem(TVA.PhasorProtocols.Anonymous.ConfigurationFrame.Cache,
                new EventArgs<IConfigurationFrame, Action<Exception>, string>(configurationFrame, OnProcessException, Name));
        }

        #region [ Data Channel Event Handlers ]

        private void m_dataChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;
            OnProcessException(new InvalidOperationException(string.Format("Data channel exception occurred while sending client data: {0}", ex.Message), ex));
        }

        private void m_dataChannel_ServerStarted(object sender, EventArgs e)
        {
            // Start concentration engine
            base.Start();

            OnStatusMessage("Data channel started.");
        }

        private void m_dataChannel_ServerStopped(object sender, EventArgs e)
        {
            OnStatusMessage("Data channel stopped.");
        }

        #endregion

        #region [ Command Channel Event Handlers ]

        private void m_commandChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            OnStatusMessage("Client connected to command channel.");
        }

        private void m_commandChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            OnStatusMessage("Client disconnected from command channel.");
        }

        private void m_commandChannel_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            DeviceCommandHandler(e.Argument1, e.Argument2, e.Argument3);
        }

        private void m_commandChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;
            OnProcessException(new InvalidOperationException(string.Format("Command channel exception occurred while sending client data: {0}", ex.Message), ex));
        }

        private void m_commandChannel_ServerStarted(object sender, EventArgs e)
        {
            OnStatusMessage("Command channel started.");
        }

        private void m_commandChannel_ServerStopped(object sender, EventArgs e)
        {
            OnStatusMessage("Command channel stopped.");
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Methods

        // Generate a descriptive phasor label
        private static string GeneratePhasorLabel(string phasorlabel, char phaseType, PhasorType phasorType)
        {
            StringBuilder label = new StringBuilder();

            label.Append(phasorlabel);

            switch (phaseType)
            {
                case '+':   // Positive sequence
                    label.Append(" +S");
                    break;
                case '-':   // Negative sequence
                    label.Append(" -S");
                    break;
                case '0':   // Zero sequence
                    label.Append(" 0S");
                    break;
                case 'A':   // A-Phase
                    label.Append(" AP");
                    break;
                case 'B':   // B-Phase
                    label.Append(" BP");
                    break;
                case 'C':   // C-Phase
                    label.Append(" CP");
                    break;
                default:    // Undetermined
                    label.Append(" ??");
                    break;
            }

            label.Append(phasorType == PhasorType.Voltage ? 'V' : 'I');

            return label.ToString();
        }

        #endregion
        
    }
}
