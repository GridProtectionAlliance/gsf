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

        public static byte[] AsByteArray(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is byte[])
                return (byte[])value;
            throw new NotSupportedException();
        }

        public static byte[] AsByteArray(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is byte[])
                return (byte[])value;
            throw new NotSupportedException();
        }

        public static byte[] AsByteArray(this DataRow row, string columnName, byte[] valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is byte[])
                return (byte[])value;
            throw new NotSupportedException();
        }

        public static byte[] AsByteArray(this DataRow reader, int ordinal, byte[] valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is byte[])
                return (byte[])value;
            throw new NotSupportedException();
        }

        #endregion

        #region [ As UInt32 ]

        public static uint? AsUInt32(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }

        public static uint? AsUInt32(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }

        public static uint AsUInt32(this DataRow row, string columnName, uint valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }

        public static uint AsUInt32(this DataRow reader, int ordinal, uint valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is uint)
                return (uint)value;
            return ((IConvertible)value).ToUInt32(null);
        }

        #endregion

        #region [ As Int32 ]

        public static int? AsInt32(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }

        public static int? AsInt32(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }

        public static int AsInt32(this DataRow row, string columnName, int valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }

        public static int AsInt32(this DataRow reader, int ordinal, int valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is int)
                return (int)value;
            return ((IConvertible)value).ToInt32(null);
        }

        #endregion

        #region [ As Byte ]

        public static byte? AsByte(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }

        public static byte? AsByte(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }

        public static byte AsByte(this DataRow row, string columnName, byte valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }

        public static byte AsByte(this DataRow reader, int ordinal, byte valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is byte)
                return (byte)value;
            return ((IConvertible)value).ToByte(null);
        }

        #endregion

        #region [ As Int16 ]

        public static short? AsInt16(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }

        public static short? AsInt16(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }

        public static short AsInt16(this DataRow row, string columnName, short valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }

        public static short AsInt16(this DataRow reader, int ordinal, short valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is short)
                return (short)value;
            return ((IConvertible)value).ToInt16(null);
        }

        #endregion

        #region [ As Guid ]

        public static Guid? AsGuid(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }

        public static Guid? AsGuid(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }

        public static Guid AsGuid(this DataRow row, string columnName, Guid valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }

        public static Guid AsGuid(this DataRow reader, int ordinal, Guid valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is Guid)
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }

        #endregion

        #region [ As Int64 ]

        public static long? AsInt64(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }

        public static long? AsInt64(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }

        public static long AsInt64(this DataRow row, string columnName, long valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }

        public static long AsInt64(this DataRow reader, int ordinal, long valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is long)
                return (long)value;
            return ((IConvertible)value).ToInt64(null);
        }

        #endregion

        #region [ As Boolean ]

        public static bool? AsBoolean(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }

        public static bool? AsBoolean(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }

        public static bool AsBoolean(this DataRow row, string columnName, bool valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }

        public static bool AsBoolean(this DataRow reader, int ordinal, bool valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is bool)
                return (bool)value;
            return ((IConvertible)value).ToBoolean(null);
        }

        #endregion

        #region [ As String ]

        public static string AsString(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is string)
                return (string)value;
            return value.ToString();
        }

        public static string AsString(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is string)
                return (string)value;
            return value.ToString();
        }

        public static string AsString(this DataRow row, string columnName, string valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is string)
                return (string)value;
            return value.ToString();
        }

        public static string AsString(this DataRow reader, int ordinal, string valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is string)
                return (string)value;
            return value.ToString();
        }

        #endregion

        #region [ As DateTime ]

        public static DateTime? AsDateTime(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime? AsDateTime(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime AsDateTime(this DataRow row, string columnName, DateTime valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }

        public static DateTime AsDateTime(this DataRow reader, int ordinal, DateTime valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is DateTime)
                return (DateTime)value;
            return ((IConvertible)value).ToDateTime(null);
        }


        #endregion

        #region [ As Double ]

        public static double? AsDouble(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }

        public static double? AsDouble(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }

        public static double AsDouble(this DataRow row, string columnName, double valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }

        public static double AsDouble(this DataRow reader, int ordinal, double valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is double)
                return (double)value;
            return ((IConvertible)value).ToDouble(null);
        }

        #endregion

        #region [ As Decimal ]

        public static decimal? AsDecimal(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }

        public static decimal? AsDecimal(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }

        public static decimal AsDecimal(this DataRow row, string columnName, decimal valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }

        public static decimal AsDecimal(this DataRow reader, int ordinal, decimal valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is decimal)
                return (decimal)value;
            return ((IConvertible)value).ToDecimal(null);
        }

        #endregion

        #region [ As Single ]

        public static float? AsSingle(this DataRow row, string columnName)
        {
            object value = row[columnName];
            if (value is DBNull)
                return null;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }

        public static float? AsSingle(this DataRow reader, int ordinal)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return null;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }

        public static float AsSingle(this DataRow row, string columnName, float valueIfNull)
        {
            object value = row[columnName];
            if (value is DBNull)
                return valueIfNull;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }

        public static float AsSingle(this DataRow reader, int ordinal, float valueIfNull)
        {
            object value = reader[ordinal];
            if (value is DBNull)
                return valueIfNull;
            if (value is float)
                return (float)value;
            return ((IConvertible)value).ToSingle(null);
        }

        #endregion

    }
}
