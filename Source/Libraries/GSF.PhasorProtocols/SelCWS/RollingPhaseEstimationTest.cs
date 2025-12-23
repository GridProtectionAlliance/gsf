using System;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Test program demonstrating the RollingPhaseEstimator.
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        System.Console.WriteLine("RollingPhaseEstimator Test Program");
        System.Console.WriteLine("===================================\n");

        // Test 1: Basic operation at nominal frequency
        TestNominalFrequency();

        // Test 2: Frequency deviation detection
        TestFrequencyDeviation();

        // Test 3: Magnitude measurement
        TestMagnitudeMeasurement();

        // Test 4: Phase angle relationships
        TestPhaseAngles();

        System.Console.WriteLine("\nAll tests completed.");
    }

    private static void TestNominalFrequency()
    {
        System.Console.WriteLine("Test 1: Nominal Frequency Operation (60 Hz)");
        System.Console.WriteLine("-------------------------------------------");

        const double sampleRate = 3000.0;
        const double nominalFreq = 60.0;
        const double amplitude = 100.0;

        RollingPhaseEstimator estimator = new(sampleRate, LineFrequency.Hz60);

        // Generate samples for 50 cycles worth of data
        const int TotalSamples = (int)(sampleRate / nominalFreq * 50);
        const long SamplePeriodNs = (long)(1e9 / sampleRate);
        const double Omega = 2.0 * Math.PI * nominalFreq;
        long epochNs = 0;

        PhaseEstimate lastEstimate = new();
        bool gotEstimate = false;
        int estimateCount = 0;

        for (int i = 0; i < TotalSamples; i++)
        {
            double t = i / sampleRate;

            // Generate balanced three-phase signals (120° apart)
            double ia = amplitude * Math.Sin(Omega * t);
            double ib = amplitude * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double ic = amplitude * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);
            double va = amplitude * Math.Sin(Omega * t);
            double vb = amplitude * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);

            if (estimator.Step(ia, ib, ic, va, vb, vc, epochNs, out PhaseEstimate? estimate))
            {
                lastEstimate = estimate!.Value;
                gotEstimate = true;
                estimateCount++;
            }

            epochNs += SamplePeriodNs;
        }

        if (gotEstimate)
        {
            System.Console.WriteLine($"  Total estimates: {estimateCount}");
            System.Console.WriteLine($"  Measured Frequency: {lastEstimate.Frequency:F4} Hz (expected: {nominalFreq} Hz)");
            System.Console.WriteLine($"  ROCOF: {lastEstimate.dFdt:F4} Hz/s (expected: ~0)");
            System.Console.WriteLine($"  VA Magnitude: {lastEstimate.Magnitudes![RollingPhaseEstimator.VA]:F4} (expected: ~{amplitude / Math.Sqrt(2):F4} RMS)");
            System.Console.WriteLine($"  Frequency error: {Math.Abs(lastEstimate.Frequency - nominalFreq):F6} Hz");
        }
        System.Console.WriteLine();
    }

    private static void TestFrequencyDeviation()
    {
        System.Console.WriteLine("Test 2: Frequency Deviation Detection (61 Hz vs 60 Hz nominal)");
        System.Console.WriteLine("--------------------------------------------------------------");

        const double sampleRate = 3000.0;
        const double actualFreq = 61.0; // 1 Hz above nominal
        const double amplitude = 100.0;

        RollingPhaseEstimator estimator = new(sampleRate, LineFrequency.Hz60);

        const int TotalSamples = (int)(sampleRate / 60.0 * 10); // 10 nominal cycles
        const long SamplePeriodNs = (long)(1e9 / sampleRate);
        const double Omega = 2.0 * Math.PI * actualFreq;
        long epochNs = 0;

        PhaseEstimate lastEstimate = new();
        bool gotEstimate = false;

        for (int i = 0; i < TotalSamples; i++)
        {
            double t = i / sampleRate;
            double ia = amplitude * Math.Sin(Omega * t);
            double ib = amplitude * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double ic = amplitude * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);
            double va = amplitude * Math.Sin(Omega * t);
            double vb = amplitude * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);

            if (estimator.Step(ia, ib, ic, va, vb, vc, epochNs, out PhaseEstimate? estimate))
            {
                lastEstimate = estimate.Value;
                gotEstimate = true;
            }

            epochNs += SamplePeriodNs;
        }

        if (gotEstimate)
        {
            System.Console.WriteLine($"  Measured Frequency: {lastEstimate.Frequency:F4} Hz (actual: {actualFreq} Hz)");
            System.Console.WriteLine($"  Frequency error: {Math.Abs(lastEstimate.Frequency - actualFreq):F4} Hz");
        }

        System.Console.WriteLine();
    }

    private static void TestMagnitudeMeasurement()
    {
        System.Console.WriteLine("Test 3: Magnitude Measurement (various amplitudes)");
        System.Console.WriteLine("--------------------------------------------------");

        const double sampleRate = 3000.0;
        const double nominalFreq = 60.0;

        RollingPhaseEstimator estimator = new(sampleRate, LineFrequency.Hz60);

        // Different amplitudes for each channel
        double[] peakAmplitudes = [100.0, 150.0, 200.0, 120.0, 180.0, 90.0];
        double[] expectedRms = new double[6];
        
        for (int i = 0; i < 6; i++)
            expectedRms[i] = peakAmplitudes[i] / Math.Sqrt(2);

        const int TotalSamples = (int)(sampleRate / nominalFreq * 5);
        const long SamplePeriodNs = (long)(1e9 / sampleRate);
        const double Omega = 2.0 * Math.PI * nominalFreq;

        long epochNs = 0;

        PhaseEstimate lastEstimate = new();
        bool gotEstimate = false;

        for (int i = 0; i < TotalSamples; i++)
        {
            double t = i / sampleRate;

            double ia = peakAmplitudes[0] * Math.Sin(Omega * t);
            double ib = peakAmplitudes[1] * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double ic = peakAmplitudes[2] * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);
            double va = peakAmplitudes[3] * Math.Sin(Omega * t);
            double vb = peakAmplitudes[4] * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double vc = peakAmplitudes[5] * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);

            if (estimator.Step(ia, ib, ic, va, vb, vc, epochNs, out PhaseEstimate? estimate))
            {
                lastEstimate = estimate.Value;
                gotEstimate = true;
            }

            epochNs += SamplePeriodNs;
        }

        if (gotEstimate)
        {
            string[] names = ["IA", "IB", "IC", "VA", "VB", "VC"];
            for (int i = 0; i < 6; i++)
            {
                double error = Math.Abs(lastEstimate.Magnitudes[i] - expectedRms[i]);
                double errorPct = error / expectedRms[i] * 100;
                System.Console.WriteLine($"  {names[i]}: Measured={lastEstimate.Magnitudes[i]:F4}, Expected={expectedRms[i]:F4}, Error={errorPct:F2}%");
            }
        }
        System.Console.WriteLine();
    }

    private static void TestPhaseAngles()
    {
        System.Console.WriteLine("Test 4: Phase Angle Relationships");
        System.Console.WriteLine("----------------------------------");

        const double sampleRate = 3000.0;
        const double nominalFreq = 60.0;
        const double amplitude = 100.0;

        RollingPhaseEstimator estimator = new(sampleRate, LineFrequency.Hz60);

        // Currents lead voltages by 30 degrees (capacitive load)
        const double CurrentLeadRad = 30.0 * Math.PI / 180.0;
        const int TotalSamples = (int)(sampleRate / nominalFreq * 5);
        const long SamplePeriodNs = (long)(1e9 / sampleRate);
        const double Omega = 2.0 * Math.PI * nominalFreq;
        long epochNs = 0;

        PhaseEstimate lastEstimate = new();
        bool gotEstimate = false;

        for (int i = 0; i < TotalSamples; i++)
        {
            double t = i / sampleRate;

            // Currents with 30° lead
            double ia = amplitude * Math.Sin(Omega * t + CurrentLeadRad);
            double ib = amplitude * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0 + CurrentLeadRad);
            double ic = amplitude * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0 + CurrentLeadRad);

            // Voltages (reference)
            double va = amplitude * Math.Sin(Omega * t);
            double vb = amplitude * Math.Sin(Omega * t - 2.0 * Math.PI / 3.0);
            double vc = amplitude * Math.Sin(Omega * t + 2.0 * Math.PI / 3.0);

            if (estimator.Step(ia, ib, ic, va, vb, vc, epochNs, out PhaseEstimate? estimate))
            {
                lastEstimate = estimate.Value;
                gotEstimate = true;
            }

            epochNs += SamplePeriodNs;
        }

        if (gotEstimate)
        {
            string[] names = ["IA", "IB", "IC", "VA", "VB", "VC"];
            
            System.Console.WriteLine("  Angles relative to VA:");
            
            for (int i = 0; i < 6; i++)
                System.Console.WriteLine($"    {names[i]}: {lastEstimate.Angles![i].ToDegrees():F2}°");
            
            System.Console.WriteLine($"\n  IA leads VA by: {lastEstimate.Angles![RollingPhaseEstimator.IA].ToDegrees():F2}° (expected: 30°)");
            System.Console.WriteLine($"  IB leads VB by: {(lastEstimate.Angles[RollingPhaseEstimator.IB].ToDegrees() - lastEstimate.Angles[RollingPhaseEstimator.VB].ToDegrees()):F2}° (expected: 30°)");
        }

        System.Console.WriteLine();
    }
}
