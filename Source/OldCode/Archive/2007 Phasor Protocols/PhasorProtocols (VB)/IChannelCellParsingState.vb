'*******************************************************************************************************
'  IChannelCellParsingState.vb - Channel data cell parsing state interface
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

''' <summary>This interface represents a protocol independent parsing state of any kind of data cell.</summary>
Public Interface IChannelCellParsingState

    Inherits IChannelParsingState

    Property PhasorCount() As Int32

    Property AnalogCount() As Int32

    Property DigitalCount() As Int32

End Interface