'*******************************************************************************************************
'  ICommonFrameHeader.vb - BPA PDCstream Common frame header interface
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
'  03/06/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace BpaPdcStream

    <CLSCompliant(False)> _
    Public Interface ICommonFrameHeader

        Inherits IChannelFrame

        Shadows ReadOnly Property FrameType() As FrameType

        ReadOnly Property FundamentalFrameType() As FundamentalFrameType

        Property PacketFlag() As Byte

        Property WordCount() As Int16

        ReadOnly Property FrameLength() As Int16

    End Interface

End Namespace
