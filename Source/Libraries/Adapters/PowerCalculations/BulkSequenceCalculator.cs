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
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.FuzzyStrings;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units.EE;
using PhasorProtocolAdapters;
using MeasurementRecord = GSF.TimeSeries.Model.Measurement;
using PhasorRecord = GSF.TimeSeries.Model.Phasor;
using DeviceRecord = GSF.TimeSeries.Model.Device;
using SignalType = GSF.Units.EE.SignalType;
using PhaseDetail = System.Tuple<GSF.TimeSeries.MeasurementKey, GSF.Units.EE.SignalType, GSF.TimeSeries.Model.Measurement, GSF.TimeSeries.Model.Phasor>;
using static PowerCalculations.SequenceCalculator.Output;

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

        // Nested Types
        private class AdapterDetail
        {
            public PhasorType PhasorType { get; set; }
            public string CustomSettings { get; set; }
            public string CompanyAcronym { get; set; }
            public string DeviceAcronym { get; set; }
            public string DeviceName { get; set; }
            public int DeviceID { get; set; }
            public int SourcePhaseCount { get; set; }
            public string PhasorLabel { get; set; }
            public int BaseKV { get; set; }
        }

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="IndependentActionAdapterManagerBase{TAdapter}.InputMeasurementKeys"/>.
        /// </summary>
        public const string DefaultInputMeasurementKeys = "FILTER ActiveMeasurements WHERE SignalType LIKE '%PH%' AND Phase IN ('A', 'B', 'C') ORDER BY PhasorID";

        // Fields
        private readonly List<AdapterDetail> m_adapterDetails;
        private ReadOnlyCollection<string> m_perAdapterOutputNames;
        private bool m_forceCalcSignalType;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates new <see cref="BulkSequenceCalculator"/>.
        /// </summary>
        public BulkSequenceCalculator()
        {
            m_adapterDetails = new List<AdapterDetail>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if the last few values should be monitored.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Flag that determines if the last few values should be monitored.")]
        [DefaultValue(true)]
        public bool TrackRecentValues { get; set; } = false;

        /// <summary>
        /// Gets or sets the sample size of the data to be monitored.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the sample size of the data to be monitored.")]
        [DefaultValue(5)]
        public int SampleSize { get; set; } = 5;

        /// <summary>
        /// Gets or sets flag that determines if positive sequence calculations should be included.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Flag that determines if positive sequence calculations should be included.")]
        [DefaultValue(true)]
        public bool IncludePositiveSequence { get; set; } = true;

        /// <summary>
        /// Gets or sets flag that determines if negative sequence calculations should be included.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Flag that determines if negative sequence calculations should be included.")]
        [DefaultValue(true)]
        public bool IncludeNegativeSequence { get; set; } = true;

        /// <summary>
        /// Gets or sets flag that determines if zero sequence calculations should be included.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Flag that determines if zero sequence calculations should be included.")]
        [DefaultValue(true)]
        public bool IncludeZeroSequence { get; set; } = true;

        /// <summary>
        /// Gets or sets flag that determines if calculations should use data even when the bad quality flag is set. When value is false and any input has bad quality, calculation will be skipped.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Flag that determines if calculations should use data even when the bad quality flag is set. When value is false and any input has bad quality, calculation will be skipped.")]
        [DefaultValue(false)]
        public bool UseBadQualityData { get; set; }

        /// <summary>
        /// Gets number of input measurements required by each adapter.
        /// </summary>
        public override int PerAdapterInputCount => 6;

        /// <summary>
        /// Gets or sets the index into the per adapter input measurements to use for target adapter name.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - value not used by this bulk calculator (see PointTagTemplate)
        public override int InputMeasurementIndexUsedForName { get; set; } = 0;

        /// <summary>
        /// Gets output measurement names.
        /// </summary>
        public override ReadOnlyCollection<string> PerAdapterOutputNames
        {
            get
            {
                if (Initialized && m_perAdapterOutputNames != null)
                    return m_perAdapterOutputNames;

                ReadOnlyCollection<string> generatePerAdapterOutputNames()
                {
                    if (IncludePositiveSequence && IncludeNegativeSequence && IncludeZeroSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(PositiveSequenceMagnitude)}",
                            $"{nameof(PositiveSequenceAngle)}",
                            $"{nameof(NegativeSequenceMagnitude)}",
                            $"{nameof(NegativeSequenceAngle)}",
                            $"{nameof(ZeroSequenceMagnitude)}",
                            $"{nameof(ZeroSequenceAngle)}"
                        });

                    if (IncludePositiveSequence && IncludeNegativeSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(PositiveSequenceMagnitude)}",
                            $"{nameof(PositiveSequenceAngle)}",
                            $"{nameof(NegativeSequenceMagnitude)}",
                            $"{nameof(NegativeSequenceAngle)}"
                        });

                    if (IncludePositiveSequence && IncludeZeroSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(PositiveSequenceMagnitude)}",
                            $"{nameof(PositiveSequenceAngle)}",
                            $"{nameof(ZeroSequenceMagnitude)}",
                            $"{nameof(ZeroSequenceAngle)}"
                        });

                    if (IncludeNegativeSequence && IncludeZeroSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(NegativeSequenceMagnitude)}",
                            $"{nameof(NegativeSequenceAngle)}",
                            $"{nameof(ZeroSequenceMagnitude)}",
                            $"{nameof(ZeroSequenceAngle)}"
                        });

                    if (IncludePositiveSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(PositiveSequenceMagnitude)}",
                            $"{nameof(PositiveSequenceAngle)}"
                        });

                    if (IncludeNegativeSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(NegativeSequenceMagnitude)}",
                            $"{nameof(NegativeSequenceAngle)}"
                        });

                    if (IncludeZeroSequence)
                        return Array.AsReadOnly(new[]
                        {
                            $"{nameof(ZeroSequenceMagnitude)}",
                            $"{nameof(ZeroSequenceAngle)}"
                        });

                    return Array.AsReadOnly(Array.Empty<string>());
                }

                m_perAdapterOutputNames = generatePerAdapterOutputNames();

                return m_perAdapterOutputNames;
            }
        }

        /// <summary>
        /// Gets output phase for each output measurement.
        /// </summary>
        public char[] OutputPhases
        {
            get
            {
                if (CurrentAdapterIndex > -1 && CurrentAdapterIndex < m_adapterDetails.Count)
                {
                    if (IncludePositiveSequence && IncludeNegativeSequence && IncludeZeroSequence)
                        return new[] { '+', '+', '-', '-', '0', '0' };

                    if (IncludePositiveSequence && IncludeNegativeSequence)
                        return new[] { '+', '+', '-', '-' };

                    if (IncludePositiveSequence && IncludeZeroSequence)
                        return new[] { '+', '+', '0', '0' };

                    if (IncludeNegativeSequence && IncludeZeroSequence)
                        return new[] { '-', '-', '0', '0' };

                    if (IncludePositiveSequence)
                        return new[] { '+', '+' };

                    if (IncludeNegativeSequence)
                        return new[] { '-', '-' };

                    if (IncludeZeroSequence)
                        return new[] { '0', '0' };
                }

                return Array.Empty<char>();
            }
        }

        /// <summary>
        /// Gets or sets default signal type to use for all output measurements when <see cref="SignalTypes"/> array is not defined.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - value not used by this bulk calculator (see SignalTypes override)
        public override SignalType SignalType { get => base.SignalType; set => base.SignalType = value; }

        /// <summary>
        /// Gets or sets flag that determines if all output signal types should be forced to CALC.
        /// </summary>
        [ConnectionStringParameter]
        [Description("STTP usage flag that determines if all output signal types should be forced to CALC. This is required if you are wanting to create local sequence calculations using data received from GEP or STTP - otherwise output values associated with source device with targeted signal types will appear as foreign measurements, i.e., non-existent on publisher, and be deleted during metadata synchronization.")]
        [DefaultValue(false)]
        public bool ForceCalcSignalType
        {
            get => m_forceCalcSignalType;
            set
            {
                m_forceCalcSignalType = value;

                if (m_forceCalcSignalType)
                    SignalType = SignalType.CALC;
            }
        }

        /// <summary>
        /// Gets signal type for each output measurement, used when each output needs to be a different type.
        /// </summary>
        public override SignalType[] SignalTypes => ForceCalcSignalType ? null : GetSignalTypes();

        private SignalType[] GetSignalTypes()
        {
            if (CurrentAdapterIndex <= -1 || CurrentAdapterIndex >= m_adapterDetails.Count)
                return null;

            switch (m_adapterDetails[CurrentAdapterIndex].PhasorType)
            {
                case PhasorType.Voltage:
                {
                    switch (PerAdapterOutputNames.Count)
                    {
                        case 6:
                            return new[] { SignalType.VPHM, SignalType.VPHA, SignalType.VPHM, SignalType.VPHA, SignalType.VPHM, SignalType.VPHA };
                        case 4:
                            return new[] { SignalType.VPHM, SignalType.VPHA, SignalType.VPHM, SignalType.VPHA };
                        default:
                            return new[] { SignalType.VPHM, SignalType.VPHA };
                    }
                }
                case PhasorType.Current:
                {
                    switch (PerAdapterOutputNames.Count)
                    {
                        case 6:
                            return new[] { SignalType.IPHM, SignalType.IPHA, SignalType.IPHM, SignalType.IPHA, SignalType.IPHM, SignalType.IPHA };
                        case 4:
                            return new[] { SignalType.IPHM, SignalType.IPHA, SignalType.IPHM, SignalType.IPHA };
                        default:
                            return new[] { SignalType.IPHM, SignalType.IPHA };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets any custom adapter settings to be added to each adapter connection string. Can be used to add
        /// settings that are custom per adapter.
        /// </summary>
        public override string CustomAdapterSettings
        {
            get
            {
                if (CurrentAdapterIndex > -1 && CurrentAdapterIndex < m_adapterDetails.Count)
                    return m_adapterDetails[CurrentAdapterIndex].CustomSettings;

                return null;
            }
        }

        /// <summary>
        /// Gets or sets template for output measurement point tag names.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - value managed automatically
        public override string PointTagTemplate
        {
            get
            {
                if (CurrentAdapterIndex < 0 || CurrentAdapterIndex >= m_adapterDetails.Count || CurrentOutputIndex < 0 || CurrentOutputIndex >= PerAdapterOutputNames.Count)
                    return base.PointTagTemplate;

                AdapterDetail adapterDetail = m_adapterDetails[CurrentAdapterIndex];
                SignalType signalType = GetSignalTypes()?[CurrentOutputIndex] ?? SignalType;
                int signalIndex = adapterDetail.SourcePhaseCount + CurrentAdapterIndex * PerAdapterOutputNames.Count + CurrentOutputIndex + 1;

                return CommonPhasorServices.CreatePointTag(adapterDetail.CompanyAcronym, adapterDetail.DeviceAcronym, null, signalType.ToString(), adapterDetail.PhasorLabel, signalIndex, OutputPhases[CurrentOutputIndex], adapterDetail.BaseKV);
            }
            set => base.PointTagTemplate = value;
        }

        /// <summary>
        /// Gets or sets template for local signal reference measurement name for source historian point.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - value managed automatically
        public override string SignalReferenceTemplate
        {
            get
            {
                if (CurrentAdapterIndex < 0 || CurrentAdapterIndex >= m_adapterDetails.Count || CurrentOutputIndex < 0 || CurrentOutputIndex >= PerAdapterOutputNames.Count)
                    return base.SignalReferenceTemplate;

                AdapterDetail adapterDetail = m_adapterDetails[CurrentAdapterIndex];
                SignalType signalType = GetSignalTypes()?[CurrentOutputIndex] ?? SignalType;
                SignalKind signalKind;
                int signalIndex;

                switch (signalType)
                {
                    case SignalType.VPHA:
                    case SignalType.VPHM:
                        signalIndex = adapterDetail.SourcePhaseCount + CurrentOutputIndex + 1 - CurrentOutputIndex % 2;
                        break;
                    case SignalType.IPHA:
                    case SignalType.IPHM:
                        signalIndex = adapterDetail.SourcePhaseCount + PerAdapterOutputNames.Count / 2 + (CurrentOutputIndex + 1 - CurrentOutputIndex % 2);
                        break;
                    default:
                        signalIndex = adapterDetail.SourcePhaseCount + CurrentAdapterIndex * PerAdapterOutputNames.Count + CurrentOutputIndex + 1;
                        break;
                }

                switch (signalType)
                {
                    case SignalType.VPHA:
                    case SignalType.IPHA:
                        signalKind = SignalKind.Angle;
                        break;
                    case SignalType.VPHM:
                    case SignalType.IPHM:
                        signalKind = SignalKind.Magnitude;
                        break;
                    default:
                        signalKind = SignalKind.Calculation;
                        break;
                }

                return SignalReference.ToString(adapterDetail.DeviceAcronym, signalKind, signalIndex);
            }
            set => base.SignalReferenceTemplate = value;
        }

        /// <summary>
        /// Gets or sets template for output measurement descriptions.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - value managed automatically
        public override string DescriptionTemplate
        {
            get
            {
                if (CurrentAdapterIndex < 0 || CurrentAdapterIndex >= m_adapterDetails.Count || CurrentOutputIndex < 0 || CurrentOutputIndex >= PerAdapterOutputNames.Count)
                    return base.DescriptionTemplate;

                AdapterDetail adapterDetail = m_adapterDetails[CurrentAdapterIndex];
                string phaseKind, measurementKind;

                switch (OutputPhases[CurrentOutputIndex])
                {
                    case '+':
                        phaseKind = "Positive";
                        break;
                    case '-':
                        phaseKind = "Negative";
                        break;
                    case '0':
                        phaseKind = "Zero";
                        break;
                    default:
                        phaseKind = "Undetermined";
                        break;
                }

                switch (GetSignalTypes()?[CurrentOutputIndex] ?? SignalType)
                {
                    case SignalType.VPHA:
                    case SignalType.IPHA:
                        measurementKind = $"{adapterDetail.PhasorType} Phase Angle";
                        break;
                    case SignalType.VPHM:
                    case SignalType.IPHM:
                        measurementKind = $"{adapterDetail.PhasorType} Magnitude";
                        break;
                    default:
                        measurementKind = $"{adapterDetail.PhasorType} Calculation";
                        break;
                }

                return $"{adapterDetail.DeviceName} {adapterDetail.PhasorLabel} {phaseKind} Sequence {measurementKind}";
            }
            set => base.DescriptionTemplate = value;
        }

        /// <summary>
        /// Gets or sets template for the parent device acronym used to group associated output measurements.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Hiding parameter from manager - all outputs automatically associated with input device
        public override string ParentDeviceAcronymTemplate
        {
            get => null;
            set => base.ParentDeviceAcronymTemplate = value;
        }

        /// <summary>
        /// Gets associated device ID for <see cref="IndependentActionAdapterManagerBase{TAdapter}.CurrentAdapterIndex"/>, if any, for measurement generation.
        /// </summary>
        public override int CurrentDeviceID
        {
            get
            {
                if (CurrentAdapterIndex > -1 && CurrentAdapterIndex < m_adapterDetails.Count)
                    return m_adapterDetails[CurrentAdapterIndex].DeviceID;

                return base.CurrentDeviceID;
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

            if (!settings.TryGetValue(nameof(InputMeasurementKeys), out string setting) || string.IsNullOrWhiteSpace(setting))
                settings[nameof(InputMeasurementKeys)] = DefaultInputMeasurementKeys;

            base.ParseConnectionString();

            // Get a local copy of the input keys as these will change often during initialization
            MeasurementKey[] inputMeasurementKeys = InputMeasurementKeys;
            SignalType[] inputMeasurementKeyTypes = InputMeasurementKeyTypes;

            if (inputMeasurementKeys.Length == 0)
            {
                OnStatusMessage(MessageLevel.Error, "No inputs were configured. Cannot initialize adapter.");
                return;
            }

            if (inputMeasurementKeys.Length != inputMeasurementKeyTypes.Length)
            {
                OnStatusMessage(MessageLevel.Error, "Parallel input measurement keys and type array lengths do not match. Cannot initialize adapter.");
                return;
            }

            // The goal of the following code is to create the needed ordered phase set input, i.e., the A, B and C phase angle/magnitude measurements,
            // as required by an individual SequenceCalculator. Since this is a "bulk" calculator, the code operates against all system inputs unless
            // further restricted by user provided filtering expression as set in the InputMeasurementKeys connection string property.

            Dictionary<int, List<PhaseDetail>> devicePhaseDetails = new Dictionary<int, List<PhaseDetail>>();

            // Build proper set of inputs where phases are grouped together
            List<MeasurementKey> inputs = new List<MeasurementKey>();
            HashSet<int> duplicatedMatches = new HashSet<int>();
            int incompleteCount = 0;

            using (AdoDataConnection connection = GetConfiguredConnection())
            {
                TableOperations<MeasurementRecord> measurementTable = new TableOperations<MeasurementRecord>(connection);
                TableOperations<PhasorRecord> phasorTable = new TableOperations<PhasorRecord>(connection);
                TableOperations<DeviceRecord> deviceTable = new TableOperations<DeviceRecord>(connection);

                for (int i = 0; i < inputMeasurementKeys.Length; i++)
                {
                    MeasurementKey key = inputMeasurementKeys[i];
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

                        // Expecting exactly two measurements per phasor source index, i.e., an angle and a magnitude, track for verification
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

                    if (phasorSourceIndexCounts.Values.Any(count => count != 2))
                    {
                        OnStatusMessage(MessageLevel.Warning, $"Measurement phasor source index count mismatch encountered for one or more phasors associated with device ID {deviceID:N0} for {nameof(BulkSequenceCalculator)}. Two measurements, one angle and one magnitude, are expected per phasor source index, input excluded.");
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

                            int aPhaseAngleIndex = aPhaseIndexes[i];
                            int bPhaseAngleIndex = bMatches[i][0];
                            int cPhaseAngleIndex = cMatches[i][0];

                            MeasurementKey aPhaseAngle = getAngleMeasurement(aPhaseAngleIndex);
                            MeasurementKey bPhaseAngle = getAngleMeasurement(bPhaseAngleIndex);
                            MeasurementKey cPhaseAngle = getAngleMeasurement(cPhaseAngleIndex);

                            if (!tryGetMagnitudeMeasurement(aPhaseAngleIndex, out MeasurementKey aPhaseMagnitude))
                            {
                                incompleteCount++;
                                continue;
                            }

                            if (!tryGetMagnitudeMeasurement(bPhaseAngleIndex, out MeasurementKey bPhaseMagnitude))
                            {
                                incompleteCount++;
                                continue;
                            }

                            if (!tryGetMagnitudeMeasurement(cPhaseAngleIndex, out MeasurementKey cPhaseMagnitude))
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

                            // Capture meta-data detail about adapter and outputs to create better measurement information
                            string companyAcronym = null;
                            string deviceAcronym = null;
                            string deviceName = null;
                            int sourcePhaseCount = 0;

                            DeviceRecord device = deviceTable.QueryRecordWhere("ID = {0}", deviceID);

                            if (device != null)
                            {
                                deviceAcronym = device.Acronym;
                                deviceName = device.Name;

                                // Lookup company acronym of associated device record
                                string result = connection.ExecuteScalar<string>("SELECT Acronym FROM Company WHERE ID = {0}", device.CompanyID);

                                if (!string.IsNullOrWhiteSpace(result))
                                    companyAcronym = result;

                                // Get maximum source index value from device's phasor records
                                int count = connection.ExecuteScalar<int>("SELECT MAX(SourceIndex) FROM Phasor WHERE DeviceID = {0}", device.ID);

                                if (count > 0)
                                    sourcePhaseCount = count;
                            }

                            if (string.IsNullOrWhiteSpace(companyAcronym))
                                companyAcronym = s_companyAcronym;

                            if (string.IsNullOrWhiteSpace(deviceAcronym))
                                deviceAcronym = Name;

                            if (string.IsNullOrWhiteSpace(deviceName))
                                deviceName = deviceAcronym;

                            if (sourcePhaseCount == 0)
                                sourcePhaseCount = 3;

                            m_adapterDetails.Add(new AdapterDetail
                            {
                                // Set target adapter phasor type, i.e., voltage or current, for custom SignalTypes
                                PhasorType = outputPhasorType,

                                // Add phasor labels to connection string for easier validation and debugging of phasor label matching
                                CustomSettings = $"A-Phase={labels[aPhaseAngleIndex]}; B-Phase={labels[bPhaseAngleIndex]}; C-Phase={labels[cPhaseAngleIndex]}",

                                CompanyAcronym = companyAcronym,
                                DeviceAcronym = deviceAcronym,
                                DeviceName = deviceName,
                                DeviceID = deviceID,
                                PhasorLabel = labels[aPhaseAngleIndex],
                                SourcePhaseCount = sourcePhaseCount,
                                BaseKV = phaseDetails[aPhaseAngleIndex].Item4.BaseKV
                            });
                        }
                    }

                    addSequenceCalculatorInputs(avPhaseIndexes, bvMatches, cvMatches, PhasorType.Voltage);
                    addSequenceCalculatorInputs(aiPhaseIndexes, biMatches, ciMatches, PhasorType.Current);
                }
            }

            if (incompleteCount > 0)
                OnStatusMessage(MessageLevel.Warning, $"{incompleteCount:N0} of the source 'A', 'B', 'C' phase sets were incomplete and were excluded as input.");

            if (duplicatedMatches.Count > 0)
                OnStatusMessage(MessageLevel.Warning, $"{duplicatedMatches.Count:N0} of the source device's 'A', 'B', 'C' phase sets had duplicated matches. Devices with the following IDs should have phasor labels updated for improved phasor matching:{Environment.NewLine}{string.Join(", ", duplicatedMatches)}");

            if (inputs.Count % PerAdapterInputCount != 0)
                OnStatusMessage(MessageLevel.Warning, $"Unexpected number of input {inputs.Count:N0} for {PerAdapterInputCount:N0} inputs per adapter.");

            if (inputs.Count == 0)
            {
                OnStatusMessage(MessageLevel.Warning, "No valid inputs were defined. Cannot initialize adapter.");
                return;
            }

            // Define properly ordered and associated set of inputs
            inputMeasurementKeys = inputs.ToArray();
            
            // Setup child adapters
            InitializeChildAdapterManagement(inputMeasurementKeys);
            
            // Update external routing tables to only needed inputs
            InputMeasurementKeys = inputMeasurementKeys;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string s_companyAcronym;

        // Static Constructor
        static BulkSequenceCalculator()
        {
            try
            {
                CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
                s_companyAcronym = systemSettings["CompanyAcronym"]?.Value;

                if (string.IsNullOrWhiteSpace(s_companyAcronym))
                    s_companyAcronym = "GPA";
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex, "Failed to initialize default company acronym");
            }
        }

        #endregion
    }
}
