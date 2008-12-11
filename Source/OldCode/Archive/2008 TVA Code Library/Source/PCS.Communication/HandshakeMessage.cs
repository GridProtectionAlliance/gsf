//*******************************************************************************************************
//  HandshakeMessage.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Text;
using PCS.Parsing;

namespace PCS.Communication
{
    [Serializable()]
    internal class HandshakeMessage : ISupportBinaryImage
    {
        /// <summary>
        /// 2-Byte identifier for <see cref="HandshakeMessage"/>.
        /// </summary>
        public static byte[] MessageIdentifier = { 0x98, 0xC4 };

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <remarks>Max size is 16.</remarks>
        public Guid ID;

        /// <summary>
        /// Gets or sets the passphrase for authentication/ciphering.
        /// </summary>
        /// <remarks>Max size is 260.</remarks>
        public string Passphrase;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandshakeMessage"/> class.
        /// </summary>
        public HandshakeMessage()
            : this(Guid.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandshakeMessage"/> class.
        /// </summary>
        public HandshakeMessage(Guid id, string passphrase)
        {
            ID = id;
            Passphrase = passphrase;
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get { return 2 + 16 + 260; }
        }

        /// <summary>
        /// Gets the binary representation of the <see cref="HandshakeMessage"/> object.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                // Create the image.
                byte[] image = new byte[BinaryLength];
                // Populate the image.
                Passphrase = Passphrase.PadRight(260).TruncateRight(260);
                Buffer.BlockCopy(MessageIdentifier, 0, image, 0, 2);
                Buffer.BlockCopy(ID.ToByteArray(), 0, image, 2, 16);
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(Passphrase), 0, image, 18, 260);
                // Return the image.
                return image;
            }
        }

        /// <summary>
        /// Initializes the <see cref="HandshakeMessage"/> object from the binary image.
        /// </summary>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= BinaryLength)
            {
                if (binaryImage[startIndex] != MessageIdentifier[0] ||
                    binaryImage[startIndex + 1] != MessageIdentifier[1])
                {
                    // Message identifier don't match.
                    return -1;
                }

                try
                {
                    // Binary image has sufficient data.
                    ID = new Guid(binaryImage.BlockCopy(startIndex + 2, 16));
                    Passphrase = Encoding.ASCII.GetString(binaryImage, startIndex + 18, 260).Trim();

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
