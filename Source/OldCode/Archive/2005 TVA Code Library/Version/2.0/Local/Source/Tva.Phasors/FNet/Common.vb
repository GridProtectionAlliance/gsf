'*******************************************************************************************************
'  Common.vb - Common IEEE1344 declarations and functions
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

        ''' <summary>Frame type mask</summary>
        Public Const FrameTypeMask As Int16 = Bit13 Or Bit14 Or Bit15                   '0x1110000000000000 See IEEE Std 1344 Section 6.1 table 3 SMPCNT word definition

        ''' <summary>Trigger mask</summary>
        Public Const TriggerMask As Int16 = Bit11 Or Bit12 Or Bit13                     '0x0011100000000000 See IEEE Std 1344 Section 6.2 table 4 Status word STAT

        ''' <summary>Frame length mask</summary>
        Public Const FrameLengthMask As Int16 = Not (TriggerMask Or Bit14 Or Bit15)     '0x0000011111111111 See IEEE Std 1344 Section 6.2 table 4 Status word STAT

        ''' <summary>Frame count mask (for multi-framed files)</summary>
        Public Const FrameCountMask As Int16 = Not (FrameTypeMask Or Bit11 Or Bit12)

        ''' <summary>Maximum frame count (for multi-framed files)</summary>
        Public Const MaximumFrameCount As Int16 = FrameCountMask

        ''' <summary>Absolute maximum number of samples</summary>
        Public Const MaximumSampleCount As Int16 = Not FrameTypeMask

        ''' <summary>Absolute maximum frame length</summary>
        Public Const MaximumFrameLength As Int16 = FrameLengthMask

        ''' <summary>Absolute maximum data length (within a frame)</summary>
        Public Const MaximumDataLength As Int16 = MaximumFrameLength - CommonFrameHeader.BinaryLength - 2

        ''' <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
        Public Const MaximumPhasorValues As Int32 = 1

        ''' <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
        ''' <remarks>IEEE 1344 doesn't support analog values</remarks>
        Public Const MaximumAnalogValues As Int32 = 0

        ''' <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
        Public Const MaximumDigitalValues As Int32 = 0

        ''' <summary>Absolute maximum number of bytes of data that could fit into a header frame</summary>
        Public Const MaximumHeaderDataLength As Int32 = MaximumDataLength

    End Class

End Namespace