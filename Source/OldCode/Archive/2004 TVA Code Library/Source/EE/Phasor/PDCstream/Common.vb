'*******************************************************************************************************
'  Common.vb - Common PDCstream declarations and functions
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Shared.Bit

Namespace EE.Phasor.PDCstream

    Public Class Common

        Public Const SyncByte As Byte = &HAA
        Public Const DescriptorPacketFlag As Byte = &H0

    End Class

    Public Enum StreamType As Byte
        Legacy = 0                      ' Standard full data stream
        Compact = 1                     ' Full data stream with PMU ID's and offsets removed from data packet
    End Enum

    Public Enum RevisionNumber As Byte
        Revision0 = 0                   ' Original revision for all to June 2002, use NTP timetag (start count 1900)
        Revision1 = 1                   ' July 2002 revision for std. 37.118, use UNIX timetag (start count 1970)
        Revision2 = 2                   ' May 2005 revision for std. 37.118, change ChanFlag for added data types
    End Enum

    <Flags()> _
    Public Enum ChannelFlags As Byte
        DataIsValid = Bit7              ' Valid if not set (yes = 0)
        TransmissionErrors = Bit6       ' Errors if set (yes = 1)              
        PMUSynchronized = Bit5          ' Not sync'd if set (yes = 0)        
        DataSortedByArrival = Bit4      ' Data out of sync if set (yes = 1)        
        <Obsolete("This bit definition is for obsolete uses that is no longer needed.", False)> _
        DataSortedByTimestamp = Bit3    ' Sorted by timestamp if not set (yes = 0)        
        PDCExchangeFormat = Bit2        ' PDC format if set (yes = 1)        
        MacrodyneFormat = Bit1          ' Macrodyne or IEEE format (Macrodyne = 1)        
        <Obsolete("This bit definition is for obsolete uses that is no longer needed.", False)> _
        TimestampIncluded = Bit0        ' Timestamp included if not set (yes = 0)
    End Enum

    <Flags()> _
    Public Enum ReservedFlags As Byte
        Reserved0 = Bit7
        Reserved1 = Bit6
        AnalogWordsMask = Bit0 Or Bit1 Or Bit2 Or Bit3 Or Bit4 Or Bit5
    End Enum

    <Flags()> _
    Public Enum IEEEFormatFlags As Byte
        Frequency = Bit7                ' Set = float, Clear = integer
        Analog = Bit6                   ' Set = float, Clear = integer
        Phasors = Bit5                  ' Set = float, Clear = integer
        Coordinates = Bit4              ' Set = polar, Clear = rectangular
        DigitalWordsMask = Bit0 Or Bit1 Or Bit2 Or Bit3
    End Enum

    <Flags()> _
    Public Enum PMUStatusFlags As Byte
        SyncInvalid = Bit0
        DataInvalid = Bit1
    End Enum

End Namespace
