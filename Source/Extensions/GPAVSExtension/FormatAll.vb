'******************************************************************************************************
'  FormatAll.vb - Gbtc
'
'  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
'
'  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
'  the NOTICE file distributed with this work for additional information regarding copyright ownership.
'  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
'  not use this file except in compliance with the License. You may obtain a copy of the License at:
'
'      http://opensource.org/licenses/MIT
'
'  Unless agreed to in writing, the subject software distributed under the License is distributed on an
'  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
'  License for the specific language governing permissions and limitations.
'
'  Code Modification History:
'  ----------------------------------------------------------------------------------------------------
'  10/02/2015 - Stephen C. Wills
'       Generated original version of source code.
'
'******************************************************************************************************

Imports System
Imports System.ComponentModel.Design
Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell

''' <summary>
''' Command handler
''' </summary>
Public NotInheritable Class FormatAll

    ''' <summary>
    ''' Command ID.
    ''' </summary>
    Public Const CommandId As Integer = 257

    ''' <summary>
    ''' Command menu group (command set GUID).
    ''' </summary>
    Public Shared ReadOnly CommandSet As New Guid("e5b1a060-7d63-4673-9524-936d738bf413")

    ''' <summary>
    ''' VS Package that provides this command, not null.
    ''' </summary>
    Private ReadOnly package As package

    ''' <summary>
    ''' Initializes a new instance of the <see cref="FormatAll"/> class.
    ''' Adds our command handlers for menu (the commands must exist in the command table file)
    ''' </summary>
    ''' <param name="package">Owner package, not null.</param>
    Private Sub New(package As package)
        If package Is Nothing Then
            Throw New ArgumentNullException("package")
        End If

        Me.package = package
        Dim commandService As OleMenuCommandService = Me.ServiceProvider.GetService(GetType(IMenuCommandService))
        If commandService IsNot Nothing Then
            Dim menuCommandId = New CommandID(CommandSet, CommandId)
            Dim menuCommand = New MenuCommand(AddressOf Me.MenuItemCallback, menuCommandId)
            commandService.AddCommand(menuCommand)
        End If
    End Sub

    ''' <summary>
    ''' Gets the instance of the command.
    ''' </summary>
    Public Shared Property Instance As FormatAll

    ''' <summary>
    ''' Get service provider from the owner package.
    ''' </summary>
    Private ReadOnly Property ServiceProvider As IServiceProvider
        Get
            Return Me.package
        End Get
    End Property

    ''' <summary>
    ''' Initializes the singleton instance of the command.
    ''' </summary>
    ''' <param name="package">Owner package, Not null.</param>
    Public Shared Sub Initialize(package As package)
        Instance = New FormatAll(package)
    End Sub

    ''' <summary>
    ''' This function is the callback used to execute the command when the menu item is clicked.
    ''' See the constructor to see how the menu item is associated with this function using
    ''' OleMenuCommandService service and MenuCommand class.
    ''' </summary>
    ''' <param name="sender">Event sender.</param>
    ''' <param name="e">Event args.</param>
    Private Sub MenuItemCallback(sender As Object, e As EventArgs)

        FormatAll()

    End Sub

    Public Sub FormatAll()

        Dim project As Project
        Dim projectObjects As Object()
        Dim window As Window
        Dim target As Object

        window = ApplicationObject.Windows.Item(Constants.vsWindowKindCommandWindow)
        projectObjects = ApplicationObject.ActiveSolutionProjects

        If projectObjects.Length = 0 Then
            Exit Sub
        End If

        project = ApplicationObject.ActiveSolutionProjects(0)

        If (ApplicationObject.ActiveWindow Is window) Then
            target = window.Object
        Else
            target = GetOutputWindowPane("List Project")
            target.Clear()
        End If

        RecurseProjectFolders(project.ProjectItems(), 0, target)

    End Sub

    Private Sub RecurseProjectFolders(ByVal projectItems As EnvDTE.ProjectItems, ByVal level As Integer, ByVal outputWinPane As Object)

        Dim projectItem As EnvDTE.ProjectItem

        For Each projectItem In projectItems
            ' Ignore item if it is not rooted in this collection (check for VC project model).
            If projectItem.Collection Is projectItems Then
                ' Execute formatting action
                PerformCodeItem(projectItem, level, outputWinPane)
                ' Recurse if this item has subitems ...
                Dim projectItems2 As EnvDTE.ProjectItems = projectItem.ProjectItems
                If projectItems2 IsNot Nothing Then RecurseProjectFolders(projectItems2, level + 1, outputWinPane)
            End If
        Next

    End Sub

    Private Sub PerformCodeItem(ByVal projectItem As EnvDTE.ProjectItem, ByVal level As Integer, ByVal outputWinPane As Object)

        Dim window As EnvDTE.Window
        Dim alreadyOpen As Boolean

        If projectItem.Name.EndsWith(".cs") Then
            alreadyOpen = projectItem.IsOpen(Constants.vsext_vk_Code)
            window = projectItem.Open(Constants.vsext_vk_Code)
            window.Activate()
            ApplicationObject.ExecuteCommand("Edit.FormatDocument")
            If Not alreadyOpen Then window.Close(vsSaveChanges.vsSaveChangesYes)
        End If

    End Sub

    Private Function GetOutputWindowPane(ByVal Name As String, Optional ByVal show As Boolean = True) As OutputWindowPane

        Dim window As Window
        Dim outputWindow As OutputWindow
        Dim outputWindowPane As OutputWindowPane

        window = ApplicationObject.Windows.Item(EnvDTE.Constants.vsWindowKindOutput)
        If show Then window.Visible = True
        outputWindow = window.Object

        Try
            outputWindowPane = outputWindow.OutputWindowPanes.Item(Name)
        Catch e As System.Exception
            outputWindowPane = outputWindow.OutputWindowPanes.Add(Name)
        End Try

        outputWindowPane.Activate()

        Return outputWindowPane

    End Function

    Private ReadOnly Property ApplicationObject() As DTE2
        Get
            Return ServiceProvider.GetService(GetType(DTE))
        End Get
    End Property
End Class
