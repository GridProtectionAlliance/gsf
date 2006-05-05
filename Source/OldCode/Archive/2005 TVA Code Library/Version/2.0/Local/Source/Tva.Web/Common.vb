'*******************************************************************************************************
'  Tva.Web.Commmon.vb - Common Functions for Web Pages
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
'  03/04/2005 - Pinal C. Patel
'       Original version of source code generated
'  04/28/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Asp.Common)
'
'*******************************************************************************************************

Imports System.Text
Imports System.Environment

Public NotInheritable Class Common

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ' Performs JavaScript encoding on given string
    Public Shared Function JavaScriptEncode(ByVal text As String) As String

        text = Replace(text, "\", "\\")
        text = Replace(text, "'", "\'")
        text = Replace(text, """", "\""")
        text = Replace(text, Convert.ToChar(8), "\b")
        text = Replace(text, Convert.ToChar(9), "\t")
        text = Replace(text, Convert.ToChar(10), "\r")
        text = Replace(text, Convert.ToChar(12), "\f")
        text = Replace(text, Convert.ToChar(13), "\n")

        Return text

    End Function

    ' Decodes JavaScript characters from given string
    Public Shared Function JavaScriptDecode(ByVal text As String) As String

        text = Replace(text, "\\", "\")
        text = Replace(text, "\'", "'")
        text = Replace(text, "\""", """")
        text = Replace(text, "\b", Convert.ToChar(8))
        text = Replace(text, "\t", Convert.ToChar(9))
        text = Replace(text, "\r", Convert.ToChar(10))
        text = Replace(text, "\f", Convert.ToChar(12))
        text = Replace(text, "\n", Convert.ToChar(13))

        Return text

    End Function

    ' Ensures a string is compliant with cookie name requirements
    Public Shared Function ValidCookieName(ByVal text As String) As String

        text = Replace(text, "=", "")
        text = Replace(text, ";", "")
        text = Replace(text, ",", "")
        text = Replace(text, Convert.ToChar(9), "")
        text = Replace(text, Convert.ToChar(10), "")
        text = Replace(text, Convert.ToChar(13), "")

        Return text

    End Function

    ' Ensures a string is compliant with cookie value requirements
    Public Shared Function ValidCookieValue(ByVal text As String) As String

        text = Replace(text, ";", "")
        text = Replace(text, ",", "")

        Return text

    End Function

    'Pinal Patel 03/04/05: Sets the focus to a web control.
    Public Shared Sub Focus(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Focus") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Focus", _
                CreateClientSideScript(ClientSideScript.Focus))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & NewLine())
            .Append("   Focus('" & control.ClientID() & "');" & NewLine())
            .Append("</script>" & NewLine())

            If Not control.Page.ClientScript.IsStartupScriptRegistered("Focus." & control.ClientID()) Then
                control.Page.ClientScript.RegisterStartupScript(control.Page.GetType(), _
                    "Focus." & control.ClientID(), .ToString())
            End If
        End With

    End Sub

    'Pinal Patel 03/04/05:  Assigns a default button (regular/link/image) for a textbox to be clicked 
    '                       when enter key is pressed in the textbox..
    Public Shared Sub DefaultButton(ByVal textbox As System.Web.UI.WebControls.TextBox, ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("DefaultButton") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "DefaultButton", _
                CreateClientSideScript(ClientSideScript.DefaultButton))
        End If

        textbox.Attributes.Add("OnKeyDown", "javascript:DefaultButton('" & control.ClientID() & "')")

    End Sub

    'Pinal Patel 03/04/05: Show text inside a textbox that can be used to provide a hint.
    Public Shared Sub SmartText(ByVal textbox As System.Web.UI.WebControls.TextBox, ByVal text As String)

        With textbox
            .Attributes.Add("Value", text)
            .Attributes.Add("OnFocus", "this.select();")
            .Attributes.Add("OnBlur", "if(this.value == ''){this.value = '" & text & "';}")
        End With

    End Sub

    'Pinal Patel 03/04/05: Shows web page as modeless dialog. Not tied to a web control.
    Public Shared Sub Show(ByVal page As System.Web.UI.Page, ByVal url As String, ByVal height As Integer, _
            ByVal width As Integer, ByVal left As Integer, ByVal top As Integer, ByVal center As Boolean, _
            ByVal help As Boolean, ByVal resizable As Boolean, ByVal status As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Show") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Show", _
                CreateClientSideScript(ClientSideScript.Show))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & NewLine())
            .Append("   Show('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & ");" & NewLine())
            .Append("</script>" & NewLine())

            If Not page.ClientScript.IsStartupScriptRegistered("Show." & url) Then
                page.ClientScript.RegisterStartupScript(page.GetType(), "Show." & url, .ToString())
            End If
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

        HookupScriptToControl(control, "javascript:return(Show('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))", "OnClick")

    End Sub

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
            .Append("<input type=""hidden"" name=""TVA_EVENT_TARGET"" value="""" />" & NewLine())
            .Append("<input type=""hidden"" name=""TVA_EVENT_ARGUMENT"" value="""" />" & NewLine())
            .Append("<script language=""javascript"">" & NewLine())
            If Not dialogResultHolder Is Nothing Then
                .Append("   ShowDialog('" & url & "', '" & dialogResultHolder.ClientID() & "', " & height & ", " & width & ", " & left & ", " & Top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & ");" & NewLine())
            Else
                .Append("   if (ShowDialog('" & url & "', null, " & height & ", " & width & ", " & left & ", " & Top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))" & NewLine())
                .Append("   {" & NewLine())
                Dim Control As System.Web.UI.Control
                For Each Control In page.Controls
                    If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_TARGET.value = 'ShowDialog';" & NewLine())
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_ARGUMENT.value = '" & url & "';" & NewLine())
                        .Append("       document." & Control.ClientID() & ".submit();" & NewLine())
                        Exit For
                    End If
                Next
                .Append("   }" & NewLine())
            End If
            .Append("</script>" & NewLine())

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
            HookupScriptToControl(control, "javascript:return(ShowDialog('" & url & "', '" & dialogResultHolder.ClientID() & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))", "OnClick")
        Else
            HookupScriptToControl(control, "javascript:return(ShowDialog('" & url & "', null, " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(help)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(status)) & "))", "OnClick")
        End If

    End Sub

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
            .Append("<script language=""javascript"">" & NewLine())
            .Append("   ShowPopup('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(scrollbars)) & ", " & Math.Abs(CInt(toolbar)) & ", " & Math.Abs(CInt(menubar)) & ", " & Math.Abs(CInt(location)) & ", " & Math.Abs(CInt(status)) & ", " & Math.Abs(CInt(directories)) & ");" & NewLine())
            .Append("</script>" & NewLine())

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

        HookupScriptToControl(control, "javascript:return(ShowPopup('" & url & "', " & height & ", " & width & ", " & left & ", " & top & ", " & Math.Abs(CInt(center)) & ", " & Math.Abs(CInt(resizable)) & ", " & Math.Abs(CInt(scrollbars)) & ", " & Math.Abs(CInt(toolbar)) & ", " & Math.Abs(CInt(menubar)) & ", " & Math.Abs(CInt(location)) & ", " & Math.Abs(CInt(status)) & ", " & Math.Abs(CInt(directories)) & "))", "OnClick")

    End Sub

    ''' <summary>
    ''' Closes the current web page when it has finished loading in the browser and returns the specified value to 
    ''' the web page that opened it.
    ''' </summary>
    ''' <param name="page">The current web page.</param>
    ''' <param name="returnValue">The value to be returned to the parent web page that open this web page.</param>
    ''' <remarks></remarks>
    Public Shared Sub Close(ByVal page As System.Web.UI.Page, ByVal returnValue As String)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Close") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Close", _
                CreateClientSideScript(ClientSideScript.Close))
        End If

        If Not page.ClientScript.IsStartupScriptRegistered("Close.Page") Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & NewLine())
                .Append("   Close('" & returnValue & "');" & NewLine())
                .Append("</script>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "Close.Page", .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Closes the current web page when the specified web control is clicked and returns the specified value to the 
    ''' web page that opened it.
    ''' </summary>
    ''' <param name="control">The web control that, when clicked, will close the current web page.</param>
    ''' <param name="returnValue">The value to be returned to the parent web page that open this web page.</param>
    ''' <remarks></remarks>
    Public Shared Sub Close(ByVal control As System.Web.UI.Control, ByVal returnValue As String)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Close") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Close", CreateClientSideScript(ClientSideScript.Close))
        End If

        HookupScriptToControl(control, "javascript:return(Close('" & returnValue & "'))", "OnClick")

    End Sub

    ''' <summary>
    ''' The buttons to be displayed in the message box.
    ''' </summary>
    ''' <remarks></remarks>
    <Flags()> _
    Public Enum MsgBoxStyle As Integer
        ''' <summary>
        ''' OK Button.
        ''' </summary>
        ''' <remarks></remarks>
        OKOnly = 0
        ''' <summary>
        ''' OK and Cancel buttons.
        ''' </summary>
        ''' <remarks></remarks>
        OKCancel = 1
        ''' <summary>
        ''' Abort,Retry, and Ignore buttons.
        ''' </summary>
        ''' <remarks></remarks>
        AbortRetryIgnore = 2
        ''' <summary>
        ''' Yes, No, and Cancel buttons.
        ''' </summary>
        ''' <remarks></remarks>
        YesNoCancel = 3
        ''' <summary>
        ''' Yes and No buttons.
        ''' </summary>
        ''' <remarks></remarks>
        YesNo = 4
        ''' <summary>
        ''' Retry and Cancel buttons.
        ''' </summary>
        ''' <remarks></remarks>
        RetryCancel = 5
        ''' <summary>
        ''' Critical message.
        ''' </summary>
        ''' <remarks></remarks>
        Critical = 16
        ''' <summary>
        ''' Warning query.
        ''' </summary>
        ''' <remarks></remarks>
        Question = 32
        ''' <summary>
        ''' Warning message.
        ''' </summary>
        ''' <remarks></remarks>
        Exclamation = 48
        ''' <summary>
        ''' Information message.
        ''' </summary>
        ''' <remarks></remarks>
        Information = 64
        ''' <summary>
        ''' First button is default.
        ''' </summary>
        ''' <remarks></remarks>
        DefaultButton1 = 0
        ''' <summary>
        ''' Second button is default.
        ''' </summary>
        ''' <remarks></remarks>
        DefaultButton2 = 256
        ''' <summary>
        ''' Third button is default.
        ''' </summary>
        ''' <remarks></remarks>
        DefaultButton3 = 512
        ''' <summary>
        ''' Fourth button is default.
        ''' </summary>
        ''' <remarks></remarks>
        DefaultButton4 = 768
        ''' <summary>
        ''' Application modal message box.
        ''' </summary>
        ''' <remarks></remarks>
        ApplicationModal = 0
        ''' <summary>
        ''' System modal message box.
        ''' </summary>
        ''' <remarks></remarks>
        SystemModal = 4096
    End Enum

    ''' <summary>
    ''' Shows a windows application style message box when the specified web page is loaded.
    ''' </summary>
    ''' <param name="page">The web page that will show the message box when loaded.</param>
    ''' <param name="prompt">The text that is to be displayed in the message box.</param>
    ''' <param name="title">The title of the message box.</param>
    ''' <param name="buttons">The buttons to be displayed in the message box.</param>
    ''' <param name="doPostBack">
    ''' True if a post-back is to be performed when either OK, Retry, or Yes buttons are clicked in the message box; 
    ''' otherwise False.
    ''' </param>
    ''' <remarks></remarks>
    Public Shared Sub MsgBox(ByVal page As System.Web.UI.Page, ByVal prompt As String, ByVal title As String, _
            ByVal buttons As MsgBoxStyle, ByVal doPostBack As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "ShowMsgBox", CreateClientSideScript(ClientSideScript.MsgBox))
        End If

        If Not page.ClientScript.IsStartupScriptRegistered("ShowMsgBox." & prompt) Then
            With New StringBuilder
                .Append("<input type=""hidden"" name=""TVA_EVENT_TARGET"" value="""" />" & NewLine())
                .Append("<input type=""hidden"" name=""TVA_EVENT_ARGUMENT"" value="""" />" & NewLine())
                .Append("<script language=""javascript"">" & NewLine())
                .Append("   if (ShowMsgBox('" & JavaScriptEncode(prompt) & " ', '" & title & "', " & buttons & ", " & LCase(doPostBack) & ")) " & NewLine())
                .Append("   {" & NewLine())
                Dim Control As System.Web.UI.Control
                For Each Control In page.Controls
                    If TypeOf Control Is System.Web.UI.HtmlControls.HtmlForm Then
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_TARGET.value = 'MsgBox';" & NewLine())
                        .Append("       " & Control.ClientID() & ".TVA_EVENT_ARGUMENT.value = '" & title & "';" & NewLine())
                        .Append("       document." & Control.ClientID() & ".submit();" & NewLine())
                        Exit For
                    End If
                Next
                .Append("   }" & NewLine())
                .Append("</script>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "ShowMsgBox." & prompt, .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Shows a windows application style message box when the specified web control is clicked.
    ''' </summary>
    ''' <param name="control">The web control that will show the message box when clicked.</param>
    ''' <param name="prompt">The text that is to be displayed in the message box.</param>
    ''' <param name="title">The title of the message box.</param>
    ''' <param name="buttons">The buttons to be displayed in the message box.</param>
    ''' <param name="doPostBack">
    ''' True if a post-back is to be performed when either OK, Retry, or Yes buttons are clicked in the message box; 
    ''' otherwise False.
    ''' </param>
    ''' <remarks></remarks>
    Public Shared Sub MsgBox(ByVal control As System.Web.UI.Control, ByVal prompt As String, ByVal title As String, _
            ByVal buttons As MsgBoxStyle, ByVal doPostBack As Boolean)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("ShowMsgBox") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "ShowMsgBox", _
                CreateClientSideScript(ClientSideScript.MsgBox))
        End If

        HookupScriptToControl(control, "javascript:return(ShowMsgBox('" & JavaScriptEncode(prompt) & " ', '" & _
            title & "', " & buttons & ", " & LCase(doPostBack) & "))", "OnClick")

    End Sub

    ''' <summary>
    ''' Refreshes the web page as soon as it is loaded.
    ''' </summary>
    ''' <param name="page">The web page that is to be refreshed.</param>
    ''' <param name="postRefresh">
    ''' True if the web page is to be refreshed after the entire web page is rendered and loaded; otherwise False.
    ''' </param>
    ''' <remarks></remarks>
    Public Shared Sub Refresh(ByVal page As System.Web.UI.Page, ByVal postRefresh As Boolean)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Refresh") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Refresh", _
                CreateClientSideScript(ClientSideScript.Refresh))
        End If

        If postRefresh Then
            If Not page.ClientScript.IsStartupScriptRegistered("PostRefresh") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   Refresh();" & NewLine())
                    .Append("</script>" & NewLine())

                    page.ClientScript.RegisterStartupScript(page.GetType(), "PostRefresh", .ToString())
                End With
            End If
        Else
            If Not page.ClientScript.IsClientScriptBlockRegistered("PreRefresh") Then
                With New StringBuilder
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   Refresh();" & NewLine())
                    .Append("</script>" & NewLine())

                    page.ClientScript.RegisterClientScriptBlock(page.GetType(), "PreRefresh", .ToString())
                End With
            End If
        End If

    End Sub

    ''' <summary>
    ''' Refreshes the web page when the specified web control is clicked.
    ''' </summary>
    ''' <param name="control">The web control that will refersh the web page when clicked.</param>
    ''' <remarks></remarks>
    Public Shared Sub Refresh(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Refresh") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Refresh", _
                CreateClientSideScript(ClientSideScript.Refresh))
        End If

        HookupScriptToControl(control, "javascript:return(Refresh())", "OnClick")

    End Sub

    ''' <summary>
    ''' Maximizes the browser window when the specified web page is loaded.
    ''' </summary>
    ''' <param name="page">The web page whose browser window is to be maximized when loaded.</param>
    ''' <remarks></remarks>
    Public Shared Sub Maximize(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Maximize") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Maximize", _
                CreateClientSideScript(ClientSideScript.Maximize))
        End If

        With New StringBuilder
            .Append("<script language=""javascript"">" & NewLine())
            .Append("   Maximize();" & NewLine())
            .Append("</script>" & NewLine())

            page.ClientScript.RegisterStartupScript(page.GetType(), "Maximize:" & Rnd(), .ToString())
        End With

    End Sub

    ''' <summary>
    ''' Maximizes the browser window when the specified web control is clicked.
    ''' </summary>
    ''' <param name="control">The web control that will maximize the browser window when clicked.</param>
    ''' <remarks></remarks>
    Public Shared Sub Maximize(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Maximize") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Maximize", _
                CreateClientSideScript(ClientSideScript.Maximize))
        End If

        HookupScriptToControl(control, "javascript:return(Maximize())", "OnClick")

    End Sub

    ''' <summary>
    ''' Minimizes the browser windows when the specified web page is loaded.
    ''' </summary>
    ''' <param name="page">The web page whose browser window is to be minimized when loaded.</param>
    ''' <remarks></remarks>
    Public Shared Sub Minimize(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsClientScriptBlockRegistered("Minimize") Then
            page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "Minimize", _
                CreateClientSideScript(ClientSideScript.Minimize))
        End If

        If page.ClientScript.IsStartupScriptRegistered("Minimize.Page") Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & NewLine())
                .Append("   Minimize();" & NewLine())
                .Append("</script>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "Minimize.Page", .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Minimizes the browser window when the specified web control is clicked.
    ''' </summary>
    ''' <param name="control">The web control that will minimize the browser window when clicked.</param>
    ''' <remarks></remarks>
    Public Shared Sub Minimize(ByVal control As System.Web.UI.Control)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("Minimize") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "Minimize", _
                CreateClientSideScript(ClientSideScript.Minimize))
        End If

        HookupScriptToControl(control, "javascript:return(Minimize())", "OnClick")

    End Sub

    ''' <summary>
    ''' Brings the browser window in which the specified web page is loaded to the foreground.
    ''' </summary>
    ''' <param name="page">The web page whose browser window is to be brought to the foreground.</param>
    ''' <remarks></remarks>
    Public Shared Sub BringToFront(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsStartupScriptRegistered("BringToFront") Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & NewLine())
                .Append("   window.focus();" & NewLine())
                .Append("</script>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "BringToFront", .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Pushes the browser window in which the specified web page is loaded to the background.
    ''' </summary>
    ''' <param name="page">The web page whose browser window is to be pushed to the background.</param>
    ''' <remarks></remarks>
    Public Shared Sub PushToBack(ByVal page As System.Web.UI.Page)

        If Not page.ClientScript.IsStartupScriptRegistered("PushToBack") Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & NewLine())
                .Append("   window.blur();" & NewLine())
                .Append("</script>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "PushToBack", .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Plays audio in the background when the specified web page is loaded.
    ''' </summary>
    ''' <param name="page">The web page in which audio is to be played.</param>
    ''' <param name="soundFilename">Name of the audio file.</param>
    ''' <param name="repeatCount">Number of times the audio is to be replayed.</param>
    ''' <remarks></remarks>
    Public Shared Sub PlayBackgroundSound(ByVal page As System.Web.UI.Page, ByVal soundFilename As String, _
            ByVal repeatCount As Integer)

        If Not page.ClientScript.IsStartupScriptRegistered("PlayBackgroundSound") Then
            With New StringBuilder
                .Append("<BGSOUND SRC=""" & soundFilename & """ LOOP=""" & repeatCount & """>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "PlayBackgroundSound", .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Runs the specified executable on the client's machine when the specified web page finishes loading in the browser.
    ''' </summary>
    ''' <param name="page">The web page that will run the executable.</param>
    ''' <param name="executable">Name of the executable to run.</param>
    ''' <remarks></remarks>
    Public Shared Sub RunClientExe(ByVal page As System.Web.UI.Page, ByVal executable As String)

        If Not page.ClientScript.IsClientScriptBlockRegistered("RunClientExe") Then
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "RunClientExe", _
                CreateClientSideScript(ClientSideScript.RunClientExe))
        End If

        If Not page.ClientScript.IsStartupScriptRegistered("RunClientExe." & executable) Then
            With New StringBuilder
                .Append("<script language=""javascript"">" & NewLine())
                .Append("   RunClientExe('" & JavaScriptEncode(executable) & "');" & NewLine())
                .Append("</script>" & NewLine())

                page.ClientScript.RegisterStartupScript(page.GetType(), "RunClientExe." & executable, .ToString())
            End With
        End If

    End Sub

    ''' <summary>
    ''' Runs the specified executable on the client's machine when the specified web control is clicked.
    ''' </summary>
    ''' <param name="control">The web control that will run the executable.</param>
    ''' <param name="executable">Name of the executable to run.</param>
    ''' <remarks></remarks>
    Public Shared Sub RunClientExe(ByVal control As System.Web.UI.Control, ByVal executable As String)

        If Not control.Page.ClientScript.IsClientScriptBlockRegistered("RunClientExe") Then
            control.Page.ClientScript.RegisterClientScriptBlock(control.Page.GetType(), "RunClientExe", _
                CreateClientSideScript(ClientSideScript.RunClientExe))
        End If

        HookupScriptToControl(control, "javascript:return(RunClientExe('" & JavaScriptEncode(executable) & "'))", _
            "OnClick")

    End Sub

#Region " Helpers "

    ''' <summary>
    ''' The different types of client-side scripts.
    ''' </summary>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Creates the appropriate client-side script based on the specified Tva.Web.ClientSideScript value.
    ''' </summary>
    ''' <param name="script">One of the Tva.Web.ClientSideScript values.</param>
    ''' <returns>The client-side script for the specified Tva.Web.ClientSideScript value</returns>
    ''' <remarks></remarks>
    Private Shared Function CreateClientSideScript(ByVal script As ClientSideScript) As String

        Dim clientScript As String = ""
        Select Case script
            Case ClientSideScript.Focus         ' Client-side script for Focus.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function Focus(controlId)" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       if (document.getElementById(controlId) != null)" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("           document.getElementById(controlId).focus();" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.DefaultButton ' Client-side script for DefaultButton.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function DefaultButton(controlId)" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       if (event.keyCode == 13)" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("           event.returnValue = false;" & NewLine())
                    .Append("           event.cancel = true;" & NewLine())
                    .Append("           if (document.getElementById(controlId) != null)" & NewLine())
                    .Append("           {" & NewLine())
                    .Append("               document.getElementById(controlId).click();" & NewLine())
                    .Append("           }" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.Show          ' Client-side script for Show.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function Show(url, height, width, left, top, center, help, resizable, status)" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       window.showModelessDialog(url, window.self,'dialogWidth:' + width + 'px;dialogHeight:' + height + 'px;left:' + left + ';top:' + top + ';center:' + center + ';help:' + help + ';resizable:' + resizable + ';status:' + status);" & NewLine())
                    .Append("       return false;" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.ShowDialog    'Client-side script for ShowDialog.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function ShowDialog(url, resultHolderId, height, width, left, top, center, help, resizable, status)" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       returnValue = window.showModalDialog(url, window.self,'dialogWidth:' + width + 'px;dialogHeight:' + height + 'px;left:' + left + ';top:' + top + ';center:' + center + ';help:' + help + ';resizable:' + resizable + ';status:' + status);" & NewLine())
                    .Append("       if (returnValue != null)" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("           if ((resultHolderId != null) && (document.getElementById(resultHolderId) != null))" & NewLine())
                    .Append("           {" & NewLine())
                    .Append("               document.getElementById(resultHolderId).value = returnValue;" & NewLine())
                    .Append("               return false;" & NewLine())
                    .Append("           }" & NewLine())
                    .Append("           else" & NewLine())
                    .Append("           {" & NewLine())
                    .Append("               return true;" & NewLine())
                    .Append("           }" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("       else" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("           return false;" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.ShowPopup     ' Client-side script for ShowPopup.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function ShowPopup(url, height, width, left, top, center, resizable, scrollbars, toolbar, menubar, location, status, directories)" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       specialCharacters = /([^a-zA-Z0-9\s])/gi;" & NewLine())
                    .Append("       popupName = url.replace(specialCharacters, '');" & NewLine())
                    .Append("       if (center)" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("           popup = window.open(url, popupName, 'height=' + height + ',width=' + width + ',top=' + ((screen.availHeight / 2) - (height / 2)) + ',left=' + ((screen.availWidth / 2) - (width / 2)) + ',resizable=' + resizable + ',scrollbars=' + scrollbars + ',toolbar=' + toolbar + ',menubar=' + menubar + ',location=' + location + ',status=' + status + ',directories=' + directories);" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("       else" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("            popup = window.open(url, popupName, 'height=' + height + ',width=' + width + ',top=' + top + ',left=' + left + ',resizable=' + resizable + ',scrollbars=' + scrollbars + ',toolbar=' + toolbar + ',menubar=' + menubar + ',location=' + location + ',status=' + status + ',directories=' + directories);" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("       popup.focus();" & NewLine())
                    .Append("       return false;" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.Close         ' Client-side script for Close.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function Close(returnValue)" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       if (returnValue == '')" & NewLine())
                    .Append("       {" & NewLine())
                    .Append("           returnValue = null;" & NewLine())
                    .Append("       }" & NewLine())
                    .Append("       window.returnValue = returnValue;" & NewLine())
                    .Append("       window.close();" & NewLine())
                    .Append("       return false;" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.MsgBox        ' Client-side script for MsgBox.
                With New StringBuilder()
                    .Append("<script language=""vbscript"">" & NewLine())
                    .Append("   Function ShowMsgBox(prompt, title, buttons, doPostBack)" & NewLine())
                    .Append("       result = MsgBox(prompt, buttons ,title)" & NewLine())
                    .Append("       If doPostBack Then" & NewLine())
                    .Append("           If (result = vbOK) Or (result = vbRetry) Or (result = vbYes) Then" & NewLine())
                    .Append("               ShowMsgBox = True" & NewLine())
                    .Append("           Else" & NewLine())
                    .Append("               ShowMsgBox = False" & NewLine())
                    .Append("           End If" & NewLine())
                    .Append("       Else" & NewLine())
                    .Append("           ShowMsgBox = False" & NewLine())
                    .Append("       End If" & NewLine())
                    .Append("   End Function" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.Refresh       ' Client-side script for Refresh.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function Refresh()" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       window.location.href = unescape(window.location.pathname);" & NewLine())
                    .Append("       return false;" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.Maximize      ' Client-side script for Maximize.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function Maximize()" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       window.moveTo(0, 0);" & NewLine())
                    .Append("       window.resizeTo(window.screen.availWidth, window.screen.availHeight);" & NewLine())
                    .Append("       return false;" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.Minimize      ' Client-side script for Minimize.
                With New StringBuilder()
                    .Append("<script language=""javascript"">" & NewLine())
                    .Append("   function Minimize()" & NewLine())
                    .Append("   {" & NewLine())
                    .Append("       window.blur();" & NewLine())
                    .Append("       return false;" & NewLine())
                    .Append("   }" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
            Case ClientSideScript.RunClientExe  ' Client-side script for RunClientExe.
                With New StringBuilder()
                    .Append("<script language=""vbscript"">" & NewLine())
                    .Append("   Function RunClientExe(exeToRun)" & NewLine())
                    .Append("       On Error Resume Next" & NewLine())
                    .Append("       Set shell = CreateObject(""WScript.Shell"")" & NewLine())
                    .Append("       returnCode = shell.Run(exeToRun)" & NewLine())
                    .Append("       Set shell = Nothing" & NewLine())
                    .Append("       If Err.number <> 0 Then" & NewLine())
                    .Append("           result = MsgBox(""Failed to execute "" & exeToRun & ""."", 16, ""RunClientExe"")" & NewLine())
                    .Append("           RunClientExe = True" & NewLine())
                    .Append("       Else" & NewLine())
                    .Append("           RunClientExe = False" & NewLine())
                    .Append("       End If" & NewLine())
                    .Append("   End Function" & NewLine())
                    .Append("</script>" & NewLine())

                    clientScript = .ToString()
                End With
        End Select
        Return clientScript

    End Function

    ''' <summary>
    ''' Associates client-side script to a web control.
    ''' </summary>
    ''' <param name="control">The control to which the client-side script is to be associated.</param>
    ''' <param name="script">The script that is to be associated with the control.</param>
    ''' <param name="attribute">Attribute of the control through which the client-side script is to be associated with the control.</param>
    ''' <remarks></remarks>
    Private Shared Sub HookupScriptToControl(ByVal control As System.Web.UI.Control, ByVal script As String, _
            ByVal attribute As String)

        If TypeOf control Is System.Web.UI.WebControls.Button Then
            DirectCast(control, System.Web.UI.WebControls.Button).Attributes.Add(attribute, script)
        ElseIf TypeOf control Is System.Web.UI.WebControls.LinkButton Then
            DirectCast(control, System.Web.UI.WebControls.LinkButton).Attributes.Add(attribute, script)
        ElseIf TypeOf control Is System.Web.UI.WebControls.ImageButton Then
            DirectCast(control, System.Web.UI.WebControls.ImageButton).Attributes.Add(attribute, script)
        ElseIf TypeOf control Is System.Web.UI.WebControls.Image Then
            DirectCast(control, System.Web.UI.WebControls.Image).Attributes.Add(attribute, script)
        ElseIf TypeOf control Is System.Web.UI.WebControls.Label Then
            DirectCast(control, System.Web.UI.WebControls.Label).Attributes.Add(attribute, script)
        ElseIf TypeOf control Is System.Web.UI.WebControls.TextBox Then
            DirectCast(control, System.Web.UI.WebControls.TextBox).Attributes.Add(attribute, script)
        End If

    End Sub

#End Region

End Class
