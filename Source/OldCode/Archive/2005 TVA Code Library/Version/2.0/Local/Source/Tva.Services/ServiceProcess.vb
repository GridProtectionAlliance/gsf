' 09-13-06

Imports System.Text
Imports System.Threading
Imports Tva.DateTime.Common

Public Class ServiceProcess

    Private m_processThread As Thread
    Private m_name As String
    Private m_parameters As Object()
    Private m_executionMethod As ExecutionMethodSignature
    Private m_serviceHelper As ServiceHelper
    Private m_currentState As ProcessState
    Private m_lastExecutionTime As Double

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

    Public ReadOnly Property LastExecutionTime() As Double
        Get
            Return m_lastExecutionTime
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
                .Append("       Last Execution Time: ")
                .Append(SecondsToText(m_lastExecutionTime))
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Public Sub Start()

        m_processThread = New Thread(AddressOf InvokeExecutionMethod)
        m_processThread.Start()

    End Sub

    Public Sub Abort()

        If m_processThread IsNot Nothing Then m_processThread.Abort()
        Me.CurrentState = ProcessState.Aborted

    End Sub

    Private Sub InvokeExecutionMethod()

        If m_executionMethod IsNot Nothing Then
            CurrentState = ProcessState.Processing
            Dim startTime As Long = Date.Now.Ticks
            m_executionMethod.Invoke(m_name, m_parameters)
            m_lastExecutionTime = TicksToSeconds(Date.Now.Ticks - startTime)
            CurrentState = ProcessState.Processed
        End If
        m_processThread = Nothing

    End Sub

End Class
