<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BajaSuscripcion.aspx.cs" Inherits="Strategic.BajaSuscripcion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("bajaSuscripcion.titulo") %></h1>
            <p><%: Traductor.T("bajaSuscripcion.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.confirmarAccion") %></h2>
            </div>

            <div class="strategic-card-body">
                <asp:Label ID="lblAdvertencia" runat="server" CssClass="mensaje-aviso"></asp:Label>

                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblClave" runat="server" AssociatedControlID="txtClave" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtClave" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100" autocomplete="current-password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valClave" runat="server" ControlToValidate="txtClave" Display="Dynamic" CssClass="campo-error" ValidationGroup="Baja"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-strategic-danger" ValidationGroup="Baja" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlNoDisponible" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <rect x="3" y="5" width="18" height="14" rx="2"></rect>
                            <path d="M3 10h18"></path>
                            <path d="M7 15h4"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblNoDisponible" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
