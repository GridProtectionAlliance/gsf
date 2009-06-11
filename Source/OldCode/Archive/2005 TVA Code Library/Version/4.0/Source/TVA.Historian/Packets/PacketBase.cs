//*******************************************************************************************************
//  PacketBase.cs
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
//  07/27/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA.Parsing;

namespace TVA.Historian.Packets
{
    /// <summary>
    /// A base class for a binary packet received by the archival process of Historian.
    /// </summary>
    /// <seealso cref="IPacket"/>
    public abstract class PacketBase : IPacket
    {
        #region [ Members ]

        // Constants
        
        /// <summary>
        /// Specifies the number of bytes in the binary image of the packet.
        /// </summary>
        /// <remarks>A value of -1 indicates that the binary image of the packet is of variable length.</remarks>
        public const int ByteCount = -1;

        // Fields
        private IArchive m_archive;
        private Func<IEnumerable<byte[]>> m_processHandler;
        private Func<IEnumerable<byte[]>> m_preProcessHandler;
        private ICommonHeader<short> m_commonHeader;
        private short m_typeID;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the packet.
        /// </summary>
        /// <param name="packetId">Numeric identifier for the packet type.</param>
        protected PacketBase(short packetId)
        {
            m_typeID = packetId;
        }

        #endregion

        #region [ Properties ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, gets the length of the packet's binary representation.
        /// </summary>
        public abstract int BinaryLength { get; }

        /// <summary>
        /// When overridden in a derived class, gets the binary representation of the packet.
        /// </summary>
        public abstract byte[] BinaryImage { get; }

        #endregion

        /// <summary>
        /// Gets or sets the current <see cref="IArchive"/>.
        /// </summary>
        public IArchive Archive
        {
            get
            {
                return m_archive;
            }
            set
            {
                m_archive = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that processes the packet.
        /// </summary>
        /// <remarks>
        /// <see cref="Func{TResult}"/> returns an <see cref="IEnumerable{T}"/> object containing the binary data to be sent back to the packet sender.
        /// </remarks>
        public Func<IEnumerable<byte[]>> ProcessHandler
        {
            get
            {
                return m_processHandler;
            }
            set
            {
                m_processHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that pre-processes the packet.
        /// </summary>
        /// <remarks>
        /// <see cref="Func{TResult}"/> returns an <see cref="IEnumerable{T}"/> object containing the binary data to be sent back to the packet sender.
        /// </remarks>
        public Func<IEnumerable<byte[]>> PreProcessHandler
        {
            get
            {
                return m_preProcessHandler;
            }
            set
            {
                m_preProcessHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PacketCommonHeader"/> for the packet.
        /// </summary>
        public ICommonHeader<short> CommonHeader
        {
            get
            {
                return m_commonHeader;
            }
            set
            {
                m_commonHeader = value;
            }
        }

        /// <summary>
        /// Gets or sets the numeric identifier for the packet type.
        /// </summary>
        public short TypeID
        {
            get
            {
                return m_typeID;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, initializes packet from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing the packet.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing the packet.</returns>
        public abstract int Initialize(byte[] binaryImage, int startIndex, int length);

        /// <summary>
        /// When overridden in a derived class, extracts time series data from the packet.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> object of <see cref="IDataPoint"/>s if the packet contains time series data; otherwise null.</returns>
        public abstract IEnumerable<IDataPoint> ExtractTimeSeriesData();

        #endregion

        #endregion
    }
}