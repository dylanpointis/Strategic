<%@ Page Title="Dar de baja usuario" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BajaUsuario.aspx.cs" Inherits="Strategic.BajaUsuario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Dar de baja usuario</h1>
            <p>Confirmación de baja o reactivación de un usuario del sistema.</p>
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

        <asp:Panel ID="pnlSinUsuario" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <circle cx="12" cy="8" r="4"></circle>
                            <path d="M4 21c0-4 3.5-7 8-7"></path>
                            <path d="M16 16l5 5"></path>
                            <path d="M21 16l-5 5"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinUsuario" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
