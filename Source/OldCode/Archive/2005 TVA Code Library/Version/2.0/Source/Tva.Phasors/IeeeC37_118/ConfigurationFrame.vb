'*******************************************************************************************************
'  ConfigurationFrame.vb - IEEE C37.118 Configuration Frame
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.DateTime
Imports Tva.Phasors.Common
Imports Tva.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase
        Implements ICommonFrameHeader

        Private m_revisionNumber As RevisionNumber
        Private m_frameType As FrameType
        Private m_version As Byte
        Private m_timeBase As Int32
        Private m_timeQualityFlags As Int32
        Private m_configurationRevision As UInt16

        Public Sub New(ByVal revisionNumber As RevisionNumber, ByVal frameType As FrameType, ByVal timeBase As Int32, ByVal idCode As Int16, ByVal ticks As Long, ByVal frameRate As Int16)

            MyBase.New(idCode, New ConfigurationCellCollection, ticks, frameRate)
            m_revisionNumber = revisionNumber
            Me.FrameType = frameType
            m_timeBase = timeBase
            m_version = IIf(Of Byte)(m_revisionNumber <= IeeeC37_118.RevisionNumber.RevisionV1, 1, 2)

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, parsedFrameHeader.FrameLength, _
                    AddressOf IeeeC37_118.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)

            CommonFrameHeader.Clone(parsedFrameHeader, Me)

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As ConfigurationCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Property RevisionNumber() As RevisionNumber Implements ICommonFrameHeader.RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
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

        Public Property ConfigurationRevision() As UInt16
            Get
                Return m_configurationRevision
            End Get
            Set(ByVal value As UInt16)
                m_configurationRevision = value
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
                If m_revisionNumber = RevisionNumber.RevisionD6 Then
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

                If m_revisionNumber > RevisionNumber.RevisionD6 Then
                    ' Add configuration revision count for version 7 and beyond
                    EndianOrder.BigEndian.CopyBytes(m_configurationRevision, buffer, 2)
                End If

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

            FrameRate = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)

            ' Parse out configuration revision count for version 7 and beyond
            If m_revisionNumber > RevisionNumber.RevisionD6 Then
                startIndex += 2
                m_configurationRevision = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex)
            End If

        End Sub

    End Class

End Namespace