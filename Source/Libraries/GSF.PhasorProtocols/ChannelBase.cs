//******************************************************************************************************
//  ChannelBase.cs - Gbtc
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
//  3/7/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of any kind
    /// of data that can be parsed or generated.<br/>
    /// This is the base class of all parsing/generating classes in the phasor protocols library;
    /// it is the root of the parsing/generating class hierarchy.
    /// </summary>
    /// <remarks>
    /// This base class represents <see cref="IChannel"/> data images for parsing or generation in
    /// terms of a header, body and footer (see <see cref="BinaryImageBase"/> for details).
    /// </remarks>
    public abstract class ChannelBase : BinaryImageBase, IChannel
    {
        #region [ Members ]

        // Fields        
        private Dictionary<string, string> m_attributes;    // Attributes dictionary

    #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="ChannelBase"/> object.
        /// </summary>
        public virtual IChannelParsingState State { get; set; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="ChannelBase"/> object.
        /// </summary>
        /// <remarks>
        /// The attributes dictionary is relevant to all channel properties.  This dictionary will only be instantiated with a call to
        /// the <c>Attributes</c> property which will begin the enumeration of relevant system properties.  This is typically used for
        /// display purposes. For example, this information is displayed in a tree view on the the <b>PMU Connection Tester</b> to display
        /// attributes of data elements that may be protocol specific.
        /// </remarks>
        public virtual Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes is null)
                    m_attributes = new Dictionary<string, string>();
                else
                    m_attributes.Clear();

                m_attributes.Add("Derived Type", GetType().FullName);
                m_attributes.Add("Binary Length", BinaryLength.ToString());

                return m_attributes;
            }
        }

        /// <summary>
        /// Gets or sets a user definable reference to an object associated with this <see cref="ChannelBase"/> object.
        /// </summary>
        public virtual object Tag { get; set; }

        /// <summary>
        /// Gets the binary image of the <see cref="ChannelBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is not typically overridden since it is the combination of the header, body and footer images.
        /// </remarks>
        public virtual byte[] BinaryImage
        {
            // This proxy property could be removed if all channel base implementations were recoded to
            // implement GenerateHeaderImage, GenerateBodyImage, and GenerateFooterImage overrides...
            get
            {
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                // Copy in header, body and footer images
                int headerLength = HeaderLength;

                if (headerLength > 0 && !(HeaderImage is null))
                {
                    Buffer.BlockCopy(HeaderImage, 0, buffer, index, headerLength);
                    index += headerLength;
                }

                int bodyLength = BodyLength;

                if (bodyLength > 0 && !(BodyImage is null))
                {
                    Buffer.BlockCopy(BodyImage, 0, buffer, index, bodyLength);
                    index += bodyLength;
                }

                int footerLength = FooterLength;

                if (footerLength > 0 && !(FooterImage is null))
                    Buffer.BlockCopy(FooterImage, 0, buffer, index, footerLength);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="ChannelBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual byte[] HeaderImage => null;

        /// <summary>
        /// Gets the binary body image of the <see cref="ChannelBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual byte[] BodyImage => null;

        /// <summary>
        /// Gets the binary footer image of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual byte[] FooterImage => null;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Generates binary image of the object and copies it into the given buffer, for <see cref="ISupportBinaryImage.BinaryLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            byte[] image = BinaryImage;
            int length = image.Length;

            if (length > 0)
            {
                buffer.ValidateParameters(startIndex, length);
                Buffer.BlockCopy(image, 0, buffer, startIndex, length);
            }

            return length;
        }

        #endregion
    }
}