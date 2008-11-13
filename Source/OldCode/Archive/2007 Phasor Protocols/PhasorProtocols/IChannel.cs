//*******************************************************************************************************
//  IChannel.cs
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using PCS.Parsing;

namespace PCS.PhasorProtocols
{
    /// <summary>This interface represents a protocol independent representation of any data type.</summary>
    /// <remarks>This is the root interface of the phasor protocol library.</remarks>
    [CLSCompliant(false)]
    public interface IChannel : IBinaryDataProducer
    {

        Type DerivedType
        {
            get;
        }

        // At its most basic level - all data represented by the protocols can either be "parsed" or "generated"
        // hence the following methods common to all elements

        void ParseBinaryImage(IChannelParsingState state, byte[] binaryImage, int startIndex);

        new ushort BinaryLength
        {
            get;
        }

        Dictionary<string, string> Attributes
        {
            get;
        }

        object Tag
        {
            get;
            set;
        }

    }
}
