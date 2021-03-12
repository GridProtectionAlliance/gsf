//******************************************************************************************************
//  ChannelFrameBase.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.Parsing;
using GSF.TimeSeries;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any frame of data that can be sent or received.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This phasor protocol implementation defines a "frame" as a collection of cells (logical units of data).
    /// For example, a <see cref="DataCellBase"/> could be defined as a PMU within a frame of data, a <see cref="DataFrameBase"/>
    /// (derived from <see cref="ChannelFrameBase{T}"/>), that contains multiple PMU's coming from a PDC.
    /// </para>
    /// <para>
    /// This class implements the <see cref="IFrame"/> interface so it can be cooperatively integrated into measurement concentration.
    /// For more information see the <see cref="ConcentratorBase"/> class.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type that the <see cref="ChannelFrameBase{T}"/> contains.</typeparam>
    [Serializable]
    public abstract class ChannelFrameBase<T> : ChannelBase, IChannelFrame<T> where T : IChannelCell
    {
        #region [ Members ]

        // Fields
        private ushort m_idCode;                            // Numeric identifier of this frame of data (e.g., ID code of the PDC)
        private readonly IChannelCellCollection<T> m_cells; // Collection of "cells" within this frame of data (e.g., PMU's in the PDC frame)
        private Ticks m_timestamp;                          // Time, represented as 100-nanosecond ticks, of this frame of data
        private readonly ShortTime m_lifespan;              // Elapsed time since creation of this frame of data
        private SourceChannel m_source;                     // Defines source channel (e.g., data or command) for channel frame
        private bool m_trustHeaderLength;                   // Determines if parsed header lengths should be trusted (normally true)
        private bool m_validateCheckSum;                    // Allows bypass of check-sum validation if devices are behaving badly
        private bool m_published;                           // Determines if this frame of data has been published (IFrame.Published)
        private int m_sortedMeasurements;                   // Total measurements published into this frame        (IFrame.SortedMeasurements)
        private IMeasurement m_lastSortedMeasurement;       // Last measurement sorted into this frame             (IFrame.LastSortedMeasurement)

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelFrameBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ChannelFrameBase{T}"/>.</param>
        /// <param name="cells">The reference to the collection of cells for this <see cref="ChannelFrameBase{T}"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ChannelFrameBase{T}"/>.</param>
        protected ChannelFrameBase(ushort idCode, IChannelCellCollection<T> cells, Ticks timestamp)
        {
            m_idCode = idCode;
            m_cells = cells;
            m_timestamp = timestamp;
            m_lifespan = ShortTime.Now;
            m_trustHeaderLength = true;
            m_validateCheckSum = true;
            Measurements = new ConcurrentDictionary<MeasurementKey, IMeasurement>();
            m_sortedMeasurements = -1;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelFrameBase{T}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelFrameBase(SerializationInfo info, StreamingContext context)
        {
            // Deserialize key frame elements...
            m_idCode = info.GetUInt16("idCode");
            m_cells = (IChannelCellCollection<T>)info.GetValue("cells", typeof(IChannelCellCollection<T>));
            m_timestamp = info.GetInt64("timestamp");
            m_lifespan = ShortTime.Now;
            m_trustHeaderLength = true;
            m_validateCheckSum = true;
            Measurements = new ConcurrentDictionary<MeasurementKey, IMeasurement>();
            m_sortedMeasurements = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public abstract FundamentalFrameType FrameType { get; }

        /// <summary>
        /// Gets or sets the data source identifier for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual SourceChannel Source
        {
            get => m_source;
            set => m_source = value;
        }

        /// <summary>
        /// Gets flag that determines if frame image can be queued for publication or should be processed immediately.
        /// </summary>
        /// <remarks>
        /// Some frames, e.g., a configuration or key frame, may be critical to processing of other frames. In this
        /// case, these types of frames should be published immediately so that subsequent frame parsing can have
        /// access to needed critical information. All other frames are assumed to be queued by default.
        /// </remarks>
        public virtual bool AllowQueuedPublication => true;

        /// <summary>
        /// Gets the strongly-typed reference to the collection of cells for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual IChannelCellCollection<T> Cells => m_cells;

        // Gets the simple object reference to the cell collection to satisfy IChannelFrame.Cells
        object IChannelFrame.Cells => m_cells;

        /// <summary>
        /// Keyed measurements in this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Represents a dictionary of measurements, keyed by <see cref="MeasurementKey"/>.
        /// </remarks>
        public ConcurrentDictionary<MeasurementKey, IMeasurement> Measurements { get; }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual ushort IDCode
        {
            get => m_idCode;
            set => m_idCode = value;
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
        {
            get => m_timestamp;
            set => m_timestamp = value;
        }

        /// <summary>
        /// Gets the life-span of this <see cref="ChannelFrameBase{T}"/> since its creation.
        /// </summary>
        public virtual ShortTime Lifespan => m_lifespan;

        /// <summary>
        /// Gets timestamp, in ticks, of when this <see cref="ChannelFrameBase{T}"/> was created.
        /// </summary>
        public virtual Ticks CreatedTimestamp => m_lifespan.UtcTime.Ticks;

        /// <summary>
        /// Gets UNIX based time representation of the ticks of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual UnixTimeTag TimeTag => new UnixTimeTag(Timestamp);

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public new virtual IChannelFrameParsingState<T> State
        {
            get => base.State as IChannelFrameParsingState<T>;
            set
            {
                base.State = value;

                if (value is null)
                    return;

                TrustHeaderLength = value.TrustHeaderLength;
                ValidateCheckSum = value.ValidateCheckSum;
            }
        }

        /// <summary>
        /// Gets or sets reference to last <see cref="IMeasurement"/> that was sorted into this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// This value is used to help monitor slow moving measurements that are being sorted into the <see cref="ChannelFrameBase{T}"/>.
        /// </remarks>
        public IMeasurement LastSortedMeasurement
        {
            get => m_lastSortedMeasurement;
            set => m_lastSortedMeasurement = value;
        }

        /// <summary>
        /// Gets or sets published state of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public bool Published
        {
            get => m_published;
            set => m_published = value;
        }

        /// <summary>
        /// Gets or sets total number of measurements that have been sorted into this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public int SortedMeasurements
        {
            get => m_sortedMeasurements == -1 ? Measurements.Count : m_sortedMeasurements;
            set => m_sortedMeasurements = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        /// <remarks>
        /// This property is overridden so the length can be extended to include a checksum.
        /// </remarks>
        public override int BinaryLength
        {
            get
            {
                // We override normal binary length so we can extend length to include checksum.
                // Also, if frame length was parsed from stream header - we use that length
                // instead of the calculated length...
                if (ParsedBinaryLength > 0)
                    return ParsedBinaryLength;

                return 2 + base.BinaryLength;
            }
        }

        /// <summary>
        /// Gets the binary image of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// This property is overridden to include a checksum in the image.
        /// </remarks>
        public override byte[] BinaryImage
        {
            get
            {
                // We override normal binary image to include checksum
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                // Copy in base image
                base.BinaryImage.CopyImage(buffer, ref index, base.BinaryLength);

                // Add check sum
                AppendChecksum(buffer, index);

                return buffer;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if header lengths should be trusted over parsed byte count.
        /// </summary>
        public virtual bool TrustHeaderLength
        {
            get => m_trustHeaderLength;
            set => m_trustHeaderLength = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if frame's check-sum should be validated - this should normally be left as <c>true</c>.
        /// </summary>
        public virtual bool ValidateCheckSum
        {
            get => m_validateCheckSum;
            set => m_validateCheckSum = value;
        }

        /// <summary>
        /// Gets calculated checksum for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public ushort Checksum
        {
            get
            {
                byte[] buffer = BinaryImage;
                return BigEndian.ToUInt16(buffer, buffer.Length - 2);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        /// <remarks>
        /// The length of the <see cref="ChannelFrameBase{T}"/> body image is the combined length of all the <see cref="Cells"/>.
        /// </remarks>
        protected override int BodyLength => m_cells.BinaryLength;

        /// <summary>
        /// Gets the binary body image of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The body image of the <see cref="ChannelFrameBase{T}"/> is the combined images of all the <see cref="Cells"/>.
        /// </remarks>
        protected override byte[] BodyImage => m_cells.BinaryImage();

        /// <summary>
        /// Gets the parsed binary length derived from the parsing state, if any.
        /// </summary>
        protected int ParsedBinaryLength { get; private set; }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ChannelFrameBase{T}"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Total Cells", Cells is null ? "null" : Cells.Count.ToString());
                baseAttributes.Add("Fundamental Frame Type", $"{(int)FrameType}: {FrameType}");
                baseAttributes.Add("ID Code", IDCode.ToString());
                baseAttributes.Add("Published", Published.ToString());
                baseAttributes.Add("Ticks", ((long)Timestamp).ToString());
                baseAttributes.Add("Timestamp", ((DateTime)Timestamp).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                baseAttributes.Add("Trusting Header Length", TrustHeaderLength.ToString());
                baseAttributes.Add("Validating Check-sum", ValidateCheckSum.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden to validate the checksum in the <see cref="ChannelFrameBase{T}"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Invalid binary image detected - check sum did not match.</exception>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            // We use data length parsed from data stream if available - in many cases we'll have to as we won't enough
            // information about cell contents at this early parsing stage
            ParsedBinaryLength = State.ParsedBinaryLength;

            // Normal binary image parsing is overridden for a frame so that checksum can be validated           
            if (!ChecksumIsValid(buffer, startIndex))
            {
                // If user selects incorrect protocol, image may be very large - so we don't log the image 
                //  byte[] binaryImageErr = buffer.BlockCopy(startIndex, length);
                //  + BitConverter.ToString(binaryImageErr)
                throw new CrcException($"Invalid binary image detected - check sum of {GetType().Name} did not match");
            }

            if (TrustHeaderLength)
            {
                // Normally one would expect image size returned should match parsed image size - but it's possible that these may
                // differ, so we assume the parsed length header length is better of the two values. This can accommodate the case
                // where some devices add padding to the end of the frame.
                base.ParseBinaryImage(buffer, startIndex, length);

                return State.ParsedBinaryLength;
            }

            // Include 2 bytes for CRC in returned parsed length
            return base.ParseBinaryImage(buffer, startIndex, length) + 2;
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The body image of the <see cref="ChannelFrameBase{T}"/> is parsed to create a collection of <see cref="Cells"/>.
        /// </remarks>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            IChannelFrameParsingState<T> state = State;
            int index = startIndex;

            // Parse all frame cells
            for (int x = 0; x < state.CellCount; x++)
            {
                T cell = state.CreateNewCell(this, state, x, buffer, index, out int parsedLength);
                m_cells.Add(cell);
                index += parsedLength;
            }

            return index - startIndex;
        }

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// Default implementation expects 2-byte big-endian ordered checksum. Override method if protocol checksum
        /// implementation is different.
        /// </remarks>
        protected virtual bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            if (ValidateCheckSum)
            {
                int sumLength = BinaryLength - 2;
                return BigEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
            }

            return true;
        }

        /// <summary>
        /// Appends checksum onto <paramref name="buffer"/> starting at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="buffer">Buffer image on which to append checksum.</param>
        /// <param name="startIndex">Index into <paramref name="buffer"/> where checksum should be appended.</param>
        /// <remarks>
        /// Default implementation encodes checksum in big-endian order and expects buffer size large enough to accommodate
        /// 2-byte checksum representation. Override method if protocol expectations are different.
        /// </remarks>
        protected virtual void AppendChecksum(byte[] buffer, int startIndex)
        {
            BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
        }

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// Override with needed checksum calculation for particular protocol.
        /// <example>
        /// This example provides a CRC-CCITT checksum:
        /// <code>
        /// using GSF.IO.Checksums;
        /// 
        /// protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        /// {
        ///     // Return calculated CRC-CCITT over given buffer...
        ///     return buffer.CrcCCITTChecksum(offset, length);
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        protected abstract ushort CalculateChecksum(byte[] buffer, int offset, int length);

        /// <summary>
        /// Compares the <see cref="ChannelFrameBase{T}"/> with an <see cref="IFrame"/>.
        /// </summary>
        /// <param name="other">The <see cref="IFrame"/> to compare with the current <see cref="ChannelFrameBase{T}"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>This frame implementation compares itself by timestamp.</remarks>
        public virtual int CompareTo(IFrame other)
        {
            // We sort frames by timestamp
            return m_timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        /// Compares the <see cref="ChannelFrameBase{T}"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ChannelFrameBase{T}"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><see cref="Object"/> is not an <see cref="IFrame"/>.</exception>
        /// <remarks>This frame implementation compares itself by timestamp.</remarks>
        public virtual int CompareTo(object obj)
        {
            if (obj is IFrame other)
                return CompareTo(other);

            throw new ArgumentException("Frame can only be compared with other IFrames...");
        }

        /// <summary>
        /// Determines whether the specified <see cref="IFrame"/> is equal to the current <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <param name="other">The <see cref="IFrame"/> to compare with the current <see cref="ChannelFrameBase{T}"/>.</param>
        /// <returns>
        /// true if the specified <see cref="IFrame"/> is equal to the current <see cref="ChannelFrameBase{T}"/>;
        /// otherwise, false.
        /// </returns>
        /// <remarks>This frame implementation compares itself by timestamp.</remarks>
        public virtual bool Equals(IFrame other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ChannelFrameBase{T}"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="ChannelFrameBase{T}"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><see cref="Object"/> is not an <see cref="IFrame"/>.</exception>
        public override bool Equals(object obj)
        {
            if (obj is IFrame other)
                return Equals(other);

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => 
            m_timestamp.GetHashCode();

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Add key frame elements for serialization...
            info.AddValue("idCode", m_idCode);
            info.AddValue("cells", m_cells, typeof(IChannelCellCollection<T>));
            info.AddValue("timestamp", (long)m_timestamp);
        }

        #endregion
    }
}