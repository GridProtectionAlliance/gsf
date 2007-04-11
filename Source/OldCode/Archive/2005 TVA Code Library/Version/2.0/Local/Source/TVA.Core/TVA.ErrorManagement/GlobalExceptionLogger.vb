' 04/03/2007

Imports System.Text
Imports System.Reflection
Imports System.Drawing
Imports System.ComponentModel
Imports System.Windows.Forms
Imports TVA.Net.Smtp

Namespace ErrorManagement

    <ToolboxBitmap(GetType(GlobalExceptionLogger))> _
    Public Class GlobalExceptionLogger
        Implements IPersistSettings, ISupportInitialize

#Region " Code Scope: Public "

        Private m_autoStart As Boolean
        Private m_logToFile As Boolean
        Private m_logToEmail As Boolean
        Private m_logToEventLog As Boolean
        Private m_logToScreenshot As Boolean
        Private m_emailRecipients As String
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String
        Private m_isRunning As Boolean
        Private m_customLoggers As List(Of LoggerMethodSignature)

        Public Delegate Sub LoggerMethodSignature(ByVal ex As Exception)

        <Category("Behavior")> _
        Public Property AutoStart() As Boolean
            Get
                Return m_autoStart
            End Get
            Set(ByVal value As Boolean)
                m_autoStart = value
            End Set
        End Property

        <Category("Logging")> _
        Public Property LogToFile() As Boolean
            Get
                Return m_logToFile
            End Get
            Set(ByVal value As Boolean)
                m_logToFile = value
            End Set
        End Property

        <Category("Logging")> _
        Public Property LogToEmail() As Boolean
            Get
                Return m_logToEmail
            End Get
            Set(ByVal value As Boolean)
                m_logToEmail = value
            End Set
        End Property

        <Category("Logging")> _
        Public Property LogToEventLog() As Boolean
            Get
                Return m_logToEventLog
            End Get
            Set(ByVal value As Boolean)
                m_logToEventLog = value
            End Set
        End Property

        <Category("Logging")> _
        Public Property LogToScreenshot() As Boolean
            Get
                Return m_logToScreenshot
            End Get
            Set(ByVal value As Boolean)
                m_logToScreenshot = value
            End Set
        End Property

        <Category("Logging")> _
        Public Property EmailRecipients() As String
            Get
                Return m_emailRecipients
            End Get
            Set(ByVal value As String)
                m_emailRecipients = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property CustomLoggers() As List(Of LoggerMethodSignature)
            Get
                Return m_customLoggers
            End Get
        End Property

        Public Sub Start()

            If Not Debugger.IsAttached Then
                ' For winform applications. 
                AddHandler System.Windows.Forms.Application.ThreadException, AddressOf UnhandledThreadException

                ' For console applications.
                AddHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledException
            End If

            LogFile.Name = LogFileName
            m_isRunning = True

        End Sub

        Public Sub [Stop]()

            If Not Debugger.IsAttached Then
                RemoveHandler System.Windows.Forms.Application.ThreadException, AddressOf UnhandledThreadException
                RemoveHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledException
            End If

            m_isRunning = False

        End Sub

        Public Sub Log(ByVal ex As Exception)

            If m_isRunning Then
                GenericExceptionHandler(ex)
            Else
                Throw New InvalidOperationException(String.Format("{0} is not running.", Me.GetType().Name))
            End If

        End Sub

