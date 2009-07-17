//*******************************************************************************************************
//  ChannelCellBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of any kind of data cell.
    /// </summary>
    /// <remarks>
    /// This phasor protocol implementation defines a "cell" as a portion of a frame, i.e., a logical unit of data.
    /// For example, a <see cref="DataCellBase"/> (dervied from <see cref="ChannelCellBase"/>) could be defined as a PMU
    /// within a frame of data, a <see cref="DataFrameBase"/>, that contains multiple PMU's coming from a PDC.
    /// </remarks>
    [Serializable()]
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
        new public virtual IChannelCellParsingState State
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
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize basic channel cell values
            info.AddValue("parent", m_parent, typeof(IChannelFrame));
            info.AddValue("id", m_idCode);
        }

        #endregion
    }
}