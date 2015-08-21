Imports System.IO
Imports System.drawing
Imports System.Data.OleDb
Imports System.ComponentModel
Imports TVA.Shared.FilePath

Namespace Utilities.Text

    <ToolboxBitmap(GetType(CSVImporter), "CSVImporter.bmp"), Description("Imports a standard CSV file into a datatable.")> _
    Public Class CSVImporter
        Inherits Component

#Region " Component Designer generated code "

        Public Sub New(ByVal Container As System.ComponentModel.IContainer)
            MyClass.New()

            'Required for Windows.Forms Class Composition Designer support
            Container.Add(Me)
        End Sub

        Public Sub New()
            MyBase.New()

            'This call is required by the Component Designer.
            InitializeComponent()

            'Add any initialization after the InitializeComponent() call

        End Sub

        'Component overrides dispose to clean up the component list.
        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not (components Is Nothing) Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Component Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Component Designer
        'It can be modified using the Component Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
            components = New System.ComponentModel.Container
        End Sub

#End Region

        Private _DataStartLine As Integer
        Private _ColumnHeaders As DataColumnCollection
        Private _HeaderIndex As Integer = 1
        Private _HasHeaders As Boolean
        Private _FilePath As String
        Private _FileName As String
        Private _CSVTable As New DataTable
        Private iFile As String
        Private strAr() As String
        Private str As String
        Private sr As IO.StreamReader
        Private ColumnCount As Integer

        <Description("Does the CSV File contain a row that is used as headers?")> _
        Public Property HasHeaders() As Boolean
            Get
                Return _HasHeaders
            End Get
            Set(ByVal Value As Boolean)
                If Value = True Then
                    If ColumnHeaders.Count > 0 Then
                        Select Case MsgBox("Do you wish to clear out you current headers?", MsgBoxStyle.YesNo)
                            Case MsgBoxResult.Yes
                                ColumnHeaders.Clear()
                                _HasHeaders = Value
                            Case MsgBoxResult.No
                        End Select
                    Else
                        _HasHeaders = Value
                    End If
                End If
            End Set
        End Property

        <Description("The index location of the row that the headers reside. This is a 1 based index")> _
        Public Property HeaderIndex() As Integer
            Get
                Return _HeaderIndex
            End Get
            Set(ByVal Value As Integer)
                _HeaderIndex = Value
            End Set
        End Property

        <Description("Represents a collection of System.Data.DataColumn objects for a System.Data.DataTable.")> _
        Public Property ColumnHeaders() As DataColumnCollection
            Get
                Return CSVTable.Columns
            End Get
            Set(ByVal Value As DataColumnCollection)
                CSVTable.Columns.Clear()
                ColumnCount = Value.Count
                For i As Integer = 0 To ColumnCount
                    CSVTable.Columns.Add(Value.Item(i))
                Next
                If Value.Count > 0 Then
                    HasHeaders = False
                End If
            End Set
        End Property

        <Description("Path Location of the file."), RefreshProperties(RefreshProperties.All)> _
        Public Property FilePath() As String
            Get
                Return _FilePath
            End Get
            Set(ByVal Value As String)
                'Check to see if the included the name of the file as part of the path
                Dim name As String

                If Len(JustFileName(Value)) > 0 Then
                    FileName = JustFileName(Value)
                End If

                _FilePath = JustPath(Value)

            End Set
        End Property

        <Description("Name of the file.")> _
        Public Property FileName() As String
            Get
                Return _FileName
            End Get
            Set(ByVal Value As String)
                _FileName = Value
            End Set
        End Property

        <Description("The index location of the first row of data. This is a 1 based index")> _
        Public Property DataStartLine() As Integer
            Get
                Return _DataStartLine
            End Get
            Set(ByVal Value As Integer)
                _DataStartLine = Value
            End Set
        End Property

        Private Property CSVTable() As DataTable
            Get
                Return _CSVTable
            End Get
            Set(ByVal Value As DataTable)
                _CSVTable = Value
            End Set
        End Property

        Public Function ReadCSV() As DataTable
            If HasHeaders Then
                'Get the Headers
                GetHeaderName()
            End If

            FillRows()

            Return CSVTable

        End Function

        Private Sub GetHeaderName()
            Dim Rcount As Integer
            iFile = FilePath & FileName
            If File.Exists(iFile) = True Then
                sr = IO.File.OpenText(iFile)
                Do While sr.Peek <> -1
                    str = sr.ReadLine
                    strAr = str.Split(",")
                    Rcount += 1
                    If Rcount = HeaderIndex Then
                        Dim i As Integer
                        For i = 0 To strAr.Length - 1
                            CSVTable.Columns.Add(strAr(i))
                        Next
                        ColumnCount = strAr.Length
                        Exit Sub
                    End If
                Loop
            End If
        End Sub

        Private Sub FillRows()
            Array.Clear(strAr, 0, strAr.Length)
            str = ""
            Dim LineCount As Integer
            Dim cropstr(ColumnCount - 1) As String

            If File.Exists(iFile) = True Then
                sr = IO.File.OpenText(iFile)
                Do While sr.Peek <> -1
                    Try
                        Array.Clear(cropstr, 0, cropstr.Length)
                    Catch ex As Exception

                    End Try


                    str = sr.ReadLine
                    strAr = str.Split(",")

                    LineCount += 1

                    If HasHeaders = False And LineCount >= DataStartLine Then
                        For i As Integer = 0 To ColumnCount - 1
                            cropstr.SetValue(strAr.GetValue(i), i)
                        Next
                        CSVTable.Rows.Add(cropstr)
                    ElseIf HeaderIndex <> LineCount And LineCount >= DataStartLine Then
                        For i As Integer = 0 To ColumnCount - 1
                            cropstr.SetValue(strAr.GetValue(i), i)
                        Next
                        CSVTable.Rows.Add(cropstr)
                    End If
                Loop
            End If
        End Sub
    End Class
End Namespace