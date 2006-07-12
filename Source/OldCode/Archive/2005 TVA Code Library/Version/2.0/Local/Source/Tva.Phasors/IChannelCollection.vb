'*******************************************************************************************************
'  IChannelCollection.vb - Channel collection interface
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

' This interface represents a protocol independent representation of a collection of any data type.
<CLSCompliant(False)> _
Public Interface IChannelCollection(Of T As IChannel)

    Inherits IChannel, IList(Of T), ISerializable

End Interface

