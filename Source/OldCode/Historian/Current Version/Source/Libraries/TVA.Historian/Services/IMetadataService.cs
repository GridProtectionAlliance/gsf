//*******************************************************************************************************
//  IMetadataService.cs
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
//  08/28/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System.ServiceModel;
using System.ServiceModel.Web;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Defines a REST web service for historian metadata.
    /// </summary>
    /// <seealso cref="SerializableMetadata"/>
    [ServiceContract()]
    public interface IMetadataService
    {
        #region [ Methods ]

        /// <summary>
        /// Writes <paramref name="metadata"/> received in <see cref="WebMessageFormat.Xml"/> format to the <see cref="Service.Archive"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="SerializableMetadata"/> object.</param>
        [OperationContract(), 
        WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Xml, UriTemplate = "/metadata/write/xml")]
        void WriteMetadataAsXml(SerializableMetadata metadata);

        /// <summary>
        /// Writes <paramref name="metadata"/> received in <see cref="WebMessageFormat.Json"/> format to the <see cref="Service.Archive"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="SerializableMetadata"/> object.</param>
        [OperationContract(), 
        WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, UriTemplate = "/metadata/write/json")]
        void WriteMetadataAsJson(SerializableMetadata metadata);

        /// <summary>
        /// Reads all metadata from the <see cref="Service.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/metadata/read/xml")]
        SerializableMetadata ReadAllMetadataAsXml();

        /// <summary>
        /// Reads a subset of metadata from the <see cref="Service.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/metadata/read/{idList}/xml")]
        SerializableMetadata ReadSelectMetadataAsXml(string idList);

        /// <summary>
        /// Reads a subset of metadata from the <see cref="Service.Archive"/> and sends it in <see cref="WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which metadata is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/metadata/read/{fromID}-{toID}/xml")]
        SerializableMetadata ReadRangeMetadataAsXml(string fromID, string toID);

        /// <summary>
        /// Reads all metadata from the <see cref="Service.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/metadata/read/json")]
        SerializableMetadata ReadAllMetadataAsJson();

        /// <summary>
        /// Reads a subset of metadata from the <see cref="Service.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/metadata/read/{idList}/json")]
        SerializableMetadata ReadSelectMetadataAsJson(string idList);

        /// <summary>
        /// Reads a subset of metadata from the <see cref="Service.Archive"/> and sends it in <see cref="WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which metadata is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        [OperationContract(), 
        WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/metadata/read/{fromID}-{toID}/json")]
        SerializableMetadata ReadRangeMetadataAsJson(string fromID, string toID);

        #endregion
    }
}
