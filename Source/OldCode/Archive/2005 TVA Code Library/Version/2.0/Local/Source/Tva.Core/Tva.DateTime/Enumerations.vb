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

Namespace DateTime

    ''' <summary>Time names enumeration used by SecondsToText function</summary>
    Public Enum TimeName
        Year
        Years
        Day
        Days
        Hour
        Hours
        Minute
        Minutes
        Second
        Seconds
        LessThan60Seconds
        NoSeconds
    End Enum

    ''' <summary>Time zone names enumeration used to look up desired time zone in GetWin32TimeZone function</summary>
    Public Enum TimeZoneName
        DaylightName
        DaylightAbbreviation
        DisplayName
        StandardName
        StandardAbbreviation
    End Enum

End Namespace