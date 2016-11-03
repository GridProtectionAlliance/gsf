//******************************************************************************************************
//  DataRowExtensions.cs - Gbtc
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
using System.Data;

namespace GSF.Data
{
    /// <summary>
    /// Static extension methods for <see cref="DataRow"/> that will automatically 
    /// type cast fields to the desired value. This is superior to DataSetExtensions.dll
    /// because if the field type does not exactly match in that case, a cast exception occurs.
    /// </summary>
    public static class DataRowExtensions
    {
        #region As Byte Array

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static byte[] AsByteArray(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            byte[] bytes = value as byte[];
            if (bytes != null)
                return bytes;
            throw new NotSupportedException();
        }

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static byte[] AsByteArray(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            byte[] bytes = value as byte[];
            if (bytes != null)
                return bytes;
            throw new NotSupportedException();
        }

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static byte[] AsByteArray(this DataRow row, string columnName, byte[] valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            byte[] bytes = value as byte[];
            if (bytes != null)
                return bytes;
            throw new NotSupportedException();
        }

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static byte[] AsByteArray(this DataRow row, int ordinal, byte[] valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            byte[] bytes = value as byte[];
            if (bytes != null)
                return bytes;
            throw new NotSupportedException();
        }

        #endregion

        #region [ As UInt32 ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static uint? AsUInt32(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static uint? AsUInt32(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static uint AsUInt32(this DataRow row, string columnName, uint valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static uint AsUInt32(this DataRow row, int ordinal, uint valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }

        #endregion

        #region [ As Int32 ]
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static int? AsInt32(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static int? AsInt32(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static int AsInt32(this DataRow row, string columnName, int valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static int AsInt32(this DataRow row, int ordinal, int valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }

        #endregion

        #region [ As Byte ]
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static byte? AsByte(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static byte? AsByte(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static byte AsByte(this DataRow row, string columnName, byte valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static byte AsByte(this DataRow row, int ordinal, byte valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }

        #endregion

        #region [ As Int16 ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static short? AsInt16(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static short? AsInt16(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static short AsInt16(this DataRow row, string columnName, short valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static short AsInt16(this DataRow row, int ordinal, short valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }

        #endregion

        #region [ As Guid ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static Guid? AsGuid(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static Guid? AsGuid(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static Guid AsGuid(this DataRow row, string columnName, Guid valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static Guid AsGuid(this DataRow row, int ordinal, Guid valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }

        #endregion

        #region [ As Int64 ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static long? AsInt64(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static long? AsInt64(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static long AsInt64(this DataRow row, string columnName, long valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static long AsInt64(this DataRow row, int ordinal, long valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }

        #endregion

        #region [ As Boolean ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static bool? AsBoolean(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static bool? AsBoolean(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static bool AsBoolean(this DataRow row, string columnName, bool valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static bool AsBoolean(this DataRow row, int ordinal, bool valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }

        #endregion

        #region [ As String ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static string AsString(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            string s = value as string;
            if (s != null)
                return s;
            return value.ToString();
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static string AsString(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            string s = value as string;
            if (s != null)
                return s;
            return value.ToString();
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static string AsString(this DataRow row, string columnName, string valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            string s = value as string;
            if (s != null)
                return s;
            return value.ToString();
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static string AsString(this DataRow row, int ordinal, string valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            string s = value as string;
            if (s != null)
                return s;
            return value.ToString();
        }

        #endregion

        #region [ As DateTime ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static DateTime? AsDateTime(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static DateTime? AsDateTime(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static DateTime AsDateTime(this DataRow row, string columnName, DateTime valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static DateTime AsDateTime(this DataRow row, int ordinal, DateTime valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }


        #endregion

        #region [ As Double ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static double? AsDouble(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static double? AsDouble(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static double AsDouble(this DataRow row, string columnName, double valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static double AsDouble(this DataRow row, int ordinal, double valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }

        #endregion

        #region [ As Decimal ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static decimal? AsDecimal(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static decimal? AsDecimal(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static decimal AsDecimal(this DataRow row, string columnName, decimal valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static decimal AsDecimal(this DataRow row, int ordinal, decimal valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }

        #endregion

        #region [ As Single ]

        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static float? AsSingle(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <returns>null if DBNull, the value otherwise.</returns>
        public static float? AsSingle(this DataRow row, int ordinal)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return null;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="columnName">the column name</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static float AsSingle(this DataRow row, string columnName, float valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }
        /// <summary>
        /// Attempts to type cast the specified column of a <see param="row"/> to the defined type.
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="ordinal">the index of the column</param>
        /// <param name="valueIfNull">the value to replace if null</param>
        /// <returns><see param="valueIfNull"/> if DBNull, the value otherwise.</returns>
        public static float AsSingle(this DataRow row, int ordinal, float valueIfNull)
        {
            object value = row[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }

        #endregion

    }
}
