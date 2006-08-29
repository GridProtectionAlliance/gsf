'*******************************************************************************************************
'  Tva.Measurements.TemporalMeasurement.vb - Time sensitive measurement implementation
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a time constrained measured value
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.DateTime.Common

Namespace Measurements

    Public Class TemporalMeasurement

        Inherits Measurement

        Private m_lagTime As Double         ' Allowed past time deviation tolerance
        Private m_leadTime As Double        ' Allowed future time deviation tolerance

        Public Sub New(ByVal lagTime As Double, ByVal leadTime As Double)

            MyClass.New(-1, Double.NaN, 0, lagTime, leadTime)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal timestamp As Date, ByVal lagTime As Double, ByVal leadTime As Double)

            MyClass.New(index, value, timestamp.Ticks, lagTime, leadTime)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal ticks As Long, ByVal lagTime As Double, ByVal leadTime As Double)

            MyBase.New(index, value, ticks)

            If lagTime <= 0 Then Throw New ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one")
            If leadTime <= 0 Then Throw New ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one")

            m_lagTime = lagTime
            m_leadTime = leadTime

        End Sub

        ''' <summary>Allowed past time deviation tolerance in seconds (can be subsecond)</summary>
        ''' <remarks>
        ''' <para>This value defines the time sensitivity to past measurement timestamps.</para>
        ''' <para>Defined the number of seconds allowed before assuming a measurement timestamp is too old.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one</exception>
        Public Property LagTime() As Double
            Get
                Return m_lagTime
            End Get
            Set(ByVal value As Double)
                If value <= 0 Then Throw New ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one")
                m_lagTime = value
            End Set
        End Property

        ''' <summary>Allowed future time deviation tolerance in seconds (can be subsecond)</summary>
        ''' <remarks>
        ''' <para>This value defines the time sensitivity to future measurement timestamps.</para>
        ''' <para>Defined the number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one</exception>
        Public Property LeadTime() As Double
            Get
                Return m_leadTime
            End Get
            Set(ByVal value As Double)
                If value <= 0 Then Throw New ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one")
                m_leadTime = value
            End Set
        End Property

        ''' <summary>Returns numeric value of this measurement, constrained within specified ticks</summary>
        ''' <remarks>Operation will return NaN if ticks are outside of time deviation tolerances</remarks>
        Public Overloads ReadOnly Property AdjustedValue(ByVal ticks As Long) As Double
            Get
                ' We only return a measurement value that is up-to-date...
                If TimeIsValid(ticks, Me.Ticks, m_lagTime, m_leadTime) Then
                    Return MyBase.AdjustedValue
                Else
                    Return Double.NaN
                End If
            End Get
        End Property

        ''' <summary>Gets or sets numeric value of this measurement, constrained within specified ticks</summary>
        ''' <remarks>
        ''' <para>Get operation will return NaN if ticks are outside of time deviation tolerances</para>
        ''' <para>Set operation will only store a value that is newer than the cached value</para>
        ''' </remarks>
        Default Public Overloads Property RawValue(ByVal ticks As Long) As Double
            Get
                ' We only return a measurement value that is up-to-date...
                If TimeIsValid(ticks, Me.Ticks, m_lagTime, m_leadTime) Then
                    Return MyBase.Value
                Else
                    Return Double.NaN
                End If
            End Get
            Set(ByVal value As Double)
                ' We only store a value that is newer than the current value
                If ticks > Me.Ticks Then
                    MyBase.Value = value
                    Me.Ticks = ticks
                End If
            End Set
        End Property

        ''' <summary>Gets or sets numeric value of this measurement, constrained within specified timestamp</summary>
        ''' <remarks>
        ''' <para>Get operation will return NaN if timestamp is outside of time deviation tolerances</para>
        ''' <para>Set operation will only store a value that is newer than the cached value</para>
        ''' </remarks>
        Default Public Overloads Property RawValue(ByVal timestamp As Date) As Double
            Get
                Return Me(timestamp.Ticks)
            End Get
            Set(ByVal value As Double)
                Me(timestamp.Ticks) = value
            End Set
        End Property

    End Class

End Namespace
