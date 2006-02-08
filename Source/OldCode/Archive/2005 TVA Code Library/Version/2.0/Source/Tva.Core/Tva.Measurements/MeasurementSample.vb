'***********************************************************************
'  MeasurementSample.vb - One second of time sync'd measurement values
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
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

Imports TVA.Shared.DateTime

' This class represents a complete sample of data for a given second - a time indexed sub-second set of synchronized measurement rows.
Public Class MeasurementSample

    Implements IComparable

    Private m_parent As MeasurementConcentrator
    Private m_baseTime As Date
    Private m_published As Boolean

    Public Rows As MeasurementValues()

    Public Sub New(ByVal parent As MeasurementConcentrator, ByVal timestamp As Date)

        m_parent = parent
        m_baseTime = BaselinedTimestamp(timestamp)
        Rows = Array.CreateInstance(GetType(MeasurementValues), m_parent.SamplesPerSecond)

        For x As Integer = 0 To Rows.Length - 1
            Rows(x) = New MeasurementValues(m_parent, m_baseTime, x)
        Next

    End Sub

    Public ReadOnly Property BaseTime() As Date
        Get
            Return m_baseTime
        End Get
    End Property

    Public ReadOnly Property Published() As Boolean
        Get
            If Not m_published Then
                Dim allPublished As Boolean = True

                ' The sample has been completely processed once all data packets have been published
                For x As Integer = 0 To Rows.Length - 1
                    If Not Rows(x).Published Then
                        allPublished = False
                        Exit For
                    End If
                Next

                If allPublished Then m_published = True
                Return allPublished
            Else
                Return True
            End If
        End Get
    End Property

    ' Data samples are sorted in timestamp order
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is MeasurementSample Then
            Return m_baseTime.CompareTo(DirectCast(obj, MeasurementSample).BaseTime)
        Else
            Throw New ArgumentException("MeasurementSample can only be compared with other SynchronizedMeasurementSamples...")
        End If

    End Function

End Class
