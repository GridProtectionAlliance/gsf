' 06/01/2007

Option Strict On

Imports System.Threading
Imports TVA.Common

Namespace Diagnostics

    Public Class PerformanceMonitor
        Implements IDisposable

#Region " Member Declaration "

        Private m_processName As String
        Private m_lastProcessorTime As Single
        Private m_lastWorkingSet As Single
        Private m_accumProcessorTime As Double
        Private m_accumWorkingSet As Double
        Private m_averagingSampleCount As Long

        Private m_processorTimeCounter As PerformanceCounter
        Private m_workingSetCounter As PerformanceCounter

        Private WithEvents m_samplingTimer As System.Timers.Timer

#End Region

#Region " Code Scope: Public "

        Public Const DefaultSamplingInterval As Integer = 1000

        Public Sub New()

            MyClass.New(DefaultSamplingInterval)

        End Sub

        Public Sub New(ByVal samplingInterval As Double)

            MyClass.New(Process.GetCurrentProcess().ProcessName, samplingInterval)

        End Sub

        Public Sub New(ByVal processName As String)

            MyClass.New(processName, DefaultSamplingInterval)

        End Sub

        Public Sub New(ByVal processName As String, ByVal samplingInterval As Double)

            MyBase.New()
            m_processName = processName
            m_processorTimeCounter = New PerformanceCounter("Process", "% Processor Time", m_processName)
            m_workingSetCounter = New PerformanceCounter("Process", "Working Set", m_processName)
            m_samplingTimer = New System.Timers.Timer(samplingInterval)
            m_samplingTimer.Start()

        End Sub

        Public ReadOnly Property ProcessName() As String
            Get
                Return m_processName
            End Get
        End Property

        Public ReadOnly Property SamplingInterval() As Double
            Get
                Return m_samplingTimer.Interval
            End Get
        End Property

        Public ReadOnly Property LastProcessorTime() As Single
            Get
                Return m_lastProcessorTime
            End Get
        End Property

        Public ReadOnly Property LastWorkingSet() As Single
            Get
                Return m_lastWorkingSet
            End Get
        End Property

        Public ReadOnly Property AverageProcessorTime() As Single
            Get
                Return Convert.ToSingle(m_accumProcessorTime / m_averagingSampleCount)
            End Get
        End Property

        Public ReadOnly Property AverageWorkingSet() As Single
            Get
                Return Convert.ToSingle(m_accumWorkingSet / m_averagingSampleCount)
            End Get
        End Property

        Public Sub Reset()

            Interlocked.Exchange(m_accumProcessorTime, 0)
            Interlocked.Exchange(m_accumWorkingSet, 0)
            Interlocked.Exchange(m_averagingSampleCount, 0)

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub m_samplingTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_samplingTimer.Elapsed

            SyncLock m_processorTimeCounter
                Interlocked.Exchange(m_lastProcessorTime, m_processorTimeCounter.NextValue())
            End SyncLock
            SyncLock m_workingSetCounter
                Interlocked.Exchange(m_lastWorkingSet, m_workingSetCounter.NextValue())
            End SyncLock

            Try
                Interlocked.Increment(m_averagingSampleCount)
                Interlocked.Exchange(m_accumProcessorTime, m_accumProcessorTime + m_lastProcessorTime)
                Interlocked.Exchange(m_accumWorkingSet, m_accumWorkingSet + m_lastWorkingSet)
            Catch ex As OverflowException
                ' We'll reset the valriable used for averaging if we encounter an overflow exception.
                Reset()
            Catch ex As Exception
                Throw
            End Try

        End Sub

#End Region

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    m_samplingTimer.Dispose()
                    m_processorTimeCounter.Dispose()
                    m_workingSetCounter.Dispose()
                End If

                ' TODO: free shared unmanaged resources
            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace