' 04/03/2007

Imports System.Text
Imports System.Reflection
Imports System.Drawing
Imports System.ComponentModel
Imports System.Diagnostics
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
        Private m_configurationCategory As String
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
                AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledExceptionHandler
            End If

        End Sub

        Public Sub [Stop]()

            If Not Debugger.IsAttached Then
                RemoveHandler System.Windows.Forms.Application.ThreadException, AddressOf UnhandledThreadException
                RemoveHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf UnhandledExceptionHandler
            End If

        End Sub

        Public Sub HandleException(ByVal ex As Exception)

        End Sub

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
        Public Property ConfigurationCategory() As String Implements IPersistSettings.ConfigurationCategory
            Get
                Return m_configurationCategory
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_configurationCategory = value
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

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

        Private Sub UnhandledThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)

            GenericExceptionHandler(e.Exception)

        End Sub

        Private Sub UnhandledExceptionHandler(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)

            GenericExceptionHandler(CType(e.ExceptionObject, Exception))

        End Sub

        Private Sub GenericExceptionHandler(ByVal ex As Exception)

            Dim exceptionString As String = ExceptionToString(ex)

            If m_logToScreenshot Then

            End If

            If m_logToEventLog Then
                EventLog.WriteEntry(System.AppDomain.CurrentDomain.FriendlyName, exceptionString, EventLogEntryType.Error)
            End If

            If m_logToEmail Then

            End If

            If m_logToFile Then

            End If

        End Sub

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
                .AppendFormat("Assembly Version:      {0}", TVA.Assembly.EntryAssembly.ToString)
                .AppendLine()
                .AppendFormat("Assembly Build Date:   {0}", TVA.Assembly.EntryAssembly.BuildDate.ToString())
                .AppendLine()
                .AppendLine()

                ' get exception-specific information
                .AppendFormat("Exception Source:      {0}", ex.Source)
                .AppendLine()
                .AppendFormat("Exception Type:        {0}", ex.GetType().FullName)
                .AppendLine()
                .AppendFormat("Exception Message:     {0}", ex.Message)
                .AppendLine()
                .AppendFormat("Exception Target Site: {0}", ex.TargetSite.Name)
                .AppendLine()
                .AppendLine()

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

    End Class

End Namespace