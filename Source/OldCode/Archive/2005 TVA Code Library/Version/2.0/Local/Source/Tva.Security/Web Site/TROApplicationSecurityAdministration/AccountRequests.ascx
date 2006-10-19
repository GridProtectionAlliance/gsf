<%@ Control Language="VB" AutoEventWireup="false" CodeFile="AccountRequests.ascx.vb" Inherits="AccountRequests" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="4" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage Account Requests"></asp:Label></td>    
    </tr> 
    <tr>
        <td class="whiteCell" width=110>&nbsp;</td>
        <td class="whiteCell" width=280></td>
        <td class="whiteCell" width=110></td>
        <td class="whiteCell" width=250></td>
    </tr>
    <tr>
        <td colspan=4 class=whiteCell>
            <asp:Label ID="LabelError" runat="server" CssClass="Label" ForeColor="#C00000"></asp:Label></td>
    </tr>
    <tr>
        <td class=whiteCell>
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="User Name:"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="LabelUserName" runat="server" CssClass="Label"></asp:Label></td>
        <td class=whiteCell></td>
        <td class=whiteCell></td>
    </tr>
    <tr>
        <td class=whiteCell>
            <asp:Label ID="Label3" runat="server" CssClass="Label" Text="First Name:"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="LabelFirstName" runat="server" CssClass="Label"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="Label4" runat="server" CssClass="Label" Text="Last Name:"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="LabelLastName" runat="server" CssClass="Label"></asp:Label></td>
    </tr>
    <tr>
        <td class=whiteCell>
            <asp:Label ID="Label5" runat="server" CssClass="Label" Text="Phone:"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="LabelPhone" runat="server" CssClass="Label"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="Label6" runat="server" CssClass="Label" Text="Email:"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="LabelEmail" runat="server" CssClass="Label"></asp:Label></td>
    </tr>
    <tr>
        <td class=whiteCell>
            <asp:Label ID="Label7" runat="server" CssClass="Label" Text="Company Name:"></asp:Label></td>
        <td class=whiteCell>
            <asp:Label ID="LabelCompany" runat="server" CssClass="Label"></asp:Label></td>
        <td class=whiteCell></td>
        <td class=whiteCell></td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
            </td>
        <td class="whiteCell" colspan="3">
            </td>        
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
            <asp:Label ID="Label8" runat="server" CssClass="Label" Text="Reason for Disapproval:"></asp:Label></td>
        <td class="whiteCell" valign="top">
            <asp:TextBox ID="TextBoxReason" runat="server" CssClass="TextBox" Height="50px" TextMode="MultiLine"
                Width="250px"></asp:TextBox></td>
        <td class="whiteCell" valign="top">
            <asp:Label ID="Label9" runat="server" CssClass="Label" Text="Company:"></asp:Label></td>
        <td class="whiteCell" valign="top">
            <asp:DropDownList ID="DropDownListCompanies" runat="server" CssClass="TextBox" Width="200px">
            </asp:DropDownList></td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
        </td>
        <td class="whiteCell" valign="top">
            <asp:Button ID="ButtonDisapprove" runat="server" CssClass="buttonText" Text="Disapprove" /></td>
        <td class="whiteCell" valign="top">
        </td>
        <td class="whiteCell" valign="top">
            <asp:Button
                ID="ButtonApprove" runat="server" CssClass="buttonText" Text="Approve" /></td>
    </tr>
    <tr>
        <td colspan=4 align=right class=whiteCell>
            &nbsp;&nbsp;</td>
    </tr>
    <tr>
        <td colspan=4 class="whiteCell" align=center valign=middle><font color="steelblue"><b>- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></font></td>
    </tr> 
    <tr>
        <td colspan=4 align=center>
            <asp:GridView ID="GridViewRequests" runat="server" AllowPaging="True" AllowSorting="False"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="15"
                Width="98%">
                
                <Columns>
                    <asp:TemplateField HeaderText="User Name" SortExpression="UserName">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateRequestInfo"
                                Text='<%# Eval("UserName") %>' CommandArgument='<%# Eval("UserName") %>'></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:BoundField DataField="UserFirstName" HeaderText="First Name" SortExpression="UserFirstName" />
                    <asp:BoundField DataField="UserLastName" HeaderText="Last Name" SortExpression="UserLastName" />                    
                    <asp:BoundField DataField="UserCompanyName" HeaderText="Company" SortExpression="CompanyName" />                    
                    <asp:BoundField DataField="ApplicationName" HeaderText="Application" SortExpression="ApplicationName" />
                </Columns>
                
                <PagerStyle Font-Names="Tahoma" Font-Size="0.8em" HorizontalAlign="Right" />
                <HeaderStyle BackColor="LightSteelBlue" Font-Bold="True" Font-Names="Tahoma" Font-Size="1em"
                    Height="22px" />
                <AlternatingRowStyle BackColor="Gainsboro" />
            </asp:GridView>
        
        </td>
    </tr>
</table>