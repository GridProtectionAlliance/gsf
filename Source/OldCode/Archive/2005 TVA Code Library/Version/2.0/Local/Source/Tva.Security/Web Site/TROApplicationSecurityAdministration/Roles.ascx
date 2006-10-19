<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Roles.ascx.vb" Inherits="Roles" %>
<%@ Register Src="GroupsForRoles.ascx" TagName="GroupsForRoles" TagPrefix="uc1" %>
<%@ Register Src="UsersForRoles.ascx" TagName="UsersForRoles" TagPrefix="uc2" %>
<%@ Register Assembly="Infragistics2.WebUI.UltraWebTab.v6.1, Version=6.1.20061.28, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.WebUI.UltraWebTab" TagPrefix="igtab" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="4" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage Roles"></asp:Label></td>    
    </tr>      
    <tr>
        <td class="whiteCell" width="125">&nbsp;</td>
        <td class="whiteCell" width="250">&nbsp;</td>
        <td class="whiteCell" width="125">&nbsp;</td>
        <td class="whiteCell" width="250">&nbsp;</td>
    </tr>     
    <tr>
        <td class="whiteCell">
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="Role Name:"></asp:Label></td>
        <td class="whiteCell" colspan=3>
            <asp:TextBox ID="TextBoxName" runat="server" CssClass="TextBox" Width="490px"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBoxName"
                CssClass="Error" ErrorMessage="*" ForeColor="" Enabled="False"></asp:RequiredFieldValidator></td>
    </tr> 
    <tr>
        <td class="whiteCell" valign="top">
            <asp:Label ID="Label3" runat="server" CssClass="Label" Text="Role Description:"></asp:Label></td>
        <td class="whiteCell" colspan="3" align="left">
            <asp:TextBox ID="TextBoxDescription" runat="server" CssClass="TextBox" Height="50px"
                TextMode="MultiLine" Width="490px"></asp:TextBox>&nbsp;
        </td>
    </tr>
    <tr>
        <td align="left" class="whiteCell" colspan="4">
            <igtab:UltraWebTab ID="UltraWebTabUsersAndGroups" runat="server" BorderColor="#949878" BorderStyle="Solid"
                        BorderWidth="0px" Font-Bold="False" Font-Italic="False" Font-Overline="False"
                        Font-Strikeout="False" Font-Underline="False" Height="305px" Width="100%" SpaceOnLeft="0">
                
                <DefaultTabStyle BackColor="WhiteSmoke" Font-Bold="False" Font-Names="Tahoma" Font-Size="8.5pt"
                            ForeColor="Black" Height="18px" Width="95px">
                    <Padding Bottom="0px" Top="0px" />
                </DefaultTabStyle>
                
                <Tabs>
                    <igtab:Tab Text="Users">
                        <Style>
                         <Padding Top="2px"></Padding>
                        </Style>
                        <ContentTemplate>
                            <uc2:UsersForRoles ID="UsersForRoles1" runat="server" />
                        </ContentTemplate>
                    </igtab:Tab>
                    <igtab:Tab Text="Groups">
                        <Style>
                            <Padding Top="2px"></Padding>
                        </Style>
                        <ContentTemplate>
                            <uc1:GroupsForRoles ID="GroupsForRoles1" runat="server" />
                        </ContentTemplate>
                    </igtab:Tab>
                </Tabs>
                <RoundedImage FillStyle="LeftMergedWithCenter" HoverImage="[ig_tab_winXP2.gif]" LeftSideWidth="7"
                            NormalImage="[ig_tab_winXP3.gif]" RightSideWidth="6" SelectedImage="[ig_tab_winXP1.gif]"
                            ShiftOfImages="2" />
                <SelectedTabStyle Font-Bold="True" Font-Size="8.5pt" Height="18px">
                     <Padding Bottom="0px" Top="0px" />
                </SelectedTabStyle>
                <BorderDetails ColorBottom="SteelBlue" ColorTop="SteelBlue" StyleBottom="Solid" StyleTop="Solid"
                            WidthBottom="1px" WidthTop="1px" WidthRight="1px" WidthLeft="1px" StyleRight="Solid" StyleLeft="Solid" ColorRight="SteelBlue" ColorLeft="SteelBlue" />
            </igtab:UltraWebTab>
        </td>
    </tr>
    <tr>
        <td class="whiteCell">
        </td>
        <td align="right" class="whiteCell" colspan="3">
            <asp:Button ID="ButtonCancel" runat="server" CssClass="buttonText" Text="Cancel" />&nbsp;
            <asp:Button ID="ButtonSave" runat="server" CssClass="buttonText" Text="Save" /></td>
    </tr>
    <tr>
        <td colspan=4 class="whiteCell" align=center valign=middle><font color="steelblue"><b>- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></font></td>
    </tr>
    <tr>
        <td colspan=2 class="whiteCell" align=left>
            <asp:DropDownList ID="DropDownListApplications" runat="server" CssClass="TextBox" Width="200" AutoPostBack="True"></asp:DropDownList>
        </td>
        <td colspan=2 class="whiteCell" align=right>
            <asp:TextBox ID="TextBoxSearch" runat="server" CssClass="TextBox"></asp:TextBox>&nbsp;<asp:Button
                ID="ButtonSearch" runat="server" CssClass="buttonText" Text="Search" />
            |
            <asp:LinkButton ID="LinkButtonShowAll" runat="server" CssClass="buttonText">Show All</asp:LinkButton>&nbsp;
        </td>
    </tr> 
    <tr>
        <td colspan=4 align=center class="whiteCell">
            <asp:GridView ID="GridViewRoles" runat="server" AllowPaging="True" AllowSorting="False"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="5"
                Width="98%">
                
                <Columns>                
                <asp:TemplateField HeaderText="Role Name" SortExpression="RoleName">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateRoleInfo"
                                Text='<%# Eval("RoleName") %>' CommandArgument='<%# Eval("RoleName") %>'></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:BoundField DataField="RoleDescription" HeaderText="Description" SortExpression="RoleDescription" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="DeleteRole"
                                Text="Delete" CommandArgument='<%# Eval("RoleName") %>'></asp:LinkButton>
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