using GSF.TimeSeries;

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
}
