//*******************************************************************************************************
//  IBinaryDataConsumer.vb - Binary data consumer interface
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/01/2007 - Pinal C. Patel
//       Original version of source code generated
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************

using System;

namespace TVA.Parsing
{
    public interface IBinaryDataConsumer
    {
        int Initialize(byte[] binaryImage, int startIndex);
    }
}