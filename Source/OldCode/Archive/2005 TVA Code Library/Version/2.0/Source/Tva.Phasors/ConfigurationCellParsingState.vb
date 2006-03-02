'*******************************************************************************************************
'  ConfigurationCellParsingState.vb - Configuration frame cell parsing state class
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

' This class represents the protocol independent common implementation of a parsing state for a set of configuration related data settings that can be sent or received from a PMU.
Public Class ConfigurationCellParsingState

    Inherits ChannelCellParsingStateBase
    Implements IConfigurationCellParsingState

    Private m_phasorDefinitionType As Type
    Private m_frequencyDefinitionType As Type
    Private m_analogDefinitionType As Type
    Private m_digitalDefinitionType As Type

    Public Sub New(ByVal phasorDefinitionType As Type, ByVal frequencyDefinitionType As Type, ByVal analogDefinitionType As Type, ByVal digitalDefinitionType As Type)

        m_phasorDefinitionType = phasorDefinitionType
        m_frequencyDefinitionType = frequencyDefinitionType
        m_analogDefinitionType = analogDefinitionType
        m_digitalDefinitionType = digitalDefinitionType

    End Sub

    Public Overrides ReadOnly Property InheritedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public ReadOnly Property PhasorDefinitionType() As Type Implements IConfigurationCellParsingState.PhasorDefinitionType
        Get
            Return m_phasorDefinitionType
        End Get
    End Property

    Public ReadOnly Property FrequencyDefinitionType() As Type Implements IConfigurationCellParsingState.FrequencyDefinitionType
        Get
            Return m_frequencyDefinitionType
        End Get
    End Property

    Public ReadOnly Property AnalogDefinitionType() As Type Implements IConfigurationCellParsingState.AnalogDefinitionType
        Get
            Return m_analogDefinitionType
        End Get
    End Property

    Public ReadOnly Property DigitalDefinitionType() As Type Implements IConfigurationCellParsingState.DigitalDefinitionType
        Get
            Return m_digitalDefinitionType
        End Get
    End Property

End Class
