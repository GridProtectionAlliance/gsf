'*******************************************************************************************************
'  ChannelFrameBase.vb - Channel data frame base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer
Imports Tva.DateTime
Imports Tva.IO.Compression.Common
Imports Tva.Phasors.Common
Imports Tva.Interop
Imports Tva.Measurements

' This class represents the protocol independent common implementation of any frame of data that can be sent or received from a PMU.
Public MustInherit Class ChannelFrameBase(Of T As IChannelCell)

    Inherits ChannelBase
    Implements IChannelFrame(Of T), IFrame, IComparable

    Private m_idCode As Int16
    Private m_cells As IChannelCellCollection(Of T)
    Private m_timeTag As UnixTimeTag
    Private m_milliseconds As Double
    Private m_synchronizationIsValid As Boolean
    Private m_dataIsValid As Boolean
    Private m_published As Boolean
    Private m_frameLength As Int16

    Protected Sub New(ByVal cells As IChannelCellCollection(Of T))

        MyBase.New()

        m_cells = cells
        m_timeTag = New UnixTimeTag(Date.Now)
        m_synchronizationIsValid = True
        m_dataIsValid = True

    End Sub

    Protected Sub New(ByVal cells As IChannelCellCollection(Of T), ByVal timeTag As UnixTimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal idCode As Int16)

        MyBase.New()

        m_cells = cells
        m_timeTag = timeTag
        m_milliseconds = milliseconds
        m_synchronizationIsValid = synchronizationIsValid
        m_dataIsValid = dataIsValid
        m_idCode = idCode

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal state As IChannelFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    Protected Sub New(ByVal state As IChannelFrameParsingState(Of T), ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(state.Cells)
        ParsedFrameLength = state.FrameLength
        ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelFrame As IChannelFrame)
    Protected Sub New(ByVal channelFrame As IChannelFrame(Of T))

        MyClass.New(channelFrame.Cells, channelFrame.TimeTag, channelFrame.Milliseconds, channelFrame.SynchronizationIsValid, channelFrame.DataIsValid, channelFrame.IDCode)

    End Sub

    Public Overridable ReadOnly Property Cells() As IChannelCellCollection(Of T) Implements IChannelFrame(Of T).Cells
        Get
            Return m_cells
        End Get
    End Property

    Public Overridable Property IDCode() As Int16 Implements IChannelFrame(Of T).IDCode
        Get
            Return m_idCode
        End Get
        Set(ByVal value As Int16)
            m_idCode = value
        End Set
    End Property

    Public Property Ticks() As Long Implements IFrame.Ticks
        Get
            Return m_timeTag.ToDateTime.Ticks
        End Get
        Set(ByVal value As Long)
            m_timeTag = New UnixTimeTag(New Date(value))
        End Set
    End Property

    Public Overridable Property TimeTag() As UnixTimeTag Implements IChannelFrame(Of T).TimeTag
        Get
            Return m_timeTag
        End Get
        Set(ByVal value As UnixTimeTag)
            m_timeTag = value
        End Set
    End Property

    Public Overridable Property NtpTimeTag() As NtpTimeTag Implements IChannelFrame(Of T).NtpTimeTag
        Get
            Return New NtpTimeTag(m_timeTag.ToDateTime)
        End Get
        Set(ByVal value As NtpTimeTag)
            m_timeTag = New UnixTimeTag(value.ToDateTime)
        End Set
    End Property

    Public Overridable Property Milliseconds() As Double Implements IChannelFrame(Of T).Milliseconds
        Get
            Return m_milliseconds
        End Get
        Set(ByVal value As Double)
            m_milliseconds = value
        End Set
    End Property

    Public Overridable ReadOnly Property Timestamp() As Date Implements IChannelFrame(Of T).Timestamp, IFrame.Timestamp
        Get
            Return TimeTag.ToDateTime.AddMilliseconds(Milliseconds)
        End Get
    End Property

    Public Overridable Property SynchronizationIsValid() As Boolean Implements IChannelFrame(Of T).SynchronizationIsValid
        Get
            Return m_synchronizationIsValid
        End Get
        Set(ByVal value As Boolean)
            m_synchronizationIsValid = value
        End Set
    End Property

    Public Overridable Property DataIsValid() As Boolean Implements IChannelFrame(Of T).DataIsValid
        Get
            Return m_dataIsValid
        End Get
        Set(ByVal value As Boolean)
            m_dataIsValid = value
        End Set
    End Property

    Public Overridable Property Published() As Boolean Implements IChannelFrame(Of T).Published, IFrame.Published
        Get
            Return m_published
        End Get
        Set(ByVal value As Boolean)
            m_published = value
        End Set
    End Property

    Protected WriteOnly Property ParsedFrameLength() As Int16
        Set(ByVal value As Int16)
            m_frameLength = value
        End Set
    End Property

    ' We override normal binary length so we can extend length to include check-sum
    ' Also - if frame length was parsed from stream header - we use that length
    ' instead of the calculated length...
    Public Overrides ReadOnly Property BinaryLength() As Int16
        Get
            If m_frameLength > 0 Then
                Return m_frameLength
            Else
                Return 2 + MyBase.BinaryLength
            End If
        End Get
    End Property

    ' We override normal binary image to include check-sum
    Public Overrides ReadOnly Property BinaryImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
            Dim index As Integer

            ' Copy in base image
            CopyImage(MyBase.BinaryImage, buffer, index, MyBase.BinaryLength)

            ' Add check sum
            AppendChecksum(buffer, index)

            Return buffer
        End Get
    End Property

    ' We override normal binary image parser to validate check-sum
    Protected Overrides Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        ' Validate checksum
        If Not ChecksumIsValid(binaryImage, startIndex) Then Throw New InvalidOperationException("Invalid binary image detected - check sum of " & InheritedType.FullName & " did not match")

        ' Perform regular data parse
        MyBase.ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    Protected Overrides ReadOnly Property BodyLength() As Int16
        Get
            Return Cells.BinaryLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Return Cells.BinaryImage
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        ' Parse all frame cells
        With DirectCast(state, IChannelFrameParsingState)
            For x As Integer = 0 To .CellCount - 1
                ' Note: Final derived frame cell classes *must* expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
                '       ' TODO: Remove manual cell constructor....
                '       Cells.Add(New IEEEC37_118.ConfigurationCell(Me, state, x, binaryImage, startIndex))
                Cells.Add(Activator.CreateInstance(.CellType, New Object() {Me, state, x, binaryImage, startIndex}))

                startIndex += Cells(x).BinaryLength
            Next
        End With

    End Sub

    Protected Overridable Function ChecksumIsValid(ByVal buffer As Byte(), ByVal startIndex As Integer) As Boolean

        ' TODO: We need read FrameLength here - not calculated binary length
        Dim length As Int16 = BinaryLength

