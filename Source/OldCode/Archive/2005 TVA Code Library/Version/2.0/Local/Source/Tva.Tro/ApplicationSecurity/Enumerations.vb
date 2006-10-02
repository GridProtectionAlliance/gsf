' 09-22-06

Imports System.ComponentModel

Namespace ApplicationSecurity

    Public Enum SecurityServer As Integer
        Development
        <EditorBrowsable(EditorBrowsableState.Never), Browsable(False)> _
        Acceptance
        <EditorBrowsable(EditorBrowsableState.Never), Browsable(False)> _
        Production
    End Enum

    Public Enum ValidRoleAction As Integer
        None
        Visible
        Enabled
        [ReadOnly]
    End Enum

End Namespace