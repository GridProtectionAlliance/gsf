'*******************************************************************************************************
'  Tva.Security.Application.Enumerations.vb - Common enumerations used for application security
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
'       Original version of source code generated
'
'*******************************************************************************************************

Namespace Application

    Public Enum SecurityServer As Integer
        Development
        Acceptance
        Production
    End Enum

    Public Enum ValidRoleAction As Integer
        None
        Visible
        Enabled
    End Enum

End Namespace
