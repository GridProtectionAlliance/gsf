' 04-11-06

Imports System.Configuration

Namespace Configuration

    Public Class CategorizedSettingsCollection
        Inherits ConfigurationElementCollection

        Default Public Shadows Property Item(ByVal index As Integer) As CategorizedSettingsElement
            Get
                Return DirectCast(MyBase.BaseGet(index), CategorizedSettingsElement)
            End Get
            Set(ByVal value As CategorizedSettingsElement)
                If MyBase.BaseGet(index) IsNot Nothing Then
                    MyBase.BaseRemoveAt(index)
                End If
                MyBase.BaseAdd(index, value)
            End Set
        End Property

        Default Public Shadows ReadOnly Property Item(ByVal name As String) As CategorizedSettingsElement
            Get
                Return DirectCast(MyBase.BaseGet(name), CategorizedSettingsElement)
            End Get
        End Property

        Public Function IndexOf(ByVal setting As CategorizedSettingsElement) As Integer

            Return MyBase.BaseIndexOf(setting)

        End Function

        Public Sub Add(ByVal name As String, ByVal value As String)

            Me.Add(name, value, False)

        End Sub

        Public Sub Add(ByVal name As String, ByVal value As String, ByVal encryptValue As Boolean)

            Me.Add(name, value, "", encryptValue)

        End Sub

        Public Sub Add(ByVal name As String, ByVal value As String, ByVal description As String)

            Me.Add(New CategorizedSettingsElement(name, value, description, False))

        End Sub

        Public Sub Add(ByVal name As String, ByVal value As String, ByVal description As String, ByVal encryptValue As Boolean)

            Me.Add(New CategorizedSettingsElement(name, value, description, encryptValue))

        End Sub

        Public Sub Add(ByVal setting As CategorizedSettingsElement)

            If MyBase.BaseGet(setting.Name()) Is Nothing Then
                ' Add the element only if it doesn't exist.
                MyBase.BaseAdd(setting)
            End If

        End Sub

        Public Sub Remove(ByVal name As String)

            MyBase.BaseRemove(name)

        End Sub

        Public Sub Remove(ByVal setting As CategorizedSettingsElement)

            If MyBase.BaseIndexOf(setting) >= 0 Then
                Me.Remove(setting.Name())
            End If

        End Sub

        Public Sub RemoveAt(ByVal index As Integer)

            MyBase.BaseRemoveAt(index)

        End Sub

        Public Sub Clear()

            MyBase.BaseClear()

        End Sub

        Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement

            Return New CategorizedSettingsElement()

        End Function

        Protected Overrides Function CreateNewElement(ByVal elementName As String) As System.Configuration.ConfigurationElement

            Return New CategorizedSettingsElement(elementName)

        End Function

        Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object

            Return DirectCast(element, CategorizedSettingsElement).Name()

        End Function

    End Class

End Namespace