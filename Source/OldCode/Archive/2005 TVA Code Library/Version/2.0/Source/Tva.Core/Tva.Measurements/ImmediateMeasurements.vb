'*******************************************************************************************************
'  Tva.Measurements.ImmediateMeasurements.vb - Lastest received measurements collection
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated for Super Phasor Data Concentrator
'  02/23/2006 - James R Carroll
'       Classes abstracted for general use and added to TVA code library
'
'*******************************************************************************************************

Namespace Measurements

    ''' <summary>This class represents the absolute latest received measurement values</summary>
    Public Class ImmediateMeasurements

        Private m_parent As Concentrator
        Private m_measurements As Dictionary(Of Integer, TemporalMeasurement)
        Private m_taggedMeasurements As Dictionary(Of String, List(Of Integer))

        Friend Sub New(ByVal parent As Concentrator)

            m_parent = parent
            m_measurements = New Dictionary(Of Integer, TemporalMeasurement)
            m_taggedMeasurements = New Dictionary(Of String, List(Of Integer))

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As ImmediateMeasurements
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Returns key collection of measurement ID's</summary>
        Public ReadOnly Property MeasurementIDs() As Dictionary(Of Integer, TemporalMeasurement).KeyCollection
            Get
                Return m_measurements.Keys
            End Get
        End Property

        ''' <summary>Returns key collection for measurement tags</summary>
        Public ReadOnly Property Tags() As Dictionary(Of String, List(Of Integer)).KeyCollection
            Get
                Return m_taggedMeasurements.Keys
            End Get
        End Property

        ''' <summary>Returns measurement ID list of specified tag, if it exists</summary>
        Public ReadOnly Property TagMeasurementIDs(ByVal tag As String) As List(Of Integer)
            Get
                Return m_taggedMeasurements(tag)
            End Get
        End Property

        ''' <summary>We retrieve measurement values within time tolerance of concentrator real-time</summary>
        Default Public ReadOnly Property Value(ByVal measurementID As Integer) As Double
            Get
                Return Measurement(measurementID)(m_parent.RealTimeTicks)
            End Get
        End Property

        ''' <summary>We only store a measurement value that is newer than the cached value</summary>
        Default Friend WriteOnly Property Value(ByVal measurementID As Integer, ByVal timestamp As Date) As Double
            Set(ByVal value As Double)
                Measurement(measurementID)(timestamp.Ticks) = value
            End Set
        End Property

        ''' <summary>We only store a measurement value that is newer than the cached value</summary>
        Default Friend WriteOnly Property Value(ByVal measurementID As Integer, ByVal ticks As Long) As Double
            Set(ByVal value As Double)
                Measurement(measurementID)(ticks) = value
            End Set
        End Property

        ''' <summary>Retrieves the specified immediate temporal measurement, creating it if needed</summary>
        Public ReadOnly Property Measurement(ByVal measurementID As Integer) As TemporalMeasurement
            Get
                SyncLock m_measurements
                    Dim value As TemporalMeasurement

                    If Not m_measurements.TryGetValue(measurementID, value) Then
                        ' Create new temporal measurement if it doesn't exist
                        With m_parent
                            value = New TemporalMeasurement(measurementID, Double.NaN, .RealTimeTicks, .LagTime, .LeadTime)
                            m_measurements.Add(measurementID, value)
                        End With
                    End If

                    Return value
                End SyncLock
            End Get
        End Property

        ''' <summary>Defines tagged measurements from a data table</summary>
        ''' <remarks>Expects tag field to be aliased as "Tag" and measurement ID field to be aliased as "ID"</remarks>
        Public Sub DefineTaggedMeasurements(ByVal taggedMeasurements As DataTable)

            For Each row As DataRow In taggedMeasurements.Rows
                AddTaggedMeasurement(row("Tag"), row("ID"))
            Next

        End Sub

        ''' <summary>Associates a new measurement ID with a tag, creating the new tag if needed</summary>
        ''' <remarks>Allows you to define "grouped" points so you can aggregate certain measurements</remarks>
        Public Sub AddTaggedMeasurement(ByVal tag As String, ByVal measurementID As Integer)

            With m_taggedMeasurements
                ' Check for new tag
                If Not .ContainsKey(tag) Then
                    .Add(tag, New List(Of Integer))
                End If

                ' Add measurement to tag's measurement list
                With .Item(tag)
                    If .BinarySearch(measurementID) < 0 Then
                        .Add(measurementID)
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

        ''' <summary>Calculates an average of all measurements associated with the specified tag</summary>
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

        ''' <summary>Returns the minimum value of all measurements</summary>
        ''' <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
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

        ''' <summary>Returns the maximum value of all measurements</summary>
        ''' <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
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

        ''' <summary>Returns the minimum value of all measurements associated with the specified tag</summary>
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

        ''' <summary>Returns the maximum value of all measurements associated with the specified tag</summary>
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
