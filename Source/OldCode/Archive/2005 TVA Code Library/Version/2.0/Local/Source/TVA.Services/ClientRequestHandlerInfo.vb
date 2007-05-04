' 05/02/2007

Public Class ClientRequestHandlerInfo

    Public Delegate Sub HandlerMethodSignature()

    Public Sub New(ByVal requestCommand As String, ByVal requestDescription As String, _
        ByVal handlerMethod As HandlerMethodSignature)

        MyClass.New(requestCommand, requestDescription, handlerMethod, True)

    End Sub

    Public Sub New(ByVal requestCommand As String, ByVal requestDescription As String, _
            ByVal handlerMethod As HandlerMethodSignature, ByVal isAdvertised As Boolean)

        MyBase.New()
        Me.Command = requestCommand
        Me.CommandDescription = requestDescription
        Me.HandlerMethod = handlerMethod
        Me.IsAdvertised = isAdvertised

    End Sub

    Public Command As String
    Public CommandDescription As String
    Public HandlerMethod As HandlerMethodSignature
    Public IsAdvertised As Boolean

End Class
