//******************************************************************************************************
//  PhysicalWriter.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/15/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using GSF.IO;
using GSF.IO.Checksums;
using Ionic.Zlib;

namespace GSF.PQDIF.Physical
{
    /// <summary>
    /// Represents a writer used to write the physical
    /// structure of a PQDIF file to a byte stream.
    /// </summary>
    public class PhysicalWriter : IDisposable
    {
        #region [ Members ]

        // Fields
        private Stream m_stream;
        private BinaryWriter m_writer;

        private CompressionStyle m_compressionStyle;
        private CompressionAlgorithm m_compressionAlgorithm;

        private bool m_leaveOpen;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="PhysicalWriter"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file where the PQDIF data is to be written.</param>
        public PhysicalWriter(string filePath)
            : this(File.Create(filePath))
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PhysicalWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream to write the PQDIF data to.</param>
        /// <param name="leaveOpen">Indicates whether to leave the stream open when disposing of the writer.</param>
        public PhysicalWriter(Stream stream, bool leaveOpen = false)
        {
            if (!stream.CanWrite)
                throw new InvalidOperationException("Cannot write to the given stream.");

            m_stream = stream;
            m_writer = new BinaryWriter(m_stream);
            m_leaveOpen = leaveOpen;
        }

        #endregion

        #region [ Properties ]

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
        /// Writes the given record to the PQDIF file.
        /// </summary>
        /// <param name="record">The record to be written to the file.</param>
        /// <param name="lastRecord">Indicates whether this record is the last record in the file.</param>
        public void WriteRecord(Record record, bool lastRecord = false)
        {
            byte[] bodyImage;
            Adler32 checksum;

            if (m_disposed)
                throw new ObjectDisposedException(GetType().Name);

            using (BlockAllocatedMemoryStream bodyStream = new BlockAllocatedMemoryStream())
            using (BinaryWriter bodyWriter = new BinaryWriter(bodyStream))
            {
                // Write the record body to the memory stream
                if ((object)record.Body != null)
                    WriteCollection(bodyWriter, record.Body.Collection);

                // Read and compress the body to a byte array
                bodyImage = bodyStream.ToArray();

                if (m_compressionAlgorithm == CompressionAlgorithm.Zlib && m_compressionStyle == CompressionStyle.RecordLevel)
                    bodyImage = ZlibStream.CompressBuffer(bodyImage);

                // Create the checksum after compression
                checksum = new Adler32();
                checksum.Update(bodyImage);

                // Write the record body to the memory stream
                if ((object)record.Body != null)
                    record.Body.Checksum = checksum.Value;
            }

            // Fix the pointer to the next
            // record before writing this record
            if (m_stream.CanSeek && m_stream.Length > 0)
            {
                m_writer.Write((int)m_stream.Length);
                m_stream.Seek(0L, SeekOrigin.End);
            }

            // Make sure the header points to the correct location based on the size of the body
            record.Header.HeaderSize = 64;
            record.Header.BodySize = bodyImage.Length;
            record.Header.NextRecordPosition = (int)m_stream.Length + record.Header.HeaderSize + record.Header.BodySize;
            record.Header.Checksum = checksum.Value;

            // Write up to the next record position
            m_writer.Write(record.Header.RecordSignature.ToByteArray());
            m_writer.Write(record.Header.RecordTypeTag.ToByteArray());
            m_writer.Write(record.Header.HeaderSize);
            m_writer.Write(record.Header.BodySize);

            // The PQDIF standard defines the NextRecordPosition to be 0 for the last record in the file
            // We treat seekable streams differently because we can go back and fix the pointers later
            if (m_stream.CanSeek || lastRecord)
                m_writer.Write(0);
            else
                m_writer.Write(record.Header.NextRecordPosition);

            // Write the rest of the header as well as the body
            m_writer.Write(record.Header.Checksum);
            m_writer.Write(record.Header.Reserved);
            m_writer.Write(bodyImage);

            // If the stream is seekable, seek to the next record
            // position so we can fix the pointer if we end up
            // writing another record to the file
            if (m_stream.CanSeek)
                m_stream.Seek(-(24 + record.Header.BodySize), SeekOrigin.Current);

            // Dispose of the writer if this is the last record
            if (!m_stream.CanSeek && lastRecord)
                Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!m_disposed)
            {
                try
                {
                    if (!m_leaveOpen)
                    {
                        m_writer.Dispose();
                        m_stream.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;
                }
            }
        }

