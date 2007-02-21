'*******************************************************************************************************
'  FrameParserBase.vb - Frame Parser Base Class
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
'  02/12/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.ComponentModel
Imports Tva.Collections
Imports Tva.IO.Common
Imports Tva.Phasors.Common

''' <summary>This class defines the basic functionality for a protocol to parse a binary data stream and return the parsed data via events</summary>
''' <remarks>Frame parsers are implemented as a write-only stream - this way data can come from any source</remarks>
<CLSCompliant(False)> _
Public MustInherit Class FrameParserBase

    Inherits Stream
    Implements IFrameParser

#Region " Public Member Declarations "

    ' Derived classes will typically also expose events to provide instances to the protocol specific final derived channel frames

    Public Event ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Implements IFrameParser.ReceivedConfigurationFrame
    Public Event ReceivedDataFrame(ByVal frame As IDataFrame) Implements IFrameParser.ReceivedDataFrame
    Public Event ReceivedHeaderFrame(ByVal frame As IHeaderFrame) Implements IFrameParser.ReceivedHeaderFrame
    Public Event ReceivedCommandFrame(ByVal frame As ICommandFrame) Implements IFrameParser.ReceivedCommandFrame
    Public Event ReceivedUndeterminedFrame(ByVal frame As IChannelFrame) Implements IFrameParser.ReceivedUndeterminedFrame
    Public Event ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer) Implements IFrameParser.ReceivedFrameBufferImage
    Public Event ConfigurationChanged() Implements IFrameParser.ConfigurationChanged
    Public Event DataStreamException(ByVal ex As Exception) Implements IFrameParser.DataStreamException

#End Region

#Region " Private Member Declarations "

    Private WithEvents m_bufferQueue As ProcessQueue(Of Byte())
    Private m_executeParseOnSeparateThread As Boolean
    Private m_dataStream As MemoryStream
    Private m_initialized As Boolean
    Private m_enabled As Boolean

#End Region

#Region " Public Methods Implementation "

    Public Overridable Sub Start() Implements IFrameParser.Start

        m_initialized = Not ProtocolUsesSyncByte
        If m_executeParseOnSeparateThread Then m_bufferQueue.Start()
        m_enabled = True

    End Sub

    Public Overridable Sub [Stop]() Implements IFrameParser.Stop

        m_enabled = False
        If m_executeParseOnSeparateThread Then m_bufferQueue.Stop()

    End Sub

    Public Overridable ReadOnly Property Enabled() As Boolean Implements IFrameParser.Enabled
        Get
            Return m_enabled
        End Get
    End Property

    Public Overridable Property ExecuteParseOnSeparateThread() As Boolean Implements IFrameParser.ExecuteParseOnSeperateThread
        Get
            Return m_executeParseOnSeparateThread
        End Get
        Set(ByVal value As Boolean)
            ' This property allows a dynamic change in state of how to process streaming data
            If value Then
                If m_bufferQueue Is Nothing Then m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessQueuedBuffers)
                If m_enabled AndAlso Not m_bufferQueue.Enabled Then m_bufferQueue.Start()
                m_executeParseOnSeparateThread = True
            Else
                m_executeParseOnSeparateThread = False
                If m_bufferQueue IsNot Nothing Then m_bufferQueue.Stop()
                m_bufferQueue = Nothing
            End If
        End Set
    End Property

    Public Overridable ReadOnly Property QueuedBuffers() As Int32 Implements IFrameParser.QueuedBuffers
        Get
            If m_executeParseOnSeparateThread Then
                Return m_bufferQueue.Count
            Else
                Return 0
            End If
        End Get
    End Property

    Public MustOverride Property ConfigurationFrame() As IConfigurationFrame Implements IFrameParser.ConfigurationFrame

    Public MustOverride ReadOnly Property ProtocolUsesSyncByte() As Boolean

    Public Overridable ReadOnly Property ProtocolSyncByte() As Byte
        Get
            Return SyncByte
        End Get
    End Property

    ' Stream implementation overrides
    Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32) Implements IFrameParser.Write

        If m_initialized Then
            If m_executeParseOnSeparateThread Then
                ' Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                m_bufferQueue.Add(CopyBuffer(buffer, offset, count))
            Else
                ' Directly parse frame using calling thread (typically communications thread)
                ParseBuffer(buffer, offset, count)
            End If
        Else
            ' Initial stream may be anywhere in the middle of a frame, so we attempt to locate sync byte to "line-up" data stream
            Dim syncBytePosition As Int32 = Array.IndexOf(buffer, ProtocolSyncByte, offset, count)

            If syncBytePosition > -1 Then
                ' Initialize data stream starting at located sync byte
                If m_executeParseOnSeparateThread Then
                    m_bufferQueue.Add(CopyBuffer(buffer, syncBytePosition, count - syncBytePosition))
                Else
                    ParseBuffer(buffer, syncBytePosition, count - syncBytePosition)
                End If

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

    Public Overridable ReadOnly Property Status() As String Implements IFrameParser.Status
        Get
            With New StringBuilder
                .Append("      Current parser state: ")
                If m_enabled Then
                    .Append("Active")
                Else
                    .Append("Idle")
                End If
                .Append(Environment.NewLine)
                If ProtocolUsesSyncByte Then
                    .Append(" Data synchronization byte: 0x")
                    .Append(ProtocolSyncByte.ToString("X"c))
                    .Append(Environment.NewLine)
                End If
                .Append("     Received config frame: ")
                .Append(IIf(ConfigurationFrame Is Nothing, "No", "Yes"))
                .Append(Environment.NewLine)
                If ConfigurationFrame IsNot Nothing Then
                    .Append("   Devices in config frame: ")
                    .Append(ConfigurationFrame.Cells.Count)
                    .Append(" total - ")
                    .Append(Environment.NewLine)
                    For x As Integer = 0 To ConfigurationFrame.Cells.Count - 1
                        .Append("               (")
                        .Append(ConfigurationFrame.Cells(x).IDCode)
                        .Append(") ")
                        .Append(ConfigurationFrame.Cells(x).StationName)
                        .Append(Environment.NewLine)
                    Next
                    .Append("     Configured frame rate: ")
                    .Append(ConfigurationFrame.FrameRate)
                    .Append(Environment.NewLine)
                End If
                .Append("  Parsing execution source: ")
                If m_executeParseOnSeparateThread Then
                    .Append("Independent thread using queued data")
                    .Append(Environment.NewLine)
                    .Append(m_bufferQueue.Status)
                Else
                    .Append("Communications thread")
                    .Append(Environment.NewLine)
                End If

                Return .ToString()
            End With
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

