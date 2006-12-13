' 09-22-06

Imports System.ComponentModel

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