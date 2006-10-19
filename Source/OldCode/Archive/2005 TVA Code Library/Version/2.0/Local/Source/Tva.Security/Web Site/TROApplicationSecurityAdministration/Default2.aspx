<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default2.aspx.vb" Inherits="Default2" %>

<%@ Register Assembly="Infragistics2.WebUI.UltraWebGrid.v6.1, Version=6.1.20061.28, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.WebUI.UltraWebGrid" TagPrefix="igtbl" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <igtbl:UltraWebGrid ID="UltraWebGrid1" runat="server">
            <Bands>
                <igtbl:UltraGridBand AddButtonCaption="Column0Column1Column2" Key="Column0Column1Column2">
                    <AddNewRow View="NotSet" Visible="NotSet">
                    </AddNewRow>
                </igtbl:UltraGridBand>
            </Bands>
            <DisplayLayout AllowColSizingDefault="Free" AllowColumnMovingDefault="OnServer" AllowDeleteDefault="Yes"
                AllowSortingDefault="OnClient" AllowUpdateDefault="Yes" BorderCollapseDefault="Separate"
                HeaderClickActionDefault="SortMulti" Name="UltraWebGrid1" RowHeightDefault="20px"
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
                    <BorderDetails ColorLeft="SteelBlue" ColorTop="SteelBlue" />
                    <Padding Left="3px" />
                </RowStyleDefault>
                <HeaderStyleDefault BackColor="LightSteelBlue" BorderStyle="Solid" HorizontalAlign="Center" Font-Bold="True">
                    <BorderDetails ColorLeft="SteelBlue" ColorTop="SteelBlue" WidthLeft="1px" WidthTop="1px" WidthBottom="1px" WidthRight="1px" />
                </HeaderStyleDefault>
                <EditCellStyleDefault BorderStyle="None" BorderWidth="0px">
                </EditCellStyleDefault>
                <FrameStyle BackColor="Window" BorderColor="InactiveCaption" BorderStyle="Solid"
                    BorderWidth="0px" Font-Names="Tahoma" Font-Size="0.8em" Font-Overline="False">
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
                <RowAlternateStyleDefault BackColor="LightSteelBlue">
                </RowAlternateStyleDefault>
            </DisplayLayout>
        </igtbl:UltraWebGrid>
        <asp:Button ID="Button1" runat="server" Text="Button" />
       
    </form>
</body>
</html>
