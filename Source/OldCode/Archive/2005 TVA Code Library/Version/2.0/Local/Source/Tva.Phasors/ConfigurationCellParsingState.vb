'*******************************************************************************************************
'  ConfigurationCellParsingState.vb - Configuration frame cell parsing state class
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

''' <summary>This class represents the protocol independent common implementation of a parsing state for a set of configuration related data settings that can be sent or received from a PMU.</summary>
<CLSCompliant(False)> _
Public Class ConfigurationCellParsingState

    Inherits ChannelCellParsingStateBase
    Implements IConfigurationCellParsingState

    Private m_createNewPhasorDefinitionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IPhasorDefinition)
    Private m_createNewFrequencyDefinitionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition)
    Private m_createNewAnalogDefinitionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IAnalogDefinition)
    Private m_createNewDigitalDefinitionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IDigitalDefinition)

    Public Sub New( _
        ByVal createNewPhasorDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IPhasorDefinition), _
        ByVal createNewFrequencyDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition), _
        ByVal createNewAnalogDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IAnalogDefinition), _
        ByVal createNewDigitalDefintionFunction As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IDigitalDefinition))

        m_createNewPhasorDefinitionFunction = createNewPhasorDefintionFunction
        m_createNewFrequencyDefinitionFunction = createNewFrequencyDefintionFunction
        m_createNewAnalogDefinitionFunction = createNewAnalogDefintionFunction
        m_createNewDigitalDefinitionFunction = createNewDigitalDefintionFunction

    End Sub

    Public Overrides ReadOnly Property InheritedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewPhasorDefinitionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IPhasorDefinition) Implements IConfigurationCellParsingState.CreateNewPhasorDefintionFunction
        Get
            Return m_createNewPhasorDefinitionFunction
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewFrequencyDefinitionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition) Implements IConfigurationCellParsingState.CreateNewFrequencyDefintionFunction
        Get
            Return m_createNewFrequencyDefinitionFunction
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewAnalogDefinitionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IAnalogDefinition) Implements IConfigurationCellParsingState.CreateNewAnalogDefintionFunction
        Get
            Return m_createNewAnalogDefinitionFunction
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewDigitalDefinitionFunction() As IConfigurationCellParsingState.CreateNewDefinitionFunctionSignature(Of IDigitalDefinition) Implements IConfigurationCellParsingState.CreateNewDigitalDefintionFunction
        Get
            Return m_createNewDigitalDefinitionFunction
        End Get
    End Property

End Class
