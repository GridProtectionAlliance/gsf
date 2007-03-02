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

        Inherits FrameParserBase

#Region " Public Member Declarations "

        ' We shadow base class events with their IEEE C37.118 specific derived versions for convinience in case users consume this class directly
        Public Event ReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)
        Public Event ReceivedConfigurationFrame1(ByVal frame As ConfigurationFrame)
        Public Event ReceivedConfigurationFrame2(ByVal frame As ConfigurationFrame)
        Public Shadows Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Shadows Event ReceivedHeaderFrame(ByVal frame As HeaderFrame)
        Public Shadows Event ReceivedCommandFrame(ByVal frame As CommandFrame)

#End Region

#Region " Private Member Declarations "

        Private m_draftRevision As DraftRevision
        Private m_configurationFrame2 As ConfigurationFrame
        Private m_configurationChangeHandled As Boolean

#End Region

#Region " Construction Functions "

        Public Sub New()

            m_draftRevision = IeeeC37_118.DraftRevision.Draft7

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyClass.New()
            m_configurationFrame2 = CastToDerivedConfigurationFrame(configurationFrame)

        End Sub

        Public Sub New(ByVal draftRevision As DraftRevision)

            m_draftRevision = draftRevision

        End Sub

        Public Sub New(ByVal draftRevision As DraftRevision, ByVal configurationFrame2 As ConfigurationFrame)

            m_draftRevision = draftRevision
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

        Public ReadOnly Property TimeBase() As Int32
            Get
                If m_configurationFrame2 Is Nothing Then
                    Return 0
                Else
                    Return m_configurationFrame2.TimeBase
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property ProtocolUsesSyncByte() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides Property ConfigurationFrame() As IConfigurationFrame
            Get
                Return m_configurationFrame2
            End Get
            Set(ByVal value As IConfigurationFrame)
                m_configurationFrame2 = CastToDerivedConfigurationFrame(value)
            End Set
        End Property

        Public Overrides Sub Start()

            m_configurationChangeHandled = False
            MyBase.Start()

        End Sub

        Public Overrides ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("IEEEC37.118 draft revision: ")
                    .Append([Enum].GetName(GetType(DraftRevision), m_draftRevision))
                    .Append(Environment.NewLine)
                    .Append("         Current time base: ")
                    .Append(TimeBase)
                    .Append(Environment.NewLine)
                    .Append(MyBase.Status)

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

#Region " Protected Methods Implementation "

        Protected Overrides Sub ParseFrame(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer, ByRef parsedFrameLength As Integer)

            ' See if there is enough data in the buffer to parse the common frame header
            If length >= CommonFrameHeader.BinaryLength Then
                ' Parse frame header
                Dim parsedFrameHeader As ICommonFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame2, buffer, offset)

                ' See if there is enough data in the buffer to parse the entire frame
                If length >= parsedFrameHeader.FrameLength Then
                    ' Expose the frame buffer image in case client needs this data for any reason
                    RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength)

                    ' Entire frame is available, so we go ahead and parse it
                    Select Case parsedFrameHeader.FrameType
                        Case FrameType.DataFrame
                            If m_configurationFrame2 Is Nothing Then
                                ' Until we receive configuration frame 2, we at least expose the part of the frame we have parsed
                                RaiseReceivedCommonFrameHeader(parsedFrameHeader)
                            Else
                                ' We can only start parsing data frames once we have successfully received a configuration frame 2...
                                RaiseReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame2, buffer, offset))
                            End If
                        Case FrameType.ConfigurationFrame2
                            Select Case m_draftRevision
                                Case DraftRevision.Draft6
                                    With New ConfigurationFrameDraft6(parsedFrameHeader, buffer, offset)
                                        m_configurationFrame2 = .This
                                        RaiseReceivedConfigurationFrame2(.This)
                                    End With
                                Case DraftRevision.Draft7
                                    With New ConfigurationFrame(parsedFrameHeader, buffer, offset)
                                        m_configurationFrame2 = .This
                                        RaiseReceivedConfigurationFrame2(.This)
                                    End With
                            End Select
                        Case FrameType.ConfigurationFrame1
                            Select Case m_draftRevision
                                Case DraftRevision.Draft6
                                    RaiseReceivedConfigurationFrame1(New ConfigurationFrameDraft6(parsedFrameHeader, buffer, offset))
                                Case DraftRevision.Draft7
                                    RaiseReceivedConfigurationFrame1(New ConfigurationFrame(parsedFrameHeader, buffer, offset))
                            End Select
                        Case FrameType.HeaderFrame
                            RaiseReceivedHeaderFrame(New HeaderFrame(parsedFrameHeader, buffer, offset))
                        Case FrameType.CommandFrame
                            RaiseReceivedCommandFrame(New CommandFrame(parsedFrameHeader, buffer, offset))
                    End Select

                    parsedFrameLength = parsedFrameHeader.FrameLength
                End If
            End If

        End Sub

        ' We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
        Protected Overrides Sub RaiseReceivedDataFrame(ByVal frame As IDataFrame)

            MyBase.RaiseReceivedDataFrame(frame)
            RaiseEvent ReceivedDataFrame(frame)

            Dim configurationChangeDetected As Boolean

            With DirectCast(frame.Cells, DataCellCollection)
                For x As Integer = 0 To .Count - 1
                    If .Item(x).ConfigurationChangeDetected Then
                        configurationChangeDetected = True

                        ' Configuration change detection flag should terminate after one minute, but
                        ' we only want to send a single notification
                        If Not m_configurationChangeHandled Then
                            m_configurationChangeHandled = True
                            MyBase.RaiseConfigurationChangeDetected()
                        End If

                        Exit For
                    End If
                Next
            End With

            If Not configurationChangeDetected Then m_configurationChangeHandled = False

        End Sub

        Protected Overrides Sub RaiseReceivedHeaderFrame(ByVal frame As IHeaderFrame)

            MyBase.RaiseReceivedHeaderFrame(frame)
            RaiseEvent ReceivedHeaderFrame(frame)

        End Sub

        Protected Overrides Sub RaiseReceivedCommandFrame(ByVal frame As ICommandFrame)

            MyBase.RaiseReceivedCommandFrame(frame)
            RaiseEvent ReceivedCommandFrame(frame)

        End Sub

#End Region

#Region " Private Methods Implementation "

        Private Sub RaiseReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)

            MyBase.RaiseReceivedUndeterminedFrame(frame)
            RaiseEvent ReceivedCommonFrameHeader(frame)

        End Sub

        Private Sub RaiseReceivedConfigurationFrame1(ByVal frame As ConfigurationFrame)

            MyBase.RaiseReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame1(frame)

        End Sub

        Private Sub RaiseReceivedConfigurationFrame2(ByVal frame As ConfigurationFrame)

            MyBase.RaiseReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame2(frame)

        End Sub

        Private Function CastToDerivedConfigurationFrame(ByVal configurationFrame As IConfigurationFrame) As ConfigurationFrame

            If TypeOf configurationFrame Is IeeeC37_118.ConfigurationFrame Then
                Return configurationFrame
            Else
                Return New IeeeC37_118.ConfigurationFrame(configurationFrame)
            End If

        End Function

#End Region

    End Class

End Namespace