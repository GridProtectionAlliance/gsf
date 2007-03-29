'*******************************************************************************************************
'  Common.vb - Common declarations and functions for phasor classes
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer
Imports TVA.Text.Common

Namespace Phasors

    ''' <summary>Common constants and functions for phasor classes</summary>
    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Typical data stream synchrnonization byte</summary>
        Public Const SyncByte As Byte = &HAA

        ''' <summary>This is a common optimized block copy function for any kind of data</summary>
        ''' <remarks>This function automatically advances index for convenience</remarks>
        Public Shared Sub CopyImage(ByVal channel As IChannel, ByVal buffer As Byte(), ByRef index As Integer)

            CopyImage(channel.BinaryImage, buffer, index, channel.BinaryLength)

        End Sub

        ''' <summary>This is a common optimized block copy function for binary data</summary>
        ''' <remarks>This function automatically advances index for convenience</remarks>
        Public Shared Sub CopyImage(ByVal source As Byte(), ByVal buffer As Byte(), ByRef index As Integer, ByVal length As Integer)

            If length > 0 Then
                BlockCopy(source, 0, buffer, index, length)
                index += length
            End If

        End Sub

        ''' <summary>Removes duplicate white space and control characters from a string</summary>
        ''' <remarks>Strings reported from IED's can be full of inconsistencies, this function "cleans-up" the strings for visualization</remarks>
        Public Shared Function GetValidLabel(ByVal value As String) As String

            Return RemoveDuplicateWhiteSpace(ReplaceControlCharacters(value, " "c)).Trim()

        End Function

        ''' <summary>Returns display friendly protocol name</summary>
        Public Shared Function GetFormattedProtocolName(ByVal protocol As PhasorProtocol) As String

            Select Case protocol
                Case PhasorProtocol.IeeeC37_118V1
                    Return "IEEE C37.118-2005 (Version 1)"
                Case PhasorProtocol.IeeeC37_118D6
                    Return "IEEE C37.118 (Draft 6)"
                Case PhasorProtocol.Ieee1344
                    Return "IEEE 1344-1995"
                Case PhasorProtocol.BpaPdcStream
                    Return "BPA PDCstream"
                Case PhasorProtocol.FNet
                    Return "Virginia Tech FNET"
                Case Else
                    Return [Enum].GetName(GetType(PhasorProtocol), protocol).Replace("_"c, "."c).ToUpper()
            End Select

        End Function

    End Class

End Namespace