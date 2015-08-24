'*******************************************************************************************************
'  IConfigurationFrame.vb - Configuration frame interface
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

''' <summary>This interface represents the protocol independent representation of any configuration frame.</summary>
<CLSCompliant(False)> _
Public Interface IConfigurationFrame

    Inherits IChannelFrame

    Shadows ReadOnly Property Cells() As ConfigurationCellCollection

    Property FrameRate() As Int16

    ReadOnly Property TicksPerFrame() As Decimal

    Sub SetNominalFrequency(ByVal value As LineFrequency)

End Interface