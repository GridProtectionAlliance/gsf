'*******************************************************************************************************
'  FrameParser.vb - FNet Frame Parser
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
'  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.ComponentModel
Imports Tva.Collections
Imports Tva.IO.Common
Imports Tva.Phasors.Common
Imports Tva.Phasors.FNet.Common

Namespace FNet

    ''' <summary>This class parses an FNet binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits Stream
        Implements IFrameParser

#Region " Public Member Declarations "
        ''' <summary>
        ''' This event is raised when a Configuration Frame has been parsed
        ''' </summary>
        ''' <remarks>
        ''' <para>See Std IEEE 1344 for the definition of configuration frame. FNET uses a similar concept</para>
        ''' <para>
        ''' Note that the FNET data steam does not contain a parsable configuration frame, but a virtual
        ''' one is created on reception of the first data frame
        ''' </para>
        ''' </remarks>
        Public Event ReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)
        ''' <summary>
        ''' This event is raised when a Data Frame has been parsed
        ''' </summary>
        ''' <remarks>See Std IEEE 1344 for the definition of data frame. FNET uses a similar concept</remarks>
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        ''' <summary>
        ''' This event is raised when an entire frame is available
        ''' </summary>
        ''' <remarks></remarks>
        Public Event ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer) Implements IFrameParser.ReceivedFrameBufferImage
        ''' <summary>
        ''' This event is raised when an exception occurs while parsing the data stream
        ''' </summary>
        ''' <remarks></remarks>
        Public Event DataStreamException(ByVal ex As Exception) Implements IFrameParser.DataStreamException

#End Region

#Region " Private Member Declarations "

        Private Event IFrameParserReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Implements IFrameParser.ReceivedConfigurationFrame
        Private Event IFrameParserReceivedDataFrame(ByVal frame As IDataFrame) Implements IFrameParser.ReceivedDataFrame
        Private Event IFrameParserReceivedHeaderFrame(ByVal frame As IHeaderFrame) Implements IFrameParser.ReceivedHeaderFrame
        Private Event IFrameParserReceivedCommandFrame(ByVal frame As ICommandFrame) Implements IFrameParser.ReceivedCommandFrame
        Private Event IFrameParserReceivedUndeterminedFrame(ByVal frame As IChannelFrame) Implements IFrameParser.ReceivedUndeterminedFrame
        Private Event IFrameParserConfigurationChanged() Implements IFrameParser.ConfigurationChanged

        Private m_executeParseOnSeparateThread As Boolean
        Private WithEvents m_bufferQueue As ProcessQueue(Of Byte())
        Private m_dataStream As MemoryStream
        Private m_configurationFrame As ConfigurationFrame
        Private m_totalFramesReceived As Long
        Private m_frameRate As Int16
        Private m_nominalFrequency As LineFrequency

#End Region

#Region " Construction Functions "

        Public Sub New()

            ' FNet devices default to 10 frames per second and 60Hz
            MyClass.New(10, LineFrequency.Hz60)

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame)

            MyClass.New(configurationFrame.FrameRate, configurationFrame.NominalFrequency)
            m_configurationFrame = configurationFrame

        End Sub

        Public Sub New(ByVal frameRate As Int16, ByVal nominalFrequency As LineFrequency)

            m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessBuffers)
            m_frameRate = frameRate
            m_nominalFrequency = nominalFrequency

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Sub Start() Implements IFrameParser.Start

            If m_executeParseOnSeparateThread Then m_bufferQueue.Start()

        End Sub

        Public Sub [Stop]() Implements IFrameParser.Stop

            If m_executeParseOnSeparateThread Then m_bufferQueue.Stop()

        End Sub

        Public ReadOnly Property Enabled() As Boolean Implements IFrameParser.Enabled
            Get
                Return m_bufferQueue.Enabled
            End Get
        End Property

        Public Property ExecuteParseOnSeparateThread() As Boolean Implements IFrameParser.ExecuteParseOnSeparateThread
            Get
                Return m_executeParseOnSeparateThread
            End Get
            Set(ByVal value As Boolean)
                m_executeParseOnSeparateThread = value
            End Set
        End Property

        Public ReadOnly Property QueuedBuffers() As Int32 Implements IFrameParser.QueuedBuffers
            Get
                Return m_bufferQueue.Count
            End Get
        End Property

        Public Property ConfigurationFrame() As ConfigurationFrame
            Get
                Return m_configurationFrame
            End Get
            Set(ByVal value As ConfigurationFrame)
                m_configurationFrame = value
            End Set
        End Property

        Public Property FrameRate() As Int16
            Get
                Return m_frameRate
            End Get
            Set(ByVal value As Int16)
                m_frameRate = value
            End Set
        End Property

        Public Property NominalFrequency() As LineFrequency
            Get
                Return m_nominalFrequency
            End Get
            Set(ByVal value As LineFrequency)
                m_nominalFrequency = value
            End Set
        End Property

        ' Stream implementation overrides
        Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32) Implements IFrameParser.Write

            If m_executeParseOnSeparateThread Then
                ' Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
                m_bufferQueue.Add(CopyBuffer(buffer, offset, count))
            Else
                ' Directly parse frame using calling thread (typically communications thread)
                ParseData(buffer, offset, count)
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
                    .Append("     Received config frame: ")
                    .Append(IIf(m_configurationFrame Is Nothing, "No", "Yes"))
                    .Append(Environment.NewLine)
                    If m_configurationFrame IsNot Nothing Then
                        .Append("     PMU's in config frame: 1 total - ")
                        .Append(Environment.NewLine)
                        .Append("               (")
                        .Append(m_configurationFrame.Cells(0).IDCode)
                        .Append(") ")
                        .Append(m_configurationFrame.Cells(0).StationName)
                        .Append(Environment.NewLine)
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

        Private Sub ParseData(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32)

            Try
                ' Prepend any left over buffer data from last parse call
                If m_dataStream IsNot Nothing Then
                    With New MemoryStream
                        .Write(m_dataStream.ToArray(), 0, m_dataStream.Length)
                        m_dataStream = Nothing

                        ' Append new incoming data
                        .Write(buffer, offset, count)

                        ' Pull all queued data together as one big buffer
                        buffer = .ToArray()
                        offset = 0
                        count = .Length
                    End With
                End If

                Dim x, startByteIndex, endByteIndex As Integer
                Dim endOfBuffer As Integer = offset + count - 1

                Do Until offset > endOfBuffer
                    ' See if there is enough data in the buffer to parse the entire frame
                    startByteIndex = -1
                    endByteIndex = -1

                    For x = offset To endOfBuffer
                        ' Found start index
                        If buffer(x) = StartByte Then startByteIndex = x

                        If buffer(x) = EndByte Then
                            If startByteIndex = -1 Then
                                ' Found end before beginning, bad buffer - keep looking
                                Continue For
                            Else
                                ' Found a complete buffer
                                endByteIndex = x
                                Exit For
                            End If
                        End If
                    Next

                    ' If there was not entire frame to parse save off remaining buffer to prepend onto next read
                    If endByteIndex = -1 Then
                        ' If there is no startByte and endByte, discard the bad buffer
                        If startByteIndex = -1 Then
                            m_dataStream = Nothing
                        Else
                            m_dataStream = New MemoryStream
                            m_dataStream.Write(buffer, offset, count - offset)
                        End If

                        Exit Do
                    End If

                    ' Entire frame is available, so we go ahead and parse it
                    RaiseEvent ReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, startByteIndex, endByteIndex - startByteIndex + 1)

                    ' If no configuration frame has been created, we create one now
                    If m_configurationFrame Is Nothing Then
                        ' Pre-parse first FNet data frame to get unit ID field and establish a virutal configuration frame
                        Dim data As String() = Encoding.ASCII.GetString(buffer, startByteIndex + 1, endByteIndex - startByteIndex - 1).Split(" "c)

                        ' Create virtual configuration frame
                        m_configurationFrame = New ConfigurationFrame(Convert.ToUInt16(data(Element.UnitID)), Date.Now.Ticks, m_frameRate, m_nominalFrequency)

                        ' Notify clients of new configuration frame
                        RaiseReceivedConfigurationFrame(m_configurationFrame)
                    End If

                    ' Provide new FNet data frame to clients
                    RaiseReceivedDataFrame(New DataFrame(m_configurationFrame, buffer, startByteIndex))

                    ' Increment offset past end of data frame
                    offset = endByteIndex + 1
                Loop
            Catch ex As Exception
                RaiseEvent DataStreamException(ex)
            End Try

        End Sub

        Private Sub m_bufferQueue_ProcessException(ByVal ex As System.Exception) Handles m_bufferQueue.ProcessException

            RaiseEvent DataStreamException(ex)

        End Sub

        Private Property IFrameParserConfigurationFrame() As IConfigurationFrame Implements IFrameParser.ConfigurationFrame
            Get
                Return m_configurationFrame
            End Get
            Set(ByVal value As IConfigurationFrame)
                ' Assign new config frame, casting if needed...
                If TypeOf value Is FNet.ConfigurationFrame Then
                    m_configurationFrame = value
                Else
                    m_configurationFrame = New FNet.ConfigurationFrame(value)
                End If
            End Set
        End Property

        Private Sub RaiseReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)

            RaiseEvent IFrameParserReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame(frame)

        End Sub

        Private Sub RaiseReceivedDataFrame(ByVal frame As DataFrame)

            RaiseEvent IFrameParserReceivedDataFrame(frame)
            RaiseEvent ReceivedDataFrame(frame)

        End Sub

#End Region

    End Class

End Namespace
