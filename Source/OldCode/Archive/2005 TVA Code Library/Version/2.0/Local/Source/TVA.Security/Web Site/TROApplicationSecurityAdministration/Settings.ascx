<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Settings.ascx.vb" Inherits="Settings" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="2" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage Security Settings"></asp:Label></td>    
    </tr> 
    <tr>
        <td class=whiteCell width=200>&nbsp;</td>
        <td class=whiteCell width=550>&nbsp;</td>
    </tr>
    <tr>
        <td class=whiteCell>
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="Setting Name:"></asp:Label></td>
        <td class=whiteCell>
            <asp:TextBox ID="TextBoxSettingName" runat="server" Width="300px" CssClass="TextBox"></asp:TextBox></td>
    </tr>
    <tr>
        <td class=whiteCell valign="top">
            <asp:Label ID="Label3" runat="server" CssClass="Label" Text="Setting Value:"></asp:Label></td>
        <td class=whiteCell>
            <asp:TextBox ID="TextBoxSettingValue" runat="server" Width="525px" Height="50px" TextMode="MultiLine" CssClass="TextBox"></asp:TextBox></td>
    </tr>
    <tr>
        <td class=whiteCell valign="top">
            <asp:Label ID="Label4" runat="server" CssClass="Label" Text="Setting Description:"></asp:Label></td>
        <td class=whiteCell>
            <asp:TextBox ID="TextBoxSettingDescription" runat="server" CssClass="TextBox" Height="50px"
                TextMode="MultiLine" Width="525px"></asp:TextBox></td>
    </tr>
    <tr>
        <td class=whiteCell></td>
        <td class=whiteCell align="right">
            <asp:Button ID="ButtonCancel" runat="server" CssClass="buttonText" Text="Cancel" />&nbsp;
            <asp:Button ID="ButtonSave" runat="server" CssClass="buttonText" Text="Save" /></td>
    </tr>
    <tr>
        <td colspan=2 class="whiteCell" align=center valign=middle><font color="steelblue"><b>- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></font></td>
    </tr> 
    <tr>
        <td class=whiteCell colspan=2 align=center>
            <asp:GridView ID="GridViewSettings" runat="server" AllowPaging="True" AllowSorting="False"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="15"
                Width="98%">
              
            <Columns>
                <asp:TemplateField HeaderText="Setting Name" SortExpression="SettingName" ItemStyle-HorizontalAlign=left>
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateSettingInfo"
                                Text='<%# Eval("SettingName") %>' CommandArgument='<%# Eval("SettingName") %>'></asp:LinkButton>
                        </ItemTemplate>
                </asp:TemplateField>                    
                <asp:BoundField DataField="SettingValue" HeaderText="Value" >
                    <ItemStyle HorizontalAlign="Left" Wrap="True" />
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