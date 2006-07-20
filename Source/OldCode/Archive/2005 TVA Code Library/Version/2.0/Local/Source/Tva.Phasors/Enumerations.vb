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

''' <summary>Phasor coordinate formats</summary>
<Serializable()> _
Public Enum CoordinateFormat As Byte
    Rectangular
    Polar
End Enum

''' <summary>Composite polar values</summary>
<Serializable()> _
Public Enum CompositePhasorValue
    Angle
    Magnitude
End Enum

''' <summary>Composite frequency values</summary>
<Serializable()> _
Public Enum CompositeFrequencyValue
    Frequency
    DfDt
End Enum

''' <summary>Phasor types</summary>
<Serializable()> _
Public Enum PhasorType As Byte
    Voltage
    Current
End Enum

''' <summary>Data transmission formats</summary>
<Serializable()> _
Public Enum DataFormat As Byte
    FixedInteger
    FloatingPoint
End Enum

''' <summary>Nominal line frequencies</summary>
<Serializable()> _
Public Enum LineFrequency As Byte
    Hz50 = 50
    Hz60 = 60
End Enum

''' <summary>Fundamental frame types</summary>
<Serializable()> _
Public Enum FundamentalFrameType
    ConfigurationFrame
    DataFrame
    HeaderFrame
    CommandFrame
    Undetermined
End Enum

''' <summary>Phasor data protocols</summary>
<Serializable()> _
Public Enum PhasorProtocol
    IeeeC37_118V1
    IeeeC37_118D6
    Ieee1344
    BpaPdcStream
End Enum

''' <summary>Phasor enabled device commands</summary>
<Serializable()> _
Public Enum DeviceCommand As Int16
    ''' <summary>0001 Turn off transmission of data frames</summary>
    DisableRealTimeData = Bit0
    ''' <summary>0010 Turn on transmission of data frames</summary>
    EnableRealTimeData = Bit1
    ''' <summary>0011 Send header file</summary>
    SendHeaderFrame = Bit0 Or Bit1
    ''' <summary>0100 Send configuration file 1</summary>
    SendConfigurationFrame1 = Bit2
    ''' <summary>0101 Send configuration file 2</summary>
    SendConfigurationFrame2 = Bit0 Or Bit2
    ''' <summary>1000 Receive extended frame for IEEE C37.118 / receive reference phasor for IEEE 1344</summary>
    ReceiveExtendedFrame = Bit3
    ''' <summary>Reserved bits</summary>
    ReservedBits = Int16.MaxValue And Not (Bit0 Or Bit1 Or Bit2 Or Bit3)
End Enum
