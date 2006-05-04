'*******************************************************************************************************
'  StandardEvent.vb - Standard DatAWare data element
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/03/2006 - James R Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

' Standard-format data event (i.e., a ProcessEvent + point ID) - used in DatAWare IP packets and DAQ plug-in's
Public Class StandardEvent

    Implements IComparable

    Public DatabaseIndex As Integer
    Public [Event] As ProcessEvent

    Public Const BinaryLength As Integer = ProcessEvent.BinaryLength + 6

    Private Const PacketTypeID As Short = 1 ' We use the standard binary IP buffer format for efficiency
    Private Shared PacketType As Byte()

    Shared Sub New()

        ' TODO: Packet type will be different for non-client level implementations - must fix!
        ' We pre-load the byte array for the packet format type - we do this at a shared instance level
        ' since this will be the same for all events
        PacketType = BitConverter.GetBytes(PacketTypeID)

    End Sub

    Public Sub New(ByVal databaseIndex As Integer, ByVal [event] As ProcessEvent)

        Me.DatabaseIndex = databaseIndex
        Me.Event = [event]

    End Sub

    Public Sub New(ByVal databaseIndex As Integer, ByVal ttag As TimeTag, ByVal value As Single, ByVal qual As Quality)

        Me.New(databaseIndex, New ProcessEvent(ttag, value, qual))

    End Sub

    Public Sub New(ByVal databaseIndex As Integer, ByVal timestamp As DateTime, ByVal value As Single, ByVal valueQuality As Quality)

        Me.New(databaseIndex, New TimeTag(timestamp), value, valueQuality)

    End Sub

    Public Sub New(ByVal databaseIndex As Integer, ByVal timestamp As String, ByVal value As Single, ByVal valueQuality As Quality)

        Me.New(databaseIndex, New TimeTag(timestamp), value, valueQuality)

    End Sub

    Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        If binaryImage Is Nothing Then
            Throw New ArgumentNullException("BinaryImage was null - could not create DatAWare.StandardEvent")
        ElseIf binaryImage.Length - startIndex < BinaryLength Then
            Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create DatAWare.StandardEvent")
        Else
            Dim packetType As Integer = BitConverter.ToInt16(binaryImage, startIndex)

            If packetType <> PacketTypeID Then
                Throw New ArgumentException("Unexcepted binaryImage packet ID type " & packetType & ", expected " & PacketTypeID & " - could not create DatAWare.StandardEvent")
            Else
                Me.DatabaseIndex = BitConverter.ToInt32(binaryImage, startIndex + 2)
                Me.Event = New ProcessEvent(binaryImage, startIndex + 6)
            End If
        End If

    End Sub

    ' For convience, we directly expose the relevant process event properties
    Public Property TTag() As TimeTag
        Get
            Return [Event].TTag
        End Get
        Set(ByVal Value As TimeTag)
            [Event].TTag = Value
        End Set
    End Property

    Public Property Quality() As Quality
        Get
            Return [Event].Quality
        End Get
        Set(ByVal Value As Quality)
            [Event].Quality = Value
        End Set
    End Property

    Public Property Value() As Single
        Get
            Return [Event].Value
        End Get
        Set(ByVal Value As Single)
            [Event].Value = Value
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

            ' Construct the binary IP buffer for this event
            Array.Copy(PacketType, 0, buffer, 0, 2)
            Array.Copy(BitConverter.GetBytes(DatabaseIndex), 0, buffer, 2, 4)
            Array.Copy([Event].BinaryImage, 0, buffer, 6, ProcessEvent.BinaryLength)

            Return buffer
        End Get
    End Property

    ' StandardEvents are sorted in TimeTag order
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is StandardEvent Then
            Return TTag.CompareTo(DirectCast(obj, StandardEvent).TTag)
        Else
            Throw New ArgumentException("StandardEvent can only be compared with other StandardEvents")
        End If

    End Function

End Class
