<%@ Control Language="VB" AutoEventWireup="false" CodeFile="UsersForRoles.ascx.vb" Inherits="UsersForRoles" %>

<br />

<table border=0 cellpadding=2 cellspacing=1 width="98%" align=center>
    <tr>
        <td>
        <div id="users" style="height: 285px; overflow:auto;">
            <asp:GridView ID="GridViewUsers" runat="server" AllowPaging="False" AllowSorting="True"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="9"
                Width="98%">
                <Columns>
                    <asp:TemplateField HeaderText="Select">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBox1" runat=server />                                
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>                                        
                    <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName" >
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserFirstName" HeaderText="First Name" SortExpression="UserFirstName" >
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>                    
                    <asp:BoundField DataField="UserLastName" HeaderText="Last Name" SortExpression="UserLastName" >
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserCompanyName" HeaderText="Company Name" SortExpression="UserCompanyName" >
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                </Columns>
                <PagerStyle Font-Names="Tahoma" Font-Size="0.8em" HorizontalAlign="Right" />
                <HeaderStyle BackColor="LightSteelBlue" Font-Bold="True" Font-Names="Tahoma" Font-Size="1em"
                    Height="22px" HorizontalAlign=Center />
                <AlternatingRowStyle BackColor="Gainsboro" />
            </asp:GridView>
            </div>           
        </td>
    </tr>
</table>
