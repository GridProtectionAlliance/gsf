//*******************************************************************************************************
//  BinaryImageBase.cs
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
//  01/06/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.Parsing
{
    /// <summary>
    /// Defines a base class that represents binary images for parsing or generation in terms of a header, body and footer.
    /// </summary>
    [Serializable()]
    public abstract class BinaryImageBase : ISupportBinaryImage
    {
        #region [ Members ]

        // Fields
        private int m_binaryLength;
        private int m_headerLength;
        private int m_bodyLength;
        private int m_footerLength;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="BinaryImageBase"/>.
        /// </summary>
        protected BinaryImageBase()
        {
            m_binaryLength = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        /// <remarks>
        /// This property is not typically overriden since it is the sum of the header, body and footer lengths.
        /// </remarks>
        public virtual int BinaryLength // <- ISupportBinaryImage.BinaryLength implementation
        {
            get
            {
                // We cache lengths as an optimization since length overrides may be complex derived calculations
                if (m_binaryLength == -1)
                {
                    m_headerLength = HeaderLength;
                    m_bodyLength = BodyLength;
                    m_footerLength = FooterLength;
                    m_binaryLength = m_headerLength + m_bodyLength + m_footerLength;
                }

                return m_binaryLength;
            }
        }

        /// <summary>
        /// Gets the binary image of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is not typically overriden since it is the combination of the header, body and footer images.
        /// </remarks>
        public virtual byte[] BinaryImage // <- ISupportBinaryImage.BinaryImage implementation
        {
            get
            {
                // Call to BinaryLength caches lengths
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                // Copy in header, body and footer images                
                if (m_headerLength > 0)
                {
                    Buffer.BlockCopy(HeaderImage, 0, buffer, index, m_headerLength);
                    index += m_headerLength;
                }

                if (m_bodyLength > 0)
                {
                    Buffer.BlockCopy(BodyImage, 0, buffer, index, m_bodyLength);
                    index += m_bodyLength;
                }

                if (m_footerLength > 0)
                    Buffer.BlockCopy(FooterImage, 0, buffer, index, m_footerLength);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        /// <remarks>
        /// This property is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual int HeaderLength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual byte[] HeaderImage
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        /// <remarks>
        /// This property is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual int BodyLength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual byte[] BodyImage
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        /// <remarks>
        /// This property is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual int FooterLength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the binary footer image of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual byte[] FooterImage
        {
            get
            {
                return null;
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
        /// This method is not typically overriden since it is parses the header, body and footer images in sequence.
        /// </remarks>
        public virtual int Initialize(byte[] binaryImage, int startIndex, int length) // <- ISupportBinaryImage.Initialize implementation
        {
            int index = startIndex;

            // Parse out header, body and footer images
            index += ParseHeaderImage(binaryImage, index, length);
            index += ParseBodyImage(binaryImage, index, length - (index - startIndex));
            index += ParseFooterImage(binaryImage, index, length - (index - startIndex));

            return (index - startIndex);
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            return 0;
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            return 0;
        }

        /// <summary>
        /// Parses the binary footer image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is typically overriden by a specific protocol implementation.
        /// </remarks>
        protected virtual int ParseFooterImage(byte[] binaryImage, int startIndex, int length)
        {
            return 0;
        }

        #endregion
    }
}