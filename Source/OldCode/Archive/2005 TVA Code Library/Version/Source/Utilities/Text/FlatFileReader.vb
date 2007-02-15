Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Threading.Thread
Imports VB = Microsoft.VisualBasic

Namespace Utilities.Text

    Public Class FlatFileReader

        Private msFilePath As String = ""
        Private msFileName As String = ""
        Private mbSuccess As Boolean = True
        Private moStart As New Collection
        Private moLength As New Collection
        Private moFieldNames As New Collection
        Private mdCreateDate As Date
        Private miFieldCount As Integer = 0
        Private mbDelete As Boolean = False
        Private msErrormessage As String = ""
        Private mbCheckForEmptyField As Boolean = False
        Private miFieldToCheck As Integer = 0
        Private moProcessException As Exception
        Private mbRethrowExceptions As Boolean = False

        Public DataTable As New DataTable

        Public Property CheckForEmptyField() As Boolean
            Get
                Return mbCheckForEmptyField
            End Get
            Set(ByVal Value As Boolean)
                mbCheckForEmptyField = Value
            End Set
        End Property

        Public Property FieldToCheck() As Integer
            Get
                Return miFieldToCheck
            End Get
            Set(ByVal Value As Integer)
                miFieldToCheck = Value
            End Set
        End Property

        Public ReadOnly Property ErrorMessage() As String
            Get
                Return msErrormessage
            End Get
        End Property

        Public Property Delete() As Boolean
            Get
                Return mbDelete
            End Get
            Set(ByVal Value As Boolean)
                mbDelete = Value
            End Set
        End Property

        Public ReadOnly Property FieldCount() As Integer
            Get
                Return miFieldCount
            End Get
        End Property

        Public ReadOnly Property ProcessException() As Exception
            Get
                Return moProcessException
            End Get
        End Property

        Public Property RethrowExceptions() As Boolean
            Get
                Return mbRethrowExceptions
            End Get
            Set(ByVal Value As Boolean)
                mbRethrowExceptions = Value
            End Set
        End Property

        Public ReadOnly Property CreateDate() As Date
            Get
                Return mdCreateDate
            End Get
        End Property

        Public Property Success() As Boolean
            Get
                Return mbSuccess
            End Get
            Set(ByVal Value As Boolean)
                mbSuccess = Value
            End Set
        End Property

        Public Property FilePath() As String
            Get
                Return msFilePath
            End Get
            Set(ByVal Value As String)
                msFilePath = Value
            End Set
        End Property

        Public Property FileName() As String
            Get
                Return msFileName
            End Get
            Set(ByVal Value As String)
                msFileName = Value
            End Set
        End Property

        Public Function AddField(ByVal sFieldName As String, ByVal oFieldType As System.Type, ByVal iStart As Integer, ByVal iLength As Integer) As Boolean
            Dim bReturnValue As Boolean = True

            Try
                DataTable.Columns.Add(sFieldName, oFieldType)
                moFieldNames.Add(sFieldName, sFieldName)
                moStart.Add(iStart, sFieldName)
                moLength.Add(iLength, sFieldName)
                miFieldCount += 1
            Catch ex As Exception
                bReturnValue = False
            End Try
            Return bReturnValue
        End Function

        Public Function Process() As Boolean
            Dim oFileStream As System.IO.FileStream
            Dim oStreamReader As System.IO.StreamReader
            Try
                Dim lFileSizeBefore As Long = 0
                Dim lFileSizeAfter As Long = 0

                Dim oValues(miFieldCount - 1) As Object

                Dim iIterate As Integer

                Dim sLine As String
                Dim sFieldName As String
                Dim iStart As Integer
                Dim iLength As Integer
                Dim bWriteRecord As Boolean = False

                ' Reset any possible existing process exception
                moProcessException = Nothing

                lFileSizeBefore = FileLen(msFilePath & msFileName)

                Sleep(500)

                Do While FileLen(msFilePath & msFileName) <> lFileSizeBefore
                    Sleep(1000)
                    lFileSizeBefore = FileLen(msFilePath & msFileName)
                Loop

                mdCreateDate = CDate(Format(System.IO.File.GetCreationTime(msFilePath & msFileName), "MM/dd/yyyy HH:mm:ss"))

                oFileStream = New System.IO.FileStream(msFilePath & msFileName, IO.FileMode.Open)

                oStreamReader = New System.IO.StreamReader(oFileStream)

                Dim bLoop As Boolean = True
                Do While bLoop
                    Try
                        sLine = oStreamReader.ReadLine
                    Catch Ex As Exception
                        bLoop = False
                    End Try
                    bLoop = Not IsNothing(sLine)
                    If bLoop Then
                        ClearArray(oValues, miFieldCount - 1)
                        For iIterate = 1 To miFieldCount
                            sFieldName = moFieldNames(iIterate)
                            iStart = moStart(sFieldName)
                            iLength = moLength(sFieldName)
                            oValues(iIterate - 1) = Trim(VB.Mid(sLine, iStart, iLength))
                        Next
                        bWriteRecord = True
                        If mbCheckForEmptyField Then
                            If Trim(oValues(miFieldToCheck)) = "" Then
                                bWriteRecord = False
                            End If
                        End If
                        If bWriteRecord Then
                            DataTable.Rows.Add(oValues)
                        End If
                    End If
                Loop
                oStreamReader.Close()
                oFileStream.Close()
                If mbDelete Then
                    System.IO.File.Delete(msFilePath & msFileName)
                End If
            Catch ex As Exception
                msErrormessage = ex.Message
                mbSuccess = False
                'ReportError(ex, "Flat File Reader -- Process")
                ClearObject(oStreamReader)
                ClearObject(oFileStream)

                ' Hold on to process exception so user can check for error...
                moProcessException = ex

                ' Rethrow exeception if user requests this type of error handling...
                If mbRethrowExceptions Then Throw ex
            End Try

            ' Method changed to return a success flag if process succeeded - can still be called as a Sub if desired...
            Return moProcessException Is Nothing

        End Function

        Private Sub ClearArray(ByRef oValues() As Object, ByVal iLength As Integer)

            Dim iIterate As Integer

            For iIterate = 0 To iLength
                oValues(iIterate) = Nothing
            Next

        End Sub

        Private Sub ClearObject(ByRef oObject As Object)

            Try
                oObject.Dispose()
            Catch
            End Try

            oObject = Nothing

        End Sub

    End Class

End Namespace
