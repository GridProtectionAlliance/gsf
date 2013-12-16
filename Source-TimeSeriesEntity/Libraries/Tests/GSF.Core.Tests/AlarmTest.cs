#region [ Code Modification History ]
/*
 * 04/06/2012 - Denis Kholine
 *   Generated original version of source code.
 *
 * 04/19/2012 - Denis Kholine
 *   Remove unit tests for private methods.
 *
 * 05/03/2012 - Denis Kholine
 *   Move Alarm class testing under SIEGate_UIUC_ITI_TSF_Alarm branch
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

#region [ Code Using ]
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.Units;
using GSF.TimeSeries;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    ///<summary>
    /// This is a test class for AlarmTest and is intended to contain all AlarmTest Unit Tests
    /// Represents an alarm that tests the values of an incoming signal to determine the state of alarm.
    ///</summary>
    [TestClass()]
    public class AlarmTest
    {
        #region [ Properties ]
        /// <summary>
        /// Gets or sets the identification number of
        /// the measurements generated for alarm events.
        /// </summary>
        public Guid? AssociatedMeasurementID = new Nullable<Guid>();
        /// <summary>
        /// Gets or sets the most recent measurement
        /// that caused the alarm to be raised.
        /// </summary>
        public IMeasurement Cause = new Measurement();
        /// <summary>
        /// Gets or sets the amount of time that the
        /// signal must be exhibiting alarming behavior
        /// before the alarm is raised.
        /// </summary>
        public double? Delay = new Nullable<double>(10F);
        /// <summary>
        /// Gets or sets the description of the alarm.
        /// </summary>
        public string Description = "Description";
        /// <summary>
        /// Gets or sets the hysteresis used when clearing
        /// alarms. This value is only relevant in greater
        /// than (or equal) and less than (or equal) operations.
        /// </summary>
        /// <remarks>
        /// <para>The hysteresis is an offset that provides padding between
        /// the point at which the alarm is raised and the point at
        /// which the alarm is cleared. For example, in the case of the
        /// <see cref="AlarmOperation.GreaterOrEqual"/> operation:</para>
        ///
        /// <list type="bullet">
        /// <item>Raised: <c>value &gt;= SetPoint</c></item>
        /// <item>Cleared: <c>value &lt; SetPoint - Hysteresis</c></item>
        /// </list>
        ///
        /// <para>The direction of the offset depends on whether the
        /// operation is greater than (or equal) or less than (or equal).
        /// The hysteresis must be greater than zero.</para>
        /// </remarks>
        public double? Hysteresis = new Nullable<double>(10F);
        /// <summary>
        /// Gets or sets the identification number of the alarm.
        /// </summary>
        public int ID = 10;

        /// <summary>
        /// Gets or sets the operation to be performed
        /// when testing values from the incoming signal.
        /// </summary>
        public AlarmOperation Operation = AlarmOperation.Equal;
        /// <summary>
        /// Gets or sets the value to be compared against
        /// the signal to determine whether to raise the
        /// alarm. This value is irrelevant for the
        /// <see cref="AlarmOperation.Flatline"/> operation.
        /// </summary>
        public double? SetPoint = new Nullable<double>(10F);
        /// <summary>
        /// Gets or sets the severity of the alarm.
        /// </summary>
        public AlarmSeverity Severity = AlarmSeverity.Information;
        /// <summary>
        /// Gets or sets the identification number of the
        /// signal whose value is monitored by the alarm.
        /// TVA_SHELBY:ABBDF
        /// </summary>
        public Guid SignalID = new Guid("3647f729-d0ed-4f79-85ad-dae2149cd432");
        /// <summary>
        /// Gets or sets the state of the alarm (raised or cleared).
        /// </summary>
        public AlarmState State = AlarmState.Raised;
        /// <summary>
        /// Gets or sets the tag name of the alarm.
        /// </summary>
        public string TagName = "Unit Test";
        /// <summary>
        /// Gets or sets a tolerance window around the
        /// <see cref="SetPoint"/> to use when comparing
        /// against the value of the signal. This value
        /// is only relevant for the <see cref="AlarmOperation.Equal"/>
        /// and <see cref="AlarmOperation.NotEqual"/> operations.
        /// </summary>
        /// <remarks>
        /// <para>The equal and not equal operations are actually
        /// internal and external range tests based on the setpoint
        /// and the tolerance. The two tests are performed as follows.</para>
        ///
        /// <list type="bullet">
        /// <item>Equal: <c>(value &gt;= SetPoint - Tolerance) &amp;&amp; (value &lt;= SetPoint + Tolerance)</c></item>
        /// <item>Not equal: <c>(value &lt; SetPoint - Tolerance) || (value &gt; SetPoint + Tolerance)</c></item>
        /// </list>
        /// </remarks>
        public double? Tolerance = new Nullable<double>(10F);
        /// <summary>
        /// Expected
        /// </summary>
        private bool expected;

        /// <summary>
        /// Target
        /// </summary>
        private Alarm target;

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
        /// A test for Alarm Constructor
        /// Creates a new instance of the <see cref="Alarm"/> class.
        /// </summary>
        [TestMethod()]
        public void AlarmConstructorTest()
        {
            Assert.AreEqual(typeof(Alarm), target.GetType());
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// A test for AssociatedMeasurementID
        /// Gets or sets the identification number of
        /// the measurements generated for alarm events.
        /// </summary>
        [TestMethod()]
        public void AssociatedMeasurementIDTest()
        {
            target.AssociatedMeasurementID = this.AssociatedMeasurementID;
            expected = (target.AssociatedMeasurementID == this.AssociatedMeasurementID);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for Cause
        /// Gets or sets the most recent measurement
        /// that caused the alarm to be raised.
        /// </summary>
        [TestMethod()]
        public void CauseTest()
        {
            IMeasurement expected = new Measurement();
            IMeasurement actual;
            target.Cause = expected;
            actual = target.Cause;
            Assert.AreEqual(expected, actual);
            Assert.IsInstanceOfType(target.Cause, typeof(IMeasurement));
        }

        /// <summary>
        /// A test for Delay
        /// Gets or sets the amount of time that the
        /// signal must be exhibiting alarming behavior
        /// before the alarm is raised.
        /// </summary>
        [TestMethod()]
        public void DelayTest()
        {
            target.Delay = this.Delay;
            expected = (target.Delay == this.Delay);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for Description
        /// Gets or sets the description of the alarm.
        /// </summary>
        [TestMethod()]
        public void DescriptionTest()
        {
            target.Description = this.Description;
            expected = (target.Description == this.Description);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// A test for Hysteresis
        /// Gets or sets the hysteresis used when clearing
        /// alarms. This value is only relevant in greater
        /// than (or equal) and less than (or equal) operations.
        /// </summary>
        /// <remarks>
        /// <para>The hysteresis is an offset that provides padding between
        /// the point at which the alarm is raised and the point at
        /// which the alarm is cleared. For example, in the case of the
        /// <see cref="AlarmOperation.GreaterOrEqual"/> operation:</para>
        ///
        /// <list type="bullet">
        /// <item>Raised: <c>value &gt;= SetPoint</c></item>
        /// <item>Cleared: <c>value &lt; SetPoint - Hysteresis</c></item>
        /// </list>
        ///
        /// <para>The direction of the offset depends on whether the
        /// operation is greater than (or equal) or less than (or equal).
        /// The hysteresis must be greater than zero.</para>
        /// </remarks>
        [TestMethod()]
        public void HysteresisTest()
        {
            Nullable<double> expected = new Nullable<double>();
            Nullable<double> actual;
            target.Hysteresis = expected;
            actual = target.Hysteresis;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ID
        /// Gets or sets the identification number of the alarm.
        /// </summary>
        [TestMethod()]
        public void IDTest()
        {
            target.ID = this.ID;
            expected = (target.ID == this.ID);
            Assert.IsTrue(expected);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            expected = false;
            target = new Alarm();
        }

        #endregion

        #region [ Test Methods ]
        /// <summary>
        /// A test for Operation
        /// Gets or sets the operation to be performed
        /// when testing values from the incoming signal.
        /// </summary>
        [TestMethod()]
        public void OperationTest()
        {
            AlarmOperation expected = new AlarmOperation();
            AlarmOperation actual;
            target.Operation = expected;
            actual = target.Operation;
            Assert.AreEqual(expected, actual);
            Assert.IsInstanceOfType(target, typeof(Alarm));
        }

        /// <summary>
        /// A test for SetPoint
        /// Gets or sets the value to be compared against
        /// the signal to determine whether to raise the
        /// alarm. This value is irrelevant for the
        /// <see cref="AlarmOperation.Flatline"/> operation.
        /// </summary>
        [TestMethod()]
        public void SetPointTest()
        {
            Nullable<double> expected = new Nullable<double>();
            Nullable<double> actual;
            target.SetPoint = expected;
            actual = target.SetPoint;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Severity
        /// Gets or sets the severity of the alarm.
        /// </summary>
        [TestMethod()]
        public void SeverityTest()
        {
            AlarmSeverity expected = this.Severity;
            AlarmSeverity actual;
            target.Severity = expected;
            actual = target.Severity;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for SignalID
        /// Gets or sets the identification number of the
        /// signal whose value is monitored by the alarm.
        /// </summary>
        [TestMethod()]
        public void SignalIDTest()
        {
            Guid expected = this.SignalID;
            Guid actual;
            target.SignalID = expected;
            actual = target.SignalID;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for State
        /// Gets or sets the state of the alarm (raised or cleared).
        /// </summary>
        [TestMethod()]
        public void StateTest()
        {
            AlarmState expected = this.State;
            AlarmState actual;
            target.State = expected;
            actual = target.State;
            Assert.AreEqual(expected, actual);
            Assert.IsInstanceOfType(target.State, typeof(AlarmState));
        }

        /// <summary>
        /// A test for TagName
        /// Gets or sets the tag name of the alarm.
        /// </summary>
        [TestMethod()]
        public void TagNameTest()
        {
            string expected = this.TagName;
            string actual;
            target.TagName = expected;
            actual = target.TagName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Test
        ///</summary>
        [TestMethod()]
        public void TestTest()
        {
            IMeasurement signal = this.Cause;
            bool expected = false;
            bool actual;
            actual = target.Test(signal);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Tolerance
        /// Gets or sets a tolerance window around the
        /// <see cref="SetPoint"/> to use when comparing
        /// against the value of the signal. This value
        /// is only relevant for the <see cref="AlarmOperation.Equal"/>
        /// and <see cref="AlarmOperation.NotEqual"/> operations.
        /// </summary>
        /// <remarks>
        /// <para>The equal and not equal operations are actually
        /// internal and external range tests based on the setpoint
        /// and the tolerance. The two tests are performed as follows.</para>
        ///
        /// <list type="bullet">
        /// <item>Equal: <c>(value &gt;= SetPoint - Tolerance) &amp;&amp; (value &lt;= SetPoint + Tolerance)</c></item>
        /// <item>Not equal: <c>(value &lt; SetPoint - Tolerance) || (value &gt; SetPoint + Tolerance)</c></item>
        /// </list>
        /// </remarks>
        [TestMethod()]
        public void ToleranceTest()
        {
            Nullable<double> expected = this.Tolerance;
            Nullable<double> actual;
            target.Tolerance = expected;
            actual = target.Tolerance;
            Assert.AreEqual(expected, actual);
        }

        #endregion [ Test Methods ]
    }
}