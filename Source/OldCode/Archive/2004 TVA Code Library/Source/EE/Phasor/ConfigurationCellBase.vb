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

Imports System.Text
Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a set of configuration related data settings that can be sent or received from a PMU.
    Public MustInherit Class ConfigurationCellBase

        Inherits ChannelBase
        Implements IConfigurationCell

        Private m_stationName As String
        Private m_idCode As Int16
        Private m_phasorDefinitions As PhasorDefinitionCollection
        Private m_frequencyDefinition As IFrequencyDefinition
        Private m_analogDefinitions As AnalogDefinitionCollection
        Private m_digitalDefinitions As DigitalDefinitionCollection

        Protected Sub New()

            MyBase.New()

            m_phasorDefinitions = New PhasorDefinitionCollection
            m_analogDefinitions = New AnalogDefinitionCollection
            m_digitalDefinitions = New DigitalDefinitionCollection

        End Sub

        Protected Sub New(ByVal stationName As String, ByVal idCode As Int16, ByVal phasorDefinitions As PhasorDefinitionCollection, ByVal frequencyDefinition As IFrequencyDefinition, ByVal analogDefinitions As AnalogDefinitionCollection, ByVal digitalDefinitions As DigitalDefinitionCollection)

            Me.StationName = stationName
            m_idCode = idCode
            m_phasorDefinitions = phasorDefinitions
            m_frequencyDefinition = frequencyDefinition
            m_analogDefinitions = analogDefinitions
            m_digitalDefinitions = digitalDefinitions

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal configurationCell As IConfigurationCell)
        Protected Sub New(ByVal configurationCell As IConfigurationCell)

            Me.New(configurationCell.StationName, configurationCell.IDCode, configurationCell.PhasorDefinitions, configurationCell.FrequencyDefinition, configurationCell.AnalogDefinitions, configurationCell.DigitalDefinitions)

        End Sub

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

        Public Overrides ReadOnly Property BinaryLength() As Short
            Get
                Return m_frequencyDefinition.BinaryLength + m_phasorDefinitions.BinaryLength + m_analogDefinitions.BinaryLength + m_digitalDefinitions.BinaryLength
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim x, index As Integer

                For x = 0 To m_phasorDefinitions.Count - 1
                    CopyImage(m_phasorDefinitions(x), buffer, index)
                Next

                CopyImage(m_frequencyDefinition, buffer, index)

                For x = 0 To m_analogDefinitions.Count - 1
                    CopyImage(m_analogDefinitions(x), buffer, index)
                Next

                For x = 0 To m_digitalDefinitions.Count - 1
                    CopyImage(m_digitalDefinitions(x), buffer, index)
                Next

                Return buffer
            End Get
        End Property

    End Class

End Namespace