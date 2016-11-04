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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Ciloci.Flee;
using GSF;
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

        // Fields
        private string m_expressionText;
        private string m_variableList;
        private string m_imports;
        private bool m_supportsTemporalProcessing;
        private bool m_skipNanOutput;
        private TimestampSource m_timestampSource;

        private readonly HashSet<string> m_variableNames;
        private readonly Dictionary<MeasurementKey, string> m_keyMapping;
        private readonly SortedDictionary<int, string> m_nonAliasedTokens;

        private string m_aliasedExpressionText;
        private readonly ExpressionContext m_expressionContext;
        private IDynamicExpression m_expression;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DynamicCalculator"/>.
        /// </summary>
        public DynamicCalculator()
        {
            m_variableNames = new HashSet<string>();
            m_keyMapping = new Dictionary<MeasurementKey, string>();
            m_nonAliasedTokens = new SortedDictionary<int, string>();
            m_expressionContext = new ExpressionContext();
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
        Description("Define the list of variables used in the expression.")]
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
                Dictionary<string, string> parsedTypeDef;
                string assemblyName, typeName;
                Assembly asm;
                Type t;

                foreach (string typeDef in value.Split(';'))
                {
                    try
                    {
                        parsedTypeDef = typeDef.ParseKeyValuePairs(',');
                        assemblyName = parsedTypeDef["assemblyName"];
                        typeName = parsedTypeDef["typeName"];
                        asm = Assembly.Load(new AssemblyName(assemblyName));
                        t = asm.GetType(typeName);

                        m_expressionContext.Imports.AddType(t);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Unable to load type from assembly: {0}", typeDef);
                        OnProcessException(new ArgumentException(message, ex));
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
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return m_supportsTemporalProcessing;
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

            base.Initialize();
            settings = Settings;

            if (OutputMeasurements.Length != 1)
                throw new ArgumentException(string.Format("Exactly one output measurement must be defined. Amount defined: {0}", OutputMeasurements.Length));

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
                m_skipNanOutput = false; //default to previous behavour

            if (settings.TryGetValue("timestampSource", out setting))
                m_timestampSource = (TimestampSource)Enum.Parse(typeof(TimestampSource), setting);
            else
                m_timestampSource = TimestampSource.Frame;
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements;
            IMeasurement measurement;
            string name;
            long timestamp;

            measurements = frame.Measurements;
            m_expressionContext.Variables.Clear();

            // Set the values of variables in the expression
            foreach (MeasurementKey key in m_keyMapping.Keys)
            {
                name = m_keyMapping[key];

                if (measurements.TryGetValue(key, out measurement))
                    m_expressionContext.Variables[name] = measurement.AdjustedValue;
                else
                    m_expressionContext.Variables[name] = double.NaN;
            }

            // Compile the expression if it has not been compiled already
            if ((object)m_expression == null)
                m_expression = m_expressionContext.CompileDynamic(m_aliasedExpressionText);

            // Get the timestamp of the measurement to be generated
            switch (m_timestampSource)
            {
                default:
                case TimestampSource.Frame:
                    timestamp = frame.Timestamp;
                    break;

                case TimestampSource.RealTime:
                    timestamp = RealTime;
                    break;

                case TimestampSource.LocalClock:
                    timestamp = DateTime.UtcNow.Ticks;
                    break;
            }

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
                throw new FormatException(string.Format("Too many equals signs: {0}", token));

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
                throw new ArgumentException(string.Format("Variable name is not unique: {0}", alias));

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
                OnNewMeasurements(new IMeasurement[] { measurement });
        }

        #endregion
    }
}
