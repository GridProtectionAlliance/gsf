//*******************************************************************************************************
//  IConnectionParameters.cs
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

using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of specific connection parameters.
    /// </summary>
    public interface IConnectionParameters : ISerializable
    {
        /// <summary>
        /// Returns True if all connection parameters are valid.
        /// </summary>
        bool ValuesAreValid { get; }
    }
}