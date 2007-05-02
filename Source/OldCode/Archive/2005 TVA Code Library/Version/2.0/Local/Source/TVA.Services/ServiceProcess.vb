' 09-13-06

Imports System.Text
Imports System.Threading
Imports TVA.DateTime.Common

Public Class ServiceProcess

    Private m_processThread As Thread
    Private m_name As String
    Private m_parameters As Object()
    Private m_executionMethod As ExecutionMethodSignature
    Private m_serviceHelper As ServiceHelper
    Private m_currentState As ProcessState
    Private m_executionStartTime As System.DateTime
    Private m_executionStopTime As System.DateTime

    Public Delegate Sub ExecutionMethodSignature(ByVal name As String, ByVal parameters As Object())

    Public Sub New(ByVal executionMethod As ExecutionMethodSignature, ByVal name As String, _
            ByVal serviceHelper As ServiceHelper)
        MyClass.New(executionMethod, name, Nothing, serviceHelper)
    End Sub

    Public Sub New(ByVal executionMethod As ExecutionMethodSignature, ByVal name As String, _
            ByVal parameters As Object(), ByVal serviceHelper As ServiceHelper)
        m_name = name
        m_parameters = parameters
        m_executionMethod = executionMethod
        m_serviceHelper = serviceHelper
        m_currentState = ProcessState.Unprocessed
    End Sub

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_name = value
            Else
                Throw New ArgumentException("Name cannot be null.")
            End If
        End Set
    End Property

    Public Property Parameters() As Object()
        Get
            Return m_parameters
        End Get
        Set(ByVal value As Object())
            m_parameters = value
        End Set
    End Property

    Public Property ExecutionMethod() As ExecutionMethodSignature
        Get
            Return m_executionMethod
        End Get
        Set(ByVal value As ExecutionMethodSignature)
            m_executionMethod = value
        End Set
    End Property

    Public Property ServiceHelper() As ServiceHelper
        Get
            Return m_serviceHelper
        End Get
        Set(ByVal value As ServiceHelper)
            m_serviceHelper = value
        End Set
    End Property

    Public Property CurrentState() As ProcessState
        Get
            Return m_currentState
        End Get
        Private Set(ByVal value As ProcessState)
            m_currentState = value
            m_serviceHelper.ProcessStateChanged(m_name, m_currentState)
        End Set
    End Property

    Public ReadOnly Property ExecutionStartTime() As System.DateTime
        Get
            Return m_executionStartTime
        End Get
    End Property

    Public ReadOnly Property ExecutionStopTime() As System.DateTime
        Get
            Return m_executionStopTime
        End Get
    End Property

    Public ReadOnly Property LastExecutionTime() As Double
        Get
            Return TicksToSeconds(m_executionStopTime.Ticks - m_executionStartTime.Ticks)
        End Get
    End Property

    Public ReadOnly Property Status() As String
        Get
            With New StringBuilder()
                .Append("              Process Name: ")
                .Append(m_name)
                .Append(Environment.NewLine)
                .Append("             Current State: ")
                .Append(m_currentState.ToString())
                .Append(Environment.NewLine)
                .Append("      Execution Start Time: ")
                If m_executionStartTime <> System.DateTime.MinValue Then
                    .Append(m_executionStartTime.ToString())
                Else
                    .Append("N/A")
                End If
                .Append(Environment.NewLine)
                .Append("       Execution Stop Time: ")
                If m_executionStopTime <> System.DateTime.MinValue Then
                    .Append(m_executionStopTime.ToString())
                Else
                    .Append("N/A")
                End If
                .Append(Environment.NewLine)
                .Append("       Last Execution Time: ")
                .Append(SecondsToText(Me.LastExecutionTime))
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Public Sub Start()

        ' Start the execution on a seperate thread.
        m_processThread = New Thread(AddressOf InvokeExecutionMethod)
        m_processThread.Start()

    End Sub

    Public Sub Abort()

        If m_processThread IsNot Nothing Then
            ' We'll abort the process only if it is currently executing.
            m_processThread.Abort()
        End If

    End Sub

    Private Sub InvokeExecutionMethod()

        If m_executionMethod IsNot Nothing Then
            CurrentState = ProcessState.Processing
            m_executionStartTime = System.DateTime.Now
            m_executionStopTime = System.DateTime.MinValue
            Try
                ' We'll keep the invokation of the delegate in Try...Catch to absorb any exceptions that
                ' were not handled by the consumer.
                m_executionMethod(m_name, m_parameters)
                Me.CurrentState = ProcessState.Processed
            Catch ex As ThreadAbortException
                Me.CurrentState = ProcessState.Aborted
            Catch ex As Exception
                ' We'll absorb any exceptions if unhandled by the client.
                Me.CurrentState = ProcessState.Exception
            Finally
                m_executionStopTime = System.DateTime.Now
            End Try
        End If
        m_processThread = Nothing

    End Sub

End Class
