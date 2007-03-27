<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ErrorPage.aspx.vb" Inherits="ErrorPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Error Occured - TRO Application Security Administration Tool</title>
    <link href="StyleSheet.css" rel="stylesheet" type="text/css" />
    <meta http-equiv="Page-Exit" content="progid:DXImageTransform.Microsoft.Fade(duration=.25)" />
</head>
<body>
    <form id="form1" runat="server">
        <table width="100%" cellpadding=0 cellspacing=0 border=0 class="PageHeader">
            <tr>
                <td height="50" width="80%" valign=middle align=left>
                    &nbsp;<img src="Images\TRO.gif" border="0" />&nbsp;<asp:Label ID="Label1" runat="server" CssClass="HeadingLabel" Text="Application Security Administration Tool" Height="31px"></asp:Label>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                </td>
                <td align=right valign=middle width="20%">
                   <a class="helpHyperLink" href="#">How do I use this tool?</a>&nbsp; <img src="Images/Help.gif" alt="Click Here for Help" border="0" />&nbsp;
                </td>
            </tr>
            <tr>
                <td bgcolor=white colspan=2 align=left>
                    &nbsp;<asp:Label ID="Label2" runat="server" Text="ERROR:" CssClass="BoldLabel" ForeColor="#C00000"></asp:Label></td>
            </tr>
            <tr>
                <td align="left" bgcolor="white" colspan="2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td align="left" bgcolor="white" colspan="2" style="height: 18px">
                    &nbsp;<asp:Label ID="Label3" runat="server" CssClass="Label" Text="An error has occured on the web page. Your request cannot be processed.  <a href=Default.aspx target=_self>Click here</a> to go back to TRO Application Security Administration Tool website."></asp:Label></td>
            </tr>
            <tr>
                <td align="left" bgcolor="white" colspan="2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td align="left" bgcolor="white" colspan="2">
                    &nbsp;<asp:Label ID="LabelErrorDetail" runat="server" CssClass="Label"></asp:Label></td>
            </tr>
        </table>
    </form>
</body>
</html>
