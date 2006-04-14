' 04-11-06

Imports System.Configuration

Namespace Configuration

    ''' <summary>
    ''' Represents a settings element within a configuration file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CategorizedSettingsElement
        Inherits ConfigurationElement

        Public Sub New()
            Me.New("")
        End Sub

        Public Sub New(ByVal name As String)
            Me.New(name, "")
        End Sub

        Public Sub New(ByVal name As String, ByVal value As String)
            Me.New(name, value, "")
        End Sub

        Public Sub New(ByVal name As String, ByVal value As String, ByVal description As String)
            MyBase.New()
            Me.Name = name
            Me.Value = value
            Me.Description = description
        End Sub

        <ConfigurationProperty("name", IsKey:=True, IsRequired:=True)> _
        Public Property Name() As String
            Get
                Return Convert.ToString(MyBase.Item("name"))
            End Get
            Set(ByVal value As String)
                MyBase.Item("name") = value
            End Set
        End Property

        <ConfigurationProperty("value", IsRequired:=True)> _
        Public Property Value() As String
            Get
                Return Convert.ToString(MyBase.Item("value"))
            End Get
            Set(ByVal value As String)
                MyBase.Item("value") = value
            End Set
        End Property

        <ConfigurationProperty("description", IsRequired:=True)> _
        Public Property Description() As String
            Get
                Return Convert.ToString(MyBase.Item("description"))
            End Get
            Set(ByVal value As String)
                MyBase.Item("description") = value
            End Set
        End Property

    End Class

End Namespace
