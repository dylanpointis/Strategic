<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PausarReactivarAutomatizacion.aspx.cs" Inherits="Strategic.PausarReactivarAutomatizacion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("pausarAutomatizacion.titulo") %></h1>
            <p><%: Traductor.T("pausarAutomatizacion.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.confirmarAccion") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="confirmacion">
                    <p><asp:Label ID="lblConfirmacion" runat="server"></asp:Label></p>
                    <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinAutomatizacion" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <p><asp:Label ID="lblSinAutomatizacion" runat="server"></asp:Label></p>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
