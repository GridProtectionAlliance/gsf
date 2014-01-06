//******************************************************************************************************
//  MeasurementWrapper.cs - Gbtc
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
//  11/05/2010 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.TimeSeries;

namespace MongoAdapters
{
    /// <summary>
    /// A wrapper object used for easy insertion to and retrieval from a MongoDB database.
    /// </summary>
    public class MeasurementWrapper
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates an empty instance of the <see cref="MeasurementWrapper"/> class.
        /// </summary>
        public MeasurementWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MeasurementWrapper"/> class.
        /// </summary>
        /// <param name="measurement">The measurement to be wrapped by the wrapper.</param>
        public MeasurementWrapper(IMeasurement<double> measurement)
        {
            SignalID = measurement.ID.ToString();
            Timestamp = measurement.Timestamp;
            Value = measurement.Value;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// String representation of the measurement's signal ID.
        /// </summary>
        public string SignalID
        {
            get;
            set;
        }

        /// <summary>
        /// The timestamp associated with the measurement.
        /// </summary>
        public long Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// The value of the measurement, before applying the adder and multiplier.
        /// </summary>
        public double Value
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a new measurement equivalent to the one being wrapped.
        /// </summary>
        /// <returns>The wrapped measurement.</returns>
        public IMeasurement<double> GetMeasurement()
        {
            Guid signalID = Guid.Parse(this.SignalID);
            return new Measurement<double>(signalID, Timestamp, Value);
        }

        #endregion
    }
}
