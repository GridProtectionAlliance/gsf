'*******************************************************************************************************
'  Tva.DateTime.NtpTimeTag.vb - Standard Network Time Protocol Timetag Class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Original version of source code generated
'  12/21/2005 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.DateTime)
'
'*******************************************************************************************************

Imports Tva.DateTime.Common

Namespace DateTime

    ''' <summary>Standard Network Time Protocol Timetag</summary>
    Public Class NtpTimeTag

        Implements IComparable

        ' NTP dates are measured as the number of seconds since 1/1/1900, so we calculate this
        ' date to get offset in ticks for later conversion...
        Private Shared m_ntpDateOffsetTicks As Long = (New Date(1900, 1, 1, 0, 0, 0)).Ticks

        Private m_seconds As Double

        ''' <summary>Creates new NTP timetag given number of seconds since 1/1/1900</summary>
        ''' <param name="seconds">Number of seconds since 1/1/1900</param>
        Public Sub New(ByVal seconds As Double)

            Value = seconds

        End Sub

        ''' <summary>Creates new NTP timetag given standard .NET DateTime</summary>
        ''' <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1900)</param>
        Public Sub New(ByVal timestamp As Date)

            ' Zero base 100-nanosecond ticks from 1/1/1900 and convert to seconds
            Value = TicksToSeconds(timestamp.Ticks - m_ntpDateOffsetTicks)

        End Sub

        ''' <summary>Value represents number of seconds since 1/1/1900</summary>
        Public Property Value() As Double
            Get
                Return m_seconds
            End Get
            Set(ByVal seconds As Double)
                m_seconds = seconds
                If m_seconds < 0 Then m_seconds = 0
            End Set
        End Property

        ''' <summary>Returns standard .NET DateTime representation for timetag</summary>
        Public Function ToDateTime() As Date

            '  Convert m_seconds to 100-nanosecond ticks and add the 1/1/1900 offset
            Return New Date(SecondsToTicks(m_seconds) + m_ntpDateOffsetTicks)

        End Function

        ''' <summary>Returns basic textual representation for timetag</summary>
        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

        ''' <summary>Compares this NTP timetag to another one</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is NtpTimeTag Then
                Return m_seconds.CompareTo(DirectCast(obj, NtpTimeTag).Value)
            ElseIf TypeOf obj Is Double Then
                Return m_seconds.CompareTo(CDbl(obj))
            Else
                Throw New ArgumentException("NtpTimeTag can only be compared with other NtpTimeTags...")
            End If

        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Return (CompareTo(obj) = 0)

        End Function

        Public Overrides Function GetHashCode() As Integer

            Return Convert.ToInt32(m_seconds * 1000)

        End Function

    End Class

End Namespace