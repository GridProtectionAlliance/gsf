Option Explicit On 

Imports System.IO
Imports System.Text

Namespace Utilities.Text

    Public Class CSVPropertiesStructure

        Private msStringDelimeter As String = """"
        Private msDateDelimeter As String = ""
        Private msDelimeter As String = ","

        Public Property StringDelimeter() As String
            Get
                Return msStringDelimeter
            End Get
            Set(ByVal Value As String)
                msStringDelimeter = Value
            End Set
        End Property

        Public Property DateDelimeter() As String
            Get
                Return msDateDelimeter
            End Get
            Set(ByVal Value As String)
                msDateDelimeter = Value
            End Set

        End Property

        Public Property Delimeter() As String
            Get
                Return msDelimeter
            End Get
            Set(ByVal Value As String)
                msDelimeter = Value
            End Set
        End Property

    End Class

    Public Class ExportData

        Public Enum ExportType
            CSVExport = 1
        End Enum

        Private msEndOfLine As String = vbCrLf
        Private msAddHeaderLine As String = ""
        Private mbAddHeader As Boolean = False
        Private moCSVProperties As New CSVPropertiesStructure
        Private moTable As DataTable
        Private moType As ExportType = ExportType.CSVExport
        Private msFileName As String = ""
        Private mbDeleteBeforeCreate As Boolean = False
        Private mbSuccess As Boolean = False
        Private msDateFormat As String = "MM/dd/yyyy HH:mm:ss"
        Private mbHeaders As Boolean = True
        Private mbShowTrueFalse As Boolean = True
        Private msErrorMessage As String = ""
        Dim mbShowMilliseconds As Boolean = True
        Public Sub Process()

            Select Case moType
                Case ExportType.CSVExport
                    ExportCSV()
            End Select
        End Sub

        Private Sub ExportCSV()
            Dim oFileStream As System.IO.FileStream
            Dim oTextStream As System.IO.StreamWriter

            Dim oStringBuilder As StringBuilder = New StringBuilder
            Dim oStringBuilderLine As StringBuilder = New StringBuilder


            Try
                If mbDeleteBeforeCreate Then
                    If Dir(msFileName) <> "" Then
                        System.IO.File.Delete(msFileName)
                    End If
                End If
                oFileStream = New System.IO.FileStream(msFileName, FileMode.OpenOrCreate)
                oTextStream = New System.IO.StreamWriter(oFileStream)
                Dim oRow As DataRow
                Dim iColumns As Integer = moTable.Columns.Count
                Dim sColumnTypes(iColumns - 1) As String
                Dim iIterate As Integer
                Dim sLineOut As String
                Dim sFieldOut As String

                For iIterate = 0 To iColumns - 1
                    sColumnTypes(iIterate) = moTable.Columns(iIterate).DataType.FullName
                Next

                If mbAddHeader Then
                    oStringBuilder.Append(msAddHeaderLine)
                    oStringBuilder.Append(msEndOfLine)
                End If

                If mbHeaders Then
                    sLineOut = ""
                    For iIterate = 0 To iColumns - 1
                        sFieldOut = moTable.Columns(iIterate).ColumnName
                        If iIterate = 0 Then
                            sLineOut = sLineOut & sFieldOut
                        Else
                            sLineOut = sLineOut & moCSVProperties.Delimeter & sFieldOut
                        End If
                    Next
                    oStringBuilder.Append(sLineOut)
                    oStringBuilder.Append(msEndOfLine)
                End If

                For Each oRow In moTable.Rows
                    sLineOut = ""
                    For iIterate = 0 To iColumns - 1
                        If iIterate <> 0 Then
                            oStringBuilder.Append(moCSVProperties.Delimeter)
                        End If
                        If Not IsDBNull(oRow(iIterate)) Then
                            Select Case UCase(sColumnTypes(iIterate))
                                Case "SYSTEM.STRING"
                                    oStringBuilder.Append(moCSVProperties.StringDelimeter)
                                    oStringBuilder.Append(oRow(iIterate))
                                    oStringBuilder.Append(moCSVProperties.StringDelimeter)
                                Case "SYSTEM.DECIMAL", "SYSTEM.INT16", "SYSTEM.DOUBLE", "SYSTEM.INT32"
                                    oStringBuilder.Append(Trim(Str(oRow(iIterate))))
                                Case "SYSTEM.DATETIME"
                                    oStringBuilder.Append(moCSVProperties.DateDelimeter)
                                    oStringBuilder.Append(Format(oRow(iIterate), msDateFormat))
                                    oStringBuilder.Append(IIf(mbShowMilliseconds, "." & Format(CType(oRow(iIterate), DateTime).Millisecond, "000"), ""))
                                    oStringBuilder.Append(moCSVProperties.DateDelimeter)
                                Case "SYSTEM.BOOLEAN"
                                    If mbShowTrueFalse Then
                                        oStringBuilder.Append(IIf(oRow(iIterate), "TRUE", "FALSE"))
                                    Else
                                        oStringBuilder.Append(IIf(oRow(iIterate), "1", "0"))
                                    End If
                                Case Else
                                    oStringBuilder.Append("")
                            End Select
                        Else
                            oStringBuilder.Append("NULL")
                        End If
                    Next
                    oStringBuilder.Append(msEndOfLine)
                Next
                oTextStream.Write(oStringBuilder.ToString)
                oTextStream.Close()
                oFileStream.Close()
                oTextStream = Nothing
                oFileStream = Nothing
                mbSuccess = True
            Catch Ex As Exception
                oTextStream = Nothing
                oFileStream = Nothing
                mbSuccess = False
                msErrorMessage = Ex.Message
            End Try


        End Sub
        Public ReadOnly Property ErrorMessage() As String
            Get
                Return msErrorMessage
            End Get
        End Property
        Public Property AddHeader() As Boolean
            Get
                Return mbAddHeader
            End Get
            Set(ByVal Value As Boolean)
                mbAddHeader = Value
            End Set
        End Property
        Public Property AddHeaderLine() As String
            Get
                Return msAddHeaderLine
            End Get
            Set(ByVal Value As String)
                msAddHeaderLine = Value
            End Set
        End Property
        Public ReadOnly Property Success() As Boolean
            Get
                Return mbSuccess
            End Get
        End Property
        Public Property ExportFileName() As String
            Get
                Return msFileName
            End Get
            Set(ByVal Value As String)
                msFileName = Value
            End Set
        End Property
        Public Property DeleteBeforeCreate() As Boolean
            Get
                Return mbDeleteBeforeCreate
            End Get
            Set(ByVal Value As Boolean)
                mbDeleteBeforeCreate = Value
            End Set
        End Property
        Public Property FileExportType() As ExportType
            Get
                Return moType
            End Get
            Set(ByVal Value As ExportType)
                moType = Value
            End Set
        End Property
        Public Property Table() As DataTable
            Get
                Return moTable
            End Get
            Set(ByVal Value As DataTable)
                moTable = Value
            End Set
        End Property
        Public Property CSVProperties() As CSVPropertiesStructure
            Get
                Return moCSVProperties
            End Get
            Set(ByVal Value As CSVPropertiesStructure)
                moCSVProperties = Value
            End Set
        End Property
        Public Property DateFormat() As String
            Get
                Return msDateFormat
            End Get
            Set(ByVal Value As String)
                msDateFormat = Value
            End Set
        End Property
        Public Property EndOfLine() As String
            Get
                Return msEndOfLine
            End Get
            Set(ByVal Value As String)
                msEndOfLine = Value
            End Set
        End Property
        Public Property Headers() As Boolean
            Get
                Return mbHeaders
            End Get
            Set(ByVal Value As Boolean)
                mbHeaders = Value
            End Set
        End Property
        Public Property ShowMilliseconds() As Boolean
            Get
                Return mbShowMilliseconds
            End Get
            Set(ByVal Value As Boolean)
                mbShowMilliseconds = Value
            End Set
        End Property
        Public Property ShowTrueFalse() As Boolean
            Get
                Return mbShowTrueFalse
            End Get
            Set(ByVal Value As Boolean)
                mbShowTrueFalse = Value
            End Set
        End Property
    End Class

End Namespace