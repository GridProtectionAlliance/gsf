'*******************************************************************************************************
'  Tva.Measurements.ImmediateMeasurements.vb - Lastest received measurements collection
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated for Super Phasor Data Concentrator
'  02/23/2006 - J. Ritchie Carroll
'       Classes abstracted for general use and added to TVA code library
'
'*******************************************************************************************************

Namespace Measurements

    ''' <summary>This class represents the absolute latest received measurement values</summary>
    Public Class ImmediateMeasurements

        Private WithEvents m_parent As Concentrator
        Private m_measurements As Dictionary(Of MeasurementKey, TemporalMeasurement)
        Private m_taggedMeasurements As Dictionary(Of String, List(Of MeasurementKey))

        Friend Sub New(ByVal parent As Concentrator)

            m_parent = parent
            m_measurements = New Dictionary(Of MeasurementKey, TemporalMeasurement)
            m_taggedMeasurements = New Dictionary(Of String, List(Of MeasurementKey))

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As ImmediateMeasurements
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Returns key collection of measurement keys</summary>
        Public ReadOnly Property MeasurementKeys() As Dictionary(Of MeasurementKey, TemporalMeasurement).KeyCollection
            Get
                Return m_measurements.Keys
            End Get
        End Property

        ''' <summary>Returns key collection for measurement tags</summary>
        Public ReadOnly Property Tags() As Dictionary(Of String, List(Of MeasurementKey)).KeyCollection
            Get
                Return m_taggedMeasurements.Keys
            End Get
        End Property

        ''' <summary>Returns measurement key list of specified tag, if it exists</summary>
        Public ReadOnly Property TagMeasurementKeys(ByVal tag As String) As List(Of MeasurementKey)
            Get
                Return m_taggedMeasurements(tag)
            End Get
        End Property

        ''' <summary>We retrieve measurement values within time tolerance of concentrator real-time</summary>
        Default Public ReadOnly Property Value(ByVal measurementID As Integer, ByVal source As String) As Double
            Get
                Return Value(New MeasurementKey(measurementID, source))
            End Get
        End Property

        ''' <summary>We retrieve measurement values within time tolerance of concentrator real-time</summary>
        Default Public ReadOnly Property Value(ByVal key As MeasurementKey) As Double
            Get
                Return Measurement(key)(m_parent.RealTimeTicks)
            End Get
        End Property

        ''' <summary>We only store a new measurement value that is newer than the cached value</summary>
        Friend Sub UpdateMeasurementValue(ByVal newMeasurement As IMeasurement)

            With newMeasurement
                Measurement(.Key)(.Ticks) = .Value
            End With

        End Sub

        ''' <summary>Retrieves the specified immediate temporal measurement, creating it if needed</summary>
        Public ReadOnly Property Measurement(ByVal measurementID As Integer, ByVal source As String) As TemporalMeasurement
            Get
                Return Measurement(New MeasurementKey(measurementID, source))
            End Get
        End Property

        ''' <summary>Retrieves the specified immediate temporal measurement, creating it if needed</summary>
        Public ReadOnly Property Measurement(ByVal key As MeasurementKey) As TemporalMeasurement
            Get
                SyncLock m_measurements
                    Dim value As TemporalMeasurement

                    If Not m_measurements.TryGetValue(key, value) Then
                        ' Create new temporal measurement if it doesn't exist
                        With m_parent
                            value = New TemporalMeasurement(key.ID, key.Source, Double.NaN, .RealTimeTicks, .LagTime, .LeadTime)
                            m_measurements.Add(key, value)
                        End With
                    End If

                    Return value
                End SyncLock
            End Get
        End Property

        ''' <summary>Defines tagged measurements from a data table</summary>
        ''' <remarks>Expects tag field to be aliased as "Tag", measurement ID field to be aliased as "ID" and source field to be aliased as "Source"</remarks>
        Public Sub DefineTaggedMeasurements(ByVal taggedMeasurements As DataTable)

            For Each row As DataRow In taggedMeasurements.Rows
                AddTaggedMeasurement(row("Tag").ToString(), New MeasurementKey(Convert.ToInt32(row("ID")), row("Source").ToString()))
            Next

        End Sub

        ''' <summary>Associates a new measurement ID with a tag, creating the new tag if needed</summary>
        ''' <remarks>Allows you to define "grouped" points so you can aggregate certain measurements</remarks>
        Public Sub AddTaggedMeasurement(ByVal tag As String, ByVal key As MeasurementKey)

            With m_taggedMeasurements
                ' Check for new tag
                If Not .ContainsKey(tag) Then
                    .Add(tag, New List(Of MeasurementKey))
                End If

                ' Add measurement to tag's measurement list
                With .Item(tag)
                    If .BinarySearch(key) < 0 Then
                        .Add(key)
                        .Sort()
                    End If
                End With
            End With

        End Sub

        ''' <summary>Calculates an average of all measurements</summary>
        ''' <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
        Public Function CalculateAverage(ByRef count As Integer) As Double

            Dim measurement As Double
            Dim total As Double

            SyncLock m_measurements
                For Each key As MeasurementKey In m_measurements.Keys
                    measurement = Value(key)
                    If Not Double.IsNaN(measurement) Then
                        total += measurement
                        count += 1
                    End If
                Next
            End SyncLock

            Return total / count

        End Function

        ''' <summary>Calculates an average of all measurements associated with the specified tag</summary>
        Public Function CalculateTagAverage(ByVal tag As String, ByRef count As Integer) As Double

            Dim measurement As Double
            Dim total As Double

            For Each key As MeasurementKey In m_taggedMeasurements(tag)
                measurement = Value(key)
                If Not Double.IsNaN(measurement) Then
                    total += measurement
                    count += 1
                End If
            Next

            Return total / count

        End Function

        ''' <summary>Returns the minimum value of all measurements</summary>
        ''' <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
        Public ReadOnly Property Minimum() As Double
            Get
                Dim minValue As Double = Double.MaxValue
                Dim measurement As Double

                SyncLock m_measurements
                    For Each key As MeasurementKey In m_measurements.Keys
                        measurement = Value(key)
                        If Not Double.IsNaN(measurement) Then
                            If measurement < minValue Then minValue = measurement
                        End If
                    Next
                End SyncLock

                Return minValue
            End Get
        End Property

        ''' <summary>Returns the maximum value of all measurements</summary>
        ''' <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
        Public ReadOnly Property Maximum() As Double
            Get
                Dim maxValue As Double = Double.MinValue
                Dim measurement As Double

                SyncLock m_measurements
                    For Each key As MeasurementKey In m_measurements.Keys
                        measurement = Value(key)
                        If Not Double.IsNaN(measurement) Then
                            If measurement > maxValue Then maxValue = measurement
                        End If
                    Next
                End SyncLock

                Return maxValue
            End Get
        End Property

        ''' <summary>Returns the minimum value of all measurements associated with the specified tag</summary>
        Public ReadOnly Property TagMinimum(ByVal tag As String) As Double
            Get
                Dim minValue As Double = Double.MaxValue
                Dim measurement As Double

                For Each key As MeasurementKey In m_taggedMeasurements(tag)
                    measurement = Value(key)
                    If Not Double.IsNaN(measurement) Then
                        If measurement < minValue Then minValue = measurement
                    End If
                Next

                Return minValue
            End Get
        End Property

        ''' <summary>Returns the maximum value of all measurements associated with the specified tag</summary>
        Public ReadOnly Property TagMaximum(ByVal tag As String) As Double
            Get
                Dim maxValue As Double = Double.MinValue
                Dim measurement As Double

                For Each key As MeasurementKey In m_taggedMeasurements(tag)
                    measurement = Value(key)
                    If Not Double.IsNaN(measurement) Then
                        If measurement > maxValue Then maxValue = measurement
                    End If
                Next

                Return maxValue
            End Get
        End Property

        Private Sub m_parent_LagTimeUpdated(ByVal lagTime As Double) Handles m_parent.LagTimeUpdated

            SyncLock m_measurements
                For Each key As MeasurementKey In m_measurements.Keys
                    Measurement(key).LagTime = lagTime
                Next
            End SyncLock

        End Sub

        Private Sub m_parent_LeadTimeUpdated(ByVal leadTime As Double) Handles m_parent.LeadTimeUpdated

            SyncLock m_measurements
                For Each key As MeasurementKey In m_measurements.Keys
                    Measurement(key).LeadTime = leadTime
                Next
            End SyncLock

        End Sub

    End Class

End Namespace
