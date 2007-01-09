<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ShowUsers.aspx.vb" Inherits="ShowUsers" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Users</title>
    <base target=_self></base>
    <link href="StyleSheet.css" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">
        <table width="600" align=center border=0 cellpadding=2 cellspacing=1>
            <tr>
                <td>&nbsp;</td>                
            </tr>
            <tr>
                <td align=left><b>
                    <asp:Label ID="LabelGroupName" runat="server" CssClass="BoldLabel" ForeColor="Black"></asp:Label></b></td>
            </tr>
            <tr>
                <td align=center>
                    <asp:GridView ID="GridViewUsers" runat="server" AllowPaging="True" AllowSorting="True"
                        AutoGenerateColumns="False"
                        EmptyDataText="No Users Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="35"
                        Width="98%">
                        
                        <Columns>
                            <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName">
                                <ItemStyle Wrap="True" HorizontalAlign=Center />
                            </asp:BoundField>
                            <asp:BoundField DataField="UserFirstName" HeaderText="First Name" SortExpression="UserFirstName" >
                                <ItemStyle Wrap="True" HorizontalAlign=left />
                            </asp:BoundField>
                            <asp:BoundField DataField="UserLastName" HeaderText="Last Name" SortExpression="UserLastName">
                                <ItemStyle Wrap="True" HorizontalAlign=left />
                            </asp:BoundField>
                            <asp:BoundField DataField="UserCompanyName" HeaderText="Company Name" SortExpression="UserCompanyName">
                                <ItemStyle Wrap="True" HorizontalAlign=left />
                            </asp:BoundField>                       
                        </Columns>  
                        
                        <PagerStyle Font-Names="Tahoma" Font-Size="0.8em" HorizontalAlign="Right" />
                        <HeaderStyle BackColor="LightSteelBlue" Font-Bold="True" Font-Names="Tahoma" Font-Size="1em"
                            Height="22px" />
                        <AlternatingRowStyle BackColor="Gainsboro" />
                    
                    </asp:GridView>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
