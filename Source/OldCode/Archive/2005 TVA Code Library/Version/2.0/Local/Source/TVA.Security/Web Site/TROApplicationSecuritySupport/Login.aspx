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
	        color: Navy;
        }
        .buttonText 
        {
	        font-family: Tahoma;
	        font-size: .7em;
	        font-weight: bold;
	        width: 75px;
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
        <table width="450" cellpadding=2 cellspacing=1 border=0 align=center>
            <tr>
                <td align=left colspan=2>
                    <asp:Image ID="ImageLogo" runat="Server" ImageUrl="Images/tva-pso_logonbanner.jpg" />
                </td>                
            </tr>
            <tr>
                <td colspan=2><asp:Label ID="Label1" runat="server" CssClass="HeadingLabel" Text="Login"></asp:Label></td>
            </tr>
            <tr>
                <td colspan=2 align="center"><asp:Label ID="Label2" runat="server" CssClass="Label">This site is for authorized users of TVA services.<br />Please enter your ID and Password.</asp:Label></td>
            </tr>            
            <tr>
                <td colspan="2"><asp:Label ID="LabelError" runat="server" CssClass="ErrorLabel" ForeColor="#C00000"></asp:Label></td>
            </tr>
            <tr>
                <td width="125"><asp:Label ID="Label3" runat="server" CssClass="Label" Text="User Name:"></asp:Label></td>
                <td width="325"><asp:TextBox ID="TextBoxUserName" runat="server" Width="150px" CssClass="TextBox"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label4" runat="server" CssClass="Label" Text="Password:"></asp:Label></td>
                <td><asp:TextBox ID="TextBoxPassword" runat="server" TextMode="Password" Width="150px" CssClass="TextBox"></asp:TextBox></td>
            </tr>
            <tr>
                <td></td>
                <td><asp:Button ID="ButtonLogin" runat="server" CssClass="buttonText" Text="Login" /></td>
            </tr>
            <tr>
                <td colspan=2>&nbsp;</td>
            </tr>                  
            <tr>
                <td colspan=2><asp:Label ID="Label12" runat="server" CssClass="Label" Text="For assistance, please contact the Operations Duty Specialist at 423-751-1700."></asp:Label></td>
            </tr>        
            <tr>
                <td colspan=2><asp:HyperLink ID="HyperLink3" runat="server" CssClass="Label" NavigateUrl="ChangePassword.aspx">Change Password</asp:HyperLink></td>
            </tr>                  
        </table>
    </form>
</body>
</html>
