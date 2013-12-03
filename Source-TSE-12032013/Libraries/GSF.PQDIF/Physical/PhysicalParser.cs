//******************************************************************************************************
//  PhysicalParser.cs - Gbtc
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
//  05/03/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using GSF.IO;
using Ionic.Zlib;

namespace GSF.PQDIF.Physical
{
    #region [ Enumerations ]

    /// <summary>
    /// Enumeration which defines the types of compression used in PQDIF files.
    /// </summary>
    public enum CompressionStyle : uint
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// Compress the entire file after the container record.
        /// This compression style is deprecated and is currently
        /// not supported by this PQDIF library.
        /// </summary>
        TotalFile = 1,

        /// <summary>
        /// Compress the body of each record.
        /// </summary>
        RecordLevel = 2
    }

    /// <summary>
    /// Enumeration which defines the algorithms used to compress PQDIF files.
    /// </summary>
    public enum CompressionAlgorithm : uint
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// Zlib compression.
        /// http://www.zlib.net/
        /// </summary>
        Zlib = 1,

        /// <summary>
        /// PKZIP compression.
        /// This compression algorithm is deprecated and
        /// is currently not supported by this PQDIF library.
        /// </summary>
        PKZIP = 64
    }

    #endregion

    /// <summary>
    /// Represents a parser which parses the physical structure of a PQDIF file.
    /// </summary>
    public class PhysicalParser : IDisposable
    {
        #region [ Members ]

        // Fields
        private string m_fileName;
        private BinaryReader m_fileReader;
        private CompressionStyle m_compressionStyle;
        private CompressionAlgorithm m_compressionAlgorithm;

        private bool m_hasNextRecord;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="PhysicalParser"/> class.
        /// </summary>
        /// <param name="fileName">Name of the PQDIF file to be parsed.</param>
        public PhysicalParser(string fileName)
        {
            FileName = fileName;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file name of the PQDIF file to be parsed.
        /// </summary>
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException("value");

                m_fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the compression style used by the PQDIF file.
        /// </summary>
        public CompressionStyle CompressionStyle
        {
            get
            {
                return m_compressionStyle;
            }
            set
            {
                if (value == CompressionStyle.TotalFile)
                    throw new ArgumentException("Total file compression has been deprecated and is not supported", "value");

                m_compressionStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the compression algorithm used by the PQDIF file.
        /// </summary>
        public CompressionAlgorithm CompressionAlgorithm
        {
            get
            {
                return m_compressionAlgorithm;
            }
            set
            {
                if (value == CompressionAlgorithm.PKZIP)
                    throw new ArgumentException("PKZIP compression has been deprecated and is not supported", "value");

                m_compressionAlgorithm = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the PQDIF file.
        /// </summary>
        public void Open()
        {
            m_fileReader = new BinaryReader(File.OpenRead(m_fileName));
            m_hasNextRecord = true;
        }

        /// <summary>
        /// Returns true if this parser has not reached the end of the PQDIF file.
        /// </summary>
        /// <returns><c>false</c> if the end of the file has been reached; <c>true</c> otherwise</returns>
        public bool HasNextRecord()
        {
            return m_hasNextRecord;
        }

        /// <summary>
        /// Reads the next record from the PQDIF file.
        /// </summary>
        /// <returns>The next record to be parsed from the PQDIF file.</returns>
        public Record NextRecord()
        {
            RecordHeader header;
            RecordBody body;

            header = ReadRecordHeader();
            body = ReadRecordBody(header.BodySize);

            if ((object)body != null)
                body.Collection.TagOfElement = header.RecordTypeTag;

            m_hasNextRecord = header.NextRecordPosition != 0;
            m_fileReader.BaseStream.Seek(header.NextRecordPosition, SeekOrigin.Begin);

            return new Record(header, body);
        }

        /// <summary>
        /// Sets the parser back to the beginning of the file.
        /// </summary>
        public void Reset()
        {
            m_fileReader.BaseStream.Seek(0, SeekOrigin.Begin);
            m_hasNextRecord = true;
        }

        /// <summary>
        /// Closes the PQDIF file.
        /// </summary>
        public void Close()
        {
            m_fileReader.Close();
            m_hasNextRecord = false;
        }

        /// <summary>
        /// Releases all resources held by this parser.
        /// </summary>
        public void Dispose()
        {
            if ((object)m_fileReader != null)
            {
                m_fileReader.Dispose();
                m_fileReader = null;
            }

            m_hasNextRecord = false;
        }

        // Reads the header of a record from the PQDIF file.
        private RecordHeader ReadRecordHeader()
        {
            return new RecordHeader
                {
                    RecordSignature = new Guid(m_fileReader.ReadBytes(16)),
                    RecordTypeTag = new Guid(m_fileReader.ReadBytes(16)),
                    HeaderSize = m_fileReader.ReadInt32(),
                    BodySize = m_fileReader.ReadInt32(),
                    NextRecordPosition = m_fileReader.ReadInt32(),
                    Checksum = m_fileReader.ReadUInt32(),
                    Reserved = m_fileReader.ReadBytes(16)
                };
        }

        // Reads the body of a record from the PQDIF file.
        private RecordBody ReadRecordBody(int byteSize)
        {
            byte[] buffer;
            MemoryStream bufferStream;

            if (byteSize == 0)
                return null;

            buffer = m_fileReader.ReadBytes(byteSize);
            Decompress(ref buffer);

            // Using provided buffer as non-expandable memory stream for record body collection
            bufferStream = new MemoryStream(buffer);

            return new RecordBody
            {
                Collection = ReadCollection(new BinaryReader(bufferStream))
            };
        }

        // Reads an element from the PQDIF file.
        private Element ReadElement(BinaryReader recordBodyReader)
        {
            Element element;

            Guid tagOfElement = new Guid(recordBodyReader.ReadBytes(16));
            ElementType typeOfElement = (ElementType)recordBodyReader.ReadByte();
            PhysicalType typeOfValue = (PhysicalType)recordBodyReader.ReadByte();
            bool isEmbedded = recordBodyReader.ReadByte() != 0;

            // Read reserved byte
            recordBodyReader.ReadByte();

            long link;
            long returnLink;

            returnLink = recordBodyReader.BaseStream.Position + 8L;

            if (!isEmbedded || typeOfElement != ElementType.Scalar)
            {
                link = recordBodyReader.ReadInt32();
                recordBodyReader.BaseStream.Seek(link, SeekOrigin.Begin);
            }

            switch (typeOfElement)
            {
                case ElementType.Collection:
                    element = ReadCollection(recordBodyReader);
                    break;

                case ElementType.Scalar:
                    element = ReadScalar(recordBodyReader, typeOfValue);
                    break;

                case ElementType.Vector:
                    element = ReadVector(recordBodyReader, typeOfValue);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown element type: {0}", typeOfElement));
            }

            element.TagOfElement = tagOfElement;
            recordBodyReader.BaseStream.Seek(returnLink, SeekOrigin.Begin);

            return element;
        }

        // Reads a collection element from the PQDIF file.
        private CollectionElement ReadCollection(BinaryReader recordBodyReader)
        {
            CollectionElement collection = new CollectionElement
                {
                    Size = recordBodyReader.ReadInt32()
                };

            for (int i = 0; i < collection.Size; i++)
                collection.AddElement(ReadElement(recordBodyReader));

            return collection;
        }

        // Reads a vector element from the PQDIF file.
        private VectorElement ReadVector(BinaryReader recordBodyReader, PhysicalType typeOfValue)
        {
            VectorElement element = new VectorElement
                {
                    Size = recordBodyReader.ReadInt32(),
                    TypeOfValue = typeOfValue
                };

            byte[] values = recordBodyReader.ReadBytes(element.Size * typeOfValue.GetByteSize());

            element.SetValues(values, 0);

            return element;
        }

        // Reads a scalar element from the PQDIF file.
        private ScalarElement ReadScalar(BinaryReader recordBodyReader, PhysicalType typeOfValue)
        {
            ScalarElement element = new ScalarElement
                {
                    TypeOfValue = typeOfValue
                };

            byte[] value = recordBodyReader.ReadBytes(typeOfValue.GetByteSize());

            element.SetValue(value, 0);

            return element;
        }

        // Decompresses the given buffer based on the compression style and algorithm currently used by the parser.
        // The result is placed back in the buffer that was sent to this method.
        private void Decompress(ref byte[] buffer)
        {
            if (CompressionAlgorithm == CompressionAlgorithm.None || CompressionStyle == CompressionStyle.None)
                return;

            byte[] readBuffer = new byte[65536];

            using (BlockAllocatedMemoryStream readStream = new BlockAllocatedMemoryStream())
            {
                int readAmount;

                // Faster here to use provided buffer as non-expandable memory stream
                using (MemoryStream bufferStream = new MemoryStream(buffer))
                {
                    using (ZlibStream inflater = new ZlibStream(bufferStream, CompressionMode.Decompress))
                    {
                        do
                        {
                            readAmount = inflater.Read(readBuffer, 0, readBuffer.Length);
                            readStream.Write(readBuffer, 0, readAmount);
                        }
                        while (readAmount != 0);
                    }
                }

                buffer = readStream.ToArray();
            }
        }

        #endregion
    }
}
