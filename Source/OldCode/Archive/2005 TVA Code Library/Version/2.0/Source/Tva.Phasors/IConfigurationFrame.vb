'*******************************************************************************************************
'  IConfigurationFrame.vb - Configuration frame interface
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

' This interface represents the protocol independent representation of any configuration frame.
Public Interface IConfigurationFrame

    Inherits IChannelFrame(Of IConfigurationCell)

    'Shadows ReadOnly Property Cells() As ConfigurationCellCollection

    Property SampleRate() As Int16

    ' TODO: Move nominal frequency property down into config cell definition
    Sub SetNominalFrequency(ByVal value As LineFrequency)

End Interface
