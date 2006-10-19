<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Login.aspx.vb" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>TRO - Application Login</title>
    <STYLE type="text/css">
        body 
        {	        
	        font-family: Tahoma;
        }

        table
        {
	        font-family: Tahoma;	
        }

        .PageHeader
        {
	        background-image: url(Images/HeaderGradient.gif);
	        background-repeat: repeat-x;
	        background-position: left top;
        }
        
        .HeadingLabel
        {
	        font-family: Franklin Gothic Medium;
	        font-size: 1.1em;
	        color: #FFFFFF;
        }
        .buttonText 
        {
	        font-family: Tahoma;
	        font-size: .7em;
	        font-weight: bold;
	        width: 75px;
        }

        .whiteCell
        {
	        background-color: White;
        }

        .TextBox
        {
	        font-family: Tahoma;
	        font-size: .7em;
        }
        
        .BoldLabel
        {
	        font-family: Tahoma;
	        font-size: .8em;
	        font-weight:bold;
	        color: #FFFFFF;
        }

        .Label
        {
	        font-family: Tahoma;
	        font-size: .8em;
	        color: #000000;
        }
        
        .ErrorLabel
        {
	        font-family: Tahoma;
	        font-size: .7em;	        
        }

        .SmallLabel
        {
	        color: #ffffff;
	        font-family:Tahoma;
	        font-size: .7em;
        }
        
    </STYLE>
    <meta http-equiv="Page-Exit" content="progid:DXImageTransform.Microsoft.Fade(duration=.25)" />
</head>
<body>
    <form id="form1" runat="server"> 
        <table width="500" cellpadding=2 cellspacing=1 border=0 align=center style="border: solid 1px lightsteelBlue">
            <tr>
                <td height="50" valign=middle align=left colspan=3 class="PageHeader"">
                    &nbsp;<img src="Images\TRO.gif" border="0" />&nbsp;<asp:Label ID="Label1" runat="server" CssClass="HeadingLabel" Text="Login" Height="31px"></asp:Label>
                    &nbsp;&nbsp;&nbsp;
                </td>                
            </tr>
            <tr>
                <td class=whiteCell width="140">&nbsp;</td>
                <td class=whiteCell width="220">&nbsp;</td>
                <td class=whiteCell width="140">&nbsp;</td>
            </tr>
            <tr>
                <td class=whiteCell colspan=3>
                    <asp:Label ID="Label2" runat="server" Text="Label" CssClass="Label">This page is for authorized users of TVA web-based services. Please enter your ID and Password.</asp:Label></td>
            </tr>
            <tr>
                <td class="whiteCell" colspan="3">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="whiteCell" colspan="3">
                    <asp:Label ID="LabelError" runat="server" CssClass="ErrorLabel" ForeColor="#C00000"></asp:Label></td>
            </tr>
            <tr>
                <td class="whiteCell" colspan="3">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class=whiteCell>
                    <asp:Label ID="Label3" runat="server" CssClass="Label" Text="User Name:"></asp:Label></td>
                <td class=whiteCell colspan=2>
                    <asp:TextBox ID="TextBoxUserName" runat="server" Width="150px" CssClass="TextBox"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="whiteCell">
                    <asp:Label ID="Label4" runat="server" CssClass="Label" Text="Password:"></asp:Label></td>
                <td class="whiteCell" colspan=2>
                    <asp:TextBox ID="TextBoxPassword" runat="server" TextMode="Password" Width="150px" CssClass="TextBox"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="whiteCell">
                </td>
                <td class="whiteCell" colspan=2>
                    <asp:Button ID="ButtonLogin" runat="server" CssClass="buttonText" Text="Login" /></td>
            </tr>
            <tr>
                <td colspan=3 class=whiteCell>&nbsp;</td>
            </tr>                  
            <tr bgcolor="LightSteelBlue">
                <td colspan=4>
                    <asp:Label ID="Label12" runat="server" CssClass="Label" Text="If you have questions, please call the TVA Information Technology Service Center at 423-751-4357." Font-Size="0.7em"></asp:Label></td>
            </tr>               
           <tr bgcolor=LightSteelBlue>
                <td align=left>
                    <asp:Label ID="Label5" runat="server" CssClass="Label" Text="New User: "></asp:Label>
                </td>
                <td align=left colspan=2>
                    <asp:HyperLink ID="HyperLink1" runat="server" CssClass="Label" NavigateUrl="ApplyForAccount.aspx">Apply for an Account</asp:HyperLink>
                </td>
            </tr>
            <tr bgcolor="lightsteelblue">
                <td align="left">
                    <asp:Label ID="Label6" runat="server" CssClass="Label" Text="Existing User: "></asp:Label></td>
                <td align="left" colspan="2">
                    <asp:HyperLink ID="HyperLink5" runat="server" CssClass="Label" NavigateUrl="Login.aspx">Login</asp:HyperLink></td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td align=left></td>
                <td align=left colspan=2>
                    <asp:HyperLink ID="HyperLink2" runat="server" CssClass="Label" NavigateUrl="ResetPassword.aspx">Reset Password</asp:HyperLink>
                </td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td></td>
                <td align=left colspan=2><asp:HyperLink ID="HyperLink3" runat="server" CssClass="Label" NavigateUrl="ChangePassword.aspx">Change Password</asp:HyperLink></td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td></td>
                <td align=left colspan=2><asp:HyperLink ID="HyperLink4" runat="server" CssClass="Label" NavigateUrl="RequestAccess.aspx">Request Application Access</asp:HyperLink></td>
            </tr>
        </table>
    </form>
</body>
</html>
