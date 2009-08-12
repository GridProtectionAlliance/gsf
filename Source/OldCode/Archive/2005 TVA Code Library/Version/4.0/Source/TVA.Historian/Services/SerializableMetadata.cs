//*******************************************************************************************************
//  SerializableMetadata.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/07/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVA.Historian.Files;

namespace TVA.Historian.Services
{
    /// <summary>
    /// Represents a container for <see cref="SerializableMetadataRecord"/>s that can be serialized using <see cref="XmlSerializer"/> or <see cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>.
    /// </summary>
    /// <seealso cref="SerializableMetadataRecord"/>
    /// <seealso cref="XmlSerializer"/>
    /// <seealso cref="DataContractSerializer"/>
    /// <seealso cref="System.Runtime.Serialization.Json.DataContractJsonSerializer"/>
    [XmlRoot("Metadata"), DataContract(Name = "Metadata")]
    public class SerializableMetadata
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMetadata"/> class.
        /// </summary>
        public SerializableMetadata()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableMetadata"/> class.
        /// </summary>
        /// <param name="metadataFile"><see cref="MetadataFile"/> object from which <see cref="SerializableMetadata"/> is to be initialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metadataFile"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="metadataFile"/> is closed.</exception>
        public SerializableMetadata(MetadataFile metadataFile)
            : this()
        {
            if (metadataFile == null)
                throw new ArgumentNullException("metadataFile");

            if (!metadataFile.IsOpen)
                throw new ArgumentException("metadataFile is closed.");

            // Process all records in the metadata file.
            List<SerializableMetadataRecord> serializedMetadataRecords = new List<SerializableMetadataRecord>();
            foreach (MetadataRecord metadataRecord in metadataFile.Read())
            {
                serializedMetadataRecords.Add(new SerializableMetadataRecord(metadataRecord));
            }
            MetadataRecords = serializedMetadataRecords.ToArray();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="SerializableMetadataRecord"/>s contained in the <see cref="SerializableMetadata"/>.
        /// </summary>
        [XmlArray(), DataMember()]
        public SerializableMetadataRecord[] MetadataRecords { get; set; }

        #endregion
    }
}
