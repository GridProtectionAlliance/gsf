' 06/04/2007

Imports System.Threading

Namespace Diagnostics

    Public Class PerformanceCounter
        Implements IDisposable

#Region " Member Declaration "

        Private m_lastValue As Single
        Private m_minimumValue As Single
        Private m_maximumValue As Single
        Private m_averagingWindow As Integer

        Private m_counter As System.Diagnostics.PerformanceCounter
        Private m_counterValues As List(Of Double)

#End Region

#Region " Code Scope: Public "

        Public Const DefaultAveragingWindow As Integer = 120

        Public Sub New(ByVal categoryName As String, ByVal counterName As String, ByVal instanceName As String)

            MyBase.New()
            m_minimumValue = Single.MaxValue
            m_maximumValue = Single.MinValue
            m_averagingWindow = DefaultAveragingWindow
            m_counter = New System.Diagnostics.PerformanceCounter(categoryName, counterName, instanceName)
            m_counterValues = New List(Of Double)()

        End Sub

        Public Property AveragingWindow() As Integer
            Get
                Return m_averagingWindow
            End Get
            Set(ByVal value As Integer)
                If value > 0 Then
                    Interlocked.Exchange(m_averagingWindow, value)
                Else
                    Throw New ArgumentOutOfRangeException("AveragingWindow", "Value must be greater than 0.")
                End If
            End Set
        End Property

        Public ReadOnly Property LastValue() As Single
            Get
                Return m_lastValue
            End Get
        End Property

        Public ReadOnly Property MinimumValue() As Single
            Get
                Return m_minimumValue
            End Get
        End Property

        Public ReadOnly Property MaximumValue() As Single
            Get
                Return m_maximumValue
            End Get
        End Property

        Public ReadOnly Property AverageValue() As Single
            Get
                SyncLock m_counterValues
                    Return Convert.ToSingle(Math.Common.Average(m_counterValues))
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property BaseCounter() As System.Diagnostics.PerformanceCounter
            Get
                Return m_counter
            End Get
        End Property

        Public Sub Sample()

            Try
                SyncLock m_counter
                    Interlocked.Exchange(m_lastValue, m_counter.NextValue())
                End SyncLock

                If m_lastValue < m_minimumValue Then Interlocked.Exchange(m_minimumValue, m_lastValue)
                If m_lastValue > m_maximumValue Then Interlocked.Exchange(m_maximumValue, m_lastValue)

                SyncLock m_counterValues
                    m_counterValues.Add(m_lastValue)
                    Do While m_counterValues.Count > m_averagingWindow
                        m_counterValues.RemoveAt(0)
                    Loop
                End SyncLock
            Catch ex As InvalidOperationException
                ' If we're monitoring performance of an application that's not running (it was not running to begin with,
                ' or it was running but it no longer running), we'll encounter an InvalidOperationException exception.
                ' In this case we'll reset the values and absorb the exception.
                Reset()
            End Try

        End Sub

        Public Sub Reset()

            Interlocked.Exchange(m_lastValue, 0)
            Interlocked.Exchange(m_minimumValue, 0)
            Interlocked.Exchange(m_maximumValue, 0)

            SyncLock m_counterValues
                m_counterValues.Clear()
            End SyncLock

        End Sub

#End Region

        Private disposedValue As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Reset()
                    SyncLock m_counter
                        m_counter.Dispose()
                    End SyncLock
                End If
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