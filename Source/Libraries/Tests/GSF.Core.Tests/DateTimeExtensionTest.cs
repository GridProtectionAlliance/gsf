//******************************************************************************************************
//  DateTimeExtensionTest.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  03/10/2011 - Aniket Salver
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{
    [TestClass]
    public class DateTimeExtensionTest
    {
        private double lagTime = 2, leadTime = 2;
        private BaselineTimeInterval testtime = BaselineTimeInterval.Day;
        private DateTime testTime1 = DateTime.UtcNow, testTime2 = DateTime.Now;
        private DateTime dtResult = DateTime.UtcNow;
        private string TestStr;
        private bool result;

        // This method will Determines if the specified UTC time is valid or not, by comparing it to the system clock time
        // and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void UtcTimeIsValid_ValidCase()
        {
            ////Act
            result = testTime2.UtcTimeIsValid(lagTime, leadTime);
            ////Assert
            Assert.AreEqual(false, result);

        }

        // This method will Determines if the specified UTC time is valid or not, by comparing it to the system clock time
        // and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void UtcTimeIsValid_InValidCase()
        {
            ////Act
            result = testTime1.UtcTimeIsValid(lagTime, leadTime);
            ////Assert
            Assert.AreEqual(true, result);
        }

        //  This method will Determines if the specified local time is valid or not, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]

        public void LocalTimeIsValid_ValidCase()
        {
            ////Act
            result = testTime1.LocalTimeIsValid(lagTime, leadTime);
            ////Assert
            Assert.AreEqual(false, result);
        }

        //  This method will Determines if the specified local time is valid or not, by comparing it to the system clock time
        //  and returns boolean variable as True for valid case and test will pass.
        [TestMethod]
        public void LocalTimeIsValid_InValidCases()
        {
            ////Act
            result = testTime2.LocalTimeIsValid(lagTime, leadTime);
            ////Assert
            Assert.AreEqual(true, result);
        }


        // This method will Determines if the specified time is valid, by comparing valid or not, by comparing it to the system clock time
        //  and returns boolean variable as False for valid case and test will pass.
        [TestMethod]
        public void TimeIsValid_ValidCase()
        {
            ////Act
            bool result = testTime1.TimeIsValid(testTime2, lagTime, leadTime);
            //// Assert
            Assert.AreEqual(false, result);
        }


        // This method will Determines if the specified time is valid, by comparing valid or not, by comparing it to the system clock time
        //  and returns boolean variable as True for valid case and test will pass.
        [TestMethod]
        public void TimeIsValid_InValidCase()
        {
            ////Act 
            bool result = testTime2.TimeIsValid(testTime2, lagTime, leadTime);
            ////Assert
            Assert.AreEqual(true, result);
        }

        // This method will creates a baselined timestamp which begins at the specified time interval,by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void BaselinedTimestamp_ValidCase()
        {
            ////Act
            DateTime result = testTime1.BaselinedTimestamp(testtime);
            ////Assert
            Assert.AreEqual(DateTime.Today, result);
        }

        // This method will creates a baselined timestamp which begins at the specified time interval, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void BaselinedTimestamp_InValidCases()
        {
            ////Act
            DateTime result = testTime2.BaselinedTimestamp(testtime);
            ////Assert
            Assert.AreEqual(DateTime.Today, result);
        }

        // This method Gets the abbreviated month name for month of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void AbbreviatedMonthName_ValidCase()
        {
            //// Act
            TestStr = testTime1.AbbreviatedMonthName();
            //// Assert
            Assert.AreEqual(DateTime.Now.AbbreviatedMonthName(), TestStr);
        }

        // This method Gets the abbreviated month name for month of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void AbbreviatedMonthName_InValidCases()
        {
            //// Act
            TestStr = testTime2.AbbreviatedMonthName();
            //// Assert
            Assert.AreEqual(DateTime.Now.AbbreviatedMonthName(), TestStr);
        }

        // This method Gets the full month name for month of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void MonthName_ValidCase()
        {
            //// Act
            TestStr = testTime1.MonthName();
            //// Assert
            Assert.AreEqual(DateTime.Now.MonthName(), TestStr);
        }

        // This method Gets the full month name for month of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void MonthName_InValidCase()
        {
            //// Act
            TestStr = testTime2.MonthName();
            //// Assert
            Assert.AreEqual(DateTime.Now.MonthName(), TestStr);
        }

        // This method Gets the abbreviated weekday name for weekday of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void AbbreviatedWeekdayName_ValidCase()
        {
            //// Act
            TestStr = testTime1.AbbreviatedWeekdayName();
            //// Assert
            Assert.AreEqual(DateTime.Now.AbbreviatedWeekdayName(), TestStr);
        }

        // This method Gets the abbreviated weekday name for weekday of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void AbbreviatedWeekdayName_InValidCase()
        {
            //// Act
            TestStr = testTime2.AbbreviatedWeekdayName();
            //// Assert
            Assert.AreEqual(DateTime.Now.AbbreviatedWeekdayName(), TestStr);
        }

        // This method Gets the shortest weekday name for weekday of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void ShortWeekdayName_ValidCase()
        {
            //// Act
            TestStr = testTime1.ShortWeekdayName();
            //// Assert
            Assert.AreEqual(DateTime.Now.ShortWeekdayName(), TestStr);
        }

        // This method Gets the shortest weekday name for weekday of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void ShortWeekdayName_InValidCase()
        {
            //// Act
            TestStr = testTime2.ShortWeekdayName();
            //// Assert
            Assert.AreEqual(DateTime.Now.ShortWeekdayName(), TestStr);

        }

        // This method Gets the full weekday name for weekday of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void WeekdayName_ValidCase()
        {
            //// Act
            TestStr = testTime1.WeekdayName();
            //// Assert
            Assert.AreEqual(DateTime.Today.DayOfWeek.ToString(), TestStr);

        }

        // This method Gets the full weekday name for weekday of the timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void WeekdayName_InValidCase()
        {
            //// Act
            TestStr = testTime2.WeekdayName();
            //// Assert
            Assert.AreEqual(DateTime.Today.DayOfWeek.ToString(), TestStr);

        }

        //  This method Converts given local time to Central timee, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void LocalTimeToCentralTime_Isvalid()
        {
            // Act
            DateTime result = testTime2.LocalTimeToCentralTime();
            double offset = -6.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -5.0;
            
            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);

        }

        // This method Converts given local time to Central timee, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void LocalTimeToCentralTime_InValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToCentralTime();
            double offset = -6.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -5.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
            
        }

        // This method Converts given local time to Mountain time, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void LocalTimeToMountainTime_ValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToMountainTime();
            double offset = -7.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -6.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
            
        }

        //  This method Converts given local time to Mountain time, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.

        [TestMethod]
        public void LocalTimeToMountainTime_InValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToMountainTime();
            double offset = -7.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -6.0;

            /// Assert
            Assert.AreEqual(dtResult.AddHours(offset), result);

        }

        // This method Converts given local time to Pacific time, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void LocalTimeToPacificTime_ValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToPacificTime();
            double offset = -8.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -7.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
        }

        // This method Converts given local time to Pacific time, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass.
        [TestMethod]
        public void LocalTimeToPacificTime_InValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToPacificTime();
            double offset = -8.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -7.0;

            /// Assert
            Assert.AreEqual(dtResult.AddHours(offset), result);
        }

        // This method Converts given local time to Universally Coordinated Time (a.k.a., Greenwich Meridian Time), by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void LocalTimeToUniversalTime_ValidCase()
        {
            //// Act
            DateTime result = testTime2.LocalTimeToUniversalTime();
            //// Assert
            Assert.AreEqual(testTime1, result);

        }

        ////  This method Converts given local time to Universally Coordinated Time (a.k.a., Greenwich Meridian Time), by comparing it to the system clock time
        ////  and returns boolean variable as true for valid case and test will pass
        //[TestMethod]
        //public void LocalTimeToUniversalTime_InValidCase()
        //{
        //    //// Act
        //    DateTime result = testTime2.LocalTimeToUniversalTime();
        //    //// Assert
        //    Assert.AreNotEqual(dtResult, result);

        //}

        // This method Converts given local time to time in specified time zone, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void LocalTimeTo_ValidCase()
        {
            //// Act
            DateTime result = testTime2.LocalTimeTo(TimeZoneInfo.Local);
            //// Assert
            Assert.AreEqual(testTime2, result);

        }

        // This method Converts given local time to time in specified time zone, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void LocalTimeTo_InValidCase()
        {
            //// Act
            DateTime result = testTime2.LocalTimeTo(TimeZoneInfo.Local);
            //// Assert
            Assert.AreEqual(testTime2, result);

        }

        // This method Converts the specified Universally Coordinated Time timestamp to Eastern time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToEasternTime_ValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToEasternTime();
            double offset = -5.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -4.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
        }

        // This method Converts the specified Universally Coordinated Time timestamp to Eastern time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToEasternTime_InValidCase()
        {
            DateTime dt = DateTime.UtcNow;
            // Act
            DateTime result = testTime1.UniversalTimeToEasternTime();
            double offset = -5.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -4.0;

            /// Assert
            Assert.AreEqual(dtResult.AddHours(offset), result);
        }

        // This method Converts the specified Universally Coordinated Time timestamp to Central time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass.
        [TestMethod]
        public void UniversalTimeToCentralTime_ValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToCentralTime();
            double offset = -6.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -5.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
        }

        // This method Converts the specified Universally Coordinated Time timestamp to Central time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToCentralTime_InValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToCentralTime();
            double offset = -6.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -5.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
        }

        //  This method Converts the specified Universally Coordinated Time timestamp to Mountain time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToMountainTime_ValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToMountainTime();
            double offset = -7.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -6.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
        }

        // This method Converts the specified Universally Coordinated Time timestamp to Mountain time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToMountainTime_InValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToMountainTime();
            double offset = -7.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -6.0;

            /// Assert
            Assert.AreEqual(dtResult.AddHours(offset), result);
        }

        // This method Converts the specified Universally Coordinated Time timestamp to Pacific time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToPacificTime_AValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToPacificTime();
            double offset = -8.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -7.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);
        }

        //  This method Converts the specified Universally Coordinated Time timestamp to Pacific time timestamp, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void UniversalTimeToPacificTime_InValidCase()
        {
            // Act
            DateTime result = testTime1.UniversalTimeToPacificTime();
            double offset = -8.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -7.0;

            /// Assert
            Assert.AreEqual(dtResult.AddHours(offset), result);
        }

        // This method Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void UniversalTimeTo_ValidCase()
        {
            //// Act
            DateTime result = testTime1.UniversalTimeTo(TimeZoneInfo.Utc);
            //// Assert
            Assert.AreEqual(testTime1, result);

        }

        // This method Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void UniversalTimeTo_InValidCase()
        {
            //// Act
            DateTime result = testTime1.UniversalTimeTo(TimeZoneInfo.Utc);
            //// Asserts
            Assert.AreEqual(dtResult, result);

        }

        // This method Converts given timestamp from one time zone to another using standard names for time zones, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void TimeZoneToTimeZone_ValidCase()
        {
            //// Act
            DateTime result = testTime2.TimeZoneToTimeZone(TimeZoneInfo.Local, TimeZoneInfo.Local);
            //// Assert
            Assert.AreEqual(testTime2, result);

        }

        // This method Converts given timestamp from one time zone to another using standard names for time zones, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void TimeZoneToTimeZone_InValidCase()
        {
            //// Act
            DateTime result = testTime2.TimeZoneToTimeZone(TimeZoneInfo.Local, TimeZoneInfo.Local); //(TimeZoneInfo.Utc, TimeZoneInfo.Utc);
            //// Assert
            Assert.AreEqual(testTime2, result);

        }

        // This method Converts given local time to Eastern time, by comparing it to the system clock time
        //  and returns boolean variable as false for valid case and test will pass
        [TestMethod]
        public void LocalTimeToEasternTime_ValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToEasternTime();
            double offset = -5.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -4.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);

        }

        // This method Converts given local time to Eastern time, by comparing it to the system clock time
        //  and returns boolean variable as true for valid case and test will pass
        [TestMethod]
        public void LocalTimeToEasternTime_InValidCase()
        {
            // Act
            DateTime result = testTime2.LocalTimeToEasternTime();
            double offset = -5.0;

            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                offset = -4.0;

            /// Assert
            Assert.AreEqual(testTime1.AddHours(offset), result);

        }
    }
}
