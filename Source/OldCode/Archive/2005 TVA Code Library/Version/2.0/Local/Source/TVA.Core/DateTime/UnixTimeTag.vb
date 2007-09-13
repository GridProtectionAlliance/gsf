'*******************************************************************************************************
'  TVA.DateTime.UnixTimeTag.vb - Standard Unix Timetag Class
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
'  10/12/2005 - J. Ritchie Carroll
'       Gnerated original version of source.
'  01/05/2006 - J. Ritchie Carroll
'       Migrated 2.0 version of source code from 1.1 source (TVA.Interop.Unix.TimeTag).
'  01/24/2006 - J. Ritchie Carroll
'       Moved into DateTime namespace and renamed to UnixTimeTag.
'  07/12/2006 - J. Ritchie Carroll
'       Modified class to be derived from new "TimeTagBase" class. 
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports TVA.DateTime.Common

Namespace DateTime

    ''' <summary>Standard Unix Timetag</summary>
    Public Class UnixTimeTag

        Inherits TimeTagBase

        ' Unix dates are measured as the number of seconds since 1/1/1970, so this class calculates this
        ' date to get the offset in ticks for later conversion.
        Private Shared m_unixDateOffsetTicks As Long = (New Date(1970, 1, 1, 0, 0, 0)).Ticks

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        ''' <summary>Creates new Unix timetag, given number of seconds since 1/1/1970.</summary>
        ''' <param name="seconds">Number of seconds since 1/1/1970.</param>
        Public Sub New(ByVal seconds As Double)

            MyBase.New(m_unixDateOffsetTicks, seconds)

        End Sub

        ''' <summary>Creates new Unix timetag, given standard .NET DateTime.</summary>
        ''' <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1970).</param>
        Public Sub New(ByVal timestamp As Date)

            MyBase.New(m_unixDateOffsetTicks, timestamp)

        End Sub

    End Class

End Namespace
