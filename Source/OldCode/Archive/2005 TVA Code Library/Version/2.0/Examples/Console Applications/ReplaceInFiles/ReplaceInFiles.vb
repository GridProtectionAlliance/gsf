' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Console

Module ReplaceInFiles

    Private ProcessedFiles As Integer
    Private WorkImageIndex As Integer
    Private WorkImages As Char() = {"\"c, "|"c, "/"c, "-"c}
    Private LastImageUpdate As Double

    Private Class CommandLineOptions

        Public FilePaths As String()
        Public FileSpecs As String()
        Public EncodeFormat As Encoding
        Public FindString As String
        Public ReplaceString As String
        Public RecurseDirectories As Boolean
        Public IgnoreCase As Boolean
        Public UseRegex As Boolean
        Public ResumeOnError As Boolean
        Public Verbose As Boolean
        Public FindExpression As Regex

        Public Sub New(ByVal Args As Arguments)

            Dim FilePath As String
            Dim ValidFilePaths As New ArrayList
            Dim ValidFileSpecs As New ArrayList

#If DEBUG Then
            For x As Integer = 0 To Args.Count - 1
                Console.WriteLine("Argument " & x & ": " & Args("OrderedArg" & x))
            Next

#End If

            ' Initialize options
            EncodeFormat = GetEncodeFormat(Args("e"))
            FindString = DecodeQuotes(Args("OrderedArg2"))
            ReplaceString = DecodeQuotes(Args("OrderedArg3"))
            RecurseDirectories = Args.Exists("r")
            IgnoreCase = Args.Exists("i")
            UseRegex = Args.Exists("x")
            ResumeOnError = Args.Exists("c")
            Verbose = Args.Exists("v")

            ' Create regular expression, if requested
            If UseRegex Then
                Try
                    FindExpression = New Regex(FindString, RegexOptions.Singleline Or RegexOptions.Compiled Or IIf(IgnoreCase, RegexOptions.IgnoreCase, RegexOptions.None))
                Catch ex As Exception
                    Console.WriteLine("ERROR: Invalid regular expression: " & ex.Message)
                    End
                End Try
            End If

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

        Private Function DecodeQuotes(ByVal Param As String) As String

            Dim QuotToken As String = Guid.NewGuid.ToString()

            Param = Replace(Param, "\\&quot;\\", QuotToken, 1, -1, CompareMethod.Text)
            Param = Replace(Param, "&quot;", """", 1, -1, CompareMethod.Text)
            Param = Replace(Param, QuotToken, "&quot;", 1, -1, CompareMethod.Text)

            Return Param

        End Function

        Private Function GetEncodeFormat(ByVal Param As String) As Encoding

            If Len(Param) = 0 Then
                ' We default to ANSI encoding
                Return Encoding.Default
            Else
                Select Case LCase(Trim(Param))
                    Case "ansi"
                        Return Encoding.Default
                    Case "ascii"
                        Return Encoding.ASCII
                    Case "utf7"
                        Return Encoding.UTF7
                    Case "utf8"
                        Return Encoding.UTF8
                    Case "unicode"
                        Return Encoding.Unicode
                    Case Else
                        Console.WriteLine("ERROR: Unrecognized encoding format specified: """ & Param & """")
                        End
                End Select
            End If

        End Function

    End Class

    Sub Main()

        Dim Args As New Arguments(Command)
        Dim StartTime As Double = Timer

        Console.WriteLine()
        Console.WriteLine("Replace text in files utility - JRC 2004")
        Console.WriteLine()

        If Len(Args("OrderedArg1")) = 0 OrElse Args.OrderedArgCount < 3 OrElse Args.ContainHelpRequest Then
            Console.WriteLine("Usage:")
            Console.WriteLine("    ReplaceInFiles -options FileSpecs ""Find Text"" ""Replacement Text""")
            Console.WriteLine()
            Console.WriteLine("    Options:")
            Console.WriteLine("              -?           Displays this help message")
            Console.WriteLine("              -e:ANSI      Files are ANSI encoded (Default)")
            Console.WriteLine("              -e:ASCII     Files are ASCII encoded")
            Console.WriteLine("              -e:UTF7      Files are UTF7 encoded")
            Console.WriteLine("              -e:UTF8      Files are UTF8 encoded")
            Console.WriteLine("              -e:Unicode   Files are Unicode encoded")
            Console.WriteLine("              -r           Recurse subdirectories")
            Console.WriteLine("              -i           Case insensitive search")
            Console.WriteLine("              -x           Find text is a regular expression")
            Console.WriteLine("              -c           Continue processing even if errors occur")
            Console.WriteLine("              -v           Enable verbose mode")
            Console.WriteLine()
            Console.WriteLine("Examples:")
            Console.WriteLine("    ReplaceInFiles -r Pet*.txt ""cat"" ""dog""")
            Console.WriteLine("    ReplaceInFiles -c ""*.txt;*.xml"" ""&quot;Quotes&quot;"" ""&quot;quotes&quot;""")
            Console.WriteLine("    ReplaceInFiles -e:UTF8 -i WordData.xml ""Two Words"" ""1 2 3 Words""")
            Console.WriteLine("    ReplaceInFiles -r -i -x -v Source\AssemblyInfo.* ""AssemblyVersion(Attribute)?\(&quot;((\*|\d+)\.)+(\*|\d+)&quot;\)"" ""AssemblyVersion(&quot;3.1.*&quot;)""")
            Console.WriteLine()
            Console.WriteLine("WARNING: This utility can make changes to mutiple files, please backup files!")
            Console.WriteLine()
            Console.WriteLine("NOTE: This utility is designed to work with text files only.  Also, you can")
            Console.WriteLine("enter &quot; into your find text or replacement text to embed a nested quote.")
            Console.WriteLine("If you really want the text ""&quot;"", enter ""\\&quot;\\"" instead.")
            Console.WriteLine()
            Console.WriteLine("The file specs parameter will accept mutiple files specifications seperated")
            Console.WriteLine("by a semi-colon.  For example, ReplaceInFiles -r ""A1\*.txt;B2\*.xml"" would")
            Console.WriteLine("scan all .txt files in folder A1 and its sub-folders, then scan all .xml")
            Console.WriteLine("files in folder B2 and all of its sub-folders.")
            End
        End If

        With New CommandLineOptions(Args)
            If Not .Verbose Then Console.Write("Processing, please wait...  ")

            For x As Integer = 0 To .FilePaths.Length - 1
                ' Replace text in files according to this file spec
                ReplaceTextInFiles(.FilePaths(x), .FileSpecs(x), .This, Len(JustPath(RemovePathSuffix(.FilePaths(x)))))
            Next

            If Not .Verbose Then Console.WriteLine(vbCrLf)
            Console.WriteLine("Finished.  Processed " & ProcessedFiles & " files in " & SecondsToText(Timer - StartTime).ToLower())
        End With

        End

    End Sub

    Private Sub ReplaceTextInFiles(ByVal FilePath As String, ByVal FileSpec As String, ByVal Options As CommandLineOptions, ByVal RootPathLength As Integer)

        If Options.Verbose Then Console.WriteLine("Scanning folder """ & Mid(FilePath, RootPathLength) & """:" & vbCrLf)

        ' Get each file in this folder according to given file spec
        For Each FileName As String In Directory.GetFiles(FilePath, FileSpec)
            ReplaceTextInFile(FileName, Options)
        Next

        If Options.Verbose Then Console.WriteLine()

        ' If recursing subdirectories, do the same for each sub folder
        If Options.RecurseDirectories Then
            For Each SubDirectory As String In Directory.GetDirectories(FilePath)
                ReplaceTextInFiles(SubDirectory, FileSpec, Options, RootPathLength)
            Next
        End If

    End Sub

    Private Sub ReplaceTextInFile(ByVal FileName As String, ByVal Options As CommandLineOptions)

        Dim TempFileName As String
        Dim SourceFile As FileStream
        Dim DestinationFile As FileStream
        Dim NextByte As Integer
        Dim Buffer As New ArrayList
        Dim FileLine As String
        Dim LineBytes As Byte()
        Dim ReplaceFailed As Boolean

        If Options.Verbose Then Console.Write("    Processing """ & JustFileName(FileName) & """...  ")

        Try
            ' Open source file and create a new temporary destination file
            TempFileName = JustPath(FileName) & GetTempFileName()
            SourceFile = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            DestinationFile = File.Create(TempFileName)

            NextByte = SourceFile.ReadByte()

            Do While NextByte <> -1
                Buffer.Add(CByte(NextByte))

                If NextByte = Asc(vbLf) Then
                    ' We've got a line of text, so let's process it
                    WriteReplacementLine(Options, Buffer, DestinationFile)
                    Buffer = New ArrayList
                    ShowWorkingImage()
                End If

                NextByte = SourceFile.ReadByte()
            Loop

            ' Flush any remaining bytes
            If Buffer.Count > 0 Then WriteReplacementLine(Options, Buffer, DestinationFile)

            SourceFile.Close()
            DestinationFile.Close()

            File.Delete(FileName)
            File.Move(TempFileName, FileName)

            ProcessedFiles += 1
        Catch ex As Exception
            HideWorkingImage()
            Console.WriteLine(vbCrLf & vbCrLf & "ERROR: Replace failed for file """ & FileName & """ due to exception: " & ex.Message)
            If Options.ResumeOnError Then
                Console.Write("    Processing resumed...  ")
            Else
                ReplaceFailed = True
            End If
        Finally
            If Not SourceFile Is Nothing Then SourceFile.Close()
            If Not DestinationFile Is Nothing Then DestinationFile.Close()
            If File.Exists(TempFileName) Then File.Delete(TempFileName)
        End Try

        If ReplaceFailed Then End
        HideWorkingImage()
        If Options.Verbose Then Console.WriteLine()

    End Sub

    Private Sub WriteReplacementLine(ByVal Options As CommandLineOptions, ByVal Buffer As ArrayList, ByVal DestinationFile As FileStream)

        With Options
            Dim FileLine As String
            Dim LineBytes As Byte() = Array.CreateInstance(GetType(Byte), Buffer.Count)
            Buffer.CopyTo(LineBytes)

            If .UseRegex Then
                FileLine = .FindExpression.Replace(.EncodeFormat.GetString(LineBytes), .ReplaceString)
            Else
                FileLine = Replace(.EncodeFormat.GetString(LineBytes), .FindString, .ReplaceString, 1, -1, IIf(.IgnoreCase, CompareMethod.Text, CompareMethod.Binary))
            End If

            ' Write this updated line out to the destination file
            LineBytes = .EncodeFormat.GetBytes(FileLine)

            DestinationFile.Write(LineBytes, 0, LineBytes.Length)
        End With

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