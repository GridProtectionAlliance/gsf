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
        Private m_averagingSampleCount As Integer

        Private m_processorTimeSamples As List(Of Double)
        Private m_workingSetSamples As List(Of Double)
        Private m_processorTimeCounter As PerformanceCounter
        Private m_workingSetCounter As PerformanceCounter

        Private WithEvents m_samplingTimer As System.Timers.Timer

#End Region

#Region " Code Scope: Public "

        Public Const DefaultSamplingInterval As Integer = 1000
        Public Const DefaultAveragingSampleCount As Integer = 120

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
            m_averagingSampleCount = DefaultAveragingSampleCount
            m_processorTimeCounter = New PerformanceCounter("Process", "% Processor Time", m_processName)
            m_workingSetCounter = New PerformanceCounter("Process", "Working Set", m_processName)
            m_processorTimeSamples = New List(Of Double)()
            m_workingSetSamples = New List(Of Double)()
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

        Public ReadOnly Property AveragingSampleCount() As Integer
            Get
                Return m_averagingSampleCount
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
                SyncLock m_processorTimeSamples
                    Return Convert.ToSingle(Math.Common.Average(m_processorTimeSamples))
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property AverageWorkingSet() As Single
            Get
                SyncLock m_workingSetSamples
                    Return Convert.ToSingle(Math.Common.Average(m_workingSetSamples))
                End SyncLock
            End Get
        End Property

#End Region

#Region " Code Scope: Private "

        Private Sub m_samplingTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_samplingTimer.Elapsed

            SyncLock m_processorTimeCounter
                Interlocked.Exchange(m_lastProcessorTime, m_processorTimeCounter.NextValue())
            End SyncLock
            SyncLock m_workingSetCounter
                Interlocked.Exchange(m_lastWorkingSet, m_workingSetCounter.NextValue())
            End SyncLock

            SyncLock m_processorTimeSamples
                m_processorTimeSamples.Add(m_lastProcessorTime)
                If m_processorTimeSamples.Count > m_averagingSampleCount Then m_processorTimeSamples.RemoveAt(0)
            End SyncLock
            SyncLock m_workingSetSamples
                m_workingSetSamples.Add(m_lastWorkingSet)
                If m_workingSetSamples.Count > m_averagingSampleCount Then m_workingSetSamples.RemoveAt(0)
            End SyncLock

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