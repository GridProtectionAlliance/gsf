//******************************************************************************************************
//  IAlarmService.cs - Gbtc
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

using System.ServiceModel;
using System.ServiceModel.Web;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Defines a REST web service for alarms.
    /// </summary>
    [ServiceContract]
    public interface IAlarmService
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="AlarmAdapter"/> used by the web service for its data.
        /// </summary>
        AlarmAdapter AlarmAdapter { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all raised alarms from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/raisedalarms/all/xml")]
        SerializableAlarmCollection ReadAllRaisedAlarmsAsXml();

        /// <summary>
        /// Reads all raised alarms from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/raisedalarms/all/json")]
        SerializableAlarmCollection ReadAllRaisedAlarmsAsJson();

        /// <summary>
        /// Reads the raised alarms with the highest severity for each signal from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/raisedalarms/severe/xml")]
        SerializableAlarmCollection ReadHighestSeverityAlarmsAsXml();

        /// <summary>
        /// Reads the raised alarms with the highest severity for each signal from the <see cref="AlarmAdapter"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableAlarmCollection"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/raisedalarms/severe/json")]
        SerializableAlarmCollection ReadHighestSeverityAlarmsAsJson();

        #endregion
    }
}
