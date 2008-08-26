'*******************************************************************************************************
'  IChannelFrameParsingState.vb - Channel data frame parsing state interface
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

''' <summary>This interface represents the protocol independent parsing state of any frame of data.</summary>
<CLSCompliant(False)> _
Public Interface IChannelFrameParsingState(Of T As IChannelCell)

    Inherits IChannelParsingState

    Delegate Function CreateNewCellFunctionSignature(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of T), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As T

    ReadOnly Property CreateNewCellFunction() As CreateNewCellFunctionSignature

    ReadOnly Property Cells() As IChannelCellCollection(Of T)

    Property CellCount() As Int32

    Property ParsedBinaryLength() As UInt16

End Interface