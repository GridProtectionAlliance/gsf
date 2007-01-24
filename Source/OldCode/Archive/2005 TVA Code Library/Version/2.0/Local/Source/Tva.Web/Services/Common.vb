'*******************************************************************************************************
'  Tva.Web.Services.Common.vb - Common web service related functions
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
'  01/24/2007 - J. Ritchie Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports Tva.Security.Application

Namespace Services

    Public Class Common

        Public Shared Function CreateWebServiceCredentials(ByVal userName As String, ByVal password As String, ByVal server As SecurityServer, ByVal passThroughAuthentication As Boolean) As AuthenticationSoapHeader

            With New AuthenticationSoapHeader
                .UserName = userName
                .Password = password
                .Server = server
                .PassThroughAuthentication = True
                Return .This
            End With

        End Function

    End Class

End Namespace
