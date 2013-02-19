//******************************************************************************************************
//  DataSetExtensions.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  02/07/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GSF.Data
{
    /// <summary>
    /// Data types available to a <see cref="DataSet"/> object.
    /// </summary>
    public enum DataType : byte
    {
        /// <summary>
        /// Boolean data type, <see cref="Boolean"/>.
        /// </summary>
        Boolean,
        /// <summary>
        /// Unsigned 8-bit byte data type, <see cref="Byte"/>.
        /// </summary>
        Byte,
        /// <summary>
        /// 16-bit character data type, <see cref="Char"/>.
        /// </summary>
        Char,
        /// <summary>
        /// Date/time data type, <see cref="DateTime"/>.
        /// </summary>
        DateTime,
        /// <summary>
        /// Decimal data type, <see cref="Decimal"/>.
        /// </summary>
        Decimal,
        /// <summary>
        /// 64-dit double precision floating point numeric data type, <see cref="Double"/>.
        /// </summary>
        Double,
        /// <summary>
        /// Unisgned 128-bit Guid integer data type, <see cref="Guid"/>.
        /// </summary>
        Guid,
        /// <summary>
        /// Signed 16-bit integer data type, <see cref="Int16"/>.
        /// </summary>
        Int16,
        /// <summary>
        /// Signed 32-bit integer data type, <see cref="Int32"/>.
        /// </summary>
        Int32,
        /// <summary>
        /// Signed 64-bit integer data type, <see cref="Int64"/>
        /// </summary>
        Int64,
        /// <summary>
        /// Signed byte data type, <see cref="SByte"/>.
        /// </summary>
        SByte,
        /// <summary>
        /// 32-bit single precision floating point numeric data type, <see cref="Single"/>.
        /// </summary>
        Single,
        /// <summary>
        /// Character array data type, <see cref="String"/>.
        /// </summary>
        String,
        /// <summary>
        /// Timespan data type, <see cref="TimeSpan"/>.
        /// </summary>
        TimeSpan,
        /// <summary>
        /// Unsigned 16-bit integer data type, <see cref="UInt16"/>.
        /// </summary>
        UInt16,
        /// <summary>
        /// Unsigned 32-bit integer data type, <see cref="UInt32"/>.
        /// </summary>
        UInt32,
        /// <summary>
        /// Unsigned 64-bit integer data type, <see cref="UInt64"/>.
        /// </summary>
        UInt64,
        /// <summary>
        /// Unsigned byte array data type.
        /// </summary>
        Blob,
        /// <summary>
        /// User defined/other data type.
        /// </summary>
        Object
    }

    /// <summary>
    /// Defines extension functions related to <see cref="DataSet"/> instances.
    /// </summary>
    public static class DataSetExtensions
    {
        // Constant array of supported data types
        private readonly static Type[] s_supportedDataTypes = new[]
        {
            // This must match DataType enum order
	        typeof(bool),
            typeof(byte),
	        typeof(char),
	        typeof(DateTime),
	        typeof(decimal),
	        typeof(double),
	        typeof(Guid),
	        typeof(short),
	        typeof(int),
	        typeof(long),
	        typeof(sbyte),
	        typeof(float),
	        typeof(string),
	        typeof(TimeSpan),
	        typeof(ushort),
	        typeof(uint),
	        typeof(ulong),
	        typeof(byte[]),
            typeof(object)
        };

        /// <summary>
        /// Serializes a <see cref="DataSet"/> to a destination <see cref="Stream"/>.
        /// </summary>
        /// <param name="source"><see cref="DataSet"/> to serialize.</param>
        /// <param name="destination"><see cref="Stream"/> to seralize <see cref="DataSet"/> on.</param>
        public static void SerializeToStream(this DataSet source, Stream destination)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source");

            if ((object)destination == null)
                throw new ArgumentNullException("destination");

            if (!destination.CanWrite)
                throw new InvalidOperationException("Cannot write to a read-only stream");

            BinaryWriter output = new BinaryWriter(destination);

            // Serialize dataset name and table count
            output.Write(source.DataSetName);
            output.Write(source.Tables.Count);

            // Serialize tables
            foreach (DataTable table in source.Tables)
            {
                List<int> columnIndices = new List<int>();
                List<DataType> columnDataTypes = new List<DataType>();
                DataType dataType;

                // Serialize table name and column count
                output.Write(table.TableName);
                output.Write(table.Columns.Count);

                // Serialize column metadata
                foreach (DataColumn column in table.Columns)
                {
                    // Get column data type, unknown types will be represented as object
                    dataType = GetDataType(column.DataType);

                    // Only objects of a known type can be properly serialized
                    if (dataType != DataType.Object)
                    {
                        // Serialize column name and type
                        output.Write(column.ColumnName);
                        output.Write((byte)dataType);

                        // Track data types and column indicies in parallel lists for faster DataRow serialization
                        columnIndices.Add(column.Ordinal);
                        columnDataTypes.Add(dataType);
                    }
                }

                // Serialize row count
                output.Write(table.Rows.Count);

                // Serialize rows
                foreach (DataRow row in table.Rows)
                {
                    object value;

                    // Serialize column data
                    for (int i = 0; i < columnIndices.Count; i++)
                    {
                        value = row[columnIndices[i]];
                      
                        switch (columnDataTypes[i])
                        {
                            case DataType.Boolean:
                                output.Write((bool)value);
                                break;
                            case DataType.Byte:
                                output.Write((byte)value);
                                break;
                            case DataType.Char:
                                output.Write((char)value);
                                break;
                            case DataType.DateTime:
                                output.Write(((DateTime)value).Ticks);
                                break;
                            case DataType.Decimal:
                                output.Write((decimal)value);
                                break;
                            case DataType.Double:
                                output.Write((double)value);
                                break;
                            case DataType.Guid:
                                output.Write(((Guid)value).ToByteArray());
                                break;
                            case DataType.Int16:
                                output.Write((short)value);
                                break;
                            case DataType.Int32:
                                output.Write((int)value);
                                break;
                            case DataType.Int64:
                                output.Write((long)value);
                                break;
                            case DataType.SByte:
                                output.Write((sbyte)value);
                                break;
                            case DataType.Single:
                                output.Write((float)value);
                                break;
                            case DataType.String:
                                output.Write((string)value);
                                break;
                            case DataType.TimeSpan:
                                output.Write(((TimeSpan)value).Ticks);
                                break;
                            case DataType.UInt16:
                                output.Write((ushort)value);
                                break;
                            case DataType.UInt32:
                                output.Write((uint)value);
                                break;
                            case DataType.UInt64:
                                output.Write((ulong)value);
                                break;
                            case DataType.Blob:
                                byte[] blob = (byte[])value;

                                if ((object)blob == null || blob.Length == 0)
                                {
                                    output.Write(0);
                                }
                                else
                                {
                                    output.Write(blob.Length);
                                    output.Write(blob);
                                }

                                break;
                         }
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes a <see cref="DataSet"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="source"><see cref="Stream"/> to deseralize <see cref="DataSet"/> from.</param>
        public static DataSet DeserializeToDataSet(this Stream source)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source");

            if (!source.CanRead)
                throw new InvalidOperationException("Cannot read from a write-only stream");

            DataSet dataset = new DataSet();
            DataRow row;
            object value;

            BinaryReader input = new BinaryReader(source);
            int tableCount;

            // Deserialize dataset name and table count
            dataset.DataSetName = input.ReadString();
            tableCount = input.ReadInt32();

            // Deserialize tables
            for (int i = 0; i < tableCount; i++)
            {
                List<int> columnIndices = new List<int>();
                List<DataType> columnDataTypes = new List<DataType>();
                DataType dataType;
                int columnCount, rowCount;

                DataTable table = dataset.Tables.Add();

                // Deserialize table name and column count
                table.TableName = input.ReadString();
                columnCount = input.ReadInt32();

                // Deserialize column metadata
                for (int j = 0; j < columnCount; j++)
                {
                    DataColumn column = table.Columns.Add();

                    // Deserialize column name and type
                    column.ColumnName = input.ReadString();
                    dataType = (DataType)input.ReadByte();
                    column.DataType = dataType.DeriveColumnType();

                    // Track data types and column indicies in parallel lists for faster DataRow deserialization
                    columnIndices.Add(column.Ordinal);
                    columnDataTypes.Add(dataType);
                }

                // Deserialize row count
                rowCount = input.ReadInt32();

                // Deserialize rows
                for (int j = 0; j < rowCount; j++)
                {
                    row = table.NewRow();

                    // Deserialize column data
                    for (int k = 0; k < columnIndices.Count; k++)
                    {
                        value = null;

                        switch (columnDataTypes[k])
                        {
                            case DataType.Boolean:
                                value = input.ReadBoolean();
                                break;
                            case DataType.Byte:
                                value = input.ReadByte();
                                break;
                            case DataType.Char:
                                value = input.ReadChar();
                                break;
                            case DataType.DateTime:
                                value = new DateTime(input.ReadInt64());
                                break;
                            case DataType.Decimal:
                                value = input.ReadDecimal();
                                break;
                            case DataType.Double:
                                value = input.ReadDouble();
                                break;
                            case DataType.Guid:
                                value = new Guid(input.ReadBytes(16));
                                break;
                            case DataType.Int16:
                                value = input.ReadInt16();
                                break;
                            case DataType.Int32:
                                value = input.ReadInt32();
                                break;
                            case DataType.Int64:
                                value = input.ReadInt64();
                                break;
                            case DataType.SByte:
                                value = input.ReadSByte();
                                break;
                            case DataType.Single:
                                value = input.ReadSingle();
                                break;
                            case DataType.String:
                                value = input.ReadString();
                                break;
                            case DataType.TimeSpan:
                                value = new TimeSpan(input.ReadInt64());
                                break;
                            case DataType.UInt16:
                                value = input.ReadUInt16();
                                break;
                            case DataType.UInt32:
                                value = input.ReadUInt32();
                                break;
                            case DataType.UInt64:
                                value = input.ReadUInt64();
                                break;
                            case DataType.Blob:
                                int byteCount = input.ReadInt32();

                                if (byteCount > 0)
                                    value = input.ReadBytes(byteCount);

                                break;
                        }

                        // Update column value
                        row[columnIndices[k]] = value;
                    }

                    // Add new row to table
                    table.Rows.Add(row);
                }
            }

            return dataset;
        }

        /// <summary>
        /// Attempts to derive <see cref="DataType"/> based on object <see cref="Type"/>.
        /// </summary>
        /// <param name="objectType"><see cref="Type"/> of object to test.</param>
        /// <returns>Derived <see cref="DataType"/> based on object <see cref="Type"/> if matched; otherse <see cref="DataType.Object"/>.</returns>
        public static DataType GetDataType(this Type objectType)
        {
            for (int i = 0; i < s_supportedDataTypes.Length; i++)
            {
                if (objectType == s_supportedDataTypes[i])
                    return (DataType)i;
            }

            return DataType.Object;
        }

        /// <summary>
        /// Gets column object <see cref="Type"/> from given <see cref="DataType"/>.
        /// </summary>
        /// <param name="dataType"><see cref="DataType"/> to derive object <see cref="Type"/> from.</param>
        /// <returns>Object <see cref="Type"/> derived from given <see cref="DataType"/>.</returns>
        public static Type DeriveColumnType(this DataType dataType)
        {
            return s_supportedDataTypes[(int)dataType];
        }
    }
}
