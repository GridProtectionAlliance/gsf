//******************************************************************************************************
//  IEEEC37_118PhaseEstimatorTest.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using GSF.PhasorProtocols.SelCWS;
using GSF.Units.EE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.PhasorProtocols.Tests
{
    /// <summary>
    /// Unit tests for <see cref="IEEEC37_118PhaseEstimator"/>, covering the IEEE C37.118-2018 Annex D
    /// phasor, frequency and ROCOF estimation for both P-class and M-class filters.
    /// </summary>
    /// <remarks>
    /// Converted from the original Gemstone console demonstration program into deterministic assertions.
    /// </remarks>
    [TestClass]
    public class IEEEC37_118PhaseEstimatorTest
    {
        // 960 Hz (= 16 samples/cycle at 60 Hz) reporting at 60 Hz matches the original Annex D test vectors.
        private const double SampleRate = 960.0D;
        private const double OutputRate = 60.0D;
        private const double NominalFrequency = 60.0D;
        private const double Sqrt2 = 1.4142135623730951D;
        private static readonly double DegreesToRadians = Math.PI / 180.0D;

        // Captures the most recent published estimate. Spans are only valid during the callback,
        // so values are copied out into fields/arrays.
        private sealed class Capture
        {
            public bool Got;
            public double Frequency;
            public double Rocof;
            public long Count;
            public readonly double[] MagnitudesRms = new double[6];
            public readonly double[] AnglesDegrees = new double[6];

            public void OnEstimate(in PhaseEstimate estimate)
            {
                Got = true;
                Frequency = estimate.Frequency;
                Rocof = estimate.dFdt;
                Count++;

                for (int i = 0; i < 6; i++)
                {
                    MagnitudesRms[i] = estimate.Magnitudes[i];
                    AnglesDegrees[i] = estimate.Angles[i].ToDegrees();
                }
            }
        }

        private static IEEEC37_118PhaseEstimator BuildEstimator(FilterClass filterClass = FilterClass.P)
        {
            return new IEEEC37_118PhaseEstimator(
                sampleRateHz: SampleRate,
                outputRateHz: OutputRate,
                nominalFrequency: LineFrequency.Hz60,
                filterClass: filterClass);
        }

        // Balanced three-phase channel phase offsets (radians): VA/VB/VC at 0/-120/+120 deg,
        // IA/IB/IC at the same with an optional current lead.
        private static double[] BalancedPhases(double currentLeadDegrees = 0.0D)
        {
            return new[]
            {
                0.0D * DegreesToRadians,
                -120.0D * DegreesToRadians,
                120.0D * DegreesToRadians,
                currentLeadDegrees * DegreesToRadians,
                (-120.0D + currentLeadDegrees) * DegreesToRadians,
                (120.0D + currentLeadDegrees) * DegreesToRadians
            };
        }

        private static double[] UniformPeaks(double amplitude)
        {
            return new[] { amplitude, amplitude, amplitude, amplitude, amplitude, amplitude };
        }

        private static long EpochNs(long sampleIndex)
        {
            return (long)Math.Round(sampleIndex * 1_000_000_000.0D / SampleRate);
        }

        // Steps the estimator with a cosine-referenced sinusoidal sample-group (VA, VB, VC, IA, IB, IC)
        // for an absolute sample index. The Annex D angle convention assumes a cosine reference.
        private static bool StepIndex(IEEEC37_118PhaseEstimator estimator, Capture capture, long sampleIndex, double frequency, double[] peak, double[] phaseRadians)
        {
            double t = sampleIndex / SampleRate;
            double w = 2.0D * Math.PI * frequency * t;

            double va = peak[0] * Math.Cos(w + phaseRadians[0]);
            double vb = peak[1] * Math.Cos(w + phaseRadians[1]);
            double vc = peak[2] * Math.Cos(w + phaseRadians[2]);
            double ia = peak[3] * Math.Cos(w + phaseRadians[3]);
            double ib = peak[4] * Math.Cos(w + phaseRadians[4]);
            double ic = peak[5] * Math.Cos(w + phaseRadians[5]);

            return estimator.Step(va, vb, vc, ia, ib, ic, EpochNs(sampleIndex), capture.OnEstimate);
        }

        // Wraps an angle in degrees to (-180, 180].
        private static double WrapDegrees(double degrees)
        {
            double result = (degrees + 180.0D) % 360.0D;

            if (result < 0.0D)
                result += 360.0D;

            return result - 180.0D;
        }

        [TestMethod]
        public void TracksNominalFrequency()
        {
            IEEEC37_118PhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            // 50 nominal cycles worth of samples
            long totalSamples = (long)(SampleRate / NominalFrequency * 50);

            for (long i = 0; i < totalSamples; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(capture.Got, "Expected an estimate once the filter window filled.");
            Assert.AreEqual(NominalFrequency, capture.Frequency, 0.1D, "Frequency should track 60 Hz.");
            Assert.AreEqual(0.0D, capture.Rocof, 0.1D, "ROCOF should be ~0 for a steady frequency.");
            Assert.AreEqual(100.0D / Sqrt2, capture.MagnitudesRms[(int)PhaseChannel.VA], 0.5D, "VA RMS magnitude.");
        }

        [TestMethod]
        public void TracksOffNominalFrequency()
        {
            IEEEC37_118PhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            const double ActualFrequency = 61.0D;

            // 30 nominal cycles for convergence
            long totalSamples = (long)(SampleRate / NominalFrequency * 30);

            for (long i = 0; i < totalSamples; i++)
                StepIndex(estimator, capture, i, ActualFrequency, peak, phases);

            Assert.IsTrue(capture.Got);
            Assert.AreEqual(ActualFrequency, capture.Frequency, 0.2D, "Frequency should track 61 Hz.");
        }

        [TestMethod]
        public void MeasuresRmsMagnitudesPerChannel()
        {
            IEEEC37_118PhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = { 100.0D, 150.0D, 200.0D, 120.0D, 180.0D, 90.0D };
            double[] phases = BalancedPhases();

            long totalSamples = (long)(SampleRate / NominalFrequency * 20);

            for (long i = 0; i < totalSamples; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(capture.Got);

            for (int ch = 0; ch < 6; ch++)
                Assert.AreEqual(peak[ch] / Sqrt2, capture.MagnitudesRms[ch], peak[ch] * 0.02D, $"RMS magnitude for channel {ch}.");
        }

        [TestMethod]
        public void ReportsAnglesWithExpectedPhaseRelationships()
        {
            IEEEC37_118PhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases(currentLeadDegrees: 30.0D);

            long totalSamples = (long)(SampleRate / NominalFrequency * 20);

            for (long i = 0; i < totalSamples; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(capture.Got);

            double va = capture.AnglesDegrees[(int)PhaseChannel.VA];

            // Annex D reports absolute synchrophasor angles, so validate the inter-channel relationships.
            Assert.AreEqual(-120.0D, WrapDegrees(capture.AnglesDegrees[(int)PhaseChannel.VB] - va), 1.5D, "VB ~ -120 deg from VA.");
            Assert.AreEqual(120.0D, WrapDegrees(capture.AnglesDegrees[(int)PhaseChannel.VC] - va), 1.5D, "VC ~ +120 deg from VA.");
            Assert.AreEqual(30.0D, WrapDegrees(capture.AnglesDegrees[(int)PhaseChannel.IA] - va), 1.5D, "IA leads VA by ~30 deg.");
        }

        [TestMethod]
        public void MClassFilterOrderExceedsPClass()
        {
            IEEEC37_118PhaseEstimator pEstimator = BuildEstimator(FilterClass.P);
            IEEEC37_118PhaseEstimator mEstimator = BuildEstimator(FilterClass.M);

            Assert.IsTrue(mEstimator.FilterOrder > pEstimator.FilterOrder, "M-class filter order should exceed P-class.");

            Capture pCapture = new();
            Capture mCapture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            long totalSamples = (long)(SampleRate / NominalFrequency * 50);

            for (long i = 0; i < totalSamples; i++)
            {
                StepIndex(pEstimator, pCapture, i, NominalFrequency, peak, phases);
                StepIndex(mEstimator, mCapture, i, NominalFrequency, peak, phases);
            }

            Assert.IsTrue(pCapture.Got && mCapture.Got);

            double expectedRms = 100.0D / Sqrt2;
            Assert.AreEqual(NominalFrequency, pCapture.Frequency, 0.1D, "P-class frequency.");
            Assert.AreEqual(NominalFrequency, mCapture.Frequency, 0.1D, "M-class frequency.");
            Assert.AreEqual(expectedRms, pCapture.MagnitudesRms[(int)PhaseChannel.VA], 0.5D, "P-class VA RMS magnitude.");
            Assert.AreEqual(expectedRms, mCapture.MagnitudesRms[(int)PhaseChannel.VA], 0.5D, "M-class VA RMS magnitude.");
        }

        [TestMethod]
        public void ProducesExpectedDecimationCount()
        {
            IEEEC37_118PhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            // Exactly one second of data.
            long totalSamples = (long)SampleRate;

            for (long i = 0; i < totalSamples; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            int samplesPerReport = (int)(SampleRate / OutputRate);
            long expected = (totalSamples - estimator.WindowSamples) / samplesPerReport;

            Assert.IsTrue(capture.Got);
            Assert.IsTrue(Math.Abs(capture.Count - expected) <= 1,
                $"Expected ~{expected} estimates at {OutputRate} Hz report rate, got {capture.Count}.");
        }
    }
}