#Region " Shared "

        Public Shared Function ExceptionToString(ByVal ex As Exception) As String

            With New StringBuilder()
                If ex.InnerException IsNot Nothing Then
                    ' sometimes the original exception is wrapped in a more relevant outer exception
                    ' the detail exception is the "inner" exception
                    ' see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/exceptdotnet.asp
                    .Append("(Inner Exception)")
                    .AppendLine()
                    .Append(ExceptionToString(ex.InnerException))
                    .AppendLine()
                    .Append("(Outer Exception)")
                    .AppendLine()
                End If

                ' get general system and app information
                .AppendFormat("Date and Time:         {0}", System.DateTime.Now)
                .AppendLine()
                .AppendFormat("Machine Name:          {0}", Environment.MachineName)
                .AppendLine()
                .AppendFormat("IP Address:            {0}", System.Net.Dns.GetHostName())
                .AppendLine()
                .AppendFormat("Current User:          {0}", System.Threading.Thread.CurrentPrincipal.Identity.Name)
                .AppendLine()
                .AppendLine()
                .AppendFormat("Application Domain:    {0}", System.AppDomain.CurrentDomain.FriendlyName)
                .AppendLine()
                .AppendFormat("Assembly Codebase:     {0}", TVA.Assembly.EntryAssembly.CodeBase)
                .AppendLine()
                .AppendFormat("Assembly Full Name:    {0}", TVA.Assembly.EntryAssembly.FullName)
                .AppendLine()
                .AppendFormat("Assembly Version:      {0}", TVA.Assembly.EntryAssembly.Version.ToString())
                .AppendLine()
                .AppendFormat("Assembly Build Date:   {0}", TVA.Assembly.EntryAssembly.BuildDate.ToString())
                .AppendLine()
                .AppendLine()

                Try
                    ' get exception-specific information
                    .AppendFormat("Exception Source:      {0}", ex.Source)
                    .AppendLine()
                    .AppendFormat("Exception Type:        {0}", ex.GetType().FullName)
                    .AppendLine()
                    .AppendFormat("Exception Message:     {0}", ex.Message)
                    .AppendLine()
                    .AppendFormat("Exception Target Site: {0}", ex.TargetSite.Name)
                    .AppendLine()
                Catch

                Finally
                    .AppendLine()
                End Try

                .Append("---- Stack Trace ----")
                .AppendLine()
                Dim stack As New StackTrace(ex, True)
                For i As Integer = 0 To stack.FrameCount - 1
                    Dim stackFrame As StackFrame = stack.GetFrame(i)
                    Dim method As MethodInfo = stackFrame.GetMethod()
                    Dim codeFileName As String = stackFrame.GetFileName()

                    ' build method name
                    .AppendFormat("   {0}.{1}.{2}", method.DeclaringType.Namespace, method.DeclaringType.Name, method.Name)

                    ' build method params
                    .Append("(")
                    Dim parameterCount As Integer = 0
                    For Each parameter As ParameterInfo In stackFrame.GetMethod.GetParameters()
                        parameterCount += 1
                        If parameterCount > 1 Then .Append(", ")
                        .AppendFormat("{0} As {1}", parameter.Name, parameter.ParameterType.Name)
                    Next
                    .Append(")")
                    .AppendLine()

                    ' if source code is available, append location info
                    .Append("       ")

                    If Not String.IsNullOrEmpty(codeFileName) Then
                        .Append(System.IO.Path.GetFileName(codeFileName))
                        .AppendFormat(": Ln {0:#0000}", stackFrame.GetFileLineNumber())
                        .AppendFormat(", Col {0:#00}", stackFrame.GetFileColumnNumber())
                        ' if IL is available, append IL location info
                        If stackFrame.GetILOffset() <> stackFrame.OFFSET_UNKNOWN Then
                            .AppendFormat(", IL {0:#0000}", stackFrame.GetILOffset())
                        End If
                    Else
                        .Append(System.IO.Path.GetFileName(TVA.Assembly.EntryAssembly.CodeBase))
                        ' native code offset is always available
                        .AppendFormat(": N {0:#00000}", stackFrame.GetNativeOffset())
                    End If
                    .AppendLine()
                Next
                .AppendLine()

                Return .ToString()
            End With

        End Function

#End Region

#Region " Interface Implementation "

