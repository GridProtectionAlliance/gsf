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

        // Constants

        /// <summary>
        /// Defines the default value for <see cref="Imports"/> property.
        /// </summary>
        public const string DefaultImports = "AssemblyName={mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}, TypeName=System.Math";

        // Fields
        private string m_expressionText;
        private string m_variableList;
        private string m_imports;
        private bool m_supportsTemporalProcessing;
        private bool m_skipNanOutput;
        private bool m_useLatestValues;
        private double m_sentinelValue;
        private TimestampSource m_timestampSource;

        private readonly ImmediateMeasurements m_latestMeasurements;
        private Ticks m_latestTimestamp;

        private readonly HashSet<string> m_variableNames;
        private readonly Dictionary<MeasurementKey, string> m_keyMapping;
        private readonly SortedDictionary<int, string> m_nonAliasedTokens;

        private string m_aliasedExpressionText;
        private readonly ExpressionContext m_expressionContext;
        private IDynamicExpression m_expression;

        private readonly DelayedSynchronizedOperation m_timerOperation;

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
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines primary keys of output measurements the action adapter expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public override IMeasurement[] OutputMeasurements
        {
            get
            {
                return base.OutputMeasurements;
            }
            set
            {
                base.OutputMeasurements = value;
            }
        }

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
        DefaultValue(DefaultImports)]
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
                        Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
                        Type type = assembly.GetType(typeName);

                        m_expressionContext.Imports.AddType(type);
                    }
                    catch (Exception ex)
                    {
                        string message = $"Unable to load type from assembly: {typeDef}";
                        OnProcessException(MessageLevel.Error, new ArgumentException(message, ex));
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
        /// Gets or sets the flag indicating whether to use the latest
        /// received values to fill in values missing from the current frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter should use latest values for missing measurements."),
        DefaultValue(true)]
        public bool UseLatestValues
        {
            get
            {
                return m_useLatestValues;
            }
            set
            {
                m_useLatestValues = value;
            }
        }

        /// <summary>
        /// Gets or sets the value used when no other value can be determined for a variable.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the value used when no other value can be determined for a variable."),
        DefaultValue(double.NaN)]
        public double SentinelValue
        {
            get
            {
                return m_sentinelValue;
            }
            set
            {
                m_sentinelValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval at which the adapter should calculate values.
        /// </summary>
        /// <remarks>
        /// Set to zero to disable the timer and calculate values upon receipt of input data.
        /// </remarks>
        [ConnectionStringParameter,
        Description("Define the interval, in seconds, at which the adapter should calculate values. Zero value executes calculations at received data rate."),
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
        [EditorBrowsable(EditorBrowsableState.Never)]
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

        /// <summary>
        /// Gets flag that determines if the implementation of the <see cref="DynamicCalculator"/> requires an output measurement.
        /// </summary>
        protected virtual bool ExpectsOutputMeasurement => true;
        
        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("           Expression Text: {0}", ExpressionText);
                status.AppendLine();
                status.AppendFormat("             Variable List: {0}", VariableList);
                status.AppendLine();
                status.AppendFormat("         Use Latest Values: {0}", UseLatestValues);
                status.AppendLine();
                status.AppendFormat("      Calculation Interval: {0}", CalculationInterval == 0.0D ? "At received data rate" : $"{CalculationInterval:N3} seconds");
                status.AppendLine();
                status.AppendFormat("          Timestamp Source: {0}", TimestampSource);
                status.AppendLine();
                status.AppendFormat("            Sentinel Value: {0}", SentinelValue);
                status.AppendLine();

                List<string> imports = new List<string>();

                if (!string.IsNullOrWhiteSpace(Imports))
                {
                    foreach (string typeDef in Imports.Split(';'))
                    {
                        Dictionary<string, string> parsedTypeDef = typeDef.ParseKeyValuePairs(',');

                        if (parsedTypeDef.TryGetValue("typeName", out string typeName))
                            imports.Add(typeName);
                    }
                }

                if (imports.Count == 0)
                    imports.Add("None Defined");

                status.AppendFormat("       Imported .NET Types: {0}", string.Join(", ", imports));
                status.AppendLine();

                return status.ToString();
            }
        }

        private new Ticks RealTime
        {
            get
            {
                switch (m_timestampSource)
                {
                    case TimestampSource.RealTime:
                        return base.RealTime;

                    case TimestampSource.LocalClock:
                        return DateTime.UtcNow;

                    default:
                        return m_latestTimestamp;
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
            const string ErrorMessage = "{0} is missing from Settings - Example: expressionText=x+y; variableList={{x = PPA:1; y = PPA:2}}";

            Dictionary<string, string> settings;
            string setting;

            settings = Settings;
            base.Initialize();

            if (ExpectsOutputMeasurement && OutputMeasurements?.Length != 1)
                throw new ArgumentException($"Exactly one output measurement must be defined. Amount defined: {OutputMeasurements?.Length ?? 0}");

            // Load required parameters

            if (!settings.TryGetValue("variableList", out setting))
                throw new ArgumentException(string.Format(ErrorMessage, "variableList"));

            VariableList = settings["variableList"];

            if (!settings.TryGetValue("expressionText", out setting))
                throw new ArgumentException(string.Format(ErrorMessage, "expressionText"));

            ExpressionText = settings["expressionText"];

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

            if (settings.TryGetValue("useLatestValues", out setting))
                m_useLatestValues = setting.ParseBoolean();
            else
                m_useLatestValues = true;

            if (settings.TryGetValue("sentinelValue", out setting))
                m_sentinelValue = double.Parse(setting);
            else
                m_sentinelValue = double.NaN;

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

            if (m_useLatestValues && m_timerOperation.Delay > 0)
                m_timerOperation.RunOnceAsync();
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            m_latestTimestamp = frame.Timestamp;

            if (m_useLatestValues)
                ProcessMeasurements(frame.Measurements.Values);
            else
                Calculate(frame.Measurements);
        }

        /// <summary>
        /// Handler for the values calculated by the <see cref="DynamicCalculator"/>.
        /// </summary>
        /// <param name="value">The value calculated by the <see cref="DynamicCalculator"/>.</param>
        protected virtual void HandleCalculatedValue(object value)
        {
            // Evaluate the expression and generate the measurement
            GenerateCalculatedMeasurement(RealTime, value as IConvertible);
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

            Calculate(measurementLookup);

            if (m_timerOperation.Delay > 0)
                m_timerOperation.RunOnceAsync();
        }

        private void Calculate(IDictionary<MeasurementKey, IMeasurement> measurements)
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
                    m_expressionContext.Variables[name] = m_sentinelValue;
            }

            // Compile the expression if it has not been compiled already
            if ((object)m_expression == null)
                m_expression = m_expressionContext.CompileDynamic(m_aliasedExpressionText);

            // Evaluate the expression and generate the measurement
            HandleCalculatedValue(m_expression.Evaluate());
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

            return Guid.TryParse(token, out signalID)
                ? MeasurementKey.LookUpBySignalID(signalID)
                : MeasurementKey.Parse(token);
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
