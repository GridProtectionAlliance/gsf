<%@ Page Language="VB" AutoEventWireup="true" CodeFile="Default.aspx.vb" Inherits="_Default" ValidateRequest="false" %>

<%@ Register Src="DataPorting.ascx" TagName="DataPorting" TagPrefix="uc9" %>

<%@ Register Src="AccessRequest.ascx" TagName="AccessRequest" TagPrefix="uc7" %>
<%@ Register Src="Settings.ascx" TagName="Settings" TagPrefix="uc8" %>

<%@ Register Src="AccountRequests.ascx" TagName="AccountRequests" TagPrefix="uc6" %>

<%@ Register Src="Groups.ascx" TagName="Groups" TagPrefix="uc5" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register Src="Companies.ascx" TagName="Companies" TagPrefix="uc4" %>

<%@ Register Src="Roles.ascx" TagName="Roles" TagPrefix="uc2" %>
<%@ Register Src="Users.ascx" TagName="Users" TagPrefix="uc3" %>

<%@ Register Src="Applications.ascx" TagName="Applications" TagPrefix="uc1" %>

<%@ Register Assembly="Infragistics2.WebUI.UltraWebTab.v6.1, Version=6.1.20061.28, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.WebUI.UltraWebTab" TagPrefix="igtab" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>TRO - Application Security Administration Tool</title>
    <link href="StyleSheet.css" rel="stylesheet" type="text/css" />
    <meta http-equiv="Page-Exit" content="progid:DXImageTransform.Microsoft.Fade(duration=.25)" />
</head>

<script language="JavaScript">
	function btnClick(e, buttonid){ 
		var bt = document.getElementById(buttonid); 
		if (typeof bt == 'object'){ 
			if (e.keyCode == 13){ 
				e.returnValue = false;
				e.cancel = true;
				bt.click(); 
				return false; 											
			} 
		} 
	} 
</script>

<body>
    <form id="form1" runat="server">
        <table width="100%" cellpadding=0 cellspacing=0 border=0 class="PageHeader">
            <tr>
                <td height="50" width="80%" valign=middle align=left>
                    &nbsp;<img src="Images\TRO.gif" border="0" />&nbsp;<asp:Label ID="Label1" runat="server" CssClass="HeadingLabel" Text="Application Security Administration Tool" Height="31px"></asp:Label>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                </td>
                <td align=right valign=middle width="20%">
                   <a class="helpHyperLink" target=_blank href="Documents/UserGuide_TROAppSecurity.pdf">How do I use this tool?</a>&nbsp; <img src="Images/Help.gif" alt="Click Here for Help" border="0" />&nbsp;
                </td>
            </tr>
            <tr>
                <td colspan=2>
                    <igtab:UltraWebTab ID="UltraWebTabTools" runat="server" BorderColor="#949878" BorderStyle="Solid"
                        BorderWidth="0px" Font-Bold="False" Font-Italic="False" Font-Overline="False"
                        Font-Strikeout="False" Font-Underline="False" Height="700px" Width="100%" SpaceOnLeft="10">
                        <DefaultTabStyle BackColor="WhiteSmoke" Font-Bold="False" Font-Names="Tahoma" Font-Size="8.5pt"
                            ForeColor="Black" Height="18px" Width="110px">
                            <Padding Bottom="0px" Top="0px" />
                        </DefaultTabStyle>
                        <Tabs>
                            <igtab:Tab Text="Applications">
                                <ContentTemplate>
                                    <uc1:Applications ID="Applications1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Roles">
                                <ContentTemplate>
                                    <uc2:Roles ID="Roles1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Groups">
                                <ContentTemplate>
                                    <uc5:Groups ID="Groups1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Users">
                                <ContentTemplate>
                                    <uc3:Users ID="Users1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Companies">
                                <ContentTemplate>
                                    <uc4:Companies ID="Companies1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Account Requests">
                                <ContentTemplate>
                                    <uc6:AccountRequests ID="AccountRequests1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Access Requests">
                                <ContentTemplate>
                                    <uc7:AccessRequest ID="AccessRequest1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Settings">
                                <ContentTemplate>
                                    <uc8:Settings ID="Settings1" runat="server" />
                                </ContentTemplate>
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                            </igtab:Tab>
                            <igtab:Tab Text="Data Porting">
                                <Style>
<Padding Top="2px"></Padding>
</Style>
                                <ContentTemplate>
                                    <uc9:DataPorting ID="DataPorting1" runat="server" />
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
                            WidthBottom="1px" WidthTop="1px" />
                    </igtab:UltraWebTab>
                </td>
            </tr>
            <tr>
                <td class="buttonRow" align=right style="height: 30px"><img src="Images\Corner.jpg" border=0 /></td>
                <td class="buttonCell" align=right valign=middle style="height: 30px">
                    <asp:Label ID="LabelUser" runat="server" CssClass="SmallLabel"></asp:Label>&nbsp;</td>
            </tr>
        </table>
    </form>
</body>
</html>
