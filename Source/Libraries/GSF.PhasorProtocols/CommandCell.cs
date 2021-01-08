//******************************************************************************************************
//  CommandCell.cs - Gbtc
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

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of an element of extended data for cells in a <see cref="ICommandFrame"/>.
    /// </summary>
    [Serializable]
    public class CommandCell : ChannelCellBase, ICommandCell
    {
        #region [ Members ]

        // Fields
        private byte m_extendedDataByte;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">A reference to the parent <see cref="ICommandFrame"/> for this <see cref="CommandCell"/>.</param>
        /// <param name="extendedDataByte">Extended data <see cref="byte"/> that represents this <see cref="CommandCell"/>.</param>
        public CommandCell(ICommandFrame parent, byte extendedDataByte)
            : base(parent, 0)
        {
            m_extendedDataByte = extendedDataByte;
        }

        /// <summary>
        /// Creates a new <see cref="CommandCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommandCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize command cell value
            m_extendedDataByte = info.GetByte("extendedDataByte");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a reference to the parent <see cref="ICommandFrame"/> for this <see cref="CommandCell"/>.
        /// </summary>
        public new virtual ICommandFrame Parent
        {
            get => base.Parent as ICommandFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets extended data <see cref="byte"/> that represents this <see cref="CommandCell"/>.
        /// </summary>
        public virtual byte ExtendedDataByte
        {
            get => m_extendedDataByte;
            set => m_extendedDataByte = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => 1;

        /// <summary>
        /// Gets the binary body image of the <see cref="CommandCell"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                return new[] { m_extendedDataByte };
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="CommandCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Extended Data Byte", "0x" + ExtendedDataByte.ToString("x"));

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

            m_extendedDataByte = buffer[startIndex];

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

            // Serialize command cell value
            info.AddValue("extendedDataByte", m_extendedDataByte);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Create new command cell delegate handler
        internal static ICommandCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<ICommandCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            parsedLength = 1;
            return new CommandCell(parent as ICommandFrame, buffer[startIndex])
            {
                IDCode = (ushort)index
            };
        }

        #endregion
    }
}