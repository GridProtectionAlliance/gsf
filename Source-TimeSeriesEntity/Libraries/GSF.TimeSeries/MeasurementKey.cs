//******************************************************************************************************
//  MeasurementKey.cs - Gbtc
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
//  10/29/2013 - Stephen C. Wills
//       Greatly simplified measurement keys to represent just the source and point ID of a measurement.
//
//******************************************************************************************************

using System;
using System.Data;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a primary key for a measurement.
    /// </summary>
    [Serializable]
    public struct MeasurementKey
    {
        #region [ Members ]

        // Fields
        private readonly string m_source;
        private readonly uint m_pointID;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="MeasurementKey"/> given the specified parameters.
        /// </summary>
        /// <param name="source">Source of the measurement that this <see cref="MeasurementKey"/> represents (e.g., name of archive).</param>
        /// <param name="pointID">Numeric ID of the measurement that this <see cref="MeasurementKey"/> represents.</param>
        /// <exception cref="ArgumentNullException">source cannot be null.</exception>
        public MeasurementKey(string source, uint pointID)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException("source", "MeasurementKey source cannot be null or empty");

            m_source = source.ToUpper();
            m_pointID = pointID;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the source of this <see cref="MeasurementKey"/>.
        /// </summary>
        /// <remarks>
        /// This value is typically used to track the archive name in which the measurement, that this <see cref="MeasurementKey"/> represents, is stored.
        /// </remarks>
        public string Source
        {
            get
            {
                return m_source;
            }
        }

        /// <summary>
        /// Gets or sets the numeric point ID of this <see cref="MeasurementKey"/>.
        /// </summary>
        public uint PointID
        {
            get
            {
                return m_pointID;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="MeasurementKey"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="MeasurementKey"/>.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", m_source ?? "__", m_pointID);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Converts the string representation of a <see cref="MeasurementKey"/> into its value equivalent.
        /// </summary>
        /// <param name="value">A string representing the <see cref="MeasurementKey"/> to convert.</param>
        /// <returns>A <see cref="MeasurementKey"/> value equivalent the representation contained in <paramref name="value"/>.</returns>
        /// <exception cref="FormatException">The value is not in the correct format for a <see cref="MeasurementKey"/> value.</exception>
        public static MeasurementKey Parse(string value)
        {
            MeasurementKey key;

            if (TryParse(value, out key))
                return key;

            throw new FormatException("The value is not in the correct format for a MeasurementKey value");
        }

        /// <summary>
        /// Attempts to convert the string representation of a <see cref="MeasurementKey"/> into its value equivalent.
        /// </summary>
        /// <param name="value">A string representing the <see cref="MeasurementKey"/> to convert.</param>
        /// <param name="key">Output <see cref="MeasurementKey"/> in which to stored parsed value.</param>
        /// <returns>A <c>true</c> if <see cref="MeasurementKey"/>representation contained in <paramref name="value"/> could be parsed; otherwise <c>false</c>.</returns>
        public static bool TryParse(string value, out MeasurementKey key)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    string[] elem = value.Split(':');

                    if (elem.Length == 2)
                    {
                        uint id;

                        if (uint.TryParse(elem[1].Trim(), out id))
                        {
                            key = new MeasurementKey(elem[0].Trim(), id);
                            return true;
                        }
                    }
                }
                catch
                {
                    // Any exceptions result in failure to parse
                }
            }

            key = default(MeasurementKey);
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Defines extension functions related to <see cref="MeasurementKey"/> objects.
    /// </summary>
    public static class MeasurementKeyExtensions
    {
        /// <summary>
        /// Parses a <see cref="MeasurementKey"/> from a <see cref="DataRow"/> for the specified <paramref name="measurementKeyColumn"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> that contains the field to parse as a <see cref="MeasurementKey"/>.</param>
        /// <param name="measurementKeyColumn">Name of column that contains the data to parse as a <see cref="MeasurementKey"/>.</param>
        /// <returns>
        /// <see cref="MeasurementKey"/> from <see cref="DataRow"/> if parse succeeds; otherwise an empty <see cref="MeasurementKey"/>.
        /// </returns>
        public static MeasurementKey GetMeasurementKey(this DataRow row, string measurementKeyColumn = "ID")
        {
            MeasurementKey key = default(MeasurementKey);

            if ((object)row != null && row.Table.Columns.Contains(measurementKeyColumn))
                MeasurementKey.TryParse(row[measurementKeyColumn].ToNonNullString("__"), out key);

            return key;
        }
    }
}