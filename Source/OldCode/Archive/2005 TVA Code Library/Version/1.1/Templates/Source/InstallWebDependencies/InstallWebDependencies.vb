Imports System.IO
Imports TVA.Console
Imports TVA.Shared.FilePath

Module InstallWebDependencies

    Sub Main()

        Dim args As New Arguments(Command, "Item")
        Dim destFileName As String

        For Each sourceFileName As String In Directory.GetFiles(args("Item1"), "*.dll")
            destFileName = args("Item2") & JustFileName(sourceFileName)
            If Not File.Exists(destFileName) Then File.Copy(sourceFileName, destFileName)
        Next

    End Sub

End Module
