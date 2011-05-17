using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Common static methods and extensions for transport library.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Returns <c>true</c> if <see cref="ServerResponse"/> code is for a solicited <see cref="ServerCommand"/>.
        /// </summary>
        /// <param name="responseCode"><see cref="ServerResponse"/> code to test.</param>
        /// <returns><c>true</c> if <see cref="ServerResponse"/> code is for a solicited <see cref="ServerCommand"/>; otherwise <c>false</c>.</returns>
        public static bool IsSolicitedResponseCode(this ServerResponse responseCode)
        {
            bool solicited = false;

            switch (responseCode)
            {
                case ServerResponse.Succeeded:
                case ServerResponse.Failed:
                    solicited = true;
                    break;
            }

            return solicited;
        }
    }
}
