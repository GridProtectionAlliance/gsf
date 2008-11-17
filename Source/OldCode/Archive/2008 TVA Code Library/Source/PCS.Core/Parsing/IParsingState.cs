//*******************************************************************************************************
//  IParsingState.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/17/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCS.Parsing
{
    /// <summary>
    /// Defines the current parsing state for an image.
    /// </summary>
    /// <remarks>
    /// Parsing state implementations will extend this class as necessary to accomodate custom parsing states.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of the identifier.</typeparam>
    public interface IParsingState<TTypeIdentifier>
    {
        /// <summary>
        /// Gets or sets the identifier used for identifying the <see cref="Type"/> to be parsed.
        /// </summary>
        TTypeIdentifier TypeID { get; set; }
    }
}
