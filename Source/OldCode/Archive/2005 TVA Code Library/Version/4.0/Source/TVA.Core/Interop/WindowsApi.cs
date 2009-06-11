//*******************************************************************************************************
//  WindowsApi.cs
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
//  01/24/2006 - J. Ritchie Carroll
//       Initial version of source created
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace TVA.Interop
{
    /// <summary>Defines common Windows API functions</summary>
    public static class WindowsApi
    {
        [DllImport("kernel32.dll")]
        private static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref string lpBuffer, int nSize, ref IntPtr Arguments);

        /// <summary>Formats and returns a .NET string containing the Windows API level error message corresponding to the specified error code.</summary>
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

        /// <summary>Formats and returns a .NET string containing the Windows API level error message from the last Win32 call.</summary>
        public static string GetLastErrorMessage()
        {
            return GetErrorMessage(Marshal.GetLastWin32Error());
        }
    }
}