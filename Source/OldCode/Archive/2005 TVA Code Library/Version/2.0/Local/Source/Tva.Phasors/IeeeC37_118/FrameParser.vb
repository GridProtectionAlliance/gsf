'*******************************************************************************************************
'  FrameParser.vb - IEEE C37.118 Frame Parser
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
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
        Implements IFrameParser

#Region " Public Member Declarations "

        Public Event ReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)
        Public Event ReceivedConfigurationFrame1(ByVal frame As ConfigurationFrame)
        Public Event ReceivedConfigurationFrame2(ByVal frame As ConfigurationFrame)
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Event ReceivedHeaderFrame(ByVal frame As HeaderFrame)
        Public Event ReceivedCommandFrame(ByVal frame As CommandFrame)
        Public Event ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer) Implements IFrameParser.ReceivedFrameBufferImage
        Public Event ConfigurationChanged() Implements IFrameParser.ConfigurationChanged
        Public Event DataStreamException(ByVal ex As Exception) Implements IFrameParser.DataStreamException

#End Region

#Region " Private Member Declarations "

        Private Event IFrameParserReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Implements IFrameParser.ReceivedConfigurationFrame
        Private Event IFrameParserReceivedDataFrame(ByVal frame As IDataFrame) Implements IFrameParser.ReceivedDataFrame
        Private Event IFrameParserReceivedHeaderFrame(ByVal frame As IHeaderFrame) Implements IFrameParser.ReceivedHeaderFrame
        Private Event IFrameParserReceivedCommandFrame(ByVal frame As ICommandFrame) Implements IFrameParser.ReceivedCommandFrame
        Private Event IFrameParserReceivedUndeterminedFrame(ByVal frame As IChannelFrame) Implements IFrameParser.ReceivedUndeterminedFrame

        Private m_draftRevision As DraftRevision
        Private WithEvents m_bufferQueue As ProcessQueue(Of Byte())
        Private m_dataStream As MemoryStream
        Private m_configurationFrame2 As ConfigurationFrame
        Private m_configurationChangeHandled As Boolean
        Private m_totalFramesReceived As Long
        Private m_initialized As Boolean

#End Region

#Region " Construction Functions "

        Public Sub New()

            m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessBuffers)
            m_draftRevision = IeeeC37_118.DraftRevision.Draft7

        End Sub

        Public Sub New(ByVal draftRevision As DraftRevision)

            MyClass.New()
            m_draftRevision = draftRevision

        End Sub

        Public Sub New(ByVal draftRevision As DraftRevision, ByVal configurationFrame2 As ConfigurationFrame)

            MyClass.New(draftRevision)
            m_configurationFrame2 = configurationFrame2

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Property DraftRevision() As DraftRevision
            Get
                Return m_draftRevision
            End Get
            Set(ByVal Value As DraftRevision)
                m_draftRevision = Value
            End Set
        End Property

        Public Sub Start() Implements IFrameParser.Start

            m_initialized = False
            m_configurationChangeHandled = False
            m_bufferQueue.Start()

        End Sub

        Public Sub [Stop]() Implements IFrameParser.Stop

            m_bufferQueue.Stop()

        End Sub

        Public ReadOnly Property Enabled() As Boolean Implements IFrameParser.Enabled
            Get
                Return m_bufferQueue.Enabled
            End Get
        End Property

        Public ReadOnly Property QueuedBuffers() As Int32 Implements IFrameParser.QueuedBuffers
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
        Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32) Implements IFrameParser.Write

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

        Public ReadOnly Property Status() As String Implements IFrameParser.Status
            Get
                With New StringBuilder
                    .Append("IEEEC37.118 draft revision: ")
                    .Append([Enum].GetName(GetType(DraftRevision), m_draftRevision))
                    .Append(Environment.NewLine)
                    .Append("     Received config frame: ")
                    .Append(IIf(m_configurationFrame2 Is Nothing, "No", "Yes"))
                    .Append(Environment.NewLine)
                    .Append("         Current time base: ")
                    .Append(TimeBase)
                    .Append(Environment.NewLine)
                    If m_configurationFrame2 IsNot Nothing Then
                        .Append("     PMU's in config frame: ")
                        .Append(m_configurationFrame2.Cells.Count)
                        .Append(" total [")
                        For x As Integer = 0 To m_configurationFrame2.Cells.Count - 1
                            .Append(" "c)
                            .Append(m_configurationFrame2.Cells(x).StationName)
                            .Append(" (")
                            .Append(m_configurationFrame2.Cells(x).IDCode)
                            .Append(")"c)
                        Next
                        .Append("]"c)
                        .Append(Environment.NewLine)
                        .Append("     Configured frame rate: ")
                        .Append(m_configurationFrame2.FrameRate)
                        .Append(Environment.NewLine)
                    End If
                    .Append(m_bufferQueue.Status)

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

