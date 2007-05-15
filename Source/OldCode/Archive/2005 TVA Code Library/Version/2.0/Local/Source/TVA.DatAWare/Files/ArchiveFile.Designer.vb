Namespace Files

    Partial Class DwArchiveFile
        Inherits System.ComponentModel.Component

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New(ByVal Container As System.ComponentModel.IContainer)
            MyClass.New()

            'Required for Windows.Forms Class Composition Designer support
            Container.Add(Me)

        End Sub

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New()
            MyBase.New()

            'This call is required by the Component Designer.
            InitializeComponent()

            m_name = Me.GetType().Name & Extension
            m_type = ArchiveFileType.Active
            m_size = 100L
            m_blockSize = 8
            m_saveOnClose = True
            m_rolloverOnFull = True
            m_rolloverPreparationThreshold = 75
            m_offloadCount = 5
            m_offloadThreshold = 90
            m_compressPoints = True
            m_discardOutOfSequencePoints = True
            m_settingsCategoryName = Me.GetType().Name

            m_rolloverPreparationThread = New System.Threading.Thread(AddressOf PrepareForRollover)
            m_buildHistoricFileListThread = New System.Threading.Thread(AddressOf BuildHistoricFileList)
            m_rolloverWaitHandle = New System.Threading.ManualResetEvent(True)

            m_historicDataQueue = TVA.Collections.ProcessQueue(Of StandardPointData).CreateRealTimeQueue(AddressOf WriteToHistoricArchiveFile)
            m_outOfSequenceDataQueue = TVA.Collections.ProcessQueue(Of StandardPointData).CreateRealTimeQueue(AddressOf InsertInCurrentArchiveFile)

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