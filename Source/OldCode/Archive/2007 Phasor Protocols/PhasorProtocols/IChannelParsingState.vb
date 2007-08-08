'*******************************************************************************************************
'  IChannelParsingState.vb - Channel parsing state interface - this is the parsing state root interface
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

''' <summary>This interface represents a protocol independent parsing state used by any kind of data.</summary>
''' <remarks>Data parsing is very format specific, classes implementing this interface create a common form for parsing state information particular to a data type.</remarks>
Public Interface IChannelParsingState

    ReadOnly Property DerivedType() As Type

    ReadOnly Property This() As IChannelParsingState

End Interface