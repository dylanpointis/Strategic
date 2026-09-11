<%@ Page Title="Dar de baja rol" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BajaRol.aspx.cs" Inherits="Strategic.BajaRol" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Dar de baja rol</h1>
            <p>Confirmación de baja o reactivación de un rol del sistema.</p>
        </div>

        <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2>Confirmar acción</h2>
            </div>

            <div class="strategic-card-body">
                <div class="confirmacion">
                    <p><asp:Label ID="lblConfirmacion" runat="server"></asp:Label></p>
                    <asp:Button ID="btnConfirmar" runat="server" Text="Dar de baja" CssClass="btn btn-strategic-danger" CausesValidation="false" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinRegistro" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <path d="M12 3l8 4v6c0 4-3.5 7-8 8-4.5-1-8-4-8-8V7z"></path>
                            <path d="M9 12l2 2 4-4"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinRegistro" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
