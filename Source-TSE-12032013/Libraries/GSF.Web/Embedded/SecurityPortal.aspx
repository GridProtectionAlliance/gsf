<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SecurityPortal.aspx.cs"
    Inherits="GSF.Web.Embedded.SecurityPortal" Theme="" StylesheetTheme="" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link runat="server" id="StyleSheet" href="~/Styles/SecurityPortal.css" rel="stylesheet"
        type="text/css" />
</head>
<body>
    <form runat="server" id="MainForm">
    <table align="center" class="Container">
        <tr>
            <td class="Header">
                <table style="width: 100%">
                    <tr>
                        <td align="left">
                            <asp:HyperLink runat="server" ID="LogoLink" Target="_blank">
                                <asp:Image runat="server" ID="LogoImage" CssClass="Images" AlternateText="Logo">
                                </asp:Image>
                            </asp:HyperLink>
                        </td>
                        <td align="right" valign="bottom">
                            <asp:HyperLink runat="server" ID="HelpLink" Target="_blank">
                                <asp:Image runat="server" ID="HelpImage" CssClass="Images" AlternateText="Help">
                                </asp:Image>
                            </asp:HyperLink>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="Content">
                <asp:PlaceHolder runat="server" ID="ContentPlaceHolder"></asp:PlaceHolder>
                <asp:Label runat="server" ID="MessageLabel" CssClass="InformationMessage"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="Footer">
                <asp:Label runat="server" ID="FooterLabel"></asp:Label>
            </td>
        </tr>
    </table>
    <asp:Panel runat="server" ID="LoginPanel" Visible="False" CssClass="ContentPanel">
        <table style="width: 100%;">
            <tr>
                <td colspan="3">
                    <div class="PanelTitle">
                        login</div>
                </td>
            </tr>
            <tr>
                <td rowspan="5">
                    <div class="MessagePrompt">
                        The web site you are trying to access requires you to login. Please login using
                        your account username and password.
                        <br />
                        <br />
                        If you do not have an account, please request one by contacting the web site administrator.</div>
                </td>
                <td rowspan="5" class="ColumnSpacer">
                </td>
                <td>
                    Username:*
                    <asp:RequiredFieldValidator runat="server" ID="LoginUsernameValidator" ControlToValidate="LoginUsername"
                        ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="LoginUsername" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CheckBox runat="server" ID="RememberUsername" Text="Remember username" TabIndex="-1"
                        CssClass="SmallText" />
                </td>
            </tr>
            <tr>
                <td>
                    Password:*
                    <asp:RequiredFieldValidator runat="server" ID="LoginPasswordValidator" ControlToValidate="LoginPassword"
                        ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="LoginPassword" CssClass="TextInputHalf" TextMode="Password">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <table style="width: 100%;">
                        <tr>
                            <td>
                                <asp:HyperLink runat="server" ID="ForgotPassword" TabIndex="-1" CssClass="SmallText"
                                    ToolTip="Forgot your password">Forgot password?</asp:HyperLink>
                            </td>
                            <td align="right">
                                <asp:HyperLink runat="server" ID="ChangePassword" TabIndex="-1" CssClass="SmallText"
                                    ToolTip="Change your password">Change password?</asp:HyperLink>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Button runat="server" ID="LoginButton" CssClass="CommandButton" OnClick="LoginButton_Click"
                        Text="Login" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel runat="server" ID="ResetPasswordCheckPanel" Visible="False" CssClass="ContentPanel">
        <table style="width: 100%;">
            <tr>
                <td colspan="3">
                    <div class="PanelTitle">
                        reset password</div>
                </td>
            </tr>
            <tr>
                <td rowspan="2">
                    <div class="MessagePrompt">
                        To begin resetting your password, enter your username and click Next.</div>
                </td>
                <td rowspan="2" class="ColumnSpacer">
                </td>
                <td>
                    Username:*
                    <asp:RequiredFieldValidator runat="server" ID="ResetPasswordUsernameValidator" ControlToValidate="ResetPasswordUsername"
                        ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="ResetPasswordUsername" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Button runat="server" ID="ResetCheckButton" CssClass="CommandButton" Text="Next"
                        onclick="ResetCheckButton_Click" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel runat="server" ID="ResetPasswordFinalPanel" Visible="False" CssClass="ContentPanel">
        <table style="width: 100%;">
            <tr>
                <td colspan="3">
                    <div class="PanelTitle">
                        reset password</div>
                </td>
            </tr>
            <tr>
                <td rowspan="2">
                    <div class="MessagePrompt">
                        To complete resetting your password, enter the answer to your security question
                        and click Reset.</div>
                </td>
                <td rowspan="2" class="ColumnSpacer">
                </td>
                <td>
                    <asp:Label runat="server" ID="ResetPasswordSecurityQuestion"></asp:Label>
                    <br />
                    <asp:TextBox runat="server" ID="ResetPasswordSecurityAnswer" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Button runat="server" ID="ResetFinalButton" CssClass="CommandButton" Text="Reset"
                        onclick="ResetFinalButton_Click" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel runat="server" ID="ChangePasswordPanel" Visible="False" CssClass="ContentPanel">
        <table style="width: 100%;">
            <tr>
                <td colspan="3">
                    <div class="PanelTitle">
                        change password</div>
                </td>
            </tr>
            <tr>
                <td rowspan="5">
                    <div class="MessagePrompt">
                        You are required to change your password periodically for security reasons.<br />
                        <br />
                        In order to change your password you must enter your username and current password
                        for verification.</div>
                </td>
                <td rowspan="5" class="ColumnSpacer">
                </td>
                <td>
                    Username:*
                    <asp:RequiredFieldValidator runat="server" ID="ChangePasswordUsernameValidator" ControlToValidate="ChangePasswordUsername"
                        ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="ChangePasswordUsername" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Old Password:*
                    <asp:RequiredFieldValidator runat="server" ID="ChangePasswordOldPasswordValidator"
                        ControlToValidate="ChangePasswordOldPassword" ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="ChangePasswordOldPassword" CssClass="TextInputHalf"
                        TextMode="Password">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    New Password:*
                    <asp:RequiredFieldValidator runat="server" ID="ChangePasswordNewPasswordValidator1"
                        ControlToValidate="ChangePasswordNewPassword" ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <asp:CompareValidator runat="server" ID="ChangePasswordNewPasswordValidator2" ControlToCompare="ChangePasswordOldPassword"
                        ControlToValidate="ChangePasswordNewPassword" ErrorMessage="(Cannot be same as old password)"
                        SetFocusOnError="True" Operator="NotEqual" Display="Dynamic">
                    </asp:CompareValidator>
                    <br />
                    <asp:TextBox runat="server" ID="ChangePasswordNewPassword" CssClass="TextInputHalf"
                        TextMode="Password">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Confirm Password:*
                    <asp:RequiredFieldValidator runat="server" ID="ChangePasswordConfirmNewPasswordValidator1"
                        ControlToValidate="ChangePasswordConfirmNewPassword" ErrorMessage="(Required)"
                        SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <asp:CompareValidator runat="server" ID="ChangePasswordConfirmNewPasswordValidator2"
                        ControlToCompare="ChangePasswordNewPassword" ControlToValidate="ChangePasswordConfirmNewPassword"
                        ErrorMessage="(Must be same as new password)" SetFocusOnError="True" Display="Dynamic">
                    </asp:CompareValidator>
                    <br />
                    <asp:TextBox runat="server" ID="ChangePasswordConfirmNewPassword" CssClass="TextInputHalf"
                        TextMode="Password">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Button runat="server" ID="ChangeButton" CssClass="CommandButton" Text="Change"
                        onclick="ChangeButton_Click" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel runat="server" ID="MyAccountPanel" CssClass="ContentPanel" Visible="False">
        <table style="width: 100%;">
            <tr>
                <td colspan="3">
                    <div class="PanelTitle">
                        my account</div>
                </td>
            </tr>
            <tr>
                <td>
                    Username:<br />
                    <asp:TextBox runat="server" ID="AccountUsername" CssClass="TextInputHalf" Enabled="False">
                    </asp:TextBox>
                </td>
                <td class="ColumnSpacer">
                </td>
                <td>
                    Company:<br />
                    <asp:TextBox runat="server" ID="AccountUserCompany" CssClass="TextInputHalf" Enabled="False">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    First Name:*
                    <asp:RequiredFieldValidator runat="server" ID="AccountUserFirstNameValidator" ControlToValidate="AccountUserFirstName"
                        ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="AccountUserFirstName" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
                <td>
                    &nbsp;
                </td>
                <td>
                    Last Name:*
                    <asp:RequiredFieldValidator runat="server" ID="AccountUserLastNameValidator" ControlToValidate="AccountUserLastName"
                        ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="AccountUserLastName" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Email Address:*
                    <asp:RequiredFieldValidator runat="server" ID="AccountUserEmailAddressValidator"
                        ControlToValidate="AccountUserEmailAddress" ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="AccountUserEmailAddress" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
                <td>
                    &nbsp;
                </td>
                <td>
                    Phone Number:*
                    <asp:RequiredFieldValidator runat="server" ID="AccountUserPhoneNumberAddressValidator"
                        ControlToValidate="AccountUserPhoneNumber" ErrorMessage="(Required)" SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox runat="server" ID="AccountUserPhoneNumber" CssClass="TextInputHalf">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Label runat="server" ID="AccountUserSecurityQuestion">?</asp:Label>
                    <asp:RequiredFieldValidator runat="server" ID="AccountUserSecurityAnswerValidator"
                        ControlToValidate="AccountUserSecurityAnswer" ErrorMessage="(Required)<br />"
                        SetFocusOnError="True">
                    </asp:RequiredFieldValidator>
                    <asp:TextBox runat="server" ID="AccountUserSecurityAnswer" CssClass="TextInputFull">
                    </asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="3" align="center">
                    <asp:Button runat="server" ID="UpdateButton" CssClass="CommandButton" OnClick="UpdateButton_Click"
                        Text="Update" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel runat="server" ID="AccessDeniedPanel" CssClass="AccessDeniedPanel" Visible="False">
        <table style="width: 100%;">
            <tr>
                <td colspan="2">
                    <div class="PanelTitle">
                        access denied</div>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image runat="server" ID="WarningImage" AlternateText="Warning" CssClass="Images" />
                </td>
                <td>
                    <div class="ErrorMessage">
                        You do not have the permission to access the requested secure resource on this web
                        site. Please contact the web site administrator if you think this was a mistake
                        or if you would like to request access to the resource.</div>
                </td>
            </tr>
        </table>
    </asp:Panel>
    </form>
</body>
</html>
