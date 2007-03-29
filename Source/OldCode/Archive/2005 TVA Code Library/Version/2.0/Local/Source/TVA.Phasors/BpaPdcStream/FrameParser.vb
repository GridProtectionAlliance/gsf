'*******************************************************************************************************
'  FrameParser.vb - BPA PDCstream Frame Parser
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
Imports TVA.Collections
Imports TVA.IO.Common
Imports TVA.IO.FilePath
Imports TVA.DateTime.Common
Imports TVA.Phasors.Common

Namespace Phasors.BpaPdcStream

    ''' <summary>This class parses a BPA PDC binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits FrameParserBase

#Region " Public Member Declarations "

        ' We shadow base class events with their BPA PDCstream specific derived versions for convinience in case users consume this class directly
        Public Event ReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)
        Public Shadows Event ReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)
        Public Shadows Event ReceivedDataFrame(ByVal frame As DataFrame)

#End Region

#Region " Private Member Declarations "

        Private m_configurationFrame As ConfigurationFrame
        Private m_configurationFileName As String
        Private m_refreshConfigurationFileOnChange As Boolean
        Private m_parseWordCountFromByte As Boolean
        Private m_lastUpdateNotification As Long
        Private WithEvents m_configurationFileWatcher As FileSystemWatcher

#End Region

#Region " Construction Functions "

        Public Sub New()
        End Sub

        Public Sub New(ByVal configurationFileName As String)

            m_configurationFileName = configurationFileName
            m_refreshConfigurationFileOnChange = True
            ResetFileWatcher()

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame)
            m_configurationFileName = m_configurationFrame.ConfigurationFileName
            m_refreshConfigurationFileOnChange = True
            ResetFileWatcher()

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame)

            m_configurationFrame = configurationFrame
            m_configurationFileName = m_configurationFrame.ConfigurationFileName
            m_refreshConfigurationFileOnChange = True
            ResetFileWatcher()

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Overrides ReadOnly Property ProtocolUsesSyncByte() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides Property ConfigurationFrame() As IConfigurationFrame
            Get
                Return m_configurationFrame
            End Get
            Set(ByVal value As IConfigurationFrame)
                m_configurationFrame = CastToDerivedConfigurationFrame(value)
            End Set
        End Property

        Public Property ConfigurationFileName() As String
            Get
                If m_configurationFrame Is Nothing Then
                    Return m_configurationFrame.ConfigurationFileName
                Else
                    Return m_configurationFileName
                End If
            End Get
            Set(ByVal value As String)
                m_configurationFileName = value
                ResetFileWatcher()
            End Set
        End Property

        ''' <summary>Set to True to automatically reload configuration file when it has changed on disk</summary>
        Public Property RefreshConfigurationFileOnChange() As Boolean
            Get
                Return m_refreshConfigurationFileOnChange
            End Get
            Set(ByVal value As Boolean)
                m_refreshConfigurationFileOnChange = value
                ResetFileWatcher()
            End Set
        End Property

        ''' <summary>
        ''' Set to True to interpret word count in packet header from a byte instead of a word - if the sync byte
        ''' (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC
        ''' stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the
        ''' frame length, setting this property to True will correctly interpret the word count.
        ''' </summary>
        Public Property ParseWordCountFromByte() As Boolean
            Get
                Return m_parseWordCountFromByte
            End Get
            Set(ByVal value As Boolean)
                m_parseWordCountFromByte = value
            End Set
        End Property

        Public Overrides ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("    INI configuration file: ")
                    .Append(m_configurationFileName)
                    .Append(Environment.NewLine)
                    If m_configurationFrame IsNot Nothing Then
                        .Append("       BPA PDC stream type: ")
                        .Append([Enum].GetName(GetType(StreamType), m_configurationFrame.StreamType))
                        .Append(Environment.NewLine)
                        .Append("   BPA PDC revision number: ")
                        .Append([Enum].GetName(GetType(RevisionNumber), m_configurationFrame.RevisionNumber))
                        .Append(Environment.NewLine)
                    End If
                    .Append(MyBase.Status)

                    Return .ToString()
                End With
            End Get
        End Property

        Public Overrides Property ConnectionParameters() As IConnectionParameters
            Get
                Return MyBase.ConnectionParameters
            End Get
            Set(ByVal value As IConnectionParameters)
                Dim parameters As BpaPdcStream.ConnectionParameters = TryCast(value, BpaPdcStream.ConnectionParameters)

                If parameters IsNot Nothing Then
                    MyBase.ConnectionParameters = parameters

                    ' Assign new incoming connection parameter values
                    m_configurationFileName = parameters.ConfigurationFileName
                    m_parseWordCountFromByte = parameters.ParseWordCountFromByte
                    m_refreshConfigurationFileOnChange = parameters.RefreshConfigurationFileOnChange
                    ResetFileWatcher()
                End If
            End Set
        End Property

