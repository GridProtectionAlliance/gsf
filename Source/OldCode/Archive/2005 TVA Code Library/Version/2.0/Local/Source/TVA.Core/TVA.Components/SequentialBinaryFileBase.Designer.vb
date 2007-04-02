Namespace Components

    Partial Class SequentialBinaryFileBase(Of T As IBinaryDataProvider)
        Inherits System.ComponentModel.Component

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New(ByVal container As System.ComponentModel.IContainer)
            MyClass.New()

            'Required for Windows.Forms Class Composition Designer support
            If (container IsNot Nothing) Then
                container.Add(Me)
            End If

        End Sub

        <System.Diagnostics.DebuggerNonUserCode()> _
        Public Sub New()
            MyBase.New()

            'This call is required by the Component Designer.
            InitializeComponent()

            m_name = Me.GetType().Name & Extension
            m_minimumRecordCount = 100
            m_loadOnOpen = True
            m_saveOnClose = False
            m_alignOnSave = False
            m_autoSaveInterval = -1
            m_autoAlignInterval = -1
            m_persistSettings = False
            m_configurationCategory = Me.GetType().Name
            m_autoSaveTimer = New System.Timers.Timer()
            m_autoAnalyzeTimer = New System.Timers.Timer()

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
            components = New System.ComponentModel.Container()
        End Sub

    End Class

End Namespace