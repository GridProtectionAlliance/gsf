//*******************************************************************************************************
//  HeaderFrameBase.cs
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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="IHeaderFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public abstract class HeaderFrameBase : ChannelFrameBase<IHeaderCell>, IHeaderFrame
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="HeaderFrameBase"/>.
        /// </summary>
        /// <param name="cells">The reference to the <see cref="HeaderCellCollection"/> for this <see cref="HeaderFrameBase"/>.</param>
        protected HeaderFrameBase(HeaderCellCollection cells)
            : base(0, cells, 0)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HeaderFrameBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected HeaderFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="HeaderFrameBase"/>.
        /// </summary>
        public override FundamentalFrameType FrameType
        {
            get
            {
                return FundamentalFrameType.HeaderFrame;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="HeaderCellCollection"/> for this <see cref="HeaderFrameBase"/>.
        /// </summary>
        public virtual new HeaderCellCollection Cells
        {
            get
            {
                return base.Cells as HeaderCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="HeaderFrameBase"/>.
        /// </summary>
        public virtual new IHeaderFrameParsingState State
        {
            get
            {
                return base.State as IHeaderFrameParsingState;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets or sets header data for this <see cref="HeaderFrameBase"/>.
        /// </summary>
        public virtual string HeaderData
        {
            get
            {
                return Encoding.ASCII.GetString(Cells.BinaryImage);
            }
            set
            {
                Cells.Clear();
                State = new HeaderFrameParsingState(0, value.Length);
                ParseBodyImage(Encoding.ASCII.GetBytes(value), 0, value.Length);
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="HeaderFrameBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Header Data", HeaderData);

                return baseAttributes;
            }
        }

        #endregion
    }
}