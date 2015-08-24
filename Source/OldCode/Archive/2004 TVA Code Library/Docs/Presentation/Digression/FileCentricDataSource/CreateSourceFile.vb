Imports System.IO
Imports System.Threading
Imports System.Data.OleDb
Imports VB = Microsoft.VisualBasic

Module CreateSourceFile

    ' Define file name:
    Const HardCodedFileName As String = "C:\DataFile.xml"

    ' Define seconds to wait for write lock
    '   Adjust this for larger files!!!
    Const SecondsToWait As Integer = 10

    Sub Main()        

        If File.Exists(HardCodedFileName) Then
            ' Wait for file write lock - don't want to overwrite
            ' file if it's in use!!!!
            Dim fs As FileStream
            Dim dblStart As Double = VB.Timer

            While True
                Try
                    fs = File.OpenWrite(HardCodedFileName)
                    fs.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If Not fs Is Nothing Then
                    Try
                        fs.Close()
                    Catch
                    End Try
                    fs = Nothing
                End If

                If SecondsToWait > 0 Then
                    If VB.Timer > dblStart + CDbl(SecondsToWait) Then
                        Throw New IOException("Could not open """ & HardCodedFileName & """ for write access, tried for " & SecondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While
        End If

        ' OK - got a read lock, lets create a new file...
        Dim cnn As New OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=C:\Clients.mdb")
        Dim cmd As OleDbCommand
        Dim rst As OleDbDataReader
        Dim output As StreamWriter    

        cnn.Open()
        cmd = New OleDbCommand("SELECT * FROM Clients WHERE ID=" & Command(), cnn)
        rst = cmd.ExecuteReader(CommandBehavior.SingleRow)
        rst.Read()

        output = File.CreateText(HardCodedFileName)
        output.WriteLine("<?xml version=""1.0"" encoding=""utf-8"" ?>")
        output.WriteLine("<client>")
        output.WriteLine("	<id>" & rst("ID") & "</id>")
        output.WriteLine("	<name>" & rst("Name") & "</name>")
        output.WriteLine("	<address>" & rst("Address") & "</address>")
        output.WriteLine("	<city>" & rst("City") & "</city>")
        output.WriteLine("	<state>" & rst("State") & "</state>")
        output.WriteLine("	<zip>" & rst("Zip") & "</zip>")
        output.WriteLine("</client>")
        output.Close()

        rst.Close()
        cnn.Close()
        End

    End Sub

End Module
