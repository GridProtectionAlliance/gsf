//*******************************************************************************************************
//  IBinaryDataProvider.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/01/2007 - Pinal C. Patel
//       Original version of source code generated
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************


namespace PCS.Parsing
{
    /// <summary>This interface allows any data structure to provide a binary representation of itself.</summary>
    public interface IBinaryDataProvider
    {
        byte[] BinaryImage
        {
            get;
        }

        int BinaryLength
        {
            get;
        }
    }
}