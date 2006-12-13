<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Users.ascx.vb" Inherits="Users" %>
<%@ Register Assembly="Infragistics2.WebUI.UltraWebGrid.v6.1, Version=6.1.20061.28, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.WebUI.UltraWebGrid" TagPrefix="igtbl" %>

<br />

<script language=javascript>
    function EnableExternalFields() {
        //alert(document.all.UltraWebTabTools__ctl3_Users1_CheckBoxIsExternal.checked);
        if (document.all.UltraWebTabTools__ctl3_Users1_CheckBoxIsExternal.checked == true) {													
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxEmail.disabled = false;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxFirstName.disabled = false;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxLastName.disabled = false;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxPassword.disabled = false;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxPhone.disabled = false;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxSecurityAnswer.disabled = false;
			document.all.UltraWebTabTools__ctl3_Users1_DropDownListSecurityQuestions.disabled = false;
		}
		else {
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxEmail.disabled = true;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxFirstName.disabled = true;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxLastName.disabled = true;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxPassword.disabled = true;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxPhone.disabled = true;
			document.all.UltraWebTabTools__ctl3_Users1_TextBoxSecurityAnswer.disabled = true;
			document.all.UltraWebTabTools__ctl3_Users1_DropDownListSecurityQuestions.disabled = true;
		}
    }
