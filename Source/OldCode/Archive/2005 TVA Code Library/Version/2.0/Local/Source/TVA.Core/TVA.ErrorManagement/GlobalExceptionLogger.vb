' PCP: 04/03/2007

Option Strict On

Imports System.Net
Imports System.Web
Imports System.Text
Imports System.Reflection
Imports System.Drawing
Imports System.ComponentModel
Imports System.Windows.Forms
Imports TVA.Net.Smtp
Imports TVA.IO.FilePath
Imports TVA.Identity

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

        Private m_autoRegister As Boolean
        Private m_logToUI As Boolean
        Private m_logToFile As Boolean
        Private m_logToEmail As Boolean
        Private m_logToEventLog As Boolean
        Private m_logToScreenshot As Boolean
        Private m_smtpServer As String
        Private m_contactName As String
        Private m_contactEmail As String
        Private m_contactPhone As String
        Private m_exitOnUnhandledException As Boolean
        Private m_parentAssembly As System.Reflection.Assembly
        Private m_errorTextMethod As UITextMethodSignature
        Private m_scopeTextMethod As UITextMethodSignature
        Private m_actionTextMethod As UITextMethodSignature
        Private m_moreInfoTextMethod As UITextMethodSignature
        Private m_loggers As List(Of LoggerMethodSignature)
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String

        Private m_logToFileOK As Boolean
        Private m_logToEmailOK As Boolean
        Private m_logToPhoneOK As Boolean
        Private m_logToEventLogOK As Boolean
        Private m_logToScreenshotOK As Boolean
        Private m_lastException As Exception
        Private m_applicationType As ApplicationType

#End Region

#Region " Code Scope: Public "

        Public Delegate Function UITextMethodSignature() As String
        Public Delegate Sub LoggerMethodSignature(ByVal ex As Exception)

        <Category("Behavior")> _
        Public Property AutoRegister() As Boolean
            Get
                Return m_autoRegister
            End Get
            Set(ByVal value As Boolean)
                m_autoRegister = value
            End Set
        End Property

        <Category("Logging")> _
        Public Property LogToUI() As Boolean
            Get
                Return m_logToUI
            End Get
            Set(ByVal value As Boolean)
                m_logToUI = value
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

        Public Property SmtpServer() As String
            Get
                Return m_smtpServer
            End Get
            Set(ByVal value As String)
                m_smtpServer = value
            End Set
        End Property

        Public Property ContactName() As String
            Get
                Return m_contactName
            End Get
            Set(ByVal value As String)
                m_contactName = value
            End Set
        End Property

        Public Property ContactEmail() As String
            Get
                Return m_contactEmail
            End Get
            Set(ByVal value As String)
                m_contactEmail = value
            End Set
        End Property

        Public Property ContactPhone() As String
            Get
                Return m_contactPhone
            End Get
            Set(ByVal value As String)
                m_contactPhone = value
            End Set
        End Property

        Public Property ExitOnUnhandledException() As Boolean
            Get
                Return m_exitOnUnhandledException
            End Get
            Set(ByVal value As Boolean)
                m_exitOnUnhandledException = value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property ParentAssembly() As System.Reflection.Assembly
            Get
                Return m_parentAssembly
            End Get
            Set(ByVal value As System.Reflection.Assembly)
                m_parentAssembly = value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property ErrorTextMethod() As UITextMethodSignature
            Get
                Return m_errorTextMethod
            End Get
            Set(ByVal value As UITextMethodSignature)
                m_errorTextMethod = value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property ScopeTextMethod() As UITextMethodSignature
            Get
                Return m_scopeTextMethod
            End Get
            Set(ByVal value As UITextMethodSignature)
                m_scopeTextMethod = value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property ActionTextMethod() As UITextMethodSignature
            Get
                Return m_actionTextMethod
            End Get
            Set(ByVal value As UITextMethodSignature)
                m_actionTextMethod = value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property MoreInfoTextMethod() As UITextMethodSignature
            Get
                Return m_moreInfoTextMethod
            End Get
            Set(ByVal value As UITextMethodSignature)
                m_moreInfoTextMethod = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ApplicationName() As String
            Get
                Select Case ApplicationType
                    Case TVA.ApplicationType.WindowsCui, TVA.ApplicationType.WindowsGui
                        Return NoFileExtension(AppDomain.CurrentDomain.FriendlyName)
                    Case TVA.ApplicationType.Web
                        Return HttpContext.Current.Request.ApplicationPath.Replace("/", "")
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ApplicationType() As ApplicationType
            Get
                If m_applicationType = TVA.ApplicationType.Unknown Then
                    m_applicationType = TVA.Common.GetApplicationType()
                End If
                Return m_applicationType
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property LastException() As Exception
            Get
                Return m_lastException
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Loggers() As List(Of LoggerMethodSignature)
            Get
                Return m_loggers
            End Get
        End Property

        Public Sub Register()

            If Not Debugger.IsAttached Then
                ' For winform applications. 
                AddHandler System.Windows.Forms.Application.ThreadException, AddressOf UnhandledThreadException

                ' For console applications.
                AddHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledException
            End If

        End Sub

        Public Sub Unregister()

            If Not Debugger.IsAttached Then
                RemoveHandler System.Windows.Forms.Application.ThreadException, AddressOf UnhandledThreadException
                RemoveHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledException
            End If

        End Sub

        Public Sub Log(ByVal ex As Exception)

            Log(ex, False)

        End Sub

        Public Sub Log(ByVal ex As Exception, ByVal exitApplication As Boolean)

            HandleException(ex, exitApplication)

        End Sub

