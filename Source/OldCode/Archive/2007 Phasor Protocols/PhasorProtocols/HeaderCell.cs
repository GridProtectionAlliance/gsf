//*******************************************************************************************************
//  HeaderCell.cs
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
    /// Represents the protocol independent common implementation of an element of header data for cells in a <see cref="IHeaderFrame"/>.
    /// </summary>
    [Serializable()]
    public class HeaderCell : ChannelCellBase, IHeaderCell
    {
        #region [ Members ]

        // Fields
        private byte m_character;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="HeaderCell"/>.
        /// </summary>
        protected HeaderCell()
        {
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

        /// <summary>
        /// Creates a new <see cref="HeaderCell"/> from the specified parameters.
        /// </summary>
        public HeaderCell(IHeaderFrame parent, byte character)
            : base(parent, false, 0)
        {
            m_character = character;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ICommandFrame"/> for this <see cref="HeaderCell"/>.
        /// </summary>
        public virtual new IHeaderFrame Parent
        {
            get
            {
                return base.Parent as IHeaderFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets ASCII character as a <see cref="Byte"/> that represents this <see cref="HeaderCell"/>.
        /// </summary>
        public virtual byte Character
        {
            get
            {
                return m_character;
            }
            set
            {
                m_character = value;
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
        /// Gets the binary body image of the <see cref="HeaderCell"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                return new byte[] { m_character };
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

                baseAttributes.Add("Character", Encoding.ASCII.GetString(new byte[] { Character }));

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
            // TODO: It is expected that parent IHeaderFrame will validate that it has
            // enough length to parse entire cell well in advance so that low level parsing
            // routines do not have to re-validate that enough length is available to parse
            // needed information as an optimization...

            m_character = binaryImage[startIndex];

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
        internal static IHeaderCell CreateNewHeaderCell(IChannelFrame parent, IChannelFrameParsingState<IHeaderCell> state, int index, byte[] binaryImage, int startIndex)
        {
            return new HeaderCell() { Parent = parent as IHeaderFrame, Character = binaryImage[startIndex], IDCode = (ushort)index };
        }

        #endregion
    }
}