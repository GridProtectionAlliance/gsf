//*******************************************************************************************************
//  IPacket.cs
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
//  07/27/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA.Parsing;

namespace TVA.Historian.Packets
{
    /// <summary>
    /// Defines a binary packet received by the archival process of DatAWare.
    /// </summary>
    public interface IPacket : ISupportBinaryImage, ISupportFrameImage<short>
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="IArchive"/>.
        /// </summary>
        IArchive Archive { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that processes the packet.
        /// </summary>
        /// <remarks>
        /// <see cref="Func{TResult}"/> returns an <see cref="IEnumerable{T}"/> object containing the binary data to be sent back to the packet sender.
        /// </remarks>
        Func<IEnumerable<byte[]>> ProcessHandler { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that pre-processes the packet.
        /// </summary>
        /// <remarks>
        /// <see cref="Func{TResult}"/> returns an <see cref="IEnumerable{T}"/> object containing the binary data to be sent back to the packet sender.
        /// </remarks>
        Func<IEnumerable<byte[]>> PreProcessHandler { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Extracts time series data from the packet.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> object of <see cref="IDataPoint"/>s if the packet contains time series data; otherwise null.</returns>
        IEnumerable<IDataPoint> ExtractTimeSeriesData();

        #endregion
    }
}