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
    Private m_configurationFrame As IConfigurationFrame
    Private m_totalFramesReceived As Long
    Private m_initialized As Boolean
    Private m_enabled As Boolean

#End Region

#Region " Construction Functions "

    Protected Sub New()


    End Sub

    Protected Sub New(ByVal configurationFrame As IConfigurationFrame)

        MyClass.New()
        m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame)

    End Sub

#End Region

#Region " Public Methods Implementation "

    Public Overridable Sub Start() Implements IFrameParser.Start

        m_initialized = False
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

    Public Overridable Property ExecuteParseOnSeparateThread() As Boolean Implements IFrameParser.ExecuteParseOnSeparateThread
        Get
            Return m_executeParseOnSeparateThread
        End Get
        Set(ByVal value As Boolean)
            If value Then
                If m_bufferQueue Is Nothing Then m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessBuffers)
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

    Public Overridable Property ConfigurationFrame() As IConfigurationFrame Implements IFrameParser.ConfigurationFrame
        Get
            Return m_configurationFrame
        End Get
        Set(ByVal value As IConfigurationFrame)
            m_configurationFrame = CastToDerivedConfigurationFrame(value)
        End Set
    End Property

    ' Stream implementation overrides
    Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32) Implements IFrameParser.Write

        If m_initialized Then
            If m_executeParseOnSeparateThread Then
                ' Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                m_bufferQueue.Add(CopyBuffer(buffer, offset, count))
            Else
                ' Directly parse frame using calling thread (typically communications thread)
                ParseData(buffer, offset, count)
            End If
        Else
            ' Initial stream may be any where in the middle of a frame, so we attempt to locate sync byte to "line-up" data stream
            Dim syncBytePosition As Int32 = Array.IndexOf(buffer, SyncByte, offset, count)

            If syncBytePosition > -1 Then
                ' Initialize data stream starting at located sync byte
                If m_executeParseOnSeparateThread Then
                    m_bufferQueue.Add(CopyBuffer(buffer, syncBytePosition, count - syncBytePosition))
                Else
                    ParseData(buffer, syncBytePosition, count - syncBytePosition)
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
                .Append("     Received config frame: ")
                .Append(IIf(m_configurationFrame Is Nothing, "No", "Yes"))
                .Append(Environment.NewLine)
                If m_configurationFrame IsNot Nothing Then
                    .Append("   Devices in config frame: ")
                    .Append(m_configurationFrame.Cells.Count)
                    .Append(" total - ")
                    .Append(Environment.NewLine)
                    For x As Integer = 0 To m_configurationFrame.Cells.Count - 1
                        .Append("               (")
                        .Append(m_configurationFrame.Cells(x).IDCode)
                        .Append(") ")
                        .Append(m_configurationFrame.Cells(x).StationName)
                        .Append(Environment.NewLine)
                    Next
                    .Append("     Configured frame rate: ")
                    .Append(m_configurationFrame.FrameRate)
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

    Protected MustOverride Function CastToDerivedConfigurationFrame(ByVal configurationFrame As IConfigurationFrame) As IConfigurationFrame

    Protected Overridable Sub ParseData(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32)

        ''Try
        ''    Dim parsedFrameHeader As ICommonFrameHeader

        ''    ' Prepend any left over buffer data from last parse call
        ''    If m_dataStream IsNot Nothing Then
        ''        With New MemoryStream
        ''            .Write(m_dataStream.ToArray(), 0, m_dataStream.Length)
        ''            m_dataStream = Nothing

        ''            ' Append new incoming data
        ''            .Write(buffer, offset, count)

        ''            ' Pull all queued data together as one big buffer
        ''            buffer = .ToArray()
        ''            offset = 0
        ''            count = .Length
        ''        End With
        ''    End If

        ''    Dim endOfBuffer As Integer = offset + count - 1

        ''    Do Until offset > endOfBuffer
        ''        ' See if there is enough data in the buffer to parse the common frame header
        ''        If offset + CommonFrameHeader.BinaryLength > count Then
        ''            ' If not, save off remaining buffer to prepend onto next read
        ''            m_dataStream = New MemoryStream
        ''            m_dataStream.Write(buffer, offset, count - offset)
        ''            Exit Do
        ''        End If

        ''        ' Parse frame header
        ''        parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame2, buffer, offset)

        ''        ' Until we receive configuration frame, we at least expose part of frame we have parsed
        ''        If m_configurationFrame Is Nothing Then RaiseReceivedCommonFrameHeader(parsedFrameHeader)

        ''        ' See if there is enough data in the buffer to parse the entire frame
        ''        If offset + parsedFrameHeader.FrameLength > count Then
        ''            ' If not, save off remaining buffer to prepend onto next read
        ''            m_dataStream = New MemoryStream
        ''            m_dataStream.Write(buffer, offset, count - offset)
        ''            Exit Do
        ''        End If

        ''        RaiseEvent ReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength)

        ''        ' Entire frame is available, so we go ahead and parse it
        ''        'Select Case parsedFrameHeader.FrameType
        ''        '    Case FrameType.DataFrame
        ''        '        ' We can only start parsing data frames once we have successfully received configuration file 2...
        ''        '        If m_configurationFrame IsNot Nothing Then RaiseReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame2, buffer, offset))
        ''        '    Case FrameType.ConfigurationFrame2
        ''        '        Select Case m_draftRevision
        ''        '            Case DraftRevision.Draft6
        ''        '                With New ConfigurationFrameDraft6(parsedFrameHeader, buffer, offset)
        ''        '                    m_configurationFrame2 = .This
        ''        '                    RaiseReceivedConfigurationFrame2(.This)
        ''        '                End With
        ''        '            Case DraftRevision.Draft7
        ''        '                With New ConfigurationFrame(parsedFrameHeader, buffer, offset)
        ''        '                    m_configurationFrame2 = .This
        ''        '                    RaiseReceivedConfigurationFrame2(.This)
        ''        '                End With
        ''        '        End Select
        ''        '    Case FrameType.ConfigurationFrame1
        ''        '        Select Case m_draftRevision
        ''        '            Case DraftRevision.Draft6
        ''        '                RaiseReceivedConfigurationFrame1(New ConfigurationFrameDraft6(parsedFrameHeader, buffer, offset))
        ''        '            Case DraftRevision.Draft7
        ''        '                RaiseReceivedConfigurationFrame1(New ConfigurationFrame(parsedFrameHeader, buffer, offset))
        ''        '        End Select
        ''        '    Case FrameType.HeaderFrame
        ''        '        RaiseReceivedHeaderFrame(New HeaderFrame(parsedFrameHeader, buffer, offset))
        ''        '    Case FrameType.CommandFrame
        ''        '        RaiseReceivedCommandFrame(New CommandFrame(parsedFrameHeader, buffer, offset))
        ''        'End Select

        ''        offset += parsedFrameHeader.FrameLength
        ''    Loop
        ''Catch ex As Exception
        ''    RaiseEvent DataStreamException(ex)
        ''End Try

    End Sub

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

    Protected Overridable Sub RaiseConfigurationChangeDetected()

        RaiseEvent ConfigurationChanged()

    End Sub

    Protected Overridable Sub RaiseDataStreamException(ByVal ex As Exception)

        RaiseEvent DataStreamException(ex)

    End Sub

#End Region

#Region " Private Methods Implementation "

    ' We process all queued data buffers that are available at once...
    Private Sub ProcessBuffers(ByVal buffers As Byte()())

        With New MemoryStream
            ' Combine all currently queued buffers
            For x As Integer = 0 To buffers.Length - 1
                .Write(buffers(x), 0, buffers(x).Length)
            Next

            ' Parse combined data buffers
            ParseData(.ToArray(), 0, .Length)
        End With

    End Sub

    Private Sub m_bufferQueue_ProcessException(ByVal ex As System.Exception) Handles m_bufferQueue.ProcessException

        RaiseDataStreamException(ex)

    End Sub

#End Region

End Class