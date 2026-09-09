<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FiltroFechas.ascx.cs" Inherits="Strategic.Componentes.FiltroFechas" %>

<div class="filtro">
    <asp:Label ID="lblDesde" runat="server" AssociatedControlID="txtDesde" CssClass="form-label"></asp:Label>
    <asp:TextBox ID="txtDesde" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
</div>

<div class="filtro">
    <asp:Label ID="lblHasta" runat="server" AssociatedControlID="txtHasta" CssClass="form-label"></asp:Label>
    <asp:TextBox ID="txtHasta" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
</div>

<asp:Panel ID="pnlAccion" runat="server" CssClass="filtro filtro-accion">
    <asp:Button ID="btnAplicar" runat="server" CssClass="btn btn-strategic"
        CausesValidation="false" OnClick="btnAplicar_Click" />
</asp:Panel>

<asp:Panel ID="pnlErrorRango" runat="server" CssClass="filtro-error" Visible="false" EnableViewState="false">
    <asp:Label ID="lblErrorRango" runat="server"></asp:Label>
</asp:Panel>
