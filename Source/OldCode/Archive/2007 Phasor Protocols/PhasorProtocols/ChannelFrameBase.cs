//*******************************************************************************************************
//  ChannelFrameBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.Measurements;

namespace PCS.PhasorProtocols
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
    [Serializable()]
    public abstract class ChannelFrameBase<T> : ChannelBase, IChannelFrame<T> where T : IChannelCell
    {
        #region [ Members ]

        // Fields
        private ushort m_idCode;                                            // Numeric identifier of this frame of data (e.g., ID code of the PDC)
        private IChannelCellCollection<T> m_cells;                          // Collection of "cells" within this frame of data (e.g., PMU's in the PDC frame)
        private Ticks m_timestamp;                                          // Time, represented as 100-nanosecond ticks, of this frame of data
        private int m_parsedBinaryLength;                                   // Binary length of frame as provided from parsed header
        private bool m_published;                                           // Determines if this frame of data has been published (IFrame.Published)
        private int m_publishedMeasurements;                                // Total measurements published by this frame          (IFrame.PublishedMeasurements)
        private Dictionary<MeasurementKey, IMeasurement> m_measurements;    // Collection of measurements published by this frame  (IFrame.Measurements)
        private IMeasurement m_lastSortedMeasurement;                       // Last measurement sorted into this frame             (IFrame.LastSortedMeasurement)

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
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public abstract FundamentalFrameType FrameType { get; }

        /// <summary>
        /// Gets the strongly-typed reference to the collection of cells for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual IChannelCellCollection<T> Cells
        {
            get
            {
                return m_cells;
            }
        }

        // Gets the simple object reference to the cell collection to satisfy IChannelFrame.Cells
        object IChannelFrame.Cells
        {
            get
            {
                return m_cells;
            }
        }

        /// <summary>
        /// Keyed measurements in this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Represents a dictionary of measurements, keyed by <see cref="MeasurementKey"/>.
        /// </remarks>
        public virtual IDictionary<MeasurementKey, IMeasurement> Measurements
        {
            get
            {
                if (m_measurements == null)
                    m_measurements = new Dictionary<MeasurementKey, IMeasurement>();

                return m_measurements;
            }
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual ushort IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets UNIX based time representation of the ticks of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual UnixTimeTag TimeTag
        {
            get
            {
                return new UnixTimeTag(Timestamp);
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        new public virtual IChannelFrameParsingState<T> State
        {
            get
            {
                return base.State as IChannelFrameParsingState<T>;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets ot sets reference to last <see cref="IMeasurement"/> that was sorted into this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// This value is used to help monitor slow moving measurements that are being sorted into the <see cref="ChannelFrameBase{T}"/>.
        /// </remarks>
        public virtual IMeasurement LastSortedMeasurement
        {
            get
            {
                return m_lastSortedMeasurement;
            }
            set
            {
                m_lastSortedMeasurement = value;
            }
        }

        /// <summary>
        /// Gets or sets published state of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual bool Published
        {
            get
            {
                return m_published;
            }
            set
            {
                m_published = value;
            }
        }

        /// <summary>
        /// Gets or sets total number of measurements that have been published for this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        public virtual int PublishedMeasurements
        {
            get
            {
                return m_publishedMeasurements;
            }
            set
            {
                m_publishedMeasurements = value;
            }
        }

        /// <summary>
        /// Determines if <see cref="ChannelFrameBase{T}"/> is only partially parsed.
        /// </summary>
        public virtual bool IsPartial
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        /// <remarks>
        /// This property is overriden so the length can be extended to include a checksum.
        /// </remarks>
        public override int BinaryLength
        {
            get
            {
                // We override normal binary length so we can extend length to include checksum.
                // Also, if frame length was parsed from stream header - we use that length
                // instead of the calculated length...
                if (m_parsedBinaryLength > 0)
                    return m_parsedBinaryLength;
                else
                    return 2 + base.BinaryLength;
            }
        }

        /// <summary>
        /// Gets the binary image of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// This property is overriden to include a checksum in the image.
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
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        /// <remarks>
        /// The length of the <see cref="ChannelFrameBase{T}"/> body image is the combined length of all the <see cref="Cells"/>.
        /// </remarks>
        protected override int BodyLength
        {
            get
            {
                return m_cells.BinaryLength;
            }
        }

        /// <summary>
        /// Gets the binary body image of this <see cref="ChannelFrameBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The body image of the <see cref="ChannelFrameBase{T}"/> is the combined images of all the <see cref="Cells"/>.
        /// </remarks>
        protected override byte[] BodyImage
        {
            get
            {
                return m_cells.BinaryImage;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ChannelFrameBase{T}"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Total Cells", Cells.Count.ToString());
                baseAttributes.Add("Fundamental Frame Type", (int)FrameType + ": " + FrameType);
                baseAttributes.Add("ID Code", IDCode.ToString());
                baseAttributes.Add("Is Partial Frame", IsPartial.ToString());
                baseAttributes.Add("Published", Published.ToString());
                baseAttributes.Add("Ticks", ((long)Timestamp).ToString());
                baseAttributes.Add("Timestamp", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overriden to validate the checksum in the <see cref="ChannelFrameBase{T}"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Invalid binary image detected - check sum did not match.</exception>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            // We override normal binary image parsing to validate frame checksum
            if (!ChecksumIsValid(binaryImage, startIndex))
                throw new InvalidOperationException("Invalid binary image detected - check sum of " + this.GetType().Name + " did not match");

            m_parsedBinaryLength = State.ParsedBinaryLength;

            return base.Initialize(binaryImage, startIndex, length);
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The body image of the <see cref="ChannelFrameBase{T}"/> is parsed to create a collection of <see cref="Cells"/>.
        /// </remarks>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            IChannelFrameParsingState<T> state = State;
            T cell;
            int parsedLength, index = startIndex;

            // Parse all frame cells
            for (int x = 0; x < state.CellCount; x++)
            {
                cell = state.CreateNewCell(this, state, x, binaryImage, index, out parsedLength);
                m_cells.Add(cell);
                index += parsedLength;
            }

            return (index - startIndex);
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
            int sumLength = BinaryLength - 2;
            return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
        }

        /// <summary>
        /// Appends checksum onto <paramref name="buffer"/> starting at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="buffer">Buffer image on which to append checksum.</param>
        /// <param name="startIndex">Index into <paramref name="buffer"/> where checksum should be appended.</param>
        /// <remarks>
        /// Default implementation encodes checksum in big-endian order and expects buffer size large enough to accomodate
        /// 2-byte checksum representation. Override method if protocol expectations are different.
        /// </remarks>
        protected virtual void AppendChecksum(byte[] buffer, int startIndex)
        {
            EndianOrder.BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
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
        /// using PCS.IO.Checksums;
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
            IFrame other = obj as IFrame;

            if (other != null)
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
            return (CompareTo(other) == 0);
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
            IFrame other = obj as IFrame;

            if (other != null)
                return Equals(other);

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return m_timestamp.GetHashCode();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
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