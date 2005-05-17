'***********************************************************************
'  Common.vb - Common declarations and functions for phasor classes
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    Public Enum PhasorFormat As Byte
        Rectangular
        Polar
    End Enum

    Public Enum PhasorType As Byte
        Voltage
        Current
    End Enum

    Public Enum DataFormat As Byte
        FixedInteger
        FloatingPoint
    End Enum

    Public Enum LineFrequency As Byte
        Hz50
        Hz60
    End Enum

    Public Enum NetworkProtocol
        Tcp
        Udp
    End Enum

    Public Delegate Sub ProcessBufferSignature(ByVal buffer As Byte(), ByVal length As Integer)

    Public Delegate Sub UpdateStatusSignature(ByVal status As String)

End Namespace