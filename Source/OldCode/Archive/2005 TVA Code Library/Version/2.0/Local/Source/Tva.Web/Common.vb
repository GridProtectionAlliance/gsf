' 04-28-06

Imports System.Text

Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ' Performs JavaScript encoding on given string
    Public Shared Function JavaScriptEncode(ByVal text As String) As String

        text = Replace(text, "\", "\\")
        text = Replace(text, "'", "\'")
        text = Replace(text, """", "\""")
        text = Replace(text, Chr(8), "\b")
        text = Replace(text, Chr(9), "\t")
        text = Replace(text, Chr(10), "\r")
        text = Replace(text, Chr(12), "\f")
        text = Replace(text, Chr(13), "\n")

        Return text

    End Function

    ' Decodes JavaScript characters from given string
    Public Shared Function JavaScriptDecode(ByVal text As String) As String

        text = Replace(text, "\\", "\")
        text = Replace(text, "\'", "'")
        text = Replace(text, "\""", """")
        text = Replace(text, "\b", Chr(8))
        text = Replace(text, "\t", Chr(9))
        text = Replace(text, "\r", Chr(10))
        text = Replace(text, "\f", Chr(12))
        text = Replace(text, "\n", Chr(13))

        Return text

    End Function

    ' Ensures a string is compliant with cookie name requirements
    Public Shared Function ValidCookieName(ByVal text As String) As String

        text = Replace(text, "=", "")
        text = Replace(text, ";", "")
        text = Replace(text, ",", "")
        text = Replace(text, Chr(9), "")
        text = Replace(text, Chr(10), "")
        text = Replace(text, Chr(13), "")

        Return text

    End Function

    ' Ensures a string is compliant with cookie value requirements
    Public Shared Function ValidCookieValue(ByVal text As String) As String

        text = Replace(text, ";", "")
        text = Replace(text, ",", "")

        Return text

    End Function

#Region " Focus "

    'Pinal Patel 03/04/05: Sets the focus to a web control.
    Public Shared Sub Focus(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Focus") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Focus", _
                CreateClientSideScript(ClientSideScript.Focus))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   Focus('" & control.ClientID() & "');" & vbCrLf)
            .Append("</script>" & vbCrLf)

            control.Page.ClientScript.RegisterStartupScript(Control.Page.GetType(), _
                "Focus." & Control.ClientID(), .ToString())
        End With

    End Sub

#End Region

#Region " DefaultButton "

    'Pinal Patel 03/04/05:  Assigns a default button (regular/link/image) for a textbox to be clicked 
    '                       when enter key is pressed in the textbox..
    Public Shared Sub DefaultButton(ByVal textbox As System.Web.UI.WebControls.TextBox, ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("DefaultButton") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "DefaultButton", _
                CreateClientSideScript(ClientSideScript.DefaultButton))
        End If

        textbox.Attributes.Add("OnKeyDown", "javascript:DefaultButton('" & control.ClientID() & "')")

    End Sub

#End Region

#Region " SmartText "

    'Pinal Patel 03/04/05: Show text inside a textbox that can be used to provide a hint.
    Public Shared Sub SmartText(ByVal textbox As System.Web.UI.WebControls.TextBox, ByVal text As String)

        With textbox
            .Attributes.Add("Value", text)
            .Attributes.Add("OnFocus", "this.select();")
            .Attributes.Add("OnBlur", "if(this.value == ''){this.value = '" & text & "';}")
        End With

    End Sub

#End Region

#Region " Show Overloads "

    'Pinal Patel 03/04/05: Shows web page as modeless dialog. Not tied to a web control.
    Public Shared Sub Show(ByVal page As System.Web.UI.Page, ByVal url As String, ByVal height As Integer, _
            ByVal width As Integer, ByVal left As Integer, ByVal top As Integer, ByVal center As Boolean, _
            ByVal help As Boolean, ByVal resizable As Boolean, ByVal status As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Show") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Show", _
                CreateClientSideScript(ClientSideScript.Show))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   Show('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & ");" & vbCrLf)
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "Show:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/04/05: Shows a web page as modeless dialog. Tied to a web control.
    Public Shared Sub Show(ByVal control As System.Web.UI.Control, ByVal url As String, ByVal height As Integer, _
            ByVal width As Integer, ByVal left As Integer, ByVal top As Integer, ByVal center As Boolean, _
            ByVal help As Boolean, ByVal resizable As Boolean, ByVal status As Boolean)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Show") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Show", _
                CreateClientSideScript(ClientSideScript.Show))
        End If

        HookupScriptToControl(control, "javascript:return(Show('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))")

    End Sub

#End Region

#Region " ShowDialog Overloads "

    'Pinal Patel 03/04/05:  Shows a web page as modal dialog. Not tied to a web control. Postback occurs 
    '                       only if a value is returned by the child window (displayed as dialog) and 
    '                       DialogResultHolder is not specified.
    Public Shared Sub ShowDialog(ByVal page As System.Web.UI.Page, ByVal url As String, ByVal dialogResultHolder As System.Web.UI.Control, _
            ByVal height As Integer, ByVal width As Integer, ByVal left As Integer, ByVal Top As Integer, _
            ByVal center As Boolean, ByVal help As Boolean, ByVal resizable As Boolean, ByVal status As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("ShowDialog") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowDialog", _
                CreateClientSideScript(ClientSideScript.ShowDialog))
        End If

        With New StringBuilder
            .Append("<input type=""hidden"" name=""TVA_EVENT_TARGET"" value="""" />" & vbCrLf)
            .Append("<input type=""hidden"" name=""TVA_EVENT_ARGUMENT"" value="""" />" & vbCrLf)
            .Append("<script language=""javascript"">" & vbCrLf)
            If Not dialogResultHolder Is Nothing Then
                .Append("   ShowDialog('" & url & "', '" & dialogResultHolder.ClientID() & "', " & height & ", " & width & ", " & left & ", " & Top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & ");" & vbCrLf)
            Else
                .Append("   if (ShowDialog('" & url & "', null, " & height & ", " & width & ", " & left & ", " & Top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))" & vbCrLf)
                .Append("   {" & vbCrLf)
                Dim Control As System.Web.UI.Control
                For Each Control In page.Controls
                    If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_TARGET.value = 'ShowDialog';" & vbCrLf)
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_ARGUMENT.value = '" & url & "';" & vbCrLf)
                        .Append("       document." & Control.ClientID() & ".submit();" & vbCrLf)
                        Exit For
                    End If
                Next
                .Append("   }" & vbCrLf)
            End If
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "ShowDialog:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/04/05:  Shows a web page as modal dialog. Tied to a web control. Postback occurs only if 
    '                       a value is returned by the child window (displayed as dialog) and DialogResultHolder 
    '                       is not specified.
    Public Shared Sub ShowDialog(ByVal control As System.Web.UI.Control, ByVal url As String, Optional ByVal dialogResultHolder As System.Web.UI.Control = Nothing, Optional ByVal height As Integer = 400, Optional ByVal width As Integer = 600, Optional ByVal left As Integer = 0, Optional ByVal top As Integer = 0, Optional ByVal center As Boolean = True, Optional ByVal help As Boolean = True, Optional ByVal resizable As Boolean = False, Optional ByVal status As Boolean = False)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("ShowDialog") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowDialog", _
                CreateClientSideScript(ClientSideScript.ShowDialog))
        End If

        If Not dialogResultHolder Is Nothing Then
            HookupScriptToControl(control, "javascript:return(ShowDialog('" & url & "', '" & dialogResultHolder.ClientID() & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))")
        Else
            HookupScriptToControl(control, "javascript:return(ShowDialog('" & url & "', null, " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))")
        End If

    End Sub

