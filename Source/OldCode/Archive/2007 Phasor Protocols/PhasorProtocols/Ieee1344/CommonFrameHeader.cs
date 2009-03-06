//*******************************************************************************************************
//  CommonFrameHeader.cs
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
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.IO.Checksums;
using PCS.Measurements;
using PCS.Parsing;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the common header for all IEEE 1344 frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, IChannelFrame
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 6;

        // Fields
        private FrameImageCollector m_frameImages;
        private short m_sampleCount;
        private short m_statusFlags;
        private Ticks m_timestamp;
        private IChannelParsingState m_state;
        private Dictionary<string, string> m_attributes;
        private object m_tag;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="typeID">The IEEE 1344 specific frame type of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(FrameType typeID, Ticks timestamp)
        {
            m_timestamp = timestamp;
            TypeID = typeID;
            FrameCount = 1;
            IsFirstFrame = true;
            IsLastFrame = true;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="configurationFrame">IEEE 1344 <see cref="ConfigurationFrame"/> if already parsed.</param>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
        {
            uint secondOfCentury;

            secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex);
            m_sampleCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4);

            // We go ahead and pre-grab cell's status flags so we can determine framelength - we
            // leave startindex at 6 so that cell will be able to parse flags as needed - note
            // this increases needed common frame header size by 2 (i.e., BinaryLength + 2)
            m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + FixedLength);

            if (TypeID == Ieee1344.FrameType.DataFrame && configurationFrame != null)
                // Data frames have subsecond time information
                m_timestamp = (new NtpTimeTag((double)secondOfCentury + (double)SampleCount / System.Math.Floor((double)Common.MaximumSampleCount / (double)configurationFrame.Period) / (double)configurationFrame.FrameRate)).ToDateTime().Ticks;
            else
                // For other frames, the best timestamp you can get is down to the whole second
                m_timestamp = (new NtpTimeTag((double)secondOfCentury)).ToDateTime().Ticks;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // It is not exepcted that common frame header will be serialized or deserialized since it's only a partial frame,
            // but its implementation is required via IChannelFrame
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the timestamp of this frame in NTP format.
        /// </summary>
        public NtpTimeTag TimeTag
        {
            get
            {
                return new NtpTimeTag(Timestamp);
            }
        }

        /// <summary>
        /// Gets or sets timestamp of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public Ticks Timestamp
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
        /// Gets or sets the IEEE 1344 specific frame type of this frame.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This returns the protocol specific frame classification which uniquely identifies the frame type.
        /// </para>
        /// <para>
        /// This is the <see cref="ICommonHeader{TTypeIdentifier}.TypeID"/> implementation.
        /// </para>
        /// </remarks>
        public FrameType TypeID
        {
            get
            {
                return (FrameType)(m_sampleCount & Common.FrameTypeMask);
            }
            set
            {
                m_sampleCount = (short)((m_sampleCount & ~Common.FrameTypeMask) | (ushort)value);
            }
        }

        /// <summary>
        /// Gets or sets flag from that determines if this is the first frame image in a series of images representing an entire frame.
        /// </summary>
        public bool IsFirstFrame
        {
            get
            {
                return (m_sampleCount & Bit.Bit12) == 0;
            }
            set
            {
                if (value)
                    m_sampleCount = (short)(m_sampleCount & ~Bit.Bit12);
                else
                    m_sampleCount = (short)(m_sampleCount | Bit.Bit12);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this is the last frame image in a series of images representing an entire frame.
        /// </summary>
        public bool IsLastFrame
        {
            get
            {
                return (m_sampleCount & Bit.Bit11) == 0;
            }
            set
            {
                if (value)
                    m_sampleCount = (short)(m_sampleCount & ~Bit.Bit11);
                else
                    m_sampleCount = (short)(m_sampleCount | Bit.Bit11);
            }
        }

        /// <summary>
        /// Gets or sets the total frame count.
        /// </summary>
        public short FrameCount
        {
            get
            {
                return (short)(m_sampleCount & Common.FrameCountMask);
            }
            set
            {
                if (value > Common.MaximumFrameCount)
                    throw new OverflowException("Frame count value cannot exceed " + Common.MaximumFrameCount);
                else
                    m_sampleCount = (short)((m_sampleCount & ~Common.FrameCountMask) | (ushort)value);
            }
        }
        
        /// <summary>
        /// Gets or sets the parsing state for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public IChannelParsingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        // Gets or sets any additional state information - satifies ICommonHeader<FrameType>.State interface property
        object ICommonHeader<FrameType>.State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value as IChannelParsingState;
            }
        }

        /// <summary>
        /// Gets the fundamental frame type of this frame.
        /// </summary>
        /// <remarks>
        /// Frames are generally classified as data, configuration or header frames. This returns the general frame classification.
        /// </remarks>
        public FundamentalFrameType FrameType
        {
            get
            {
                // Translate IEEE 1344 specific frame type to fundamental frame type
                switch (TypeID)
                {
                    case Ieee1344.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case Ieee1344.FrameType.ConfigurationFrame:
                        return FundamentalFrameType.ConfigurationFrame;
                    case Ieee1344.FrameType.HeaderFrame:
                        return FundamentalFrameType.HeaderFrame;
                    default:
                        return FundamentalFrameType.Undetermined;
                }
            }
        }

        /// <summary>
        /// Gets or sets reference to a <see cref="FrameImageCollector"/> associated the IEEE 1344 frame, if any.
        /// </summary>
        public FrameImageCollector FrameImages
        {
            get
            {
                return m_frameImages;
            }
            set
            {
                m_frameImages = value;
            }
        }

        /// <summary>
        /// Gets or sets the entire length of the IEEE 1344 frame.
        /// </summary>
        public ushort FrameLength
        {
            get
            {
                return (ushort)(m_statusFlags & Common.FrameLengthMask);
            }
            set
            {
                if (value > Common.MaximumFrameLength)
                    throw new OverflowException("Frame length value cannot exceed " + Common.MaximumFrameLength);
                else
                    m_statusFlags = (short)((m_statusFlags & ~Common.FrameLengthMask) | (ushort)value);
            }
        }

        /// <summary>
        /// Gets or sets the length of the data in the IEEE 1344 frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
        /// </summary>
        public ushort DataLength
        {
            get
            {
                // Data length will be frame length minus common header length minus crc16
                return (ushort)(FrameLength - FixedLength - 2);
            }
            set
            {
                if (value > Common.MaximumDataLength)
                    throw new OverflowException("Data length value cannot exceed " + Common.MaximumDataLength);
                else
                    FrameLength = (ushort)(value + FixedLength + 2);
            }
        }

        /// <summary>
        /// Gets or sets the sample number (i.e., frame count) of this frame.
        /// </summary>
        public short SampleCount
        {
            get
            {
                return (short)(m_sampleCount & ~Common.FrameTypeMask);
            }
            set
            {
                if (value > Common.MaximumSampleCount)
                    throw new OverflowException("Sample count value cannot exceed " + Common.MaximumSampleCount);
                else
                    m_sampleCount = (short)((m_sampleCount & Common.FrameTypeMask) | (ushort)value);
            }
        }

        /// <summary>
        /// Gets the binary image of the common header portion of this frame.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[BinaryLength];

                // Make sure frame length gets included in status flags for generated binary image
                FrameLength = (ushort)BinaryLength;

                EndianOrder.BigEndian.CopyBytes((uint)TimeTag.Value, buffer, 0);
                EndianOrder.BigEndian.CopyBytes(m_sampleCount, buffer, 4);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the binary image of the common header portion of this frame.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return FixedLength;
            }
        }

        /// <summary>
        /// Determines if <see cref="IChannelFrame"/> is only partially parsed.
        /// </summary>
        /// <remarks>
        /// This frame is not complete - it only represents the parsed common "header" for frames.
        /// </remarks>
        public bool IsPartial
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes == null)
                    m_attributes = new Dictionary<string, string>();
                else
                    m_attributes.Clear();

                m_attributes.Add("Derived Type", this.GetType().Name);
                m_attributes.Add("Binary Length", BinaryLength.ToString());
                m_attributes.Add("Total Cells", "0");
                m_attributes.Add("Fundamental Frame Type", (int)FrameType + ": " + FrameType);
                m_attributes.Add("ID Code", "undefined");
                m_attributes.Add("Is Partial Frame", IsPartial.ToString());
                m_attributes.Add("Published", "n/a");
                m_attributes.Add("Ticks", "undefined");
                m_attributes.Add("Timestamp", "n/a");
                AppendHeaderAttributes(m_attributes);

                return m_attributes;
            }
        }

        /// <summary>
        /// User definable object used to hold a reference associated with the <see cref="IChannel"/> object.
        /// </summary>
        public object Tag
        {
            get
            {
                return m_tag;
            }
            set
            {
                m_tag = value;
            }
        }

        #region [ IChannelFrame Implementation ]

        ushort IChannelFrame.IDCode
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        object IChannelFrame.Cells
        {
            get
            {
                return null;
            }
        }

        UnixTimeTag IChannelFrame.TimeTag
        {
            get
            {
                // IChannelFrame expects timestamp in Unix timetag format
                return new UnixTimeTag(0);
            }
        }

        int ISupportBinaryImage.Initialize(byte[] binaryImage, int startIndex, int length)
        {
            // The common frame header is parsed during construction
            throw new NotImplementedException();
        }

        int IFrame.PublishedMeasurements
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IDictionary<MeasurementKey, IMeasurement> IFrame.Measurements
        {
            get
            {
                return null;
            }
        }

        bool IFrame.Published
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IMeasurement IFrame.LastSortedMeasurement
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="IFrame"/>.
        /// </summary>
        /// <param name="other">An <see cref="IFrame"/> value to compare to this instance.</param>
        /// <returns>
        /// True if <paramref name="other"/> has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(IFrame other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="IFrame"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="other">An <see cref="IFrame"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(IFrame other)
        {
            return (this as IFrame).Timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an <see cref="IFrame"/>.</exception>
        public int CompareTo(object obj)
        {
            IFrame other = obj as IFrame;

            if (other != null)
                return CompareTo(other);

            throw new ArgumentException("Frame can only be compared with other IFrames...");
        }

        /// <summary>
        /// Appends header specific attributes to <paramref name="attributes"/> dictionary.
        /// </summary>
        /// <param name="attributes">Dictionary to append header specific attributes to.</param>
        internal void AppendHeaderAttributes(Dictionary<string, string> attributes)
        {
            attributes.Add("Frame Type", (int)TypeID + ": " + TypeID);

            if (FrameImages != null)
            {
                attributes.Add("Frame Length", FrameImages.BinaryLength.ToString());
                attributes.Add("Frame Images", FrameImages.Count.ToString());
            }
            else
            {
                attributes.Add("Frame Length", FrameLength.ToString());
                attributes.Add("Frame Images", "0");
            }
            
            attributes.Add("Frame Count", FrameCount.ToString());
            attributes.Add("Sample Count", m_sampleCount.ToString());
            attributes.Add("Status Flags", m_statusFlags.ToString());
            attributes.Add("Is First Frame", IsFirstFrame.ToString());
            attributes.Add("Is Last Frame", IsLastFrame.ToString());
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Validates the CRC16 for the specified IEEE 1344 buffer.
        /// </summary>
        public static bool ChecksumIsValid(byte[] buffer, int startIndex, int length)
        {
            int sumLength = length - 2;
            return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) == buffer.Crc16Checksum(startIndex, sumLength);
        }

        #endregion       
    }
}