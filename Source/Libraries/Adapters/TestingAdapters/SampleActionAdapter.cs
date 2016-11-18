//******************************************************************************************************
//  SampleActionAdapter.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/06/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GSF;
using GSF.Data;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;

// Define PhasorMeasurements as pair of measurement keys representing an angle and a magnitude, i.e., Tuple<MeasurementKey, MeasurementKey>:
using PhasorMeasurements = System.Tuple<GSF.TimeSeries.MeasurementKey, GSF.TimeSeries.MeasurementKey>;

// Define LineMeasurements as pair of PhasorMeasurements representing measurement keys for a voltage and an associated current phasor:
using LineMeasurements = System.Tuple</* PhasorMeasurements, PhasorMeasurements >*/
        System.Tuple<GSF.TimeSeries.MeasurementKey, GSF.TimeSeries.MeasurementKey>, 
        System.Tuple<GSF.TimeSeries.MeasurementKey, GSF.TimeSeries.MeasurementKey>>;

namespace TestingAdapters
{
    /// <summary>
    /// Defines a sample action adapter for reference.
    /// </summary>
    [Description("Sample Action: Example action adapter code")]
    class SampleActionAdapter : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const double SqrtOf3 = 1.7320508075688772935274463415059D;

        // Fields
        private LineMeasurements[] m_lineMeasurements;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets an alternate database connection to use to get configuration information.
        /// </summary>
        [Description(
            "Defines the needed parameters for an alternate database connection used to get configuration information. Example: " +
            "alternateDatabaseConnection={connectionString={Data Source=localhost\\SQLEXPRESS; Initial Catalog=AltDB; Integrated Security=SSPI}; " +
            "dataProviderString={AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; " +
            "ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter}}")]
        [DefaultValue("")]
        public string AlternateDatabaseConnection { get; set;  }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="ActionAdapterBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string alternateDatabaseConnection;
            AdoDataConnection connection = null;

            try
            {
                // Load optional parameters
                if (settings.TryGetValue("AlternateDatabaseConnection", out alternateDatabaseConnection) && !string.IsNullOrEmpty(alternateDatabaseConnection))
                {
                    string connectionString, dataProviderString;
                    settings = alternateDatabaseConnection.ParseKeyValuePairs();
                    settings.TryGetValue("ConnectionString", out connectionString);
                    settings.TryGetValue("DataProviderString", out dataProviderString);

                    if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(dataProviderString))
                        connection = new AdoDataConnection(connectionString, dataProviderString);
                }

                // Fall back on current openPDC database if alternate connection was not provided
                if ((object)connection == null)
                    connection = new AdoDataConnection("systemSettings");

                // This example assumes stored procedure returns a table with a column named ID that is a Guid, each row representing an input measurement
                MeasurementKey[] inputKeys = connection.RetrieveData("GetMyInputsProc @param1 @param2", "param1Value", "param2Value").Rows.OfType<DataRow>()
                    .Select(row => MeasurementKey.LookUpBySignalID(Guid.Parse(row["ID"].ToString()))).ToArray();

                // Lookup signal types for defined input measurements
                SignalType[] inputTypes = inputKeys.Select(key => LookupSignalType(key)).ToArray();

                // Segregate inputs into voltage and current phasor measurements
                MeasurementKey[] voltageAngles = inputKeys.Where((key, index) => inputTypes[index] == SignalType.VPHA).ToArray();
                MeasurementKey[] voltageMagnitudes = inputKeys.Where((key, index) => inputTypes[index] == SignalType.VPHM).ToArray();
                MeasurementKey[] currentAngles = inputKeys.Where((key, index) => inputTypes[index] == SignalType.IPHA).ToArray();
                MeasurementKey[] currentMagnitudes = inputKeys.Where((key, index) => inputTypes[index] == SignalType.IPHM).ToArray();

                // Validate input measurement types have quartic alignment
                if (voltageAngles.Length != currentAngles.Length || voltageAngles.Length != voltageMagnitudes.Length || currentAngles.Length != currentMagnitudes.Length)
                    throw new InvalidOperationException("A different number of voltage and current phasor input measurement keys were supplied - the current and voltage phasors inputs must be supplied in pairs to represent a line measurement set, i.e., one set of voltage phasor input measurements must be supplied for each current phasor input measurement in a consecutive sequence (e.g., VA1;VM1;IA1;IM1;  VA2;VM2;IA2;IM2; ... VAn;VMn;IAn;IMn)");

