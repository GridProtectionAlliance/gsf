using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctionBase
{
    internal class TimeConversion
    {
        /// <summary>
        /// Converts a double value from a specified time unit and scaling factor to a <see cref="Time"/> object.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <param name="target">The target time unit and scaling factor for the conversion.</param>
        /// <returns>A <see cref="Time"/> object that represents the converted value.</returns>
        public static Time FromTimeUnits(double value, TargetTimeUnit target)
        {
            double time = Time.ConvertFrom(value, target.Unit);

            if (!double.IsNaN(target.Factor))
                time *= target.Factor;

            return time;
        }

        /// <summary>
        /// Converts a <see cref="Time"/> object to a double value, scaled by a specified time unit and scaling factor.
        /// </summary>
        /// <param name="value">The <see cref="Time"/> object to convert.</param>
        /// <param name="target">The target time unit and scaling factor for the conversion.</param>
        /// <returns>A double value that represents the converted and scaled time.</returns>
        public static double ToTimeUnits(Time value, TargetTimeUnit target)
        {
            double time = value.ConvertTo(target.Unit);

            if (!double.IsNaN(target.Factor))
                time /= target.Factor;

            return time;
        }
    }
}
