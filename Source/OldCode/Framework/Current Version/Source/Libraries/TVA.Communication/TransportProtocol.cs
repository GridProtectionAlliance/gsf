//*******************************************************************************************************
//  TransportProtocol.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/03/2008 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA.Communication
{
    /// <summary>
    /// Indicates the protocol used in server-client communication.
    /// </summary>
    public enum TransportProtocol
    {
        /// <summary>
        /// <see cref="TransportProtocol"/> is Transmission Control Protocol.
        /// </summary>
        Tcp,
        /// <summary>
        /// <see cref="TransportProtocol"/> is User Datagram Protocol.
        /// </summary>
        Udp,
        /// <summary>
        /// <see cref="TransportProtocol"/> is serial interface.
        /// </summary>
        Serial,
        /// <summary>
        /// <see cref="TransportProtocol"/> is file-system based.
        /// </summary>
        File
    }
}
