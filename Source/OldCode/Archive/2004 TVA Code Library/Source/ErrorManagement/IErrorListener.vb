Namespace ErrorManagement

    Public Interface IErrorListener

        Sub Start()
        Function FilterError(ByVal ErrorManager As IErrorManager) As Boolean
        Sub PostError(ByVal ErrorManager As IErrorManager)
        Sub Finish()

    End Interface

End Namespace

