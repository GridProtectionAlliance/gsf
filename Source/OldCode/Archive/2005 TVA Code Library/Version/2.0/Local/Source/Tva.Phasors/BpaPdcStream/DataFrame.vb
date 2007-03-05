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

        Private m_packetNumber As Byte
        Private m_sampleNumber As Int16

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

        Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, 0, configurationFrame, _
                AddressOf BpaPdcStream.DataCell.CreateNewDataCell), binaryImage, startIndex)

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

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType
            Get
                Return Phasors.FundamentalFrameType.DataFrame
            End Get
        End Property

        Public Property PacketNumber() As Byte
            Get
                Return m_packetNumber
            End Get
            Set(ByVal Value As Byte)
                If Value < 1 Then Throw New ArgumentOutOfRangeException("Data packets must be numbered from 1 to 255")
                m_packetNumber = Value
            End Set
        End Property

        Public Property SampleNumber() As Int16
            Get
                Return m_sampleNumber
            End Get
            Set(ByVal Value As Int16)
                m_sampleNumber = Value
            End Set
        End Property

        Public ReadOnly Property NtpTimeTag() As DateTime.NtpTimeTag
            Get
                Return New DateTime.NtpTimeTag(TicksToSeconds(Ticks))
            End Get
        End Property

        <CLSCompliant(False)> _
        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Int32, ByVal length As Int32) As UInt16

            ' PDCstream uses simple XOR checksum
            Return Xor16BitCheckSum(buffer, offset, length)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                If ConfigurationFrame.StreamType = StreamType.Legacy Then
                    Return 12 + Cells.Count * 8
                Else
                    Return 12
                End If
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)

                buffer(0) = SyncByte
                buffer(1) = Convert.ToByte(1)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
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
            Dim secondOfCentury As UInt32
            Dim frameLength As Int16
            Dim dataCellCount As Int16
            Dim timestamp As Date

            If binaryImage(startIndex) <> SyncByte Then
                Throw New InvalidOperationException("Bad Data Stream: Expected sync byte AA as first byte in PDCstream data frame, got " & binaryImage(startIndex).ToString("X"c).PadLeft(2, "0"c))
            End If

            m_packetNumber = binaryImage(startIndex + 1)

            If m_packetNumber = DescriptorPacketFlag Then
                Throw New InvalidOperationException("Bad Data Stream: This is not a PDCstream data frame - looks like a configuration frame.")
            End If

            frameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2) * 2
            secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 4)
            m_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
            dataCellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)

            If dataCellCount <> configurationFrame.Cells.Count Then
                Throw New InvalidOperationException("Stream/Config File Mismatch: PMU count (" & dataCellCount & ") in stream does not match defined count in configuration file:" & configurationFrame.Cells.Count)
            End If

            If configurationFrame.RevisionNumber = RevisionNumber.Revision0 Then
                timestamp = (New DateTime.UnixTimeTag(secondOfCentury)).ToDateTime()
            Else
                timestamp = (New DateTime.NtpTimeTag(secondOfCentury)).ToDateTime()
            End If

            Ticks = timestamp.AddMilliseconds(m_sampleNumber * (1000@ / configurationFrame.FrameRate)).Ticks

            ' We don't need PMU info in data frame from a parsing perspective, even if available in legacy stream - so we're done...

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

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace