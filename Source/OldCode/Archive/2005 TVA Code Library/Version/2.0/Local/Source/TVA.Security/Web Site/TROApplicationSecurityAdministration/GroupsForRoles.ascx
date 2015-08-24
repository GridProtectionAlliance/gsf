<%@ Control Language="VB" AutoEventWireup="false" CodeFile="GroupsForRoles.ascx.vb" Inherits="GroupsForRoles" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="98%" align=center>
    <tr>
        <td><div id="users" style="height: 285px; overflow:auto;">
            <asp:GridView ID="GridViewGroups" runat="server" AllowPaging="False" AllowSorting="False"
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
                    <asp:BoundField DataField="GroupName" HeaderText="Group Name">
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="GroupDescription" HeaderText="Description">
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