'*******************************************************************************************************
'  IDataFrameParsingState.vb - Data frame parsing state interface
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

' This interface represents the protocol independent parsing state of any frame of data.
<CLSCompliant(False)> _
Public Interface IDataFrameParsingState

    Inherits IChannelFrameParsingState(Of IDataCell)

    ReadOnly Property ConfigurationFrame() As IConfigurationFrame

    Shadows ReadOnly Property Cells() As DataCellCollection

End Interface
