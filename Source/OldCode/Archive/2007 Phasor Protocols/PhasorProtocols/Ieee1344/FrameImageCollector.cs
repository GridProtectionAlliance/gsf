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

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Collects frame images until a full IEEE 1344 frame has been received.
    /// </summary>
    public class FrameImageCollector : IDisposable
    {
        #region [ Members ]

        // Fields
        private MemoryStream m_frameQueue;
        private int m_frameCount;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameImageCollector"/>.
        /// </summary>
        public FrameImageCollector()
        {
            m_frameQueue = new MemoryStream();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FrameImageCollector"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FrameImageCollector()
        {
            Dispose(false);
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
        /// Releases all the resources used by the <see cref="FrameImageCollector"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameImageCollector"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_frameQueue != null)
                            m_frameQueue.Dispose();

                        m_frameQueue = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

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