</script>
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="4" bgcolor="#77AADD">
            <asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Manage Users"></asp:Label></td>    
    </tr> 
    <tr>
        <td class="whiteCell" colspan=4>
            <asp:Label ID="LabelMsg" runat="server" CssClass="Label" ForeColor="#C00000"></asp:Label></td>        
    </tr>   
    <tr>
        <td class="whiteCell" style="width: 110px">
            <asp:Label ID="Label6" runat="server" CssClass="Label" Text="Company Name:"></asp:Label></td>
        <td class="whiteCell">
            <asp:DropDownList ID="DropDownListCompanies" runat="server" CssClass="TextBox" Width="203px">
            </asp:DropDownList></td>        
        <td class="whiteCell">
            <asp:CheckBox ID="CheckBoxIsLocked" runat="server" CssClass="TextBox" Text=" Is Locked?" /></td>        
       <td class="whiteCell">
           <asp:CheckBox ID="CheckBoxIsExternal" runat="server" CssClass="TextBox" Text=" Is External" /></td>
    </tr>
    <tr>
        <td class="whiteCell" style="width: 110px">
            <asp:Label ID="Label2" runat="server" CssClass="Label" Text="User Name:"></asp:Label></td>
        <td class="whiteCell" width="280">
            <asp:TextBox ID="TextBoxUserName" runat="server" CssClass="TextBox" Width="200px"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBoxUserName"
                CssClass="Error" ErrorMessage="*" ForeColor="" Enabled="False"></asp:RequiredFieldValidator></td>
        <td class="whiteCell" width="110">
            <asp:Label ID="Label3" runat="server" CssClass="Label" Text="Password:"></asp:Label></td>
        <td class="whiteCell" width="250">
            <asp:TextBox ID="TextBoxPassword" runat="server" CssClass="TextBox" Width="200px" TextMode="Password" Enabled="False"></asp:TextBox></td>
    </tr>  
    <tr>
        <td class="whiteCell" style="width: 110px">
            <asp:Label ID="Label4" runat="server" CssClass="Label" Text="First Name:"></asp:Label></td>
        <td class="whiteCell">
            <asp:TextBox ID="TextBoxFirstName" runat="server" CssClass="TextBox" Width="200px" Enabled="False"></asp:TextBox></td>
        <td class="whiteCell">
            <asp:Label ID="Label5" runat="server" CssClass="Label" Text="Last Name:"></asp:Label></td>
        <td class="whiteCell">
            <asp:TextBox ID="TextBoxLastName" runat="server" CssClass="TextBox" Width="200px" Enabled="False"></asp:TextBox></td>
    </tr> 
    <tr>
        <td class="whiteCell" style="width: 110px">
            <asp:Label ID="Label9" runat="server" CssClass="Label" Text="Security Question:"></asp:Label></td>
        <td class="whiteCell">
            <asp:DropDownList ID="DropDownListSecurityQuestions" runat="server" CssClass="TextBox"
                Width="260px" Enabled="False">
            </asp:DropDownList></td>
        <td class="whiteCell">
            <asp:Label ID="Label10" runat="server" CssClass="Label" Text="Security Answer:"></asp:Label></td>
        <td class="whiteCell">
            <asp:TextBox ID="TextBoxSecurityAnswer" runat="server" CssClass="TextBox" Width="200px" Enabled="False"></asp:TextBox></td>
    </tr>
    <tr>
        <td class="whiteCell" style="width: 110px">
            <asp:Label ID="Label7" runat="server" Text="Email:" CssClass="Label"></asp:Label></td>
        <td class="whiteCell">
            <asp:TextBox ID="TextBoxEmail" runat="server" Width="200px" CssClass="TextBox" Enabled="False"></asp:TextBox></td>
        <td class="whiteCell">
            <asp:Label ID="Label8" runat="server" CssClass="Label" Text="Phone:"></asp:Label></td>
        <td class="whiteCell">
            <asp:TextBox ID="TextBoxPhone" runat="server" Width="200px" CssClass="TextBox" Enabled="False"></asp:TextBox></td>
    </tr>    
    <tr>
        <td align="left" class="whiteCell" colspan="4" style="height: 28px">
            <igtbl:ultrawebgrid id="UltraWebGridRoles" runat="server" Height="200px" Width="675px">
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
                    BorderWidth="0px" Font-Names="Tahoma" Font-Size="0.8em" Font-Overline="False" Height="200px" Width="675px">
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
        <td colspan=4 class="whiteCell" align=right style="height: 28px">
            <asp:Button ID="ButtonCancel" runat="server" CssClass="buttonText" Text="Cancel" />&nbsp;
            <asp:Button ID="ButtonSave" runat="server" CssClass="buttonText" Text="Save" />&nbsp;</td>
    </tr>    
    <tr>
        <td colspan=4 class="whiteCell" align=center valign=middle><font color="steelblue"><b>- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></font></td>
    </tr>    
    <tr>
        <td colspan=2 class="whiteCell" align=left>
            <asp:DropDownList ID="DropDownListSelectCompanies" runat="server" CssClass="TextBox"
                Width="200px" AutoPostBack="True">
            </asp:DropDownList></td>    
        <td colspan=2 class="whiteCell" align=right>
            <asp:TextBox ID="TextBoxSearch" runat="server" CssClass="TextBox"></asp:TextBox>&nbsp;<asp:Button
                ID="ButtonSearch" runat="server" CssClass="buttonText" Text="Search" />
            |
            <asp:LinkButton ID="LinkButtonShowAll" runat="server" CssClass="buttonText">Show All</asp:LinkButton>&nbsp;</td>
    </tr>
    <tr>
        <td class="whiteCell" colspan=4 align=center valign=top>
            <asp:GridView ID="GridViewUsers" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False"
                EmptyDataText="Data Not Available." Font-Names="Tahoma" Font-Size="0.7em" PageSize="7"
                Width="98%">
                
                <Columns>
                    <asp:TemplateField HeaderText="User Name" SortExpression="UserName">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="false" CommandName="PopulateUserInfo"
                                Text='<%# Eval("UserName") %>' CommandArgument='<%# Eval("UserName") %>'></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:BoundField DataField="UserFirstName" HeaderText="First Name" SortExpression="UserFirstName" />
                    <asp:BoundField DataField="UserLastName" HeaderText="Last Name" SortExpression="UserLastName" />                    
                    <asp:BoundField DataField="UserEmailAddress" HeaderText="Email" />
                    <asp:BoundField DataField="UserPhoneNumber" HeaderText="Phone" />
                    <asp:CheckBoxField DataField="UserIsLockedOut" HeaderText="Locked Out" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="DeleteUser"
                                Text="Delete" CommandArgument='<%# Eval("UserName") %>'></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                
                <PagerStyle Font-Names="Tahoma" Font-Size="0.8em" HorizontalAlign="Right" />
                <HeaderStyle BackColor="LightSteelBlue" Font-Bold="True" Font-Names="Tahoma" Font-Size="1em"
                    Height="22px" />
                <AlternatingRowStyle BackColor="Gainsboro" />
            </asp:GridView>
            &nbsp;
        </td>
    </tr>  
</table>