#Region " IPersistSettings "

        <Category("Settings")> _
        Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
            Get
                Return m_persistSettings
            End Get
            Set(ByVal value As Boolean)
                m_persistSettings = value
            End Set
        End Property

        <Category("Settings")> _
        Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
            Get
                Return m_settingsCategoryName
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_settingsCategoryName = value
                Else
                    Throw New ArgumentNullException("ConfigurationCategory")
                End If
            End Set
        End Property

        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

        End Sub

        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

        End Sub

#End Region

#Region " ISupportInitialize "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                LoadSettings()
                If m_autoStart Then Start()
            End If

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

        Private ReadOnly Property ApplicationName() As String
            Get
                Return System.AppDomain.CurrentDomain.FriendlyName
            End Get
        End Property

        Private ReadOnly Property LogFileName() As String
            Get
                Return ApplicationName & ".ExceptionLog.txt"
            End Get
        End Property

        Private ReadOnly Property ScreenshotFileName() As String
            Get
                Return ApplicationName & ".ExceptionScreenshot.png"
            End Get
        End Property

        Private Sub GenericExceptionHandler(ByVal ex As Exception)

            Dim exceptionString As String = ExceptionToString(ex)

            ExceptionToScreenshot(exceptionString)
            ExceptionToEventLog(exceptionString)
            ExceptionToEmail(exceptionString)
            ExceptionToFile(exceptionString)

        End Sub

        Private Sub ExceptionToFile(ByVal exceptionMessage As String)

            If m_logToFile Then
                Try
                    If Not LogFile.IsOpen Then LogFile.Open()
                    LogFile.WriteTimestampedLine(exceptionMessage)
                Catch ex As Exception

                Finally
                    LogFile.Close()
                End Try
            End If

        End Sub

        Private Sub ExceptionToEmail(ByVal exceptionMessage As String)

            If m_logToEmail AndAlso Not String.IsNullOrEmpty(m_emailRecipients) Then
                Try
                    With New SimpleMailMessage()
                        .Sender = String.Format("{0}@tva.gov", Me.GetType().Name)
                        .Recipients = m_emailRecipients
                        .Subject = String.Format("Exception in {0} at {1}", ApplicationName, Date.Now.ToString())
                        .Body = exceptionMessage
                        .Attachments = TVA.IO.FilePath.AbsolutePath(ScreenshotFileName)
                        .Send()
                    End With
                Catch ex As Exception

                End Try
            End If

        End Sub

        Private Sub ExceptionToEventLog(ByVal exceptionMessage As String)

            If m_logToEventLog Then
                Try
                    ' Write the formatted exception message to the event log.
                    EventLog.WriteEntry(ApplicationName, exceptionMessage, EventLogEntryType.Error)
                Catch ex As Exception

                End Try
            End If

        End Sub

        Private Sub ExceptionToScreenshot(ByVal exceptionMessage As String)

            If m_logToScreenshot Then
                Try
                    Dim fullScreen As New Size(0, 0)
                    For Each myScreen As Screen In Screen.AllScreens
                        If fullScreen.IsEmpty Then
                            fullScreen = myScreen.Bounds.Size
                        Else
                            If myScreen.Bounds.Location.X > 0 Then fullScreen.Width += myScreen.Bounds.Width
                            If myScreen.Bounds.Location.Y > 0 Then fullScreen.Height += myScreen.Bounds.Height
                        End If
                    Next

                    Using snap As Bitmap = TVA.Drawing.Image.CaptureScreenshot(fullScreen, Imaging.ImageFormat.Png)
                        snap.Save(ScreenshotFileName)
                    End Using
                Catch ex As Exception

                End Try
            End If

        End Sub

#Region " Event Handlers "

        Private Sub UnhandledThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)

            GenericExceptionHandler(e.Exception)

        End Sub

        Private Sub UnhandledException(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)

            GenericExceptionHandler(CType(e.ExceptionObject, Exception))

        End Sub

#End Region

#End Region

    End Class

End Namespace