'*******************************************************************************************************
'  Common.vb - Common PDCstream declarations and functions
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace BpaPdcStream

    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Const DescriptorPacketFlag As Byte = &H0

        Public Const MaximumPhasorValues As Int32 = Byte.MaxValue
        Public Const MaximumAnalogValues As Int32 = ReservedFlags.AnalogWordsMask
        Public Const MaximumDigitalValues As Int32 = IEEEFormatFlags.DigitalWordsMask

    End Class

End Namespace
