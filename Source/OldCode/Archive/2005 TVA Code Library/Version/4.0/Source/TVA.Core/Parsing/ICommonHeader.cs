//*******************************************************************************************************
//  ICommonHeader.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/17/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Parsing
{
    /// <summary>
    /// Defines the common header of a frame image for a set of parsed types, consisting at least of a type ID.
    /// </summary>
    /// <remarks>
    /// Header implementations can extend this interface as necessary to accomodate protocol specific header images.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    public interface ICommonHeader<TTypeIdentifier>
    {
        /// <summary>
        /// Gets or sets the identifier used for identifying the <see cref="Type"/> to be parsed.
        /// </summary>
        TTypeIdentifier TypeID { get; }

        /// <summary>
        /// Gets or sets any additional state information that might be needed for parsing.
        /// </summary>
        object State { get; set; }
    }
}
