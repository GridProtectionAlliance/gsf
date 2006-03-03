'*******************************************************************************************************
'  IChannelFrame.vb - Channel data frame interface
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

Imports Tva.DateTime

' This interface represents the protocol independent representation of any frame of data.
Public Interface IChannelFrame

    Inherits IChannel

    ReadOnly Property Cells() As IChannelCellCollection(Of IChannel)

    Property IDCode() As Int16

    Property Ticks() As Long                    ' Ticks of this frame

    ReadOnly Property TimeTag() As UnixTimeTag  ' UNIX based time of this frame

    ReadOnly Property Timestamp() As Date       ' .NET timestamp of this frame

    Property SynchronizationIsValid() As Boolean

    Property DataIsValid() As Boolean

    Property Published() As Boolean

End Interface
