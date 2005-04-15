'***********************************************************************
'  IChannelFrame.vb - Basic frame interface
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor

    ' This interface represents the protocol independent representation of any phasor frame.
    Public Interface IChannelFrame

        ReadOnly Property InheritedType() As Type

        ReadOnly Property This() As IChannelFrame

        Property TimeTag() As NtpTimeTag

        Property Milliseconds() As Double

        ReadOnly Property Timestamp() As DateTime

        Property SynchronizationIsValid() As Boolean

        Property DataIsValid() As Boolean

        ReadOnly Property Name() As String

        ReadOnly Property DataLength() As Int16

        Property DataImage() As Byte()

        ReadOnly Property BinaryLength() As Int16

        ReadOnly Property BinaryImage() As Byte()

    End Interface

End Namespace