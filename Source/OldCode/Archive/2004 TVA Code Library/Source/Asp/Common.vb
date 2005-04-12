' James Ritchie Carroll - 2003
' Pinal Patel 03/04/05 - added many handy ASP.NET functions...
Option Explicit On 

Imports System.Text
Imports System.Text.RegularExpressions

Namespace Asp

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Performs JavaScript encoding on given string
        Public Shared Function JavaScriptEncode(ByVal Str As String) As String

            Str = Replace(Str, "\", "\\")
            Str = Replace(Str, "'", "\'")
            Str = Replace(Str, """", "\""")
            Str = Replace(Str, Chr(8), "\b")
            Str = Replace(Str, Chr(9), "\t")
            Str = Replace(Str, Chr(10), "\r")
            Str = Replace(Str, Chr(12), "\f")
            Str = Replace(Str, Chr(13), "\n")

            Return Str

        End Function

        ' Decodes JavaScript characters from given string
        Public Shared Function JavaScriptDecode(ByVal Str As String) As String

            Str = Replace(Str, "\\", "\")
            Str = Replace(Str, "\'", "'")
            Str = Replace(Str, "\""", """")
            Str = Replace(Str, "\b", Chr(8))
            Str = Replace(Str, "\t", Chr(9))
            Str = Replace(Str, "\r", Chr(10))
            Str = Replace(Str, "\f", Chr(12))
            Str = Replace(Str, "\n", Chr(13))

            Return Str

        End Function

        ' Ensures a string is compliant with cookie name requirements
        Public Shared Function ValidCookieName(ByVal Str As String) As String

            Str = Replace(Str, "=", "")
            Str = Replace(Str, ";", "")
            Str = Replace(Str, ",", "")
            Str = Replace(Str, Chr(9), "")
            Str = Replace(Str, Chr(10), "")
            Str = Replace(Str, Chr(13), "")

            Return Str

        End Function

        ' Ensures a string is compliant with cookie value requirements
        Public Shared Function ValidCookieValue(ByVal Str As String) As String

            Str = Replace(Str, ";", "")
            Str = Replace(Str, ",", "")

            Return Str

        End Function

#Region "Common function for creating client-side script"

        Private Enum ScriptCode As Integer

            Focus
            DefaultButton
            Show
            ShowDialog
            ShowPopup
            Close
            MsgBox
            Refresh
            Maximize
            Minimize
            RunClientExe

        End Enum

        Private Shared Function CreateClientSideScript(ByVal Code As ScriptCode) As String

            Select Case Code
                Case ScriptCode.Focus
                    'Client-side script for Focus.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function Focus(strControlId)" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       if (document.getElementById(strControlId) != null)" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("           document.getElementById(strControlId).focus();" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.DefaultButton
                    'Client-side script for DefaultButton.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function DefaultButton(strControlId)" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       if (event.keyCode == 13)" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("           event.returnValue = false;" & vbCrLf)
                        .Append("           event.cancel = true;" & vbCrLf)
                        .Append("           if (document.getElementById(strControlId) != null)" & vbCrLf)
                        .Append("           {" & vbCrLf)
                        .Append("               document.getElementById(strControlId).click();" & vbCrLf)
                        .Append("           }" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.Show
                    'Client-side script for Show.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function Show(strUrl, intHeight, intWidth, intLeft, intTop, intCenter, intHelp, intResizable, intStatus)" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       window.showModelessDialog(strUrl, window.self,'dialogWidth:' + intWidth + 'px;dialogHeight:' + intHeight + 'px;left:' + intLeft + ';top:' + intTop + ';center:' + intCenter + ';help:' + intHelp + ';resizable:' + intResizable + ';status:' + intStatus);" & vbCrLf)
                        .Append("       return false;" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.ShowDialog
                    'Client-side script for ShowDialog.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function ShowDialog(strUrl, strResultHolderId, intHeight, intWidth, intLeft, intTop, intCenter, intHelp, intResizable, intStatus)" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       strReturnValue = window.showModalDialog(strUrl, window.self,'dialogWidth:' + intWidth + 'px;dialogHeight:' + intHeight + 'px;left:' + intLeft + ';top:' + intTop + ';center:' + intCenter + ';help:' + intHelp + ';resizable:' + intResizable + ';status:' + intStatus);" & vbCrLf)
                        .Append("       if (strReturnValue != null)" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("           if ((strResultHolderId != null) && (document.getElementById(strResultHolderId) != null))" & vbCrLf)
                        .Append("           {" & vbCrLf)
                        .Append("               document.getElementById(strResultHolderId).value = strReturnValue;" & vbCrLf)
                        .Append("               return false;" & vbCrLf)
                        .Append("           }" & vbCrLf)
                        .Append("           else" & vbCrLf)
                        .Append("           {" & vbCrLf)
                        .Append("               return true;" & vbCrLf)
                        .Append("           }" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("       else" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("           return false;" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.ShowPopup
                    'Client-side script for ShowPopup.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function ShowPopup(strUrl, intHeight, intWidth, intLeft, intTop, intCenter, intResizable, intScrollbars, intToolbar, intMenubar, intLocation, intStatus, intDirectories)" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       reSpecialCharacters = /([^a-zA-Z0-9\s])/gi;" & vbCrLf)
                        .Append("       strPopupName = strUrl.replace(reSpecialCharacters, '');" & vbCrLf)
                        .Append("       if (intCenter)" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("           objPopup = window.open(strUrl, strPopupName, 'height=' + intHeight + ',width=' + intWidth + ',top=' + ((screen.availHeight / 2) - (intHeight / 2)) + ',left=' + ((screen.availWidth / 2) - (intWidth / 2)) + ',resizable=' + intResizable + ',scrollbars=' + intScrollbars + ',toolbar=' + intToolbar + ',menubar=' + intMenubar + ',location=' + intLocation + ',status=' + intStatus + ',directories=' + intDirectories);" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("       else" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("            objPopup = window.open(strUrl, strPopupName, 'height=' + intHeight + ',width=' + intWidth + ',top=' + intTop + ',left=' + intLeft + ',resizable=' + intResizable + ',scrollbars=' + intScrollbars + ',toolbar=' + intToolbar + ',menubar=' + intMenubar + ',location=' + intLocation + ',status=' + intStatus + ',directories=' + intDirectories);" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("       objPopup.focus();" & vbCrLf)
                        .Append("       return false;" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.Close
                    'Client-side script for Close.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function Close(strReturnValue)" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       if (strReturnValue == '')" & vbCrLf)
                        .Append("       {" & vbCrLf)
                        .Append("           strReturnValue = null;" & vbCrLf)
                        .Append("       }" & vbCrLf)
                        .Append("       window.returnValue = strReturnValue;" & vbCrLf)
                        .Append("       window.close();" & vbCrLf)
                        .Append("       return false;" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.MsgBox
                    'Client-side script for MsgBox.
                    With New StringBuilder
                        .Append("<script language=""vbscript"">" & vbCrLf)
                        .Append("   Function ShowMsgBox(strPrompt, strTitle, intButtons, bolDoPostBack)" & vbCrLf)
                        .Append("       intResult = MsgBox(strPrompt, intButtons ,strTitle)" & vbCrLf)
                        .Append("       If bolDoPostBack Then" & vbCrLf)
                        .Append("           If (intResult = vbOK) Or (intResult = vbRetry) Or (intResult = vbYes) Then" & vbCrLf)
                        .Append("               ShowMsgBox = True" & vbCrLf)
                        .Append("           Else" & vbCrLf)
                        .Append("               ShowMsgBox = False" & vbCrLf)
                        .Append("           End If" & vbCrLf)
                        .Append("       Else" & vbCrLf)
                        .Append("           ShowMsgBox = False" & vbCrLf)
                        .Append("       End If" & vbCrLf)
                        .Append("   End Function" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.Refresh
                    'Client-side script for Refresh.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function Refresh()" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       window.location.href = unescape(window.location.pathname);" & vbCrLf)
                        .Append("       return false;" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.Maximize
                    'Client-side script for Maximize.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function Maximize()" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       window.moveTo(0, 0);" & vbCrLf)
                        .Append("       window.resizeTo(window.screen.availWidth, window.screen.availHeight);" & vbCrLf)
                        .Append("       return false;" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.Minimize
                    'Client-side script for Minimize.
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   function Minimize()" & vbCrLf)
                        .Append("   {" & vbCrLf)
                        .Append("       window.blur();" & vbCrLf)
                        .Append("       return false;" & vbCrLf)
                        .Append("   }" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
                Case ScriptCode.RunClientExe
                    'Client-side script for RunClientExe.
                    With New StringBuilder
                        .Append("<script language=""vbscript"">" & vbCrLf)
                        .Append("   Function RunClientExe(strExeToRun)" & vbCrLf)
                        .Append("       On Error Resume Next" & vbCrLf)
                        .Append("       Set oShell = CreateObject(""WScript.Shell"")" & vbCrLf)
                        .Append("       intReturnCode = oShell.Run(strExeToRun)" & vbCrLf)
                        .Append("       Set oShell = Nothing" & vbCrLf)
                        .Append("       If Err.number <> 0 Then" & vbCrLf)
                        .Append("           intResult = MsgBox(""Failed to execute "" & strExeToRun & ""."", 16, ""RunClientExe"")" & vbCrLf)
                        .Append("           RunClientExe = True" & vbCrLf)
                        .Append("       Else" & vbCrLf)
                        .Append("           RunClientExe = False" & vbCrLf)
                        .Append("       End If" & vbCrLf)
                        .Append("   End Function" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Return .ToString()
                    End With
            End Select

        End Function

#End Region

#Region "Common fuction to hookup client-side script to a control."

        Private Shared Sub HookupScriptToControl(ByVal Control As System.Web.UI.Control, ByVal Script As String, Optional ByVal Attribute As String = "OnClick")

            If TypeOf Control Is System.Web.UI.WebControls.Button Then
                CType(Control, System.Web.UI.WebControls.Button).Attributes.Add(Attribute, Script)
            ElseIf TypeOf Control Is System.Web.UI.WebControls.LinkButton Then
                CType(Control, System.Web.UI.WebControls.LinkButton).Attributes.Add(Attribute, Script)
            ElseIf TypeOf Control Is System.Web.UI.WebControls.ImageButton Then
                CType(Control, System.Web.UI.WebControls.ImageButton).Attributes.Add(Attribute, Script)
            ElseIf TypeOf Control Is System.Web.UI.WebControls.Image Then
                CType(Control, System.Web.UI.WebControls.Image).Attributes.Add(Attribute, Script)
            ElseIf TypeOf Control Is System.Web.UI.WebControls.Label Then
                CType(Control, System.Web.UI.WebControls.Label).Attributes.Add(Attribute, Script)
            ElseIf TypeOf Control Is System.Web.UI.WebControls.TextBox Then
                CType(Control, System.Web.UI.WebControls.TextBox).Attributes.Add(Attribute, Script)
            End If

        End Sub

#End Region

#Region "Code for Focus"

        'Pinal Patel 03/04/05: Sets the focus to a web control.
        Public Shared Sub Focus(ByVal Control As System.Web.UI.Control)

            If Not Control.Page.IsClientScriptBlockRegistered("Focus") Then
                Control.Page.RegisterClientScriptBlock("Focus", CreateClientSideScript(ScriptCode.Focus))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   Focus('" & Control.ClientID() & "');" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Control.Page.RegisterStartupScript("Focus:" & Control.ClientID() & Rnd(), .ToString())
            End With

        End Sub

#End Region

#Region "Code for DefaultButton"

        'Pinal Patel 03/04/05:  Assigns a default button (regular/link/image) for a textbox to be clicked 
        '                       when enter key is pressed in the textbox..
        Public Shared Sub DefaultButton(ByVal Textbox As System.Web.UI.WebControls.TextBox, ByVal Control As System.Web.UI.Control)

            If Not Control.Page.IsClientScriptBlockRegistered("DefaultButton") Then
                Control.Page.RegisterClientScriptBlock("DefaultButton", CreateClientSideScript(ScriptCode.DefaultButton))
            End If

            Textbox.Attributes.Add("OnKeyDown", "javascript:DefaultButton('" & Control.ClientID() & "')")

        End Sub

#End Region

#Region "Code for SmartText"

        'Pinal Patel 03/04/05: Show text inside a textbox that can be used to provide a hint.
        Public Shared Sub SmartText(ByVal Textbox As System.Web.UI.WebControls.TextBox, ByVal Text As String)

            With Textbox
                .Attributes.Add("Value", Text)
                .Attributes.Add("OnFocus", "this.select();")
                .Attributes.Add("OnBlur", "if(this.value == ''){this.value = '" & Text & "';}")
            End With

        End Sub

#End Region

#Region "Code for Show"

        'Pinal Patel 03/04/05: Shows web page as modeless dialog. Not tied to a web control.
        Public Shared Sub Show(ByVal Page As System.Web.UI.Page, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            If Not Page.IsClientScriptBlockRegistered("Show") Then
                Page.RegisterClientScriptBlock("Show", CreateClientSideScript(ScriptCode.Show))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   Show('" & Url & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & ");" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("Show:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/04/05: Shows a web page as modeless dialog. Tied to a web control.
        Public Shared Sub Show(ByVal Control As System.Web.UI.Control, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            If Not Control.Page.IsClientScriptBlockRegistered("Show") Then
                Control.Page.RegisterClientScriptBlock("Show", CreateClientSideScript(ScriptCode.Show))
            End If

            HookupScriptToControl(Control, "javascript:return(Show('" & Url & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & "))")

        End Sub

#End Region

#Region "Code for ShowDialog"

        'Pinal Patel 03/04/05:  Shows a web page as modal dialog. Not tied to a web control. Postback occurs 
        '                       only if a value is returned by the child window (displayed as dialog) and 
        '                       DialogResultHolder is not specified.
        Public Shared Sub ShowDialog(ByVal Page As System.Web.UI.Page, ByVal Url As String, Optional ByVal DialogResultHolder As System.Web.UI.Control = Nothing, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            If Not Page.IsClientScriptBlockRegistered("ShowDialog") Then
                Page.RegisterClientScriptBlock("ShowDialog", CreateClientSideScript(ScriptCode.ShowDialog))
            End If

            With New StringBuilder
                .Append("<input type=""hidden"" name=""TVA_EVENT_TARGET"" value="""" />" & vbCrLf)
                .Append("<input type=""hidden"" name=""TVA_EVENT_ARGUMENT"" value="""" />" & vbCrLf)
                .Append("<script language=""javascript"">" & vbCrLf)
                If Not DialogResultHolder Is Nothing Then
                    .Append("   ShowDialog('" & Url & "', '" & DialogResultHolder.ClientID() & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & ");" & vbCrLf)
                Else
                    .Append("   if (ShowDialog('" & Url & "', null, " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & "))" & vbCrLf)
                    .Append("   {" & vbCrLf)
                    Dim Control As System.Web.UI.Control
                    For Each Control In Page.Controls
                        If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                            .Append("       " & Control.ClientID() & ".TVA_EVENT_TARGET.value = 'ShowDialog';" & vbCrLf)
                            .Append("       " & Control.ClientID() & ".TVA_EVENT_ARGUMENT.value = '" & Url & "';" & vbCrLf)
                            .Append("       document." & Control.ClientID() & ".submit();" & vbCrLf)
                            Exit For
                        End If
                    Next
                    .Append("   }" & vbCrLf)
                End If
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("ShowDialog:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/04/05:  Shows a web page as modal dialog. Tied to a web control. Postback occurs only if 
        '                       a value is returned by the child window (displayed as dialog) and DialogResultHolder 
        '                       is not specified.
        Public Shared Sub ShowDialog(ByVal Control As System.Web.UI.Control, ByVal Url As String, Optional ByVal DialogResultHolder As System.Web.UI.Control = Nothing, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            If Not Control.Page.IsClientScriptBlockRegistered("ShowDialog") Then
                Control.Page.RegisterClientScriptBlock("ShowDialog", CreateClientSideScript(ScriptCode.ShowDialog))
            End If

            If Not DialogResultHolder Is Nothing Then
                HookupScriptToControl(Control, "javascript:return(ShowDialog('" & Url & "', '" & DialogResultHolder.ClientID() & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & "))")
            Else
                HookupScriptToControl(Control, "javascript:return(ShowDialog('" & Url & "', null, " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & "))")
            End If

        End Sub

#End Region

#Region "Code for ShowPopup"

        'Pinal Patel 03/04/05: Shows web page as old fashion popup window. Not tied to a web control.
        Public Shared Sub ShowPopup(ByVal Page As System.Web.UI.Page, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Scrollbars As Boolean = False, Optional ByVal Toolbar As Boolean = False, Optional ByVal Menubar As Boolean = False, Optional ByVal Location As Boolean = False, Optional ByVal Status As Boolean = False, Optional ByVal Directories As Boolean = False)

            If Not Page.IsClientScriptBlockRegistered("ShowPopup") Then
                Page.RegisterClientScriptBlock("ShowPopup", CreateClientSideScript(ScriptCode.ShowPopup))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   ShowPopup('" & Url & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Scrollbars)) & ", " & Math.Abs(CInt(Toolbar)) & ", " & Math.Abs(CInt(Menubar)) & ", " & Math.Abs(CInt(Location)) & ", " & Math.Abs(CInt(Status)) & ", " & Math.Abs(CInt(Directories)) & ");" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("ShowPopup:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/04/05: Shows web page as old fashion popup. Tied to a web control.
        Public Shared Sub ShowPopup(ByVal Control As System.Web.UI.Control, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Scrollbars As Boolean = False, Optional ByVal Toolbar As Boolean = False, Optional ByVal Menubar As Boolean = False, Optional ByVal Location As Boolean = False, Optional ByVal Status As Boolean = False, Optional ByVal Directories As Boolean = False)

            If Not Control.Page.IsClientScriptBlockRegistered("ShowPopup") Then
                Control.Page.RegisterClientScriptBlock("ShowPopup", CreateClientSideScript(ScriptCode.ShowPopup))
            End If

            HookupScriptToControl(Control, "javascript:return(ShowPopup('" & Url & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Scrollbars)) & ", " & Math.Abs(CInt(Toolbar)) & ", " & Math.Abs(CInt(Menubar)) & ", " & Math.Abs(CInt(Location)) & ", " & Math.Abs(CInt(Status)) & ", " & Math.Abs(CInt(Directories)) & "))")

        End Sub

#End Region

#Region "Code for Close"

        'Pinal Patel 03/04/05:  Closes a web page. Not tied to a web control. Returns a value to the parent 
        '                       window if any (used in conjunction to ShowDialog).
        Public Shared Sub Close(ByVal Page As System.Web.UI.Page, Optional ByVal ReturnValue As String = Nothing)

            If Not Page.IsClientScriptBlockRegistered("Close") Then
                Page.RegisterClientScriptBlock("Close", CreateClientSideScript(ScriptCode.Close))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   Close('" & ReturnValue & "');" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("Close:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/04/05:  Closes a web pages. Tied to a web control. Return a value to the parent 
        '                       window if any (used in conjunction with ShowDialog)
        Public Shared Sub Close(ByVal Control As System.Web.UI.Control, Optional ByVal ReturnValue As String = Nothing)

            If Not Control.Page.IsClientScriptBlockRegistered("Close") Then
                Control.Page.RegisterClientScriptBlock("Close", CreateClientSideScript(ScriptCode.Close))
            End If

            HookupScriptToControl(Control, "javascript:return(Close('" & ReturnValue & "'))")

        End Sub

#End Region

#Region "Code for MsgBox"

        'Pinal Patel 03/04/05: Enumeration to specify message box style.
        Public Enum MsgBoxStyle As Integer
            OKOnly = 0
            OKCancel = 1
            AbortRetryIgnore = 2
            YesNoCancel = 3
            YesNo = 4
            RetryCancel = 5

            Critical = 16
            Question = 32
            Exclamation = 48
            Information = 64

            DefaultButton1 = 0
            DefaultButton2 = 256
            DefaultButton3 = 512
            DefaultButton4 = 768

            ApplicationModal = 0
            SystemModal = 4096
        End Enum

        'Pinal Patel 03/04/05:  Show a message box similar to the one available in windows apps. 
        '                       Not tied to a web control. Postbacks when ok/retry/yes is clicked.
        Public Shared Sub MsgBox(ByVal Page As System.Web.UI.Page, ByVal Prompt As String, Optional ByVal Title As String = "Message Box", Optional ByVal Buttons As MsgBoxStyle = MsgBoxStyle.OKOnly, Optional ByVal DoPostBack As Boolean = True)

            If Not Page.IsClientScriptBlockRegistered("ShowMsgBox") Then
                Page.RegisterClientScriptBlock("ShowMsgBox", CreateClientSideScript(ScriptCode.MsgBox))
            End If

            With New StringBuilder
                .Append("<input type=""hidden"" name=""TVA_EVENT_TARGET"" value="""" />" & vbCrLf)
                .Append("<input type=""hidden"" name=""TVA_EVENT_ARGUMENT"" value="""" />" & vbCrLf)
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   if (ShowMsgBox('" & JavaScriptEncode(Prompt) & " ', '" & Title & "', " & Buttons & ", " & LCase(DoPostBack) & ")) " & vbCrLf)
                .Append("   {" & vbCrLf)
                Dim Control As System.Web.UI.Control
                For Each Control In Page.Controls
                    If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_TARGET.value = 'MsgBox';" & vbCrLf)
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_ARGUMENT.value = '" & Title & "';" & vbCrLf)
                        .Append("       document." & Control.ClientID() & ".submit();" & vbCrLf)
                        Exit For
                    End If
                Next
                .Append("   }" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("ShowMsgBox:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/04/05:  Show a message box similar to the one available in windows apps. 
        '                       Tied to a web control. Postbacks when ok/retry/yes is clicked.
        Public Shared Sub MsgBox(ByVal Control As System.Web.UI.Control, ByVal Prompt As String, Optional ByVal Title As String = "Message Box", Optional ByVal Buttons As MsgBoxStyle = MsgBoxStyle.OKOnly, Optional ByVal DoPostBack As Boolean = True)

            If Not Control.Page.IsClientScriptBlockRegistered("ShowMsgBox") Then
                Control.Page.RegisterClientScriptBlock("ShowMsgBox", CreateClientSideScript(ScriptCode.MsgBox))
            End If

            HookupScriptToControl(Control, "javascript:return(ShowMsgBox('" & JavaScriptEncode(Prompt) & " ', '" & Title & "', " & Buttons & ", " & LCase(DoPostBack) & "))")

        End Sub

#End Region

#Region "Code for Refresh"

        'Pinal Patel 03/08/05: Causes the web page to refresh. Not tied to a web control.
        Public Shared Sub Refresh(ByVal Page As System.Web.UI.Page, Optional ByVal PostRefresh As Boolean = False)

            If Not Page.IsClientScriptBlockRegistered("Refresh") Then
                Page.RegisterClientScriptBlock("Refresh", CreateClientSideScript(ScriptCode.Refresh))
            End If

            If PostRefresh Then
                If Not Page.IsStartupScriptRegistered("PostRefresh") Then
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   Refresh();" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Page.RegisterStartupScript("PostRefresh", .ToString())
                    End With
                End If
            Else
                If Not Page.IsClientScriptBlockRegistered("PreRefresh") Then
                    With New StringBuilder
                        .Append("<script language=""javascript"">" & vbCrLf)
                        .Append("   Refresh();" & vbCrLf)
                        .Append("</script>" & vbCrLf)

                        Page.RegisterClientScriptBlock("PreRefresh", .ToString())
                    End With
                End If
            End If

        End Sub

        'Pinal Patel 03/08/05: Causes the web page to refresh. Tied to a web control.
        Public Shared Sub Refresh(ByVal Control As System.Web.UI.Control)

            If Not Control.Page.IsClientScriptBlockRegistered("Refresh") Then
                Control.Page.RegisterClientScriptBlock("Refresh", CreateClientSideScript(ScriptCode.Refresh))
            End If

            HookupScriptToControl(Control, "javascript:return(Refresh())")

        End Sub

#End Region

#Region "Code for Maximize"

        'Pinal Patel 03/09/05: Maximizes the web page to available screen size. Not tied to a web control.
        Public Shared Sub Maximize(ByVal Page As System.Web.UI.Page)

            If Not Page.IsClientScriptBlockRegistered("Maximize") Then
                Page.RegisterClientScriptBlock("Maximize", CreateClientSideScript(ScriptCode.Maximize))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   Maximize();" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("Maximize:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/09/05: Maximizes the web page to available screen size. Tied to a web control.
        Public Shared Sub Maximize(ByVal Control As System.Web.UI.Control)

            If Not Control.Page.IsClientScriptBlockRegistered("Maximize") Then
                Control.Page.RegisterClientScriptBlock("Maximize", CreateClientSideScript(ScriptCode.Maximize))
            End If

            HookupScriptToControl(Control, "javascript:return(Maximize())")

        End Sub

#End Region

#Region "Code for Minimize"

        'Pinal Patel 03/09/05:  Performs a fake minimize by pushing the web page into the background.
        '                       Not tied to a web control.
        Public Shared Sub Minimize(ByVal Page As System.Web.UI.Page)

            If Not Page.IsClientScriptBlockRegistered("Minimize") Then
                Page.RegisterClientScriptBlock("Minimize", CreateClientSideScript(ScriptCode.Minimize))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   Minimize();" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("Minimize:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/09/05:  Performs a fake minimize by pushing the web page into the background.
        '                       Tied to a web control.
        Public Shared Sub Minimize(ByVal Control As System.Web.UI.Control)

            If Not Control.Page.IsClientScriptBlockRegistered("Minimize") Then
                Control.Page.RegisterClientScriptBlock("Minimize", CreateClientSideScript(ScriptCode.Minimize))
            End If

            HookupScriptToControl(Control, "javascript:return(Minimize())")

        End Sub

#End Region

#Region "Code for BringToFront"

        'Pinal Patel 03/14/05:  Brings the web page to the front. Not tied to a web control.
        Public Shared Sub BringToFront(ByVal Page As System.Web.UI.Page)

            If Not Page.IsStartupScriptRegistered("BringToFront") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   window.focus();" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Page.RegisterStartupScript("BringToFront", .ToString())
                End With
            End If

        End Sub

#End Region

#Region "Code for PushToBack"

        'Pinal Patel 03/14/05:  Push the web page back (like minimize). Not tied to a web control.
        Public Shared Sub PushToBack(ByVal Page As System.Web.UI.Page)

            If Not Page.IsStartupScriptRegistered("PushToBack") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   window.blur();" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Page.RegisterStartupScript("PushToBack", .ToString())
                End With
            End If

        End Sub

#End Region

#Region "Code for PlayBackgroundSound"

        'Pinal Patel 03/15/05:  Plays sound in the background. Not tied to a web control.
        Public Shared Sub PlayBackgroundSound(ByVal Page As System.Web.UI.Page, ByVal SoundFilename As String, Optional ByVal LoopCount As Integer = 0)

            If Not Page.IsStartupScriptRegistered("PlayBackgroundSound") Then
                With New StringBuilder
                    .Append("<BGSOUND SRC=""" & SoundFilename & """ LOOP=""" & LoopCount & """>" & vbCrLf)

                    Page.RegisterStartupScript("PlayBackgroundSound", .ToString())
                End With
            End If

        End Sub

#End Region

#Region "Code for RunClientExe"

        '03/22/05 Pinal Patel
        Public Shared Sub RunClientExe(ByVal Page As System.Web.UI.Page, ByVal Executable As String)

            If Not Page.IsClientScriptBlockRegistered("RunClientExe") Then
                Page.RegisterClientScriptBlock("RunClientExe", CreateClientSideScript(ScriptCode.RunClientExe))
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   RunClientExe('" & JavaScriptEncode(Executable) & "');" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("RunClientExe:" & Rnd(), .ToString())
            End With

        End Sub

        '03/22/05 Pinal Patel
        Public Shared Sub RunClientExe(ByVal Control As System.Web.UI.Control, ByVal Executable As String)

            If Not Control.Page.IsClientScriptBlockRegistered("RunClientExe") Then
                Control.Page.RegisterClientScriptBlock("RunClientExe", CreateClientSideScript(ScriptCode.RunClientExe))
            End If

            HookupScriptToControl(Control, "javascript:return(RunClientExe('" & JavaScriptEncode(Executable) & "'))")

        End Sub

#End Region

        Public Function HEXtoRGB(ByVal clr As String) As Array

            Dim clrR As String
            Dim clrG As String
            Dim clrB As String
            Dim rgb(2) As String

            clr = Right(clr, Len(clr) - 1)
            clrR = Left$(clr, 2)
            clrG = Mid$(clr, 3, 2)
            clrB = Right$(clr, 2)
            clr = CLng("&H" & clrB & clrG & clrR)
            clrB = (clr \ 65536) And &HFF
            clrG = (clr \ 256) And &HFF
            clrR = clr And &HFF
            rgb(0) = clrR
            rgb(1) = clrG
            rgb(2) = clrB

            Return rgb

        End Function

    End Class

End Namespace