#End Region

#Region " Protected Methods Implementation "

        Protected Overrides Sub ParseFrame(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer, ByRef parsedFrameLength As Integer)

            ' See if there is enough data in the buffer to parse the common frame header.
            ' Note that in order to get time tag for data frames, we'll need at least six more bytes 
            If length >= CommonFrameHeader.BinaryLength + 6 Then
                ' Parse frame header
                Dim parsedFrameHeader As ICommonFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, m_parseWordCountFromByte, buffer, offset)

                ' See if there is enough data in the buffer to parse the entire frame
                If length >= parsedFrameHeader.FrameLength Then
                    ' Expose the frame buffer image in case client needs this data for any reason
                    RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength)

                    ' Entire frame is available, so we go ahead and parse it
                    Select Case parsedFrameHeader.FrameType
                        Case FrameType.DataFrame
                            If m_configurationFrame Is Nothing Then
                                ' Until we receive configuration frame, we at least expose the part of the frame we have parsed
                                RaiseReceivedCommonFrameHeader(parsedFrameHeader)
                            Else
                                ' We can only start parsing data frames once we have successfully received a configuration frame...
                                RaiseReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame, buffer, offset))
                            End If
                        Case FrameType.ConfigurationFrame
                            ' Parse new configuration frame
                            With New ConfigurationFrame(parsedFrameHeader, m_configurationFileName, buffer, offset)
                                m_configurationFrame = .This
                                RaiseReceivedConfigurationFrame(.This)
                            End With
                    End Select

                    parsedFrameLength = parsedFrameHeader.FrameLength
                End If
            End If

        End Sub

        ' We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
        Protected Overrides Sub RaiseReceivedDataFrame(ByVal frame As IDataFrame)

            MyBase.RaiseReceivedDataFrame(frame)
            RaiseEvent ReceivedDataFrame(frame)

        End Sub

        Protected Overrides Sub RaiseReceivedConfigurationFrame(ByVal frame As IConfigurationFrame)

            MyBase.RaiseReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame(frame)

        End Sub

#End Region

#Region " Private Methods Implementation "

        Private Sub RaiseReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)

            MyBase.RaiseReceivedUndeterminedFrame(frame)
            RaiseEvent ReceivedCommonFrameHeader(frame)

        End Sub

        Private Function CastToDerivedConfigurationFrame(ByVal configurationFrame As IConfigurationFrame) As ConfigurationFrame

            If TypeOf configurationFrame Is BpaPdcStream.ConfigurationFrame Then
                Return configurationFrame
            Else
                Return New BpaPdcStream.ConfigurationFrame(configurationFrame)
            End If

        End Function

        Private Sub m_configurationFileWatcher_Changed(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles m_configurationFileWatcher.Changed

            ' File watcher sends several notifications for file change - we only want to report one,
            ' so we ignore repeated file change notifications that occur within 1/2 a second
            If m_lastUpdateNotification = 0 OrElse TicksToSeconds(Date.Now.Ticks - m_lastUpdateNotification) > 0.5 Then
                If m_configurationFrame IsNot Nothing Then m_configurationFrame.Refresh()
                RaiseConfigurationChangeDetected()
            End If

            m_lastUpdateNotification = Date.Now.Ticks

        End Sub

        Private Sub ResetFileWatcher()

            If m_refreshConfigurationFileOnChange AndAlso Not String.IsNullOrEmpty(m_configurationFileName) AndAlso File.Exists(m_configurationFileName) Then
                Try
                    ' Create a new file watcher for configuration file - we'll automatically refresh configuration file
                    ' when this file gets updated...
                    m_configurationFileWatcher = New FileSystemWatcher(JustPath(m_configurationFileName), JustFileName(m_configurationFileName))
                    m_configurationFileWatcher.EnableRaisingEvents = True
                    m_configurationFileWatcher.IncludeSubdirectories = False
                    m_configurationFileWatcher.NotifyFilter = NotifyFilters.LastWrite
                Catch ex As Exception
                    RaiseDataStreamException(ex)
                End Try
            Else
                m_configurationFileWatcher = Nothing
            End If

        End Sub

#End Region

    End Class

End Namespace
