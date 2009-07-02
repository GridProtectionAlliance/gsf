//*******************************************************************************************************
//  ChannelCellParsingStateBase.cs
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
//  03/07/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of the parsing state used by any <see cref="IChannelCell"/>.
    /// </summary>
    public abstract class ChannelCellParsingStateBase : ChannelParsingStateBase, IChannelCellParsingState
    {
        #region [ Members ]

        // Fields
        private int m_phasorCount;
        private int m_analogCount;
        private int m_digitalCount;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of phasor elements associated with the <see cref="IChannelCell"/> being parsed.
        /// </summary>
        public virtual int PhasorCount
        {
            get
            {
                return m_phasorCount;
            }
            set
            {
                m_phasorCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of analog elements associated with the <see cref="IChannelCell"/> being parsed.
        /// </summary>
        public virtual int AnalogCount
        {
            get
            {
                return m_analogCount;
            }
            set
            {
                m_analogCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of digital elements associated with the <see cref="IChannelCell"/> being parsed.
        /// </summary>
        public virtual int DigitalCount
        {
            get
            {
                return m_digitalCount;
            }
            set
            {
                m_digitalCount = value;
            }
        }

        #endregion
    }
}