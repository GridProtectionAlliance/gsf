' 04-11-06

Imports System.Configuration
Imports Tva.Security.Cryptography.Common

Namespace Configuration

    ''' <summary>
    ''' Represents a settings element within a configuration file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CategorizedSettingsElement
        Inherits ConfigurationElement

        Private Const CryptoKey As String = "{A910EA83-338E-41fc-9AF9-2752BD5CB0B8}"

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
            Me.New(name, value, description, False)
        End Sub

        Public Sub New(ByVal name As String, ByVal value As String, ByVal description As String, ByVal encrypted As Boolean)
            MyBase.New()
            Me.Name = name
            Me.Value = value
            Me.Description = description
            Me.Encrypted = encrypted
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
                Return DecryptValue(Convert.ToString(MyBase.Item("value")))
            End Get
            Set(ByVal value As String)
                MyBase.Item("value") = EncryptValue(value)
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

        <ConfigurationProperty("encrypted", IsRequired:=True)> _
        Public Property Encrypted() As Boolean
            Get
                Return Convert.ToBoolean(MyBase.Item("encrypted"))
            End Get
            Set(ByVal value As Boolean)
                Dim elementValue As String = Me.Value() ' Get the decrypted value if encrypted.
                MyBase.Item("encrypted") = value
                Me.Value = elementValue ' Setting the value again will cause encryption to be performed if required.
            End Set
        End Property

        Private Function EncryptValue(ByVal value As String) As String

            Dim encryptedValue As String = value
            If MyBase.Item("encrypted") IsNot Nothing AndAlso Convert.ToBoolean(MyBase.Item("encrypted")) Then
                encryptedValue = Encrypt(value, EncryptLevel.Level4)
            End If
            Return encryptedValue

        End Function

        Private Function DecryptValue(ByVal value As String) As String

            Dim decryptedValue As String = value
            If MyBase.Item("encrypted") IsNot Nothing AndAlso Convert.ToBoolean(MyBase.Item("encrypted")) Then
                decryptedValue = Decrypt(value, EncryptLevel.Level4)
            End If
            Return decryptedValue

        End Function

    End Class

End Namespace
