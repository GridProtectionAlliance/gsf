'*******************************************************************************************************
'  DataPacket.vb - PDCstream data packet
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
Imports System.Buffer
Imports System.Text
Imports Tva.DateTime.Common
Imports Tva.Math.Common
Imports Tva.Phasors.Common
Imports Tva.Phasors.BpaPdcStream.Common

Namespace BpaPdcStream

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False), Serializable()> _
    Public Class DataFrame

        Inherits DataFrameBase
        Implements ICommonFrameHeader

        Private m_packetNumber As Byte
        Private m_sampleNumber As Int16
        Private m_legacyLabels As String()

        Public Sub New()

            MyBase.New(New DataCellCollection)
            m_packetNumber = 1

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize data frame
            m_packetNumber = info.GetByte("packetNumber")
            m_sampleNumber = info.GetInt16("sampleNumber")

        End Sub

        Public Sub New(ByVal sampleNumber As Int16)

            MyClass.New()
            m_sampleNumber = sampleNumber

        End Sub

        ' If you are going to create multiple data packets, you can use this constructor
        ' Note that this only starts becoming necessary if you start hitting data size
        ' limits imposed by the nature of the transport protocol...
        Public Sub New(ByVal packetNumber As Byte, ByVal sampleNumber As Int16)

            MyClass.New(sampleNumber)
            Me.PacketNumber = packetNumber

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, parsedFrameHeader.FrameLength, configurationFrame, _
                AddressOf BpaPdcStream.DataCell.CreateNewDataCell), binaryImage, startIndex)

            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal dataFrame As IDataFrame)

            MyBase.New(dataFrame)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As DataCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Shadows Property ConfigurationFrame() As ConfigurationFrame
            Get
                Return MyBase.ConfigurationFrame
            End Get
            Set(ByVal value As ConfigurationFrame)
                MyBase.ConfigurationFrame = value
            End Set
        End Property

        Public Property PacketNumber() As Byte Implements ICommonFrameHeader.PacketNumber
            Get
                Return m_packetNumber
            End Get
            Set(ByVal value As Byte)
                m_packetNumber = value
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

        Public ReadOnly Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return BpaPdcStream.FrameType.DataFrame
            End Get
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
        End Property

        Public Property WordCount() As Int16 Implements ICommonFrameHeader.WordCount
            Get
                Return MyBase.BinaryLength / 2
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value * 2
            End Set
        End Property

        Public ReadOnly Property FrameLength() As Short Implements ICommonFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
        End Property

        Public ReadOnly Property NtpTimeTag() As DateTime.NtpTimeTag
            Get
                Return New DateTime.NtpTimeTag(TicksToSeconds(Ticks))
            End Get
        End Property

        Public ReadOnly Property LegacyLabels() As String()
            Get
                Return m_legacyLabels
            End Get
        End Property

        <CLSCompliant(False)> _
        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Int32, ByVal length As Int32) As UInt16

            ' PDCstream uses simple XOR checksum
            Return Xor16BitCheckSum(buffer, offset, length)

        End Function

        ' Oddly enough, check sum for frames in BPA PDC stream is little-endian
        Protected Overrides Sub AppendChecksum(ByVal buffer() As Byte, ByVal startIndex As Integer)

            EndianOrder.LittleEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Protected Overrides Function ChecksumIsValid(ByVal buffer() As Byte, ByVal startIndex As Integer) As Boolean

            Dim sumLength As Int16 = BinaryLength - 2
            Return EndianOrder.LittleEndian.ToUInt16(buffer, startIndex + sumLength) = CalculateChecksum(buffer, startIndex, sumLength)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                If ConfigurationFrame.StreamType = StreamType.Legacy Then
                    Return 12 + ConfigurationFrame.Cells.Count * 8
                Else
                    Return 12
                End If
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)

                ' Common in common frame header portion of header image
                System.Buffer.BlockCopy(CommonFrameHeader.BinaryImage(Me), 0, buffer, 0, CommonFrameHeader.BinaryLength)

                If ConfigurationFrame.RevisionNumber = RevisionNumber.Revision0 Then
                    EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(NtpTimeTag.Value), buffer, 4)
                Else
                    EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(TimeTag.Value), buffer, 4)
                End If

                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_sampleNumber), buffer, 8)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 10)

                ' If producing a legacy format, include additional header
                If ConfigurationFrame.StreamType = StreamType.Legacy Then
                    Dim index As Integer = 12
                    Dim reservedBytes As Byte() = CreateArray(Of Byte)(2)
                    Dim offset As Int16

                    For x As Integer = 0 To Cells.Count - 1
                        With Cells(x)
                            CopyImage(Encoding.ASCII.GetBytes(.IDLabel), buffer, index, 4)
                            CopyImage(reservedBytes, buffer, index, 2)
                            EndianOrder.BigEndian.CopyBytes(offset, buffer, index)
                            index += 2
                            offset += .BinaryLength
                        End With
                    Next
                End If

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim configurationFrame As BpaPdcStream.ConfigurationFrame = DirectCast(DirectCast(state, IDataFrameParsingState).ConfigurationFrame, BpaPdcStream.ConfigurationFrame)
            Dim dataCellCount As Int16

            ' Only need to parse what wan't already parsed in common frame header
            dataCellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)

            If dataCellCount <> configurationFrame.Cells.Count Then
                Throw New InvalidOperationException("Stream/Config File Mismatch: PMU count (" & dataCellCount & ") in stream does not match defined count in configuration file (" & configurationFrame.Cells.Count & ")")
            End If

            ' Note: because "HeaderLength" needs configuration frame and is called before associated configuration frame
            ' assignment normally occurs - we assign configuration frame in advance...
            Me.ConfigurationFrame = configurationFrame

            ' We'll at least retrieve legacy labels if defined (might be useful for debugging dynamic changes in data-stream)
            If configurationFrame.StreamType = StreamType.Legacy Then
                Dim index As Integer = 12

                For x As Integer = 0 To configurationFrame.Cells.Count - 1
                    m_legacyLabels = CreateArray(Of String)(configurationFrame.Cells.Count)
                    With configurationFrame.Cells(x)
                        m_legacyLabels(x) = Encoding.ASCII.GetString(binaryImage, index, 4)
                        index += 8
                    End With
                Next
            End If

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize data frame
            info.AddValue("packetNumber", m_packetNumber)
            info.AddValue("sampleNumber", m_sampleNumber)

        End Sub

        Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Packet Number", m_packetNumber)
                baseAttributes.Add("Sample Number", m_sampleNumber)

                If m_legacyLabels IsNot Nothing Then
                    baseAttributes.Add("Legacy Label Count", m_legacyLabels.Length)

                    For x As Integer = 0 To m_legacyLabels.Length - 1
                        baseAttributes.Add("    Legacy Label " & x, m_legacyLabels(x))
                    Next
                End If

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace