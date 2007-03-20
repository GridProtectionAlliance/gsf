'*******************************************************************************************************
'  DataCellParsingState.vb - BPA PDCstream specific data frame cell parsing state class
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

Namespace BpaPdcStream

    ''' <summary>This class represents the BPA PDCstream protocol implementation of the parsing state of a data frame cell that can be sent or received from a PMU.</summary>
    <CLSCompliant(False)> _
    Public Class DataCellParsingState

        Inherits Phasors.DataCellParsingState

        Private m_isPdcBlockHeader As Boolean
        Private m_isPdcBlockPmu As Boolean

        Public Sub New( _
            ByVal configurationCell As IConfigurationCell, _
            ByVal createNewPhasorValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue), _
            ByVal createNewFrequencyValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue), _
            ByVal createNewAnalogValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue), _
            ByVal createNewDigitalValueFunction As IDataCellParsingState.CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue))

            MyBase.New(configurationCell, createNewPhasorValueFunction, createNewFrequencyValueFunction, createNewAnalogValueFunction, createNewDigitalValueFunction)

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

            ' PDC Block PMU's phase 2 phasors, 0 analogs and 1 digital
            If isPdcBlockPmu Then
                PhasorCount = 2
                AnalogCount = 0
                DigitalCount = 1
            End If

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

        Public Property IsPdcBlockHeader() As Boolean
            Get
                Return m_isPdcBlockHeader
            End Get
            Set(ByVal value As Boolean)
                m_isPdcBlockHeader = value
            End Set
        End Property

    End Class

End Namespace
