'*******************************************************************************************************
'  TVA.Configuration.CategorizedSettingsCollection.vb - Categorized Settings Collection
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/11/2006 - Pinal C. Patel
'       Generated original version of source code.
'  05/25/2006 - Pinal C. Patel
'       Modified the Item(name) property to add a configuration element if it does not exist.
'  08/17/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************

Imports System.Configuration

Namespace Configuration

    ''' <summary>
    ''' Represents a configuration element containing a collection of 
    ''' TVA.Configuration.CategorizedSettingsElement within a configuration file.
    ''' </summary>
    Public Class CategorizedSettingsElementCollection
        Inherits ConfigurationElementCollection

        ''' <summary>
        ''' Gets or sets the TVA.Configuration.CategorizedSettingsElement object at the specified index.
        ''' </summary>
        ''' <param name="index">The zero-based index of the TVA.Configuration.CategorizedSettingsElement to 
        ''' return.</param>
        ''' <returns>The TVA.Configuration.CategorizedSettingsElement at the specified index; otherwise null.</returns>
        Default Public Shadows Property Item(ByVal index As Integer) As CategorizedSettingsElement
            Get
                If index >= MyBase.Count() Then
                    Throw New IndexOutOfRangeException()
                End If
                Return DirectCast(MyBase.BaseGet(index), CategorizedSettingsElement)
            End Get
            Set(ByVal value As CategorizedSettingsElement)
                If MyBase.BaseGet(index) IsNot Nothing Then
                    MyBase.BaseRemoveAt(index)
                End If
                MyBase.BaseAdd(index, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the TVA.Configuration.CategorizedSettingsElement object with the specified name.
        ''' </summary>
        ''' <param name="name">The name of the TVA.Configuration.CategorizedSettingsElement to return.</param>
        ''' <returns>The TVA.Configuration.CategorizedSettingsElement with the specified name; otherwise null.</returns>
        Default Public Shadows ReadOnly Property Item(ByVal name As String) As CategorizedSettingsElement
            Get
                Return Item(name, False)
            End Get
        End Property

        ''' <summary>
        ''' Gets the TVA.Configuration.CategorizedSettingsElement object with the specified name.
        ''' </summary>
        ''' <param name="name">The name of the TVA.Configuration.CategorizedSettingsElement to return.</param>
        ''' <param name="ensureExistance">True, if the setting is to be created if it does not exist; 
        ''' otherwise, false.</param>
        ''' <returns>The TVA.Configuration.CategorizedSettingsElement with the specified name; otherwise null.</returns>
        Default Public Shadows ReadOnly Property Item(ByVal name As String, ByVal ensureExistance As Boolean) As CategorizedSettingsElement
            Get
                If ensureExistance AndAlso MyBase.BaseGet(name) Is Nothing Then
                    Add(name, "")
                End If

                Return DirectCast(MyBase.BaseGet(name), CategorizedSettingsElement)
            End Get
        End Property

        ''' <summary>
        ''' Gets the index of the specified TVA.Configuration.CategorizedSettingsElement.
        ''' </summary>
        ''' <param name="setting">The TVA.Configuration.CategorizedSettingsElement whose index is to be returned.</param>
        ''' <returns>The index of the specified TVA.Configuration.CategorizedSettingsElement; otherwise -1.</returns>
        Public Function IndexOf(ByVal setting As CategorizedSettingsElement) As Integer

            Return MyBase.BaseIndexOf(setting)

        End Function

        ''' <summary>
        ''' Adds a TVA.Configuration.CategorizedSettingsElement with the specified name and value string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        Public Sub Add(ByVal name As String, ByVal value As String)

            Add(name, value, False)

        End Sub

        ''' <summary>
        ''' Adds a TVA.Configuration.CategorizedSettingsElement with the specified name and value string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="encryptValue">True, if the value string of the element is to be encrypted; otherwise, 
        ''' false.</param>
        Public Sub Add(ByVal name As String, ByVal value As String, ByVal encryptValue As Boolean)

            Add(name, value, "", encryptValue)

        End Sub

        ''' <summary>
        ''' Adds a TVA.Configuration.CategorizedSettingsElement with the specified name, value and description 
        ''' string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="description">The description string of the element.</param>
        Public Sub Add(ByVal name As String, ByVal value As String, ByVal description As String)

            Add(New CategorizedSettingsElement(name, value, description, False))

        End Sub

        ''' <summary>
        ''' Adds a TVA.Configuration.CategorizedSettingsElement with the specified name, value and description 
        ''' string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="description">The description string of the element.</param>
        ''' <param name="encryptValue">True, if the value string of the element is to be encrypted; otherwise, 
        ''' false.</param>
        Public Sub Add(ByVal name As String, ByVal value As String, ByVal description As String, ByVal encryptValue As Boolean)

            Add(New CategorizedSettingsElement(name, value, description, encryptValue))

        End Sub

        ''' <summary>
        ''' Adds the specified TVA.Configuration.CategorizedSettingsElement to the 
        ''' TVA.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="setting">The TVA.Configuration.CategorizedSettingsElement to add.</param>
        Public Sub Add(ByVal setting As CategorizedSettingsElement)

            If MyBase.BaseGet(setting.Name()) Is Nothing Then
                ' Adds the element only if it does not exist.
                MyBase.BaseAdd(setting)
            End If

        End Sub

        ''' <summary>
        ''' Removes a TVA.Configuration.CategorizedSettingsElement with the specified name from the 
        ''' TVA.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="name">The name of the TVA.Configuration.CategorizedSettingsElement to remove.</param>
        Public Sub Remove(ByVal name As String)

            MyBase.BaseRemove(name)

        End Sub

        ''' <summary>
        ''' Removes the specified TVA.Configuration.CategorizedSettingsElement from the 
        ''' TVA.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="setting">The TVA.Configuration.CategorizedSettingsElement to remove.</param>
        Public Sub Remove(ByVal setting As CategorizedSettingsElement)

            If MyBase.BaseIndexOf(setting) >= 0 Then
                Remove(setting.Name())
            End If

        End Sub

        ''' <summary>
        ''' Removes the TVA.Configuration.CategorizedSettingsElement at the specified location from the 
        ''' TVA.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="index">The index location of the TVA.Configuration.CategorizedSettingsElement to 
        ''' remove.</param>
        Public Sub RemoveAt(ByVal index As Integer)

            MyBase.BaseRemoveAt(index)

        End Sub

        ''' <summary>
        ''' Removes all TVA.Configuration.CategorizedSettingsElement from the 
        ''' TVA.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        Public Sub Clear()

            MyBase.BaseClear()

        End Sub

#Region " Required ConfigurationElementCollection Overrides "

        Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement

            Return New CategorizedSettingsElement()

        End Function

        Protected Overrides Function CreateNewElement(ByVal elementName As String) As System.Configuration.ConfigurationElement

            Return New CategorizedSettingsElement(elementName)

        End Function

        Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object

            Return DirectCast(element, CategorizedSettingsElement).Name()

        End Function

#End Region

    End Class

End Namespace