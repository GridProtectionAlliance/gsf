//******************************************************************************************************
//  DisturbanceCategory.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  09/04/2019 - Christoph Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Disturbance Categories (as defined in IEEE 1159).
    /// </summary>
    public static class DisturbanceCategory
    {
        /// <summary>
        /// The ID for no distrubance or undefined.
        /// </summary>
        public static readonly Guid None = new Guid("67f6af8f-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 Transient.
        /// </summary>
        public static readonly Guid Transient = new Guid("67f6af90-f753-0x11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 Impulsive Transient.
        /// </summary>
        public static readonly Guid ImpulsiveTransient = new Guid("dd56ef60-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Impulsive Transient with nanosecond duration.
        /// </summary>
        public static readonly Guid ImpulsiveTransient_nano = new Guid("dd56ef61-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Impulsive Transient with microsecond duration.
        /// </summary>
        public static readonly Guid ImpulsiveTransient_micro = new Guid("dd56ef63-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Impulsive Transient with milisecond duration.
        /// </summary>

        public static readonly Guid ImpulsiveTransient_mili = new Guid("dd56ef64-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Oscillatory Transient.
        /// </summary>
        public static readonly Guid OscillatoryTransient = new Guid("dd56ef65-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Low Frequency Oscillatory Transient.
        /// </summary>
        public static readonly Guid OscillatoryTransient_low = new Guid("dd56ef66-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Medium Frequency Oscillatory Transient.
        /// </summary>
        public static readonly Guid OscillatoryTransient_medium = new Guid("dd56ef67-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 High Frequency Oscillatory Transient.
        /// </summary>
        public static readonly Guid OscillatoryTransient_high = new Guid("dd56ef68-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation
        /// </summary>n
        public static readonly Guid RMSVariationShortDuration  = new Guid("67f6af91-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Instantaneous duration.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_Instantaneous = new Guid("dd56ef69-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Instantaneous Sag.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_InstantaneousSag = new Guid("dd56ef6a-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Instantaneous Swell.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_InstantaneousSwell = new Guid("dd56ef6b-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Momentary Duration.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_Momentary = new Guid("dd56ef6c-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Momentary Interruption.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_MomentaryInterruption = new Guid("dd56ef6d-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Momentary Sag.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_MomentarySag = new Guid("dd56ef6e-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Momentary Swell.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_MomentarySwell = new Guid("dd56ef6f-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159Short Duration RMS Variation - Temporary Duration.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_Temporary = new Guid("dd56ef70-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Temporary Interruption.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_TemporaryInterruption = new Guid("dd56ef71-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Temporary Sag.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_TemporarySag = new Guid("dd56ef72-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Short Duration RMS Variation - Temporary Swell.
        /// </summary>
        public static readonly Guid RMSVariationShortDuration_TemporarySwell = new Guid("dd56ef73-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159  Long Duration RMS Variation.
        /// </summary>
        public static readonly Guid RMSVariationLongDuration = new Guid("67f6af92-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 Long Duration RMS Variation - Interruption.
        /// </summary>
        public static readonly Guid RMSVariationLongDuration_Interrruption = new Guid("dd56ef74-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Long Duration RMS Variation - Undervoltage.
        /// </summary>
        public static readonly Guid RMSVariationLongDuration_UnderVoltage = new Guid("dd56ef75-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Long Duration RMS Variation - Overvoltage.
        /// </summary>
        public static readonly Guid RMSVariationLongDuration_OverVoltage = new Guid("dd56ef76-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Imbalance.
        /// </summary>
        public static readonly Guid Imbalance = new Guid("dd56ef77-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Power Frequency Variation.
        /// </summary>
        public static readonly Guid PowerFrequencyVariation = new Guid("dd56ef7e-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Voltage Fluctuation.
        /// </summary>
        public static readonly Guid VoltageFuctuation = new Guid("67f6af93-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 Waveform Distortion.
        /// </summary>
        public static readonly Guid WaveformDistortion = new Guid("67f6af94-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 DC offset of voltage or current waveform.
        /// </summary>
        public static readonly Guid DCoffset = new Guid("dd56ef78-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Waveform Harmonics Present.
        /// </summary>
        public static readonly Guid WaveformHarmonics = new Guid("dd56ef79-7edd-11d2-b30a-00609789d193");

        /// <summary>
        /// The ID for a IEEE 1159 Waveform Interharmonics Present.
        /// </summary>

        public static readonly Guid WaveformInterHarmonics = new Guid("dd56ef7a-7edd-11d2-b30a-00609789d193");
        /// <summary>
        /// The ID for a IEEE 1159 Waveform Notching Present.
        /// </summary>
        public static readonly Guid WaveformNotching = new Guid("67f6af95-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for a IEEE 1159 Waveform Noise Present.
        /// </summary>
        public static readonly Guid WaveformNoise = new Guid("67f6af96-f753-11cf-9d89-0080c72e70a3");



        /// <summary>
        /// Gets information about the Disturbance identified by the given ID.
        /// </summary>
        /// <param name="disturbanceCategoryID">Globally unique identifier for the Disturbance Category.</param>
        /// <returns>The information about the vendor.</returns>
        public static Identifier GetInfo(Guid disturbanceCategoryID)
        {
            Identifier identifier;
            return DisturbanceLookup.TryGetValue(disturbanceCategoryID, out identifier) ? identifier : null;
        }

        /// <summary>
        /// Converts the given Disturbance ID to a string containing the name of the Disturbance.
        /// </summary>
        /// <param name="disturbanceCategoryID">The ID of the Disturbance to be converted to a string.</param>
        /// <returns>A string containing the name of the Disturbance Category with the given ID.</returns>
        public static string ToString(Guid disturbanceCategoryID)
        {
            return GetInfo(disturbanceCategoryID)?.Name ?? disturbanceCategoryID.ToString();
        }

        private static Dictionary<Guid, Identifier> DisturbanceLookup
        {
            get
            {
                Tag disturbanceTag = Tag.GetTag(ObservationRecord.DisturbanceCategoryTag);

                if (s_disturbanceTag != disturbanceTag)
                {
                    s_disturbanceLookup = disturbanceTag.ValidIdentifiers.ToDictionary(id => Guid.Parse(id.Value));
                    s_disturbanceTag = disturbanceTag;
                }

                return s_disturbanceLookup;
            }
        }

        private static Tag s_disturbanceTag;
        private static Dictionary<Guid, Identifier> s_disturbanceLookup;
    }
}
