<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AltaFamilia.aspx.cs" Inherits="Strategic.AltaFamilia" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("altaFamilia.titulo") %></h1>
            <p><%: Traductor.T("altaFamilia.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("familia.datos") %></h2>
                <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="80"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre" Display="Dynamic" CssClass="campo-error" ValidationGroup="Familia"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblDescripcion" runat="server" AssociatedControlID="txtDescripcion" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" MaxLength="150"></asp:TextBox>
                        <p class="campo-ayuda"><%: Traductor.T("familia.ayudaDescripcion") %></p>
                    </div>
                </div>

                <sc:SelectorPermisos ID="selectorPermisos" runat="server" />

                <div class="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-strategic" ValidationGroup="Familia" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinRegistro" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <path d="M4 6h6l2 2h8v10H4z"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinRegistro" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
