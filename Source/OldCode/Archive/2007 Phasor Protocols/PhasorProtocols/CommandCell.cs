//*******************************************************************************************************
//  CommandCell.cs
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of an element of extended data for cells in a <see cref="ICommandFrame"/>.
    /// </summary>
    [Serializable()]
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
        /// <param name="extendedDataByte">Extended data <see cref="Byte"/> that represents this <see cref="CommandCell"/>.</param>
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
        public virtual new ICommandFrame Parent
        {
            get
            {
                return base.Parent as ICommandFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets extended data <see cref="Byte"/> that represents this <see cref="CommandCell"/>.
        /// </summary>
        public virtual byte ExtendedDataByte
        {
            get
            {
                return m_extendedDataByte;
            }
            set
            {
                m_extendedDataByte = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="CommandCell"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                return new byte[] { m_extendedDataByte };
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
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            m_extendedDataByte = binaryImage[startIndex];

            return 1;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
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
        internal static ICommandCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<ICommandCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            parsedLength = 1;
            return new CommandCell(parent as ICommandFrame, binaryImage[startIndex]) { IDCode = (ushort)index };
        }

        #endregion
    }
}