'*******************************************************************************************************
'  TVA.Math.RealTimeSlope.vb - Calculates slope of real-time data stream using linear regression
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2004 - J. Ritchie Carroll
'       Initial version of source generated for Real-Time Frequency Monitor
'  01/24/2006 - J. Ritchie Carroll
'       Integrated into code library
'
'*******************************************************************************************************

Imports System.Threading
Imports System.Math
Imports TVA.Math.Common
Imports TVA.DateTime.Common

Namespace Math

    ''' <summary>Calculates slope for a real-time continuous data stream</summary>
    Public Class RealTimeSlope

        Private m_regressionInterval As Integer
        Private m_pointCount As Integer
        Private m_xValues As List(Of Double)
        Private m_yValues As List(Of Double)
        Private m_slope As Double
        Private m_lastSlope As Double
        Private m_slopeRun As Date
        Private m_calculating As Boolean

        Public Event Status(ByVal message As String)
        Public Event Recalculated()

        ''' <summary>Creates a new instance of the real-time slope calculation class</summary>
        ''' <param name="regressionInterval">Time span over which to calculate slope</param>
        ''' <param name="estimatedRefreshInterval">Estimated data points per second</param>
        Public Sub New(ByVal regressionInterval As Integer, ByVal estimatedRefreshInterval As Double)

            m_regressionInterval = regressionInterval
            m_pointCount = m_regressionInterval * (1 / estimatedRefreshInterval)
            m_xValues = New List(Of Double)
            m_yValues = New List(Of Double)
            m_slopeRun = Date.Now

        End Sub

        ''' <summary>Adds a new x, y data pair to continuous data set</summary>
        ''' <param name="x">New x-axis value</param>
        ''' <param name="y">New y-axis value</param>
        Public Sub Calculate(ByVal x As Double, ByVal y As Double)

            SyncLock m_xValues
                ' Add latest values to regression data set
                m_xValues.Add(x)
                m_yValues.Add(y)

                ' Keep a constant number of points by removing them from the left
                Do While m_xValues.Count > m_pointCount
                    m_xValues.RemoveAt(0)
                Loop

                Do While m_yValues.Count > m_pointCount
                    m_yValues.RemoveAt(0)
                Loop
            End SyncLock

            If m_xValues.Count >= m_pointCount AndAlso Not m_calculating Then
                ' We perform curve fit calculation on seperate thread since it could be time consuming...
                ThreadPool.QueueUserWorkItem(AddressOf PerformCalculation)
            End If

        End Sub

        Private Sub PerformCalculation(ByVal state As Object)

            Try
                m_calculating = True

                Dim xValues As Double()
                Dim yValues As Double()

                ' Calculations are made against a copy of the current data set to keep lock time on
                ' data values down to a minimum - this allows data to be added with minimal delay
                SyncLock m_xValues
                    xValues = m_xValues.ToArray()
                    yValues = m_yValues.ToArray()
                End SyncLock

                ' Take new values and calculate slope (curve fit for 1st order polynomial)
                m_slope = CurveFit(1, m_pointCount, xValues, yValues)(1)
            Catch ex As Exception
                RaiseEvent Status("CurveFit failed: " & ex.Message)
            Finally
                m_calculating = False
            End Try

            If Sign(m_slope) <> Sign(m_lastSlope) Then m_slopeRun = Date.Now
            m_lastSlope = m_slope

            ' Notify consumer of new calculated slope
            RaiseEvent Recalculated()

        End Sub

        ''' <summary>Current calculated slope for data set</summary>
        Public ReadOnly Property Slope() As Double
            Get
                Return m_slope
            End Get
        End Property

        ''' <summary>Run-time, in seconds, for which slope has maintained a continuous positive or negative trend</summary>
        Public ReadOnly Property RunTime() As Double
            Get
                Return (m_regressionInterval + TicksToSeconds(Date.Now.Ticks - m_slopeRun.Ticks))
            End Get
        End Property

    End Class

End Namespace
