//******************************************************************************************************
//  IFrame.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/14/2011 - J. Ritchie Carroll
//       Added received and published timestamps for measurements.
//  05/11/2011 - J. Ritchie Carroll
//       Changed IFrame to require a concurrent dictionary.
//  10/31/2013 - Stephen C. Wills
//       Simplified IFrame to the basic form of timestamp and values.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Abstract frame interface representing a collection of time-series entities at an exact moment in time.
    /// </summary>
    public interface IFrame : IEquatable<IFrame>, IComparable<IFrame>, IComparable
    {
        /// <summary>
        /// Gets exact timestamp, in <see cref="Ticks"/>, of the data represented in this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        Ticks Timestamp
        {
            get;
        }

        /// <summary>
        /// Keyed time-series entities in this <see cref="IFrame"/>.
        /// </summary>
        /// <remarks>
        /// Represents a dictionary of time-series entities, keyed by signal ID.
        /// </remarks>
        IDictionary<Guid, ITimeSeriesEntity> Entities
        {
            get;
        }
    }

    /// <summary>
    /// Defines extension functions related to <see cref="IFrame"/> objects.
    /// </summary>
    public static class IFrameExtensions
    {
        /// <summary>
        /// Attempts to retrieve the specified <paramref name="signalID"/> as a <paramref name="value"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> instance to retrieve <paramref name="value"/> from.</param>
        /// <param name="signalID"><see cref="Guid"/> based <see cref="ITimeSeriesEntity.ID"/> to lookup.</param>
        /// <param name="value">Retuned value if lookup succeeds.</param>
        /// <typeparam name="T">Type of <see cref="ITimeSeriesEntity"/> class to find.</typeparam>
        /// <returns>
        /// <c>true</c> if <see cref="signalID"/> was found in dictionary and <see cref="ITimeSeriesEntity"/> value retrieved was
        /// able to be cast as type <typeparamref name="T"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is <c>null</c>.</exception>
        public static bool TryGetEntity<T>(this IFrame frame, Guid signalID, out T value) where T : class, ITimeSeriesEntity
        {
            if ((object)frame == null)
                throw new ArgumentNullException("frame");

            ITimeSeriesEntity entity;

            if (frame.Entities.TryGetValue(signalID, out entity))
            {
                value = entity as T;

                if ((object)value != null)
                    return true;
            }

            value = null;
            return false;
        }
    }
}