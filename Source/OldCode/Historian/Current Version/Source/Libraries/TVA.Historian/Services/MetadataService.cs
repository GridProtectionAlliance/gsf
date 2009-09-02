//*******************************************************************************************************
//  MetadataService.cs
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

using System;
using System.Collections.Generic;
using System.ServiceModel;
using TVA.Historian.Files;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Represents a REST web service for historian metadata.
    /// </summary>
    /// <seealso cref="SerializableMetadata"/>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MetadataService : ServiceBase, IMetadataService
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataService"/> class.
        /// </summary>
        public MetadataService()
            :base()
        {
            ServiceUri = "http://localhost:5151/historian";
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Writes <paramref name="metadata"/> received in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format to the <see cref="ServiceBase.Archive"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="SerializableMetadata"/> object.</param>
        public void WriteMetadataAsXml(SerializableMetadata metadata)
        {
            WriteMetadata(metadata);
        }

        /// <summary>
        /// Writes <paramref name="metadata"/> received in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format to the <see cref="ServiceBase.Archive"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="SerializableMetadata"/> object.</param>
        public void WriteMetadataAsJson(SerializableMetadata metadata)
        {
            WriteMetadata(metadata);
        }

        /// <summary>
        /// Reads all metadata from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        public SerializableMetadata ReadAllMetadataAsXml()
        {
            return ReadMetadata();
        }

        /// <summary>
        /// Reads a subset of metadata from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        public SerializableMetadata ReadSelectMetadataAsXml(string idList)
        {
            return ReadMetadata(idList);
        }

        /// <summary>
        /// Reads a subset of metadata from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which metadata is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        public SerializableMetadata ReadRangeMetadataAsXml(string fromID, string toID)
        {
            return ReadMetadata(fromID, toID);
        }

        /// <summary>
        /// Reads all metadata from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        public SerializableMetadata ReadAllMetadataAsJson()
        {
            return ReadMetadata();
        }

        /// <summary>
        /// Reads a subset of metadata from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="idList">A comma or semi-colon delimited list of IDs for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        public SerializableMetadata ReadSelectMetadataAsJson(string idList)
        {
            return ReadMetadata(idList);
        }

        /// <summary>
        /// Reads a subset of metadata from the <see cref="ServiceBase.Archive"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <param name="fromID">Starting ID in the ID range for which metadata is to be read.</param>
        /// <param name="toID">Ending ID in the ID range for which metadata is to be read.</param>
        /// <returns>An <see cref="SerializableMetadata"/> object.</returns>
        public SerializableMetadata ReadRangeMetadataAsJson(string fromID, string toID)
        {
            return ReadMetadata(fromID, toID);
        }

        private void WriteMetadata(SerializableMetadata metadata)
        {
            try
            {
                // Ensure that writing data is allowed.
                if (!CanWrite)
                    throw new InvalidOperationException("Write operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Write all metadata records to the archive.
                foreach (SerializableMetadataRecord record in metadata.MetadataRecords)
                {
                    Archive.WriteMetaData(record.HistorianID, record.Deflate().BinaryImage);
                }
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableMetadata ReadMetadata()
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read all metadata records from the archive.
                int id = 0;
                byte[] buffer = null;
                SerializableMetadata metadata = new SerializableMetadata();
                List<SerializableMetadataRecord> records = new List<SerializableMetadataRecord>();
                while (true)
                {
                    try
                    {
                        id++;
                        buffer = Archive.ReadMetaData(id);
                        records.Add(new SerializableMetadataRecord(new MetadataRecord(id, buffer, 0, buffer.Length)));
                    }
                    catch
                    {
                        // Exception will be thrown when the specified id is not valid indicating EOF.
                        break;
                    }
                }
                metadata.MetadataRecords = records.ToArray();

                return metadata;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableMetadata ReadMetadata(string idList)
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read specified metadata records from the archive.
                int id = 0;
                byte[] buffer = null;
                SerializableMetadata metadata = new SerializableMetadata();
                List<SerializableMetadataRecord> records = new List<SerializableMetadataRecord>();
                foreach (string singleID in idList.Split(',', ';'))
                {
                    id = int.Parse(singleID);
                    buffer = Archive.ReadMetaData(id);
                    records.Add(new SerializableMetadataRecord(new MetadataRecord(id, buffer, 0, buffer.Length)));
                }
                metadata.MetadataRecords = records.ToArray();

                return metadata;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        private SerializableMetadata ReadMetadata(string fromID, string toID)
        {
            try
            {
                // Ensure that reading data is allowed.
                if (!CanRead)
                    throw new InvalidOperationException("Read operation is prohibited.");

                // Ensure that data archive is available.
                if (Archive == null)
                    throw new ArgumentNullException("Archive");

                // Read specified metadata records from the archive.
                byte[] buffer = null;
                SerializableMetadata metadata = new SerializableMetadata();
                List<SerializableMetadataRecord> records = new List<SerializableMetadataRecord>();
                for (int id = int.Parse(fromID); id <= int.Parse(toID); id++)
                {
                    buffer = Archive.ReadMetaData(id);
                    records.Add(new SerializableMetadataRecord(new MetadataRecord(id, buffer, 0, buffer.Length)));
                }
                metadata.MetadataRecords = records.ToArray();

                return metadata;
            }
            catch (Exception ex)
            {
                // Notify about the encountered processing exception.
                OnServiceProcessError(ex);
                throw;
            }
        }

        #endregion
    }
}
