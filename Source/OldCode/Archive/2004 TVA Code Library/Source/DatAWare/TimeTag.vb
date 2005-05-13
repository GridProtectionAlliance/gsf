'***********************************************************************
'  TimeTag.vb - DatAWare TimeTag Class
'  Copyright © 2005 - TVA, all rights reserved
'  
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source created
'
'***********************************************************************
Option Explicit On 

Namespace DatAWare

    Public Class TimeTag

        Implements IComparable

        ' DatAWare time tags are measured as the number of seconds since January 1, 1995,
        ' so we calculate this date to get offset in ticks for later conversion...
        Private Shared timeTagOffsetTicks As Long = (New DateTime(1995, 1, 1, 0, 0, 0)).Ticks

        Private ttag As Double

        Public Sub New(ByVal ttag As Double)

            Value = ttag

        End Sub

        Public Sub New(ByVal dtm As DateTime)

            ' Zero base 100-nanosecond ticks from 1/1/1995 and convert to seconds
            Value = (dtm.Ticks - timeTagOffsetTicks) / 10000000L

        End Sub

        Public Sub New(ByVal timestamp As String)

            Me.New(Convert.ToDateTime(timestamp))

        End Sub

        Public Property Value() As Double
            Get
                Return ttag
            End Get
            Set(ByVal val As Double)
                ttag = val
                If ttag < 0 Then ttag = 0
            End Set
        End Property

        Public Function ToDateTime() As DateTime

            ' Convert time tag seconds to 100-nanosecond ticks and add the 1/1/1995 offset
            Return New DateTime(ttag * 10000000L + timeTagOffsetTicks)

        End Function

        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

        ' TimeTags are sorted in value order
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is TimeTag Then
                Return ttag.CompareTo(DirectCast(obj, TimeTag).Value)
            ElseIf TypeOf obj Is Double Then
                Return ttag.CompareTo(CDbl(obj))
            Else
                Throw New ArgumentException("TimeTag can only be compared with other TimeTags...")
            End If

        End Function

    End Class

End Namespace