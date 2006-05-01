'*******************************************************************************************************
'  IPhasorDefinition.vb - Phasor value definition interface
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This class represents the protocol independent interface of a phasor value definition.
<CLSCompliant(False)> _
Public Interface IPhasorDefinition

    Inherits IChannelDefinition

    ReadOnly Property CoordinateFormat() As CoordinateFormat

    Property [Type]() As PhasorType

    Property VoltageReference() As IPhasorDefinition

End Interface
