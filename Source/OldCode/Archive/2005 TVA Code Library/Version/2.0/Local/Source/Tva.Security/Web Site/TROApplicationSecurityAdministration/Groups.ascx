<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Groups.ascx.vb" Inherits="Groups" %>
<%@ Register Assembly="Infragistics2.WebUI.UltraWebGrid.v6.1, Version=6.1.20061.28, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.WebUI.UltraWebGrid" TagPrefix="igtbl" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="4" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage User Groups"></asp:Label></td>    
    </tr>    
    <tr>
        <td width="150" class="whiteCell">&nbsp;</td>
        <td width="225" class="whiteCell"></td>
        <td width="125" class="whiteCell"></td>
        <td width="250" class="whiteCell"></td>
    </tr>
    <tr>
        <td class="whiteCell">
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="Group Name:"></asp:Label></td>
        <td class="whiteCell" colspan="3">
            <asp:TextBox ID="TextBoxName" runat="server" Width="575px" CssClass="TextBox"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBoxName"
                CssClass="Error" ErrorMessage="*" ForeColor="" Enabled="False"></asp:RequiredFieldValidator></td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
            <asp:Label ID="Label3" runat="server" CssClass="Label" Text="Group Description:"></asp:Label></td>
        <td class="whiteCell" colspan="3">
            <asp:TextBox ID="TextBoxDescription" runat="server" Height="40px" TextMode="MultiLine"
                Width="575px" CssClass="TextBox"></asp:TextBox></td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
        </td>
        <td align="left" class="whiteCell" colspan="3">
        
            <div id="users" style="height:192px; overflow:auto;">
            <asp:GridView ID="GridViewUsers" runat="server" AllowPaging="False" AllowSorting="True"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="12"
                Width="98%">
                
                <Columns>
                    <asp:TemplateField HeaderText="Select">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBox1" runat=server />                                
                        </ItemTemplate>
                    </asp:TemplateField>                                        
                    <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName">
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserFirstName" HeaderText="First Name" SortExpression="UserFirstName" >
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserLastName" HeaderText="Last Name" SortExpression="UserLastName">
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="UserCompanyName" HeaderText="Company Name" SortExpression="UserCompanyName">
                        <ItemStyle Wrap="True" />
                    </asp:BoundField>                    
                </Columns>
                
                <PagerStyle Font-Names="Tahoma" Font-Size="0.8em" HorizontalAlign="Right" />
                <HeaderStyle BackColor="LightSteelBlue" Font-Bold="True" Font-Names="Tahoma" Font-Size="1em"
                    Height="22px" />
                <AlternatingRowStyle BackColor="Gainsboro" />                
            </asp:GridView>
            </div>
        </td>
    </tr>
    <tr>
        <td class="whiteCell" valign="top">
        </td>
        <td align="left" class="whiteCell" colspan="3">
            <igtbl:ultrawebgrid id="UltraWebGridRoles" runat="server" Width="575px">
            <Bands>
                <igtbl:UltraGridBand AddButtonCaption="Column0Column1Column2" Key="Column0Column1Column2">
                    <AddNewRow View="NotSet" Visible="NotSet">
                    </AddNewRow>
                </igtbl:UltraGridBand>
            </Bands>
            <DisplayLayout AllowColSizingDefault="Free" AllowColumnMovingDefault="OnServer" AllowDeleteDefault="Yes"
                AllowSortingDefault="OnClient" AllowUpdateDefault="Yes" BorderCollapseDefault="Separate"
                HeaderClickActionDefault="SortMulti" Name="ctl00xUltraWebGridRoles" RowHeightDefault="20px"
                RowSelectorsDefault="No" SelectTypeRowDefault="Extended" Version="4.00" ViewType="Hierarchical">
                <GroupByBox>
                    <Style BackColor="ActiveBorder" BorderColor="Window"></Style>
                </GroupByBox>
                <GroupByRowStyleDefault BackColor="Control" BorderColor="Window">
                </GroupByRowStyleDefault>
                <FooterStyleDefault BackColor="LightGray" BorderStyle="Solid" BorderWidth="1px">
                    <BorderDetails ColorLeft="White" ColorTop="White" WidthLeft="1px" WidthTop="1px" />
                </FooterStyleDefault>
                <RowStyleDefault BackColor="Window" BorderStyle="Solid" BorderWidth="1px">
                    <BorderDetails ColorLeft="Gainsboro" ColorTop="Gainsboro" />
                    <Padding Left="3px" />
                </RowStyleDefault>
                <HeaderStyleDefault BackColor="LightSteelBlue" BorderStyle="Solid" HorizontalAlign="Center" Font-Bold="True">
                    <BorderDetails ColorLeft="SteelBlue" ColorTop="SteelBlue" WidthLeft="1px" WidthTop="1px" WidthBottom="1px" WidthRight="1px" />
                </HeaderStyleDefault>
                <EditCellStyleDefault BorderStyle="None" BorderWidth="0px">
                </EditCellStyleDefault>
                <FrameStyle BackColor="Window" BorderColor="InactiveCaption" BorderStyle="Solid"
                    BorderWidth="0px" Font-Names="Tahoma" Font-Size="0.8em" Font-Overline="False" Height="200px" Width="575px">
                </FrameStyle>
                <Pager>
                    <Style BackColor="LightGray" BorderStyle="Solid" BorderWidth="1px">
<BorderDetails ColorTop="White" WidthLeft="1px" WidthTop="1px" ColorLeft="White"></BorderDetails>
</Style>
                </Pager>
                <AddNewBox Hidden="False">
                    <Style BackColor="Window" BorderColor="InactiveCaption" BorderStyle="Solid" BorderWidth="1px">
<BorderDetails ColorTop="White" WidthLeft="1px" WidthTop="1px" ColorLeft="White"></BorderDetails>
</Style>
                </AddNewBox>
                <RowAlternateStyleDefault BackColor="Gainsboro">
                </RowAlternateStyleDefault>
            </DisplayLayout>
</igtbl:ultrawebgrid>
        </td>
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
            <asp:TextBox ID="TextBoxSearch" runat="server" CssClass="TextBox"></asp:TextBox>&nbsp;<asp:Button
                ID="ButtonSearch" runat="server" CssClass="buttonText" Text="Search" />
            |
            <asp:LinkButton ID="LinkButtonShowAll" runat="server" CssClass="buttonText">Show All</asp:LinkButton>&nbsp;</td>
    </tr>
    <tr>
        <td align="center" class="whiteCell" colspan="4">
            <asp:GridView ID="GridViewGroups" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="5"
                Width="98%">
                
                <Columns>
                    <asp:TemplateField HeaderText="Group Name" SortExpression="GroupName">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateGroupInfo"
                                    Text='<%# Eval("GroupName") %>' CommandArgument='<%# Eval("GroupName") %>'></asp:LinkButton>
                            </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:BoundField DataField="GroupDescription" HeaderText="Description" SortExpression="GroupDescription" >
                        <ItemStyle HorizontalAlign="Left" Wrap="True" />
                    </asp:BoundField>            
                    <asp:TemplateField>
                         <ItemTemplate>
                                <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="DeleteGroup"
                                    Text="Delete" CommandArgument='<%# Eval("GroupName") %>'></asp:LinkButton>
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