#Region " Private Methods Implementation "

        ' We process all queued data buffers that are available at once...
        Private Sub ProcessBuffers(ByVal buffers As Byte()())

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
                parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame2, buffer, index)

                ' Until we receive configuration frame, we at least expose part of frame we have parsed
                If m_configurationFrame2 Is Nothing Then RaiseReceivedCommonFrameHeader(parsedFrameHeader)

                ' See if there is enough data in the buffer to parse the entire frame
                If index + parsedFrameHeader.FrameLength > buffer.Length Then
                    ' If not, save off remaining buffer to prepend onto next read
                    m_dataStream = New MemoryStream
                    m_dataStream.Write(buffer, index, buffer.Length - index)
                    Exit Do
                End If

                RaiseEvent ReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, index, parsedFrameHeader.FrameLength)

                ' Entire frame is available, so we go ahead and parse it
                Select Case parsedFrameHeader.FrameType
                    Case FrameType.DataFrame
                        ' We can only start parsing data frames once we have successfully received configuration file 2...
                        If m_configurationFrame2 IsNot Nothing Then RaiseReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame2, buffer, index))
                    Case FrameType.ConfigurationFrame2
                        Select Case m_draftRevision
                            Case DraftRevision.Draft6
                                With New ConfigurationFrameDraft6(parsedFrameHeader, buffer, index)
                                    m_configurationFrame2 = .This
                                    RaiseReceivedConfigurationFrame2(.This)
                                End With
                            Case DraftRevision.Draft7
                                With New ConfigurationFrame(parsedFrameHeader, buffer, index)
                                    m_configurationFrame2 = .This
                                    RaiseReceivedConfigurationFrame2(.This)
                                End With
                        End Select
                    Case FrameType.ConfigurationFrame1
                        Select Case m_draftRevision
                            Case DraftRevision.Draft6
                                RaiseReceivedConfigurationFrame1(New ConfigurationFrameDraft6(parsedFrameHeader, buffer, index))
                            Case DraftRevision.Draft7
                                RaiseReceivedConfigurationFrame1(New ConfigurationFrame(parsedFrameHeader, buffer, index))
                        End Select
                    Case FrameType.HeaderFrame
                        RaiseReceivedHeaderFrame(New HeaderFrame(parsedFrameHeader, buffer, index))
                    Case FrameType.CommandFrame
                        RaiseReceivedCommandFrame(New CommandFrame(parsedFrameHeader, buffer, index))
                End Select

                index += parsedFrameHeader.FrameLength
            Loop

        End Sub

        Private Sub m_bufferQueue_ProcessException(ByVal ex As System.Exception) Handles m_bufferQueue.ProcessException

            RaiseEvent DataStreamException(ex)

        End Sub

        Private Property IFrameParserConfigurationFrame() As IConfigurationFrame Implements IFrameParser.ConfigurationFrame
            Get
                Return m_configurationFrame2
            End Get
            Set(ByVal value As IConfigurationFrame)
                ' Assign new config frame, casting if needed...
                If TypeOf value Is IeeeC37_118.ConfigurationFrame Then
                    m_configurationFrame2 = value
                Else
                    If m_draftRevision = DraftRevision.Draft7 Then
                        m_configurationFrame2 = New IeeeC37_118.ConfigurationFrame(value)
                    Else
                        m_configurationFrame2 = New IeeeC37_118.ConfigurationFrameDraft6(value)
                    End If
                End If
            End Set
        End Property

        Private Sub RaiseReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)

            RaiseEvent IFrameParserReceivedUndeterminedFrame(frame)
            RaiseEvent ReceivedCommonFrameHeader(frame)

        End Sub

        Private Sub RaiseReceivedConfigurationFrame1(ByVal frame As ConfigurationFrame)

            RaiseEvent IFrameParserReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame1(frame)

        End Sub

        Private Sub RaiseReceivedConfigurationFrame2(ByVal frame As ConfigurationFrame)

            RaiseEvent IFrameParserReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame2(frame)

        End Sub

        Private Sub RaiseReceivedDataFrame(ByVal frame As DataFrame)

            RaiseEvent IFrameParserReceivedDataFrame(frame)
            RaiseEvent ReceivedDataFrame(frame)

            ' We only need to look at first PMU data cell to determine if configuration has changed
            If frame.Cells.Count > 0 Then
                If frame.Cells(0).ConfigurationChangeDetected Then
                    ' Notification should terminate after one minute...
                    If Not m_configurationChangeHandled Then
                        m_configurationChangeHandled = True
                        RaiseEvent ConfigurationChanged()
                    End If
                Else
                    m_configurationChangeHandled = False
                End If
            End If

        End Sub

        Private Sub RaiseReceivedHeaderFrame(ByVal frame As HeaderFrame)

            RaiseEvent IFrameParserReceivedHeaderFrame(frame)
            RaiseEvent ReceivedHeaderFrame(frame)

        End Sub

        Private Sub RaiseReceivedCommandFrame(ByVal frame As CommandFrame)

            RaiseEvent IFrameParserReceivedCommandFrame(frame)
            RaiseEvent ReceivedCommandFrame(frame)

        End Sub

#End Region

    End Class

End Namespace