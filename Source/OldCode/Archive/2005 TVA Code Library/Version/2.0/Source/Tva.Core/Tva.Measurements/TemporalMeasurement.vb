'***********************************************************************
'  TemporalMeasurement.vb - Stores and retrieves up-to-date measurements
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Public Class TemporalMeasurement

    Inherits Measurement

    Private m_timeDeviationTolerance As Integer

    Public Sub New(ByVal timeDeviationTolerance As Integer)

        MyBase.New()
        m_timeDeviationTolerance = timeDeviationTolerance

    End Sub

    Public Sub New(ByVal timeDeviationTolerance As Integer, ByVal index As Integer, ByVal value As Double, ByVal timestamp As Date)

        MyBase.New(index, value, timestamp)
        m_timeDeviationTolerance = timeDeviationTolerance

    End Sub

    Default Public Shadows Property Value(ByVal currentTime As Date) As Double
        Get
            ' We only return a measurement value that is up-to-date...
            If (currentTime.Ticks - Timestamp.Ticks) / 10000000L <= m_timeDeviationTolerance Then
                Return MyBase.Value
            Else
                Return Double.NaN
            End If
        End Get
        Set(ByVal value As Double)
            ' We only store a value that is newer than the current value
            If currentTime.Ticks > Timestamp.Ticks Then
                MyBase.Value = value
                Timestamp = currentTime
            End If
        End Set
    End Property

End Class