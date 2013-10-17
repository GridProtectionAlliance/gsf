//******************************************************************************************************
//  SerializableRangeTest.cs - Gbtc
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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a container for <see cref="SerializableOutOfRangeMeasurement"/>s that can be serialized using <see cref="XmlSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlType("RangeTest"), DataContract(Name = "RangeTest", Namespace = "")]
    public class SerializableRangeTest
    {

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableRangeTest"/>.
        /// </summary>
        public SerializableRangeTest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableRangeTest"/>.
        /// </summary>
        /// <param name="acronym">The acronym of the <see cref="SerializableRangeTest"/></param>
        public SerializableRangeTest(string acronym)
        {
            Acronym = acronym;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableRangeTest"/>.
        /// </summary>
        /// <param name="outOfRangeMeasurements">The collection of out-of-range measurements to be contained by this <see cref="SerializableRangeTest"/>.</param>
        /// <param name="lowRange">The lower boundary of the measurements' values.</param>
        /// <param name="highRange">The upper boundary of the measurements' values.</param>
        public SerializableRangeTest(ICollection<IMeasurement> outOfRangeMeasurements, double lowRange, double highRange)
        {
            List<SerializableOutOfRangeMeasurement> serializableOutOfRangeMeasurements = new List<SerializableOutOfRangeMeasurement>();
            foreach (IMeasurement measurement in outOfRangeMeasurements)
            {
                serializableOutOfRangeMeasurements.Add(new SerializableOutOfRangeMeasurement(measurement, lowRange, highRange));
            }
            OutOfRangeMeasurements = serializableOutOfRangeMeasurements.ToArray();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the index of the range test.
        /// </summary>
        [XmlAttribute, DataMember(Order = 0)]
        public string Acronym { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SerializableOutOfRangeMeasurement"/>s contained in the <see cref="SerializableRangeTest"/>.
        /// </summary>
        [XmlArray, DataMember(Order = 1)]
        public SerializableOutOfRangeMeasurement[] OutOfRangeMeasurements { get; set; }

        #endregion
        
    }
}
