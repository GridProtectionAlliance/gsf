//*******************************************************************************************************
//  GoodbyeMessage.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R. Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using TVA.Parsing;

namespace TVA.Communication
{
    [Serializable()]
    internal class GoodbyeMessage : ISupportBinaryImage
    {
        /// <summary>
        /// 2-Byte identifier for <see cref="GoodbyeMessage"/>.
        /// </summary>
        public static byte[] MessageIdentifier = { 0xA3, 0xC4 };

        /// <summary>
        /// Gets or sets the disconnecting client's ID.
        /// </summary>
        /// <remarks>Max size is 16.</remarks>
        public Guid ID;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoodbyeMessage"/> class.
        /// </summary>
        public GoodbyeMessage()
            : this(Guid.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoodbyeMessage"/> class.
        /// </summary>
        public GoodbyeMessage(Guid id)
        {
            ID = id;
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get { return MessageIdentifier.Length + 16; }
        }

        /// <summary>
        /// Gets the binary representation of the <see cref="GoodbyeMessage"/> object.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                // Create the image.
                byte[] image = new byte[BinaryLength];
                // Populate the image.
                Buffer.BlockCopy(MessageIdentifier, 0, image, 0, MessageIdentifier.Length);
                Buffer.BlockCopy(ID.ToByteArray(), 0, image, MessageIdentifier.Length, 16);
                // Return the image.
                return image;
            }
        }

        /// <summary>
        /// Initializes the <see cref="GoodbyeMessage"/> object from the binary image.
        /// </summary>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= BinaryLength)
            {
                if (binaryImage.CompareTo(0, MessageIdentifier, 0, MessageIdentifier.Length) != 0)
                    // Message identifier don't match.
                    return -1;

                try
                {
                    // Binary image has sufficient data.
                    ID = new Guid(binaryImage.BlockCopy(startIndex + MessageIdentifier.Length, 16));

                    return BinaryLength;
                }
                catch
                {
                    return -1;
                }
            }
            else
            {
                // Binary image doesn't have sufficient data.
                return -1;
            }
        }
    }
}
