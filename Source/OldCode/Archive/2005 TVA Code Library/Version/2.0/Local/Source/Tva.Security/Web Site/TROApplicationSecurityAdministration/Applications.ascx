<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Applications.ascx.vb" Inherits="Applications" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="4" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage Applications"></asp:Label></td>    
    </tr>    
    <tr>
        <td width="150" class="whiteCell">&nbsp;</td>
        <td width="225" class="whiteCell"></td>
        <td width="125" class="whiteCell"></td>
        <td width="250" class="whiteCell"></td>
    </tr>
    <tr>
        <td class="whiteCell">
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="Application Name:"></asp:Label></td>
        <td class="whiteCell" colspan="3">
            <asp:TextBox ID="TextBoxName" runat="server" Width="575px" CssClass="TextBox"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBoxName"
                CssClass="Error" ErrorMessage="*" ForeColor="" Enabled="False"></asp:RequiredFieldValidator></td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
            <asp:Label ID="Label3" runat="server" CssClass="Label" Text="Application Description:"></asp:Label></td>
        <td class="whiteCell" colspan="3">
            <asp:TextBox ID="TextBoxDescription" runat="server" Height="50px" TextMode="MultiLine"
                Width="575px" CssClass="TextBox"></asp:TextBox></td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
        </td>
        <td align="right" class="whiteCell" colspan="3">
            <asp:Button ID="ButtonCancel" runat="server" CssClass="buttonText" Text="Cancel" />&nbsp;
            &nbsp;<asp:Button ID="ButtonSave" runat="server" CssClass="buttonText" Text="Save" /></td>
    </tr>
    <tr>
        <td colspan=4 class="whiteCell" align=center valign=middle><font color="steelblue"><b>- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></font></td>
    </tr> 
    <tr>
        <td colspan=4 align=right class="whiteCell">
            &nbsp;<asp:TextBox ID="TextBoxSearch" runat="server" CssClass="TextBox"></asp:TextBox>&nbsp;<asp:Button
                ID="ButtonSearch" runat="server" CssClass="buttonText" Text="Search" />
            |
            <asp:LinkButton ID="LinkButtonShowAll" runat="server" CssClass="buttonText">Show All</asp:LinkButton>&nbsp;</td>
    </tr>
    <tr>
        <td align="center" class="whiteCell" colspan="4">
            <asp:GridView ID="GridViewApplications" runat="server" AllowPaging="True" AllowSorting="False"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="15"
                Width="98%">
              
            <Columns>
                <asp:TemplateField HeaderText="Application Name" SortExpression="ApplicationName">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateApplicationInfo"
                                Text='<%# Eval("ApplicationName") %>' CommandArgument='<%# Eval("ApplicationName") %>'></asp:LinkButton>
                        </ItemTemplate>
                </asp:TemplateField>                    
                <asp:BoundField DataField="ApplicationDescription" HeaderText="Description" >
                    <ItemStyle HorizontalAlign="Left" Wrap="True" />
                </asp:BoundField>            
                <asp:TemplateField>
                     <ItemTemplate>
                            <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="DeleteApplication"
                                Text="Delete" CommandArgument='<%# Eval("ApplicationName") %>'></asp:LinkButton>
                     </ItemTemplate>
                </asp:TemplateField>    
            </Columns>  
              
            <PagerStyle Font-Names="Tahoma" Font-Size="0.8em" HorizontalAlign="Right" />
                <HeaderStyle BackColor="LightSteelBlue" Font-Bold="True" Font-Names="Tahoma" Font-Size="1em"
                    Height="22px" />
                <AlternatingRowStyle BackColor="Gainsboro" />
            </asp:GridView>
        </td>
    </tr>
</table>
