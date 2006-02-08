'***********************************************************************
'  ImmediateMeasurements.vb - Absolute latest received measurements
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

' This is essentially a collection of measurements at a given timestamp
Public Class ImmediateMeasurements

    Private m_parent As MeasurementConcentrator
    Private m_measurements As Hashtable
    Private m_taggedMeasurements As Hashtable
    Private m_totalReporting As Integer
    Private m_taggedTotalReporting As Hashtable
    Private m_currentTimestamp As Date

    Public Sub New(ByVal parent As MeasurementConcentrator)

        m_parent = parent
        m_taggedMeasurements = New Hashtable
        m_measurements = New Hashtable
        m_taggedTotalReporting = New Hashtable

    End Sub

    Public Sub DefineMeasurements(ByVal measurements As DataTable)

        Dim tag As String
        Dim id As Integer

        For Each row As DataRow In measurements.Rows
            ' Get relevant fields
            tag = row("Tag")
            id = row("ID")

            ' Check for new tag
            If Not m_taggedMeasurements.ContainsKey(tag) Then
                m_taggedMeasurements.Add(tag, New ArrayList)
                m_taggedTotalReporting.Add(tag, Double.NaN)
            End If

            ' Add measurement to tag's measurement list
            DirectCast(m_taggedMeasurements(tag), ArrayList).Add(id)

            ' Add measurement to overall frequency measurement list
            m_measurements.Add(id, New TemporalMeasurement(m_parent.TimeDeviationTolerance))
        Next

    End Sub

    Public ReadOnly Property This() As ImmediateMeasurements
        Get
            Return Me
        End Get
    End Property

    Public Property CurrentTimeStamp() As Date
        Get
            Return m_currentTimestamp
        End Get
        ' TODO: Make this Set property have friend access when migrating to 2.0 framework...
        Set(ByVal value As Date)
            If value.Ticks > m_currentTimestamp.Ticks Then m_currentTimestamp = value
        End Set
    End Property

    Public ReadOnly Property MeasurementIDs() As ICollection
        Get
            Return m_measurements.Keys
        End Get
    End Property

    Public ReadOnly Property Tags() As ICollection
        Get
            Return m_taggedMeasurements.Keys
        End Get
    End Property

    Public ReadOnly Property TagMeasurementIDs(ByVal tag As String) As ICollection
        Get
            Return m_taggedMeasurements(tag)
        End Get
    End Property

    Default Public ReadOnly Property Value(ByVal measurementID As Integer) As Double
        Get
            Return Measurement(measurementID)(m_currentTimestamp)
        End Get
    End Property

    Public ReadOnly Property Measurement(ByVal measurementID As Integer) As TemporalMeasurement
        Get
            SyncLock m_measurements.SyncRoot
                Return m_measurements(measurementID)
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property TotalReporting() As Integer
        Get
            Return m_totalReporting
        End Get
    End Property

    Public ReadOnly Property TotalTagReporting(ByVal tag As String) As Integer
        Get
            Return m_taggedTotalReporting(tag)
        End Get
    End Property

    Public ReadOnly Property Average() As Double
        Get
            Dim measurement As Double
            Dim total As Double
            Dim count As Integer

            SyncLock m_measurements.SyncRoot
                For Each entry As DictionaryEntry In m_measurements
                    measurement = Value(entry.Key)
                    If Not Double.IsNaN(measurement) Then
                        total += measurement
                        count += 1
                    End If
                Next
            End SyncLock

            m_totalReporting = count
            Return total / count
        End Get
    End Property

    Public ReadOnly Property Minimum() As Double
        Get
            Dim minValue As Double = Double.MaxValue
            Dim measurement As Double

            SyncLock m_measurements.SyncRoot
                For Each entry As DictionaryEntry In m_measurements
                    measurement = Value(entry.Key)
                    If Not Double.IsNaN(measurement) Then
                        If measurement < minValue Then minValue = measurement
                    End If
                Next
            End SyncLock

            Return minValue
        End Get
    End Property

    Public ReadOnly Property Maximum() As Double
        Get
            Dim maxValue As Double = Double.MinValue
            Dim measurement As Double

            SyncLock m_measurements.SyncRoot
                For Each entry As DictionaryEntry In m_measurements
                    measurement = Value(entry.Key)
                    If Not Double.IsNaN(measurement) Then
                        If measurement > maxValue Then maxValue = measurement
                    End If
                Next
            End SyncLock

            Return maxValue
        End Get
    End Property

    Public ReadOnly Property TagAverage(ByVal tag As String) As Double
        Get
            Dim measurement As Double
            Dim total As Double
            Dim count As Integer

            For Each measurementID As Integer In m_taggedMeasurements(tag)
                measurement = Value(measurementID)
                If Not Double.IsNaN(measurement) Then
                    total += measurement
                    count += 1
                End If
            Next

            m_taggedTotalReporting(tag) = count
            Return total / count
        End Get
    End Property

End Class
