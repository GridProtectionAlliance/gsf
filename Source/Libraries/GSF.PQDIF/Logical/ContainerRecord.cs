//******************************************************************************************************
//  ContainerRecord.cs - Gbtc
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
//  05/03/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Represents the container record in a PQDIF file. There can be only
    /// one container record in a PQDIF file, and it is the first physical
    /// <see cref="Record"/>.
    /// </summary>
    public class ContainerRecord
    {
        #region [ Members ]

        // Fields
        private readonly Record m_physicalRecord;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ContainerRecord"/> class.
        /// </summary>
        /// <param name="physicalRecord">The physical structure of the container record.</param>
        private ContainerRecord(Record physicalRecord)
        {
            m_physicalRecord = physicalRecord;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the physical structure of the container record.
        /// </summary>
        public Record PhysicalRecord
        {
            get
            {
                return m_physicalRecord;
            }
        }

        /// <summary>
        /// Gets the major version number of the PQDIF file writer.
        /// </summary>
        public uint WriterMajorVersion
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetVectorByTag(VersionInfoTag)
                    .GetUInt4(0);
            }
            set
            {
                VectorElement versionInfoElement = m_physicalRecord.Body.Collection.GetVectorByTag(VersionInfoTag);

                if ((object)versionInfoElement == null)
                {
                    versionInfoElement = new VectorElement()
                    {
                        Size = 4,
                        TagOfElement = VersionInfoTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(versionInfoElement);
                }

                versionInfoElement.SetUInt4(0, value);
            }
        }

        /// <summary>
        /// Gets the minor version number of the PQDIF file writer.
        /// </summary>
        public uint WriterMinorVersion
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetVectorByTag(VersionInfoTag)
                    .GetUInt4(1);
            }
            set
            {
                VectorElement versionInfoElement = m_physicalRecord.Body.Collection.GetVectorByTag(VersionInfoTag);

                if ((object)versionInfoElement == null)
                {
                    versionInfoElement = new VectorElement()
                    {
                        Size = 4,
                        TagOfElement = VersionInfoTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(versionInfoElement);
                }

                versionInfoElement.SetUInt4(1, value);
            }
        }

        /// <summary>
        /// Gets the compatible major version that the file can be read as.
        /// </summary>
        public uint CompatibleMajorVersion
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetVectorByTag(VersionInfoTag)
                    .GetUInt4(2);
            }
            set
            {
                VectorElement versionInfoElement = m_physicalRecord.Body.Collection.GetVectorByTag(VersionInfoTag);

                if ((object)versionInfoElement == null)
                {
                    versionInfoElement = new VectorElement()
                    {
                        Size = 4,
                        TagOfElement = VersionInfoTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(versionInfoElement);
                }

                versionInfoElement.SetUInt4(2, value);
            }
        }

        /// <summary>
        /// Gets the compatible minor version that the file can be read as.
        /// </summary>
        public uint CompatibleMinorVersion
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetVectorByTag(VersionInfoTag)
                    .GetUInt4(3);
            }
            set
            {
                VectorElement versionInfoElement = m_physicalRecord.Body.Collection.GetVectorByTag(VersionInfoTag);

                if ((object)versionInfoElement == null)
                {
                    versionInfoElement = new VectorElement()
                    {
                        Size = 4,
                        TagOfElement = VersionInfoTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(versionInfoElement);
                }

                versionInfoElement.SetUInt4(3, value);
            }
        }

        /// <summary>
        /// Gets the name of the file at the time the file was written.
        /// </summary>
        public string FileName
        {
            get
            {
                VectorElement fileNameElement = m_physicalRecord.Body.Collection.GetVectorByTag(FileNameTag);
                return Encoding.ASCII.GetString(fileNameElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                VectorElement fileNameElement = m_physicalRecord.Body.Collection.GetVectorByTag(FileNameTag);

                if ((object)fileNameElement == null)
                {
                    fileNameElement = new VectorElement()
                    {
                        TagOfElement = FileNameTag,
                        TypeOfValue = PhysicalType.Char1
                    };

                    m_physicalRecord.Body.Collection.AddElement(fileNameElement);
                }

                fileNameElement.Size = bytes.Length;
                fileNameElement.SetValues(bytes, 0);
            }
        }

        /// <summary>
        /// Gets the date and time of file creation.
        /// </summary>
        public DateTime Creation
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetScalarByTag(CreationTag)
                    .GetTimestamp();
            }
            set
            {
                ScalarElement creationElement = m_physicalRecord.Body.Collection.GetScalarByTag(CreationTag);

                if ((object)creationElement == null)
                {
                    creationElement = new ScalarElement()
                    {
                        TagOfElement = CreationTag,
                        TypeOfValue = PhysicalType.Timestamp
                    };

                    m_physicalRecord.Body.Collection.AddElement(creationElement);
                }

                creationElement.SetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets the notes stored in the file.
        /// </summary>
        public string Notes
        {
            get
            {
                VectorElement notesElement = m_physicalRecord.Body.Collection.GetVectorByTag(NotesTag);

                if ((object)notesElement == null)
                    return null;

                return Encoding.ASCII.GetString(notesElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                VectorElement notesElement = m_physicalRecord.Body.Collection.GetVectorByTag(NotesTag);

                if ((object)notesElement == null)
                {
                    notesElement = new VectorElement()
                    {
                        TagOfElement = NotesTag,
                        TypeOfValue = PhysicalType.Char1
                    };

                    m_physicalRecord.Body.Collection.AddElement(notesElement);
                }

                notesElement.Size = bytes.Length;
                notesElement.SetValues(bytes, 0);
            }
        }

        /// <summary>
        /// Gets the style of compression used to compress the PQDIF file.
        /// </summary>
        public CompressionStyle CompressionStyle
        {
            get
            {
                CollectionElement collection = m_physicalRecord.Body.Collection;
                ScalarElement compressionStyleElement = collection.GetScalarByTag(CompressionStyleTag);
                uint compressionStyleID = (uint)CompressionStyle.None;

                if ((object)compressionStyleElement != null)
                    compressionStyleID = compressionStyleElement.GetUInt4();

                return (CompressionStyle)compressionStyleID;
            }
            set
            {
                CollectionElement collection = m_physicalRecord.Body.Collection;
                ScalarElement compressionStyleElement = collection.GetScalarByTag(CompressionStyleTag);

                if ((object)compressionStyleElement == null)
                {
                    compressionStyleElement = new ScalarElement()
                    {
                        TagOfElement = CompressionStyleTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(compressionStyleElement);
                }

                compressionStyleElement.SetUInt4((uint)value);
            }
        }

        /// <summary>
        /// Gets the compression algorithm used to compress the PQDIF file.
        /// </summary>
        public CompressionAlgorithm CompressionAlgorithm
        {
            get
            {
                CollectionElement collection = m_physicalRecord.Body.Collection;
                ScalarElement compressionAlgorithmElement = collection.GetScalarByTag(CompressionAlgorithmTag);
                uint compressionAlgorithmID = (uint)CompressionAlgorithm.None;

                if ((object)compressionAlgorithmElement != null)
                    compressionAlgorithmID = compressionAlgorithmElement.GetUInt4();

                return (CompressionAlgorithm)compressionAlgorithmID;
            }
            set
            {
                CollectionElement collection = m_physicalRecord.Body.Collection;
                ScalarElement compressionAlgorithmElement = collection.GetScalarByTag(CompressionAlgorithmTag);

                if ((object)compressionAlgorithmElement == null)
                {
                    compressionAlgorithmElement = new ScalarElement()
                    {
                        TagOfElement = CompressionAlgorithmTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(compressionAlgorithmElement);
                }

                compressionAlgorithmElement.SetUInt4((uint)value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Removes the element identified by the given tag from the record.
        /// </summary>
        /// <param name="tag">The tag of the element to be removed.</param>
        public void RemoveElement(Guid tag)
        {
            m_physicalRecord.Body.Collection.RemoveElementsByTag(tag);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        
        /// <summary>
        /// Tag that identifies the version info.
        /// </summary>
        public static readonly Guid VersionInfoTag = new Guid("89738607-f1c3-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the file name.
        /// </summary>
        public static readonly Guid FileNameTag = new Guid("89738608-f1c3-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the date and time of creation.
        /// </summary>
        public static readonly Guid CreationTag = new Guid("89738609-f1c3-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the notes stored in the PQDIF file.
        /// </summary>
        public static readonly Guid NotesTag = new Guid("89738617-f1c3-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the compression style of the PQDIF file.
        /// </summary>
        public static readonly Guid CompressionStyleTag = new Guid("8973861b-f1c3-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the compression algorithm used when writing the PQDIF file.
        /// </summary>
        public static readonly Guid CompressionAlgorithmTag = new Guid("8973861c-f1c3-11cf-9d89-0080c72e70a3");

        // Static Methods

        /// <summary>
        /// Creates a new container record from the given physical record
        /// if the physical record is of type container. Returns null if
        /// it is not.
        /// </summary>
        /// <param name="physicalRecord">The physical record used to create the container record.</param>
        /// <returns>The new container record, or null if the physical record does not define a container record.</returns>
        public static ContainerRecord CreateContainerRecord(Record physicalRecord)
        {
            bool isValidPhysicalRecord = physicalRecord.Header.TypeOfRecord == RecordType.Container;
            return (isValidPhysicalRecord) ? new ContainerRecord(physicalRecord) : null;
        }

        #endregion
    }
}
