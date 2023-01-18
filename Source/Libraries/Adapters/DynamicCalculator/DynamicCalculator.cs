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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Ciloci.Flee;
using GSF;
using GSF.Collections;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using SparseArray = System.Collections.Generic.Dictionary<int, double>;

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
        public const string DefaultImports = $"AssemblyName=mscorlib, TypeName=System.Math; AssemblyName=mscorlib, TypeName=System.DateTime; AssemblyName=DynamicCalculator, TypeName=DynamicCalculator.{nameof(AggregateFunctions)}";

        private const string TimeVariable = "TIME";
        private const string UtcTimeVariable = "UTCTIME";
        private const string LocalTimeVariable = "LOCALTIME";
        private const string SystemNameVariable = "SYSTEMNAME";

        private static readonly string[] ReservedVariableNames = { TimeVariable, UtcTimeVariable, LocalTimeVariable, SystemNameVariable };

        // Fields
        private string m_expressionText;
        private string m_variableList;
        private string m_imports;
        private bool m_supportsTemporalProcessing;

        private readonly ImmediateMeasurements m_latestMeasurements;
        private Ticks m_latestTimestamp;
        private double m_latestValue;

        private readonly HashSet<string> m_variableNames;
        private readonly Dictionary<string, int> m_arrayVariableLengths;
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
            m_latestMeasurements = new ImmediateMeasurements
            {
                RealTimeFunction = () => RealTime
            };

            m_variableNames = new HashSet<string>();
            m_arrayVariableLengths = new Dictionary<string, int>();
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
        [ConnectionStringParameter]
        [Description("Defines primary keys of output measurements the action adapter expects; can be one of a filter expression, measurement key, point tag or Guid.")]
        [CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public override IMeasurement[] OutputMeasurements
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        /// <summary>
        /// Gets or sets the textual representation of the expression.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the arithmetic expression used to perform calculations.")]
        public string ExpressionText
        {
            get => m_expressionText;
            set
            {
                m_expressionText = value;
                PerformAliasReplacement();
            }
        }

        /// <summary>
        /// Gets or sets the list of variables used in the expression.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the unique list of variables used in the expression. Any defined aliased variables must be unique per defined dynamic calculator or e-mail notifier instance. Note that \"TIME\" is reserved, this returns the current timestamp for the calculation in ticks.")]
        public string VariableList
        {
            get => m_variableList;
            set
            {
                if (m_variableList == value)
                    return;

                // Store the variable list for get operations
                m_variableList = value;

                // Empty the collection of variable names
                m_variableNames.Clear();
                m_arrayVariableLengths.Clear();
                m_keyMapping.Clear();
                m_nonAliasedTokens.Clear();

                // If the value is null, do not attempt to process it
                if (value is null)
                    return;

                // Build the collection of variable names with the new value
                foreach (string token in value.Split(';'))
                    AddVariable(token);

                // Perform alias replacement on tokens that were not explicitly aliased
                PerformAliasReplacement();

                // Build the key list which will define the input measurements for this adapter
                string keyList = m_keyMapping.Keys.Select(key => key.ToString())
                            .Aggregate((runningKeyList, nextKey) => runningKeyList + ";" + nextKey);

                // Set the input measurements for this adapter
                InputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(DataSource, true, keyList);
            }
        }

        /// <summary>
        /// Gets or sets the list of types which define
        /// methods to be imported into the expression parser.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the list of types which define methods to be imported into the expression parser.")]
        [DefaultValue(DefaultImports)]
        public string Imports
        {
            get => m_imports;
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
        [ConnectionStringParameter]
        [Description("Define the flag indicating if this adapter supports temporal processing.")]
        [DefaultValue(false)]
        public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

        /// <summary>
        /// Gets or sets the flag indicating whether to use the latest
        /// received values to fill in values missing from the current frame.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the flag indicating if this adapter should use latest values for missing measurements.")]
        [DefaultValue(true)]
        public bool UseLatestValues { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether to skip processing of an output with a value of NaN.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the flag indicating whether to skip processing of an output with a value of NaN.")]
        [DefaultValue(false)]
        public bool SkipNaNOutput { get; set; }

        /// <summary>
        /// Gets or sets the value used when no other value can be determined for a variable.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the value used when no other value can be determined for a variable.")]
        [DefaultValue(double.NaN)]
        public double SentinelValue { get; set; }

        /// <summary>
        /// Gets or sets the interval at which the adapter should calculate values.
        /// </summary>
        /// <remarks>
        /// Set to zero to disable the timer and calculate values upon receipt of input data.
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Define the interval, in seconds, at which the adapter should calculate values. Zero value executes calculations at received data rate.")]
        [DefaultValue(0)]
        public double CalculationInterval
        {
            get => m_timerOperation.Delay / 1000.0D;
            set => m_timerOperation.Delay = (int)(value * 1000.0D);
        }

        /// <summary>
        /// Gets or sets the source of the timestamps of the calculated values.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the source of the timestamps of the calculated values.")]
        [DefaultValue(TimestampSource.Frame)]
        public TimestampSource TimestampSource { get; set; }

        /// <summary>
        /// Gets or sets primary keys of input measurements the dynamic calculator expects.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override MeasurementKey[] InputMeasurementKeys
        {
            get => base.InputMeasurementKeys;
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
        /// Gets defined variable collection with current values.
        /// </summary>
        protected IDictionary<string, object> Variables => m_expressionContext.Variables;
        
        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendLine($"           Expression Text: {ExpressionText}");
                status.AppendLine($"             Variable List: {VariableList}");
                status.AppendLine($"         Use Latest Values: {UseLatestValues}");
                status.AppendLine($"      Calculation Interval: {(CalculationInterval == 0.0D ? "At received data rate" : $"{CalculationInterval:N3} seconds")}");
                status.AppendLine($"          Timestamp Source: {TimestampSource}");
                status.AppendLine($"           Skip NaN Values: {SkipNaNOutput}");
                status.AppendLine($"            Sentinel Value: {SentinelValue}");

                if (ExpectsOutputMeasurement)
                    status.AppendLine($"     Last Calculated Value: {m_latestValue}");

                List<string> imports = new();

                if (!string.IsNullOrWhiteSpace(Imports))
                {
                    foreach (string typeDef in Imports.Split(';'))
                    {
                        Dictionary<string, string> parsedTypeDef = typeDef.ParseKeyValuePairs(',');

                        if (parsedTypeDef.TryGetValue("typeName", out string typeName))
                            imports.Add(typeName);
                    }
                }

                status.AppendLine();
                status.AppendLine($"Imported .NET Types ({imports.Count:N0} total):");

                foreach (string import in imports)
                    status.AppendLine($"        {import.TruncateRight(70)}");

                return status.ToString();
            }
        }

        private new Ticks RealTime =>
            TimestampSource switch
            {
                TimestampSource.RealTime => base.RealTime,
                TimestampSource.LocalClock => DateTime.UtcNow,
                TimestampSource.Frame => m_latestTimestamp,
                _ => m_latestTimestamp
            };

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="DynamicCalculator"/>.
        /// </summary>
        public override void Initialize()
        {
            const string ErrorMessage = "{0} is missing from Settings - Example: expressionText=x+y; variableList={{x = PPA:1; y = PPA:2}}";

            Dictionary<string, string> settings = Settings;
            base.Initialize();

            if (ExpectsOutputMeasurement && OutputMeasurements?.Length != 1)
                throw new ArgumentException($"Exactly one output measurement must be defined. Amount defined: {OutputMeasurements?.Length ?? 0}");

            // Load required parameters
            if (!settings.TryGetValue(nameof(VariableList), out string setting))
                throw new ArgumentException(string.Format(ErrorMessage, nameof(VariableList)));

            VariableList = setting;

            if (!settings.TryGetValue(nameof(ExpressionText), out setting))
                throw new ArgumentException(string.Format(ErrorMessage, nameof(ExpressionText)));

            ExpressionText = setting;

            // Load optional parameters
            Imports = settings.TryGetValue(nameof(Imports), out setting) ? setting : DefaultImports;
            m_supportsTemporalProcessing = settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting) && setting.ParseBoolean();

            SkipNaNOutput = settings.TryGetValue(nameof(SkipNaNOutput), out setting) && setting.ParseBoolean();
            TimestampSource = settings.TryGetValue(nameof(TimestampSource), out setting) && Enum.TryParse(setting, out TimestampSource timestampSource) ? timestampSource : TimestampSource.Frame;
            CalculationInterval = settings.TryGetValue(nameof(CalculationInterval), out setting) ? double.Parse(setting) : 0;
            UseLatestValues = !settings.TryGetValue(nameof(UseLatestValues), out setting) || setting.ParseBoolean();
            SentinelValue = settings.TryGetValue(nameof(SentinelValue), out setting) ? double.Parse(setting) : double.NaN;

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

            if (UseLatestValues && m_timerOperation.Delay > 0)
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

            if (UseLatestValues)
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

                if (measurement.Timestamp > m_latestTimestamp && measurement.Timestamp.UtcTimeIsValid(LagTime, LeadTime))
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
            m_expressionContext.Variables.Clear();

            // Set the values of variables in the expression
            foreach (MeasurementKey key in m_keyMapping.Keys)
            {
                string name = m_keyMapping[key];
                int arrayMarkerIndex = name.IndexOf('[');

                if (arrayMarkerIndex > 0)
                {
                    string alias = name.Substring(0, arrayMarkerIndex);
                    int index = int.Parse(name.Substring(arrayMarkerIndex + 1, name.Length - arrayMarkerIndex - 2));

                    if (m_expressionContext.Variables.GetOrAdd(alias, _ => new SparseArray()) is not SparseArray array)
                        throw new NullReferenceException($"Failed to create array variable \"{alias}[]\".");

                    if (measurements.TryGetValue(key, out IMeasurement measurement))
                        array[index] = measurement.AdjustedValue;
                    else
                        array[index] = SentinelValue;
                }
                else
                {
                    if (measurements.TryGetValue(key, out IMeasurement measurement))
                        m_expressionContext.Variables[name] = measurement.AdjustedValue;
                    else
                        m_expressionContext.Variables[name] = SentinelValue;
                }
            }

            // Convert SparseArray instances to normal arrays
            foreach (KeyValuePair<string, int> kvp in m_arrayVariableLengths)
            {
                string alias = kvp.Key;
                int length = kvp.Value;
                object value = null;

                if (m_expressionContext.Variables.TryGetValue(alias, ref value) && value is SparseArray sparseArray)
                {
                    double[] array = new double[length];

                    for (int i = 0; i < length; i++)
                    {
                        if (sparseArray.TryGetValue(i, out double measurementValue))
                            array[i] = measurementValue;
                        else
                            array[i] = SentinelValue;
                    }

                    m_expressionContext.Variables[alias] = array;
                }
                else
                {
                    m_expressionContext.Variables[alias] = Enumerable.Repeat(SentinelValue, length).ToArray();
                }
            }

            // Handle special constants
            m_expressionContext.Variables[TimeVariable] = RealTime.Value;
            m_expressionContext.Variables[UtcTimeVariable] = DateTime.UtcNow;
            m_expressionContext.Variables[LocalTimeVariable] = DateTime.Now;
            m_expressionContext.Variables[SystemNameVariable] = SystemName;

            // Compile the expression if it has not been compiled already
            m_expression ??= m_expressionContext.CompileDynamic(m_aliasedExpressionText);

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

            if (splitToken.Length > 2)
                throw new FormatException($"Too many equals signs: {token}");

            string alias = splitToken[0].Trim();
            string target = splitToken[1].Trim();

            if (string.IsNullOrWhiteSpace(alias))
                throw new FormatException($"Variable name cannot be empty: {token}");

            // Check for variable declarations defined as array inputs
            if (alias.EndsWith("[]"))
            {
                alias = alias.Substring(0, alias.Length - 2).Trim();
                string[] targets = target.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < targets.Length; i++)
                {
                    target = targets[i].Trim();
                    MeasurementKey key = GetKey(target);
                    AddMapping(key, alias, $"{alias}[{i}]");
                }

                m_arrayVariableLengths[alias] = targets.Length;
            }
            else
            {
                MeasurementKey key = GetKey(target);
                AddMapping(key, alias);
            }
        }

        // Adds a variable to the key-variable map which has not been explicitly aliased.
        private void AddNotAliasedVariable(string token)
        {
            token = token.Trim();
            m_nonAliasedTokens.Add(-token.Length, token);

            MeasurementKey key = GetKey(token);
            string alias = token.ReplaceCharacters('_', c => !char.IsLetterOrDigit(c));

            // Ensure that the generated alias is unique
            while (m_variableNames.Contains(alias))
                alias += "_";

            AddMapping(key, alias);
        }

        // Adds the given mapping to the key-variable map.
        private void AddMapping(MeasurementKey key, string alias, string indexedAlias = null)
        {
            if (indexedAlias is null && m_variableNames.Contains(alias))
                throw new ArgumentException($"Variable name is not unique: {alias}");

            foreach (string reservedName in ReservedVariableNames)
            {
                if (alias.Equals(reservedName, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Variable name \"{reservedName}\" is reserved.");
            }

            m_variableNames.Add(alias);
            m_keyMapping.Add(key, indexedAlias ?? alias);
        }

        // Performs alias replacement on tokens that were not explicitly aliased.
        private void PerformAliasReplacement()
        {
            StringBuilder aliasedExpressionTextBuilder = new(m_expressionText);

            foreach (string token in m_nonAliasedTokens.Values)
            {
                MeasurementKey key = GetKey(token);
                string alias = m_keyMapping[key];
                aliasedExpressionTextBuilder.Replace(token, alias);
            }

            m_aliasedExpressionText = aliasedExpressionTextBuilder.ToString();
        }

        // Gets a measurement key based on a token which
        // may be either a signal ID, measurement key or point tag.
        private MeasurementKey GetKey(string token)
        {
            if (Guid.TryParse(token, out Guid signalID))
                return MeasurementKey.LookUpBySignalID(signalID);

            if (MeasurementKey.TryParse(token, out MeasurementKey key))
                return key;

            const string MeasurementTable = "ActiveMeasurements";

            if (DataSource is not null && DataSource.Tables.Contains(MeasurementTable))
            {
                DataRow[] rows = DataSource.Tables[MeasurementTable].Select($"PointTag = '{token}'");

                if (rows.Length > 0)
                    key = MeasurementKey.LookUpOrCreate(rows[0]["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), rows[0]["ID"].ToString());
            }

            if (key == default || key == MeasurementKey.Undefined)
                throw new InvalidOperationException($"Could not parse token \"{token}\" as a Guid, measurement key, or point tag");

            return key;
        }

        // Generates a measurement with the given value and sends it into the system
        private void GenerateCalculatedMeasurement(long timestamp, IConvertible value)
        {
            if (value is null)
                throw new InvalidOperationException("Calculation must not return a type that does not convert to double.");

            IMeasurement calculatedMeasurement = Measurement.Clone(OutputMeasurements[0], Convert.ToDouble(value), timestamp);
            OnNewMeasurement(calculatedMeasurement);
            m_latestValue = calculatedMeasurement.AdjustedValue;
        }

        // Helper method to raise the NewMeasurements event
        // when only a single measurement is to be provided.
        private void OnNewMeasurement(IMeasurement measurement)
        {
            // Skip processing of an output with a value of NaN unless configured to process NaN outputs
            if (!SkipNaNOutput || !double.IsNaN(measurement.Value))
                OnNewMeasurements(new[] { measurement });
        }

        #endregion

        #region [ Static ]

        // Static Constructor
        static DynamicCalculator()
        {
            try
            {
                CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
                systemSettings.Add("SystemName", "", "Name of system that will be prefixed to system level tags, when defined. Value should follow tag naming conventions, e.g., no spaces and all upper case.");
                SystemName = systemSettings["SystemName"].Value;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            if (string.IsNullOrWhiteSpace(SystemName))
                SystemName = string.Empty;
        }

        // Static Properties
        internal static string SystemName { get; }

        #endregion
    }
}
