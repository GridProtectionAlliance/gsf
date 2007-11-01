<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ChangePassword.aspx.vb" Inherits="ChangePassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>TRO - Change Password</title>
    
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
                <td align=left colspan=2><asp:Image ID="ImageLogo" runat="Server" ImageUrl="Images/tva-pso_logonbanner.jpg" /></td>                
            </tr>
            <tr>
                <td align=left colspan=2><asp:Label ID="Label1" runat="server" CssClass="HeadingLabel" Text="Change Password"></asp:Label></td>                
            </tr>
            <tr>
                <td width=125>&nbsp;</td>
                <td width=325>&nbsp;</td>
            </tr>
            <tr>
                <td colspan=2><asp:Label ID="LabelError" runat="server" CssClass="ErrorLabel" ForeColor="#C00000"></asp:Label></td>
            </tr>            
            <tr>
                <td><asp:Label ID="Label2" runat="server" Text="User Name:" CssClass="Label"></asp:Label></td>
                <td><asp:TextBox ID="TextBoxUserName" runat="server" CssClass="TextBox" Width="150px"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label3" runat="server" Text="Old Password:" CssClass="Label"></asp:Label></td>
                <td><asp:TextBox ID="TextBoxPassword" runat="server" CssClass="TextBox" Width="150px" TextMode="Password"></asp:TextBox></td>
            </tr>
            <tr>
                <td><asp:Label ID="Label4" runat="server" Text="New Password:" CssClass="Label"></asp:Label></td>
                <td><asp:TextBox ID="TextBoxNewPassword" runat="server" CssClass="TextBox" Width="150px" TextMode="Password"></asp:TextBox></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label ID="Label7" runat="server" Text="Confirm New Password:" CssClass="Label"></asp:Label></td>
                <td>
                    <asp:TextBox ID="TextBoxConfirmNewPassword" runat="server" CssClass="TextBox" Width="150px" TextMode="Password"></asp:TextBox><br />
                    <asp:CompareValidator ID="CompareValidator1" runat="server" ControlToCompare="TextBoxNewPassword"
                        ControlToValidate="TextBoxConfirmNewPassword" CssClass="Label" ErrorMessage="Confirn password text does not match with the new password text."
                        ForeColor="#C00000"></asp:CompareValidator></td>
            </tr>
            <tr>
                <td></td>
                <td><asp:Button ID="ButtonSubmit" runat="server" CssClass="buttonText" Text="Submit" /></td>
            </tr>
            <tr>
                <td colspan=2>&nbsp;</td>
            </tr>
            <tr>
                <td colspan=2><asp:Label ID="Label5" runat="server" CssClass="Label" Text="For assistance, please contact the Operations Duty Specialist at 423-751-1700."></asp:Label></td>
            </tr>        
            <tr>
                <td colspan=2><asp:HyperLink ID="HyperLink5" runat="server" CssClass="Label" NavigateUrl="Login.aspx">Login</asp:HyperLink></td>
            </tr>              
        </table>
    </form>
</body>
</html>
