'*******************************************************************************************************
'  Common.vb - Common IEEE C37.118 declarations and functions
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

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
        Public Const MaximumPhasorValues As Int32 = UInt16.MaxValue \ 4 - CommonFrameHeader.BinaryLength - 8

        ''' <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
        Public Const MaximumAnalogValues As Int32 = UInt16.MaxValue \ 2 - CommonFrameHeader.BinaryLength - 8

        ''' <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
        Public Const MaximumDigitalValues As Int32 = UInt16.MaxValue \ 2 - CommonFrameHeader.BinaryLength - 8

        ''' <summary>Absolute maximum number of bytes of data that could fit into a header frame</summary>
        Public Const MaximumHeaderDataLength As Int32 = UInt16.MaxValue - CommonFrameHeader.BinaryLength - 2

        ''' <summary>Absolute maximum number of bytes of extended data that could fit into a command frame</summary>
        Public Const MaximumExtendedDataLength As Int32 = UInt16.MaxValue - CommonFrameHeader.BinaryLength - 4

        ''' <summary>Time quality flags mask</summary>
        Public Const TimeQualityFlagsMask As Int32 = Bit31 Or Bit30 Or Bit29 Or Bit28 Or Bit27 Or Bit26 Or Bit25 Or Bit24

    End Class

End Namespace