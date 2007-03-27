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
Imports TVA.Collections
Imports TVA.IO.Common
Imports TVA.Phasors.Common

Namespace Ieee1344

    ''' <summary>This class parses an IEEE 1344 binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits FrameParserBase

#Region " Public Member Declarations "

        ' We shadow base class events with their IEEE 1344 specific derived versions for convinience in case users consume this class directly
        Public Event ReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)
        Public Shadows Event ReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)
        Public Shadows Event ReceivedDataFrame(ByVal frame As DataFrame)
        Public Shadows Event ReceivedHeaderFrame(ByVal frame As HeaderFrame)

#End Region

#Region " Private Member Declarations "

        Private m_configurationFrame As ConfigurationFrame
        Private m_configurationFrameCollection As CommonFrameHeader.CommonFrameHeaderInstance
        Private m_headerFrameCollection As CommonFrameHeader.CommonFrameHeaderInstance

#End Region

#Region " Construction Functions "

        Public Sub New()

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame)

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame)

            m_configurationFrame = configurationFrame

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Overrides ReadOnly Property ProtocolUsesSyncByte() As Boolean
            Get
                Return False
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

#End Region

#Region " Protected Methods Implementation "

        Protected Overrides Sub ParseFrame(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer, ByRef parsedFrameLength As Integer)

            ' See if there is enough data in the buffer to parse the common frame header.
            ' Note that in order to get status flags (which contain frame length), we need at least two more bytes 
            If length >= CommonFrameHeader.BinaryLength + 2 Then
                ' Parse frame header
                Dim parsedFrameHeader As ICommonFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, buffer, offset)

                ' See if there is enough data in the buffer to parse the entire frame
                If length >= parsedFrameHeader.FrameLength Then
                    ' Expose the frame buffer image in case client needs this data for any reason
                    RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength)

                    ' Entire frame is availble, so we go ahead and parse it
                    Select Case parsedFrameHeader.FrameType
                        Case FrameType.DataFrame
                            If m_configurationFrame Is Nothing Then
                                ' Until we receive configuration frame, we at least expose the part of the data frame we have parsed
                                RaiseReceivedCommonFrameHeader(parsedFrameHeader)
                            Else
                                ' We can only start parsing data frames once we have successfully received a configuration frame...
                                RaiseReceivedDataFrame(New DataFrame(parsedFrameHeader, m_configurationFrame, buffer, offset))
                            End If
                        Case FrameType.ConfigurationFrame
                            ' Cumulate all partial frames together as one complete frame
                            With DirectCast(parsedFrameHeader, CommonFrameHeader.CommonFrameHeaderInstance)
                                If .IsFirstFrame Then m_configurationFrameCollection = parsedFrameHeader

                                If m_configurationFrameCollection IsNot Nothing Then
                                    Try
                                        m_configurationFrameCollection.AppendFrameImage(buffer, offset, .FrameLength)

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
                            ' Cumulate all partial frames together as one complete frame
                            With DirectCast(parsedFrameHeader, CommonFrameHeader.CommonFrameHeaderInstance)
                                If .IsFirstFrame Then m_headerFrameCollection = parsedFrameHeader

                                If m_headerFrameCollection IsNot Nothing Then
                                    Try
                                        m_headerFrameCollection.AppendFrameImage(buffer, offset, .FrameLength)

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

                    parsedFrameLength = parsedFrameHeader.FrameLength
                End If
            End If

        End Sub

        ' We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
        Protected Overrides Sub RaiseReceivedConfigurationFrame(ByVal frame As IConfigurationFrame)

            MyBase.RaiseReceivedConfigurationFrame(frame)
            RaiseEvent ReceivedConfigurationFrame(frame)

        End Sub

        Protected Overrides Sub RaiseReceivedDataFrame(ByVal frame As IDataFrame)

            MyBase.RaiseReceivedDataFrame(frame)
            RaiseEvent ReceivedDataFrame(frame)

        End Sub

        Protected Overrides Sub RaiseReceivedHeaderFrame(ByVal frame As IHeaderFrame)

            MyBase.RaiseReceivedHeaderFrame(frame)
            RaiseEvent ReceivedHeaderFrame(frame)

        End Sub

#End Region

#Region " Private Methods Implementation "

        Private Sub RaiseReceivedCommonFrameHeader(ByVal frame As ICommonFrameHeader)

            MyBase.RaiseReceivedUndeterminedFrame(frame)
            RaiseEvent ReceivedCommonFrameHeader(frame)

        End Sub

        Private Function CastToDerivedConfigurationFrame(ByVal configurationFrame As IConfigurationFrame) As ConfigurationFrame

            If TypeOf configurationFrame Is Ieee1344.ConfigurationFrame Then
                Return configurationFrame
            Else
                Return New Ieee1344.ConfigurationFrame(configurationFrame)
            End If

        End Function

#End Region

    End Class

End Namespace
