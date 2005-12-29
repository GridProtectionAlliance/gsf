'*******************************************************************************************************
'  Tva.Assembly.vb - Assembly Information Class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/29/2005 - Pinal C Patel
'       Original version of source code generated
'  12/29/2005 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Assembly)
'
'*******************************************************************************************************

Imports System.IO
Imports System.Resources
Imports System.Reflection
Imports System.Reflection.Assembly
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Public Class Assembly

    Private m_assembly As System.Reflection.Assembly
    Private Shared m_callingAssembly As Assembly
    Private Shared m_entryAssembly As Assembly
    Private Shared m_executingAssembly As Assembly

    ''' <summary>
    ''' Initializes a new instance of Tva.Assembly for the specified System.Reflection.Assembly.  
    ''' </summary>
    ''' <param name="asm">An instance of System.Reflection.Assembly for which a Tva.Assembly instance is to be created.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal asm As System.Reflection.Assembly)
        m_assembly = asm
    End Sub

    ''' <summary>
    ''' Get the Tva.Assembly instance of the assembly that invoked the currently executing method.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The Tva.Assembly instance of the assembly that invoked the currently executing method.</returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property CallingAssembly() As Assembly
        Get
            If m_callingAssembly Is Nothing Then m_callingAssembly = New Assembly(GetCallingAssembly())
            Return m_callingAssembly
        End Get
    End Property

    ''' <summary>
    ''' Gets the Tva.Assembly instance of the process executable in the default application domain.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The Tva.Assembly instance of the process executable in the default application domain.</returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property EntryAssembly() As Assembly
        Get
            If m_entryAssembly Is Nothing Then m_entryAssembly = New Assembly(GetEntryAssembly())
            Return m_entryAssembly
        End Get
    End Property

    ''' <summary>
    ''' Gets the Tva.Assembly instance of the assembly that contains the code that is currently executing.
    ''' </summary>
    ''' <value></value>
    ''' <returns>the Tva.Assembly instance of the assembly that contains the code that is currently executing.</returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property ExecutingAssembly() As Assembly
        Get
            If m_executingAssembly Is Nothing Then m_executingAssembly = New Assembly(GetExecutingAssembly())
            Return m_executingAssembly
        End Get
    End Property

    ''' <summary>
    ''' Gets the title information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The title information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Title() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyTitleAttribute)), _
                AssemblyTitleAttribute).Title()
        End Get
    End Property

    ''' <summary>
    ''' Gets the description information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The description information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Description() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyDescriptionAttribute)), _
                AssemblyDescriptionAttribute).Description()
        End Get
    End Property

    ''' <summary>
    ''' Gets the company name information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The company name information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Company() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyCompanyAttribute)), _
                AssemblyCompanyAttribute).Company()
        End Get
    End Property

    ''' <summary>
    ''' Gets the product name information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The product name information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Product() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyProductAttribute)), _
                AssemblyProductAttribute).Product()
        End Get
    End Property

    ''' <summary>
    ''' Gets the copyright information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The copyright information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Copyright() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyCopyrightAttribute)), _
                AssemblyCopyrightAttribute).Copyright()
        End Get
    End Property

    ''' <summary>
    ''' Gets the trademark information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The trademark information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Trademark() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyTrademarkAttribute)), _
                AssemblyTrademarkAttribute).Trademark()
        End Get
    End Property

    ''' <summary>
    ''' Gets the configuration information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The configuration information of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Configuration() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyConfigurationAttribute)), _
                AssemblyConfigurationAttribute).Configuration()
        End Get
    End Property

    ''' <summary>
    ''' Gets a boolean value indicating if the assembly has been built as delay-signed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the assembly has been built as delay-signed; otherwise, False.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DelaySign() As Boolean
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyDelaySignAttribute)), _
                AssemblyDelaySignAttribute).DelaySign()
        End Get
    End Property

    ''' <summary>
    ''' Gets the version information of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The version information of the assembly</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property InformationalVersion() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyInformationalVersionAttribute)), _
                AssemblyInformationalVersionAttribute).InformationalVersion()
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the file containing the key pair used to generate a strong name for the attributed assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A string containing the name of the file that contains the key pair.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property KeyFile() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(AssemblyKeyFileAttribute)), _
                AssemblyKeyFileAttribute).KeyFile()
        End Get
    End Property

    ''' <summary>
    ''' Gets the culture name of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The culture name of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CultureName() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(NeutralResourcesLanguageAttribute)), _
                NeutralResourcesLanguageAttribute).CultureName()
        End Get
    End Property

    ''' <summary>
    ''' Gets the assembly version used to instruct the System.Resources.ResourceManager to ask for a particular version of a satellite assembly to simplify updates of the main assembly of an application.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property SatelliteContractVersion() As String
        Get
            Return DirectCast(GetCustomAttribute(GetType(SatelliteContractVersionAttribute)), _
                SatelliteContractVersionAttribute).Version()
        End Get
    End Property

    ''' <summary>
    ''' Gets the string representing the assembly version used to indicates to a COM client that all classes in the current version of the assembly are compatible with classes in an earlier version of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The string representing the assembly version in MajorVersion.MinorVersion.RevisionNumber.BuildNumber format.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ComCompatibleVersion() As String
        Get
            With DirectCast(GetCustomAttribute(GetType(ComCompatibleVersionAttribute)), ComCompatibleVersionAttribute)
                Return .MajorVersion().ToString() & "." & .MinorVersion().ToString() & "." _
                    & .RevisionNumber().ToString() & "." & .BuildNumber().ToString()
            End With
        End Get
    End Property

    ''' <summary>
    '''  Gets a boolean value indicating if the assembly is exposed to COM.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the assembly is exposed to COM; otherwise, False.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ComVisible() As Boolean
        Get
            'Returns the ComVisible attribute.
            Return DirectCast(GetCustomAttribute(GetType(ComVisibleAttribute)), ComVisibleAttribute).Value()
        End Get
    End Property

    ''' <summary>
    ''' Get the assembly GUID that is used as an ID if the assembly is exposed to COM.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The assembly GUID that is used as an ID if the assembly is exposed to COM.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Guid() As String
        Get
            'Returns the Guid attribute.
            Return DirectCast(GetCustomAttribute(GetType(GuidAttribute)), GuidAttribute).Value()
        End Get
    End Property

    ''' <summary>
    ''' Gets the string representing the assembly version number in MajorVersion.MinorVersion format.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The string representing the assembly version number in MajorVersion.MinorVersion format.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TypeLibVersion() As String
        Get
            'Returns the TypeLibVersion attribute.
            With DirectCast(GetCustomAttribute(GetType(TypeLibVersionAttribute)), TypeLibVersionAttribute)
                Return .MajorVersion().ToString() & "." & .MinorVersion().ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' Gets a boolean value indicating whether the indicated program element is CLS-compliant.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the program element is CLS-compliant; otherwise, False.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CLSCompliant() As Boolean
        Get
            'Returns the CLSCompliant attribute.
            Return DirectCast(GetCustomAttribute(GetType(CLSCompliantAttribute)), _
                CLSCompliantAttribute).IsCompliant()
        End Get
    End Property

    ''' <summary>
    ''' Gets a value that indicates whether the runtime will track information during code generation for the debugger.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the runtime will track information during code generation for the debugger; otherwise, False.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Debuggable() As Boolean
        Get
            'Returns the Debuggable attribute.
            Return DirectCast(GetCustomAttribute(GetType(DebuggableAttribute)), _
                DebuggableAttribute).IsJITTrackingEnabled()
        End Get
    End Property

    ''' <summary>
    ''' Gets the path or UNC location of the loaded file that contains the manifest.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The location of the loaded file that contains the manifest.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Location() As String
        Get
            'Returns the location of the assembly.
            Return m_assembly.Location().ToLower()
        End Get
    End Property

    ''' <summary>
    ''' Gets the location of the assembly as specified originally, for example, in an System.Reflection.AssemblyName object.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The location of the assembly as specified originally.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CodeBase() As String
        Get
            'Returns the location of the assembly in codebase format.
            Return m_assembly.CodeBase().Replace("file:///", "").ToLower()
        End Get
    End Property

    ''' <summary>
    ''' Gets the display name of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The display name of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FullName() As String
        Get
            'Returns the full name of the assembly.
            Return m_assembly.FullName()
        End Get
    End Property

    ''' <summary>
    ''' Gets the simple, unencrypted name of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A string that is the simple, unencrypted name of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            'Returns the name of the assembly.
            Return m_assembly.GetName().Name()
        End Get
    End Property

    ''' <summary>
    ''' Gets the major, minor, revision, and build numbers of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A System.Version object representing the major, minor, revision, and build numbers of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Version() As Version
        Get
            'Returns the Version attribute.
            Return m_assembly.GetName().Version()
        End Get
    End Property

    ''' <summary>
    ''' Gets the string representing the version of the common language runtime (CLR) saved in the file containing the manifest.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The string representing the CLR version folder name. This is not a full path.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ImageRuntimeVersion() As String
        Get
            'Returns the version of Common Language Runtime.
            Return m_assembly.ImageRuntimeVersion()
        End Get
    End Property

    ''' <summary>
    ''' Gets a boolean value indicating whether the assembly was loaded from the global assembly cache.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the assembly was loaded from the global assembly cache; otherwise, False.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property GACLoaded() As Boolean
        Get
            'Returns whether the assembly was loaded from the GAC.
            Return m_assembly.GlobalAssemblyCache()
        End Get
    End Property

    ''' <summary>
    ''' Gets the date and time when the assembly was last built.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The date and time when the assembly was last built.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property BuildDate() As Date
        Get
            'Returns the date and time when the assembly was last built.
            Return File.GetLastWriteTime(m_assembly.Location())
        End Get
    End Property

    ''' <summary>
    ''' Gets the root namespace of the assembly.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The root namespace of the assembly.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property RootNamespace() As String
        Get
            'Returns the root namespace of the assembly.
            Return m_assembly.GetExportedTypes(0).Namespace()
        End Get
    End Property

    ''' <summary>
    ''' Get a collection of assembly attributes exposed by the assembly.
    ''' </summary>
    ''' <returns>A System.Specialized.KeyValueCollection of assembly attributes.</returns>
    ''' <remarks></remarks>
    Public Function GetAttributes() As Specialized.NameValueCollection

        Dim assemblyAttributes As New Specialized.NameValueCollection()

        With assemblyAttributes
            'Add some values that are not in AssemblyInfo.
            .Add("Full Name", FullName())
            .Add("Name", Name())
            .Add("Version", Version().ToString())
            .Add("Image Runtime Version", ImageRuntimeVersion())
            .Add("Build Date", BuildDate().ToString())
            .Add("Location", Location())
            .Add("Code Base", CodeBase())
            .Add("GAC Loaded", GACLoaded().ToString())


            'Add all attributes available from AssemblyInfo.
            For Each assemblyAttribute As Object In m_assembly.GetCustomAttributes(False)
                If TypeOf assemblyAttribute Is AssemblyTitleAttribute Then
                    .Add("Title", Title())
                ElseIf TypeOf assemblyAttribute Is AssemblyDescriptionAttribute Then
                    .Add("Description", Description())
                ElseIf TypeOf assemblyAttribute Is AssemblyCompanyAttribute Then
                    .Add("Company", Company())
                ElseIf TypeOf assemblyAttribute Is AssemblyProductAttribute Then
                    .Add("Product", Product())
                ElseIf TypeOf assemblyAttribute Is AssemblyCopyrightAttribute Then
                    .Add("Copyright", Copyright())
                ElseIf TypeOf assemblyAttribute Is AssemblyTrademarkAttribute Then
                    .Add("Trademark", Trademark())
                ElseIf TypeOf assemblyAttribute Is AssemblyConfigurationAttribute Then
                    .Add("Configuration", Configuration())
                ElseIf TypeOf assemblyAttribute Is AssemblyDelaySignAttribute Then
                    .Add("Delay Sign", DelaySign().ToString())
                ElseIf TypeOf assemblyAttribute Is AssemblyInformationalVersionAttribute Then
                    .Add("Informational Version", InformationalVersion())
                ElseIf TypeOf assemblyAttribute Is AssemblyKeyFileAttribute Then
                    .Add("Key File", KeyFile())
                ElseIf TypeOf assemblyAttribute Is NeutralResourcesLanguageAttribute Then
                    assemblyAttributes.Add("Culture Name", CultureName())
                ElseIf TypeOf assemblyAttribute Is SatelliteContractVersionAttribute Then
                    .Add("Satellite Contract Version", SatelliteContractVersion())
                ElseIf TypeOf assemblyAttribute Is ComCompatibleVersionAttribute Then
                    .Add("Com Compatible Version", ComCompatibleVersion())
                ElseIf TypeOf assemblyAttribute Is ComVisibleAttribute Then
                    .Add("Com Visible", ComVisible().ToString())
                ElseIf TypeOf assemblyAttribute Is GuidAttribute Then
                    assemblyAttributes.Add("Guid", Guid())
                ElseIf TypeOf assemblyAttribute Is TypeLibVersionAttribute Then
                    .Add("Type Lib Version", TypeLibVersion())
                ElseIf TypeOf assemblyAttribute Is CLSCompliantAttribute Then
                    .Add("CLS Compliant", CLSCompliant().ToString())
                ElseIf TypeOf assemblyAttribute Is DebuggableAttribute Then
                    .Add("Debuggable", Debuggable().ToString())
                End If
            Next
        End With


        Return assemblyAttributes

    End Function

    ''' <summary>
    ''' Gets the specified assembly attribute if it is exposed by the assembly.
    ''' </summary>
    ''' <param name="attributeType">Type of the attribute to get.</param>
    ''' <returns>The assembly attribute.</returns>
    ''' <remarks></remarks>
    Public Function GetCustomAttribute(ByVal attributeType As Type) As Object

        'Returns the requested assembly attribute.
        Dim assemblyAttributes As Object() = m_assembly.GetCustomAttributes(attributeType, False)
        If assemblyAttributes.Length() >= 1 Then
            Return assemblyAttributes(0)
        Else
            Throw New ApplicationException("Assembly does not expose this attribute")
        End If

    End Function

    ''' <summary>
    ''' Get the specified embedded resource from the assembly.
    ''' </summary>
    ''' <param name="resourceName">Name of the embedded resource to get.</param>
    ''' <returns>The embedded resource.</returns>
    ''' <remarks></remarks>
    Public Function GetEmbeddedResource(ByVal resourceName As String) As Stream

        'Extract and return the requested embedded resource.
        Return m_assembly.GetManifestResourceStream(RootNamespace() & "." & resourceName)

    End Function

    ''' <summary>
    ''' Load the specified assembly that is embedded as a resource in the assembly.
    ''' </summary>
    ''' <param name="assemblyName">Name of the assembly to load.</param>
    ''' <remarks></remarks>
    Public Shared Sub LoadAssemblyFromResource(ByVal assemblyName As System.String)

        Static addedResolver As Boolean

        ' Hook into assembly resolve event for current domain so we can load assembly from embedded resource
        If Not addedResolver Then
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblyFromResource
            addedResolver = True
        End If

        ' Load the assembly (this will invoke event that will resolve assembly from resource)
        AppDomain.CurrentDomain.Load(assemblyName)

    End Sub

    Private Shared Function ResolveAssemblyFromResource(ByVal sender As Object, ByVal e As ResolveEventArgs) As System.Reflection.Assembly

        Static assemblyCache As New Hashtable
        Dim resourceAssembly As System.Reflection.Assembly
        Dim shortName As String = e.Name.Split(","c)(0)

        resourceAssembly = assemblyCache(shortName)

        If resourceAssembly Is Nothing Then
            ' Loop through all of the resources in the executing assembly
            For Each name As String In GetEntryAssembly.GetManifestResourceNames()
                ' See if the embedded resource name matches assembly we are trying to load
                If String.Compare(Path.GetFileNameWithoutExtension(name), EntryAssembly.RootNamespace() & "." & shortName, True) = 0 Then
                    ' If so, load embedded resource assembly into a binary buffer
                    With GetEntryAssembly.GetManifestResourceStream(name)
                        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), .Length)
                        .Read(buffer, 0, .Length)
                        .Close()

                        ' Load assembly from binary buffer
                        resourceAssembly = Load(buffer)
                        assemblyCache.Add(shortName, resourceAssembly)
                        Exit For
                    End With
                End If
            Next
        End If

        Return resourceAssembly

    End Function

End Class