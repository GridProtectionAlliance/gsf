//*******************************************************************************************************
//  IArchive.cs
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
//  05/21/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/02/2009 - Pinal C. Patel
//       Modified ReadData() to take start and end times as strings for flexibility.
//
//*******************************************************************************************************

using System.Collections.Generic;

namespace TVA.Historian
{
    /// <summary>
    /// Defines a repository where time series data is warehoused by a historian.
    /// </summary>
    /// <seealso cref="IDataPoint"/>
    public interface IArchive
    {
        #region [ Methods ]

        /// <summary>
        /// Opens the repository.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the repository.
        /// </summary>
        void Close();

        /// <summary>
        /// Writes time series data to the repository.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to be written.</param>
        void WriteData(IDataPoint dataPoint);

        /// <summary>
        /// Writes meta information for the specified <paramref name="historianID"/> to the repository.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="metaData">Binary image of the meta information.</param>
        void WriteMetaData(int historianID, byte[] metaData);

        /// <summary>
        /// Writes state information for the specified <paramref name="historianID"/> to the repository.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="stateData">Binary image of the state information.</param>
        void WriteStateData(int historianID, byte[] stateData);

        /// <summary>
        /// Reads time series data from the repository.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="IDataPoint"/>s are to be read.</param>
        /// <param name="startTime"><see cref="System.String"/> representation of the start time (in GMT) of the timespan for which <see cref="IDataPoint"/>s are to be read.</param>
        /// <param name="endTime"><see cref="System.String"/> representation of the end time (in GMT) of the timespan for which <see cref="IDataPoint"/>s are to be read.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="IDataPoint"/>s.</returns>
        IEnumerable<IDataPoint> ReadData(int historianID, string startTime, string endTime);

        /// <summary>
        /// Read meta information for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing meta information.</returns>
        byte[] ReadMetaData(int historianID);

        /// <summary>
        /// Reads state information for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing state information.</returns>
        byte[] ReadStateData(int historianID);

        /// <summary>
        /// Reads meta information summary for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing meta information summary.</returns>
        byte[] ReadMetaDataSummary(int historianID);

        /// <summary>
        /// Read state information summary for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A <see cref="byte"/> array containing state information summary.</returns>
        byte[] ReadStateDataSummary(int historianID);

        #endregion
    }
}
