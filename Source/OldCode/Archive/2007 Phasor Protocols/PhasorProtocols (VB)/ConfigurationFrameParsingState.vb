'*******************************************************************************************************
'  ConfigurationFrameParsingState.vb - Configuration frame parsing state class
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

''' <summary>This class represents the protocol independent common implementation the parsing state of a data frame that can be sent or received from a PMU.</summary>
<CLSCompliant(False)> _
Public Class ConfigurationFrameParsingState

    Inherits ChannelFrameParsingStateBase(Of IConfigurationCell)
    Implements IConfigurationFrameParsingState

    Public Sub New(ByVal cells As ConfigurationCellCollection, ByVal frameLength As Int16, ByVal createNewCellFunction As IChannelFrameParsingState(Of IConfigurationCell).CreateNewCellFunctionSignature)

        MyBase.New(cells, frameLength, createNewCellFunction)

    End Sub

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As ConfigurationCellCollection Implements IConfigurationFrameParsingState.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

End Class