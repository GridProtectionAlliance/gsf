'*******************************************************************************************************
'  Tva.Configuration.CategorizedSettingsElement.vb - Categorized Settings Element
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
'  05/25/2006 - J. Ritchie Carroll
'       Added Try/Catch safety wrapper around GetTypedValue implementation
'  06/01/2006 - J. Ritchie Carroll
'       Added GetTypedValue overload to handle boolean types as a special case
'
'*******************************************************************************************************

Imports System.Configuration
Imports Tva.Common
Imports Tva.Text.Common
Imports Tva.Security.Cryptography
Imports Tva.Security.Cryptography.Common

Namespace Configuration

    ''' <summary>
    ''' Represents a configuration element under the categories of categorizedSettings section within 
    ''' a configuration file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CategorizedSettingsElement
        Inherits ConfigurationElement

        Private Const CryptoKey As String = "0679d9ae-aca5-4702-a3f5-604415096987"

        ''' <summary>
        ''' This constructor is required by the configuration API and is for internal use only.
        ''' </summary>
        ''' <remarks></remarks>
        Friend Sub New()
            MyClass.New("")
        End Sub

        ''' <summary>
        ''' This constructor is required by the configuration API and is for internal use only.
        ''' </summary>
        ''' <remarks></remarks>
        Friend Sub New(ByVal name As String)
            MyClass.New(name, "")
        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Configuration.CategorizedSettingsElement with the specified
        ''' name and value information.
        ''' </summary>
        ''' <param name="name">The identifier string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal name As String, ByVal value As String)
            MyClass.New(name, value, "")
        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Configuration.CategorizedSettingsElement with the specified
        ''' name and value information.
        ''' </summary>
        ''' <param name="name">The identifier string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="description">The description string of the element.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal name As String, ByVal value As String, ByVal description As String)
            MyClass.New(name, value, description, False)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Configuration.CategorizedSettingsElement with the specified
        ''' name and value information.
        ''' </summary>
        ''' <param name="name">The identifier string of the element.</param>
        ''' <param name="value">The value string of the element.</param>
        ''' <param name="description">The description string of the element.</param>
        ''' <param name="encrypted">True if the value string of the element is to be encrypted; otherwise False.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal name As String, ByVal value As String, ByVal description As String, ByVal encrypted As Boolean)
            MyBase.New()
            Me.Name = name
            Me.Value = value
            Me.Description = description
            Me.Encrypted = encrypted
        End Sub

        ''' <summary>
        ''' Gets or sets the identifier string of the element.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The identifier string of the element.</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("name", IsKey:=True, IsRequired:=True)> _
        Public Property Name() As String
            Get
                Return Convert.ToString(MyBase.Item("name"))
            End Get
            Set(ByVal value As String)
                MyBase.Item("name") = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the value string of the element.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The value string of the element.</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("value", IsRequired:=True)> _
        Public Property Value() As String
            Get
                Return DecryptValue(Convert.ToString(MyBase.Item("value")))
            End Get
            Set(ByVal value As String)
                MyBase.Item("value") = EncryptValue(value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the element value as the specified type.
        ''' </summary>
        ''' <typeparam name="T">Type to which the value string is to be converted.</typeparam>
        ''' <param name="defaultValue">The default value to return if the value string is empty.</param>
        ''' <returns>The type-coerced value of the referenced setting</returns>
        ''' <remarks>If this function fails to properly coerce value to specified type, default value is returned</remarks>
        Public Function GetTypedValue(Of T)(ByVal defaultValue As T) As T

            Try
                Dim stringValue As String = Value()

                If Not String.IsNullOrEmpty(stringValue) Then
                    ' Element's value string is present - convert it to the proper type
                    Return CType(DirectCast(stringValue, Object), T)
                Else
                    ' Element's value string is not present, so use the default
                    Return defaultValue
                End If
            Catch
                Return defaultValue
            End Try

        End Function

        ''' <summary>
        ''' Gets or sets the description string of the element.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The description string of the element.</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("description", IsRequired:=True)> _
        Public Property Description() As String
            Get
                Return Convert.ToString(MyBase.Item("description"))
            End Get
            Set(ByVal value As String)
                MyBase.Item("description") = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean indicating whether the value string of the element is to be encrypted.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the value string of the element is to be encrypted; otherwise False.</returns>
        ''' <remarks></remarks>
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
                ' The element's value is to be encrypted, so encrypt it.
                encryptedValue = Encrypt(value, CryptoKey, EncryptLevel.Level4)
            End If
            Return encryptedValue

        End Function

        Private Function DecryptValue(ByVal value As String) As String

            Dim decryptedValue As String = value
            If MyBase.Item("encrypted") IsNot Nothing AndAlso Convert.ToBoolean(MyBase.Item("encrypted")) Then
                ' The element's value has been encrypted, so decrypt it.
                decryptedValue = Decrypt(value, CryptoKey, EncryptLevel.Level4)
            End If
            Return decryptedValue

        End Function

    End Class

End Namespace
