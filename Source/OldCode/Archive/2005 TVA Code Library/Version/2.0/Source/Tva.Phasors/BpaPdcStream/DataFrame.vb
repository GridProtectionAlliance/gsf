'*******************************************************************************************************
'  DataPacket.vb - PDCstream data packet
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

Imports System.Buffer
Imports Tva.Interop
Imports Tva.DateTime
Imports Tva.Math.Common
Imports Tva.Phasors.BpaPdcStream.Common

Namespace BpaPdcStream

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False)> _
    Public Class DataFrame

        Inherits DataFrameBase

        Private m_packetNumber As Byte
        Private m_sampleNumber As Int16

        Public Sub New()

            MyBase.New(New DataCellCollection)
            m_packetNumber = 1

        End Sub

        Public Sub New(ByVal sampleNumber As Int16)

            MyClass.New()
            m_sampleNumber = sampleNumber

        End Sub

        ' If you are going to create multiple data packets, you can use this constructor
        ' Note that this only starts becoming necessary if you start hitting data size
        ' limits imposed by the nature of the protocol...
        Public Sub New(ByVal packetNumber As Byte, ByVal sampleNumber As Int16)

            MyClass.New(sampleNumber)
            Me.PacketNumber = packetNumber

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' TODO: Provide static data cell creation function
            MyBase.New(New DataFrameParsingState(New DataCellCollection, 0, configurationFrame, Nothing), binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal dataFrame As IDataFrame)

            MyBase.New(dataFrame)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As DataCellCollection
            Get
                Return MyBase.Cells
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

        'Public Property DataCellCount() As Int16
        '    Get
        '        Return m_dataCellCount
        '    End Get
        '    Set(ByVal Value As Int16)
        '        m_dataCellCount = Value
        '    End Set
        'End Property

        <CLSCompliant(False)> _
        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer) As UInt16

            ' PDCstream uses simple XOR checksum
            Return Xor16BitCheckSum(buffer, offset, length)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return 12
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)

                buffer(0) = SyncByte
                buffer(1) = Convert.ToByte(1)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
                EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(TimeTag.Value), buffer, 4)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_sampleNumber), buffer, 8)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 10)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Dim configurationFrame As BpaPdcStream.ConfigurationFrame = DirectCast(state, IDataFrameParsingState).ConfigurationFrame

            Dim dataCellCount As Int16
            Dim frameLength As Int16

            If binaryImage(startIndex) <> Common.SyncByte Then
                Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in PDCstream data frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))
            End If

            m_packetNumber = binaryImage(startIndex + 1)

            If m_packetNumber = DescriptorPacketFlag Then
                Throw New InvalidOperationException("Bad Data Stream: This is not a PDCstream data frame - looks like a configuration frame.")
            End If

            frameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            Ticks = (New UnixTimeTag(EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 4))).ToDateTime.Ticks
            m_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
            dataCellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)

            ' TODO: validate frame length??

            If dataCellCount <> configurationFrame.Cells.Count Then
                Throw New InvalidOperationException("Stream/Config File Mismatch: PMU count (" & dataCellCount & ") in stream does not match defined count in configuration file:" & configurationFrame.Cells.Count)
            End If

            ' Skip through redundant header information for legacy streams...
            If configurationFrame.StreamType = StreamType.Legacy Then
                ' We are not validating this data or looking for changes since this information
                ' was already transmitted via the descriptor....
            End If

        End Sub

        ' TODO: place this in proper override...
        'Public Overrides ReadOnly Property ProtocolSpecificDataLength() As Int16
        '    Get
        '        Return 12
        '    End Get
        'End Property

        'Public Overrides ReadOnly Property ProtocolSpecificDataImage() As Byte()
        '    Get
        '        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), ProtocolSpecificDataLength)

        '        buffer(0) = Common.SyncByte
        '        buffer(1) = Convert.ToByte(1)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(TimeTag.Value), buffer, 4)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_sampleNumber), buffer, 8)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 10)

        '        Return buffer
        '    End Get
        'End Property

        'Private m_configFile As ConfigurationFrame
        'Private m_timeTag As UnixTimeTag
        'Private m_timeStamp As DateTime
        'Private m_sampleNumber As Integer

        'Public Cells As DataCell()
        'Public Published As Boolean

        'Public Const SyncByte As Byte = &HAA

        'Public Sub New(ByVal configFile As ConfigurationFrame, ByVal timeStamp As DateTime, ByVal index As Integer)

        '    m_configFile = configFile
        '    m_timeTag = New UnixTimeTag(timeStamp)
        '    m_sampleNumber = index

        '    ' We precalculate a regular .NET timestamp with milliseconds sitting in the middle of the sample index
        '    m_timeStamp = timeStamp.AddMilliseconds((m_sampleNumber + 0.5@) * (1000@ / m_configFile.FrameRate))

        '    With m_configFile
        '        Cells = Array.CreateInstance(GetType(DataCell), .PMUCount)

        '        For x As Integer = 0 To Cells.Length - 1
        '            'Cells(x) = New DataCell(.PMU(x), index)
        '        Next
        '    End With

        'End Sub

        'Public ReadOnly Property TimeTag() As UnixTimeTag
        '    Get
        '        Return m_timeTag
        '    End Get
        'End Property

        'Public ReadOnly Property Index() As Integer
        '    Get
        '        Return m_sampleNumber
        '    End Get
        'End Property

        'Public ReadOnly Property TimeStamp() As DateTime
        '    Get
        '        Return m_timeStamp
        '    End Get
        'End Property

        'Public ReadOnly Property ReadyToPublish() As Boolean
        '    Get
        '        Static packetReady As Boolean

        '        If Not packetReady Then
        '            Dim isReady As Boolean = True

        '            ' If we have data for each cell in the row, we can go ahead and publish it...
        '            For x As Integer = 0 To Cells.Length - 1
        '                If Cells(x).IsEmpty Then
        '                    isReady = False
        '                    Exit For
        '                End If
        '            Next

        '            If isReady Then packetReady = True
        '            Return isReady
        '        Else
        '            Return True
        '        End If
        '    End Get
        'End Property

        'Public ReadOnly Property BinaryLength() As Integer
        '    Get
        '        Dim length As Integer = 14

        '        For x As Integer = 0 To Cells.Length - 1
        '            length += Cells(x).BinaryLength
        '        Next

        '        Return length
        '    End Get
        'End Property

        'Public ReadOnly Property BinaryImage() As Byte()
        '    Get
        '        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
        '        Dim pmuID As Byte()
        '        Dim index As Integer

        '        buffer(0) = SyncByte
        '        buffer(1) = Convert.ToByte(1)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToUInt32(m_timeTag.Value), buffer, 4)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(m_sampleNumber), buffer, 8)
        '        EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Length), buffer, 10)
        '        index = 12

        '        For x As Integer = 0 To Cells.Length - 1
        '            BlockCopy(Cells(x).BinaryImage, 0, buffer, index, Cells(x).BinaryLength)
        '            index += Cells(x).BinaryLength
        '        Next

        '        ' Add check sum
        '        BlockCopy(BitConverter.GetBytes(XorCheckSum(buffer, 0, index)), 0, buffer, index, 2)

        '        Return buffer
        '    End Get
        'End Property

        Public Overrides ReadOnly Property Measurements() As System.Collections.Generic.IDictionary(Of Integer, Measurements.IMeasurement)
            Get
                ' TODO: Yoo-Hoo!
            End Get
        End Property

    End Class

End Namespace