'*******************************************************************************************************
'  Common.vb - Common FNet declarations and functions
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

Namespace FNet

    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Data frame start byte</summary>
        Public Const StartByte As Byte = &H1

        ''' <summary>Data frame start byte</summary>
        Public Const EndByte As Byte = &H0

        ''' <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
        Public Const MaximumPhasorValues As Int32 = 1

        ''' <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
        ''' <remarks>IEEE 1344 doesn't support analog values</remarks>
        Public Const MaximumAnalogValues As Int32 = 0

        ''' <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
        Public Const MaximumDigitalValues As Int32 = 0

    End Class

End Namespace