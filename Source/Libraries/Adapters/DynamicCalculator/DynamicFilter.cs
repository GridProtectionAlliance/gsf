//******************************************************************************************************
//  DynamicFilter.cs - Gbtc
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Ciloci.Flee;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DynamicCalculator
{
    /// <summary>
    /// Represents the target operation type for a filter.
    /// </summary>
    public enum FilterOperation
    {
        /// <summary>
        /// Defines filter operation that removes measurements from processing when expression evaluates to true.
        /// </summary>
        [Description("Removes measurements from processing when expression evaluates to true.")]
        RemoveWhenTrue,

        /// <summary>
        /// Defines filter operation that changes measurement values based on expression evaluation.
        /// </summary>
        [Description("Changes measurement values based on expression evaluation.")]
        ValueAugmentation
    }    

    /// <summary>
    /// The DynamicFilter is a filter adapter which takes multiple input measurements
    /// and performs a calculation on those measurements to augment their values
    /// before they are routed to other adapters.
    /// </summary>
    [Description("Dynamic Filter: Performs arithmetic augmentations on multiple input signals")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DynamicFilter : DynamicCalculator, IFilterAdapter
    {
        // Class is derived from an ActionAdapter but implements IFilterAdapter. Technically this
        // would cause adapter to be shown in both action adapter and filter adapter UI screens,
        // however, UI adapter configuration screen adapter type filtering was modified so that a
        // filter adapter marked with a browsable state of Advanced would be allowed, this causes
        // adapter to be hidden from action adapter UI but shown on filter adapters UI.

        #region [ Members ]

        // Constants
        private const int DefaultExecutionOrder = 0;

        private const string IndexVariable = "INDEX";

        // Fields
        private HashSet<MeasurementKey> m_inputMeasurementKeys;
        private ReadOnlyDictionary<string, MeasurementKey> m_variableKeys;
        private string[] m_reservedVariableNames;
        private long m_processedMeasurements;
        private long m_removedMeasurements;
        private long m_skippedRemovalSets;
        private bool m_valueIsArray;
        private int m_valueArrayLength;
        private object m_result;
        private int m_index;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DynamicFilter"/> class.
        /// </summary>
        public DynamicFilter() => 
            m_inputMeasurementKeys = new HashSet<MeasurementKey>();

        #endregion

        #region [ Properties ]
        
        /// <summary>
        /// Gets or sets the boolean expression used to determine if the database operation should be executed.
        /// </summary>
        [ConnectionStringParameter]
        [Description
        (
            "Defines the expression used to augment or remove measurements before publication to other adapters in the TSL Iaon session.\n" + 
            "Using 'value[INDEX]' will allow operation on each measurement when 'value' variable is defined as an array, i.e., 'value[]'.\n" +
            "Example: (CAST(value[INDEX], int) AND 48) = 0"
        )]
        public new string ExpressionText // Redeclared to provide a more relevant description and example value for this adapter
        {
            get => base.ExpressionText;
            set => base.ExpressionText = value;
        }

        /// <summary>
        /// Gets or sets the list of variables used in the expression.
        /// </summary>
        [ConnectionStringParameter]
        [Description
        (
            "Defines the unique list of variables used in the expression. Variable named 'value' is required and can be an array, as 'value[]'.\n" +
            "For array definition, use a comma separated list of targets or a filter expression.\n" +
            "Example: value[]=FILTER ActiveMeasurements WHERE SignalType='DIGI'"
        )]
        public new string VariableList // Redeclared to provide a more relevant description and example value for this adapter
        {
            get => base.VariableList;
            set => base.VariableList = value;
        }

        /// <summary>
        /// Gets or sets the value that determines the order in which filter adapters are executed.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the value that determines the order in which filter adapters are executed.")]
        [DefaultValue(DefaultExecutionOrder)]
        public virtual int ExecutionOrder { get; set; }

        /// <summary>
        /// Gets or sets the operation type of the filter calculation.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines ValueAugmentation.")]
        [DefaultValue(FilterOperation.ValueAugmentation)]
        public FilterOperation FilterOperation { get; set; }

        #region [ Hidden Properties ]

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="AdapterBase"/> expects, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override MeasurementKey[] InputMeasurementKeys // Overridden to auto-capture measurement key hash set
        {
            get => base.InputMeasurementKeys;
            set
            {
                base.InputMeasurementKeys = value;

                if (!m_inputMeasurementKeys.SetEquals(value))
                    m_inputMeasurementKeys = new HashSet<MeasurementKey>(value);
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="AdapterBase"/> will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IMeasurement[] OutputMeasurements // Redeclared to hide property - not relevant to this adapter
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int FramesPerSecond // Redeclared to hide property - not relevant to this adapter
        {
            get => base.FramesPerSecond;
            set => base.FramesPerSecond = value;
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double LagTime // Redeclared to hide property - not relevant to this adapter
        {
            get => base.LagTime;
            set => base.LagTime = value;
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double LeadTime // Redeclared to hide property - not relevant to this adapter
        {
            get => base.LeadTime;
            set => base.LeadTime = value;
        }

        /// <summary>
        /// Gets or sets the interval at which the adapter should calculate values.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double CalculationInterval // Redeclared to hide property - not relevant to this adapter
        {
            get => base.CalculationInterval;
            set => base.CalculationInterval = value;
        }        
        
        /// <summary>
        /// Gets or sets the source of the timestamps of the calculated values.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new TimestampSource TimestampSource // Redeclared to hide property - not relevant to this adapter
        { 
            get => base.TimestampSource; 
            set => base.TimestampSource = value;
        }

        /// <summary>
        /// Gets or sets the flag indicating whether to use the latest
        /// received values to fill in values missing from the current frame.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Redeclared to hide property - not relevant to this adapter
        public new bool UseLatestValues
        { 
            get => base.UseLatestValues; 
            set => base.UseLatestValues = value;
        }

        /// <summary>
        /// Gets or sets the flag indicating whether to skip processing of an output with a value of NaN.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Redeclared to hide property - not relevant to this adapter
        public new bool SkipNaNOutput
        { 
            get => base.SkipNaNOutput; 
            set => base.SkipNaNOutput = value; 
        }

        #endregion

        /// <summary>
        /// Gets the list of reserved variable names.
        /// </summary>
        protected override string[] ReservedVariableNames => 
            m_reservedVariableNames ??= base.ReservedVariableNames.Concat(new [] { IndexVariable }).ToArray();

        /// <summary>
        /// Gets flag that determines if the implementation of the <see cref="DynamicCalculator"/> requires an output measurement.
        /// </summary>
        protected override bool ExpectsOutputMeasurement => false; 

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
                status.AppendLine($"           Execution Order: {ExecutionOrder}");
                status.AppendLine($"          Filter Operation: {FilterOperation}");
                status.AppendLine($"         Target Value Type: {(m_valueIsArray ? "Array" : "Singleton")}");

                if (FilterOperation == FilterOperation.RemoveWhenTrue)
                {
                    status.AppendLine($"    Processed Measurements: {m_processedMeasurements:N0}");
                    status.AppendLine($"      Removed Measurements: {m_removedMeasurements:N0}");
                    status.AppendLine($"      Skipped Removal Sets: {m_skippedRemovalSets:N0}");
                }
                else
                {
                    status.AppendLine($"    Augmented Measurements: {m_processedMeasurements:N0}");
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]
        
        /// <summary>
        /// Initializes <see cref="DatabaseNotifier"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;

            settings[nameof(FramesPerSecond)] = "1";
            settings[nameof(LagTime)] = "5";
            settings[nameof(LeadTime)] = "5";
            settings[nameof(CalculationInterval)] = "0";
            settings[nameof(TimestampSource)] = nameof(TimestampSource.LocalClock);
            settings[nameof(UseLatestValues)] = false.ToString();
            settings[nameof(SkipNaNOutput)] = false.ToString();
            
            base.Initialize();

            if (settings.TryGetValue(nameof(ExecutionOrder), out string setting)  && int.TryParse(setting, out int executionOrder))
                ExecutionOrder = executionOrder;

            FilterOperation = settings.TryGetValue(nameof(FilterOperation), out setting) && Enum.TryParse(setting, out FilterOperation filterOperation) ? 
                filterOperation : 
                FilterOperation.ValueAugmentation;

            ReadOnlyCollection<string> variableNames = VariableNames;

            m_valueIsArray = variableNames.Contains("value[]");

            if (!variableNames.Contains("value") && !m_valueIsArray)
                throw new InvalidOperationException("Variable named 'value', or 'value[]' when defined as an array, is required.");

            m_valueArrayLength = m_valueIsArray ? ArrayVariableLengths["value"] : 0;
            m_variableKeys = VariableKeys;
        }

        /// <summary>
        /// Handler for new measurements that have not yet been routed.
        /// </summary>
        /// <param name="measurements">Measurements that have not yet been routed.</param>
        public void HandleNewMeasurements(ICollection<IMeasurement> measurements)
        {
            // Measurements are handled here prior to being routing, so manual filtering is required
            IReadOnlyDictionary<MeasurementKey, IMeasurement> inputs = measurements
                .Where(measurement => m_inputMeasurementKeys.Contains(measurement.Key))
                .ToDictionary(measurement => measurement.Key);
         
            if (inputs.Count == 0)
                return;

            Dictionary<MeasurementKey, int> indexes = null;

            if (FilterOperation == FilterOperation.RemoveWhenTrue)
            {
                // Iaon session will automatically convert readonly measurements to a list when filter adapters are defined,
                // however, if user is manually injecting measurements or using a custom TSL implementation, it is still
                // possible for measurements to be published from a readonly source, e.g., an array, so we check that here
                if (measurements.IsReadOnly)
                {
                    // Cannot remove measurements when adapter publishes new measurements as a readonly collection, e.g., from an array
                    if (m_skippedRemovalSets == 0L)
                        OnStatusMessage(MessageLevel.Warning, $"Detected target measurements for possible filter removal from an adapter that is publishing new measurements with readonly data sets, e.g., from an array. Adapter should be updated to publish new measurements from a 'List' instead of an 'Array' so that measurements can be removed from processing as needed by the {nameof(DynamicFilter)} \"{Name}\".");

                    Interlocked.Increment(ref m_skippedRemovalSets);
                    return;
                }
                else
                {
                    int index = 0;
                    indexes = measurements.ToDictionary(measurement => measurement.Key, _ => index++);
                }
            }

            void removeMeasurements(List<int> indexesToBeRemoved)
            {
                if (indexesToBeRemoved.Count == 0)
                    return;

                bool nativeOperation = true;

                if (measurements is not List<IMeasurement> workingList)
                {
                    workingList = new List<IMeasurement>(measurements);
                    nativeOperation = false;
                }

                indexesToBeRemoved.Sort();

                for (int i = indexesToBeRemoved.Count - 1; i >= 0; i--)
                    workingList.RemoveAt(indexesToBeRemoved[i]);

                Interlocked.Add(ref m_removedMeasurements, indexesToBeRemoved.Count);

                if (nativeOperation)
                    return;

                measurements.Clear();

                foreach (IMeasurement measurement in workingList)
                    measurements.Add(measurement);
            }

            if (m_valueIsArray)
            {
                if (FilterOperation == FilterOperation.RemoveWhenTrue)
                    removeMeasurements(ProcessRemoveWhenTrueForArray(inputs, indexes));
                else
                    ProcessValueAugmentationForArray(inputs);
            }
            else
            {
                if (FilterOperation == FilterOperation.RemoveWhenTrue)
                    removeMeasurements(ProcessRemoveWhenTrueForSingleton(inputs, indexes));
                else
                    ProcessValueAugmentationForSingleton(inputs);
            }

            Interlocked.Add(ref m_processedMeasurements, inputs.Count);
        }

        private List<int> ProcessRemoveWhenTrueForArray(IReadOnlyDictionary<MeasurementKey, IMeasurement> inputs, IReadOnlyDictionary<MeasurementKey, int> indexes)
        {
            List<int> indexesToBeRemoved = new();

            for (m_index = 0; m_index < m_valueArrayLength; m_index++)
            {
                Calculate(inputs, m_index);

                // If calculation result is true, measurement is targeted for removal
                if (m_result.ToString().ParseBoolean())
                    indexesToBeRemoved.Add(indexes[m_variableKeys[$"value[{m_index}]"]]);
            }

            return indexesToBeRemoved;
        }
        
        private void ProcessValueAugmentationForArray(IReadOnlyDictionary<MeasurementKey, IMeasurement> inputs)
        {
            for (m_index = 0; m_index < m_valueArrayLength; m_index++)
            {
                Calculate(inputs, m_index);

                // If calculation result is a convertible type, we update measurement value
                if (m_result is IConvertible result)
                    inputs[m_variableKeys[$"value[{m_index}]"]].Value = Convert.ToDouble(result);
            }
        }

        private List<int> ProcessRemoveWhenTrueForSingleton(IReadOnlyDictionary<MeasurementKey, IMeasurement> inputs, IReadOnlyDictionary<MeasurementKey, int> indexes)
        {
            List<int> indexesToBeRemoved = new(1);

            Calculate(inputs);

            // If calculation result is true, measurement is targeted for removal
            if (m_result.ToString().ParseBoolean())
                indexesToBeRemoved.Add(indexes[m_variableKeys["value"]]);

            return indexesToBeRemoved;
        }

        private void ProcessValueAugmentationForSingleton(IReadOnlyDictionary<MeasurementKey, IMeasurement> inputs)
        {
            Calculate(inputs);

            // If calculation result is a convertible type, we update measurement value
            if (m_result is IConvertible result)
                inputs[m_variableKeys["value"]].Value = Convert.ToDouble(result);
        }

        /// <summary>
        /// Handler for assignment of special variables, e.g., constants, for the <see cref="DynamicCalculator"/>.
        /// </summary>
        /// <param name="variables">Variable set to current calculation.</param>
        /// <remarks>
        /// Special constants should be defined in the <see cref="DynamicCalculator.ReservedVariableNames"/> array.
        /// </remarks>
        protected override void HandleSpecialVariables(VariableCollection variables)
        {
            base.HandleSpecialVariables(variables);

            if (m_valueIsArray)
                variables[IndexVariable] = m_index;
        }

        /// <summary>
        /// Handler for the values calculated by the <see cref="DynamicCalculator"/>.
        /// </summary>
        /// <param name="value">The value calculated by the <see cref="DynamicCalculator"/>.</param>
        protected override void HandleCalculatedValue(object value) => 
            m_result = value;

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength) => 
            $"{m_processedMeasurements} measurements processed so far...".CenterText(maxLength);

        #endregion
    }
}
