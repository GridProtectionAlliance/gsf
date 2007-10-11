'*******************************************************************************************************
'  IChannel.vb - Channel interface - this is the root interface
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

Imports TVA.Parsing

''' <summary>This interface represents a protocol independent representation of any data type.</summary>
<CLSCompliant(False)> _
Public Interface IChannel

    Inherits IBinaryDataProvider

    ReadOnly Property DerivedType() As Type

    ReadOnly Property This() As IChannel

    ' At its most basic level - all data represented by the protocols can either be "parsed" or "generated"
    ' hence the following methods common to all elements

    Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

    Shadows ReadOnly Property BinaryLength() As UInt16

    ReadOnly Property Attributes() As Dictionary(Of String, String)

    Property Tag() As Object

End Interface