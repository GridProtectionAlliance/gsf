' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports TVA.Console
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath

Module IncrementBuildNumber

    Private Enum BuildPart
        Major
        Minor
        BuildNumber
        Revision
    End Enum

    Sub Main()

        Dim Args As New Arguments(Command)
        Dim VersionFileName As String = Args("OrderedArg1")

        Console.WriteLine()
        Console.WriteLine("Increment build number utility - JRC 2004")
        Console.WriteLine()

        If Len(VersionFileName) = 0 OrElse Args.ContainHelpRequest Then
            Console.WriteLine("Usage:")
            Console.WriteLine("    IncrementBuildNumber VersionFile")
            End
        End If

        If File.Exists(VersionFileName) Then
            If GetFileLength(VersionFileName) > 0 Then
                Dim Version As String = GetVersion(VersionFileName)
                Dim Parts As String() = Version.Split("."c)

                If Parts.Length = 4 Then
                    ' Update build number
                    Parts(BuildPart.BuildNumber) = CStr(CInt(Parts(BuildPart.BuildNumber)) + 1)
                    Parts(BuildPart.Revision) = CStr(CInt(Timer))

                    ' Reassemble...
                    Version = GetNewBuildNumber(Parts)

                    ' Update build number file
                    WriteVersion(VersionFileName, Version)

                    Console.WriteLine("Build number changed to: " & Version)
                Else
                    Console.WriteLine("ERROR: Invalid build number format """ & VersionFileName & """, expecting ""n.n.n.n""")
                End If
            Else
                Console.WriteLine("ERROR: Specified build version file """ & VersionFileName & """ is empty, expecting build number in format ""n.n.n.n""")
            End If
        Else
            Console.WriteLine("ERROR: Specified build version file """ & VersionFileName & """ does not exist")
        End If

        End

    End Sub

    Private Function GetVersion(ByVal FileName As String) As String

        Dim VersionFile As StreamReader = File.OpenText(FileName)
        Dim Version As String = VersionFile.ReadLine()

        VersionFile.Close()
        Return Version

    End Function

    Private Sub WriteVersion(ByVal FileName As String, ByVal BuildNumber As String)

        Dim VersionFile As StreamWriter = File.CreateText(FileName)

        VersionFile.WriteLine(BuildNumber)
        VersionFile.Close()

    End Sub

    Private Function GetNewBuildNumber(ByVal Parts As String()) As String

        Return Parts(BuildPart.Major) & "." & Parts(BuildPart.Minor) & "." & Parts(BuildPart.BuildNumber) & "." & Parts(BuildPart.Revision)

    End Function

End Module