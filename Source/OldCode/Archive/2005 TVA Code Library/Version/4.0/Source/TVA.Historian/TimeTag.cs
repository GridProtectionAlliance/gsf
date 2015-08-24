//*******************************************************************************************************
//  TimeTag.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/03/2006 - James R. Carroll
//       Initial version of source imported from 1.1 code library
//  07/12/2006 - James R. Carroll
//       Modified class to be derived from new "TimeTagBase" class
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Historian
{
    /// <summary>
    /// Represents a historian time tag as number of seconds from the <see cref="BaseDate"/>.
    /// </summary>
    public class TimeTag : TimeTagBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTag"/> class.
        /// </summary>
        /// <param name="seconds">Number of seconds since the <see cref="BaseDate"/>.</param>
        public TimeTag(double seconds)
            : base(BaseDate.Ticks, seconds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTag"/> class.
        /// </summary>
        /// <param name="timestamp"><see cref="DateTime"/> value on or past the <see cref="BaseDate"/>.</param>
        public TimeTag(DateTime timestamp)
            : base(BaseDate.Ticks, timestamp)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns the default text representation of <see cref="TimeTag"/>.
        /// </summary>
        /// <returns><see cref="string"/> that represents <see cref="TimeTag"/>.</returns>
        public override string ToString()
        {
            return ToString("dd-MMM-yyyy HH:mm:ss.fff");
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Represents the smallest possible value of <see cref="TimeTag"/>.
        /// </summary>
        public static readonly TimeTag MinValue;

        /// <summary>
        /// Represents the largest possible value of <see cref="TimeTag"/>.
        /// </summary>
        public static readonly TimeTag MaxValue;

        /// <summary>
        /// Represents the base <see cref="DateTime"/> (01/01/1995) for <see cref="TimeTag"/>.
        /// </summary>
        public static readonly DateTime BaseDate;

        // Static Constructor

        static TimeTag()
        {
            BaseDate = new DateTime(1995, 1, 1, 0, 0, 0);
            MinValue = new TimeTag(0.0);
            MaxValue = new TimeTag(2147483647.999);
        }

        #endregion
    }
}
