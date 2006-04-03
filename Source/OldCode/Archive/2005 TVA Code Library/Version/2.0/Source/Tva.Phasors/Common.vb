'*******************************************************************************************************
'  Common.vb - Common declarations and functions for phasor classes
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer
Imports Tva.Common
Imports Tva.Interop.Bit

''' <summary>Phasor coordinate format</summary>
Public Enum CoordinateFormat As Byte
    Rectangular
    Polar
End Enum

''' <summary>Phasor type</summary>
Public Enum PhasorType As Byte
    Voltage
    Current
End Enum

''' <summary>Data transmission format</summary>
Public Enum DataFormat As Byte
    FixedInteger
    FloatingPoint
End Enum

''' <summary>Nominal line frequency</summary>
Public Enum LineFrequency As Byte
    Hz50
    Hz60
End Enum

''' <summary>Fundamental frame types</summary>
Public Enum FundamentalFrameType
    ConfigurationFrame
    DataFrame
    HeaderFrame
    CommandFrame
    Undetermined
End Enum

''' <summary>Phasor data transport layer</summary>
Public Enum DataTransportLayer
    Tcp
    Udp
    Com
End Enum

''' <summary>PMU data transport protocol</summary>
Public Enum Protocol
    IeeeC37_118V1
    IeeeC37_118D6
    Ieee1344
    BpaPdcStream
End Enum

''' <summary>PMU commands</summary>
Public Enum Command As Int16
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

<CLSCompliant(False)> _
Public Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>Data stream synchrnonization byte</summary>
    Public Const SyncByte As Byte = &HAA

    ''' <summary>This is a common optimized block copy function for any kind of data</summary>
    Public Shared Sub CopyImage(ByVal channel As IChannel, ByVal buffer As Byte(), ByRef index As Integer)

        With channel
            CopyImage(.BinaryImage, buffer, index, .BinaryLength)
        End With

    End Sub

    ''' <summary>This is a common optimized block copy function for binary data</summary>
    Public Shared Sub CopyImage(ByVal source As Byte(), ByVal buffer As Byte(), ByRef index As Integer, ByVal length As Integer)

        If length > 0 Then
            BlockCopy(source, 0, buffer, index, length)
            index += length
        End If

    End Sub

    Public Shared ReadOnly Property NominalFrequencyValue(ByVal nominalFrequency As LineFrequency) As Single
        Get
            Return IIf(nominalFrequency = LineFrequency.Hz60, 60.0!, 50.0!)
        End Get
    End Property

End Class
