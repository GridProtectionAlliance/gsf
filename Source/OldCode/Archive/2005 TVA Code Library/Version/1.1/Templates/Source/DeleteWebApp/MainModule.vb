Module MainModule

    Public Sub Main()

        Dim args As New TVA.Console.Arguments(Command, "Item")

        Shell("cscript.exe c:\inetpub\adminscripts\adsutil.vbs APPDELETE W3SVC/1/ROOT/" & args("Item1"), AppWinStyle.Hide, True, -1)
        Shell("cscript.exe c:\inetpub\adminscripts\adsutil.vbs DELETE W3SVC/1/ROOT/" & args("Item1"), AppWinStyle.Hide, True, -1)

    End Sub

End Module
