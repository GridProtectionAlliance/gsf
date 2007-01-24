'*******************************************************************************************************
'  Tva.Web.Services.AuthenticationSoapHeader.vb - Soap header used for authentication
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/24/2007 - Shyni John
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Xml.Serialization
Imports System.Web.Services.Protocols
Imports Tva.Security.Application

Namespace Services

    <SerializableAttribute(), _
    XmlTypeAttribute([Namespace]:="http://troweb/DataServices/"), _
    XmlRootAttribute([Namespace]:="http://troweb/DataServices/", IsNullable:=False)> _
    Public Class AuthenticationSoapHeader

        Inherits SoapHeader

        Private m_userName As String
        Private m_password As String
        Private m_securityServer As SecurityServer
        Private m_passThroughAuthentication As Boolean

        Public ReadOnly Property This() As AuthenticationSoapHeader
            Get
                Return Me
            End Get
        End Property

        Public Property UserName() As String
            Get
                Return m_userName
            End Get
            Set(ByVal value As String)
                m_userName = value
            End Set
        End Property

        Public Property Password() As String
            Get
                Return m_password
            End Get
            Set(ByVal value As String)
                m_password = value
            End Set
        End Property

        Public Property Server() As SecurityServer
            Get
                Return m_securityServer
            End Get
            Set(ByVal value As SecurityServer)
                m_securityServer = value
            End Set
        End Property

        Public Property PassThroughAuthentication() As Boolean
            Get
                Return m_passThroughAuthentication
            End Get
            Set(ByVal value As Boolean)
                m_passThroughAuthentication = value
            End Set
        End Property

    End Class

End Namespace
