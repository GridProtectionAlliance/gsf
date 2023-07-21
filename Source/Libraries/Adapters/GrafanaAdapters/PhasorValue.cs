using GSF.TimeSeries;
using System.Collections.Generic;

namespace GrafanaAdapters
{
    /// <summary>
    /// Defines a structure that represents an individual time-series value from a data source.
    /// </summary>
    public struct PhasorValue
    {
        /// <summary>
        /// Query magnitude target, e.g., a point-tag.
        /// </summary>
        public string MagnitudeTarget;
        /// <summary>
        /// Query magnitude target, e.g., a point-tag.
        /// </summary>
        public string AngleTarget;

        /// <summary>
        /// Queried value.
        /// </summary>
        public double Magnitude;
        /// <summary>
        /// Queried value.
        /// </summary>
        public double Angle;

        /// <summary>
        /// Timestamp, in Unix epoch milliseconds, of queried value.
        /// </summary>
        public double Time;

        /// <summary>
        /// Flags for queried value.
        /// </summary>
        public MeasurementStateFlags Flags;
    }

    /// <summary>
    /// Helper class to compare two phasors
    /// </summary>
    public class PhasorValueComparer : IComparer<PhasorValue>
    {
        /// <summary>
        /// Compare function
        /// </summary>
        public int Compare(PhasorValue x, PhasorValue y)
        {
            int result = x.Magnitude.CompareTo(y.Magnitude);

            if (result != 0)
                return result;

            result = x.Angle.CompareTo(y.Angle);

            if (result != 0)
                return result;

            return x.Time.CompareTo(y.Time);
        }
    }
}