#End Region

#Region " ShowPopup Overloads "

    'Pinal Patel 03/04/05: Shows web page as old fashion popup window. Not tied to a web control.
    Public Shared Sub ShowPopup(ByVal page As System.Web.UI.Page, ByVal url As String, ByVal height As Integer, _
            ByVal width As Integer, ByVal left As Integer, ByVal top As Integer, ByVal center As Boolean, _
            ByVal resizable As Boolean, ByVal scrollbars As Boolean, ByVal toolbar As Boolean, _
            ByVal menubar As Boolean, ByVal location As Boolean, ByVal status As Boolean, ByVal directories As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("ShowPopup") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowPopup", _
                CreateClientSideScript(ClientSideScript.ShowPopup))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   ShowPopup('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(scrollbars)) & ", " & Math.Abs(CInt(toolbar)) & ", " & Math.Abs(CInt(menubar)) & ", " & Math.Abs(CInt(location)) & ", " & Math.Abs(CInt(status)) & ", " & Math.Abs(CInt(directories)) & ");" & vbCrLf)
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "ShowPopup:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/04/05: Shows web page as old fashion popup. Tied to a web control.
    Public Shared Sub ShowPopup(ByVal control As System.Web.UI.Control, ByVal url As String, ByVal height As Integer, _
            ByVal width As Integer, ByVal left As Integer, ByVal top As Integer, ByVal center As Boolean, _
            ByVal resizable As Boolean, ByVal scrollbars As Boolean, ByVal toolbar As Boolean, _
            ByVal menubar As Boolean, ByVal location As Boolean, ByVal status As Boolean, ByVal directories As Boolean)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("ShowPopup") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowPopup", _
                CreateClientSideScript(ClientSideScript.ShowPopup))
        End If

        HookupScriptToControl(control, "javascript:return(ShowPopup('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(scrollbars)) & ", " & Math.Abs(CInt(toolbar)) & ", " & Math.Abs(CInt(menubar)) & ", " & Math.Abs(CInt(location)) & ", " & Math.Abs(CInt(status)) & ", " & Math.Abs(CInt(directories)) & "))")

    End Sub

