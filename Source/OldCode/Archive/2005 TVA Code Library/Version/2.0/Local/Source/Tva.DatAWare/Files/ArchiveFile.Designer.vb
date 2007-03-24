Namespace Files

    Partial Class ArchiveFile
        Inherits System.ComponentModel.Component

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New(ByVal Container As System.ComponentModel.IContainer)
            MyClass.New()

            'Required for Windows.Forms Class Composition Designer support
            Container.Add(Me)

        End Sub

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New()

            MyClass.New(False)

        End Sub

        'Component overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                Close()         ' Close the file.
                SaveSettings()  ' Saves settings to the config file.
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Component Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Component Designer
        'It can be modified using the Component Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.CurrentLocationFileSystemWatcher = New System.IO.FileSystemWatcher
            Me.OffloadLocationFileSystemWatcher = New System.IO.FileSystemWatcher
            CType(Me.CurrentLocationFileSystemWatcher, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.OffloadLocationFileSystemWatcher, System.ComponentModel.ISupportInitialize).BeginInit()
            '
            'CurrentLocationFileSystemWatcher
            '
            Me.CurrentLocationFileSystemWatcher.EnableRaisingEvents = True
            Me.CurrentLocationFileSystemWatcher.Filter = "*.d"
            Me.CurrentLocationFileSystemWatcher.IncludeSubdirectories = True
            '
            'OffloadLocationFileSystemWatcher
            '
            Me.OffloadLocationFileSystemWatcher.EnableRaisingEvents = True
            Me.OffloadLocationFileSystemWatcher.Filter = "*.d"
            Me.OffloadLocationFileSystemWatcher.IncludeSubdirectories = True
            CType(Me.CurrentLocationFileSystemWatcher, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.OffloadLocationFileSystemWatcher, System.ComponentModel.ISupportInitialize).EndInit()

        End Sub
        Friend WithEvents CurrentLocationFileSystemWatcher As System.IO.FileSystemWatcher
        Friend WithEvents OffloadLocationFileSystemWatcher As System.IO.FileSystemWatcher

    End Class

End Namespace