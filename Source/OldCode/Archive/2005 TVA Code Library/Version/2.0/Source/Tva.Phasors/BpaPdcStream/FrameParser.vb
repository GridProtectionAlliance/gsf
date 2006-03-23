'*******************************************************************************************************
'  FrameParser.vb - BPA PDCstream Frame Parser
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
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.ComponentModel
Imports Tva.Collections
Imports Tva.IO.Common
Imports Tva.Phasors.IeeeC37_118.Common

Namespace BpaPdcStream

    ''' <summary>This class parses a BPA PDC binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits Stream

#Region " Public Member Declarations "

        Public Event ReceivedConfigurationFrame1(ByVal frame As ConfigurationFrame)
        Public Event ReceivedConfigurationFrame2(ByVal frame As ConfigurationFrame)
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Event DataStreamException(ByVal ex As Exception)

#End Region

#Region " Private Member Declarations "

        Private WithEvents m_bufferQueue As ProcessQueue(Of Byte())
        Private m_dataStream As MemoryStream
        Private m_totalFramesReceived As Long
        Private m_configurationFrame As ConfigurationFrame
        Private m_initialized As Boolean

        Private Const BufferSize As Int32 = 4096   ' 4Kb buffer

#End Region

#Region " Construction Functions "

        Public Sub New()

            m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessBuffer)

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Sub Start()

            m_bufferQueue.Start()

        End Sub

        Public Sub [Stop]()

            m_bufferQueue.Stop()

        End Sub

        Public ReadOnly Property Enabled() As Boolean
            Get
                Return m_bufferQueue.Enabled
            End Get
        End Property

        Public ReadOnly Property QueuedBuffers() As Int32
            Get
                Return m_bufferQueue.Count
            End Get
        End Property

        ' Stream implementation overrides
        Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32)

            If m_initialized Then
                ' Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                m_bufferQueue.Add(CopyBuffer(buffer, offset, count))
            Else
                ' Initial stream may be any where in the middle of a frame, so we attempt to locate sync byte to "line-up" data stream
                Dim syncBytePosition As Int32 = Array.IndexOf(buffer, SyncByte, offset, count)

                If syncBytePosition > -1 Then
                    ' Initialize data stream starting at located sync byte
                    m_bufferQueue.Add(CopyBuffer(buffer, syncBytePosition, count - syncBytePosition))
                    m_initialized = True
                End If
            End If

        End Sub

        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return True
            End Get
        End Property

#Region " Unimplemented Stream Overrides "

        ' This is a write only stream - so the following methods do not apply to this stream
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function Read(ByVal buffer() As Byte, ByVal offset As Int32, ByVal count As Int32) As Int32

            Throw New NotImplementedException("Cannnot read from WriteOnly stream")

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Function Seek(ByVal offset As Long, ByVal origin As System.IO.SeekOrigin) As Long

            Throw New NotImplementedException("WriteOnly stream has no position")

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub SetLength(ByVal value As Long)

            Throw New NotImplementedException("WriteOnly stream has no length")

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides ReadOnly Property Length() As Long
            Get
                Throw New NotImplementedException("WriteOnly stream has no length")
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Property Position() As Long
            Get
                Throw New NotImplementedException("WriteOnly stream has no position")
            End Get
            Set(ByVal value As Long)
                Throw New NotImplementedException("WriteOnly stream has no position")
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Sub Flush()

            ' Nothing to do, no need to throw an error...

        End Sub

#End Region

#End Region

#Region " Private Methods Implementation "

        Private Sub ProcessBuffer(ByVal buffer As Byte())

            'Dim parsedFrameHeader As ICommonFrameHeader
            'Dim index As Int32

            'If m_dataStream IsNot Nothing Then
            '    m_dataStream.Write(buffer, 0, buffer.Length)
            '    buffer = m_dataStream.ToArray()
            '    m_dataStream = Nothing
            'End If

            'Do Until index >= buffer.Length
            '    ' See if there is enough data in the buffer to parse a frame header
            '    If index + CommonFrameHeader.BinaryLength > buffer.Length Then
            '        ' If not, save off remaining buffer to prepend onto next read
            '        m_dataStream = New MemoryStream
            '        m_dataStream.Write(buffer, index, buffer.Length - index)
            '        Exit Do
            '    End If

            '    ' Parse frame header
            '    parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_revisionNumber, m_configFrame2, buffer, index)

            '    ' See if there is enough data in the buffer to parse the entire frame
            '    If index + parsedFrameHeader.FrameLength > buffer.Length Then
            '        ' If not, save off remaining buffer to prepend onto next read
            '        m_dataStream = New MemoryStream
            '        m_dataStream.Write(buffer, index, buffer.Length - index)
            '        Exit Do
            '    End If

            '    ' Entire frame is availble, so we go ahead and parse it
            '    RaiseEvent ReceivedFrame(parsedFrameHeader, buffer, index)

            '    Select Case parsedFrameHeader.FrameType
            '        Case FrameType.DataFrame
            '            ' We can only start parsing data frames once we have successfully received configuration file 2...
            '            If Not m_configFrame2 Is Nothing Then
            '                With New DataFrame(parsedFrameHeader, m_configFrame2, buffer, index)
            '                    RaiseEvent ReceivedDataFrame(.This)
            '                End With
            '            End If
            '        Case FrameType.HeaderFrame
            '            'With New HeaderFrame(parsedFrameHeader, buffer, startIndex)
            '            '    If .IsFirstFrame Then m_headerFile = New HeaderFile

            '            '    m_headerFile.AppendNextFrame(.This)

            '            '    If .IsLastFrame Then
            '            '        RaiseEvent ReceivedHeaderFile(m_headerFile)
            '            '        m_headerFile = Nothing
            '            '    End If

            '            '    startIndex += .FrameLength
            '            'End With
            '        Case FrameType.ConfigurationFrame1
            '            With New ConfigurationFrame(parsedFrameHeader, buffer, index)
            '                m_configFrame1 = .This
            '                RaiseEvent ReceivedConfigFile1(.This)
            '            End With
            '        Case FrameType.ConfigurationFrame2
            '            With New ConfigurationFrame(parsedFrameHeader, buffer, index)
            '                m_configFrame2 = .This
            '                RaiseEvent ReceivedConfigFile2(.This)
            '            End With
            '    End Select

            '    index += parsedFrameHeader.FrameLength
            'Loop

        End Sub

        Private Sub m_bufferQueue_ProcessException(ByVal ex As System.Exception) Handles m_bufferQueue.ProcessException

            RaiseEvent DataStreamException(ex)

        End Sub

#End Region

    End Class

End Namespace
