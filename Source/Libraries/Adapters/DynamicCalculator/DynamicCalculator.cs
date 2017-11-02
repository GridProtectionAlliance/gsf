//******************************************************************************************************
//  DynamicCalculator.cs - Gbtc
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
//  02/06/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Ciloci.Flee;
using GSF;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DynamicCalculator
{
    /// <summary>
    /// Represents the source of a timestamp.
    /// </summary>
    public enum TimestampSource
    {
        /// <summary>
        /// An incoming frame timestamp.
        /// </summary>
        Frame,

        /// <summary>
        /// Real-time as defined by the concentration engine.
        /// </summary>
        RealTime,

        /// <summary>
        /// The system's local clock.
        /// </summary>
        LocalClock
    }

    /// <summary>
    /// The DynamicCalculator is an action adapter which takes multiple
    /// input measurements and performs a calculation on those measurements
    /// to generate its own calculated measurement.
    /// </summary>
    [Description("Dynamic Calculator: Performs arithmetic operations on multiple input signals")]
    public class DynamicCalculator : ActionAdapterBase
    {
        #region [ Members ]

        // Nested Types
        private class DelayedSynchronizedOperation : SynchronizedOperationBase
        {
            private Action m_delayedAction;

            public DelayedSynchronizedOperation(Action action, Action<Exception> exceptionAction)
                : base(action, exceptionAction)
            {
                m_delayedAction = () =>
                {
                    if (ExecuteAction())
                        ExecuteActionAsync();
                };
            }

            public int Delay { get; set; }

            protected override void ExecuteActionAsync()
            {
                m_delayedAction.DelayAndExecute(Delay);
            }
        }

        // Fields
        private string m_expressionText;
        private string m_variableList;
        private string m_imports;
        private bool m_supportsTemporalProcessing;
        private bool m_skipNanOutput;
        private bool m_useConcentrator;
        private TimestampSource m_timestampSource;

        private readonly ImmediateMeasurements m_latestMeasurements;
        private Ticks m_latestTimestamp;

        private readonly HashSet<string> m_variableNames;
        private readonly Dictionary<MeasurementKey, string> m_keyMapping;
        private readonly SortedDictionary<int, string> m_nonAliasedTokens;

        private string m_aliasedExpressionText;
        private readonly ExpressionContext m_expressionContext;
        private IDynamicExpression m_expression;

        private DelayedSynchronizedOperation m_timerOperation;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DynamicCalculator"/>.
        /// </summary>
        public DynamicCalculator()
        {
            m_latestMeasurements = new ImmediateMeasurements();
            m_latestMeasurements.RealTimeFunction = () => RealTime;

            m_variableNames = new HashSet<string>();
            m_keyMapping = new Dictionary<MeasurementKey, string>();
            m_nonAliasedTokens = new SortedDictionary<int, string>();
            m_expressionContext = new ExpressionContext();

            m_timerOperation = new DelayedSynchronizedOperation(ProcessLatestMeasurements, ex => OnProcessException(MessageLevel.Error, ex));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the textual representation of the expression.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the arithmetic expression used to perform calculations.")]
        public string ExpressionText
        {
            get
            {
                return m_expressionText;
            }
            set
            {
                m_expressionText = value;
                PerformAliasReplacement();
            }
        }

        /// <summary>
        /// Gets or sets the list of variables used in the expression.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the app-domain unique list of variables used in the expression. Any defined aliased variables must be unique per defined dynamic calculator or e-mail notifier instance")]
        public string VariableList
        {
            get
            {
                return m_variableList;
            }
            set
            {
                string keyList;

                if (m_variableList != value)
                {
                    // Store the variable list for get operations
                    m_variableList = value;

                    // Empty the collection of variable names
                    m_variableNames.Clear();
                    m_keyMapping.Clear();
                    m_nonAliasedTokens.Clear();

                    // If the value is null, do not attempt to process it
                    if ((object)value == null)
                        return;

                    // Build the collection of variable names with the new value
                    foreach (string token in value.Split(';'))
                        AddVariable(token);

                    // Perform alias replacement on tokens that were not explicitly aliased
                    PerformAliasReplacement();

                    // Build the key list which will define the input measurements for this adapter
                    keyList = m_keyMapping.Keys.Select(key => key.ToString())
                        .Aggregate((runningKeyList, nextKey) => runningKeyList + ";" + nextKey);

                    // Set the input measurements for this adapter
                    InputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(DataSource, true, keyList);
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of types which define
        /// methods to be imported into the expression parser.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the list of types which define methods to be imported into the expression parser."),
        DefaultValue("AssemblyName={mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}, TypeName=System.Math")]
        public string Imports
        {
            get
            {
                return m_imports;
            }
            set
            {
                foreach (string typeDef in value.Split(';'))
                {
                    try
                    {
                        Dictionary<string, string> parsedTypeDef = typeDef.ParseKeyValuePairs(',');
                        string assemblyName = parsedTypeDef["assemblyName"];
                        string typeName = parsedTypeDef["typeName"];
                        Assembly asm = Assembly.Load(new AssemblyName(assemblyName));
                        Type t = asm.GetType(typeName);

                        m_expressionContext.Imports.AddType(t);
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Error, new ArgumentException($"Unable to load type from assembly: {typeDef}", ex));
                    }
                }

                m_imports = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter supports temporal processing."),
        DefaultValue(false)]
        public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

        /// <summary>
        /// Gets or sets the flag indicating whether to concentrate
        /// incoming data or to just use the latest received values.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter should concentrate incoming data."),
        DefaultValue(true)]
        public bool UseConcentrator
        {
            get
            {
                return m_useConcentrator;
            }
            set
            {
                m_useConcentrator = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval at which the adapter should calculate values.
        /// </summary>
        /// <remarks>
        /// Set to zero to disable the timer and calculate values upon receipt of input data.
        /// </remarks>
        [ConnectionStringParameter,
        Description("Define the interval, in seconds, at which the adapter should calculate values."),
        DefaultValue(0)]
        public double CalculationInterval
        {
            get
            {
                return m_timerOperation.Delay / 1000.0D;
            }
            set
            {
                m_timerOperation.Delay = (int)(value * 1000.0D);
            }
        }

        /// <summary>
        /// Gets or sets the source of the timestamps of the calculated values.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the source of the timestamps of the calculated values."),
        DefaultValue(TimestampSource.Frame)]
        public TimestampSource TimestampSource
        {
            get
            {
                return m_timestampSource;
            }
            set
            {
                m_timestampSource = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the dynamic calculator expects.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines primary keys of input measurements the dynamic calculator expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public override MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return base.InputMeasurementKeys;
            }
            set
            {
                base.InputMeasurementKeys = value;
                m_latestMeasurements.ClearMeasurementCache();
            }
        }

        private new Ticks RealTime
        {
            get
            {
                switch (m_timestampSource)
                {
                    default:
                        return m_latestTimestamp;

                    case TimestampSource.LocalClock:
                        return DateTime.UtcNow;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="DynamicCalculator"/>.
        /// </summary>
        public override void Initialize()
        {
            string errorMessage = "{0} is missing from Settings - Example: expressionText=x+y; variableList={{x = PPA:1; y = PPA:2}}";

            Dictionary<string, string> settings;
            string setting;

            settings = Settings;

            // Load useConcentrator setting before other parameters in case we
            // need to inject defaults for FramesPerSecond, LagTime, and LeadTime
            if (settings.TryGetValue("useConcentrator", out setting))
                m_useConcentrator = setting.ParseBoolean();
            else
                m_useConcentrator = true;

            if (!m_useConcentrator)
            {
                if (!settings.ContainsKey("FramesPerSecond"))
                    settings.Add("FramesPerSecond", "30");

                if (!settings.ContainsKey("LagTime"))
                    settings.Add("LagTime", "3");

                if (!settings.ContainsKey("LeadTime"))
                    settings.Add("LeadTime", "1");
            }

            base.Initialize();

            if (OutputMeasurements?.Length != 1)
                throw new ArgumentException($"Exactly one output measurement must be defined. Amount defined: {OutputMeasurements?.Length ?? 0}");

            // Load required parameters

            if (!settings.TryGetValue("expressionText", out setting))
                throw new ArgumentException(string.Format(errorMessage, "expressionText"));

            ExpressionText = settings["expressionText"];

            if (!settings.TryGetValue("variableList", out setting))
                throw new ArgumentException(string.Format(errorMessage, "variableList"));

            VariableList = settings["variableList"];

            // Load optional parameters

            if (settings.TryGetValue("imports", out setting))
                Imports = setting;
            else
                Imports = "AssemblyName={mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}, TypeName=System.Math";

            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = false;

            // When skipNanOutput is true, then any output measurement which 
            // would have a value of NaN is skipped.
            // This prevents the NaN outputs that could otherwise occur when some inputs to the calculation
            // are at widely differing periods.
            if (settings.TryGetValue("skipNanOutput", out setting))
                m_skipNanOutput = setting.ParseBoolean();
            else
                m_skipNanOutput = false; //default to previous behavior

            if (settings.TryGetValue("timestampSource", out setting))
                m_timestampSource = (TimestampSource)Enum.Parse(typeof(TimestampSource), setting);
            else
                m_timestampSource = TimestampSource.Frame;

            if (settings.TryGetValue("publicationInterval", out setting))
                CalculationInterval = double.Parse(setting);
            else
                CalculationInterval = 0;

            m_latestMeasurements.LagTime = LagTime;
            m_latestMeasurements.LeadTime = LeadTime;
        }

        /// <summary>
        /// Starts the <see cref="DynamicCalculator"/> or restarts it if it is already running.
        /// </summary>
        [AdapterCommand("Starts the action adapter or restarts it if it is already running.", "Administrator", "Editor")]
        public override void Start()
        {
            base.Start();

            if (!m_useConcentrator && m_timerOperation.Delay > 0)
                m_timerOperation.RunOnceAsync();
        }

        /// <summary>
        /// Queues a collection of measurements for processing. Measurements are automatically filtered to the defined <see cref="IAdapter.InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// Measurements are filtered against the defined <see cref="IAdapter.InputMeasurementKeys"/>.
        /// </remarks>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (m_useConcentrator)
                base.QueueMeasurementsForProcessing(measurements);
            else
                ProcessMeasurements(measurements);
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            long timestamp;

            // Get the timestamp of the measurement to be generated
            switch (m_timestampSource)
            {
                default:
                    timestamp = frame.Timestamp;
                    break;

                case TimestampSource.RealTime:
                    timestamp = base.RealTime;
                    break;

                case TimestampSource.LocalClock:
                    timestamp = DateTime.UtcNow.Ticks;
                    break;
            }

            Calculate(timestamp, measurements);
        }

        private void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
            {
                m_latestMeasurements.UpdateMeasurementValue(measurement);

                if (measurement.Timestamp > m_latestTimestamp)
                    m_latestTimestamp = measurement.Timestamp;
            }

            if (m_timerOperation.Delay <= 0)
                ProcessLatestMeasurements();
        }

        private void ProcessLatestMeasurements()
        {
            if (!Enabled)
                return;

            IDictionary<MeasurementKey, IMeasurement> measurementLookup = m_latestMeasurements
                .Cast<IMeasurement>()
                .ToDictionary(measurement => measurement.Key);

            Calculate(RealTime, measurementLookup);

            if (m_timerOperation.Delay > 0)
                m_timerOperation.RunOnceAsync();
        }

        private void Calculate(Ticks timestamp, IDictionary<MeasurementKey, IMeasurement> measurements)
        {
            IMeasurement measurement;

            m_expressionContext.Variables.Clear();

            // Set the values of variables in the expression
            foreach (MeasurementKey key in m_keyMapping.Keys)
            {
                string name = m_keyMapping[key];

                if (measurements.TryGetValue(key, out measurement))
                    m_expressionContext.Variables[name] = measurement.AdjustedValue;
                else
                    m_expressionContext.Variables[name] = double.NaN;
            }

            // Compile the expression if it has not been compiled already
            if ((object)m_expression == null)
                m_expression = m_expressionContext.CompileDynamic(m_aliasedExpressionText);

            // Evaluate the expression and generate the measurement
            GenerateCalculatedMeasurement(timestamp, m_expression.Evaluate() as IConvertible);
        }

        // Adds a variable to the key-variable map.
        private void AddVariable(string token)
        {
            // This determines whether the variable has been
            // explicitly aliased or not and then delegates
            // the work to the appropriate helper method
            if (token.Contains('='))
                AddAliasedVariable(token);
            else
                AddNotAliasedVariable(token);
        }

        // Adds an explicitly aliased variable to the key-variable map.
        private void AddAliasedVariable(string token)
        {
            string[] splitToken = token.Split('=');
            MeasurementKey key;
            string alias;

            if (splitToken.Length > 2)
                throw new FormatException($"Too many equals signs: {token}");

            key = GetKey(splitToken[1].Trim());
            alias = splitToken[0].Trim();
            AddMapping(key, alias);
        }

        // Adds a variable to the key-variable map which has not been explicitly aliased.
        private void AddNotAliasedVariable(string token)
        {
            string alias;
            MeasurementKey key;

            token = token.Trim();
            m_nonAliasedTokens.Add(-token.Length, token);

            key = GetKey(token);
            alias = token.ReplaceCharacters('_', c => !char.IsLetterOrDigit(c));

            // Ensure that the generated alias is unique
            while (m_variableNames.Contains(alias))
                alias += "_";

            AddMapping(key, alias);
        }

        // Adds the given mapping to the key-variable map.
        private void AddMapping(MeasurementKey key, string alias)
        {
            if (m_variableNames.Contains(alias))
                throw new ArgumentException($"Variable name is not unique: {alias}");

            m_variableNames.Add(alias);
            m_keyMapping.Add(key, alias);
        }

        // Performs alias replacement on tokens that were not explicitly aliased.
        private void PerformAliasReplacement()
        {
            StringBuilder aliasedExpressionTextBuilder = new StringBuilder(m_expressionText);
            MeasurementKey key;
            string alias;

            foreach (string token in m_nonAliasedTokens.Values)
            {
                key = GetKey(token);
                alias = m_keyMapping[key];
                aliasedExpressionTextBuilder.Replace(token, alias);
            }

            m_aliasedExpressionText = aliasedExpressionTextBuilder.ToString();
        }

        // Gets a measurement key based on a token which
        // may be either a signal ID or measurement key.
        private MeasurementKey GetKey(string token)
        {
            Guid signalID;
            MeasurementKey key;

            if (Guid.TryParse(token, out signalID))
            {
                // Defined using the measurement's GUID
                key = MeasurementKey.LookUpBySignalID(signalID);
            }
            else
            {
                // Defined using the measurement's key
                key = MeasurementKey.Parse(token);
            }

            return key;
        }

        // Generates a measurement with the given value and sends it into the system
        private void GenerateCalculatedMeasurement(long timestamp, IConvertible value)
        {
            IMeasurement calculatedMeasurement;

            if ((object)value == null)
                throw new InvalidOperationException("Calculation must not return a type that does not convert to double.");

            calculatedMeasurement = Measurement.Clone(OutputMeasurements[0], Convert.ToDouble(value), timestamp);
            OnNewMeasurement(calculatedMeasurement);
        }

        // Helper method to raise the NewMeasurements event
        // when only a single measurement is to be provided.
        private void OnNewMeasurement(IMeasurement measurement)
        {
            // skip processing of an output with a value of NaN unless configured to process NaN outputs
            if (!m_skipNanOutput || !double.IsNaN(measurement.Value))
                OnNewMeasurements(new[] { measurement });
        }

        #endregion
    }
}
