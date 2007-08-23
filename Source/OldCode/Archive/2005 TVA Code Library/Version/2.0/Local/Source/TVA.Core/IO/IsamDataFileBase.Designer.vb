Namespace IO

    Partial Class IsamDataFileBase(Of T As IBinaryDataProvider)
        Inherits System.ComponentModel.Component

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New(ByVal container As System.ComponentModel.IContainer)
            MyClass.New()

            'Required for Windows.Forms Class Composition Designer support.
            If (container IsNot Nothing) Then
                container.Add(Me)
            End If

        End Sub

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New()
            MyBase.New()

            'Required by the Component Designer.
            InitializeComponent()

            m_name = Me.GetType().Name & Extension
            m_minimumRecordCount = 0
            m_loadOnOpen = True
            m_reloadOnModify = True
            m_autoSaveInterval = -1
            m_settingsCategoryName = Me.GetType().Name

            m_autoSaveTimer = New System.Timers.Timer()
            m_loadWaitHandle = New System.Threading.ManualResetEvent(True)
            m_saveWaitHandle = New System.Threading.ManualResetEvent(True)

        End Sub

        'Overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                Close()         ' Closes the file.
                SaveSettings()  ' Saves settings to the config file.
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Component Designer.
        Private components As System.ComponentModel.IContainer

        'NOTE: Required by the Component Designer. 
        'Can be modified using the Component Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.FileSystemWatcher = New System.IO.FileSystemWatcher
            CType(Me.FileSystemWatcher, System.ComponentModel.ISupportInitialize).BeginInit()
            '
            'FileSystemWatcher
            '
            Me.FileSystemWatcher.EnableRaisingEvents = True
            CType(Me.FileSystemWatcher, System.ComponentModel.ISupportInitialize).EndInit()

        End Sub
        Friend WithEvents FileSystemWatcher As System.IO.FileSystemWatcher

    End Class

End Namespace