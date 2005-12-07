Imports System.IO
Imports TVA.Console

Module DeleteWebDependencies

    Sub Main()

        Dim args As New Arguments(Command, "Item")

        For Each sourceFileName As String In Directory.GetFiles(args("Item1"), "*.dll")
            Try
                File.Delete(sourceFileName)
            Catch
            End Try
        Next

    End Sub

End Module
