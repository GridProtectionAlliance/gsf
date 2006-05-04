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

Namespace Ieee1344

    ''' <summary>Frame type</summary>
    Public Enum FrameType As Int16
        ''' <summary>000 Data frame</summary>
        DataFrame = Nill
        ''' <summary>001 Header frame</summary>
        HeaderFrame = Bit13
        ''' <summary>010 Configuration frame</summary>
        ConfigurationFrame = Bit14
        ''' <summary>011 Reserved flags 0</summary>
        Reserved0 = Bit13 Or Bit14
        ''' <summary>110 Reserved flags 1</summary>
        Reserved1 = Bit14 Or Bit15
        ''' <summary>100 Reserved flags 2</summary>
        Reserved2 = Bit15
        ''' <summary>101 User defined flags 0</summary>
        UserDefined0 = Bit13 Or Bit15
        ''' <summary>101 User defined flags 1</summary>
        UserDefined1 = Bit13 Or Bit14 Or Bit15
    End Enum

    ''' <summary>Trigger status</summary>
    Public Enum TriggerStatus As Int16
        ''' <summary>111 Frequency trigger</summary>
        FrequencyTrigger = Bit13 Or Bit12 Or Bit11
        ''' <summary>110 df/dt trigger</summary>
        DfDtTrigger = Bit13 Or Bit12
        ''' <summary>101 Angle trigger</summary>
        AngleTrigger = Bit13 Or Bit11
        ''' <summary>100 Overcurrent trigger</summary>
        OverCurrentTrigger = Bit13
        ''' <summary>011 Undervoltage trigger</summary>
        UnderVoltageTrigger = Bit12 Or Bit11
        ''' <summary>101 Rate trigger</summary>
        RateTrigger = Bit12
        ''' <summary>001 User defined</summary>
        UserDefined = Bit11
        ''' <summary>000 Unused</summary>
        Unused = Nill
    End Enum

    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Frame type mask</summary>
        Public Const FrameTypeMask As Int16 = Bit13 Or Bit14 Or Bit15

        ''' <summary>Trigger mask</summary>
        Public Const TriggerMask As Int16 = Bit11 Or Bit12 Or Bit13

        ''' <summary>Frame length mask</summary>
        Public Const FrameLengthMask As Int16 = Not (TriggerMask Or Bit14 Or Bit15)

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
        Public Const MaximumPhasorValues As Int32 = MaximumDataLength \ 4 - 6

        ''' <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
        ''' <remarks>IEEE 1344 doesn't support analog values</remarks>
        Public Const MaximumAnalogValues As Int32 = 0

        ''' <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
        Public Const MaximumDigitalValues As Int32 = MaximumDataLength \ 2 - 6

        ''' <summary>Absolute maximum number of bytes of data that could fit into a header frame</summary>
        Public Const MaximumHeaderDataLength As Int32 = MaximumDataLength

    End Class

End Namespace