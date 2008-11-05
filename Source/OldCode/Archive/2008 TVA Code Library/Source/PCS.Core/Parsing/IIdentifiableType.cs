//*******************************************************************************************************
//  IIdentifiableType.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/05/2008 - Pinal C Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.Parsing
{
    /// <summary>
    /// Specifies that the <see cref="Type"/> has an identifier for identification.
    /// </summary>
    /// <typeparam name="TIdentifier">Type of the identifier.</typeparam>
    public interface IIdentifiableType<TIdentifier>
    {
        /// <summary>
        /// Gets the identifier that can be used for identifying the <see cref="Type"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="TypeID"/> must be unique across all siblings implementing a common <see cref="Type"/>.
        /// </remarks>
        TIdentifier TypeID { get; }
    }
}
