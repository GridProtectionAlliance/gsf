'*******************************************************************************************************
'  Common.vb - Common declarations and functions for phasor classes
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer

Namespace EE.Phasor

    Public Enum PhasorFormat As Byte
        Rectangular
        Polar
    End Enum

    Public Enum PhasorType As Byte
        Voltage
        Current
    End Enum

    Public Enum DataFormat As Byte
        FixedInteger
        FloatingPoint
    End Enum

    Public Enum LineFrequency As Byte
        Hz50
        Hz60
    End Enum

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' This is a common optimized block copy function for any kind of data
        Public Shared Sub CopyImage(ByVal channel As IChannel, ByVal buffer As Byte(), ByRef index As Integer)

            With channel
                CopyImage(.BinaryImage, buffer, index, .BinaryLength)
            End With

        End Sub

        ' This is a common optimized block copy function for any kind of data
        Public Shared Sub CopyImage(ByVal source As Byte(), ByVal buffer As Byte(), ByRef index As Integer, ByVal length As Int16)

            If length > 0 Then
                BlockCopy(source, 0, buffer, index, length)
                index += length
            End If

        End Sub

    End Class

End Namespace