' 09-13-06

Imports System.Threading

Public Class ServiceProcess

    Private m_processThread As Thread
    Private m_name As String
    Private m_parameters As Object()
    Private m_executionMethod As ExecutionMethodSignature
    Private m_serviceHelper As ServiceHelper

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

    Public Sub StartProcess()

        m_processThread = New Thread(AddressOf InvokeProcessExecutionMethod)
        m_processThread.Start()

    End Sub

    Public Sub AbortProcess()

        If m_processThread IsNot Nothing Then m_processThread.Abort()
        m_serviceHelper.ProcessStateChanged(m_name, ProcessState.Aborted)

    End Sub

    Private Sub InvokeProcessExecutionMethod()

        If m_executionMethod IsNot Nothing Then
            m_serviceHelper.ProcessStateChanged(m_name, ProcessState.Processing)
            m_executionMethod.Invoke(m_name, m_parameters)
            m_serviceHelper.ProcessStateChanged(m_name, ProcessState.Processed)
        End If
        m_processThread = Nothing

    End Sub

End Class
