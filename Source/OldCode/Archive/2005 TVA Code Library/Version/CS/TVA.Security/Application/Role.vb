'*******************************************************************************************************
'  TVA.Security.Application.Role.vb - Application role defined in the security database
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
'  09/26/2006 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Namespace Application

    ''' <summary>
    ''' Represents an application role defined in the security database.
    ''' </summary>
    <Serializable()> _
    Public Class Role

#Region " Member Declaration "

        Private m_name As String
        Private m_description As String
        Private m_application As Application

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Creates an instance of application roles defined in the security database.
        ''' </summary>
        ''' <param name="name">Name of the role.</param>
        ''' <param name="description">Description of the role.</param>
        ''' <param name="application">Name of the application to which the role belongs.</param>
        Public Sub New(ByVal name As String, ByVal description As String, ByVal application As Application)

            m_name = name
            m_description = description
            m_application = application

        End Sub

        ''' <summary>
        ''' Gets the application to which the role belongs.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Application to which the role belongs.</returns>
        Public ReadOnly Property Application() As Application
            Get
                Return m_application
            End Get
        End Property

        ''' <summary>
        ''' Gets the role's name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Name of the role.</returns>
        Public ReadOnly Property Name() As String
            Get
                Return m_name
            End Get
        End Property

        ''' <summary>
        ''' Gets the role's description.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Description of the role.</returns>
        Public ReadOnly Property Description() As String
            Get
                Return m_description
            End Get
        End Property

        ''' <summary>
        ''' Compares this roles with another role based on the name and description.
        ''' </summary>
        ''' <param name="obj">Another role to compare against.</param>
        ''' <returns>True if roles are the same; otherwise False.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Dim other As Role = TryCast(obj, Role)
            If other IsNot Nothing Then
                Return (m_name = other.Name AndAlso m_description = other.Description AndAlso m_application.Equals(other.Application))
            Else
                Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
            End If

        End Function

#End Region

    End Class

End Namespace