//******************************************************************************************************
//  QuantityType.cs - Gbtc
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
//  05/16/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// The high-level description of the type of
    /// quantity which is being captured by a channel.
    /// </summary>
    public static class QuantityType
    {
        /// <summary>
        /// Point-on-wave measurements.
        /// </summary>
        public static readonly Guid WaveForm = new Guid("67f6af80-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Time-based logged entries.
        /// </summary>
        public static readonly Guid ValueLog = new Guid("67f6af82-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Time-domain measurements including
        /// magnitudes and (optionally) phase angle.
        /// </summary>
        public static readonly Guid Phasor = new Guid("67f6af81-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Frequency-domain measurements including
        /// magnitude and (optionally) phase angle.
        /// </summary>
        public static readonly Guid Response = new Guid("67f6af85-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Time, latitude, longitude, value, polarity, ellipse.
        /// </summary>
        public static readonly Guid Flash = new Guid("67f6af83-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// BinLow, BinHigh, BinID, count.
        /// </summary>
        public static readonly Guid Histogram = new Guid("67f6af87-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// XBinLow, XBinHigh, YBinLow, YBinHigh, BinID, count.
        /// </summary>
        public static readonly Guid Histogram3D = new Guid("67f6af88-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Probability, value.
        /// </summary>
        public static readonly Guid CPF = new Guid("67f6af89-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// X-values and y-values.
        /// </summary>
        public static readonly Guid XY = new Guid("67f6af8a-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Magnitude and duration.
        /// </summary>
        public static readonly Guid MagDur = new Guid("67f6af8b-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// X-values, y-values, and z-values.
        /// </summary>
        public static readonly Guid XYZ = new Guid("67f6af8c-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Time, magnitude, and duration.
        /// </summary>
        public static readonly Guid MagDurTime = new Guid("67f6af8d-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Time, magnitude, duration, and count.
        /// </summary>
        public static readonly Guid MagDurCount = new Guid("67f6af8e-f753-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Gets the name of the quantity type with the given ID.
        /// </summary>
        /// <param name="quantityTypeID">The ID of the quantity type.</param>
        /// <returns>The name of the quantity type with the given ID.</returns>
        public static string ToString(Guid quantityTypeID)
        {
            string name;
            return QuantityTypeNames.TryGetValue(quantityTypeID, out name) ? name : null;
        }

        /// <summary>
        /// Determines whether the given ID is a quantity type ID.
        /// </summary>
        /// <param name="id">The ID to be tested.</param>
        /// <returns>True if the given ID is a quantity type ID; false otherwise.</returns>
        public static bool IsQuantityTypeID(Guid id)
        {
            return QuantityTypeNames.ContainsKey(id);
        }

        private static readonly Dictionary<Guid, string> QuantityTypeNames = new Dictionary<Guid, string>()
        {
            { WaveForm, "WaveForm" },
            { ValueLog, "ValueLog" },
            { Phasor, "Phasor" },
            { Response, "Response" },
            { Flash, "Flash" },
            { Histogram, "Histogram" },
            { Histogram3D, "Histogram3D" },
            { CPF, "CPF" },
            { XY, "XY" },
            { MagDur, "MagDur" },
            { XYZ, "XYZ" },
            { MagDurTime, "MagDurTime" },
            { MagDurCount, "MagDurCount" }
        }; 
    }
}
