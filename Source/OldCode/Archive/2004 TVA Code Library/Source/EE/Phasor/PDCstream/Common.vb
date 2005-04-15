'***********************************************************************
'  Common.vb - Common PDCstream declarations and functions
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Shared.Bit

Namespace EE.Phasor.PDCstream

    Public Enum PhasorType
        Voltage
        Current
        DontCare
    End Enum

    <Flags()> _
    Public Enum ChannelFlags As Byte
        DataIsValid = Bit0              ' Valid if not set (yes = 0)
        TransmissionErrors = Bit1       ' Errors if set (yes = 1)
        PMUSynchronized = Bit2          ' Not sync'd if set (yes = 0)
        DataSortedByArrival = Bit3      ' Data out of sync if set (yes = 1)
        DataSortedByTimestamp = Bit4    ' Sorted by timestamp if not set (yes = 0)
        PDCExchangeFormat = Bit5        ' PDC format if set (yes = 1)
        MacrodyneFormat = Bit6          ' Macrodyne or IEEE format (Macrodyne = 1)
        TimestampIncluded = Bit7        ' Timestamp included if not set (yes = 0)
    End Enum

    <Flags()> _
    Public Enum PMUStatusFlags As Byte
        SyncInvalid = Bit0
        DataInvalid = Bit1
    End Enum

End Namespace