        private void WriteCollection(BinaryWriter writer, CollectionElement collection)
        {
            int linkPosition = (int)writer.BaseStream.Position + 4 + (28 * collection.Size);

            writer.Write(collection.Size);

            foreach (Element element in collection.Elements)
            {
                bool isEmbedded = IsEmbedded(element);

                writer.Write(element.TagOfElement.ToByteArray());
                writer.Write((byte)element.TypeOfElement);
                writer.Write((byte)element.TypeOfValue);
                writer.Write(isEmbedded ? (byte)1 : (byte)0);
                writer.Write((byte)0);

                if (!isEmbedded)
                {
                    int padSize = GetPaddedByteSize(element);
                    writer.Write(linkPosition);
                    writer.Write(padSize);
                    linkPosition += padSize;
                }
                else
                {
                    WriteScalar(writer, element as ScalarElement);

                    for (int i = element.TypeOfValue.GetByteSize(); i < 8; i++)
                        writer.Write((byte)0);
                }
            }

            foreach (Element element in collection.Elements)
            {
                if (IsEmbedded(element))
                    continue;

                switch (element.TypeOfElement)
                {
                    case ElementType.Collection:
                        WriteCollection(writer, element as CollectionElement);
                        break;

                    case ElementType.Scalar:
                        WriteScalar(writer, element as ScalarElement);
                        break;

                    case ElementType.Vector:
                        WriteVector(writer, element as VectorElement);
                        break;
                }

                int byteSize = GetByteSize(element);
                int padSize = GetPaddedByteSize(element);

                for (int i = 0; i < padSize - byteSize; i++)
                    writer.Write((byte)0);
            }
        }

        private void WriteVector(BinaryWriter writer, VectorElement vector)
        {
            writer.Write(vector.Size);
            writer.Write(vector.GetValues());
        }

        private void WriteScalar(BinaryWriter writer, ScalarElement scalar)
        {
            writer.Write(scalar.GetValue());
        }

        private int GetPaddedByteSize(Element element)
        {
            int byteSize = GetByteSize(element);
            int padSize = byteSize + 3;
            return (padSize / 4) * 4;
        }

        private int GetByteSize(Element element)
        {
            switch (element.TypeOfElement)
            {
                case ElementType.Collection:
                    return GetByteSize(element as CollectionElement);

                case ElementType.Vector:
                    return GetByteSize(element as VectorElement);

                case ElementType.Scalar:
                    return GetByteSize(element as ScalarElement);

                default:
                    return 0;
            }
        }

        private int GetByteSize(CollectionElement collection)
        {
            int sum;

            if ((object)collection == null)
                return 0;

            sum = collection.Elements
                .Where(element => !IsEmbedded(element))
                .Sum(GetPaddedByteSize);

            return 4 + (28 * collection.Size) + sum;
        }

        private int GetByteSize(VectorElement vector)
        {
            if ((object)vector == null)
                return 0;

            return 4 + (vector.Size * vector.TypeOfValue.GetByteSize());
        }

        private int GetByteSize(ScalarElement scalar)
        {
            if ((object)scalar == null)
                return 0;

            return scalar.TypeOfValue.GetByteSize();
        }

        private bool IsEmbedded(Element element)
        {
            return element.TypeOfElement == ElementType.Scalar && element.TypeOfValue.GetByteSize() < 8;
        }

        #endregion
    }
}
