'*******************************************************************************************************
'  Common.vb - Common PDCstream declarations and functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
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

Namespace BpaPdcStream

    ''' <summary>Stream type</summary>
    Public Enum StreamType As Byte
        ''' <summary>Standard full data stream</summary>
        Legacy = 0
        ' <summary>Full data stream with PMU ID's and offsets removed from data packet</summary>
        Compact = 1
    End Enum

    ''' <summary>Stream revision number</summary>
    Public Enum RevisionNumber As Byte
        ''' <summary>Original revision for all to June 2002, use NTP timetag (start count 1900)</summary>
        Revision0 = 0
        ''' <summary>July 2002 revision for std. 37.118, use UNIX timetag (start count 1970)</summary>
        Revision1 = 1
        ''' <summary>May 2005 revision for std. 37.118, change ChanFlag for added data types</summary>
        Revision2 = 2
    End Enum

    ''' <summary>Channel flags</summary>
    <Flags()> _
    Public Enum ChannelFlags As Byte
        ''' <summary>Valid if not set (yes = 0)</summary>
        DataIsValid = Bit7
        ''' <summary>Errors if set (yes = 1)</summary>
        TransmissionErrors = Bit6
        ''' <summary>Not sync'd if set (yes = 0)</summary>
        PMUSynchronized = Bit5
        ''' <summary>Data out of sync if set (yes = 1)</summary>
        DataSortedByArrival = Bit4
        ''' <summary>Sorted by timestamp if not set (yes = 0)</summary>
        DataSortedByTimestamp = Bit3
        ''' <summary>PDC format if set (yes = 1)</summary>
        PDCExchangeFormat = Bit2
        ''' <summary>Macrodyne or IEEE format (Macrodyne = 1)</summary>
        MacrodyneFormat = Bit1
        ''' <summary>Timestamp included if not set (yes = 0)</summary>
        TimestampIncluded = Bit0
    End Enum

    ''' <summary>Reserved flags</summary>
    <Flags()> _
    Public Enum ReservedFlags As Byte
        Reserved0 = Bit7
        Reserved1 = Bit6
        AnalogWordsMask = Bit0 Or Bit1 Or Bit2 Or Bit3 Or Bit4 Or Bit5
    End Enum

    ''' <summary>IEEE format flags</summary>
    <Flags()> _
    Public Enum IEEEFormatFlags As Byte
        ''' <summary>Frequency data format: Set = float, Clear = integer</summary>
        Frequency = Bit7
        ''' <summary>Analog data format: Set = float, Clear = integer</summary>
        Analog = Bit6
        ''' <summary>Phasor data format: Set = float, Clear = integer</summary>
        Phasors = Bit5
        ''' <summary>Phasor coordinate format: Set = polar, Clear = rectangular</summary>
        Coordinates = Bit4
        ''' <summary>Digital words mask</summary>
        DigitalWordsMask = Bit0 Or Bit1 Or Bit2 Or Bit3
    End Enum

    ''' <summary>PMU status flags</summary>
    <Flags()> _
    Public Enum PMUStatusFlags As Byte
        ''' <summary>Synchonization is invalid</summary>
        SyncInvalid = Bit0
        ''' <summary>Data is invalid</summary>
        DataInvalid = Bit1
    End Enum

    <CLSCompliant(False)> _
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Const DescriptorPacketFlag As Byte = &H0

        Public Const MaximumPhasorValues As Int32 = Byte.MaxValue
        Public Const MaximumAnalogValues As Int32 = ReservedFlags.AnalogWordsMask
        Public Const MaximumDigitalValues As Int32 = IEEEFormatFlags.DigitalWordsMask

    End Class

End Namespace
