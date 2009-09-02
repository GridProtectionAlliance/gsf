//*******************************************************************************************************
//  ITimeSeriesDataService.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/01/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System.ServiceModel;
using System.ServiceModel.Web;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Defines a REST web service for time-series data.
    /// </summary>
    /// <seealso cref="SerializableTimeSeriesData"/>
    [ServiceContract()]
    public interface ITimeSeriesDataService
    {
        #region [ Methods ]

        /// <summary>
        /// Writes <paramref name="data"/> received in <see cref="WebMessageFormat.Xml"/> format to the <see cref="ServiceBase.Archive"/>.
        /// </summary>
        /// <param name="data">An <see cref="SerializableTimeSeriesData"/> object.</param>
        [OperationContract(), 
        WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Xml, UriTemplate = "/timeseriesdata/write/xml")]
        void WriteTimeSeriesDataAsXml(SerializableTimeSeriesData data);

        /// <summary>
        /// Writes <paramref name="data"/> received in <see cref="WebMessageFormat.Json"/> format to the <see cref="ServiceBase.Archive"/>.
        /// </summary>
        /// <param name="data">An <see cref="SerializableTimeSeriesData"/> object.</param>
        [OperationContract(), 
        WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, UriTemplate = "/timeseriesdata/write/json")]
        void WriteTimeSeriesDataAsJson(SerializableTimeSeriesData data);

        /// <summary>
        /// Reads current time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/timeseriesdata/read/current/{idList}/xml")]
        SerializableTimeSeriesData ReadSelectCurrentTimeSeriesDataAsXml(string idList);

        /// <summary>
        /// Reads current time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which current time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/timeseriesdata/read/current/{fromID}-{toID}/xml")]
        SerializableTimeSeriesData ReadRangeCurrentTimeSeriesDataAsXml(string fromID, string toID);

        /// <summary>
        /// Reads current time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/timeseriesdata/read/current/{idList}/json")]
        SerializableTimeSeriesData ReadSelectCurrentTimeSeriesDataAsJson(string idList);

        /// <summary>
        /// Reads current time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which current time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which current time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/timeseriesdata/read/current/{fromID}-{toID}/json")]
        SerializableTimeSeriesData ReadRangeCurrentTimeSeriesDataAsJson(string fromID, string toID);

        /// <summary>
        /// Reads historic time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/timeseriesdata/read/historic/{idList}/{startTime}/{endTime}/xml")]
        SerializableTimeSeriesData ReadSelectHistoricTimeSeriesDataAsXml(string idList, string startTime, string endTime);

        /// <summary>
        /// Reads historic time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/timeseriesdata/read/historic/{fromID}-{toID}/{startTime}/{endTime}/xml")]
        SerializableTimeSeriesData ReadRangeHistoricTimeSeriesDataAsXml(string fromID, string toID, string startTime, string endTime);

        /// <summary>
        /// Reads historic time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/timeseriesdata/read/historic/{idList}/{startTime}/{endTime}/json")]
        SerializableTimeSeriesData ReadSelectHistoricTimeSeriesDataAsJson(string idList, string startTime, string endTime);

        /// <summary>
        /// Reads historic time-series data from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which historic time-series data is to be read.</param>
        /// <param name="startTime">Start time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <param name="endTime">End time in <see cref="System.String"/> format of the timespan for which historic time-series data is to be read.</param>
        /// <returns>An <see cref="SerializableTimeSeriesData"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/timeseriesdata/read/historic/{fromID}-{toID}/{startTime}/{endTime}/json")]
        SerializableTimeSeriesData ReadRangeHistoricTimeSeriesDataAsJson(string fromID, string toID, string startTime, string endTime);

        #endregion
    }
}