#If DEBUG Then
            Dim bufferSum As UInt16 = EndianOrder.BigEndian.ToInt16(buffer, startIndex + length - 2)
            Dim calculatedSum As UInt16 = CalculateChecksum(buffer, startIndex, length - 2)
            Debug.WriteLine("Buffer Sum = " & bufferSum & ", Calculated Sum = " & calculatedSum)
            Return (bufferSum = calculatedSum)
#Else
        Return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + length - 2) = CalculateChecksum(buffer, startIndex, length - 2)
#End If

    End Function

    Protected Overridable Sub AppendChecksum(ByVal buffer As Byte(), ByVal startIndex As Integer)

        EndianOrder.BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

    End Sub

    <CLSCompliant(False)> _
    Protected Overridable Function CalculateChecksum(ByVal buffer As Byte(), ByVal offset As Integer, ByVal length As Integer) As UInt16

        Return CRC_CCITT(UInt16.MaxValue, buffer, offset, length)
        'Return CRC16(-1, buffer, offset, length)

    End Function

    ' We sort frames by timetag
    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

        If TypeOf obj Is IChannelFrame(Of T) Then
            Return m_timeTag.CompareTo(DirectCast(obj, IChannelFrame(Of T)).TimeTag)
        Else
            Throw New ArgumentException(InheritedType.Name & " can only be compared with other IChannelFrames...")
        End If

    End Function

    Public ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, IMeasurement) Implements IFrame.Measurements
        Get
            Throw New NotImplementedException("Not implemented for now...")
        End Get
    End Property

    Private ReadOnly Property IFrameThis() As IFrame Implements IFrame.This
        Get
            Return Me
        End Get
    End Property

End Class
