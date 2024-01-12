//******************************************************************************************************
//  TimeSliceScannerAsync.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  01/06/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters;

/// <summary>
/// Reads series of <see cref="IDataSourceValue{T}"/> instances for the same time interval.
/// </summary>
public class TimeSliceScannerAsync<T> where T : struct, IDataSourceValue<T>
{
    private readonly List<IAsyncEnumerator<T>> m_enumerators;

    private TimeSliceScannerAsync(List<IAsyncEnumerator<T>> enumerators, double tolerance)
    {
        m_enumerators = enumerators;
        Tolerance = tolerance;
    }

    /// <summary>
    /// Gets a flag that determines if data read has been completed.
    /// </summary>
    public bool DataReadComplete => m_enumerators.Count == 0;

    /// <summary>
    /// Gets time tolerance for data slices in Unix epoch milliseconds.
    /// </summary>
    public double Tolerance { get; }

    /// <summary>
    /// Reads next time slice from the series set.
    /// </summary>
    /// <param name="lastValue">
    /// Set to <c>true</c> to only publish last value per-series in time-slice for specified <see cref="Tolerance"/>;
    /// otherwise, <c>false</c> to publish all series values since last slice.
    /// </param>
    /// <returns>Next time slice.</returns>
    public async Task<IAsyncEnumerable<T>> ReadNextTimeSliceAsync(bool lastValue = true)
    {
        if (lastValue)
        {
            Dictionary<string, T> nextSlice = new(StringComparer.OrdinalIgnoreCase);
            await ReadNextTimeSliceAsync(value => nextSlice[value.Target] = value).ConfigureAwait(false);
            return nextSlice.Values.ToAsyncEnumerable();
        }
        else
        {
            List<T> nextSlice = new();
            await ReadNextTimeSliceAsync(value => nextSlice.Add(value)).ConfigureAwait(false);
            return nextSlice.ToAsyncEnumerable();
        }
    }

    private async Task ReadNextTimeSliceAsync(Action<T> addValue)
    {
        T dataPoint;
        double publishTime = double.MaxValue;

        // Find minimum publication time for current values
        foreach (IAsyncEnumerator<T> enumerator in m_enumerators)
        {
            dataPoint = enumerator.Current;

            if (dataPoint.Time < publishTime)
                publishTime = dataPoint.Time;
        }

        publishTime += Tolerance;

        List<int> completed = new();
        int index = 0;

        // Publish all values at the current time
        foreach (IAsyncEnumerator<T> enumerator in m_enumerators)
        {
            bool enumerationComplete = false;
            dataPoint = enumerator.Current;

            if (dataPoint.Time <= publishTime)
            {
                // Attempt to advance to next data point, tracking completed enumerators
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    enumerationComplete = true;
                    completed.Add(index);
                }

                addValue(dataPoint);

                // Make sure any point IDs with duplicated times directly follow
                if (!enumerationComplete)
                {
                    while (enumerator.Current.Time <= publishTime)
                    {
                        addValue(enumerator.Current);

                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            completed.Add(index);
                            break;
                        }
                    }
                }
            }

            index++;
        }

        // Remove completed enumerators
        if (completed.Count == 0)
            return;

        completed.Sort();

        // Remove highest numeric indexes first to retain source index integrity
        for (int i = completed.Count - 1; i >= 0; i--)
        {
            int indexToRemove = completed[i];
            await m_enumerators[indexToRemove].DisposeAsync().ConfigureAwait(false);
            m_enumerators.RemoveAt(indexToRemove);
        }

        completed.Clear();
    }

    /// <summary>
    /// Creates a new <see cref="TimeSliceScannerAsync{T}"/>.
    /// </summary>
    /// <param name="dataSources">Set of <see cref="DataSourceValueGroup{T}"/> series to scan.</param>
    /// <param name="tolerance">Time tolerance for data slices in Unix epoch milliseconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async ValueTask<TimeSliceScannerAsync<T>> Create(IAsyncEnumerable<DataSourceValueGroup<T>> dataSources, double tolerance, CancellationToken cancellationToken)
    {
        List<IAsyncEnumerator<T>> enumerators = new();

        await foreach (DataSourceValueGroup<T> group in dataSources.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            IAsyncEnumerator<T> enumerator = group.Source.GetAsyncEnumerator(cancellationToken);

            // Add enumerator to the list if it has at least one value
            if (await enumerator.MoveNextAsync().ConfigureAwait(false))
                enumerators.Add(enumerator);
            else
                await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        return new TimeSliceScannerAsync<T>(enumerators, tolerance);
    }
}