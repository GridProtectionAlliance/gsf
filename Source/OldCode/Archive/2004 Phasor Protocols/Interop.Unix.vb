'***********************************************************************
'  Interop.Unix.vb - Unix Interoperability Classes
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace Interop.Unix

    Public Class TimeTag

        Implements IComparable

        ' Unix dates are measured as the number of seconds since 1/1/1970, so we calculate this
        ' date to get offset in ticks for later conversion...
        Private Shared unixDateOffsetTicks As Long = (New DateTime(1970, 1, 1, 0, 0, 0)).Ticks

        Private seconds As Double

        Public Sub New(ByVal seconds As Double)

            Value = seconds

        End Sub

        Public Sub New(ByVal dtm As DateTime)

            ' Zero base 100-nanosecond ticks from 1/1/1970 and convert to seconds
            Value = (dtm.Ticks - unixDateOffsetTicks) / 10000000L

        End Sub

        Public Property Value() As Double
            Get
                Return seconds
            End Get
            Set(ByVal val As Double)
                seconds = val
                If seconds < 0 Then seconds = 0
            End Set
        End Property

        Public Function ToDateTime() As DateTime

            ' Convert seconds to 100-nanosecond ticks and add the 1/1/1970 offset
            Return New DateTime(seconds * 10000000L + unixDateOffsetTicks)

        End Function

        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

        ' TimeTags are sorted in value order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is TimeTag Then
                Return seconds.CompareTo(DirectCast(obj, TimeTag).Value)
            ElseIf TypeOf obj Is Double Then
                Return seconds.CompareTo(CDbl(obj))
            Else
                Throw New ArgumentException("TimeTag can only be compared with other TimeTags...")
            End If

        End Function

    End Class

End Namespace
