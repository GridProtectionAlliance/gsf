//******************************************************************************************************
//  Alarm.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  01/31/2012 - Stephen C. Wills
//       Generated original version of source code.
//  02/10/2012 - Stephen C. Wills
//       Moved to TimeSeriesFramework project.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    #region [ Enumerations ]

    /// <summary>
    /// Represents the two states that an alarm can be in: raised or cleared.
    /// </summary>
    public enum AlarmState
    {
        /// <summary>
        /// Indicates that an alarm is cleared.
        /// </summary>
        Cleared = 0,

        /// <summary>
        /// Indicates that an alarm has been raised.
        /// </summary>
        Raised = 1
    }

    /// <summary>
    /// Represents the severity of alarms.
    /// </summary>
    public enum AlarmSeverity
    {
        /// <summary>
        /// Indicates that an alarm is of no importance.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that an alarm is informative, but not dangerous.
        /// </summary>
        Information = 50,

        /// <summary>
        /// Indicates that an alarm is not very important.
        /// </summary>
        Low = 150,

        /// <summary>
        /// Indicates that an alarm is somewhat important.
        /// </summary>
        MediumLow = 300,

        /// <summary>
        /// Indicates that an alarm is moderately importance.
        /// </summary>
        Medium = 500,

        /// <summary>
        /// Indicates that an alarm is important.
        /// </summary>
        MediumHigh = 700,

        /// <summary>
        /// Indicates that an alarm is very important.
        /// </summary>
        High = 850,

        /// <summary>
        /// Indicates than an alarm signifies a dangerous situation.
        /// </summary>
        Critical = 950,

        /// <summary>
        /// Indicates that an alarm reports bad data.
        /// </summary>
        Error = 1000
    }

    /// <summary>
    /// Represents the operation to be performed
    /// when testing values from an incoming signal.
    /// </summary>
    public enum AlarmOperation
    {
        /// <summary>
        /// Internal range test
        /// </summary>
        Equal = 1,

        /// <summary>
        /// External range test
        /// </summary>
        NotEqual = 2,

        /// <summary>
        /// Upper bound
        /// </summary>
        GreaterOrEqual = 11,

        /// <summary>
        /// Lower bound
        /// </summary>
        LessOrEqual = 21,

        /// <summary>
        /// Upper limit
        /// </summary>
        GreaterThan = 12,

        /// <summary>
        /// Lower limit
        /// </summary>
        LessThan = 22,

        /// <summary>
        /// Latched value
        /// </summary>
        Flatline = 3
    }

    #endregion

    /// <summary>
    /// Represents an alarm that tests the values of
    /// an incoming signal to determine the state of alarm.
    /// </summary>
    public class Alarm
    {
        #region [ Members ]

        // Fields
        private Ticks m_lastNegative;

        private double m_lastValue;
        private Ticks m_lastChanged;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Alarm"/> class.
        /// </summary>
        public Alarm()
        {
            State = 0;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the identification number of the alarm.
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tag name of the alarm.
        /// </summary>
        public string TagName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the identification number of the
        /// signal whose value is monitored by the alarm.
        /// </summary>
        public Guid SignalID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the identification number of
        /// the measurements generated for alarm events.
        /// </summary>
        public Guid? AssociatedMeasurementID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the alarm.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the severity of the alarm.
        /// </summary>
        public AlarmSeverity Severity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operation to be performed
        /// when testing values from the incoming signal.
        /// </summary>
        public AlarmOperation Operation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value to be compared against
        /// the signal to determine whether to raise the
        /// alarm. This value is irrelevant for the
        /// <see cref="AlarmOperation.Flatline"/> operation.
        /// </summary>
        public double? SetPoint
        {
            get;
            set;
        }

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
        public double? Tolerance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the amount of time that the
        /// signal must be exhibiting alarming behavior
        /// before the alarm is raised.
        /// </summary>
        public double? Delay
        {
            get;
            set;
        }

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
        public double? Hysteresis
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state of the alarm (raised or cleared).
        /// </summary>
        public AlarmState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the most recent measurement
        /// that caused the alarm to be raised.
        /// </summary>
        public IMeasurement Cause
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Tests the value of the given signal to determine
        /// whether the alarm should be raised or cleared.
        /// </summary>
        /// <param name="signal">The signal whose value is to be tested.</param>
        /// <returns>true if the alarm's state changed; false otherwise</returns>
        public bool Test(IMeasurement signal)
        {
            AlarmState previousState = State;

            if (State == AlarmState.Raised && ClearsAlarm(signal))
            {
                State = AlarmState.Cleared;
            }
            else if (State == AlarmState.Cleared && RaisesAlarm(signal))
            {
                State = AlarmState.Raised;
                Cause = signal;
            }

            return State != previousState;
        }

        // Tests the given measurement to determine whether
        // its value triggers an alarm raised event.
        private bool RaisesAlarm(IMeasurement signal)
        {
            if (Operation != AlarmOperation.Flatline)
                return CheckRange(signal);

            return CheckFlatline(signal);
        }

        // Tests the given measurement to determine whether
        // its value indicates that the signal has been in
        // the alarming range for the configured delay time.
        private bool CheckRange(IMeasurement signal)
        {
            bool result = GetTestResult(signal);
            Ticks dist;

            if (!result)
            {
                // Keep track of the last time
                // the signal was in range
                m_lastNegative = signal.Timestamp;
            }
            else
            {
                // Get the amount of time since the
                // last time the value was in range
                dist = signal.Timestamp - m_lastNegative;

                // If the amount of time is larger than
                // the delay threshold, raise the alarm
                if (dist >= Ticks.FromSeconds(Delay.Value))
                    return true;
            }

            return false;
        }

        // Tests the signal to determine whether its
        // value is within the configured alarming range.
        private bool GetTestResult(IMeasurement signal)
        {
            double value = signal.Value;

            switch (Operation)
            {
                case AlarmOperation.Equal:
                    return (value <= SetPoint + Tolerance) && (value >= SetPoint - Tolerance);

                case AlarmOperation.NotEqual:
                    return (value < SetPoint - Tolerance) || (value > SetPoint + Tolerance);

                case AlarmOperation.GreaterOrEqual:
                    return value >= SetPoint;

                case AlarmOperation.LessOrEqual:
                    return value <= SetPoint;

                case AlarmOperation.GreaterThan:
                    return value > SetPoint;

                case AlarmOperation.LessThan:
                    return value < SetPoint;
            }

            return false;
        }

        // Tests the given measurement to determine whether
        // its value triggers an alarm cleared event.
        private bool ClearsAlarm(IMeasurement signal)
        {
            if (Operation != AlarmOperation.Flatline)
                return CheckRangeClear(signal);

            return !CheckFlatline(signal);
        }

        // Tests the given measurement to determine whether its value
        // indicates that the signal has left the alarming range.
        private bool CheckRangeClear(IMeasurement signal)
        {
            double value = signal.Value;

            switch (Operation)
            {
                case AlarmOperation.GreaterOrEqual:
                    return (value < SetPoint - Hysteresis);

                case AlarmOperation.LessOrEqual:
                    return (value > SetPoint + Hysteresis);

                case AlarmOperation.GreaterThan:
                    return (value <= SetPoint - Hysteresis);

                case AlarmOperation.LessThan:
                    return (value >= SetPoint + Hysteresis);

                default:
                    return !GetTestResult(signal);
            }
        }

        // Determines whether the given value has
        // flatlined over the configured delay interval.
        private bool CheckFlatline(IMeasurement signal)
        {
            long dist, diff;

            if (signal.Value != m_lastValue)
            {
                m_lastChanged = signal.Timestamp;
                m_lastValue = signal.Value;
            }

            dist = Ticks.FromSeconds(Delay.Value);
            diff = signal.Timestamp - m_lastChanged;

            return diff >= dist;
        }

        #endregion
    }
}
