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

        Private m_revisionNumber As RevisionNumber

        Public TimeBase As Int32 = 1000000
        Public FractionSec As Int32

        Public Sub New(ByVal parsedFrameHeader As FrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' TODO: Define new static configuration cell creation function
            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, parsedFrameHeader.FrameLength, Nothing), binaryImage, startIndex)

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

        Public Property RevisionNumber() As RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Short
            Get
                Return 20
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                'Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)
                'Dim index As Integer

                'buffer(0) = SyncByte
                'buffer(1) = DescriptorPacketFlag
                'EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(BinaryLength \ 2), buffer, 2)
                'buffer(4) = StreamType
                'buffer(5) = RevisionNumber
                'EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 6)
                'EndianOrder.BigEndian.CopyBytes(RowLength(True), buffer, 8) ' <-- Important: This step calculates all PMU row offsets!
                'EndianOrder.BigEndian.CopyBytes(PacketsPerSample, buffer, 12)
                'EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 14)

                'Return buffer
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

            ParsedFrameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            IDCode = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
            Ticks = (New UnixTimeTag(EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 6))).ToDateTime.Ticks
            FractionSec = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)
            TimeBase = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 14)
            parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 18)

        End Sub


        'Protected Overrides ReadOnly Property HeaderLength() As Int16
        '    Get
        '        Return 12
        '    End Get
        'End Property

        'Protected Overrides ReadOnly Property HeaderImage() As Byte()
        '    Get
        '        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)

        '        buffer(0) = SyncByte
        '        buffer(1) = Convert.ToByte(1)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(TimeTag.Value), buffer, 4)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_sampleNumber), buffer, 8)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 10)

        '        Return buffer
        '    End Get
        'End Property

        'Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        '    Dim configurationFrame As PDCStream.ConfigurationFrame = DirectCast(state, IDataFrameParsingState).ConfigurationFrame

        '    Dim dataCellCount As Int16
        '    Dim frameLength As Int16

        '    If binaryImage(startIndex) <> Common.SyncByte Then
        '        Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in PDCstream data frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))
        '    End If

        '    m_packetNumber = binaryImage(startIndex + 1)

        '    If m_packetNumber = DescriptorPacketFlag Then
        '        Throw New InvalidOperationException("Bad Data Stream: This is not a PDCstream data frame - looks like a configuration frame.")
        '    End If

        '    frameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
        '    TimeTag = New UnixTimeTag(EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 4))
        '    m_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
        '    dataCellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)

        '    ' TODO: validate frame length??

        '    If dataCellCount <> configurationFrame.Cells.Count Then
        '        Throw New InvalidOperationException("Stream/Config File Mismatch: PMU count (" & dataCellCount & ") in stream does not match defined count in configuration file:" & configurationFrame.Cells.Count)
        '    End If

        '    ' Skip through redundant header information for legacy streams...
        '    If configurationFrame.StreamType = StreamType.Legacy Then
        '        ' We are not validating this data or looking for changes since this information
        '        ' was already transmitted via the descriptor....
        '    End If

        'End Sub


        Public Overrides ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, Measurements.IMeasurement)
            Get
                ' TODO: Determine what to do with this concerning concentration
            End Get
        End Property

    End Class

End Namespace