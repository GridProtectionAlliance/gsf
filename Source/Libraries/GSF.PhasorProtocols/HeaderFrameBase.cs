//******************************************************************************************************
//  HeaderFrameBase.cs - Gbtc
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
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="IHeaderFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
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
        public new virtual HeaderCellCollection Cells
        {
            get
            {
                return base.Cells as HeaderCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="HeaderFrameBase"/>.
        /// </summary>
        public new virtual IHeaderFrameParsingState State
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
                return Encoding.ASCII.GetString(Cells.BinaryImage());
            }
            set
            {
                Cells.Clear();
                State = new HeaderFrameParsingState(0, value.Length, true, true);
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