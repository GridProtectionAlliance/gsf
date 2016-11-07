//******************************************************************************************************
//  DataSetEqualityComparer.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  08/09/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data
{
    /// <summary>
    /// Equality comparer for <see cref="T:System.Data.DataSet"/> objects.
    /// </summary>
    public class DataSetEqualityComparer : IEqualityComparer<DataSet>
    {
        private DataSetEqualityComparer()
        {
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <see cref="T:System.Data.DataSet"/> to compare.</param>
        /// <param name="y">The second object of type <see cref="T:System.Data.DataSet"/> to compare.</param>
        public bool Equals(DataSet x, DataSet y)
        {
            DataTable yTable;
            object xField;
            object yField;

            // If the two data sets are the same object, they are equal
            if ((object)x == (object)y)
                return true;

            // Test for null
            if ((object)x == null || (object)y == null)
                return false;

            // Check that the number of tables match
            if (x.Tables.Count != y.Tables.Count)
                return false;

            foreach (DataTable xTable in x.Tables)
            {
                // Check that both data sets have this table defined
                if (!y.Tables.Contains(xTable.TableName))
                    return false;

                yTable = y.Tables[xTable.TableName];

                // Check that both tables have the same number of rows
                if (xTable.Rows.Count != yTable.Rows.Count)
                    return false;

                // Check that both tables have the same number of columns
                if (xTable.Columns.Count != yTable.Columns.Count)
                    return false;

                for (int i = 0; i < xTable.Rows.Count; i++)
                {
                    foreach (DataColumn column in xTable.Columns)
                    {
                        // Check that both tables contain this particular column
                        if (!yTable.Columns.Contains(column.ColumnName))
                            return false;

                        xField = xTable.Rows[i][column.ColumnName];
                        yField = yTable.Rows[i][column.ColumnName];

                        if (xField == DBNull.Value || yField == DBNull.Value)
                        {
                            // At least one value is DBNull,
                            // so this checks if they are both DBNull
                            if (xField != yField)
                                return false;
                        }
                        else if (!xField.Equals(yField))
                        {
                            // The values are not equal
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Data.DataSet"/> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(DataSet obj)
        {
            int hashCode = 0;
            object field;

            if ((object)obj == null)
                throw new ArgumentNullException(nameof(obj));

            // This method is required by IEqualityComparer<T> - although the following 
            // should work, this is not likely the most efficient way to do this...
            foreach (DataTable table in obj.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        field = row[column];

                        if ((object)field != null)
                            hashCode ^= field.GetHashCode();
                    }
                }
            }

            return hashCode;
        }

        /// <summary>
        /// Default instance of the <see cref="DataSetEqualityComparer"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DataSetEqualityComparer Default = new DataSetEqualityComparer();
    }
}
