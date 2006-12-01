'*******************************************************************************************************
'  ProcessEvent.vb - Most basic data element in DatAWare
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
'  05/03/2006 - J. Ritchie Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

' This is the most basic form of a point of data in DatAWare (used by ReadEvent and ReadRange)
Public Class ProcessEvent

    Implements IComparable

    Public TimeTag As TimeTag
    Public QualityBits As Integer
    Public Value As Single

    ' ***************************************
    ' *  Bit usage of IntEvent.Quality word
    ' *
    ' *    Bits 0-4 (data quality indicator, a number between 0 and 31, 5 bits.
    ' *              Maps to the same qualities as used by PMS process computer.)
    ' *
    ' *             mask = &H1F
    ' *
    ' *    Bits 5-10 (index of time-zone used, number between 0 and 51, 6 bits)
    ' *
    ' *             mask = &H7E0
    ' *
    ' *    Bit 11 (Flag for Daylight Savings Time, one bit.  When set, indicates
    ' *            DST is in effect.  When clear, Standard Time.
    ' *
    ' *             mask = &H800
    ' *
    ' ***************************************

    Public Const BinaryLength As Integer = 16
    Private Const QualityMask As Integer = &H1F

    Public Sub New(ByVal timeTag As TimeTag, ByVal value As Single, ByVal quality As Quality)

        Me.TimeTag = timeTag
        Me.Value = value
        Me.Quality = quality
        QualityBits = -1    ' A quality set to -1 tells Archiver to perform limit checking

    End Sub

    Public Sub New(ByVal timestamp As Date, ByVal value As Single, ByVal quality As Quality)

        Me.New(New TimeTag(timestamp), value, quality)

    End Sub

    Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        If binaryImage Is Nothing Then
            Throw New ArgumentNullException("BinaryImage was null - could not create DatAWare.ProcessEvent")
        ElseIf binaryImage.Length - startIndex < BinaryLength Then
            Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create DatAWare.ProcessEvent")
        Else
            Me.TimeTag = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex))
            Me.QualityBits = BitConverter.ToInt32(binaryImage, startIndex + 8)
            Me.Value = BitConverter.ToSingle(binaryImage, startIndex + 12)
        End If

    End Sub

    Public Property Quality() As Quality
        Get
            Return CType((QualityBits And QualityMask), Quality)
        End Get
        Set(ByVal value As Quality)
            QualityBits = (QualityBits Or value)
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BinaryLength)

            ' Construct the binary IP buffer for this event
            Array.Copy(BitConverter.GetBytes(TimeTag.Value), 0, buffer, 0, 8)
            Array.Copy(BitConverter.GetBytes(QualityBits), 0, buffer, 8, 4)
            Array.Copy(BitConverter.GetBytes(Value), 0, buffer, 12, 4)

            Return buffer
        End Get
    End Property

    ' Process events are sorted in TimeTag order
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is ProcessEvent Then
            Return TimeTag.CompareTo(DirectCast(obj, ProcessEvent).TimeTag)
        Else
            Throw New ArgumentException("ProcessEvent can only be compared with other ProcessEvents")
        End If

    End Function

End Class
