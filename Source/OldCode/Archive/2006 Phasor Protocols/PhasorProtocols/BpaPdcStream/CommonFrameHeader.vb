'*******************************************************************************************************
'  CommonFrameHeader.vb - BPA PDCstream Common frame header functions
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports TVA.DateTime
Imports TVA.DateTime.Common
Imports PhasorProtocols.Common
Imports PhasorProtocols.IeeeC37_118.Common

Namespace BpaPdcStream

    ' This class generates and parses a frame header specfic to BPA PDCstream
    <CLSCompliant(False), Serializable()> _
    Public NotInheritable Class CommonFrameHeader

#Region " Internal Common Frame Header Instance Class "

        ' This class is used to temporarily hold parsed frame header
        Private Class CommonFrameHeaderInstance

            Implements ICommonFrameHeader

            Private m_packetFlag As Byte
            Private m_wordCount As Int16
            Private m_idCode As UInt16
            Private m_sampleNumber As Int16
            Private m_ticks As Long
            Private m_attributes As Dictionary(Of String, String)
            Private m_tag As Object

            Public Sub New()

            End Sub

            Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

                Throw New NotImplementedException()

            End Sub

            Public ReadOnly Property This() As IChannel Implements IChannel.This
                Get
                    Return Me
                End Get
            End Property

            Private ReadOnly Property IFrameThis() As Measurements.IFrame Implements Measurements.IFrame.This
                Get
                    Return Me
                End Get
            End Property

            Public ReadOnly Property DerivedType() As System.Type Implements IChannel.DerivedType
                Get
                    Return Me.GetType()
                End Get
            End Property

            Public ReadOnly Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
                Get
                    Return IIf(m_packetFlag = BpaPdcStream.FrameType.ConfigurationFrame, BpaPdcStream.FrameType.ConfigurationFrame, BpaPdcStream.FrameType.DataFrame)
                End Get
            End Property

            Public ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements IChannelFrame.FrameType, ICommonFrameHeader.FundamentalFrameType
                Get
                    ' Translate BPA PDCstream specific frame type to fundamental frame type
                    Select Case FrameType
                        Case BpaPdcStream.FrameType.ConfigurationFrame
                            Return FundamentalFrameType.ConfigurationFrame
                        Case BpaPdcStream.FrameType.DataFrame
                            Return FundamentalFrameType.DataFrame
                        Case Else
                            Return FundamentalFrameType.Undetermined
                    End Select
                End Get
            End Property

            Public ReadOnly Property FrameLength() As Short Implements ICommonFrameHeader.FrameLength
                Get
                    Return 2 * m_wordCount
                End Get
            End Property

            Public Property PacketFlag() As Byte Implements ICommonFrameHeader.PacketNumber
                Get
                    Return m_packetFlag
                End Get
                Set(ByVal value As Byte)
                    m_packetFlag = value
                End Set
            End Property

            Public Property WordCount() As Int16 Implements ICommonFrameHeader.WordCount
                Get
                    Return m_wordCount
                End Get
                Set(ByVal value As Int16)
                    m_wordCount = value
                End Set
            End Property

            Public Property SampleNumber() As Int16 Implements ICommonFrameHeader.SampleNumber
                Get
                    Return m_sampleNumber
                End Get
                Set(ByVal value As Int16)
                    m_sampleNumber = value
                End Set
            End Property

            Public Property IDCode() As UInt16 Implements IChannelFrame.IDCode
                Get
                    Return m_idCode
                End Get
                Set(ByVal value As UInt16)
                    m_idCode = value
                End Set
            End Property

            Public Property Ticks() As Long Implements IChannelFrame.Ticks
                Get
                    Return m_ticks
                End Get
                Set(ByVal value As Long)
                    m_ticks = value
                End Set
            End Property

            Public Property StartSortTime() As Long Implements TVA.Measurements.IFrame.StartSortTime
                Get
                    Return 0
                End Get
                Set(ByVal value As Long)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Property LastSortTime() As Long Implements TVA.Measurements.IFrame.LastSortTime
                Get
                    Return 0
                End Get
                Set(ByVal value As Long)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Private ReadOnly Property IBinaryDataProviderBinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
                Get
                    Return 0
                End Get
            End Property

            Public ReadOnly Property BinaryLength() As UInt16 Implements IChannel.BinaryLength
                Get
                    Return 0
                End Get
            End Property

            Public Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer) Implements IChannel.ParseBinaryImage

                Throw New NotImplementedException()

            End Sub

            Public ReadOnly Property Cells() As Object Implements IChannelFrame.Cells
                Get
                    Return Nothing
                End Get
            End Property

            Public Property Published() As Boolean Implements IChannelFrame.Published
                Get
                    Return False
                End Get
                Set(ByVal value As Boolean)
                    Throw New NotImplementedException()
                End Set
            End Property

            ' This frame is not complete - it only represents the parsed common "header" for frames
            Public ReadOnly Property IsPartial() As Boolean Implements IChannelFrame.IsPartial
                Get
                    Return True
                End Get
            End Property

            Public ReadOnly Property Timestamp() As Date Implements IChannelFrame.Timestamp
                Get
                    Return New Date(m_ticks)
                End Get
            End Property

            Public ReadOnly Property TimeTag() As UnixTimeTag Implements IChannelFrame.TimeTag
                Get
                    Return New UnixTimeTag(Timestamp)
                End Get
            End Property

            Public Overloads Function Equals(ByVal other As TVA.Measurements.IFrame) As Boolean Implements System.IEquatable(Of Measurements.IFrame).Equals

                Return (CompareTo(other) = 0)

            End Function

            Public Function CompareTo(ByVal other As TVA.Measurements.IFrame) As Integer Implements System.IComparable(Of Measurements.IFrame).CompareTo

                Return m_ticks.CompareTo(other.Ticks)

            End Function

            Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

                Dim other As Measurements.IFrame = TryCast(obj, Measurements.IFrame)
                If other IsNot Nothing Then Return CompareTo(other)
                Throw New ArgumentException("Frame can only be compared with other IFrames...")

            End Function

            Private Function IFrameClone() As TVA.Measurements.IFrame Implements TVA.Measurements.IFrame.Clone

                Return Me

            End Function

            Private ReadOnly Property IFrameMeasurements() As IDictionary(Of Measurements.MeasurementKey, Measurements.IMeasurement) Implements Measurements.IFrame.Measurements
                Get
                    Throw New NotImplementedException()
                End Get
            End Property

            Private Property IFramePublishedMeasurements() As Integer Implements Measurements.IFrame.PublishedMeasurements
                Get
                    Return 0
                End Get
                Set(ByVal value As Integer)
                    Throw New NotImplementedException()
                End Set
            End Property

            Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData

                Throw New NotImplementedException()

            End Sub

            Public ReadOnly Property Attributes() As Dictionary(Of String, String) Implements IChannel.Attributes
                Get
                    ' Create a new attributes dictionary or clear the contents of any existing one
                    If m_attributes Is Nothing Then
                        m_attributes = New Dictionary(Of String, String)
                    Else
                        m_attributes.Clear()
                    End If

                    m_attributes.Add("Derived Type", DerivedType.Name)
                    m_attributes.Add("Binary Length", BinaryLength)
                    m_attributes.Add("Total Cells", "0")
                    m_attributes.Add("Fundamental Frame Type", FundamentalFrameType & ": " & [Enum].GetName(GetType(FundamentalFrameType), FundamentalFrameType))
                    m_attributes.Add("ID Code", IDCode)
                    m_attributes.Add("Is Partial Frame", IsPartial)
                    m_attributes.Add("Published", Published)
                    m_attributes.Add("Ticks", Ticks)
                    m_attributes.Add("Timestamp", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                    m_attributes.Add("Frame Type", FrameType & ": " & [Enum].GetName(GetType(FrameType), FrameType))
                    m_attributes.Add("Frame Length", FrameLength)
                    m_attributes.Add("Packet Flag", m_packetFlag)
                    m_attributes.Add("Word Count", m_wordCount)
                    m_attributes.Add("Sample Number", m_sampleNumber)

                    Return m_attributes
                End Get
            End Property

            Public Property Tag() As Object Implements IChannel.Tag
                Get
                    Return m_tag
                End Get
                Set(ByVal value As Object)
                    m_tag = value
                End Set
            End Property

        End Class

#End Region

        Public Const BinaryLength As UInt16 = 4

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Note: in order to parse timestamp from data frame, this parse procedure needs six more bytes above and beyond common frame header binary length
        Public Shared Function ParseBinaryImage(ByVal configurationFrame As ConfigurationFrame, ByVal parseWordCountFromByte As Boolean, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As ICommonFrameHeader

            If binaryImage(startIndex) <> SyncByte Then Throw New InvalidOperationException("Bad data stream, expected sync byte AA as first byte in BPA PDCstream frame, got " & binaryImage(startIndex).ToString("X"c).PadLeft(2, "0"c))

            With New CommonFrameHeaderInstance
                ' Parse out packet flags and word count information...
                .PacketFlag = binaryImage(startIndex + 1)

                ' Some older streams have a bad word count (e.g., the NYISO data stream has a 0x01 as the third byte
                ' in the stream - this should be a 0x00 to make the word count come out correctly).  The following
                ' compensates for this erratic behavior
                If parseWordCountFromByte Then
                    .WordCount = Convert.ToInt16(binaryImage(startIndex + 3))
                Else
                    .WordCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
                End If

                If .FrameType = FrameType.ConfigurationFrame Then
                    ' We just assume current timestamp for configuration frames since they don't provide one
                    .Ticks = Date.UtcNow.Ticks
                Else
                    ' Next six bytes in data frame is the timestamp - so we go ahead and get it
                    Dim secondOfCentury As UInt32 = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 4)
                    .SampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)

                    If configurationFrame Is Nothing Then
                        ' Until configuration is available, we make a guess at time tag type - this will just be
                        ' used for display purposes until a configuration frame arrives.  If second of century
                        ' is greater than 3155673600 (SOC value for NTP timestamp 1/1/2007), then this is likely
                        ' an NTP time stamp (else this is a Unix time tag for the year 2069 - not likely).
                        If secondOfCentury > 3155673600 Then
                            .Ticks = (New NtpTimeTag(secondOfCentury)).ToDateTime().Ticks
                        Else
                            .Ticks = (New UnixTimeTag(secondOfCentury)).ToDateTime().Ticks
                        End If
                    Else
                        If configurationFrame.RevisionNumber = RevisionNumber.Revision0 Then
                            .Ticks = (New NtpTimeTag(secondOfCentury)).ToDateTime().Ticks + _
                                (.SampleNumber * configurationFrame.TicksPerFrame)
                        Else
                            .Ticks = (New UnixTimeTag(secondOfCentury)).ToDateTime().Ticks + _
                                (.SampleNumber * configurationFrame.TicksPerFrame)
                        End If
                    End If
                End If

                Return .This
            End With

        End Function

        Public Shared Function BinaryImage(ByVal frameHeader As ICommonFrameHeader) As Byte()

            Dim buffer As Byte() = CreateArray(Of Byte)(BinaryLength)

            buffer(0) = SyncByte
            buffer(1) = frameHeader.PacketNumber
            EndianOrder.BigEndian.CopyBytes(frameHeader.WordCount, buffer, 2)

            Return buffer

        End Function

        Public Shared Sub Clone(ByVal sourceFrameHeader As ICommonFrameHeader, ByVal destinationFrameHeader As ICommonFrameHeader)

            destinationFrameHeader.PacketNumber = sourceFrameHeader.PacketNumber
            destinationFrameHeader.WordCount = sourceFrameHeader.WordCount
            destinationFrameHeader.Ticks = sourceFrameHeader.Ticks
            destinationFrameHeader.SampleNumber = sourceFrameHeader.SampleNumber

        End Sub

    End Class

End Namespace
