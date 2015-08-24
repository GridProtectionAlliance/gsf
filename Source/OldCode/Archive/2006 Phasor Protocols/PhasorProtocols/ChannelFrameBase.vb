'*******************************************************************************************************
'  ChannelFrameBase.vb - Channel data frame base class
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.Buffer
Imports TVA.DateTime
Imports TVA.DateTime.Common
Imports TVA.IO.Compression.Common
Imports PhasorProtocols.Common
Imports TVA.Measurements

''' <summary>This class represents the protocol independent common implementation of any frame of data that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelFrameBase(Of T As IChannelCell)

    Inherits ChannelBase
    Implements IChannelFrame

    Private m_idCode As UInt16
    Private m_cells As IChannelCellCollection(Of T)
    Private m_ticks As Long
    Private m_published As Boolean
    Private m_publishedMeasurements As Integer
    Private m_parsedBinaryLength As UInt16
    Private m_measurements As Dictionary(Of MeasurementKey, IMeasurement)
    Private m_startSortTime As Long
    Private m_lastSortTime As Long
    Private m_lastSortedMeasurement As IMeasurement

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        ' Deserialize key frame elements...
        m_idCode = info.GetUInt16("idCode")
        m_cells = info.GetValue("cells", GetType(IChannelCellCollection(Of T)))
        m_ticks = info.GetInt64("ticks")

    End Sub

    Protected Sub New(ByVal cells As IChannelCellCollection(Of T))

        m_cells = cells
        m_ticks = Date.UtcNow.Ticks

    End Sub

    Protected Sub New(ByVal idCode As UInt16, ByVal cells As IChannelCellCollection(Of T), ByVal ticks As Long)

        m_idCode = idCode
        m_cells = cells
        m_ticks = ticks

    End Sub

    Protected Sub New(ByVal idCode As UInt16, ByVal cells As IChannelCellCollection(Of T), ByVal timeTag As UnixTimeTag)

        MyClass.New(idCode, cells, timeTag.ToDateTime.Ticks)

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal state As IChannelFrameParsingState(Of T), ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    Protected Sub New(ByVal state As IChannelFrameParsingState(Of T), ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyClass.New(state.Cells)
        ParsedBinaryLength = state.ParsedBinaryLength
        ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelFrame As IChannelFrame)
    Protected Sub New(ByVal channelFrame As IChannelFrame)

        MyClass.New(channelFrame.IDCode, channelFrame.Cells, channelFrame.Ticks)

    End Sub

    Protected MustOverride ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements IChannelFrame.FrameType

    Protected Overridable ReadOnly Property Cells() As IChannelCellCollection(Of T)
        Get
            Return m_cells
        End Get
    End Property

    Private ReadOnly Property IChannelFrameCells() As Object Implements IChannelFrame.Cells
        Get
            Return m_cells
        End Get
    End Property

    Public Overridable ReadOnly Property Measurements() As IDictionary(Of MeasurementKey, IMeasurement) Implements IFrame.Measurements
        Get
            If m_measurements Is Nothing Then m_measurements = New Dictionary(Of MeasurementKey, IMeasurement)
            Return m_measurements
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

    Public Overridable Property Ticks() As Long Implements IChannelFrame.Ticks
        Get
            Return m_ticks
        End Get
        Set(ByVal value As Long)
            m_ticks = value
        End Set
    End Property

    Private ReadOnly Property IFrameThis() As IFrame Implements IFrame.This
        Get
            Return Me
        End Get
    End Property

    Private Function IFrameClone() As IFrame Implements IFrame.Clone

        ' We don't need to make a "clone" of the measurements in this frame since phasor concentration
        ' handles measurement assignment in a special way - so we just return ourself for publication
        Return Me

    End Function

    Public Property StartSortTime() As Long Implements IFrame.StartSortTime
        Get
            Return m_startSortTime
        End Get
        Set(ByVal value As Long)
            m_startSortTime = value
        End Set
    End Property

    Public Property LastSortTime() As Long Implements IFrame.LastSortTime
        Get
            Return m_lastSortTime
        End Get
        Set(ByVal value As Long)
            m_lastSortTime = value
        End Set
    End Property

    Public Property LastSortedMeasurement() As TVA.Measurements.IMeasurement Implements TVA.Measurements.IFrame.LastSortedMeasurement
        Get
            Return m_lastSortedMeasurement
        End Get
        Set(ByVal value As TVA.Measurements.IMeasurement)
            m_lastSortedMeasurement = value
        End Set
    End Property

    Public Overridable ReadOnly Property TimeTag() As UnixTimeTag Implements IChannelFrame.TimeTag
        Get
            Return New UnixTimeTag(Timestamp)
        End Get
    End Property

    Public Overridable ReadOnly Property Timestamp() As Date Implements IChannelFrame.Timestamp
        Get
            Return New Date(m_ticks)
        End Get
    End Property

    Public Overridable Property Published() As Boolean Implements IChannelFrame.Published
        Get
            Return m_published
        End Get
        Set(ByVal value As Boolean)
            m_published = value
        End Set
    End Property

    Public Property PublishedMeasurements() As Integer Implements IFrame.PublishedMeasurements
        Get
            Return m_publishedMeasurements
        End Get
        Set(ByVal value As Integer)
            m_publishedMeasurements = value
        End Set
    End Property

    Public Overridable ReadOnly Property IsPartial() As Boolean Implements IChannelFrame.IsPartial
        Get
            Return False
        End Get
    End Property

    Protected Overridable WriteOnly Property ParsedBinaryLength() As UInt16
        Set(ByVal value As UInt16)
            m_parsedBinaryLength = value
        End Set
    End Property

    ' We override normal binary length so we can extend length to include check-sum
    ' Also - if frame length was parsed from stream header - we use that length
    ' instead of the calculated length...
    Public Overrides ReadOnly Property BinaryLength() As UInt16
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
            Dim buffer As Byte() = CreateArray(Of Byte)(BinaryLength)
            Dim index As Int32

            ' Copy in base image
            CopyImage(MyBase.BinaryImage, buffer, index, MyBase.BinaryLength)

            ' Add check sum
            AppendChecksum(buffer, index)

            Return buffer
        End Get
    End Property

    ' We override normal binary image parser to validate check-sum
    Protected Overrides Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        ' Validate checksum
        If Not ChecksumIsValid(binaryImage, startIndex) Then Throw New InvalidOperationException("Invalid binary image detected - check sum of " & DerivedType.Name & " did not match")

        ' Perform regular data parse
        MyBase.ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return m_cells.BinaryLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Return m_cells.BinaryImage
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        ' Parse all frame cells
        With DirectCast(state, IChannelFrameParsingState(Of T))
            For x As Int32 = 0 To .CellCount - 1
                m_cells.Add(.CreateNewCellFunction.Invoke(Me, state, x, binaryImage, startIndex))
                startIndex += m_cells.Item(x).BinaryLength
            Next
        End With

    End Sub

    Protected Overridable Function ChecksumIsValid(ByVal buffer As Byte(), ByVal startIndex As Int32) As Boolean

        Dim sumLength As Int16 = BinaryLength - 2
        Return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) = CalculateChecksum(buffer, startIndex, sumLength)

    End Function

    Protected Overridable Sub AppendChecksum(ByVal buffer As Byte(), ByVal startIndex As Int32)

        EndianOrder.BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

    End Sub

    Protected Overridable Function CalculateChecksum(ByVal buffer As Byte(), ByVal offset As Int32, ByVal length As Int32) As UInt16

        ' We implement CRC CCITT check sum as the default, but each protocol can override as necessary
        Return CRC_CCITT(UInt16.MaxValue, buffer, offset, length)

    End Function

    ' We sort frames by timestamp
    Public Function CompareTo(ByVal other As Measurements.IFrame) As Integer Implements System.IComparable(Of Measurements.IFrame).CompareTo

        Return m_ticks.CompareTo(other.Ticks)

    End Function

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        Dim other As Measurements.IFrame = TryCast(obj, Measurements.IFrame)
        If other IsNot Nothing Then Return CompareTo(other)
        Throw New ArgumentException("Frame can only be compared with other IFrames...")

    End Function

    Public Overloads Function Equals(ByVal other As Measurements.IFrame) As Boolean Implements System.IEquatable(Of Measurements.IFrame).Equals

        Return (CompareTo(other) = 0)

    End Function

    Public Overridable Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData

        ' Add key frame elements for serialization...
        info.AddValue("idCode", m_idCode)
        info.AddValue("cells", m_cells, GetType(IChannelCellCollection(Of T)))
        info.AddValue("ticks", m_ticks)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Total Cells", Cells.Count)
            baseAttributes.Add("Fundamental Frame Type", FundamentalFrameType & ": " & [Enum].GetName(GetType(FundamentalFrameType), FundamentalFrameType))
            baseAttributes.Add("ID Code", IDCode)
            baseAttributes.Add("Is Partial Frame", IsPartial)
            baseAttributes.Add("Published", Published)
            baseAttributes.Add("Ticks", Ticks)
            baseAttributes.Add("Timestamp", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))

            Return baseAttributes
        End Get
    End Property

End Class