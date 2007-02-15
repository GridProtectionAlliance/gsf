' 06/21/2004 JRC - Bug fix - added code to check for null source and target sites from logged exceptions

Imports System
Imports System.Threading
Imports System.ComponentModel
Imports System.Windows.Forms.Application
Imports System.Web
Imports TVA.Shared.String

Namespace ErrorManagement

    Public Class ErrorHandler

        ' Error handler's implementation of the error management interface
        Private Class ErrorManager

            Implements IErrorManager
            Implements IEnumerable

            ' Improvement: represent error data in XML for conformity with info-bus
            Protected tErrorType As ErrorType
            Protected dtTimestamp As Date
            Protected sPointTag As String
            Protected oException As Exception
            Protected sErrorNumber As String
            Protected sErrorMessage As String
            Protected sSql As String
            Protected oDbCommand As IDbCommand
            Protected fUnhandled As Boolean
            Protected oListeners As New ArrayList
            Protected bInitialized As Boolean = False

            Public Property Unhandled() As Boolean Implements IErrorManager.Unhandled
                Get
                    Return fUnhandled
                End Get
                Set(ByVal Value As Boolean)
                    fUnhandled = Value
                End Set
            End Property

            Property ErrorType() As ErrorType Implements IErrorManager.ErrorType
                Get
                    Return tErrorType
                End Get
                Set(ByVal Value As ErrorType)
                    tErrorType = Value
                End Set
            End Property

            Property Timestamp() As Date Implements IErrorManager.Timestamp
                Get
                    Return dtTimestamp
                End Get
                Set(ByVal Value As Date)
                    dtTimestamp = Value
                End Set
            End Property

            Property PointTag() As String Implements IErrorManager.PointTag
                Get
                    Return sPointTag
                End Get
                Set(ByVal Value As String)
                    sPointTag = Value
                End Set
            End Property

            Property ErrorException() As Exception Implements IErrorManager.ErrorException
                Get
                    Return oException
                End Get
                Set(ByVal Value As Exception)
                    oException = Value
                End Set
            End Property

            Property ErrorNumber() As String Implements IErrorManager.ErrorNumber
                Get
                    Return sErrorNumber
                End Get
                Set(ByVal Value As String)
                    sErrorNumber = Value
                End Set
            End Property

            Property ErrorMessage() As String Implements IErrorManager.ErrorMessage
                Get
                    Return sErrorMessage
                End Get
                Set(ByVal Value As String)
                    sErrorMessage = Value
                End Set
            End Property

            Property Sql() As String Implements IErrorManager.Sql
                Get
                    Return sSql
                End Get
                Set(ByVal Value As String)
                    sSql = Value
                End Set
            End Property

            Property DbCommand() As IDbCommand Implements IErrorManager.DbCommand
                Get
                    Return oDbCommand
                End Get
                Set(ByVal Value As IDbCommand)
                    oDbCommand = Value
                End Set
            End Property

            Public Sub AddListener(ByVal oErrorListener As IErrorListener)
                oListeners.Add(oErrorListener)
            End Sub

            Public Sub RemoveListener(ByVal oErrorListener As IErrorListener)
                oListeners.Remove(oErrorListener)
            End Sub

            Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return oListeners.GetEnumerator()
            End Function

            Protected Overridable Sub Start()
                If Not bInitialized Then    'init listeners first time
                    InitListeners()
                End If
                InitState()
            End Sub

            Protected Overridable Sub InitState()
                tErrorType = ErrorType.tUnexpectedError
                dtTimestamp = Now()
                sPointTag = Nothing
                oException = Nothing
                sErrorNumber = Nothing
                sErrorMessage = Nothing
                sSql = Nothing
                oDbCommand = Nothing
            End Sub

            Protected Overridable Sub InitListeners()
                Dim oListener As IErrorListener
                For Each oListener In oListeners
                    oListener.Start()
                Next
                bInitialized = True
            End Sub

            Public Overridable Sub Finish()
                EndListeners()
                InitState()    'release any object references remaining
            End Sub

            Protected Overridable Sub EndListeners()
                Dim oListener As IErrorListener
                For Each oListener In oListeners
                    oListener.Finish()
                Next
                bInitialized = False
            End Sub

            'broadcast PostError message to listener agents
            Protected Overridable Sub PublishError()
                Dim oListener As IErrorListener
                For Each oListener In oListeners
                    oListener.PostError(Me)
                Next
            End Sub

            Public Overridable Sub LogError(ByVal ex As System.Exception, Optional ByVal msg As String = Nothing) Implements IErrorManager.LogError

                Start()
                tErrorType = ErrorType.tError
                sPointTag = NotEmpty(ex.Source, "<unknown source>") & "."
                If ex.TargetSite Is Nothing Then
                    sPointTag &= "<unknown target site>"
                Else
                    sPointTag &= NotEmpty(ex.TargetSite.Name, "<unknown target site>")
                End If
                oException = ex
                sErrorMessage = msg
                PublishError()

            End Sub

            Public Overridable Sub LogError(ByVal num As String, ByVal msg As String, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogError
                Start()
                tErrorType = ErrorType.tError
                sPointTag = pointTag
                sErrorNumber = num
                sErrorMessage = msg
                PublishError()
            End Sub

            Public Overridable Sub LogMessage(ByVal msg As String, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogMessage
                Start()
                tErrorType = ErrorType.tMessage
                sPointTag = pointTag
                sErrorMessage = msg
                PublishError()
            End Sub

            Public Overridable Sub LogSqlError(ByVal ex As System.Exception, ByVal oDbCommand As IDbCommand, Optional ByVal msg As String = Nothing) Implements IErrorManager.LogSqlError
                Start()
                tErrorType = ErrorType.tSqlError
                sPointTag = NotEmpty(ex.Source, "<unknown source>") & "."
                If ex.TargetSite Is Nothing Then
                    sPointTag &= "<unknown target site>"
                Else
                    sPointTag &= NotEmpty(ex.TargetSite.Name, "<unknown target site>")
                End If
                oException = ex
                sErrorMessage = msg
                sSql = oDbCommand.CommandText
                Me.oDbCommand = oDbCommand
                PublishError()
            End Sub

            Public Overridable Sub LogSqlError(ByVal msg As String, ByVal oDbCommand As System.Data.IDbCommand, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogSqlError
                Start()
                tErrorType = ErrorType.tSqlError
                sPointTag = pointTag
                sErrorMessage = msg
                sSql = oDbCommand.CommandText
                Me.oDbCommand = oDbCommand
                PublishError()
            End Sub

            Public Overridable Sub LogSqlError(ByVal msg As String, Optional ByVal sql As String = Nothing, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogSqlError
                Start()
                tErrorType = ErrorType.tSqlError
                sPointTag = pointTag
                sErrorMessage = msg
                sSql = sql
                PublishError()
            End Sub

            Public Overridable Sub LogUnexpectedSqlError(ByVal ex As System.Exception, ByVal oDbCommand As IDbCommand, Optional ByVal msg As String = Nothing) Implements IErrorManager.LogUnexpectedSqlError
                Start()
                tErrorType = ErrorType.tUnexpectedSqlError
                sPointTag = NotEmpty(ex.Source, "<unknown source>") & "."
                If ex.TargetSite Is Nothing Then
                    sPointTag &= "<unknown target site>"
                Else
                    sPointTag &= NotEmpty(ex.TargetSite.Name, "<unknown target site>")
                End If
                oException = ex
                sErrorMessage = msg
                sSql = oDbCommand.CommandText
                Me.oDbCommand = oDbCommand
                PublishError()
            End Sub

            Public Overridable Sub LogUnexpectedSqlError(ByVal msg As String, ByVal oDbCommand As System.Data.IDbCommand, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogUnexpectedSqlError
                Start()
                tErrorType = ErrorType.tUnexpectedSqlError
                sPointTag = pointTag
                sErrorMessage = msg
                sSql = oDbCommand.CommandText
                Me.oDbCommand = oDbCommand
                PublishError()
            End Sub

            Public Overridable Sub LogUnexpectedSqlError(ByVal msg As String, Optional ByVal sql As String = Nothing, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogUnexpectedSqlError
                Start()
                tErrorType = ErrorType.tUnexpectedSqlError
                sPointTag = pointTag
                sErrorMessage = msg
                sSql = sql
                PublishError()
            End Sub

            Public Overridable Sub LogUnexpectedError(ByVal num As String, ByVal msg As String, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogUnexpectedError
                Start()
                tErrorType = ErrorType.tUnexpectedError
                sPointTag = pointTag
                sErrorNumber = num
                sErrorMessage = msg
                PublishError()
            End Sub

            Public Overridable Sub LogUnexpectedError(ByVal ex As System.Exception, Optional ByVal msg As String = Nothing) Implements IErrorManager.LogUnexpectedError
                Start()
                tErrorType = ErrorType.tUnexpectedError
                sPointTag = NotEmpty(ex.Source, "<unknown source>") & "."
                If ex.TargetSite Is Nothing Then
                    sPointTag &= "<unknown target site>"
                Else
                    sPointTag &= NotEmpty(ex.TargetSite.Name, "<unknown target site>")
                End If
                oException = ex
                sErrorMessage = msg
                PublishError()
            End Sub

            Public Overridable Sub LogWarning(ByVal msg As String, Optional ByVal pointTag As String = Nothing) Implements IErrorManager.LogWarning
                Start()
                tErrorType = ErrorType.tWarning
                sPointTag = pointTag
                sErrorMessage = msg
                PublishError()
            End Sub

            Protected Overrides Sub Finalize()
                If bInitialized Then
                    Finish()
                End If
                MyBase.Finalize()
            End Sub

        End Class

        Private Shared ErrManager As New ErrorManager
        Private Shared IsInitialized As Boolean = False

        Public Shared Sub AddListener(ByRef Listener As IErrorListener)

            If Not IsInitialized Then Initialize()
            ErrManager.AddListener(Listener)

        End Sub

        Public Shared Sub RemoveListener(ByRef Listener As IErrorListener)

            ErrManager.RemoveListener(Listener)

        End Sub

        Public Shared ReadOnly Property Initialized() As Boolean
            Get
                Return IsInitialized
            End Get
        End Property

        Public Shared Sub LogError(ByVal ex As Exception, ByVal IsUnhandled As Boolean, Optional ByVal Message As String = Nothing)

            SyncLock GetType(ErrorHandler)
                ErrManager.Unhandled = IsUnhandled
                ErrManager.LogError(ex, Message)
            End SyncLock

        End Sub

        Public Shared Sub Initialize()

            IsInitialized = True

            AddHandler System.Windows.Forms.Application.ThreadException, AddressOf ErrorHandler.HandleUnhandled
            AddHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf ErrorHandler.HandleUnhandled
            'AddHandler Thread.GetDomain().UnhandledException, AddressOf ErrorHandler.HandleUnhandled

        End Sub

        Public Shared Sub MethodHandler(ByVal ex As Exception, Optional ByVal sender As Object = Nothing)

            Try
                SyncLock GetType(ErrorHandler)
                    LogError(ex, False, "Method Exception")
                End SyncLock
            Catch
            End Try

        End Sub

        Public Shared Sub HandleUnhandled(ByVal sender As Object, ByVal args As UnhandledExceptionEventArgs)

            Try
                SyncLock GetType(ErrorHandler)
                    LogError(args.ExceptionObject, True, "Unhandled Exception")
                End SyncLock
            Catch
                'log error somewhere
            End Try

        End Sub

        Public Shared Sub HandleUnhandled(ByVal sender As Object, ByVal args As ThreadExceptionEventArgs)

            Try
                SyncLock GetType(ErrorHandler)
                    LogError(args.Exception, True, "Thread Exception")
                End SyncLock
            Catch
                'log error somewhere
            End Try

        End Sub

    End Class

End Namespace