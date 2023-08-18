using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctionBase
{
    /// <summary>
    /// Represents a time unit that can be targeted in OpenHistorian Grafana functions.
    /// This class is designed to handle various forms of time units and provides
    /// a way to parse time units from strings.
    /// </summary>
    public class TargetTimeUnit
    {
        /// <summary>
        /// Gets or sets the base time unit.
        /// </summary>
        public TimeUnit Unit;

        /// <summary>
        /// Gets or sets the factor by which to scale the base time unit.
        /// </summary>
        public double Factor = double.NaN;

        /// <summary>
        /// Tries to parse a string representation of a time unit to a <see cref="TargetTimeUnit"/>.
        /// </summary>
        /// <param name="value">The string representation of the time unit to parse.</param>
        /// <param name="targetTimeUnit">
        /// When this method returns, contains the <see cref="TargetTimeUnit"/> equivalent
        /// of the time unit contained in <paramref name="value"/>, if the conversion succeeded,
        /// or null if the conversion failed. The conversion fails if the <paramref name="value"/>
        /// is null or is not of the correct format. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse(string value, out TargetTimeUnit targetTimeUnit)
        {
            if (Enum.TryParse(value, out TimeUnit timeUnit))
            {
                targetTimeUnit = new TargetTimeUnit
                {
                    Unit = timeUnit
                };

                return true;
            }

            switch (value?.ToLowerInvariant())
            {
                case "milliseconds":
                    targetTimeUnit = new TargetTimeUnit
                    {
                        Unit = TimeUnit.Seconds,
                        Factor = SI.Milli
                    };

                    return true;
                case "microseconds":
                    targetTimeUnit = new TargetTimeUnit
                    {
                        Unit = TimeUnit.Seconds,
                        Factor = SI.Micro
                    };

                    return true;
                case "nanoseconds":
                    targetTimeUnit = new TargetTimeUnit
                    {
                        Unit = TimeUnit.Seconds,
                        Factor = SI.Nano
                    };

                    return true;
            }

            targetTimeUnit = null;
            return false;
        }
    }
}
