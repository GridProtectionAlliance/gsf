'*******************************************************************************************************
'  DataFrameParsingState.vb - Data frame parsing state class
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

' This class represents the protocol independent common implementation the parsing state of a data frame that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public Class DataFrameParsingState

    Inherits ChannelFrameParsingStateBase(Of IDataCell)
    Implements IDataFrameParsingState

    Private m_configurationFrame As IConfigurationFrame

    Public Sub New(ByVal cells As DataCellCollection, ByVal frameLength As Int16, ByVal configurationFrame As IConfigurationFrame, ByVal createNewCellFunction As IChannelFrameParsingState(Of IDataCell).CreateNewCellFunctionSignature)

        MyBase.New(cells, frameLength, createNewCellFunction)

        CellCount = configurationFrame.Cells.Count
        m_configurationFrame = configurationFrame

    End Sub

    Public Overrides ReadOnly Property InheritedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable ReadOnly Property ConfigurationFrame() As IConfigurationFrame Implements IDataFrameParsingState.ConfigurationFrame
        Get
            Return m_configurationFrame
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As DataCellCollection Implements IDataFrameParsingState.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

End Class
