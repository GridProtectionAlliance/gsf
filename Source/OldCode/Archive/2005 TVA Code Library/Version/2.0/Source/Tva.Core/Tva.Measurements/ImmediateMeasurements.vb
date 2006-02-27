'*******************************************************************************************************
'  Tva.Measurements.ImmediateMeasurements.vb - Collection of latest received measurements
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents the absolute latest received measurements
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    ' This is essentially a collection of measurements at a given timestamp
    Public Class ImmediateMeasurements

        Private m_measurements As Dictionary(Of Integer, TemporalMeasurement)
        Private m_taggedMeasurements As Dictionary(Of String, List(Of Integer))
        Private m_timeDeviationTolerance As Double
        Private m_ticks As Long

        Public Sub New(ByVal timeDeviationTolerance As Double)

            m_timeDeviationTolerance = timeDeviationTolerance
            m_measurements = New Dictionary(Of Integer, TemporalMeasurement)
            m_taggedMeasurements = New Dictionary(Of String, List(Of Integer))

        End Sub

        ''' <summary>Defines tagged measurements from a data table</summary>
        ''' <remarks>Expects tag to be aliased as "Tag" and measurement ID to be aliased as "ID"</remarks>
        Public Sub DefineTaggedMeasurements(ByVal measurements As DataTable)

            Dim tag As String
            Dim id As Integer

            For Each row As DataRow In measurements.Rows
                ' Get relevant fields
                tag = row("Tag")
                id = row("ID")

                ' Check for new tag
                If Not m_taggedMeasurements.ContainsKey(tag) Then
                    m_taggedMeasurements.Add(tag, New List(Of Integer))
                End If

                ' Add measurement to tag's measurement list
                m_taggedMeasurements(tag).Add(id)
            Next

        End Sub

        Public ReadOnly Property This() As ImmediateMeasurements
            Get
                Return Me
            End Get
        End Property

        Public Sub AddTaggedMeasurement(ByVal tag As String, ByVal measurementID As Integer)

            ' Check for new tag
            If Not m_taggedMeasurements.ContainsKey(tag) Then
                m_taggedMeasurements.Add(tag, New List(Of Integer))
            End If

            ' Add measurement to tag's measurement list
            m_taggedMeasurements(tag).Add(measurementID)

        End Sub

        Public Property Ticks() As Long
            Get
                Return m_ticks
            End Get
            Friend Set(ByVal value As Long)
                ' TODO: this must be equalized with SampleQueue base time - better to just reference that instead.
                m_ticks = value
            End Set
        End Property

        Public ReadOnly Property Timestamp() As Date
            Get
                Return New Date(m_ticks)
            End Get
        End Property

        Public ReadOnly Property MeasurementIDs() As Dictionary(Of Integer, TemporalMeasurement).KeyCollection
            Get
                Return m_measurements.Keys
            End Get
        End Property

        Public ReadOnly Property Tags() As Dictionary(Of String, List(Of Integer)).KeyCollection
            Get
                Return m_taggedMeasurements.Keys
            End Get
        End Property

        Public ReadOnly Property TagMeasurementIDs(ByVal tag As String) As List(Of Integer)
            Get
                Return m_taggedMeasurements(tag)
            End Get
        End Property

        Default Public ReadOnly Property Value(ByVal measurementID As Integer) As Double
            Get
                Return Measurement(measurementID)(m_ticks)
            End Get
        End Property

        Public ReadOnly Property Measurement(ByVal measurementID As Integer) As TemporalMeasurement
            Get
                SyncLock m_measurements
                    Dim value As TemporalMeasurement

                    If Not m_measurements.TryGetValue(measurementID, value) Then
                        ' Create new temporal measurement if it doesn't exist
                        value = New TemporalMeasurement(measurementID, Double.NaN, m_ticks, m_timeDeviationTolerance)
                        m_measurements.Add(measurementID, value)
                    End If

                    Return value
                End SyncLock
            End Get
        End Property

        Public Function CalculateAverage(ByRef count As Integer) As Double

            Dim measurement As Double
            Dim total As Double

            SyncLock m_measurements
                For Each entry As KeyValuePair(Of Integer, TemporalMeasurement) In m_measurements
                    measurement = Value(entry.Key)
                    If Not Double.IsNaN(measurement) Then
                        total += measurement
                        count += 1
                    End If
                Next
            End SyncLock

            Return total / count

        End Function

        Public Function CalculateTagAverage(ByVal tag As String, ByRef count As Integer) As Double

            Dim measurement As Double
            Dim total As Double

            For Each measurementID As Integer In m_taggedMeasurements(tag)
                measurement = Value(measurementID)
                If Not Double.IsNaN(measurement) Then
                    total += measurement
                    count += 1
                End If
            Next

            Return total / count

        End Function

        Public ReadOnly Property Minimum() As Double
            Get
                Dim minValue As Double = Double.MaxValue
                Dim measurement As Double

                SyncLock m_measurements
                    For Each entry As KeyValuePair(Of Integer, TemporalMeasurement) In m_measurements
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

                SyncLock m_measurements
                    For Each entry As KeyValuePair(Of Integer, TemporalMeasurement) In m_measurements
                        measurement = Value(entry.Key)
                        If Not Double.IsNaN(measurement) Then
                            If measurement > maxValue Then maxValue = measurement
                        End If
                    Next
                End SyncLock

                Return maxValue
            End Get
        End Property

        Public ReadOnly Property TagMinimum(ByVal tag As String) As Double
            Get
                Dim minValue As Double = Double.MaxValue
                Dim measurement As Double

                For Each measurementID As Integer In m_taggedMeasurements(tag)
                    measurement = Value(measurementID)
                    If Not Double.IsNaN(measurement) Then
                        If measurement < minValue Then minValue = measurement
                    End If
                Next

                Return minValue
            End Get
        End Property

        Public ReadOnly Property TagMaximum(ByVal tag As String) As Double
            Get
                Dim maxValue As Double = Double.MinValue
                Dim measurement As Double

                For Each measurementID As Integer In m_taggedMeasurements(tag)
                    measurement = Value(measurementID)
                    If Not Double.IsNaN(measurement) Then
                        If measurement > maxValue Then maxValue = measurement
                    End If
                Next

                Return maxValue
            End Get
        End Property

    End Class

End Namespace
