'*******************************************************************************************************
'  HeaderFrame.vb - IEEE1344 Header Frame
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
Imports Tva.DateTime
Imports Tva.Phasors.Ieee1344.Common
Imports Tva.IO.Compression.Common

Namespace Ieee1344

    <CLSCompliant(False), Serializable()> _
    Public Class HeaderFrame

        Inherits HeaderFrameBase
        Implements ICommonFrameHeader

        Private m_idCode As UInt64
        Private m_sampleCount As Int16
        Private m_statusFlags As Int16

        Public Sub New()

            MyBase.New(New HeaderCellCollection(MaximumHeaderDataLength))
            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.HeaderFrame

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize header frame
            m_idCode = info.GetUInt64("idCode64Bit")
            m_sampleCount = info.GetInt16("sampleCount")
            m_statusFlags = info.GetInt16("statusFlags")

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New HeaderFrameParsingState(New HeaderCellCollection(MaximumHeaderDataLength), parsedFrameHeader.FrameLength, _
                parsedFrameHeader.FrameLength - CommonFrameHeader.BinaryLength - 2), binaryImage, startIndex)

            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.HeaderFrame
            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal headerFrame As IHeaderFrame)

            MyBase.New(headerFrame)
            CommonFrameHeader.FrameType(Me) = Ieee1344.FrameType.HeaderFrame

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows Property IDCode() As UInt64 Implements ICommonFrameHeader.IDCode
            Get
                Return m_idCode
            End Get
            Set(ByVal value As UInt64)
                m_idCode = value
            End Set
        End Property

        Public Shadows ReadOnly Property TimeTag() As NtpTimeTag Implements ICommonFrameHeader.TimeTag
            Get
                Return CommonFrameHeader.TimeTag(Me)
            End Get
        End Property

        Public ReadOnly Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return CommonFrameHeader.FrameType(Me)
            End Get
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
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

        Private Property InternalStatusFlags() As Int16 Implements ICommonFrameHeader.InternalStatusFlags
            Get
                Return m_statusFlags
            End Get
            Set(ByVal value As Int16)
                m_statusFlags = value
            End Set
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

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize header frame
            info.AddValue("idCode64Bit", m_idCode)
            info.AddValue("sampleCount", m_sampleCount)
            info.AddValue("statusFlags", m_statusFlags)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Frame Type", FrameType & ": " & [Enum].GetName(GetType(FrameType), FrameType))
                baseAttributes.Add("Frame Length", FrameLength)
                baseAttributes.Add("64-Bit ID Code", IDCode)
                baseAttributes.Add("Sample Count", m_sampleCount)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace