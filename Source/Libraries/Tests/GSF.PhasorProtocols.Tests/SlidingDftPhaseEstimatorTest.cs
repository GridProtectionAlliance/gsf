//******************************************************************************************************
//  SlidingDftPhaseEstimatorTest.cs - Gbtc
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
    /// Unit tests for <see cref="SlidingDftPhaseEstimator"/>, covering the core phasor/frequency estimation
    /// and the missing-data handling (phase-continued coast and resynchronization).
    /// </summary>
    [TestClass]
    public class SlidingDftPhaseEstimatorTest
    {
        private const double SampleRate = 3000.0D;
        private const double NominalFrequency = 60.0D;
        private const double Sqrt2 = 1.4142135623730951D;
        private static readonly double DegreesToRadians = Math.PI / 180.0D;

        // Captures the most recent published estimate. Spans are only valid during the callback,
        // so scalar values are copied out into arrays.
        private sealed class Capture
        {
            public bool Got;
            public double Frequency;
            public double Rocof;
            public readonly double[] MagnitudesRms = new double[6];
            public readonly double[] AnglesDegrees = new double[6];

            // Running sums over every published estimate. With internal smoothing off, the raw per-sample
            // frequency/ROCOF carry deterministic ripple (spectral-leakage beat off-nominal; timestamp-
            // rounding jitter in dt), so a single sample is unrepresentative. The mean over the run recovers
            // the true value (the ripple is zero-mean about it).
            private double m_frequencySum;
            private double m_rocofSum;
            private long m_count;

            public double MeanFrequency => m_count > 0 ? m_frequencySum / m_count : 0.0D;
            public double MeanRocof => m_count > 0 ? m_rocofSum / m_count : 0.0D;

            public void OnEstimate(in PhaseEstimate estimate)
            {
                Got = true;
                Frequency = estimate.Frequency;
                Rocof = estimate.dFdt;

                m_frequencySum += estimate.Frequency;
                m_rocofSum += estimate.dFdt;
                m_count++;

                for (int i = 0; i < 6; i++)
                {
                    MagnitudesRms[i] = estimate.Magnitudes[i];
                    AnglesDegrees[i] = estimate.Angles[i].ToDegrees();
                }
            }
        }

        // Builds an estimator configured for deterministic, full-rate testing: publish rate equals sample
        // rate (an estimate per sample) and all smoothing disabled so outputs reflect the raw per-sample DFT.
        private static SlidingDftPhaseEstimator BuildEstimator(int maxGapFillSamples = SlidingDftPhaseEstimator.DefaultMaxGapFillSamples)
        {
            return new SlidingDftPhaseEstimator(
                sampleRateHz: SampleRate,
                outputRateHz: SampleRate,
                nominalFrequency: LineFrequency.Hz60,
                referenceChannel: PhaseChannel.VA,
                targetCycles: 2,
                enableIntervalAveraging: false,
                enablePublishEMA: false,
                publishAnglesTauSeconds: 0.0D,
                publishMagnitudesTauSeconds: 0.0D,
                publishFrequencyTauSeconds: 0.0D,
                publishRocofTauSeconds: 0.0D,
                sampleFrequencyTauSeconds: 0.0D,
                sampleRocofTauSeconds: 0.0D,
                recalculationCycles: 10,
                maxGapFillSamples: maxGapFillSamples);
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

        // Contiguous per-sample epoch (nanoseconds) for an absolute sample index.
        private static long EpochNs(long sampleIndex)
        {
            return (long)Math.Round(sampleIndex * 1_000_000_000.0D / SampleRate);
        }

        // Steps the estimator with the sinusoidal sample-group for an absolute sample index, using an
        // explicit epoch (so gaps and out-of-order timestamps can be simulated).
        private static bool StepWithEpoch(SlidingDftPhaseEstimator estimator, Capture capture, long sampleIndex, double frequency, double[] peak, double[] phaseRadians, long epochNs)
        {
            double t = sampleIndex / SampleRate;
            double w = 2.0D * Math.PI * frequency * t;

            double va = peak[0] * Math.Sin(w + phaseRadians[0]);
            double vb = peak[1] * Math.Sin(w + phaseRadians[1]);
            double vc = peak[2] * Math.Sin(w + phaseRadians[2]);
            double ia = peak[3] * Math.Sin(w + phaseRadians[3]);
            double ib = peak[4] * Math.Sin(w + phaseRadians[4]);
            double ic = peak[5] * Math.Sin(w + phaseRadians[5]);

            return estimator.Step(va, vb, vc, ia, ib, ic, epochNs, capture.OnEstimate);
        }

        // Steps with the natural contiguous epoch for the given sample index.
        private static bool StepIndex(SlidingDftPhaseEstimator estimator, Capture capture, long sampleIndex, double frequency, double[] peak, double[] phaseRadians)
        {
            return StepWithEpoch(estimator, capture, sampleIndex, frequency, peak, phaseRadians, EpochNs(sampleIndex));
        }

        private static double[] UniformPeaks(double amplitude)
        {
            return new[] { amplitude, amplitude, amplitude, amplitude, amplitude, amplitude };
        }

        [TestMethod]
        public void TracksNominalFrequency()
        {
            SlidingDftPhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            for (long i = 0; i < 600; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(capture.Got, "Expected an estimate once the window filled.");
            // Assert on the mean: per-sample frequency/ROCOF carry zero-mean ripple when smoothing is off.
            Assert.AreEqual(NominalFrequency, capture.MeanFrequency, 0.1D, "Mean frequency should track 60 Hz.");
            Assert.AreEqual(0.0D, capture.MeanRocof, 0.1D, "Mean ROCOF should be ~0 for a steady frequency.");
            Assert.AreEqual(100.0D / Sqrt2, capture.MagnitudesRms[(int)PhaseChannel.VA], 0.5D, "VA RMS magnitude.");
        }

        [TestMethod]
        public void TracksOffNominalFrequency()
        {
            SlidingDftPhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            const double ActualFrequency = 61.0D; // within the +/-10% clamp

            for (long i = 0; i < 600; i++)
                StepIndex(estimator, capture, i, ActualFrequency, peak, phases);

            Assert.IsTrue(capture.Got);
            // Off-nominal per-sample frequency oscillates (spectral-leakage beat) about the true value;
            // its mean recovers it. A single sample lands mid-beat (~60.5) and is not representative.
            Assert.AreEqual(ActualFrequency, capture.MeanFrequency, 0.1D, "Mean frequency should track 61 Hz.");
        }

        [TestMethod]
        public void MeasuresRmsMagnitudesPerChannel()
        {
            SlidingDftPhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = { 100.0D, 150.0D, 200.0D, 120.0D, 180.0D, 90.0D };
            double[] phases = BalancedPhases();

            for (long i = 0; i < 600; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(capture.Got);

            for (int ch = 0; ch < 6; ch++)
                Assert.AreEqual(peak[ch] / Sqrt2, capture.MagnitudesRms[ch], peak[ch] * 0.02D, $"RMS magnitude for channel {ch}.");
        }

        [TestMethod]
        public void ReportsAnglesRelativeToReference()
        {
            SlidingDftPhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases(currentLeadDegrees: 30.0D);

            for (long i = 0; i < 600; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(capture.Got);
            Assert.AreEqual(0.0D, capture.AnglesDegrees[(int)PhaseChannel.VA], 0.05D, "VA is the reference (~0 deg).");
            Assert.AreEqual(-120.0D, capture.AnglesDegrees[(int)PhaseChannel.VB], 1.5D, "VB ~ -120 deg.");
            Assert.AreEqual(120.0D, capture.AnglesDegrees[(int)PhaseChannel.VC], 1.5D, "VC ~ +120 deg.");
            Assert.AreEqual(30.0D, capture.AnglesDegrees[(int)PhaseChannel.IA], 1.5D, "IA leads VA by ~30 deg.");
        }

        [TestMethod]
        public void CoastsAcrossSmallGap()
        {
            // Auto max-gap-fill resolves to one window (100 samples), so a 50-sample drop coasts.
            SlidingDftPhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            for (long i = 0; i < 300; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(estimator.IsReady, "Estimator should be ready before the gap.");

            // Drop samples 300..349 (50 missing), then resume at 350.
            bool produced = StepIndex(estimator, capture, 350, NominalFrequency, peak, phases);

            Assert.IsTrue(produced, "Coasting a sub-threshold gap should still publish an estimate.");
            Assert.IsTrue(estimator.IsReady, "Estimator should remain ready after coasting.");
            Assert.AreEqual(NominalFrequency, capture.Frequency, 0.5D, "Frequency should stay stable through a coasted gap.");
            Assert.AreEqual(100.0D / Sqrt2, capture.MagnitudesRms[(int)PhaseChannel.VA], 2.0D, "Magnitude should stay stable through a coasted gap.");
        }

        [TestMethod]
        public void ResynchronizesAcrossLargeGap()
        {
            // Small fill bound forces a 50-sample drop to resynchronize.
            SlidingDftPhaseEstimator estimator = BuildEstimator(maxGapFillSamples: 10);
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            for (long i = 0; i < 300; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(estimator.IsReady, "Estimator should be ready before the gap.");

            // Drop samples 300..349 (50 missing) > maxGapFill (10) -> resync.
            bool produced = StepIndex(estimator, capture, 350, NominalFrequency, peak, phases);

            Assert.IsFalse(produced, "A gap beyond the fill bound should resync (no estimate this step).");
            Assert.IsFalse(estimator.IsReady, "Estimator should be refilling after a resync.");
        }

        [TestMethod]
        public void IgnoresBackwardTimestamp()
        {
            SlidingDftPhaseEstimator estimator = BuildEstimator();
            Capture capture = new();
            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases();

            for (long i = 0; i < 300; i++)
                StepIndex(estimator, capture, i, NominalFrequency, peak, phases);

            long processedBefore = estimator.TotalSamplesProcessed;

            // Resend with an epoch earlier than the last accepted sample.
            bool produced = StepWithEpoch(estimator, capture, 300, NominalFrequency, peak, phases, EpochNs(299) - 1000L);

            Assert.IsFalse(produced, "A backward/duplicate timestamp should be ignored.");
            Assert.AreEqual(processedBefore, estimator.TotalSamplesProcessed, "Ignored samples must not advance processing.");
        }

        [TestMethod]
        public void CoastApproximatesContinuousStream()
        {
            // Compare a continuously-fed estimator against one that coasts a 50-sample drop. Once the gap
            // has slid out of the window, both should agree closely, validating that coasting does not
            // corrupt long-term state and that the synthesized phase continuation is faithful.
            SlidingDftPhaseEstimator continuous = BuildEstimator();
            SlidingDftPhaseEstimator gapped = BuildEstimator();
            Capture continuousCapture = new();
            Capture gappedCapture = new();

            double[] peak = UniformPeaks(100.0D);
            double[] phases = BalancedPhases(currentLeadDegrees: 15.0D);

            for (long i = 0; i <= 200; i++)
            {
                StepIndex(continuous, continuousCapture, i, NominalFrequency, peak, phases);
                StepIndex(gapped, gappedCapture, i, NominalFrequency, peak, phases);
            }

            // Continuous gets every sample; gapped drops 201..250 (50 missing) then resumes contiguously.
            for (long i = 201; i <= 400; i++)
                StepIndex(continuous, continuousCapture, i, NominalFrequency, peak, phases);

            for (long i = 251; i <= 400; i++)
                StepIndex(gapped, gappedCapture, i, NominalFrequency, peak, phases);

            Assert.IsTrue(continuousCapture.Got && gappedCapture.Got);
            Assert.AreEqual(continuousCapture.Frequency, gappedCapture.Frequency, 0.05D, "Frequency should converge after the gap flushes.");

            for (int ch = 0; ch < 6; ch++)
            {
                Assert.AreEqual(continuousCapture.MagnitudesRms[ch], gappedCapture.MagnitudesRms[ch], 0.5D, $"Magnitude mismatch on channel {ch}.");
                Assert.AreEqual(continuousCapture.AnglesDegrees[ch], gappedCapture.AnglesDegrees[ch], 0.5D, $"Angle mismatch on channel {ch}.");
            }
        }
    }
}
