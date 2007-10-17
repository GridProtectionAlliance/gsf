'*******************************************************************************************************
'  ConfigurationCellBase.vb - Configuration cell base class
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

Imports System.Runtime.Serialization
Imports System.Buffer
Imports System.Text
Imports PhasorProtocols.Common
Imports TVA.Text.Common

''' <summary>This class represents the protocol independent common implementation of a set of configuration related data settings that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
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
    Private m_revisionCount As UInt16

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize configuration cell values
        Me.StationName = info.GetString("stationName")
        Me.IDLabel = info.GetString("idLabel")
        m_phasorDefinitions = info.GetValue("phasorDefinitions", GetType(PhasorDefinitionCollection))
        m_frequencyDefinition = info.GetValue("frequencyDefinition", GetType(IFrequencyDefinition))
        m_analogDefinitions = info.GetValue("analogDefinitions", GetType(AnalogDefinitionCollection))
        m_digitalDefinitions = info.GetValue("digitalDefinitions", GetType(DigitalDefinitionCollection))
        m_nominalFrequency = info.GetValue("nominalFrequency", GetType(LineFrequency))
        m_revisionCount = info.GetUInt16("revisionCount")

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Int32, ByVal maximumAnalogs As Int32, ByVal maximumDigitals As Int32)

        MyBase.New(parent, alignOnDWordBoundry)

        m_phasorDefinitions = New PhasorDefinitionCollection(maximumPhasors)
        m_analogDefinitions = New AnalogDefinitionCollection(maximumAnalogs)
        m_digitalDefinitions = New DigitalDefinitionCollection(maximumDigitals)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency, ByVal maximumPhasors As Int32, ByVal maximumAnalogs As Int32, ByVal maximumDigitals As Int32)

        MyClass.New(parent, alignOnDWordBoundry, maximumPhasors, maximumAnalogs, maximumDigitals)
        MyClass.IDCode = idCode
        m_nominalFrequency = nominalFrequency

    End Sub

    Protected Sub New(ByVal parent As IConfigurationFrame, ByVal alignOnDWordBoundry As Boolean, ByVal maximumPhasors As Int32, ByVal maximumAnalogs As Int32, ByVal maximumDigitals As Int32, ByVal state As IConfigurationCellParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

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

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

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
            If String.IsNullOrEmpty(value) Then value = "undefined"

            value = value.Trim()

            If value.Length > MaximumStationNameLength Then
                Throw New OverflowException("Station name length cannot exceed " & MaximumStationNameLength)
            Else
                m_stationName = GetValidLabel(value)
            End If
        End Set
    End Property

    Public Overridable ReadOnly Property StationNameImage() As Byte() Implements IConfigurationCell.StationNameImage
        Get
            Return Encoding.ASCII.GetBytes(m_stationName.PadRight(MaximumStationNameLength))
        End Get
    End Property

    Public Overridable ReadOnly Property MaximumStationNameLength() As Int32 Implements IConfigurationCell.MaximumStationNameLength
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
            If value Is Nothing Then value = ""

            If value.Trim.Length > IDLabelLength Then
                Throw New OverflowException("ID label must not be more than " & IDLabelLength & " characters in length")
            Else
                m_idLabel = GetValidLabel(value).Trim()
            End If
        End Set
    End Property

    Public Overridable ReadOnly Property IDLabelImage() As Byte() Implements IConfigurationCell.IDLabelImage
        Get
            If IDLabelLength < Int32.MaxValue Then
                Return Encoding.ASCII.GetBytes(m_idLabel.PadRight(IDLabelLength))
            Else
                Return Encoding.ASCII.GetBytes(m_idLabel)
            End If
        End Get
    End Property

    Public Overridable ReadOnly Property IDLabelLength() As Int32 Implements IConfigurationCell.IDLabelLength
        Get
            ' We don't restrict this for most protocols...
            Return Int32.MaxValue
        End Get
    End Property

    Public Overridable ReadOnly Property PhasorDefinitions() As PhasorDefinitionCollection Implements IConfigurationCell.PhasorDefinitions
        Get
            Return m_phasorDefinitions
        End Get
    End Property

    Public MustOverride Property PhasorCoordinateFormat() As CoordinateFormat Implements IConfigurationCell.PhasorCoordinateFormat

    Public MustOverride Property PhasorDataFormat() As DataFormat Implements IConfigurationCell.PhasorDataFormat

    Public Overridable Property FrequencyDefinition() As IFrequencyDefinition Implements IConfigurationCell.FrequencyDefinition
        Get
            Return m_frequencyDefinition
        End Get
        Set(ByVal value As IFrequencyDefinition)
            m_frequencyDefinition = value
        End Set
    End Property

    Public MustOverride Property FrequencyDataFormat() As DataFormat Implements IConfigurationCell.FrequencyDataFormat

    Public Overridable ReadOnly Property AnalogDefinitions() As AnalogDefinitionCollection Implements IConfigurationCell.AnalogDefinitions
        Get
            Return m_analogDefinitions
        End Get
    End Property

    Public MustOverride Property AnalogDataFormat() As DataFormat Implements IConfigurationCell.AnalogDataFormat

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

    Public Overridable Property RevisionCount() As UInt16 Implements IConfigurationCell.RevisionCount
        Get
            Return m_revisionCount
        End Get
        Set(ByVal value As UInt16)
            m_revisionCount = value
        End Set
    End Property

    Public Overridable Function CompareTo(ByVal other As IConfigurationCell) As Integer Implements System.IComparable(Of IConfigurationCell).CompareTo

        ' We sort configuration cells by ID code...
        Return IDCode.CompareTo(other.IDCode)

    End Function

    Public Overridable Function CompareTo(ByVal obj As Object) As Int32 Implements IComparable.CompareTo

        Dim other As IConfigurationCell = TryCast(obj, IConfigurationCell)

        If other Is Nothing Then
            Throw New ArgumentException("ConfigurationCell can only be compared to other ConfigurationCells")
        Else
            Return CompareTo(other)
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

    Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        Dim length As Int32 = Array.IndexOf(binaryImage, Convert.ToByte(0), startIndex, MaximumStationNameLength) - startIndex

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
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)
            Dim index As Int32

            ' Copy in common cell images (channel names)
            CopyImage(m_phasorDefinitions, buffer, index)
            CopyImage(m_analogDefinitions, buffer, index)
            CopyImage(m_digitalDefinitions, buffer, index)

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)
        Dim x As Int32

        ' By the very nature of the IEEE protocols supporting the same order of phasor, analog and digital labels
        ' we are able to "automatically" parse this data out in the configuration cell base class - BEAUTIFUL!!!
        For x = 0 To parsingState.PhasorCount - 1
            m_phasorDefinitions.Add(parsingState.CreateNewPhasorDefintionFunction.Invoke(Me, binaryImage, startIndex))
            startIndex += m_phasorDefinitions(x).MaximumLabelLength
        Next

        For x = 0 To parsingState.AnalogCount - 1
            m_analogDefinitions.Add(parsingState.CreateNewAnalogDefintionFunction.Invoke(Me, binaryImage, startIndex))
            startIndex += m_analogDefinitions(x).MaximumLabelLength
        Next

        For x = 0 To parsingState.DigitalCount - 1
            m_digitalDefinitions.Add(parsingState.CreateNewDigitalDefintionFunction.Invoke(Me, binaryImage, startIndex))
            startIndex += m_digitalDefinitions(x).MaximumLabelLength
        Next

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

    Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        m_frequencyDefinition = DirectCast(state, IConfigurationCellParsingState).CreateNewFrequencyDefintionFunction.Invoke(Me, binaryImage, startIndex)

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize configuration cell values
        info.AddValue("stationName", StationName)
        info.AddValue("idLabel", IDLabel)
        info.AddValue("phasorDefinitions", m_phasorDefinitions, GetType(PhasorDefinitionCollection))
        info.AddValue("frequencyDefinition", m_frequencyDefinition, GetType(IFrequencyDefinition))
        info.AddValue("analogDefinitions", m_analogDefinitions, GetType(AnalogDefinitionCollection))
        info.AddValue("digitalDefinitions", m_digitalDefinitions, GetType(DigitalDefinitionCollection))
        info.AddValue("nominalFrequency", m_nominalFrequency, GetType(LineFrequency))
        info.AddValue("revisionCount", m_revisionCount)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Station Name", StationName)
            baseAttributes.Add("ID Label", IDLabel)
            baseAttributes.Add("Phasor Coordinate Format", PhasorCoordinateFormat & ": " & [Enum].GetName(GetType(CoordinateFormat), PhasorCoordinateFormat))
            baseAttributes.Add("Phasor Data Format", PhasorDataFormat & ": " & [Enum].GetName(GetType(DataFormat), PhasorDataFormat))
            baseAttributes.Add("Frequency Data Format", FrequencyDataFormat & ": " & [Enum].GetName(GetType(DataFormat), FrequencyDataFormat))
            baseAttributes.Add("Analog Data Format", AnalogDataFormat & ": " & [Enum].GetName(GetType(DataFormat), AnalogDataFormat))
            baseAttributes.Add("Total Phasor Definitions", PhasorDefinitions.Count)
            baseAttributes.Add("Total Analog Definitions", AnalogDefinitions.Count)
            baseAttributes.Add("Total Digital Definitions", DigitalDefinitions.Count)
            baseAttributes.Add("Nominal Frequency", NominalFrequency & " Hz")
            baseAttributes.Add("Revision Count", RevisionCount)
            baseAttributes.Add("Maximum Station Name Length", MaximumStationNameLength)
            baseAttributes.Add("ID Label Length", IDLabelLength)

            Return baseAttributes
        End Get
    End Property

End Class