//******************************************************************************************************
//  BulkSequenceCalculator.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  04/27/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GSF.Collections;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.FuzzyStrings;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units.EE;
using MeasurementRecord = GSF.TimeSeries.Model.Measurement;
using PhasorRecord = GSF.TimeSeries.Model.Phasor;
using SignalType = GSF.Units.EE.SignalType;
using PhaseDetail = System.Tuple<GSF.TimeSeries.MeasurementKey, GSF.Units.EE.SignalType, GSF.TimeSeries.Model.Measurement, GSF.TimeSeries.Model.Phasor>;

namespace PowerCalculations
{
    /// <summary>
    /// Represents an adapter that manages bulk calculation of positive, negative and zero sequence values based on A, B and C phase data inputs.
    /// </summary>
    [Description("Bulk Sequence Calculator: Manages bulk calculation of positive, negative and zero sequence values based on A, B and C phase data inputs.")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public class BulkSequenceCalculator : IndependentActionAdapterManagerBase<SequenceCalculator>
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="IndependentActionAdapterManagerBase{TAdapter}.InputMeasurementKeys"/>.
        /// </summary>
        public const string DefaultInputMeasurementKeys = "FILTER ActiveMeasurements WHERE SignalType LIKE '%PH%' AND Phase IN ('A', 'B', 'C') ORDER BY PhasorID";

        // Fields
        private readonly List<PhasorType> m_outputAdapterPhasorTypes;
        private readonly List<string> m_customAdapterSettings;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates new <see cref="BulkSequenceCalculator"/>.
        /// </summary>
        public BulkSequenceCalculator()
        {
            m_outputAdapterPhasorTypes = new List<PhasorType>();
            m_customAdapterSettings = new List<string>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets number of input measurement required by each adapter.
        /// </summary>
        public override int PerAdapterInputCount => 6;

        /// <summary>
        /// Gets or sets the index into the per adapter input measurements to use for target adapter name.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the index into the per adapter input measurements to use for target adapter name.")]
        [DefaultValue(0)]
        public override int InputMeasurementIndexUsedForName { get; set; } = 0;

        /// <summary>
        /// Gets output measurement names.
        /// </summary>
        public override ReadOnlyCollection<string> PerAdapterOutputNames => Array.AsReadOnly(SequenceCalculator.Outputs.Select(output => $"{output}").ToArray());

        /// <summary>
        /// Gets signal type for each output measurement, used when each output needs to be a different type.
        /// </summary>
        public override SignalType[] SignalTypes
        {
            get
            {
                if (CurrentAdapterIndex > -1 && CurrentAdapterIndex < m_outputAdapterPhasorTypes.Count)
                {
                    switch (m_outputAdapterPhasorTypes[CurrentAdapterIndex])
                    {
                        case PhasorType.Voltage:
                            return new[] { SignalType.VPHM, SignalType.VPHA, SignalType.VPHM, SignalType.VPHA, SignalType.VPHM, SignalType.VPHA };
                        case PhasorType.Current:
                            return new[] { SignalType.IPHM, SignalType.IPHA, SignalType.IPHM, SignalType.IPHA, SignalType.IPHM, SignalType.IPHA };
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets any custom adapter settings to be added to each adapter connection string. Can be used to add
        /// settings that are custom per adapter.
        /// </summary>
        public override string CustomAdapterSettings
        {
            get
            {
                if (CurrentAdapterIndex > -1 && CurrentAdapterIndex < m_customAdapterSettings.Count)
                    return m_customAdapterSettings[CurrentAdapterIndex];

                return null;
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="IndependentActionAdapterManagerBase{TAdapter}"/> will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - outputs managed automatically
        public override IMeasurement[] OutputMeasurements
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses connection string.
        /// </summary>
        public override void ParseConnectionString()
        {
            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue(nameof(InputMeasurementKeys), out string inputMeasurementKeys) || string.IsNullOrWhiteSpace(inputMeasurementKeys))
                settings[nameof(InputMeasurementKeys)] = DefaultInputMeasurementKeys;

            base.ParseConnectionString();

            Dictionary<int, List<PhaseDetail>> devicePhaseDetails = new Dictionary<int, List<PhaseDetail>>();

            using (AdoDataConnection connection = GetConfiguredConnection())
            {
                TableOperations<MeasurementRecord> measurementTable = new TableOperations<MeasurementRecord>(connection);
                TableOperations<PhasorRecord> phasorTable = new TableOperations<PhasorRecord>(connection);

                for (int i = 0; i < InputMeasurementKeys.Length; i++)
                {
                    MeasurementKey key = InputMeasurementKeys[i];
                    SignalType signalType = InputMeasurementKeyTypes[i];

                    switch (signalType)
                    {
                        case SignalType.VPHM:
                        case SignalType.IPHM:
                        case SignalType.VPHA:
                        case SignalType.IPHA:
                            MeasurementRecord measurement = measurementTable.QueryRecordWhere("SignalID = {0}", key.SignalID);
                            
                            if (measurement?.DeviceID != null)
                            {
                                int deviceID = measurement.DeviceID.Value;
                                PhasorRecord phasor = phasorTable.QueryRecordWhere("DeviceID = {0} AND SourceIndex = {1}", deviceID, measurement.PhasorSourceIndex.GetValueOrDefault());

                                if (phasor == null)
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"No phasor associated with input \"{key}\" [{key.SignalID}] for {nameof(BulkSequenceCalculator)}, input excluded.");
                                }
                                else if (string.IsNullOrWhiteSpace(phasor.Label))
                                {
                                    OnStatusMessage(MessageLevel.Warning, $"Phasor associated with input \"{key}\" [{key.SignalID}] for {nameof(BulkSequenceCalculator)} has an undefined label, input excluded.");
                                }
                                else
                                {
                                    char phase = char.ToUpperInvariant(phasor.Phase);

                                    if (phase == 'A' || phase == 'B' || phase == 'C')
                                    {
                                        List<PhaseDetail> phases = devicePhaseDetails.GetOrAdd(deviceID, id => new List<PhaseDetail>());
                                        phases.Add(new PhaseDetail(key, signalType, measurement, phasor));
                                    }
                                    else
                                    {
                                        OnStatusMessage(MessageLevel.Warning, $"Unexpected phase type '{phase}' in input \"{key}\" [{key.SignalID}] for {nameof(BulkSequenceCalculator)}. Expected one of 'A', 'B', or 'C', input excluded.");
                                    }
                                }
                            }

                            break;
                        default:
                            OnStatusMessage(MessageLevel.Warning, $"Unexpected signal type \"{signalType}\" in input \"{key}\" [{key.SignalID}] for {nameof(BulkSequenceCalculator)}. Expected one of \"{SignalType.VPHM}\", \"{SignalType.VPHA}\", \"{SignalType.IPHM}\", or \"{SignalType.IPHA}\", input excluded.");
                            break;
                    }
                }
            }

            // Build proper set of inputs where phases are grouped together
            List<MeasurementKey> inputs = new List<MeasurementKey>();
            HashSet<int> duplicatedMatches = new HashSet<int>();
            int incompleteCount = 0;

            foreach (KeyValuePair<int, List<PhaseDetail>> kvp in devicePhaseDetails)
            {
                int deviceID = kvp.Key;
                List<PhaseDetail> phaseDetails = kvp.Value;

                // Check if the device has the minimum needed phasor count to perform sequence calculation
                if (phaseDetails.Count < PerAdapterInputCount)
                {
                    // This is common as not all devices report A, B and C phases - so no warning is displayed
                    incompleteCount++;
                    continue;
                }

                // Extract parallel phase label list and find all phase voltage/current angle indexes
                string[] labels = new string[phaseDetails.Count];
                List<int> avPhaseIndexes = new List<int>(); // A-phase Voltage Indexes
                List<int> bvPhaseIndexes = new List<int>(); // B-phase Voltage Indexes
                List<int> cvPhaseIndexes = new List<int>(); // C-phase Voltage Indexes
                List<int> aiPhaseIndexes = new List<int>(); // A-phase Current Indexes
                List<int> biPhaseIndexes = new List<int>(); // B-phase Current Indexes
                List<int> ciPhaseIndexes = new List<int>(); // C-phase Current Indexes

                Dictionary<int, int> phasorSourceIndexCounts = new Dictionary<int, int>();

                for (int i = 0; i < phaseDetails.Count; i++)
                {
                    PhaseDetail phaseDetail = phaseDetails[i];
                    SignalType signalType = phaseDetail.Item2;
                    MeasurementRecord measurement = phaseDetail.Item3;
                    PhasorRecord phasor = phaseDetail.Item4;
                    labels[i] = phasor.Label.Trim().ToUpperInvariant().Replace(' ', '_');
                    char phase = char.ToUpperInvariant(phasor.Phase);

                    int phasorSourceIndex = measurement.PhasorSourceIndex.GetValueOrDefault();

                    if (phasorSourceIndex > 0)
                        phasorSourceIndexCounts[phasorSourceIndex] = phasorSourceIndexCounts.GetOrAdd(phasorSourceIndex, 0) + 1;

                    // Just focusing on indexes for angle measurements since both angle and magnitude share same phase label
                    switch (signalType)
                    {
                        case SignalType.VPHA: // Voltage Phase Angle Measurement
                            switch (phase)
                            {
                                case 'A':
                                    avPhaseIndexes.Add(i);
                                    break;
                                case 'B':
                                    bvPhaseIndexes.Add(i);
                                    break;
                                case 'C':
                                    cvPhaseIndexes.Add(i);
                                    break;
                            }
                            break;
                        case SignalType.IPHA: // Current Phase Angle Measurement
                            switch (phase)
                            {
                                case 'A':
                                    aiPhaseIndexes.Add(i);
                                    break;
                                case 'B':
                                    biPhaseIndexes.Add(i);
                                    break;
                                case 'C':
                                    ciPhaseIndexes.Add(i);
                                    break;
                            }
                            break;
                    }
                }

                if (phasorSourceIndexCounts.Values.Any(count => count > 2))
                {
                    OnStatusMessage(MessageLevel.Warning, $"Encountered too many measurement phasor source indexes associated with device ID {deviceID:N0} for {nameof(BulkSequenceCalculator)}. Two measurements, one angle and one magnitude, are expected per phasor source index, input excluded.");
                    incompleteCount++;
                    continue;
                }

                // Find matching label indexes, in best matching order
                List<int> matchingLabelIndexes(string aPhaseLabel, IReadOnlyList<int> phaseIndexes, IReadOnlyList<string> phaseLabels)
                {
                    if (phaseIndexes.Count == 0)
                        return new List<int>();

                    Debug.Assert(phaseIndexes.Count == phaseLabels.Count, "Target phase index and label list lengths do not match");

                    // Check for exact match, best possible case
                    for (int i = 0; i < phaseLabels.Count; i++)
                    {
                        if (aPhaseLabel.Equals(phaseLabels[i]))
                            return new List<int>(new[] { phaseIndexes[i] });
                    }

                    // Try fuzzy match for longest common sub-sequence length
                    List<string> fuzzyMatches = phaseLabels.Select(aPhaseLabel.LongestCommonSubsequence).ToList();

                    // Sort descending by greatest matching lengths
                    List<Tuple<int, int>> matchPriority = fuzzyMatches.Select((label, index) => new Tuple<int, int>(index, label.Length)).ToList();
                    matchPriority.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                    return matchPriority.Select(item => phaseIndexes[item.Item1]).ToList();
                }

                // Extract B-phase and C-phase labels to be matched with A-phase labels for voltages
                List<string> bvPhaseLabels = bvPhaseIndexes.Select(index => labels[index]).ToList();
                List<string> cvPhaseLabels = cvPhaseIndexes.Select(index => labels[index]).ToList();

                // Find matching b and c phase for a phase voltages
                List<int>[] bvMatches = new List<int>[avPhaseIndexes.Count];
                List<int>[] cvMatches = new List<int>[avPhaseIndexes.Count];

                for (int i = 0; i < avPhaseIndexes.Count; i++)
                {
                    string avPhaseLabel = labels[avPhaseIndexes[i]];
                    bvMatches[i] = matchingLabelIndexes(avPhaseLabel, bvPhaseIndexes, bvPhaseLabels);
                    cvMatches[i] = matchingLabelIndexes(avPhaseLabel, cvPhaseIndexes, cvPhaseLabels);
                }

                // Extract B-phase and C-phase labels to be matched with A-phase labels for currents
                List<string> biPhaseLabels = biPhaseIndexes.Select(index => labels[index]).ToList();
                List<string> ciPhaseLabels = ciPhaseIndexes.Select(index => labels[index]).ToList();

                // Find matching b and c phase for a phase currents
                List<int>[] biMatches = new List<int>[aiPhaseIndexes.Count];
                List<int>[] ciMatches = new List<int>[aiPhaseIndexes.Count];

                for (int i = 0; i < aiPhaseIndexes.Count; i++)
                {
                    string aiPhaseLabel = labels[aiPhaseIndexes[i]];
                    biMatches[i] = matchingLabelIndexes(aiPhaseLabel, biPhaseIndexes, biPhaseLabels);
                    ciMatches[i] = matchingLabelIndexes(aiPhaseLabel, ciPhaseIndexes, ciPhaseLabels);
                }

                bool updateOverlappingMatches(int i, int j, List<int>[] matches)
                {
                    if (matches[i].Count > 0 && matches[j].Count > 1 && matches[i][0] == matches[j][0])
                    {
                        // Use next best match when more than one A-phase matched same target phase
                        matches[j].RemoveAt(0);
                        duplicatedMatches.Add(deviceID);
                        return true;
                    }

                    return false;
                }

                // Check for overlapping matches - each A-phase set needs a unique matching set
                void checkForOverlaps(int aPhaseCount, List<int>[] bMatches, List<int>[] cMatches)
                {
                    bool hadOverlaps = true;

                    while (hadOverlaps)
                    {
                        hadOverlaps = false;

                        for (int i = 0; i < aPhaseCount; i++)
                        {
                            for (int j = 0; j < aPhaseCount; j++)
                            {
                                if (i == j)
                                    continue;

                                hadOverlaps |= updateOverlappingMatches(i, j, bMatches);
                                hadOverlaps |= updateOverlappingMatches(i, j, cMatches);
                            }
                        }
                    }
                }

                checkForOverlaps(avPhaseIndexes.Count, bvMatches, cvMatches);
                checkForOverlaps(aiPhaseIndexes.Count, biMatches, ciMatches);

                MeasurementKey getAngleMeasurement(int index) => phaseDetails[index].Item1;

                bool tryGetMagnitudeMeasurement(int angleMeasurementIndex, out MeasurementKey magnitudeMeasurement)
                {
                    PhaseDetail phaseDetail = phaseDetails[angleMeasurementIndex];
                    MeasurementRecord angleMeasurementRecord = phaseDetail.Item3;
                    SignalType signalType = phaseDetail.Item2;
                    int phasorSourceIndex = angleMeasurementRecord.PhasorSourceIndex.GetValueOrDefault();

                    // Phasor source index is one-based - a zero value means value was null
                    if (phasorSourceIndex == 0)
                    {
                        OnStatusMessage(MessageLevel.Warning, $"Measurement \"{angleMeasurementRecord.PointTag}\" [{signalType}] did not define a PhasorSourceIndex - this was unexpected, A/B/C phasor-set will be skipped for sequence calculations.");
                        magnitudeMeasurement = default(MeasurementKey);
                        return false;
                    }

                    // Find voltage measurement with matching device ID and phasor index
                    for (int i = 0; i < phaseDetails.Count; i++)
                    {
                        if (i == angleMeasurementIndex)
                            continue;

                        phaseDetail = phaseDetails[i];
                        MeasurementRecord magnitudeMeasurementRecord = phaseDetail.Item3;
                        Debug.Assert(magnitudeMeasurementRecord.DeviceID == angleMeasurementRecord.DeviceID, "Unexpected unrelated device measurement found in set.");

                        if (magnitudeMeasurementRecord.PhasorSourceIndex.GetValueOrDefault() != phasorSourceIndex)
                            continue;

                        signalType = phaseDetail.Item2;

                        if (signalType != SignalType.VPHM && signalType != SignalType.IPHM)
                        {
                            OnStatusMessage(MessageLevel.Warning, $"Measurement \"{magnitudeMeasurementRecord.PointTag}\" [{signalType}] is not a magnitude measurement - this was unexpected, A/B/C phasor-set will be skipped for sequence calculations.");
                            magnitudeMeasurement = default(MeasurementKey);
                            return false;
                        }

                        magnitudeMeasurement = phaseDetail.Item1;
                        return true;
                    }

                    magnitudeMeasurement = default(MeasurementKey);
                    return false;
                }

                void addSequenceCalculatorInputs(IReadOnlyList<int> aPhaseIndexes, List<int>[] bMatches, List<int>[] cMatches, PhasorType outputPhasorType)
                {
                    for (int i = 0; i < aPhaseIndexes.Count; i++)
                    {
                        if (bMatches[i].Count == 0 || cMatches[i].Count == 0)
                        {
                            incompleteCount++;
                            continue;
                        }

                        int aPhaseIndex = aPhaseIndexes[i];
                        int bPhaseIndex = bMatches[i][0];
                        int cPhaseIndex = cMatches[i][0];

                        MeasurementKey aPhaseAngle = getAngleMeasurement(aPhaseIndex);
                        MeasurementKey bPhaseAngle = getAngleMeasurement(bPhaseIndex);
                        MeasurementKey cPhaseAngle = getAngleMeasurement(cPhaseIndex);

                        if (!tryGetMagnitudeMeasurement(aPhaseIndex, out MeasurementKey aPhaseMagnitude))
                        {
                            incompleteCount++;
                            continue;
                        }

                        if (!tryGetMagnitudeMeasurement(bPhaseIndex, out MeasurementKey bPhaseMagnitude))
                        {
                            incompleteCount++;
                            continue;
                        }

                        if (!tryGetMagnitudeMeasurement(cPhaseIndex, out MeasurementKey cPhaseMagnitude))
                        {
                            incompleteCount++;
                            continue;
                        }

                        // Add the six SequenceCalculator input measurements in desired order
                        inputs.Add(aPhaseAngle);
                        inputs.Add(bPhaseAngle);
                        inputs.Add(cPhaseAngle);
                        inputs.Add(aPhaseMagnitude);
                        inputs.Add(bPhaseMagnitude);
                        inputs.Add(cPhaseMagnitude);

                        // Set target adapter phasor type, i.e., voltage or current, for custom SignalTypes
                        m_outputAdapterPhasorTypes.Add(outputPhasorType);

                        // Add phasor labels to connection string for easier validation and debugging of phasor label matching
                        m_customAdapterSettings.Add($"A-Phase={labels[aPhaseIndex]}; B-Phase={labels[bPhaseIndex]}; C-Phase={labels[cPhaseIndex]}");
                    }
                }

                addSequenceCalculatorInputs(avPhaseIndexes, bvMatches, cvMatches, PhasorType.Voltage);
                addSequenceCalculatorInputs(aiPhaseIndexes, biMatches, ciMatches, PhasorType.Current);
            }

            if (incompleteCount > 0)
                OnStatusMessage(MessageLevel.Warning, $"{incompleteCount:N0} of the source 'A', 'B', 'C' phase sets were incomplete and were excluded as input.");

            if (duplicatedMatches.Count > 0)
                OnStatusMessage(MessageLevel.Warning, $"{duplicatedMatches.Count:N0} of the source device's 'A', 'B', 'C' phase sets had duplicated matches. Devices with the following IDs should have phasor labels updated for better phasor matching:{Environment.NewLine}{string.Join(", ", duplicatedMatches)}");

            if (inputs.Count % PerAdapterInputCount != 0)
                OnStatusMessage(MessageLevel.Warning, $"Unexpected number of input {inputs.Count:N0} for {PerAdapterInputCount:N0} inputs per adapter.");

            // Define properly ordered and associated set of inputs
            InputMeasurementKeys = inputs.ToArray();

            InitializeChildAdapterManagement();
        }

        #endregion
    }
}
