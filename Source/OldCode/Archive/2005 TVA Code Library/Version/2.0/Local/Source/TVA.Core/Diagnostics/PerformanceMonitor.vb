' 06/01/2007
' JRC: 08/08/2007 - Added lock contention rate and datagram / sec performance counters...
' PCP: 09/04/2007 - Added Status property 

Imports System.Text
Imports TVA.Text.Common

Namespace Diagnostics

    Public Class PerformanceMonitor
        Implements IDisposable

#Region " Member Declaration "

        Private m_processName As String
        Private m_counters As List(Of PerformanceCounter)

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
            m_counters = New List(Of PerformanceCounter)

            If System.Diagnostics.PerformanceCounterCategory.Exists("Process") Then
                m_counters.Add(New PerformanceCounter("Process", "% Processor Time", m_processName))
                m_counters.Add(New PerformanceCounter("Process", "IO Data Bytes/sec", m_processName))
                m_counters.Add(New PerformanceCounter("Process", "IO Data Operations/sec", m_processName))
                m_counters.Add(New PerformanceCounter("Process", "Handle Count", m_processName))
                m_counters.Add(New PerformanceCounter("Process", "Thread Count", m_processName))
                m_counters.Add(New PerformanceCounter("Process", "Working Set", m_processName))
            End If

            If System.Diagnostics.PerformanceCounterCategory.Exists("IPv4") Then
                m_counters.Add(New PerformanceCounter("IPv4", "Datagrams Sent/sec", ""))
                m_counters.Add(New PerformanceCounter("IPv4", "Datagrams Received/sec", ""))
            ElseIf System.Diagnostics.PerformanceCounterCategory.Exists("IP") Then
                m_counters.Add(New PerformanceCounter("IP", "Datagrams Sent/sec", ""))
                m_counters.Add(New PerformanceCounter("IP", "Datagrams Received/sec", ""))
            End If

            If System.Diagnostics.PerformanceCounterCategory.Exists(".NET CLR LocksAndThreads") Then
                m_counters.Add(New PerformanceCounter(".NET CLR LocksAndThreads", "Contention Rate / sec", m_processName))
            End If

            m_samplingTimer = New System.Timers.Timer(samplingInterval)
            m_samplingTimer.Start()

        End Sub

        Public Property ProcessName() As String
            Get
                Return m_processName
            End Get
            Set(ByVal value As String)
                m_processName = value
                SyncLock m_counters
                    For Each counter As PerformanceCounter In m_counters
                        SyncLock counter.BaseCounter
                            counter.BaseCounter.InstanceName = m_processName
                        End SyncLock
                    Next
                End SyncLock
            End Set
        End Property

        Public Property SamplingInterval() As Double
            Get
                Return m_samplingTimer.Interval
            End Get
            Set(ByVal value As Double)
                m_samplingTimer.Interval = value
            End Set
        End Property

        Public ReadOnly Property Counters() As List(Of PerformanceCounter)
            Get
                Return m_counters
            End Get
        End Property

        Public ReadOnly Property Counters(ByVal counterName As String) As PerformanceCounter
            Get
                Dim match As PerformanceCounter = Nothing
                SyncLock m_counters
                    For Each counter As PerformanceCounter In m_counters
                        SyncLock counter.BaseCounter
                            If String.Compare(counter.BaseCounter.CounterName, counterName, True) = 0 Then
                                match = counter
                                Exit For
                            End If
                        End SyncLock
                    Next
                End SyncLock
                Return match
            End Get
        End Property

        ''' <summary>
        ''' Gets the counter that monitors the processor utilization of the process.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The TVA.Diagnostics.PerformanceCounter instance that monitors the processor utilization of the process.
        ''' </returns>
        Public ReadOnly Property CPUUsage() As PerformanceCounter
            Get
                Return Counters("% Processor Time")
            End Get
        End Property

        ''' <summary>
        ''' Gets the counter that monitors the IP based datagrams sent / second of the system.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The TVA.Diagnostics.PerformanceCounter instance that monitors the IP based datagrams sent / second of the system.
        ''' </returns>
        Public ReadOnly Property DatagramSendRate() As PerformanceCounter
            Get
                Return Counters("Datagrams Sent/sec")
            End Get
        End Property

        ''' <summary>
        ''' Gets the counter that monitors the IP based datagrams received / second of the system.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The TVA.Diagnostics.PerformanceCounter instance that monitors the IP based datagrams received / second of the system.
        ''' </returns>
        Public ReadOnly Property DatagramReceiveRate() As PerformanceCounter
            Get
                Return Counters("Datagrams Received/sec")
            End Get
        End Property

        ''' <summary>
        ''' Gets the counter that monitors the .NET threading contention rate / second of the process.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The TVA.Diagnostics.PerformanceCounter instance that monitors the .NET threading contention rate / second of the process.
        ''' </returns>
        Public ReadOnly Property ThreadingContentionRate() As PerformanceCounter
            Get
                Return Counters("Contention Rate / sec")
            End Get
        End Property

        ''' <summary>
        ''' Gets the counter that monitors the memory utilization of the process.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The TVA.Diagnostics.PerformanceCounter instance that monitors the memory utilization of the process.
        ''' </returns>
        Public ReadOnly Property MemoryUsage() As PerformanceCounter
            Get
                Return Counters("Working Set")
            End Get
        End Property

        Public ReadOnly Property IOUsage() As PerformanceCounter
            Get
                Return Counters("IO Data Bytes/sec")
            End Get
        End Property

        Public ReadOnly Property IOActivity() As PerformanceCounter
            Get
                Return Counters("IO Data Operations/sec")
            End Get
        End Property

        Public ReadOnly Property HandleCount() As PerformanceCounter
            Get
                Return Counters("Handle Count")
            End Get
        End Property

        Public ReadOnly Property ThreadCount() As PerformanceCounter
            Get
                Return Counters("Thread Count")
            End Get
        End Property

        Public ReadOnly Property Status() As String
            Get
                Dim processorCount As Integer = System.Environment.ProcessorCount
                Dim counter As PerformanceCounter

                With New StringBuilder()
                    .Append("Counter".PadRight(20))
                    .Append(" "c)
                    .Append(CenterText("Last", 13))
                    .Append(" "c)
                    .Append(CenterText("Average", 13))
                    .Append(" "c)
                    .Append(CenterText("Maximum", 13))
                    .Append(" "c)
                    .Append(CenterText("Units", 16))
                    .AppendLine()
                    .Append(New String("-"c, 20))
                    .Append(" "c)
                    .Append(New String("-"c, 13))
                    .Append(" "c)
                    .Append(New String("-"c, 13))
                    .Append(" "c)
                    .Append(New String("-"c, 13))
                    .Append(" "c)
                    .Append(New String("-"c, 16))
                    .AppendLine()

                    counter = CPUUsage

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("CPU Utilization".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText((counter.LastValue / processorCount).ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText((counter.AverageValue / processorCount).ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText((counter.MaximumValue / processorCount).ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Percent / CPU")
                        .AppendLine()
                    End If

                    counter = MemoryUsage

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("Process Memory Usage".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText((counter.LastValue / 1048576).ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText((counter.AverageValue / 1048576).ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText((counter.MaximumValue / 1048576).ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Megabytes")
                        .AppendLine()
                    End If

                    counter = HandleCount

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("Process Handle Count".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText(counter.LastValue.ToString(), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.AverageValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.MaximumValue.ToString(), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Total Handles")
                        .AppendLine()
                    End If

                    counter = ThreadCount

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("Process Thread Count".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText(counter.LastValue.ToString(), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.AverageValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.MaximumValue.ToString(), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Total Threads")
                        .AppendLine()
                    End If

                    counter = IOUsage

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("I/O Data Rate".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText((counter.LastValue / 1024).ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText((counter.AverageValue / 1024).ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText((counter.MaximumValue / 1024).ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Kilobytes / sec")
                        .AppendLine()
                    End If

                    counter = IOActivity

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("I/O Activity Rate".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText(counter.LastValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.AverageValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.MaximumValue.ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Operations / sec")
                        .AppendLine()
                    End If

                    counter = DatagramReceiveRate

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("Incoming Packet Rate".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText(counter.LastValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.AverageValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.MaximumValue.ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Datagrams / sec")
                        .AppendLine()
                    End If

                    counter = DatagramSendRate

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("Outgoing Packet Rate".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText(counter.LastValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.AverageValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.MaximumValue.ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Datagrams / sec")
                        .AppendLine()
                    End If

                    counter = ThreadingContentionRate

                    If counter IsNot Nothing Then
                        '        12345678901234567890
                        .Append("Lock Contention Rate".PadLeft(20))
                        .Append(" "c)
                        .Append(CenterText(counter.LastValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.AverageValue.ToString("0.00"), 13))
                        .Append(" "c)
                        .Append(CenterText(counter.MaximumValue.ToString("0.00"), 13))
                        .Append(" "c)
                        '        1234567890123456
                        .Append("Attempts / sec")
                        .AppendLine()
                    End If

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

#Region " Code Scope: Private "

        Private Sub m_samplingTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_samplingTimer.Elapsed

            SyncLock m_counters
                For Each counter As PerformanceCounter In m_counters
                    counter.Sample()
                Next
            End SyncLock

        End Sub

#End Region

        Private disposedValue As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    m_samplingTimer.Dispose()
                    SyncLock m_counters
                        For Each counter As PerformanceCounter In m_counters
                            counter.Dispose()
                        Next
                        m_counters.Clear()
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