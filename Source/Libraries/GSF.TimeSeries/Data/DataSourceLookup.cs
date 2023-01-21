//******************************************************************************************************
//  DataSourceLookups.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/31/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using GSF.Diagnostics;
using GSF.Units.EE;

namespace GSF.TimeSeries.Data
{
    /// <summary>
    /// Creates a cached lookup so certain metadata so lookups can occur with quickly.
    /// </summary>
    public static class DataSourceLookups
    {
        private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(DataSourceLookups), MessageClass.Framework);

        private static readonly List<WeakReference<DataSourceLookupCache>> s_dataSetLookups = new();

        /// <summary>
        /// Gets/Creates the lookup cache for the provided dataset.
        /// </summary>
        /// <param name="dataSet">The non-null dataset provided by the time-series framework</param>
        /// <returns>Lookup cache for the provided dataset.</returns>
        public static DataSourceLookupCache GetLookupCache(DataSet dataSet)
        {
            if (dataSet is null)
                throw new ArgumentNullException(nameof(dataSet));

            // Since adding datasets will be rare, the penalty associated with a lock on the entire set will be minor.
            lock (s_dataSetLookups)
            {
                DataSourceLookupCache target;

                for (int index = 0; index < s_dataSetLookups.Count; index++)
                {
                    WeakReference<DataSourceLookupCache> item = s_dataSetLookups[index];

                    if (item.TryGetTarget(out target) && target.DataSet is not null)
                    {
                        if (ReferenceEquals(target.DataSet, dataSet))
                        {
                            return target;
                        }
                    }
                    else
                    {
                        s_log.Publish(MessageLevel.Info, "A DataSet object has been disposed or garbage collected. It will be removed from the list.");
                        s_dataSetLookups.RemoveAt(index);
                        index--;
                    }
                }

                s_log.Publish(MessageLevel.Info, "Creating a lookup cache for a dataset");
                target = new DataSourceLookupCache(dataSet);

                s_dataSetLookups.Add(new WeakReference<DataSourceLookupCache>(target));

                return target;
            }
        }

        /// <summary>
        /// Gets/Creates the <see cref="ActiveMeasurementsTableLookup"/> for the provided dataset.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns><see cref="ActiveMeasurementsTableLookup"/> for the provided dataset.</returns>
        public static ActiveMeasurementsTableLookup ActiveMeasurements(DataSet dataSet) => GetLookupCache(dataSet).ActiveMeasurements;

        /// <summary>
        /// Lookups up metadata record from provided <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="dataSource">Target <see cref="DataSet"/>.</param>
        /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
        /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
        /// <returns>Metadata data row, if found; otherwise, <c>null</c>.</returns>
        public static DataRow LookupMetadata(this DataSet dataSource, Guid signalID, string measurementTable = nameof(ActiveMeasurements))
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));

            DataRow[] records = dataSource.Tables[measurementTable].Select($"SignalID = '{signalID}'");
            
            return records.Length > 0 ? records[0] : null;
        }

        /// <summary>
        /// Gets signal type for given measurement key
        /// </summary>
        /// <param name="dataSource">Target <see cref="DataSet"/>.</param>
        /// <param name="key">Source <see cref="MeasurementKey"/>.</param>
        /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
        /// <returns><see cref="SignalType"/> as defined for measurement key in data source.</returns>
        public static SignalType GetSignalType(this DataSet dataSource, MeasurementKey key, string measurementTable = nameof(ActiveMeasurements))
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));

            DataRow record = dataSource.LookupMetadata(key.SignalID, measurementTable);

            if (record is not null && Enum.TryParse(record[nameof(SignalType)].ToString(), out SignalType signalType))
                return signalType;

            return SignalType.NONE;
        }

        /// <summary>
        /// Gets signal types for given measurement keys.
        /// </summary>
        /// <param name="dataSource">Target <see cref="DataSet"/>.</param>
        /// <param name="keys">Source set of <see cref="MeasurementKey"/> values.</param>
        /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
        /// <returns><see cref="SignalType"/> values for each defined measurement key as configured in data source.</returns>
        public static SignalType[] GetSignalTypes(this DataSet dataSource, MeasurementKey[] keys, string measurementTable = nameof(ActiveMeasurements))
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));

            if (keys is null || keys.Length == 0)
                return Array.Empty<SignalType>();

            SignalType[] signalTypes = new SignalType[keys.Length];

            for (int i = 0; i < signalTypes.Length; i++)
                signalTypes[i] = dataSource.GetSignalType(keys[i], measurementTable);

            return signalTypes;
        }

        /// <summary>
        /// Gets signal types for given measurement keys.
        /// </summary>
        /// <param name="dataSource">Target <see cref="DataSet"/>.</param>
        /// <param name="measurements">Source set of <see cref="IMeasurement"/> values.</param>
        /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
        /// <returns><see cref="SignalType"/> values for each defined measurement key as configured in data source.</returns>
        public static SignalType[] GetSignalTypes(this DataSet dataSource, IMeasurement[] measurements, string measurementTable = nameof(ActiveMeasurements))
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));

            if (measurements is null || measurements.Length == 0)
                return Array.Empty<SignalType>();

            SignalType[] signalTypes = new SignalType[measurements.Length];

            for (int i = 0; i < signalTypes.Length; i++)
                signalTypes[i] = dataSource.GetSignalType(measurements[i].Key, measurementTable);

            return signalTypes;
        }
    }
}
