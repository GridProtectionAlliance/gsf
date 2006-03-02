'*******************************************************************************************************
'  ConfigurationCellBase.vb - Configuration cell base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer
Imports System.Text
Imports Tva.Interop
Imports Tva.Phasors.Common

' This class represents the protocol independent common implementation of a set of configuration related data settings that can be sent or received from a PMU.
Public MustInherit Class ConfigurationCellBase

    Inherits ChannelCellBase(Of IConfigurationCell)
    Implements IConfigurationCell

    Private m_stationName As String
    Private m_idCode As Int16
    Private m_idLabel As String
    Private m_phasorDefinitions As PhasorDefinitionCollection
    Private m_frequencyDefinition As IFrequencyDefinition
    Private m_analogDefinitions As AnalogDefinitionCollection
    Private m_digitalDefinitions As DigitalDefinitionCollection
    Private m_sampleRate As Int16

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer)

        MyBase.New(parent, alignOnDWordBoundry)

        m_phasorDefinitions = New PhasorDefinitionCollection(maximumPhasors)
        m_analogDefinitions = New AnalogDefinitionCollection(maximumAnalogs)
        m_digitalDefinitions = New DigitalDefinitionCollection(maximumDigitals)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer, ByVal state As IConfigurationCellParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(parent, alignOnDWordBoundry, maximumPhasors, maximumAnalogs, maximumDigitals)
        ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal stationName As String, ByVal idCode As Int16, ByVal idLabel As String, ByVal phasorDefinitions As PhasorDefinitionCollection, ByVal frequencyDefinition As IFrequencyDefinition, ByVal analogDefinitions As AnalogDefinitionCollection, ByVal digitalDefinitions As DigitalDefinitionCollection, ByVal sampleRate As Int16)

        MyBase.New(parent, alignOnDWordBoundry)

        Me.StationName = stationName
        m_idCode = idCode
        Me.IDLabel = idLabel
        m_phasorDefinitions = phasorDefinitions
        m_frequencyDefinition = frequencyDefinition
        m_analogDefinitions = analogDefinitions
        m_digitalDefinitions = digitalDefinitions
        m_sampleRate = sampleRate

    End Sub

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

    ' Derived classes are expected to expose a Public Sub New(ByVal configurationCell As IConfigurationCell)
    Protected Sub New(ByVal configurationCell As IConfigurationCell)

        MyClass.New(configurationCell.Parent, configurationCell.AlignOnDWordBoundry, configurationCell.StationName, configurationCell.IDCode, _
            configurationCell.IDLabel, configurationCell.PhasorDefinitions, configurationCell.FrequencyDefinition, _
            configurationCell.AnalogDefinitions, configurationCell.DigitalDefinitions, configurationCell.SampleRate)

    End Sub

    Public Overridable Shadows ReadOnly Property Parent() As IConfigurationFrame Implements IConfigurationCell.Parent
        Get
            Return MyBase.Parent
        End Get
    End Property

    Public Overridable Property StationName() As String Implements IConfigurationCell.StationName
        Get
            Return m_stationName
        End Get
        Set(ByVal value As String)
            If Len(Trim(Value)) > MaximumStationNameLength Then
                Throw New OverflowException("Station name length cannot exceed " & MaximumStationNameLength)
            Else
                m_stationName = Trim(Replace(Value, Chr(20), " "))
            End If
        End Set
    End Property

    Public Overridable ReadOnly Property StationNameImage() As Byte() Implements IConfigurationCell.StationNameImage
        Get
            Return Encoding.ASCII.GetBytes(m_stationName.PadRight(MaximumStationNameLength))
        End Get
    End Property

    Public Overridable ReadOnly Property MaximumStationNameLength() As Integer Implements IConfigurationCell.MaximumStationNameLength
        Get
            ' Typical station name length is 16 characters
            Return 16
        End Get
    End Property

    Public Overridable Property IDCode() As Int16 Implements IConfigurationCell.IDCode
        Get
            Return m_idCode
        End Get
        Set(ByVal value As Int16)
            m_idCode = Value
        End Set
    End Property

    Public Overridable Property IDLabel() As String Implements IConfigurationCell.IDLabel
        Get
            Return m_idLabel
        End Get
        Set(ByVal value As String)
            Dim length As Integer = Len(Trim(Value))
            If length > IDLabelLength Then
                Throw New OverflowException("ID label must be less than " & IDLabelLength & " characters in length")
            Else
                m_idLabel = Value.PadRight(IDLabelLength)
            End If
        End Set
    End Property

    Public Overridable ReadOnly Property IDLabelImage() As Byte() Implements IConfigurationCell.IDLabelImage
        Get
            Return Encoding.ASCII.GetBytes(m_idLabel)
        End Get
    End Property

    Public Overridable ReadOnly Property IDLabelLength() As Integer Implements IConfigurationCell.IDLabelLength
        Get
            ' ID label length is 4 characters
            Return 4
        End Get
    End Property

    Public Overridable ReadOnly Property PhasorDefinitions() As PhasorDefinitionCollection Implements IConfigurationCell.PhasorDefinitions
        Get
            Return m_phasorDefinitions
        End Get
    End Property

    Public Overridable Property FrequencyDefinition() As IFrequencyDefinition Implements IConfigurationCell.FrequencyDefinition
        Get
            Return m_frequencyDefinition
        End Get
        Set(ByVal value As IFrequencyDefinition)
            m_frequencyDefinition = Value
        End Set
    End Property

    Public Overridable ReadOnly Property AnalogDefinitions() As AnalogDefinitionCollection Implements IConfigurationCell.AnalogDefinitions
        Get
            Return m_analogDefinitions
        End Get
    End Property

    Public Overridable ReadOnly Property DigitalDefinitions() As DigitalDefinitionCollection Implements IConfigurationCell.DigitalDefinitions
        Get
            Return m_digitalDefinitions
        End Get
    End Property

    Public Overridable ReadOnly Property SampleRate() As Int16 Implements IConfigurationCell.SampleRate
        Get
            Return Parent.SampleRate
        End Get
    End Property

    Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

        ' We sort configuration cells by ID code...
        If TypeOf obj Is IConfigurationCell Then
            Return IDCode.CompareTo(DirectCast(obj, IConfigurationCell).IDCode)
        Else
            Throw New ArgumentException("ConfigurationCell can only be compared to other ConfigurationCells")
        End If

    End Function

    Protected Overrides ReadOnly Property BodyLength() As Int16
        Get
            ' TODO: Make this body length specific
            Return m_phasorDefinitions.BinaryLength + m_analogDefinitions.BinaryLength + m_digitalDefinitions.BinaryLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)
            Dim index As Integer

            ' Copy in common cell images
            ' TODO: copy in only for cell body
            CopyImage(m_phasorDefinitions, buffer, index)
            CopyImage(m_analogDefinitions, buffer, index)
            CopyImage(m_digitalDefinitions, buffer, index)

            Return buffer
        End Get
    End Property

    Protected Overrides ReadOnly Property FooterLength() As Short
        Get
            Return 2 + 4 * (m_phasorDefinitions.Count + m_analogDefinitions.Count + m_digitalDefinitions.Count)
        End Get
    End Property

    Protected Overrides ReadOnly Property FooterImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)
            Dim index As Integer

            ' Copy in common cell images
            ' TODO: copy in only for cell footer
            CopyImage(m_phasorDefinitions, buffer, index)
            CopyImage(m_analogDefinitions, buffer, index)
            CopyImage(m_digitalDefinitions, buffer, index)

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        Dim parsingState As IConfigurationCellParsingState = state
        Dim x As Integer

        With parsingState
            For x = 0 To .PhasorCount - 1
                m_phasorDefinitions.Add(Activator.CreateInstance(.PhasorDefinitionType, New Object() {Me, binaryImage, startIndex}))
                'startIndex += m_phasorDefinitions(x).BodyLength
                startIndex += m_phasorDefinitions(x).MaximumLabelLength
            Next

            For x = 0 To .AnalogCount - 1
                m_analogDefinitions.Add(Activator.CreateInstance(.AnalogDefinitionType, New Object() {Me, binaryImage, startIndex}))
                'startIndex += m_analogDefinitions(x).BodyLength
                startIndex += m_analogDefinitions(x).MaximumLabelLength
            Next

            For x = 0 To .DigitalCount - 1
                m_digitalDefinitions.Add(Activator.CreateInstance(.DigitalDefinitionType, New Object() {Me, binaryImage, startIndex}))
                'startIndex += m_digitalDefinitions(x).BodyLength
                startIndex += 16 * m_digitalDefinitions(x).MaximumLabelLength
            Next
        End With

    End Sub

    Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

        Dim parsingState As IConfigurationCellParsingState = state

        ' TODO: Parse out conversion factors and digital status words
        m_frequencyDefinition = Activator.CreateInstance(parsingState.FrequencyDefinitionType, New Object() {Me, binaryImage, startIndex + 4 * (m_phasorDefinitions.Count + m_analogDefinitions.Count + m_digitalDefinitions.Count)})

    End Sub

End Class
