//******************************************************************************************************
//  DataSetSerializationTest.cs - Gbtc
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
//  02/19/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using GSF.Data;
using GSF.IO;
using GSF.Units;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GSF.Core.Tests
{
    [TestClass()]
    public class DataSetSerializationTest
    {
        // This method will Determines if the specified UTC time is valid or not, by comparing it to the system clock time
        // and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void DataSetSerialization_ValidCase()
        {
            const int RowCount = ushort.MaxValue * 20;

            //Act
            StringBuilder results = new StringBuilder();
            Ticks stopTime, startTime;
            
            // TODO: Add more tables and columns for all serializable data types to make test more comprehensive
            DataSet sourceDataSet = new DataSet("source");
            DataTable table = sourceDataSet.Tables.Add("table1");

            table.Columns.Add("col1", typeof(string));
            table.Columns.Add("col2", typeof(int));
            table.Columns.Add("col3", typeof(bool));

            startTime = PrecisionTimer.UtcNow.Ticks;

            for (int i = 0; i < RowCount; i++)
            {
                DataRow row = table.NewRow();

                row[0] = new string((char)GSF.Security.Cryptography.Random.Int16Between(32, 128), GSF.Security.Cryptography.Random.Int16Between(5, 30));
                row[1] = GSF.Security.Cryptography.Random.Int32;
                row[2] = GSF.Security.Cryptography.Random.Boolean;

                table.Rows.Add(row);
            }

            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Initial random sample dataset created with {0} rows. ({1})\r\n", RowCount, (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendLine();

            FileStream stream;
            string path = FilePath.GetApplicationDataFolder();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fileName = Path.Combine(path, "DataSet.bin");

            startTime = PrecisionTimer.UtcNow.Ticks;

            stream = new FileStream(fileName, FileMode.Create);
            sourceDataSet.SerializeToStream(stream);
            stream.Flush();
            stream.Close();
            stream.Dispose();

            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Dataset binary serialization time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            string xmlFileName = Path.Combine(path, "DataSet.xml");

            startTime = PrecisionTimer.UtcNow.Ticks;

            sourceDataSet.WriteXml(xmlFileName, XmlWriteMode.WriteSchema);

            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Dataset XML serialization time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendLine();

            DataSet destinationDataSet;

            startTime = PrecisionTimer.UtcNow.Ticks;

            stream = new FileStream(fileName, FileMode.Open);
            destinationDataSet = stream.DeserializeToDataSet();
            stream.Close();
            stream.Dispose();

            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Dataset binary deserialization time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));

            DataSet tempDataSet;

            startTime = PrecisionTimer.UtcNow.Ticks;

            tempDataSet = new DataSet();
            tempDataSet.ReadXml(xmlFileName);

            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Dataset XML deserialization time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendLine();

            startTime = PrecisionTimer.UtcNow.Ticks;

            // Validate that source and destination dataset are the same
            Assert.AreEqual(sourceDataSet.DataSetName, destinationDataSet.DataSetName);
            Assert.AreEqual(sourceDataSet.Tables.Count, destinationDataSet.Tables.Count);

            foreach (DataTable sourceTable in sourceDataSet.Tables)
            {
                bool tableExists = destinationDataSet.Tables.Contains(sourceTable.TableName);
                Assert.IsTrue(tableExists);

                DataTable destinationTable = destinationDataSet.Tables[sourceTable.TableName];
                Assert.AreEqual(sourceTable.Columns.Count, destinationTable.Columns.Count);

                foreach (DataColumn sourceColumn in sourceTable.Columns)
                {
                    bool columnExists = destinationTable.Columns.Contains(sourceColumn.ColumnName);
                    Assert.IsTrue(columnExists);

                    DataColumn destinationColumn = destinationTable.Columns[sourceColumn.ColumnName];
                    Assert.IsTrue(sourceColumn.DataType == destinationColumn.DataType);
                }

                Assert.AreEqual(sourceTable.Rows.Count, destinationTable.Rows.Count);

                for (int i = 0; i < sourceTable.Rows.Count; i++)
                {
                    DataRow sourceRow = sourceTable.Rows[i];
                    DataRow destinationRow = destinationTable.Rows[i];

                    for (int j = 0; j < sourceTable.Columns.Count; j++)
                    {
                        Assert.AreEqual(sourceRow[j], destinationRow[j]);
                    }
                }
            }

            stopTime = PrecisionTimer.UtcNow.Ticks;
            results.AppendFormat("Dataset validation time: {0}\r\n", (stopTime - startTime).ToElapsedTimeString(4));
            results.AppendLine();
            
            FileInfo xmlFile = new FileInfo(xmlFileName);
            FileInfo binFile = new FileInfo(fileName);

            results.AppendFormat("Binary serialization size =  {0}\r\n", SI2.ToScaledString(binFile.Length, "B"));
            results.AppendFormat("XML serialization size =  {0}\r\n", SI2.ToScaledString(xmlFile.Length, "B"));

            results.AppendFormat("Size Improvement = {0:0.00%}\r\n", (xmlFile.Length - binFile.Length) / (double)xmlFile.Length);

            Debug.WriteLine(results.ToString());
        }
    }
}
