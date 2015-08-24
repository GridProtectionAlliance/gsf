Imports TVA.ESO.SSAM
Imports System.ComponentModel
Imports System.Drawing

Namespace ErrorManagement
    <ToolboxBitmap(GetType(SsamErrorListener), "SsamErrorListener.bmp"), DefaultProperty("SsamApi")> _
    Public Class SsamErrorListener

        Inherits System.ComponentModel.Component
        Implements IErrorListener

        Protected tEntityType As SsamEntityTypeID = SsamEntityTypeID.ssamFlowEntity
        Protected sShortName As String 'should be set externally
        Protected sErrorNumber As String
        Protected sErrorMessage As String
        Protected sDescription As String
        Protected ssamEvent As SsamEventTypeID
        Protected oSsamApi As SsamApi

        'Public Shared mSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB)
        'Public Shared inHandeler = False
        Protected bIsThreadAbort As Boolean

        Public Property SsamApi() As SsamApi
            Get
                Return oSsamApi
            End Get
            Set(ByVal Value As SsamApi)
                oSsamApi = Value
            End Set
        End Property

        Public Property EntityType() As SsamEntityTypeID
            Get
                Return tEntityType
            End Get
            Set(ByVal Value As SsamEntityTypeID)
                tEntityType = Value
            End Set
        End Property

        Public Property ShortName() As String
            Get
                Return sShortName
            End Get
            Set(ByVal Value As String)
                sShortName = Value
            End Set
        End Property

        Public Property ErrorNumber() As String
            Get
                Return sErrorNumber
            End Get
            Set(ByVal Value As String)
                sErrorNumber = Value
            End Set
        End Property

        Public Property ErrorMessage() As String
            Get
                Return sErrorMessage
            End Get
            Set(ByVal Value As String)
                sErrorMessage = Value
            End Set
        End Property

        Public Property Description() As String
            Get
                Return sDescription
            End Get
            Set(ByVal Value As String)
                sDescription = Value
            End Set
        End Property

        <Description("Specify flag if you like to abort thread where unhandled exception happened.")> _
        Public Property AbortThread() As Boolean
            Get
                If bIsThreadAbort.Equals(Nothing) Then
                    bIsThreadAbort = False
                End If
                Return bIsThreadAbort
            End Get
            Set(ByVal Value As Boolean)
                bIsThreadAbort = Value
            End Set
        End Property

        Public Sub New() '(ByVal objSsamApi As SsamApi)
            MyBase.new()
            ErrorHandler.AddListener(Me)
            'SsamApi = objSsamApi
        End Sub

        Public Sub PostError(ByVal oErrorManager As IErrorManager) Implements IErrorListener.PostError
            If FilterError(oErrorManager) Then
                OutputError(oErrorManager)
            End If
        End Sub

        Protected Overridable Sub OutputError(ByVal oErrorManager As IErrorManager)
            'While (inHandeler)
            '    Threading.Thread.Sleep(100)
            'End While
            'inHandeler = True

            ssamEvent = SsamEventTypeID.ssamInfoEvent
            Select Case oErrorManager.ErrorType    'map message types to SSAM event types
                Case ErrorType.tMessage
                    ssamEvent = SsamEventTypeID.ssamInfoEvent
                Case ErrorType.tWarning
                    ssamEvent = SsamEventTypeID.ssamInfoEvent
                Case ErrorType.tError
                    ssamEvent = SsamEventTypeID.ssamErrorEvent
                Case ErrorType.tSqlError
                    ssamEvent = SsamEventTypeID.ssamErrorEvent
                Case ErrorType.tUnexpectedError
                    ssamEvent = SsamEventTypeID.ssamErrorEvent
                Case ErrorType.tUnexpectedSqlError
                    ssamEvent = SsamEventTypeID.ssamErrorEvent
                Case Else
                    ssamEvent = SsamEventTypeID.ssamInfoEvent
            End Select

            ErrorNumber = oErrorManager.ErrorNumber
            If ErrorNumber Is Nothing Then
                ErrorNumber = "N/A"
            End If

            ErrorMessage = oErrorManager.ErrorMessage
            If ErrorMessage Is Nothing And Not oErrorManager.ErrorException Is Nothing Then
                ErrorMessage = oErrorManager.ErrorException.Message
            End If

            'Filtered to SQL errors description only
            'If Not oErrorManager.Sql Is Nothing Then
            '    Description = oErrorManager.Sql
            'Else
            '    Description = Nothing
            'End If
            'Just for now
            Description = oErrorManager.ErrorException.ToString()

            'Description = oErrorManager.ErrorException.ToString()
            'If Not oSsamApi Is Nothing And Len(ShortName) > 0 Then
            'oSsamApi.LogSsamEvent(ssamEvent, EntityType, ShortName, ErrorNumber, ErrorMessage, Dscription)
            '
            Dim iThID As Integer
            iThID = System.Threading.Thread.CurrentThread.GetHashCode()
            Dim v As Hashtable
            v = SsamProperties.HT
            Dim iEntType As SsamEntityTypeID, sFlowName As String

            'give them default values from manually typed properties of visual SssamErrorListener control
            iEntType = SsamProperties.SsamEntityType
            sFlowName = SsamProperties.ShortName
            Dim oFlow As SsamProperties.FlowInfo
            If v.ContainsKey(iThID) Then
                oFlow = v.Item(iThID)
                If Not oFlow.Equals(Nothing) Then
                    If (oFlow.iSsamEntityTypeID > 0) Then
                        iEntType = oFlow.iSsamEntityTypeID
                    End If

                    If (oFlow.FlowName > "") Then
                        sFlowName = oFlow.FlowName
                    End If
                End If
            End If
            Try
                oSsamApi.LogSsamEvent(ssamEvent, iEntType, sFlowName, ErrorNumber, ErrorMessage, Description)
                ' need do next line only for unhandled errors
                If oErrorManager.Unhandled Then
                    SsamProperties.ClearFlowName()
                    If bIsThreadAbort = True Then
                        System.Threading.Thread.CurrentThread.Abort()
                        'GC.Collect()
                    End If
                End If
            Catch
                ' check SsamApi.Success and do something if failed to log event
            Finally
                'inHandeler = False
            End Try

            'End If
        End Sub

        Public Function FilterError(ByVal oErrorManager As IErrorManager) As Boolean Implements IErrorListener.FilterError
            Return True    'SSAM listens to all kinds of errors and messages by default; override to filter
        End Function

        Public Sub Start() Implements IErrorListener.Start

        End Sub

        Public Sub Finish() Implements IErrorListener.Finish

        End Sub

        Private Sub InitializeComponent()

        End Sub
    End Class

    Public Class SsamProperties
        Protected Shared tSsamEntityType As SsamEntityTypeID
        Protected Shared sShortName As String
        Private Shared _HT As Hashtable

        Public Structure FlowInfo
            Public FlowName As String
            Public iSsamEntityTypeID As SsamEntityTypeID
        End Structure

        Protected Sub New()
            MyBase.new()
        End Sub

        Shared Property SsamEntityType() As SsamEntityTypeID
            Get
                Return SsamProperties.tSsamEntityType
            End Get
            Set(ByVal Value As SsamEntityTypeID)
                tSsamEntityType = Value
            End Set
        End Property

        Shared Property ShortName() As String
            Get
                Return SsamProperties.sShortName
            End Get
            Set(ByVal Value As String)
                sShortName = Value
            End Set
        End Property

        Shared Sub SetSsamPropertiesObject(ByVal tEntType As SsamEntityTypeID, ByVal sShortN As String)
            SsamEntityType = tEntType
            ShortName = sShortN
        End Sub

        Shared ReadOnly Property HT() As Hashtable
            Get
                If _HT Is Nothing Then
                    _HT = New Hashtable
                End If
                Return _HT
            End Get
        End Property

        Public Shared Sub SetFlowName(ByVal FlowName As String, ByVal iSsamEntityTypeID As SsamEntityTypeID) ', ByVal iThreadId As Integer)
            Dim _FlInf As FlowInfo
            Dim iThreadId As Integer
            _FlInf.FlowName = FlowName
            _FlInf.iSsamEntityTypeID = iSsamEntityTypeID
            Try
                'iThreadId = System.Threading.Thread.CurrentContext.ContextID()
                iThreadId = System.Threading.Thread.CurrentThread.GetHashCode()
                If HT.ContainsKey(iThreadId) Then
                    HT.Remove(iThreadId)
                End If
                HT.Add(iThreadId, _FlInf)
            Catch
                'do something
            End Try
        End Sub

        Public Shared Sub ClearFlowName()
            Dim iThreadId As Integer
            Try
                'iThreadId = System.Threading.Thread.CurrentContext.ContextID()
                iThreadId = System.Threading.Thread.CurrentThread.GetHashCode()
                If HT.ContainsKey(iThreadId) Then
                    HT.Remove(iThreadId)
                End If
            Catch
                'do something
                'was not able to get ThreadID perhaps
            End Try
        End Sub
    End Class
End Namespace
