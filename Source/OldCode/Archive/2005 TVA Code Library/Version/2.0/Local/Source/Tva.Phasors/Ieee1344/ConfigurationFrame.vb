'*******************************************************************************************************
'  ConfigurationFrame.vb - IEEE1344 Configuration Frame
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
Imports System.Text
Imports Tva.DateTime
Imports Tva.Phasors.Common
Imports Tva.IO.Compression.Common

Namespace Ieee1344

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase
        Implements ICommonFrameHeader

        Private m_idCode As UInt64
        Private m_sampleCount As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration frame
            m_idCode = info.GetUInt64("idCode64Bit")
            m_sampleCount = info.GetInt16("sampleCount")

        End Sub

        Public Sub New(ByVal frameType As FrameType, ByVal idCode As UInt64, ByVal ticks As Long, ByVal frameRate As Int16)

            MyBase.New(idCode Mod UInt16.MaxValue, New ConfigurationCellCollection, ticks, frameRate)
            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.ConfigurationFrame
            m_idCode = idCode

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, parsedFrameHeader.FrameLength, _
                    AddressOf Ieee1344.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)

            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.ConfigurationFrame
            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)
            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.ConfigurationFrame

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As ConfigurationCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Shadows Property IDCode() As UInt64 Implements ICommonFrameHeader.IDCode
            Get
                Return m_idCode
            End Get
            Set(ByVal value As UInt64)
                m_idCode = value

                ' Base classes constrain maximum value to 65535
                If m_idCode > UInt16.MaxValue Then
                    MyBase.IDCode = UInt16.MaxValue
                Else
                    MyBase.IDCode = Convert.ToUInt16(value)
                End If
            End Set
        End Property

        Public ReadOnly Property FrameLength() As Int16 Implements ICommonFrameHeader.FrameLength
            Get
                Return CommonFrameHeader.FrameLength(Me)
            End Get
        End Property

        Public ReadOnly Property DataLength() As Int16 Implements ICommonFrameHeader.DataLength
            Get
                Return CommonFrameHeader.DataLength(Me)
            End Get
        End Property

        Private Property InternalSampleCount() As Int16 Implements ICommonFrameHeader.InternalSampleCount
            Get
                Return m_sampleCount
            End Get
            Set(ByVal value As Int16)
                m_sampleCount = value
            End Set
        End Property

        ' Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share status flags with our only child
        ' and expose the value at the parent level for convience in determing frame length at the frame level
        Private Property InternalStatusFlags() As Int16 Implements ICommonFrameHeader.InternalStatusFlags
            Get
                Return Cells(0).StatusFlags
            End Get
            Set(ByVal value As Int16)
                Cells(0).StatusFlags = value
            End Set
        End Property

        Public Shadows ReadOnly Property TimeTag() As NtpTimeTag Implements ICommonFrameHeader.TimeTag
            Get
                Return CommonFrameHeader.TimeTag(Me)
            End Get
        End Property

        ' Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share nominal frequency with our only child
        ' and expose the value at the parent level for convience
        Public Property NominalFrequency() As LineFrequency
            Get
                Return Cells(0).NominalFrequency
            End Get
            Set(ByVal value As LineFrequency)
                Cells(0).NominalFrequency = value
            End Set
        End Property

        Public Property Period() As Int16
            Get
                Return NominalFrequency / FrameRate * 100
            End Get
            Set(ByVal value As Int16)
                FrameRate = NominalFrequency * 100 / value
            End Set
        End Property

        Public ReadOnly Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return Ieee1344.FrameType.ConfigurationFrame
            End Get
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
        End Property

        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Int32, ByVal length As Int32) As UInt16

            ' IEEE 1344 uses CRC16 to calculate checksum for frames
            Return CRC16(UInt16.MaxValue, buffer, offset, length)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return CommonFrameHeader.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return CommonFrameHeader.BinaryImage(Me)
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            ' Header was preparsed by common frame header...

            ' IEEE 1344 only supports a single PMU...
            DirectCast(state, IConfigurationFrameParsingState).CellCount = 1

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As UInt16
            Get
                Return 2
            End Get
        End Property

        Protected Overrides ReadOnly Property FooterImage() As Byte()
            Get
                Return EndianOrder.BigEndian.GetBytes(Period)
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

            Period = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration frame
            info.AddValue("idCode64Bit", m_idCode)
            info.AddValue("sampleCount", m_sampleCount)

        End Sub

        Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
            Get
                With MyBase.Attributes
                    .Add("64-Bit ID Code", IDCode)
                End With

                Return MyBase.Attributes
            End Get
        End Property

    End Class

End Namespace