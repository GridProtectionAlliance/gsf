'*******************************************************************************************************
'  ConfigurationFrame.vb - IEEE C37.118 Configuration Frame
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
Imports System.Text
Imports TVA.DateTime
Imports TVA.Phasors.Common
Imports TVA.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase
        Implements ICommonFrameHeader

        Private m_frameType As FrameType
        Private m_version As Byte
        Private m_timeBase As Int32
        Private m_timeQualityFlags As Int32

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration frame
            m_frameType = info.GetValue("frameType", GetType(FrameType))
            m_version = info.GetByte("version")
            m_timeBase = info.GetInt32("timeBase")
            m_timeQualityFlags = info.GetInt32("timeQualityFlags")

        End Sub

        Public Sub New(ByVal frameType As FrameType, ByVal timeBase As Int32, ByVal idCode As UInt16, ByVal ticks As Long, ByVal frameRate As Int16, ByVal version As Byte)

            MyBase.New(idCode, New ConfigurationCellCollection, ticks, frameRate)
            Me.FrameType = frameType
            m_timeBase = timeBase
            m_version = version

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, parsedFrameHeader.FrameLength, _
                    AddressOf IeeeC37_118.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)

            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)

            ' Assign default values for a 37.118 configuration frame
            m_frameType = IeeeC37_118.FrameType.ConfigurationFrame2
            m_version = 1
            m_timeBase = 100000

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

        Public Overridable ReadOnly Property DraftRevision() As DraftRevision
            Get
                Return IeeeC37_118.DraftRevision.Draft7
            End Get
        End Property

        Public Property FrameType() As FrameType Implements ICommonFrameHeader.FrameType
            Get
                Return m_frameType
            End Get
            Set(ByVal value As FrameType)
                If value = IeeeC37_118.FrameType.ConfigurationFrame2 OrElse value = IeeeC37_118.FrameType.ConfigurationFrame1 Then
                    m_frameType = value
                Else
                    Throw New InvalidCastException("Invalid frame type specified for configuration frame.  Can only be ConfigurationFrame1 or ConfigurationFrame2")
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements ICommonFrameHeader.FundamentalFrameType
            Get
                Return MyBase.FundamentalFrameType
            End Get
        End Property

        Public Property Version() As Byte Implements ICommonFrameHeader.Version
            Get
                Return m_version
            End Get
            Set(ByVal value As Byte)
                m_version = CommonFrameHeader.Version(Me, value)
            End Set
        End Property

        Public Property FrameLength() As Int16 Implements ICommonFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value
            End Set
        End Property

        Public Property TimeBase() As Int32
            Get
                Return m_timeBase
            End Get
            Set(ByVal value As Int32)
                If value = 0 Then
                    m_timeBase = 1000000
                Else
                    m_timeBase = value
                End If
            End Set
        End Property

        Private ReadOnly Property ICommonFrameHeaderTimeBase() As Int32 Implements ICommonFrameHeader.TimeBase
            Get
                Return m_timeBase
            End Get
        End Property

        Private Property InternalTimeQualityFlags() As Int32 Implements ICommonFrameHeader.InternalTimeQualityFlags
            Get
                Return m_timeQualityFlags
            End Get
            Set(ByVal value As Int32)
                m_timeQualityFlags = value
            End Set
        End Property

        Public ReadOnly Property SecondOfCentury() As UInt32 Implements ICommonFrameHeader.SecondOfCentury
            Get
                Return CommonFrameHeader.SecondOfCentury(Me)
            End Get
        End Property

        Public ReadOnly Property FractionOfSecond() As Int32 Implements ICommonFrameHeader.FractionOfSecond
            Get
                Return CommonFrameHeader.FractionOfSecond(Me)
            End Get
        End Property

        Public Property TimeQualityFlags() As TimeQualityFlags Implements ICommonFrameHeader.TimeQualityFlags
            Get
                Return CommonFrameHeader.TimeQualityFlags(Me)
            End Get
            Set(ByVal value As TimeQualityFlags)
                CommonFrameHeader.TimeQualityFlags(Me) = value
            End Set
        End Property

        Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode Implements ICommonFrameHeader.TimeQualityIndicatorCode
            Get
                Return CommonFrameHeader.TimeQualityIndicatorCode(Me)
            End Get
            Set(ByVal value As TimeQualityIndicatorCode)
                CommonFrameHeader.TimeQualityIndicatorCode(Me) = value
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return CommonFrameHeader.BinaryLength + 6
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)
                Dim index As Int32

                CopyImage(CommonFrameHeader.BinaryImage(Me), buffer, index, CommonFrameHeader.BinaryLength)
                EndianOrder.BigEndian.CopyBytes(m_timeBase, buffer, index)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, index + 4)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            ' We parse the C37.18 stream specific header image here...
            Dim parsingState As IConfigurationFrameParsingState = DirectCast(state, IConfigurationFrameParsingState)

            m_timeBase = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 14)
            parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 18)

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As UInt16
            Get
                If DraftRevision = DraftRevision.Draft6 Then
                    Return 2
                Else
                    Return 4
                End If
            End Get
        End Property

        Protected Overrides ReadOnly Property FooterImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(FooterLength)

                EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 0)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

            FrameRate = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration frame
            info.AddValue("frameType", m_frameType, GetType(FrameType))
            info.AddValue("version", m_version)
            info.AddValue("timeBase", m_timeBase)
            info.AddValue("timeQualityFlags", m_timeQualityFlags)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Frame Type", FrameType & ": " & [Enum].GetName(GetType(FrameType), FrameType))
                baseAttributes.Add("Frame Length", FrameLength)
                baseAttributes.Add("Version", Version)
                baseAttributes.Add("Second of Century", SecondOfCentury)
                baseAttributes.Add("Fraction of Second", FractionOfSecond)
                baseAttributes.Add("Time Quality Flags", TimeQualityFlags & ": " & [Enum].GetName(GetType(TimeQualityFlags), TimeQualityFlags))
                baseAttributes.Add("Time Quality Indicator Code", TimeQualityIndicatorCode & ": " & [Enum].GetName(GetType(TimeQualityIndicatorCode), TimeQualityIndicatorCode))
                baseAttributes.Add("Time Base", TimeBase)
                baseAttributes.Add("Draft Revision", DraftRevision & ": " & [Enum].GetName(GetType(DraftRevision), DraftRevision))

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace