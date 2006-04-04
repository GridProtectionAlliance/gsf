'*******************************************************************************************************
'  HeaderFrameParsingState.vb - Header frame parsing state class
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

' This class represents the protocol independent common implementation the parsing state of a header frame that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public Class HeaderFrameParsingState

    Inherits ChannelFrameParsingStateBase(Of IHeaderCell)
    Implements IHeaderFrameParsingState

    Public Sub New(ByVal cells As HeaderCellCollection, ByVal frameLength As Int16, ByVal dataLength As Int16)

        MyBase.New(cells, frameLength, AddressOf HeaderCell.CreateNewHeaderCell)

        CellCount = dataLength

    End Sub

    Public Overrides ReadOnly Property InheritedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As HeaderCellCollection Implements IHeaderFrameParsingState.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

End Class
