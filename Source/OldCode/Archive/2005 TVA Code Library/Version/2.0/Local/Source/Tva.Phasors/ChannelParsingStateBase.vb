'*******************************************************************************************************
'  ChannelParsingStateBase.vb - Parsing state base class
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
'  3/7/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This class represents the common implementation of the protocol independent parsing state class used by any kind of data.
' This class is inherited by subsequent classes to provide parsing state information particular to a data type's needs.
Public MustInherit Class ChannelParsingStateBase

    Implements IChannelParsingState

    ' This is expected to be overriden by the final derived class
    Public MustOverride ReadOnly Property InheritedType() As Type Implements IChannelParsingState.InheritedType

    Public Overridable ReadOnly Property This() As IChannelParsingState Implements IChannelParsingState.This
        Get
            Return Me
        End Get
    End Property

End Class
