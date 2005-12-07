Module MainModule

    Public Sub Main()

        ' Arg1 = "IIS Folder Path"  ex: "MyWebApp"
        ' Arg2 = "Default Document" ex: "Default.aspx"
        ' Arg3 = "Physical Path"    ex: "D:\Inetpub\wwwroot\MyWebApp"

        Dim args As New TVA.Console.Arguments(Command, "Item")
        Dim wto As New TVA.WizTools

        ' Create new ASP.NET admin application in IIS
        Shell("cscript.exe c:\inetpub\adminscripts\adsutil.vbs CREATE W3SVC/1/ROOT/" & args("Item1") & " ""IIsWebVirtualDir""", AppWinStyle.Hide, True, -1)
        Shell("cscript.exe c:\inetpub\adminscripts\adsutil.vbs SET W3SVC/1/ROOT/" & args("Item1") & "/Path """ & args("Item3") & """", AppWinStyle.Hide, True, -1)
        Shell("cscript.exe c:\inetpub\adminscripts\adsutil.vbs SET W3SVC/1/ROOT/" & args("Item1") & "/DefaultDoc """ & args("Item2") & """", AppWinStyle.Hide, True, -1)
        Shell("cscript.exe c:\inetpub\adminscripts\adsutil.vbs APPCREATEINPROC W3SVC/1/ROOT/" & args("Item1"), AppWinStyle.Hide, True, -1)

        ' Fix common bug with ASP.NET access to windows temp folder
        wto.UpdateACL("C:\Windows\Temp", "ASPNET", "F")

        ' Allow ASP.NET access to the specified virtual folder
        wto.UpdateACL(args("Item3"), "ASPNET", "F")

    End Sub

End Module
