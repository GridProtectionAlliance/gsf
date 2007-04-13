' PCP: 04/03/2007

Imports System.Text
Imports System.Reflection
Imports System.Drawing
Imports System.ComponentModel
Imports System.Windows.Forms
Imports TVA.Net.Smtp

Namespace ErrorManagement

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Adapted from exception handling code by Jeff Atwood of CodingHorror.com. Demo projects for handling unhandled
    ''' exception in both windows and web environment by Jeff Atwood are available at The Code Project web site. 
    ''' See: http://www.codeproject.com/script/articles/list_articles.asp?userid=450027
    ''' </remarks>
    <ToolboxBitmap(GetType(GlobalExceptionLogger))> _
    Public Class GlobalExceptionLogger
        Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_autoStart As Boolean
        Private m_logToUI As Boolean
        Private m_logToFile As Boolean
        Private m_logToEmail As Boolean
        Private m_logToEventLog As Boolean
        Private m_logToScreenshot As Boolean
        Private m_emailServer As String
        Private m_emailRecipients As String
        Private m_contactPersonName As String
        Private m_contactPersonPhone As String
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String
        Private m_customLoggers As List(Of LoggerMethodSignature)
        Private m_parentAssembly As System.Reflection.Assembly

#End Region

#Region " Code Scope: Public "

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
        Public Property EmailServer() As String
            Get
                Return m_emailServer
            End Get
            Set(ByVal value As String)
                m_emailServer = value
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

        Public Property ContactPersonName() As String
            Get
                Return m_contactPersonName
            End Get
            Set(ByVal value As String)
                m_contactPersonName = value
            End Set
        End Property

        Public Property ContactPersonPhone() As String
            Get
                Return m_contactPersonPhone
            End Get
            Set(ByVal value As String)
                m_contactPersonPhone = value
            End Set
        End Property

        <Browsable(False)> _
        Public Property ParentAssembly() As System.Reflection.Assembly
            Get
                Return m_parentAssembly
            End Get
            Set(ByVal value As System.Reflection.Assembly)
                m_parentAssembly = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ApplicationName() As String
            Get
                Return System.AppDomain.CurrentDomain.FriendlyName
            End Get
        End Property

        Public ReadOnly Property ApplicationType() As ApplicationType
            Get
                Return TVA.Common.GetApplicationType()
            End Get
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

        End Sub

        Public Sub [Stop]()

            If Not Debugger.IsAttached Then
                RemoveHandler System.Windows.Forms.Application.ThreadException, AddressOf UnhandledThreadException
                RemoveHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledException
            End If

        End Sub

        Public Sub Log(ByVal ex As Exception)

            GenericExceptionHandler(ex)

        End Sub

#Region " Shared "

        Public Shared Function ExceptionToString(ByVal ex As Exception) As String

            Dim parentAssembly As System.Reflection.Assembly
            Select Case TVA.Common.GetApplicationType()
                Case ApplicationType.WindowsGui, TVA.ApplicationType.WindowsCui
                    parentAssembly = System.Reflection.Assembly.GetEntryAssembly()
                Case ApplicationType.Web
                    parentAssembly = System.Reflection.Assembly.GetCallingAssembly()
            End Select

            Return ExceptionToString(ex, parentAssembly)

        End Function

        Public Shared Function SystemInfo() As String

            With New StringBuilder()
                .AppendFormat("Date and Time:         {0}", System.DateTime.Now)
                .AppendLine()
                Select Case TVA.Common.GetApplicationType()
                    Case ApplicationType.WindowsGui, TVA.ApplicationType.WindowsCui
                        .AppendFormat("Machine Name:          {0}", System.Environment.MachineName)
                        .AppendLine()
                        .AppendFormat("Machine IP:            {0}", System.Net.Dns.GetHostEntry(System.Environment.MachineName).AddressList(0).ToString())
                        .AppendLine()
                        .AppendFormat("Current User:          {0}", System.Threading.Thread.CurrentPrincipal.Identity.Name)
                        .AppendLine()
                    Case ApplicationType.Web
                        .AppendFormat("Server Name:           {0}", System.Environment.MachineName)
                        .AppendLine()
                        .AppendFormat("Server IP:             {0}", System.Net.Dns.GetHostEntry(System.Environment.MachineName).AddressList(0).ToString())
                        .AppendLine()
                        .AppendFormat("Process User:          {0}", System.Security.Principal.WindowsIdentity.GetCurrent().Name)
                        .AppendLine()
                        .AppendFormat("Remote User:           {0}", System.Web.HttpContext.Current.Request.ServerVariables("REMOTE_USER"))
                        .AppendLine()
                        .AppendFormat("Remote Host:           {0}", System.Web.HttpContext.Current.Request.ServerVariables("REMOTE_HOST"))
                        .AppendLine()
                        .AppendFormat("Remote Address:        {0}", System.Web.HttpContext.Current.Request.ServerVariables("REMOTE_ADDR"))
                        .AppendLine()
                        .AppendFormat("HTTP Agent:            {0}", System.Web.HttpContext.Current.Request.ServerVariables("HTTP_USER_AGENT"))
                        .AppendLine()
                        .AppendFormat("HTTP Referer:          {0}", System.Web.HttpContext.Current.Request.ServerVariables("HTTP_REFERER"))
                        .AppendLine()
                        .AppendFormat("Web Page URL:          {0}", System.Web.HttpContext.Current.Request.Url.ToString())
                        .AppendLine()
                End Select

                Return .ToString()
            End With

        End Function

        Public Shared Function ApplicationInfo() As String

            Dim parentAssembly As System.Reflection.Assembly
            Select Case TVA.Common.GetApplicationType()
                Case ApplicationType.WindowsGui, TVA.ApplicationType.WindowsCui
                    ' For a windows application the entry assembly will be the executable.
                    parentAssembly = System.Reflection.Assembly.GetEntryAssembly()
                Case ApplicationType.Web
                    ' For a web site in .Net 2.0 we don't have an entry assembly. However, at this point the
                    ' calling assembly will be consumer of this function (i.e. one of the web site DLLs).
                    ' See: http://msdn.microsoft.com/msdnmag/issues/06/01/ExtremeASPNET/
                    parentAssembly = System.Reflection.Assembly.GetCallingAssembly()
            End Select

            Return ApplicationInfo(parentAssembly)

        End Function

        Public Shared Function ExceptionGeneralInfo(ByVal ex As Exception) As String

            With New StringBuilder()
                .AppendFormat("Exception Source:      {0}", ex.Source)
                .AppendLine()
                .AppendFormat("Exception Type:        {0}", ex.GetType().FullName)
                .AppendLine()
                .AppendFormat("Exception Message:     {0}", ex.Message)
                .AppendLine()
                If ex.TargetSite IsNot Nothing Then
                    .AppendFormat("Exception Target Site: {0}", ex.TargetSite.Name)
                    .AppendLine()
                End If

                Return .ToString()
            End With

        End Function

        Public Shared Function ExceptionStackTrace(ByVal ex As Exception) As String

            With New StringBuilder()
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
                        Dim appType As ApplicationType = TVA.Common.GetApplicationType()
                        If appType = ApplicationType.WindowsGui OrElse appType = TVA.ApplicationType.WindowsCui Then
                            .Append(System.IO.Path.GetFileName(TVA.Assembly.EntryAssembly.CodeBase))
                        Else
                            .Append("(unknown file)")
                        End If
                        ' native code offset is always available
                        .AppendFormat(": N {0:#00000}", stackFrame.GetNativeOffset())
                    End If
                    .AppendLine()
                Next

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

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                        AutoStart = .Item("AutoStart").GetTypedValue(m_autoStart)
                        LogToFile = .Item("LogToFile").GetTypedValue(m_logToFile)
                        LogToEmail = .Item("LogToEmail").GetTypedValue(m_logToEmail)
                        LogToEventLog = .Item("LogToEventLog").GetTypedValue(m_logToEventLog)
                        LogToScreenshot = .Item("LogToScreenshot").GetTypedValue(m_logToScreenshot)
                        EmailServer = .Item("EmailServer").GetTypedValue(m_emailServer)
                        EmailRecipients = .Item("EmailRecipients").GetTypedValue(m_emailRecipients)
                        ContactPersonName = .Item("ContactPersonName").GetTypedValue(m_contactPersonName)
                        ContactPersonPhone = .Item("ContactPersonPhone").GetTypedValue(m_contactPersonPhone)
                    End With
                Catch ex As Exception
                    ' Most likely we'll never encounter an exception here.
                End Try
            End If

        End Sub

        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                        .Clear()
                        With .Item("AutoStart", True)
                            .Value = m_autoStart.ToString()
                            .Description = ""
                        End With
                        With .Item("LogToFile", True)
                            .Value = m_logToFile.ToString()
                            .Description = ""
                        End With
                        With .Item("LogToEmail", True)
                            .Value = m_logToEmail.ToString()
                            .Description = ""
                        End With
                        With .Item("LogToEventLog", True)
                            .Value = m_logToEventLog.ToString()
                            .Description = ""
                        End With
                        With .Item("LogToScreenshot", True)
                            .Value = m_logToScreenshot.ToString()
                            .Description = ""
                        End With
                        With .Item("EmailServer", True)
                            .Value = m_emailServer
                            .Description = ""
                        End With
                        With .Item("EmailRecipients", True)
                            .Value = m_emailRecipients
                            .Description = ""
                        End With
                        With .Item("ContactPersonName", True)
                            .Value = m_contactPersonName
                            .Description = ""
                        End With
                        With .Item("ContactPersonPhone", True)
                            .Value = m_contactPersonPhone
                            .Description = ""
                        End With
                    End With
                    TVA.Configuration.Common.SaveSettings()
                Catch ex As Exception
                    ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
                End Try
            End If

        End Sub

#End Region

#Region " ISupportInitialize "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                LoadSettings()              ' Load settings from the config file.
                If m_autoStart Then Start() ' Start the logger automatically if specified.
            End If

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

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

            Dim exceptionString As String = ExceptionToString(ex, m_parentAssembly)

            ExceptionToScreenshot(exceptionString)
            ExceptionToEventLog(exceptionString)
            ExceptionToEmail(exceptionString)
            ExceptionToFile(exceptionString)

            For Each logger As LoggerMethodSignature In m_customLoggers
                Try
                    logger.Invoke(ex)
                Catch

                End Try
            Next

        End Sub

        Private Sub ExceptionToUI()

            If m_logToUI Then
                Select Case ApplicationType
                    Case TVA.ApplicationType.WindowsGui

                    Case TVA.ApplicationType.WindowsCui

                    Case TVA.ApplicationType.Web

                End Select
            End If

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
                        .Subject = String.Format("Exception in {0} at {1}", ApplicationName, System.DateTime.Now.ToString())
                        .Body = exceptionMessage
                        .Attachments = TVA.IO.FilePath.AbsolutePath(ScreenshotFileName)
                        .MailServer = m_emailServer
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

            If m_logToScreenshot AndAlso _
                    (ApplicationType = ApplicationType.WindowsGui OrElse ApplicationType = ApplicationType.WindowsCui) Then
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

                    Using screenshot As Bitmap = TVA.Drawing.Image.CaptureScreenshot(fullScreen, Imaging.ImageFormat.Png)
                        screenshot.Save(ScreenshotFileName)
                    End Using
                Catch ex As Exception

                End Try
            End If

        End Sub

#Region " Shared "

        Private Shared Function ExceptionToString(ByVal ex As Exception, ByVal parentAssembly As System.Reflection.Assembly) As String

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

                ' Get general system information.
                .Append(SystemInfo)
                .AppendLine()
                ' Get general application information.
                .Append(ApplicationInfo(parentAssembly))
                .AppendLine()
                ' Get general exception information.
                .Append(ExceptionGeneralInfo(ex))
                .AppendLine()
                ' Get the stack trace for the exception.
                .Append("---- Stack Trace ----")
                .AppendLine()
                .Append(ExceptionStackTrace(ex))
                .AppendLine()

                Return .ToString()
            End With

        End Function

        Private Shared Function ApplicationInfo(ByVal parentAssembly As System.Reflection.Assembly) As String

            With New StringBuilder()
                Dim parentAssemblyInfo As New TVA.Assembly(parentAssembly)
                .AppendFormat("Application Domain:    {0}", System.AppDomain.CurrentDomain.FriendlyName)
                .AppendLine()
                .AppendFormat("Assembly Codebase:     {0}", parentAssemblyInfo.CodeBase)
                .AppendLine()
                .AppendFormat("Assembly Full Name:    {0}", parentAssemblyInfo.FullName)
                .AppendLine()
                .AppendFormat("Assembly Version:      {0}", parentAssemblyInfo.Version.ToString())
                .AppendLine()
                .AppendFormat("Assembly Build Date:   {0}", parentAssemblyInfo.BuildDate.ToString())
                .AppendLine()
                .AppendFormat(".Net Runtime Version:  {0}", System.Environment.Version.ToString())
                .AppendLine()

                Return .ToString()
            End With

        End Function

#End Region

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