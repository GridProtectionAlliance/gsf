//*******************************************************************************************************
//  ChannelParsingStateBase.cs
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
//  3/7/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent parsing state class used by any kind of data.<br/>
    /// This is the base class of all parsing state classes in the phasor protocols library;
    /// it is the root of the parsing state class hierarchy.
    /// </summary>
    /// <remarks>
    /// This class is inherited by subsequent classes to provide parsing state information particular to data type needs.
    /// </remarks>
    public abstract class ChannelParsingStateBase : IChannelParsingState
    {
        #region [ Members ]

        // Fields
        private int m_parsedBinaryLength;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the length of the associated <see cref="IChannel"/> object being parsed from the binary image.
        /// </summary>
        public virtual int ParsedBinaryLength
        {
            get
            {
                return m_parsedBinaryLength;
            }
            set
            {
                m_parsedBinaryLength = value;
            }
        }

        #endregion
    }
}