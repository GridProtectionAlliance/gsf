//******************************************************************************************************
//  SerializableFlatlinedMeasurement.cs - Gbtc
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
//  12/10/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/11/2009 - Pinal C. Patel
//       Changed Timestamp to string and TimeSinceLastChange to double.
//  12/16/2009 - Stephen C. Wills
//       Refactored most of the implementation into the SerializableMeasurement base class.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a flatlined <see cref="IMeasurement"/> that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlType("FlatlinedMeasurement"), DataContract(Name = "FlatlinedMeasurement", Namespace = "")]
    public class SerializableFlatlinedMeasurement : SerializableMeasurement
    {

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableFlatlinedMeasurement"/> class.
        /// </summary>
        public SerializableFlatlinedMeasurement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableFlatlinedMeasurement"/> class.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> from which <see cref="SerializableFlatlinedMeasurement"/> is to be initialized.</param>
        /// <param name="timeSinceLastChange">The amount of time since the flatlined measurement last changed in ticks.</param>
        public SerializableFlatlinedMeasurement(IMeasurement measurement, long timeSinceLastChange)
            : base(measurement)
        {
            TimeSinceLastChange = Ticks.ToSeconds(timeSinceLastChange);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the amount of time in seconds since the <see cref="IMeasurement"/> last changed its value.
        /// </summary>
        [XmlAttribute, DataMember(Order = 6)]
        public double TimeSinceLastChange { get; set; }

        #endregion
    }
}
