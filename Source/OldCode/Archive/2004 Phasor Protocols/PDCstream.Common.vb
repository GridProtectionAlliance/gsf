'***********************************************************************
'  PDCstream.Common.vb - Common Structures for the PDCstream format
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace PDCstream

    Public Class Common

        Private Sub New()

            ' This is a shared function class not meant for instantiation

        End Sub

        Friend Const Bit0 As Byte = &H1     ' 00000001 = 1
        Friend Const Bit1 As Byte = &H2     ' 00000010 = 2
        Friend Const Bit2 As Byte = &H4     ' 00000100 = 4
        Friend Const Bit3 As Byte = &H8     ' 00001000 = 8
        Friend Const Bit4 As Byte = &H10    ' 00010000 = 16
        Friend Const Bit5 As Byte = &H20    ' 00100000 = 32
        Friend Const Bit6 As Byte = &H40    ' 01000000 = 64
        Friend Const Bit7 As Byte = &H80    ' 10000000 = 128

        <Flags()> _
        Public Enum ChannelFlags As Byte
            DataIsValid = Bit0              ' Valid if not set (yes = 0)
            TransmissionErrors = Bit1       ' Errors if set (yes = 1)
            PMUSynchronized = Bit2          ' Not sync'd if set (yes = 0)
            DataSortedByArrival = Bit3      ' Data out of sync if set (yes = 1)
            DataSortedByTimestamp = Bit4    ' Sorted by timestamp if not set (yes = 0)
            PDCExchangeFormat = Bit5        ' PDC format if set (yes = 1)
            MacrodyneFormat = Bit6          ' Macrodyne or IEEE format (Macrodyne = 1)
            TimestampIncluded = Bit7        ' Timestamp included if not set (yes = 0)
        End Enum

        <Flags()> _
        Public Enum PMUStatusFlags As Byte
            SyncInvalid = Bit0
            DataInvalid = Bit1
        End Enum

        Public Shared Function XorCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Int16

            Dim sum As Int16

            ' Word length XOR check-sum
            For x As Integer = 0 To length - 1 Step 2
                sum = sum Xor BitConverter.ToInt16(data, startIndex + x)
            Next

            Return sum

        End Function

    End Class

End Namespace
