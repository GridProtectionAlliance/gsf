//******************************************************************************************************
//  AlarmService.cs - Gbtc
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
//  02/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ServiceModel;
using GSF.ServiceModel;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a REST web service for alarms.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AlarmService : SelfHostingService, IAlarmService
    {
        #region [ Members ]

        // Fields
        private AlarmAdapter m_alarmAdapter;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmService"/> class.
        /// </summary>
        /// <param name="adapter">The adapter whose alarms are served by this service.</param>
        public AlarmService(AlarmAdapter adapter)
        {
            AlarmAdapter = adapter;

            Singleton = true;
            PersistSettings = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the alarm adapter whose
        /// alarms are served by this service.
        /// </summary>
        public AlarmAdapter AlarmAdapter
        {
            get
            {
                return m_alarmAdapter;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                m_alarmAdapter = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all raised alarms from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        public SerializableAlarmCollection ReadAllRaisedAlarmsAsXml()
        {
            return ReadAllRaisedAlarms();
        }

        /// <summary>
        /// Reads all raised alarms from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        public SerializableAlarmCollection ReadAllRaisedAlarmsAsJson()
        {
            return ReadAllRaisedAlarms();
        }

        /// <summary>
        /// Reads the raised alarms with the highest severity for each signal from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        public SerializableAlarmCollection ReadHighestSeverityAlarmsAsXml()
        {
            return ReadHighestSeverityAlarms();
        }

        /// <summary>
        /// Reads the raised alarms with the highest severity for each signal from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        public SerializableAlarmCollection ReadHighestSeverityAlarmsAsJson()
        {
            return ReadHighestSeverityAlarms();
        }

        // Reads all raised alarms from the <see cref="AlarmAdapter"/>.
        private SerializableAlarmCollection ReadAllRaisedAlarms()
        {
            return new SerializableAlarmCollection(AlarmAdapter.GetRaisedAlarms());
        }

        // Reads the raised alarms with the highest severity for each signal from the <see cref="AlarmAdapter"/>.
        private SerializableAlarmCollection ReadHighestSeverityAlarms()
        {
            return new SerializableAlarmCollection(AlarmAdapter.GetHighestSeverityAlarms());
        }

        #endregion
    }
}
