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
Imports Tva.DateTime.Common
Imports Tva.IO.Compression.Common
Imports Tva.Phasors.Common
Imports Tva.Interop
Imports Tva.Measurements

' This class represents the protocol independent common implementation of any frame of data that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public MustInherit Class ChannelFrameBase(Of T As IChannelCell)

    Inherits ChannelBase
    Implements IChannelFrame, IFrame, IComparable

    Private m_idCode As UInt16
    Private m_cells As IChannelCellCollection(Of T)
    Private m_ticks As Long
    Private m_published As Boolean
    Private m_parsedBinaryLength As Int16

    Protected Sub New(ByVal cells As IChannelCellCollection(Of T))

        MyBase.New()

        m_cells = cells
        m_ticks = Date.UtcNow.Ticks

    End Sub

    Protected Sub New(ByVal idCode As UInt16, ByVal cells As IChannelCellCollection(Of T), ByVal ticks As Long)

        MyBase.New()

        m_idCode = idCode
        m_cells = cells
        m_ticks = ticks

    End Sub

    Protected Sub New(ByVal idCode As UInt16, ByVal cells As IChannelCellCollection(Of T), ByVal timeTag As UnixTimeTag)

        MyClass.New(idCode, cells, timeTag.ToDateTime.Ticks)

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal state As IChannelFrameParsingState(Of T), ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    Protected Sub New(ByVal state As IChannelFrameParsingState(Of T), ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(state.Cells)
        ParsedBinaryLength = state.ParsedBinaryLength
        ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelFrame As IChannelFrame)
    Protected Sub New(ByVal channelFrame As IChannelFrame)

        MyClass.New(channelFrame.IDCode, channelFrame.Cells, channelFrame.Ticks)

    End Sub

    Public Overridable ReadOnly Property Cells() As IChannelCellCollection(Of IChannelCell) Implements IChannelFrame.Cells
        Get
            Return m_cells
        End Get
    End Property

    Public Overridable Property IDCode() As UInt16 Implements IChannelFrame.IDCode
        Get
            Return m_idCode
        End Get
        Set(ByVal value As UInt16)
            m_idCode = value
        End Set
    End Property

    Public Overridable Property Ticks() As Long Implements IChannelFrame.Ticks, IFrame.Ticks
        Get
            Return m_ticks
        End Get
        Set(ByVal value As Long)
            m_ticks = value
        End Set
    End Property

    Public Overridable ReadOnly Property TimeTag() As UnixTimeTag Implements IChannelFrame.TimeTag
        Get
            Return New UnixTimeTag(TicksToSeconds(m_ticks))
        End Get
    End Property

    Public Overridable ReadOnly Property Timestamp() As Date Implements IChannelFrame.Timestamp, IFrame.Timestamp
        Get
            Return New Date(m_ticks)
        End Get
    End Property

    Public Overridable Property Published() As Boolean Implements IChannelFrame.Published, IFrame.Published
        Get
            Return m_published
        End Get
        Set(ByVal value As Boolean)
            m_published = value
        End Set
    End Property

    Protected WriteOnly Property ParsedBinaryLength() As Int16
        Set(ByVal value As Int16)
            m_parsedBinaryLength = value
        End Set
    End Property

    ' We override normal binary length so we can extend length to include check-sum
    ' Also - if frame length was parsed from stream header - we use that length
    ' instead of the calculated length...
    Public Overrides ReadOnly Property BinaryLength() As Int16
        Get
            If m_parsedBinaryLength > 0 Then
                Return m_parsedBinaryLength
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
        With DirectCast(state, IChannelFrameParsingState(Of T))
            For x As Integer = 0 To .CellCount - 1
                .CreateNewCellFunction.Invoke(Me, state, x, binaryImage, startIndex)
                startIndex += Cells(x).BinaryLength
            Next
        End With

    End Sub

    Protected Overridable Function ChecksumIsValid(ByVal buffer As Byte(), ByVal startIndex As Integer) As Boolean

#If DEBUG Then
        Dim bufferSum As UInt16 = EndianOrder.BigEndian.ToInt16(buffer, startIndex + m_parsedBinaryLength - 2)
        Dim calculatedSum As UInt16 = CalculateChecksum(buffer, startIndex, m_parsedBinaryLength - 2)
        Debug.WriteLine("Buffer Sum = " & bufferSum & ", Calculated Sum = " & calculatedSum)
        Return (bufferSum = calculatedSum)
#Else
        Return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + m_frameLength - 2) = CalculateChecksum(buffer, startIndex, m_frameLength - 2)
#End If

    End Function

    Protected Overridable Sub AppendChecksum(ByVal buffer As Byte(), ByVal startIndex As Integer)

        EndianOrder.BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

    End Sub

    Protected Overridable Function CalculateChecksum(ByVal buffer As Byte(), ByVal offset As Integer, ByVal length As Integer) As UInt16

        ' We implement CRC CCITT check sum as the default, but each protocol can override as necessary
        Return CRC_CCITT(UInt16.MaxValue, buffer, offset, length)

    End Function

    ' We sort frames by timestamp
    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

        If TypeOf obj Is IChannelFrame Then
            Return m_ticks.CompareTo(DirectCast(obj, IChannelFrame).Ticks)
        Else
            Throw New ArgumentException(InheritedType.Name & " can only be compared with other IChannelFrames...")
        End If

    End Function

    Public MustOverride ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, IMeasurement) Implements IFrame.Measurements

    Private ReadOnly Property IFrameThis() As IFrame Implements IFrame.This
        Get
            Return Me
        End Get
    End Property

End Class
