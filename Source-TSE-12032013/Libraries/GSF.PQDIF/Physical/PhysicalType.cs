//******************************************************************************************************
//  PhysicalType.cs - Gbtc
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
//  05/02/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.PQDIF.Physical
{

    #region [ Enumerations ]

    /// <summary>
    /// Enumeration that defines the types of values stored in
    /// <see cref="ScalarElement"/>s and <see cref="VectorElement"/>s.
    /// </summary>
    public enum PhysicalType : byte
    {
        /// <summary>
        /// 1-byte boolean
        /// </summary>
        Boolean1 = 1,

        /// <summary>
        /// 2-byte boolean
        /// </summary>
        Boolean2 = 2,

        /// <summary>
        /// 4-byte boolean
        /// </summary>
        Boolean4 = 3,

        /// <summary>
        /// 1-byte character (ASCII)
        /// </summary>
        Char1 = 10,

        /// <summary>
        /// 2-byte character (UTF-16)
        /// </summary>
        Char2 = 11,

        /// <summary>
        /// 8-bit signed integer
        /// </summary>
        Integer1 = 20,

        /// <summary>
        /// 16-bit signed integer
        /// </summary>
        Integer2 = 21,

        /// <summary>
        /// 32-bit signed integer
        /// </summary>
        Integer4 = 22,

        /// <summary>
        /// 8-bit unsigned integer
        /// </summary>
        UnsignedInteger1 = 30,

        /// <summary>
        /// 16-bit unsigned integer
        /// </summary>
        UnsignedInteger2 = 31,

        /// <summary>
        /// 32-bit unsigned integer
        /// </summary>
        UnsignedInteger4 = 32,

        /// <summary>
        /// 32-bit floating point number
        /// </summary>
        Real4 = 40,

        /// <summary>
        /// 64-bit floating point number
        /// </summary>
        Real8 = 41,

        /// <summary>
        /// 8-byte complex number
        /// </summary>
        /// <remarks>
        /// The first four bytes represent the real part of the complex
        /// number, and the last four bytes represent the imaginary part.
        /// Both values are 64-bit floating point numbers.
        /// </remarks>
        Complex8 = 42,

        /// <summary>
        /// 16-byte complex number
        /// </summary>
        /// <remarks>
        /// The first eight bytes represent the real part of the complex
        /// number, and the last eight bytes represent the imaginary part.
        /// Both values are 64-bit floating point numbers.
        /// </remarks>
        Complex16 = 43,

        /// <summary>
        /// 12-byte timestamp
        /// </summary>
        /// <remarks>
        /// The first four bytes represent the days since January 1, 1900
        /// UTC. The last eight bytes represent the number of seconds since
        /// midnight. The number of days is an unsigned 32-bit integer, and
        /// the number of seconds is a 64-bit floating point number.
        /// </remarks>
        Timestamp = 50,

        /// <summary>
        /// 128-bit globally unique identifier
        /// </summary>
        Guid = 60
    }

    #endregion

    /// <summary>
    /// Defines extension methods for <see cref="PhysicalType"/>.
    /// </summary>
    public static class PysicalTypeExtensions
    {
        /// <summary>
        /// Gets the size of the physical type, in bytes.
        /// </summary>
        /// <param name="type">The physical type.</param>
        /// <returns>The size of the physical type, in bytes.</returns>
        public static int GetByteSize(this PhysicalType type)
        {
            switch (type)
            {
                case PhysicalType.Boolean1:
                case PhysicalType.Char1:
                case PhysicalType.Integer1:
                case PhysicalType.UnsignedInteger1:
                    return 1;

                case PhysicalType.Boolean2:
                case PhysicalType.Char2:
                case PhysicalType.Integer2:
                case PhysicalType.UnsignedInteger2:
                    return 2;

                case PhysicalType.Boolean4:
                case PhysicalType.Integer4:
                case PhysicalType.UnsignedInteger4:
                case PhysicalType.Real4:
                    return 4;

                case PhysicalType.Real8:
                case PhysicalType.Complex8:
                    return 8;

                case PhysicalType.Timestamp:
                    return 12;

                case PhysicalType.Complex16:
                case PhysicalType.Guid:
                    return 16;

                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
