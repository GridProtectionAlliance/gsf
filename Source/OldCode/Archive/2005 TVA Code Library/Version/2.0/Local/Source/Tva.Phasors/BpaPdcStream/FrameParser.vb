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
Imports Tva.Collections
Imports Tva.IO.Common
Imports Tva.Phasors.Common

Namespace BpaPdcStream

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

#End Region

#Region " Construction Functions "

        Public Sub New()
        End Sub

        Public Sub New(ByVal configurationFileName As String)

            m_configurationFileName = configurationFileName

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame)
            m_configurationFileName = m_configurationFrame.ConfigurationFileName

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame)

            m_configurationFrame = configurationFrame
            m_configurationFileName = m_configurationFrame.ConfigurationFileName

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
            End Set
        End Property

        Public Overrides ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("    INI configuration file: ")
                    .Append(m_configurationFrame.ConfigurationFileName)
                    .Append(Environment.NewLine)
                    .Append("       BPA PDC stream type: ")
                    .Append([Enum].GetName(GetType(StreamType), m_configurationFrame.ConfigurationFileName))
                    .Append(Environment.NewLine)
                    .Append("   BPA PDC revision number: ")
                    .Append([Enum].GetName(GetType(RevisionNumber), m_configurationFrame.RevisionNumber))
                    .Append(Environment.NewLine)
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
                End If
            End Set
        End Property

#End Region

#Region " Protected Methods Implementation "

        Protected Overrides Sub ParseFrame(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer, ByRef parsedFrameLength As Integer)

            ' See if there is enough data in the buffer to parse the common frame header
            If length >= CommonFrameHeader.BinaryLength Then
                ' Parse frame header
                Dim parsedFrameHeader As ICommonFrameHeader = CommonFrameHeader.ParseBinaryImage(buffer, offset)

                ' See if there is enough data in the buffer to parse the entire frame
                If length >= parsedFrameHeader.FrameLength Then
                    ' Expose the frame buffer image in case client needs this data for any reason
                    RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength)

                    ' Entire frame is available, so we go ahead and parse it
                    Select Case parsedFrameHeader.FrameType
                        Case FrameType.DataFrame
                            If m_configurationFrame Is Nothing Then
                                ' Until we receive configuration frame 2, we at least expose the part of the frame we have parsed
                                RaiseReceivedCommonFrameHeader(parsedFrameHeader)
                            Else
                                ' We can only start parsing data frames once we have successfully received a configuration frame 2...
                                RaiseReceivedDataFrame(New DataFrame(m_configurationFrame, buffer, offset))
                            End If
                        Case FrameType.ConfigurationFrame
                            If m_configurationFrame Is Nothing OrElse CompareBuffers(buffer, offset, parsedFrameHeader.FrameLength, m_configurationFrame.BinaryImage, 0, m_configurationFrame.BinaryLength) <> 0 Then
                                If m_configurationFrame IsNot Nothing Then MyBase.RaiseConfigurationChangeDetected()
                                With New ConfigurationFrame(m_configurationFileName, buffer, offset)
                                    m_configurationFrame = .This
                                    RaiseReceivedConfigurationFrame(.This)
                                End With
                            End If
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

            If TypeOf configurationFrame Is IeeeC37_118.ConfigurationFrame Then
                Return configurationFrame
            Else
                Return New BpaPdcStream.ConfigurationFrame(configurationFrame)
            End If

        End Function

#End Region

    End Class

End Namespace
