'*******************************************************************************************************
'  Tva.Configuration.CategorizedSettingsCollection.vb - Categorized Settings Collection
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
'       Original version of source code generated
'  05/25/2006 - Pinal C. Patel
'       Modified the Item(name) property to add a configuration element if it doesn't exist.
'
'*******************************************************************************************************

Imports System.Configuration

Namespace Configuration

    ''' <summary>
    ''' Represents a configuration element containing a collection of Tva.Configuration.CategorizedSettingsElement 
    ''' within a configuration file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CategorizedSettingsCollection
        Inherits ConfigurationElementCollection

        ''' <summary>
        ''' Gets or sets the Tva.Configuration.CategorizedSettingsElement object at the specified index.
        ''' </summary>
        ''' <param name="index">The zero-based index of the Tva.Configuration.CategorizedSettingsElement to return.</param>
        ''' <value></value>
        ''' <returns>The Tva.Configuration.CategorizedSettingsElement at the specified index; otherwise null.</returns>
        ''' <remarks></remarks>
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
        ''' Gets the Tva.Configuration.CategorizedSettingsElement object with the specified name.
        ''' </summary>
        ''' <param name="name">The name of the Tva.Configuration.CategorizedSettingsElement to return.</param>
        ''' <value></value>
        ''' <returns>The Tva.Configuration.CategorizedSettingsElement with the specified name; otherwise null.</returns>
        ''' <remarks></remarks>
        Default Public Shadows ReadOnly Property Item(ByVal name As String) As CategorizedSettingsElement
            Get
                If MyBase.BaseGet(name) Is Nothing Then
                    ' We'll add a configuration element for the specified name if it doesn't exist.
                    MyClass.Add(name, "")
                End If
                Return DirectCast(MyBase.BaseGet(name), CategorizedSettingsElement)
            End Get
        End Property

        ''' <summary>
        ''' Gets the index of the specified Tva.Configuration.CategorizedSettingsElement.
        ''' </summary>
        ''' <param name="setting">The Tva.Configuration.CategorizedSettingsElement whose index is to be returned.</param>
        ''' <returns>The index of the specified Tva.Configuration.CategorizedSettingsElement; otherwise -1.</returns>
        ''' <remarks></remarks>
        Public Function IndexOf(ByVal setting As CategorizedSettingsElement) As Integer

            Return MyBase.BaseIndexOf(setting)

        End Function

        ''' <summary>
        ''' Adds a Tva.Configuration.CategorizedSettingsElement with the specified name and value string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <remarks></remarks>
        Public Sub Add(ByVal name As String, ByVal value As String)

            MyClass.Add(name, value, False)

        End Sub

        ''' <summary>
        ''' Adds a Tva.Configuration.CategorizedSettingsElement with the specified name and value string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="encryptValue">True if the value string of the element is to be encrypted; otherwise False.</param>
        ''' <remarks></remarks>
        Public Sub Add(ByVal name As String, ByVal value As String, ByVal encryptValue As Boolean)

            MyClass.Add(name, value, "", encryptValue)

        End Sub

        ''' <summary>
        ''' Adds a Tva.Configuration.CategorizedSettingsElement with the specified name, value and description string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="description">The description string of the element.</param>
        ''' <remarks></remarks>
        Public Sub Add(ByVal name As String, ByVal value As String, ByVal description As String)

            MyClass.Add(New CategorizedSettingsElement(name, value, description, False))

        End Sub

        ''' <summary>
        ''' Adds a Tva.Configuration.CategorizedSettingsElement with the specified name, value and description string.
        ''' </summary>
        ''' <param name="name">The name string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="description">The description string of the element.</param>
        ''' <param name="encryptValue">True if the value string of the element is to be encrypted; otherwise False.</param>
        ''' <remarks></remarks>
        Public Sub Add(ByVal name As String, ByVal value As String, ByVal description As String, ByVal encryptValue As Boolean)

            MyClass.Add(New CategorizedSettingsElement(name, value, description, encryptValue))

        End Sub

        ''' <summary>
        ''' Adds the specified Tva.Configuration.CategorizedSettingsElement to the Tva.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="setting">The Tva.Configuration.CategorizedSettingsElement to add.</param>
        ''' <remarks></remarks>
        Public Sub Add(ByVal setting As CategorizedSettingsElement)

            If MyBase.BaseGet(setting.Name()) Is Nothing Then
                ' Add the element only if it doesn't exist.
                MyBase.BaseAdd(setting)
            End If

        End Sub

        ''' <summary>
        ''' Removes a Tva.Configuration.CategorizedSettingsElement with the specified name from the Tva.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="name">The name of the Tva.Configuration.CategorizedSettingsElement to remove.</param>
        ''' <remarks></remarks>
        Public Sub Remove(ByVal name As String)

            MyBase.BaseRemove(name)

        End Sub

        ''' <summary>
        ''' Removes the specified Tva.Configuration.CategorizedSettingsElement from the Tva.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="setting">The Tva.Configuration.CategorizedSettingsElement to remove.</param>
        ''' <remarks></remarks>
        Public Sub Remove(ByVal setting As CategorizedSettingsElement)

            If MyBase.BaseIndexOf(setting) >= 0 Then
                MyClass.Remove(setting.Name())
            End If

        End Sub

        ''' <summary>
        ''' Remove the Tva.Configuration.CategorizedSettingsElement at the specified location from the Tva.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <param name="index">The index location of the Tva.Configuration.CategorizedSettingsElement to remove.</param>
        ''' <remarks></remarks>
        Public Sub RemoveAt(ByVal index As Integer)

            MyBase.BaseRemoveAt(index)

        End Sub

        ''' <summary>
        ''' Removes all Tva.Configuration.CategorizedSettingsElement from the Tva.Configuration.CategorizedSettingsCollection.
        ''' </summary>
        ''' <remarks></remarks>
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