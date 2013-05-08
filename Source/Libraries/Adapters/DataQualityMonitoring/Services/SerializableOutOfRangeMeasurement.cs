//******************************************************************************************************
//  SerializableOutOfRangeMeasurement.cs - Gbtc
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
//  12/16/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       MOdified Header.
//
//******************************************************************************************************

using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents an out-of-range <see cref="IMeasurement"/> that can be serialized using <see cref="XmlSerializer"/>, <see cref="DataContractSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlType("OutOfRangeMeasurement"), DataContract(Name = "OutOfRangeMeasurement", Namespace = "")]
    public class SerializableOutOfRangeMeasurement : SerializableMeasurement
    {

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableOutOfRangeMeasurement"/> class.
        /// </summary>
        public SerializableOutOfRangeMeasurement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableOutOfRangeMeasurement"/> class.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> from which <see cref="SerializableOutOfRangeMeasurement"/> is to be initialized.</param>
        /// <param name="lowRange">The lower boundary of the <see cref="IMeasurement"/>'s value.</param>
        /// <param name="highRange">The upper boundary of the <see cref="IMeasurement"/>'s value.</param>
        public SerializableOutOfRangeMeasurement(IMeasurement measurement, double lowRange, double highRange)
            : base(measurement)
        {
            LowRange = lowRange;
            HighRange = highRange;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the lower boundary of the <see cref="IMeasurement"/>'s value.
        /// </summary>
        [XmlAttribute, DataMember(Order = 6)]
        public double LowRange { get; set; }

        /// <summary>
        /// Gets or sets the upper boundary of the <see cref="IMeasurement"/>'s value.
        /// </summary>
        [XmlAttribute, DataMember(Order = 7)]
        public double HighRange { get; set; }

        #endregion
        
    }
}
