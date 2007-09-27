<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ApplyForAccount.aspx.vb" Inherits="ApplyForAccount" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>TRO - Apply for Account</title>
    
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
        <table width="750" cellpadding=2 cellspacing=1 border=0 align=center style="border: solid 1px LightSteelBlue">
            <tr>
                <td height="50" valign=middle align=left colspan=4 class="PageHeader"">
                    &nbsp;<img src="Images\Logo.gif" border="0" />&nbsp;<asp:Label ID="Label1" runat="server" CssClass="HeadingLabel" Text="Apply for an Account" Height="31px"></asp:Label>
                    &nbsp;&nbsp;&nbsp;
                </td>                
            </tr>
            <tr>
                <td class="whiteCell" width=145>&nbsp;</td>
                <td class="whiteCell" width=275>&nbsp;</td>
                <td class="whiteCell" width=130>&nbsp;</td>
                <td class="whiteCell" width=200>&nbsp;</td>
            </tr>
            <tr>
                <td colspan=4 class=whiteCell>
                    <asp:Label ID="Label2" runat="server" CssClass="Label" Text="Please complete the form below. All fields are required.  After you submit your request, you will be contacted by phone or by e-mail to confirm your registration."></asp:Label></td>
            </tr>
            <tr>
                <td class="whiteCell" colspan="4">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="whiteCell" colspan="4">
                    <asp:Label ID="LabelError" runat="server" CssClass="ErrorLabel" ForeColor="#C00000"></asp:Label></td>
            </tr>
            <tr>
                <td class="whiteCell" colspan="4">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label3" runat="server" CssClass="Label" Text="User Name:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxUserName" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBoxUserName"
                        CssClass="Label" ErrorMessage="*"></asp:RequiredFieldValidator></td>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label4" runat="server" CssClass="Label" Text="Password:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxPassword" runat="server" CssClass="TextBox" Width="200px" TextMode="Password"></asp:TextBox>
                    </td>
            </tr>
            <tr>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label7" runat="server" CssClass="Label" Text="Security Question:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:DropDownList ID="DropDownListSecurityQuestions" runat="server" CssClass="TextBox" Width="270px">
                    </asp:DropDownList></td>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label8" runat="server" CssClass="Label" Text="Security Answer:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxSecurityAnswer" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
                    </td>
            </tr>
            <tr>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label5" runat="server" CssClass="Label" Text="First Name:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxFirstName" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="TextBoxFirstName"
                        CssClass="Label" ErrorMessage="*"></asp:RequiredFieldValidator></td>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label6" runat="server" CssClass="Label" Text="Last Name:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxLastName" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="TextBoxLastName"
                        CssClass="Label" ErrorMessage="*"></asp:RequiredFieldValidator></td>
            </tr>            
            <tr>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label9" runat="server" CssClass="Label" Text="Phone:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxPhone" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="TextBoxPhone"
                        CssClass="Label" ErrorMessage="*"></asp:RequiredFieldValidator></td>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label10" runat="server" CssClass="Label" Text="Email:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxEmail" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="TextBoxEmail"
                        CssClass="Label" ErrorMessage="*"></asp:RequiredFieldValidator></td>
            </tr>
            <tr>
                <td class="whiteCell" align=left>
                    <asp:Label ID="Label11" runat="server" CssClass="Label" Text="Company:"></asp:Label></td>
                <td class="whiteCell" align=left>
                    <asp:TextBox ID="TextBoxCompanyName" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox></td>
                <td class="whiteCell" align=left></td>
                <td class="whiteCell" align=left></td>
            </tr>
            <tr>
                <td class="whiteCell" align=right colspan=4>
                    <asp:Button ID="ButtonSubmit" runat="server" CssClass="buttonText" Text="Submit" /></td>
            </tr>
            <tr>
                <td colspan=4 class=whiteCell align=center>&nbsp;</td>
            </tr>  
            <tr bgcolor="LightSteelBlue">
                <td colspan=4>
                    <asp:Label ID="Label12" runat="server" CssClass="Label" Text="If you have questions, please call the TVA Information Technology Service Center at 423-751-4357." Font-Size="0.7em"></asp:Label></td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td align=left>
                    <asp:Label ID="Label13" runat="server" CssClass="Label" Text="New User: "></asp:Label>
                </td>
                <td align=left colspan=3>
                    <asp:HyperLink ID="HyperLink1" runat="server" CssClass="Label" NavigateUrl="ApplyForAccount.aspx">Apply for an Account</asp:HyperLink>
                </td>
            </tr>
            <tr bgcolor="lightsteelblue">
                <td align="left">
                    <asp:Label ID="Label14" runat="server" CssClass="Label" Text="Existing User: "></asp:Label></td>
                <td align="left" colspan="3">
                    <asp:HyperLink ID="HyperLink5" runat="server" CssClass="Label" NavigateUrl="Login.aspx">Login</asp:HyperLink></td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td align=left></td>
                <td align=left colspan=3>
                    <asp:HyperLink ID="HyperLink2" runat="server" CssClass="Label" NavigateUrl="ResetPassword.aspx">Reset Password</asp:HyperLink>
                </td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td></td>
                <td align=left colspan=3 contenteditable=""><asp:HyperLink ID="HyperLink3" runat="server" CssClass="Label" NavigateUrl="ChangePassword.aspx">Change Password</asp:HyperLink></td>
            </tr>
            <tr bgcolor=LightSteelBlue>
                <td></td>
                <td align=left colspan=3><asp:HyperLink ID="HyperLink4" runat="server" CssClass="Label" NavigateUrl="RequestAccess.aspx">Request Application Access</asp:HyperLink></td>
            </tr>
        </table>
    </form>
</body>
</html>
