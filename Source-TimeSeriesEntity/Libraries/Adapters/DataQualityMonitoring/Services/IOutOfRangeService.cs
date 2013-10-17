//******************************************************************************************************
//  IOutOfRangeService.cs - Gbtc
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
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Defines a REST web service for out-of-range measurements.
    /// </summary>
    [ServiceContract]
    public interface IOutOfRangeService
    {

        #region [ Properties ]

        /// <summary>
        /// Gets or collection of <see cref="RangeTest"/>s used by the web service for its data.
        /// </summary>
        ICollection<RangeTest> Tests { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all out-of-range measurements from the <see cref="Tests"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/outofrangemeasurements/read/xml")]
        SerializableRangeTestCollection ReadAllOutOfRangeMeasurementsAsXml();

        /// <summary>
        /// Reads all out-of-range measurements from the <see cref="Tests"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/outofrangemeasurements/read/json")]
        SerializableRangeTestCollection ReadAllOutOfRangeMeasurementsAsJson();

        /// <summary>
        /// Reads all out-of-range measurements with the specified signal type and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="signalType">The signal type of the desired out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/outofrangemeasurements/read/signaltype:{signalType}/xml")]
        SerializableRangeTestCollection ReadOutOfRangeMeasurementsWithSignalTypeAsXml(string signalType);

        /// <summary>
        /// Reads all out-of-range measurements with the specified signal type and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="signalType">The signal type of the desired out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/outofrangemeasurements/read/signaltype:{signalType}/json")]
        SerializableRangeTestCollection ReadOutOfRangeMeasurementsWithSignalTypeAsJson(string signalType);

        /// <summary>
        /// Reads all out-of-range measurements from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/outofrangemeasurements/read/device:{device}/xml")]
        SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromDeviceAsXml(string device);

        /// <summary>
        /// Reads all out-of-range measurements from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/outofrangemeasurements/read/device:{device}/json")]
        SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromDeviceAsJson(string device);

        /// <summary>
        /// Reads all out-of-range measurements from the specified <see cref="RangeTest"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="test">The name of the <see cref="RangeTest"/> to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/outofrangemeasurements/read/test:{test}/xml")]
        SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromTestAsXml(string test);

        /// <summary>
        /// Reads all out-of-range measurements from the specified <see cref="RangeTest"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="test">The name of the <see cref="RangeTest"/> to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTest"/> object.</returns>
        [OperationContract,
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/outofrangemeasurements/read/test:{test}/json")]
        SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromTestAsJson(string test);

        #endregion
        
    }
}
