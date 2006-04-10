'*******************************************************************************************************
'  FrameParser.vb - IEEE C37.118 Frame Parser
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
Imports Tva.Phasors.Common

Namespace IeeeC37_118

    ''' <summary>This class parses an IEEE C37.118 binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits Stream

#Region " Public Member Declarations "

        Public Event ReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)
        Public Event ReceivedConfigurationFrame1(ByVal frame As ConfigurationFrame)
        Public Event ReceivedConfigurationFrame2(ByVal frame As ConfigurationFrame)
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Event ReceivedHeaderFrame(ByVal frame As HeaderFrame)
        Public Event ReceivedCommandFrame(ByVal frame As CommandFrame)
        Public Event ReceivedFrameBufferImage(ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer)
        Public Event DataStreamException(ByVal ex As Exception)

#End Region

#Region " Private Member Declarations "

        Private m_revisionNumber As RevisionNumber
        Private WithEvents m_bufferQueue As ProcessQueue(Of Byte())
        Private m_dataStream As MemoryStream
        Private m_configurationFrame2 As ConfigurationFrame
        Private m_totalFramesReceived As Long
        Private m_initialized As Boolean

#End Region

#Region " Construction Functions "

        Public Sub New()

            m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessBuffer)
            m_revisionNumber = IeeeC37_118.RevisionNumber.RevisionV1

#If DEBUG Then
            m_bufferQueue.DebugMode = True
#End If

        End Sub

        Public Sub New(ByVal revisionNumber As RevisionNumber)

            MyClass.New()
            m_revisionNumber = revisionNumber

        End Sub

        Public Sub New(ByVal revisionNumber As RevisionNumber, ByVal configurationFrame2 As ConfigurationFrame)

            MyClass.New(revisionNumber)
            m_configurationFrame2 = configurationFrame2

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Property RevisionNumber() As RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
        End Property

        Public Sub Start()

            m_initialized = False
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

        Public ReadOnly Property TimeBase() As Int32
            Get
                If m_configurationFrame2 Is Nothing Then
                    Return 0
                Else
                    Return m_configurationFrame2.TimeBase
                End If
            End Get
        End Property

        Public Property ConfigurationFrame() As ConfigurationFrame
            Get
                Return m_configurationFrame2
            End Get
            Set(ByVal value As ConfigurationFrame)
                m_configurationFrame2 = value
            End Set
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

        ' We process all queued data buffers that are available at once...
        Private Sub ProcessBuffer(ByVal buffers As Byte()())

            Dim parsedFrameHeader As ICommonFrameHeader
            Dim buffer As Byte()
            Dim index As Integer

            With New MemoryStream
                ' Add any left over buffer data from last processing run
                If m_dataStream IsNot Nothing Then
                    .Write(m_dataStream.ToArray(), 0, m_dataStream.Length)
                    m_dataStream = Nothing
                End If

                ' Add all currently queued buffers
                For x As Integer = 0 To buffers.Length - 1
                    .Write(buffers(x), 0, buffers(x).Length)
                Next

                ' Pull all queued data together as one big buffer
                buffer = .ToArray()
            End With

            Do Until index >= buffer.Length
                ' See if there is enough data in the buffer to parse the common frame header
                If index + CommonFrameHeader.BinaryLength > buffer.Length Then
                    ' If not, save off remaining buffer to prepend onto next read
                    m_dataStream = New MemoryStream
                    m_dataStream.Write(buffer, index, buffer.Length - index)
                    Exit Do
                End If

                ' Parse frame header
                parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_revisionNumber, m_configurationFrame2, buffer, index)

                ' Until we receive configuration frame, we at least expose part of frame we have parsed
                If m_configurationFrame2 Is Nothing Then RaiseEvent ReceivedCommonFrameHeader(parsedFrameHeader)

                ' See if there is enough data in the buffer to parse the entire frame
                If index + parsedFrameHeader.FrameLength > buffer.Length Then
                    ' If not, save off remaining buffer to prepend onto next read
                    m_dataStream = New MemoryStream
                    m_dataStream.Write(buffer, index, buffer.Length - index)
                    Exit Do
                End If

                RaiseEvent ReceivedFrameBufferImage(buffer, index, parsedFrameHeader.FrameLength)

                ' Entire frame is availble, so we go ahead and parse it
                Select Case parsedFrameHeader.FrameType
                    Case FrameType.DataFrame
                        ' We can only start parsing data frames once we have successfully received configuration file 2...
                        If m_configurationFrame2 IsNot Nothing Then RaiseEvent ReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame2, buffer, index))
                    Case FrameType.ConfigurationFrame2
                        With New ConfigurationFrame(parsedFrameHeader, buffer, index)
                            m_configurationFrame2 = .This
                            RaiseEvent ReceivedConfigurationFrame2(.This)
                        End With
                    Case FrameType.ConfigurationFrame1
                        RaiseEvent ReceivedConfigurationFrame1(New ConfigurationFrame(parsedFrameHeader, buffer, index))
                    Case FrameType.HeaderFrame
                        RaiseEvent ReceivedHeaderFrame(New HeaderFrame(parsedFrameHeader, buffer, index))
                    Case FrameType.CommandFrame
                        RaiseEvent ReceivedCommandFrame(New CommandFrame(parsedFrameHeader, buffer, index))
                End Select

                index += parsedFrameHeader.FrameLength
            Loop

        End Sub

        Private Sub m_bufferQueue_ProcessException(ByVal ex As System.Exception) Handles m_bufferQueue.ProcessException

            RaiseEvent DataStreamException(ex)

        End Sub

#End Region

    End Class

End Namespace