#End Region

#Region " Close Overloads "

    'Pinal Patel 03/04/05:  Closes a web page. Not tied to a web control. Returns a value to the parent 
    '                       window if any (used in conjunction to ShowDialog).
    Public Shared Sub Close(ByVal page As System.Web.UI.Page, ByVal returnValue As String)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Close") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Close", _
                CreateClientSideScript(ClientSideScript.Close))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   Close('" & returnValue & "');" & vbCrLf)
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "Close:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/04/05:  Closes a web pages. Tied to a web control. Return a value to the parent 
    '                       window if any (used in conjunction with ShowDialog)
    Public Shared Sub Close(ByVal control As System.Web.UI.Control, ByVal returnValue As String)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Close") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Close", CreateClientSideScript(ClientSideScript.Close))
        End If

        HookupScriptToControl(control, "javascript:return(Close('" & returnValue & "'))")

    End Sub

#End Region

#Region " MsgBox Overloads "

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
    Public Shared Sub MsgBox(ByVal page As System.Web.UI.Page, ByVal prompt As String, ByVal title As String, _
            ByVal buttons As MsgBoxStyle, ByVal doPostBack As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowMsgBox", CreateClientSideScript(ClientSideScript.MsgBox))
        End If

        With New StringBuilder
            .Append("<input type=""hidden"" name=""TVA_EVENT_TARGET"" value="""" />" & vbCrLf)
            .Append("<input type=""hidden"" name=""TVA_EVENT_ARGUMENT"" value="""" />" & vbCrLf)
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   if (ShowMsgBox('" & JavaScriptEncode(prompt) & " ', '" & title & "', " & buttons & ", " & LCase(doPostBack) & ")) " & vbCrLf)
            .Append("   {" & vbCrLf)
            Dim Control As System.Web.UI.Control
            For Each Control In page.Controls
                If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                    .Append("       " & Control.ClientID() & ".TVA_EVENT_TARGET.value = 'MsgBox';" & vbCrLf)
                    .Append("       " & Control.ClientID() & ".TVA_EVENT_ARGUMENT.value = '" & title & "';" & vbCrLf)
                    .Append("       document." & Control.ClientID() & ".submit();" & vbCrLf)
                    Exit For
                End If
            Next
            .Append("   }" & vbCrLf)
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "ShowMsgBox:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/04/05:  Show a message box similar to the one available in windows apps. 
    '                       Tied to a web control. Postbacks when ok/retry/yes is clicked.
    Public Shared Sub MsgBox(ByVal control As System.Web.UI.Control, ByVal prompt As String, ByVal title As String, _
            ByVal buttons As MsgBoxStyle, ByVal doPostBack As Boolean)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowMsgBox", _
                CreateClientSideScript(ClientSideScript.MsgBox))
        End If

        HookupScriptToControl(control, "javascript:return(ShowMsgBox('" & JavaScriptEncode(prompt) & " ', '" & title & "', " & buttons & ", " & LCase(doPostBack) & "))")

    End Sub

#End Region

#Region "Code for Refresh"

    'Pinal Patel 03/08/05: Causes the web page to refresh. Not tied to a web control.
    Public Shared Sub Refresh(ByVal page As System.Web.UI.Page, ByVal postRefresh As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Refresh") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Refresh", _
                CreateClientSideScript(ClientSideScript.Refresh))
        End If

        If postRefresh Then
            If Not page.ClientScript.IsStartupScriptRegistered("PostRefresh") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   Refresh();" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    page.ClientScript.RegisterStartupScript(page.GetType(), "PostRefresh", .ToString())
                End With
            End If
        Else
            If Not page.ClientScript.IsClientScriptBlockRegistered("PreRefresh") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & vbCrLf)
                    .Append("   Refresh();" & vbCrLf)
                    .Append("</script>" & vbCrLf)

                    page.ClientScript.RegisterClientScriptBlock(page.GetType(), "PreRefresh", .ToString())
                End With
            End If
        End If

    End Sub

    'Pinal Patel 03/08/05: Causes the web page to refresh. Tied to a web control.
    Public Shared Sub Refresh(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Refresh") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Refresh", _
                CreateClientSideScript(ClientSideScript.Refresh))
        End If

        HookupScriptToControl(Control, "javascript:return(Refresh())")

    End Sub

#End Region

