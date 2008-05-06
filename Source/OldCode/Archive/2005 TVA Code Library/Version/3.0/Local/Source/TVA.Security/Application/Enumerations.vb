'*******************************************************************************************************
'  TVA.Security.Application.Enumerations.vb - Common enumerations used for application security
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
'  09/22/2006 - Pinal C. Patel
'       Original version of source code generated.
'  04/22/2008 - Pinal C. Patel
'       Added AuthenticationMode enumeration.
'
'*******************************************************************************************************

Namespace Application

    ''' <summary>
    ''' Specifies the server to be used for authentication.
    ''' </summary>
    Public Enum SecurityServer
        ''' <summary>
        ''' Use the development server.
        ''' </summary>
        Development
        ''' <summary>
        ''' Use the acceptance server.
        ''' </summary>
        Acceptance
        ''' <summary>
        ''' Use the production server.
        ''' </summary>
        Production
    End Enum

    ''' <summary>
    ''' Specifies the control property to be set if the current user is in a specified role.
    ''' </summary>
    Public Enum ValidRoleAction
        ''' <summary>
        ''' No control property is be set.
        ''' </summary>
        None
        ''' <summary>
        ''' Control's Visible property is to be set.
        ''' </summary>
        Visible
        ''' <summary>
        ''' Control's Enabled property is to be set.
        ''' </summary>
        Enabled
    End Enum

    ''' <summary>
    ''' Specifies the mode of authentication.
    ''' </summary>
    Public Enum AuthenticationMode
        ''' <summary>
        ''' Internal users are authenticated against the Active Directory and external users are 
        ''' authenticated against credentials stored in the security database.
        ''' </summary>
        AD
        ''' <summary>
        ''' Users are authenticated against the RSA Security server.
        ''' </summary>
        RSA
    End Enum

End Namespace
