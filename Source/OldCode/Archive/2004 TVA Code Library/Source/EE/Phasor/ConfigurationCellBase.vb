'***********************************************************************
'  ConfigurationCellBase.vb - Configuration cell base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Buffer
Imports System.Text
Imports TVA.Interop
Imports TVA.EE.Phasor.Common

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a set of configuration related data settings that can be sent or received from a PMU.
    Public MustInherit Class ConfigurationCellBase

        Inherits ChannelCellBase
        Implements IConfigurationCell

        Private m_stationName As String
        Private m_idCode As Int16
        Private m_idLabel As String
        Private m_phasorDefinitions As PhasorDefinitionCollection
        Private m_frequencyDefinition As IFrequencyDefinition
        Private m_analogDefinitions As AnalogDefinitionCollection
        Private m_digitalDefinitions As DigitalDefinitionCollection
        Private m_sampleRate As Int16

        Protected Sub New(ByVal parent As IConfigurationFrame)

            MyBase.New(parent)

            m_phasorDefinitions = New PhasorDefinitionCollection
            m_analogDefinitions = New AnalogDefinitionCollection
            m_digitalDefinitions = New DigitalDefinitionCollection

        End Sub

        Protected Sub New(ByVal parent As IConfigurationFrame, ByVal stationName As String, ByVal idCode As Int16, ByVal idLabel As String, ByVal phasorDefinitions As PhasorDefinitionCollection, ByVal frequencyDefinition As IFrequencyDefinition, ByVal analogDefinitions As AnalogDefinitionCollection, ByVal digitalDefinitions As DigitalDefinitionCollection, ByVal sampleRate As Int16)

            MyBase.New(parent)

            Me.StationName = stationName
            m_idCode = idCode
            Me.IDLabel = idLabel
            m_phasorDefinitions = phasorDefinitions
            m_frequencyDefinition = frequencyDefinition
            m_analogDefinitions = analogDefinitions
            m_digitalDefinitions = digitalDefinitions
            m_sampleRate = sampleRate

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal configurationCell As IConfigurationCell)
        Protected Sub New(ByVal configurationCell As IConfigurationCell)

            Me.New(configurationCell.Parent, configurationCell.StationName, configurationCell.IDCode, configurationCell.IDLabel, _
                configurationCell.PhasorDefinitions, configurationCell.FrequencyDefinition, configurationCell.AnalogDefinitions, _
                configurationCell.DigitalDefinitions, configurationCell.SampleRate)

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
            Set(ByVal Value As String)
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
            Set(ByVal Value As Int16)
                m_idCode = Value
            End Set
        End Property

        Public Overridable Property IDLabel() As String Implements IConfigurationCell.IDLabel
            Get
                Return m_idLabel
            End Get
            Set(ByVal Value As String)
                Dim length As Integer = Len(Trim(Value))
                If length > IDLabelLength Or length < IDLabelLength Then
                    Throw New OverflowException("ID label must be exactly " & IDLabelLength & " characters in length")
                Else
                    m_idLabel = Value
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
            Set(ByVal Value As IFrequencyDefinition)
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
                ' TODO: FIX THIS!!!
                'Return ProtocolSpecificDataLength + m_frequencyDefinition.BinaryLength + m_phasorDefinitions.BinaryLength + m_analogDefinitions.BinaryLength + m_digitalDefinitions.BinaryLength
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)
                Dim index As Integer = 0 'ProtocolSpecificDataLength

                ' Copy in protocol specific data image
                'If index > 0 Then BlockCopy(ProtocolSpecificDataImage, 0, buffer, 0, ProtocolSpecificDataLength)

                ' Copy in common cell image
                CopyImage(m_phasorDefinitions, buffer, index)
                CopyImage(m_frequencyDefinition, buffer, index)
                CopyImage(m_analogDefinitions, buffer, index)
                CopyImage(m_digitalDefinitions, buffer, index)

                Return buffer
            End Get
        End Property

    End Class

End Namespace