#Region "Code for Maximize"

    'Pinal Patel 03/09/05: Maximizes the web page to available screen size. Not tied to a web control.
    Public Shared Sub Maximize(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Maximize") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Maximize", _
                CreateClientSideScript(ClientSideScript.Maximize))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   Maximize();" & vbCrLf)
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "Maximize:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/09/05: Maximizes the web page to available screen size. Tied to a web control.
    Public Shared Sub Maximize(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Maximize") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Maximize", _
                CreateClientSideScript(ClientSideScript.Maximize))
        End If

        HookupScriptToControl(Control, "javascript:return(Maximize())")

    End Sub

#End Region

#Region "Code for Minimize"

    'Pinal Patel 03/09/05:  Performs a fake minimize by pushing the web page into the background.
    '                       Not tied to a web control.
    Public Shared Sub Minimize(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Minimize") Then
            page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "Minimize", _
                CreateClientSideScript(ClientSideScript.Minimize))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   Minimize();" & vbCrLf)
            .Append("</script>" & vbCrLf)

            Page.ClientScript.RegisterStartupScript(Page.GetType(), "Minimize:" & Rnd(), .ToString())
        End With

    End Sub

    'Pinal Patel 03/09/05:  Performs a fake minimize by pushing the web page into the background.
    '                       Tied to a web control.
    Public Shared Sub Minimize(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Minimize") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Minimize", _
                CreateClientSideScript(ClientSideScript.Minimize))
        End If

        HookupScriptToControl(Control, "javascript:return(Minimize())")

    End Sub

#End Region

#Region "Code for BringToFront"

    'Pinal Patel 03/14/05:  Brings the web page to the front. Not tied to a web control.
    Public Shared Sub BringToFront(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsStartupScriptRegistered("BringToFront") Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   window.focus();" & vbCrLf)
                .Append("</script>" & vbCrLf)

                page.ClientScript.RegisterStartupScript(page.GetType(), "BringToFront", .ToString())
            End With
        End If

    End Sub

#End Region

#Region "Code for PushToBack"

    'Pinal Patel 03/14/05:  Push the web page back (like minimize). Not tied to a web control.
    Public Shared Sub PushToBack(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsStartupScriptRegistered("PushToBack") Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & vbCrLf)
                .Append("   window.blur();" & vbCrLf)
                .Append("</script>" & vbCrLf)

                page.ClientScript.RegisterStartupScript(page.GetType(), "PushToBack", .ToString())
            End With
        End If

    End Sub

#End Region

#Region "Code for PlayBackgroundSound"

    'Pinal Patel 03/15/05:  Plays sound in the background. Not tied to a web control.
    Public Shared Sub PlayBackgroundSound(ByVal page As System.Web.UI.Page, ByVal soundFilename As String, _
            ByVal loopCount As Integer)

        If Not page.ClientScript.IsStartupScriptRegistered("PlayBackgroundSound") Then
            With New StringBuilder
                .Append("<BGSOUND SRC=""" & soundFilename & """ LOOP=""" & loopCount & """>" & vbCrLf)

                page.ClientScript.RegisterStartupScript(page.GetType(), "PlayBackgroundSound", .ToString())
            End With
        End If

    End Sub

#End Region

#Region "Code for RunClientExe"

    '03/22/05 Pinal Patel
    Public Shared Sub RunClientExe(ByVal page As System.Web.UI.Page, ByVal executable As String)

        If Not page.ClientScript.IsClientScriptBlockRegistered("RunClientExe") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "RunClientExe", _
                CreateClientSideScript(ClientSideScript.RunClientExe))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & vbCrLf)
            .Append("   RunClientExe('" & JavaScriptEncode(Executable) & "');" & vbCrLf)
            .Append("</script>" & vbCrLf)

            page.ClientScript.RegisterStartupScript(page.GetType(), "RunClientExe:" & Rnd(), .ToString())
        End With

    End Sub

    '03/22/05 Pinal Patel
    Public Shared Sub RunClientExe(ByVal control As System.Web.UI.Control, ByVal executable As String)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("RunClientExe") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "RunClientExe", _
                CreateClientSideScript(ClientSideScript.RunClientExe))
        End If

        HookupScriptToControl(control, "javascript:return(RunClientExe('" & JavaScriptEncode(Executable) & "'))")

    End Sub

#End Region

#Region " Helpers "

    Private Enum ClientSideScript As Integer

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

    Private Shared Function CreateClientSideScript(ByVal script As ClientSideScript) As String

        Select Case script
            Case ClientSideScript.Focus
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
            Case ClientSideScript.DefaultButton
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
            Case ClientSideScript.Show
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
            Case ClientSideScript.ShowDialog
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
            Case ClientSideScript.ShowPopup
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
            Case ClientSideScript.Close
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
            Case ClientSideScript.MsgBox
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
            Case ClientSideScript.Refresh
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
            Case ClientSideScript.Maximize
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
            Case ClientSideScript.Minimize
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
            Case ClientSideScript.RunClientExe
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

End Class
