//*******************************************************************************************************
//  ArchiveFileStatistics.cs
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
//  06/26/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using TVA.Units;

namespace TVA.Historian.Files
{
    /// <summary>
    /// A class that contains the statistics of an <see cref="ArchiveFile"/>.
    /// </summary>
    /// <seealso cref="ArchiveFile"/>
    public class ArchiveFileStatistics
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Current usage (in %) of the <see cref="ArchiveFile"/>.
        /// </summary>
        public float FileUsage;

        /// <summary>
        /// Current rate of data compression (in %) in the <see cref="ArchiveFile"/>.
        /// </summary>
        public float CompressionRate;

        /// <summary>
        /// <see cref="Time"/> over which the <see cref="AverageWriteSpeed"/> is calculated.
        /// </summary>
        public Time AveragingWindow;

        /// <summary>
        /// Average number of time series data points written to the <see cref="ArchiveFile"/> in one second.
        /// </summary>
        public int AverageWriteSpeed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFileStatistics"/> class.
        /// </summary>
        internal ArchiveFileStatistics()
        {
        }

        #endregion
    }
}