#Region " Shared "

        Public Shared Function ExceptionToString(ByVal ex As Exception) As String

            Dim parentAssembly As System.Reflection.Assembly
            Select Case TVA.Common.GetApplicationType()
                Case TVA.ApplicationType.WindowsCui, ApplicationType.WindowsGui
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
                    Case TVA.ApplicationType.WindowsCui, ApplicationType.WindowsGui
                        Dim currentUserInfo As New UserInfo(System.Threading.Thread.CurrentPrincipal.Identity.Name)
                        .AppendFormat("Machine Name:          {0}", Environment.MachineName)
                        .AppendLine()
                        .AppendFormat("Machine IP:            {0}", Dns.GetHostEntry(Environment.MachineName).AddressList(0).ToString())
                        .AppendLine()
                        .AppendFormat("Current User ID:       {0}", currentUserInfo.LoginID)
                        .AppendLine()
                        .AppendFormat("Current User Name:     {0}", currentUserInfo.FullName)
                        .AppendLine()
                        .AppendFormat("Current User Phone:    {0}", currentUserInfo.Telephone)
                        .AppendLine()
                        .AppendFormat("Current User Email:    {0}", currentUserInfo.Email)
                        .AppendLine()
                    Case ApplicationType.Web
                        Dim remoteUserInfo As New UserInfo(System.Threading.Thread.CurrentPrincipal.Identity.Name, True)
                        .AppendFormat("Server Name:           {0}", Environment.MachineName)
                        .AppendLine()
                        .AppendFormat("Server IP:             {0}", Dns.GetHostEntry(Environment.MachineName).AddressList(0).ToString())
                        .AppendLine()
                        .AppendFormat("Process User:          {0}", System.Security.Principal.WindowsIdentity.GetCurrent().Name)
                        .AppendLine()
                        .AppendFormat("Remote User ID:        {0}", remoteUserInfo.LoginID)
                        .AppendLine()
                        .AppendFormat("Remote User Name:      {0}", remoteUserInfo.FullName)
                        .AppendLine()
                        .AppendFormat("Remote User Phone:     {0}", remoteUserInfo.Telephone)
                        .AppendLine()
                        .AppendFormat("Remote User Email:     {0}", remoteUserInfo.Email)
                        .AppendLine()
                        .AppendFormat("Remote Host:           {0}", HttpContext.Current.Request.ServerVariables("REMOTE_HOST"))
                        .AppendLine()
                        .AppendFormat("Remote Address:        {0}", HttpContext.Current.Request.ServerVariables("REMOTE_ADDR"))
                        .AppendLine()
                        .AppendFormat("HTTP Agent:            {0}", HttpContext.Current.Request.ServerVariables("HTTP_USER_AGENT"))
                        .AppendLine()
                        .AppendFormat("HTTP Referer:          {0}", HttpContext.Current.Request.ServerVariables("HTTP_REFERER"))
                        .AppendLine()
                        .AppendFormat("Web Page URL:          {0}", HttpContext.Current.Request.Url.ToString())
                        .AppendLine()
                End Select

                Return .ToString()
            End With

        End Function

        Public Shared Function ApplicationInfo() As String

            Dim parentAssembly As System.Reflection.Assembly
            Select Case TVA.Common.GetApplicationType()
                Case TVA.ApplicationType.WindowsCui, ApplicationType.WindowsGui
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
                    Dim method As MemberInfo = stackFrame.GetMethod()
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
                        If appType = ApplicationType.WindowsCui OrElse appType = TVA.ApplicationType.WindowsGui Then
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

            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    If .Count > 0 Then
                        AutoRegister = .Item("AutoRegister").GetTypedValue(m_autoRegister)
                        LogToUI = .Item("LogToUI").GetTypedValue(m_logToUI)
                        LogToFile = .Item("LogToFile").GetTypedValue(m_logToFile)
                        LogToEmail = .Item("LogToEmail").GetTypedValue(m_logToEmail)
                        LogToEventLog = .Item("LogToEventLog").GetTypedValue(m_logToEventLog)
                        LogToScreenshot = .Item("LogToScreenshot").GetTypedValue(m_logToScreenshot)
                        SmtpServer = .Item("EmailServer").GetTypedValue(m_smtpServer)
                        ContactEmail = .Item("EmailRecipients").GetTypedValue(m_contactEmail)
                        ContactName = .Item("ContactPersonName").GetTypedValue(m_contactName)
                        ContactPhone = .Item("ContactPersonPhone").GetTypedValue(m_contactPhone)
                    End If
                End With
            Catch ex As Exception
                ' We'll encounter exceptions if the settings are not present in the config file.
            End Try

        End Sub

        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                        .Clear()
                        With .Item("AutoRegister", True)
                            .Value = m_autoRegister.ToString()
                            .Description = "True if the logger is to be automatically registered for handling unhandled exceptions after initialization is complete; otherwise False."
                        End With
                        With .Item("LogToUI", True)
                            .Value = m_logToUI.ToString()
                            .Description = "True if an encountered exception is to be logged to the User Interface; otherwise False."
                        End With
                        With .Item("LogToFile", True)
                            .Value = m_logToFile.ToString()
                            .Description = "True if an encountered exception is to be logged to a file; otherwise False."
                        End With
                        With .Item("LogToEmail", True)
                            .Value = m_logToEmail.ToString()
                            .Description = "True if an email is to be sent to ContactEmail with the details of an encountered exception; otherwise False."
                        End With
                        With .Item("LogToEventLog", True)
                            .Value = m_logToEventLog.ToString()
                            .Description = "True if an encountered exception is to be logged to the Event Log; otherwise False."
                        End With
                        With .Item("LogToScreenshot", True)
                            .Value = m_logToScreenshot.ToString()
                            .Description = "True if a screenshot is to be taken when an exception is encountered; otherwise False."
                        End With
                        With .Item("SmtpServer", True)
                            .Value = m_smtpServer
                            .Description = "Name of the SMTP server to be used for sending the email message."
                        End With
                        With .Item("ContactName", True)
                            .Value = m_contactName
                            .Description = "Name of the person that the end-user can contact when an exception is encountered."
                        End With
                        With .Item("ContactEmail", True)
                            .Value = m_contactEmail
                            .Description = "Comma-seperated list of recipient email addresses for the email message."
                        End With
                        With .Item("ContactPhone", True)
                            .Value = m_contactPhone
                            .Description = "Phone number of the person that the end-user can contact when an exception is encountered."
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
                If m_autoRegister Then Register() ' Start the logger automatically if specified.
                m_parentAssembly = System.Reflection.Assembly.GetCallingAssembly()
            End If

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

        Private ReadOnly Property LogFileName() As String
            Get
                Return AbsolutePath(ApplicationName & ".ExceptionLog.txt")
            End Get
        End Property

        Private ReadOnly Property ScreenshotFileName() As String
            Get
                Return AbsolutePath(ApplicationName & ".ExceptionScreenshot.png")
            End Get
        End Property

        Private Sub HandleException(ByVal ex As Exception, ByVal exitApplication As Boolean)

            m_lastException = ex

            Try
                If Not m_loggers.Contains(AddressOf ExceptionToScreenshot) Then
                    m_loggers.Add(AddressOf ExceptionToScreenshot)
                End If
                If Not m_loggers.Contains(AddressOf ExceptionToEventLog) Then
                    m_loggers.Add(AddressOf ExceptionToEventLog)
                End If
                If Not m_loggers.Contains(AddressOf ExceptionToEmail) Then
                    m_loggers.Add(AddressOf ExceptionToEmail)
                End If
                If Not m_loggers.Contains(AddressOf ExceptionToFile) Then
                    m_loggers.Add(AddressOf ExceptionToFile)
                End If
                If Not m_loggers.Contains(AddressOf ExceptionToUI) Then
                    m_loggers.Add(AddressOf ExceptionToUI)
                End If
            Catch

            End Try

            For Each logger As LoggerMethodSignature In m_loggers
                Try
                    logger.Invoke(ex)
                Catch

                End Try
            Next

            If exitApplication AndAlso _
                    (ApplicationType = TVA.ApplicationType.WindowsCui OrElse ApplicationType = TVA.ApplicationType.WindowsGui) Then
                Application.Exit()
                System.Diagnostics.Process.GetCurrentProcess().Kill()
            End If

        End Sub

        Private Sub ExceptionToUI(ByVal exception As Exception)

            If m_logToUI Then
                Try
                    Select Case ApplicationType
                        Case TVA.ApplicationType.WindowsCui
                            ExceptionToWindowsCui()
                        Case TVA.ApplicationType.WindowsGui
                            ExceptionToWindowsGui()
                        Case TVA.ApplicationType.Web
                            ExceptionToWebPage()
                    End Select
                Catch ex As Exception

                End Try
            End If

        End Sub

        Private Sub ExceptionToWindowsGui()

            With New GelDialog()
                .Text = String.Format(.Text, ApplicationName)
                .PictureBoxIcon.Image = System.Drawing.SystemIcons.Error.ToBitmap()
                .RichTextBoxError.Text = m_errorTextMethod()
                .RichTextBoxScope.Text = m_scopeTextMethod()
                .RichTextBoxAction.Text = m_actionTextMethod()
                .RichTextBoxMoreInfo.Text = m_moreInfoTextMethod()

                .ShowDialog()
            End With

        End Sub

        Private Sub ExceptionToWindowsCui()

            With New StringBuilder()
                .AppendFormat("{0} has encountered a problem", ApplicationName)
                .AppendLine()
                .AppendLine()
                .Append("What happened:")
                .AppendLine()
                .Append(m_errorTextMethod())
                .AppendLine()
                .AppendLine()
                .Append("How this will affect you:")
                .AppendLine()
                .Append(m_scopeTextMethod())
                .AppendLine()
                .AppendLine()
                .Append("What you can do about it:")
                .AppendLine()
                .Append(m_actionTextMethod())
                .AppendLine()
                .AppendLine()
                .Append("More information:")
                .AppendLine()
                .Append(m_moreInfoTextMethod())
                .AppendLine()

                System.Console.Write(.ToString())
            End With

        End Sub

        Private Sub ExceptionToWebPage()

            With New StringBuilder()
                .Append("<HTML>")
                .AppendLine()
                .Append("<HEAD>")
                .AppendLine()
                .Append("<TITLE>")
                .AppendLine()
                .AppendFormat("{0} has encountered a problem", ApplicationName)
                .AppendLine()
                .Append("</TITLE>")
                .AppendLine()
                .Append("<STYLE>")
                .AppendLine()
                .Append("body {font-family:""Verdana"";font-weight:normal;font-size: .7em;color:black; background-color:white;}")
                .AppendLine()
                .Append("b {font-family:""Verdana"";font-weight:bold;color:black;margin-top: -5px}")
                .AppendLine()
                .Append("H1 { font-family:""Verdana"";font-weight:normal;font-size:18pt;color:red }")
                .AppendLine()
                .Append("H2 { font-family:""Verdana"";font-weight:normal;font-size:14pt;color:maroon }")
                .AppendLine()
                .Append("pre {font-family:""Lucida Console"";font-size: .9em}")
                .AppendLine()
                .Append("</STYLE>")
                .AppendLine()
                .Append("</HEAD>")
                .AppendLine()
                .Append("<BODY>")
                .AppendLine()
                .Append("<H1>")
                .AppendLine()
                .AppendFormat("The {0} website has encountered a problem", ApplicationName)
                .AppendLine()
                .Append("<hr width=100% size=1 color=silver></H1>")
                .AppendLine()
                .Append("<H2>What Happened</H2>")
                .AppendLine()
                .Append("<BLOCKQUOTE>")
                .AppendLine()
                .Append(m_errorTextMethod())
                .AppendLine()
                .Append("</BLOCKQUOTE>")
                .AppendLine()
                .Append("<H2>How this will affect you</H2>")
                .AppendLine()
                .Append("<BLOCKQUOTE>")
                .AppendLine()
                .Append(m_scopeTextMethod())
                .AppendLine()
                .Append("</BLOCKQUOTE>")
                .AppendLine()
                .Append("<H2>What you can do about it</H2>")
                .AppendLine()
                .Append("<BLOCKQUOTE>")
                .AppendLine()
                .Append(m_actionTextMethod())
                .AppendLine()
                .Append("</BLOCKQUOTE>")
                .AppendLine()
                .Append("<INPUT type=button value=""More Information &gt;&gt;"" onclick=""this.style.display='none'; document.getElementById('MoreInfo').style.display='block'"">")
                .AppendLine()
                .Append("<DIV style='display:none;' id='MoreInfo'>")
                .AppendLine()
                .Append("<H2>More information</H2>")
                .AppendLine()
                .Append("<TABLE width=""100%"" bgcolor=""#ffffcc"">")
                .AppendLine()
                .Append("<TR><TD>")
                .AppendLine()
                .Append("<CODE><PRE>")
                .AppendLine()
                .Append(m_moreInfoTextMethod())
                .AppendLine()
                .Append("</PRE></CODE>")
                .AppendLine()
                .Append("<TD><TR>")
                .AppendLine()
                .Append("</DIV>")
                .AppendLine()
                .Append("</BODY>")
                .AppendLine()
                .Append("</HTML>")
                .AppendLine()

                HttpContext.Current.Response.Write(.ToString())
                HttpContext.Current.Response.Flush()
                HttpContext.Current.Response.End()
                HttpContext.Current.Server.ClearError()
            End With

        End Sub

        Private Sub ExceptionToFile(ByVal exception As Exception)

            If m_logToFile Then
                Try
                    m_logToFileOK = False

                    LogFile.Name = LogFileName
                    If Not LogFile.IsOpen Then LogFile.Open()
                    LogFile.WriteTimestampedLine(ExceptionToString(exception, m_parentAssembly))

                    m_logToFileOK = True
                Catch ex As Exception

                Finally
                    LogFile.Close()
                End Try
            End If

        End Sub

        Private Sub ExceptionToEmail(ByVal exception As Exception)

            If m_logToEmail AndAlso Not String.IsNullOrEmpty(m_contactEmail) Then
                Try
                    m_logToEmailOK = False

                    With New SimpleMailMessage()
                        .Sender = String.Format("{0}@tva.gov", Environment.MachineName)
                        .Recipients = m_contactEmail
                        .Subject = String.Format("Exception in {0} at {1}", ApplicationName, System.DateTime.Now.ToString())
                        .Body = ExceptionToString(exception, m_parentAssembly)
                        .Attachments = AbsolutePath(ScreenshotFileName)
                        .MailServer = m_smtpServer
                        .Send()
                    End With

                    m_logToEmailOK = True
                Catch ex As Exception

                End Try
            End If

        End Sub

        Private Sub ExceptionToEventLog(ByVal exception As Exception)

            If m_logToEventLog Then
                Try
                    m_logToEventLogOK = False

                    ' Write the formatted exception message to the event log.
                    EventLog.WriteEntry(ApplicationName, ExceptionToString(exception, m_parentAssembly), EventLogEntryType.Error)

                    m_logToEventLogOK = True
                Catch ex As Exception

                End Try
            End If

        End Sub

        Private Sub ExceptionToScreenshot(ByVal exception As Exception)

            If m_logToScreenshot AndAlso _
                    (ApplicationType = ApplicationType.WindowsCui OrElse ApplicationType = ApplicationType.WindowsGui) Then
                Try
                    m_logToScreenshotOK = False

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

                    m_logToScreenshotOK = True
                Catch ex As Exception

                End Try
            End If

        End Sub

