//*******************************************************************************************************
//  ChannelFrameBase.vb - Channel data frame base class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PCS;
using PCS.Interop;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of any frame of data that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelFrameBase<T> : ChannelBase, IChannelFrame where T : IChannelCell
    {
        private ushort m_idCode;
        private IChannelCellCollection<T> m_cells;
        private long m_ticks;
        private bool m_published;
        private int m_publishedMeasurements;
        private ushort m_parsedBinaryLength;
        private Dictionary<MeasurementKey, IMeasurement> m_measurements;
        private IMeasurement m_lastSortedMeasurement;

        protected ChannelFrameBase()
        {
        }

        protected ChannelFrameBase(SerializationInfo info, StreamingContext context)
        {
            // Deserialize key frame elements...
            m_idCode = info.GetUInt16("idCode");
            m_cells = (IChannelCellCollection<T>)info.GetValue("cells", typeof(IChannelCellCollection<T>));
            m_ticks = info.GetInt64("ticks");
        }

        protected ChannelFrameBase(IChannelCellCollection<T> cells)
        {
            m_cells = cells;
            m_ticks = DateTime.UtcNow.Ticks;
        }

        protected ChannelFrameBase(ushort idCode, IChannelCellCollection<T> cells, long ticks)
        {
            m_idCode = idCode;
            m_cells = cells;
            m_ticks = ticks;
        }

        protected ChannelFrameBase(ushort idCode, IChannelCellCollection<T> cells, UnixTimeTag timeTag)
            : this(idCode, cells, timeTag.ToDateTime().Ticks)
        {
        }

        // Derived classes are expected to expose a Protected Sub New(ByVal state As IChannelFrameParsingState(Of T), ByVal binaryImage As Byte(), ByVal startIndex As int)
        protected ChannelFrameBase(IChannelFrameParsingState<T> state, byte[] binaryImage, int startIndex)
            : this(state.Cells)
        {
            ParsedBinaryLength = state.ParsedBinaryLength;
            ParseBinaryImage(state, binaryImage, startIndex);
        }

        // Derived classes are expected to expose a Protected Sub New(ByVal channelFrame As IChannelFrame)
        protected ChannelFrameBase(IChannelFrame channelFrame)
            : this(channelFrame.IDCode, (IChannelCellCollection<T>)channelFrame.Cells, channelFrame.Ticks)
        {
        }

        FundamentalFrameType IChannelFrame.FrameType
        {
            get
            {
                return this.FundamentalFrameType;
            }
        }

        abstract protected FundamentalFrameType FundamentalFrameType
        {
            get;
        }

        virtual protected IChannelCellCollection<T> Cells
        {
            get
            {
                return m_cells;
            }
        }

        object IChannelFrame.Cells
        {
            get
            {
                return m_cells;
            }
        }

        public virtual IDictionary<MeasurementKey, IMeasurement> Measurements
        {
            get
            {
                if (m_measurements == null)
                {
                    m_measurements = new Dictionary<MeasurementKey, IMeasurement>();
                }
                return m_measurements;
            }
        }

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

        public virtual long Ticks
        {
            get
            {
                return m_ticks;
            }
            set
            {
                m_ticks = value;
            }
        }

        public IMeasurement LastSortedMeasurement
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

        public virtual UnixTimeTag TimeTag
        {
            get
            {
                return new UnixTimeTag(Timestamp);
            }
        }

        public virtual DateTime Timestamp
        {
            get
            {
                return new DateTime(m_ticks);
            }
        }

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

        public int PublishedMeasurements
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

        public virtual bool IsPartial
        {
            get
            {
                return false;
            }
        }

        protected virtual ushort ParsedBinaryLength
        {
            set
            {
                m_parsedBinaryLength = value;
            }
        }

        // We override normal binary length so we can extend length to include check-sum
        // Also - if frame length was parsed from stream header - we use that length
        // instead of the calculated length...
        public override ushort BinaryLength
        {
            get
            {
                if (m_parsedBinaryLength > 0)
                {
                    return m_parsedBinaryLength;
                }
                else
                {
                    return (ushort)(2U + base.BinaryLength);
                }
            }
        }

        // We override normal binary image to include check-sum
        public override byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                // Copy in base image
                PhasorProtocols.Common.CopyImage(base.BinaryImage, buffer, ref index, base.BinaryLength);

                // Add check sum
                AppendChecksum(buffer, index);

                return buffer;
            }
        }

        // We override normal binary image parser to validate check-sum
        override public void ParseBinaryImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
            // Validate checksum
            if (!ChecksumIsValid(binaryImage, startIndex))
            {
                throw (new InvalidOperationException(@"Invalid binary image detected - check sum of " + DerivedType.Name + @" did not match"));
            }

            // Perform regular data parse
            base.ParseBinaryImage(state, binaryImage, startIndex);
        }

        protected override ushort BodyLength
        {
            get
            {
                return m_cells.BinaryLength;
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                return m_cells.BinaryImage;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
            // Parse all frame cells
            IChannelFrameParsingState<T> frameParsingState = (IChannelFrameParsingState<T>)state;

            for (int x = 0; x <= frameParsingState.CellCount - 1; x++)
            {
                m_cells.Add(frameParsingState.CreateNewCellFunction(this, frameParsingState, x, binaryImage, startIndex));
                startIndex += m_cells[x].BinaryLength;
            }
        }

        protected virtual bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            int sumLength = (int)BinaryLength - 2;
            return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
        }

        protected virtual void AppendChecksum(byte[] buffer, int startIndex)
        {
            EndianOrder.BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
        }

        protected virtual ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // We implement CRC CCITT check sum as the default, but each protocol can override as necessary
            return TVA.IO.Compression.Common.CRC_CCITT(ushort.MaxValue, buffer, offset, length);
        }

        // We sort frames by timestamp
        public int CompareTo(IFrame other)
        {
            return m_ticks.CompareTo(other.Ticks);
        }

        public int CompareTo(object obj)
        {
            IFrame other = obj as IFrame;
            if (other != null)
            {
                return CompareTo(other);
            }
            throw (new ArgumentException(@"Frame can only be compared with other IFrames..."));
        }

        public bool Equals(IFrame other)
        {
            return (CompareTo(other) == 0);
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Add key frame elements for serialization...
            info.AddValue("idCode", m_idCode);
            info.AddValue("cells", m_cells, typeof(IChannelCellCollection<T>));
            info.AddValue("ticks", m_ticks);
        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Total Cells", Cells.Count.ToString());
                baseAttributes.Add("Fundamental Frame Type", (int)FundamentalFrameType + ": " + FundamentalFrameType);
                baseAttributes.Add("ID Code", IDCode.ToString());
                baseAttributes.Add("Is Partial Frame", IsPartial.ToString());
                baseAttributes.Add("Published", Published.ToString());
                baseAttributes.Add("Ticks", Ticks.ToString());
                baseAttributes.Add("Timestamp", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                return baseAttributes;
            }
        }
    }
}
