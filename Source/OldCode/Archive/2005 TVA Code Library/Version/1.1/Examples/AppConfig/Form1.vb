Imports TVA.Config.Common

Public Class Form1
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()

        MyBase.New()

        Debug.WriteLine(SharedConfigFileName())
        Settings.Create("Form1.AccessibleName", "(None)")
        Settings.Save()

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
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader()
        '
        'Form1
        '
        Me.AccessibleName = CType(configurationAppSettings.GetValue("Form1.AccessibleName", GetType(System.String)), String)
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Name = "Form1"
        Me.Text = "Form1"

    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Variables.Create("EvalTest", "DateTime(new Date())", VariableType.Eval)
        Variables.Create("BoolTest1", True, VariableType.Bool)
        Variables.Create("BoolTest2", False, VariableType.Bool)
        Variables.Create("IntTest", 12, VariableType.Int)
        Variables.Create("NewTest", 12, VariableType.Int, "Hello?", 12, )
        Variables.Create("FloatTest", 12.4567987324098, VariableType.Float)
        Variables.Create("DateTest", Now(), VariableType.Date)
        Variables.Create("StringTest", "YadaYadaYada", VariableType.Text)
        Variables.Create("UnspecifiedTest", 123)
        Variables.Create("UnspecifiedTest2", Now())
        Variables.Create("UnspecifiedTest3", "hohoho")
        Variables.Create("UndeterminedTest", "---", VariableType.Undetermined)
        Variables.Create("UndeterminedTest2", Now(), VariableType.Undetermined)
        Variables.Create("CanYouUseMe3", 12, VariableType.Int, "Can You Use Me?", 15)
        Variables.Create("DBTest", "SELECT Data FROM Variables WHERE Name='AppTitle' AND CategoryID=1", VariableType.Database)
        Variables("DateTest") = Now()

#If Debug Then
        Debug.WriteLine(Variables("DateTest"))
        Variables.WriteLog = True
#End If

    End Sub

End Class