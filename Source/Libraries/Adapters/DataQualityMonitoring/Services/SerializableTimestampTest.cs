//******************************************************************************************************
//  SerializableTimestampTest.cs - Gbtc
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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a container for <see cref="SerializableBadTimestampMeasurement"/>s that can be serialized using <see cref="XmlSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlRoot("TimestampTest"), DataContract(Name = "TimestampTest", Namespace = "")]
    public class SerializableTimestampTest
    {

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTimestampTest"/>.
        /// </summary>
        public SerializableTimestampTest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTimestampTest"/>.
        /// </summary>
        /// <param name="badTimestampMeasurements">
        /// A dictionary where the values are <see cref="LinkedList{T}"/>s containing measurements and the keys are the arrival
        /// times of the measurements. The measurements are to be stored in this <see cref="SerializableTimestampTest"/>.
        /// </param>
        public SerializableTimestampTest(Dictionary<Ticks, LinkedList<IMeasurement>> badTimestampMeasurements)
        {
            List<SerializableBadTimestampMeasurement> serializableBadTimestampMeasurements = new List<SerializableBadTimestampMeasurement>();

            foreach (Ticks arrivalTime in badTimestampMeasurements.Keys)
            {
                foreach (IMeasurement measurement in badTimestampMeasurements[arrivalTime])
                {
                    serializableBadTimestampMeasurements.Add(new SerializableBadTimestampMeasurement(measurement, arrivalTime));
                }
            }

            BadTimestampMeasurements = serializableBadTimestampMeasurements.ToArray();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="SerializableBadTimestampMeasurement"/>s contained in the <see cref="SerializableTimestampTest"/>.
        /// </summary>
        [XmlArray, DataMember]
        public SerializableBadTimestampMeasurement[] BadTimestampMeasurements { get; set; }

        #endregion
    }
}
