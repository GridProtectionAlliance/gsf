'*******************************************************************************************************
'  Enumerations.vb - Global enumerations for this namespace
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
'  07/11/2007 - J. Ritchie Carroll
'       Moved all namespace level enumerations into "Enumerations.vb" file
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

End Namespace