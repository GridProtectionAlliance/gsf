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
<CLSCompliant(False)> _
Public Class ConfigurationCellParsingState

    Inherits ChannelCellParsingStateBase
    Implements IConfigurationCellParsingState

    Private m_createNewPhasorDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IPhasorDefinition)
    Private m_createNewFrequencyDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition)
    Private m_createNewAnalogDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IAnalogDefinition)
    Private m_createNewDigitalDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IDigitalDefinition)

    Public Sub New( _
    ByVal createNewPhasorDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IPhasorDefinition), _
    ByVal createNewFrequencyDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition), _
    ByVal createNewAnalogDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IAnalogDefinition), _
    ByVal createNewDigitalDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IDigitalDefinition))

        m_createNewPhasorDefintionFunction = createNewPhasorDefintionFunction
        m_createNewFrequencyDefintionFunction = createNewFrequencyDefintionFunction
        m_createNewAnalogDefintionFunction = createNewAnalogDefintionFunction
        m_createNewDigitalDefintionFunction = createNewDigitalDefintionFunction

    End Sub

    Public Overrides ReadOnly Property InheritedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public ReadOnly Property CreateNewPhasorDefintionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IPhasorDefinition) Implements IConfigurationCellParsingState.CreateNewPhasorDefintionFunction
        Get
            Return m_createNewPhasorDefintionFunction
        End Get
    End Property

    Public ReadOnly Property CreateNewFrequencyDefintionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition) Implements IConfigurationCellParsingState.CreateNewFrequencyDefintionFunction
        Get
            Return m_createNewFrequencyDefintionFunction
        End Get
    End Property

    Public ReadOnly Property CreateNewAnalogDefintionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IAnalogDefinition) Implements IConfigurationCellParsingState.CreateNewAnalogDefintionFunction
        Get
            Return m_createNewAnalogDefintionFunction
        End Get
    End Property

    Public ReadOnly Property CreateNewDigitalDefintionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IDigitalDefinition) Implements IConfigurationCellParsingState.CreateNewDigitalDefintionFunction
        Get
            Return m_createNewDigitalDefintionFunction
        End Get
    End Property

End Class
