//*******************************************************************************************************
//  FrameImageCollector.cs
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
//  12/30/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Collects frame images until a full IEEE 1344 frame has been received.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001")]  // See constructor note below...
    public class FrameImageCollector
    {
        #region [ Members ]

        // Fields
        private MemoryStream m_frameQueue;
        private int m_frameCount;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameImageCollector"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816")]
        public FrameImageCollector()
        {
            // As an optimzation in context of usage, we don't implement IDisposable for
            // this class just for the memory stream since its Close method (i.e., Dispose)
            // does nothing (reflect it and look for yourself). Additionally, we go ahead
            // and suppress the finalizer for this stream to reduce that overhead too.
            m_frameQueue = new MemoryStream();
            GC.SuppressFinalize(m_frameQueue);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the combined binary image of all the collected frame images.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                return m_frameQueue.ToArray();
            }
        }

        /// <summary>
        /// Gets the length of the combined binary image of all the collected frame images.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return (int)m_frameQueue.Length;
            }
        }

        /// <summary>
        /// Gets the total number frames appended to <see cref="FrameImageCollector"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return m_frameCount;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Appends the current frame image to the frame image collection.
        /// </summary>
        public void AppendFrameImage(byte[] binaryImage, int offset, int length)
        {
            // Validate CRC of frame image being appended
            if (!CommonFrameHeader.ChecksumIsValid(binaryImage, offset, length))
                throw new InvalidOperationException("Invalid binary image detected - check sum of individual IEEE 1344 interleaved frame transmission did not match");

            // Include initial header in new stream...
            if (m_frameQueue.Length == 0)
                m_frameQueue.Write(binaryImage, offset, CommonFrameHeader.FixedLength);

            // Skip past header
            offset += CommonFrameHeader.FixedLength;

            // Include frame image
            m_frameQueue.Write(binaryImage, offset, length - CommonFrameHeader.FixedLength);

            // Track total frame images
            m_frameCount++;
        }

        #endregion
    }
}