'*******************************************************************************************************
'  DataCellParsingState.vb - BPA PDCstream specific data frame cell parsing state class
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace BpaPdcStream

    ''' <summary>This class represents the BPA PDCstream protocol implementation of the parsing state of a data frame cell that can be sent or received from a PMU.</summary>
    <CLSCompliant(False)> _
    Public Class DataCellParsingState

        Inherits PhasorProtocols.DataCellParsingState

        Private m_isPdcBlockPmu As Boolean
        Private m_index As Integer

        Public Sub New( _
            ByVal configurationCell As IConfigurationCell, _
            ByVal createNewPhasorValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue), _
            ByVal createNewFrequencyValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue), _
            ByVal createNewAnalogValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue), _
            ByVal createNewDigitalValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue), _
            ByVal index As Integer)

            MyBase.New(configurationCell, createNewPhasorValueFunction, createNewFrequencyValueFunction, createNewAnalogValueFunction, createNewDigitalValueFunction)

            m_index = index

        End Sub

        Public Sub New( _
            ByVal configurationCell As IConfigurationCell, _
            ByVal createNewPhasorValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue), _
            ByVal createNewFrequencyValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue), _
            ByVal createNewAnalogValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue), _
            ByVal createNewDigitalValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue), _
            ByVal isPdcBlockPmu As Boolean)

            MyBase.New(configurationCell, createNewPhasorValueFunction, createNewFrequencyValueFunction, createNewAnalogValueFunction, createNewDigitalValueFunction)

            m_isPdcBlockPmu = isPdcBlockPmu

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property ConfigurationCell() As ConfigurationCell
            Get
                Return MyBase.ConfigurationCell
            End Get
        End Property

        Public ReadOnly Property IsPdcBlockPmu() As Boolean
            Get
                Return m_isPdcBlockPmu
            End Get
        End Property

        Public ReadOnly Property Index() As Integer
            Get
                Return m_index
            End Get
        End Property

    End Class

End Namespace
