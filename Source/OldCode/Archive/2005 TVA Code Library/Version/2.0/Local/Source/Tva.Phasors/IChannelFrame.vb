'*******************************************************************************************************
'  IChannelFrame.vb - Channel data frame interface
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports TVA.DateTime
Imports TVA.Measurements

''' <summary>This interface represents the protocol independent representation of any frame of data.</summary>
<CLSCompliant(False)> _
Public Interface IChannelFrame

    Inherits IChannel, IFrame, IComparable, ISerializable

    ReadOnly Property FrameType() As FundamentalFrameType

    ReadOnly Property Cells() As Object

    Property IDCode() As UInt16

    ReadOnly Property TimeTag() As UnixTimeTag  ' UNIX based time of this frame

    ReadOnly Property IsPartial() As Boolean

End Interface
