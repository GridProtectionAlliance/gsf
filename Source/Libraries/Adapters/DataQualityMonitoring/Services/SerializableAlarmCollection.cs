//******************************************************************************************************
//  SerializableAlarmCollection.cs - Gbtc
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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a container for <see cref="SerializableAlarm"/>s that can be serialized using <see cref="XmlSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    [XmlRoot("RaisedAlarms"), DataContract(Name = "RaisedAlarms", Namespace = "")]
    public class SerializableAlarmCollection
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SerializableAlarmCollection"/> class.
        /// </summary>
        public SerializableAlarmCollection()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SerializableAlarmCollection"/> class.
        /// </summary>
        /// <param name="raisedAlarms">The collection of raised alarms to be contained by this <see cref="SerializableAlarmCollection"/>.</param>
        public SerializableAlarmCollection(ICollection<Alarm> raisedAlarms)
        {
            Alarms = raisedAlarms.Select(alarm => new SerializableAlarm(alarm)).ToArray();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="SerializableAlarm"/>s contained in the <see cref="SerializableAlarmCollection"/>.
        /// </summary>
        [XmlArray, DataMember]
        public SerializableAlarm[] Alarms { get; set; }

        #endregion
    }
}
