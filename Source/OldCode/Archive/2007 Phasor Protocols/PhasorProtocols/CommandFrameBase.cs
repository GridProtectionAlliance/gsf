//*******************************************************************************************************
//  CommandFrameBase.cs
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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public abstract class CommandFrameBase : ChannelFrameBase<ICommandCell>, ICommandFrame
    {
        #region [ Members ]

        // Fields
        private DeviceCommand m_command;

        #endregion

        #region [ Constructors ]

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

        /// <summary>
        /// Creates a new <see cref="CommandFrameBase"/> from the specified parameters.
        /// </summary>
        protected CommandFrameBase(CommandCellCollection cells, DeviceCommand command)
            : base(0, cells, 0)
        {
            m_command = command;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public override FundamentalFrameType FrameType
        {
            get
            {
                return FundamentalFrameType.CommandFrame;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public virtual new CommandCellCollection Cells
        {
            get
            {
                return base.Cells as CommandCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DeviceCommand"/> for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public virtual DeviceCommand Command
        {
            get
            {
                return m_command;
            }
            set
            {
                m_command = value;
            }
        }

        /// <summary>
        /// Gets or sets extended binary image data for this <see cref="CommandFrameBase"/>.
        /// </summary>
        public virtual byte[] ExtendedData
        {
            get
            {
                return Cells.BinaryImage;
            }
            set
            {
                Cells.Clear();
                State = new CommandFrameParsingState(0, value.Length);
                ParseBodyImage(value, 0, value.Length);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderImage
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the binary header image of this <see cref="CommandFrameBase"/>.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[2];

                EndianOrder.BigEndian.CopyBytes((short)m_command, buffer, 0);

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
                    baseAttributes.Add("Extended Data", ByteEncoding.Hexadecimal.GetString(Cells.BinaryImage, 0, Cells.Count));
                else
                    baseAttributes.Add("Extended Data", "<null>");

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            m_command = (DeviceCommand)EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            return 2;
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