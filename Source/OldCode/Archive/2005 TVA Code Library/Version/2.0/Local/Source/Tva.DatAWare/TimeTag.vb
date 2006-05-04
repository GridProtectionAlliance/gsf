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
'
'*******************************************************************************************************

Imports Tva.DateTime.Common

''' <summary>Standard DatAWare Timetag</summary>
Public Class TimeTag

    Implements IComparable

    ' DatAWare time tags are measured as the number of seconds since January 1, 1995,
    ' so we calculate this date to get offset in ticks for later conversion...
    Private Shared m_timeTagOffsetTicks As Long = (New Date(1995, 1, 1, 0, 0, 0)).Ticks

    Private m_seconds As Double

    ''' <summary>Creates new DatAWare timetag given number of seconds since 1/1/1995</summary>
    ''' <param name="seconds">Number of seconds since 1/1/1995</param>
    Public Sub New(ByVal seconds As Double)

        Value = seconds

    End Sub

    ''' <summary>Creates new DatAWare timetag given standard .NET DateTime</summary>
    ''' <param name="timestamp">.NET DateTime to create DatAWare timetag from (minimum valid date is 1/1/1995)</param>
    Public Sub New(ByVal timestamp As Date)

        ' Zero base 100-nanosecond ticks from 1/1/1995 and convert to seconds
        Value = TicksToSeconds(timestamp.Ticks - m_timeTagOffsetTicks)

    End Sub

    ''' <summary>Properly formats a .NET DateTime into a DatAWare timetag string format</summary>
    Public Shared ReadOnly Property StringFormat(ByVal timestamp As Date) As String
        Get
            Return (New TimeTag(timestamp)).ToString()
        End Get
    End Property

    ''' <summary>Value represents number of seconds since 1/1/1995</summary>
    Public Property Value() As Double
        Get
            Return m_seconds
        End Get
        Set(ByVal val As Double)
            m_seconds = val
            If m_seconds < 0 Then m_seconds = 0
        End Set
    End Property

    ''' <summary>Returns standard .NET DateTime representation for timetag</summary>
    Public Function ToDateTime() As Date

        ' Convert m_seconds to 100-nanosecond ticks and add the 1/1/1995 offset
        Return New Date(SecondsToTicks(m_seconds) + m_timeTagOffsetTicks)

    End Function

    ''' <summary>Returns basic textual representation for timetag</summary>
    Public Overrides Function ToString() As String

        Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

    End Function

    ''' <summary>Compares this DatAWare timetag to another one</summary>
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is TimeTag Then
            Return m_seconds.CompareTo(DirectCast(obj, TimeTag).Value)
        ElseIf TypeOf obj Is Double Then
            Return m_seconds.CompareTo(CDbl(obj))
        Else
            Throw New ArgumentException("DatAWare TimeTag can only be compared with other TimeTags...")
        End If

    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        Return (CompareTo(obj) = 0)

    End Function

    Public Overrides Function GetHashCode() As Integer

        Return Convert.ToInt32(m_seconds * 1000)

    End Function

End Class