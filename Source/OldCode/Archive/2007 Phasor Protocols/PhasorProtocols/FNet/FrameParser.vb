'*******************************************************************************************************
'  FrameParser.vb - FNet Frame Parser
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
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
Imports TVA.Collections
Imports TVA.IO.Common
Imports TVA.Text.Common
Imports PhasorProtocols.Common
Imports PhasorProtocols.FNet.Common

Namespace FNet

    ''' <summary>This class parses an FNet binary data stream and returns parsed data via events</summary>
    ''' <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
    <CLSCompliant(False)> _
    Public Class FrameParser

        Inherits FrameParserBase

#Region " Public Member Declarations "

        ' We shadow base class events with their FNET specific derived versions for convinience in case users consume this class directly 

        ''' <summary>This event is raised when a virtual Configuration Frame has been created</summary>
        ''' <remarks>
        ''' <para>See Std IEEE 1344 for the definition of a configuration frame.  This FNET implementation defines a similar concept</para>
        ''' <para>Note that the FNET data steam does not contain a parsable configuration frame, but a virtual one is created on reception of the first data frame</para>
        ''' </remarks>
        Public Shadows Event ReceivedConfigurationFrame(ByVal frame As ConfigurationFrame)

        ''' <summary>This event is raised when a Data Frame has been parsed</summary>
        ''' <remarks>See Std IEEE 1344 for the definition of a data frame.  FNET uses a similar concept</remarks>
        Public Shadows Event ReceivedDataFrame(ByVal frame As DataFrame)

#End Region

#Region " Private Member Declarations "

        Private m_configurationFrame As ConfigurationFrame
        Private m_ticksOffset As Long
        Private m_frameRate As Int16
        Private m_nominalFrequency As LineFrequency
        Private m_stationName As String

#End Region

#Region " Construction Functions "

        Public Sub New()

            ' FNet devices default to 10 frames per second, 60Hz and 11 second time offset
            MyClass.New(DefaultFrameRate, DefaultNominalFrequency, DefaultTicksOffset)

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyClass.New(configurationFrame.FrameRate, DefaultNominalFrequency, DefaultTicksOffset)

            m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame)

            ' If abstract configuration frame has any cells, we use first cell's nominal frequency...
            If m_configurationFrame.Cells.Count > 0 Then m_nominalFrequency = configurationFrame.Cells(0).NominalFrequency

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame)

            MyClass.New(configurationFrame.FrameRate, configurationFrame.NominalFrequency, configurationFrame.TicksOffset)
            m_configurationFrame = configurationFrame

        End Sub

        Public Sub New(ByVal frameRate As Int16, ByVal nominalFrequency As LineFrequency, ByVal ticksOffset As Long)

            m_frameRate = frameRate
            m_nominalFrequency = nominalFrequency
            m_ticksOffset = ticksOffset

        End Sub

#End Region

#Region " Public Methods Implementation "

        Public Overrides ReadOnly Property ProtocolSyncByte() As Byte
            Get
                Return StartByte
            End Get
        End Property

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

        Public Property TicksOffset() As Long
            Get
                Return m_ticksOffset
            End Get
            Set(ByVal value As Long)
                m_ticksOffset = value
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

        Public Property StationName() As String
            Get
                Return m_stationName
            End Get
            Set(ByVal value As String)
                m_stationName = value
            End Set
        End Property

        Public Overrides Property ConnectionParameters() As IConnectionParameters
            Get
                Return MyBase.ConnectionParameters
            End Get
            Set(ByVal value As IConnectionParameters)
                Dim parameters As FNet.ConnectionParameters = TryCast(value, FNet.ConnectionParameters)

                If parameters IsNot Nothing Then
                    MyBase.ConnectionParameters = parameters

                    ' Assign new incoming connection parameter values
                    m_ticksOffset = parameters.TicksOffset
                    m_frameRate = parameters.FrameRate
                    m_nominalFrequency = parameters.NominalFrequency
                    m_stationName = parameters.StationName
                End If
            End Set
        End Property

#End Region

#Region " Protected Methods Implementation "

        Protected Overrides Sub ParseFrame(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer, ByRef parsedFrameLength As Integer)

            Dim startByteIndex As Integer = -1
            Dim endByteIndex As Integer = -1

            ' See if there is enough data in the buffer to parse the entire frame
            For x As Integer = offset To offset + length - 1
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

            ' If there was an entire frame to parse, begin actual parse sequence
            If endByteIndex > -1 Then
                ' Entire frame is available, so we go ahead and parse it
                RaiseReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, startByteIndex, endByteIndex - startByteIndex + 1)

                ' If no configuration frame has been created, we create one now
                If m_configurationFrame Is Nothing Then
                    ' Pre-parse first FNet data frame to get unit ID field and establish a virutal configuration frame
                    Dim data As String() = RemoveDuplicateWhiteSpace(Encoding.ASCII.GetString(buffer, startByteIndex + 1, endByteIndex - startByteIndex - 1)).Trim().Split(" "c)

                    ' Make sure all the needed data elements exist (could be a bad frame)
                    If data.Length >= 8 Then
                        ' Create virtual configuration frame
                        m_configurationFrame = New ConfigurationFrame(Convert.ToUInt16(data(Element.UnitID)), Date.Now.Ticks, m_frameRate, m_nominalFrequency, m_stationName, m_ticksOffset)

                        ' Notify clients of new configuration frame
                        RaiseReceivedConfigurationFrame(m_configurationFrame)
                    End If
                End If

                ' Provide new FNet data frame to clients
                RaiseReceivedDataFrame(New DataFrame(m_configurationFrame, buffer, startByteIndex))

                ' Define actual parsed frame length so base class can increment offset past end of data frame
                parsedFrameLength = endByteIndex - startByteIndex + 1
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

#End Region

#Region " Private Methods Implementation "

        Private Function CastToDerivedConfigurationFrame(ByVal configurationFrame As IConfigurationFrame) As ConfigurationFrame

            If TypeOf configurationFrame Is FNet.ConfigurationFrame Then
                Return configurationFrame
            Else
                Return New FNet.ConfigurationFrame(configurationFrame)
            End If

        End Function

#End Region

    End Class

End Namespace
