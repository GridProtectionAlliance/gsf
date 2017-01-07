//******************************************************************************************************
//  ChannelCellBase.cs - Gbtc
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
    /// Represents the common implementation of the protocol independent representation of any kind of data cell.
    /// </summary>
    /// <remarks>
    /// This phasor protocol implementation defines a "cell" as a portion of a frame, i.e., a logical unit of data.
    /// For example, a <see cref="DataCellBase"/> (derived from <see cref="ChannelCellBase"/>) could be defined as a PMU
    /// within a frame of data, a <see cref="DataFrameBase"/>, that contains multiple PMU's coming from a PDC.
    /// </remarks>
    [Serializable]
    public abstract class ChannelCellBase : ChannelBase, IChannelCell
    {
        #region [ Members ]

        // Fields
        private IChannelFrame m_parent;         // Reference to parent frame of this channel cell
        private ushort m_idCode;                // Numeric identifier of this logical unit of data (e.g., PMU ID code)

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelCellBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">A reference to the parent <see cref="IChannelFrame"/> for this <see cref="ChannelCellBase"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ChannelCellBase"/>.</param>
        protected ChannelCellBase(IChannelFrame parent, ushort idCode)
        {
            m_parent = parent;
            m_idCode = idCode;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelCellBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelCellBase(SerializationInfo info, StreamingContext context)
        {
            // Deserialize basic channel cell values
            m_parent = (IChannelFrame)info.GetValue("parent", typeof(IChannelFrame));
            m_idCode = info.GetUInt16("id");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a reference to the parent <see cref="IChannelFrame"/> for this <see cref="ChannelCellBase"/>.
        /// </summary>
        public virtual IChannelFrame Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ChannelCellBase"/>.
        /// </summary>
        public new virtual IChannelCellParsingState State
        {
            get
            {
                return base.State as IChannelCellParsingState;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets or sets the numeric ID code for this <see cref="ChannelCellBase"/>.
        /// </summary>
        /// <remarks>
        /// Most phasor measurement devices define some kind of numeric identifier (e.g., a hardware identifier coded into the device ROM); this is the
        /// abstract representation of this identifier.
        /// </remarks>
        public virtual ushort IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ChannelCellBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("ID Code", IDCode.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the string representation of this <see cref="ChannelCellBase"/>.
        /// </summary>
        /// <returns>String representation of this <see cref="ChannelCellBase"/>.</returns>
        public override string ToString()
        {
            return IDCode.ToString();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize basic channel cell values
            info.AddValue("parent", m_parent, typeof(IChannelFrame));
            info.AddValue("id", m_idCode);
        }

        #endregion
    }
}