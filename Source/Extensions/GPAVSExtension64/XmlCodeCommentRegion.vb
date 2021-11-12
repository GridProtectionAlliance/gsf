'******************************************************************************************************
'  XmlCodeCommentRegion.vb - Gbtc
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
Public NotInheritable Class XmlCodeCommentRegion

    ''' <summary>
    ''' Command ID.
    ''' </summary>
    Public Const CommandId As Integer = 258

    ''' <summary>
    ''' Command menu group (command set GUID).
    ''' </summary>
    Public Shared ReadOnly CommandSet As New Guid("e5b1a060-7d63-4673-9524-936d738bf413")

    ''' <summary>
    ''' VS Package that provides this command, not null.
    ''' </summary>
    Private ReadOnly package As package

    ''' <summary>
    ''' Initializes a new instance of the <see cref="XmlCodeCommentRegion"/> class.
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
    Public Shared Property Instance As XmlCodeCommentRegion

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
        Instance = New XmlCodeCommentRegion(package)
    End Sub

    ''' <summary>
    ''' This function is the callback used to execute the command when the menu item is clicked.
    ''' See the constructor to see how the menu item is associated with this function using
    ''' OleMenuCommandService service and MenuCommand class.
    ''' </summary>
    ''' <param name="sender">Event sender.</param>
    ''' <param name="e">Event args.</param>
    Private Sub MenuItemCallback(sender As Object, e As EventArgs)

        XmlCodeCommentRegion()

    End Sub

    Public Sub XmlCodeCommentRegion()

        Dim selection As EnvDTE.TextSelection
        Dim startPoint As EnvDTE.EditPoint
        Dim endPoint As TextPoint
        Dim commentStart As String

        selection = ApplicationObject.ActiveDocument.Selection()
        startPoint = selection.TopPoint.CreateEditPoint()
        endPoint = selection.BottomPoint
        commentStart = LineOrientedXmlCodeCommentStart()
        ApplicationObject.UndoContext.Open("Xml Code Comment Region")

        Try
            Do While (True)
                Dim line As Integer
                line = startPoint.Line
                startPoint.Insert(commentStart)
                startPoint.LineDown()
                startPoint.StartOfLine()
                If (line = endPoint.Line) Then Exit Do
            Loop
        Finally
            ' If an error occurred, then make sure that the undo context is cleaned up.
            ' Otherwise, the editor can be left in a perpetual undo context.
            ApplicationObject.UndoContext.Close()
        End Try

    End Sub

    Private Function LineOrientedXmlCodeCommentStart(Optional ByVal document As Document = Nothing) As String

        Dim extension As String

        If document Is Nothing Then document = ApplicationObject.ActiveDocument

        extension = document.Name

        If (extension.EndsWith(".cs") Or extension.EndsWith(".cpp") Or extension.EndsWith(".h") Or extension.EndsWith(".idl") Or extension.EndsWith(".jsl")) Then
            Return "/// "
        ElseIf (extension.EndsWith(".vb")) Then
            Return "''' "
        Else
            Throw New Exception("Unrecognized file type. You can add this file type by modifying the function LineOrientedXmlCodeCommentStart to include the extension of this file.")
        End If

    End Function

    Private ReadOnly Property ApplicationObject() As DTE2
        Get
            Return ServiceProvider.GetService(GetType(DTE))
        End Get
    End Property
End Class
