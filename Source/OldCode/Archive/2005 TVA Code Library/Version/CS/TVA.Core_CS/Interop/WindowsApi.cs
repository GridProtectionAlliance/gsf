//*******************************************************************************************************
//  TVA.Interop.WindowsApi.vb - Common Windows API Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/24/2006 - J. Ritchie Carroll
//       Initial version of source created
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace TVA
{
    namespace Interop
    {
        /// <summary>Defines common Windows API functions</summary>
        public static class WindowsApi
        {
            [DllImport("kernel32.dll")]
            private static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref string lpBuffer, int nSize, ref IntPtr Arguments);

            /// <summary>Formats and returns a .NET string containing the Windows API level error message corresponding to the specified error code</summary>
            public static string GetErrorMessage(int errorCode)
            {
                const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
                const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
                const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

                int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
                int messageSize = 255;
                string lpMsgBuf = "";
                IntPtr ptrlpSource = IntPtr.Zero;
                IntPtr prtArguments = IntPtr.Zero;

                if (FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0, ref lpMsgBuf, messageSize, ref prtArguments) == 0)
                    throw new InvalidOperationException("Failed to format message for error code " + errorCode);

                return lpMsgBuf;
            }
        }
    }
}
