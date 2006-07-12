'*******************************************************************************************************
'  Tva.DateTime.NtpTimeTag.vb - Standard Network Time Protocol Timetag Class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - J. Ritchie Carroll
'       Original version of source code generated
'  12/21/2005 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.DateTime)
'  07/12/2006 - J. Ritchie Carroll
'       Modified class to be derived from new "TimeTagBase" class 
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.DateTime.Common

Namespace DateTime

    ''' <summary>Standard Network Time Protocol Timetag</summary>
    Public Class NtpTimeTag

        Inherits TimeTagBase

        ' NTP dates are measured as the number of seconds since 1/1/1900, so we calculate this
        ' date to get offset in ticks for later conversion...
        Private Shared m_ntpDateOffsetTicks As Long = (New Date(1900, 1, 1, 0, 0, 0)).Ticks

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        ''' <summary>Creates new NTP timetag given number of seconds since 1/1/1900</summary>
        ''' <param name="seconds">Number of seconds since 1/1/1900</param>
        Public Sub New(ByVal seconds As Double)

            MyBase.New(m_ntpDateOffsetTicks, seconds)

        End Sub

        ''' <summary>Creates new NTP timetag given standard .NET DateTime</summary>
        ''' <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1900)</param>
        Public Sub New(ByVal timestamp As Date)

            MyBase.New(m_ntpDateOffsetTicks, timestamp)

        End Sub

    End Class

End Namespace