'***********************************************************************
'  Common.vb - Common PC37_118 declarations and functions
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports TVA.Shared.Bit

Namespace EE.Phasor.PC37_118

    Public Enum PhasorFormat
        Rectangular
        Polar
    End Enum

    Public Enum PhasorType As Byte
        Voltage
        Current
    End Enum

    Public Enum PMUFrameType As Short
        DataFrame = 0
        HeaderFrame = Bit13
        ConfigurationFrame = Bit14
        Reserved0 = Bit13 Or Bit14
        Reserved1 = Bit14 Or Bit15
        Reserved2 = Bit15
        UserDefined0 = Bit13 Or Bit15
        UserDefined1 = Bit13 Or Bit14 Or Bit15
    End Enum

    Public Enum PMUCommand As Short
        DisableRealTimeData = Bit0
        EnableRealTimeData = Bit1
        SendHeaderFile = Bit0 Or Bit1
        SendConfigFile1 = Bit2
        SendConfigFile2 = Bit0 Or Bit2
        ReceiveReferencePhasor = Bit3
    End Enum

    Public Enum PMUTriggerStatus As Short
        None = 0
        FrequencyTrigger = Bit11 Or Bit12 Or Bit13
        DfDtTrigger = Bit12 Or Bit13
        AngleTrigger = Bit11 Or Bit13
        OverCurrentTrigger = Bit13
        UnderVoltageTrigger = Bit11 Or Bit12
        RateTrigger = Bit12
        UserDefined = Bit11
    End Enum

    Public Enum PMULineFrequency
        _50Hz
        _60Hz
    End Enum

End Namespace