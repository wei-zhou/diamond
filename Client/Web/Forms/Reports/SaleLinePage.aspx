<%@ Page Language="C#" MasterPageFile="~/Forms/Reports/ReportLayout.Master" AutoEventWireup="true" CodeBehind="SaleLinePage.aspx.cs" Inherits="Home.Forms.Reports.SaleLinePage" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">
    <asp:ScriptManager ID="ScriptManager" runat="server" />
    <rsweb:ReportViewer ID="ReportViewer" runat="server" ProcessingMode="Remote" SizeToReportContent="True">
        <ServerReport ReportPath="/diamond/reports/saleline" ReportServerUrl="http://localhost/reportserver" />
    </rsweb:ReportViewer>
</asp:Content>
