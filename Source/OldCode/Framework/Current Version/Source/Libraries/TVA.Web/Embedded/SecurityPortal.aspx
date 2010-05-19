<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SecurityPortal.aspx.cs"
    Inherits="TVA.Web.Embedded.SecurityPortal" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        body, input
        {
            color: #3e3e3e;
            font-family: Trebuchet MS, Tahoma, Arial;
            font-size: 10pt;
        }
        a:link, a: visited
        {
            text-decoration: underline;
        }
        a:hover
        {
            text-decoration: none;
        }
        .Container
        {
            width: 500px;
            border-collapse: collapse;
        }
        .Header
        {
            border-width: 1px;
            border-color: #808080;
            border-style: dotted;
        }
        .Content
        {
            text-align: center;
            min-height: 200px;
            background-color: #eaeaea;
            border-width: 1px;
            border-color: #808080;
            border-left-style: dotted;
            border-right-style: dotted;
        }
        .Footer, a:link, a:visited, a:hover
        {
            height: 25px;
            text-align: center;
            color: #ffffff;
            background-color: #808080;
            border: solid 1px #808080;
        }
        .Images
        {
            padding: 5px;
            border: none;
        }
        .PanelTitle
        {
            color: #000000;
            font-size: 20pt;
            text-align: center;
            text-transform: lowercase;
            padding-bottom: 10px;
        }
        .ErrorMessage
        {
            color: #ff0000;
        }
        .TextInput
        {
            color: #000000;
            width: 200px;
            margin: 1px 0px 5px 0px;
        }
        .CommandButton
        {
            color: #000000;
            width: 75px;
            margin-top: 10px;
        }
        .ColumnSpacer
        {
            width: 50px;
        }
        .LoginPanel
        {
            text-align: left;
            width: 450px;
            margin: 10px 25px 10px 25px;
        }
        .LoginPrompt
        {
            text-align: center;
            width: 200px;
            font-style: italic;
        }
        .MyAccountPanel
        {
            text-align: left;
            width: 450px;
            margin: 10px 25px 10px 25px;
        }
        .AccessDeniedPanel
        {
            text-align: left;
            width: 400px;
            margin: 10px 50px 10px 50px;
        }
    </style>
</head>
<body>
    <form id="MainForm" runat="server">
    <table align="center" class="Container">
        <tr>
            <td class="Header">
                <table style="width: 100%">
                    <tr>
                        <td align="left">
                            <asp:Image ID="LogoImage" runat="server" CssClass="Images" 
                                AlternateText="Logo" />
                        </td>
                        <td align="right" valign="bottom">
                            <asp:ImageButton ID="HelpLink" runat="server" CssClass="Images" AlternateText="Help" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="Content">
                <asp:PlaceHolder ID="ContentPlaceHolder" runat="server"></asp:PlaceHolder>
                <asp:Label ID="ErrorMessageLabel" runat="server" CssClass="ErrorMessage"></asp:Label>
                <br />
                <br />
            </td>
        </tr>
        <tr>
            <td class="Footer">
                <asp:HyperLink ID="CompanyLink" runat="server" Target="_blank">[CompanyLink]</asp:HyperLink>
            </td>
        </tr>
    </table>
    <asp:Panel ID="LoginPanel" runat="server" Visible="False" CssClass="LoginPanel">
        <table style="width: 100%;">
            <tr>
                <td colspan="3">
                    <div class="PanelTitle">
                        login</div>
                </td>
            </tr>
            <tr>
                <td rowspan="4">
                    <div class="LoginPrompt">
                        The web site you are trying to access requires you to login. Please login using
                        your account username and password. If you do not have an account, please request
                        one by contacting the web site administrator.</div>
                </td>
                <td rowspan="5" class="ColumnSpacer">
                </td>
                <td>
                    Username:*
                    <asp:RequiredFieldValidator ID="UsernameRequiredFieldValidator" runat="server" ControlToValidate="UsernameInput"
                        ErrorMessage="(Required)" SetFocusOnError="True"></asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox ID="UsernameInput" runat="server" CssClass="TextInput"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Password:*
                    <asp:RequiredFieldValidator ID="PasswordRequiredFieldValidator" runat="server" ControlToValidate="PasswordInput"
                        ErrorMessage="(Required)" SetFocusOnError="True"></asp:RequiredFieldValidator>
                    <br />
                    <asp:TextBox ID="PasswordInput" runat="server" CssClass="TextInput" TextMode="Password"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:CheckBox ID="RememberMeCheckBox" runat="server" Text="Remember me" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Button ID="LoginButton" runat="server" CssClass="CommandButton" OnClick="LoginButton_Click"
                        Text="Login" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel ID="MyAccountPanel" runat="server" CssClass="MyAccountPanel" Visible="False">
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
                    <asp:TextBox ID="UsernameLabel" runat="server" CssClass="TextInput" Enabled="False"></asp:TextBox>
                </td>
                <td class="ColumnSpacer">
                </td>
                <td>
                    Company:<br />
                    <asp:TextBox ID="CompanyLabel" runat="server" CssClass="TextInput" Enabled="False"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    First Name:<br />
                    <asp:TextBox ID="FirstNameInput" runat="server" CssClass="TextInput"></asp:TextBox>
                </td>
                <td>
                    &nbsp;
                </td>
                <td>
                    Last Name:<br />
                    <asp:TextBox ID="LastNameInput" runat="server" CssClass="TextInput"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Email Address:<br />
                    <asp:TextBox ID="EmailAddressInput" runat="server" CssClass="TextInput"></asp:TextBox>
                </td>
                <td>
                    &nbsp;
                </td>
                <td>
                    Phone Number:<br />
                    <asp:TextBox ID="PhoneNumberInput" runat="server" CssClass="TextInput"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="3" align="center">
                    <asp:Button ID="UpdateButton" runat="server" CssClass="CommandButton" OnClick="UpdateButton_Click"
                        Text="Update" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel ID="AccessDeniedPanel" runat="server" CssClass="AccessDeniedPanel" Visible="False">
        <table style="width: 100%;">
            <tr>
                <td colspan="2">
                    <div class="PanelTitle">
                        access denied</div>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="WarningImage" runat="server" AlternateText="Warning" CssClass="Images" />
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