#Region " UI Text Delegates "

        Private Function GetErrorText() As String

            With New StringBuilder()
                .AppendFormat("An unexpected exception has occurred in {0}. ", ApplicationName)
                .Append("This may be due to an inconsistent system state or a programming error.")

                Return .ToString()
            End With

        End Function

        Private Function GetScopeText() As String

            With New StringBuilder()
                Select Case ApplicationType
                    Case TVA.ApplicationType.WindowsCui, TVA.ApplicationType.WindowsGui
                        .Append("The action you requested was not performed.")
                    Case TVA.ApplicationType.Web
                        .Append("The current page will not load.")
                End Select

                Return .ToString()
            End With

        End Function

        Private Function GetActionText() As String

            With New StringBuilder()
                Select Case ApplicationType
                    Case TVA.ApplicationType.WindowsCui, TVA.ApplicationType.WindowsGui
                        .AppendFormat("Restart {0}, and try repeating your last action. ", ApplicationName)
                    Case TVA.ApplicationType.Web
                        .AppendFormat("Close your browser, navigate back to the {0} website, and try repeating you last action. ", ApplicationName)
                End Select
                .Append("Try alternative methods of performing the same action. ")
                If Not String.IsNullOrEmpty(m_contactName) AndAlso _
                        (Not String.IsNullOrEmpty(m_contactPhone) OrElse Not String.IsNullOrEmpty(m_contactPhone)) Then
                    .AppendFormat("If you need immediate assistance, contact {0} ", m_contactName)
                    If Not String.IsNullOrEmpty(m_contactEmail) Then
                        .AppendFormat("via email at {0}", m_contactEmail)
                        If Not String.IsNullOrEmpty(m_contactPhone) Then
                            .Append(" or ")
                        End If
                    End If
                    If Not String.IsNullOrEmpty(m_contactPhone) Then
                        .AppendFormat("via phone at {0}", m_contactPhone)
                    End If
                    .Append(".")
                End If

                Return .ToString()
            End With

        End Function

        Private Function GetMoreInfoText() As String

            Dim bullet As String
            Select Case ApplicationType
                Case TVA.ApplicationType.WindowsCui
                    bullet = "-"
                Case TVA.ApplicationType.Web, TVA.ApplicationType.WindowsGui
                    bullet = "•"
            End Select

            With New StringBuilder()
                .Append("The following information about the error was automatically captured:")
                .AppendLine()
                .AppendLine()
                If m_logToScreenshot Then
                    .AppendFormat(" {0} ", bullet)
                    If m_logToScreenshotOK Then
                        .Append("a screenshot was taken of the desktop at:")
                        .AppendLine()
                        .Append("   ")
                        .Append(ScreenshotFileName)
                    Else
                        .Append("a screenshot could NOT be taken of the desktop.")
                    End If
                    .AppendLine()
                End If
                If m_logToEventLog Then
                    .AppendFormat(" {0} ", bullet)
                    If m_logToEventLogOK Then
                        .Append("an event was written to the application log")
                    Else
                        .Append("an event could NOT be written to the application log")
                    End If
                    .AppendLine()
                End If
                If m_logToFile Then
                    .AppendFormat(" {0} ", bullet)
                    If m_logToFileOK Then
                        .Append("details were written to a text log at:")
                    Else
                        .Append("details could NOT be written to the text log at:")
                    End If
                    .AppendLine()
                    .Append("   ")
                    .Append(LogFileName)
                    .AppendLine()
                End If
                If m_logToEmail Then
                    .AppendFormat(" {0} ", bullet)
                    If m_logToEmailOK Then
                        .Append("an email has been sent to:")
                    Else
                        .Append("an email could NOT be sent to:")
                    End If
                    .AppendLine()
                    .Append("   ")
                    .Append(m_contactEmail)
                    .AppendLine()
                End If
                .AppendLine()
                .AppendLine()
                .Append("Detailed error information follows:")
                .AppendLine()
                .AppendLine()
                .Append(ExceptionToString(m_lastException, m_parentAssembly))

                Return .ToString()
            End With

        End Function

#End Region

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
                .AppendFormat(".Net Runtime Version:  {0}", Environment.Version.ToString())
                .AppendLine()

                Return .ToString()
            End With

        End Function

#End Region

#Region " Event Handlers "

        Private Sub UnhandledThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)

            HandleException(e.Exception, m_exitOnUnhandledException)

        End Sub

        Private Sub UnhandledException(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)

            HandleException(CType(e.ExceptionObject, Exception), m_exitOnUnhandledException)

        End Sub

#End Region

#End Region

    End Class

End Namespace