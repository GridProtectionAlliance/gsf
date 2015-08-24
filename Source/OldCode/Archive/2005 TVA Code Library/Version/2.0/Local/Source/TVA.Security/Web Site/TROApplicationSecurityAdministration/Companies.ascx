<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Companies.ascx.vb" Inherits="Companies" %>
<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="4" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage Companies"></asp:Label></td>    
    </tr>    
    <tr>
        <td width="150" class="whiteCell">&nbsp;</td>
        <td width="225" class="whiteCell"></td>
        <td width="125" class="whiteCell"></td>
        <td width="250" class="whiteCell"></td>
    </tr>
    <tr>
        <td class="whiteCell">
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="Company Name:"></asp:Label></td>
        <td class="whiteCell" colspan="3">
            <asp:TextBox ID="TextBoxName" runat="server" Width="575px" CssClass="TextBox"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBoxName"
                CssClass="Error" ErrorMessage="*" ForeColor="" Enabled="False"></asp:RequiredFieldValidator></td>
    </tr>    
    <tr>
        <td class="whiteCell" valign="top">
        </td>
        <td align="right" class="whiteCell" colspan="3">
            <asp:Label ID="LabelMessage" runat="server" CssClass="Label" ForeColor="#C00000"></asp:Label>&nbsp;
            <asp:Button ID="ButtonCancel" runat="server" CssClass="buttonText" Text="Cancel" />&nbsp;
            &nbsp;<asp:Button ID="ButtonSave" runat="server" CssClass="buttonText" Text="Save" /></td>
    </tr>
    <tr>
        <td colspan=4 class="whiteCell" align=center valign=middle><font color="steelblue"><b>- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></font></td>
    </tr> 
    <tr>
        <td colspan=4 align=right class="whiteCell">
            <asp:TextBox ID="TextBoxSearch" runat="server" CssClass="TextBox"></asp:TextBox>&nbsp;<asp:Button
                ID="ButtonSearch" runat="server" CssClass="buttonText" Text="Search" />
            |
            <asp:LinkButton ID="LinkButtonShowAll" runat="server" CssClass="buttonText">Show All</asp:LinkButton>&nbsp;
        </td>
    </tr>
    <tr>
        <td align="center" class="whiteCell" colspan="4">
            <asp:GridView ID="GridViewCompanies" runat="server" AllowPaging="True" AllowSorting="False"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="22"
                Width="98%">
                
                <Columns>
                    <asp:TemplateField HeaderText="Company Name" SortExpression="CompanyName">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateCompanyInfo"
                                Text='<%# Eval("CompanyName") %>' CommandArgument='<%# Eval("CompanyName") %>'></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>                                        
                    <asp:TemplateField>
                     <ItemTemplate>
                            <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="DeleteCompany"
                                Text="Delete" CommandArgument='<%# Eval("CompanyName") %>'></asp:LinkButton>
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
