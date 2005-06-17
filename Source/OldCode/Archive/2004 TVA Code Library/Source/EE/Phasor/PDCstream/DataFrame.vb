'***********************************************************************
'  DataPacket.vb - PDC stream data packet
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Buffer
Imports TVA.Interop
Imports TVA.Shared.Math

Namespace EE.Phasor.PDCstream

    ' This is essentially a "row" of PMU data at a given timestamp
    Public Class DataFrame

        Inherits DataFrameBase

        Private m_index As Int16
        Private m_dataCellCount As Int16
        Private m_frameLength As Int16

        Public Sub New()

            MyBase.New(New DataCellCollection)

        End Sub

        Public Sub New(ByVal index As Int16)

            Me.New()
            m_index = index

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(New DataCellCollection, configurationFrame, binaryImage, startIndex, GetType(DataCell))

            If binaryImage(startIndex) <> Common.SyncByte Then
                Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in data frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))
            End If

            ' TODO: check byte 1 - is version??
            'Buffer(1) = Convert.ToByte(1)

            m_frameLength = EndianOrder.ReverseToInt16(binaryImage, startIndex + 2)
            TimeTag = New Unix.TimeTag(EndianOrder.ReverseToInt32(binaryImage, startIndex + 4))
            m_index = EndianOrder.ReverseToInt16(binaryImage, startIndex + 8)
            m_dataCellCount = EndianOrder.ReverseToInt16(binaryImage, startIndex + 10)

            ' TODO: validate frame length??

            If m_dataCellCount <> Cells.Count Then
                Throw New InvalidOperationException("Stream/Config File Mismatch: PMU count (" & m_dataCellCount & ") in stream does not match defined count in configuration file (" & Cells.Count & ")")
            End If

            If Not ChecksumIsValid(binaryImage, startIndex) Then
                Throw New InvalidOperationException("Bad Data Stream: Invalid buffer image detected - check sum of " & Name & " did not match")
            End If

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

        Public Property Index() As Int16
            Get
                Return m_index
            End Get
            Set(ByVal Value As Int16)
                m_index = Value
            End Set
        End Property

        Public Property DataCellCount() As Int16
            Get
                Return m_dataCellCount
            End Get
            Set(ByVal Value As Int16)
                m_dataCellCount = Value
            End Set
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TVA.EE.Phasor.PDCstream.DataFrame"
            End Get
        End Property

        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer) As Int16

            ' PDCstream uses simple XOR checksum
            Return XorCheckSum(buffer, offset, length)

        End Function

        Public Overrides ReadOnly Property ProtocolSpecificDataLength() As Short
            Get
                Return 12
            End Get
        End Property

        Public Overrides ReadOnly Property ProtocolSpecificDataImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), ProtocolSpecificDataLength)

                buffer(0) = Common.SyncByte
                buffer(1) = Convert.ToByte(1)
                EndianOrder.SwapCopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
                EndianOrder.SwapCopyBytes(Convert.ToUInt32(TimeTag.Value), buffer, 4)
                EndianOrder.SwapCopyBytes(Convert.ToInt16(m_index), buffer, 8)
                EndianOrder.SwapCopyBytes(Convert.ToInt16(Cells.Count), buffer, 10)

                Return buffer
            End Get
        End Property

        'Private m_configFile As ConfigurationFrame
        'Private m_timeTag As Unix.TimeTag
        'Private m_timeStamp As DateTime
        'Private m_index As Integer

        'Public Cells As DataCell()
        'Public Published As Boolean

        'Public Const SyncByte As Byte = &HAA

        'Public Sub New(ByVal configFile As ConfigurationFrame, ByVal timeStamp As DateTime, ByVal index As Integer)

        '    m_configFile = configFile
        '    m_timeTag = New Unix.TimeTag(timeStamp)
        '    m_index = index

        '    ' We precalculate a regular .NET timestamp with milliseconds sitting in the middle of the sample index
        '    m_timeStamp = timeStamp.AddMilliseconds((m_index + 0.5@) * (1000@ / m_configFile.SampleRate))

        '    With m_configFile
        '        Cells = Array.CreateInstance(GetType(DataCell), .PMUCount)

        '        For x As Integer = 0 To Cells.Length - 1
        '            'Cells(x) = New DataCell(.PMU(x), index)
        '        Next
        '    End With

        'End Sub

        'Public ReadOnly Property TimeTag() As Unix.TimeTag
        '    Get
        '        Return m_timeTag
        '    End Get
        'End Property

        'Public ReadOnly Property Index() As Integer
        '    Get
        '        Return m_index
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
        '        EndianOrder.SwapCopyBytes(Convert.ToInt16(buffer.Length \ 2), buffer, 2)
        '        EndianOrder.SwapCopyBytes(Convert.ToUInt32(m_timeTag.Value), buffer, 4)
        '        EndianOrder.SwapCopyBytes(Convert.ToInt16(m_index), buffer, 8)
        '        EndianOrder.SwapCopyBytes(Convert.ToInt16(Cells.Length), buffer, 10)
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

        '' We sort data packets by timetag and index
        'Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        '    If TypeOf obj Is DataPacket Then
        '        Dim comparison As Integer = m_timeTag.CompareTo(DirectCast(obj, DataPacket).TimeTag)

        '        If comparison = 0 Then
        '            Return m_index.CompareTo(DirectCast(obj, DataPacket).Index)
        '        Else
        '            Return comparison
        '        End If
        '    Else
        '        Throw New ArgumentException("DataPacket can only be compared with other DataPackets...")
        '    End If

        'End Function

    End Class

End Namespace