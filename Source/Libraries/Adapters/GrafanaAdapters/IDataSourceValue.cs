//******************************************************************************************************
//  IDataSourceValue.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/20/2023 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Data;
using GSF.TimeSeries;

namespace GrafanaAdapters
{
    /// <summary>
    /// Defines an interface for a data source value.
    /// </summary>
    public interface IDataSourceValue
    {
        /// <summary>
        /// Gets timestamp, in Unix epoch milliseconds, of data source value.
        /// </summary>
        double Time { get; }

        /// <summary>
        /// Gets time-series array values of data source value, e.g., [Value, Time].
        /// </summary>
        double[] TimeSeriesValue { get; }

        /// <summary>
        /// Gets flags of data source value.
        /// </summary>
        MeasurementStateFlags Flags { get; }

        /// <summary>
        /// Looks up metadata for the specified target.
        /// </summary>
        /// <param name="metadata">Metadata data set.</param>
        /// <param name="target">Target to lookup.</param>
        /// <returns>Filtered metadata rows for the specified target.</returns>
        DataRow[] LookupMetadata(DataSet metadata, string target);
    }
}
