//******************************************************************************************************
//  SerializableFlatlineTest.cs - Gbtc
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
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modifed Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a container for <see cref="SerializableFlatlinedMeasurement"/>s that can be serialized using <see cref="XmlSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlRoot("FlatlineTest"), DataContract(Name = "FlatlineTest", Namespace = "")]
    public class SerializableFlatlineTest
    {

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableFlatlineTest"/> class.
        /// </summary>
        public SerializableFlatlineTest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableFlatlineTest"/> class.
        /// </summary>
        /// <param name="flatlinedMeasurements">The collection of flatlined measurements to be contained by this <see cref="SerializableFlatlineTest"/>.</param>
        /// <param name="currentTime">The current time in ticks.</param>
        public SerializableFlatlineTest(ICollection<IMeasurement> flatlinedMeasurements, long currentTime)
        {
            List<SerializableFlatlinedMeasurement> serializableFlatlinedMeasurements = new List<SerializableFlatlinedMeasurement>();
            foreach (IMeasurement measurement in flatlinedMeasurements)
            {
                long timeSinceLastChange = currentTime - measurement.Timestamp;
                serializableFlatlinedMeasurements.Add(new SerializableFlatlinedMeasurement(measurement, timeSinceLastChange));
            }
            FlatlinedMeasurements = serializableFlatlinedMeasurements.ToArray();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="SerializableFlatlinedMeasurement"/>s contained in the <see cref="SerializableFlatlineTest"/>.
        /// </summary>
        [XmlArray, DataMember]
        public SerializableFlatlinedMeasurement[] FlatlinedMeasurements { get; set; }

        #endregion
        
    }
}
