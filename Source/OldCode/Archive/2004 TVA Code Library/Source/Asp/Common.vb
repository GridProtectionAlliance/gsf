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

#Region "Code for Focus"

        'Pinal Patel 03/04/05: Sets the focus to a web control.
        Public Shared Sub Focus(ByVal Control As System.Web.UI.Control)

            If Not Control.Page.IsClientScriptBlockRegistered("Focus") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   function Focus(varControlName)" & vbCrLf)
                    .Append("   {" & vbCrLf)
                    .Append("       if (document.getElementById(varControlName) != null)" & vbCrLf)
                    .Append("       {" & vbCrLf)
                    .Append("           document.getElementById(varControlName).focus();" & vbCrLf)
                    .Append("       }" & vbCrLf)
                    .Append("   }" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Control.Page.RegisterClientScriptBlock("Focus", .ToString())
                End With
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

            Textbox.Attributes.Add("OnKeyDown", "if (event.keyCode == 13) {event.returnValue = false; event.cancel = true; document.all." & Control.ClientID & ".click();}")

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
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   function Show(varUrl, varHeight, varWidth, varLeft, varTop, varCenter, varHelp, varResizable, varStatus)" & vbCrLf)
                    .Append("   {" & vbCrLf)
                    .Append("       window.showModelessDialog(varUrl, window.self,'dialogWidth:' + varWidth + 'px;dialogHeight:' + varHeight + 'px;left:' + varLeft + ';top:' + varTop + ';center:' + varCenter + ';help:' + varHelp + ';resizable:' + varResizable + ';status:' + varStatus);")
                    .Append("   }" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Page.RegisterClientScriptBlock("Show", .ToString())
                End With
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   Show('" & Url & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & ");" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("Show:" & Rnd(), .ToString())
            End With

        End Sub

        'Shows a web page as modeless dialog. Tied to a web control.
        Public Shared Sub Show(ByVal Control As System.Web.UI.Control, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            Dim strShowScript As String = "window.showModelessDialog('" & Url & "', window.self,'dialogWidth:" & Width & " px;dialogHeight:" & Height & "px;left:" & Left & ";top:" & Top & ";center:" & Math.Abs(CInt(Center)) & ";help:" & Math.Abs(CInt(Help)) & ";resizable:" & Math.Abs(CInt(Resizable)) & ";status:" & Math.Abs(CInt(Status)) & "'); return (false);"

            Select Case Control.GetType.ToString()
                Case "System.Web.UI.WebControls.Button"
                    CType(Control, System.Web.UI.WebControls.Button).Attributes.Add("onclick", strShowScript)
                Case "System.Web.UI.WebControls.LinkButton"
                    CType(Control, System.Web.UI.WebControls.LinkButton).Attributes.Add("onclick", strShowScript)
                Case "System.Web.UI.WebControls.ImageButton"
                    CType(Control, System.Web.UI.WebControls.ImageButton).Attributes.Add("onclick", strShowScript)
                Case "System.Web.UI.WebControls.TextBox"
                    CType(Control, System.Web.UI.WebControls.TextBox).Attributes.Add("onclick", strShowScript)
                Case "System.Web.UI.WebControls.Image"
                    CType(Control, System.Web.UI.WebControls.Image).Attributes.Add("onclick", strShowScript)
            End Select

        End Sub

#End Region

#Region "Code for ShowDialog"

        'Pinal Patel 03/04/05:  Shows a web page as modal dialog. Not tied to a web control. Postback occurs 
        '                       only if a value is returned by the child window (displayed as dialog) and 
        '                       DialogResultHolder is not specified.
        Public Shared Sub ShowDialog(ByVal Page As System.Web.UI.Page, ByVal Url As String, Optional ByVal DialogResultHolder As System.Web.UI.Control = Nothing, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            If Not Page.IsClientScriptBlockRegistered("ShowDialog") Then
                With New StringBuilder
                    .Append("<input type=""hidden"" name=""EVENT_TARGET"" value=""ShowDialog"" />" & vbCrLf)
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   function ShowDialog(varUrl, varResultHolderName, varFormName, varHeight, varWidth, varLeft, varTop, varCenter, varHelp, varResizable, varStatus)" & vbCrLf)
                    .Append("   {" & vbCrLf)
                    .Append("       varReturnValue = window.showModalDialog(varUrl, window.self,'dialogWidth:' + varWidth + 'px;dialogHeight:' + varHeight + 'px;left:' + varLeft + ';top:' + varTop + ';center:' + varCenter + ';help:' + varHelp + ';resizable:' + varResizable + ';status:' + varStatus);" & vbCrLf)
                    .Append("       if ((varResultHolderName == null) && (varReturnValue != null)) {document.writeln('<script language=\'javascript\'>document.' + varFormName + '.submit()<\/script>');}" & vbCrLf)
                    .Append("       else if ((varResultHolderName != null) && (varReturnValue != null)) {document.getElementById(varResultHolderName).value = varReturnValue;}" & vbCrLf)
                    .Append("   }" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Page.RegisterClientScriptBlock("ShowDialog", .ToString())
                End With
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                If DialogResultHolder Is Nothing Then
                    Dim Control As System.Web.UI.Control
                    For Each Control In Page.Controls
                        If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                            .Append("   ShowDialog('" & Url & "', null, '" & Control.ClientID() & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & ");" & vbCrLf)
                            Exit For
                        End If
                    Next
                Else
                    .Append("   ShowDialog('" & Url & "', '" & DialogResultHolder.ClientID() & "', null, " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Help)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Status)) & ");" & vbCrLf)
                End If
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("ShowDialog:" & Rnd(), .ToString())
            End With

        End Sub

        'Shows a web page as modal dialog. Tied to a web control. Postback occurs only if a value is returned by the child window (displayed as dialog) and DialogResultHolder is not specified.
        Public Shared Sub ShowDialog(ByVal Control As System.Web.UI.Control, ByVal Url As String, Optional ByVal DialogResultHolder As System.Web.UI.Control = Nothing, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Help As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Status As Boolean = False)

            Dim strShowDialogScript As String
            If DialogResultHolder Is Nothing Then
                strShowDialogScript = "varReturnValue = window.showModalDialog('" & Url & "', window.self,'dialogWidth:" & Width & " px;dialogHeight:" & Height & "px;left:" & Left & ";top:" & Top & ";center:" & Math.Abs(CInt(Center)) & ";help:" & Math.Abs(CInt(Help)) & ";resizable:" & Math.Abs(CInt(Resizable)) & ";status:" & Math.Abs(CInt(Status)) & "'); if (varReturnValue != null) {return (true);} else {return (false);}"
            Else
                strShowDialogScript = "varReturnValue = window.showModalDialog('" & Url & "', window.self,'dialogWidth:" & Width & " px;dialogHeight:" & Height & "px;left:" & Left & ";top:" & Top & ";center:" & Math.Abs(CInt(Center)) & ";help:" & Math.Abs(CInt(Help)) & ";resizable:" & Math.Abs(CInt(Resizable)) & ";status:" & Math.Abs(CInt(Status)) & "'); if (varReturnValue != null) {document.all." & DialogResultHolder.ClientID() & ".value = varReturnValue;} return (false);"
            End If

            Select Case Control.GetType.ToString()
                Case "System.Web.UI.WebControls.Button"
                    CType(Control, System.Web.UI.WebControls.Button).Attributes.Add("onclick", strShowDialogScript)
                Case "System.Web.UI.WebControls.LinkButton"
                    CType(Control, System.Web.UI.WebControls.LinkButton).Attributes.Add("onclick", strShowDialogScript)
                Case "System.Web.UI.WebControls.ImageButton"
                    CType(Control, System.Web.UI.WebControls.ImageButton).Attributes.Add("onclick", strShowDialogScript)
                Case "System.Web.UI.WebControls.TextBox"
                    CType(Control, System.Web.UI.WebControls.TextBox).Attributes.Add("onclick", strShowDialogScript)
                Case "System.Web.UI.WebControls.Image"
                    CType(Control, System.Web.UI.WebControls.Image).Attributes.Add("onclick", strShowDialogScript)
            End Select

        End Sub

#End Region

#Region "Code for ShowPopup"

        'Pinal Patel 03/04/05: Shows web page as old fashion popup window. Not tied to a web control.
        Public Shared Sub ShowPopup(ByVal Page As System.Web.UI.Page, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Scrollbars As Boolean = False, Optional ByVal Toolbar As Boolean = False, Optional ByVal Menubar As Boolean = False, Optional ByVal Location As Boolean = False, Optional ByVal Status As Boolean = False, Optional ByVal Directories As Boolean = False)

            If Not Page.IsClientScriptBlockRegistered("ShowPopup") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   function ShowPopup(varUrl, varHeight, varWidth, varLeft, varTop, varCenter, varResizable, varScrollbars, varToolbar, varMenubar, varLocation, varStatus, varDirectories)" & vbCrLf)
                    .Append("   {" & vbCrLf)
                    .Append("       if (varCenter) {varPopup = window.open(varUrl, 'Popup' + Math.floor(Math.random() * 101), 'height=' + varHeight + ',width=' + varWidth + ',top=' + ((screen.availHeight / 2) - (varHeight / 2)) + ',left=' + ((screen.availWidth / 2) - (varWidth / 2)) + ',resizable=' + varResizable + ',scrollbars=' + varScrollbars + ',toolbar=' + varToolbar + ',menubar=' + varMenubar + ',location=' + varLocation + ',status=' + varStatus + ',directories=' + varDirectories);}" & vbCrLf)
                    .Append("       else {varPopup = window.open(varUrl, 'Popup' + Math.floor(Math.random() * 101), 'height=' + varHeight + ',width=' + varWidth + ',top=' + varTop + ',left=' + varLeft + ',resizable=' + varResizable + ',scrollbars=' + varScrollbars + ',toolbar=' + varToolbar + ',menubar=' + varMenubar + ',location=' + varLocation + ',status=' + varStatus + ',directories=' + varDirectories);}" & vbCrLf)
                    .Append("       if (window.focus) {varPopup.focus();}" & vbCrLf)
                    .Append("   }" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Page.RegisterClientScriptBlock("ShowPopup", .ToString())
                End With
            End If

            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   ShowPopup('" & Url & "', " & Height & ", " & Width & ", " & Left & ", " & Top & ", " & Math.Abs(CInt(Center)) & ", " & Math.Abs(CInt(Resizable)) & ", " & Math.Abs(CInt(Scrollbars)) & ", " & Math.Abs(CInt(Toolbar)) & ", " & Math.Abs(CInt(Menubar)) & ", " & Math.Abs(CInt(Location)) & ", " & Math.Abs(CInt(Status)) & ", " & Math.Abs(CInt(Directories)) & ");" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("ShowPopup:" & Rnd(), .ToString())
            End With

        End Sub

        'Shows web page as old fashion popup. Tied to a web control.
        Public Shared Sub ShowPopup(ByVal Control As System.Web.UI.Control, ByVal Url As String, Optional ByVal Height As Integer = 400, Optional ByVal Width As Integer = 600, Optional ByVal Left As Integer = 0, Optional ByVal Top As Integer = 0, Optional ByVal Center As Boolean = True, Optional ByVal Resizable As Boolean = False, Optional ByVal Scrollbars As Boolean = False, Optional ByVal Toolbar As Boolean = False, Optional ByVal Menubar As Boolean = False, Optional ByVal Location As Boolean = False, Optional ByVal Status As Boolean = False, Optional ByVal Directories As Boolean = False)

            Dim strShowPopupScript As String
            If Center Then
                strShowPopupScript = "varPopup = window.open('" & Url & "', 'Popup', 'height=" & Height & ",width=" & Width & ",top=' + ((screen.availHeight / 2) - " & (Height / 2) & ") + ',left=' + ((screen.availWidth / 2) - " & (Width / 2) & ") + ',resizable=" & Math.Abs(CInt(Resizable)) & ",scrollbars=" & Math.Abs(CInt(Scrollbars)) & ",toolbar=" & Math.Abs(CInt(Toolbar)) & ",menubar=" & Math.Abs(CInt(Menubar)) & ",location=" & Math.Abs(CInt(Location)) & ",status=" & Math.Abs(CInt(Status)) & ",directories=" & Math.Abs(CInt(Directories)) & "'); if (window.focus) {varPopup.focus();} return (false);"
            Else
                strShowPopupScript = "varPopup = window.open('" & Url & "', 'Popup', 'height=" & Height & ",width=" & Width & ",top=" & Top & ",left=" & Left & ",resizable=" & Math.Abs(CInt(Resizable)) & ",scrollbars=" & Math.Abs(CInt(Scrollbars)) & ",toolbar=" & Math.Abs(CInt(Toolbar)) & ",menubar=" & Math.Abs(CInt(Menubar)) & ",location=" & Math.Abs(CInt(Location)) & ",status=" & Math.Abs(CInt(Status)) & ",directories=" & Math.Abs(CInt(Directories)) & "'); if (window.focus) {varPopup.focus();} return (false);"
            End If

            Select Case Control.GetType.ToString()
                Case "System.Web.UI.WebControls.Button"
                    CType(Control, System.Web.UI.WebControls.Button).Attributes.Add("onclick", strShowPopupScript)
                Case "System.Web.UI.WebControls.LinkButton"
                    CType(Control, System.Web.UI.WebControls.LinkButton).Attributes.Add("onclick", strShowPopupScript)
                Case "System.Web.UI.WebControls.ImageButton"
                    CType(Control, System.Web.UI.WebControls.ImageButton).Attributes.Add("onclick", strShowPopupScript)
                Case "System.Web.UI.WebControls.TextBox"
                    CType(Control, System.Web.UI.WebControls.TextBox).Attributes.Add("onclick", strShowPopupScript)
                Case "System.Web.UI.WebControls.Image"
                    CType(Control, System.Web.UI.WebControls.Image).Attributes.Add("onclick", strShowPopupScript)
            End Select

        End Sub

#End Region

#Region "Code for Close"

        'Pinal Patel 03/04/05:  Closes a web page. Not tied to a web control. Returns a value to the parent 
        '                       window if any (used in conjunction to ShowDialog).
        Public Shared Sub Close(ByVal Page As System.Web.UI.Page, Optional ByVal ReturnValue As String = Nothing)

            If Not Page.IsStartupScriptRegistered("Close") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    If ReturnValue Is Nothing Then
                        .Append("   window.returnValue = null;" & vbCrLf)
                    Else
                        .Append("   window.returnValue = '" & ReturnValue & "';" & vbCrLf)
                    End If
                    .Append("   window.close();" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    Page.RegisterStartupScript("Close", .ToString())
                End With
            End If

        End Sub

        'Pinal Patel 03/04/05:  Closes a web pages. Tied to a web control. Return a value to the parent 
        '                       window if any (used in conjunction with ShowDialog)
        Public Shared Sub Close(ByVal Control As System.Web.UI.Control, Optional ByVal ReturnValue As String = Nothing)

            Dim strCloseScript As String
            If ReturnValue Is Nothing Then
                strCloseScript = "window.returnValue = null; window.close();"
            Else
                strCloseScript = "window.returnValue = '" & ReturnValue & "'; window.close();"
            End If

            Select Case Control.GetType.ToString()
                Case "System.Web.UI.WebControls.Button"
                    CType(Control, System.Web.UI.WebControls.Button).Attributes.Add("onclick", strCloseScript)
                Case "System.Web.UI.WebControls.LinkButton"
                    CType(Control, System.Web.UI.WebControls.LinkButton).Attributes.Add("onclick", strCloseScript)
                Case "System.Web.UI.WebControls.ImageButton"
                    CType(Control, System.Web.UI.WebControls.ImageButton).Attributes.Add("onclick", strCloseScript)
                Case "System.Web.UI.WebControls.TextBox"
                    CType(Control, System.Web.UI.WebControls.TextBox).Attributes.Add("onclick", strCloseScript)
                Case "System.Web.UI.WebControls.Image"
                    CType(Control, System.Web.UI.WebControls.Image).Attributes.Add("onclick", strCloseScript)
            End Select

        End Sub

#End Region

#Region "Code for MsgBox"

        'Pinal Patel 03/04/05:Enumeration to specify message box style
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

        Private Shared Function CreateMsgBoxFunction() As String

            'Pinal Patel 03/04/05:  Creates a VB Script function that shows a message box and returns a boolean 
            '                       indicating whether or not a postback is to be performed.
            With New StringBuilder
                .Append("<script language=""vbscript"">" & vbCrLf)
                .Append("   Function ShowMsgBox(strPrompt, strTitle, intButtons, bolDoPostBack)" & vbCrLf)
                .Append("       varResult = MsgBox(strPrompt, intButtons ,strTitle)" & vbCrLf)
                .Append("       If bolDoPostBack Then" & vbCrLf)
                .Append("           If (varResult = vbOK) Or (varResult = vbRetry) Or (varResult = vbYes) Then ShowMsgBox = True Else ShowMsgBox = False" & vbCrLf)
                .Append("       Else" & vbCrLf)
                .Append("           ShowMsgBox = False" & vbCrLf)
                .Append("       End If" & vbCrLf)
                .Append("   End Function" & vbCrLf)
                .Append("</script>" & vbCrLf)

                Return .ToString()
            End With

        End Function

        'Pinal Patel 03/04/05:  Show a message box similar to the one available in windows apps. 
        '                       Not tied to a web control. Postbacks when ok/retry/yes is clicked.
        Public Shared Sub MsgBox(ByVal Page As System.Web.UI.Page, ByVal Prompt As String, Optional ByVal Title As String = "Message Box", Optional ByVal Buttons As MsgBoxStyle = MsgBoxStyle.OKOnly, Optional ByVal DoPostBack As Boolean = True)

            If Not Page.IsClientScriptBlockRegistered("ShowMsgBox") Then
                Page.RegisterClientScriptBlock("ShowMsgBox", CreateMsgBoxFunction())
            End If

            Prompt = Regex.Replace(Prompt, vbCrLf, "\n")
            With New StringBuilder
                .Append("<input type=""hidden"" name=""EVENT_TARGET"" value=""MsgBox"" />" & vbCrLf)
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("if (ShowMsgBox('" & Prompt & " ', '" & Title & "', " & Buttons & ", " & LCase(DoPostBack) & ")) " & vbCrLf)
                Dim Control As System.Web.UI.Control
                For Each Control In Page.Controls
                    If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                        .Append("{document." & Control.ClientID() & ".submit();}" & vbCrLf)
                        Exit For
                    End If
                Next
                .Append("</script>" & vbCrLf)

                Page.RegisterStartupScript("ShowMsgBox:" & Rnd(), .ToString())
            End With

        End Sub

        'Pinal Patel 03/04/05:  Show a message box similar to the one available in windows apps. 
        '                       Tied to a web control. Postbacks when ok/retry/yes is clicked.
        Public Shared Sub MsgBox(ByVal Control As System.Web.UI.Control, ByVal Prompt As String, Optional ByVal Title As String = "Message Box", Optional ByVal Buttons As MsgBoxStyle = MsgBoxStyle.OKOnly, Optional ByVal DoPostBack As Boolean = True)

            If Not Control.Page.IsClientScriptBlockRegistered("ShowMsgBox") Then
                Control.Page.RegisterClientScriptBlock("ShowMsgBox", CreateMsgBoxFunction())
            End If

            Prompt = Regex.Replace(Prompt, vbCrLf, "\n")
            Dim strMsgBox As String = "return(ShowMsgBox('" & Prompt & " ', '" & Title & "', " & Buttons & ", " & LCase(DoPostBack) & "));"

            Select Case Control.GetType.ToString()
                Case "System.Web.UI.WebControls.Button"
                    CType(Control, System.Web.UI.WebControls.Button).Attributes.Add("onclick", strMsgBox)
                Case "System.Web.UI.WebControls.LinkButton"
                    CType(Control, System.Web.UI.WebControls.LinkButton).Attributes.Add("onclick", strMsgBox)
                Case "System.Web.UI.WebControls.ImageButton"
                    CType(Control, System.Web.UI.WebControls.ImageButton).Attributes.Add("onclick", strMsgBox)
                Case "System.Web.UI.WebControls.TextBox"
                    CType(Control, System.Web.UI.WebControls.TextBox).Attributes.Add("onclick", strMsgBox)
                Case "System.Web.UI.WebControls.Image"
                    CType(Control, System.Web.UI.WebControls.Image).Attributes.Add("onclick", strMsgBox)
            End Select

        End Sub

#End Region

    End Class

End Namespace