#Region " Protected Methods Implementation "

    ''' <summary>
    ''' Protocol specific frame parsing algorithm
    ''' </summary>
    ''' <param name="buffer">Buffer containing data to parse</param>
    ''' <param name="offset">Offset index into buffer that represents where to start parsing</param>
    ''' <param name="length">Maximum length of valid data from offset</param>
    ''' <param name="parsedFrameLength">Derived implementations update this value with the length of the data that was parsed</param>
    ''' <remarks>
    ''' Implementors can choose to focus on parsing a single frame in the buffer even if there are other frames available in the buffer.
    ''' Base class will continue to move through buffer on behalf of derived class until all the buffer has been processed.  Any data
    ''' that remains unparsed will be prepended to next received buffer.
    ''' </remarks>
    Protected MustOverride Sub ParseFrame(ByVal buffer As Byte(), ByVal offset As Int32, ByVal length As Int32, ByRef parsedFrameLength As Int32)

    Protected Overridable Sub RaiseReceivedConfigurationFrame(ByVal frame As IConfigurationFrame)

        RaiseEvent ReceivedConfigurationFrame(frame)

    End Sub

    Protected Overridable Sub RaiseReceivedDataFrame(ByVal frame As IDataFrame)

        RaiseEvent ReceivedDataFrame(frame)

    End Sub

    Protected Overridable Sub RaiseReceivedHeaderFrame(ByVal frame As IHeaderFrame)

        RaiseEvent ReceivedHeaderFrame(frame)

    End Sub

    Protected Overridable Sub RaiseReceivedCommandFrame(ByVal frame As ICommandFrame)

        RaiseEvent ReceivedCommandFrame(frame)

    End Sub

    Protected Overridable Sub RaiseReceivedUndeterminedFrame(ByVal frame As IChannelFrame)

        RaiseEvent ReceivedUndeterminedFrame(frame)

    End Sub

    Protected Overridable Sub RaiseReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer)

        RaiseEvent ReceivedFrameBufferImage(frameType, binaryImage, offset, length)

    End Sub

    Protected Overridable Sub RaiseConfigurationChangeDetected()

        RaiseEvent ConfigurationChanged()

    End Sub

    Protected Overridable Sub RaiseDataStreamException(ByVal ex As Exception)

        RaiseEvent DataStreamException(ex)

    End Sub

#End Region

#Region " Private Methods Implementation "

    ' We process all queued data buffers that are available at once...
    Private Sub ProcessQueuedBuffers(ByVal buffers As Byte()())

        With New MemoryStream
            ' Combine all currently queued buffers
            For x As Integer = 0 To buffers.Length - 1
                .Write(buffers(x), 0, buffers(x).Length)
            Next

            ' Parse combined data buffers
            ParseBuffer(.ToArray(), 0, .Length)
        End With

    End Sub

    Private Sub ParseBuffer(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32)

        Try
            ' Prepend any left over buffer data from last parse call
            If m_dataStream IsNot Nothing Then
                With New MemoryStream
                    .Write(m_dataStream.ToArray(), 0, m_dataStream.Length)
                    m_dataStream = Nothing

                    ' Append new incoming data
                    .Write(buffer, offset, count)

                    ' Pull all combined data together as one big buffer
                    buffer = .ToArray()
                    offset = 0
                    count = .Length
                End With
            End If

            Dim endOfBuffer As Integer = offset + count - 1
            Dim parsedFrameLength As Int32

            ' Move through buffer parsing all available frames
            Do Until offset > endOfBuffer
                parsedFrameLength = 0

                ' Call derived class frame parsing algorithm - this is protocol specific
                ParseFrame(buffer, offset, endOfBuffer - offset + 1, parsedFrameLength)

                If parsedFrameLength > 0 Then
                    ' If frame was parsed, increment buffer offset by frame length
                    offset += parsedFrameLength
                Else
                    ' If not, save off remaining buffer to prepend onto next read
                    m_dataStream = New MemoryStream
                    m_dataStream.Write(buffer, offset, count - offset)
                    Exit Do
                End If
            Loop
        Catch ex As Exception
            RaiseEvent DataStreamException(ex)
        End Try

    End Sub

    Private Sub m_bufferQueue_ProcessException(ByVal ex As System.Exception) Handles m_bufferQueue.ProcessException

        RaiseDataStreamException(ex)

    End Sub

#End Region

End Class