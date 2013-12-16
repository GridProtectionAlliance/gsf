#region [ Code Modification History ]
/*
 *  06/18/2012 Denis Kholine
 *    Generated original version of source code
 */

#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/

#endregion

#region [ Using ]
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for SerializableMeasurementTest and is intended
    ///to contain all SerializableMeasurementTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SerializableMeasurementTest
    {
        #region [ Members ]
        private DateTime datetime1;
        private Measurement measurement1;
        private MeasurementKey measurementkey1;
        private Guid signalid1;

        #endregion

        #region [ Context ]
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        #region
        //
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //

        /// <summary>
        ///A test for BinaryLength
        ///</summary>
        [TestMethod()]
        public void BinaryLengthTest()
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            SerializableMeasurement target = new SerializableMeasurement(encoding);
            int actual;
            actual = target.BinaryLength;
            bool expected = (actual == 64);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for GenerateBinaryImage
        ///</summary>
        [TestMethod()]
        public void GenerateBinaryImageTest()
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            SerializableMeasurement target = new SerializableMeasurement(encoding);
            byte[] buffer = new byte[64];
            int startIndex = 0;
            int expected = 64;
            int actual;
            actual = target.GenerateBinaryImage(buffer, startIndex);
            Assert.AreEqual(expected, actual);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            datetime1 = DateTime.UtcNow;
            signalid1 = Guid.NewGuid();
            measurementkey1 = new MeasurementKey(signalid1, 10, "UnitTest");
            measurement1 = new Measurement();
            measurement1.Key = measurementkey1;
            measurement1.StateFlags = MeasurementStateFlags.Normal;
            measurement1.Value = 10;
            measurement1.PublishedTimestamp = datetime1;
            measurement1.ReceivedTimestamp = datetime1;
            measurement1.Timestamp = datetime1;
            measurement1.TagName = "M1";
            measurement1.ID = Guid.NewGuid();
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for ParseBinaryImage
        ///</summary>
        [TestMethod()]
        public void ParseBinaryImageTest()
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            SerializableMeasurement target = new SerializableMeasurement(measurement1, encoding);
            byte[] buffer = new byte[72];
            IBufferInit(ref buffer);
            int startIndex = 0;
            int length = 72;
            int expected = 69;
            int actual;
            actual = target.ParseBinaryImage(buffer, startIndex, length);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SerializableMeasurement Constructor
        ///</summary>
        [TestMethod()]
        public void SerializableMeasurementConstructorTest()
        {
            IMeasurement measurement = measurement1;
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            SerializableMeasurement target = new SerializableMeasurement(measurement, encoding);
        }

        /// <summary>
        ///A test for SerializableMeasurement Constructor
        ///</summary>
        [TestMethod()]
        public void SerializableMeasurementConstructorTest1()
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            SerializableMeasurement target = new SerializableMeasurement(encoding);
        }

        /// <summary>
        /// EndianOrder.BigEndian.TRCopyBytes(Key.ID, buffer, index);

        /// Field:      Bytes:   <br/>
        /// ---------   ---------<br/>
        ///  Key ID         4    <br/>
        /// SourceLen       4    <br/>
        ///  Source     SourceLen<br/>
        /// Signal ID      16    <br/>
        ///  TagLen         4    <br/>
        ///   Tag        TagLen  <br/>
        ///   Value         8    <br/>
        ///   Adder         8    <br/>
        /// Multipler       8    <br/>
        ///   Ticks         8    <br/>
        ///   Flags         4    <br/>
        ///
        /// </summary>
        /// <param name="buffer"></param>
        private void IBufferInit(ref byte[] buffer)
        {
            double size = 4;
            byte[] sizeInBytes = BitConverter.GetBytes(size);

            byte[] KeyID = new byte[4] { 0, 0, 0, 10 };
            byte[] SourceLen = new byte[4] { 0, 0, 0, 4 };
            byte[] Source = new byte[4] { sizeInBytes[0], sizeInBytes[1], sizeInBytes[2], sizeInBytes[3] };
            byte[] SignalID = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 };
            byte[] TagLen = new byte[4] { 0, 0, 0, 1 };
            byte[] Tag = new byte[4] { 1, 1, 1, 1 };
            byte[] Value = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            byte[] Adder = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            byte[] Multiplier = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            byte[] Ticks = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            byte[] Flags = new byte[4] { 1, 1, 1, 1 };

            int length;

            length = 0;
            Buffer.BlockCopy(KeyID, 0, buffer, length, KeyID.Length);
            length = length + KeyID.Length;
            Buffer.BlockCopy(SourceLen, 0, buffer, length, SourceLen.Length);
            length = length + SourceLen.Length;
            Buffer.BlockCopy(Source, 0, buffer, length, Source.Length);
            length = length + Source.Length;
            Buffer.BlockCopy(SignalID, 0, buffer, length, SignalID.Length);
            length = length + SignalID.Length;
            Buffer.BlockCopy(TagLen, 0, buffer, length, TagLen.Length);
            length = length + TagLen.Length;
            Buffer.BlockCopy(Tag, 0, buffer, length, Tag.Length);
            length = length + Tag.Length;
            Buffer.BlockCopy(Value, 0, buffer, length, Value.Length);
            length = length + Value.Length;
            Buffer.BlockCopy(Adder, 0, buffer, length, Adder.Length);
            length = length + Adder.Length;
            Buffer.BlockCopy(Multiplier, 0, buffer, length, Multiplier.Length);
            length = length + Multiplier.Length;
            Buffer.BlockCopy(Ticks, 0, buffer, length, Ticks.Length);
            length = length + Ticks.Length;
            Buffer.BlockCopy(Flags, 0, buffer, length, Flags.Length);
        }

        #endregion
    }
}