//******************************************************************************************************
//  CommandFrameBase.cs - Gbtc
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
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public abstract class CommandFrameBase : ChannelFrameBase<ICommandCell>, ICommandFrame
    {
        #region [ Members ]

        // Fields
        private DeviceCommand m_command;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrameBase"/> from specified parameters.
        /// </summary>
        /// <param name="cells">The reference to the <see cref="CommandCellCollection"/> for this <see cref="CommandFrameBase"/>.</param>
        /// <param name="command">The <see cref="DeviceCommand"/> for this <see cref="CommandFrameBase"/>.</param>
        protected CommandFrameBase(CommandCellCollection cells, DeviceCommand command)
            : base(0, cells, 0)
        {
            m_command = command;
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrameBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommandFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize command frame
            m_command = (DeviceCommand)info.GetValue("command", typeof(DeviceCommand));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public override FundamentalFrameType FrameType => FundamentalFrameType.CommandFrame;

        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public new virtual CommandCellCollection Cells => base.Cells as CommandCellCollection;

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="CommandFrameBase"/>.
        /// </summary>
        public new virtual ICommandFrameParsingState State
        {
            get => base.State as ICommandFrameParsingState;
            set => base.State = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DeviceCommand"/> for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public virtual DeviceCommand Command
        {
            get => m_command;
            set => m_command = value;
        }

        /// <summary>
        /// Gets or sets extended binary image data for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public virtual byte[] ExtendedData
        {
            get => Cells.BinaryImage();
            set
            {
                Cells.Clear();
                State = new CommandFrameParsingState(0, value.Length, true, true);
                ParseBodyImage(value, 0, value.Length);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => base.BodyLength + 2;

        /// <summary>
        /// Gets the binary body image of this <see cref="CommandFrameBase"/>.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];
                int index = 2;

                BigEndian.CopyBytes((short)m_command, buffer, 0);
                base.BodyImage.CopyImage(buffer, ref index, base.BodyLength);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="CommandFrameBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Device Command", (int)Command + ": " + Command);

                if (Cells.Count > 0)
                    baseAttributes.Add("Extended Data", ByteEncoding.Hexadecimal.GetString(Cells.BinaryImage()));
                else
                    baseAttributes.Add("Extended Data", "<null>");

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
            int parsedLength = 2;

            m_command = (DeviceCommand)BigEndian.ToInt16(buffer, startIndex);
            parsedLength += base.ParseBodyImage(buffer, startIndex + 2, length);

            return parsedLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize command frame
            info.AddValue("command", m_command, typeof(DeviceCommand));
        }

        #endregion
    }
}