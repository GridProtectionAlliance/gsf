'*******************************************************************************************************
'  TVA.Tibco.Common.vb - Common global Tibco functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/30/2007 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports Microsoft.Web.Services3.Security.Tokens

Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    Public Shared Function GetUserNameToken() As UsernameToken

        Return New UsernameToken("esocss", "pwd4ctrl", PasswordOption.SendHashed)

    End Function

End Class