                // Reduce inputs to validated set of source measurements
                InputMeasurementKeys = voltageAngles.Concat(voltageMagnitudes).Concat(currentAngles).Concat(currentMagnitudes).Distinct().ToArray();

                // Store phasor pairs as line measurements
                PhasorMeasurements voltageMeasurements, currentMeasurements;
                m_lineMeasurements = new LineMeasurements[voltageAngles.Length];

                // Needed values for a line consist of measurements for voltage phasors and current phasors
                for (int i = 0; i < m_lineMeasurements.Length; i++)
                {
                    voltageMeasurements = new PhasorMeasurements(voltageAngles[i], voltageMagnitudes[i]);
                    currentMeasurements = new PhasorMeasurements(currentAngles[i], currentMagnitudes[i]);
                    m_lineMeasurements[i] = new LineMeasurements(voltageMeasurements, currentMeasurements);
                }

                // Define output measurements as defined in stored procedure
                OutputMeasurements = connection.RetrieveData("GetMyOutputsProc @param1 @param2", "param1Value", "param2Value").Rows.OfType<DataRow>()
                    .Select(row => new Measurement() { Metadata = MeasurementKey.LookUpBySignalID(Guid.Parse(row["ID"].ToString())).Metadata }).ToArray();
            }
            finally
            {
                if ((object)connection != null)
                    connection.Dispose();
            }
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        /// <remarks>
        /// If user implemented publication function consistently exceeds available publishing time (i.e., <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds),
        /// concentration will fall behind. A small amount of this time is required by the <see cref="ConcentratorBase"/> for processing overhead, so actual total time
        /// available for user function process will always be slightly less than <c>1 / <see cref="ConcentratorBase.FramesPerSecond"/></c> seconds.
        /// </remarks>
        protected override void PublishFrame(IFrame frame, int index)
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            PhasorMeasurements voltageMeasurements, currentMeasurements;
            IMeasurement voltageAngle, voltageMagnitude, currentAngle, currentMagnitude;
            double[] results = new double[OutputMeasurements.Length];

            for (int i = 0; i < results.Length; i++)
                results[i] = double.NaN;

            for (int i = 0; i < m_lineMeasurements.Length; i++)
            {
                // Get measurement keys for voltage and current phasors
                voltageMeasurements = m_lineMeasurements[i].Item1;
                currentMeasurements = m_lineMeasurements[i].Item2;

                // Get angle and magnitude values of voltage and current phasors
                if (measurements.TryGetValue(voltageMeasurements.Item1, out voltageAngle) && 
                    measurements.TryGetValue(voltageMeasurements.Item2, out voltageMagnitude) && 
                    measurements.TryGetValue(currentMeasurements.Item1, out currentAngle) &&
                    measurements.TryGetValue(currentMeasurements.Item2, out currentMagnitude))
                {
                    // Do your calculation on measurements (in this example, a rough megawatt calculation)...
                    results[i] = 
                        (SqrtOf3 * voltageMagnitude.AdjustedValue) * currentMagnitude.AdjustedValue * 
                        Math.Cos(Angle.FromDegrees(currentAngle.AdjustedValue - voltageAngle.AdjustedValue)) / SI.Mega;
                }
            }

            // Publish results
            IMeasurement[] outputMeasurements = new IMeasurement[OutputMeasurements.Length];

            for (int i = 0; i < outputMeasurements.Length; i++)
                outputMeasurements[i] = Measurement.Clone(OutputMeasurements[i], results[i], frame.Timestamp);

            OnNewMeasurements(outputMeasurements);
        }

        // Lookup signal type for given measurement key
        private SignalType LookupSignalType(MeasurementKey key)
        {
            try
            {
                DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", key));

                if (filteredRows.Length > 0)
                    return (SignalType)Enum.Parse(typeof(SignalType), filteredRows[0]["SignalType"].ToString(), true);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to lookup signal type for measurement {0}: {1}", key, ex.Message), ex));
            }

            return SignalType.NONE;
        }

        #endregion
    }
}
