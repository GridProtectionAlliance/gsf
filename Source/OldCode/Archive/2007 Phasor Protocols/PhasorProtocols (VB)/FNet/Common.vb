'*******************************************************************************************************
'  Common.vb - Common FNet declarations and functions
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
'  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace FNet

    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>FNET data frame start byte</summary>
        Public Const StartByte As Byte = &H1

        ''' <summary>FNET data frame start byte</summary>
        Public Const EndByte As Byte = &H0

        ''' <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
        Public Const MaximumPhasorValues As Int32 = 1

        ''' <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
        ''' <remarks>FNET doesn't support analog values</remarks>
        Public Const MaximumAnalogValues As Int32 = 0

        ''' <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
        ''' <remarks>FNET doesn't support digital values</remarks>
        Public Const MaximumDigitalValues As Int32 = 0

        ''' <summary>Default frame rate for FNET devices is 10 frames per second</summary>
        Public Const DefaultFrameRate As Int16 = 10

        ''' <summary>Default nominal frequency for FNET devices is 60Hz</summary>
        Public Const DefaultNominalFrequency As LineFrequency = LineFrequency.Hz60

        ''' <summary>Default real-time ticks offset for FNET</summary>
        Public Const DefaultTicksOffset As Long = 110000000

    End Class

End Namespace