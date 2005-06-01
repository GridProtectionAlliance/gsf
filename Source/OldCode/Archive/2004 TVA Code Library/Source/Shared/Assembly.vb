'Author: Pinal Patel
'Created: 04/29/05
'Modified: 06/01/05
'Description: This class reads the assembly attributes from an AssemblyInfo.vb file.



'Namespaces used.
Imports System.Text.RegularExpressions

Namespace [Shared]

    Public Class [Assembly]

        Private m_Assembly As System.Reflection.Assembly
        Private Shared m_EntryAssembly As [Assembly]

        Public Sub New(ByVal Asm As Reflection.Assembly)
            'Set the assembly.
            m_Assembly = Asm
        End Sub

        Public Shared ReadOnly Property EntryAssembly() As [Assembly]
            Get
                If m_EntryAssembly Is Nothing Then m_EntryAssembly = New [Assembly](System.Reflection.Assembly.GetEntryAssembly())
                Return m_EntryAssembly
            End Get
        End Property

        Public ReadOnly Property Title() As String
            Get
                'Returns the Title attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyTitleAttribute)), System.Reflection.AssemblyTitleAttribute).Title.ToString()
            End Get
        End Property

        Public ReadOnly Property Description() As String
            Get
                'Returns the Description attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyDescriptionAttribute)), System.Reflection.AssemblyDescriptionAttribute).Description.ToString()
            End Get
        End Property

        Public ReadOnly Property Company() As String
            Get
                'Returns the Company attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyCompanyAttribute)), System.Reflection.AssemblyCompanyAttribute).Company.ToString()
            End Get
        End Property

        Public ReadOnly Property Product() As String
            Get
                'Returns the Product attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyProductAttribute)), System.Reflection.AssemblyProductAttribute).Product.ToString()
            End Get
        End Property

        Public ReadOnly Property Copyright() As String
            Get
                'Returns the Copyright attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyCopyrightAttribute)), System.Reflection.AssemblyCopyrightAttribute).Copyright.ToString()
            End Get
        End Property

        Public ReadOnly Property Trademark() As String
            Get
                'Returns the Trademark attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyTrademarkAttribute)), System.Reflection.AssemblyTrademarkAttribute).Trademark.ToString()
            End Get
        End Property

        Public ReadOnly Property Configuration() As String
            Get
                'Returns the Configuration attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyConfigurationAttribute)), System.Reflection.AssemblyConfigurationAttribute).Configuration()
            End Get
        End Property

        Public ReadOnly Property DelaySign() As Boolean
            Get
                'Returns the DelaySign attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyDelaySignAttribute)), System.Reflection.AssemblyDelaySignAttribute).DelaySign()
            End Get
        End Property

        Public ReadOnly Property InformationalVersion() As String
            Get
                'Returns the InformationalVersion attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyInformationalVersionAttribute)), System.Reflection.AssemblyInformationalVersionAttribute).InformationalVersion()
            End Get
        End Property

        Public ReadOnly Property KeyFile() As String
            Get
                'Returns the KeyFile attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Reflection.AssemblyKeyFileAttribute)), System.Reflection.AssemblyKeyFileAttribute).KeyFile()
            End Get
        End Property

        Public ReadOnly Property NeutralResourcesLanguage() As String
            Get
                'Returns the NeutralResourcesLanguage attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Resources.NeutralResourcesLanguageAttribute)), System.Resources.NeutralResourcesLanguageAttribute).CultureName()
            End Get
        End Property

        Public ReadOnly Property SatelliteContractVersion() As String
            Get
                'Returns the SatelliteContractVersionAttribute attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Resources.SatelliteContractVersionAttribute)), System.Resources.SatelliteContractVersionAttribute).Version()
            End Get
        End Property

        Public ReadOnly Property ComCompatibleVersion() As String
            Get
                'Returns the ComCompatibleVersion attribute.
                Dim oComCompatibleVersionAttribute As System.Runtime.InteropServices.ComCompatibleVersionAttribute = DirectCast(GetCustomAttribute(GetType(System.Runtime.InteropServices.ComCompatibleVersionAttribute)), System.Runtime.InteropServices.ComCompatibleVersionAttribute)
                Return oComCompatibleVersionAttribute.MajorVersion().ToString() & "." & oComCompatibleVersionAttribute.MinorVersion().ToString() & "." & oComCompatibleVersionAttribute.RevisionNumber().ToString() & "." & oComCompatibleVersionAttribute.BuildNumber().ToString()
            End Get
        End Property

        Public ReadOnly Property ComVisible() As Boolean
            Get
                'Returns the ComVisible attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Runtime.InteropServices.ComVisibleAttribute)), System.Runtime.InteropServices.ComVisibleAttribute).Value()
            End Get
        End Property

        Public ReadOnly Property Guid() As String
            Get
                'Returns the Guid attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Runtime.InteropServices.GuidAttribute)), System.Runtime.InteropServices.GuidAttribute).Value()
            End Get
        End Property

        Public ReadOnly Property TypeLibVersion() As String
            Get
                'Returns the TypeLibVersion attribute.
                Dim oTypeLibVersionAttribute As System.Runtime.InteropServices.TypeLibVersionAttribute = DirectCast(GetCustomAttribute(GetType(System.Runtime.InteropServices.TypeLibVersionAttribute)), System.Runtime.InteropServices.TypeLibVersionAttribute)
                Return oTypeLibVersionAttribute.MajorVersion().ToString() & "." & oTypeLibVersionAttribute.MinorVersion().ToString()
            End Get
        End Property

        Public ReadOnly Property CLSCompliant() As Boolean
            Get
                'Returns the CLSCompliant attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.CLSCompliantAttribute)), System.CLSCompliantAttribute).IsCompliant()
            End Get
        End Property

        Public ReadOnly Property Debuggable() As Boolean
            Get
                'Returns the Debuggable attribute.
                Return DirectCast(GetCustomAttribute(GetType(System.Diagnostics.DebuggableAttribute)), System.Diagnostics.DebuggableAttribute).IsJITTrackingEnabled()
            End Get
        End Property

        Public ReadOnly Property Location() As String
            Get
                'Returns the location of the assembly.
                Return m_Assembly.Location().ToLower()
            End Get
        End Property

        Public ReadOnly Property CodeBase() As String
            Get
                'Returns the location of the assembly in codebase format.
                Return m_Assembly.CodeBase().Replace("file:///", "").ToLower()
            End Get
        End Property

        Public ReadOnly Property FullName() As String
            Get
                'Returns the full name of the assembly.
                Return m_Assembly.FullName()
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                'Returns the name of the assembly.
                Return m_Assembly.GetName().Name()
            End Get
        End Property

        Public ReadOnly Property Version() As Version
            Get
                'Returns the Version attribute.
                Return m_Assembly.GetName().Version()
            End Get
        End Property

        Public ReadOnly Property ImageRuntimeVersion() As String
            Get
                'Returns the version of Common Language Runtime.
                Return m_Assembly.ImageRuntimeVersion()
            End Get
        End Property

        Public ReadOnly Property GACLoaded() As Boolean
            Get
                'Returns whether the assembly was loaded from the GAC.
                Return m_Assembly.GlobalAssemblyCache()
            End Get
        End Property

        Public ReadOnly Property BuildDate() As Date
            Get
                'Returns the date and time when the assembly was last built.
                Return IO.File.GetLastWriteTime(m_Assembly.Location())
            End Get
        End Property

        Public Function GetAttributes() As Specialized.NameValueCollection

            Dim nvc As New Specialized.NameValueCollection

            With nvc
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
                For Each oAttribute As Object In m_Assembly.GetCustomAttributes(False)
                    'Dim strName As String = Regex.Match(oAttribute.GetType().ToString(), "(\.Assembly|\.)(?<Name>[^.]*)Attribute$", RegexOptions.IgnoreCase).Groups("Name").Value()

                    If TypeOf oAttribute Is System.Reflection.AssemblyTitleAttribute Then
                        .Add("Title", Title())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyDescriptionAttribute Then
                        .Add("Description", Description())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyCompanyAttribute Then
                        .Add("Company", Company())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyProductAttribute Then
                        .Add("Product", Product())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyCopyrightAttribute Then
                        .Add("Copyright", Copyright())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyTrademarkAttribute Then
                        .Add("Trademark", Trademark())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyConfigurationAttribute Then
                        .Add("Configuration", Configuration())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyDelaySignAttribute Then
                        .Add("Delay Sign", DelaySign().ToString())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyInformationalVersionAttribute Then
                        .Add("Informational Version", InformationalVersion())
                    ElseIf TypeOf oAttribute Is System.Reflection.AssemblyKeyFileAttribute Then
                        .Add("Key File", KeyFile())
                    ElseIf TypeOf oAttribute Is System.Resources.NeutralResourcesLanguageAttribute Then
                        nvc.Add("Neutral Resources Language", NeutralResourcesLanguage())
                    ElseIf TypeOf oAttribute Is System.Resources.SatelliteContractVersionAttribute Then
                        .Add("Satellite Contract Version", SatelliteContractVersion())
                    ElseIf TypeOf oAttribute Is System.Runtime.InteropServices.ComCompatibleVersionAttribute Then
                        .Add("Com Compatible Version", ComCompatibleVersion())
                    ElseIf TypeOf oAttribute Is System.Runtime.InteropServices.ComVisibleAttribute Then
                        .Add("Com Visible", ComVisible().ToString())
                    ElseIf TypeOf oAttribute Is System.Runtime.InteropServices.GuidAttribute Then
                        nvc.Add("Guid", Guid())
                    ElseIf TypeOf oAttribute Is System.Runtime.InteropServices.TypeLibVersionAttribute Then
                        .Add("Type Lib Version", TypeLibVersion())
                    ElseIf TypeOf oAttribute Is System.CLSCompliantAttribute Then
                        .Add("CLS Compliant", CLSCompliant().ToString())
                    ElseIf TypeOf oAttribute Is System.Diagnostics.DebuggableAttribute Then
                        .Add("Debuggable", Debuggable().ToString())
                    End If
                Next
            End With


            Return nvc

        End Function

        Public Function GetCustomAttribute(ByVal AttributeType As Type) As Object

            Dim objAttributes As Object() = m_Assembly.GetCustomAttributes(AttributeType, False)
            If objAttributes.Length() >= 1 Then
                Return objAttributes(0)
            Else
                Throw New ApplicationException("Assembly does not expose this attribute")
            End If

        End Function

    End Class

End Namespace
