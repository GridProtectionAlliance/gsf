'*******************************************************************************************************
'  TVA.Security.Application.Group.vb - User group defined in the security database
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
'  10/19/2006 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Namespace Application

    ''' <summary>
    ''' Represents a user group defined in the security database.
    ''' </summary>
    <Serializable()> _
    Public Class Group

#Region " Member Declaration "

        Private m_name As String
        Private m_description As String

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Creates an instance of user group defined in the security database.
        ''' </summary>
        ''' <param name="name">Name of the user group.</param>
        ''' <param name="description">Description of the user group.</param>
        Public Sub New(ByVal name As String, ByVal description As String)

            m_name = name
            m_description = description

        End Sub

        ''' <summary>
        ''' Gets the user group's name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Name of the user group.</returns>
        Public ReadOnly Property Name() As String
            Get
                Return m_name
            End Get
        End Property

        ''' <summary>
        ''' Gets the user group's description.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Description of the user group.</returns>
        Public ReadOnly Property Description() As String
            Get
                Return m_description
            End Get
        End Property

        ''' <summary>
        ''' Compares this user group with another user group based on the name and description.
        ''' </summary>
        ''' <param name="obj">Another user group to compare against.</param>
        ''' <returns>True if user groups are the same; otherwise False.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Dim other As Group = TryCast(obj, Group)
            If other IsNot Nothing Then
                Return (m_name = other.Name AndAlso m_description = other.Description)
            Else
                Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
            End If

        End Function

#End Region

    End Class

End Namespace