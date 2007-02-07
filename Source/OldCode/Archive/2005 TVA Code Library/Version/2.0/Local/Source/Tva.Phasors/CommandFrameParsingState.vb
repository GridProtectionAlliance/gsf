'*******************************************************************************************************
'  CommandFrameParsingState.vb - Command frame parsing state class
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

''' <summary>This class represents the protocol independent common implementation the parsing state of a command frame that can be sent or received from a PMU.</summary>
<CLSCompliant(False)> _
Public Class CommandFrameParsingState

    Inherits ChannelFrameParsingStateBase(Of ICommandCell)
    Implements ICommandFrameParsingState

    Public Sub New(ByVal cells As CommandCellCollection, ByVal frameLength As Int16, ByVal dataLength As Int16)

        MyBase.New(cells, frameLength, AddressOf CommandCell.CreateNewCommandCell)

        CellCount = dataLength

    End Sub

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As CommandCellCollection Implements ICommandFrameParsingState.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

End Class
