//******************************************************************************************************
//  SerializableBadTimestampMeasurement.cs - Gbtc
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
//  12/18/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents an <see cref="IMeasurement"/> with a bad timestamp that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlType("BadTimestampMeasurement"), DataContract(Name = "BadTimestampMeasurement", Namespace = "")]
    public class SerializableBadTimestampMeasurement : SerializableMeasurement
    {

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableBadTimestampMeasurement"/> class.
        /// </summary>
        public SerializableBadTimestampMeasurement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableBadTimestampMeasurement"/> class.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> from which <see cref="SerializableOutOfRangeMeasurement"/> is to be initialized.</param>
        /// <param name="arrivalTime">The arrival time of the <see cref="IMeasurement"/>.</param>
        public SerializableBadTimestampMeasurement(IMeasurement measurement, Ticks arrivalTime)
            : base(measurement)
        {
            TimeOfArrival = ((DateTime)arrivalTime).ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the arrival time of the <see cref="IMeasurement"/>.
        /// </summary>
        [XmlAttribute, DataMember(Order = 6)]
        public string TimeOfArrival { get; set; }

        #endregion
        
    }
}
