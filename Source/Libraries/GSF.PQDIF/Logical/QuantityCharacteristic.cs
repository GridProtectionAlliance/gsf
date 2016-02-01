//******************************************************************************************************
//  QuantityCharacteristic.cs - Gbtc
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
//  05/20/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Specifies additional detail about the meaning of the series data.
    /// </summary>
    public static class QuantityCharacteristic
    {
        /// <summary>
        /// No quantity characteristic.
        /// </summary>
        public static readonly Guid None = new Guid("a6b31adf-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Instantaneous f(t).
        /// </summary>
        public static readonly Guid Instantaneous = new Guid("a6b31add-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Spectra F(F).
        /// </summary>
        public static readonly Guid Spectra = new Guid("a6b31ae9-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Peak value.
        /// </summary>
        public static readonly Guid Peak = new Guid("a6b31ae2-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// RMS value.
        /// </summary>
        public static readonly Guid RMS = new Guid("a6b31ae5-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Harmonic RMS.
        /// </summary>
        public static readonly Guid HRMS = new Guid("a6b31adc-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Frequency.
        /// </summary>
        public static readonly Guid Frequency = new Guid("07ef68af-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Total harmonic distortion (%).
        /// </summary>
        public static readonly Guid TotalTHD = new Guid("a6b31aec-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Even harmonic distortion (%).
        /// </summary>
        public static readonly Guid EvenTHD = new Guid("a6b31ad4-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Odd harmonic distortion (%).
        /// </summary>
        public static readonly Guid OddTHD = new Guid("a6b31ae0-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Crest factor.
        /// </summary>
        public static readonly Guid CrestFactor = new Guid("a6b31ad2-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Form factor.
        /// </summary>
        public static readonly Guid FormFactor = new Guid("a6b31adb-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Arithmetic sum.
        /// </summary>
        public static readonly Guid ArithSum = new Guid("a6b31ad0-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Zero sequence component unbalance (%).
        /// </summary>
        public static readonly Guid S0S1 = new Guid("a6b31ae7-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Negative sequence component unbalance (%).
        /// </summary>
        public static readonly Guid S2S1 = new Guid("a6b31ae8-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Positive sequence component.
        /// </summary>
        public static readonly Guid SPos = new Guid("a6b31aea-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Negative sequence component.
        /// </summary>
        public static readonly Guid SNeg = new Guid("d71a4b91-3c92-11d4-9f2c-002078e0b723");

        /// <summary>
        /// Zero sequence component.
        /// </summary>
        public static readonly Guid SZero = new Guid("d71a4b92-3c92-11d4-9f2c-002078e0b723");

        /// <summary>
        /// Imbalance by max deviation from average.
        /// </summary>
        public static readonly Guid AvgImbal = new Guid("a6b31ad1-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Total THD normalized to RMS.
        /// </summary>
        public static readonly Guid TotalTHDRMS = new Guid("f3d216e0-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Odd THD normalized to RMS.
        /// </summary>
        public static readonly Guid OddTHDRMS = new Guid("f3d216e1-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Even THD normalized to RMS.
        /// </summary>
        public static readonly Guid EvenTHDRMS = new Guid("f3d216e2-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Total interharmonic distortion.
        /// </summary>
        public static readonly Guid TID = new Guid("f3d216e3-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Total interharmonic distortion normalized to RMS.
        /// </summary>
        public static readonly Guid TIDRMS = new Guid("f3d216e4-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Interharmonic RMS.
        /// </summary>
        public static readonly Guid IHRMS = new Guid("f3d216e5-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Spectra by harmonic group index.
        /// </summary>
        public static readonly Guid SpectraHGroup = new Guid("53be6ba8-0789-455b-9a95-da128683dda7");

        /// <summary>
        /// Spectra by interharmonic group index.
        /// </summary>
        public static readonly Guid SpectraIGroup = new Guid("5e51e006-9c95-4c5e-878f-7ca87c0d2a0e");

        /// <summary>
        /// TIF.
        /// </summary>
        public static readonly Guid TIF = new Guid("a6b31aeb-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Flicker average RMS value.
        /// </summary>
        public static readonly Guid FlkrMagAvg = new Guid("a6b31ad6-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// dV/V base.
        /// </summary>
        public static readonly Guid FlkrMaxDVV = new Guid("a6b31ad8-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Frequence of maximum flicker harmonic.
        /// </summary>
        public static readonly Guid FlkrFreqMax = new Guid("a6b31ad5-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Magnitude of maximum flicker harmonic.
        /// </summary>
        public static readonly Guid FlkrMagMax = new Guid("a6b31ad7-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Spectrum weighted average.
        /// </summary>
        public static readonly Guid FlkrWgtAvg = new Guid("a6b31ada-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Flicker spectrum VRMS(F).
        /// </summary>
        public static readonly Guid FlkrSpectrum = new Guid("a6b31ad9-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Short term flicker.
        /// </summary>
        public static readonly Guid FlkrPST = new Guid("515bf320-71ca-11d4-a4b3-444553540000");

        /// <summary>
        /// Long term flicker.
        /// </summary>
        public static readonly Guid FlkrPLT = new Guid("515bf321-71ca-11d4-a4b3-444553540000");

        /// <summary>
        /// TIF normalized to RMS.
        /// </summary>
        public static readonly Guid TIFRMS = new Guid("f3d216e6-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Sliding PLT.
        /// </summary>
        public static readonly Guid FlkrPLTSlide = new Guid("2257ec05-06ea-4709-b43a-0c00534d554a");

        /// <summary>
        /// Pi LPF.
        /// </summary>
        public static readonly Guid FlkrPiLPF = new Guid("4d693eec-5d1d-4531-993a-793b5356c63d");

        /// <summary>
        /// Pi max.
        /// </summary>
        public static readonly Guid FlkrPiMax = new Guid("126de61c-6691-4d16-8fdf-46482bca4694");

        /// <summary>
        /// Pi root.
        /// </summary>
        public static readonly Guid FlkrPiRoot = new Guid("e065b621-ffdb-4598-9330-4d09353988b6");

        /// <summary>
        /// Pi root LPF.
        /// </summary>
        public static readonly Guid FlkrPiRootLPF = new Guid("7d11f283-1ce7-4e58-8af0-79048793b8a7");

        /// <summary>
        /// IT.
        /// </summary>
        public static readonly Guid IT = new Guid("a6b31ade-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// RMS value of current for a demand interval.
        /// </summary>
        public static readonly Guid RMSDemand = new Guid("07ef68a0-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Transformer derating factor.
        /// </summary>
        public static readonly Guid ANSITDF = new Guid("8786ca10-9113-11d3-b930-0050da2b1f4d");

        /// <summary>
        /// Transformer K factor.
        /// </summary>
        public static readonly Guid KFactor = new Guid("8786ca11-9113-11d3-b930-0050da2b1f4d");

        /// <summary>
        /// Total demand distortion.
        /// </summary>
        public static readonly Guid TDD = new Guid("f3d216e7-2aa5-11d5-a4b3-444553540000");

        /// <summary>
        /// Peak demand current.
        /// </summary>
        public static readonly Guid RMSPeakDemand = new Guid("72e82a44-336c-11d5-a4b3-444553540000");

        /// <summary>
        /// Real power (watts).
        /// </summary>
        public static readonly Guid P = new Guid("a6b31ae1-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Reactive power (VAR).
        /// </summary>
        public static readonly Guid Q = new Guid("a6b31ae4-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Apparent power (VA).
        /// </summary>
        public static readonly Guid S = new Guid("a6b31ae6-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// True power factor - (Vrms * Irms) / P.
        /// </summary>
        public static readonly Guid PF = new Guid("a6b31ae3-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Displacement factor - Cosine of the phase angle between fundamental frequency voltage and current phasors.
        /// </summary>
        public static readonly Guid DF = new Guid("a6b31ad3-b451-11d1-ae17-0060083a2628");

        /// <summary>
        /// Value of active power for a demand interval.
        /// </summary>
        public static readonly Guid PDemand = new Guid("07ef68a1-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of reactive power for a demand interval.
        /// </summary>
        public static readonly Guid QDemand = new Guid("07ef68a2-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of apparent power for a demand interval.
        /// </summary>
        public static readonly Guid SDemand = new Guid("07ef68a3-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of displacement power factor for a demand interval.
        /// </summary>
        public static readonly Guid DFDemand = new Guid("07ef68a4-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of true power factor for a demand interval.
        /// </summary>
        public static readonly Guid PFDemand = new Guid("07ef68a5-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Predicted value of active power for current demand interval.
        /// </summary>
        public static readonly Guid PPredDemand = new Guid("672d0305-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Predicted value of reactive power for current demand interval.
        /// </summary>
        public static readonly Guid QPredDemand = new Guid("672d0306-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Predicted value of apparent power for current demand interval.
        /// </summary>
        public static readonly Guid SPredDemand = new Guid("672d0307-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of active power coincident with reactive power demand.
        /// </summary>
        public static readonly Guid PCoQDemand = new Guid("672d030a-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of active power coincident with apparent power demand.
        /// </summary>
        public static readonly Guid PCoSDemand = new Guid("672d030b-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power coincident with active power demand.
        /// </summary>
        public static readonly Guid QCoPDemand = new Guid("672d030d-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power coincident with apparent power demand.
        /// </summary>
        public static readonly Guid QCoSDemand = new Guid("672d030e-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of displacement power factor coincident with apparent power demand.
        /// </summary>
        public static readonly Guid DFCoSDemand = new Guid("07ef68ad-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of true power factor coincident with apparent power demand.
        /// </summary>
        public static readonly Guid PFCoSDemand = new Guid("07ef68ae-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of true power factor coincident with active power demand.
        /// </summary>
        public static readonly Guid PFCoPDemand = new Guid("672d0308-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of true power factor coincident with reactive power demand.
        /// </summary>
        public static readonly Guid PFCoQDemand = new Guid("672d0309-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of the power angle at fundamental frequency. 
        /// </summary>
        public static readonly Guid AngleFund = new Guid("672d030f-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of the reactive power at fundamental frequency.
        /// </summary>
        public static readonly Guid QFund = new Guid("672d0310-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// True power factor - IEEE vector calculations.
        /// </summary>
        public static readonly Guid PFVector = new Guid("672d0311-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Displacement factor - IEEE vector calculations.
        /// </summary>
        public static readonly Guid DFVector = new Guid("672d0312-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of apparent power - IEEE vector calculations.
        /// </summary>
        public static readonly Guid SVector = new Guid("672d0314-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency apparent power - IEEE vector calculations.
        /// </summary>
        public static readonly Guid SVectorFund = new Guid("672d0315-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency apparent power.
        /// </summary>
        public static readonly Guid SFund = new Guid("672d0316-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Apparent power coincident with active power demand.
        /// </summary>
        public static readonly Guid SCoPDemand = new Guid("672d0317-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Apparent power coincident with reactive power demand.
        /// </summary>
        public static readonly Guid SCoQDemand = new Guid("672d0318-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// True power factor - IEEE arithmetic calculations.
        /// </summary>
        public static readonly Guid PFArith = new Guid("1c39fb00-a6aa-11d4-a4b3-444553540000");

        /// <summary>
        /// Displacement factor - IEEE arithmetic calculations.
        /// </summary>
        public static readonly Guid DFArith = new Guid("1c39fb01-a6aa-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of apparent power - IEEE arithmetic calculations.
        /// </summary>
        public static readonly Guid SArith = new Guid("1c39fb02-a6aa-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency apparent power - IEEE arithmetic calculations.
        /// </summary>
        public static readonly Guid SArithFund = new Guid("1c39fb03-a6aa-11d4-a4b3-444553540000");

        /// <summary>
        /// Peak apparent power demand.
        /// </summary>
        public static readonly Guid SPeakDemand = new Guid("72e82a43-336c-11d5-a4b3-444553540000");

        /// <summary>
        /// Peak reactive power demand.
        /// </summary>
        public static readonly Guid QPeakDemand = new Guid("72e82a42-336c-11d5-a4b3-444553540000");

        /// <summary>
        /// Peak active power demand.
        /// </summary>
        public static readonly Guid PPeakDemand = new Guid("72e82a41-336c-11d5-a4b3-444553540000");

        /// <summary>
        /// Net harmonic active power.
        /// </summary>
        public static readonly Guid PHarmonic = new Guid("b82b5c80-55c7-11d5-a4b3-444553540000");

        /// <summary>
        /// Arithmetic sum harmonic active power.
        /// </summary>
        public static readonly Guid PHarmonicUnsigned = new Guid("b82b5c81-55c7-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency real power.
        /// </summary>
        public static readonly Guid PFund = new Guid("1cdda475-1ebb-42d8-8087-d01b0b5cfa97");

        /// <summary>
        /// Value of active power integrated over time (Energy - watt-hours).
        /// </summary>
        public static readonly Guid PIntg = new Guid("07ef68a6-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of active power integrated over time (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid PIntgPos = new Guid("07ef68a7-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of active fundamental frequency power integrated over time
        /// (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid PIntgPosFund = new Guid("672d0300-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of active power integrated over time (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid PIntgNeg = new Guid("07ef68a8-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of active fundamental frequency power integrated over time
        /// (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid PIntgNegFund = new Guid("672d0301-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power integrated over time (var-hours).
        /// </summary>
        public static readonly Guid QIntg = new Guid("07ef68a9-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of reactive power integrated over time (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid QIntgPos = new Guid("07ef68aa-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of fundamental frequency reactive power integrated over time
        /// (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid QIntgPosFund = new Guid("672d0303-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency reactive power integrated over time
        /// (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid QIntgNegFund = new Guid("672d0304-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power integrated over time (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid QIntgNeg = new Guid("07ef68ab-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of apparent power integrated over time (VA-hours).
        /// </summary>
        public static readonly Guid SIntg = new Guid("07ef68ac-9ff5-11d2-b30b-006008b37183");

        /// <summary>
        /// Value of fundamental frequency apparent power integrated over time (VA-hours).
        /// </summary>
        public static readonly Guid SIntgFund = new Guid("672d0313-7810-11d4-a4b3-444553540000");

        /// <summary>
        /// Value of active power integrated over time (Energy - watt-hours).
        /// </summary>
        public static readonly Guid PIVLIntg = new Guid("f098a9a0-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of active power integrated over time (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid PIVLIntgPos = new Guid("f098a9a1-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of active fundamental frequency power integrated over time
        /// (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid PIVLIntgPosFund = new Guid("f098a9a2-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of active power integrated over time (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid PIVLIntgNeg = new Guid("f098a9a3-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of active fundamental frequency power integrated over time
        /// (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid PIVLIntgNegFund = new Guid("f098a9a4-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power integrated over time (var-hours).
        /// </summary>
        public static readonly Guid QIVLIntg = new Guid("f098a9a5-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power integrated over time (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid QIVLIntgPos = new Guid("f098a9a6-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency reactive power integrated over time
        /// (Energy - watt-hours) in the positive direction (toward load).
        /// </summary>
        public static readonly Guid QIVLIntgPosFund = new Guid("f098a9a7-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency reactive power integrated over time
        /// (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid QIVLIntgNegFund = new Guid("f098a9a8-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of reactive power integrated over time (Energy - watt-hours) in the negative direction (away from load).
        /// </summary>
        public static readonly Guid QIVLIntgNeg = new Guid("f098a9a9-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of apparent power integrated over time (VA-hours).
        /// </summary>
        public static readonly Guid SIVLIntg = new Guid("f098a9aa-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// Value of fundamental frequency apparent power integrated over time (VA-hours).
        /// </summary>
        public static readonly Guid SIVLIntgFund = new Guid("f098a9ab-3ee4-11d5-a4b3-444553540000");

        /// <summary>
        /// D axis components.
        /// </summary>
        public static readonly Guid DAxisField = new Guid("d347ba65-e34c-11d4-82d9-00e09872a094");

        /// <summary>
        /// Q axis components.
        /// </summary>
        public static readonly Guid QAxis = new Guid("d347ba64-e34c-11d4-82d9-00e09872a094");

        /// <summary>
        /// Rotational position.
        /// </summary>
        public static readonly Guid Rotational = new Guid("d347ba62-e34c-11d4-82d9-00e09872a094");

        /// <summary>
        /// D axis components.
        /// </summary>
        public static readonly Guid DAxis = new Guid("d347ba63-e34c-11d4-82d9-00e09872a094");

        /// <summary>
        /// Linear position.
        /// </summary>
        public static readonly Guid Linear = new Guid("d347ba61-e34c-11d4-82d9-00e09872a094");

        /// <summary>
        /// Transfer function.
        /// </summary>
        public static readonly Guid TransferFunc = new Guid("5202bd07-245c-11d5-a4b3-444553540000");

        /// <summary>
        /// Status data.
        /// </summary>
        public static readonly Guid Status = new Guid("b82b5c83-55c7-11d5-a4b3-444553540000");

        /// <summary>
        /// Gets information about the quantity
        /// characteristic identified by the given ID.
        /// </summary>
        /// <param name="quantityCharacteristicID">The identifier for the quantity characteristic.</param>
        /// <returns>Inforamtion about the quantity characteristic.</returns>
        public static Identifier GetInfo(Guid quantityCharacteristicID)
        {
            Identifier identifier;
            return QuantityCharacteristicLookup.TryGetValue(quantityCharacteristicID, out identifier) ? identifier : null;
        }

        /// <summary>
        /// Returns the name of the given quantity characteristic.
        /// </summary>
        /// <param name="quantityCharacteristicID">The GUID tag which identifies the quantity characteristic.</param>
        /// <returns>The name of the given quantity characteristic.</returns>
        public static string ToName(Guid quantityCharacteristicID)
        {
            return GetInfo(quantityCharacteristicID)?.Name;
        }

        /// <summary>
        /// Returns a string representation of the given quantity characteristic.
        /// </summary>
        /// <param name="quantityCharacteristicID">The GUID tag which identifies the quantity characteristic.</param>
        /// <returns>The name of the given quantity characteristic.</returns>
        public static string ToString(Guid quantityCharacteristicID)
        {
            return GetInfo(quantityCharacteristicID)?.Description;
        }

        private static Dictionary<Guid, Identifier> QuantityCharacteristicLookup
        {
            get
            {
                Tag quantityCharacteristicTag = Tag.GetTag(SeriesDefinition.QuantityCharacteristicIDTag);

                if (s_quantityCharacteristicTag != quantityCharacteristicTag)
                {
                    s_quantityCharacteristicLookup = quantityCharacteristicTag.ValidIdentifiers.ToDictionary(id => Guid.Parse(id.Value));
                    s_quantityCharacteristicTag = quantityCharacteristicTag;
                }

                return s_quantityCharacteristicLookup;
            }
        }

        private static Tag s_quantityCharacteristicTag;
        private static Dictionary<Guid, Identifier> s_quantityCharacteristicLookup;
    }
}
