//*******************************************************************************************************
//  IHeaderFrame.cs
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

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of header frame that contains
    /// a collection of <see cref="IHeaderCell"/> objects.
    /// </summary>
    public interface IHeaderFrame : IChannelFrame
    {
        /// <summary>
        /// Gets reference to the <see cref="HeaderCellCollection"/> for this <see cref="IHeaderFrame"/>.
        /// </summary>
        new HeaderCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="IHeaderFrame"/>.
        /// </summary>
        new HeaderFrameParsingState State { get; set; }

        /// <summary>
        /// Gets or sets header data for this <see cref="IHeaderFrame"/>.
        /// </summary>
        string HeaderData { get; set; }
    }
}