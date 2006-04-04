'*******************************************************************************************************
'  Tva.DateTime.UnixTimeTag.vb - Standard Unix Timetag Class
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
'  10/12/2005 - James R Carroll
'       Original version of source generated
'  01/05/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Interop.Unix.TimeTag)
'  01/24/2006 - James R Carroll
'       Moved into DateTime namespace and renamed to UnixTimeTag
'
'*******************************************************************************************************

Imports Tva.DateTime.Common

Namespace DateTime

    ''' <summary>Standard Unix Timetag</summary>
    Public Class UnixTimeTag

        Implements IComparable

        ' Unix dates are measured as the number of seconds since 1/1/1970, so we calculate this
        ' date to get offset in ticks for later conversion...
        Private Shared m_unixDateOffsetTicks As Long = (New Date(1970, 1, 1, 0, 0, 0)).Ticks

        Private m_seconds As Double

        ''' <summary>Creates new Unix timetag given number of seconds since 1/1/1900</summary>
        ''' <param name="seconds">Number of seconds since 1/1/1970</param>
        Public Sub New(ByVal seconds As Double)

            Value = seconds

        End Sub

        ''' <summary>Creates new Unix timetag given standard .NET DateTime</summary>
        ''' <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1970)</param>
        Public Sub New(ByVal timestamp As Date)

            ' Zero base 100-nanosecond ticks from 1/1/1970 and convert to seconds
            Value = TicksToSeconds(timestamp.Ticks - m_unixDateOffsetTicks)

        End Sub

        ''' <summary>Value represents number of seconds since 1/1/1970</summary>
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

            ' Convert m_seconds to 100-nanosecond ticks and add the 1/1/1970 offset
            Return New Date(SecondsToTicks(m_seconds) + m_unixDateOffsetTicks)

        End Function

        ''' <summary>Returns basic textual representation for timetag</summary>
        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

        ''' <summary>Compares this Unix timetag to another one</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is UnixTimeTag Then
                Return m_seconds.CompareTo(DirectCast(obj, UnixTimeTag).Value)
            ElseIf TypeOf obj Is Double Then
                Return m_seconds.CompareTo(CDbl(obj))
            Else
                Throw New ArgumentException("UnixTimeTag can only be compared with other UnixTimeTags...")
            End If

        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Return (CompareTo(obj) = 0)

        End Function

        Public Overrides Function GetHashCode() As Integer

            Return System.Convert.ToInt32(m_seconds * 1000)

        End Function

    End Class

End Namespace
