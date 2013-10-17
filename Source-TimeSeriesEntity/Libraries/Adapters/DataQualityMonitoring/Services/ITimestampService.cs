//******************************************************************************************************
//  ITimestampService.cs - Gbtc
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
//  12/21/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ServiceModel;
using System.ServiceModel.Web;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Defines a REST web service for measurements with bad timestamps.
    /// </summary>
    [ServiceContract]
    public interface ITimestampService
    {

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="TimestampTest"/> used by the web service for its data.
        /// </summary>
        TimestampTest Test { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all measurements with bad timestamps from the <see cref="Test"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/badtimestampmeasurements/read/xml")]
        SerializableTimestampTest ReadAllBadTimestampMeasurementsAsXml();

        /// <summary>
        /// Reads all measurements with bad timestamps from the <see cref="Test"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/badtimestampmeasurements/read/json")]
        SerializableTimestampTest ReadAllBadTimestampMeasurementsAsJson();

        /// <summary>
        /// Reads all measurements with bad timestamps from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for flatlined measurements.</param>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/badtimestampmeasurements/read/{device}/xml")]
        SerializableTimestampTest ReadBadTimestampMeasurementsFromDeviceAsXml(string device);

        /// <summary>
        /// Reads all measurements with bad timestamps from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for flatlined measurements.</param>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/badtimestampmeasurements/read/{device}/json")]
        SerializableTimestampTest ReadBadTimestampMeasurementsFromDeviceAsJson(string device);

        #endregion

    }
}
