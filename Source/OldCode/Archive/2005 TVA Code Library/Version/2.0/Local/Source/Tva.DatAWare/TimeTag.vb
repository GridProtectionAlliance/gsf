'*******************************************************************************************************
'  TimeTag.vb - DatAWare TimeTag Class
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
'  05/03/2006 - J. Ritchie Carroll
'       Initial version of source imported from 1.1 code library
'  07/12/2006 - J. Ritchie Carroll
'       Modified class to be derived from new "TimeTagBase" class 
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.DateTime

''' <summary>Standard DatAWare Timetag</summary>
Public Class TimeTag

    Inherits TimeTagBase

    ' DatAWare time tags are measured as the number of seconds since January 1, 1995,
    ' so we calculate this date to get offset in ticks for later conversion...
    Private Shared m_timeTagOffsetTicks As Long = (New Date(1995, 1, 1, 0, 0, 0)).Ticks

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Shared ReadOnly MinValue As TimeTag = New TimeTag(0)

    Public Shared ReadOnly MaxValue As TimeTag = New TimeTag(2147483647.999)

    ''' <summary>Creates new DatAWare timetag given number of seconds since 1/1/1995</summary>
    ''' <param name="seconds">Number of seconds since 1/1/1995</param>
    Public Sub New(ByVal seconds As Double)

        MyBase.New(m_timeTagOffsetTicks, seconds)

    End Sub

    ''' <summary>Creates new DatAWare timetag given standard .NET DateTime</summary>
    ''' <param name="timestamp">.NET DateTime to create DatAWare timetag from (minimum valid date is 1/1/1995)</param>
    Public Sub New(ByVal timestamp As Date)

        MyBase.New(m_timeTagOffsetTicks, timestamp)

    End Sub

    ''' <summary>Properly formats a .NET DateTime into a DatAWare timetag string format</summary>
    Public Shared ReadOnly Property StringFormat(ByVal timestamp As Date) As String
        Get
            Return (New TimeTag(timestamp)).ToString()
        End Get
    End Property

    ''' <summary>Returns standard string representation for a DatAWare timetag</summary>
    Public Overrides Function ToString() As String

        Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

    End Function

End Class