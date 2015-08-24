//*******************************************************************************************************
//  ChannelFrameParsingStateBase.cs
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
//  01/14/2005 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IChannelFrame"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of <see cref="IChannelCell"/> referenced by <see cref="IChannelFrame"/> being parsed.</typeparam>
    public abstract class ChannelFrameParsingStateBase<T> : ChannelParsingStateBase, IChannelFrameParsingState<T> where T : IChannelCell
    {
        #region [ Members ]

        // Fields
        private int m_cellCount;
        private CreateNewCellFunction<T> m_createNewCellFunction;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelFrameParsingStateBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="IChannelFrame"/> being parsed.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="IChannelCell"/> instances.</param>
        protected ChannelFrameParsingStateBase(int parsedBinaryLength, CreateNewCellFunction<T> createNewCellFunction)
        {
            base.ParsedBinaryLength = parsedBinaryLength;
            m_createNewCellFunction = createNewCellFunction;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to delegate used to create a new <see cref="IChannelCell"/> object.
        /// </summary>
        public virtual CreateNewCellFunction<T> CreateNewCell
        {
            get
            {
                return m_createNewCellFunction;
            }
        }

        /// <summary>
        /// Gets or sets number of cells that are expected in associated <see cref="IChannelFrame"/> being parsed.
        /// </summary>
        public virtual int CellCount
        {
            get
            {
                return m_cellCount;
            }
            set
            {
                m_cellCount = value;
            }
        }

        #endregion
    }
}