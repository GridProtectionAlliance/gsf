'*******************************************************************************************************
'  TVA.Interop.WindowsApi.vb - Common Windows API Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/24/2006 - J. Ritchie Carroll
'       Initial version of source created
'
'*******************************************************************************************************

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>Defines common Windows API functions</summary>
    Public NotInheritable Class WindowsApi

        <DllImport("kernel32.dll")> _
        Private Shared Function FormatMessage(ByVal dwFlags As Integer, ByRef lpSource As IntPtr, ByVal dwMessageId As Integer, ByVal dwLanguageId As Integer, ByRef lpBuffer As String, ByVal nSize As Integer, ByRef Arguments As IntPtr) As Integer
        End Function

        ''' <summary>Formats and returns a .NET string containing the Windows API level error message corresponding to the specified error code</summary>
        Public Shared Function GetErrorMessage(ByVal errorCode As Integer) As String

            Const FORMAT_MESSAGE_ALLOCATE_BUFFER As Integer = &H100
            Const FORMAT_MESSAGE_IGNORE_INSERTS As Integer = &H200
            Const FORMAT_MESSAGE_FROM_SYSTEM As Integer = &H1000

            Dim messageSize As Integer = 255
            Dim lpMsgBuf As String = ""
            Dim dwFlags As Integer = FORMAT_MESSAGE_ALLOCATE_BUFFER Or FORMAT_MESSAGE_FROM_SYSTEM Or FORMAT_MESSAGE_IGNORE_INSERTS
            Dim ptrlpSource As IntPtr = IntPtr.Zero
            Dim prtArguments As IntPtr = IntPtr.Zero

            If FormatMessage(dwFlags, ptrlpSource, errorCode, 0, lpMsgBuf, messageSize, prtArguments) = 0 Then
                Throw New InvalidOperationException("Failed to format message for error code " & errorCode)
            End If

            Return lpMsgBuf

        End Function

    End Class

End Namespace
