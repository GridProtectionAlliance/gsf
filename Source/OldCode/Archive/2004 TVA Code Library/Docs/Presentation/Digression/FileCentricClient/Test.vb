Imports System.IO
Imports System.Threading
Imports System.Text
Imports VB = Microsoft.VisualBasic

Public Class Test
    Inherits System.Windows.Forms.Form

    ' Define file name:
    Const HardCodedFileName As String = "C:\DataFile.xml"

    ' Define seconds to wait for read lock
    '   Adjust this for larger files!!!
    Const SecondsToWait As Integer = 10

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Results As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.Results = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'Results
        '
        Me.Results.Font = New System.Drawing.Font("Comic Sans MS", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Results.Location = New System.Drawing.Point(8, 8)
        Me.Results.Name = "Results"
        Me.Results.Size = New System.Drawing.Size(272, 256)
        Me.Results.TabIndex = 0
        '
        'Test
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.Add(Me.Results)
        Me.Name = "Test"
        Me.Text = "File Centric Client Example"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If File.Exists(HardCodedFileName) Then
            ' Wait for file read lock - don't want to attempt reading
            ' file if it's in use!!!!
            Dim fs As FileStream
            Dim dblStart As Double = VB.Timer

            While True
                Try
                    fs = File.OpenRead(HardCodedFileName)
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
                        Throw New IOException("Could not open """ & HardCodedFileName & """ for read access, tried for " & SecondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

            Dim xmlDoc As New Xml.XmlDocument
            Dim msg As New StringBuilder

            xmlDoc.Load(HardCodedFileName)

            msg.Append("ID = " & xmlDoc.SelectSingleNode("client/id").InnerText & vbCrLf)
            msg.Append("Name = " & xmlDoc.SelectSingleNode("client/name").InnerText & vbCrLf)
            msg.Append("Address = " & xmlDoc.SelectSingleNode("client/address").InnerText & vbCrLf)
            msg.Append("City = " & xmlDoc.SelectSingleNode("client/city").InnerText & vbCrLf)
            msg.Append("State = " & xmlDoc.SelectSingleNode("client/state").InnerText & vbCrLf)
            msg.Append("Zip = " & xmlDoc.SelectSingleNode("client/zip").InnerText & vbCrLf)
            Results.Text = msg.ToString()

        End If

    End Sub
End Class
