//******************************************************************************************************
//  SerializableAlarm.cs - Gbtc
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
//  02/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a serializable <see cref="Alarm"/> that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlType("Alarm"), DataContract(Name = "Alarm", Namespace = "")]
    public class SerializableAlarm
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SerializableAlarm"/> class.
        /// </summary>
        public SerializableAlarm()
        {
            Description = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SerializableAlarm"/> class.
        /// </summary>
        /// <param name="sourceAlarm"><see cref="Alarm"/> from which <see cref="SerializableAlarm"/> is to be initialized.</param>
        public SerializableAlarm(Alarm sourceAlarm)
        {
            ID = sourceAlarm.ID;
            TagName = sourceAlarm.TagName;
            Severity = (int)sourceAlarm.Severity;
            State = sourceAlarm.State;
            SignalID = sourceAlarm.SignalID.ToString();
            TimeRaised = ((DateTime)sourceAlarm.Cause.Timestamp).ToString("MM/dd/yyyy HH:mm:ss");
            ValueAtTimeRaised = sourceAlarm.Cause.AdjustedValue;
            Description = sourceAlarm.Description;
            Operation = (int)sourceAlarm.Operation;
            SetPoint = sourceAlarm.SetPoint ?? default(double);
            Tolerance = sourceAlarm.Tolerance ?? default(double);
            Delay = sourceAlarm.Delay ?? default(double);
            Hysteresis = sourceAlarm.Hysteresis ?? default(double);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the identification number of the alarm.
        /// </summary>
        [XmlAttribute, DataMember(Order = 0)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the tag name of the alarm.
        /// </summary>
        [XmlAttribute, DataMember(Order = 1)]
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the severity of the alarm.
        /// </summary>
        [XmlAttribute, DataMember(Order = 2)]
        public int Severity { get; set; }

        /// <summary>
        /// Gets or sets the state of the alarm (raised or cleared).
        /// </summary>
        [XmlAttribute, DataMember(Order = 3)]
        public AlarmState State { get; set; }

        /// <summary>
        /// Gets or sets the identification number of the
        /// signal whose value is monitored by the alarm.
        /// </summary>
        [XmlAttribute, DataMember(Order = 4)]
        public string SignalID { get; set; }

        /// <summary>
        /// Gets or sets the time at which the alarm was raised.
        /// </summary>
        [XmlAttribute, DataMember(Order = 5)]
        public string TimeRaised { get; set; }

        /// <summary>
        /// Gets or sets the value of the signal
        /// at the time that the alarm was raised.
        /// </summary>
        [XmlAttribute, DataMember(Order = 6)]
        public double ValueAtTimeRaised { get; set; }

        /// <summary>
        /// Gets or sets the description of the alarm.
        /// </summary>
        [XmlAttribute, DataMember(Order = 7)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the operation to be performed
        /// when testing values from the incoming signal.
        /// </summary>
        [XmlAttribute, DataMember(Order = 8)]
        public int Operation { get; set; }

        /// <summary>
        /// Gets or sets the value to be compared against
        /// the signal to determine whether to raise the
        /// alarm.
        /// </summary>
        [XmlAttribute, DataMember(Order = 9)]
        public double SetPoint { get; set; }

        /// <summary>
        /// Gets or sets a tolerance window around the
        /// <see cref="SetPoint"/> to use when comparing
        /// against the value of the signal.
        /// </summary>
        [XmlAttribute, DataMember(Order = 10)]
        public double Tolerance { get; set; }

        /// <summary>
        /// Gets or sets the amount of time that the
        /// signal must be exhibiting alarming behavior
        /// before the alarm is raised.
        /// </summary>
        [XmlAttribute, DataMember(Order = 11)]
        public double Delay { get; set; }

        /// <summary>
        /// Gets or sets the hysteresis used when clearing alarms.
        /// </summary>
        [XmlAttribute, DataMember(Order = 12)]
        public double Hysteresis { get; set; }

        #endregion
    }
}
