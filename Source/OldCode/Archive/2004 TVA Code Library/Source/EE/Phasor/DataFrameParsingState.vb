'*******************************************************************************************************
'  DataFrameParsingState.vb - Data frame parsing state class
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
    Public Class DataFrameParsingState

        Inherits ChannelFrameParsingStateBase
        Implements IDataFrameParsingState

        Private m_configurationFrame As IConfigurationFrame

        Public Sub New(ByVal cells As DataCellCollection, ByVal cellType As Type, ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(cells, cellType)

            CellCount = configurationFrame.Cells.Count
            m_configurationFrame = configurationFrame

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public ReadOnly Property ConfigurationFrame() As IConfigurationFrame Implements IDataFrameParsingState.ConfigurationFrame
            Get
                Return m_configurationFrame
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As DataCellCollection Implements IDataFrameParsingState.Cells
            Get
                Return MyBase.Cells
            End Get
        End Property

    End Class

End Namespace