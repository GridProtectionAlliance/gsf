'*******************************************************************************************************
'  IChannelFrame.vb - Channel data frame interface
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports TVA.Interop
Imports TVA.Shared.DateTime

Namespace EE.Phasor

    ' This interface represents the protocol independent representation of any frame of data.
    Public Interface IChannelFrame

        Inherits IChannel

        ReadOnly Property Cells() As IChannelCellCollection

        Property TimeTag() As Unix.TimeTag          ' UNIX based time of this frame (accurate to seconds)

        Property NtpTimeTag() As NtpTimeTag         ' Network Time Protocol time of this frame (accurate to seconds)

        Property Milliseconds() As Double           ' Millisecond offset of this frame

        ReadOnly Property Timestamp() As DateTime   ' .NET timestamp of this frame (accurate to milliseconds)

        Property SynchronizationIsValid() As Boolean

        Property DataIsValid() As Boolean

        Property Published() As Boolean

    End Interface

End Namespace