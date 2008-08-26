Imports System.IO
Imports System.Reflection

Namespace My

    ' The following events are availble for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup

            m_assemblyCache = New Dictionary(Of String, Assembly)
            LoadAssemblyFromResource("TVA.Core")

        End Sub

#Region " Load Embedded Assembly "

        Private m_addedResolver As Boolean
        Private m_assemblyCache As Dictionary(Of String, Assembly)

        Public Sub LoadAssemblyFromResource(ByVal assemblyName As String)

            ' Hook into assembly resolve event for current domain so we can load assembly from embedded resource
            If Not m_addedResolver Then
                AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblyFromResource
                m_addedResolver = True
            End If

            ' Load the assembly (this will invoke event that will resolve assembly from resource)
            AppDomain.CurrentDomain.Load(assemblyName)

        End Sub

        Private Function ResolveAssemblyFromResource(ByVal sender As Object, ByVal e As ResolveEventArgs) As System.Reflection.Assembly

            'LogMessage("Resolving assembly...")

            Dim resourceAssembly As System.Reflection.Assembly = Nothing
            Dim shortName As String = e.Name.Split(","c)(0)

            If Not m_assemblyCache.TryGetValue(shortName, resourceAssembly) Then
                ' Loop through all of the resources in the executing assembly
                For Each name As String In Assembly.GetEntryAssembly.GetManifestResourceNames()
                    ' See if the embedded resource name matches assembly we are trying to load
                    If String.Compare(Path.GetFileNameWithoutExtension(name), Me.GetType().Assembly.GetExportedTypes(0).Namespace & "." & shortName, True) = 0 Then
                        ' If so, load embedded resource assembly into a binary buffer
                        With Assembly.GetEntryAssembly.GetManifestResourceStream(name)
                            Dim length As Integer = CInt(.Length)
                            Dim buffer(length - 1) As Byte
                            .Read(buffer, 0, length)
                            .Close()

                            ' Load assembly from binary buffer
                            resourceAssembly = Assembly.Load(buffer)
                            m_assemblyCache.Add(shortName, resourceAssembly)
                            Exit For
                        End With
                    End If
                Next
            End If

            Return resourceAssembly

        End Function

#End Region

    End Class

End Namespace

