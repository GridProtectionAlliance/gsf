'*******************************************************************************************************
'  ConfigurationFrame.vb - IEEE C37.118 Configuration Frame
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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
Imports Tva.Interop
Imports Tva.DateTime
Imports Tva.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase
        Implements IFrameHeader

        Private m_frameType As FrameType = IeeeC37_118.FrameType.ConfigurationFrame2
        Private m_version As Byte = 1
        Private m_frameLength As Int16
        Private m_timeBase As Int32 = 10000
        Private m_timeQualityFlags As Int32
        Private m_revisionNumber As RevisionNumber

        Public Sub New(ByVal parsedFrameHeader As IFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' TODO: Define new static configuration cell creation function
            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, parsedFrameHeader.FrameLength, Nothing), binaryImage, startIndex)
            FrameHeader.Clone(parsedFrameHeader, Me)

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

        Public Property FrameType() As FrameType Implements IFrameHeader.FrameType
            Get
                Return m_frameType
            End Get
            Set(ByVal value As FrameType)
                If value = IeeeC37_118.FrameType.ConfigurationFrame1 Or value = IeeeC37_118.FrameType.ConfigurationFrame2 Then
                    m_frameType = value
                Else
                    Throw New InvalidCastException("Invalid frame type specified for configuration frame.  Can only be ConfigurationFrame1 or ConfigurationFrame2")
                End If
            End Set
        End Property

        Public Property Version() As Byte Implements IFrameHeader.Version
            Get
                Return m_version
            End Get
            Set(ByVal value As Byte)
                FrameHeader.Version(Me) = value
            End Set
        End Property

        Public Property FrameLength() As Int16 Implements IFrameHeader.FrameLength
            Get
                Return MyBase.BinaryLength
            End Get
            Set(ByVal value As Int16)
                MyBase.ParsedBinaryLength = value
            End Set
        End Property

        Public Overrides Property IDCode() As UInt16 Implements IFrameHeader.IDCode
            Get
                Return MyBase.IDCode
            End Get
            Set(ByVal value As UShort)
                MyBase.IDCode = value
            End Set
        End Property

        Public Overrides Property Ticks() As Long Implements IFrameHeader.Ticks
            Get
                Return MyBase.Ticks
            End Get
            Set(ByVal value As Long)
                MyBase.Ticks = value
            End Set
        End Property

        Public Property TimeBase() As Int32 Implements IFrameHeader.TimeBase
            Get
                Return m_timeBase
            End Get
            Set(ByVal value As Int32)
                m_timeBase = value
            End Set
        End Property

        Private Property InternalTimeQualityFlags() As Int32 Implements IFrameHeader.InternalTimeQualityFlags
            Get
                Return m_timeQualityFlags
            End Get
            Set(ByVal value As Int32)
                m_timeQualityFlags = value
            End Set
        End Property

        Public ReadOnly Property SecondOfCentury() As UInt32 Implements IFrameHeader.SecondOfCentury
            Get
                Return FrameHeader.SecondOfCentury(Me)
            End Get
        End Property

        Public ReadOnly Property FractionOfSecond() As Int32 Implements IFrameHeader.FractionOfSecond
            Get
                Return FrameHeader.FractionOfSecond(Me)
            End Get
        End Property

        Public Property TimeQualityFlags() As TimeQualityFlags Implements IFrameHeader.TimeQualityFlags
            Get
                Return FrameHeader.TimeQualityFlags(Me)
            End Get
            Set(ByVal value As TimeQualityFlags)
                FrameHeader.TimeQualityFlags(Me) = value
            End Set
        End Property

        Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode Implements IFrameHeader.TimeQualityIndicatorCode
            Get
                Return FrameHeader.TimeQualityIndicatorCode(Me)
            End Get
            Set(ByVal value As TimeQualityIndicatorCode)
                FrameHeader.TimeQualityIndicatorCode(Me) = value
            End Set
        End Property

        Public Property RevisionNumber() As RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return FrameHeader.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return FrameHeader.BinaryImage(Me)
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' We parse the C37.18 stream specific header image here...
            'Dim configurationFrame As ConfigurationFrame = DirectCast(state, IDataFrameParsingState).ConfigurationFrame
            Dim parsingState As IConfigurationFrameParsingState = DirectCast(state, IConfigurationFrameParsingState)

            If binaryImage(startIndex) <> Common.SyncByte Then
                Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in PDCstream configuration frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))
            End If

            ' versionFrameType = binaryImage(startIndex + 1)

            'ParsedFrameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            'IDCode = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
            'Ticks = (New UnixTimeTag(EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 6))).ToDateTime.Ticks
            'FractionSec = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)
            'TimeBase = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 14)
            parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 18)

        End Sub

        Public Overrides ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, Measurements.IMeasurement)
            Get
                ' TODO: Determine what to do with this concerning concentration
            End Get
        End Property

    End Class

End Namespace