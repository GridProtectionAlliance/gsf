<%@ Control Language="VB" AutoEventWireup="false" CodeFile="DataPorting.ascx.vb" Inherits="DataPorting" %>

<br />
<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
     <tr>
        <td colspan="7" bgcolor="#77AADD"><asp:Label ID="Label1" runat="server" CssClass="BoldLabel" Text="Data Porting Interface"></asp:Label></td>    
    </tr>    
    <tr>
        <td width="60" class="whiteCell">&nbsp;</td>
        <td width="160" class="whiteCell"></td>
        <td width="60" class="whiteCell"></td>
        <td width="140" class="whiteCell"></td>
        <td width="60" class="whiteCell"></td>
        <td width="140" class="whiteCell"></td>
        <td width="80" class="whiteCell"></td>
    </tr>
    <tr>
        <td class="whiteCell"><asp:Label ID="Label4" runat="server" CssClass="Label" Text="Application:"></asp:Label></td>
        <td class="whiteCell"><asp:DropDownList ID="DropDownListApplication" runat="server"></asp:DropDownList></td>
        <td class="whiteCell"><asp:Label ID="Label2" runat="server" CssClass="Label" Text="Source:"></asp:Label></td>
        <td class="whiteCell"><asp:DropDownList ID="DropDownListSource" runat="server"></asp:DropDownList></td>
        <td class="whiteCell"><asp:Label ID="Label3" runat="server" CssClass="Label" Text="Destination:"></asp:Label></td>
        <td class="whiteCell"><asp:DropDownList ID="DropDownListDestination" runat="server"></asp:DropDownList></td>
        <td class="whiteCell" align="right"><asp:Button ID="ButtonCompare" runat="server" CssClass="buttonText" Text="Compare" /></td>
    </tr>    
</table>

<table border=0 cellpadding=2 cellspacing=1 width="750" align=center>
    <tr>
        <td width="150" class="whiteCell"></td>
        <td width="225" class="whiteCell"></td>
        <td width="125" class="whiteCell"></td>
        <td width="250" class="whiteCell"></td>
    </tr>
</table>
