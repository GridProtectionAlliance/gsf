//******************************************************************************************************
//  HeaderCell.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of an element of header data for cells in a <see cref="IHeaderFrame"/>.
    /// </summary>
    [Serializable]
    public class HeaderCell : ChannelCellBase, IHeaderCell
    {
        #region [ Members ]

        // Fields
        private byte m_character;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="HeaderCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">A reference to the parent <see cref="ICommandFrame"/> for this <see cref="HeaderCell"/>.</param>
        /// <param name="character">ASCII character as a <see cref="byte"/> that represents this <see cref="HeaderCell"/>.</param>
        public HeaderCell(IHeaderFrame parent, byte character)
            : base(parent, 0)
        {
            m_character = character;
        }

        /// <summary>
        /// Creates a new <see cref="HeaderCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected HeaderCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize header cell value
            m_character = info.GetByte("character");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a reference to the parent <see cref="ICommandFrame"/> for this <see cref="HeaderCell"/>.
        /// </summary>
        public new virtual IHeaderFrame Parent
        {
            get => base.Parent as IHeaderFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets ASCII character as a <see cref="byte"/> that represents this <see cref="HeaderCell"/>.
        /// </summary>
        public virtual byte Character
        {
            get => m_character;
            set => m_character = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => 1;

        /// <summary>
        /// Gets the binary body image of the <see cref="HeaderCell"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                return new[] { m_character };
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="HeaderCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Character", Encoding.ASCII.GetString(new[] { Character }));

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            m_character = buffer[startIndex];

            return 1;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize header cell value
            info.AddValue("character", m_character);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Create new header cell delegate handler
        internal static IHeaderCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IHeaderCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            parsedLength = 1;
            return new HeaderCell(parent as IHeaderFrame, buffer[startIndex])
            {
                IDCode = (ushort)index
            };
        }

        #endregion
    }
}