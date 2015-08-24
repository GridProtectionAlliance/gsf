' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Console

Module UpdateTimeStamp

    Private ProcessedFiles As Integer
    Private WorkImageIndex As Integer
    Private WorkImages As Char() = {"\"c, "|"c, "/"c, "-"c}
    Private LastImageUpdate As Double

    Private Class CommandLineOptions

        Public FilePaths As String()
        Public FileSpecs As String()
        Public Timestamp As DateTime
        Public RecurseDirectories As Boolean
        Public ResumeOnError As Boolean
        Public Verbose As Boolean

        Public Sub New(ByVal Args As Arguments)

            Dim FilePath As String
            Dim ValidFilePaths As New ArrayList
            Dim ValidFileSpecs As New ArrayList

            ' Initialize options
            RecurseDirectories = Args.Exists("r")
            ResumeOnError = Args.Exists("c")
            Verbose = Args.Exists("v")

            ' Parse timestamp
            Try
                Timestamp = CDate(Args("OrderedArg2"))
            Catch ex As Exception
                Console.WriteLine("ERROR: Bad timestamp: " & ex.Message)
                End
            End Try

            ' Validate each file spec
            For Each FileSpec As String In Args("OrderedArg1").Split(";"c)
                FileSpec = Trim(FileSpec)

                If Len(FileSpec) > 0 Then
                    If Len(JustFileName(FileSpec)) = Len(FileSpec) Then
                        ' No path given, assume current directory
                        FilePath = AddPathSuffix(Directory.GetCurrentDirectory)
                    Else
                        ' Path specified, so use it...
                        FilePath = Trim(JustPath(FileSpec))
                    End If

                    If Not Directory.Exists(FilePath) Then
                        Console.WriteLine("ERROR: Path does not exist: """ & FilePath & """")
                        End
                    End If

                    ValidFilePaths.Add(FilePath)
                    ValidFileSpecs.Add(JustFileName(FileSpec))
                End If
            Next

            FilePaths = Array.CreateInstance(GetType(String), ValidFilePaths.Count)
            ValidFilePaths.CopyTo(FilePaths)

            FileSpecs = Array.CreateInstance(GetType(String), ValidFileSpecs.Count)
            ValidFileSpecs.CopyTo(FileSpecs)

        End Sub

        Public ReadOnly Property This() As CommandLineOptions
            Get
                Return Me
            End Get
        End Property

    End Class

    Sub Main()

        Dim Args As New Arguments(Command)
        Dim StartTime As Double = Timer

        Console.WriteLine()
        Console.WriteLine("Update Timestamp for files utility - JRC 2004")
        Console.WriteLine()

        If Len(Args("OrderedArg1")) = 0 OrElse Args.OrderedArgCount < 2 OrElse Args.ContainHelpRequest Then
            Console.WriteLine("Usage:")
            Console.WriteLine("    UpdateTimeStamp -options FileSpecs ""TimeStamp""")
            Console.WriteLine()
            Console.WriteLine("    Options:")
            Console.WriteLine("              -?           Displays this help message")
            Console.WriteLine("              -r           Recurse subdirectories")
            Console.WriteLine("              -c           Continue processing even if errors occur")
            Console.WriteLine("              -v           Enable verbose mode")
            Console.WriteLine()
            Console.WriteLine("Examples:")
            Console.WriteLine("    UpdateTimeStamp -r *.txt ""06/05/2004 9:20:00 AM""")
            Console.WriteLine("    UpdateTimeStamp -r -c -v ""*.txt;*.xml"" ""01/01/2000 00:00:00""")
            Console.WriteLine()
            Console.WriteLine("The file specs parameter will accept mutiple files specifications seperated")
            Console.WriteLine("by a semi-colon.  For example, UpdateTimeStamp -r ""A1\*.txt;B2\*.xml"" would")
            Console.WriteLine("scan all .txt files in folder A1 and its sub-folders, then scan all .xml")
            Console.WriteLine("files in folder B2 and all of its sub-folders.")
            End
        End If

        With New CommandLineOptions(Args)
            If Not .Verbose Then Console.Write("Processing, please wait...  ")

            For x As Integer = 0 To .FilePaths.Length - 1
                ' Replace text in files according to this file spec
                UpdateTimestampForFiles(.FilePaths(x), .FileSpecs(x), .This, Len(JustPath(RemovePathSuffix(.FilePaths(x)))))
            Next

            If Not .Verbose Then Console.WriteLine(vbCrLf)
            Console.WriteLine("Finished.  Processed " & ProcessedFiles & " files in " & SecondsToText(Timer - StartTime).ToLower())
        End With

        End

    End Sub

    Private Sub UpdateTimestampForFiles(ByVal FilePath As String, ByVal FileSpec As String, ByVal Options As CommandLineOptions, ByVal RootPathLength As Integer)

        If Options.Verbose Then Console.WriteLine("Scanning folder """ & Mid(FilePath, RootPathLength) & """:" & vbCrLf)

        ' Get each file in this folder according to given file spec
        For Each FileName As String In Directory.GetFiles(FilePath, FileSpec)
            UpdateTimestampForFile(FileName, Options)
        Next

        If Options.Verbose Then Console.WriteLine()

        ' If recursing subdirectories, do the same for each sub folder
        If Options.RecurseDirectories Then
            For Each SubDirectory As String In Directory.GetDirectories(FilePath)
                UpdateTimestampForFiles(SubDirectory, FileSpec, Options, RootPathLength)
            Next
        End If

    End Sub

    Private Sub UpdateTimestampForFile(ByVal FileName As String, ByVal Options As CommandLineOptions)

        Dim UpdateFailed As Boolean

        If Options.Verbose Then Console.Write("    Processing """ & JustFileName(FileName) & """...  ")

        Try
            File.SetCreationTime(FileName, Options.Timestamp)
            File.SetLastAccessTime(FileName, Options.Timestamp)
            File.SetLastWriteTime(FileName, Options.Timestamp)
            ShowWorkingImage()
            ProcessedFiles += 1
        Catch ex As Exception
            HideWorkingImage()
            Console.WriteLine(vbCrLf & vbCrLf & "ERROR: Updated failed for file """ & FileName & """ due to exception: " & ex.Message)
            If Options.ResumeOnError Then
                Console.Write("    Processing resumed...  ")
            Else
                UpdateFailed = True
            End If
        End Try

        If UpdateFailed Then End
        HideWorkingImage()
        If Options.Verbose Then Console.WriteLine()

    End Sub

    Private Sub ShowWorkingImage()

        If Timer - LastImageUpdate > 0.05 Then
            If WorkImageIndex > WorkImages.Length - 1 Then WorkImageIndex = 0
            Console.Write(Chr(8))
            Console.Write(WorkImages(WorkImageIndex))
            WorkImageIndex += 1
            LastImageUpdate = Timer
        End If

    End Sub

    Private Sub HideWorkingImage()

        Console.Write(Chr(8))
        Console.Write(" "c)

    End Sub

End Module
