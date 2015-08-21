//*******************************************************************************************************
//  ActionAdapterBase.cs
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
//  07/29/2006 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TVA.Collections;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents the base class for action adapters.
    /// </summary>
    /// <remarks>
    /// This base class acts as a measurement concentrator which time aligns all incoming measurements for proper processing.
    /// Derived classes are expected to override <see cref="ConcentratorBase.PublishFrame"/> to handle time aligned measurements
    /// and call <see cref="OnNewMeasurements"/> for any new measurements that may get created.
    /// </remarks>
    [CLSCompliant(false)]
	public abstract class ActionAdapterBase : ConcentratorBase, IActionAdapter
	{
        #region [ Members ]

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Provides new measurements from action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        // Fields
        private string m_name;
        private uint m_id;
        private string m_connectionString;
        private Dictionary<string, string> m_settings;
        private DataSet m_dataSource;
        private bool m_initialized;
        private MeasurementKey[] m_inputMeasurementKeys;
        private List<MeasurementKey> m_inputMeasurementKeysHash;
        private IMeasurement[] m_outputMeasurements;
        private int m_minimumMeasurementsToUse;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="ActionAdapterBase"/>.
        /// </summary>
        protected ActionAdapterBase()
            : base(30, 1.0D, 1.0D)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets key/value pair connection information specific to action adapter.
        /// </summary>
        /// <remarks>
        /// Example connection string format:<br/>
        /// expectedFramesPerSecond=30; lagTime=1.0; leadTime=0.5; minimumMeasurementsToUse=-1;
        /// useLocalClockAsRealTime=true; allowSortsByArrival=false;
        /// inputMeasurementKeys={P1:1245;P1:1247;P2:1335}; outputMeasurements={P3:1345,60.0,1.0;P3:1346;P3:1347}
        /// <br/>
        /// Elements in inputMeasurementKeys are defined as "ArchiveSource:MeasurementID"<br/>
        /// Elements in outputMeasurements are defined as "ArchiveSource:MeasurementID,Adder,Multiplier", the adder and multiplier are optional
        /// defaulting to 0.0 and 1.0 respectively.
        /// <br/>
        /// Note that expectedFramesPerSecond, lagTime and leadTime are required parameters, all other parameters are optional.
        /// </remarks>
        public virtual string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;

                // Preparse settings upon connection string assignment
                if (string.IsNullOrEmpty(m_connectionString))
                    m_settings = new Dictionary<string, string>();
                else
                    m_settings = m_connectionString.ParseKeyValuePairs();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="ActionAdapterBase"/>.
        /// </summary>
        public virtual DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        public virtual IMeasurement[] OutputMeasurements
        {
            get
            {
                return m_outputMeasurements;
            }
            set
            {
                m_outputMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the action adapter expects.
        /// </summary>
        public virtual MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return m_inputMeasurementKeys;
            }
            set
            {
                m_inputMeasurementKeys = value;

                // Update input key lookup hash table
                m_inputMeasurementKeysHash = new List<MeasurementKey>(value);
                m_inputMeasurementKeysHash.Sort();
            }
        }

        /// <summary>
        /// Gets or sets minimum number of input measurements required for adapter.  Set to -1 to require all.
        /// </summary>
        public virtual int MinimumMeasurementsToUse
        {
            get
            {
                // Default to all measurements if minimum is not specified
                if (m_minimumMeasurementsToUse < 1)
                    return InputMeasurementKeys.Length;
                
                return m_minimumMeasurementsToUse;
            }
            set
            {
                m_minimumMeasurementsToUse = value;
            }
        }

        /// <summary>
        /// Gets name of the action adapter.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets numeric ID associated with this action adapter.
        /// </summary>
        public virtual uint ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if the action adapter has been initialized successfully.
        /// </summary>
        public virtual bool Initialized
        {
            get
            {
                return m_initialized;
            }
            set
            {
                m_initialized = value;
            }
        }

        /// <summary>
        /// Gets settings <see cref="Dictionary{TKey,TValue}"/> parsed when <see cref="ConnectionString"/> was assigned.
        /// </summary>
        protected Dictionary<string, string> Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Returns the detailed status of the action adapter.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
			{
				const int MaxMeasurementsToShow = 6;
				
				StringBuilder status = new StringBuilder();

                status.AppendFormat("    Referenced data source: {0}, {1} tables", DataSource.DataSetName, DataSource.Tables.Count);
                status.AppendLine();
                status.AppendFormat("       Adapter initialized: {0}", Initialized);
                status.AppendLine();
                status.AppendFormat("         Operational state: {0}", Enabled ? "Running" : "Stopped");
                status.AppendLine();
                status.AppendFormat("                Adpater ID: {0}", ID);
                status.AppendLine();

                Dictionary<string, string> keyValuePairs = Settings;
                char[] keyChars;
                string value;

                status.AppendFormat("         Connection string: {0} key/value pairs", keyValuePairs.Count);
                //                            1         2         3         4         5         6         7
                //                   123456789012345678901234567890123456789012345678901234567890123456789012345678
                //                                         Key = Value
                //                                                        1         2         3         4         5
                //                                               12345678901234567890123456789012345678901234567890
                status.AppendLine();
                status.AppendLine();

                foreach (KeyValuePair<string, string> item in keyValuePairs)
                {
                    keyChars = item.Key.Trim().ToCharArray();
                    keyChars[0] = char.ToUpper(keyChars[0]);

                    value = item.Value.Trim();
                    if (value.Length > 50)
                        value = value.TruncateRight(47) + "...";

                    status.AppendFormat("{0} = {1}", (new string(keyChars)).TruncateRight(25).PadLeft(25), value.PadRight(50));
                    status.AppendLine();
                }

                status.AppendLine();
                status.Append("   Output measurement ID\'s: ");

				if (OutputMeasurements.Length <= MaxMeasurementsToShow)
				{
					status.Append(OutputMeasurements.ToDelimitedString(','));
				}
				else
				{
					IMeasurement[] outputMeasurements = new IMeasurement[MaxMeasurementsToShow];
					Array.Copy(OutputMeasurements, 0, outputMeasurements, 0, MaxMeasurementsToShow);
					status.Append(outputMeasurements.ToDelimitedString(','));
					status.Append(",...");
				}

				status.AppendLine();
				status.Append("    Input measurement ID\'s: ");
				
                if (InputMeasurementKeys.Length <= MaxMeasurementsToShow)
				{
					status.Append(InputMeasurementKeys.ToDelimitedString(','));
				}
				else
				{
					MeasurementKey[] inputMeasurements = new MeasurementKey[MaxMeasurementsToShow];
					Array.Copy(InputMeasurementKeys, 0, inputMeasurements, 0, MaxMeasurementsToShow);
					status.Append(inputMeasurements.ToDelimitedString(','));
					status.Append(",...");
				}

				status.AppendLine();
				status.Append(" Minimum measurements used: ");
				status.Append(MinimumMeasurementsToUse);
				status.AppendLine();
				status.Append(base.Status);

				return status.ToString();
			}
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="ActionAdapterBase"/>.
        /// </summary>
        public virtual void Initialize()
        {
            Dictionary<string, string> settings = Settings;
            string setting;

            // Load required parameters
            base.FramesPerSecond = int.Parse(settings["expectedFramesPerSecond"]);
            base.LagTime = double.Parse(settings["lagTime"]);
            base.LeadTime = double.Parse(settings["leadTime"]);

            // Load optional parameters
            if (settings.TryGetValue("useLocalClockAsRealTime", out setting))
                base.UseLocalClockAsRealTime = setting.ParseBoolean();

            if (settings.TryGetValue("allowSortsByArrival", out setting))
                base.AllowSortsByArrival = setting.ParseBoolean();

            if (settings.TryGetValue("inputMeasurementKeys", out setting))
                InputMeasurementKeys = ParseInputMeasurementKeys(setting);

            if (settings.TryGetValue("outputMeasurements", out setting))
                OutputMeasurements = ParseOutputMeasurements(setting);

            if (settings.TryGetValue("minimumMeasurementsToUse", out setting))
                MinimumMeasurementsToUse = int.Parse(setting);
        }

        /// <summary>
        /// Starts the <see cref="ActionAdapterBase"/>, if it is not already running.
        /// </summary>
        [AdapterCommand("Starts the action adapter, if it is not already running.")]
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Stops the <see cref="ActionAdapterBase"/>.
        /// </summary>
        [AdapterCommand("Stops the action adapter.")]
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="ActionAdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public virtual string GetShortStatus(int maxLength)
        {
            return string.Format("Total input measurements: {0}, total output measurements: {1}", InputMeasurementKeys.Length, OutputMeasurements.Length).PadLeft(maxLength);
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        /// <remarks>
        /// Measurement is filtered against the defined <see cref="InputMeasurementKeys"/>.
        /// </remarks>
        public virtual void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            // If this is an input measurement to this adapter, sort it!
            if (IsInputMeasurement(measurement.Key))
                SortMeasurement(measurement);
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// Measurements are filtered against the defined <see cref="InputMeasurementKeys"/>.
        /// </remarks>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            List<IMeasurement> inputMeasurements = new List<IMeasurement>();

            foreach (IMeasurement measurement in measurements)
            {
                if (IsInputMeasurement(measurement.Key))
                    inputMeasurements.Add(measurement);
            }

            if (inputMeasurements.Count > 0)
                SortMeasurements(inputMeasurements);
        }

        /// <summary>
        /// Determines if specified measurement key is defined in <see cref="InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="item">Primary key of measurement to find.</param>
        /// <returns>true if specified measurement key is defined in <see cref="InputMeasurementKeys"/>.</returns>
        public virtual bool IsInputMeasurement(MeasurementKey item)
        {
            return (m_inputMeasurementKeysHash.BinarySearch(item) >= 0);
        }

        /// <summary>
        /// Attempts to retrieve the minimum needed number of measurements from the frame (as specified by MinimumMeasurementsToUse)
        /// </summary>
        /// <param name="frame">Source frame for the measurements</param>
        /// <param name="measurements">Return array of measurements</param>
        /// <returns>True if minimum needed number of measurements were returned in measurement array</returns>
        /// <remarks>
        /// <para>
        /// Remember this function will *only* return the minimum needed number of measurements, no more.  If you want to use
        /// all available measurements in your adapter you should just use Frame.Measurements.Values directly.
        /// </para>
        /// <para>
        /// Note that the measurements array parameter will be created if the reference is null, otherwise if caller creates
        /// array it must be sized to MinimumMeasurementsToUse
        /// </para>
        /// </remarks>
        protected virtual bool TryGetMinimumNeededMeasurements(IFrame frame, ref IMeasurement[] measurements)
        {
            int index = 0, minNeeded = MinimumMeasurementsToUse;
            IDictionary<MeasurementKey, IMeasurement> frameMeasurements = frame.Measurements;
            MeasurementKey[] measurementKeys = InputMeasurementKeys;
            IMeasurement measurement;

            if (measurements == null || measurements.Length < minNeeded)
                measurements = new IMeasurement[minNeeded];

            // Loop through all input measurements to see if they exist in this frame
            for (int x = 0; x < measurementKeys.Length; x++)
            {
                if (frameMeasurements.TryGetValue(measurementKeys[x], out measurement))
                {
                    measurements[index++] = measurement;
                    if (index == minNeeded)
                        break;
                }
            }

            return (index == minNeeded);
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(status));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convienence.
        /// </remarks>
        protected virtual void OnStatusMessage(string formattedStatus, params object[] args)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
        }

        // Parse input measurement keys from connection string
        private static MeasurementKey[] ParseInputMeasurementKeys(string value)
        {
            List<MeasurementKey> keys = new List<MeasurementKey>();
            
            foreach (string item in value.Split(';'))
            {
                keys.Add(MeasurementKey.Parse(item));
            }

            return keys.ToArray();
        }

        // Parse output measurements from connection string
        private static IMeasurement[] ParseOutputMeasurements(string value)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();
            MeasurementKey key;
            string[] elem;
            double adder, multipler;

            foreach (string item in value.Split(';'))
            {
                elem = item.Trim().Split(',');

                key = MeasurementKey.Parse(elem[0]);

                if (elem.Length > 1)
                    adder = double.Parse(elem[1].Trim());
                else
                    adder = 0.0D;

                if (elem.Length > 2)
                    multipler = double.Parse(elem[2].Trim());
                else
                    multipler = 1.0D;

                measurements.Add(new Measurement(key.ID, key.Source, string.Empty, adder, multipler));
            }

            return measurements.ToArray();
        }

        #endregion		
	}
}