'*******************************************************************************************************
'  Enumerations.vb - Global enumerations for this namespace
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
'  03/11/2007 - J. Ritchie Carroll
'       Moved all namespace level enumerations into "Enumerations.vb" file
'
'*******************************************************************************************************

Public Enum SignalType
    ''' <summary>Phase Angle</summary>
    Angle
    ''' <summary>Phase Magnitude</summary>
    Magnitude
    ''' <summary>Line Frequency</summary>
    Frequency
    ''' <summary>Frequency Delta (dF/dt)</summary>
    dFdt
    ''' <summary>Status Flags</summary>
    Status
    ''' <summary>Digital Value</summary>
    Digital
    ''' <summary>Analog Value</summary>
    Analog
    ''' <summary>Calculated Value</summary>
    Calculation
    ''' <summary>Undetermined Signal Type</summary>
    Unknown
End Enum
