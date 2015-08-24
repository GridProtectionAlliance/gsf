Imports System.IO
Imports System.ComponentModel
Imports System.Drawing

Namespace ErrorManagement

    <ToolboxBitmap(GetType(TextErrorListener), "TextErrorListener.bmp"), DefaultProperty("FileName")> _
    Public Class TextErrorListener
        Inherits System.ComponentModel.Component
        Implements IErrorListener

        Protected sFileName As String

        Protected oFileStream As FileStream
        Protected oFileStreamWriter As StreamWriter
        Protected bErrorReceived As Boolean

        Public Sub New() 'ByVal fName As String)
            MyBase.New()
            'FileName = fName
            ErrorHandler.AddListener(Me)
        End Sub

        <Description("Specify log file name. Otherwise file name is generated for you.")> _
        Public Property FileName() As String
            Get
                Return sFileName
            End Get
            Set(ByVal Value As String)
                sFileName = Value
            End Set
        End Property

        Public Sub PostError(ByVal oErrorManager As IErrorManager) Implements IErrorListener.PostError
            If FilterError(oErrorManager) Then
                OutputError(oErrorManager)
            End If
        End Sub

        'generate a unique file name using the text error listener's hash code and the current time in ticks
        Protected Overridable Sub GenerateLogFileName()
            FileName = "Log" & CStr(Me.GetHashCode()) & "-" & CStr(Now().Ticks()) & ".txt"
        End Sub

        Protected Overridable Sub OutputError(ByVal oErrorManager As IErrorManager)
            bErrorReceived = False
            'if necessary, generate new log-file name
            If sFileName Is Nothing Then
                GenerateLogFileName()
            ElseIf Len(sFileName) <= 0 Then
                GenerateLogFileName()
            End If
            'create stream and open file
            oFileStream = New FileStream(sFileName, FileMode.OpenOrCreate, FileAccess.Write)
            'create stream writer
            oFileStreamWriter = New StreamWriter(oFileStream)
            'seek to the end for append
            oFileStreamWriter.BaseStream.Seek(0, SeekOrigin.End)
            'write error in simple-text format
            If oErrorManager.ErrorException Is Nothing Then
                oFileStreamWriter.WriteLine("{0:yyyy-MM-dd HH:mm:ss}@{1} #{2} - {3}", oErrorManager.Timestamp, _
                   oErrorManager.PointTag, oErrorManager.ErrorNumber, oErrorManager.ErrorMessage)
            Else    'got exception
                oFileStreamWriter.WriteLine("{0:yyyy-MM-dd HH:mm:ss}@{1} #{2} - {3}", oErrorManager.Timestamp, _
                   oErrorManager.PointTag, "N/A", oErrorManager.ErrorException.Message)
            End If
            If oErrorManager.ErrorType = ErrorType.tSqlError Or oErrorManager.ErrorType = ErrorType.tUnexpectedSqlError Then
                oFileStreamWriter.WriteLine("{0}{1}SQL: {2}", vbCrLf, vbTab, oErrorManager.Sql)
            End If
            oFileStreamWriter.Flush()    'update underlying file
            oFileStreamWriter.Close()    'close stream
            oFileStream.Close()  'close file
            bErrorReceived = True
        End Sub

        Public Function FilterError(ByVal oErrorManager As IErrorManager) As Boolean Implements IErrorListener.FilterError
            Return True
        End Function

        Public Sub Start() Implements TVA.ErrorManagement.IErrorListener.Start
        End Sub

        Public Sub Finish() Implements TVA.ErrorManagement.IErrorListener.Finish
        End Sub

        Public Property WasErrorReceived() As Boolean
            Get
                Return bErrorReceived
            End Get
            Set(ByVal Value As Boolean)
                bErrorReceived = Value
            End Set
        End Property

    End Class

End Namespace