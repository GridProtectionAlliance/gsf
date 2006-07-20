'*******************************************************************************************************
'  FrameParser.vb - IEEE1344 Frame Parser
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

Namespace Ieee1344

    ''' <summary>This class parses an IEEE 1344 binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits Stream
        Implements IFrameParser

#Region " Public Member Declarations "

        Public Event ReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)
        Public Event ReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)
        Public Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Event ReceivedHeaderFrame(ByVal frame As HeaderFrame)
        Public Event ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer) Implements IFrameParser.ReceivedFrameBufferImage
        Public Event DataStreamException(ByVal ex As Exception) Implements IFrameParser.DataStreamException

#End Region

#Region " Private Member Declarations "

        Private Event IFrameParserReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Implements IFrameParser.ReceivedConfigurationFrame
        Private Event IFrameParserReceivedDataFrame(ByVal frame As IDataFrame) Implements IFrameParser.ReceivedDataFrame
        Private Event IFrameParserReceivedHeaderFrame(ByVal frame As IHeaderFrame) Implements IFrameParser.ReceivedHeaderFrame
        Private Event IFrameParserReceivedCommandFrame(ByVal frame As ICommandFrame) Implements IFrameParser.ReceivedCommandFrame
        Private Event IFrameParserReceivedUndeterminedFrame(ByVal frame As IChannelFrame) Implements IFrameParser.ReceivedUndeterminedFrame
        Private Event IFrameParserConfigurationChanged() Implements IFrameParser.ConfigurationChanged

        Private WithEvents m_bufferQueue As ProcessQueue(Of Byte())
        Private m_dataStream As MemoryStream
        Private m_configurationFrame As ConfigurationFrame
        Private m_totalFramesReceived As Long
        Private m_configurationFrameCollection As CommonFrameHeader.CommonFrameHeaderInstance
        Private m_headerFrameCollection As CommonFrameHeader.CommonFrameHeaderInstance

#End Region

#Region " Construction Functions "

        Public Sub New()

            m_bufferQueue = ProcessQueue(Of Byte()).CreateRealTimeQueue(AddressOf ProcessBuffers)

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame)

            MyClass.New()
            m_configurationFrame = configurationFrame

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Sub Start() Implements IFrameParser.Start

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

        Public Property ConfigurationFrame() As ConfigurationFrame
            Get
                Return m_configurationFrame
            End Get
            Set(ByVal value As ConfigurationFrame)
                m_configurationFrame = value
            End Set
        End Property

        ' Stream implementation overrides
        Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32) Implements IFrameParser.Write

            ' Queue up received data buffer for real-time parsing and return to data collection as quickly as possible...
            m_bufferQueue.Add(CopyBuffer(buffer, offset, count))

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
                        .Append("     PMU's in config frame: ")
                        .Append(m_configurationFrame.Cells.Count)
                        .Append(Environment.NewLine)
                        .Append("    Configured PMU ID code: ")
                        .Append(m_configurationFrame.IDCode)
                        .Append(Environment.NewLine)
                        .Append("     Configured frame rate: ")
                        .Append(m_configurationFrame.FrameRate)
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
                If index + CommonFrameHeader.BinaryLength + 2 > buffer.Length Then
                    ' If not, save off remaining buffer to prepend onto next read
                    m_dataStream = New MemoryStream
                    m_dataStream.Write(buffer, index, buffer.Length - index)
                    Exit Do
                End If

                ' Parse frame header
                parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, buffer, index)

                ' Until we receive configuration frame, we at least expose part of frame we have parsed
                If m_configurationFrame Is Nothing Then RaiseReceivedCommonFrameHeader(parsedFrameHeader)

                ' See if there is enough data in the buffer to parse the entire frame
                If index + parsedFrameHeader.FrameLength > buffer.Length Then
                    ' If not, save off remaining buffer to prepend onto next read
                    m_dataStream = New MemoryStream
                    m_dataStream.Write(buffer, index, buffer.Length - index)
                    Exit Do
                End If

                RaiseEvent ReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, index, parsedFrameHeader.FrameLength)

                ' Entire frame is availble, so we go ahead and parse it
                Select Case parsedFrameHeader.FrameType
                    Case FrameType.DataFrame
                        ' We can only start parsing data frames once we have successfully received a configuration file...
                        If m_configurationFrame IsNot Nothing Then
                            RaiseReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame, buffer, index))
                        End If
                    Case FrameType.ConfigurationFrame
                        ' Cumulate all partial frames together as once complete frame
                        With DirectCast(parsedFrameHeader, CommonFrameHeader.CommonFrameHeaderInstance)
                            If .IsFirstFrame Then m_configurationFrameCollection = parsedFrameHeader

                            If m_configurationFrameCollection IsNot Nothing Then
                                Try
                                    m_configurationFrameCollection.AppendFrameImage(buffer, index, .FrameLength)

                                    If .IsLastFrame Then
                                        m_configurationFrame = New ConfigurationFrame(m_configurationFrameCollection, m_configurationFrameCollection.BinaryImage, 0)
                                        RaiseReceivedConfigurationFrame(m_configurationFrame)
                                        m_configurationFrameCollection = Nothing
                                    End If
                                Catch
                                    ' If CRC check or other exception occurs, we cancel frame cumulation process
                                    m_configurationFrameCollection = Nothing
                                    Throw
                                End Try
                            End If
                        End With
                    Case FrameType.HeaderFrame
                        ' Cumulate all partial frames together as once complete frame
                        With DirectCast(parsedFrameHeader, CommonFrameHeader.CommonFrameHeaderInstance)
                            If .IsFirstFrame Then m_headerFrameCollection = parsedFrameHeader

                            If m_headerFrameCollection IsNot Nothing Then
                                Try
                                    m_headerFrameCollection.AppendFrameImage(buffer, index, .FrameLength)

                                    If .IsLastFrame Then
                                        RaiseReceivedHeaderFrame(New HeaderFrame(m_headerFrameCollection, m_headerFrameCollection.BinaryImage, 0))
                                        m_headerFrameCollection = Nothing
                                    End If
                                Catch
                                    ' If CRC check or other exception occurs, we cancel frame cumulation process
                                    m_headerFrameCollection = Nothing
                                    Throw
                                End Try
                            End If
                        End With
                End Select

                index += parsedFrameHeader.FrameLength
            Loop

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
                If TypeOf value Is Ieee1344.ConfigurationFrame Then
                    m_configurationFrame = value
                Else
                    m_configurationFrame = New Ieee1344.ConfigurationFrame(value)
                End If
            End Set
        End Property

        Private Sub RaiseReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)

            RaiseEvent ReceivedCommonFrameHeader(frame)
            RaiseEvent IFrameParserReceivedUndeterminedFrame(frame)

        End Sub

        Private Sub RaiseReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)

            RaiseEvent ReceivedConfigurationFrame(frame)
            RaiseEvent IFrameParserReceivedConfigurationFrame(frame)

        End Sub

        Private Sub RaiseReceivedDataFrame(ByVal frame As DataFrame)

            RaiseEvent ReceivedDataFrame(frame)
            RaiseEvent IFrameParserReceivedDataFrame(frame)

        End Sub

        Private Sub RaiseReceivedHeaderFrame(ByVal frame As HeaderFrame)

            RaiseEvent ReceivedHeaderFrame(frame)
            RaiseEvent IFrameParserReceivedHeaderFrame(frame)

        End Sub

#End Region

    End Class

End Namespace
