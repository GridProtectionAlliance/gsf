'***********************************************************************
'  Common.vb - Common IEEE1344 declarations and functions
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
Namespace EE.Phasor.IEEE1344

    Public Enum PMUFrameType
        DataFrame
        HeaderFrame
        ConfigurationFrame
    End Enum

    Public Enum PMUTriggerStatus
        None
        FrequencyTrigger
        DfDtTrigger
        AngleTrigger
        OverCurrentTrigger
        UnderVoltageTrigger
        RateTrigger
        Undetermined
    End Enum

End Namespace