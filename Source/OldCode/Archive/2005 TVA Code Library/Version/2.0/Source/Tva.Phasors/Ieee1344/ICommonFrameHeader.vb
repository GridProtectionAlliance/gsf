'*******************************************************************************************************
'  ICommonFrameHeader.vb - IEEE 1344 Common frame header interface
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

Imports Tva.DateTime

Namespace Ieee1344

    <CLSCompliant(False)> _
    Public Interface ICommonFrameHeader

        Inherits IChannelFrame

        Shadows Property IDCode() As UInt64

        Shadows ReadOnly Property FrameType() As FrameType

        ReadOnly Property FrameLength() As Int16

        ReadOnly Property DataLength() As Int16

        Shadows ReadOnly Property TimeTag() As NtpTimeTag

        Property SynchronizationIsValid() As Boolean

        Property DataIsValid() As Boolean

        Property TriggerStatus() As TriggerStatus

        Property InternalSampleCount() As Int16

        Property InternalStatusFlags() As Int16

    End Interface

End Namespace
