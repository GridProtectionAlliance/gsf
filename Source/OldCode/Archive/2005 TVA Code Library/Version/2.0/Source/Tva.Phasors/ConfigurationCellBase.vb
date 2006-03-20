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
Imports Tva.Collections.Common
Imports Tva.Phasors.Common

' This class represents the protocol independent common implementation of a set of configuration related data settings that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public MustInherit Class ConfigurationCellBase

    Inherits ChannelCellBase
    Implements IConfigurationCell

    Private m_stationName As String
    Private m_idLabel As String
    Private m_phasorDefinitions As PhasorDefinitionCollection
    Private m_frequencyDefinition As IFrequencyDefinition
    Private m_analogDefinitions As AnalogDefinitionCollection
    Private m_digitalDefinitions As DigitalDefinitionCollection
    Private m_nominalFrequency As LineFrequency

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer)

        MyBase.New(parent, alignOnDWordBoundry)

        m_phasorDefinitions = New PhasorDefinitionCollection(maximumPhasors)
        m_analogDefinitions = New AnalogDefinitionCollection(maximumAnalogs)
        m_digitalDefinitions = New DigitalDefinitionCollection(maximumDigitals)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer)

        MyClass.New(parent, alignOnDWordBoundry, maximumPhasors, maximumAnalogs, maximumDigitals)
        MyClass.IDCode = idCode
        m_nominalFrequency = nominalFrequency

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Integer, ByVal maximumAnalogs As Integer, ByVal maximumDigitals As Integer, ByVal state As IConfigurationCellParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(parent, alignOnDWordBoundry, maximumPhasors, maximumAnalogs, maximumDigitals)
        ParseBinaryImage(state, binaryImage, startIndex)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency, ByVal stationName As String, ByVal idLabel As String, ByVal phasorDefinitions As PhasorDefinitionCollection, ByVal frequencyDefinition As IFrequencyDefinition, ByVal analogDefinitions As AnalogDefinitionCollection, ByVal digitalDefinitions As DigitalDefinitionCollection)

        MyBase.New(parent, alignOnDWordBoundry, idCode)

        m_nominalFrequency = nominalFrequency
        Me.StationName = stationName
        Me.IDLabel = idLabel
        m_phasorDefinitions = phasorDefinitions
        m_frequencyDefinition = frequencyDefinition
        m_analogDefinitions = analogDefinitions
        m_digitalDefinitions = digitalDefinitions

    End Sub

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

    ' Derived classes are expected to expose a Public Sub New(ByVal configurationCell As IConfigurationCell)
    Protected Sub New(ByVal configurationCell As IConfigurationCell)

        MyClass.New(configurationCell.Parent, configurationCell.AlignOnDWordBoundry, configurationCell.IDCode, configurationCell.NominalFrequency, _
            configurationCell.StationName, configurationCell.IDLabel, configurationCell.PhasorDefinitions, configurationCell.FrequencyDefinition, _
            configurationCell.AnalogDefinitions, configurationCell.DigitalDefinitions)

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
            If Len(Trim(value)) > MaximumStationNameLength Then
                Throw New OverflowException("Station name length cannot exceed " & MaximumStationNameLength)
            Else
                m_stationName = Trim(Replace(value, Chr(20), " "))
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

    Public Overridable Property IDLabel() As String Implements IConfigurationCell.IDLabel
        Get
            Return m_idLabel
        End Get
        Set(ByVal value As String)
            Dim length As Integer = Len(Trim(value))
            If length > IDLabelLength Then
                Throw New OverflowException("ID label must not be more than " & IDLabelLength & " characters in length")
            Else
                m_idLabel = value.PadRight(IDLabelLength)
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
            m_frequencyDefinition = value
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

    Public Overridable Property NominalFrequency() As LineFrequency Implements IConfigurationCell.NominalFrequency
        Get
            Return m_nominalFrequency
        End Get
        Set(ByVal value As LineFrequency)
            m_nominalFrequency = value
        End Set
    End Property

    Public Overridable ReadOnly Property FrameRate() As Int16 Implements IConfigurationCell.FrameRate
        Get
            Return Parent.FrameRate
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

    ' Only the station name is common to configuration frame headers in IEEE protocols
    Protected Overrides ReadOnly Property HeaderLength() As UInt16
        Get
            Return MaximumStationNameLength
        End Get
    End Property

    Protected Overrides ReadOnly Property HeaderImage() As Byte()
        Get
            Return StationNameImage
        End Get
    End Property

    Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        Dim length As Integer = Array.IndexOf(binaryImage, Convert.ToByte(0), startIndex, MaximumStationNameLength) - startIndex

        If length < 0 Then length = MaximumStationNameLength

        StationName = Encoding.ASCII.GetString(binaryImage, startIndex, length)

    End Sub

    ' Channel names of IEEE C37.118 and IEEE 1344 configuration frames are common in order and type - so they are defined in the base class
    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return m_phasorDefinitions.BinaryLength + m_analogDefinitions.BinaryLength + m_digitalDefinitions.BinaryLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)
            Dim index As Integer

            ' Copy in common cell images (channel names)
            CopyImage(m_phasorDefinitions, buffer, index)
            CopyImage(m_analogDefinitions, buffer, index)
            CopyImage(m_digitalDefinitions, buffer, index)

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)
        Dim x As Integer

        ' By the very nature of the IEEE protocols supporting the same order of phasor, analog and digital labels
        ' we are able to "automatically" parse this data out in the configuration cell base class - BEAUTIFUL!!!
        With parsingState
            For x = 0 To .PhasorCount - 1
                m_phasorDefinitions.Add(.CreateNewPhasorDefintionFunction.Invoke(Me, binaryImage, startIndex))
                startIndex += m_phasorDefinitions(x).MaximumLabelLength
            Next

            For x = 0 To .AnalogCount - 1
                m_analogDefinitions.Add(.CreateNewAnalogDefintionFunction.Invoke(Me, binaryImage, startIndex))
                startIndex += m_analogDefinitions(x).MaximumLabelLength
            Next

            For x = 0 To .DigitalCount - 1
                m_digitalDefinitions.Add(.CreateNewDigitalDefintionFunction.Invoke(Me, binaryImage, startIndex))
                startIndex += m_digitalDefinitions(x).MaximumLabelLength
            Next
        End With

    End Sub

    ' Footer for IEEE protocols contains nominal frequency definition, so we use this to initialize frequency definition
    Protected Overrides ReadOnly Property FooterLength() As UInt16
        Get
            Return 2
        End Get
    End Property

    Protected Overrides ReadOnly Property FooterImage() As Byte()
        Get
            Return m_frequencyDefinition.BinaryImage
        End Get
    End Property

    Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        m_frequencyDefinition = DirectCast(state, IConfigurationCellParsingState).CreateNewFrequencyDefintionFunction.Invoke(Me, binaryImage, startIndex)

    End Sub

End Class
