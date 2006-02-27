'*******************************************************************************************************
'  Tva.Measurements.TemporalMeasurement.vb - Time sensitive measurement implementation
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a time constrained measured value
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.DateTime.Common

Namespace Measurements

    Public Class TemporalMeasurement

        Inherits Measurement

        Private m_timeDeviationTolerance As Double

        Public Sub New(ByVal timeDeviationTolerance As Double)

            MyClass.New(-1, Double.NaN, 0, timeDeviationTolerance)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal timestamp As Date, ByVal timeDeviationTolerance As Integer)

            MyClass.New(index, value, timestamp.Ticks, timeDeviationTolerance)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal ticks As Long, ByVal timeDeviationTolerance As Integer)

            MyBase.New(index, value, ticks)
            m_timeDeviationTolerance = timeDeviationTolerance

        End Sub

        ''' <summary>Allowed base time deviation in seconds</summary>
        Public Property TimeDeviationTolerance() As Double
            Get
                Return m_timeDeviationTolerance
            End Get
            Set(ByVal value As Double)
                m_timeDeviationTolerance = value
            End Set
        End Property

        ''' <summary>Gets or sets numeric value of this measurement, constrained within specified ticks</summary>
        ''' <remarks>
        ''' <para>Get operation will return NaN if ticks are outside of time deviation tolerance</para>
        ''' <para>Set operation will not store value if ticks are outside of time deviation tolerance</para>
        ''' </remarks>
        Default Public Overloads Property Value(ByVal ticks As Long) As Double
            Get
                ' We only return a measurement value that is up-to-date...
                If System.Math.Abs(TicksToSeconds(Me.Ticks - ticks)) <= m_timeDeviationTolerance Then
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
        ''' <para>Get operation will return NaN if timestamp is outside of time deviation tolerance</para>
        ''' <para>Set operation will not store value if timestamp is outside of time deviation tolerance</para>
        ''' </remarks>
        Default Public Overloads Property Value(ByVal timestamp As Date) As Double
            Get
                Return Me(timestamp.Ticks)
            End Get
            Set(ByVal value As Double)
                Me(timestamp.Ticks) = value
            End Set
        End Property

    End Class

End Namespace
