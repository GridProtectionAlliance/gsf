'*******************************************************************************************************
'  DataCellParsingState.vb - Data frame cell parsing state class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation the parsing state of a data frame that can be sent or received from a PMU.
    Public Class DataCellParsingState

        Inherits ChannelCellParsingStateBase
        Implements IDataCellParsingState

        Private m_phasorValueType As Type
        Private m_frequencyValueType As Type
        Private m_analogValueType As Type
        Private m_digitalValueType As Type
        Private m_configurationCell As IConfigurationCell

        Public Sub New(ByVal phasorValueType As Type, ByVal frequencyValueType As Type, ByVal analogValueType As Type, ByVal digitalValueType As Type, ByVal configurationCell As IConfigurationCell)

            m_phasorValueType = phasorValueType
            m_frequencyValueType = frequencyValueType
            m_analogValueType = analogValueType
            m_digitalValueType = digitalValueType
            m_configurationCell = configurationCell

            With configurationCell
                PhasorCount = .PhasorDefinitions.Count
                AnalogCount = .AnalogDefinitions.Count
                DigitalCount = .DigitalDefinitions.Count
            End With

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public ReadOnly Property ConfigurationCell() As IConfigurationCell Implements IDataCellParsingState.ConfigurationCell
            Get
                Return m_configurationCell
            End Get
        End Property

        Public ReadOnly Property PhasorValueType() As Type Implements IDataCellParsingState.PhasorValueType
            Get
                Return m_phasorValueType
            End Get
        End Property

        Public ReadOnly Property FrequencyValueType() As Type Implements IDataCellParsingState.FrequencyValueType
            Get
                Return m_frequencyValueType
            End Get
        End Property

        Public ReadOnly Property AnalogValueType() As Type Implements IDataCellParsingState.AnalogValueType
            Get
                Return m_analogValueType
            End Get
        End Property

        Public ReadOnly Property DigitalValueType() As Type Implements IDataCellParsingState.DigitalValueType
            Get
                Return m_digitalValueType
            End Get
        End Property

    End Class

End Namespace