'*******************************************************************************************************
'  DataCellParsingState.vb - Data frame cell parsing state class
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

''' <summary>This class represents the protocol independent common implementation of the parsing state of a data frame cell that can be sent or received from a PMU.</summary>
<CLSCompliant(False)> _
Public Class DataCellParsingState

    Inherits ChannelCellParsingStateBase
    Implements IDataCellParsingState

    Private m_configurationCell As IConfigurationCell
    Private m_createNewPhasorValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue)
    Private m_createNewFrequencyValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue)
    Private m_createNewAnalogValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue)
    Private m_createNewDigitalValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue)

    Public Sub New( _
        ByVal configurationCell As IConfigurationCell, _
        ByVal createNewPhasorValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue), _
        ByVal createNewFrequencyValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue), _
        ByVal createNewAnalogValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue), _
        ByVal createNewDigitalValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue))

        m_configurationCell = configurationCell
        m_createNewPhasorValueFunction = createNewPhasorValueFunction
        m_createNewFrequencyValueFunction = createNewFrequencyValueFunction
        m_createNewAnalogValueFunction = createNewAnalogValueFunction
        m_createNewDigitalValueFunction = createNewDigitalValueFunction

        PhasorCount = m_configurationCell.PhasorDefinitions.Count
        AnalogCount = m_configurationCell.AnalogDefinitions.Count
        DigitalCount = m_configurationCell.DigitalDefinitions.Count

    End Sub

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable ReadOnly Property ConfigurationCell() As IConfigurationCell Implements IDataCellParsingState.ConfigurationCell
        Get
            Return m_configurationCell
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewPhasorValueFunction() As IDataCellParsingState.CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue) Implements IDataCellParsingState.CreateNewPhasorValueFunction
        Get
            Return m_createNewPhasorValueFunction
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewFrequencyValueFunction() As IDataCellParsingState.CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue) Implements IDataCellParsingState.CreateNewFrequencyValueFunction
        Get
            Return m_createNewFrequencyValueFunction
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewAnalogValueFunction() As IDataCellParsingState.CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue) Implements IDataCellParsingState.CreateNewAnalogValueFunction
        Get
            Return m_createNewAnalogValueFunction
        End Get
    End Property

    Public Overridable ReadOnly Property CreateNewDigitalValueFunction() As IDataCellParsingState.CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue) Implements IDataCellParsingState.CreateNewDigitalValueFunction
        Get
            Return m_createNewDigitalValueFunction
        End Get
    End Property

End Class