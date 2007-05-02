' 05/02/2007

Public Class ClientRequestHandlerInfo

    Public Delegate Sub HandlerMethodSignature()

    Public Sub New(ByVal requestType As String, ByVal requestDescription As String, ByVal handlerMethod As HandlerMethodSignature)

        MyBase.New()
        Me.RequestType = requestType
        Me.RequestDescription = requestDescription
        Me.HandlerMethod = handlerMethod

    End Sub

    Public RequestType As String
    Public RequestDescription As String
    Public HandlerMethod As HandlerMethodSignature

End Class
