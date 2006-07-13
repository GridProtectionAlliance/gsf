'*******************************************************************************************************
'  IAnalogValue.vb - Analog value interface
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

''' <summary>This interface represents a protocol independent analog value.</summary>
<CLSCompliant(False)> _
Public Interface IAnalogValue

    Inherits IChannelValue(Of IAnalogDefinition)

    Property Value() As Single

    Property IntegerValue() As Int16

End Interface
